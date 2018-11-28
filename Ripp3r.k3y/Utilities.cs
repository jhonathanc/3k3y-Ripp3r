using System;
using System.IO;
using System.Text;
using Ionic.Zlib;
using Ripp3r.k3y.Properties;

namespace Ripp3r
{
    public static class Utilities
    {
        public static readonly byte[] Encrypted3KISO = Encoding.ASCII.GetBytes("Encrypted 3K ISO");
        public static readonly byte[] Decrypted3KISO = Encoding.ASCII.GetBytes("Decrypted 3K ISO");
        public static readonly byte[] Encrypted3KBuild = Encoding.ASCII.GetBytes("Encrypted 3K BLD");
        public static readonly byte[] Decrypted3KBuild = Encoding.ASCII.GetBytes("Decrypted 3K BLD");
        public static readonly byte[] Encrypted3KFailedBuild = Encoding.ASCII.GetBytes("Encrypted 3K BLF");
        public static readonly byte[] Decrypted3KFailedBuild = Encoding.ASCII.GetBytes("Decrypted 3K BLF");

        public static readonly byte[] D2_KEY = { 0x7C, 0xDD, 0x0E, 0x02, 0x07, 0x6E, 0xFE, 0x45, 0x99, 0xB1, 0xB8, 0x2C, 0x35, 0x99, 0x19, 0xB3 };
        public static readonly byte[] D2_IV = { 0x22, 0x26, 0x92, 0x8D, 0x44, 0x03, 0x2F, 0x43, 0x6A, 0xFD, 0x26, 0x7E, 0x74, 0x8B, 0x23, 0x93 };

        public const long SectorSize = 0x800L;

        internal static long RoundToSector(this long offset)
        {
            return (long) Math.Ceiling(offset/(SectorSize*1.0));
        }

        internal static string InstallationId
        {
            get
            {
                if (string.IsNullOrEmpty(Settings.Default.InstallationId))
                {
                    Settings.Default.InstallationId = Guid.NewGuid().ToString().Md5().AsString();
                    Settings.Default.Save();
                }
                return Settings.Default.InstallationId;
            }
        }

        public static string PublicKey
        {
            get { return Settings.Default.PublicKey; }
            set
            {
                Settings.Default.PublicKey = value;
                Settings.Default.Save();
            }
        }

        internal static string FindUpdateVersion(Stream fs, long updateOffset)
        {
            fs.Seek(updateOffset, SeekOrigin.Begin);

            byte[] magic = new byte[5];
            fs.Read(magic, 0, magic.Length);
            if (Encoding.ASCII.GetString(magic) != "SCEUF") return String.Format("0000");

            fs.Seek(updateOffset + 0x3E, SeekOrigin.Begin);
            byte[] offset = new byte[2];
            fs.Read(offset, 0, 2);

            ushort versionOffset = BitConverter.ToUInt16(offset, 0).Swap();
            fs.Seek(updateOffset + versionOffset, SeekOrigin.Begin);

            byte[] version = new byte[4];
            fs.Read(version, 0, version.Length);
            return Encoding.ASCII.GetString(version);
        }

        internal static void Compress(this Stream input, BinaryWriter output)
        {
            long pos = input.Position;
            input.Position = 0;

            MemoryStream memStream = new MemoryStream();
            using (
                GZipStream gZipStream = new GZipStream(memStream, CompressionMode.Compress, CompressionLevel.Level9,
                                                       true))
            {
                gZipStream.LastModified = new DateTime(1970, 1, 1);
                input.CopyTo(gZipStream);
            }
            input.Position = pos;

            output.Write((UInt32) memStream.Length);
            memStream.Position = 0;
            memStream.CopyTo(output.BaseStream);
        }

        internal static Stream Uncompress(this BinaryReader input)
        {
            uint length = input.ReadUInt32();
            byte[] header = new byte[length];
            input.Read(header, 0, header.Length);
            return new MemoryStream(GZipStream.UncompressBuffer(header));
        }
    }
}
