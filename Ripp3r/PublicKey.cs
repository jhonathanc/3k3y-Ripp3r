using System.Windows.Forms;

namespace Ripp3r
{
    public partial class PublicKey : Form
    {
        public PublicKey()
        {
            InitializeComponent();
            textBox1.Text = Utilities.PublicKey;
        }

        private void btnOk_Click(object sender, System.EventArgs e)
        {
            Utilities.PublicKey = textBox1.Text;
            Interaction.Instance.ReportMessage(string.IsNullOrEmpty(Utilities.PublicKey)
                                                   ? "Using default public key"
                                                   : "Using personal public key");
        }
    }
}
