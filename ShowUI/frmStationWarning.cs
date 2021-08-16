using System;
using System.Drawing;
using System.Windows.Forms;

namespace ShowUI
{
    public partial class frmStationWarning : Form
    {
        public frmStationWarning()
        {
            InitializeComponent();
        }

        private void frmStationWarning_Load(object sender, EventArgs e)
        {
            this.Location = new Point(Screen.PrimaryScreen.Bounds.Right - this.Width,
                                      Screen.PrimaryScreen.Bounds.Height - (Screen.PrimaryScreen.Bounds.Height - this.Height));
        }
    }
}