using Microsoft.Win32;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ShowUI
{
    public partial class TimeDoNotUse : Form
    {
        public TimeDoNotUse()
        {
            InitializeComponent();
        }

        private void TimeDoNotUse_Load(object sender, EventArgs e)
        {
            this.Location = new Point(Screen.PrimaryScreen.Bounds.Right - this.Width, 30);
        }

        private uint second = 0;
        private const string subkey = @"SOFTWARE\NETGEAR\STATION";
        private RegistryKey _OpenKey = Registry.LocalMachine.OpenSubKey(subkey);
        private RegistryKey _StationKey;

        private void CountTime_Tick(object sender, EventArgs e)
        {
            this.TopMost = true;
            second++;
            uint val = second;
            TimeSpan result = TimeSpan.FromSeconds(val);
            string fromTimeString = result.ToString("hh':'mm':'ss");
            lblTime.Text = "Dont Test: " + fromTimeString;
            this.BackColor = Color.Red;
            this.ForeColor = Color.Black;
        }

        private int checkTime = 0;

        private void CheckTest_Tick(object sender, EventArgs e)
        {
            if (checkTime > 60)
            {
                CountTime.Enabled = true;
                CountTime.Start();
            }
            try
            {
                string openkey = _OpenKey.GetValue("OpenKey", "").ToString();
                _StationKey = _OpenKey.OpenSubKey(openkey.Remove(0, 1));
                string testFinish = _StationKey.GetValue("TestFlag", "0").ToString();
                if (testFinish.Contains("1"))
                {
                    checkTime++;
                }
                else
                {
                    lblTime.Text = "Tester In Use";
                    this.BackColor = Color.Green;
                    CountTime.Enabled = true;
                    CountTime.Stop();
                    checkTime = 0;
                }
            }
            catch
            {
                //checkTime++;
                lblTime.Text = "Tester In Use";
                this.BackColor = Color.Green;
                CountTime.Enabled = true;
                CountTime.Stop();
                checkTime = 0;
            }
        }
    }
}