using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Ripp3r.Controls;
using Ripp3r.Iso9660;
using Ripp3r.Properties;

namespace Ripp3r
{
    internal partial class Form1 : Form
    {
        private enum AppStates { DETECTING, READY, ERROR, DISK_DETECTING, AUTHENTICATING, DISK_TYPE_DETECTING };
        private enum AppStims { NO_DISK_PRESENT, DISK_PRESENT, DRIVE_AUTH_FAILED, DRIVE_AUTH_PASSED, PS3_DISK, NO_PS3_DISK, CANCEL, DRIVE_PRESENT, DRIVE_ABSENT, DRIVE_DETECTED, KEYS_LOADED };

        private readonly GameTDB gameTdb;
        private readonly LogFile logFile;
        private AppStates theAppState;
        private ODD theODD;
        private string gameId;
        private byte[] EID4;
        private byte[] KE;
        private byte[] IE;

        private bool has3DumpLoaded;

        private CancellationTokenSource cancellation;
        private Task currentTask;
        private Stopwatch startedAt;
        private long lastUpdate;

        private int progressValue;
        private bool progressReportError;
        private readonly FormsInteraction formsInteraction;

        /*************************************************************
        *************************************************************/

        public Form1()
        {
            InitializeComponent();
            formsInteraction = new FormsInteraction(this, ReportProgress, SetProgressMaximum, AddEvent);
            Interaction.SetInteraction(formsInteraction);

            components = new Container();
            components.Add(open3DumpFileDialog);
            components.Add(saveFileDialogISO);

            gameTdb = new GameTDB();
            logFile = new LogFile();

            btnRipGame.Enabled = false;
            btnEject.Enabled = false;

            Load3Dump();
        }

        private void Load3Dump()
        {
            if (Settings.Default.UseDefaultKeys)
            {
                ODD.LoadDefaultKeys();
                ShowKeys(true);
                has3DumpLoaded = true;
            }
            else
            {
                string EID4Str = Settings.Default.EID4Str;
                string KEStr = Settings.Default.KEStr;
                string IEStr = Settings.Default.IEStr;

                theAppState = AppStates.DETECTING;
                has3DumpLoaded = EID4Str.Length != 0 && EID4Str.Length != 0 && EID4Str.Length != 0;
                if (!has3DumpLoaded)
                {
                    AddEvent("No key data, please load your 3Dump.bin", ReportType.Warning);
                    return;
                }

                EID4 = EID4Str.AsByteArray();
                KE = KEStr.AsByteArray();
                IE = IEStr.AsByteArray();
                ODD.LoadKeys(EID4, KE, IE);
                ShowKeys(false);
            }
        }
        /*************************************************************
        *************************************************************/
        private void ShowKeys(bool UsingDefaults)
        {
            if (UsingDefaults)
            {
                AddEvent("Using Default Keys");
            }
            else
            {
                AddEvent("Key1 : " + ODD.GetKey1().AsString());
                AddEvent("Key2 : " + ODD.GetKey2().AsString());
            }
        }

