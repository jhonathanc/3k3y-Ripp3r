using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Ionic.Zip;
using Ripp3r.Iso9660;
using Ripp3r.Streams;
using Ripp3r.k3y.Properties;

namespace Ripp3r
{
    public class IrdCreateFile : IrdCreator
    {
        protected override Stream Open()
        {
            return new FileStream(Path, FileMode.Open, FileAccess.Read);
        }

        private bool CheckIfValid()
        {
            // First, let's analyse it
            IsoCryptoClass icc = new IsoCryptoClass();

            // Only valid if the iso has been a ripped encrypted ISO, deny Builded ISO's
            IsValid = icc.AnalyseISO(Path) && icc.IsDecrypting && !icc.IsBuildedISO;
            if (IsValid)
            {
                Regions = icc.Regions;
                FileInfo fi = new FileInfo(Path);
                EndOfDataSector = Regions.Last().End;
                TotalSectors = (uint)(fi.Length / Utilities.SectorSize);
            }
            return IsValid;
        }

        public override async Task CreateIRD(CancellationToken cancellation)
        {
            string[] paths = await Interaction.Instance.GetIsoFile();
            if (paths == null) return;
            foreach (string p in paths)
            {
                Path = p;
                if (string.IsNullOrEmpty(Path)) continue;

                if (!CheckIfValid())
                {
                    Interaction.Instance.ReportMessage(Resources.InvalidIsoForIrdFileCreation, ReportType.Fail);
                    continue;
                }

                if (cancellation.IsCancellationRequested) break;

                string savePath = await Interaction.Instance.GetIrdOutputFile(Path);
                if (string.IsNullOrEmpty(savePath)) return;

                Interaction.Instance.SetProgressMaximum((int)TotalSectors);
                Interaction.Instance.TaskBegin();

                await TaskEx.Run(() => InternalCreateIRD(savePath, cancellation));
            }
            Interaction.Instance.TaskComplete();
        }
    }

    public class IrdCreateDisc : IrdCreator
    {
        private readonly ODD _odd;
        private readonly bool _saveIso;

        internal IrdCreateDisc(ODD odd, bool saveIso)
        {
            _odd = odd;
            _saveIso = saveIso;
        }

        protected override Stream Open()
        {
            try
            {
                return new DiscStream(_odd);
            }
            catch (BadReadException e)
            {
                Interaction.Instance.ReportMessage(e.Message, ReportType.Fail);
            }
            catch (AuthenticationException e)
            {
                Interaction.Instance.ReportMessage(e.Message, ReportType.Fail);
            }
            return null;
        }

        private bool CheckIfValid()
        {
            // First, let's analyse it
            IsoCryptoClass icc = new IsoCryptoClass();

            // Only valid if the iso has been a ripped encrypted ISO, deny Builded ISO's
            IsValid = icc.AnalyseISO(_odd) && icc.IsDecrypting && !icc.IsBuildedISO;
            if (IsValid)
            {
                Regions = icc.Regions;
                EndOfDataSector = Regions.Last().End;
                TotalSectors = _odd.GetNumSectors() + 1;
            }
            return IsValid;
        }

        public override async Task CreateIRD(CancellationToken cancellation)
        {
            if (!CheckIfValid())
            {
                Interaction.Instance.ReportMessage("This is not a valid PS3 disc.", ReportType.Fail);
                return;
            }

            string savePath = await Interaction.Instance.GetIrdOutputFile(null);
            if (string.IsNullOrEmpty(savePath)) return;

            string isoPath = _saveIso ? await Interaction.Instance.GetIsoPath(savePath) : null;

            Interaction.Instance.SetProgressMaximum((int)TotalSectors);
            Interaction.Instance.TaskBegin();

            await TaskEx.Run(() => InternalCreateIRD(savePath, cancellation, isoPath));
            Interaction.Instance.TaskComplete();
        }
    }

    public abstract class IrdCreator
    {
        internal IrdCreator()
        {
        }

        public static IrdCreator Create()
        {
            return new IrdCreateFile();
        }

        public static IrdCreator Create(ODD odd, bool saveIso = false)
        {
            return new IrdCreateDisc(odd, saveIso);
        }

        public abstract Task CreateIRD(CancellationToken cancellation);

        protected abstract Stream Open();

