using System;
using System.Windows.Forms;

namespace ShowUI
{
    public partial class FrmReport : Form
    {
        private Utilities ul = new Utilities();
        private ShowUI.SFISB05_SV.Servicepostdata objSfisSv = new ShowUI.SFISB05_SV.Servicepostdata();
        private const string _StationKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\NETGEAR\AutoDL";

        public FrmReport()
        {
            InitializeComponent();
        }

        private void BandGridview()
        {
            string Arr = ul.GetValueByKey("ERRORCODE_LIST").ToString();
            //string Arr = Registry.GetValue(_StationKey, "ERRORCODE_LIST", "").ToString();
            //string Pass = Registry.GetValue(_StationKey, "PASS", "").ToString();
            string Pass = ul.GetValueByKey("PASS");
            string Qty = ul.GetValueByKey("QtybyTester");
            string[] ErrList = Arr.Split(',');
            lblPass.Text = Pass;
            lblInput.Text = Qty;
            this.grvErr.Columns.Add("col1", "Error");
            this.grvErr.Columns.Add("col2", "Number");
            int Fail = 0;
            //for (int i = 0; i < ErrList.Length; i++)
            //{
            int i = 0;
            try
            {
                foreach (string Err in ErrList)
                {
                    string NErr = Err.Split(':')[0];
                    string ErrorCount = Err.Split(':')[1];

                    this.grvErr.Rows.Add();
                    this.grvErr[1, i].Value = ErrorCount;
                    this.grvErr[0, i].Value = NErr;
                    Fail = Fail + int.Parse(ErrorCount);
                    i++;
                    // grvErr.Rows.Add(NErr, ErrorCount);
                    //grvErr.Rows.Add(NErr);
                }
                lblFail.Text = Fail.ToString();
            }
            catch (Exception er)
            {
            }
        }

        private void FrmReport_Load(object sender, EventArgs e)
        {
            BandGridview();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ul.SetValueByKey("ERRORCODE_LIST", "");
            ul.SetValueByKey("PASS", "");
        }
    }
}