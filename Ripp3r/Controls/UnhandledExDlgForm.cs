using System;
using System.Windows.Forms;

namespace Ripp3r.Controls
{
    public partial class UnhandledExDlgForm: Form
    {
        public UnhandledExDlgForm()
        {
            InitializeComponent();
        }

        private void UnhandledExDlgForm_Load(object sender, EventArgs e)
        {
            buttonNotSend.Focus();
            labelExceptionDate.Text = string.Format(labelExceptionDate.Text, DateTime.Now);
            linkLabelData.Left = labelLinkTitle.Right;
        }
    }
}