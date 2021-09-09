using System;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ShowUI
{
    public partial class frmChangePass : Form
    {
        private ShowUI.Utilities ul = new ShowUI.Utilities();
        private string tmpIps = "";

        //private string conn = "";
        public string userName = "V0917332";

        public string passcode = "SanderPatrick";
        private bool forceChangePasscode = false;

        public frmChangePass(string User, string password, bool _forceChangePasscode)
        {
            userName = User;
            passcode = password;
            forceChangePasscode = _forceChangePasscode;
            InitializeComponent();
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                string svIp = ul.GetServerIP("SSO", "10.224.81.37");
                //MessageBox.Show(svIp);
                tmpIps += " SSO:" + svIp;
                string connectionString = @"Data Source=" + svIp + ";Initial Catalog=SSO;uid=sa;pwd=********;";
                string user_name = txtUser.Text;
                string Pass_old = txtPassOld.Text.Trim();
                string Pass_new = txtPassNew.Text.Trim();
                string Pass_repeat = txtPassNewRepeat.Text.Trim();
                //SqlConnection conn = new SqlConnection(connectionString);
                string err = "";
                if (!PasscodeValidation(Pass_new, ref err))
                {
                    label6.Text = err;
                }
                else
                {
                    //conn.Open();
                    //SqlDataReader reader;
                    string sqlStr = @"select username, Password from Users where username='" + user_name + "'";

                    ToSSO conn = new ToSSO();
                    //SqlCommand cmd = new SqlCommand(sqlStr, conn);
                    DataTable dt = conn.DataTable_Sql(sqlStr, svIp);

                    //reader = cmd.ExecuteReader();
                    if (dt.Rows.Count == 0)
                    {
                        label6.Text = "(*) User name does not exist ! Tài khoản không tồn tại !";
                    }
                    else
                    {
                        string dtPass_old = dt.Rows[0]["Password"].ToString().Trim();

                        if (Pass_old != dtPass_old)
                        {
                            label6.Text = "(*) Password is not correct ! Mật mã không đúng !";
                        }
                        else
                        {
                            if (Pass_new != Pass_repeat)
                            {
                                label6.Text = "(*) Please enter Password new again ! Nhập lại mật mã mới không giống !";
                            }
                            else
                            {
                                string sqlupdate = @"update Users set Password ='" + Pass_new + "', ChangePasscode = 1 where username='" + user_name + "'";
                                conn.Execute_NonSQL(sqlupdate, svIp);
                                label6.Text = "(*) Change Password successfully !";
                                label6.ForeColor = Color.Green;
                                timer1.Enabled = true;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private int countDown = 10;

        private void timer1_Tick(object sender, EventArgs e)
        {
            countDown--;
            label6.Text = "(*) Change Password successfully ! Close in " + countDown + " seconds ...";
            if (countDown <= 0)
            {
                this.Close();
            }
        }

        public bool PasscodeValidation(string pw, ref string err)
        {
            err = "";
            bool checkflag = false;
            if (pw.Length >= 6)
            {
                checkflag = true;
            }
            else
            {
                err = "(*) Password nhỏ hơn 6 kí tự !";
                return checkflag;
            }
            if (Regex.Match(pw, @"[0-9]", RegexOptions.ECMAScript).Success)
                checkflag = true;
            else
            {
                err = "(*) Password không có số !";
                checkflag = false;
                return checkflag;
            }
            if (Regex.Match(pw, @"[a-z]", RegexOptions.ECMAScript).Success && Regex.Match(pw, @"[A-Z]", RegexOptions.ECMAScript).Success)
                checkflag = true;
            else
            {
                checkflag = false;
                err = "(*) Password không có chữ cái viết hoa và thường !";
                return checkflag;
            }
            if (Regex.Match(pw, @"[!,@,#,$,%,^,&,*,(,),_,?,~,-]", RegexOptions.ECMAScript).Success)
                checkflag = true;
            else
            {
                checkflag = false;
                err = "(*) Password không có kí tự đặc biệt !";
                return checkflag;
            }

            return checkflag;
        }

        private void frmChangePass_Load(object sender, EventArgs e)
        {
            txtUser.Text = userName;
            txtPassOld.Text = passcode;
            label8.Text = "{ Password: Lớn hơn 6 kí tự - Gồm chứ hoa, chữ thường,kí tự đặc biệt và số }";

            //txtUser.Enabled = false;
            //txtPassOld.Enabled = false;
            label7.Text = "{ Require to Change Passcode Due to Security Issues }";
            //if (forceChangePasscode)
            //{
            //}
            //else {
            //   label7.Visible = false;
            //}
        }
    }
}