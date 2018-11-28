using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Ripp3r
{
    internal partial class Licenses : Form
    {
        public Licenses()
        {
            InitializeComponent();
        }

        private void Licenses_Load(object sender, EventArgs e)
        {
            Stream s = GetType().Assembly.GetManifestResourceStream("Ripp3r.Licenses.html");
            licensesBrowser.DocumentStream = s;
        }

        private void licensesBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (!e.Url.IsAbsoluteUri || !e.Url.Scheme.StartsWith("http")) return;

            // External link, open in the default browser instead of IE
            Process.Start(e.Url.ToString());
            e.Cancel = true;
        }
    }
}
