using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Crc;
using Ionic.Zlib;
using Ripp3r.Streams;

namespace Ripp3r
{
    public class IrdFile
    {
        public string FullPath { get; private set; }

        private const int IrdVersion = 6;
        private static readonly int[] CompatibleVersions = {};
        private static readonly byte[] MAGIC = new[] { (byte) '3', (byte) 'I', (byte) 'R', (byte) 'D'};
        private string gameId;

        private static readonly byte[] EMPTY = new byte[]
            {0xd4, 0x1d, 0x8c, 0xd9, 0x8f, 0x00, 0xb2, 0x04, 0xe9, 0x80, 0x09, 0x98, 0xec, 0xf8, 0x42, 0x7e};

        public Stream Header { get; set; }
        public Stream Footer { get; set; }

        public uint Crc { get; private set; }
        public int Version { get; private set; }

        public byte[] Data1 { get; private set; }
        public byte[] Data2 { get; private set; }
        public byte[] PIC { get; private set; }

        public string GameVersion { get; set; }
        public string AppVersion { get; set; }

        public List<byte[]> RegionHashes { get; private set; }

        public string GameName { get; set; }

        public string GameID
        {
            get { return gameId; }
            set
            {
                if (value[4] == '-') value = value.Remove(4, 1);
                gameId = value;
            }
        }

        public string UpdateVersion { get; set; }

        public Dictionary<long, byte[]> FileHashes { get; set; }

        private IrdFile()
        {
            RegionHashes = new List<byte[]>();
        }

        private static Stream Open(string path)
        {
            // Detect if this is a zipped or non-zipped item
            FileStream fs = File.OpenRead(path);
            byte[] buffer = new byte[4];
            fs.Read(buffer, 0, 4);
            fs.Seek(0, SeekOrigin.Begin);

            if (buffer.AsString() == MAGIC.AsString())
                return fs;

            GZipStream gz = new GZipStream(fs, CompressionMode.Decompress, CompressionLevel.Level9, false);
            return gz;
        }

        public static IrdFile New()
        {
            return new IrdFile();
        }

