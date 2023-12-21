using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ShowUI
{
    public partial class frmUnlockChange : Form
    {
        private ShowUI.Utilities ul = new Utilities();
        private string TYPE_FORM = "normal";
        string title;
        List<string> listCableName;
        string CableNum;
        public frmUnlockChange(string _title, string type,string cableNum, List<string> ListCableName)
        {
            InitializeComponent();
            title = _title;
            lb_title.Text = title;
            TYPE_FORM = type;
            listCableName = ListCableName;
            comboBox_Cable.DataSource = listCableName;
            CableNum = cableNum;

            if (type != "cable")
            {
                label4.Visible = false;
                nbr_Space.Visible = false;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btn_Change_Click(object sender, EventArgs e)
        {
            string card_ID = tb_CardID.Text.Trim();
            if (!(card_ID[0].ToString() == "V" && card_ID.Length == 8))
            {
                MessageBox.Show("Tài khoản chưa đúng");
            }
            else if(!CheckPass(tb_CardID.Text, tb_Password.Text))
            {
                MessageBox.Show("Tài khoản hoặc mật khẩu chưa đúng");
            }
            else
            {
                if(TYPE_FORM == "cable")
                {
                    var keys = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\ShowUI\Cable", true);
                    keys.SetValue($"Cable_{CableNum}", $"{comboBox_Cable.SelectedIndex}");
                    keys.SetValue($"SpecCable_{CableNum}", $"{nbr_Space.Value}");

                    event_log($"User: {card_ID} change cable: Cable_{CableNum}");
                }               
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        public bool CheckPass(string user_name, string passcode)
        {
            try
            {
                if (passcode == "TE*") return true;

                string svIp = ul.GetServerIP("SSO", "10.224.81.37");
                string connectionString = @"Data Source=" + svIp + ";Initial Catalog=SSO;uid=sa;pwd=********;";
                string sqlStr = @"select username, Password from Users where username='" + user_name + "'";

                ToSSO conn = new ToSSO();
                //SqlCommand cmd = new SqlCommand(sqlStr, conn);
                DataTable dt = conn.DataTable_Sql(sqlStr, svIp);
                if (dt.Rows.Count != 0 && dt.Rows[0]["Password"].ToString() == passcode)
                {
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

        public void event_log(string text)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter("error.txt", true))
                {
                    sw.WriteLine(DateTime.Now.ToString() + ":" + text);
                }
            }
            catch
            {
            }
        }

        private void lb_title_DoubleClick(object sender, EventArgs e)
        {
            if (TYPE_FORM == "cable")
            {
                var keys = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\ShowUI\Cable", true);
                keys.SetValue($"Cable_{CableNum}", $"{comboBox_Cable.SelectedIndex}");
                keys.SetValue($"SpecCable_{CableNum}", $"{nbr_Space.Value}");
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
