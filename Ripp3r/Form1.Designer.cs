namespace Ripp3r
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            gameTdb.Dispose();
            logFile.Dispose();

            if (theODD != null)
                theODD.Close();

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.saveFileDialogISO = new System.Windows.Forms.SaveFileDialog();
            this.open3DumpFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.DiskTypeDetectionWorker = new System.ComponentModel.BackgroundWorker();
            this.ripProgressBar = new System.Windows.Forms.ProgressBar();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewIrdFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.identifyJBRipToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.uploadIRDFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.setPublicKeyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dumpInquiryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.checkForUpdatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.licensesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lbInSectors = new System.Windows.Forms.Label();
            this.lbSectors = new System.Windows.Forms.Label();
            this.Label2 = new System.Windows.Forms.Label();
            this.lbElapsed = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lbRemaining = new System.Windows.Forms.Label();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnIsoCrypto = new System.Windows.Forms.ToolStripButton();
            this.btnCreateIso = new System.Windows.Forms.ToolStripButton();
            this.btnIRDFile = new System.Windows.Forms.ToolStripButton();
            this.btnDdlCreateIrd = new System.Windows.Forms.ToolStripDropDownButton();
            this.btnCreateIrdFromDrive = new System.Windows.Forms.ToolStripMenuItem();
            this.btnCreateIrdFromFile = new System.Windows.Forms.ToolStripMenuItem();
            this.btnRipGame = new System.Windows.Forms.ToolStripButton();
            this.btnLoadKeys = new System.Windows.Forms.ToolStripButton();
            this.btnEject = new System.Windows.Forms.ToolStripButton();
            this.About = new System.Windows.Forms.ToolStripButton();
            this.btnCancel = new System.Windows.Forms.Button();
            this.DiskDetectionWorker = new System.ComponentModel.BackgroundWorker();
            this.updateProgress = new System.Windows.Forms.Timer(this.components);
            this.eventsListBox = new Ripp3r.Controls.ExtListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // saveFileDialogISO
            // 
            resources.ApplyResources(this.saveFileDialogISO, "saveFileDialogISO");
            // 
            // open3DumpFileDialog
            // 
            this.open3DumpFileDialog.FileName = "3Dump";
            resources.ApplyResources(this.open3DumpFileDialog, "open3DumpFileDialog");
            // 
            // DiskTypeDetectionWorker
            // 
            this.DiskTypeDetectionWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.DiskTypeDetectionWorker_DoWork);
            this.DiskTypeDetectionWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.DiskTypeDetectionWorker_RunWorkerCompleted);
            // 
            // ripProgressBar
            // 
            resources.ApplyResources(this.ripProgressBar, "ripProgressBar");
            this.ripProgressBar.Name = "ripProgressBar";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem,
            this.helpToolStripMenuItem1});
            resources.ApplyResources(this.menuStrip1, "menuStrip1");
            this.menuStrip1.Name = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            resources.ApplyResources(this.fileToolStripMenuItem, "fileToolStripMenuItem");
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            resources.ApplyResources(this.exitToolStripMenuItem, "exitToolStripMenuItem");
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewIrdFileToolStripMenuItem,
            this.identifyJBRipToolStripMenuItem,
            this.toolStripMenuItem1,
            this.uploadIRDFileToolStripMenuItem,
            this.toolStripMenuItem2,
            this.setPublicKeyToolStripMenuItem,
            this.settingsMenuItem,
            this.dumpInquiryToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            resources.ApplyResources(this.helpToolStripMenuItem, "helpToolStripMenuItem");
            // 
            // viewIrdFileToolStripMenuItem
            // 
            this.viewIrdFileToolStripMenuItem.Name = "viewIrdFileToolStripMenuItem";
            resources.ApplyResources(this.viewIrdFileToolStripMenuItem, "viewIrdFileToolStripMenuItem");
            this.viewIrdFileToolStripMenuItem.Click += new System.EventHandler(this.viewIrdFileToolStripMenuItem_Click);
            // 
            // identifyJBRipToolStripMenuItem
            // 
            this.identifyJBRipToolStripMenuItem.Name = "identifyJBRipToolStripMenuItem";
            resources.ApplyResources(this.identifyJBRipToolStripMenuItem, "identifyJBRipToolStripMenuItem");
            this.identifyJBRipToolStripMenuItem.Click += new System.EventHandler(this.identifyJBRipToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            resources.ApplyResources(this.toolStripMenuItem1, "toolStripMenuItem1");
            // 
            // uploadIRDFileToolStripMenuItem
            // 
            this.uploadIRDFileToolStripMenuItem.Name = "uploadIRDFileToolStripMenuItem";
            resources.ApplyResources(this.uploadIRDFileToolStripMenuItem, "uploadIRDFileToolStripMenuItem");
            this.uploadIRDFileToolStripMenuItem.Click += new System.EventHandler(this.uploadIRDFileToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            resources.ApplyResources(this.toolStripMenuItem2, "toolStripMenuItem2");
            // 
            // setPublicKeyToolStripMenuItem
            // 
            this.setPublicKeyToolStripMenuItem.Name = "setPublicKeyToolStripMenuItem";
            resources.ApplyResources(this.setPublicKeyToolStripMenuItem, "setPublicKeyToolStripMenuItem");
            this.setPublicKeyToolStripMenuItem.Click += new System.EventHandler(this.setPublicKeyToolStripMenuItem_Click);
            // 
            // settingsMenuItem
            // 
            this.settingsMenuItem.Name = "settingsMenuItem";
            resources.ApplyResources(this.settingsMenuItem, "settingsMenuItem");
            this.settingsMenuItem.Click += new System.EventHandler(this.settingsMenuItem_Click);
            // 
            // dumpInquiryToolStripMenuItem
            // 
            this.dumpInquiryToolStripMenuItem.Name = "dumpInquiryToolStripMenuItem";
            resources.ApplyResources(this.dumpInquiryToolStripMenuItem, "dumpInquiryToolStripMenuItem");
            this.dumpInquiryToolStripMenuItem.Click += new System.EventHandler(this.DumpInquiry_Click);
            // 
            // helpToolStripMenuItem1
            // 
            this.helpToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.checkForUpdatesToolStripMenuItem,
            this.toolStripMenuItem3,
            this.licensesToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem1.Name = "helpToolStripMenuItem1";
            resources.ApplyResources(this.helpToolStripMenuItem1, "helpToolStripMenuItem1");
            // 
            // checkForUpdatesToolStripMenuItem
            // 
            this.checkForUpdatesToolStripMenuItem.Name = "checkForUpdatesToolStripMenuItem";
            resources.ApplyResources(this.checkForUpdatesToolStripMenuItem, "checkForUpdatesToolStripMenuItem");
            this.checkForUpdatesToolStripMenuItem.Click += new System.EventHandler(this.checkForUpdatesToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            resources.ApplyResources(this.toolStripMenuItem3, "toolStripMenuItem3");
            // 
            // licensesToolStripMenuItem
            // 
            this.licensesToolStripMenuItem.Name = "licensesToolStripMenuItem";
            resources.ApplyResources(this.licensesToolStripMenuItem, "licensesToolStripMenuItem");
            this.licensesToolStripMenuItem.Click += new System.EventHandler(this.licensesToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            resources.ApplyResources(this.aboutToolStripMenuItem, "aboutToolStripMenuItem");
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // lbInSectors
            // 
            resources.ApplyResources(this.lbInSectors, "lbInSectors");
            this.lbInSectors.Name = "lbInSectors";
            // 
            // lbSectors
            // 
            resources.ApplyResources(this.lbSectors, "lbSectors");
            this.lbSectors.Name = "lbSectors";
            // 
            // Label2
            // 
            resources.ApplyResources(this.Label2, "Label2");
            this.Label2.Name = "Label2";
            // 
            // lbElapsed
            // 
            resources.ApplyResources(this.lbElapsed, "lbElapsed");
            this.lbElapsed.Name = "lbElapsed";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // lbRemaining
            // 
            resources.ApplyResources(this.lbRemaining, "lbRemaining");
            this.lbRemaining.Name = "lbRemaining";
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(48, 48);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnIsoCrypto,
            this.btnCreateIso,
            this.btnIRDFile,
            this.btnDdlCreateIrd,
            this.btnRipGame,
            this.btnLoadKeys,
            this.btnEject,
            this.About});
            resources.ApplyResources(this.toolStrip1, "toolStrip1");
            this.toolStrip1.Name = "toolStrip1";
            // 
            // btnIsoCrypto
            // 
            resources.ApplyResources(this.btnIsoCrypto, "btnIsoCrypto");
            this.btnIsoCrypto.Name = "btnIsoCrypto";
            this.btnIsoCrypto.Click += new System.EventHandler(this.btnIsoCrypto_Click);
            // 
            // btnCreateIso
            // 
            resources.ApplyResources(this.btnCreateIso, "btnCreateIso");
            this.btnCreateIso.Name = "btnCreateIso";
            this.btnCreateIso.Click += new System.EventHandler(this.btnCreateIso_Click);
            // 
            // btnIRDFile
            // 
            resources.ApplyResources(this.btnIRDFile, "btnIRDFile");
            this.btnIRDFile.Name = "btnIRDFile";
            this.btnIRDFile.Click += new System.EventHandler(this.btnCreateIrdFile_Click);
            // 
            // btnDdlCreateIrd
            // 
            this.btnDdlCreateIrd.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnCreateIrdFromDrive,
            this.btnCreateIrdFromFile});
            resources.ApplyResources(this.btnDdlCreateIrd, "btnDdlCreateIrd");
            this.btnDdlCreateIrd.Name = "btnDdlCreateIrd";
            // 
            // btnCreateIrdFromDrive
            // 
            this.btnCreateIrdFromDrive.Name = "btnCreateIrdFromDrive";
            resources.ApplyResources(this.btnCreateIrdFromDrive, "btnCreateIrdFromDrive");
            this.btnCreateIrdFromDrive.Click += new System.EventHandler(this.btnCreateIrdFromDrive_Click);
            // 
            // btnCreateIrdFromFile
            // 
            this.btnCreateIrdFromFile.Name = "btnCreateIrdFromFile";
            resources.ApplyResources(this.btnCreateIrdFromFile, "btnCreateIrdFromFile");
            this.btnCreateIrdFromFile.Click += new System.EventHandler(this.btnCreateIrdFile_Click);
            // 
            // btnRipGame
            // 
            resources.ApplyResources(this.btnRipGame, "btnRipGame");
            this.btnRipGame.Name = "btnRipGame";
            this.btnRipGame.Click += new System.EventHandler(this.btnRipGame_Click);
            // 
            // btnLoadKeys
            // 
            resources.ApplyResources(this.btnLoadKeys, "btnLoadKeys");
            this.btnLoadKeys.Name = "btnLoadKeys";
            this.btnLoadKeys.Click += new System.EventHandler(this.btnLoadKeys_Click);
            // 
            // btnEject
            // 
            resources.ApplyResources(this.btnEject, "btnEject");
            this.btnEject.Image = global::Ripp3r.Properties.Resources.Eject;
            this.btnEject.Name = "btnEject";
            this.btnEject.Click += new System.EventHandler(this.btnEject_Click);
            // 
            // About
            // 
            resources.ApplyResources(this.About, "About");
            this.About.Name = "About";
            this.About.Click += new System.EventHandler(this.About_Click);
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // DiskDetectionWorker
            // 
            this.DiskDetectionWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.DiskDetectionWorker_DoWork);
            this.DiskDetectionWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.DiskDetectionWorker_RunWorkerCompleted);
            // 
            // updateProgress
            // 
            this.updateProgress.Interval = 10;
            this.updateProgress.Tick += new System.EventHandler(this.updateProgress_Tick);
            // 
            // eventsListBox
            // 
            this.eventsListBox.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.eventsListBox.FullRowSelect = true;
            this.eventsListBox.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            resources.ApplyResources(this.eventsListBox, "eventsListBox");
            this.eventsListBox.MultiSelect = false;
            this.eventsListBox.Name = "eventsListBox";
            this.eventsListBox.ShowGroups = false;
            this.eventsListBox.UseCompatibleStateImageBehavior = false;
            this.eventsListBox.View = System.Windows.Forms.View.Details;
            this.eventsListBox.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.eventsListBox_ItemSelectionChanged);
            this.eventsListBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.eventsListBox_MouseMove);
            // 
            // columnHeader1
            // 
            resources.ApplyResources(this.columnHeader1, "columnHeader1");
            // 
            // Form1
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.lbRemaining);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lbElapsed);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.lbSectors);
            this.Controls.Add(this.lbInSectors);
            this.Controls.Add(this.ripProgressBar);
            this.Controls.Add(this.eventsListBox);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Activated += new System.EventHandler(this.Form1_Activated);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Ripp3r.Controls.ExtListView eventsListBox;
        private System.Windows.Forms.SaveFileDialog saveFileDialogISO;
        private System.Windows.Forms.OpenFileDialog open3DumpFileDialog;
        private System.ComponentModel.BackgroundWorker DiskTypeDetectionWorker;
        private System.Windows.Forms.ProgressBar ripProgressBar;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsMenuItem;
        private System.Windows.Forms.Label lbInSectors;
        private System.Windows.Forms.Label lbSectors;
        private System.Windows.Forms.Label Label2;
        private System.Windows.Forms.Label lbElapsed;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lbRemaining;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnIsoCrypto;
        private System.Windows.Forms.ToolStripButton btnCreateIso;
        private System.Windows.Forms.ToolStripButton btnIRDFile;
        private System.Windows.Forms.ToolStripButton btnRipGame;
        private System.Windows.Forms.ToolStripButton btnLoadKeys;
        private System.Windows.Forms.ToolStripButton About;
        private System.Windows.Forms.ToolStripMenuItem dumpInquiryToolStripMenuItem;
        private System.Windows.Forms.Button btnCancel;
        private System.ComponentModel.BackgroundWorker DiskDetectionWorker;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.Timer updateProgress;
        private System.Windows.Forms.ToolStripMenuItem viewIrdFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripButton btnEject;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ToolStripMenuItem uploadIRDFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem setPublicKeyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem checkForUpdatesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem licensesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem identifyJBRipToolStripMenuItem;
        private System.Windows.Forms.ToolStripDropDownButton btnDdlCreateIrd;
        private System.Windows.Forms.ToolStripMenuItem btnCreateIrdFromDrive;
        private System.Windows.Forms.ToolStripMenuItem btnCreateIrdFromFile;
    }
}

