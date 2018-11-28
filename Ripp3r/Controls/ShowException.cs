using System.Windows.Forms;

namespace Ripp3r.Controls
{
    public partial class ShowException : Form
    {
        public ShowException(string content)
        {
            InitializeComponent();

            txtException.Text = content;
        }
    }
}
