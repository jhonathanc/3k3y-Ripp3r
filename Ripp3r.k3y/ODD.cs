using System;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ripp3r.Streams;

namespace Ripp3r
{
    public abstract class ODD
    {
        public string DriveLetter { get; private set; }

        public enum DiskType { ERROR, EMPTY, PS1, PS2, PS3, UNKNOWN };

        private static string driveLetter;

        internal static readonly byte[] Key1 = new byte[0x10];
        internal static readonly byte[] Key2 = new byte[0x10];
        static private readonly byte[] Key7 = new byte[0x10];
        static private readonly byte[] Key8 = new byte[0x10];

        private readonly byte[] Key3 = { 0x12, 0x6c, 0x6b, 0x59, 0x45, 0x37, 0x0e, 0xee, 0xca, 0x68, 0x26, 0x2d, 0x02, 0xdd, 0x12, 0xd2 };
        private readonly byte[] Key4 = { 0xd9, 0xa2, 0x0a, 0x79, 0x66, 0x6c, 0x27, 0xd1, 0x10, 0x32, 0xac, 0xcf, 0x0d, 0x7f, 0xb5, 0x01 };
        private readonly byte[] Key5 = { 0x19, 0x76, 0x6f, 0xbc, 0x77, 0xe4, 0xe7, 0x5c, 0xf4, 0x41, 0xe4, 0x8b, 0x94, 0x2c, 0x5b, 0xd9 };
        private readonly byte[] Key6 = { 0x50, 0xcb, 0xa7, 0xf0, 0xc2, 0xa7, 0xc0, 0xf6, 0xf3, 0x3a, 0x21, 0x43, 0x26, 0xac, 0x4e, 0xf3 };

        internal readonly byte[] FixedKey30 = { 0xDC, 0x08, 0x2F, 0x83, 0x7F, 0x14, 0x87, 0xC2, 0x00, 0x8B, 0x7B, 0xC9, 0x20, 0xC5, 0x5B, 0xD9 };
        internal readonly byte[] FixedKey31 = { 0xE8, 0x08, 0x85, 0xF9, 0x6E, 0xD0, 0xF3, 0x67, 0x52, 0xCE, 0x52, 0xBE, 0xC3, 0xC7, 0x4E, 0xF3 };

        private readonly byte[] IV1 = { 0x22, 0x26, 0x92, 0x8d, 0x44, 0x03, 0x2f, 0x43, 0x6a, 0xfd, 0x26, 0x7e, 0x74, 0x8b, 0x23, 0x93 };
        private readonly byte[] IV2 = { 0xe8, 0x0b, 0x3f, 0x0c, 0xd6, 0x56, 0x6d, 0xd0 };
        private readonly byte[] IV3 = { 0x3b, 0xd6, 0x24, 0x02, 0x0b, 0xd3, 0xf8, 0x65, 0xe8, 0x0b, 0x3f, 0x0c, 0xd6, 0x56, 0x6d, 0xd0 };

        static private readonly byte[] InitialSeed = { 0x3E, 0xC2, 0x0C, 0x17, 0x02, 0x19, 0x01, 0x97,0x8A, 0x29, 0x71, 0x79, 0x38, 0x29, 0xD3, 0x08, 
                                        0x04, 0x29, 0xFA, 0x84, 0xE3, 0x3E, 0x7F, 0x73,0x0C, 0x1D, 0x41, 0x6E, 0xEA, 0x25, 0xCA, 0xFB, 
                                        0x3D, 0xE0, 0x2B, 0xC0, 0x05, 0xEA, 0x49, 0x0B,0x03, 0xE9, 0x91, 0x98, 0xF8, 0x3F, 0x10, 0x1F, 
                                        0x1B, 0xA3, 0x4B, 0x50, 0x58, 0x94, 0x28, 0xAD,0xD2, 0xB3, 0xEB, 0x3F, 0xF4, 0xC3, 0x1A, 0x58  };

        private DiskType CurrentDiskType = DiskType.EMPTY;

        static private byte[] theInquiryResponse;
        /*************************************************************
         *************************************************************/
        protected ODD(string dl)
        {
            DriveLetter = dl;
        }

        internal static CDB CDB { get; private set; }

