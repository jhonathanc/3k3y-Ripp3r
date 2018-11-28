using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiscUtils;
using Ripp3r.Streams;
using Resources = Ripp3r.k3y.Properties.Resources;

namespace Ripp3r.Iso9660
{
    public class IsoBuilder
    {
        internal string Path { get; private set; }
        public List<Region> Regions { get; private set; }

        private readonly PS3CDBuilder cdBuilder;
        private SparseStream sparseStream;
        private byte[] randomData2;

        public IsoBuilder()
        {
            Regions = new List<Region>();
            cdBuilder = new PS3CDBuilder { VolumeIdentifier = "PS3VOLUME", UseJoliet = true };
        }

        public async Task<bool> CreateIso(CancellationToken cancellation)
        {
            try
            {
                // Get the JB directory
                Path = await Interaction.Instance.GetJBDirectory();
                if (string.IsNullOrEmpty(Path)) return false;

                if (!IsValidDirectory(Path))
                {
                    Interaction.Instance.ShowMessageDialog(Resources.NotAValidJBDirectory);
                    return false;
                }

                if (!Path.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
                    Path += System.IO.Path.DirectorySeparatorChar;

                // Get the resulting iso filename
                string savePath = await Interaction.Instance.GetSaveIsoPath();
                if (string.IsNullOrEmpty(savePath)) return false;

                IrdFile irdFile;
                try
                {
                    string irdPath = await Interaction.Instance.GetIrdFile();
                    if (string.IsNullOrEmpty(irdPath)) return false;
                    irdFile = IrdFile.Load(irdPath);
                }
                catch (FileLoadException e)
                {
                    Interaction.Instance.ReportMessage(e.Message);
                    return false;
                }

                if (!CheckParams(irdFile)) return false;

                if (!(await ValidateOrGetUpdate(irdFile, cancellation))) return false;

                Interaction.Instance.TaskBegin();

                randomData2 = MakeData2Random(irdFile.Data2);

                // Okay, got that figured out, now we have to parse the partition table 
                // and get the regions
                Interaction.Instance.ReportMessage("Parsing IRD file");
                ParseTableOfContents(irdFile);
                cdBuilder.TotalLength = ((LastContentSector + 1)*Utilities.SectorSize) + irdFile.Footer.Length;

                bool isValidFile;

                Interaction.Instance.SetProgressMaximum((int) (LastContentSector - FirstContentSector));
                Interaction.Instance.ReportMessage("Checking files...");
                await AddFiles(cancellation);
                Interaction.Instance.ReportProgress(0); // Reset progress

                IsoCryptoClass icc = new IsoCryptoClass();
                try
                {
                    icc.AnalyseISOBuilder(this);
                }
                catch (Exception e)
                {
                    Interaction.Instance.ReportMessage(e.Message);
                    return false;
                }

                // Verify that the IRD and choosen game directory are the same
                if (irdFile.GameID != icc.GameID)
                {
                    Interaction.Instance.ReportMessage("This IRD file is not valid for this JB directory");
                    return false;
                }

                Interaction.Instance.ReportProgress(0);
                Interaction.Instance.ReportMessage("Validating files...");
                using (Stream s = Build())
                {
                    Interaction.Instance.SetProgressMaximum(FilesEndSector - FilesStartSector);
                    bool validFiles = await TaskEx.Run(() => CheckFiles(cancellation, irdFile));
                    if (!validFiles)
                    {
                        if (cancellation.IsCancellationRequested) return false;

                        // Ask if the user wants to continue
                        Interaction.Instance.ReportMessage("Invalid hashes found, will continue anyway.");
                    }
                    if (cancellation.IsCancellationRequested) return false;

                    Interaction.Instance.ReportProgress(0);
                    Interaction.Instance.ReportMessage(string.Format("Creating ISO '{0}...'", savePath));

                    // Fix regions to write (skip header, and make last region shorter
                    icc.Regions.First().Start = (uint) FirstContentSector;
                    icc.Regions.First().Length -= (uint) FirstContentSector;
                    icc.Regions.Last().Length = (uint) (LastContentSector - icc.Regions.Last().Start) + 1;

                    // Skip partition tables and all, we write those ourselves
                    s.Seek(FirstContentSector*Utilities.SectorSize, SeekOrigin.Begin);

                    using (FileStream fs = new FileStream(savePath, FileMode.Create, FileAccess.ReadWrite))
                    {
                        WriteHeader(fs, irdFile, randomData2, cancellation);
                        if (cancellation.IsCancellationRequested) return false;
                        fs.Seek(FirstContentSector*Utilities.SectorSize, SeekOrigin.Begin);
                        isValidFile = await icc.DoWork(s, fs, cancellation, true, irdFile);
                        if (cancellation.IsCancellationRequested) return false;
                        WriteFooter(fs, irdFile, isValidFile, cancellation);
                        if (cancellation.IsCancellationRequested) return false;

                        if (!isValidFile)
                            Interaction.Instance.SetProgressError();
                    }
                }
                Interaction.Instance.ReportMessage(
                    string.Format("All complete, resulting ISO {0} the original.",
                                  isValidFile ? "MATCHES" : "DOES NOT MATCH"));
            }
            catch (PS3BuilderException)
            {
                Interaction.Instance.TaskComplete();
                Interaction.Instance.ReportMessage("Create ISO aborted due to previous errors.");
                return false;
            }
            catch (Exception e)
            {
                Interaction.Instance.TaskComplete();
                Interaction.Instance.ReportMessage(e.Message);
                return false;
            }

            Interaction.Instance.TaskComplete();
            return true;
        }

        private bool CheckParams(IrdFile irdFile)
        {
            // Check the game id
            ParamSfo sfo = ParamSfo.Load(System.IO.Path.Combine(Path, @"PS3_GAME\PARAM.SFO"));
            string gameID = sfo.GetStringValue("TITLE_ID");
            if (gameID != irdFile.GameID)
            {
                Interaction.Instance.ReportMessage(
                    string.Format("This IRD file is for another game (IRD File: {0}, JB Rip: {1})", irdFile.GameID, gameID),
                    ReportType.Fail);
                return false;
            }

            // Check the game version
            string gameVersion = sfo.GetStringValue("VERSION");
            if (gameVersion != irdFile.GameVersion)
            {
                Interaction.Instance.ReportMessage(
                    string.Format("This IRD file is for another release version (IRD File: {0}, JB Rip: {1})",
                                  irdFile.GameVersion, gameVersion), ReportType.Fail);
                return false;
            }

            // Check the game version
            string appVersion = sfo.GetStringValue("APP_VER");
            if (appVersion != irdFile.AppVersion)
            {
                Interaction.Instance.ReportMessage(
                    string.Format("This IRD file is for another application version (IRD File: {0}, JB Rip: {1})",
                                  irdFile.AppVersion, appVersion), ReportType.Fail);
                return false;
            }
            return true;
        }

        private async Task<bool> ValidateOrGetUpdate(IrdFile irdFile, CancellationToken cancellation)
        {
            // All zeroes, then no update is available (release date games only!)
            if (irdFile.UpdateVersion != "0000")
            {
                // Check the PS3Update version
                string ps3updatePath = System.IO.Path.Combine(Path, "PS3_UPDATE", "PS3UPDAT.PUP");
                bool invalidUpdate = false, downloadUpdate = false;
                if (!File.Exists(ps3updatePath))
                {
                    invalidUpdate = true;
                    downloadUpdate = await Interaction.Instance.DownloadUpdate(
                        string.Format(
                            "The file PS3UPDAT.PUP is missing from your JB rip. Automatically try and download version {0}?",
                            irdFile.UpdateVersion));
                }
                else
                {
                    using (FileStream fs = new FileStream(ps3updatePath, FileMode.Open, FileAccess.Read))
                    {
                        string version = Utilities.FindUpdateVersion(fs, 0);
                        if (version == null || version != irdFile.UpdateVersion)
                        {
                            invalidUpdate = true;
                            string msg = version != null
                                             ? string.Format(
                                                 "The file PS3UPDAT.PUP is version {0}, while the original disc contains version {1}. Either continue (and accept the invalid hash), or replace the PS3UPDAT.PUP with the correct version.\nAutomatically try to download the correct update file?",
                                                 version, irdFile.UpdateVersion)
                                             : string.Format(
                                                 "The file PS3UPDAT.PUP is either empty, or not a valid file. The original disc contains version {0}. Automatically try to download the correct update file?",
                                                 irdFile.UpdateVersion);

                            downloadUpdate = await Interaction.Instance.DownloadUpdate(msg);
                        }
                    }
                }

                if (invalidUpdate)
                {
                    if (!downloadUpdate)
                    {
                        Interaction.Instance.ReportMessage(
                            "The PS3UPDAT.PUP file is missing or incorrect, and you've choosen to not download the correct file. Download the file manually. The required version is " +
                            irdFile.UpdateVersion,
                            ReportType.Fail);
                        Interaction.Instance.ReportMessage("Create ISO aborted.", ReportType.Fail);
                        return false;
                    }

                    // Download the correct update
                    Interaction.Instance.TaskBegin(true);
                    Interaction.Instance.ReportMessage("Download update file...");
                    bool success =
                        await
                        UpdateDownloader.Download(irdFile.UpdateVersion, irdFile.GameID, ps3updatePath, cancellation);

                    if (cancellation.IsCancellationRequested) return false;

                    if (success)
                    {
                        Interaction.Instance.ReportMessage("Updatefile successfully downloaded.", ReportType.Success);
                    }
                    else
                    {
                        Interaction.Instance.ReportMessage(
                            "Failed to download the update file. You should try to locate the update file yourself.",
                            ReportType.Fail);
                        return false;
                    }
                }
            }
            return true;
        }

        private static byte[] MakeData2Random(byte[] data2)
        {
            byte[] d2 = new byte[data2.Length];
            ODD.AESDecrypt(Utilities.D2_KEY, Utilities.D2_IV, data2, 0, data2.Length, d2, 0);

            // Fetch the last 4 bytes
            int val = BitConverter.ToInt32(d2, 12);
            if (val == 0) return data2;

            // Fill the last part with a 1, and let the iso builder fill it with crap
            int newval = new Random().Next(1, 0x1FFFFF);
            byte[] rnd = BitConverter.GetBytes(newval).Swap();
            Array.Copy(rnd, 0, d2, 12, rnd.Length);

            ODD.AESEncrypt(Utilities.D2_KEY, Utilities.D2_IV, d2, 0, d2.Length, data2, 0);
            return data2;
        }

        private void ParseTableOfContents(IrdFile irdFile)
        {
            using (BinaryReader br = new BinaryReaderExt(irdFile.Header, Encoding.ASCII, true))
            {
                irdFile.Header.Seek(0, SeekOrigin.Begin);
                uint amountOfRegions = br.ReadUInt32().Swap();
                br.BaseStream.Position = 8;
                uint start = br.ReadUInt32().Swap();
                for (uint i = 0; i < (2*amountOfRegions) - 1; i++)
                {
                    br.BaseStream.Position = (i + 3)*4;
                    uint next = br.ReadUInt32().Swap();
                    Regions.Add(i%2 == 0
                                    ? (Region) new PlainRegion(i, false, true, true, start, next)
                                    : new DecryptedRegion(i, irdFile.Data1, randomData2, start, next, false));
                    start = next;
                }
            }
            irdFile.Header.Seek(0, SeekOrigin.Begin);

            PS3CDReader reader = new PS3CDReader(irdFile.Header);
            FirstContentSector = reader.Members.First(d => d.IsFile).StartSector;
            LastContentSector = reader.Members.Last().StartSector + reader.Members.Last().TotalSectors - 1;
            cdBuilder.ExtentInfo = reader.Members;
        }

        private long FirstContentSector { get; set; }
        private long LastContentSector { get; set; }

        private async Task AddFiles(CancellationToken cancellation)
        {
            await TaskEx.Run(() =>
                {
                    InternalAddFiles(Path, Path, cancellation);
                    CheckAndFixMissingFiles(cancellation);
                }, cancellation);
        }

        private void CheckAndFixMissingFiles(CancellationToken cancellation)
        {
            bool invalid = false;
            foreach (DirectoryMemberInformation dmi in cdBuilder.ExtentInfo.Where(e => !e.Added).Distinct())
            {
                if (cancellation.IsCancellationRequested) return;

                // Don't care about directories, they are always found (this will fix empty directories)
                // Directories are always written, because the partition table + directory information
                // is stored in the IRD file. Only files are important...
                if (!dmi.IsFile)
                {
                    dmi.Added = true;
                    continue;
                }

                // Don't care about 0 byte files. Those will just be marked as added, since only an entry 
                // in the partition table is enough.
                if (dmi.Length == 0)
                {
                    dmi.Added = true;
                    continue;
                }

                invalid = true;
                Interaction.Instance.ReportMessage("Missing file: " + dmi.Path);
            }

            if (invalid) throw new PS3BuilderException(); // Missing files
        }

        private void InternalAddFiles(string path, string basePath, CancellationToken cancellation)
        {
            string[] files = Directory.GetFiles(path);
            string[] directories = Directory.GetDirectories(path);

            Array.Sort(files); // Sort in order, to be sure

            for (int i = 0; i < files.Length; i++)
            {
                if (cancellation.IsCancellationRequested) return;

                string file = files[i];
                string fileName = file.Substring(basePath.Length);
                if (!IsPartOfDisc(fileName))
                {
                    // Is this a 666xx filename? Then they should be merged...
                    string ext = System.IO.Path.GetExtension(fileName);
                    if (!string.IsNullOrEmpty(ext) && ext.StartsWith(".666"))
                    {
                        // Get the correct filename
                        string subFilename = fileName.Substring(0, fileName.Length - ext.Length);
                        if (IsPartOfDisc(subFilename))
                        {
                            string subFile = file.Substring(0, file.Length - ext.Length);

                            // Add as stream, with all files
                            IEnumerable<string> splitFiles = files.Where(f => f.StartsWith(subFile + ".666")).ToList();
                            SplitStream s = new SplitStream(splitFiles);
                            cdBuilder.AddFile(subFilename, s);
                            i += splitFiles.Count() - 1;

                            // Find the correct dmi and update it
                            SetStream(subFilename, s);
                            continue;
                        }
                    }

                    Interaction.Instance.ReportMessage(string.Format("Skipping file '{0}'", fileName));
                    continue;
                }
                cdBuilder.AddFile(fileName, file);
            }
            foreach (string directory in directories)
            {
                if (cancellation.IsCancellationRequested) return;

                string dirName = directory.Substring(basePath.Length);
                if (!IsPartOfDisc(dirName))
                {
                    Interaction.Instance.ReportMessage(string.Format("Skipping directory '{0}'", dirName));
                    continue;
                }

                cdBuilder.AddDirectory(dirName);
                InternalAddFiles(directory, basePath, cancellation);
            }
        }

        private bool SetStream(string path, Stream stream)
        {
            path = '/' + path.Replace('\\', '/'); // Uses forward slash
            List<DirectoryMemberInformation> dmis = cdBuilder.ExtentInfo.Where(e => e.CheckPath(path)).ToList();

            foreach (var dmi in dmis)
            {
                dmi.Stream = stream;
            }

            return dmis.Count != 0;
        }

        private bool IsPartOfDisc(string path)
        {
            path = '/' + path.Replace('\\', '/'); // Uses forward slash
            List<DirectoryMemberInformation> dmis = cdBuilder.ExtentInfo.Where(e => e.CheckPath(path)).ToList();

            foreach (var dmi in dmis)
            {
                dmi.Added = true;
            }

            return dmis.Count != 0;
        }

        private static bool IsValidDirectory(string folder)
        {
            string path = System.IO.Path.Combine(folder, "PS3_DISC.SFB");
            return File.Exists(path);
        }

        private int FilesStartSector
        {
            get { return (int) cdBuilder.ExtentInfo.First().StartSector; }
        }

        private int FilesEndSector
        {
            get { return (int)(cdBuilder.ExtentInfo.Last().StartSector + cdBuilder.ExtentInfo.Last().TotalSectors); }
        }

        private Stream Build()
        {
            return sparseStream ?? (sparseStream = cdBuilder.Build());
        }

        private void WriteHeader(Stream fs, IrdFile irdFile, byte[] data2, CancellationToken token)
        {
            irdFile.Header.Seek(0, SeekOrigin.Begin);

            byte[] buffer = new byte[Utilities.SectorSize];
            for (int i = 0; i < irdFile.Header.Length; i += buffer.Length)
            {
                irdFile.Header.Read(buffer, 0, buffer.Length);
                fs.Write(buffer, 0, buffer.Length);

                Interaction.Instance.ReportProgress((int)(fs.Position / Utilities.SectorSize));
                if (token.IsCancellationRequested) return;
            }

            // Write the K3YIdentification, Data1, Data2 and PIC data back
            fs.Seek(0xF70, SeekOrigin.Begin);
            fs.Write(Utilities.Encrypted3KBuild, 0, Utilities.Encrypted3KBuild.Length);
            fs.Write(irdFile.Data1, 0, irdFile.Data1.Length);
            fs.Write(irdFile.Data2, 0, data2.Length);
            fs.Write(irdFile.PIC, 0, irdFile.PIC.Length);
        }

        private void WriteFooter(Stream fs, IrdFile irdFile, bool isValidFile, CancellationToken token)
        {
            // Fix identifier
            long pos = fs.Position;
            fs.Seek(0xf70, SeekOrigin.Begin);
            byte[] e3kb = isValidFile ? Utilities.Encrypted3KBuild : Utilities.Encrypted3KFailedBuild;
            fs.Write(e3kb, 0, e3kb.Length);
            fs.Seek(pos, SeekOrigin.Begin);

            irdFile.Footer.Seek(0, SeekOrigin.Begin);

            fs.Seek(-irdFile.Footer.Length, SeekOrigin.End); // Seek a small bit until the end of the last file, and the start of the footer

            byte[] buffer = new byte[Utilities.SectorSize];
            for (int i = 0; i < irdFile.Footer.Length; i += buffer.Length)
            {
                irdFile.Footer.Read(buffer, 0, buffer.Length);
                fs.Write(buffer, 0, buffer.Length);

                Interaction.Instance.ReportProgress((int) (fs.Position / Utilities.SectorSize));
                if (token.IsCancellationRequested) return;
            }
        }

        private bool CheckFiles(CancellationToken cancellation, IrdFile irdFile)
        {
            bool retval = true;

            int sectors = 0;

            // ExtentInfo contains all files from the original ISO. That means that a file that is
            // missing, it will still be available in the ExtentInfo.
            foreach (var grp in cdBuilder.ExtentInfo.Where(d => d.IsFile).GroupBy(l => l.StartSector))
            {
                var dmi = grp.First();
                byte[] hash;
                if (dmi.Stream != null)
                {
                    hash = dmi.Stream.GetMd5(cancellation, 0, ref sectors);
                    dmi.Stream.Seek(0, SeekOrigin.Begin);
                }
                else
                {
                    string filename = System.IO.Path.Combine(Path, dmi.Path.Substring(1));
                    if (!File.Exists(filename))
                    {
                        Interaction.Instance.ReportMessage("File not found: " + dmi.Path);
                        retval = false;
                        continue;
                    }

                    hash = filename.GetMd5(cancellation, dmi.Length, ref sectors);
                }
                if (cancellation.IsCancellationRequested) break;

                // Find the corresponding file
                KeyValuePair<long, byte[]> irdHash = irdFile.FileHashes.FirstOrDefault(f => f.Key == dmi.StartSector);
                if (irdHash.Value == null) continue;

                if (hash.AsString() != irdHash.Value.AsString())
                {
                    Interaction.Instance.ReportMessage("Invalid hash of file: " + dmi.Path);
                    retval = false;
                }

                if (cancellation.IsCancellationRequested) break;
            }
            return retval;
        }

        public static async Task Identify()
        {
            string directory = await Interaction.Instance.GetJBDirectory();
            if (string.IsNullOrEmpty(directory)) return;

            if (!IsValidDirectory(directory))
            {
                Interaction.Instance.ReportMessage("Invalid JB directory", ReportType.Fail);
                return;
            }
            // Parse PARAM.SFO
            ParamSfo sfo = ParamSfo.Load(System.IO.Path.Combine(directory, "PS3_GAME\\PARAM.SFO"));
            string gameid = sfo.GetStringValue("TITLE_ID");
            string appver = sfo.GetStringValue("APP_VER");
            string gamever = sfo.GetStringValue("VERSION");

            Interaction.Instance.ReportMessage("Title: " + sfo.GetStringValue("TITLE"));
            Interaction.Instance.ReportMessage("Game ID: " + gameid);
            Interaction.Instance.ReportMessage("App version: " + appver);
            Interaction.Instance.ReportMessage("Release version: " + gamever);

            await IrdUploader.Search(gameid, appver, gamever);
        }
    }
}
