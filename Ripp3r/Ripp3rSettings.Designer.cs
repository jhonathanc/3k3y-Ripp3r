namespace Ripp3r
{
    partial class Ripp3rSettings
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
            this.cbUnit = new System.Windows.Forms.ComboBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.cbOutput = new System.Windows.Forms.CheckBox();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.cbAmountOfCores = new System.Windows.Forms.CheckBox();
            this.numCores = new System.Windows.Forms.NumericUpDown();
            this.cbMultipart = new System.Windows.Forms.CheckBox();
            this.cbCompress = new System.Windows.Forms.CheckBox();
            this.cbGameTDB = new System.Windows.Forms.CheckBox();
            this.cbGameTDBLanguage = new System.Windows.Forms.ComboBox();
            this.size = new Ripp3r.Int32TextBox();
            this.cbUseDefaultKeys = new System.Windows.Forms.CheckBox();
            this.cbCheckForUpdates = new System.Windows.Forms.CheckBox();
            this.cbCreateIRDWhileRipping = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.numCores)).BeginInit();
            this.SuspendLayout();
            // 
            // cbUnit
            // 
            this.cbUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbUnit.FormattingEnabled = true;
            this.cbUnit.Items.AddRange(new object[] {
            "B",
            "KB",
            "MB",
            "GB"});
            this.cbUnit.Location = new System.Drawing.Point(302, 61);
            this.cbUnit.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbUnit.Name = "cbUnit";
            this.cbUnit.Size = new System.Drawing.Size(65, 24);
            this.cbUnit.TabIndex = 3;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(293, 256);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 10;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(213, 256);
            this.btnOk.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 9;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.button2_Click);
            // 
            // cbOutput
            // 
            this.cbOutput.AutoSize = true;
            this.cbOutput.Location = new System.Drawing.Point(12, 88);
            this.cbOutput.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbOutput.Name = "cbOutput";
            this.cbOutput.Size = new System.Drawing.Size(198, 21);
            this.cbOutput.TabIndex = 6;
            this.cbOutput.Text = "Custom output destination:";
            this.cbOutput.UseVisualStyleBackColor = true;
            this.cbOutput.CheckedChanged += new System.EventHandler(this.cbOutput_CheckedChanged);
            // 
            // txtOutput
            // 
            this.txtOutput.Location = new System.Drawing.Point(32, 115);
            this.txtOutput.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.Size = new System.Drawing.Size(299, 22);
            this.txtOutput.TabIndex = 7;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(338, 115);
            this.btnBrowse.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(31, 23);
            this.btnBrowse.TabIndex = 8;
            this.btnBrowse.Text = "...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // cbAmountOfCores
            // 
            this.cbAmountOfCores.AutoSize = true;
            this.cbAmountOfCores.Location = new System.Drawing.Point(12, 143);
            this.cbAmountOfCores.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbAmountOfCores.Name = "cbAmountOfCores";
            this.cbAmountOfCores.Size = new System.Drawing.Size(235, 21);
            this.cbAmountOfCores.TabIndex = 11;
            this.cbAmountOfCores.Text = "Limit amount of concurrent tasks";
            this.cbAmountOfCores.UseVisualStyleBackColor = true;
            this.cbAmountOfCores.CheckedChanged += new System.EventHandler(this.cbAmountOfCores_CheckedChanged);
            // 
            // numCores
            // 
            this.numCores.Location = new System.Drawing.Point(252, 143);
            this.numCores.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.numCores.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numCores.Name = "numCores";
            this.numCores.Size = new System.Drawing.Size(80, 22);
            this.numCores.TabIndex = 12;
            this.numCores.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // cbMultipart
            // 
            this.cbMultipart.AutoSize = true;
            this.cbMultipart.Location = new System.Drawing.Point(12, 61);
            this.cbMultipart.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbMultipart.Name = "cbMultipart";
            this.cbMultipart.Size = new System.Drawing.Size(186, 21);
            this.cbMultipart.TabIndex = 1;
            this.cbMultipart.Text = "Create multi-part zipfiles:";
            this.cbMultipart.UseVisualStyleBackColor = true;
            this.cbMultipart.CheckedChanged += new System.EventHandler(this.cbMultipart_CheckedChanged);
            // 
            // cbCompress
            // 
            this.cbCompress.AutoSize = true;
            this.cbCompress.Location = new System.Drawing.Point(12, 34);
            this.cbCompress.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbCompress.Name = "cbCompress";
            this.cbCompress.Size = new System.Drawing.Size(137, 21);
            this.cbCompress.TabIndex = 0;
            this.cbCompress.Text = "Compress output";
            this.cbCompress.UseVisualStyleBackColor = true;
            this.cbCompress.CheckedChanged += new System.EventHandler(this.cbCompress_CheckedChanged);
            // 
            // cbGameTDB
            // 
            this.cbGameTDB.AutoSize = true;
            this.cbGameTDB.Location = new System.Drawing.Point(12, 224);
            this.cbGameTDB.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbGameTDB.Name = "cbGameTDB";
            this.cbGameTDB.Size = new System.Drawing.Size(219, 21);
            this.cbGameTDB.TabIndex = 13;
            this.cbGameTDB.Text = "Enable GameTDB Info/Covers";
            this.cbGameTDB.UseVisualStyleBackColor = true;
            this.cbGameTDB.Visible = false;
            this.cbGameTDB.CheckedChanged += new System.EventHandler(this.cbGameTDB_CheckedChanged);
            // 
            // cbGameTDBLanguage
            // 
            this.cbGameTDBLanguage.DisplayMember = "Value";
            this.cbGameTDBLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbGameTDBLanguage.FormattingEnabled = true;
            this.cbGameTDBLanguage.Location = new System.Drawing.Point(252, 224);
            this.cbGameTDBLanguage.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbGameTDBLanguage.Name = "cbGameTDBLanguage";
            this.cbGameTDBLanguage.Size = new System.Drawing.Size(115, 24);
            this.cbGameTDBLanguage.TabIndex = 14;
            this.cbGameTDBLanguage.ValueMember = "Key";
            this.cbGameTDBLanguage.Visible = false;
            // 
            // size
            // 
            this.size.Location = new System.Drawing.Point(204, 61);
            this.size.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.size.Name = "size";
            this.size.Size = new System.Drawing.Size(92, 22);
            this.size.TabIndex = 2;
            // 
            // cbUseDefaultKeys
            // 
            this.cbUseDefaultKeys.AutoSize = true;
            this.cbUseDefaultKeys.Location = new System.Drawing.Point(12, 170);
            this.cbUseDefaultKeys.Margin = new System.Windows.Forms.Padding(4);
            this.cbUseDefaultKeys.Name = "cbUseDefaultKeys";
            this.cbUseDefaultKeys.Size = new System.Drawing.Size(139, 21);
            this.cbUseDefaultKeys.TabIndex = 15;
            this.cbUseDefaultKeys.Text = "Use Default Keys";
            this.cbUseDefaultKeys.UseVisualStyleBackColor = true;
            this.cbUseDefaultKeys.CheckedChanged += new System.EventHandler(this.cbUseDefaultKeys_CheckedChanged);
            // 
            // cbCheckForUpdates
            // 
            this.cbCheckForUpdates.AutoSize = true;
            this.cbCheckForUpdates.Location = new System.Drawing.Point(12, 9);
            this.cbCheckForUpdates.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbCheckForUpdates.Name = "cbCheckForUpdates";
            this.cbCheckForUpdates.Size = new System.Drawing.Size(145, 21);
            this.cbCheckForUpdates.TabIndex = 16;
            this.cbCheckForUpdates.Text = "Check for updates";
            this.cbCheckForUpdates.UseVisualStyleBackColor = true;
            // 
            // cbCreateIRDWhileRipping
            // 
            this.cbCreateIRDWhileRipping.AutoSize = true;
            this.cbCreateIRDWhileRipping.Location = new System.Drawing.Point(12, 197);
            this.cbCreateIRDWhileRipping.Margin = new System.Windows.Forms.Padding(4);
            this.cbCreateIRDWhileRipping.Name = "cbCreateIRDWhileRipping";
            this.cbCreateIRDWhileRipping.Size = new System.Drawing.Size(181, 21);
            this.cbCreateIRDWhileRipping.TabIndex = 17;
            this.cbCreateIRDWhileRipping.Text = "Create IRD while ripping";
            this.cbCreateIRDWhileRipping.UseVisualStyleBackColor = true;
            // 
            // Ripp3rSettings
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(381, 290);
            this.Controls.Add(this.cbCreateIRDWhileRipping);
            this.Controls.Add(this.cbCheckForUpdates);
            this.Controls.Add(this.cbUseDefaultKeys);
            this.Controls.Add(this.cbGameTDBLanguage);
            this.Controls.Add(this.cbGameTDB);
            this.Controls.Add(this.numCores);
            this.Controls.Add(this.cbAmountOfCores);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtOutput);
            this.Controls.Add(this.cbOutput);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.cbUnit);
            this.Controls.Add(this.size);
            this.Controls.Add(this.cbMultipart);
            this.Controls.Add(this.cbCompress);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Ripp3rSettings";
            this.ShowInTaskbar = false;
            this.Text = "Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Ripp3rSettings_FormClosing);
            this.Load += new System.EventHandler(this.ZipSettings_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numCores)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox cbCompress;
        private System.Windows.Forms.CheckBox cbMultipart;
        private Int32TextBox size;
        private System.Windows.Forms.ComboBox cbUnit;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.CheckBox cbOutput;
        private System.Windows.Forms.TextBox txtOutput;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.CheckBox cbAmountOfCores;
        private System.Windows.Forms.NumericUpDown numCores;
        private System.Windows.Forms.CheckBox cbGameTDB;
        private System.Windows.Forms.ComboBox cbGameTDBLanguage;
        private System.Windows.Forms.CheckBox cbUseDefaultKeys;
        private System.Windows.Forms.CheckBox cbCheckForUpdates;
        private System.Windows.Forms.CheckBox cbCreateIRDWhileRipping;
    }
}