        public static IrdFile Load(string path)
        {
            Exception ex = null;

            bool update_ird_file = false;
            IrdFile ird = new IrdFile();
            try
            {
                ird.FullPath = path;
                using (Stream s = Open(path))
                {
                    using (CrcCalculatorStream crc = new CrcCalculatorStream(s, true))
                    {
                        using (BinaryReader br = new BinaryReader(crc))
                        {
                            // Read magic
                            byte[] magic = br.ReadBytes(4);
                            if (magic.AsString() != MAGIC.AsString())
                            {
                                ex = new FileLoadException("Invalid IRD file", path);
                                throw ex;
                            }

                            // Read version
                            ird.Version = br.ReadByte();
                            if (ird.Version != IrdVersion)
                            {
                                if (!CompatibleVersions.Any(v => v == ird.Version))
                                {
                                    ex = new FileLoadException("Unsupported IRD file version (Found version " + ird.Version + ")",
                                                                path);
                                    throw ex;
                                }
                                update_ird_file = true;
                            }

                            ird.gameId = Encoding.ASCII.GetString(br.ReadBytes(9)).Trim('\0');
                            ird.GameName = br.ReadString();

                            // Read version of update file
                            byte[] update = br.ReadBytes(4);
                            ird.UpdateVersion = Encoding.ASCII.GetString(update).Trim('\0');

                            // Read the gameversion
                            byte[] gameVersion = br.ReadBytes(5);
                            ird.GameVersion = Encoding.ASCII.GetString(gameVersion).Trim('\0');

                            byte[] appVersion = br.ReadBytes(5);
                            ird.AppVersion = Encoding.ASCII.GetString(appVersion).Trim('\0');

                            // Read header and footer sectors
                            ird.Header = br.Uncompress();
                            ird.Footer = br.Uncompress();

                            // Read region hashes
                            int amountOfHashes = br.ReadByte();
                            for (int i = 0; i < amountOfHashes; i++)
                            {
                                byte[] hash = br.ReadBytes(0x10);
                                ird.RegionHashes.Add(hash);
                            }

                            // Read file hashes
                            int amountOfFiles = br.ReadInt32();
                            ird.FileHashes = new Dictionary<long, byte[]>(amountOfFiles);
                            for (int i = 0; i < amountOfFiles; i++)
                            {
                                long sector = br.ReadInt64();
                                byte[] hash = br.ReadBytes(0x10);

                                ird.FileHashes.Add(sector, hash);
                            }

                            // Read amount of attachments and extra config fields
                            int extraConfig = br.ReadUInt16();
                            int attachments = br.ReadUInt16();

                            // Yes, we don't use these for now, but we might in the future

                            ird.Data1 = br.ReadBytes(0x10);
                            ird.Data2 = br.ReadBytes(0x10);
                            ird.PIC = br.ReadBytes(0x73);
                        }
                        ird.Crc = (uint) crc.Crc;
                    }
                    byte[] crcValue = new byte[4];
                    s.Read(crcValue, 0, 4);
                    if (ird.Crc != BitConverter.ToUInt32(crcValue, 0))
                    {
                        ex = new FileLoadException("Invalid CRC value in the IRD file", path);
                        throw ex;
                    }
                }
                if (update_ird_file)
                {
                    Interaction.Instance.ReportMessage("Updating IRD file to latest version: " + Path.GetFileName(path));
                    ird.Save(path);
                }

                return ird;
            }
            catch (ZlibException)
            {
                // Annoying bug in the zlib decompression of Ionic.Zip
                if (ex != null) throw ex;
                return ird;
            }
            catch (EndOfStreamException e)
            {
                throw new FileLoadException("Unexpected end of IRD file", path, e);
            }
        }

        public void ExtractAuthData()
        {
            // Extract region 0xF80 - FE9
            // Empty 0xF70 - 0xF7F
            Header.Position = 0xF80;
            Data1 = new byte[0x10];
            Data2 = new byte[0x10];
            PIC = new byte[0x73];
            Header.Read(Data1, 0, Data1.Length);
            Header.Read(Data2, 0, Data2.Length);
            Header.Read(PIC, 0, PIC.Length);

            const int idLength = 0x10 + 0x10 + 0x10 + 0x73;

            byte[] empty = new byte[idLength];
            Header.Position = 0xF70;
            Header.Write(empty, 0, empty.Length);

            PatchD2();
        }

        // This method will add crap to the D2 data
        private void PatchD2()
        {
            byte[] d2 = new byte[Data2.Length];
            ODD.AESDecrypt(Utilities.D2_KEY, Utilities.D2_IV, Data2, 0, Data2.Length, d2, 0);

            // Fetch the last 4 bytes
            int val = BitConverter.ToInt32(d2, 12);
            if (val == 0) return;

            // Fill the last part with a 1, and let the iso builder fill it with crap
            const int newval = 1;
            byte[] rnd = BitConverter.GetBytes(newval).Swap();
            Array.Copy(rnd, 0, d2, 12, rnd.Length);

            ODD.AESEncrypt(Utilities.D2_KEY, Utilities.D2_IV, d2, 0, d2.Length, Data2, 0);
        }

