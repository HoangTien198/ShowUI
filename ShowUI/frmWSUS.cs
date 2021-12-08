using Microsoft.Win32;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
            if (valueWinverion.Trim().Length > 0)
            {
                InsertWinver(valueWinverion);
            }
        }

        public void InsertWinver(string winver)
        {
            string ip = GetIp();
            if (ip.Trim().Length == 0) return;

            DBHelper conn = new DBHelper();
            string SqlCheck = $@"select * from [dbo].[PCInfo] where PCName='{Environment.MachineName}'";
            string SqlUpdate = $@"UPDATE [dbo].[PCInfo] SET [Winver] = '{winver}'  WHERE PCName='{Environment.MachineName}'";

            DataTable checkInfo = conn.DataTable_Sql(SqlCheck, "10.224.81.162,1734", "TestLineInfo");
            if (checkInfo.Rows.Count == 0)
            {
                // conn.Execute_NonSQL(Sql, "10.224.81.162,1734", "TestLineInfo");
            }
            else
            {
                conn.Execute_NonSQL(SqlUpdate, "10.224.81.162,1734", "TestLineInfo");
            }
        }

        public string GetIp()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(x => x.AddressFamily == AddressFamily.InterNetwork).ToList();
            foreach (var ip in host)
            {
                if (ip.ToString().Contains("138.101") || ip.ToString().Contains("172.16")) return ip.ToString();
            }

            return "";
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

        private void frmWSUS_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Hide();
        }
    }
}