using System;
using System.IO;
using System.Threading.Tasks;
using Ripp3r;

namespace ripp3rcon
{
    class ConsoleInteraction : IInteraction
    {
        private readonly Options _config;
        private readonly ConsoleWriter _consoleWriter;
        private readonly LogFile _logFile;

        public ConsoleInteraction(Options config, ConsoleWriter consoleWriter)
        {
            _config = config;
            _logFile = new LogFile();
            _consoleWriter = consoleWriter;
        }

        public void TaskBegin(bool inBytes)
        {
            _consoleWriter.StartProgress();
        }

        public void TaskComplete()
        {
            _consoleWriter.StopProgress();
        }

        public void UpdateFound(Version version, string releaseNotes)
        {
            string msg = string.Format("Updated version {0} found. Go to http://www.3k3y.com to download.", version.ToString(2));
            _consoleWriter.Write(msg);
        }

        public void Terminate()
        {
            Environment.Exit(0);
        }

        public bool Compress { get { return _config.CryptConfigVerb.Compress; } }
        public long PartSize { get { return _config.CryptConfigVerb.PartSize.GetValueOrDefault(0); } }
        public bool MultiPart { get { return _config.CryptConfigVerb.PartSize.HasValue; } }
        public int AmountOfCores
        {
            get
            {
                return _config.ConfigBase.Cores.GetValueOrDefault(0) > 0
                           ? Math.Min(Environment.ProcessorCount,
                                      _config.CryptConfigVerb.Cores.GetValueOrDefault(Environment.ProcessorCount))
                           : Environment.ProcessorCount;
            } 
        }

#pragma warning disable 1998 // Disable async without await warning

        #region Crypt

        public async Task<string> GetCryptFilename()
        {
            return _config.CryptConfigVerb.InputFile;
        }

        public async Task<string> GetCryptOutputFilename(string inputfilename, bool isdecrypting)
        {
            if (string.IsNullOrEmpty(_config.CryptConfigVerb.OutputFile))
            {
                int index = inputfilename.LastIndexOf(Path.DirectorySeparatorChar);
                string dir = index != -1 ? inputfilename.Substring(0, index) : string.Empty;
                string file = index != -1 ? inputfilename.Substring(index) : inputfilename;

                string baseName = Path.Combine(dir, Path.GetFileNameWithoutExtension(file));

                if (file.EndsWith(".zip")) // In case of file with multiple extensions
                    baseName = Path.Combine(dir, Path.GetFileNameWithoutExtension(baseName));

                return baseName + (isdecrypting ? ".dec" : ".iso");
            }
            return _config.CryptConfigVerb.OutputFile;
        }

        #endregion

        #region Create ISO

        public async Task<bool> DownloadUpdate(string message)
        {
            return _config.CreateIsoVerb.DownloadUpdate;
        }

        public async Task<string> GetJBDirectory()
        {
            return _config.CreateIsoVerb.InputDirectory;
        }

        public async Task<string> GetSaveIsoPath()
        {
            return _config.CreateIsoVerb.Output;
        }

        public async Task<string> GetIrdFile()
        {
            return _config.CreateIsoVerb.IrdFile;
        }

        #endregion

        #region Create IRD

        public async Task<string[]> GetIsoFile()
        {
            return new[] {_config.CreateIrdVerb.IsoFile};
        }

        public async Task<string> GetIrdOutputFile(string inputFile)
        {
            if (string.IsNullOrEmpty(_config.CreateIrdVerb.IrdFile))
            {
                string dir = Path.GetDirectoryName(inputFile);
                string file = Path.GetFileNameWithoutExtension(inputFile) + ".ird";
                return string.IsNullOrEmpty(dir) ? file : Path.Combine(dir, file);
            }
            return _config.CreateIrdVerb.IrdFile;
        }

        public async Task<string> GetIsoPath(string irdFile)
        {
            return null;
        }

        #endregion

        #region Upload IRD

        public async Task<string[]> GetIrdFiles()
        {
            return new[] {_config.CreateIsoVerb.IrdFile};
        }

        #endregion

        #region GameTDB

        public bool GameTDB { get { return false; } }
        public string GameTDBLanguage { get { return string.Empty; } }

        #endregion

#pragma warning restore 1998

        public void ShowMessageDialog(string message)
        {
            _logFile.Log(message);
            Console.WriteLine(message);
        }

        public void SetProgressError()
        {
        }

        public void ReportProgress(int progress)
        {
            _consoleWriter.SetProgressValue(progress);
        }

        public void ReportMessage(string message, ReportType reportType)
        {
            if (reportType != ReportType.Url)
                _logFile.Log(message);
            _consoleWriter.Write(message, reportType);
        }

        public void SetProgressMaximum(int max)
        {
            _consoleWriter.SetProgressMaximum(max);
        }
    }
}
