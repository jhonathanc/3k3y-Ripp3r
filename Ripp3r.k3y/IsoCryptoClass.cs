using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ripp3r.Iso9660;
using Ripp3r.Streams;

namespace Ripp3r
{
    public class IsoCryptoClass
    {
        private string baseName;
        private string inputFilename;

        private UInt32 NumPlainRegions;
        public Region[] Regions { get; private set; }

        private UInt32 amountOfSectors;

        public string GameID { get; private set; }

        /*************************************************************
        *************************************************************/
        public bool IsDecrypting { get; private set; }
        public bool IsBuildedISO { get; private set; }
        public bool IsValidHash { get; private set; }
        /*************************************************************
        *************************************************************/

        public bool AnalyseISOBuilder(IsoBuilder bld)
        {
            ParamSfo sfo = ParamSfo.Load(Path.Combine(bld.Path, @"PS3_GAME\PARAM.SFO"));
            GameID = sfo.GetStringValue("TITLE_ID");
            NumPlainRegions = (uint) ((bld.Regions.Count + 1)/2);
            Regions = bld.Regions.ToArray();
            amountOfSectors = Regions.Last().Length;
            return true;
        }

        /*************************************************************
        *************************************************************/
        public bool AnalyseISO(ODD odd)
        {
            bool IsPlain = true;
            try
            {
                using (DiscStream inputStream = new DiscStream(odd))
                {
                    byte[] buffer = new byte[0x1000];
                    inputStream.Read(buffer, 0, 0x1000);

                    NumPlainRegions = BitConverter.ToUInt32(buffer, 0).Swap();
                    Regions = new Region[(NumPlainRegions * 2) - 1];

                    if (buffer[0xF70 + 0xA] != '3' || buffer[0xF70 + 0xB] != 'K')
                    {
                        Interaction.Instance.ReportMessage("Invalid ISO file. This file does not contain PIC/D1/D2 data");
                        return false;
                    }

                    IsDecrypting = buffer[0xF70] != 0x44; // Begins with E or D
                    IsBuildedISO = buffer[0xF70 + 0xD] == 0x42; // End in BLD
                    IsValidHash = IsBuildedISO && buffer[0xF70 + 0xF] != 0x44; // Ends in BLF

                    int pos = 8;
                    UInt32 Current = BitConverter.ToUInt32(buffer, pos).Swap();
                    pos += 4;
                    for (UInt32 i = 0; i < (2 * NumPlainRegions) - 1; i++)
                    {
                        UInt32 Next = BitConverter.ToUInt32(buffer, pos).Swap();
                        pos += 4;
                        if (IsPlain)
                        {
                            Regions[i] = new PlainRegion(i, IsDecrypting, IsBuildedISO, IsValidHash, Current, Next);
                        }
                        else
                        {
                            Regions[i] = IsDecrypting
                                             ? (Region)new CryptedRegion(i, inputStream.Data1, inputStream.Data2, Current, Next, false)
                                             : new DecryptedRegion(i, inputStream.Data1, inputStream.Data2, Current, Next, false);
                        }
                        IsPlain = !IsPlain;
                        Current = Next;
                    }
                    /* Record this for later */
                    amountOfSectors = Current;

                    // Game ID is at offset 0x810, length = 10
                    GameID = Encoding.ASCII.GetString(buffer, 0x810, 10);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }
        /*************************************************************
        *************************************************************/
        public bool AnalyseISO(string fileName)
        {
            bool IsPlain = true;
            try
            {
                string dir = Path.GetDirectoryName(fileName);
                baseName = string.IsNullOrEmpty(dir)
                               ? Path.GetFileNameWithoutExtension(fileName)
                               : Path.Combine(dir, Path.GetFileNameWithoutExtension(fileName));

                if (baseName.EndsWith(".zip")) // In case of multipart file
                    baseName = string.IsNullOrEmpty(dir)
                                   ? Path.GetFileNameWithoutExtension(baseName)
                                   : Path.Combine(dir, Path.GetFileNameWithoutExtension(baseName));

                inputFilename = fileName;
                using (Ripp3rStream inputStream = new Ripp3rStream(fileName, false))
                {
                    byte[] buffer = new byte[0x1000];
                    inputStream.Read(buffer, 0, 0x1000);

                    NumPlainRegions = BitConverter.ToUInt32(buffer, 0).Swap();
                    Regions = new Region[(NumPlainRegions*2) - 1];

                    if (buffer[0xF70 + 0xA] != '3' || buffer[0xF70 + 0xB] != 'K')
                    {
                        Interaction.Instance.ReportMessage("Invalid ISO file. This file does not contain PIC/D1/D2 data");
                        return false;
                    }

                    IsDecrypting = buffer[0xF70] != 0x44; // Begins with E or D
                    IsBuildedISO = buffer[0xF70 + 0xD] == 0x42; // End in BLD
                    IsValidHash = IsBuildedISO && buffer[0xF70 + 0xF] != 0x44; // Ends in BLF

                    /* Read the game Key */
                    byte[] d1 = new byte[0x10];
                    Array.Copy(buffer, 0xF80, d1, 0, 0x10);
                    byte[] d2 = new byte[0x10];
                    Array.Copy(buffer, 0xF90, d2, 0, 0x10);

                    int pos = 8;
                    UInt32 Current = BitConverter.ToUInt32(buffer, pos).Swap();
                    pos += 4;
                    for (UInt32 i = 0; i < (2*NumPlainRegions) - 1; i++)
                    {
                        UInt32 Next = BitConverter.ToUInt32(buffer, pos).Swap();
                        pos += 4;
                        if (IsPlain)
                        {
                            Regions[i] = new PlainRegion(i, IsDecrypting, IsBuildedISO, IsValidHash, Current, Next);
                        }
                        else
                        {
                            Regions[i] = IsDecrypting
                                             ? (Region)new CryptedRegion(i, d1, d2, Current, Next, inputStream.StreamType == StreamType.Zip)
                                             : new DecryptedRegion(i, d1, d2, Current, Next, inputStream.StreamType == StreamType.Zip);
                        }
                        IsPlain = !IsPlain;
                        Current = Next;
                    }
                    /* Record this for later */
                    amountOfSectors = Current;

                    // Game ID is at offset 0x810, length = 10
                    GameID = Encoding.ASCII.GetString(buffer, 0x810, 10);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }
        /*************************************************************
        *************************************************************/

        public async Task Process(CancellationToken cancellation)
        {
            string filename = await Interaction.Instance.GetCryptFilename();

            if (string.IsNullOrEmpty(filename)) return;

            if (!AnalyseISO(filename))
            {
                Interaction.Instance.ReportMessage(string.Format("Failed, invalid ISO {0}", filename), ReportType.Fail);
                return;
            }
            string outputFilename = await Interaction.Instance.GetCryptOutputFilename(filename, IsDecrypting);
            if (string.IsNullOrEmpty(outputFilename)) return;

            Interaction.Instance.ReportMessage(string.Format("{0}{1}", IsDecrypting ? "Decrypting " : "Encrypting ",
                                   filename));
            Interaction.Instance.ReportMessage(string.Concat("Game ID: ", GameID));
            Interaction.Instance.SetProgressMaximum((int)GetNumSectors() + 1);
            Interaction.Instance.TaskBegin();

            // Encrypt stuff
            bool result = await DoWork(outputFilename, cancellation);
            Interaction.Instance.ReportMessage(result ? "Success" : "Fail", result ? ReportType.Success : ReportType.Fail);

            Interaction.Instance.TaskComplete();
        }
        /*************************************************************
        *************************************************************/
        public UInt32 GetNumSectors()
        {
            return amountOfSectors;
        }

        public async Task<bool> DoWork(Stream inputStream, Stream outputStream, CancellationToken cancellation, bool isBuilding = false, IrdFile irdFile = null)
        {
            bool retval = true;
            if ((IsDecrypting && !Interaction.Instance.Compress && inputStream.CanSeek) ||
                (!IsDecrypting && inputStream.CanSeek))
            {
                long length = !isBuilding || irdFile == null
                                  ? inputStream.Length
                                  : (Regions.Last().End + 1) * Utilities.SectorSize + irdFile.Footer.Length;
                outputStream.SetLength(length);
            }

            for (UInt32 i = 0; !cancellation.IsCancellationRequested && i < (NumPlainRegions * 2) - 1; i++)
            {
                Interaction.Instance.ReportMessage(
                    string.Format("Processing {3} region {0} ({1:X2}-{2:X2})", i, Regions[i].Start,
                                  Regions[i].End, Regions[i].Type));

                await Regions[i].CopyRegion(cancellation, inputStream, outputStream)
                              .ConfigureAwait(false);

                if (cancellation.IsCancellationRequested) return false;

                if (!isBuilding) continue;

                if (irdFile != null)
                {
                    bool match = Regions[i].DestinationHash.AsString() == irdFile.RegionHashes[(int) i].AsString();
                    if (match)
                        Interaction.Instance.ReportMessage("Region hash VALID: " + Regions[i].DestinationHash.AsString(), ReportType.Success);
                    else
                    {
                        retval = false;
                        Interaction.Instance.ReportMessage(
                            "Region hash INVALID. Found: " + Regions[i].DestinationHash.AsString() + ", expected: " +
                            irdFile.RegionHashes[(int) i].AsString(), ReportType.Fail);
                    }
                }
                else
                    Interaction.Instance.ReportMessage("Region " + i + " hash: " + Regions[i].DestinationHash.AsString());
            }
            return retval;
        }

        public async Task<bool> DoWork(string outputFilename, CancellationToken cancellation)
        {
            bool retval = true;

            Trace.WriteLine(string.Format("Amount of processors: {0}", Environment.ProcessorCount));
            Trace.WriteLine(string.Format("Amount of regions: {0}", Regions.Length));
            for (int i = 0; i < Regions.Length; i++)
            {
                Trace.WriteLine(string.Format("Region {0}: {1} from {2:X2} with length {3:X2}", i, Regions[i].GetType().Name,
                           Regions[i].Start, Regions[i].Length));
            }

            using (Ripp3rStream inputStream = new Ripp3rStream(inputFilename))
            {
                using (Ripp3rStream outputStream =
                    new Ripp3rStream(
                        IsDecrypting && Interaction.Instance.Compress ? StreamType.Zip : StreamType.Normal,
                        outputFilename))
                {
                    retval &= await DoWork(inputStream, outputStream, cancellation);

                    outputStream.StopCalculateHash();
                    inputStream.StopCalculateHash();

                    if (!cancellation.IsCancellationRequested)
                    {
                        string outputBaseName = Path.Combine(Path.GetDirectoryName(outputFilename),
                                                             Path.GetFileNameWithoutExtension(outputFilename));

                        SaveHashes(outputBaseName, outputStream, inputStream);
                        outputStream.Close();

                        if (Interaction.Instance.Compress && Interaction.Instance.MultiPart &&
                            outputStream.IsMultipart)
                        {
                            // Write sfv file
                            WriteSfv(outputBaseName, outputStream.GetSfvContent());
                        }

#if DEBUG
                        CompareHashes(IsDecrypting, outputBaseName, outputStream.Hash.AsString());
#endif
                    }
                }
            }
            return retval;
        }

        private void SaveHashes(string outputBaseName, Ripp3rStream outputStream, Ripp3rStream inputStream)
        {
            string outputHash = outputStream.Hash.AsString();
            string inputHash = inputStream.Hash.AsString();
            Interaction.Instance.ReportMessage("Input md5: " + inputHash);
            Interaction.Instance.ReportMessage("Output md5: " + outputHash);

            if (outputStream.StreamType == StreamType.Zip)
            {
                byte[] hashAsString = Encoding.ASCII.GetBytes(inputHash);
                outputStream.AddFile(string.Concat(Path.GetFileName(inputFilename), ".md5"),
                                     hashAsString);
            }
            outputHash.ToFile(string.Concat(outputBaseName, IsDecrypting ? ".dec.md5" : ".enc.md5"));
            inputHash.ToFile(string.Concat(outputBaseName, IsDecrypting ? ".iso.md5" : ".dec.md5"));
        }

        private static void WriteSfv(string outputBaseName, string getSfvContent)
        {
            using (StreamWriter sw = new StreamWriter(string.Concat(outputBaseName, ".sfv")))
            {
                sw.Write(getSfvContent);
            }
        }

#if DEBUG
        private static void CompareHashes(bool isDecrypting, string outputBaseName, string outputHash)
        {
            if (isDecrypting) return;

            string origHash = string.Concat(outputBaseName, ".iso.md5");
            if (!File.Exists(origHash)) return;

            using (StreamReader sr = new StreamReader(origHash))
            {
                string hash = sr.ReadToEnd().Trim();

                if (hash == outputHash)
                    Interaction.Instance.ReportMessage("Hashes MATCH! (Compared .enc.md5 and .iso.md5", ReportType.Success);
                else
                {
                    Interaction.Instance.ReportMessage("Original hash: " + hash);
                    Interaction.Instance.ReportMessage("Apparently they don't match, slap the developer!", ReportType.Fail);
                }
            }
        }
#endif
    }
}