        public void Save(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                using (
                    GZipStream outputStream = new GZipStream(fs, CompressionMode.Compress, CompressionLevel.Level9))
                {
                    outputStream.LastModified = new DateTime(1970, 1, 1);

                    uint crcValue;
                    using (CrcCalculatorStream crc = new CrcCalculatorStream(outputStream, true))
                    {
                        using (BinaryWriter bw = new BinaryWriter(crc))
                        {
                            bw.Write(MAGIC); // Magic header
                            bw.Write((byte) IrdVersion); // IrdVersion bit, version 1 ;) 255 versions should be enough

                            byte[] gameIdBuf = Encoding.ASCII.GetBytes(GameID);
                            crc.Write(gameIdBuf, 0, 9);

                            bw.Write(GameName);

                            byte[] update = Encoding.ASCII.GetBytes(UpdateVersion);
                            crc.Write(update, 0, 4);

                            byte[] buf = Encoding.ASCII.GetBytes(GameVersion);
                            byte[] gameVersion = new byte[5];
                            Array.Copy(buf, 0, gameVersion, 0, buf.Length); // GameVersion can be string.Empty
                            crc.Write(gameVersion, 0, 5);

                            buf = Encoding.ASCII.GetBytes(AppVersion);
                            byte[] appVersion = new byte[5];
                            Array.Copy(buf, 0, appVersion, 0, buf.Length); // AppVersion can be string.Empty
                            crc.Write(appVersion, 0, 5);

                            Header.Compress(bw);
                            Footer.Compress(bw);

                            // Save the hashes
                            bw.Write((byte) RegionHashes.Count);
                            foreach (byte[] hash in RegionHashes)
                            {
                                bw.Write(hash);
                            }
                            bw.Write(FileHashes.Count);
                            foreach (KeyValuePair<long, byte[]> file in FileHashes)
                            {
                                bw.Write(file.Key);
                                bw.Write(file.Value ?? EMPTY);
                            }

                            bw.Write((UInt16) 0); // Extra configurations
                            bw.Write((UInt16) 0); // Attachments

                            bw.Write(Data1);
                            bw.Write(Data2);
                            bw.Write(PIC);
                        }
                        crcValue = (uint) crc.Crc;
                    }
                    byte[] crcBuffer = BitConverter.GetBytes(crcValue);
                    outputStream.Write(crcBuffer, 0, 4);

                    Version = IrdVersion;
                    Crc = crcValue;
                }
            }
        }

        private static bool IsLatest(string path)
        {
            Stream s = Open(path);
            int version;
            using (BinaryReader br = new BinaryReaderExt(s, Encoding.ASCII, true))
            {
                byte[] magic = br.ReadBytes(4);
                if (magic.AsString() != MAGIC.AsString())
                {
                    throw new FileLoadException("Invalid IRD file", path);
                }

                version = br.ReadByte();
            }
            try
            {
                s.Close();
            }
            catch (ZlibException)
            {
                // Bug in ZLib, it will throw an error when you don't read the whole stream!
            }
            return version == IrdVersion;
        }

        private static bool CanLoadVersion(string path)
        {
            Stream s = Open(path);
            int version;
            using (BinaryReader br = new BinaryReaderExt(s, Encoding.ASCII, true))
            {
                byte[] magic = br.ReadBytes(4);
                if (magic.AsString() != MAGIC.AsString())
                {
                    throw new FileLoadException("Invalid IRD file", path);
                }

                version = br.ReadByte();
            }
            try
            {
                s.Close();
            }
            catch (ZlibException)
            {
                // Bug in ZLib, it will throw an error when you don't read the whole stream!
            }
            return version == IrdVersion || CompatibleVersions.Any(v => v == version);
        }

        public static string GetHash(string path)
        {
            if (!CanLoadVersion(path))
                throw new FileLoadException("Invalid IRD file version");
            if (!IsLatest(path))
            {
                Load(path);
            }

            using (Stream s = Open(path))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    s.CopyTo(ms);
                    ms.Position = 0;

                    byte[] content = new byte[ms.Length - (0x10 + 0x73 + 4)];
                    ms.Read(content, 0, content.Length);

                    string md5_1 = content.Md5().AsString();
                    content = new byte[0x73];
                    ms.Seek(0x10, SeekOrigin.Current);
                    ms.Read(content, 0, content.Length);

                    string md5_2 = content.Md5().AsString();

                    return (md5_1 + md5_2).Md5().AsString();
                }
            }
        }
    }
}
