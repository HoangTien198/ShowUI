using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ZedGraph;


namespace ShowUI
{
    public partial class frmDebug : Form
    {
        public frmDebug()
        {
            InitializeComponent();
        }


        private void frmDebug_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            string[] rrData, yrData;
            // Lets generate sine and cosine wave
            double[] x, y, z, t;

            RegistryKey _OpenKey = Registry.LocalMachine, _StationKey = Registry.LocalMachine;
            Random random = new Random();
            try
            {
                _OpenKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\NETGEAR\STATION");

                string sOpenKey = _OpenKey.GetValue("OpenKey", "").ToString();

                _StationKey = _OpenKey.OpenSubKey(sOpenKey.Remove(0, 1));
            }
            catch
            { }
            if (_StationKey != null)
            {
                rrData = _StationKey.GetValue("TotRetest", "0").ToString().Split(',');
                yrData = _StationKey.GetValue("TotYield", "0").ToString().Split(',');
                x = new double[rrData.Length - 1];
                y = new double[rrData.Length - 1];
                z = new double[yrData.Length - 1];
                t = new double[yrData.Length - 1];
                for (int i = 0; i < rrData.Length - 1; i++)
                {
                    try
                    {
                        x[i] = i;
                        y[i] = Convert.ToDouble(rrData[i]);
                    }
                    catch
                    { }
                }

                for (int i = 0; i < yrData.Length - 1; i++)
                {
                    try
                    {
                        z[i] = i;
                        t[i] = Convert.ToDouble(yrData[i]);
                    }
                    catch
                    { }
                }
            }
            else
            {
                x = new double[0];
                y = new double[0];
                z = new double[0];
                t = new double[0];
            }
            // This is to remove all plots
            RRChart.GraphPane.CurveList.Clear();
            YRChart.GraphPane.CurveList.Clear();
            // GraphPane object holds one or more Curve objects (or plots)
            GraphPane RRPane = RRChart.GraphPane;
            GraphPane YRPane = YRChart.GraphPane;
            // PointPairList holds the data for plotting, X and Y arrays 
            PointPairList pRR = new PointPairList(x, y);
            PointPairList pYR = new PointPairList(z, t);

            // Add cruves to RRPane object
            LineItem cRR = RRPane.AddCurve(null, pRR, Color.Blue, SymbolType.None);
            LineItem cYR = YRPane.AddCurve(null, pYR, Color.Blue, SymbolType.None);
            cRR.Line.Width = 1.5F;
            cYR.Line.Width = 1.5F;

            cRR.Line.IsSmooth = true;
            cYR.Line.IsSmooth = true;
            //Grid
            RRPane.XAxis.MajorGrid.IsVisible = true;
            RRPane.YAxis.MajorGrid.IsVisible = true;
            YRPane.XAxis.MajorGrid.IsVisible = true;
            YRPane.YAxis.MajorGrid.IsVisible = true;
            RRPane.XAxis.Scale.Max = x.Length - 1;
            YRPane.XAxis.Scale.Max = t.Length - 1;
            RRPane.XAxis.Scale.FontSpec.Size = 18;
            RRPane.YAxis.Scale.FontSpec.Size = 18;
            YRPane.XAxis.Scale.FontSpec.Size = 18;
            YRPane.YAxis.Scale.FontSpec.Size = 18;
            RRPane.Chart.Fill.Color = Color.OrangeRed;
            YRPane.Chart.Fill.Color = Color.GreenYellow;
            RRPane.Fill.Color = Color.Silver;
            YRPane.Fill.Color = Color.Silver;
            RRPane.Title.Text = "Station retest rate";
            RRPane.Title.FontSpec.Size = 20;
            RRPane.XAxis.Title.Text = "Time";
            RRPane.XAxis.Title.FontSpec.Size = 18;

