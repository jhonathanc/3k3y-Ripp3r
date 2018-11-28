using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ripp3r.Controls;
using Ripp3r.Properties;

namespace Ripp3r
{
    internal class FormsInteraction : IInteraction
    {
        private readonly Form1 _owner;
        private readonly Action<int> _progressHandler;
        private readonly Action<int> _progressMaximum;
        private readonly Action<string, ReportType> _addEvent;
        private readonly TaskScheduler uiScheduler;

        public FormsInteraction(Form1 owner, Action<int> progressHandler, Action<int> progressMaximum,
                                Action<string, ReportType> addEvent)
        {
            _owner = owner;
            _progressHandler = progressHandler;
            _progressMaximum = progressMaximum;
            _addEvent = addEvent;
            uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
        }

        internal async Task<T> UIExecute<T>(Func<T> action)
        {
            return await Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, uiScheduler);
        }

        internal async Task UIExecute(Action action)
        {
            await Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, uiScheduler);
        }

        public async void TaskBegin(bool inBytes)
        {
            await UIExecute(() => _owner.TaskBegin(inBytes));
        }

        public async void TaskComplete()
        {
            await UIExecute(() => _owner.TaskComplete());
        }

        public async void UpdateFound(Version version, string releaseNotes)
        {
            await UIExecute(() => _owner.UpdateFound(version, releaseNotes));
        }

        public void Terminate()
        {
            Application.Exit();
        }

        public bool Compress
        {
            get { return Settings.Default.compressOutput; }
        }

        public long PartSize
        {
            get { return Settings.Default.CalculatedSize; }
        }

        public bool MultiPart
        {
            get { return Settings.Default.zipMultipart; }
        }

        public int AmountOfCores
        {
            get { return Settings.Default.AmountOfCores; }
        }

        public void SetProgressError()
        {
            _owner.SetProgressError();
        }

        #region Crypt

        public async Task<string> GetCryptFilename()
        {
            string file = null;
            await UIExecute(() =>
                {
                    using (OpenFileDialog ofd = new OpenFileDialog())
                    {
                        if (!string.IsNullOrEmpty(Settings.Default.Path_Crypt))
                            ofd.InitialDirectory = Settings.Default.Path_Crypt;
                        ofd.Filter = Resources.Filter3K3yGameIso;
                        ofd.Title = Resources.SelectGameIso;
                        if (ofd.ShowDialog() != DialogResult.OK) return;

                        file = ofd.FileName;
                        Settings.Default.Path_Crypt = Path.GetDirectoryName(file);
                        Settings.Default.Save();
                    }
                });
            return file;
        }

#pragma warning disable 1998
        public async Task<string> GetCryptOutputFilename(string inputfilename, bool isdecrypting)
        {
            string baseName = string.Concat(Path.GetDirectoryName(inputfilename), "\\",
                                            Path.GetFileNameWithoutExtension(inputfilename));
            if (baseName.EndsWith(".zip")) // In case of multipart file
                baseName = string.Concat(Path.GetDirectoryName(baseName), "\\",
                                         Path.GetFileNameWithoutExtension(baseName));

            string outputBaseName = string.IsNullOrEmpty(Settings.Default.destinationFolder)
                                        ? baseName
                                        : Path.Combine(Settings.Default.destinationFolder, Path.GetFileName(baseName));

            return outputBaseName + (isdecrypting ? ".dec" : ".iso");
        }
