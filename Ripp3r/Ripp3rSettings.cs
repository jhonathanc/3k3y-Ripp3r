using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Ripp3r.Properties;

namespace Ripp3r
{
    public partial class Ripp3rSettings : Form
    {
        private readonly Dictionary<string, string> Languages;
        private bool loading;

        public Ripp3rSettings()
        {
            Languages = new Dictionary<string, string>
                {
                    {"EN", "English"},
                    {"JA", "Japanese"},
                    {"FR", "French"},
                    {"DE", "German"},
                    {"ES", "Spanish"},
                    {"IT", "Italian"},
                    {"NL", "Dutch"},
                    {"PT", "Portuguese"},
                    {"RU", "Russian"},
                    {"KO", "Korean"},
                    {"ZHCN", "Chinese (Traditional)"},
                    {"ZHTW", "Chinese (Simplified)"}
                };

            InitializeComponent();

            components = new Container();
            components.Add(folderBrowserDialog);
        }

        private void ZipSettings_Load(object sender, EventArgs e)
        {
            loading = true;

            cbGameTDBLanguage.DataSource = new BindingSource(Languages, null);

            cbCheckForUpdates.Checked = Settings.Default.CheckForUpdates;
            cbCompress.Checked = Settings.Default.compressOutput;
            cbMultipart.Checked = Settings.Default.zipMultipart;
            size.Text = Settings.Default.zipPartSize.ToString(CultureInfo.InvariantCulture);
            cbUnit.SelectedIndex = Settings.Default.zipPartUnit;
            cbOutput.Checked = !string.IsNullOrEmpty(Settings.Default.destinationFolder);
            txtOutput.Text = Settings.Default.destinationFolder;
            numCores.Maximum = Environment.ProcessorCount;

            if (Settings.Default.AmountOfCores != 0)
            {
                cbAmountOfCores.Checked = true;
                numCores.Value = Math.Min(Settings.Default.AmountOfCores, Environment.ProcessorCount);
            }
            cbGameTDB.Checked = Settings.Default.GameTDB;
            cbGameTDBLanguage.SelectedValue = Settings.Default.GameTDBLanguage;
            cbUseDefaultKeys.Checked = Settings.Default.UseDefaultKeys;
            cbCreateIRDWhileRipping.Checked = Settings.Default.CreateIRDWhileRipping;

            cbCompress_CheckedChanged(sender, e);
            cbOutput_CheckedChanged(sender, e);
            cbAmountOfCores_CheckedChanged(sender, e);
            cbGameTDB_CheckedChanged(sender, e);

            loading = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.SelectedPath = txtOutput.Text;
            if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
            {
                txtOutput.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void cbMultipart_CheckedChanged(object sender, EventArgs e)
        {
            size.Enabled = cbUnit.Enabled = cbMultipart.Enabled && cbMultipart.Checked;
        }

        private void cbOutput_CheckedChanged(object sender, EventArgs e)
        {
            txtOutput.Enabled = btnBrowse.Enabled = cbOutput.Checked;
        }

        private void cbCompress_CheckedChanged(object sender, EventArgs e)
        {
            cbMultipart.Enabled = cbCompress.Checked;
            cbMultipart_CheckedChanged(sender, e);
        }

        private void Ripp3rSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult != DialogResult.OK) return;

            bool invalid = false;
            if (cbCompress.Checked)
            {
                if (cbMultipart.Checked)
                {
                    if (!size.Value.HasValue)
                    {
                        MessageBox.Show(
                            @"Please enter a valid value for the size of the multi-part zip file, or disable the multi-part feature.");
                        invalid = true;
                    }
                    else
                    {
                        // Size
                        long cal = (long) (size.Value*(Math.Pow(1024, cbUnit.SelectedIndex)));
                        if (cal < 1024 * 1024)
                        {
                            MessageBox.Show(
                                @"Are you trying to fill up your harddrive with a million of different parts? Be reasonable, and pick a bigger part size.");
                            invalid = true;
                        }
                    }
                }
            }
            if (cbOutput.Checked)
            {
                if (txtOutput.Text.Length < 3)
                {
                    MessageBox.Show(@"Choose a location where to save the files.");
                    invalid = true;
                }
                else if (txtOutput.Text.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                {
                    MessageBox.Show(@"The specified path contains invalid characters. Please correct the path, or disable the custom output directory.");
                    invalid = true;
                }
                else if (txtOutput.Text.Substring(1, 2) != @":\")
                {
                    MessageBox.Show(@"The specified path is relative, please choose an absolute path (starting with <drive>:\).");
                    invalid = true;
                }
                else if (!Directory.Exists(txtOutput.Text))
                {
                    if (
                        MessageBox.Show(@"The output directory does not exist. Shall I create it?", @"Settings",
                                        MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        Directory.CreateDirectory(txtOutput.Text);
                    }
                    else
                        invalid = true;
                }
            }

            e.Cancel = invalid;
            if (invalid)
            {
                DialogResult = DialogResult.None;
                return;
            }

            Settings.Default.CheckForUpdates = cbCheckForUpdates.Checked;
            Settings.Default.compressOutput = cbCompress.Checked;
            if (cbCompress.Checked)
            {
                Settings.Default.zipMultipart = cbMultipart.Checked;
                if (cbMultipart.Checked)
                {
                    if (size.Value.HasValue)
                        Settings.Default.zipPartSize = size.Value.Value;
                    Settings.Default.zipPartUnit = cbUnit.SelectedIndex;
                }
                Settings.Default.zipWinzipCompat = false; // No support for winzip atm, file format is too complex
            }
            Settings.Default.destinationFolder = cbOutput.Checked ? txtOutput.Text : null;
            Settings.Default.AmountOfCores = cbAmountOfCores.Checked ? (int) numCores.Value : 0;
            Settings.Default.GameTDB = cbGameTDB.Checked;
            Settings.Default.GameTDBLanguage = cbGameTDBLanguage.SelectedValue.ToString();
            Settings.Default.UseDefaultKeys = cbUseDefaultKeys.Checked;
            Settings.Default.CreateIRDWhileRipping = cbCreateIRDWhileRipping.Checked;
            Settings.Default.Save();
        }

        private void cbAmountOfCores_CheckedChanged(object sender, EventArgs e)
        {
            numCores.Enabled = cbAmountOfCores.Checked;
        }

        private void cbGameTDB_CheckedChanged(object sender, EventArgs e)
        {
            cbGameTDBLanguage.Enabled = cbGameTDB.Checked;
        }

        private void cbUseDefaultKeys_CheckedChanged(object sender, EventArgs e)
        {
            if (loading) return;

            if (!cbUseDefaultKeys.Checked) return;

            cbUseDefaultKeys.Checked =
                MessageBox.Show(
                    Resources.DefaultKeysWarningSettings,
                    Resources.DefaultKeysCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) ==
                DialogResult.Yes;
        }
    }
}