            //RRPane.XAxis.Type = AxisType.LinearAsOrdinal;
            //RRPane.YAxis.Type = AxisType.LinearAsOrdinal;
            RRPane.YAxis.Title.Text = "Percent";
            RRPane.YAxis.Title.FontSpec.Size = 18;
            YRPane.Title.Text = "Yeild rate";
            YRPane.Title.FontSpec.Size = 20;
            YRPane.XAxis.Title.Text = "Time";
            YRPane.XAxis.Title.FontSpec.Size = 18;
            //YRPane.XAxis.Type = AxisType.LinearAsOrdinal;
            //YRPane.YAxis.Type = AxisType.LinearAsOrdinal;
            YRPane.YAxis.Title.Text = "Percent";
            YRPane.YAxis.Title.FontSpec.Size = 18;
            // I add all three functions just to be sure it refeshes the plot.   
            RRChart.AxisChange();
            RRChart.Invalidate();
            RRChart.Refresh();

            YRChart.AxisChange();
            YRChart.Invalidate();
            YRChart.Refresh();
        }

        private void tabDebug_Selected(object sender, TabControlEventArgs e)
        {
            if (e.TabPageIndex == 1) // for tab Debug
            {
                //    if (bgWorkerErrorData.IsBusy != true)
                //    {
                //        bgWorkerErrorData.RunWorkerAsync();
                //    }
                //bgWorkerErrorData.RunWorkerAsync();
                Thread _tLoadDebugData = new Thread(LoadDebugData);
                _tLoadDebugData.IsBackground = true;
                _tLoadDebugData.Start();
            }


        }

        Utilities wl = new Utilities();
        string ate_name, model_name, station_name;
        
        public void LoadDebugData()
        {
            if (!wl.CheckNetworkAvailable())
            {
                ShowUIApp.AutoClosingMessageBox.Show("Network is not available. Please check again!","Warning Message Box",10000);
                return;
            }
            try
            {
                ate_name =  Environment.MachineName;//"B05-L13-PT01";//
                station_name = wl.GetStation().Trim();//"PT1";// 
                model_name = wl.GetModel().Trim();//"CM400-1AZNASV1";// 
                 this.Invoke((MethodInvoker)delegate
                {
                    picBoxSearch.Visible = true;
                    lbMessage.Text = "Searching data for model: " + model_name + " station: " + station_name + " ate: " + ate_name + " ...";
                    dataGridView1.DataSource = null;
                });

              
                B05_SERVICE_CENTER.B05_Service svD = new B05_SERVICE_CENTER.B05_Service();
                DataSet ds = svD.P_B05_ERROR_TRACKING(ate_name, model_name, station_name);

                this.Invoke((MethodInvoker)delegate
                {
                    picBoxSearch.Visible = false;
                    dataGridView1.DataSource = ds.Tables[0];
                    lbMessage.Text = "(*)Total: " + ds.Tables[0].Rows.Count + " records for model: " + model_name + " station: " + station_name + " ate: " + ate_name + "";
                   
                }); 
            }
            catch (Exception r)
            {
                //MessageBox.Show(r.ToString());
            }
            //
        }

        private void bgWorkerErrorData_DoWork(object sender, DoWorkEventArgs e)
        {
            // BackgroundWorker wk = new BackgroundWorker();

            LoadDebugData();
            
        }

        private void bgWorkerErrorData_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            
        }

        private void bgWorkerErrorData_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            string link = e.ToString();

            int rowIdx = e.RowIndex;
            int cellIdx = e.ColumnIndex;
            DataGridViewRow selectedRow = dataGridView1.Rows[rowIdx];
            if (dataGridView1.Columns[e.ColumnIndex].Name =="DETAIL_LOG_PATH")
            {
                
                //MessageBox.Show(selectedRow.Cells[cellIdx].Value.ToString());
            }
           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string model = tbxmodel.Text;
            wl.SetValueByKey("HOTFIX", "SanderPatrick");
            MessageBox.Show("hotfix completed.");
            this.Close();
        }

        private void btApplySetting_Click(object sender, EventArgs e)
        {
            string model = txtModelSetting.Text;
            string station = txtStationSetting.Text;
            //ShowUIApp.IniFile.WriteValue("MAIN_UI", "MODEL_NAME", model, ".\\UISetup.ini");
            //ShowUIApp.IniFile.WriteValue("MAIN_UI", "STATION", station, ".\\UISetup.ini");
           
        }

        //
    }
}
