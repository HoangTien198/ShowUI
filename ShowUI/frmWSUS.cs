using Microsoft.Win32;
using System;
using System.Drawing;
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
                                      Screen.PrimaryScreen.Bounds.Height - (Screen.PrimaryScreen.Bounds.Height - this.Height + 2));
            string valueUpdateOSReg = "";
            string valueWinverion = "";
            RegistryKey UpdateOSReg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", true);
            RegistryKey Winverion = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", true);
            try
            {
                valueUpdateOSReg = UpdateOSReg.GetValue("WUServer", "").ToString().Replace("http://", "").Replace(":8530", "");
            }
            catch (Exception ex)
            {
                valueUpdateOSReg = "";
            }
            valueWinverion = Winverion.GetValue("DisplayVersion", "").ToString();
            if (valueWinverion.Trim().Length == 0)
            {
                valueWinverion = Winverion.GetValue("ReleaseId", "").ToString();
            }
            label1.Text = "Winver: " + valueWinverion + " - WSUS: " + valueUpdateOSReg;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //this.TopMost = true;
            //string valueUpdateOSReg = "";
            //RegistryKey UpdateOSReg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", true);
            //try
            //{
            //    valueUpdateOSReg = UpdateOSReg.GetValue("WUServer", "").ToString();
            //}
            //catch (Exception)
            //{
            //    valueUpdateOSReg = "";
            //}

            //label1.Text = "WSUS Server: " + valueUpdateOSReg;
        }
    }
}