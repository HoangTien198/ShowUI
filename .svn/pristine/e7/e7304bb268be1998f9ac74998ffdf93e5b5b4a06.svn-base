using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WannaService
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            WebReference.Servicepostdata test = new WebReference.Servicepostdata();

            DataTable dt1 = new DataTable();
            dt1 = test.GET_NOUPDATE_WANNACRY("TEGW", "138.101");

            string strHostName = "";
            strHostName = Dns.GetHostName();
            string myIP = Dns.GetHostByName(strHostName).AddressList[0].ToString();
            if (dt1.Rows.Count > 0)
            {
                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    if (dt1.Rows[i]["IP"].ToString().Contains(myIP))
                    {
                        //iWanna = false;
                    }
                }
            }
        }
    }
}
