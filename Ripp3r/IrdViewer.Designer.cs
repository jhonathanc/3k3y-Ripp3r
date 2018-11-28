namespace Ripp3r
{
    partial class IrdViewer
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

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IrdViewer));
            this.tabCtrl = new System.Windows.Forms.TabControl();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.lblAppVersion = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.lblGameName = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.lblIRDHash = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.lblGameVersion = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.lblFooterLength = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.lblHeaderLength = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.lblCrc = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lvHashes = new System.Windows.Forms.ListView();
            this.Id = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Start = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.End = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Hash = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lblUpdateVersion = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lblAmountOfRegions = new System.Windows.Forms.Label();
            this.lblGameId = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tabFiles = new System.Windows.Forms.TabPage();
            this.pbLoadingFiles = new System.Windows.Forms.ProgressBar();
            this.lblLoadingFiles = new System.Windows.Forms.Label();
            this.lvFiles = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabAuth = new System.Windows.Forms.TabPage();
            this.picHex = new Ripp3r.Controls.Hex.HexControl();
            this.label9 = new System.Windows.Forms.Label();
            this.lblData2 = new System.Windows.Forms.Label();
            this.lblData1 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.tabHeader = new System.Windows.Forms.TabPage();
            this.headerHex = new Ripp3r.Controls.Hex.HexControl();
            this.tabFooter = new System.Windows.Forms.TabPage();
            this.footerHex = new Ripp3r.Controls.Hex.HexControl();
            this.pbLoading = new System.Windows.Forms.ProgressBar();
            this.tabCtrl.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.tabFiles.SuspendLayout();
            this.tabAuth.SuspendLayout();
            this.tabHeader.SuspendLayout();
            this.tabFooter.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabCtrl
            // 
            this.tabCtrl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabCtrl.Controls.Add(this.tabGeneral);
            this.tabCtrl.Controls.Add(this.tabFiles);
            this.tabCtrl.Controls.Add(this.tabAuth);
            this.tabCtrl.Controls.Add(this.tabHeader);
            this.tabCtrl.Controls.Add(this.tabFooter);
            this.tabCtrl.Location = new System.Drawing.Point(12, 12);
            this.tabCtrl.Name = "tabCtrl";
            this.tabCtrl.SelectedIndex = 0;
            this.tabCtrl.Size = new System.Drawing.Size(839, 565);
            this.tabCtrl.TabIndex = 0;
            this.tabCtrl.Visible = false;
            // 
            // tabGeneral
            // 
            this.tabGeneral.Controls.Add(this.lblAppVersion);
            this.tabGeneral.Controls.Add(this.label16);
            this.tabGeneral.Controls.Add(this.lblGameName);
            this.tabGeneral.Controls.Add(this.label15);
            this.tabGeneral.Controls.Add(this.lblIRDHash);
            this.tabGeneral.Controls.Add(this.label14);
            this.tabGeneral.Controls.Add(this.lblGameVersion);
            this.tabGeneral.Controls.Add(this.label13);
            this.tabGeneral.Controls.Add(this.lblFooterLength);
            this.tabGeneral.Controls.Add(this.label8);
            this.tabGeneral.Controls.Add(this.lblHeaderLength);
            this.tabGeneral.Controls.Add(this.label10);
            this.tabGeneral.Controls.Add(this.lblCrc);
            this.tabGeneral.Controls.Add(this.label7);
            this.tabGeneral.Controls.Add(this.lvHashes);
            this.tabGeneral.Controls.Add(this.lblUpdateVersion);
            this.tabGeneral.Controls.Add(this.label6);
            this.tabGeneral.Controls.Add(this.lblAmountOfRegions);
            this.tabGeneral.Controls.Add(this.lblGameId);
            this.tabGeneral.Controls.Add(this.lblVersion);
            this.tabGeneral.Controls.Add(this.label4);
            this.tabGeneral.Controls.Add(this.label3);
            this.tabGeneral.Controls.Add(this.label2);
            this.tabGeneral.Controls.Add(this.label1);
            this.tabGeneral.Location = new System.Drawing.Point(4, 25);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabGeneral.Size = new System.Drawing.Size(831, 536);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "General";
            this.tabGeneral.UseVisualStyleBackColor = true;
            // 
            // lblAppVersion
            // 
            this.lblAppVersion.AutoSize = true;
            this.lblAppVersion.Location = new System.Drawing.Point(120, 141);
            this.lblAppVersion.Name = "lblAppVersion";
            this.lblAppVersion.Size = new System.Drawing.Size(12, 17);
            this.lblAppVersion.TabIndex = 23;
            this.lblAppVersion.Text = ".";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(6, 141);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(87, 17);
            this.label16.TabIndex = 22;
            this.label16.Text = "App version:";
            // 
            // lblGameName
            // 
            this.lblGameName.AutoSize = true;
            this.lblGameName.Location = new System.Drawing.Point(120, 73);
            this.lblGameName.Name = "lblGameName";
            this.lblGameName.Size = new System.Drawing.Size(12, 17);
            this.lblGameName.TabIndex = 21;
            this.lblGameName.Text = ".";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(6, 73);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(89, 17);
            this.label15.TabIndex = 20;
            this.label15.Text = "Game name:";
            // 
            // lblIRDHash
            // 
            this.lblIRDHash.AutoSize = true;
            this.lblIRDHash.Location = new System.Drawing.Point(120, 275);
            this.lblIRDHash.Name = "lblIRDHash";
            this.lblIRDHash.Size = new System.Drawing.Size(12, 17);
            this.lblIRDHash.TabIndex = 19;
            this.lblIRDHash.Text = ".";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(6, 275);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(68, 17);
            this.label14.TabIndex = 18;
            this.label14.Text = "IRD Hash";
            // 
            // lblGameVersion
            // 
            this.lblGameVersion.AutoSize = true;
            this.lblGameVersion.Location = new System.Drawing.Point(120, 107);
            this.lblGameVersion.Name = "lblGameVersion";
            this.lblGameVersion.Size = new System.Drawing.Size(12, 17);
            this.lblGameVersion.TabIndex = 17;
            this.lblGameVersion.Text = ".";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(6, 107);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(100, 17);
            this.label13.TabIndex = 16;
            this.label13.Text = "Game version:";
            // 
            // lblFooterLength
            // 
            this.lblFooterLength.AutoSize = true;
            this.lblFooterLength.Location = new System.Drawing.Point(122, 339);
            this.lblFooterLength.Name = "lblFooterLength";
            this.lblFooterLength.Size = new System.Drawing.Size(12, 17);
            this.lblFooterLength.TabIndex = 15;
            this.lblFooterLength.Text = ".";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(8, 339);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(97, 17);
            this.label8.TabIndex = 14;
            this.label8.Text = "Footer Length";
            // 
            // lblHeaderLength
            // 
            this.lblHeaderLength.AutoSize = true;
            this.lblHeaderLength.Location = new System.Drawing.Point(121, 307);
            this.lblHeaderLength.Name = "lblHeaderLength";
            this.lblHeaderLength.Size = new System.Drawing.Size(12, 17);
            this.lblHeaderLength.TabIndex = 13;
            this.lblHeaderLength.Text = ".";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(7, 307);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(103, 17);
            this.label10.TabIndex = 12;
            this.label10.Text = "Header Length";
            // 
            // lblCrc
            // 
            this.lblCrc.AutoSize = true;
            this.lblCrc.Location = new System.Drawing.Point(121, 241);
            this.lblCrc.Name = "lblCrc";
            this.lblCrc.Size = new System.Drawing.Size(12, 17);
            this.lblCrc.TabIndex = 11;
            this.lblCrc.Text = ".";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(7, 241);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(40, 17);
            this.label7.TabIndex = 10;
            this.label7.Text = "CRC:";
            // 
            // lvHashes
            // 
            this.lvHashes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvHashes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Id,
            this.Start,
            this.End,
            this.Hash});
            this.lvHashes.FullRowSelect = true;
            this.lvHashes.Location = new System.Drawing.Point(123, 369);
            this.lvHashes.MultiSelect = false;
            this.lvHashes.Name = "lvHashes";
            this.lvHashes.ShowGroups = false;
            this.lvHashes.Size = new System.Drawing.Size(628, 161);
            this.lvHashes.TabIndex = 9;
            this.lvHashes.UseCompatibleStateImageBehavior = false;
            this.lvHashes.View = System.Windows.Forms.View.Details;
            // 
            // Id
            // 
            this.Id.Text = "Id";
            this.Id.Width = 50;
            // 
            // Start
            // 
            this.Start.Text = "Start";
            this.Start.Width = 120;
            // 
            // End
            // 
            this.End.Text = "End";
            this.End.Width = 120;
            // 
            // Hash
            // 
            this.Hash.Text = "Hash";
            this.Hash.Width = 290;
            // 
            // lblUpdateVersion
            // 
            this.lblUpdateVersion.AutoSize = true;
            this.lblUpdateVersion.Location = new System.Drawing.Point(120, 175);
            this.lblUpdateVersion.Name = "lblUpdateVersion";
            this.lblUpdateVersion.Size = new System.Drawing.Size(12, 17);
            this.lblUpdateVersion.TabIndex = 8;
            this.lblUpdateVersion.Text = ".";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 175);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(108, 17);
            this.label6.TabIndex = 7;
            this.label6.Text = "Update version:";
            // 
            // lblAmountOfRegions
            // 
            this.lblAmountOfRegions.AutoSize = true;
            this.lblAmountOfRegions.Location = new System.Drawing.Point(120, 209);
            this.lblAmountOfRegions.Name = "lblAmountOfRegions";
            this.lblAmountOfRegions.Size = new System.Drawing.Size(12, 17);
            this.lblAmountOfRegions.TabIndex = 6;
            this.lblAmountOfRegions.Text = ".";
            // 
            // lblGameId
            // 
            this.lblGameId.AutoSize = true;
            this.lblGameId.Location = new System.Drawing.Point(120, 41);
            this.lblGameId.Name = "lblGameId";
            this.lblGameId.Size = new System.Drawing.Size(12, 17);
            this.lblGameId.TabIndex = 5;
            this.lblGameId.Text = ".";
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Location = new System.Drawing.Point(120, 13);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(12, 17);
            this.lblVersion.TabIndex = 4;
            this.lblVersion.Text = ".";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 369);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(60, 17);
            this.label4.TabIndex = 3;
            this.label4.Text = "Hashes:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 209);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 17);
            this.label3.TabIndex = 2;
            this.label3.Text = "Regions:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 17);
            this.label2.TabIndex = 1;
            this.label2.Text = "Game Id:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "File version:";
            // 
            // tabFiles
            // 
            this.tabFiles.Controls.Add(this.pbLoadingFiles);
            this.tabFiles.Controls.Add(this.lblLoadingFiles);
            this.tabFiles.Controls.Add(this.lvFiles);
            this.tabFiles.Location = new System.Drawing.Point(4, 25);
            this.tabFiles.Name = "tabFiles";
            this.tabFiles.Padding = new System.Windows.Forms.Padding(3);
            this.tabFiles.Size = new System.Drawing.Size(831, 536);
            this.tabFiles.TabIndex = 3;
            this.tabFiles.Text = "Files";
            this.tabFiles.UseVisualStyleBackColor = true;
            // 
            // pbLoadingFiles
            // 
            this.pbLoadingFiles.Location = new System.Drawing.Point(316, 268);
            this.pbLoadingFiles.Name = "pbLoadingFiles";
            this.pbLoadingFiles.Size = new System.Drawing.Size(198, 23);
            this.pbLoadingFiles.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.pbLoadingFiles.TabIndex = 12;
            // 
            // lblLoadingFiles
            // 
            this.lblLoadingFiles.AutoSize = true;
            this.lblLoadingFiles.Location = new System.Drawing.Point(371, 248);
            this.lblLoadingFiles.Name = "lblLoadingFiles";
            this.lblLoadingFiles.Size = new System.Drawing.Size(88, 17);
            this.lblLoadingFiles.TabIndex = 11;
            this.lblLoadingFiles.Text = "Loading files";
            // 
            // lvFiles
            // 
            this.lvFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader2});
            this.lvFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvFiles.FullRowSelect = true;
            this.lvFiles.Location = new System.Drawing.Point(3, 3);
            this.lvFiles.MultiSelect = false;
            this.lvFiles.Name = "lvFiles";
            this.lvFiles.ShowGroups = false;
            this.lvFiles.Size = new System.Drawing.Size(825, 530);
            this.lvFiles.TabIndex = 10;
            this.lvFiles.UseCompatibleStateImageBehavior = false;
            this.lvFiles.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Filename";
            this.columnHeader1.Width = 377;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Sector";
            this.columnHeader3.Width = 100;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Length";
            this.columnHeader4.Width = 100;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Hash";
            this.columnHeader2.Width = 241;
            // 
            // tabAuth
            // 
            this.tabAuth.Controls.Add(this.picHex);
            this.tabAuth.Controls.Add(this.label9);
            this.tabAuth.Controls.Add(this.lblData2);
            this.tabAuth.Controls.Add(this.lblData1);
            this.tabAuth.Controls.Add(this.label11);
            this.tabAuth.Controls.Add(this.label12);
            this.tabAuth.Location = new System.Drawing.Point(4, 25);
            this.tabAuth.Name = "tabAuth";
            this.tabAuth.Padding = new System.Windows.Forms.Padding(3);
            this.tabAuth.Size = new System.Drawing.Size(831, 536);
            this.tabAuth.TabIndex = 4;
            this.tabAuth.Text = "Authorization";
            this.tabAuth.UseVisualStyleBackColor = true;
            // 
            // picHex
            // 
            this.picHex.AutoScroll = true;
            this.picHex.BackColor = System.Drawing.Color.Transparent;
            this.picHex.Columns = ((byte)(8));
            this.picHex.Font = new System.Drawing.Font("Courier New", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.picHex.ForeColor = System.Drawing.Color.Black;
            this.picHex.Location = new System.Drawing.Point(117, 65);
            this.picHex.Name = "picHex";
            this.picHex.SelectionBackground = System.Drawing.Color.Black;
            this.picHex.SelectionColor = System.Drawing.Color.Yellow;
            this.picHex.Size = new System.Drawing.Size(702, 420);
            this.picHex.Stream = null;
            this.picHex.TabIndex = 11;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 73);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(33, 17);
            this.label9.TabIndex = 10;
            this.label9.Text = "PIC:";
            // 
            // lblData2
            // 
            this.lblData2.AutoSize = true;
            this.lblData2.Location = new System.Drawing.Point(120, 41);
            this.lblData2.Name = "lblData2";
            this.lblData2.Size = new System.Drawing.Size(12, 17);
            this.lblData2.TabIndex = 9;
            this.lblData2.Text = ".";
            this.lblData2.DoubleClick += new System.EventHandler(this.lblData2_DoubleClick);
            // 
            // lblData1
            // 
            this.lblData1.AutoSize = true;
            this.lblData1.Location = new System.Drawing.Point(120, 13);
            this.lblData1.Name = "lblData1";
            this.lblData1.Size = new System.Drawing.Size(12, 17);
            this.lblData1.TabIndex = 8;
            this.lblData1.Text = ".";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 41);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(54, 17);
            this.label11.TabIndex = 7;
            this.label11.Text = "Data 2:";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(6, 13);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(54, 17);
            this.label12.TabIndex = 6;
            this.label12.Text = "Data 1:";
            // 
            // tabHeader
            // 
            this.tabHeader.Controls.Add(this.headerHex);
            this.tabHeader.Location = new System.Drawing.Point(4, 25);
            this.tabHeader.Name = "tabHeader";
            this.tabHeader.Padding = new System.Windows.Forms.Padding(3);
            this.tabHeader.Size = new System.Drawing.Size(831, 536);
            this.tabHeader.TabIndex = 1;
            this.tabHeader.Text = "Header";
            this.tabHeader.UseVisualStyleBackColor = true;
            // 
            // headerHex
            // 
            this.headerHex.AutoScroll = true;
            this.headerHex.BackColor = System.Drawing.Color.Transparent;
            this.headerHex.Columns = ((byte)(8));
            this.headerHex.Dock = System.Windows.Forms.DockStyle.Fill;
            this.headerHex.Font = new System.Drawing.Font("Courier New", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.headerHex.ForeColor = System.Drawing.Color.Black;
            this.headerHex.Location = new System.Drawing.Point(3, 3);
            this.headerHex.Name = "headerHex";
            this.headerHex.SelectionBackground = System.Drawing.Color.Black;
            this.headerHex.SelectionColor = System.Drawing.Color.Yellow;
            this.headerHex.Size = new System.Drawing.Size(825, 530);
            this.headerHex.Stream = null;
            this.headerHex.TabIndex = 0;
            // 
            // tabFooter
            // 
            this.tabFooter.Controls.Add(this.footerHex);
            this.tabFooter.Location = new System.Drawing.Point(4, 25);
            this.tabFooter.Name = "tabFooter";
            this.tabFooter.Padding = new System.Windows.Forms.Padding(3);
            this.tabFooter.Size = new System.Drawing.Size(831, 536);
            this.tabFooter.TabIndex = 2;
            this.tabFooter.Text = "Footer";
            this.tabFooter.UseVisualStyleBackColor = true;
            // 
            // footerHex
            // 
            this.footerHex.AutoScroll = true;
            this.footerHex.BackColor = System.Drawing.Color.Transparent;
            this.footerHex.Columns = ((byte)(8));
            this.footerHex.Dock = System.Windows.Forms.DockStyle.Fill;
            this.footerHex.Font = new System.Drawing.Font("Courier New", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.footerHex.ForeColor = System.Drawing.Color.Black;
            this.footerHex.Location = new System.Drawing.Point(3, 3);
            this.footerHex.Name = "footerHex";
            this.footerHex.SelectionBackground = System.Drawing.Color.Black;
            this.footerHex.SelectionColor = System.Drawing.Color.Yellow;
            this.footerHex.Size = new System.Drawing.Size(825, 530);
            this.footerHex.Stream = null;
            this.footerHex.TabIndex = 1;
            // 
            // pbLoading
            // 
            this.pbLoading.Location = new System.Drawing.Point(332, 283);
            this.pbLoading.Name = "pbLoading";
            this.pbLoading.Size = new System.Drawing.Size(198, 23);
            this.pbLoading.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.pbLoading.TabIndex = 13;
            // 
            // IrdViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(863, 589);
            this.Controls.Add(this.pbLoading);
            this.Controls.Add(this.tabCtrl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(881, 636);
            this.Name = "IrdViewer";
            this.Text = "IrdViewer";
            this.tabCtrl.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.tabGeneral.PerformLayout();
            this.tabFiles.ResumeLayout(false);
            this.tabFiles.PerformLayout();
            this.tabAuth.ResumeLayout(false);
            this.tabAuth.PerformLayout();
            this.tabHeader.ResumeLayout(false);
            this.tabFooter.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabCtrl;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.TabPage tabHeader;
        private System.Windows.Forms.TabPage tabFooter;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblAmountOfRegions;
        private System.Windows.Forms.Label lblGameId;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Label lblUpdateVersion;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ColumnHeader Id;
        private System.Windows.Forms.ColumnHeader Hash;
        private System.Windows.Forms.Label lblCrc;
        private System.Windows.Forms.Label label7;
        private Controls.Hex.HexControl headerHex;
        private Controls.Hex.HexControl footerHex;
        private System.Windows.Forms.Label lblFooterLength;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label lblHeaderLength;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TabPage tabFiles;
        private System.Windows.Forms.ListView lvFiles;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ListView lvHashes;
        private System.Windows.Forms.ColumnHeader Start;
        private System.Windows.Forms.ColumnHeader End;
        private System.Windows.Forms.TabPage tabAuth;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label lblData2;
        private System.Windows.Forms.Label lblData1;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private Controls.Hex.HexControl picHex;
        private System.Windows.Forms.Label lblGameVersion;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label lblIRDHash;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label lblGameName;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label lblAppVersion;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label lblLoadingFiles;
        private System.Windows.Forms.ProgressBar pbLoadingFiles;
        private System.Windows.Forms.ProgressBar pbLoading;
    }
}