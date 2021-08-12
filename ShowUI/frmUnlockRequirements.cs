using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using ShowUIApp;

namespace ShowUI
{
    public partial class frmUnlockRequirements : Form
    {
        bool checkDataLowhight = false;
        string errorDetail = "";
        DateTime startTime;
        public frmUnlockRequirements(bool isData,string locdetail,DateTime dt)
        {
            InitializeComponent();

            IDEmp.Hide();
            checkDataLowhight = isData;
            errorDetail = locdetail;
            startTime = dt;
        }

        private void lblClose_Click(object sender, EventArgs e)
        {

            this.Close();
        }
        
        public string GetUser()
        {
            return txtUser.Text;
        }
        public string GetPw()
        {
            return txtPass.Text;
        }
     
        private void btnOK_Click(object sender, EventArgs e)
        {
            
            if(checkDataLowhight == false)
            {
                //force to change password
                string passcode = "";
                bool forceChangePasscode = RequiredChangePasscode(txtUser.Text.Trim(), ref passcode);
                if (forceChangePasscode)
                {
                    frmChangePass frm = new frmChangePass(txtUser.Text.Trim(), passcode, true); //force to change password
                    frm.ShowDialog();
                }
                else
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            else
            {
                
                if (checkHHEmp() == true) {
                    if (reason.Text.Trim().Length==0)
                    {
                        AutoClosingMessageBox.Show("Mời nhập lí do ", "reason", 2000);
                    }
                    else
                    {
                        if (reason.Text.Trim().Length == 1)
                        {
                            if (reason.Text.Contains("1"))
                            {
                                reason.Text = "fisrt run line";
                            }
                            if (reason.Text.Contains("2"))
                            {
                                reason.Text = "fixture issue";  
                            }
                            if (reason.Text.Contains("3"))
                            {
                                reason.Text = "equipment issue";
                            }

                        }
                        ExTable();

                    }


                }
                else
                {
                    AutoClosingMessageBox.Show("Tên đăng nhập hoặc mật khẩu sai, liên hệ TE", "Login Fail", 2000);
                }
            }
            
          
        }
        ShowUI.Utilities ul = new Utilities();
        public void ExTable()
        {
            string sqlEx = "insert into DataUnlockShowUI(EmpID,PC_IP,PC_NAME,Line,Model,ReasonOpen,Station,OpenTime,LockReason,LockTime) values(";
            string value = Int32.Parse(IDEmp.Text)+",'"+ ul.GetNICGatewayIP()+"','"+ Environment.MachineName+"','" + GetLineOfTester().Trim().Remove(0, 1)+"','"+ul.GetProduct()+"','"+reason.Text.Trim()+"','"+ul.GetStation()+"','" + DateTime.Now+"',N'"+ errorDetail + "','"+ startTime + "')";
            string sql = sqlEx + value;
            DbHHEmp hhEmp = new DbHHEmp();
            try
            {
                int ex = hhEmp.Execute_NonSQL(sql, "10.224.81.62,1734");
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception)
            {
                this.DialogResult = DialogResult.No;

            }
            
        }

        public bool checkHHEmp()
        {
    
            string queryEmp = "select * from Employee where HonHaiCode = '" + txtUser.Text.Trim() + "' and PassWord = '" + txtPass.Text.Trim()+"'";

            DbHHEmp hhEmp = new DbHHEmp();
            DataTable dt = hhEmp.DataTable_Sql(queryEmp, "10.224.81.62,1734");
            if (dt.Rows.Count == 0)
            {
                return false;
            }

            IDEmp.Text = dt.Rows[0]["EmpID"].ToString().Trim();
            return true;
        }

        
        public bool RequiredChangePasscode(string user_name, ref string passcode)
        {
            try
            {
                string svIp = ul.GetServerIP("SSO", "10.224.81.37");
                string connectionString = @"Data Source=" + svIp + ";Initial Catalog=SSO;uid=sa;pwd=********;";
                string sqlStr = @"select username, Password from Users where username='" + user_name + "' and ChangePasscode=0 ";

                ToSSO conn = new ToSSO();
                //SqlCommand cmd = new SqlCommand(sqlStr, conn);
                DataTable dt = conn.DataTable_Sql(sqlStr, svIp);
                if (dt.Rows.Count != 0)
                {
                    passcode = dt.Rows[0]["Password"].ToString().Trim();
                    return true;
                }

                else
                    return false;
            }
            catch (Exception)
            {
                return false; // serverdie -> dont do any thing
            }

        }
        
        private void btnCancel_Click(object sender, EventArgs e)
        {

            this.Close();
        }

        private void frmUnlockRequirements_Enter(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void txtPass_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void frmUnlockRequirements_Load(object sender, EventArgs e)
        {
            txtUser.Text = "V0974919";
            txtPass.Text = "123456";
            this.TopMost = true;
        }
        public string GetLineOfTester()
        {

            try
            {
                string line = Environment.MachineName.Substring(Environment.MachineName.IndexOf("L") + 1, 2);
                char[] aline = line.ToCharArray();
                string testline = "";
                int tmpLine = 0;
                for (int i = 0; i < aline.Length; i++)
                {
                    int x;
                    bool isNum = Int32.TryParse(aline[i].ToString(), out x);
                    if (!isNum)
                    {
                        break;
                    }
                    testline += aline[i];
                    tmpLine = Convert.ToInt32(testline);
                }
                if (tmpLine == 0)
                {
                    return "L";
                }
                else
                {
                    return "L" + tmpLine;
                }
            }
            catch (Exception)
            {
                return "L";
                //throw;
            }

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }
}
