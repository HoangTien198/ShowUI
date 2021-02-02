using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ShowUI
{
    public partial class frmWSUS : Form
    {
        public frmWSUS()
        {
            InitializeComponent();
        }

        private void frmWSUS_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            this.Location = new Point(Screen.PrimaryScreen.Bounds.Right - this.Width,
                                      Screen.PrimaryScreen.Bounds.Height - (Screen.PrimaryScreen.Bounds.Height - this.Height*2+2));
            string valueUpdateOSReg = "";
            RegistryKey UpdateOSReg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", true);
            try
            {
                valueUpdateOSReg = UpdateOSReg.GetValue("WUServer", "").ToString();
            }
            catch (Exception)
            {

                valueUpdateOSReg = "";
            }

            label1.Text = "WSUS Server: " + valueUpdateOSReg;

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.TopMost = true;
            string valueUpdateOSReg = "";
            RegistryKey UpdateOSReg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", true);
            try
            {
                valueUpdateOSReg = UpdateOSReg.GetValue("WUServer", "").ToString();
            }
            catch (Exception)
            {

                valueUpdateOSReg = "";
            }

            label1.Text = "WSUS Server: " + valueUpdateOSReg;
        }
    }
}
