using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ripp3r.Iso9660;

namespace Ripp3r
{
    internal partial class IrdViewer : Form
    {
        private byte[] d2;
        private bool isdecrypted;

        public IrdViewer(string filename)
        {
            InitializeComponent();

            Task.Factory.StartNew(() => LoadIrd(filename), CancellationToken.None, TaskCreationOptions.None,
                                  TaskScheduler.FromCurrentSynchronizationContext());
        }

        private async void LoadIrd(string filename)
        {
            IrdFile irdFile = await TaskEx.Run(() => IrdFile.Load(filename));

            lblVersion.Text = irdFile.Version.ToString(CultureInfo.InvariantCulture);
            lblUpdateVersion.Text = irdFile.UpdateVersion;
            lblGameId.Text = irdFile.GameID;
            lblGameName.Text = irdFile.GameName;
            lblGameVersion.Text = irdFile.GameVersion;
            lblAppVersion.Text = irdFile.AppVersion;
            lblAmountOfRegions.Text = irdFile.RegionHashes.Count.ToString(CultureInfo.InvariantCulture);

            Region[] regions = await TaskEx.Run(() => irdFile.Header.GetRegions());
            for (int i = 0; i < irdFile.RegionHashes.Count && i < regions.Length; i++)
            {
                ListViewItem itm = new ListViewItem(i.ToString(CultureInfo.InvariantCulture));
                itm.SubItems.Add(regions[i].Start.ToString("X2"));
                itm.SubItems.Add(regions[i].End.ToString("X2"));
                itm.SubItems.Add(irdFile.RegionHashes[i].AsString());
                lvHashes.Items.Add(itm);
            }

            lblData1.Text = irdFile.Data1.AsString();
            lblData2.Text = irdFile.Data2.AsString();
            picHex.Stream = new MemoryStream(irdFile.PIC); // Shouldn't I dispose this one?
            d2 = irdFile.Data2;

            lblHeaderLength.Text =
                (irdFile.Header.Length / Utilities.SectorSize).ToString(CultureInfo.InvariantCulture);
            lblFooterLength.Text =
                (irdFile.Footer.Length / Utilities.SectorSize).ToString(CultureInfo.InvariantCulture);

            headerHex.Stream = irdFile.Header;
            footerHex.Stream = irdFile.Footer;

            lblCrc.Text = BitConverter.GetBytes(irdFile.Crc).AsString();
            lblIRDHash.Text = IrdFile.GetHash(irdFile.FullPath);

            tabCtrl.Visible = true;
            pbLoading.Visible = false;

            lvFiles.BeginUpdate();

            // Load files
            PS3CDReader cdReader = await TaskEx.Run(() => new PS3CDReader(irdFile.Header));
            foreach (var dmi in cdReader.Members.Where(d => d.IsFile).Distinct())
            {
                ListViewItem itm = new ListViewItem(dmi.Path);
                KeyValuePair<long, byte[]> h = irdFile.FileHashes.FirstOrDefault(f => f.Key == dmi.StartSector);
                itm.SubItems.Add(dmi.StartSector.ToString("X2"));
                itm.SubItems.Add(dmi.Length.ToString("X2"));
                itm.SubItems.Add(h.Value.AsString());
                lvFiles.Items.Add(itm);
            }

            lvFiles.EndUpdate();
            lblLoadingFiles.Visible = pbLoadingFiles.Visible = false;
            lvFiles.Visible = true;
        }

        private void lblData2_DoubleClick(object sender, EventArgs e)
        {
            if (isdecrypted)
                ODD.AESEncrypt(Utilities.D2_KEY, Utilities.D2_IV, d2, 0, d2.Length, d2, 0);
            else
                ODD.AESDecrypt(Utilities.D2_KEY, Utilities.D2_IV, d2, 0, d2.Length, d2, 0);
            lblData2.Text = d2.AsString();
            isdecrypted = !isdecrypted;
        }
    }
}
