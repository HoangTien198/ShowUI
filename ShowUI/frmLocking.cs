using Fii;
using Microsoft.Win32;
using ShowUIApp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ShowUI
{
    public partial class frmLocking : Form
    {
        private const string subkey = @"SOFTWARE\NETGEAR\STATION";

        private string _KindOfError = "";
        private string _ErrorDetail = "";

        //string YRStopSpec = "";
        //string RTRStopSpec = "";
        private string client_ip = "";

        private string _STARTTIME;
        private List<string> ListStationIp = new List<string>();
        private bool _UseSpecMode;
        private string TestDUTCount = "";
        private int _TestedDUT = 0;
        private string _globalUsedMode = "0";
        private string _BufferDUT = "50";
        private DateTime startTime = DateTime.Now;

        //private string conn;
        private string connectionString;

        private int idFii;

        public frmLocking()
        {
            InitializeComponent();
            lbBoom.Text = "Cable over spec! gọi Te!";
        }

        public frmLocking(string _LockCondition, string _Error, bool UseSpecMode, int TestedDUT, string globalUsedMode, string BufferDUT, int _idFii)
        {
            InitializeComponent();
            tbl_unlockData.Hide();
            tbl_unlockData.Visible = false;
            lbBoom.Text = _LockCondition;
            _ErrorDetail = _LockCondition;
            _KindOfError = _Error;
            idFii = _idFii;
            _STARTTIME = DateTime.Now.ToString("yyyyMMddHHmmss");
            string EStation = ul.GetValueByKey("ErrStation");
            //ListStationIp = ListIp;
            _UseSpecMode = UseSpecMode;
            _TestedDUT = TestedDUT;
            _globalUsedMode = globalUsedMode;
            _BufferDUT = BufferDUT;
            if (_KindOfError.Contains("StopAll"))
            {
                //tbUnlock.Visible = false;
                if (EStation == "" && ul.GetValueByKey("ERROR_CODE") == "")
                {
                    lblClose.Visible = false;
                    groupBox2.Visible = false;
                }
            }
            if (_KindOfError.Contains("USB") || _KindOfError.Contains("Virus") || _KindOfError.Contains("Cable") || _KindOfError.Contains("SPECIALLOCK") || _KindOfError.Contains("low"))
            {
                lblClose.Visible = false;
            }

            if (_KindOfError.Contains("low"))
            {
                tbl_unlockData.Visible = true;
                tbl_unlockData.Show();
                tbUnlock.Hide();
            }

            tbl_unlockData.Visible = true;
            tbl_unlockData.Show();

            tbUnlock.Hide();

            TestDUTCount = _BufferDUT;
            //
        }

        private string FixEmpID = "";
        private bool flagInsert = false;
        private ShowUI.SvFPS.WebService svSaveErrs = new SvFPS.WebService();
        private ShowUI.Utilities ul = new ShowUI.Utilities();
        private ShowUI.GET_ATE_SUM_TESTED_DUT.Servicepostdata svWIPGroup = new ShowUI.GET_ATE_SUM_TESTED_DUT.Servicepostdata();
        private ShowUI.SFISB05_SV.Servicepostdata sfisB05 = new SFISB05_SV.Servicepostdata();
        private ShowUI.B05_SERVICE_CENTER.B05_Service CENTER_B05_SV = new B05_SERVICE_CENTER.B05_Service();

        private void tbUnlock_Click(object sender, EventArgs e)
        {
            try
            {
                if (tbxAction.Text != "")
                {
                    using (frmUnlockRequirements frm = new frmUnlockRequirements(false, "", startTime))
                    {
                        this.TopMost = false;
                        frm.TopMost = true;

                        if (frm.ShowDialog() == DialogResult.OK)
                        {
                            string EmpID = frm.GetUser().Trim();
                            string Pw = frm.GetPw().Trim();
                            string Count4Error = "";
                            if (EmpID == "" || Pw == "")
                            {
                                AutoClosingMessageBox.Show("(*) Em.ID/ Password is empty! Try again! Mã NV/ Mật không để trống! Thử lại!", "AutoCloseMessageBox", 10000);
                            }
                            else
                            {
                                FixEmpID = EmpID;

                                SqlConnection conn = new SqlConnection(connectionString);

                                try
                                {
                                    conn.Open();
                                    SqlDataReader reader;
                                    string sqlStr = @"select * from Users where username='" + EmpID + "' and password = '" + Pw + "'";
                                    SqlCommand cmd = new SqlCommand(sqlStr, conn);
                                    SqlDataAdapter da = new SqlDataAdapter(sqlStr, conn);
                                    DataSet ds = new DataSet();
                                    String model = ul.GetProduct();
                                    da.Fill(ds);
                                    reader = cmd.ExecuteReader();
                                    if (reader.HasRows)
                                    {
                                        //Alan 31-03-2018

                                        if (_KindOfError.Contains("OverTime"))
                                        {
                                            if (ds.Tables[0].Rows[0]["Dep"].ToString().Contains("TE-Setup"))
                                            {
                                                ul.SetValueByKey("StopMachine", "0");
                                                ul.SetValueByKey("FixFlag", "1");
                                                SetStopMachineStatus(true);
                                            }
                                            else
                                            {
                                                AutoClosingMessageBox.Show("Không đủ quyền xử lý. Gọi TE-SetUp!", "AutoCloseMessageBox", 10000);
                                                //ul.event_log("OverTime: " + client_ip + sName + GetLineOfTester().Trim() + ul.GetModel() + ul.GetStation() + EmpID);
                                                //ul.SetValueByKey("StopMachine", "1");
                                                //ul.SetValueByKey("FixFlag", "1");
                                            }
                                        }
                                        // End Alan 31-03-2018

                                        //2017.03.014
                                        if (_KindOfError.Contains("SAMPLING_CONTROL"))
                                        {
                                            if (ds.Tables[0].Rows[0]["Dep"].ToString().Contains("QA") || model.Contains("U12I345") || model.Contains("U12I370"))
                                            {
                                                ul.SetValueByKey("StopMachine", "0");
                                                ul.SetValueByKey("FixFlag", "1");
                                                ul.SetValueByKey("SamplingTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")); // delay 1h
                                                string iRate = ""; string S_Output = ""; string S_CheckOutput = "";
                                                iRate = ul.GetValueByKey("Samp_IRate");
                                                S_Output = ul.GetValueByKey("Samp_Output");
                                                S_CheckOutput = ul.GetValueByKey("Samp_CheckOutput");
                                                string Str_Sampling = "Rate: " + iRate + "Samp_Output: " + S_Output + "Samp_CheckOutput: " + S_CheckOutput;
                                                AutoClosingMessageBox.Show("Dữ liệu unlock đã được lưu. Thanks " + EmpID + "!\nBạn có 1h để test đủ số sản phẩm cần thiết!", "AutoCloseMessageBox", 10000);
                                                ul.event_log("SAMPLING_UNLOCK" + client_ip + sName + GetLineOfTester().Trim() + ul.GetModel() + ul.GetStation() + EmpID + Str_Sampling);
                                                Thread _tSaveUnlockDataSampling = new Thread(() => SaveUnlockDataSampling(_KindOfError, EmpID, tbxAction.Text + "", Str_Sampling));
                                                _tSaveUnlockDataSampling.IsBackground = true;
                                                _tSaveUnlockDataSampling.Start();
                                                SetStopMachineStatus(true);
                                                // FiiData.UpdateFii(idFii, tbxAction.Text, EmpID);
                                            }
                                            else
                                            {
                                                string iRate = ""; string S_Output = ""; string S_CheckOutput = "";
                                                iRate = ul.GetValueByKey("Samp_IRate");
                                                S_Output = ul.GetValueByKey("Samp_Output");
                                                S_CheckOutput = ul.GetValueByKey("Samp_CheckOutput");
                                                string Str_Sampling = "Rate: " + iRate + "Samp_Output: " + S_Output + "Samp_CheckOutput: " + S_CheckOutput;

                                                ul.event_log("SAMPLING_UNLOCK: " + client_ip + sName + GetLineOfTester().Trim() + ul.GetModel() + ul.GetStation() + EmpID + Str_Sampling);
                                                //ul.SetValueByKey("StopMachine", "1");
                                                //ul.SetValueByKey("FixFlag", "1");
                                            }
                                        }

                                        // for locking tester mannually
                                        if (_KindOfError.Contains("SPECIALLOCK"))
                                        {
                                            //if (ds.Tables[0].Rows[0]["uRole"].ToString() == "10")
                                            //{
                                            ul.SetValueByKey("StopMachine", "0");
                                            string Key = GetWebUnlockPath("ATELOCKEY").Trim();
                                            ShowUI.B05_SERVICE_CENTER.B05_Service SvLock = new ShowUI.B05_SERVICE_CENTER.B05_Service();
                                            string lockReason = "";
                                            if (tbxAction.Text.Contains("Click here to open virtual"))
                                            {
                                                lockReason = "Tester is fixed already";
                                            }
                                            else
                                            {
                                                lockReason = tbxAction.Text;
                                            }

                                            int lockResult = SvLock.SHOWUI_SPECIAL_LOCK(Key, client_ip, "", sName, "0", "", FixEmpID, lockReason);

                                            if (lockResult == 1)
                                            {
                                                AutoClosingMessageBox.Show("Thông tin mở máy đã được lưu. Mở khóa máy thành công!", "Notice Message Box", 5000);
                                                SetWebUnlockPath("SPECIALLOCK", Key);

                                                //FiiData.UpdateFii(idFii, tbxAction.Text, EmpID);
                                            }
                                            SetStopMachineStatus(true);
                                            Application.Exit();
                                            this.Close();
                                        }

                                        //end for special lock

                                        //end for special lock
                                        //for locking if dont having golden data
                                        if (_KindOfError.Contains("Golden Data"))
                                        {
                                            //SetValueByKey("FixFlag", "1");
                                            ul.SetValueByKey("StopMachine", "0");
                                            AutoClosingMessageBox.Show("Testing is aborted until having testing golden data! Thanks " + EmpID + "!", "AutoCloseMessageBox", 10000);
                                            SetStopMachineStatus(true);
                                            ul.ShellExecute(@"TASKKILL /IM NetgearAutoDL.exe /F /T");
                                            ul.ShellExecute(@"TASKKILL /IM " + ul.GetValueByKey("TPG_NAME") + " /F /T");
                                            ul.ShellExecute(@"TASKKILL /IM ShowUI.exe /F /T");

                                            this.Close();
                                        }

                                        //end
                                        //for locking if dont update showui data
                                        if (_KindOfError.Contains("showui data"))
                                        {
                                            ul.SetValueByKey("StopMachine", "0");
                                            ul.SetValueByKey("FixFlag", "1");
                                            AutoClosingMessageBox.Show("Bạn có 15 phút để thực hiện sửa lỗi ! Thanks " + EmpID + "!", "AutoCloseMessageBox", 10000);
                                            SetStopMachineStatus(true);

                                            this.Close();
                                        }

                                        // for checking interference

                                        if (_KindOfError.Contains("interference data"))
                                        {
                                            ul.SetValueByKey("StopMachine", "0");
                                            ul.SetValueByKey("FixFlag", "1");
                                            AutoClosingMessageBox.Show("Bạn có 15 phút để thực hiện sửa lỗi ! Thanks " + EmpID + "!", "AutoCloseMessageBox", 10000);
                                            InterferenceStart();

                                            this.Close();
                                        }

                                        //22.01.2019 Adele lock all station when ErrorCode
                                        if (_KindOfError.Contains("StopAll") && ul.GetValueByKey("ErrStation") != "")
                                        {
                                            if (ds.Tables[0].Rows[0]["Dep"].ToString().Contains("QA"))
                                            {
                                                ul.SetValueByKey("StopMachine", "0");
                                                ul.SetValueByKey("ErrStation", "");
                                                ul.SetValueByKey("ERROR_CODE", "");
                                                SetWebUnlockPath("StopStation", "tmpDefault");
                                                AutoClosingMessageBox.Show("Xin vui lòng sửa lỗi ErrorCode " + ul.GetValueByKey("ErrStation") + " ! Thanks " + EmpID + "!", "AutoCloseMessageBox", 10000);
                                                //InterferenceStart();
                                                SetStopMachineStatus(true);

                                                this.Close();
                                            }
                                            else
                                            {
                                                AutoClosingMessageBox.Show("Không đủ quyền xử lý. Gọi PQE/IPQC!", "AutoCloseMessageBox", 10000);
                                            }
                                        }

                                        ////2019/02/13 Adele Lock when CMTS issue
                                        if (_KindOfError.Contains("CMTS"))
                                        {
                                            if (ds.Tables[0].Rows[0]["Dep"].ToString().Contains("TE-Setup"))
                                            {
                                                ul.SetValueByKey("StopMachine", "0");
                                                ul.SetValueByKey("CMTS_ISSUE", "");
                                                AutoClosingMessageBox.Show("Xin vui lòng sửa lỗi CMTS! Thanks " + EmpID + "!", "AutoCloseMessageBox", 10000);
                                                //InterferenceStart();
                                                SetStopMachineStatus(true);

                                                this.Close();
                                            }
                                            else
                                            {
                                                AutoClosingMessageBox.Show("Không đủ quyền xử lý. Gọi TE-Setup!", "AutoCloseMessageBox", 10000);
                                            }
                                        }

                                        //for check ate checklist & tpg version not correct
                                        if (_KindOfError.Contains("tpgversion data"))
                                        {
                                            ul.SetValueByKey("StopMachine", "0");
                                            ul.SetValueByKey("FixFlag", "1");
                                            AutoClosingMessageBox.Show("Bạn có 15 phút để thực hiện sửa lỗi ! Thanks " + EmpID + "!", "AutoCloseMessageBox", 10000);
                                            SetStopMachineStatus(true);

                                            this.Close();
                                        }

                                        if (_KindOfError.Contains("ate-checklist data"))
                                        {
                                            ul.SetValueByKey("StopMachine", "0");
                                            ul.SetValueByKey("FixFlag", "1");
                                            AutoClosingMessageBox.Show("Bạn có 15 phút để thực hiện sửa lỗi ! Thanks " + EmpID + "!", "AutoCloseMessageBox", 10000);
                                            SetStopMachineStatus(true);

                                            this.Close();
                                        }

                                        if (_KindOfError.Contains("Fixture"))
                                        {
                                            ul.SetValueByKey("StopMachine", "0");
                                            //ul.SetValueByKey("FixFlag", "1");
                                            AutoClosingMessageBox.Show("Vui lòng dùng đúng Fixture trên server ! Thanks " + EmpID + "!", "AutoCloseMessageBox", 10000);
                                            SetStopMachineStatus(true);

                                            this.Close();
                                        }

                                        if (_KindOfError.Contains("ErrorCodeInHour"))
                                        {
                                            if (File.Exists(".\\ErrorCodeList.txt"))
                                            {
                                                File.SetAttributes(".\\ErrorCodeList.txt", FileAttributes.Normal);
                                                File.Delete(".\\ErrorCodeList.txt");
                                            }
                                            ul.SetValueByKey("StopMachine", "0");
                                            ul.SetValueByKey("FixFlag", "1");
                                            AutoClosingMessageBox.Show("Bạn có 15 phút để thực hiện sửa lỗi ! Thanks " + EmpID + "!", "AutoCloseMessageBox", 10000);
                                            SetStopMachineStatus(true);
                                            this.Close();
                                        }

                                        ///
                                        string RRYRdata = ul.GetValueByKey("SFISDATA");

                                        if (_KindOfError.Contains("R_OVERSPEC"))
                                        {
                                            if (ds.Tables[0].Rows[0]["Dep"].ToString().Contains("QA") || ds.Tables[0].Rows[0]["Dep"].ToString().Contains("PE"))
                                            {
                                                ul.SetValueByKey("StopMachine", "0");
                                                ul.SetValueByKey("FixFlag", "1");
                                                AutoClosingMessageBox.Show("Bạn có 15 phút để thực hiện sửa lỗi ! Thanks " + EmpID + "!", "AutoCloseMessageBox", 10000);
                                                try
                                                {
                                                    string s = CENTER_B05_SV.P_B05_ARLO_UPH_SOLVED(ul.GetValueByKey("MO"), ul.GetStation(), ul.GetModel(), _KindOfError, FixEmpID, tbxAction.Text.Trim());
                                                    ul.event_log("ArloUphMonitor: " + s);
                                                }
                                                catch (Exception r)
                                                {
                                                    ul.event_log("ArloUphMonitor: " + r.ToString());
                                                }
                                                SetStopMachineStatus(true);

                                                // update table record
                                            }
                                            this.Close();
                                        }

                                        if (_KindOfError.Contains("YRate") || _KindOfError.Contains("RTRate"))
                                        {
                                            if (_KindOfError.Contains("YRate") && ds.Tables[0].Rows[0]["Dep"].ToString().Contains("QA"))
                                            {
                                                ul.SetValueByKey("StopMachine", "0");
                                                //ul.SetValueByKey("StopStation", "tmpDefault");
                                                if (RRYRdata != "")
                                                {
                                                    if (ul.GetValueByKey("BRTRate") == "0" && _KindOfError.Contains("RTRate"))
                                                    {
                                                        ul.SetValueByKey("BRTRate", TestDUTCount);
                                                        float RTRate = Convert.ToSingle(RRYRdata.Substring(48, 6));
                                                        AutoClosingMessageBox.Show("Bạn có thêm " + TestDUTCount + " lần thực hiện kiểm tra!", "AutoCloseMessageBox", 10000);
                                                    }

                                                    if (ul.GetValueByKey("BYRate") == "0" && _KindOfError.Contains("YRate"))
                                                    {
                                                        // for NPI
                                                        if (!_KindOfError.Contains("NPI"))
                                                        {
                                                            ul.SetValueByKey("BYRate", TestDUTCount);
                                                            float YRate = Convert.ToSingle(RRYRdata.Substring(54, 6));
                                                            AutoClosingMessageBox.Show("Bạn có thêm " + TestDUTCount + " lần thực hiện kiểm tra!", "AutoCloseMessageBox", 10000);
                                                        }
                                                        else //??
                                                        {
                                                            ul.SetValueByKey("NPI_ERROR", "");
                                                            AutoClosingMessageBox.Show("Yeah. Keep moving!", "AutoCloseMessageBox", 10000);
                                                        }
                                                    }
                                                    flagInsert = true;
                                                }

                                                SetStopMachineStatus(false);
                                            }
                                            else
                                            {
                                                if (_KindOfError.Contains("RTRate") && ds.Tables[0].Rows[0]["Dep"].ToString().Contains("TE-Setup"))
                                                {
                                                    ul.SetValueByKey("StopMachine", "0");
                                                    //ul.SetValueByKey("StopStation", "tmpDefault");
                                                    if (RRYRdata != "")
                                                    {
                                                        if (ul.GetValueByKey("BRTRate") == "0" && _KindOfError.Contains("RTRate"))
                                                        {
                                                            ul.SetValueByKey("BRTRate", TestDUTCount);
                                                            float RTRate = Convert.ToSingle(RRYRdata.Substring(48, 6));
                                                            AutoClosingMessageBox.Show("Bạn có thêm " + TestDUTCount + " lần thực hiện kiểm tra!", "AutoCloseMessageBox", 10000);
                                                        }
                                                    }
                                                    SetStopMachineStatus(true);
                                                    flagInsert = true;
                                                    //this.Close();
                                                }
                                                else
                                                {
                                                    AutoClosingMessageBox.Show("Không đủ quyền xử lý. Gọi qa/pqe!", "AutoCloseMessageBox", 10000);
                                                }
                                            }
                                            this.Close();
                                        }
                                        else
                                        {
                                            ul.SetValueByKey("StopMachine", "0");
                                            if (_KindOfError.Contains("Testtime"))
                                            {
                                                flagInsert = true;
                                                if (Convert.ToInt32(ul.GetValueByKey("TestTimeLowLimit")) < 10)
                                                {
                                                    ul.SetValueByKey("Testtime", "00:0" + ul.GetValueByKey("TestTimeLowLimit"));
                                                }
                                                else
                                                {
                                                    ul.SetValueByKey("Testtime", "00:" + ul.GetValueByKey("TestTimeLowLimit"));
                                                }

                                                AutoClosingMessageBox.Show("Đã thực hiện sửa lỗi! Thanks " + EmpID + "!", "AutoCloseMessageBox", 10000);
                                            }

                                            if (_KindOfError.Contains("3_Continuous"))
                                            {
                                                flagInsert = true;
                                                ul.SetValueByKey("CountSameError", "");
                                                ul.SetValueByKey("CountError", "");
                                                AutoClosingMessageBox.Show("Đã thực hiện sửa lỗi! Thanks " + EmpID + "!", "AutoCloseMessageBox", 10000);
                                            }

                                            if (_KindOfError.Contains("4_Continuous"))
                                            {
                                                flagInsert = true;
                                                Count4Error = ul.GetValueByKey("CountError").Trim();
                                                ul.SetValueByKey("CountError", "");
                                                ul.SetValueByKey("CountSameError", "");
                                                AutoClosingMessageBox.Show("Đã thực hiện sửa lỗi! Thanks " + EmpID + "!", "AutoCloseMessageBox", 10000);
                                            }

                                            if (_KindOfError.Contains("USB") || _KindOfError.Contains("Virus") || _KindOfError.Contains("Cable") || _KindOfError.Contains("PathLoss"))
                                            {
                                                flagInsert = true;
                                                ul.SetValueByKey("FixFlag", "1");
                                                AutoClosingMessageBox.Show("Bạn có 15 phút để khắc phục lỗi! Thanks " + EmpID + "!", "AutoCloseMessageBox", 10000);
                                            }

                                            SetStopMachineStatus(true);
                                            this.Close();
                                        }

                                        if (flagInsert == true)
                                        {
                                            //Thread _tUpdateUnlockStatus = new Thread(UpdateUnlockStatus);
                                            //_tUpdateUnlockStatus.IsBackground = true;
                                            //_tUpdateUnlockStatus.Start();

                                            string Action = "";
                                            if (tbxAction.Text.Contains("Click here to open virtual keyboard"))
                                            {
                                                Action = "No Actions " + _KindOfError + " " + Count4Error; ;
                                            }
                                            else
                                            {
                                                Action = tbxAction.Text + " " + _KindOfError + " " + Count4Error; ;
                                            }

                                            string[] tmp = _KindOfError.Split(' ');
                                            string errors = tmp[0];
                                            string _ENDTIME = "";

                                            if (isMinimized == true)
                                            {
                                                DelayTimer.Enabled = false;
                                                TimeSpan tsptimeMinimized = endMinimized.Subtract(startMinimized);
                                                double MinutesMinimized = tsptimeMinimized.TotalMinutes;

                                                _ENDTIME = DateTime.Now.AddMinutes(-MinutesMinimized).ToString("yyyy/MM/dd hh:mm:ss");
                                            }
                                            else
                                            {
                                                _ENDTIME = DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss");
                                            }

                                            if (GetLineOfTester().Trim() != "L" && _model_name != "X" && ul.GetStation() != "BOOMS")
                                            {
                                                Thread _tSaveUnlockData = new Thread(() => SaveUnlockData(errors, EmpID, tbxAction.Text));
                                                _tSaveUnlockData.IsBackground = true;
                                                _tSaveUnlockData.Start();
                                                //CENTER_B05_SV.SHOWUI_UNLOCK_RECORD(client_ip, sName, GetLineOfTester().Trim(),ul.GetModel(),ul.GetStation(),errors,EmpID,"",_STARTTIME,tbxAction.Text);
                                                //svSaveErrs.ShowUI_SaveSolvedError(sName, GetLineOfTester().Trim(), ul.GetStation().Trim(), ul.GetModel().Trim(), errors, EmpID, _STARTTIME, _ENDTIME, Action);
                                            }
                                            this.Close();
                                        }

                                        timer1.Enabled = false;
                                        //this.Close();
                                        // for minimize form
                                        isFirstTimeMinimized = 0;
                                        File.Delete(@".\frmActive.ini");
                                    }
                                    else
                                    {
                                        AutoClosingMessageBox.Show("Tài khoản và Mật khẩu không đúng. Thử lại!", "AutoCloseMessageBox", 10000);
                                    }
                                    conn.Close();
                                }
                                catch (Exception r)
                                {
                                    //In case crash server
                                    timer1.Enabled = false;
                                    DelayTimer.Enabled = false;
                                    ul.SetValueByKey("FixFlag", "1");
                                    this.Close();
                                }
                            }
                        }
                        else
                        {
                            this.TopMost = true;
                        }
                    }
                }
                else
                {
                    AutoClosingMessageBox.Show("Đối sách không được để trắng. Nếu không có, điền vào No Action!. Thanks", "AutoCloseMessageBox", 15000);
                }// end if
            }
            catch (Exception)
            {
                //throw;
                timer1.Enabled = false;
                DelayTimer.Enabled = false;
                this.Close();
            }
        }

        public void SaveUnlockData(string errors, string EmpID, string tbxAction)
        {
            try
            {
                CENTER_B05_SV.SHOWUI_UNLOCK_RECORD(client_ip, sName, GetLineOfTester().Trim(), ul.GetModel(), ul.GetStation(), errors, EmpID, "", _STARTTIME, tbxAction);
                ul.event_log("Unlock " + client_ip + sName + GetLineOfTester().Trim() + ul.GetModel() + ul.GetStation() + EmpID + tbxAction);
            }
            catch (Exception)
            {
            }
        }

        public void SaveUnlockDataSampling(string errors, string EmpID, string tbxAction, string Sampling)
        {
            try
            {
                CENTER_B05_SV.SHOWUI_UNLOCK_RECORD(client_ip, sName, GetLineOfTester().Trim(), ul.GetModel(), ul.GetStation(), errors, EmpID, Sampling, _STARTTIME, tbxAction);
                ul.event_log(client_ip + sName + GetLineOfTester().Trim() + ul.GetModel() + ul.GetStation() + EmpID + Sampling);
            }
            catch (Exception)
            {
            }
        }

        private RegistryKey _OpenKey;
        private string sName = Environment.MachineName;
        private string SubKey = @"SOFTWARE\Netgear\STATION";

        public string GetLineOfTester()
        {
            try
            {
                string line = sName.Substring(sName.IndexOf("L") + 1, 2);
                char[] aline = line.ToCharArray();
                string testline = "";
                int tmpLine = 0;
                for (int i = 0; i < aline.Length; i++)
                {
                    int x;
                    bool isNum = Int32.TryParse(aline[i].ToString(), out x);
                    if (!isNum)
                    {
                        break;
                    }
                    testline += aline[i];
                }
                tmpLine = Convert.ToInt32(testline);
                if (tmpLine == 0)
                {
                    return "L";
                }
                else
                {
                    return "L" + tmpLine;
                }
            }
            catch (Exception)
            {
                return "L";
                //throw;
            }
        }

        public string GetStopmachineSpec(double inputPCS, string type)
        {
            string rturnValue = "";
            if (type.Contains("YRate"))
                rturnValue = "0.5";
            if (type.Contains("RTRate"))
                rturnValue = "99.5";

            try
            {
                //globalUsedMode = "2";
                //TestedDUT = 400;
                //MessageBox.Show(_line_name);
                //_model_name = "CM500-100NASV1";
                // if testedDUT less than and equal to 10 pcs, dont care

                //if (inputPCS > 10)
                //{
                //    //globalUsedMode = "2";
                //    //sName = "B05-L13-PT01";
                //    string linename = GetLineOfTester().Trim();
                //    SqlConnection connection = new SqlConnection(conn);
                //    connection.Open();
                //    SqlDataReader reader;
                //    //globalUsedMode = "2";
                //    //string sqlStr = @"select * from tblStopMachineSpec join tblTypeOfProduct on tblStopMachineSpec.Type = tblTypeOfProduct.Type where tblTypeOfProduct.Line='" + GetLineOfTester() + "'";
                //    string sqlStr = "";
                //    if (_globalUsedMode == "1")
                //    {
                //        //sqlStr = @"select UsedMode,TmpRTRate,TmpYRate,LowerNumDUT,UpperNumDUT,RTRSpec,YRSpec from tblTypeOfProductExt join tblTypeOfProduct on tblTypeOfProductExt.LineName = tblTypeOfProduct.Line where tblTypeOfProduct.Line='" + linename + "' and " + TestedDUT + " > tblTypeOfProductExt.LowerNumDUT and " + TestedDUT + " <=tblTypeOfProductExt.UpperNumDUT";
                //        // mode = 1 is
                //        sqlStr += "select UsedMode,TmpRTRate,TmpYRate,LowerNumDUT,UpperNumDUT,RTRSpec,YRSpec";
                //        sqlStr += " from tblTypeOfProductExt a,tblTypeOfProduct b";
                //        sqlStr += " where a.LineName = b.Line and b.Line='" + linename + "' ";
                //        sqlStr += " and " + inputPCS + " > a.LowerNumDUT and " + inputPCS + " <=a.UpperNumDUT";
                //        sqlStr += " union all ";
                //        sqlStr += "select UsedMode,TmpRTRate,TmpYRate,LowerNumDUT,UpperNumDUT,RTRSpec,YRSpec ";
                //        sqlStr += " from tblTypeOfProductExt a, tblTypeOfProduct b ";
                //        sqlStr += " where a.LineName = b.Line and b.Line='" + linename + "' ";
                //        sqlStr += " and  a.UpperNumDUT = (select max(a.UpperNumDUT) from tblTypeOfProductExt a, tblTypeOfProduct b where a.LineName = b.Line and b.Line='" + linename + "')";
                //    }
                //    else if (_globalUsedMode == "2")
                //    {//B05-L13-PT08
                //        //sqlStr = @"select * from tblTypeOfProduct a, tblTypeOfProductDetail b where a.currentmodel = b.model_name and b.ate_name ='"+sName+"' and b.ate_ip='"+client_ip+"' and b.model_name='"+ul.GetModel().Trim()+"'";
                //        sqlStr += "select LowerNumDUT,UpperNumDUT,RTRSpec,YRSpec from tblTypeOfProductDetail";
                //        sqlStr += " where ate_name ='" + sName + "'";
                //        sqlStr += " and ate_ip='" + client_ip + "'";
                //        sqlStr += " and model_name='" + _model_name + "'";
                //        sqlStr += " and lowerNumDUT < " + inputPCS + " and UpperNumDUT >= " + inputPCS + "";
                //        sqlStr += " union all ";
                //        sqlStr += "select LowerNumDUT,UpperNumDUT,RTRSpec,YRSpec from tblTypeOfProductDetail";
                //        sqlStr += " where  ate_name ='" + sName + "'";
                //        sqlStr += " and ate_ip='" + client_ip + "'";
                //        sqlStr += " and model_name='" + _model_name + "'";
                //        sqlStr += " and  UpperNumDUT = (select max(UpperNumDUT)";
                //        sqlStr += " from tblTypeOfProductDetail";
                //        sqlStr += " where ate_name ='" + sName + "'";
                //        sqlStr += " and ate_ip='" + client_ip + "'";
                //        sqlStr += " and model_name='" + _model_name + "')";
                //    }
                //    else
                //    {
                //        sqlStr = @"select TmpRTRate,TmpYRate from  tblTypeOfProduct where line ='" + linename + "'";
                //    }
                //    //event_log(sqlStr);
                //    SqlCommand cmd = new SqlCommand(sqlStr, connection);
                //    SqlDataAdapter da = new SqlDataAdapter(sqlStr, conn);
                //    DataSet ds = new DataSet();
                //    da.Fill(ds);
                //    reader = cmd.ExecuteReader();
                //    if (reader.HasRows)
                //    {
                //        if (_globalUsedMode == "2" || _globalUsedMode == "1")
                //        {
                //            if (type.Contains("YRate"))
                //                rturnValue = ds.Tables[0].Rows[0]["YRSpec"].ToString();
                //            if (type.Contains("RTRate"))
                //                rturnValue = ds.Tables[0].Rows[0]["RTRSpec"].ToString();
                //        }
                //        else
                //        {
                //            if (type.Contains("YRate"))
                //                rturnValue = ds.Tables[0].Rows[0]["TmpYRate"].ToString();
                //            if (type.Contains("RTRate"))
                //                rturnValue = ds.Tables[0].Rows[0]["TmpRTRate"].ToString();
                //        }
                //    }
                //    else
                //    {
                //        da.Dispose();
                //        reader.Close();
                //        cmd.Dispose();

                //        // if globalUsedMode == 2 but dont exist, insert new
                //        if (_globalUsedMode == "2")
                //        {
                //            string sqlGetSpec = "select LowerNumDUT,UpperNumDUT,RTRate,YRate from tblStopMachineSpec order by LowerNumDUT asc";
                //            cmd = new SqlCommand(sqlGetSpec, connection);
                //            da = new SqlDataAdapter(sqlGetSpec, conn);
                //            ds = new DataSet();
                //            da.Fill(ds);
                //            reader = cmd.ExecuteReader();
                //            if (reader.HasRows)
                //            {
                //                string[,] specValue = new string[ds.Tables[0].Rows.Count, ds.Tables[0].Columns.Count];
                //                for (int j = 0; j < ds.Tables[0].Rows.Count; j++)
                //                {
                //                    for (int k = 0; k < ds.Tables[0].Columns.Count; k++)
                //                    {
                //                        specValue[j, k] = ds.Tables[0].Rows[j][k].ToString();
                //                    }
                //                }

                //                cmd.Dispose();
                //                reader.Close();
                //                da.Dispose();
                //                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                //                {
                //                    sqlStr = @"insert into tblTypeOfProductDetail(ate_ip,ate_name,station_name,date_time,lowerNumDUT,upperNumDUT,RTRSpec,YRSpec,model_name)";
                //                    sqlStr += "values(@_client_ip,@_sName,@_station,@_date_time,@_lowerDUT,@_upperDUT,@_RTRSpec,@_YRSpec,@_model)";
                //                    cmd = new SqlCommand(sqlStr, connection);//'" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")'" + _model_name + "'
                //                    cmd.Parameters.Add("@_client_ip", SqlDbType.NVarChar, 50);
                //                    cmd.Parameters.Add("@_sName", SqlDbType.NVarChar, 50);
                //                    cmd.Parameters.Add("@_station", SqlDbType.NVarChar, 50);
                //                    cmd.Parameters.Add("@_date_time", SqlDbType.DateTime);
                //                    cmd.Parameters.Add("@_lowerDUT", SqlDbType.Int);
                //                    cmd.Parameters.Add("@_upperDUT", SqlDbType.Int);
                //                    cmd.Parameters.Add("@_RTRSpec", SqlDbType.NVarChar, 50);
                //                    cmd.Parameters.Add("@_YRSpec", SqlDbType.NVarChar, 50);
                //                    cmd.Parameters.Add("@_model", SqlDbType.NVarChar, 50);

                //                    cmd.Parameters["@_client_ip"].Value = client_ip;
                //                    cmd.Parameters["@_sName"].Value = sName;
                //                    cmd.Parameters["@_station"].Value = ul.GetStation().Trim();
                //                    cmd.Parameters["@_date_time"].Value = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                //                    cmd.Parameters["@_lowerDUT"].Value = Convert.ToInt32(specValue[i, 0]);
                //                    cmd.Parameters["@_upperDUT"].Value = Convert.ToInt32(specValue[i, 1]);
                //                    cmd.Parameters["@_RTRSpec"].Value = specValue[i, 2];
                //                    cmd.Parameters["@_YRSpec"].Value = specValue[i, 3];
                //                    cmd.Parameters["@_model"].Value = _model_name;

                //                    cmd.ExecuteNonQuery();
                //                    cmd.Dispose();
                //                }
                //            }
                //            else
                //            { //
                //                da.Dispose();
                //                reader.Close();
                //            }
                //        }//end if
                //    } //end if
                //    cmd.Dispose();
                //    da.Dispose();
                //    reader.Close();
                //    connection.Close();
                // } // end if inpcs > 10
            }
            catch (Exception e)
            {
                ul.event_log("GetStopmachineSpec Exception: " + e.ToString());
            }

            //AutoClosingMessageBox.Show("GetStopmachineSpec: UseSpecMode->" + UseSpecMode.ToString() + "  Retest Rate/Yeild Rate->" + RTRate + "/" + YRate, "sss", 5000);
            return rturnValue;
        }

        public string GetYRateSpec()
        {
            string YRate = "0";
            string start_time = DateTime.Now.ToString("yyyyMMdd");
            string end_time = DateTime.Now.ToString("yyyyMMddHHmm");
            string model = ul.GetModel();
            string line = GetLineOfTester();
            string mo = ul.GetValueByKey("MO");
            string station = ul.GetStation();
            double TotalTestedDUT = 0;
            //if (ConnectServer60 == "0" && NetWorkConnection == true)
            //{
            try
            {
                string TmpDate = DateTime.Now.ToString("HHmm");
                int ComparedTime = 730;

                ComparedTime = Convert.ToInt32(TmpDate);

                if (ComparedTime >= 730 && ComparedTime <= 1930)
                {
                    start_time = start_time + "0730";
                }
                else
                {
                    if (DateTime.Now.Hour < 7)
                        start_time = DateTime.Now.AddDays(-1).ToString("yyyyMMdd") + "1930";
                    else
                        start_time = start_time + "1930";
                }

                DataTable dtYRate = sfisB05.GET_R_STATION_REC_T(start_time, end_time);

                DataRow[] results = dtYRate.Select("SECTION_NAME = 'SI' AND MODEL_NAME='" + model + "' AND GROUP_NAME='" + station + "'");
                foreach (DataRow dr in results)
                {
                    TotalTestedDUT += (Convert.ToDouble(dr["PASS_QTY"].ToString()) + Convert.ToDouble(dr["FAIL_QTY"].ToString()));
                }
                YRate = GetStopmachineSpec(TotalTestedDUT, "YRate");
            }
            catch (Exception r)
            {
                ul.event_log("GetYRateSpec Exception: " + r.ToString());
            }
            // }// end if network

            // MessageBox.Show(YRate.ToString());
            return YRate;
        }

        public string GetRTRateSpec()
        {
            string RTR = "0.5";

            RTR = GetStopmachineSpec(_TestedDUT, "RTRate");

            return RTR;
        }

        //public void GetStopmachineSpec()
        //{
        //    string RTRate = "99.5";
        //    string YRate = "0.5";

        //    try
        //    {
        //            //globalUsedMode = "2";
        //            //TestedDUT = 11;
        //            //MessageBox.Show(_line_name);
        //            //_model_name = "CM500-100NASV1";
        //            // if testedDUT less than and equal to 10 pcs, dont care
        //            if (_TestedDUT > 10)
        //            {
        //                string linename = GetLineOfTester().Trim();
        //                SqlConnection connection = new SqlConnection(conn);
        //                connection.Open();
        //                SqlDataReader reader;
        //                //string sqlStr = @"select * from tblStopMachineSpec join tblTypeOfProduct on tblStopMachineSpec.Type = tblTypeOfProduct.Type where tblTypeOfProduct.Line='" + GetLineOfTester() + "'";
        //                string sqlStr = "";
        //                if (_globalUsedMode == "1")
        //                {
        //                    //sqlStr = @"select UsedMode,TmpRTRate,TmpYRate,LowerNumDUT,UpperNumDUT,RTRSpec,YRSpec from tblTypeOfProductExt join tblTypeOfProduct on tblTypeOfProductExt.LineName = tblTypeOfProduct.Line where tblTypeOfProduct.Line='" + linename + "' and " + TestedDUT + " > tblTypeOfProductExt.LowerNumDUT and " + TestedDUT + " <=tblTypeOfProductExt.UpperNumDUT";

        //                    sqlStr += "select UsedMode,TmpRTRate,TmpYRate,LowerNumDUT,UpperNumDUT,RTRSpec,YRSpec";
        //                    sqlStr += " from tblTypeOfProductExt a,tblTypeOfProduct b";
        //                    sqlStr += " where a.LineName = b.Line and b.Line='" + linename + "' ";
        //                    sqlStr += " and " + _TestedDUT + " > a.LowerNumDUT and " + _TestedDUT + " <=a.UpperNumDUT";
        //                    sqlStr += " union all ";
        //                    sqlStr += "select UsedMode,TmpRTRate,TmpYRate,LowerNumDUT,UpperNumDUT,RTRSpec,YRSpec ";
        //                    sqlStr += " from tblTypeOfProductExt a, tblTypeOfProduct b ";
        //                    sqlStr += " where a.LineName = b.Line and b.Line='" + linename + "' ";
        //                    sqlStr += " and  a.UpperNumDUT = (select max(a.UpperNumDUT) from tblTypeOfProductExt a, tblTypeOfProduct b where a.LineName = b.Line and b.Line='" + linename + "')";

        //                }
        //                else if (_globalUsedMode == "2")
        //                {
        //                    //sqlStr = @"select * from tblTypeOfProduct a, tblTypeOfProductDetail b where a.currentmodel = b.model_name and b.ate_name ='"+sName+"' and b.ate_ip='"+client_ip+"' and b.model_name='"+GetModel().Trim()+"'";
        //                    sqlStr += "select LowerNumDUT,UpperNumDUT,RTRSpec,YRSpec from tblTypeOfProductDetail";
        //                    sqlStr += " where ate_name ='" + sName + "'";
        //                    sqlStr += " and ate_ip='" + client_ip + "'";
        //                    sqlStr += " and model_name='" + _model_name + "'";
        //                    sqlStr += " and lowerNumDUT < " + _TestedDUT + " and UpperNumDUT >= " + _TestedDUT + "";
        //                    sqlStr += " union all ";
        //                    sqlStr += "select LowerNumDUT,UpperNumDUT,RTRSpec,YRSpec from tblTypeOfProductDetail";
        //                    sqlStr += " where  ate_name ='" + sName + "'";
        //                    sqlStr += " and ate_ip='" + client_ip + "'";
        //                    sqlStr += " and model_name='" + _model_name + "'";
        //                    sqlStr += " and  UpperNumDUT = (select max(UpperNumDUT)";
        //                    sqlStr += " from tblTypeOfProductDetail";
        //                    sqlStr += " where ate_name ='" + sName + "'";
        //                    sqlStr += " and ate_ip='" + client_ip + "'";
        //                    sqlStr += " and model_name='" + _model_name + "')";
        //                }
        //                else
        //                {
        //                    sqlStr = @"select TmpRTRate,TmpYRate from  tblTypeOfProduct where line ='" + linename + "'";
        //                }
        //                //event_log(sqlStr);
        //                SqlCommand cmd = new SqlCommand(sqlStr, connection);
        //                SqlDataAdapter da = new SqlDataAdapter(sqlStr, conn);
        //                DataSet ds = new DataSet();
        //                da.Fill(ds);
        //                reader = cmd.ExecuteReader();
        //                if (reader.HasRows)
        //                {
        //                    if (_globalUsedMode == "2" || _globalUsedMode == "1")
        //                    {
        //                        RTRate = ds.Tables[0].Rows[0]["RTRSpec"].ToString();
        //                        YRate = ds.Tables[0].Rows[0]["YRSpec"].ToString();
        //                    }
        //                    else
        //                    {
        //                        RTRate = ds.Tables[0].Rows[0]["TmpRTRate"].ToString();
        //                        YRate = ds.Tables[0].Rows[0]["TmpYRate"].ToString();
        //                    }
        //                }

        //                da.Dispose();
        //                reader.Close();
        //                connection.Close();
        //            } // end if TestedDUT >= 10
        //    }
        //    catch (Exception e)
        //    {
        //        // AutoClosingMessageBox.Show(e.ToString(), "MSG", 5000);
        //    }

        //}
        public void NoticeMessage()
        {
            //try
            //{
            //    SvFPS.WebService sv = new SvFPS.WebService();
            //    sv.App_NoticeStopMachine_Add(GetLineOfTester().Trim(), GetStation().Trim(), sName, GetModel().Trim(), _KindOfError);
            //}
            //catch (Exception)
            //{
            //}
        }

        private void frmLocking_Load(object sender, EventArgs e)
        {
            try
            {
                //string svIp = ul.GetServerIP("dbGeneral", "10.224.81.37");
                FiiData.QRCode(pictureBox1);
                Thread _tdoInitializeInfo = new Thread(doInitializeInfo);
                _tdoInitializeInfo.IsBackground = true;
                _tdoInitializeInfo.Start();

                int widthScreen = Screen.PrimaryScreen.WorkingArea.Width;
                int heightScreen = Screen.PrimaryScreen.WorkingArea.Height;
                this.SetBounds(0, 0, widthScreen, heightScreen);

                // fix for sreen too small
                if (widthScreen <= 850)
                {
                    groupBox1.SetBounds(this.Location.X + 2, this.Location.Y + 2, widthScreen - 2, heightScreen - 2);

                    groupBox2.SetBounds(groupBox1.Location.X + 2, groupBox1.Location.Y + 3 * (heightScreen / 4), widthScreen - 10, groupBox1.Height / 4 - 10);

                    lblClose.SetBounds(groupBox1.Location.X + (groupBox1.Width - 25), groupBox1.Location.Y + 15, 20, 19);
                    tbUnlock.SetBounds(this.Location.X + (groupBox2.Width - 120), this.Location.Y + 45, 100, 32);
                    lbGeneralMsg.SetBounds(this.Location.X + 15, groupBox1.Location.Y + 15, groupBox1.Width - 15, groupBox1.Height / 4);// = widthScreen - 20;

                    lbGeneralMsg.Font = new Font(lbGeneralMsg.Font.FontFamily, 30, lbGeneralMsg.Font.Style);

                    lbBoom.SetBounds(lbGeneralMsg.Location.X + 15, groupBox1.Location.Y + 2 * (heightScreen / 4), groupBox1.Width - 20, groupBox1.Height / 4);// = widthScreen - 20;

                    lbBoom.Font = new Font(lbGeneralMsg.Font.FontFamily, 25, lbGeneralMsg.Font.Style);
                    tbxAction.Width = groupBox1.Width / 2;
                }
                if (widthScreen > 1095 || heightScreen > 590)
                {
                    groupBox1.SetBounds(widthScreen / 2 - 1095 / 2, heightScreen / 2 - 590 / 2, 1095, 590);
                }

                // 2016 10 08 for interference data
                if (_KindOfError.Contains("interference data"))
                {
                    InterferenceStop();
                }

                tbxAction.Text = "Click here to open virtual keyboard. Bấm vào đây để mở bàn phím ảo!";

                // Get value for stopline

                // timer1.Enabled = true;
                // timer1.Interval = 10000;
                this.TopMost = true;
            }
            catch (Exception)
            { }
        }

        public void doInitializeInfo()
        {
            try
            {
                //conn = @"Data Source=10.224.81.162,1734;Initial Catalog=dbGeneral;uid=sa;pwd=********;Connection Timeout=5";
                string svIp = "10.224.81.162,1734";//ul.GetServerIP("SSO", "10.224.81.37");
                connectionString = @"Data Source=10.224.81.162,1734;Initial Catalog=SSO;uid=sa;pwd=********;Connection Timeout=5";

                // Get ServrerIp for ToDB() dbMO
                serverIp = IniFile.ReadIniFile("DATABASE", "SERVER_NAME", "10.224.81.37", @"F:\Temp\TE-PROGRAM\TE-DATABASE\SOURCE.ini");
                client_ip = ul.GetNICGatewayIP();

                _model_name = ul.GetValueByKey("SFISMODEL").Trim();
                _line_name = GetLineOfTester().Trim().Remove(0, 1);
                _station_name = ul.GetStation().Trim();
                _ate_name = sName;
                _ate_ip = client_ip;
            }
            catch (Exception)
            {
            }
        }

        public string GetWebUnlockPath(string _key)
        {
            try
            {
                RegistryKey kiwi = Registry.LocalMachine.OpenSubKey(subkey, true);
                string SN = kiwi.GetValue(_key, "").ToString();
                return SN;
            }
            catch (Exception)
            {
                return "X";
                //throw;
            }
        }

        public void SetWebUnlockPath(string _reg, string _val)
        {
            try
            {
                RegistryKey kiwi = Registry.LocalMachine.OpenSubKey(subkey, true);
                kiwi.SetValue(_reg, _val, RegistryValueKind.String);
                //return SN;
            }
            catch (Exception)
            {
                //return "X";
                //throw;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                //if (GetWebUnlockPath("StopStation").Remove(1, GetWebUnlockPath("StopStation").Length - 1) == "0")
                //{
                //MessageBox.Show("OK timer1");

                if (CheckAntiVirusSoftUpdate() && _KindOfError.Contains("Virus"))
                {
                    //this.Close();
                }
                if (GetStopMachineStatus())
                {
                    if (_KindOfError.Contains("YRate"))
                    {
                        ul.SetValueByKey("BYRate", TestDUTCount);
                    }

                    if (_KindOfError.Contains("RTRate"))
                    {
                        ul.SetValueByKey("BRTRate", TestDUTCount);
                    }

                    if (_KindOfError.Contains("StopAll") && ul.GetValueByKey("ErrStation") == "")
                    {
                        ul.SetValueByKey("BYRate", TestDUTCount);
                    }

                    //ul.SetValueByKey("StopStation", "tmpDefault");
                    if (ul.GetValueByKey("ErrStation") == "" && ul.GetValueByKey("ERROR_CODE") == "")
                    {
                        SetWebUnlockPath("StopStation", "tmpDefault");
                        SetStopMachineStatus(true);
                        ul.SetValueByKey("StopMachine", "0");
                    }

                    //Thread _tUpdateUnlockStatus = new Thread(UpdateUnlockStatus);
                    ////_tUpdateUnlockStatus.IsBackground = true;
                    //_tUpdateUnlockStatus.Start();\
                    timer1.Enabled = false;
                    DelayTimer.Enabled = false;
                    //this.Close();
                }
            }
            catch (Exception)
            {
            }
        }

        private string localPathSetup = @"F:\lsy\Test\DownloadConfig\AutoDL\Setup.ini";

        public int VirusOutOfDateSpec()
        {
            try
            {
                int NumOfDate = Convert.ToInt32(IniFile.ReadIniFile("NumDate", "num", null, localPathSetup));
                if (IniFile.ReadIniFile("NumDate", "num", null, localPathSetup) == null)
                {
                    return 7;
                }

                return NumOfDate;
            }
            catch (Exception ex)
            {
                return 7;
            }
        }

        //bool flagVirus;
        private int NumOfCable;// fixed num of cable = 10

        private int[] BlinkFlag;

        protected bool CheckUSBDisable()
        {
            string KeyUSB = @"SYSTEM\CurrentControlSet\services";
            RegistryKey keys = Registry.LocalMachine.OpenSubKey(KeyUSB + @"\usbstor\", true);
            bool UsbStatus = false;
            if (keys != null)
            {
                string USBStattus = keys.GetValue("Start", "").ToString();
                if (USBStattus == "4")
                {
                    UsbStatus = true;
                }
                else
                {
                    UsbStatus = false;
                }
                return UsbStatus;
            }
            else
            {
                keys = Registry.LocalMachine.OpenSubKey(KeyUSB, true);
                RegistryKey usbstor = keys.CreateSubKey("usbstor");
                usbstor.SetValue("Start", "4", RegistryValueKind.DWord);
                UsbStatus = true;
            }

            return UsbStatus;
        }

        private bool BeforeDaysNotice = false;
        private double ExpiredDayLeft = 0;

        protected bool CheckAntiVirusSoftUpdate()
        {
            bool CheckAnti = false;
            try
            {
                int NumOfDay = VirusOutOfDateSpec();
                string KeyAnti = @"SOFTWARE\Symantec\SharedDefs\";
                RegistryKey key = Registry.LocalMachine.OpenSubKey(KeyAnti, true);
                if (key != null)
                {
                    string tmpUpdateDay = key.GetValue("NAVCORP_70", "").ToString();

                    for (int i = 0; i <= NumOfDay; i++)
                    {
                        DateTime dtCheck = DateTime.Now.AddDays(-(i));
                        TimeSpan tps = DateTime.Now.Subtract(dtCheck);

                        if (tmpUpdateDay.Contains(dtCheck.ToString("yyyyMMdd")))
                        {
                            int Notice = Convert.ToInt32(IniFile.ReadIniFile("NumDate", "Notice", "3", localPathSetup));
                            if ((NumOfDay - tps.TotalDays) < Notice)
                            {
                                //MessageBox.Show(Convert.ToString(tps.TotalDays));
                                ExpiredDayLeft = NumOfDay - tps.TotalDays;
                                BeforeDaysNotice = true;
                            }

                            CheckAnti = true;
                        }
                    }
                }
                else
                {
                }
                return CheckAnti;
            }
            catch (Exception ex)
            {
                return CheckAnti;
            }
        }

        private int CountDelay = 0;
        private DateTime startMinimized;
        private DateTime endMinimized;
        private bool isMinimized = false;
        private int isFirstTimeMinimized = 0;

        private void lblClose_Click(object sender, EventArgs e)
        {
            try
            {
                using (frmUnlockRequirements frm = new frmUnlockRequirements(false, "", startTime))
                {
                    this.TopMost = false;
                    frm.TopMost = true;

                    if (frm.ShowDialog() == DialogResult.OK)
                    {
                        string EmpID = frm.GetUser().Trim();
                        string Pw = frm.GetPw().Trim();

                        if (EmpID == "" || Pw == "")
                        {
                            AutoClosingMessageBox.Show("(*) Em.ID/ Password is empty! Try again! Mã NV/ Mật không để trống! Thử lại!", "AutoCloseMessageBox", 10000);
                        }
                        else
                        {
                            SqlConnection conn = new SqlConnection(connectionString);

                            conn.Open();
                            SqlDataReader reader;
                            string sqlStr = @"select * from Users where username='" + EmpID + "' and password = '" + Pw + "'";
                            SqlCommand cmd = new SqlCommand(sqlStr, conn);
                            SqlDataAdapter da = new SqlDataAdapter(sqlStr, conn);
                            DataSet ds = new DataSet();
                            da.Fill(ds);
                            reader = cmd.ExecuteReader();
                            if (reader.HasRows)
                            {
                                ShowUIApp.AutoClosingMessageBox.Show("Bạn có 15 phút để tìm ra nguyên nhân & sửa lỗi. Sau đó, nhập Action để hoàn thành việc mở khóa máy", "AutoClosingMessage", 4000);
                                if (isFirstTimeMinimized == 0)
                                {
                                    startMinimized = DateTime.Now;
                                    //MessageBox.Show("--->Start time:"+startMinimized.ToShortDateString());
                                    isFirstTimeMinimized = 1;
                                }

                                isMinimized = true;
                                File.WriteAllText(@".\frmActive.ini", "[Active]\nStatus=1");
                                ul.SetValueByKey("StopMachine", "0");
                                ul.SetValueByKey("FixFlag", "");

                                SetStopMachineStatus(false);

                                DelayTimer.Enabled = true;
                                DelayTimer.Interval = 1000;
                                this.WindowState = FormWindowState.Minimized;
                                this.Hide();
                            }
                            else
                            {
                                ShowUIApp.AutoClosingMessageBox.Show("wrong username & password", "AutoClosingMessage", 4000);
                            }
                            da.Dispose();
                            reader.Close();
                            conn.Close();
                        }// end check empty
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private string _model_name, _line_name, _station_name, _ate_name, _ate_ip, serverIp;

        private bool isInterfereceShowUI = false;

        public void InterferenceStop()
        {
            if (ul.GetValueByKey("INTERFERENCE_PC") == ul.GetNICGatewayIP())
            {
                isInterfereceShowUI = true;
                ToDB conn = new ToDB();
                string InterfereceClientIP = ul.GetValueByKey("INTERFERENCE_CLIENT_PC");

                string updateSql = "update Status set stop_status = 1, stop_ate = '1Interference_" + sName + "' where";
                string[] ipClient = InterfereceClientIP.Split(',');
                foreach (string _ip in ipClient)
                {
                    if (updateSql.EndsWith("where"))
                    {
                        updateSql += " ate_ip = '" + _ip.Trim() + "'";
                    }
                    else
                    {
                        updateSql += " or ate_ip = '" + _ip.Trim() + "'";
                    }
                }
                conn.Execute_NonSQL(updateSql, serverIp);
            }
        }

        public void InterferenceStart()
        {
            if (ul.GetValueByKey("INTERFERENCE_PC") == ul.GetNICGatewayIP())
            {
                isInterfereceShowUI = true;
                ToDB conn = new ToDB();
                string InterfereceClientIP = ul.GetValueByKey("INTERFERENCE_CLIENT_PC");

                string updateSql = "update Status set stop_status = 0, stop_ate = null where";
                string[] ipClient = InterfereceClientIP.Split(',');
                foreach (string _ip in ipClient)
                {
                    if (updateSql.EndsWith("where"))
                    {
                        updateSql += " ate_ip = '" + _ip.Trim() + "'";
                    }
                    else
                    {
                        updateSql += " or ate_ip = '" + _ip.Trim() + "'";
                    }
                }
                conn.Execute_NonSQL(updateSql, serverIp);
            }
        }

        public void SetStopMachineStatus(bool updateSingle)
        {
            try
            {
                ToDB conn = new ToDB();
                if (updateSingle)
                {
                    // update
                    string updateSql = "update Status set stop_status = 0, stop_ate = null";
                    updateSql += " where model_name ='" + _model_name + "' and  line_name = '" + _line_name + "' and station_name = '" + _station_name + "' and  ate_name ='" + _ate_name + "' and ate_ip='" + _ate_ip + "'";
                    conn.Execute_NonSQL(updateSql, serverIp);
                }
                else
                {
                    // update all ate in station of a line testing 1 model
                    string updateSql = "update Status set stop_status = 0, stop_ate = null";
                    updateSql += " where model_name ='" + _model_name + "' and  line_name = '" + _line_name + "' and station_name = '" + _station_name + "'";
                    conn.Execute_NonSQL(updateSql, serverIp);
                }
            }
            catch (Exception r)
            {
                //MessageBox.Show(r.ToString());
            }
        }

        private void lbBoom_Click(object sender, EventArgs e)
        {
        }

        private void tbl_unlockData_Click(object sender, EventArgs e)
        {
            using (frmUnlockRequirements frm = new frmUnlockRequirements(true, lbBoom.Text, startTime))
            {
                this.TopMost = false;
                frm.TopMost = true;

                if (frm.ShowDialog() == DialogResult.OK)
                {
                    ul.SetValueByKey("StopMachine", "0");
                    ul.SetValueByKey("LockTime", DateTime.Now.ToString());
                    this.Close();
                }
                else
                {
                    AutoClosingMessageBox.Show("Server Err, contact TE", "ERR 500", 2000);
                }
            }
        }

        public bool GetStopMachineStatus()
        {
            bool rturnValue = false;
            try
            {
                //_model_name = "C7000-100NASV1";
                //_line_name = "13";
                //_station_name = "PT";
                //_ate_name = "B05-L13-PT01";
                //_ate_ip = "10.224.84.171";

                ToDB conn = new ToDB();
                string sqlIsLocked = "select stop_status,stop_ate from Status where model_name ='" + _model_name + "' and  line_name = '" + _line_name + "' and station_name = '" + _station_name + "' and  ate_name ='" + _ate_name + "' and ate_ip='" + _ate_ip + "'";
                DataTable dt = conn.DataTable_Sql(sqlIsLocked, serverIp);
                if (dt.Rows.Count != 0)
                {
                    string rsult = dt.Rows[0]["stop_status"].ToString();

                    if (rsult == "0")
                    {
                        if (ul.GetValueByKey("ErrStation") == "")
                        {
                            SetWebUnlockPath("StopStation", "0Default");
                        }
                        rturnValue = true;
                    }
                }
                return rturnValue;
            }
            catch (Exception e)
            {
                ul.event_log("GetStopMachineStatus: " + e.ToString());
                if (ul.GetValueByKey("ErrStation") == "")
                {
                    SetWebUnlockPath("StopStation", "0Default");
                }
                return true;
            }
        }

        private void DelayTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                CountDelay++;
                if (CountDelay == 900) // if 900 = 15' hasnot fix
                {
                    DelayTimer.Enabled = false;
                    lblClose.Visible = false;
                    endMinimized = DateTime.Now;
                    //MessageBox.Show("--->End Time:" + endMinimized.ToShortDateString());
                    //File.Delete(@".\frmActive.ini");
                    ul.SetValueByKey("StopMachine", "1");
                    ul.SetValueByKey("FixFlag", "");
                    lbGeneralMsg.Text = "Timeout! Hãy Nhập Actions Để Hoàn Thành Mở Máy";
                    ul.SetValueByKey("BYRate", "0");
                    this.WindowState = FormWindowState.Normal;
                    this.Show();
                    this.ShowIcon = false;
                    this.ShowInTaskbar = false;
                }
            }
            catch (Exception)
            {  //throw;
            }
        }

        private int isClicked = 0;

        private void tbxAction_Click(object sender, EventArgs e)
        {
            if (isClicked == 0)
            {
                tbxAction.Text = "";
                Process.Start("osk.exe");
                isClicked = 1;
            }

            //tbxAction.Focus();
            //this.TopMost = true;
        }

        private void SendUdpSocket(string IPaddr, int Port, string Msg)
        {
            try
            {
                IPAddress.Parse(IPaddr);
            }
            catch
            {
                return;
            }
            UdpClient udpClient1 = new UdpClient();
            udpClient1.Connect(IPAddress.Parse(IPaddr), Port);
            udpClient1.EnableBroadcast = true;

            //udpClient1.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 0);
            Byte[] senddata = Encoding.ASCII.GetBytes(Msg);
            udpClient1.Send(senddata, senddata.Length);
            udpClient1.Close();
        }

        protected string GetRegPath()
        {
            string rgPath = SubKey;

            _OpenKey = Registry.LocalMachine.OpenSubKey(SubKey + "\\" + ul.GetProduct() + "\\" + ul.GetStation());
            try
            {
                if (_OpenKey != null)
                {
                    rgPath = SubKey + "\\" + ul.GetProduct() + "\\" + ul.GetStation();
                    return rgPath;
                }
                return rgPath;
            }
            catch (Exception)
            {
                return rgPath;
            }
        }
    }
}