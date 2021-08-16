using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ShowUI
{
    public partial class frmScanID : Form
    {
        private static int SimpleLX, SimpleLY, SimpleH, SimpleW, blCloseX, blCloseY;
        private List<string> ListCableName = new List<string>();

        public frmScanID(List<string> _ListCableName)
        {
            InitializeComponent();
            ListCableName = _ListCableName;
            SimpleLX = this.Location.X;
            SimpleLY = this.Location.Y;
            SimpleH = this.Height;
            SimpleW = this.Width;
            blCloseX = lblClose.Location.X;
            blCloseY = lblClose.Location.Y;
            txtReason.Enabled = false;
            cbxCables.Enabled = false;
            label2.ForeColor = Color.Gray;
            label3.ForeColor = Color.Gray;
        }

        public string getUsername()
        {
            return txtEmpID.Text;
        }

        public string getPassword()
        {
            return txtPw.Text;
        }

        public string getCable()
        {
            return cbxCables.Text;
        }

        public string getReason()
        {
            return txtReason.Text;
        }

        public string getlable()
        {
            return txtlabelCable.Text.Trim();
        }

        public int getTypeChange()
        {
            if (ckChangeByCables.Checked)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        private void lblClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void frmScanID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.DialogResult = DialogResult.OK;
            }
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private ShowUI.Utilities ul = new Utilities();

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

        private void btOk_Click(object sender, EventArgs e)
        {
            string passcode = "";
            bool forceChangePasscode = RequiredChangePasscode(txtEmpID.Text.Trim(), ref passcode);
            if (forceChangePasscode)
            {
                frmChangePass frm = new frmChangePass(txtEmpID.Text.Trim(), passcode, true); //force to change password
                frm.ShowDialog();
            }
            else
            {
                this.DialogResult = DialogResult.OK;
            }
        }

        //2019/01/15 Adele add check cableID - ongoing (chan doi ko muon lam)
        public bool CheckCableID(string ID)
        {
            try
            {
                string sqlID = "";
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void txtPw_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.DialogResult = DialogResult.OK;
            }
        }

        private void frmScanID_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
        }

        private void ckChangeByCables_Click(object sender, EventArgs e)
        {
            txtReason.Enabled = true;
            cbxCables.Enabled = true;
            label2.ForeColor = Color.Red;
            label3.ForeColor = Color.Red;
            cbxCables.Items.Clear();
            for (int i = 0; i < ListCableName.Count; i++)
            {
                cbxCables.Items.Add(ListCableName[i].ToString());
            }
            //int widthScreen = Screen.PrimaryScreen.WorkingArea.Width;
            //int heightScreen = Screen.PrimaryScreen.WorkingArea.Height;
            //this.SetBounds(this.Location.X - (heightScreen / 2 - this.Height), this.Location.Y, widthScreen / 2, this.Height);
            //lblClose.SetBounds(lblClose.Location.X + 233, lblClose.Location.Y, lblClose.Width, lblClose.Height);
        }

        private void ckOverSpec_Click(object sender, EventArgs e)
        {
            txtReason.Enabled = false;
            cbxCables.Enabled = false;
            label2.ForeColor = Color.Gray;
            label3.ForeColor = Color.Gray;
            //this.SetBounds(SimpleLX, SimpleLY, SimpleH, SimpleW);
            //lblClose.SetBounds(blCloseX, blCloseY, lblClose.Width, lblClose.Height);
        }
    }
}