#pragma warning restore 1998

        #endregion

        #region Create ISO

        public async Task<bool> DownloadUpdate(string message)
        {
            return
                await
                UIExecute(
                    () =>
                    MessageBox.Show(_owner, message, string.Empty, MessageBoxButtons.YesNo) ==
                    DialogResult.Yes);
        }

        public async Task<string> GetJBDirectory()
        {
            string dir = null;
            await UIExecute(() =>
                {
                    using (FolderBrowserDialog dlg = new FolderBrowserDialog())
                    {
                        if (!string.IsNullOrEmpty(Settings.Default.Path_JBDir))
                            dlg.SelectedPath = Settings.Default.Path_JBDir;
                        dlg.Description = Resources.SelectDirectoryContainingJBFiles;
                        dlg.ShowNewFolderButton = false;
                        if (dlg.ShowFocusedDialog() != DialogResult.OK) return;

                        dir = dlg.SelectedPath;
                        Settings.Default.Path_JBDir = dlg.SelectedPath;
                        Settings.Default.Save();
                    }
                });
            return dir;
        }

        public async Task<string> GetSaveIsoPath()
        {
            string file = null;
            await UIExecute(() =>
                {
                    using (SaveFileDialog sdlg = new SaveFileDialog())
                    {
                        if (!string.IsNullOrEmpty(Settings.Default.Path_SaveISOPath))
                            sdlg.InitialDirectory = Settings.Default.Path_SaveISOPath;

                        sdlg.Title = Resources.SaveIsoLocation;
                        sdlg.Filter = Resources.FilterIsoFiles;
                        if (sdlg.ShowDialog() != DialogResult.OK) return;

                        file = sdlg.FileName;
                        Settings.Default.Path_SaveISOPath = Path.GetDirectoryName(file);
                        Settings.Default.Save();
                    }
                });
            return file;
        }

        public async Task<string> GetIrdFile()
        {
            string irdFile = null;

            await UIExecute(() =>
                {
                    using (OpenFileDialog odlg = new OpenFileDialog {Filter = Resources.FilterIRDFile})
                    {
                        if (!string.IsNullOrEmpty(Settings.Default.Path_IRD))
                            odlg.InitialDirectory = Settings.Default.Path_IRD;

                        odlg.Title = Resources.IRDFileToUse;
                        if (odlg.ShowDialog() != DialogResult.OK) return;
                        
                        irdFile = odlg.FileName;
                        Settings.Default.Path_IRD = Path.GetDirectoryName(irdFile);
                        Settings.Default.Save();
                    }
                });
            return irdFile;
        }

        #endregion

        #region Create IRD

        public async Task<string[]> GetIsoFile()
        {
            string[] files = null;
            await UIExecute(() =>
                {
                    using (OpenFileDialog ofd = new OpenFileDialog())
                    {
                        if (!string.IsNullOrEmpty(Settings.Default.Path_ISO))
                            ofd.InitialDirectory = Settings.Default.Path_ISO;

                        ofd.Title = Resources.SelectIsoToCreateIrdFileFrom;
                        ofd.Filter = Resources.FilterIsoFiles;
                        ofd.CheckFileExists = true;
                        ofd.Multiselect = true;
                        if (ofd.ShowDialog(_owner) != DialogResult.OK) return;
                        
                        files = ofd.FileNames;
                        Settings.Default.Path_ISO = Path.GetDirectoryName(ofd.FileName);
                        Settings.Default.Save();
                    }
                });
            return files;
        }

        private async Task<string> GetSaveIrdPath()
        {
            string file = null;
            await UIExecute(() =>
                {
                    using (SaveFileDialog sdlg = new SaveFileDialog())
                    {
                        if (!string.IsNullOrEmpty(Settings.Default.Path_SaveIRDPath))
                            sdlg.InitialDirectory = Settings.Default.Path_SaveIRDPath;

                        sdlg.Title = Resources.WhereSaveIrdFile;
                        sdlg.Filter = Resources.FilterIRDFile;
                        if (sdlg.ShowDialog() != DialogResult.OK) return;

                        file = sdlg.FileName;
                        Settings.Default.Path_SaveIRDPath = Path.GetDirectoryName(file);
                        Settings.Default.Save();
                    }
                });
            return file;
        }

        public async Task<string> GetIsoPath(string irdFile)
        {
            string filename = string.IsNullOrEmpty(irdFile) ? null : Path.GetFileNameWithoutExtension(irdFile);

            string file = null;
            await UIExecute(() =>
            {
                using (SaveFileDialog sdlg = new SaveFileDialog())
                {
                    if (!string.IsNullOrEmpty(Settings.Default.Path_SaveISOPath))
                        sdlg.InitialDirectory = Settings.Default.Path_SaveISOPath;

                    sdlg.FileName = filename;
                    sdlg.Title = Resources.SaveIsoLocation;
                    sdlg.Filter = Resources.FilterIsoFiles;
                    if (sdlg.ShowDialog() != DialogResult.OK) return;

                    file = sdlg.FileName;
                    Settings.Default.Path_SaveISOPath = Path.GetDirectoryName(file);
                    Settings.Default.Save();
                }
            });
            return file;
        }

        public async Task<string> GetIrdOutputFile(string inputFile)
        {
            if (string.IsNullOrEmpty(inputFile))
            {
                return await GetSaveIrdPath();
            }

            if (string.IsNullOrEmpty(Settings.Default.destinationFolder))
            {
                string dir = Path.GetDirectoryName(inputFile);
                string filename = Path.GetFileNameWithoutExtension(inputFile);
                return Path.Combine(dir, string.Concat(filename, ".ird"));
            }
            else
            {
                string filename = Path.GetFileNameWithoutExtension(inputFile);
                return Path.Combine(Settings.Default.destinationFolder, string.Concat(filename, ".ird"));
            }
        }

        #endregion

        #region Upload IRD

        public async Task<string[]> GetIrdFiles()
        {
            string[] irdFiles = null;

            await UIExecute(() =>
                {
                    using (OpenFileDialog odlg = new OpenFileDialog {Filter = Resources.FilterIRDFile})
                    {
                        if (!string.IsNullOrEmpty(Settings.Default.Path_IRD))
                            odlg.InitialDirectory = Settings.Default.Path_IRD;

                        odlg.Multiselect = true;
                        odlg.Title = Resources.IRDFileToUse;
                        if (odlg.ShowDialog() != DialogResult.OK) return;
                        
                        irdFiles = odlg.FileNames;
                        Settings.Default.Path_IRD = Path.GetDirectoryName(odlg.FileName);
                        Settings.Default.Save();
                    }
                });
            return irdFiles;
        }

        #endregion

        #region GameTDB

        public bool GameTDB
        {
            get { return Settings.Default.GameTDB; }
        }

        public string GameTDBLanguage
        {
            get { return Settings.Default.GameTDBLanguage; }
        }

        #endregion

        public async void ShowMessageDialog(string msg)
        {
            await UIExecute(() => MessageBox.Show(_owner, msg));
        }

#pragma warning disable 4014
        public void ReportProgress(int val)
        {
            UIExecute(() => _progressHandler(val));
        }

        public void ReportMessage(string msg, ReportType reportType)
        {
            UIExecute(() => _addEvent(msg, reportType));
        }

        public void SetProgressMaximum(int max)
        {
            UIExecute(() => _progressMaximum(max));
        }
#pragma warning restore 4014
    }
}
