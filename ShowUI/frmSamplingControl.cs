using Microsoft.Win32;
using ShowUIApp;
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace ShowUI
{
    public partial class frmSamplingControl : Form
    {
        private Utilities ul = new Utilities();
        private double iRate = 150.0;
        private double sampleRate = 0.0;
        private string model = "";
        private string product = "";
        private string station = "";
        private string mo = "";
        private string mo_s = "";
        private string show_sampling = "";
        private string sampleStation = "";
        private string checkingStation = "";
        private string stationInfo = "";
        private string srPath = "";
        private string rateFix = "";
        //private ShowUI.SFISB05_SV.Servicepostdata objSfisSv = new ShowUI.SFISB05_SV.Servicepostdata();
        private ShowUI.SFIS_QZ_80.Servicepostdata objSfisSv = new ShowUI.SFIS_QZ_80.Servicepostdata();
        private bool isUpdateQtyFinish = true;

        //bool isFinishThread = false;
        public frmSamplingControl()
        {
            objSfisSv.Timeout = 30000;
            InitializeComponent();
        }

        private void frmSamplingControl_Load(object sender, EventArgs e)
        {
            //string Show_Sampling = IniFile.ReadIniFile("Sampling_Control", "Show_Sampling", "0", srPath).Trim();
            //if (Show_Sampling == "1")
            //{
            //    this.Opacity = 0;
            //}
            CheckForIllegalCrossThreadCalls = false;
            this.Invoke((MethodInvoker)delegate
            {
                btCountDown.Text = "30(s)";
            });
            //ShowUIApp.showUI fr = new showUI();
            //fr.ShowWarningMessage("SAMPLING_CONTROL", "Sampling");
            this.TopMost = true;

            try
            {
                model = ul.GetModel();
                product = ul.GetProduct();
                station = ul.GetStation();
                srPath = @"F:\lsy\Test\DownloadConfig\" + product + ".ini";
                stationInfo = IniFile.ReadIniFile("Sampling_Control", "SamplingStation_CheckingStation", "A_B", srPath);
                string timeOut = IniFile.ReadIniFile("Sampling_Control", "Interval", "4800(s)", srPath);
                //string timeOut = "40(s)";
                string show_wd = IniFile.ReadIniFile("Sampling_Control", "show_window", "0", srPath);

                ul.SetValueByKey("SamplingTimeOut", "1");
                ul.SetValueByKey("Show_sampling", show_wd);
                lbStation.Text = stationInfo;
                this.Invoke((MethodInvoker)delegate
                {
                    btCountDown.Text = timeOut;
                });
            }
            catch (Exception)
            {
            }

            Thread _sSampling = new Thread(Show_sampling);
            _sSampling.IsBackground = true;
            _sSampling.Start();

            Thread _tSampling = new Thread(Sampling);
            _tSampling.IsBackground = true;
            _tSampling.Start();
            //
            tUpdateOutput.Start();
            tCountDown.Start();
        }

        public void Initialize()
        {
        }

        //2017.11.13 Adele update not show sampling_test_window
        public void Show_sampling()
        {
            try
            {
                //show_sampling = ul.GetValueByKey("Show_sampling").ToString();
                string[] st = stationInfo.Split(',');
                for (int i = 0; i < st.Length; i++)
                {
                    string[] stDetail = st[i].Split('/');
                    if (stDetail[1].Trim() == station && stDetail[0].Trim() != "")
                    {
                        //MessageBox.Show(stDetail[1].Trim() + "<-chk ? sple->" + stDetail[0].Trim());
                        checkingStation = stDetail[1].Trim();
                        sampleStation = stDetail[0].Trim();
                        ul.SetValueByKey("Samp_Output", lbSamplingOutput.Text.ToString());
                        ul.SetValueByKey("Samp_CheckOutput", lbCheckingOutput.Text.ToString());
                        sampleRate = Convert.ToDouble(IniFile.ReadIniFile("Sampling_Control", "SamplingRate_" + sampleStation, "0", srPath));
                        rateFix = IniFile.ReadIniFile("Sampling_Control", "SamplingRate_" + sampleStation, "0", srPath);
                        show_sampling = IniFile.ReadIniFile("Sampling_Control", "show_window_" + checkingStation, "", srPath);
                        ul.SetValueByKey("Show_sampling", show_sampling.ToString());

                        if (show_sampling == "0")
                        {
                            this.Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        public void LockShowUI()
        {
            Process[] pro = Process.GetProcessesByName("CloseShowUILock");
            if (pro.Length > 0)
            {
                MessageBox.Show("Close showUI");
                foreach (var item in pro)
                {
                    item.Kill();
                }
            }
            showUI frmShowUI = new showUI();
            var _LockingMessage = "Lỗi: Sản lượng Sampling không đạt! gọi pd!";
            //  ul.SetValueByKey("FixFlag", "0");
            ul.SetValueByKey("StopMachine", "1");
            string Show_Sampling = IniFile.ReadIniFile("Sampling_Control", "Show_Sampling", "0", srPath).Trim();
            string Show_Message = IniFile.ReadIniFile("Sampling_Control", "Show_Message", "Có lỗi trạm test, gọi TE kiểm tra", srPath).Trim();
            if (Show_Sampling.Trim() == "1")
            {
                frmShowUI.ShowWarningMessage(Show_Message, "SAMPLING_CONTROL");
            }
            else
            {
                frmShowUI.ShowWarningMessage(_LockingMessage, "SAMPLING_CONTROL");
            }
        }

        public void Sampling()
        {
            string KeySN = @"SOFTWARE\Netgear\SaveSN\OlyAdeleUse";
            RegistryKey rSN = Registry.LocalMachine.OpenSubKey(KeySN, true);

            try
            {
                //Thread.Sleep(10000);
                isUpdateQtyFinish = false;
                double iCurrentStationTestedDUT = 0;
                double iSampleStationTestedDUT = 0;
                iRate = 150;

                //mo = ul.GetValueByKey("MO");
                mo = rSN.GetValue("MO", "").ToString();
                mo_s = ul.GetValueByKey("SN").ToString();
                string SN = rSN.GetValue("SN", "").ToString();
                string SN_s = ul.GetValueByKey("SN").ToString();

                bool flagOKConditions = false;
                if (stationInfo != "A_B")
                {
                    try
                    {
                        string[] st = stationInfo.Split(',');
                        //MessageBox.Show(st.Length+"");
                        for (int i = 0; i < st.Length; i++)
                        {
                            string[] stDetail = st[i].Split('/');
                            //
                            //MessageBox.Show(stDetail[1].Trim() + "<-chk " + station + "? sple->" + stDetail[0].Trim());
                            if (stDetail[1].Trim() == station && stDetail[0].Trim() != "")
                            {
                                //MessageBox.Show(stDetail[1].Trim() + "<-chk ? sple->" + stDetail[0].Trim());
                                checkingStation = stDetail[1].Trim();
                                sampleStation = stDetail[0].Trim();
                                sampleRate = Convert.ToDouble(IniFile.ReadIniFile("Sampling_Control", "SamplingRate_" + sampleStation, "0", srPath));
                                show_sampling = IniFile.ReadIniFile("Sampling_Control", "show_window_" + checkingStation, "", srPath);
                                ul.event_log("MO check: " + mo.Trim());
                                this.Invoke((MethodInvoker)delegate
                                {
                                    lbStation.Text = sampleStation + "/" + checkingStation;
                                    lblMO.Text = mo.Trim();
                                    //CẦN TEST Ở TRẠM FT4 / NEEED TEST IN FT4 STATION
                                    // lblWarnningTest.Text = "CẦN TEST Ở TRẠM "+sampleStation + "(Sampling) / " + checkingStation;
                                    // lblWarnningTest.Text = "CẦN TEST Ở TRẠM " + sampleStation + "/ NEEED TEST IN " + sampleStation + " STATION";
                                    lblWarnningTest.Text = "Máy sẽ bị khóa sau: ";
                                    //}
                                });

                                ul.SetValueByKey("SamplingStation", sampleStation);
                                flagOKConditions = true;
                                break;
                            }
                        }
                    }
                    catch (Exception re)
                    {//MessageBox.Show(re.ToString());
                    }

                    if (flagOKConditions == true)
                    {
                        //ShowUI.SFISB05_SV.Servicepostdata objSfisSv = new ShowUI.SFISB05_SV.Servicepostdata();
                        string strshiftDate = ul.checkDateShift();
                        string[] sShiftDate = strshiftDate.Split('_');
                        //MessageBox.Show(useFunc);
                        //ADele modify
                        //while (ul.GetValueByKey("SN") == "" || ul.GetValueByKey("SN") == "DEFAULT") // wait untill have sn
                        //{
                        //    Thread.Sleep(759);
                        //}

                        //while (ul.GetValueByKey("MO") == "")
                        //{
                        //    Thread.Sleep(759);
                        //}
                        //

                        //while ((SN == "" || SN == "DEFAULT") && (SN_s == "" || SN_s == "DEFAULT")) // wait untill have sn
                        //{
                        //    Thread.Sleep(759);
                        //}

                        //while (mo == "" && mo_s == "")
                        //{
                        //    Thread.Sleep(759);
                        //}
                        string _now = DateTime.Now.ToString("yyyyMMdd");
                        DataTable dtCurrentStationTestedDUT = objSfisSv.GET_TOTAL_PASSFAIL_MODEL(checkingStation.Trim(), sShiftDate[0].Trim(), _now + "01", _now + "24", ul.GetModel());

                        ul.event_log("Sampling checkingStation: " + dtCurrentStationTestedDUT.Rows.Count + " " + checkingStation.Trim() + " " + sShiftDate[0].Trim() + " " + sShiftDate[1].Trim() + " " + sShiftDate[2].Trim() + " " + mo.Trim());
                        ul.event_log("Product: " + ul.GetModel());

                        //DataTable dtCurrentStationTestedDUT = objSfisSv.GET_TOTAL_PASSFAIL("FT6", "D", "201703090730", "201703091830", mo.Trim());

                        if (dtCurrentStationTestedDUT.Rows.Count != 0)
                        {
                            iCurrentStationTestedDUT = int.Parse(dtCurrentStationTestedDUT.Rows[0][0].ToString());
                        }

                        DataTable dtSampleStationTestedDUT = objSfisSv.GET_TOTAL_PASSFAIL_MODEL(sampleStation, sShiftDate[0].Trim(), _now + "01", _now + "24", ul.GetModel());
                        ul.event_log("Sampling sampleStation: " + ul.GetModel());

                        //MessageBox.Show(iSampleStationTestedDUT + "  " + iSampleStationTestedDUT);
                        if (dtSampleStationTestedDUT.Rows.Count != 0)
                        {
                            iSampleStationTestedDUT = int.Parse(dtSampleStationTestedDUT.Rows[0][0].ToString());
                        }

                        if (iCurrentStationTestedDUT > 0)
                        {
                            iRate = (iSampleStationTestedDUT / iCurrentStationTestedDUT) * 100;
                            iRate = Math.Round(iRate, 2);

                            if (iRate < sampleRate)
                            {
                                ul.event_log("flagOKConditions: " + flagOKConditions);
                                ul.event_log("iCurrentStationTestedDUT: " + iCurrentStationTestedDUT + "IRate: " + iRate + " sampleRate: " + sampleRate);
                                this.Invoke((MethodInvoker)delegate
                                {
                                    this.BackColor = Color.Red;
                                    ul.event_log("SamplingTimeOut: " + ul.GetValueByKey("SamplingTimeOut"));
                                    ul.SetValueByKey("Samp_Output", iSampleStationTestedDUT.ToString());
                                    ul.SetValueByKey("Samp_CheckOutput", iCurrentStationTestedDUT.ToString());
                                });
                                WarningSize();
                            }
                            else
                            {
                                ul.event_log("else: iCurrentStationTestedDUT: " + iCurrentStationTestedDUT + "IRate: " + iRate + " sampleRate: " + sampleRate);
                                this.Invoke((MethodInvoker)delegate
                                {
                                    this.BackColor = Color.Green;
                                });
                                NormalSize();
                            }
                        }
                        this.Invoke((MethodInvoker)delegate
                        {
                            lbSRateSpec.Text = sampleRate + "%";
                            lbSamplingOutput.Text = (iSampleStationTestedDUT + "").Trim().Length <= 0 ? "0" : iSampleStationTestedDUT + "";
                            lbSamplingOutput.TextAlign = ContentAlignment.MiddleRight;
                            lbCheckingOutput.Text = iCurrentStationTestedDUT + "";
                            lbIRate.Text = iRate + "%";//rateFix + "%";
                            lblMO.Text = mo_s.Trim();
                            stw.Stop();
                            stw.Reset();
                        });
                    }
                    else
                    {
                        //this.Invoke((MethodInvoker)delegate
                        //{
                        //    this.Close();
                        //});
                    } //
                }//end ifels
                else
                {
                    //this.Invoke((MethodInvoker)delegate
                    //{
                    //    this.Close();
                    //});
                }
            }
            catch (Exception r)
            {
                lbSRateSpec.Text = sampleRate + "%";
                lbSamplingOutput.Text = "45";
                lbCheckingOutput.Text = "642";
                lbIRate.Text = "7%";
                //MessageBox.Show(r.ToString());
                ul.event_log("SamplingControl xxxx: " + r.ToString());
            }
            //isFinishThread = true;
            //    Thread.Sleep(300000);// check 5min 1times
            //}
            isUpdateQtyFinish = true;
        }

        private int fCountDown = 0;

        private void tCountDown_Tick(object sender, EventArgs e)
        {
            int remainSecond = Convert.ToInt32(btCountDown.Text.Replace("(s)", "").Trim());

            if (remainSecond > 5)
            {
                remainSecond = remainSecond - 1;
                btCountDown.Text = remainSecond + "(s)";
                fCountDown++;
                if (fCountDown >= 35)
                {
                    btCountDown.Enabled = true;
                }
            }
            else
            {
                tCountDown.Enabled = false;

                //Thread _tSampling = new Thread(Sampling);
                //_tSampling.IsBackground = true;
                //_tSampling.Start();

                //while (isFinishThread)
                //{
                //    Thread.Sleep(500);
                //}
                try
                {
                    Sampling();
                }
                catch
                {
                }
                if (iRate < sampleRate)
                {
                    ul.SetValueByKey("SamplingTimeOut", "0");
                    ul.SetValueByKey("Samp_IRate", iRate.ToString());

                    ul.SetValueByKey("StopMachine", "1");
                    ul.event_log("SamplingTimeOut: " + ul.GetValueByKey("SamplingTimeOut") + "StopMachine: " + ul.GetValueByKey("StopMachine"));
                    try
                    {
                        //CheckNTGR checkNtgr = new CheckNTGR();
                        //if (checkNtgr.CheckLockSP())
                        //{
                        Thread _lockUI = new Thread(LockShowUI);
                        _lockUI.IsBackground = true;
                        _lockUI.Start();
                        //}
                    }
                    catch (Exception ex)
                    {
                    }
                }
                else
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        this.BackColor = Color.Green;
                    });
                    ul.SetValueByKey("SamplingTimeOut", "1");
                    ul.SetValueByKey("SamplingCountDown", "0");
                }

                tCountDown.Enabled = true;
                this.Close();
            }
        }

        private Stopwatch stw = new Stopwatch();
        private int fCount = 0;

        private void tUpdateOutput_Tick(object sender, EventArgs e)
        {
            tUpdateOutput.Enabled = false;
            //tUpdateOutput.Interval = 120000;
            tUpdateOutput.Interval = 500;

            if (isUpdateQtyFinish)
            {
                Thread _tSampling = new Thread(Sampling);
                _tSampling.IsBackground = true;
                _tSampling.Start();
            }

            tUpdateOutput.Enabled = true;
        }

        private void btCountDown_Click(object sender, EventArgs e)
        {
            stw.Start();
            if (stw.Elapsed.TotalSeconds >= 35 || fCount == 0)
            {
                fCount = 1; //
                Thread _tSampling = new Thread(Sampling);
                _tSampling.IsBackground = true;
                _tSampling.Start();
                stw.Stop();
                stw.Reset();
            }
        }

        public void NormalSize()
        {
            this.Invoke((MethodInvoker)delegate
            {
                this.Size = new Size(377, 74);
            });
        }

        public void WarningSize()
        {
            this.Invoke((MethodInvoker)delegate
            {
                this.Size = new Size(377, 152);
            });
        }

        private void frmSamplingControl_Shown(object sender, EventArgs e)
        {
            string Show_Sampling = IniFile.ReadIniFile("Sampling_Control", "Show_Sampling", "0", srPath).Trim();
            if (Show_Sampling == "1")
            {
                this.Opacity = 0;
            }
        }
    }
}