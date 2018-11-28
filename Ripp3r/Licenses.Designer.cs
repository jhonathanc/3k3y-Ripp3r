namespace Ripp3r
{
    partial class Licenses
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
            this.licensesBrowser = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // licensesBrowser
            // 
            this.licensesBrowser.AllowWebBrowserDrop = false;
            this.licensesBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.licensesBrowser.IsWebBrowserContextMenuEnabled = false;
            this.licensesBrowser.Location = new System.Drawing.Point(0, 0);
            this.licensesBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.licensesBrowser.Name = "licensesBrowser";
            this.licensesBrowser.Size = new System.Drawing.Size(443, 508);
            this.licensesBrowser.TabIndex = 0;
            this.licensesBrowser.WebBrowserShortcutsEnabled = false;
            this.licensesBrowser.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.licensesBrowser_Navigating);
            // 
            // Licenses
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(443, 508);
            this.Controls.Add(this.licensesBrowser);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Licenses";
            this.Text = "Licenses";
            this.Load += new System.EventHandler(this.Licenses_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser licensesBrowser;
    }
}