using System;
using System.Windows.Forms;

namespace Ripp3r
{
    internal partial class MyAbout : Form
    {
        public MyAbout()
        {
            InitializeComponent();
        }

        private void MyAbout_Load(object sender, EventArgs e)
        {
            Version v = GetType().Assembly.GetName().Version;
            lblVersion.Text = string.Format(lblVersion.Text, v.ToString(2));

            DateTime dt = new DateTime(2000, 1, 1).AddDays(v.Build).AddSeconds(v.Revision * 2);
            lblBld.Text = string.Format(lblBld.Text, dt);
        }
    }
}