        /*************************************************************
         *************************************************************/

        public static ODD getODD(UsbDevice device)
        {
            driveLetter = device.Device;

            CDB = new CDB();
            if (!CDB.Open(device))
            {
                Trace.WriteLine(string.Format("Open drive '{0}' failed", driveLetter));
                return null;
            }
            return TestCDB(CDB);
        }

        public static ODD getODD(string dl)
        {
            driveLetter = dl;

            CDB = new CDB();
            if (!CDB.Open(driveLetter))
            {
                Trace.WriteLine(string.Format("Open drive {0} failed", driveLetter));
                return null;
            }
            return TestCDB(CDB);
        }

        private static ODD TestCDB(CDB cdb)
        {
            theInquiryResponse = new byte[0x3C];
            if (CDB.DoInquiry(theInquiryResponse) != CDB.IoResult.OK)
            {
                Trace.WriteLine(string.Format("INQ on drive {0} failed", driveLetter));
                CDB.Close();
                return null;
            }
            string IStr = Encoding.ASCII.GetString(theInquiryResponse, 0x08, 0x1C);
            ODD odd = PS3_Drive.Parse(driveLetter, IStr);
            if (odd != null) return odd;
            Interaction.Instance.ReportMessage(string.Format("Drive {0} is NOT a PS3 drive", driveLetter), ReportType.Warning);
            CDB.Close();
            return null;
        }
        /*************************************************************
        *************************************************************/
        public void Close()
        {
            CDB.Close();
        }
        /**********************************************************************************
        **********************************************************************************/
        static public void LoadKeys(byte[] EID4, byte[] KE, byte[] IE)
        {
            byte[] EID4Key = new byte[0x20];
            byte[] EID4IV = new byte[0x10];
            byte[] Buffer = new byte[0x40];

            AESEncrypt(KE, IE, InitialSeed, 0, 0x40, Buffer, 0x00);

            Array.Copy(Buffer, 0x20, EID4Key, 0x00, 0x20);
            Array.Copy(Buffer, 0x10, EID4IV, 0x00, 0x10);

            AESDecrypt(EID4Key, EID4IV, EID4, 0, 0x20, Buffer, 0x00);

            Array.Copy(Buffer, 0x00, Key1, 0x00, 0x10);
            Array.Copy(Buffer, 0x10, Key2, 0x00, 0x10);
        }
        /**********************************************************************************
        **********************************************************************************/
        static public void LoadDefaultKeys()
        {
            byte[] dk1 = { 0x04, 0xFC, 0xA3, 0xF5, 0xE5, 0x74, 0x6F, 0x3E, 0xD8, 0x21, 0xF9, 0xF9, 0x15, 0xAB, 0x5B, 0xD9 };
            byte[] dk2 = { 0x44, 0xA2, 0x7B, 0xD3, 0xAD, 0x5E, 0x68, 0xB2, 0x55, 0x78, 0x1C, 0x46, 0x3A, 0xC3, 0x4E, 0xF3 };

            Array.Copy(dk1, 0x00, Key1, 0x00, 0x10);
            Array.Copy(dk2, 0x00, Key2, 0x00, 0x10);
        }
        /*************************************************************
        *************************************************************/
        public byte[] GetInquiryData()
        {
            return theInquiryResponse;
        }
        /*****************************************************
        *****************************************************/
        static public byte[] GetKey1()
        {
            return Key1;
        }
        /*****************************************************
        *****************************************************/
        static public byte[] GetKey2()
        {
            return Key2;
        }
        /*****************************************************
        *****************************************************/
        void TripleDESEncrypt(byte[] Key, byte[] IV, byte[] Source, int SOffset, int SLength, byte[] Destination, int DOffset)
        {
            using (TripleDES Alg = TripleDES.Create())
            {
                Alg.Padding = PaddingMode.None;
                Alg.Mode = CipherMode.CBC;
                Alg.Key = Key;
                Alg.IV = IV;

                using (ICryptoTransform encryptor = Alg.CreateEncryptor())
                {
                    MemoryStream msEncrypt = new MemoryStream();
                    CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);

                    csEncrypt.Write(Source, SOffset, SLength);
                    csEncrypt.FlushFinalBlock();
                    byte[] cipherBuffer = msEncrypt.ToArray();
                    msEncrypt.Close();
                    csEncrypt.Close();

                    Array.Copy(cipherBuffer, 0x00, Destination, DOffset, SLength);
                }
            }
        }
        /*****************************************************
        *****************************************************/
        static public void AESEncrypt(byte[] Key, byte[] IV, byte[] Source, int SOffset, int SLength, byte[] Destination, int DOffset)
        {
            using (Aes aesAlg = Aes.Create())
            {
                if (aesAlg == null)
                    throw new InvalidOperationException(
                        "The AES decryptor is not available. Check your encryption settings (Windows Policy)");
                aesAlg.Padding = PaddingMode.None;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                using (ICryptoTransform encryptor = aesAlg.CreateEncryptor())
                {
                    MemoryStream msEncrypt = new MemoryStream();
                    CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);

                    csEncrypt.Write(Source, SOffset, SLength);
                    csEncrypt.FlushFinalBlock();
                    byte[] cipherBuffer = msEncrypt.ToArray();
                    msEncrypt.Close();
                    csEncrypt.Close();

                    Array.Copy(cipherBuffer, 0x00, Destination, DOffset, SLength);
                }
            }
        }

        /// <summary>
        /// Don't forget to dispose this!!!
        /// </summary>
        /// <returns></returns>
        public static Aes CreateAes()
        {
            Aes aes = Aes.Create();
            if (aes == null) throw new NullReferenceException("Can't create AES cypher");
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.None;
            return aes;
        }

        public static ICryptoTransform CreateDecryptor(byte[] key, byte[] iv, Aes aes = null)
        {
            bool created = false;
            try
            {
                if (aes == null)
                {
                    created = true;
                    aes = CreateAes();
                }

                aes.Key = key;
                aes.IV = iv;
                return aes.CreateDecryptor();
            }
            finally
            {
                if (created && aes != null) aes.Dispose();
            }
        }

        /*****************************************************
        *****************************************************/

        public static void AESDecrypt(byte[] key, byte[] iv, byte[] Source, int SOffset, int SLength, byte[] Destination,
                                      int DOffset)
        {
            using (ICryptoTransform decryptor = CreateDecryptor(key, iv))
            {
                AESDecrypt(decryptor, Source, SOffset, SLength, Destination, DOffset);
            }
        }

        public static void AESDecrypt(Aes aes, byte[] key, byte[] iv, byte[] Source, int SOffset, int SLength, byte[] Destination,
                                      int DOffset)
        {
            using (ICryptoTransform decryptor = CreateDecryptor(key, iv, aes))
            {
                AESDecrypt(decryptor, Source, SOffset, SLength, Destination, DOffset);
            }
        }

        private static void AESDecrypt(ICryptoTransform decryptor, byte[] Source, int SOffset, int SLength, byte[] Destination, int DOffset)
        {
            using (MemoryStream msDecrypt = new MemoryStream())
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Write))
                {
                    csDecrypt.Write(Source, SOffset, SLength);
                    csDecrypt.FlushFinalBlock();
                    byte[] plainBuffer = msDecrypt.ToArray();
                    Array.Copy(plainBuffer, 0x00, Destination, DOffset, SLength);
                }
            }
        }
        /*****************************************************
        *****************************************************/
        void CalculateSessionKeys(byte[] r1, byte[] r2)
        {
            byte[] Buffer = new byte[0x10];

            Array.Copy(r1, 0x00, Buffer, 0x00, 0x08);
            Array.Copy(r2, 0x08, Buffer, 0x08, 0x08);
            AESEncrypt(Key3, IV1, Buffer, 0x00, 0x10, Key7, 0x00);

            Array.Copy(r1, 0x08, Buffer, 0x00, 0x08);
            Array.Copy(r2, 0x00, Buffer, 0x08, 0x08);
            AESEncrypt(Key4, IV1, Buffer, 0x00, 0x10, Key8, 0x00);
        }
        /*****************************************************
        *****************************************************/

        internal bool EstablishSessionKeys(byte keyselection, byte[] KA, byte[] KB)
        {
            byte[] rnd1 = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F };
            byte[] Hdr = { 0x00, 0x10, 0x00, 0x00 };
            byte[] Buffer = new byte[0x14];
            byte[] Buffer2 = new byte[0x24];
            byte[] r1 = new byte[0x10];
            byte[] r2 = new byte[0x10];

            Array.Copy(Hdr, 0x00, Buffer, 0x00, 0x04);

            AESEncrypt(KA, IV1, rnd1, 0x00, 0x10, Buffer, 0x04);

            if (CDB.DoSendKey(keyselection, Buffer) != CDB.IoResult.OK)
            {
                return false;
            }

            if (CDB.DoReportKey(keyselection, Buffer2) != CDB.IoResult.OK)
            {
                return false;
            }

            using (Aes aes = CreateAes())
            {
                using (ICryptoTransform decryptor = CreateDecryptor(KB, IV1, aes))
                {
                    AESDecrypt(decryptor, Buffer2, 0x04, 0x10, r1, 0x00);
                }
                using (ICryptoTransform decryptor = CreateDecryptor(KB, IV1, aes))
                {
                    AESDecrypt(decryptor, Buffer2, 0x14, 0x10, r2, 0x00);
                }
            }

            /* Check RND1 */
            for (int i = 0; i < 0x10; i++)
            {
                if (rnd1[i] != r1[i])
                {
                    return false;
                }
            }

            CalculateSessionKeys(r1, r2);

            if (keyselection == 0)
            {
                Array.Copy(Hdr, 0x00, Buffer, 0x00, 0x04);

                AESEncrypt(KA, IV1, r2, 0x00, 0x10, Buffer, 0x04);

                if (CDB.DoSendKey(0x02, Buffer) != CDB.IoResult.OK)
                {
                    return false;
                }
            }
            else
            {
                /* Report Policy */
                if (CDB.DoReportKey(keyselection, Buffer2) != CDB.IoResult.OK)
                {
                    return false;
                }
                //result = (int)Buffer[7];
            }
            return true;
        }
        /*****************************************************
        *****************************************************/
        Boolean Authenticate()
        {
            byte[] cdbbody = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA6, 0x59 };
            byte[] cdbbuffer = new byte[0x08];
            byte[] cipheredPayload = new byte[0x54];
            byte[] e1payload = {    0x9f, 0x3e, 0x00, 0x00, 0x1e, 0x79, 0x18, 0x8e, 0x09, 0x3b, 0xc8, 0x77, 0x95, 0xb2, 0xcf, 0x2a,
                                    0xe7, 0xaf, 0x9b, 0xb4, 0x86, 0x80, 0x18, 0x28, 0xc2, 0xca, 0x05, 0xba, 0xd1, 0xf2, 0x78, 0xf1,
                                    0x80, 0x1f, 0xea, 0xcb, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                    0x00, 0x00, 0x00, 0x00, 0x8d, 0xb3, 0x46, 0x93, 0x42, 0x64, 0x81, 0x60, 0x16, 0x8f, 0x51, 0xd1,
                                    0x93, 0x76, 0x23, 0x95, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};

            cipheredPayload[0] = 0x00;
            cipheredPayload[1] = 0x50;
            cipheredPayload[2] = 0x00;
            cipheredPayload[3] = 0x00;

            AESEncrypt(Key7, IV3, e1payload, 0x00, 0x50, cipheredPayload, 0x04);

            TripleDESEncrypt(Key7, IV2, cdbbody, 0x00, 0x08, cdbbuffer, 0x00);

            return (CDB.DoUnknownE1(cdbbuffer, cipheredPayload) == CDB.IoResult.OK);
        }
        /*****************************************************
        *****************************************************/

        internal Boolean GetData(byte[] data1, byte[] data2)
        {
            byte[] data = new byte[0x30];
            byte[] e1cdbbody = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0A, 0xF5 };
            byte[] e0cdbbody = { 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x61, 0x9B };
            byte[] cdbbuffer = new byte[0x08];
            byte[] cipheredPayload = new byte[0x54];
            byte[] e1payload = {   0x9B, 0xCD, 0x00, 0x00, 0x63, 0x17, 0xA8, 0xCD, 0x12, 0x50, 0xD7, 0x0A, 0x19, 0x5D, 0x7E, 0x02,
                                   0xB0, 0xDB, 0x94, 0x6F, 0xCF, 0x2C, 0xCF, 0x4D, 0xEF, 0x20, 0xE7, 0x4C, 0x9A, 0x1E, 0x68, 0x06,
                                   0x02, 0x8A, 0x00, 0x46, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                   0x00, 0x00, 0x00, 0x00, 0x6E, 0x8A, 0x6F, 0x2D, 0xDE, 0x07, 0x20, 0x2B, 0xF4, 0xD0, 0xBB, 0xDB,
                                   0x8A, 0x0B, 0x80, 0x5A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};

            cipheredPayload[0] = 0x00;
            cipheredPayload[1] = 0x50;
            cipheredPayload[2] = 0x00;
            cipheredPayload[3] = 0x00;

            AESEncrypt(Key7, IV3, e1payload, 0x00, 0x50, cipheredPayload, 0x04);

            TripleDESEncrypt(Key7, IV2, e1cdbbody, 0x00, 0x08, cdbbuffer, 0x00);

            if (CDB.DoUnknownE1(cdbbuffer, cipheredPayload) != CDB.IoResult.OK)
            {
                return false;
            }

            TripleDESEncrypt(Key7, IV2, e0cdbbody, 0x00, 0x08, cdbbuffer, 0x00);

            if (CDB.DoUnknownE0(cdbbuffer, cipheredPayload) != CDB.IoResult.OK)
            {
                return false;
            }

            AESDecrypt(Key7, IV3, cipheredPayload, 0x04, 0x30, data, 0x00);
            AESDecrypt(Key8, IV3, data, 0x03, 0x10, data1, 0x00);
            AESDecrypt(Key8, IV3, data, 0x13, 0x10, data2, 0x00);

            return true;
        }
        /*****************************************************
        *****************************************************/
        public bool AuthenticateDrive()
        {
            if (EstablishSessionKeys(0, Key1, Key2) == false)
            {
                return false;
            }
            return Authenticate() && EstablishSessionKeys(1, Key5, Key6);
        }
        /*****************************************************
        *****************************************************/
        public bool GetDiskPresence()
        {
            byte[] buffer = new byte[8];
            if (CDB.DoGetEventStatusNotification(buffer) != CDB.IoResult.OK)
            {
                return false;
            }
            return (buffer[5] == 2);
        }
        /*****************************************************
        *****************************************************/
        public Boolean GetDiskType()
        {
            byte[] buffer = new byte[8];

            if (CDB.DoGetConfiguration(buffer) != CDB.IoResult.OK)
            {
                CurrentDiskType = DiskType.ERROR;
                return false;
            }
            ushort dtype = buffer[6];
            dtype <<= 8;
            dtype |= buffer[7];
            switch (dtype)
            {
                case 0x0000:
                    CurrentDiskType = DiskType.EMPTY;
                    return false;
                case 0xFF71:
                    CurrentDiskType = DiskType.PS3;
                    return true;
                case 0xFF61:
                    CurrentDiskType = DiskType.PS2;
                    return false;
                case 0xFF50:
                    CurrentDiskType = DiskType.PS1;
                    return false;
            }
            CurrentDiskType = DiskType.UNKNOWN;
            return false;
        }
        /*****************************************************
        *****************************************************/
        public DiskType GetCurrentDiskType()
        {
            return CurrentDiskType;
        }
        /*****************************************************
        *****************************************************/

        internal static bool ReadWithRetry(byte[] buffer, UInt32 lba, UInt32 numSectors)
        {
            int i = 0;
            do
            {
                if (CDB.DoRead12(buffer, lba, numSectors) == CDB.IoResult.OK)
                {
                    return true;
                }
                if (i == 0)
                {
                    Interaction.Instance.ReportMessage(string.Format("Read Failed, Retrying FilesStartSector {0:X8}", lba));
                }
            } while (i++ < 0x40);
            return false;
        }
        /*****************************************************
        *****************************************************/
        public static Boolean ReadMetaData(out string gameId)
        {
            gameId = string.Empty;

            int i = 0;
            char[] P3 = { 'P', 'l', 'a', 'y', 'S', 't', 'a', 't', 'i', 'o', 'n', '3' };
            byte[] DataBuffer = new byte[0x02 * Utilities.SectorSize];

            while ((CDB.DoRead12(DataBuffer, 0, 2) != CDB.IoResult.OK) && (i++ < 20)) { }

            if (i == 20)
                return false;

            for (i = 0; i < P3.Length; i++)
            {
                if (P3[i] != DataBuffer[Utilities.SectorSize + i])
                {
                    return false;
                }
            }

            i = 0;
            do
            {
                gameId += (char)DataBuffer[0x810 + i++];
            } while (DataBuffer[0x810 + i] != ' ');

            return true;
        }

        public uint GetNumSectors()
        {
            byte[] CapacityData = new byte[0x08];
            /* Read the game capacity */
            if (CDB.DoReadCapacity(CapacityData) != CDB.IoResult.OK)
            {
                return 0;
            }
            uint NumSectors = CapacityData[0];
            NumSectors <<= 8;
            NumSectors |= CapacityData[1];
            NumSectors <<= 8;
            NumSectors |= CapacityData[2];
            NumSectors <<= 8;
            NumSectors |= CapacityData[3];
            return NumSectors;
        }

        /*****************************************************
        *****************************************************/
        public async Task DoRip(CancellationToken cancellation)
        {
            string isoFileName = await Interaction.Instance.GetIsoPath(null);
            if (string.IsNullOrEmpty(isoFileName)) return;

            Interaction.Instance.TaskBegin();

            try
            {
                using (FileStream isoFile = new FileStream(isoFileName, FileMode.Create, FileAccess.Write))
                {
                    using (DiscStream ds = new DiscStream(this))
                    {
                        long amountOfBytesToRead = ds.Length;
                        long totalRead = 0;

                        Interaction.Instance.SetProgressMaximum((int) amountOfBytesToRead.RoundToSector());

                        while (amountOfBytesToRead > 0)
                        {
                            byte[] buffer = new byte[0x20*Utilities.SectorSize];
                            int bytesRead = await ds.ReadAsync(buffer, 0, buffer.Length);
                            await isoFile.WriteAsync(buffer, 0, bytesRead);

                            if (cancellation.IsCancellationRequested) break;

                            totalRead += bytesRead;
                            amountOfBytesToRead -= bytesRead;

                            Interaction.Instance.ReportProgress((int) totalRead.RoundToSector());
                        }
                    }
                }
                Interaction.Instance.ReportMessage("Rip complete");
            }
            catch (Exception e)
            {
                Interaction.Instance.ReportMessage(e.Message, ReportType.Fail);
            }
            Interaction.Instance.TaskComplete();
            /*
            UInt32 CurrentSector = 0;
            byte[] picData = new byte[0x73];
            byte[] Data1 = new byte[0x10];
            byte[] Data2 = new byte[0x10];

            byte[] DataBuffer = new byte[0x20 * Utilities.SectorSize];

            FileStream ISOFile;
            BinaryWriter bw;

            // Read the PIC data
            if (CDB.DoReadPICZone(picData) != CDB.IoResult.OK)
            {
                Interaction.Instance.ReportMessage("Cannot read PIC data", ReportType.Fail);
                Interaction.Instance.TaskComplete();
                return;
            }
            // Read the Data1/Data2
            if (EstablishSessionKeys(0, Key1, Key2) == false)
            {
                Interaction.Instance.ReportMessage("Cannot establish session keys", ReportType.Fail);
                Interaction.Instance.TaskComplete();
                return;
            }
            if (GetData(Data1, Data2) == false)
            {
                Interaction.Instance.ReportMessage("Cannot extract D1/D2 keys", ReportType.Fail);
                Interaction.Instance.TaskComplete();
                return;
            }
            if (EstablishSessionKeys(1, FixedKey30, FixedKey31) == false)
            {
                Interaction.Instance.ReportMessage("Cannot establish session keys", ReportType.Fail);
                Interaction.Instance.TaskComplete();
                return;
            }

            // Read the game capacity
            uint NumSectors = GetNumSectors();

            if (ReadWithRetry(DataBuffer, CurrentSector, 0x20) == false)
            {
                Interaction.Instance.ReportMessage("Failed to read from the disc", ReportType.Fail);
                Interaction.Instance.TaskComplete();
                return;
            }

            // Open the ISO file
            try
            {
                ISOFile = new FileStream(theIsoFileName, FileMode.Create, FileAccess.Write);
                bw = new BinaryWriter(ISOFile);
            }
            catch (Exception ee)
            {
                string fail = ee.Message;
                gameRipWorker.ReportProgress(0, fail);
                return false;
            }

            Array.Copy(Utilities.Encrypted3KISO, 0, DataBuffer, 0xF70, 0x10);
            Array.Copy(Data1, 0, DataBuffer, 0xF80, 0x10);
            Array.Copy(Data2, 0, DataBuffer, 0xF90, 0x10);
            Array.Copy(picData, 0, DataBuffer, 0xFA0, 0x73);
            bw.Write(DataBuffer, 0, (int)(0x20 * Utilities.SectorSize));

            CurrentSector = 0x20;

            CDB.DoSetCDROMSpeed(0xFFFF);

            while (CurrentSector <= NumSectors)
            {
                if (gameRipWorker.CancellationPending)
                {
                    bw.Close();
                    ISOFile.Close();
                    return true;
                }
                if (ReadWithRetry(DataBuffer, CurrentSector, 0x20) == false)
                {
                    bw.Close();
                    ISOFile.Close();
                    return false;
                }
                bw.Write(DataBuffer, 0, (int)(0x20 * Utilities.SectorSize));

                CurrentSector += 0x20;
                gameRipWorker.ReportProgress((int)CurrentSector, null);
            }

            if (NumSectors > CurrentSector)
            {
                if (gameRipWorker.CancellationPending)
                {
                    bw.Close();
                    ISOFile.Close();
                    return true;
                }
                if (ReadWithRetry(DataBuffer, CurrentSector, (NumSectors - CurrentSector)) == false)
                {
                    bw.Close();
                    ISOFile.Close();
                    return false;
                }
                bw.Write(DataBuffer, 0, (int)((NumSectors - CurrentSector) * Utilities.SectorSize));
            }
            bw.Close();
            ISOFile.Close();
            return true;
             */
        }

        /*****************************************************
        *****************************************************/
        public virtual string DriveName
        {
            get { throw new NotImplementedException(); }
            protected set { throw new NotImplementedException(); }
        }
        /*****************************************************
        *****************************************************/
        public virtual string FWVersion
        {
            get { throw new NotImplementedException(); }
            protected set { throw new NotImplementedException(); }
        }
        /*****************************************************
        *****************************************************/
        public void Eject()
        {
            CDB.DoStartStop();
        }
        /*****************************************************
        *****************************************************/
        public bool WritePBlock()
        {
            byte[] B2 = new byte[0x60];

            byte[] p_block = {
                                0xC9, 0x4B, 0x96, 0x28, 0xE5, 0x40, 0x90, 0x85, 0x36, 0x0F, 0x4A, 0x36, 0x32, 0x57, 0xCD, 0xAD,
                                0x81, 0x1C, 0x51, 0x3A, 0xDD, 0x57, 0x73, 0x72, 0x39, 0x7D, 0x5D, 0xCE, 0xF4, 0x6E, 0xB3, 0x22,
                                0x75, 0xF3, 0x7D, 0xD1, 0x49, 0xA3, 0x7B, 0xE7, 0xAD, 0x53, 0x60, 0x6F, 0x49, 0x1E, 0xA0, 0xAC,
                                0x04, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFD, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                0xA5, 0x59, 0x12, 0xBD, 0x21, 0xAF, 0x6F, 0x55, 0xD1, 0x49, 0x32, 0xFF, 0xC0, 0x48, 0xBF, 0x9C 
                             };

            byte[] UnlockB2 = { 0x00, 0x0E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
                                0x2D, 0x06, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00 };

            CDB.DoStartStop();

            /* P Block */
            if (CDB.DoModeSelect(UnlockB2) != CDB.IoResult.OK)
                return false;
            if (CDB.DoWriteBuffer(5, 2, p_block) != CDB.IoResult.OK)
                return false;
            if (CDB.DoReadBuffer(2, B2) != CDB.IoResult.OK)
                return false;
            return true;
        }
    }
}