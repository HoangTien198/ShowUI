using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ZedGraph;
using Microsoft.Win32;

namespace ShowUIApp
{
    public partial class frmChart : Form
    {
        public frmChart()
        {
            InitializeComponent();
        }

        private void frmChart_Load(object sender, EventArgs e)
        {
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
                x = new double[rrData.Length-1];
                y = new double[rrData.Length-1];
                z = new double[yrData.Length-1];
                t = new double[yrData.Length-1];
                for (int i = 0; i < rrData.Length-1; i++)
                {
                    try
                    {
                        x[i] = i;
                        y[i] = Convert.ToDouble(rrData[i]);
                    }
                    catch
                    { }
                }

                for (int i = 0; i < yrData.Length-1; i++)
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
            RRPane.XAxis.Scale.Max = x.Length-1;
            YRPane.XAxis.Scale.Max = t.Length-1;
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

        private void RRChart_Load(object sender, EventArgs e)
        {

        }
    }
}