using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using DiscUtils.Iso9660;
using Microsoft.Win32;
using Ripp3r.Streams;

namespace Ripp3r
{
    public static class Extensions
    {
        public static T ToStruct<T>(this IntPtr ptr) where T : struct
        {
            return ptr == IntPtr.Zero ? new T() : (T) Marshal.PtrToStructure(ptr, typeof (T));
        }

        public static T ToStruct<T>(this object currentStruct) where T : struct
        {
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(T)));
            Marshal.StructureToPtr(currentStruct, ptr, false);
            T val = ptr.ToStruct<T>();
            Marshal.FreeHGlobal(ptr);
            return val;
        }

        public static IntPtr ToPtr<T>(this T val) where T : struct
        {
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof (T)));
            Marshal.StructureToPtr(val, ptr, false);
            return ptr;
        }

        public static string GetStringValue(this RegistryKey key, string name)
        {
            object val = key.GetValue(name);
            return val != null ? val.ToString() : null;
        }

        public static string AsString(this byte[] val)
        {
            StringBuilder sb = new StringBuilder(val.Length * 2);
            Array.ForEach(val, b => sb.AppendFormat("{0:x2}", b));
            return sb.ToString();
        }

        public static byte[] AsByteArray(this string val)
        {
            byte[] retval = new byte[val.Length / 2];
            for (int i = 0; i < retval.Length; i++)
            {
                retval[i] = (byte) int.Parse(val.Substring(i*2, 2), NumberStyles.HexNumber);
            }
            return retval;
        }

        public static void ToFile(this string buffer, string path)
        {
            using (StreamWriter md5File = new StreamWriter(path, false))
            {
                md5File.Write(buffer);
            }
        }

        public static UInt16 Swap(this UInt16 inValue)
        {
            byte[] byteArray = BitConverter.GetBytes(inValue);
            Array.Reverse(byteArray);
            return BitConverter.ToUInt16(byteArray, 0);
        }

        public static UInt32 Swap(this UInt32 inValue)
        {
            byte[] byteArray = BitConverter.GetBytes(inValue);
            Array.Reverse(byteArray);
            return BitConverter.ToUInt32(byteArray, 0);
        }

        public static byte[] Swap(this byte[] inValue)
        {
            Array.Reverse(inValue);
            return inValue;
        }

        public static string Join(this string content)
        {
            return content.Replace("\r\n", "").Replace("\n", "");
        }

        public static byte[] Md5(this byte[] content)
        {
            using (MD5 md5 = MD5.Create())
            {
                md5.TransformFinalBlock(content, 0, content.Length);
                return md5.Hash;
            }
        }

        public static byte[] Md5(this string content)
        {
            byte[] b = Encoding.ASCII.GetBytes(content);
            return b.Md5();
        }

        public static byte[] GetMd5(this Stream s, CancellationToken cancellation, long length, ref int sector)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] buffer = new byte[Utilities.SectorSize];
                int read;
                do
                {
                    read = s.Read(buffer, 0, buffer.Length);
                    sector++;
                    Interaction.Instance.ReportProgress(sector);
                    if (cancellation.IsCancellationRequested) return null;
                    if (read != 0) md5.TransformBlock(buffer, 0, read, null, 0);
                } while (read != 0);

                // Check the position, if it's less than length, add zeroes
                // i.e. pad the file (especially for the update file)
                int missing = (int)(length - s.Position);
                if (missing > 0) Array.Clear(buffer, 0, buffer.Length);
                while (missing > 0)
                {
                    int l = missing > buffer.Length ? buffer.Length : missing;
                    md5.TransformBlock(buffer, 0, l, null, 0);
                    missing -= l;
                    Interaction.Instance.ReportProgress(sector++);
                }

                md5.TransformFinalBlock(new byte[0], 0, 0);

                return md5.Hash;
            }
        }

        public static byte[] GetMd5(this string filepath,
                                    CancellationToken cancellation, long length, ref int sector)
        {
            using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                return fs.GetMd5(cancellation, length, ref sector);
            }
        }

        public static string MakeShortFileName(this string path)
        {
            string dir = Path.GetDirectoryName(path);
            string longName = Path.GetFileName(path);

//            if (longName.Length > 31) longName = longName.Substring(0, 31);

            if (longName.Length <= 31 && IsoUtilities.IsValidFileName(longName))
            {
                return path;
            }

            char[] shortNameChars = longName.ToUpper(CultureInfo.InvariantCulture).ToCharArray();
            for (int i = 0; i < shortNameChars.Length; ++i)
            {
                if (!IsoUtilities.IsValidDChar(shortNameChars[i]) && shortNameChars[i] != '.' && shortNameChars[i] != ';')
                {
                    shortNameChars[i] = '_';
                }
            }

            string filename = new string(shortNameChars);
            if (filename.Length > 31) filename = filename.Substring(0, 31);

            return Path.Combine(dir, filename);
        }

        public static Region[] GetRegions(this Stream s)
        {
            using (BinaryReader br = new BinaryReaderExt(s, Encoding.ASCII, true))
            {
                s.Seek(0xF70, SeekOrigin.Begin);
                bool isDecrypting = br.ReadByte() != 0x44; // Begins with E or D
                s.Seek(0xF70 + 0xD, SeekOrigin.Begin);
                bool isBuildedISO = br.ReadByte() == 0x42; // End in BLD
                s.Seek(0xF70 + 0xF, SeekOrigin.Begin);
                bool isValidHash = isBuildedISO && br.ReadByte() != 0x44; // Ends in BLF

                /* Read the game Key */
                s.Seek(0xF80, SeekOrigin.Begin);
                byte[] d1 = br.ReadBytes(0x10);
                byte[] d2 = br.ReadBytes(0x10);

                s.Seek(0, SeekOrigin.Begin);
                uint plainRegions = br.ReadUInt32().Swap();
                Region[] regions = new Region[(plainRegions * 2) - 1];

                s.Seek(4, SeekOrigin.Current);
                uint current = br.ReadUInt32().Swap();
                bool isPlain = true;
                for (uint i = 0; i < regions.Length; i++)
                {
                    uint next = br.ReadUInt32().Swap();
                    if (isPlain)
                    {
                        regions[i] = new PlainRegion(i, isDecrypting, isBuildedISO, isValidHash, current, next);
                    }
                    else
                    {
                        regions[i] = isDecrypting
                                         ? (Region)new CryptedRegion(i, d1, d2, current, next, false)
                                         : new DecryptedRegion(i, d1, d2, current, next, false);
                    }
                    isPlain = !isPlain;
                    current = next;
                }
                s.Seek(0, SeekOrigin.Begin);
                return regions;
            }
        }
    }
}
