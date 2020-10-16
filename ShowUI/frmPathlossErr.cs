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
    public partial class frmPathlossErr : Form
    {
        public frmPathlossErr()
        {
            InitializeComponent();
            List<string> dtErr = File.ReadAllLines("SavedList.txt").ToList();
            foreach (var item in dtErr)
            {
                rtErr.AppendText(Environment.NewLine + item);
            }
            this.TopMost = true;
        }

        private void close_Click(object sender, EventArgs e)
        {
            this.Dispose();
            this.Close();
        }
    }
}