        /*************************************************************
        *************************************************************/
        private void btnLoadKeys_Click(object sender, EventArgs e)
        {
            if (open3DumpFileDialog.ShowDialog() != DialogResult.OK) return;

            byte[] eid4 = new byte[0x20];
            byte[] cmac = new byte[0x10];
            byte[] ke = new byte[0x20];
            byte[] ie = new byte[0x10];

            try
            {
                using (FileStream the3DumpFile = new FileStream(open3DumpFileDialog.FileName, FileMode.Open,
                                                                FileAccess.Read))
                {
                    using (BinaryReader br = new BinaryReader(the3DumpFile))
                    {
                        br.Read(eid4, 0, 0x20);
                        br.Read(cmac, 0, 0x10);
                        br.Read(ke, 0, 0x20);
                        br.Read(ie, 0, 0x10);
                    }
                }
            }
            catch (Exception ee)
            {
                string fail = ee.Message;
                AddEvent(fail, ReportType.Fail);
                return;
            }

            string hex = BitConverter.ToString(eid4);
            hex = hex.Replace("-", "");
            Settings.Default.EID4Str = hex;

            hex = BitConverter.ToString(cmac);
            hex = hex.Replace("-", "");
            Settings.Default.CMACStr = hex;

            hex = BitConverter.ToString(ke);
            hex = hex.Replace("-", "");
            Settings.Default.KEStr = hex;

            hex = BitConverter.ToString(ie);
            hex = hex.Replace("-", "");
            Settings.Default.IEStr = hex;

            Settings.Default.Save();

            has3DumpLoaded = true;

            ODD.LoadKeys(eid4, ke, ie);
            ShowKeys(false);

            AuthenticateDrive();
        }
        /*************************************************************
        *************************************************************/
        private void StateMachine(AppStims theStim)
        {
            switch (theAppState)
            {
                case AppStates.ERROR:
                    HandleError(theStim);
                    break;
                case AppStates.DETECTING:
                    HandleDetecting(theStim);
                    break;
                case AppStates.AUTHENTICATING:
                    HandleAuthenticating(theStim);
                    break;

                case AppStates.DISK_DETECTING:
                    HandleDiskDetecting(theStim);
                    break;
                case AppStates.DISK_TYPE_DETECTING:
                    HandleDiskTypeDetecting(theStim);
                    break;
                case AppStates.READY:
                    HandleReady(theStim);
                    break;
            }
        }
        /*************************************************************
        *************************************************************/
        private void HandleDetecting(AppStims theAppStims)
        {
            switch (theAppStims)
            {
                case AppStims.KEYS_LOADED:
                    break;
                case AppStims.DRIVE_DETECTED:
                    theAppState = AppStates.AUTHENTICATING;
                    break;
            }
        }
        /*************************************************************
        *************************************************************/
        private void HandleAuthenticating(AppStims theAppStims)
        {
            switch (theAppStims)
            {
                case AppStims.KEYS_LOADED:
                    AuthenticateDrive();
                    break;
                case AppStims.DRIVE_AUTH_FAILED:
                    //keys.Enabled = true;
                    theAppState = AppStates.ERROR;
                    break;
                case AppStims.DRIVE_AUTH_PASSED:
                    //keys.Enabled = true;
                    theAppState = AppStates.DISK_DETECTING;
                    break;
            }
        }
        /*************************************************************
        *************************************************************/
        private void HandleError(AppStims theAppStims)
        {
            switch (theAppStims)
            {
                case AppStims.DRIVE_PRESENT:
                    break;
                case AppStims.DRIVE_ABSENT:
                    AddEvent("Drive Removed");
                    theAppState = AppStates.DETECTING;
                    break;
            }
        }
        /*************************************************************
        *************************************************************/
        private void HandleDiskDetecting(AppStims theAppStims)
        {
            switch (theAppStims)
            {
                case AppStims.DRIVE_PRESENT:
                    DiskDetectionWorker.RunWorkerAsync();
                    break;
                case AppStims.DRIVE_ABSENT:
                    AddEvent("Drive Removed");
                    theAppState = AppStates.DETECTING;
                    break;
                case AppStims.DISK_PRESENT:
                    theAppState = AppStates.DISK_TYPE_DETECTING;
                    break;
                case AppStims.NO_DISK_PRESENT:
                    /* Wait for user to swap disks */
                    break;
            }
        }
        /*************************************************************
        *************************************************************/
        private void HandleDiskTypeDetecting(AppStims theAppStims)
        {
            switch (theAppStims)
            {
                case AppStims.PS3_DISK:
                    btnRipGame.Enabled = true;
                    btnEject.Enabled = true;
                    AddEvent("Ready");
                    theAppState = AppStates.READY;
                    break;
                case AppStims.NO_PS3_DISK:
                    btnEject.Enabled = true;
                    /* Wait for user to swap disks */
                    break;
                case AppStims.NO_DISK_PRESENT:
                    btnEject.Enabled = false;
                    /* Wait for user to swap disks */
                    break;
                case AppStims.DRIVE_ABSENT:
                    theAppState = AppStates.DETECTING;
                    break;
            }
        }
        /*************************************************************
        *************************************************************/
        private void HandleReady(AppStims theAppStims)
        {
            switch (theAppStims)
            {
                case AppStims.DISK_PRESENT:
                    btnRipGame.Enabled = true;
                    btnEject.Enabled = true;
                    break;
                case AppStims.NO_DISK_PRESENT:
                    AuthenticateDrive();
                    /* Wait for user to swap disks */
                    btnRipGame.Enabled = false;
                    btnEject.Enabled = false;
                    theAppState = AppStates.DISK_DETECTING;
                    break;
                case AppStims.DRIVE_ABSENT:
                    theAppState = AppStates.DETECTING;
                    break;
            }
        }