        protected async Task InternalCreateIRD(string savePath, CancellationToken cancellation, string isoPath = null)
        {
            byte[] buffer = new byte[Utilities.SectorSize];

            List<FileHash> fileList = new List<FileHash>();

            try
            {
                // First, get the partition table of this iso
                using (Stream fs = Open())
                {
                    if (fs == null) return;
                    PS3CDReader reader = new PS3CDReader(fs);
                    List<DirectoryMemberInformation> files = reader.Members.Where(d => d.IsFile).Distinct().ToList();
                    StartOfDataSector = files.First().StartSector;
                    EndOfDataSector = files.Last().StartSector + files.Last().TotalSectors;

                    var updateFile = files.FirstOrDefault(
                        d => d.Path.Equals("/PS3_UPDATE/PS3UPDAT.PUP", StringComparison.OrdinalIgnoreCase));
                    long updateOffset = updateFile != null ? updateFile.StartSector*Utilities.SectorSize : 0;

                    fileList.AddRange(files.Select(d => new FileHash(d.StartSector, d.Length)));

                    if (cancellation.IsCancellationRequested) return;

                    IrdFile irdFile = IrdFile.New();

                    using (Stream s = reader.OpenFile("PS3_GAME\\PARAM.SFO", FileMode.Open, FileAccess.Read))
                    {
                        ParamSfo sfo = ParamSfo.Load(s);
                        irdFile.AppVersion = sfo.GetStringValue("APP_VER");
                        irdFile.GameVersion = sfo.GetStringValue("VERSION");
                        irdFile.GameID = sfo.GetStringValue("TITLE_ID");
                        irdFile.GameName = sfo.GetStringValue("TITLE");
                    }

                    Interaction.Instance.ReportMessage("Processing " + (string.IsNullOrEmpty(irdFile.GameName)
                                                                            ? irdFile.GameID
                                                                            : irdFile.GameName));

                    irdFile.UpdateVersion = Utilities.FindUpdateVersion(fs, updateOffset);

                    using (FileStream isoStream = string.IsNullOrEmpty(isoPath)
                                                      ? null
                                                      : new FileStream(isoPath, FileMode.Create, FileAccess.Write))
                    {
                        if (isoStream != null) isoStream.SetLength(TotalSectors*Utilities.SectorSize);

                        using (IOStream ioStream = new IOStream(fs, isoStream))
                        {
                            ioStream.Seek(0, SeekOrigin.Begin);

                            // Read the header, until StartOfDataSector (in sectors)
                            irdFile.Header = new MemoryStream();
                            for (int i = 0; i < StartOfDataSector; i++)
                            {
                                int bytesRead = ioStream.Read(buffer, 0, buffer.Length);
                                irdFile.Header.Write(buffer, 0, bytesRead);
                                Interaction.Instance.ReportProgress(i);
                                if (cancellation.IsCancellationRequested) return;
                            }
                            irdFile.ExtractAuthData();

                            // Fix the regions for the actual interesting data
                            Regions.First().Start = (uint) StartOfDataSector;
                            Regions.First().Length -= (uint) StartOfDataSector;
                            Regions.Last().Length = (uint) EndOfDataSector - Regions.Last().Start;

                            // Now, we should calculate the md5 sums of all regions
                            Interaction.Instance.ReportMessage("Calculating hashes for " + Regions.Length + " regions.");
                            for (int i = 0; i < Regions.Length; i++)
                            {
                                // Calculate the hash
                                Interaction.Instance.ReportMessage(
                                    "Calculate hash for region " + i + " (" + Regions[i].Start.ToString("X2") + "-" +
                                    Regions[i].End.ToString("X2") + ")");

                                ioStream.Seek(Regions[i].Start*Utilities.SectorSize, SeekOrigin.Begin);

                                using (FileHashStream fhs = new FileHashStream(fileList, Regions[i].Start))
                                {
                                    await Regions[i].CopyRegion(cancellation, ioStream, fhs);
                                    if (cancellation.IsCancellationRequested) return;
                                    irdFile.RegionHashes.Add(Regions[i].SourceHash);
                                    Interaction.Instance.ReportMessage("Region " + i + " hash: " +
                                                                       Regions[i].SourceHash.AsString());
                                }
                            }

                            ioStream.Seek(EndOfDataSector*Utilities.SectorSize, SeekOrigin.Begin);
                            irdFile.Footer = new MemoryStream();
                            for (long i = EndOfDataSector; i < TotalSectors; i++)
                            {
                                int bytesRead = ioStream.Read(buffer, 0, buffer.Length);
                                irdFile.Footer.Write(buffer, 0, bytesRead);
                                Interaction.Instance.ReportProgress((int) (ioStream.Position/Utilities.SectorSize));
                                if (cancellation.IsCancellationRequested) return;
                            }
                        }
                    }
                    irdFile.FileHashes = fileList.ToDictionary(t => t.StartSector, t => t.Hash);
                    irdFile.Save(savePath);
                    Interaction.Instance.ReportMessage("All done, IRD file saved to " + savePath);
                }
            }
            catch (BadReadException e)
            {
                Interaction.Instance.ReportMessage(e.Message, ReportType.Fail);
            }
            catch (AuthenticationException e)
            {
                Interaction.Instance.ReportMessage(e.Message, ReportType.Fail);
            }
        }
    
        protected bool IsValid { get; set; }
        protected string Path { get; set; }
        private long StartOfDataSector { get; set; }
        protected long EndOfDataSector { get; set; }
        protected long TotalSectors { get; set; }
        protected Region[] Regions { get; set; }
    }
}
