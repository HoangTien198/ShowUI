using Microsoft.Win32;
using ShowUIApp;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace ShowUI
{
    public partial class frmCableChange : Form
    {
        private Utilities ul = new Utilities();
        private string pathCable = "";
        private int totalCable = 0;

        public frmCableChange()
        {
            InitializeComponent();
            string KeyModel = @"SOFTWARE\Netgear\AutoDL";
            RegistryKey rSN = Registry.LocalMachine.OpenSubKey(KeyModel, true);
            string local = rSN.GetValue("LOCAL_PATH", "").ToString().Trim();
            if (!local.EndsWith("\\"))
            {
                pathCable = local + "\\" + "CableCtrlTime.ini";
            }
            else
            {
                pathCable = local + "CableCtrlTime.ini";
            }

            totalCable = int.Parse(IniFile.ReadIniFile("ConnectControl", "TotalCable", "0", pathCable));

            cbCable.Items.Clear();
            cbCable.DisplayMember = "Text";
            cbCable.ValueMember = "Value";

            for (int i = 0; i < totalCable; i++)
            {
                string cbName = IniFile.ReadIniFile("CableName", "Cable_" + i, "N/A", pathCable);
                string cbUseTime = GetConnectorUsingTimes("Cable" + i).ToString();
                string maxTime = IniFile.ReadIniFile("MaxTimes", "Cable_" + i, "5000", pathCable);
                if (int.Parse(cbUseTime) < int.Parse(maxTime))
                {
                    continue;
                }
                //string cbTime = IniFile.ReadIniFile("MaxTimes", "Cable_" + i, "5000", pathCable);
                cbCable.Items.Add(new BoxItem { Text = cbName, Value = "Cable_" + i });
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (tbEmp.Text.Trim().Length <= 0)
            {
                MessageBox.Show("Insert Employee ID, Plz");
                return;
            }

            if (cbCable.Text.Trim().Length <= 0)
            {
                MessageBox.Show("Select cable type, Plz");
                return;
            }
            string cbKey = "";
            try
            {
                cbKey = (cbCable.SelectedItem as BoxItem).Value;
            }
            catch (Exception)
            {
                MessageBox.Show("Select cable type, If haven't cable type in select box, plz call TE-Pro Config");
                return;
            }
            //.Split('_').First()+ (cbCable.SelectedItem as BoxItem).Value.Split('_').Last();
            string cbLife = IniFile.ReadIniFile("MaxTimes", cbKey, "5000", pathCable);
            string cbUseTime = GetConnectorUsingTimes(cbKey.Split('_').First() + cbKey.Split('_').Last()).ToString();
            string resion = rtResion.Text;
            string model = ul.GetModel();
            string station = ul.GetStation();
            string dateChange = DateTime.Now.ToString("yyyyMMdd");
            DateTime timeChange = DateTime.Now;
            string cbType = cbCable.Text;
            string sql = $@"INSERT INTO [dbo].[CableData]
           ([EmpId]
           ,[Model]
           ,[Station]
           ,[PCName]
           ,[CableType]
           ,[DateChange]
           ,[TimeChange],[Lifecyle],[UseTime],[Reason])
     VALUES('{tbEmp.Text}','{model}','{station}','{Environment.MachineName}','{cbType}','{dateChange}','{timeChange}','{cbLife}','{cbUseTime}','{resion}')";
            try
            {
                string stringConn = @"Data Source=10.224.81.162,1734;Initial Catalog=CableDB;uid=sa;pwd=********;";
                SqlConnection conn = new SqlConnection(stringConn);
                SqlCommand cmd = new SqlCommand(sql, conn);
                int row = 0;
                conn.Open();

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                row = cmd.ExecuteNonQuery();
                conn.Dispose();
                conn.Close();
                this.Close();
            }
            catch (Exception ex)
            {
                AutoClosingMessageBox.Show("co loi trong qua trinh xu ly: " + ex.Message, "Error Excute", 2000);
            }

            SetConnectorUsingTimes(cbKey.Split('_').First() + cbKey.Split('_').Last());
            Application.Restart();
        }

        public int GetConnectorUsingTimes(string KeyName)
        {
            try
            {
                RegistryKey keys;
                string Product = ul.GetProduct().Trim();
                string Station = ul.GetStation().Trim();
                string Kplace = @"SYSTEM\CurrentControlSet\services" + @"\" + Product + @"\" + Station;
                keys = Registry.LocalMachine.OpenSubKey(Kplace, true);
                int rVal = Convert.ToInt32(keys.GetValue(KeyName, "142").ToString());
                return rVal;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public void SetConnectorUsingTimes(string KeyName)
        {
            try
            {
                RegistryKey keys;
                string Product = ul.GetProduct().Trim();
                string Station = ul.GetStation().Trim();
                string Kplace = @"SYSTEM\CurrentControlSet\services" + @"\" + Product + @"\" + Station;
                keys = Registry.LocalMachine.OpenSubKey(Kplace, true);
                keys.SetValue(KeyName, "0");
            }
            catch (Exception)
            {
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Activate();
        }
    }

    public class BoxItem
    {
        public string Text { get; set; }

        public string Value { get; set; }
    }
}