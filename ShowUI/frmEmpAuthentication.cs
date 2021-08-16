using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace ShowUI
{
    public partial class frmEmpAuthentication : Form
    {
        private B05_SERVICE_CENTER.B05_Service SVB05 = new B05_SERVICE_CENTER.B05_Service();
        private string loginRole = "";
        private string sName = Environment.MachineName;

        public frmEmpAuthentication(string lRole)
        {
            InitializeComponent();
            loginRole = lRole;
            if (loginRole != "2")
            {
                tbxPw.Enabled = true;
            }
        }

        private void frmEmpAuthentication_Load(object sender, EventArgs e)
        {
        }

        public bool EmpAuthentication(string emp, string pw)
        {
            bool isValid = false;
            if (SVB05.P_B05_TE_CHECK_USERS(emp, pw))
            {
                isValid = true;
            }
            //if ((emp.StartsWith("V") || emp.StartsWith("F")) && emp.Length >= 8)
            //{
            //    char[] tmp = emp.ToCharArray();
            //    for (int i = 1; i < tmp.Length; i++)
            //    {
            //        int number;
            //        isValid = Int32.TryParse(tmp[i].ToString(), out number);
            //    }
            //}
            return isValid;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            tbxEmp.Focus();
            GotoSite();
        }

        public void GotoSite()
        {
            try
            {
                string emp = tbxEmp.Text.Trim();
                string pw = tbxPw.Text.Trim();
                lbMsg.Visible = true;

                if (EmpAuthentication(emp, pw))
                {
                    lbMsg.ForeColor = Color.Wheat;
                    lbMsg.Text = "(*) validating completed.";
                    string model_name = GetValueByKey("SFISMODEL").Trim();
                    string ate_name = sName;
                    string error_code = GetValueByKey("ERRORCODE").Trim();

                    string webAddress = "http://10.224.81.63/CenterAction/Action/AddActionAbnormal.aspx?ModelName=" + model_name + "&TesterName=" + ate_name + "&ErrorCode=" + error_code + "&DateTime=" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "&BU=B05&Owner=" + emp + "";// account to autologin & redirect to ShowData folder
                    Process.Start(webAddress);
                    this.Close();
                }
                else
                {
                    lbMsg.ForeColor = Color.Red;
                    lbMsg.Text = "(*) employeeid is not valid. authenticating false.";
                }
            }
            catch (Exception)
            {
            }
        }

        public string GetValueByKey(string _key)
        {
            try
            {
                RegistryKey kiwi = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Netgear\STATION", true);
                string[] _Key = kiwi.GetValue("OpenKey", null).ToString().Split('\\');
                string _Model = _Key[1];
                string _Sta = _Key[2];
                kiwi = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Netgear\STATION" + "\\" + _Model + "\\" + _Sta, true);
                string SN = kiwi.GetValue(_key, "").ToString();
                return SN;
            }
            catch (Exception)
            {
                return "";
                //throw;
            }
        }

        private void btGo_Click(object sender, EventArgs e)
        {
            //NoticeBell b = new NoticeBell();
            //b.ShowDialog();
            GotoSite();
        }

        private void tbxPw_TextChanged(object sender, EventArgs e)
        {
            GotoSite();
        }

        private void lbPassword_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (tbxPw.Enabled == false)
            {
                tbxPw.Enabled = true;
                tbxPw.BackColor = Color.Wheat;
            }
            else
            {
                tbxPw.Enabled = false;
                tbxPw.BackColor = Color.Ivory;
            }
        }

        public void CallWebservice()
        {
        }

        ///
    }
}