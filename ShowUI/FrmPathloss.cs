using ShowUI.AutomationHelper;
using ShowUIApp;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ShowUI
{
    public partial class FrmPathloss : Form
    {
        private string LocalPath = "";

        public FrmPathloss()
        {
            InitializeComponent();
        }

        private void FrmPathloss_Load(object sender, EventArgs e)
        {
            this.Location = new Point(Screen.FromPoint(this.Location).WorkingArea.Left,
                                  Screen.PrimaryScreen.WorkingArea.Height - this.Height);

            label1.Font = new Font("Microsoft Sans Serif", 14, FontStyle.Bold);

            ShowUI.Utilities ul = new ShowUI.Utilities();
            string Modalname = ul.GetProduct();
            string Station = ul.GetStation();
            LocalPath = IniFile.ReadIniFile(Modalname, Station, "empty", @"F:\lsy\ID\PathlossControl\Config\PathLossConfig.txt");
            if (File.Exists("ErrorPathloss.txt"))
            {
                try
                {
                    File.Delete("ErrorPathloss.txt");
                }
                catch (Exception)
                {
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                timer1.Enabled = false;
                timer1.Interval = 600000;
            });
            try
            {
                this.TopMost = true;
                if (File.Exists("ErrorPathloss.txt"))
                {
                    string label = "";
                    var data = File.ReadAllLines("ErrorPathloss.txt").ToList();
                    if (data.Count < 1)
                    {
                        label = "PASS";
                        this.BackColor = Color.Green;
                    }
                    else
                    {
                        foreach (var item in data)
                        {
                            label += item + Environment.NewLine;
                        }
                        this.BackColor = Color.Red;
                    }
                    label1.Text = label;
                }
            }
            catch (Exception)
            {
            }
            this.Invoke((MethodInvoker)delegate
            {
                timer1.Enabled = true;
            });
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            //MessageBox.Show("sadasd");
            this.Invoke((MethodInvoker)delegate
            {
                timer2.Enabled = false;
            });
            try
            {
                ShowUI.Utilities ul = new ShowUI.Utilities();
                string Modalname = ul.GetModel();
                string Station = ul.GetStation();
                AutomationCopyPathlossHelper copyToServerAuto = new AutomationCopyPathlossHelper();
                if (!Directory.Exists(LocalPath))
                {
                    // MessageBox.Show(LocalPath);
                    return;
                }
                bool isLock = false;
                copyToServerAuto.CopyToAutomationServerDB(LocalPath, out isLock);
            }
            catch (Exception)
            {
            }

            this.Invoke((MethodInvoker)delegate
            {
                timer2.Enabled = true;
            });
        }

        private void FrmPathloss_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                ShowUI.Utilities ul = new ShowUI.Utilities();
                string Modalname = ul.GetModel();
                string Station = ul.GetStation();
                AutomationCopyPathlossHelper copyToServerAuto = new AutomationCopyPathlossHelper();
                if (!Directory.Exists(LocalPath))
                {
                    // MessageBox.Show(LocalPath);
                    return;
                }
                bool isLock = false;
                copyToServerAuto.CopyToAutomationServerDB(LocalPath, out isLock);
            }
            catch (Exception)
            {
            }
            try
            {
                this.TopMost = true;
                if (File.Exists("ErrorPathloss.txt"))
                {
                    string label = "";
                    var data = File.ReadAllLines("ErrorPathloss.txt").ToList();
                    if (data.Count < 1)
                    {
                        label = "PASS";
                        this.BackColor = Color.Green;
                    }
                    else
                    {
                        foreach (var item in data)
                        {
                            label += item + Environment.NewLine;
                        }
                        this.BackColor = Color.Red;
                    }
                    label1.Text = label;
                }
            }
            catch (Exception)
            {
            }
        }
    }
}