        /*************************************************************
         * The drive detection worker thread
         *************************************************************/
        private static readonly object detectingDrivesObject = new object();
        private static bool isDetectingDrives;

        private void DetectDriveAsync(string drive = null)
        {
            // First check if this is a cd-rom drive
            if (!string.IsNullOrEmpty(drive))
            {
                DriveInfo driveInfo = DriveInfo.GetDrives()
                                               .FirstOrDefault(
                                                   d =>
                                                   d.RootDirectory.ToString().StartsWith(drive) &&
                                                   d.DriveType == DriveType.CDRom);

                if (driveInfo == null) return;
            }

            Task.Factory.StartNew(() => DetectDrives(drive), CancellationToken.None);
        }
        /*************************************************************
        *************************************************************/
        private void DetectDrives(string drive = null)
        {
            lock (detectingDrivesObject)
            {
                if (isDetectingDrives) return;
                isDetectingDrives = true;
            }

            ODD odd = string.IsNullOrEmpty(drive) ? CdRom.FindDrive() : CdRom.CheckDrive(drive);
            if (odd == null && theODD == null)
            {
                isDetectingDrives = false;
                return;
            }

            if (theODD != null) theODD.Close();
            theODD = odd;

            isDetectingDrives = false;
            if (theODD == null) return;
            StateMachine(AppStims.DRIVE_DETECTED);
            Interaction.Instance.ReportMessage("Drive Detected OK", ReportType.Success);
            AuthenticateDrive();
        }
        /*************************************************************
        *************************************************************/
        private void AddEvent(string str, ReportType reportType = ReportType.Normal)
        {
            if (reportType != ReportType.Url)
                logFile.Log(str);

            ListViewItem lvItem = new ListViewItem(str);
            switch (reportType)
            {
                case ReportType.Fail:
                    lvItem.ForeColor = Color.Red;
                    break;
                case ReportType.Success:
                    lvItem.ForeColor = Color.Green;
                    break;
                case ReportType.Warning:
                    lvItem.ForeColor = Color.OrangeRed; // Should be yellow, but that's not visible at all
                    break;
                case ReportType.Url:
                    string[] parts = str.Split('|');
                    if (parts.Length == 2)
                    {
                        lvItem.ForeColor = Color.Blue;
                        lvItem.Font = new Font(lvItem.Font, FontStyle.Underline);
                        lvItem.Text = parts[0];
                        lvItem.Tag = parts[1];
                    }
                    break;
            }

            eventsListBox.EnsureVisible(eventsListBox.Items.Add(lvItem).Index);
        }
        /*************************************************************
        *************************************************************/
        private async void AuthenticateDrive()
        {
            if (theODD == null) return;

            if (!has3DumpLoaded)
            {
                return;
            }

            if (Settings.Default.UseDefaultKeys)
            {
                ODD.LoadDefaultKeys();
                if (!theODD.AuthenticateDrive())
                {
                    bool writeKeys = await formsInteraction.UIExecute(() => MessageBox.Show(
                        Resources.UseDefaultKeys,
                        Resources.DefaultKeysCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) ==
                                                                            DialogResult.Yes);

                    if (writeKeys)
                    {
                        // Authentication failed, 
                        if (!theODD.WritePBlock())
                        {
                            Interaction.Instance.ReportMessage("Couldn't load P Block - Drive Authentication Failed", ReportType.Fail);
                            return;
                        }
                    }
                }
            }

            bool theResult = theODD.AuthenticateDrive();
            if (!theResult)
            {
                Interaction.Instance.ReportMessage("Drive Authentication Failed", ReportType.Fail);
                StateMachine(AppStims.DRIVE_AUTH_FAILED);
                await formsInteraction.UIExecute(() => { btnLoadKeys.Enabled = true; });
            }
            else
            {
                Interaction.Instance.ReportMessage("Drive Authenticated OK", ReportType.Success);
                StateMachine(AppStims.DRIVE_AUTH_PASSED);
                await formsInteraction.UIExecute(() => { btnLoadKeys.Enabled = false; });
                StateMachine(AppStims.DISK_PRESENT);
                await formsInteraction.UIExecute(() => DiskTypeDetectionWorker.RunWorkerAsync());
            }
        }
        /*************************************************************
        *************************************************************/
        private void DiskDetectionWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bool theResult = (bool)e.Result;
            StateMachine(theResult ? AppStims.DISK_PRESENT : AppStims.NO_DISK_PRESENT);
        }
        /*************************************************************
        *************************************************************/
        private void DiskDetectionWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = theODD != null && theODD.GetDiskPresence();
        }
        /*************************************************************
        *************************************************************/
        private void DiskTypeDetectionWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bool updateSM = true;
            bool theResult = (bool)e.Result;
            switch(theODD.GetCurrentDiskType())
            {
                case ODD.DiskType.PS3:
                    bool isPs3Disc = ODD.ReadMetaData(out gameId);
                    AddEvent(!isPs3Disc
                                 ? "Unknown Disk Type"
                                 : string.Format("PS3 Game Disk : '{0}'", gameId), isPs3Disc ? ReportType.Normal : ReportType.Warning);
                    break;
                case ODD.DiskType.PS2:
                    AddEvent("PS2 Game Disk");
                    break;
                case ODD.DiskType.PS1:
                    AddEvent("PS1 Game Disk");
                    break;
                case ODD.DiskType.UNKNOWN:
                    AddEvent("Unknown Disk Type", ReportType.Warning);
                    break;
                case ODD.DiskType.EMPTY:
                    AddEvent("No disc in drive", ReportType.Warning);
                    updateSM = false;
                    break;
            }
            if(updateSM)
                StateMachine(theResult ? AppStims.PS3_DISK : AppStims.NO_PS3_DISK);

            btnDdlCreateIrd.Visible = theResult;
            btnIRDFile.Visible = !theResult;
        }
        /*************************************************************
        *************************************************************/
        private void DiskTypeDetectionWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = theODD.GetDiskType();
        }
        /*************************************************************
        *************************************************************/
        private void DumpInquiry_Click(object sender, EventArgs e)
        {
            byte[] InquiryData = theODD.GetInquiryData();
            using (SaveFileDialog sfd = new SaveFileDialog
                {
                    FileName = "drive.inq",
                    Filter = @"Inquiry Data (*.inq)|*.inq|All files (*.*)|*.*"
                })
            {
                if (sfd.ShowDialog() != DialogResult.OK) return;
                string InquiryFileName = sfd.FileName;

                try
                {
                    using (FileStream INQFile = new FileStream(InquiryFileName, FileMode.Create, FileAccess.Write))
                    {
                        using (BinaryWriter bw = new BinaryWriter(INQFile))
                        {
                            bw.Write(InquiryData, 0, 0x3C);
                        }
                    }
                }
                catch (Exception ee)
                {
                    string fail = ee.Message;
                    AddEvent(fail, ReportType.Fail);
                }
            }
        }
        /*************************************************************
        *************************************************************/
        private void About_Click(object sender, EventArgs e)
        {
            using (MyAbout ab = new MyAbout())
            {
                ab.ShowDialog();
            }
        }
        /*************************************************************
        *************************************************************/
        private static void SaveEnabledState(params ToolStripItem[] btns)
        {
            foreach (var btn in btns.Where(btn => btn.Tag == null || (bool) btn.Tag != btn.Enabled))
                btn.Tag = btn.Enabled;
        }

        /*************************************************************
        *************************************************************/
        private static void RestoreEnabledState(params ToolStripItem[] btns)
        {
            foreach (var btn in btns.Where(b => b.Tag is bool))
                btn.Enabled = (bool) btn.Tag;
        }

        /*************************************************************
        *************************************************************/

        internal void TaskBegin(bool inBytes = false)
        {
            SaveEnabledState(btnCreateIso, btnIsoCrypto, btnIRDFile, btnDdlCreateIrd, btnLoadKeys, btnRipGame, btnEject, uploadIRDFileToolStripMenuItem, setPublicKeyToolStripMenuItem, checkForUpdatesToolStripMenuItem);

            lbInSectors.Text = inBytes ? "Bytes:" : "Sectors:";
            progressReportError = false;
            btnCancel.Enabled = true;
            btnCreateIso.Enabled =
                btnIsoCrypto.Enabled =
                btnIRDFile.Enabled =
                btnDdlCreateIrd.Enabled = 
                btnRipGame.Enabled =
                btnLoadKeys.Enabled =
                uploadIRDFileToolStripMenuItem.Enabled =
                setPublicKeyToolStripMenuItem.Enabled = checkForUpdatesToolStripMenuItem.Enabled = false;

            startedAt = Stopwatch.StartNew();
            lbElapsed.Visible = lbRemaining.Visible = lbSectors.Visible = true;
            updateProgress.Enabled = true;
            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal);
        }

        /*************************************************************
        *************************************************************/

        internal void TaskComplete()
        {
            btnCancel.Enabled = false;
            if (cancellation != null)
            {
                cancellation.Dispose();
                cancellation = null;
            }
            currentTask = null;
            updateProgress.Enabled = false;
            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);

            updateProgress_Tick(null, EventArgs.Empty); // Force one final update

            progressValue = 0;
            updateProgress_Tick(null, EventArgs.Empty); // And reset the progressbar to zero

            RestoreEnabledState(btnCreateIso, btnIsoCrypto, btnIRDFile, btnDdlCreateIrd, btnLoadKeys, btnRipGame, btnEject,
                                uploadIRDFileToolStripMenuItem, setPublicKeyToolStripMenuItem,
                                checkForUpdatesToolStripMenuItem);
            
            if (startedAt == null) return;
            startedAt.Stop();
            startedAt.Reset();
            startedAt = null;
        }

        /*************************************************************
        *************************************************************/

        public async void UpdateFound(Version version, string releaseNotes)
        {
            string msg = string.Format("Version {0} is available{1}{1}{2}{1}{1}Do you want to update now?", version.ToString(2), Environment.NewLine, releaseNotes);
            if (MessageBox.Show(this, msg, Resources.UpdateFound, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                cancellation = new CancellationTokenSource();
                Ripp3rUpdate update = new Ripp3rUpdate();
                currentTask = update.DownloadAndInstallUpdate(cancellation.Token);
                await currentTask;
            }
        }

        /*************************************************************
        *************************************************************/

        private async void btnIsoCrypto_Click(object sender, EventArgs e)
        {
            cancellation = new CancellationTokenSource();
            IsoCryptoClass cryptoClass = new IsoCryptoClass();
            currentTask = cryptoClass.Process(cancellation.Token);
            await currentTask;
        }

        /*************************************************************
        *************************************************************/
        private void SetProgressMaximum(int max)
        {
            ripProgressBar.Maximum = max;
        }

        internal void SetProgressError()
        {
            progressReportError = true;
        }

        private void updateProgress_Tick(object sender, EventArgs e)
        {
            int val = progressValue;

            if (val > ripProgressBar.Maximum)
                // Increase the maximum, this is an calculation error somewhere
            {
                Debug.WriteLine("Increasing progressbar maximum, you've messed up somewhere");
                ripProgressBar.Maximum = val;
            }
            ripProgressBar.Value = val;
            TaskbarManager.Instance.SetProgressValue(val, ripProgressBar.Maximum);

            if (progressReportError) TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Error);

            lbSectors.Text = string.Format("{0}/{1}", val, ripProgressBar.Maximum);
            if (startedAt == null || (val != ripProgressBar.Maximum && (DateTime.Now.Ticks - lastUpdate) <= TimeSpan.TicksPerSecond))
                return;

            lbElapsed.Visible = lbRemaining.Visible = lbSectors.Visible = true;

            // Calculate the labels
            lbElapsed.Text = startedAt.Elapsed.ToString(@"hh\:mm\:ss");
            if (progressValue > 10)
            {
                TimeSpan remaining =
                    new TimeSpan((long)((startedAt.Elapsed.Ticks / (progressValue * 1.0)) * ripProgressBar.Maximum) -
                                 startedAt.Elapsed.Ticks);
                lbRemaining.Text = remaining.ToString(@"hh\:mm\:ss");
            }
            lastUpdate = DateTime.Now.Ticks;
        }

        /*************************************************************
       *************************************************************/
        private void ReportProgress(int progress)
        {
            if (progress != -1)
            {
                progressValue = progress;
            }
        }

        /*************************************************************
       *************************************************************/
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Native.WM_DEVICECHANGE)
            {
                Native.DEV_BROADCAST_HDR broadcast = m.LParam.ToStruct<Native.DEV_BROADCAST_HDR>();

                // Was a device added?
                switch ((Native.DeviceBroadcastType) m.WParam.ToInt32())
                {
                    case Native.DeviceBroadcastType.Arrival:
                        // Apparently, you'll get this event when a new device has been added, but 
                        // also when a disc has been inserted.
                        if (broadcast.DeviceType == Native.DeviceType.Volume)
                        {
                            Native.DEV_BROADCAST_VOLUME volume = (Native.DEV_BROADCAST_VOLUME) m.GetLParam(typeof(Native.DEV_BROADCAST_VOLUME));
                            IEnumerable<string> drives = Native.MaskToDrives(volume.Unitmask);
                            foreach (string drive in drives)
                            {
                                //AddEvent("Volume " + drive + " added");
                                if (theODD == null || theODD.DriveLetter != drive) // This event can contain the same drive!
                                {
                                    DetectDriveAsync(drive);
                                }

                                if (theODD == null || theODD.DriveLetter != drive ||
                                    (volume.Flags & Native.BroadcastFlags.Media) == 0) continue;

                                // CD-ROM inserted into the drive
                                // Check if this is a PS3 DVD. If it is, enable the rip button
                                AddEvent("Disc inserted");
                                StateMachine(AppStims.DISK_PRESENT);
                                AuthenticateDrive();
                            }
                        }
                        break;
                    case Native.DeviceBroadcastType.QueryRemove:
                        // Prevent windows from ejecting the device safe when we are still ripping
                    case Native.DeviceBroadcastType.RemoveComplete:
                        // Check again if our drive or volume still exists.
                        if (broadcast.DeviceType == Native.DeviceType.Volume)
                        {
                            Native.DEV_BROADCAST_VOLUME volume = (Native.DEV_BROADCAST_VOLUME)m.GetLParam(typeof(Native.DEV_BROADCAST_VOLUME));
                            IEnumerable<string> drives = Native.MaskToDrives(volume.Unitmask).ToList();
                            if (theODD == null && drives.Any())
                            {
                                // Perhaps we can now redetect the drive?
                                foreach (string drive in drives)
                                {
                                    DetectDrives(drive);
                                    if (theODD != null) break;
                                }
                            }

                            if (drives.Any(drive => theODD != null && drive == theODD.DriveLetter))
                            {
                                Debug.WriteLine("This is our authenticated drive! F00l.");

                                if ((volume.Flags & Native.BroadcastFlags.Media) > 0)
                                {
                                    // CD-ROM removed from the drive
                                    AddEvent("Disc removed");
                                    StateMachine(AppStims.NO_DISK_PRESENT);

                                    btnDdlCreateIrd.Visible = false;
                                    btnIRDFile.Visible = true;
                                }
                                else
                                {
                                    if (theODD != null)
                                    {
                                        theODD.Close();
                                        theODD = null;
                                    }
                                    AddEvent("Drive removed!");
                                    StateMachine(AppStims.DRIVE_ABSENT);

                                    btnDdlCreateIrd.Visible = false;
                                    btnIRDFile.Visible = true;
                                }
                            }
                        }
                        break;
                }
            }
            base.WndProc(ref m);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Interaction.Instance.ReportMessage(string.IsNullOrEmpty(Utilities.PublicKey)
                                                   ? "Using default public key"
                                                   : "Using personal public key");

            // Check for updates, if enabled
            if (Settings.Default.CheckForUpdates) Ripp3rUpdate.CheckForUpdate();

            DetectDriveAsync();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Settings.Default.Save();
        }

        private void settingsMenuItem_Click(object sender, EventArgs e)
        {
            using (Ripp3rSettings settings = new Ripp3rSettings())
            {
                bool useDefaultKeys = Settings.Default.UseDefaultKeys;
                if (settings.ShowDialog(this) != DialogResult.OK) return;

                gameTdb.IsEnabled = Settings.Default.GameTDB;
                gameTdb.Language = Settings.Default.GameTDBLanguage;

                if (Settings.Default.UseDefaultKeys != useDefaultKeys)
                    Load3Dump(); // Load the keys
            }
        }

        private async void btnCreateIrdFile_Click(object sender, EventArgs e)
        {
            await CreateIrd(IrdCreator.Create());
        }

        private async void btnCreateIrdFromDrive_Click(object sender, EventArgs e)
        {
            await CreateIrd(IrdCreator.Create(theODD));
        }

        private Task CreateIrd(IrdCreator creator)
        {
            cancellation = new CancellationTokenSource();
            currentTask = creator.CreateIRD(cancellation.Token);
            return currentTask;
        }

        private async void btnCreateIso_Click(object sender, EventArgs e)
        {
            IsoBuilder isoBuilder = new IsoBuilder();

            cancellation = new CancellationTokenSource();
            currentTask =
                TaskEx.Run(
                    async () => await isoBuilder.CreateIso(cancellation.Token));
            await currentTask;
        }

        private async void btnRipGame_Click(object sender, EventArgs e)
        {
            if (Settings.Default.CreateIRDWhileRipping)
            {
                // Call create Ird method
                await CreateIrd(IrdCreator.Create(theODD, true));
            }
            else
            {
                cancellation = new CancellationTokenSource();
                currentTask = TaskEx.Run(async () => await theODD.DoRip(cancellation.Token));
                await currentTask;
            } 
        }

        private async void btnCancel_Click(object sender, EventArgs e)
        {
            cancellation.Cancel();
            AddEvent("Cancelling...");

            // We should wait for the tasks to finish here
            btnCancel.Enabled = false;

            if (currentTask != null)
                await currentTask;

            AddEvent("Current task cancelled");

            StateMachine(AppStims.CANCEL);
            ReportProgress(0); // Reset progressbar
            theAppState = AppStates.READY;

            TaskComplete();
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            eventsListBox.Refresh();
        }

        private async void uploadIRDFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Utilities.PublicKey) && !Settings.Default.AskedForPersonalKey)
            {
                Settings.Default.AskedForPersonalKey = true;
                Settings.Default.Save();

                if (MessageBox.Show(Resources.HavePersonalKey, string.Empty, MessageBoxButtons.YesNo) ==
                    DialogResult.Yes)
                {
                    setPublicKeyToolStripMenuItem_Click(sender, e);
                    if (string.IsNullOrEmpty(Utilities.PublicKey))
                    {
                        Interaction.Instance.ReportMessage("Public key not specified, aborting upload", ReportType.Warning);
                        return;
                    }
                }
            }

            cancellation = new CancellationTokenSource();
            currentTask =
                TaskEx.Run(
                    async () => await IrdUploader.Upload(cancellation.Token));
            await currentTask;
        }

        private void viewIrdFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog ofd = new OpenFileDialog {Filter = Resources.FilterIRDFile})
                {
                    if (ofd.ShowDialog(this) != DialogResult.OK) return;

                    IrdViewer irdViewer = new IrdViewer(ofd.FileName);
                    irdViewer.Show();
                    irdViewer.FormClosed += (o, args) => ((IrdViewer) o).Dispose();
                }
            }
            catch (FileLoadException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnEject_Click(object sender, EventArgs e)
        {
            theODD.Eject();
        }

        private void setPublicKeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (PublicKey pk = new PublicKey())
            {
                pk.ShowDialog(this);
            }
        }

        private void eventsListBox_MouseMove(object sender, MouseEventArgs e)
        {
            var info = eventsListBox.HitTest(e.Location);
            eventsListBox.Cursor = (info.Item == null || info.Item.Tag == null) ? Cursors.Default : Cursors.Hand;
        }

        private void eventsListBox_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (!e.IsSelected) return;
            e.Item.Selected = false;

            if (e.Item.Tag == null) return;

            string url = e.Item.Tag.ToString();
            Debug.WriteLine(url);
            Process.Start(url);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About_Click(sender, e);
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Ripp3rUpdate.CheckForUpdate(true);
        }

        private void licensesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Licenses().ShowDialog(this);
        }

        private async void identifyJBRipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await IsoBuilder.Identify();
        }
    }
}
