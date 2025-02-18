using System;
using System.Windows.Forms;

namespace ShowUIApp
{
    public partial class WarningButton : Form
    {
        public WarningButton()
        {
            InitializeComponent();
        }

        public string getUsername()
        {
            return txtUser.Text;
        }

        public string getPassword()
        {
            return txtPass.Text;
        }

        private void lblClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void WarningButton_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.DialogResult = DialogResult.OK;
            }
        }

        private void WarningButton_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
        }
    }
}