using System;
using System.Windows.Forms;

namespace ShowUI
{
    public partial class frmTextBox : Form
    {
        public frmTextBox()
        {
            InitializeComponent();
        }

        public string getInput()
        {
            return txtContent.Text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
    }
}