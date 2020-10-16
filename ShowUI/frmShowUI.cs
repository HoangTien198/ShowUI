using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Linq;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Net.NetworkInformation;

using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Reflection;
using System.Management;

using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Media;
using ShowUI.B05_SERVICE_CENTER;
using Fii;
using ShowUI.Helper;
using ShowUI;
using ShowUI.AutomationHelper;
using ShowUI.Common;

namespace ShowUIApp
{
    public partial class showUI : Form
    {
        const string subkey = @"SOFTWARE\NETGEAR\STATION";
        string currentDirectory = Environment.CurrentDirectory;
        string serverPath = @"F:\lsy\ID\SPC_Folder";
        string openkey, orgPass = "0";
        RegistryKey _OpenKey, _StationKey;

        string sName = Environment.MachineName;
        string sModel, sType, sModelName, _Model, client_ip = "", client_mac = "";
        DateTime nDate;
        IniFile fSpcControl, ModelConfig, fSPCspec;
        float YRGreen, YRYellow, SRRGreen, SRRYellow, TRRGreen, TRRYellow;
        string connectionStringSrv37, UpdateStatus = "x", connSrv60, svConnSyncDate;
        frmChart _Chart = new frmChart();
        string xxx = "xxx";
        string pathUpdate = @"F:\lsy\Test\DownloadConfig\AutoDL\";
        bool running = false;
        DateTime timeSleep = DateTime.Now;
        bool fake = false;
        //int mark = 0;
        int markPass;
        //int markCurPass;
        //float YR = 0;
        Random rd = new Random();
        PictureBox[] pbCable = new PictureBox[13];
        //PictureBox[] pbUSB = new PictureBox[4];
        ToolTip[] tl = new ToolTip[13];
        bool FKey, UseSpecMode; // SpecMode = false -> use web QA config mode, = true use TE config mode / Fkey= false -> lock, true, not lock
        string _TypeOfManufacturing, Shift, ModelNameChangeATE, CheckSum, CurrentStation;
        string YRStopSpec = "0.0";
        string RTRStopSpec = "100";
        int TestTimeHighLimit = 36000;
        int TestTimeLowLimit = 5;
        int TestedDUT = 0;
        int CountTimeToFix = 0;
        bool iModel = true;
        bool iFixture = false;
        bool iWanna = true;
        bool iError = true;
        //DateTime dtIdle = DateTime.Now;
        //int flagIdleMode = 0;


        //20.2.2016 to decide connect to server or not
        string ConnectServer37 = "0";
        string ConnectServer60 = "0";
        string ConnectServer73 = "0";
        string ConnectServer63 = "0";
        string ConnectServerSfis = "0";
        string BoolUpdateConnectorUsingTime = "0";
        ShowUI.Utilities ul = new ShowUI.Utilities();
        //29.2.2016
        string sfis_mo, config_mo, reg_mo, enableCheck_mo;
        //string mo_connectString = "";
        // for storing first time num of cable
        string debug = "";
        string disableLockMachine = "0";
        bool firstCheckSp = true;
        // 2016.06.04 for lock 1 tester for special purpose
        //station compare
        bool checkStationCompare = false;
        string stationCompare = "";

        public showUI()
        {
            InitializeComponent();
            //CompareStation();
            // checkValueCurrent();
        }
        private bool checkPort445()
        {
            try
            {
                string[] lines;
                try
                {
                    lines = File.ReadAllLines(@"F:\lsy\Test\DownloadConfig\AutoDL\Unlock445.txt");
                }
                catch (Exception e)
                {
                    lines = null;
                }

                if (lines != null)
                {
                    foreach (string name in lines)
                    {
                        if (sName == name)
                        {
                            return false;
                        }
                    }
                }


                using (TcpClient tcpclient = new TcpClient())
                {
                    IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
                    IPAddress[] ipAddress = ipHostInfo.AddressList;
                    foreach (IPAddress i in ipAddress)
                    {
                        if (!i.ToString().Contains("192.168"))
                        {
                            try
                            {
                                tcpclient.Connect(i, 445);
                                return true;
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                event_log(DateTime.Now.ToString() + " : Check port 445 error : " + ex.Message);
                return false;
            }

        }
        //protected void CompareStation()
        //{
        //    try
        //    {

        //        ConnectShowUI conn = new ConnectShowUI();
        //        string sql = $"select top 1 * from CompareStation where StationReg='{ul.GetStation()}' and Enable = 1";
        //        var row = conn.DataTable_Sql(sql, "10.224.81.62");

        //        if (row.Rows.Count > 0)
        //        {
        //            checkStationCompare = true;
        //            stationCompare = row.Rows[0][2].ToString();
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        checkStationCompare = false;
        //        stationCompare = "";
        //    }
        //}
        private string checkRickSoftware()
        {
            try
            {
                B05_Service service = new B05_Service();
                string rs = service.checkRickSoftware(sName);
                if (rs != "")
                {
                    return rs;
                }
                return "";
            }
            catch (Exception ex)
            {
                event_log(DateTime.Now.ToString() + " : Check rick software error : " + ex.Message);
                return "";
            }
        }
        public void checkWannacry()
        {
            //ShowUI.ISCService.Servicepostdata ISC = new ShowUI.ISCService.Servicepostdata();
            try
            {

                IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
                string myIP = ipHostInfo.AddressList[0].ToString();
                //bool Wresult = CENTER_B05_SV.checkwannacry(myIP);
                bool Wresult = ISCB05.checkwannacry(myIP);
                if (Wresult == false)
                {
                    iWanna = false;
                    event_log("Check Wannacry : " + myIP + " doesnot update wannacry!");
                }

            }
            catch (Exception ex)
            {
                event_log("Check Wannacry error: " + ex.Message.ToString());
            }

        }
        bool fail3in10 = false;
        string p1 = "";
        string p2 = "";
        string p3 = "";
        //int isNTGR = 0;
        public void LockFail3in10()
        {
            try
            {
                if (ul.GetValueByKey("ERRORCODE") != "" && p1 == "")
                {
                    p1 = ul.GetValueByKey("QtybyTester").ToString();
                }
                else if (ul.GetValueByKey("ERRORCODE") != "" && p2 == "")
                {
                    p2 = ul.GetValueByKey("QtybyTester").ToString();
                }
                else if (ul.GetValueByKey("ERRORCODE") != "" && p3 == "")
                {
                    p3 = ul.GetValueByKey("QtybyTester").ToString();
                }
                if (p1 != "" && p2 != "" && p3 != "")
                {
                    if ((int.Parse(p3) - int.Parse(p1)) <= 10)
                    {
                        fail3in10 = true;
                        p1 = p2;
                        p2 = p3;
                        p3 = "";
                    }
                    else
                    {
                        p1 = p2;
                        p2 = p3;
                        p3 = "";
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        public void checkErrorLockStation()
        {
            string pServer = @"F:\lsy\Test\DownloadConfig\AutoDL\";
            //string pServer = @"D:\AutoDL\";
            FileInfo fConfig = new FileInfo(pServer + "EModelConfig.txt");
            string KeyModel = @"SOFTWARE\Netgear\AutoDL";
            RegistryKey rSN = Registry.LocalMachine.OpenSubKey(KeyModel, true);
            string Model = rSN.GetValue("MODELNAME", "").ToString().Trim();
            string station = rSN.GetValue("STATION", "").ToString().Trim();
            string fLine = "", fStation, fError;
            if (fConfig.Exists)
            {

                string fstr = IniFile.ReadIniFile(Model, "Station", "", fConfig.ToString());
                if (fstr.Contains(","))
                {
                    string[] fstring = fstr.Split(',');
                    for (int i = 0; i < fstring.Length; i++)
                    {

                        fLine = fstring[i].Trim();
                        if (fLine.Length > 0)
                        {
                            fStation = fLine.Split(':')[0].ToString();
                            fError = fLine.Split(':')[1].ToString();
                            string Err = ul.GetValueByKey("ERROR_CODE");
                            if (station == fStation && fError == Err)
                            {
                                SetWebUnlockPath("StopStation", "1" + Err);
                                ul.SetValueByKey("ErrStation", Err);
                            }
                        }

                    }
                }
            }
        }
        public void CheckFixture()
        {
            //string fixture = ul.GetValueByKey("FIXTURE_NAME").ToString();
            //string Fixpath = ul.GetValueByKey("FIXTURE_PATH").ToString();
            string KeyModel = @"SOFTWARE\Netgear\AutoDL";
            string PAutoDL = @"D:\AutoDL\";
            //string PAutoDL = @"D:\ADELE\SHOWUI\ShowUI_Sona\ShowUI\ShowUI\bin\Release";
            RegistryKey rSN = Registry.LocalMachine.OpenSubKey(KeyModel, true);

            string Fixpath = rSN.GetValue("FIXTURE_PATH", "").ToString().Trim();
            string fixture = rSN.GetValue("FIXTURE_NAME", "").ToString().Trim();
            string local = rSN.GetValue("LOCAL_PATH", "").ToString().Trim();

            FileInfo fServer = new FileInfo(Fixpath + fixture);
            FileInfo fLocal = new FileInfo(local + "Fixtute.txt");
            string dServer = fServer.LastWriteTime.ToString();
            string dLocal = fLocal.LastWriteTime.ToString();

            string source = Path.Combine(Fixpath, fixture);
            string dest = Path.Combine(PAutoDL, fixture);

            string source1 = Path.Combine(local, "Fixture.txt");
            string dest1 = Path.Combine(PAutoDL, "Fixture.txt");

            if (File.Exists(dest))
            {
                File.Delete(dest);
            }
            if (File.Exists(dest1))
            {
                File.Delete(dest1);
            }
            if (File.Exists(source))
            {
                File.Copy(source, dest, true);
            }
            if (File.Exists(source1))
            {
                File.Copy(source1, dest1, true);
            }


            CRC32Calc crc32Program = new CRC32Calc();
            FileStream f2 = new FileStream(Path.Combine(Fixpath, fixture), FileMode.Open, FileAccess.Read, FileShare.Read, 8192);
            FileStream f1 = new FileStream(Path.Combine(local, "Fixture.txt"), FileMode.Open, FileAccess.Read, FileShare.Read, 8192);
            UInt32 chkf2 = crc32Program.GetCrc32(f2);
            UInt32 chkf1 = crc32Program.GetCrc32(f1);
            if (chkf1 != chkf2)
            {
                //iFixture = true;

                System.Diagnostics.Process pro = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                //local = local.Replace("\\","\\\\");
                //string strcmd = " /c findstr /i /L /x /v  /g:" + local + "Fixture.txt" + " " + local + fixture + ">unique.txt";
                string strcmd = " /c findstr /i /L /x /v  /g:Fixture.txt " + fixture + ">unique.txt";

                startInfo.Arguments = strcmd;
                pro.StartInfo = startInfo;
                pro.Start();
                //Process.Start("cmd.exe", strcmd);
                string NewLine = "";
                //FileInfo fUni = new FileInfo(PAutoDL + "unique.txt");
                FileInfo fUni = new FileInfo(PAutoDL + "unique.txt");
                int count = 0;
                iFixture = false;
                if (fUni.Exists)
                {
                    //string[] allLines = File.ReadAllLines(PAutoDL + "unique.txt");
                    string[] allLines = File.ReadAllLines(PAutoDL + "unique.txt");

                    for (int iLine = 0; iLine < allLines.Length; iLine++)
                    {
                        NewLine = allLines[iLine].Trim();
                        if (!string.IsNullOrEmpty(NewLine.Trim()))
                        {
                            if (NewLine.Contains("Cycle"))
                            {
                            }
                            else if (NewLine.Contains("Autorun"))
                            {
                            }
                            else
                            {
                                count++;
                            }
                        }
                    }
                    if (count > 0)
                    {
                        iFixture = true;
                    }
                }
            }
            //crc32.GetCrc32()
        }
        public void checkValueCurrent()
        {

            markPass = System.Convert.ToInt32(_StationKey.GetValue("PASS", "").ToString());
            //markFail = System.Convert.ToInt32(_StationKey.GetValue("FAIL", "").ToString());
        }
        public bool check_spec(string src, string des, float spec)
        {
            int i = 0;
            float _src, _des;
            string[] tgSrc = new string[10];
            string[] tgDes = new string[10];
            tgSrc = src.Split(' ');
            tgDes = des.Split(' ');
            //if (tgDes.Length != tgSrc.Length) return false;
            for (i = 0; i < tgSrc.Length; i++)
            {
                try
                {
                    _des = Convert.ToSingle(tgDes[i]);
                    _src = Convert.ToSingle(tgSrc[i]);
                }
                catch
                {
                    return false;
                }
                if ((_des < (_src - spec)) || (_des > (_src + spec)))
                    return false;

            }
            return true;
        }
        private string DeCrypt(string strDecypt, string key)
        {
            try
            {
                byte[] keyArr;
                byte[] DeCryptArr = Convert.FromBase64String(strDecypt);
                MD5CryptoServiceProvider MD5Hash = new MD5CryptoServiceProvider();
                keyArr = MD5Hash.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                TripleDESCryptoServiceProvider tripDes = new TripleDESCryptoServiceProvider();
                tripDes.Key = keyArr;
                tripDes.Mode = CipherMode.ECB;
                tripDes.Padding = PaddingMode.PKCS7;
                ICryptoTransform transform = tripDes.CreateDecryptor();
                byte[] arrResult = transform.TransformFinalBlock(DeCryptArr, 0, DeCryptArr.Length);
                return UTF8Encoding.UTF8.GetString(arrResult);
            }
            catch (Exception exx)
            {
                return exx.Message.ToString();
            }
        }

        private void CheckSPC_Tick(object sender, EventArgs e)
        {
            //sync time server
            //System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
            //psi.CreateNoWindow = true;
            //psi.UseShellExecute = false;
            //psi.RedirectStandardInput = true;
            //psi.RedirectStandardOutput = true;
            //psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            //psi.FileName = "net.exe";
            //psi.Arguments = @"time \\10.224.81.37 /set /y";
            //System.Diagnostics.Process p = System.Diagnostics.Process.Start(psi);
            try
            {
                int widthsc = Screen.PrimaryScreen.WorkingArea.Width;
                this.Size = new Size(widthsc, 60);
                btnCall.SetBounds(widthsc - 88, 32, 83, 24);
                string newPass;
                if (xxx.Equals("#FLY"))
                {
                    xxx = "xxx";
                    IniFile.WriteValue("Jerry", "Boom", "0", pathUpdate + "Ctrl.ini");
                }
                if (_StationKey != null)
                {
                    newPass = _StationKey.GetValue("PASS", "1").ToString();
                    if (sType.Contains("PT") || sType.Contains("FT") || sType.Contains("RI") || sType.Contains("RC"))
                    {
                        if (!orgPass.Equals(newPass))
                        {
                            orgPass = newPass;
                            if (_StationKey != null)
                            {
                                sModel = _StationKey.GetValue("SFISDATA", "").ToString();
                                if (sModel.Length > 32)
                                {
                                    sModel = sModel.Substring(5, 25).Trim();
                                    sModelName = ModelConfig.ReadString("Model", sModel);
                                    fSPCspec = new IniFile(@"F:\lsy\Test\DownloadConfig\" + sModelName);
                                    try
                                    {
                                        writeSPCfile();
                                    }
                                    catch (Exception exx)
                                    {
                                        event_log("CheckSPC: " + exx.Message.ToString());
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                //throw;
            }

        }

        string tmpIps = ""; // get string of serverip;
        ShowUI.GET_ATE_SUM_TESTED_DUT.Servicepostdata getSumTestedDUT = new ShowUI.GET_ATE_SUM_TESTED_DUT.Servicepostdata();
        ShowUI.B05_SERVICE_CENTER.B05_Service CENTER_B05_SV = new ShowUI.B05_SERVICE_CENTER.B05_Service();
        ShowUI.SFISB05_SV.Servicepostdata sfisB05 = new ShowUI.SFISB05_SV.Servicepostdata();
        ShowUI.ISCB05_Service.B05_Service ISCB05 = new ShowUI.ISCB05_Service.B05_Service();
        private void showUI_Load(object sender, EventArgs e)
        {
            // CopyServerAuto.Enabled = true;
            try
            {
                TextWriter tw = new StreamWriter("SavedList.txt");
                tw.Dispose();
                tw.Close();
            }
            catch (Exception)
            {


            }
            #region timerCounter
            //Thread _tDU = new Thread(ShowTimeDontTest);
            //_tDU.IsBackground = true;
            //_tDU.Start();
            Thread _tInsertPCKPI = new Thread(InsertPCKPI);
            _tInsertPCKPI.IsBackground = true;
            _tInsertPCKPI.Start();
            #endregion
            #region Pathloss
            CheckPathloss();
            #endregion

            #region TcpAck
            Thread _tcpAck = new Thread(SetTcpAck);
            _tcpAck.IsBackground = true;
            _tcpAck.Start();
            #endregion


            #region DownLoadDLL
            Thread _downloadDll = new Thread(downLoadDll);
            _downloadDll.IsBackground = true;
            _downloadDll.Start();


            #endregion

            #region CheckNTGR
            //CheckNTGR check = new CheckNTGR();
            //isNTGR = check.CheckModel();


            #endregion

            #region download check antivirut
            //AntiVirusCheck.CreateFolder();
            #endregion

            #region CopyServerAuto
            //AutomationCopyPathlossHelper copyServerAuto = new AutomationCopyPathlossHelper();
            //copyServerAuto.CreateIni();
            //Copy117DB();
            //Thread.Sleep(200);
            #endregion

            #region IQTime


            IQSNtxt.Hide();
            //         //create Iqtestime folder and copy ini file
            //         IQtesttimehelper.CreateFolder();

            ////close process iqtesttime
            //Process[] pro = Process.GetProcessesByName("IQTestTimes");
            //foreach (var item in pro)
            //{
            //	item.Kill();
            //}
            //Process[] pro2 = Process.GetProcessesByName("IQTestTimesDetail");
            //foreach (var item in pro2)
            //{
            //	item.Kill();
            //}
            ////
            //if (!File.Exists(".\\FShowUIConfig.txt"))
            //{
            //	File.Create(".\\FShowUIConfig.txt");
            //}



            ////IniFile.WriteValue("Common", "Netgear", "false", ".\\FShowUIConfig.txt");
            //int IQ_Netgear = System.Convert.ToInt32(IniFile.ReadIniFile("COMMON", "IQ_Netgear", "0", ".\\Setup.ini"));
            //if (IQ_Netgear == 1)
            //{
            //	try
            //	{
            //		string ModelName = ul.GetProduct();
            //		_model_name = ul.GetValueByKey("SFISMODEL").Trim();
            //		string stationLocal = ul.GetStation();
            //		string pathCopy = IniFile.ReadIniFile(ModelName, ul.GetStation(), "loser", ".\\IQTESTTIME\\LocalDetailLog.ini");
            //		//string checkStation = IniFile.ReadIniFile("IQCheckStation", _model_name, "0", ".\\IQTESTTIME\\IQCheckStation.ini");
            //		string checkNetgearSql = $"select * from ProjectName where ProjectName ='{ModelName}'";
            //		ConnectShowUI connCheckNt = new ConnectShowUI();
            //		DataTable checkNt = connCheckNt.DataTable_Sql(checkNetgearSql, "10.224.81.62");
            //		if (checkNt.Rows.Count > 0)
            //		{
            //			string dicr = @"C:\LitePoint";
            //			if (System.IO.Directory.Exists(dicr) && pathCopy.Contains("loser"))
            //			{
            //				string filepath = @"C:\LitePoint\IQTestTimes.exe";
            //				if (System.IO.File.Exists(filepath))
            //				{
            //					File.Delete(filepath);

            //				}
            //				File.Copy(@"F:\lsy\Test\DownloadConfig\AutoDL\IQTestTimes.exx", @"C:\LitePoint\IQTestTimes.exe", false);
            //				if (Process.GetProcessesByName("IQTestTimes").Length < 1)
            //				{
            //					ProcessStartInfo startProgram = new ProcessStartInfo();
            //					startProgram.FileName = @"C:\LitePoint\IQTestTimes.exe";
            //					startProgram.WorkingDirectory = @"C:\LitePoint\";
            //					Process.Start(startProgram);
            //				}
            //			}
            //			else if (!pathCopy.Contains("loser"))
            //			{
            //				pathCopy = pathCopy + $"{_model_name}\\{stationLocal}";
            //				if (System.IO.Directory.Exists(pathCopy))
            //				{
            //					if (System.IO.File.Exists(pathCopy + "\\IQTestTimesDetail.exe"))
            //					{
            //						File.Delete(pathCopy + "\\IQTestTimesDetail.exe");
            //					}
            //					File.Copy(@"F:\lsy\Test\DownloadConfig\AutoDL\IQTestTimesDetail.exx", pathCopy + "\\IQTestTimesDetail.exe", false);
            //					if (Process.GetProcessesByName("IQTestTimesDetail").Length < 1)
            //					{
            //						ProcessStartInfo startProgram = new ProcessStartInfo();
            //						startProgram.FileName = pathCopy + "\\IQTestTimesDetail.exe";
            //						startProgram.WorkingDirectory = pathCopy + "\\";
            //						Process.Start(startProgram);
            //					}

            //				}


            //			}
            //		}
            //	}
            //	catch (Exception ex)
            //	{
            //	}
            //}
            //else
            //{
            //	string dicr = @"C:\LitePoint";
            //	if (System.IO.Directory.Exists(dicr))
            //	{
            //		string filepath = @"C:\LitePoint\IQTestTimes.exe";
            //		if (!System.IO.File.Exists(filepath))
            //		{
            //			File.Copy(@"F:\lsy\Test\DownloadConfig\AutoDL\IQTestTimes.exx", @"C:\LitePoint\IQTestTimes.exe", false);
            //		}
            //		if (Process.GetProcessesByName("IQTestTimes").Length < 1)
            //		{
            //			ProcessStartInfo startProgram = new ProcessStartInfo();
            //			startProgram.FileName = @"C:\LitePoint\IQTestTimes.exe";
            //			//startProgram.WorkingDirectory = Environment.CurrentDirectory;
            //			Process.Start(startProgram);
            //		}
            //	}
            //}

            #endregion

            #region FakeShowUI
            //string isNetgear = IniFile.ReadIniFile("Common", "Netgear", "false", ".\\FShowUIConfig.txt");
            //if (isNetgear.Contains("true"))
            //{
            //	try
            //	{
            //		client_ip = ul.GetNICGatewayIP();
            //		_model_name = ul.GetValueByKey("SFISMODEL").Trim();

            //		int ProjID;
            //		int PCID;
            //		int FakeID;
            //		int SpecID;
            //		ConnectShowUI conn = new ConnectShowUI();

            //		ul.SetValueByKey("StartCheck", DateTime.Now.ToString());

            //		int checkStation = conn.CreateOrUpdateDB("SationName", ul.GetStation(), "StationInfo", "10.224.81.62");
            //		if (checkStation == 0)
            //		{
            //			string insertSt = $"insert into StationInfo(SationName) values('{ul.GetStation()}')";
            //			checkStation = conn.CreateAndGetID("10.224.81.62", insertSt, "StationInfo");


            //		}
            //		int checkProj = conn.CreateOrUpdateDB("DotNamePro", _model_name, "ProjectInfo", "10.224.81.62", "StationID", checkStation.ToString());

            //		int checkPC = conn.CreateOrUpdateDB("PC_IP", client_ip, "PCInfo", "10.224.81.62");

            //		if (checkProj == 0)
            //		{
            //			string insertProj = $"insert into ProjectInfo(DotNamePro,StationID) values('{_model_name}',{checkStation})";
            //			ProjID = conn.CreateAndGetID("10.224.81.62", insertProj, "ProjectInfo");

            //		}
            //		else
            //		{
            //			string updateProj = $"update ProjectInfo set DotNamePro='{_model_name}', StationID = {checkStation} where ProjectID={checkProj}";
            //			ProjID = conn.CreateAndGetID("10.224.81.62", updateProj, "ProjectInfo", "ProjectID", checkProj.ToString());

            //		}
            //		if (checkPC == 0)
            //		{
            //			string insertPC = $"insert into PCInfo(PC_IP,PC_Name,ProjectID) values('{client_ip}','{sName}',{ProjID}) ";
            //			PCID = conn.CreateAndGetID("10.224.81.62", insertPC, "PCInfo");

            //		}
            //		else
            //		{
            //			string updatePC = $"update PCInfo set PC_IP ='{client_ip}' ,PC_Name='{sName}',ProjectID={ProjID} where PC_ID={checkPC}";
            //			PCID = conn.CreateAndGetID("10.224.81.62", updatePC, "PCInfo", "PC_IP", client_ip);

            //		}
            //		FakeShowUIHelper fake = new FakeShowUIHelper();
            //		var configFake = fake.FakeUI(_model_name, ul.GetStation());
            //		int checkFake = conn.CreateOrUpdateDB("ProjectID", ProjID, "FProject", "10.224.81.62");
            //		int checkSpec = conn.CreateOrUpdateDB("ProjectID", ProjID, "Spec", "10.224.81.62");
            //		if (configFake != null)
            //		{
            //			if (checkFake == 0)
            //			{
            //				string insertFake = $"insert into FProject(ProjectID,Fake) values({ProjID},1) ";
            //				FakeID = conn.CreateAndGetID("10.224.81.62", insertFake, "FProject");
            //			}
            //			else
            //			{
            //				string updateFake = $"update FProject set ProjectID = {ProjID},Fake=1 where ID={checkFake} ";
            //				FakeID = conn.CreateAndGetID("10.224.81.62", updateFake, "FProject", "ProjectID", ProjID.ToString());
            //			}
            //			if (checkSpec == 0)
            //			{
            //				string insertSpec = $"insert into Spec(ProjectID,TRR,TRRLow,TRRHight,SRR,SRRLow,SRRHight,YR,YRLow,YRHight) values({ProjID},{configFake.fakeTRR},{configFake.spaceRandTRR1},{configFake.spaceRandTRR2},{configFake.fakeSRR},{configFake.spaceRandSRR1},{configFake.spaceRandSRR2},{configFake.fakeTYR},{configFake.spaceRandTYR1},{configFake.spaceRandTYR2}) ";
            //				SpecID = conn.CreateAndGetID("10.224.81.62", insertSpec, "Spec");
            //			}
            //			else
            //			{
            //				string updateSpec = $"update Spec set ProjectID = {ProjID},TRR={configFake.fakeTRR},TRRLow={configFake.spaceRandTRR1},TRRHight={configFake.spaceRandTRR2},SRR={configFake.fakeSRR},SRRLow={configFake.spaceRandSRR1},SRRHight={configFake.spaceRandSRR2},YR={configFake.fakeTYR},YRLow={configFake.spaceRandTYR1},YRHight={configFake.spaceRandTYR2} where SpecID={checkSpec} ";
            //				SpecID = conn.CreateAndGetID("10.224.81.62", updateSpec, "Spec", "ProjectID", ProjID.ToString());
            //			}

            //		}
            //		//else
            //		//{
            //		//    if (checkFake == 0)
            //		//    {
            //		//        string insertFake = $"insert into FProject(ProjectID,Fake) values({ProjID},0) ";
            //		//        FakeID = conn.CreateAndGetID("10.224.81.62", insertFake, "FProject");
            //		//    }
            //		//    else
            //		//    {
            //		//        string updateFake = $"update FProject set ProjectID = {ProjID} where,Fake=0";
            //		//        FakeID = conn.CreateAndGetID("10.224.81.62", updateFake, "FProject", "ProjectID",ProjID.ToString());
            //		//    }
            //		//}



            //	}
            //	catch (Exception ex)
            //	{
            //		MessageBox.Show("lOI O DAY");
            //		MessageBox.Show(ex.ToString());
            //	}

            //}
            #endregion

            #region Common
            try
            {

                lblTotalRateFake.Hide();
                lblRetestRateFake.Hide();
                lblYeildRateFake.Hide();
                //string KeySN = @"SOFTWARE\Netgear\SaveSN\OlyAdeleUse";
                //RegistryKey rSN = Registry.LocalMachine.OpenSubKey(KeySN, true);
                //string strSN;
                //strSN = rSN.GetValue("SN", "").ToString();
                //DataTable dtMO = sfisB05.FIND_MO_NUMBER(strSN, "");
                //ul.event_log("New SN: " + strSN);
                //string mo_number = "";
                //mo_number = dtMO.Rows[0][0].ToString();
                //ul.SetValueByKey("MO", mo_number);
                //lblMO.Text = mo_number.Trim();
                //rSN.SetValue("MO", mo_number);
                //double newYRS =0;

                //double newTRRS = 0;
                //double RetestRate =  0;
                //MessageBox.Show(CENTER_B05_SV.B05_SI_GETFAKE_YR("", "", "RBW30-FXN-NAS","PT", "", "", ref newYRS, ref newTRRS));
                //passData = new string[6];


                //connectionStringSrv37 = @"Data Source=10.224.81.37,1433;Initial Catalog=Ars_System;uid=sa;pwd=********;Connection Timeout=5";
                //passData[0] = "7";
                //passData[1] = "PT";
                //passData[2] = "B06-L7-PT-01";
                //passData[3] = "138.101.13.71";
                //passData[4] = "U12H340";// U12H333
                //passData[5] = "RBW30-FXN-NAS";// EX6700-NASV1
                //double TotalRate = ul.GreenShowUI(newTRRS, connectionStringSrv37, ConnectServer37, passData);

                CheckForIllegalCrossThreadCalls = false;
                //    string start_time = "201609240730";
                //    string end_time = "201609240930";
                //    DataTable dtRTRate = svGetData.GET_FULL_R_STATION_ATE_T(start_time, end_time);

                //    DataRow[] results = dtRTRate.Select("STATION_NAME = 'B05-L1-FT1-2' AND MODEL_NAME='VMC4030-FXN' AND GROUP_NAME='FT1'");
                //    MessageBox.Show(dtRTRate.Rows.Count + " " + results.Length);
                //    DataTable dtYRate = svGetData.GET_R_STATION_REC_T(start_time, end_time);
                //    results = dtYRate.Select("SECTION_NAME = 'SI' AND MODEL_NAME='VMC4030-FXN' AND GROUP_NAME='FT1'");
                //    MessageBox.Show(dtYRate.Rows.Count + " " + results.Length);
                //    return;

                event_log("ShowUI starting ...");

                //2019.04.18 Nathan add for auto copy dll file
                try
                {
                    if (!File.Exists(@".\zxing.dll"))
                    {
                        File.Copy(@"F:\lsy\Test\DownloadConfig\AutoDL\zxing.dll", @"D:\AutoDL\zxing.dll", true);
                        Application.Exit();
                    }
                }
                catch (Exception exxx)
                {
                    MessageBox.Show("Error loading : " + exxx.Message);
                }

                //Thread _tControlRunCtrl = new Thread(ControlRunCtrl);
                //_tControlRunCtrl.IsBackground = true;
                //_tControlRunCtrl.Start();
                ///return;
                //if (GetPercentFreeSpace("D:\\") < 0.3)
                //{
                //    MessageBox.Show("o dia D gan full");
                //}
                CheckNetConnection();
                // check update showUI
                if (NetWorkConnection)
                {
                    UpdateShowUI();
                    //put in the first
                    ServerDisableStatus();

                    // Thread _tSpecialLockExecute = new Thread(SpecialLockExecute);
                    // _tSpecialLockExecute.IsBackground = true;
                    //_tSpecialLockExecute.Start();


                    Thread _tGlobalParInitial = new Thread(GlobalValueInitialize);
                    _tGlobalParInitial.IsBackground = true;
                    _tGlobalParInitial.Start();


                    try
                    {
                        connectionStringSrv37 = @"Data Source=10.224.81.37,1433;Initial Catalog=Ars_System;uid=sa;pwd=********;Connection Timeout=5";
                        tmpIps += "ARS_System:10.224.81.37";
                        connSrv60 = @"Data Source=10.224.81.62;Initial Catalog=dbGeneral;uid=sa;pwd=********;Connection Timeout=5";
                        tmpIps += " dbGeneral:10.224.81.62";
                        svConnSyncDate = @"Data Source=10.224.81.73;Initial Catalog=ShowUI;uid=sa;pwd=Password123;Connection Timeout=5";
                        tmpIps += " ShowUI:10.224.81.73";
                        //Get ServrerIp for ToDB() dbMO 

                        serverIp = IniFile.ReadIniFile("DATABASE", "SERVER_NAME", "10.224.81.62", @"F:\Temp\TE-PROGRAM\TE-DATABASE\SOURCE.ini");

                        string modelNPI = ul.GetValueByKey("SFISMODEL");
                        string NPIShowUI = IniFile.ReadIniFile("ENABLE_SHOWUI", modelNPI, "0", @"F:\lsy\Test\DownloadConfig\Setup.ini");
                        // for show UI from local need set config file
                        string enableShowUI = IniFile.ReadIniFile("Debug", "Enable", "0", @".\Debug.ini");

                        if (NPIShowUI == "1" && enableShowUI == "0")
                        {
                            npiflag = true;
                            panel1.Visible = false;
                            flowPanel.Visible = false;
                            panel2.Visible = false;
                            panel3.Visible = false;
                            //btnCall.Visible = false;
                            fPanelHData.Visible = false;
                            notifyShowUI.Visible = true;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }

                if (NetWorkConnection)
                {
                    //MessageBox.Show(serverIp);
                    // Synd timer with server firstly

                    Thread _tSync = new Thread(SysnSvTime);
                    _tSync.IsBackground = true;
                    _tSync.Start();
                    //

                    /// Make sure all tester maped to wireless shared folder of 37 & 60 so that CopyToServer function work correctly
                    //Thread _tMapToWirelessServer = new Thread(NetAuthentication);
                    // _tMapToWirelessServer.IsBackground = true;
                    //_tMapToWirelessServer.Start();

                    /// 2016.03.26 Check Chanel.txt file
                    Thread _tCheckChanelSpecRange = new Thread(CheckChanelSpecRange);
                    _tCheckChanelSpecRange.IsBackground = true;
                    _tCheckChanelSpecRange.Start();

                    try
                    {
                        System.Diagnostics.FileVersionInfo AD_server = System.Diagnostics.FileVersionInfo.GetVersionInfo(@"F:\lsy\Test\DownloadConfig\AutoDL\NetgearAutoDL.exx");
                        Version AD_SV_ver = new Version(AD_server.FileVersion);
                        System.Diagnostics.FileVersionInfo AD_local = System.Diagnostics.FileVersionInfo.GetVersionInfo(@"D:\AutoDL\NetgearAutoDL.exe");
                        Version AD_LC_ver = new Version(AD_local.FileVersion);
                        switch (AD_SV_ver.CompareTo(AD_LC_ver))
                        {
                            case 1:
                                System.Diagnostics.Process[] kill_AD = System.Diagnostics.Process.GetProcessesByName("NetgearAutoDL");
                                foreach (System.Diagnostics.Process p in kill_AD)
                                    p.Kill();
                                ShellExecute("taskkill /IM NetgearAutoDL.exe /F");

                                File.Copy(@"F:\lsy\Test\DownloadConfig\AutoDL\NetgearAutoDL.exx", @"D:\AutoDL\NetgearAutoDL.exe", true);
                                System.Diagnostics.Process.Start(@"D:\AutoDL\NetgearAutoDL.exe");
                                event_log("Update NetgearAutoDL: OK");
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        //MessageBox.Show(r.ToString());
                    }

                }

                client_ip = ul.GetNICGatewayIP();

                if (NetWorkConnection)
                {
                    client_mac = GetMacAddress(client_ip);
                    if (client_mac != null)
                    {
                        client_mac = client_mac.Substring((client_mac.IndexOf("=") + 1), client_mac.Length - (client_mac.IndexOf("=") + 1)).Trim();
                    }
                    else
                    {
                        client_mac = "";
                    }

                    //
                    //syn time
                    IniFile fSetting = new IniFile(@"F:\lsy\Test\DownloadConfig\AutoDL\Setup.ini");
                    //string encrypt = fSetting.ReadString("CONNECTSQL", "SERVER");
                    //if(encrypt.Length<1)
                    //   encrypt = "IoANcBKCYRlY6XDg/Gc7yO5HSRTe9sgHcJBbCLWrMMoV/4q2pntfoGrWMrV3mq2LEma4PVUEWVbBDuQ04Dmdlad9WfPphVOTHgW3jQwccuGacUXX9zsdcw==";
                    //string encrypt = "IoANcBKCYRlY6XDg/Gc7yO5HSRTe9sgHcJBbCLWrMMoV/4q2pntfoGrWMrV3mq2LEma4PVUEWVbBDuQ04Dmdlad9WfPphVOTHgW3jQwccuGacUXX9zsdcw==";
                    serverPath = fSetting.ReadString("COMMON", "SPC_Folder");
                }

                this.Top = 0;
                this.Left = 0;
                int widthsc = Screen.PrimaryScreen.WorkingArea.Width;
                this.Size = new Size(widthsc, 60);
                //btnCall.SetBounds(widthsc - 88, 32, 83, 24);
                _OpenKey = Registry.LocalMachine.OpenSubKey(subkey);

                if (_OpenKey != null)
                {
                    openkey = _OpenKey.GetValue("OpenKey", "").ToString();
                    update_infoTPG();
                    if (_StationKey != null)
                        orgPass = _StationKey.GetValue("PASS", "1").ToString();
                    //MessageBox.Show(_StationKey.GetType().ToString());
                }
                else
                {
                    //get MAC off office computer if dont have subkey
                    //if (client_ip.StartsWith("10"))
                    //{
                    sType = "BOOMS";
                    //}

                }


                ///markPass =  System.Convert.ToInt32(_StationKey.GetValue("PASS", "1").ToString());


                //checkValueCurrent();

                lblStation.Text = sName;
                if (NetWorkConnection)
                {
                    ModelConfig = new IniFile(@"F:\lsy\Test\DownloadConfig\ModelConfig.ini");
                    fSpcControl = new IniFile(@"F:\lsy\Test\DownloadConfig\AutoDL\SPC_Color_Define.ini");
                    try
                    {
                        YRGreen = Convert.ToSingle(fSpcControl.ReadString("SPC_COLOR_SPEC", "SPCYieldGreen"));
                        YRYellow = Convert.ToSingle(fSpcControl.ReadString("SPC_COLOR_SPEC", "SPCYieldYellow"));
                        SRRGreen = Convert.ToSingle(fSpcControl.ReadString("SPC_COLOR_SPEC", "SPCCurGreen"));
                        SRRYellow = Convert.ToSingle(fSpcControl.ReadString("SPC_COLOR_SPEC", "SPCCurYellow"));
                        TRRGreen = Convert.ToSingle(fSpcControl.ReadString("SPC_COLOR_SPEC", "SPCStaGreen"));
                        TRRYellow = Convert.ToSingle(fSpcControl.ReadString("SPC_COLOR_SPEC", "SPCStaYellow"));
                    }
                    catch
                    {
                        YRGreen = 98.5F;
                        YRYellow = 97;
                        SRRGreen = 3;
                        SRRYellow = 5;
                        TRRGreen = 3;
                        TRRYellow = 5;
                    }

                }

                WriteSPC.Enabled = true;
                UpdateYR.Enabled = true;

                ////set cable & usb & virus
                for (int i = 0; i < 12; i++)
                {

                    pbCable[i] = new PictureBox();

                    pbCable[i].Name = i.ToString();
                    pbCable[i].Height = 23;
                    pbCable[i].Width = 12;
                    pbCable[i].Margin = new Padding(2, 3, 2, 3);
                    pbCable[i].Click += new EventHandler(OnClickDisplayPictureBox);
                    pbCable[i].Cursor = Cursors.Hand;
                    flowPanel.Controls.Add(pbCable[i]);
                    tl[i] = new ToolTip();
                    tl[i].Active = true;
                    tl[i].IsBalloon = true;

                }
                pbVirus.Click += new EventHandler(OnClickDisplayPictureBox);
                pbUsb.Click += new EventHandler(OnClickDisplayPictureBox);


                //
                //if (this.Width < 1024)
                //{
                int widthScreen = Screen.PrimaryScreen.WorkingArea.Width;
                ///flPanel.Controls.Add(lbMTP);

                if (npiflag == true)
                {
                    // for npi dont show retest yeild rate by range of time
                    int XflagLocation = Convert.ToInt32(lblYeildRate.Location.X.ToString());
                    int YflagLocation = Convert.ToInt32(lblYeildRate.Location.Y.ToString());
                    fPanelDate.SetBounds(XflagLocation + lblYeildRate.Width, YflagLocation + 1, 133, 25);
                    fPanelDate.BackColor = panel1.BackColor;
                    //
                    int x = Convert.ToInt32(fPanelDate.Location.X.ToString());
                    flowPanel.SetBounds(x + fPanelDate.Width, 0, 235, 30);

                    flowPanel.BackColor = Color.RoyalBlue;

                }
                else
                {

                    if (widthScreen >= 1280)
                    {
                        //MessageBox.Show(widthScreen+"widthScreen >= 1280");
                        //fPanelHData
                        int x = Convert.ToInt32(lblYeildRate.Location.X.ToString());
                        fPanelHData.SetBounds(x + lblYeildRate.Width, 0, 245, 30);
                        fPanelHData.BackColor = Color.RoyalBlue;
                        // set location of history yeildrate/ retest rate
                        //hflowPanel.SetBounds();

                        int fPanelHDataX = Convert.ToInt32(fPanelHData.Location.X.ToString());
                        int XlblShowUI = Convert.ToInt32(lblShowUI.Location.X.ToString());

                        int Xflag = fPanelHDataX + fPanelHData.Width;
                        //MessageBox.Show(fPanelHDataX + " + " + fPanelHData.Width + " = " + Xflag + " ? " + lblShowUI.Location.X + "");
                        //fPanelWidth = XlblShowUI - flowPanelX - 
                        //flowPanel.SetBounds(Xflag, fPanelHData.Location.Y, lblShowUI.Location.X - Xflag, 50);
                        flowPanel.SetBounds(Xflag, fPanelHData.Location.Y, lblShowUI.Location.X - Xflag, 30);
                        //flowPanel.SetBounds()
                        // panelReport.SetBounds(Xflag + flowPanel.Width, flowPanel.Location.Y, lblShowUI.Location.X - Xflag, 50);

                        flowPanel.BackColor = Color.RoyalBlue;
                        //panelReport.BackColor = Color.RoyalBlue;

                    }
                    else
                    {
                        // MessageBox.Show("widthScreen <= 1280");

                        //fPanelHData.SetBounds(flowPanel.Location.X - fPanelHData.Width, 29, 230, 30);
                        //lbhDuration.BackColor = Color.Lime;
                        //lbhDuration.ForeColor = Color.Blue;

                        fPanelHData.SetBounds(widthScreen - 450, 30, fPanelHData.Width, 30);

                        //flowPanel.SetBounds(fPanelHData.Location.X - flowPanel.Width - 10, 30, 230, 30); // minus 10 for not overlap
                        flowPanel.SetBounds(fPanelHData.Location.X + 50, 30, 230, 30);
                        flowPanel.SetBounds(flowPanel.Location.X + 50, 30, flowPanel.Width, 30);

                        //flowPanel.SetBounds()

                        flowPanel.FlowDirection = FlowDirection.RightToLeft;


                        fPanelDate.Visible = true;
                        fPanelDate.BackColor = Color.RoyalBlue;
                        //panelReport.BackColor = Color.RoyalBlue;

                        fPanelDate.SetBounds(lblYeildRate.Location.X + lblYeildRate.Width, lblYeildRate.Location.Y + 2, fPanelDate.Width, fPanelDate.Height);

                        int xlblShowUI = lblShowUI.Location.X;
                        //MessageBox.Show((fPanelDate.Location.X + fPanelDate.Width) + " ? " + xlblShowUI);
                        if (fPanelDate.Location.X + fPanelDate.Width >= xlblShowUI)
                        {
                            //fPanelDate.SetBounds(flowPanel.Location.X - fPanelDate.Width, flowPanel.Location.Y + 2, fPanelDate.Width, lbhRTRate.Height);
                            //fPanelDate.SetBounds(lblTotalRate)
                            fPanelDate.SetBounds(flowPanel.Location.X - fPanelDate.Width, flowPanel.Location.Y + 2, fPanelDate.Width, lblTotalRate.Height);
                        }
                        //Disable lbShowEvent 

                        lblShowUI.MouseHover -= new EventHandler(lblShowUI_MouseHover);
                        lblShowUI.MouseLeave -= new EventHandler(lblShowUI_MouseLeave);
                    }
                }

                // 


                //UpdateMOStation();
                //MessageBox.Show(GetLineOfTester());
                //end ^^
                //RegistryKey rkApp = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",true);
                //rkApp.SetValue("ShowUI",Application.ExecutablePath.ToString());
                // Get all ComputerInfo
                //if (client_ip.StartsWith("10"))
                //{
                //    ManagementObjectSearcher objsrch_ram = new ManagementObjectSearcher("select * from win32_PhysicalMemory");
                //    foreach (ManagementObject mo in objsrch_ram.Get())
                //    {
                //        UInt64 RamCapacity = Convert.ToUInt64(mo.Properties["Capacity"].Value.ToString()) / 1024 / 1024 / 1024;
                //        //MessageBox.Show(Convert.ToString(RamCapacity));
                //        break;
                //    }

                //    ManagementObjectSearcher objsrch_cpu = new ManagementObjectSearcher("select * from win32_processor");
                //    foreach (ManagementObject mo in objsrch_cpu.Get())
                //    {
                //        string CPU = Convert.ToString(mo.Properties["Name"].Value.ToString());
                //        //MessageBox.Show(Convert.ToString(CPU));
                //        break;
                //    }
                //    ManagementObjectSearcher objsrch_hdd = new ManagementObjectSearcher("select * from win32_DiskDrive");
                //    foreach (ManagementObject mo in objsrch_hdd.Get())
                //    {
                //        UInt64 hdd = Convert.ToUInt64(mo.Properties["Size"].Value.ToString()) / 1024 / 1024 / 1024;
                //        //MessageBox.Show(Convert.ToString(hdd));
                //        break;
                //    }
                //    ManagementObjectSearcher objsrch_monitor = new ManagementObjectSearcher("select * from win32_USBController");
                //    foreach (ManagementObject mo in objsrch_monitor.Get())
                //    {
                //        string MonitorSN = Convert.ToString(mo.Properties["Manufacturer"].Value.ToString());
                //        MessageBox.Show(Convert.ToString(MonitorSN));
                //        break;
                //    }

                //}
                //end get monitor information


                //Process[] name = Process.GetProcessesByName("AShengClient.exe");

                //if (name.Length < 1)
                //{
                //    AShengClientPath = @"C:\Windows\AShengClient.exe";
                //}
                //else
                //{
                //    MessageBox.Show(name.ToString());
                //}

                //foreach (Process proc in Process.GetProcesses())
                //{
                //    if (proc.ProcessName == "AShengClient")
                //    {
                //        AShengClientPath = proc.MainModule.FileName;
                //    }
                //    else
                //    {

                //    }
                //}


                //

                //MessageBox.Show(ConnectServer37 + "--" + ConnectServer60 + "--" + ConnectServer73 + "--" + ConnectServer63 + "--" + ConnectServerSfis);

                StopMachineEnable();
                SetRegistries();

                if (NetWorkConnection)
                {
                    //Thread _tGetStationIp = new Thread(GetIPofStation);
                    //_tGetStationIp.IsBackground = true;
                    //_tGetStationIp.Start();

                    Thread _tUpdateSpecByLine = new Thread(UpdateSpecByLine);
                    _tUpdateSpecByLine.IsBackground = true;
                    _tUpdateSpecByLine.Start();


                    Thread _tConnectorsControl = new Thread(ConnectorsControl);
                    _tConnectorsControl.IsBackground = true;
                    _tConnectorsControl.Start();

                    Thread _tCheckSymatecDisable = new Thread(CheckAntivirusDisable);
                    _tCheckSymatecDisable.IsBackground = true;
                    _tCheckSymatecDisable.Start();


                    Thread _tUpdateMOStation = new Thread(UpdateMOStation);
                    _tUpdateMOStation.IsBackground = true;
                    _tUpdateMOStation.Start();

                    // Update ars system in the end of formload to make sure all data is got
                    Thread update_sta_info = new Thread(update_station_info);
                    update_sta_info.IsBackground = true;
                    update_sta_info.Start();

                    // for load range of time by 1 hour load 1 time
                    //_tShowfPanelHData = new Thread(RunfPanelHData);
                    //_tShowfPanelHData.Name = "_tShowfPanelHData";
                    //_tShowfPanelHData.IsBackground = true;
                    //_tShowfPanelHData.Start();
                    //return;


                    //20170309
                    Thread _tControlRunCtrl = new Thread(ControlRunCtrl);
                    _tControlRunCtrl.IsBackground = true;
                    _tControlRunCtrl.Start();
                    //29.03.2016 Display Sfis protocol and setbound panel of MTP version based panel of SFISprotocol
                    //Thread _tSFISProtocol = new Thread(SFISProtocol);
                    //_tSFISProtocol.IsBackground = true;
                    //_tSFISProtocol.Start();
                }


                try
                {
                    if (File.Exists(@".\frmActive.ini"))
                    {
                        File.SetAttributes(@".\frmActive.ini", FileAttributes.Normal);
                        File.Delete(@".\frmActive.ini");
                    }

                    if (File.Exists(@".\error.txt"))
                    {
                        FileInfo fInfo = new FileInfo(@".\error.txt");
                        if (fInfo.Length / 1024 / 1024 >= 5)
                        {
                            File.SetAttributes(@".\error.txt", FileAttributes.Normal);
                            File.Delete(@".\error.txt");
                        }

                    }
                }
                catch (Exception er)
                {
                }

                //
                if (NetWorkConnection)
                {
                    Thread _tGetUseSpecMode = new Thread(GetUseSpecMode);
                    _tGetUseSpecMode.IsBackground = true;
                    _tGetUseSpecMode.Start();

                    // 2016.06.08  need put in the end because need first get current station & model

                    Thread _tStartApp = new Thread(StartApps);
                    _tStartApp.IsBackground = true;
                    _tStartApp.Start();

                    // 2016.06.11  for check testflag by tpg
                    //Thread _tTPGUpdateTestFlag = new Thread(TPGUpdateTestFlag);
                    //_tTPGUpdateTestFlag.IsBackground = true;
                    //_tTPGUpdateTestFlag.Start();

                    // 2016.07.23 ATE CheckList for MNRP
                    if (ul.GetStation() == "NMRP")
                    {
                        Thread _tInsertMNRPCheckList = new Thread(InsertPCKPI);
                        _tInsertMNRPCheckList.IsBackground = true;
                        _tInsertMNRPCheckList.Start();
                    }
                    //
                    // 2016.07.23 Dont check checklist
                    //Thread _tCheckCheckATE = new Thread(CheckCheckATE);
                    //_tCheckCheckATE.IsBackground = true;
                    //_tCheckCheckATE.Start();

                    // 2016.07.25 Check TPG version is correct or not
                    //Thread _tCheckTPGInfo = new Thread(CheckTPGInfo);
                    // _tCheckTPGInfo.IsBackground = true;
                    //_tCheckTPGInfo.Start();


                    // 2016.10.08 Check Interferece // check if this PC is run interferce server or not
                    Thread _tCheckInterference = new Thread(CheckInterference);
                    _tCheckInterference.IsBackground = true;
                    _tCheckInterference.Start();

                    if (ul.GetStation().Contains("FTS"))
                        this.TopMost = false;

                    //20170527 fixbug 
                    Thread _tUpdateCableStatus = new Thread(CableStatus);
                    _tUpdateCableStatus.IsBackground = true;
                    _tUpdateCableStatus.Start();

                    //GetRepassYeildRate();

                    //Thread _GetFYR = new Thread(GetRepassYeildRate);
                    //_GetFYR.IsBackground = true;
                    //_GetFYR.Start();
                    //Thread _tdoUpdateCableUsingTimes = new Thread(doUpdateCableUsingTimes);
                    //_tdoUpdateCableUsingTimes.IsBackground = true;
                    //_tdoUpdateCableUsingTimes.Start();
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading : " + ex.Message);
            }
            #endregion
        }
        string globalUsedMode = "0";
        public void downLoadDll()
        {
            try
            {
                File.Copy(@"F:\lsy\Test\DownloadConfig\AutoDL\Newtonsoft.Json.dll", ".//Newtonsoft.Json.dll", true);
            }
            catch (Exception)
            {


            }


            if (!File.Exists(".//Newtonsoft.Json.xml"))
            {
                File.Copy(@"F:\lsy\Test\DownloadConfig\AutoDL\Newtonsoft.Json.xml", ".//Newtonsoft.Json.xml", false);
            }
        }
        public void GetUseSpecMode()
        {
            try
            {
                string linename = GetLineOfTester().Trim();
                UseSpecMode = false;
                SqlConnection connection = new SqlConnection(connSrv60);
                connection.Open();
                SqlDataReader reader;
                //string sqlStr = @"select * from tblStopMachineSpec join tblTypeOfProduct on tblStopMachineSpec.Type = tblTypeOfProduct.Type where tblTypeOfProduct.Line='" + GetLineOfTester() + "'";
                string sqlStr = @"select UsedMode from tblTypeOfProduct  where tblTypeOfProduct.Line='" + linename + "'";
                //MessageBox.Show(sqlStr);
                SqlCommand cmd = new SqlCommand(sqlStr, connection);
                SqlDataAdapter da = new SqlDataAdapter(sqlStr, connSrv60);
                DataSet ds = new DataSet();
                da.Fill(ds);
                reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    // 0 for lock tester after 100 pcs
                    // 1 for lock by range of tested pcs by line
                    // 2 for lock teset range of tested pcs by tester
                    if (ds.Tables[0].Rows[0]["UsedMode"].ToString() == "1")
                    {
                        UseSpecMode = true;
                        globalUsedMode = "1";
                    }

                    if (ds.Tables[0].Rows[0]["UsedMode"].ToString() == "2")
                    {
                        UseSpecMode = true;
                        globalUsedMode = "2";
                    }
                }

                //event_log("UseSpecMode: " + UseSpecMode.ToString());
            }
            catch (Exception)
            {
            }
        }

        public void OnClickDisplayPictureBox(object sender, EventArgs e)
        {
            try
            {
                //MessageBox.Show(((PictureBox)sender).Name);
                if (((PictureBox)sender).Name == "pbVirus")
                {
                    toolTipVirus.Show(toolTipVirus.GetToolTip(pbVirus), pbVirus, 2000);//SetToolTip(pbVirus, tooltipContent);
                }
                else if (((PictureBox)sender).Name == "pbUsb")
                {
                    toolTipUSB.Show(toolTipUSB.GetToolTip(pbUsb), pbUsb, 2000);//SetToolTip(pbVirus, tooltipContent);
                }
                else
                {
                    if (!CheckCableStatus)
                        doChangeCable();
                    else
                    {
                        int count = Convert.ToInt32(((PictureBox)sender).Name);
                        tl[count].ToolTipTitle = TopMostUseCableName[count];
                        tl[count].IsBalloon = true;
                        tl[count].Show(tl[count].GetToolTip(pbCable[count]), pbCable[count], 2000);

                    }

                }
            }
            catch (Exception r)
            {
                //MessageBox.Show(r.ToString());
            }
        }

        public void StartApps()
        {
            while (true)
            {
                try
                {
                    //2016.06.08 -> Fix bug cable not the same, if station/ model change -> kill showUI // if run >= 2 model at the same time (arlo ) ==> k kill
                    string tempStation = ul.GetStation().Trim();
                    string tempModel = ul.GetModel().Trim();

                    if (CurrentStation != tempStation || ModelNameChangeATE != tempModel)
                    {
                        string[] temp = list_model_dont_kill_showui.Split(',');
                        bool killShowUI = true;
                        foreach (string iModel in temp)
                        {
                            if (tempModel == iModel)
                            {
                                killShowUI = false;
                                ModelNameChangeATE = tempModel;
                                if (!_tShowfPanelHData.IsAlive)
                                {
                                    ul.SetValueByKey("SN", "");
                                    _tShowfPanelHData = new Thread(RunfPanelHData);
                                    _tShowfPanelHData.Name = "_tShowfPanelHData";
                                    _tShowfPanelHData.IsBackground = true;
                                    _tShowfPanelHData.Start();
                                }
                                break;
                            }
                        }
                        if (killShowUI)
                            Application.Exit();
                    }


                    //if (File.Exists(@"C:\Windows\AShengClient.exe"))
                    //{
                    //    if (Process.GetProcessesByName("AShengClient").Length < 1)
                    //        Process.Start(@"C:\Windows\AShengClient.exe");
                    //}
                }
                catch (Exception)
                {
                }
                Thread.Sleep(5000);
            }

        }

        public void ConnectorsControl()
        {
            while (true)
            {
                try
                {
                    if (CheckCableStatus)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            // for fix bug cable is not the same in a station
                            for (int count = 0; count < 12; count++)
                            {
                                if (pbCable[count].Image != null)
                                {
                                    pbCable[count].Image = null;
                                }

                            }
                            NumOfCable = GetNumOfCable();
                            timerCableStatus.Enabled = true;

                        });
                        UpdateConnectorsUseTime = 1; // reset UpdateConnectorsUseTime status = 1 to used the using time from sever
                        event_log("Connectors Control: Running ...");
                    }//end CheckCableStatus

                    //2016.06.27 for check symantec disable

                }
                catch (Exception)
                {
                }
                //}
                Thread.Sleep(90000); // 12.01.2016 change to 1'30 to not crash server cos too much connection 90000

            }
        }

        string list_model_dont_kill_showui = "";
        public void GlobalValueInitialize()
        {

            try
            {
                CurrentDUT = ul.GetValueByKey("SN").Trim();
                CurrentStation = ul.GetStation().Trim();
                TmpCountErr = ul.GetValueByKey("ERRORCODE").Trim();
                ModelNameChangeATE = ul.GetModel().Trim();
                CheckSum = GetCkSum().Trim();
                NumOfCable = GetNumOfCable();
                _TypeOfManufacturing = GetTypeOfManufacturing();
                Shift = "X";

                string TmpDate = DateTime.Now.ToString("HHmm");
                int ComparedTime = 730;

                ComparedTime = Convert.ToInt32(TmpDate);

                if (ComparedTime >= 730 && ComparedTime <= 1930)
                {
                    Shift = "D";
                }
                else
                {
                    Shift = "N";
                }

                // Get Yrate + RTR from dtbase


                TestTimeHighLimit = GetTestTimeControl("TestTimeHighLimit"); //Convert.ToInt32(ul.GetValueByKey("TestTimeHighLimit"));
                                                                             //MessageBox.Show(Convert.ToString(TestTimeHighLimit));
                TestTimeLowLimit = GetTestTimeControl("TestTimeLowLimit");//Convert.ToInt32(ul.GetValueByKey("TestTimeLowLimit"));


                if (ul.GetValueByKey("StartDUT") == "0" || ul.GetValueByKey("StartDUT") == "*" && NetWorkConnection == true)
                {

                    TestedDUT = sfisB05.GET_STATION_PASS_FAIL(ul.GetModel(), sName);
                }
                else
                {
                    TestedDUT = Convert.ToInt32(ul.GetValueByKey("StartDUT"));
                }
                //  list model dont kill showui as change model name > arlo run more model at the same time
                list_model_dont_kill_showui = IniFile.ReadIniFile("LIST_MODEL_DONT_KILL_SHOWUI", "MODEL_LIST", "sander,patrick", @"F:\lsy\Test\DownloadConfig\Setup.ini");

            }
            catch (Exception)
            {
            }
        }

        public void StopMachineEnable()
        {
            //^^

            try
            {
                if (NetWorkConnection)
                {
                    //string conn = @"Data Source=10.224.81.37;Initial Catalog=dbGeneral;uid=sa;pwd=********;";
                    SqlConnection connection = new SqlConnection(connSrv60);

                    connection.Open();
                    string sqlFake = "select * from iconner where boom=@vr and Line=@vLine";
                    SqlCommand command = new SqlCommand(sqlFake, connection);
                    command.Parameters.Add("@vr", SqlDbType.NChar, 10);
                    command.Parameters["@vr"].Value = '1';
                    command.Parameters.Add("@vLine", SqlDbType.NChar, 10);
                    command.Parameters["@vLine"].Value = GetLineOfTester().Trim();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        //MessageBox.Show("KILL");
                        FKey = true;
                        string tmpStr = lblShowUI.Text.Remove(0, 1);
                        lblShowUI.Text = "V " + tmpStr;
                        event_log("Disable StopMachine: OK");
                    }
                    else
                    {
                        //MessageBox.Show("Fkey false");
                        FKey = false;
                    }
                    connection.Close();
                }
                else
                {
                    FKey = true;
                }

            }
            catch (Exception r)
            {
                string tmpStr = lblShowUI.Text.Remove(0, 1);
                lblShowUI.Text = "V " + tmpStr;
                event_log("Disable StopMachine: " + r.ToString());
                FKey = true;
            }

        }

        //start synd server time
        [DllImport("kernel32.dll", EntryPoint = "SetSystemTime", SetLastError = true)]
        public extern static bool Win32SetSystemTime(ref SystemTime sysTime);


        public void SysnSvTime()
        {
            while (true)
            {
                if (NetWorkConnection)
                {
                    try
                    {

                        int svTimezone = Convert.ToInt32(GetServerTimeZone(svConnSyncDate));
                        DateTime convertedtime = Convert.ToDateTime(GetServerDatetime(svConnSyncDate));
                        SetServerDatetime(convertedtime, svTimezone);
                        event_log("Server time synchronisation: completed & running ...");
                        // end synd server time
                    }
                    catch (Exception r)
                    {
                        event_log("Server time synchronisation: " + r.ToString());
                        //throw;
                    }

                }
                Thread.Sleep(3600000);// sleep for 1hour
            }
        }

        public string GetServerDatetime(string conn)
        {
            SqlConnection connection = new SqlConnection(conn);
            connection.Open();
            string sqlGetDate = "select convert(varchar,getdate(),120) as CurrentDatetime";
            SqlDataAdapter da = new SqlDataAdapter(sqlGetDate, connection);
            DataSet ds = new DataSet();
            da.Fill(ds);
            string SvDatetime = ds.Tables[0].Rows[0]["CurrentDatetime"].ToString();
            connection.Close();
            return SvDatetime;
        }
        public string GetServerTimeZone(string conn)
        {
            SqlConnection connection = new SqlConnection(conn);
            connection.Open();
            string sqlGetDate = "select datediff(hh,getutcdate(),getdate()) as svtimezone";
            SqlDataAdapter da = new SqlDataAdapter(sqlGetDate, connection);
            DataSet ds = new DataSet();
            da.Fill(ds);
            string SvTimeZone = ds.Tables[0].Rows[0]["svtimezone"].ToString();
            connection.Close();
            return SvTimeZone;
        }
        public void SetServerDatetime(DateTime dt, int svTimeZone)
        {
            SystemTime updatedTime = new SystemTime();
            updatedTime.Year = (ushort)dt.Year;
            updatedTime.Month = (ushort)dt.Month;
            updatedTime.Day = (ushort)dt.Day;
            updatedTime.Hour = (ushort)(dt.Hour - svTimeZone);
            updatedTime.Minute = (ushort)dt.Minute;
            updatedTime.Second = (ushort)dt.Second;
            if (!Win32SetSystemTime(ref updatedTime))
            {
                throw new Win32Exception();
            }
        }

        public struct SystemTime
        {
            public ushort Year;
            public ushort Month;
            public ushort DayOfWeek;
            public ushort Day;
            public ushort Hour;
            public ushort Minute;
            public ushort Second;
            public ushort Millisecond;
        }

        //end synd server time
        // end form load
        private string GetMacAddress(string ipAddress)
        {
            string macAddress = string.Empty;
            if (ipAddress.Contains("No IP Prefix"))
            {
                ipAddress = "10.224.81.37";

            }
            try
            {

                System.Diagnostics.ProcessStartInfo processStartInfo = new System.Diagnostics.ProcessStartInfo();
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                processStartInfo.FileName = "nbtstat";
                processStartInfo.RedirectStandardInput = false;
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.Arguments = "-a " + ipAddress;
                processStartInfo.UseShellExecute = false;
                processStartInfo.CreateNoWindow = true;
                process = System.Diagnostics.Process.Start(processStartInfo);

                int Counter = -1;

                while (Counter <= -1)
                {
                    // Look for the words "mac address" in the output.
                    Counter = macAddress.Trim().ToLower().IndexOf("mac address", 0);
                    if (Counter > -1)
                    {
                        break;
                    }
                    macAddress = process.StandardOutput.ReadLine();
                }
                process.WaitForExit();
                macAddress = macAddress.Trim();
            }
            catch (Exception)
            {
                //Do nothing
            }
            return macAddress;
        }

        //Start Sander code

        bool AnitiVirusDisable = false;
        public void CheckAntivirusDisable()
        {

            ///MessageBox.Show(pr);
            //return;
            while (true)
            {
                // Process p = Process.GetProcessesByName("NetgearAutoDL",null);

                try
                {
                    Process[] proc = Process.GetProcessesByName("NetgearAutoDL");
                    string TPG = ul.GetValueByKey("TPG_NAME").Trim();
                    if (proc.Length > 0)
                    {
                        if (!proc[0].MainModule.FileName.Contains(@"D:\AutoDL"))
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                AutoClosingMessageBox.Show("Chương trình NetgearAutoDL không mở từ thư mục D:\\AutoDL.\nHãy kiểm tra lại...\nProgram will be aborted ...", "WarningMessageBox", 7000);

                                ShellExecute("taskkill /IM NetgearAutoDL.exe /F");
                                AutoClosingMessageBox.Show("TPG will be aborted in 5s ...", "WarningMessageBox", 5000);
                                ShellExecute("taskkill /IM " + TPG + " /F");
                                AutoClosingMessageBox.Show("ShowUI will close in 2s ...", "WarningMessageBox", 2000);
                                Application.Exit();
                            });
                        }
                    }
                    // 
                }
                catch (Exception)
                {
                }

                try
                {
                    if (IniFile.ReadIniFile("CheckSymantec", "Disable", "0", "Setting.txt").Contains("0"))
                    {
                        return;
                    }
                    System.ServiceProcess.ServiceController svC = new System.ServiceProcess.ServiceController("Symantec Endpoint Protection");
                    if (svC.Status != System.ServiceProcess.ServiceControllerStatus.Running)
                    {
                        AnitiVirusDisable = true;
                        try
                        {
                            svC.Start();
                            AnitiVirusDisable = false;
                        }
                        catch (Exception)
                        {
                        }
                    }

                }
                catch (Exception)
                {
                    AnitiVirusDisable = true;
                }

                Thread.Sleep(15000); // for 5'
            }

        }

        // Get the updating date
        string localPathSetup = @"F:\lsy\Test\DownloadConfig\AutoDL\Setup.ini";
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
                event_log("Antivirus-->" + ex.ToString());
                return 7;
            }
        }

        //bool flagVirus;
        int NumOfCable;// fixed num of cable = 10

        int[] BlinkFlag;
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
                if (FKey)
                {
                    UsbStatus = true;
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

        bool BeforeDaysNotice = false;
        double ExpiredDayLeft = 0;
        string KeyAnti = "";
        protected bool CheckAntiVirusSoftUpdate()
        {
            if (IniFile.ReadIniFile("CheckSymantec", "Update", "0", localPathSetup).Contains("0"))
            {
                return true;
            }
            bool CheckAnti = false;
            try
            {
                int NumOfDay = VirusOutOfDateSpec();
                string KeyAnti = @"SOFTWARE\Symantec\SharedDefs\";
                string KeyAntiUpdate = @"SOFTWARE\Symantec\Symantec Endpoint Protection\CurrentVersion\SharedDefs\SDSDefs\";

                //path=C:\Program Files\Common Files\Symantec Shared\VirusDefs
                //path=C:\Program Files\Common Files\Symantec Shared\VirusDefs
                //KeyAnti = IniFile.ReadIniFile("PathAnti", "path" , "", ".\\Setup.ini");

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
                    //flagVirus = false;
                    if (FKey)
                    {
                        CheckAnti = true;
                    }
                }
                //fake check anti = false
                if (FKey)
                {
                    CheckAnti = true;
                }

                return CheckAnti;
            }
            catch (Exception ex)
            {
                event_log("Antivirus-->" + ex.ToString());
                return CheckAnti;

            }
        }
        protected bool CheckAntiVirusSoftUpdateWin10()
        {
            if (IniFile.ReadIniFile("CheckSymantec", "Update", "0", localPathSetup).Contains("0"))
            {
                return true;
            }
            bool CheckAntiUpdate = false;
            try
            {
                int NumOfDay = VirusOutOfDateSpec();
                //string KeyAnti = @"SOFTWARE\Symantec\SharedDefs\";
                string KeyAntiUpdate = @"SOFTWARE\Symantec\Symantec Endpoint Protection\CurrentVersion\SharedDefs\SDSDefs\";

                //path=C:\Program Files\Common Files\Symantec Shared\VirusDefs
                //path=C:\Program Files\Common Files\Symantec Shared\VirusDefs
                //KeyAnti = IniFile.ReadIniFile("PathAnti", "path" , "", ".\\Setup.ini");

                RegistryKey key = Registry.LocalMachine.OpenSubKey(KeyAntiUpdate, true);

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

                            CheckAntiUpdate = true;

                        }
                    }
                }
                else
                {
                    //flagVirus = false;
                    if (FKey)
                    {
                        CheckAntiUpdate = true;
                    }
                }
                //fake check anti = false
                if (FKey)
                {
                    CheckAntiUpdate = true;
                }

                return CheckAntiUpdate;
            }
            catch (Exception ex)
            {
                event_log("Antivirus Update Win10: -->" + ex.ToString());
                return CheckAntiUpdate;

            }
        }
        string tooltipContentVR;
        string tooltipContentUSB;
        protected void AntiVirusInfo()
        {
            try
            {
                System.ServiceProcess.ServiceController svC = System.ServiceProcess.ServiceController.GetServices()
                                                                                                    .FirstOrDefault(s => s.DisplayName == "Symantec Endpoint Protection");
                if (svC == null)
                {
                    toolTipVirus.ToolTipTitle = "AntiVirus havent install";
                    toolTipVirus.ToolTipIcon = ToolTipIcon.Warning;
                    tooltipContentVR = "Please, Install AntiVirus Software!";
                    pbVirus.Image = ShowUI.Properties.Resources.security_off;
                    return;
                }
                else
                {
                    pbVirus.Image = ShowUI.Properties.Resources.security_on;
                }

            }
            catch (Exception)
            {


            }
            try
            {
                // Check update anivirus software
                //this.Invoke(new MethodInvoker(delegate()
                //    {
                bool viruscondition = CheckAntiVirusSoftUpdate();
                bool virusWin10 = CheckAntiVirusSoftUpdateWin10();
                bool usbCondition = CheckUSBDisable();
                tooltipContentVR = "";
                tooltipContentUSB = "";
                //event_log("Antivius Checking: Check software antivirus completed with status:" + viruscondition.ToString());
                if (viruscondition == false & virusWin10 == false)
                {
                    // Invoke your control like this

                    timerCheckUdateAntiVirusSoft.Interval = 379;
                    timerCheckUdateAntiVirusSoft.Enabled = true;
                    //timerCheckUdateAntiVirusSoft.Start();

                    toolTipVirus.ToolTipTitle = "AntiVirus Is Out of Date!";
                    toolTipVirus.ToolTipIcon = ToolTipIcon.Warning;
                    tooltipContentVR = "Please,Update Anti Virus Software!";
                    pbVirus.Image = ShowUI.Properties.Resources.security_off;
                    //toolTipVirus.SetToolTip(pbVirus, tooltipContent);
                }
                else
                {
                    if (BeforeDaysNotice == true)
                    {
                        timerCheckUdateAntiVirusSoft.Interval = 379;
                        timerCheckUdateAntiVirusSoft.Enabled = true;
                        //timerCheckUdateAntiVirusSoft.Start();
                        if (ExpiredDayLeft == 0)
                        {
                            int RemainingHour = 24 - DateTime.Now.Hour;
                            toolTipVirus.ToolTipTitle = "AntiVirus Is Out of Date in " + ExpiredDayLeft + " Hours!";
                        }
                        else
                        {
                            toolTipVirus.ToolTipTitle = "AntiVirus Is Out of Date in " + ExpiredDayLeft + " Hours!";
                        }

                        toolTipVirus.ToolTipIcon = ToolTipIcon.Warning;
                        tooltipContentVR = "Please,Update Anti Virus Software ...";
                        pbVirus.Image = ShowUI.Properties.Resources.security_offyellow;
                        //toolTipVirus.SetToolTip(pbVirus, tooltipContent);
                    }
                    else
                    {
                        pbVirus.Image = ShowUI.Properties.Resources.security_on;
                        toolTipVirus.ToolTipTitle = "AntiVirus Is Updated!";
                        toolTipVirus.ToolTipIcon = ToolTipIcon.Info;
                        tooltipContentVR = "Everything Is OK! Have A Good Day!";
                        //toolTipVirus.SetToolTip(pbVirus, tooltipContent);
                    }
                }
                //event_log("USB Checking: Check software antivirus completed with status:" + usbCondition.ToString());
                if (usbCondition == false)
                {
                    pbUsb.Image = ShowUI.Properties.Resources.usb_red;
                    toolTipUSB.ToolTipTitle = "USB Status";
                    toolTipUSB.ToolTipIcon = ToolTipIcon.Warning;
                    tooltipContentUSB = "Enable ...";
                    //toolTipUSB.SetToolTip(pbUsb, tooltipContent);
                }
                else
                {
                    pbUsb.Image = ShowUI.Properties.Resources.usb_green;
                    toolTipUSB.ToolTipTitle = "USB Status";
                    toolTipUSB.ToolTipIcon = ToolTipIcon.Info;
                    tooltipContentUSB = "Disable ...";
                    //toolTipUSB.SetToolTip(pbUsb, tooltipContent);
                }
                //  }));
            }
            catch (Exception)
            {
            }
            ///
        }

        protected string GetCableCtrlTimesPath()
        {

            string Station = ul.GetStation();
            string Product = ul.GetProduct();
            string sfisModel = ul.GetModel().Trim();
            Station = Station.ToUpper();
            string tmpPathDownloadConfig = @"F:\lsy\Test\DownloadConfig\" + Product + ".ini";
            string tmpPathProduct = "";
            string CtrlCableUsetimesPath = "";
            //_OpenKey = Registry.LocalMachine.OpenSubKey(SubKey);

            try
            {
                if (File.Exists(tmpPathDownloadConfig))
                {
                    string SectionModel = IniFile.ReadIniFile("ModelSection", sfisModel, null, tmpPathDownloadConfig);

                    tmpPathProduct = IniFile.ReadIniFile(SectionModel, "TPG_FOLDER_" + Station, null, tmpPathDownloadConfig);

                    CtrlCableUsetimesPath = tmpPathProduct + "CableCtrlTime.ini";

                    if (File.Exists(CtrlCableUsetimesPath))

                        CtrlCableUsetimesPath = tmpPathProduct + "CableCtrlTime.ini";
                    else
                        event_log("GetCableCtrlTimesPath: Cant get content of CableCtrlTime.ini file: SectionModel = " + SectionModel + " Station = " + Station + " > " + CtrlCableUsetimesPath);
                }
                else
                {
                    event_log("GetCableCtrlTimesPath: Not Exist CableCtrlTime.ini file in server > " + tmpPathDownloadConfig);
                }

            }
            catch (Exception e)
            {
                // if doesn exist 
                event_log("GetCableCtrlTimesPath Exception: " + e.ToString());
                //return @"\\10.224.81.37\wireless\Temp\TE-PROGRAM\TE-PRO1\Sander\PT\CableCtrlTime.ini";
            }
            return CtrlCableUsetimesPath;
        }

        protected int GetNumOfCable()
        {
            int NumCable = 1;
            string CtrTimePath = GetCableCtrlTimesPath();
            int bufferCable;
            if (File.Exists(CtrTimePath))
            {
                string cbn = IniFile.ReadIniFile("ConnectControl", "TotalCable ", "f", CtrTimePath);
                //MessageBox.Show(cbn + "<<");
                try
                {
                    Int32.TryParse(cbn, out NumCable);
                    bufferCable = NumCable;
                }
                catch (Exception)
                {

                    event_log("GetNumOfCable Exception:  " + CtrTimePath + " has wrong config");
                }

            }
            else
            {
                // fake cable = 1
                NumCable = 1;

                event_log("GetNumOfCable :  " + CtrTimePath + " not exist");
            }

            return NumCable;

        }

        protected string GetProductPath()
        {
            string Product = ul.GetProduct();
            string Stattion = ul.GetStation().ToUpper();
            string ProductPath = "";
            string OpenSubKey = @"SYSTEM\CurrentControlSet\services";
            RegistryKey keys = null;
            string KeyPlace = OpenSubKey + @"\" + Product + @"\" + Stattion;

            try
            {

                keys = Registry.LocalMachine.OpenSubKey(KeyPlace, true);
                if (keys == null)
                {
                    keys = Registry.LocalMachine.OpenSubKey(OpenSubKey + @"\" + Product, true);
                    if (keys == null)
                    {

                        keys = Registry.LocalMachine.OpenSubKey(OpenSubKey, true);
                        keys.CreateSubKey(Product);
                        keys = Registry.LocalMachine.OpenSubKey(OpenSubKey + @"\" + Product, true);
                        keys.CreateSubKey(Stattion);
                        keys = Registry.LocalMachine.OpenSubKey(KeyPlace, true);
                        for (int i = 0; i < NumOfCable; i++)
                        {
                            keys.SetValue("Cable" + i, "0");
                        }
                    }
                    else
                    {
                        keys = Registry.LocalMachine.OpenSubKey(OpenSubKey + @"\" + Product, true);
                        keys.CreateSubKey(Stattion);
                        keys = Registry.LocalMachine.OpenSubKey(KeyPlace, true);
                        //newKeyCreate = keys.CreateSubKey(Station);
                        for (int i = 0; i < NumOfCable; i++)
                        {
                            keys.SetValue("Cable" + i, "0");
                        }

                    }

                }
                else
                {
                    // keys = Registry.LocalMachine.OpenSubKey(KeyPlace, true);
                    for (int i = 0; i < NumOfCable; i++)
                    {
                        int tmpVal = GetConnectorUsingTimes("Cable" + i);
                        keys.SetValue("Cable" + i, tmpVal.ToString());
                    }
                }
                ProductPath = KeyPlace;
                //MessageBox.Show(ProductPath);
                return ProductPath;
            }
            catch (Exception r)
            {
                return OpenSubKey + ProductName;
            }

        }

        bool CheckCableStatus = true;
        string[] TopMostUseCableName;
        string[] TopMostUseCableSpec;
        double[] TopMostUseCableNameUseTimes;
        string[] MainInfo = new string[6];
        int UpdateConnectorsUseTime = 1; // 1 for ok, 0 for not ok
        ShowUI.ATE_CHECKLIST.WebService SvConnectorsUseTimes = new ShowUI.ATE_CHECKLIST.WebService();
        int tenp = 0;
        protected void CableStatus()
        {
            isUpdateConnectorUsingTimesDone = true;
            event_log("CableStatus Start OK -> CheckCableStatus " + CheckCableStatus.ToString());
            while (true)
            {
                try
                {
                    if (isUpdateConnectorUsingTimesDone)
                    {
                        isUpdateConnectorUsingTimesDone = false;
                        if (CheckCableStatus)
                        {
                            //isUpdateConnectorUsingTimesDone = false;
                            //int NumOfCable = GetNumOfCable();
                            string CtrlCableUsetimesPath = GetCableCtrlTimesPath();
                            string ProductPath = GetProductPath();
                            MainInfo[0] = GetLineOfTester().Trim();
                            MainInfo[1] = ul.GetStation().Trim();
                            MainInfo[2] = sName;
                            MainInfo[3] = client_ip;
                            MainInfo[4] = ul.GetModel().Trim();
                            MainInfo[5] = ul.GetProduct().Trim();
                            _OpenKey = Registry.LocalMachine.OpenSubKey(subkey);

                            timerBlinkCable.Enabled = false;

                            // Khoi tao mang picbox va tooltip
                            ///Control fControl = null;

                            if (_OpenKey != null)
                            {
                                //if (CtrlCableUsetimesPath != "")
                                //{
                                if (ProductPath != "")
                                {
                                    TopMostUseCableName = new string[NumOfCable];
                                    TopMostUseCableSpec = new string[NumOfCable];
                                    TopMostUseCableNameUseTimes = new double[NumOfCable];
                                    //double[] TopPercentageUseTimes = new double[NumOfCable];
                                    RegistryKey keys = Registry.LocalMachine.OpenSubKey(ProductPath, true);
                                    int tmpNumOfCable = 0;
                                    for (int i = 0; i < NumOfCable; i++)
                                    {
                                        string cabble = "Cable" + i;
                                        string UseTimes = keys.GetValue(cabble, "").ToString();

                                        if (UseTimes != "")
                                        {
                                            string[] FakeCableName = new string[12] {
                                                "USB_Cable",
                                                "RF1_Cable",
                                                "RF3_Cable",
                                                "LAN1_Cable",
                                                "Power_Cable",
                                                "RF2_Cable",
                                                "LAN2_Cable",
                                                "USB1_Cable",
                                                "Ethernet1_Cable",
                                                "RF4_Cable",
                                                "Ethernet2_Cable",
                                                "Coxial_Cable"
                                            };
                                            string CableName = IniFile.ReadIniFile("CableName", "Cable_" + tmpNumOfCable, FakeCableName[0], CtrlCableUsetimesPath);
                                            if (NumOfCable >= FakeCableName.Length)
                                            {
                                                CableName = "ConnectorTimes (" + (i + 1) + ")";
                                            }
                                            else
                                            {
                                                CableName = IniFile.ReadIniFile("CableName", "Cable_" + tmpNumOfCable, FakeCableName[i], CtrlCableUsetimesPath);
                                            }
                                            //MessageBox.Show(CableName);
                                            TopMostUseCableName[tmpNumOfCable] = CableName;
                                            string Spec = IniFile.ReadIniFile("MaxTimes", "Cable_" + tmpNumOfCable, "5000", CtrlCableUsetimesPath);
                                            TopMostUseCableSpec[tmpNumOfCable] = Spec;
                                            TopMostUseCableNameUseTimes[tmpNumOfCable] = Convert.ToDouble(UseTimes);
                                            if (TopMostUseCableNameUseTimes[tmpNumOfCable] > 500)
                                            {
                                                try
                                                {
                                                    keys.SetValue(cabble, "1");
                                                    TopMostUseCableNameUseTimes[tmpNumOfCable] = 1;
                                                }
                                                catch
                                                {


                                                }

                                            }
                                            //TopPercentageUseTimes[tmpNumOfCable] = Convert.ToDouble(TopMostUseCableNameUseTimes[tmpNumOfCable]) / Convert.ToDouble(Spec);
                                            tmpNumOfCable++;
                                        }
                                    }

                                    try
                                    {
                                        // 3.11.2015 Change to Check Connectors Using Times Via DB, not from Registry

                                        if (UpdateConnectorsUseTime == 1 && ConnectServer63 == "0" && NetWorkConnection == true) // for for case database is freeze then use registry values
                                        {
                                            //MainInfo[0]="L1";
                                            double[] NewUseTimes = SvConnectorsUseTimes.ConnectorsUseTimes(MainInfo, TopMostUseCableName, TopMostUseCableNameUseTimes, TopMostUseCableSpec, 0, "", "", "");
                                            ///* debug cod
                                            // * 
                                            //double[] NewUseTimes = new double[1] { 4900 };

                                            //if (tenp!=0)
                                            //{
                                            //    NewUseTimes = new double[1] { 0 };
                                            //}
                                            //tenp++;

                                            if (NewUseTimes.Length != 0)
                                            {
                                                TopMostUseCableNameUseTimes = NewUseTimes;
                                                //MessageBox.Show(BoolUpdate + "---" + NewUseTimes.Length.ToString());
                                                if (BoolUpdateConnectorUsingTime == "1")
                                                {
                                                    for (int i = 0; i < TopMostUseCableNameUseTimes.Length; i++)
                                                    {
                                                        //MessageBox.Show(BoolUpdate);
                                                        keys.SetValue("Cable" + i, TopMostUseCableNameUseTimes[i].ToString());
                                                    }

                                                }
                                            }
                                        }
                                    }
                                    catch (Exception r)
                                    {
                                        event_log("ConnectorsUseTimesUpdateInsert: " + r.ToString());
                                    }
                                    //

                                    BlinkFlag = new int[tmpNumOfCable];
                                    NumOfCable = tmpNumOfCable;


                                    this.Invoke((MethodInvoker)delegate
                                    {

                                        for (int count = 0; count < NumOfCable; count++)
                                        {
                                            int Spec = Convert.ToInt32(TopMostUseCableSpec[count]);
                                            int UnderSpec = 85 * (Spec / 100);
                                            int UpperSpec = 95 * (Spec / 100);
                                            int UseTimes = Convert.ToInt32(TopMostUseCableNameUseTimes[count]);

                                            if (UnderSpec <= UseTimes && UseTimes <= UpperSpec)
                                            {
                                                tl[count].ToolTipIcon = ToolTipIcon.Warning;
                                                pbCable[count].Image = ShowUI.Properties.Resources.yellow;
                                                BlinkFlag[count] = 888;
                                            }

                                            else if (UseTimes < UnderSpec)
                                            {
                                                tl[count].ToolTipIcon = ToolTipIcon.Info;
                                                pbCable[count].Image = ShowUI.Properties.Resources.green;
                                                BlinkFlag[count] = 888;
                                            }

                                            else if (UseTimes > UpperSpec)
                                            {
                                                tl[count].ToolTipIcon = ToolTipIcon.Warning;
                                                pbCable[count].Image = ShowUI.Properties.Resources.red;
                                                BlinkFlag[count] = count;
                                                timerBlinkCable.Enabled = true;
                                                CheckCableStatus = false;
                                                //MessageBox.Show(pbCable[count], UseTimes + "/" + Spec + "");

                                            }
                                            tl[count].ToolTipTitle = TopMostUseCableName[count];
                                            tl[count].SetToolTip(pbCable[count], UseTimes + "/" + Spec + "");

                                        }
                                    });
                                }
                                else
                                {
                                    this.Invoke((MethodInvoker)delegate
                                    {

                                        //else CableCtrl.ini = null
                                        //NumOfCable = GetNumOfCable();
                                        BlinkFlag = new int[NumOfCable];
                                        for (int i = 0; i < NumOfCable; i++)
                                        {
                                            BlinkFlag[i] = 888;
                                        }
                                        timerCableStatus.Enabled = false;
                                        timerBlinkCable.Enabled = false;
                                        // MessageBox.Show("Don't exist file CableCtrlTimes.ini! Không tồn tại file CableCtrlTimes.ini");
                                    });
                                }

                            }
                            else
                            {
                                this.Invoke((MethodInvoker)delegate
                                {
                                    //else _OpenKey/ Station = null
                                    //NumOfCable = GetNumOfCable();
                                    BlinkFlag = new int[NumOfCable];
                                    for (int i = 0; i < NumOfCable; i++)
                                    {
                                        BlinkFlag[i] = 888;
                                    }

                                    timerCableStatus.Enabled = false;
                                    timerBlinkCable.Enabled = false;
                                    // MessageBox.Show("Don't exist station name! Không tồn tại tên trạm testing!");
                                });
                            }
                            //event_log("CableStatus update OK -> CheckCableStatus " + CheckCableStatus.ToString());
                            isUpdateConnectorUsingTimesDone = true;
                        }// end if CheckCableStatus == true
                        else
                        {
                            event_log("CableStatus Update Break OK");
                            isUpdateConnectorUsingTimesDone = true;
                            break;
                        }
                    }
                }
                catch (Exception r)
                {
                    event_log("CableStatus Update Exception -> CheckCableStatus " + CheckCableStatus.ToString());
                }

                Thread.Sleep(2000);
            }//end while

        }
        public bool checkModelName()
        {
            try
            {
                string KeyModel = @"SOFTWARE\Netgear\AutoDL";
                RegistryKey rSN = Registry.LocalMachine.OpenSubKey(KeyModel, true);
                string aModel;
                aModel = rSN.GetValue("MODELNAME", "").ToString().Trim();
                string rModel;
                rModel = ul.GetValueByKey("SFISMODEL").ToString().Trim();
                if (aModel != rModel)
                {
                    iModel = false;
                }
            }
            catch (Exception)
            {
                iModel = true;
            }
            return iModel;
        }
        public void doUpdateCableUsingTimes()
        {
            while (true)
            {
                //if (isUpdateConnectorUsingTimesDone)
                //{
                //    Thread _tUpdateCableStatus = new Thread(CableStatus);
                //    _tUpdateCableStatus.IsBackground = true;
                //    _tUpdateCableStatus.Start();
                //}
                //Thread.Sleep(30000);
            }
        }


        bool isUpdateConnectorUsingTimesDone = true;
        private void timerCableStatus_Tick(object sender, EventArgs e)
        {
            timerCableStatus.Enabled = false;

            //flowPanel.Controls.Clear();
            try
            {
                AntiVirusInfo();
                // CableStatus();

            }
            catch (Exception)
            {
            }

            timerCableStatus.Enabled = true;
        }
        private void timerCheckUdateAntiVirusSoft_Tick(object sender, EventArgs e)
        {
            try
            {
                bool localCheck = CheckAntiVirusSoftUpdate();
                bool localCheckUpdate = CheckAntiVirusSoftUpdateWin10();
                if (localCheck == false && localCheckUpdate == false)
                {
                    //MessageBox.Show(Convert.ToString(countToDisplayColor));
                    if (DateTime.Now.Second % 2 == 0)
                    {
                        pbVirus.Image = ShowUI.Properties.Resources.security_offyellow;
                    }
                    else
                    {
                        pbVirus.Image = ShowUI.Properties.Resources.security_off;
                    }
                }
                else if (BeforeDaysNotice == true)
                {
                    pbVirus.Image = ShowUI.Properties.Resources.security_offyellow;
                }
                else
                {
                    pbVirus.Image = ShowUI.Properties.Resources.security_on;
                }
            }
            catch (Exception)
            {


            }



        }

        private void timerBlinkCable_Tick(object sender, EventArgs e)
        {

            timerBlinkCable.Interval = 246;

            try
            {
                for (int i = 0; i < BlinkFlag.Length; i++)
                {
                    if (BlinkFlag[i] != 888)
                    {
                        //MessageBox.Show("Nho");
                        if (DateTime.Now.Second % 2 == 0)
                        {
                            pbCable[BlinkFlag[i]].Image = ShowUI.Properties.Resources.grey;
                        }
                        else
                        {
                            pbCable[BlinkFlag[i]].Image = ShowUI.Properties.Resources.red;
                        }
                    }
                }
            }
            catch (Exception)
            {
                //MessageBox.Show(exx.Message.ToString());
            }
        }
        private void PicScanBarCode_Click(object sender, EventArgs e)
        {
        }
        private void CableChange_Click(object sender, EventArgs e)
        {
            doChangeCable();
        }

        public void doChangeCable()
        {
            if (!NetWorkConnection)
            {
                MessageBox.Show("Please check network connection.");
                return;
            }
            string ChangeDtime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            string Product = ul.GetProduct();
            string Station = ul.GetStation();
            string CtrlCableUsetimesPath = GetCableCtrlTimesPath();
            string ProductPath = GetProductPath();

            List<string> _PassListUpdateTopMostUseCableName = new List<string>();
            if (Station != "")
            {
                if (CtrlCableUsetimesPath != "")
                {
                    if (ProductPath != "")
                    {

                        //NumOfCable = GetNumOfCable(); ;

                        int tmpNumOfCable = 0;
                        RegistryKey keys = Registry.LocalMachine.OpenSubKey(ProductPath, true);

                        for (int i = 0; i < NumOfCable; i++)
                        {

                            string cable = "Cable" + i;
                            string UseTimes = keys.GetValue(cable, "").ToString();
                            if (UseTimes != "")
                            {
                                string CableName = IniFile.ReadIniFile("CableName", "Cable_" + tmpNumOfCable, "ConnectorTimes (" + (i + 1) + ")", CtrlCableUsetimesPath);
                                TopMostUseCableName[tmpNumOfCable] = CableName;
                                _PassListUpdateTopMostUseCableName.Add(CableName);
                                tmpNumOfCable++;
                            }

                        }
                    }
                }
            }

            using (ShowUI.frmScanID ScanID = new ShowUI.frmScanID(_PassListUpdateTopMostUseCableName))
            {
                if (ScanID.ShowDialog() == DialogResult.OK)
                {
                    string EmpID = ScanID.getUsername().Trim();
                    string Pw = ScanID.getPassword().Trim();
                    int ChangeType = ScanID.getTypeChange();
                    string ChangeCableName = ScanID.getCable().Trim();
                    string ChangeReason = ScanID.getReason().Trim();
                    string labelcale = ScanID.getlable();

                    if (EmpID == "" || Pw == "")
                    {
                        ShowUIApp.AutoClosingMessageBox.Show("(*) Em.ID/ Password is empty! Try again! Mã NV/ Mật không để trống! Thử lại!", "AutoClosingBox", 4000);
                    }
                    else
                    {
                        if (labelcale == "")
                        {
                            ShowUIApp.AutoClosingMessageBox.Show("(*) Cable.ID is empty! Try again! Mã cable không để trống! Thử lại!", "AutoClosingBox", 4000);
                        }
                        else
                        {
                            //string svIp = ul.GetServerIP("SSO", "10.224.81.37");
                            //MessageBox.Show(svIp);
                            tmpIps += " SSO:10.224.81.62";
                            string connectionStringSSO = @"Data Source=10.224.81.62;Initial Catalog=SSO;uid=sa;pwd=********;Connection Timeout=5";

                            SqlConnection conn = new SqlConnection(connectionStringSSO);

                            try
                            {
                                conn.Open();
                                SqlDataReader reader;
                                string sqlStr = @"select username, password, dep from Users where username='" + EmpID + "' and password = '" + Pw + "'";
                                SqlCommand cmd = new SqlCommand(sqlStr, conn);
                                reader = cmd.ExecuteReader();
                                if (reader.HasRows)
                                {
                                    //SqlConnection ConnnectToCableCtrlInfo = new SqlConnection(connToCableCtrlInfo);
                                    try
                                    {
                                        //ConnnectToCableCtrlInfo.Open();
                                        if (Station != "")
                                        {
                                            if (CtrlCableUsetimesPath != "")
                                            {
                                                if (ProductPath != "")
                                                {

                                                    //NumOfCable = GetNumOfCable(); ;

                                                    string[] TopMostUseCableName = new string[NumOfCable];
                                                    string[] TopMostUseCableNameAbb = new string[NumOfCable];
                                                    string[] TopMostUseCableSpec = new string[NumOfCable];
                                                    double[] TopMostUseCableNameUseTimes = new double[NumOfCable];

                                                    List<string> _ListUpdateTopMostUseCableName = new List<string>();
                                                    List<string> _ListUpdateTopMostUseCableSpec = new List<string>();
                                                    List<string> _ListUpdateTopMostUseCableNameUseTimes = new List<string>();
                                                    int tmpNumOfCable = 0;
                                                    RegistryKey keys = Registry.LocalMachine.OpenSubKey(ProductPath, true);

                                                    for (int i = 0; i < NumOfCable; i++)
                                                    {

                                                        string cable = "Cable" + i;
                                                        string UseTimes = keys.GetValue(cable, "").ToString();
                                                        if (UseTimes != "")
                                                        {
                                                            string CableName = IniFile.ReadIniFile("CableName", "Cable_" + tmpNumOfCable, "ConnectorTimes (" + (i + 1) + ")", CtrlCableUsetimesPath);
                                                            TopMostUseCableName[tmpNumOfCable] = CableName;
                                                            string Spec = IniFile.ReadIniFile("MaxTimes", "Cable_" + tmpNumOfCable, "5000", CtrlCableUsetimesPath);
                                                            TopMostUseCableSpec[tmpNumOfCable] = Spec;
                                                            TopMostUseCableNameUseTimes[tmpNumOfCable] = Convert.ToDouble(UseTimes);
                                                            TopMostUseCableNameAbb[tmpNumOfCable] = cable;

                                                            int _Spec = Convert.ToInt32(Spec);
                                                            int _UpperSpec = 95 * (_Spec / 100);
                                                            int _UseTimes = Convert.ToInt32(UseTimes);

                                                            if (ChangeType == 1)
                                                            {
                                                                _ListUpdateTopMostUseCableName.Add(CableName);
                                                                _ListUpdateTopMostUseCableSpec.Add(Spec);
                                                                _ListUpdateTopMostUseCableNameUseTimes.Add(UseTimes);
                                                            }
                                                            else
                                                            {
                                                                if (_UseTimes > _UpperSpec && ChangeType == 0)
                                                                {
                                                                    //MessageBox.Show(CableName);
                                                                    _ListUpdateTopMostUseCableName.Add(CableName);
                                                                    _ListUpdateTopMostUseCableSpec.Add(Spec);
                                                                    _ListUpdateTopMostUseCableNameUseTimes.Add(UseTimes);
                                                                }
                                                                else
                                                                {
                                                                    _ListUpdateTopMostUseCableName.Add("SanderPatrick");
                                                                    _ListUpdateTopMostUseCableSpec.Add(Spec);
                                                                    _ListUpdateTopMostUseCableNameUseTimes.Add(UseTimes);
                                                                }
                                                                //ul.event_log(CableName);
                                                            }

                                                            tmpNumOfCable++;
                                                        }

                                                    }
                                                    string regCableName = "";
                                                    string[] updateTopMostUseCableName;
                                                    string[] updateTopMostUseCableSpec;
                                                    double[] updateTopMostUseCableNameUseTimes;
                                                    //0 for change by overspec
                                                    if (ChangeType == 0)
                                                    {
                                                        try
                                                        {
                                                            //MessageBox.Show(ChangeType.ToString());
                                                            updateTopMostUseCableName = new string[_ListUpdateTopMostUseCableName.Count];
                                                            updateTopMostUseCableSpec = new string[_ListUpdateTopMostUseCableName.Count];
                                                            updateTopMostUseCableNameUseTimes = new double[_ListUpdateTopMostUseCableName.Count];

                                                            for (int i = 0; i < _ListUpdateTopMostUseCableName.Count; i++)
                                                            {

                                                                if (_ListUpdateTopMostUseCableName[i] != "SanderPatrick")
                                                                {
                                                                    regCableName = "Cable" + i;
                                                                    keys.SetValue(regCableName, "0");
                                                                    //MessageBox.Show(_ListUpdateTopMostUseCableName[i] + "--" + regCableName);
                                                                    updateTopMostUseCableName[i] = _ListUpdateTopMostUseCableName[i].ToString();
                                                                    updateTopMostUseCableSpec[i] = _ListUpdateTopMostUseCableSpec[i].ToString();
                                                                    updateTopMostUseCableNameUseTimes[i] = Convert.ToDouble(_ListUpdateTopMostUseCableNameUseTimes[i].ToString());

                                                                    //for timer start to update status
                                                                    CheckCableStatus = true;
                                                                }

                                                            }
                                                            //timerCableStatus.Enabled = true;
                                                            // 3.11.2015 Change to Check Connectors Using Times Via DB, not from Registry

                                                            ShowUI.ATE_CHECKLIST.WebService SvConnectorsUseTimes = new ShowUI.ATE_CHECKLIST.WebService();
                                                            //double[] NewUseTimes = SvConnectorsUseTimes.ConnectorsUseTimes(MainInfo, updateTopMostUseCableName, updateTopMostUseCableNameUseTimes, updateTopMostUseCableSpec, 1, EmpID, "OVER SPEC", "Connectors Using Times Out Of Spec.");
                                                            double[] NewUseTimes = SvConnectorsUseTimes.ConnectorsUseTimes_New(MainInfo, updateTopMostUseCableName, updateTopMostUseCableNameUseTimes, updateTopMostUseCableSpec, 1, EmpID, "OVER SPEC", "Connectors Using Times Out Of Spec.", labelcale);

                                                        }
                                                        catch (Exception r)
                                                        {
                                                            UpdateConnectorsUseTime = 0;
                                                            event_log("ConnectorsUseTimesChangeByOverSpec" + r.ToString());
                                                            //MessageBox.Show("dfdfd"+r.ToString());
                                                        }
                                                    }

                                                    //1 for change by single cable
                                                    if (ChangeType == 1)
                                                    {
                                                        try
                                                        {
                                                            updateTopMostUseCableName = new string[1];
                                                            updateTopMostUseCableSpec = new string[1];
                                                            updateTopMostUseCableNameUseTimes = new double[1];
                                                            updateTopMostUseCableName[0] = ChangeCableName;
                                                            //MessageBox.Show(_ListUpdateTopMostUseCableName.Count.ToString());
                                                            for (int i = 0; i < _ListUpdateTopMostUseCableName.Count; i++)
                                                            {
                                                                //MessageBox.Show(_ListUpdateTopMostUseCableName[i]);
                                                                if (_ListUpdateTopMostUseCableName[i].ToString() == ChangeCableName)
                                                                {

                                                                    regCableName = "Cable" + i;
                                                                    keys.SetValue(regCableName, "0");
                                                                    updateTopMostUseCableSpec[0] = _ListUpdateTopMostUseCableSpec[i].ToString();
                                                                    updateTopMostUseCableNameUseTimes[0] = Convert.ToDouble(_ListUpdateTopMostUseCableNameUseTimes[i].ToString());
                                                                    CheckCableStatus = true;
                                                                }

                                                            }

                                                            //timerCableStatus.Enabled = true;

                                                            ShowUI.ATE_CHECKLIST.WebService SvConnectorsUseTimes = new ShowUI.ATE_CHECKLIST.WebService();
                                                            //double[] NewUseTimes = SvConnectorsUseTimes.ConnectorsUseTimes(MainInfo, updateTopMostUseCableName, updateTopMostUseCableNameUseTimes, updateTopMostUseCableSpec, 1, EmpID, "BROKEN", ChangeReason);
                                                            double[] NewUseTimes = SvConnectorsUseTimes.ConnectorsUseTimes_New(MainInfo, updateTopMostUseCableName, updateTopMostUseCableNameUseTimes, updateTopMostUseCableSpec, 1, EmpID, "BROKEN", ChangeReason, labelcale);
                                                        }
                                                        catch (Exception r)
                                                        {
                                                            UpdateConnectorsUseTime = 0;
                                                            event_log("ConnectorsUseTimesChangeBySingleCable" + r.ToString());
                                                            //MessageBox.Show(r.ToString());
                                                        }

                                                    }// end if changetype = 1


                                                }
                                            }

                                        }
                                        //ConnnectToCableCtrlInfo.Close();
                                        conn.Close();
                                        if (CheckCableStatus)
                                        {
                                            MessageBox.Show("Update successfully! Thanks " + EmpID + "! Cập nhật thành công! Cảm ơn " + EmpID + "! Chúc bạn ngày làm việc hiệu quả!");

                                            Thread _tUpdateCableStatus = new Thread(CableStatus);
                                            _tUpdateCableStatus.IsBackground = true;
                                            _tUpdateCableStatus.Start();
                                        }
                                        else
                                        {
                                            ul.event_log("CheckCableStatus: " + CheckCableStatus.ToString());
                                        }

                                        //timerCableStatus.Enabled = true;
                                        //this.Close();
                                    }
                                    catch (Exception r)
                                    {
                                        ul.event_log("ChangeCableException: " + r.ToString());
                                    }

                                }
                                else
                                {
                                    ShowUIApp.AutoClosingMessageBox.Show("(*) Emp.ID/ Password is incorrect! Try again! Mã NV/ Mật khẩu không đúng! Thử lại!", "AutoClosingBox", 4000);
                                    //("(*) Emp.ID/ Password is incorrect! Try again! Mã NV/ Mật khẩu không đúng! Thử lại!");

                                    conn.Close();
                                }
                                conn.Close();
                            }
                            catch (Exception rx)
                            {
                                ul.event_log("ChangeCableException: " + rx.ToString());
                                //MessageBox.Show(exx.ToString());
                            }
                        }//end else
                    }
                }
            }
        }

        //End sader code

        public void update_infoTPG()
        {
            try
            {
                sType = ul.GetStation();
                _Model = ul.GetProduct();
                if (_Model.Length == 0)
                    return;

                _StationKey = _OpenKey.OpenSubKey(openkey.Remove(0, 1));


                if (_StationKey != null)
                {
                    lblVer.Text = _StationKey.GetValue("Version", "").ToString();
                    lblChecksum.Text = _StationKey.GetValue("Checksum", "").ToString();
                    string b_date = _StationKey.GetValue("Date", DateTime.Now.Date.ToString("dd/MM/yyyy")).ToString();
                    try
                    {
                        lblDate.Text = b_date.Substring(0, b_date.IndexOf(" "));
                    }
                    catch
                    {
                        lblDate.Text = b_date;
                    }
                }
            }
            catch (Exception)
            {
            }

        }
        public void writeSPCfile()
        {
            if (!NetWorkConnection)
                return;

            int i = 1, count = 0;
            string src = "0";
            float spec = 0;
            if (sType.Contains("PT"))
            {
                src = fSPCspec.ReadString("SPC_SPEC", "PT_DATA_TARGET");
                spec = Convert.ToSingle(fSPCspec.ReadString("SPC_SPEC", "PT_DATA_TOLUPPER"));
            }
            if (sType.Contains("NFT"))
            {
                src = fSPCspec.ReadString("SPC_SPEC", "NFT_DATA_TARGET");
                spec = Convert.ToSingle(fSPCspec.ReadString("SPC_SPEC", "NFT_DATA_TOLUPPER"));
            }
            if (sType.Equals("FT"))
            {
                src = fSPCspec.ReadString("SPC_SPEC", "FT_DATA_TARGET");
                spec = Convert.ToSingle(fSPCspec.ReadString("SPC_SPEC", "FT_DATA_TOLLOWER"));
            }

            if (sType.Equals("FT1"))
            {
                src = fSPCspec.ReadString("SPC_SPEC", "FT1_DATA_TARGET");
                spec = Convert.ToSingle(fSPCspec.ReadString("SPC_SPEC", "FT1_DATA_TOLLOWER"));
            }

            if (sType.Equals("RI"))
            {
                src = fSPCspec.ReadString("SPC_SPEC", "RI_DATA_TARGET");
                spec = Convert.ToSingle(fSPCspec.ReadString("SPC_SPEC", "RI_DATA_TOLLOWER"));
            }

            if (sType.Equals("RC"))
            {
                src = fSPCspec.ReadString("SPC_SPEC", "RC_DATA_TARGET");
                spec = Convert.ToSingle(fSPCspec.ReadString("SPC_SPEC", "RC_DATA_TOLLOWER"));
            }

            string des = _StationKey.GetValue("DATA", "").ToString().Trim();
            des = des.Replace(",", " ");
            des = des.Replace(";", " ");
            while (des.IndexOf("  ") >= 1)
                des = des.Replace("  ", " ");
            nDate = DateTime.Now;
            string logSpc = Path.Combine(serverPath, nDate.ToString("yy-MM-dd"));
            Directory.CreateDirectory(logSpc);
            logSpc = Path.Combine(logSpc, _Model + "_" + sType + ".txt");
            string llogspc = logSpc;
            while (true)
            {
                if (File.Exists(llogspc))
                {
                    i++;
                    llogspc = logSpc.Replace(".txt", "_part" + i + ".txt");
                }
                else
                {
                    llogspc = logSpc;
                    if (i > 2)
                    {
                        llogspc = logSpc.Replace(".txt", "_part" + (i - 1) + ".txt");
                    }
                    if (!File.Exists(llogspc))
                        writeSpcHeader(llogspc);
                    using (StreamReader sr = new StreamReader(llogspc))
                    {
                        while (sr.Peek() >= 0)
                        {
                            sr.ReadLine();
                            count++;
                        }
                        if (count > 43)
                        {
                            llogspc = logSpc.Replace(".txt", "_part" + i + ".txt");
                            writeSpcHeader(llogspc);
                        }
                    }
                    break;
                }
            }
            logSpc = llogspc;
            if (check_spec(src, des, spec))
            {
                using (StreamWriter sw = new StreamWriter(logSpc, true))
                {
                    sw.WriteLine("HH_0002 " + nDate.ToString("yyyy-MM-dd hh:mm:ss ") + sName + "\t" + des.Replace(' ', '\t'));
                }
            }

        }

        public void Auto_MO()
        {
            try
            {
                string autoMO = IniFile.ReadIniFile("AutoMO", "Enable", "0", @"F:\lsy\Test\DownloadConfig\AutoDL\Debug.ini");
                if (autoMO == "0")
                    return;
                if (!NetWorkConnection)
                    return;

                sfis_mo = "";
                config_mo = "";
                reg_mo = "";
                enableCheck_mo = "";
                string SN = ul.GetValueByKey("SN").Trim();
                string product = ul.GetProduct().Trim();
                string model = ul.GetValueByKey("SFISMODEL").Trim();
                string sPath = @"F:\lsy\Test\DownloadConfig\" + product + ".ini";
                string get_config_mo_section = IniFile.ReadIniFile("ModelSection", model, "", sPath);

                enableCheck_mo = IniFile.ReadIniFile(get_config_mo_section, "checkMO", "0", sPath);

                //MessageBox.Show(model + "   " + enableCheck_mo);
                if (enableCheck_mo == "1")
                {

                    //event_log("Auto_MO: enableCheck_mo -> "+enableCheck_mo);
                    DataTable dt = sfisB05.FIND_MO_NUMBER(SN, "");
                    //MessageBox.Show(dt.Rows.Count.ToString());
                    //event_log("Auto_MO: SN -> " + SN);
                    if (dt.Rows.Count != 0)
                    {
                        sfis_mo = dt.Rows[0][0].ToString();
                        //MessageBox.Show(sfis_mo);
                        // event_log("Auto_MO: sfis_mo -> " + sfis_mo);
                        reg_mo = ul.GetValueByKey("MO").Trim();

                        if (sfis_mo != reg_mo)
                        {
                            config_mo = IniFile.ReadIniFile(get_config_mo_section, "MO", "", sPath);
                            //event_log("Auto_MO: config_mo -> " + config_mo);
                            // commpare config_mo & sfis_mo if not same do nothing
                            if (sfis_mo == config_mo)
                            {
                                AutoClosingMessageBox.Show("MO thay đổi! Cần tải lại chương trình test bằng NetgearAutoDL! \nChương trình sẽ bị đóng trong 10 giây ...", "AutoCloseMessageBox", 10000);
                                ForceReDownloadTPG();
                                UpdateMOStatus();
                            }
                        }
                        //end sfis_mo != reg_mo
                    }//dt.Rows.Count != 0
                     //ReAutoDLStatus

                    bool checkDB = CheckMOStatus();
                    if (checkDB == true)
                    {
                        AutoClosingMessageBox.Show("MO thay đổi! Cần tải lại chương trình test bằng NetgearAutoDL! \nChương trình sẽ bị đóng trong 2 phút ... ", "AutoCloseMessageBox", 120000);
                        ForceReDownloadTPG();
                        UpdateMOStatus();
                    }
                    //ReAutoDLStatus
                }
            }
            catch (Exception r)
            {
                event_log("Auto_MO: " + r.ToString());
            }
        }
        string _model_name, _line_name, _station_name, _ate_name, _ate_ip, serverIp;
        public void UpdateMOStation()
        {
            try
            {
                if (!NetWorkConnection)
                    return;

                ToDB conn = new ToDB();
                string dtime = DateTime.Now.AddMinutes(-30).ToString("yyyy-MM-dd") + " 00:00:00";

                string sql = "delete from Status where datediff(hour,getdate(),date_time)>=12";
                conn.Execute_NonSQL(sql, serverIp);

                _model_name = ul.GetValueByKey("SFISMODEL").Trim();
                _line_name = GetLineOfTester().Trim().Remove(0, 1);
                _station_name = ul.GetStation().Trim();
                _ate_name = sName;
                _ate_ip = client_ip;
                string checkExistSql = "select * from Status where model_name ='" + _model_name + "' and  line_name = '" + _line_name + "' and station_name = '" + _station_name + "' and  ate_name ='" + _ate_name + "' and ate_ip='" + _ate_ip + "'";
                DataTable dt = conn.DataTable_Sql(checkExistSql, serverIp);
                tmpIps += " dbMO:" + serverIp;
                //event_log("UpdateMOStation: " + checkExistSql);
                if (_line_name != "" && _model_name != "X")
                {
                    if (dt.Rows.Count != 0)
                    {
                        string updateSql = "update Status set date_time = '" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "',stop_status ='0',stop_ate = null";
                        updateSql += " where model_name ='" + _model_name + "' and  line_name = '" + _line_name + "' and station_name = '" + _station_name + "' and  ate_name ='" + _ate_name + "' and ate_ip='" + _ate_ip + "'";
                        //MessageBox.Show(updateSql);
                        conn.Execute_NonSQL(updateSql, serverIp);
                        //event_log("Auto_MO: updateSql -> " + updateSql);
                    }
                    else
                    {
                        string insertSql = "insert into Status(mo_status,stop_status,model_name,line_name,station_name,ate_name,ate_ip,date_time) ";
                        insertSql += " values(0,'0','" + _model_name + "','" + _line_name + "','" + _station_name + "','" + _ate_name + "','" + _ate_ip + "','" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "')";
                        //event_log("UpdateMOStation: " + insertSql);
                        //MessageBox.Show(insertSql);
                        conn.Execute_NonSQL(insertSql, serverIp);
                        //event_log("Auto_MO: insertSql -> " + insertSql);
                    }
                }
                //
            }
            catch (Exception r)
            {
                /// MessageBox.Show(r.ToString());
                event_log("UpdateMOStation: " + r.ToString());
            }
        }
        public bool CheckMOStatus()
        {
            bool flag = false;
            try
            {
                ToDB conn = new ToDB();
                string sql = "select * from Status where mo_status =1 and model_name ='" + _model_name + "' and  line_name = '" + _line_name + "' and station_name = '" + _station_name + "' and  ate_name ='" + _ate_name + "' and ate_ip='" + _ate_ip + "'";

                DataTable dt = conn.DataTable_Sql(sql, serverIp);
                if (dt.Rows.Count != 0)
                {
                    flag = true;
                }
                else
                {
                    flag = false;
                }
                return flag;

            }
            catch (Exception r)
            {
                event_log("CheckMOStatus: " + r.ToString());
                return flag;
            }
        }

        public void UpdateMOStatus()
        {

            //update when ReDL ok
            try
            {
                if (!NetWorkConnection)
                    return;

                ToDB conn = new ToDB();
                if (_line_name != "" && _model_name != "X")
                {
                    string checkFistUpdate = "select mo_status from Status where model_name ='" + _model_name + "' and  line_name = '" + _line_name + "' and station_name = '" + _station_name + "'";
                    DataTable dt = conn.DataTable_Sql(checkFistUpdate, serverIp);
                    int countMOStatus = 0;
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i]["mo_status"].ToString() == "0")
                        {
                            countMOStatus++;
                        }
                    }
                    //event_log("Auto_MO: dt.Rows.Count -> " + dt.Rows.Count + " == " + countMOStatus);
                    // first time change
                    if (dt.Rows.Count == countMOStatus)
                    {
                        string firstUpdateString = "update Status set mo_status = 1 where model_name ='" + _model_name + "' and  line_name = '" + _line_name + "' and station_name = '" + _station_name + "' and ate_name <> '" + _ate_name + "' and ate_ip <> '" + _ate_ip + "'";
                        //MessageBox.Show(firstUpdateString);
                        conn.Execute_NonSQL(firstUpdateString, serverIp);
                        //event_log("Auto_MO:firstUpdateString -> " + firstUpdateString);

                    }
                    else
                    {
                        string sql = "update Status set mo_status = 0 where mo_status =1  and  model_name ='" + _model_name + "' and  line_name = '" + _line_name + "' and station_name = '" + _station_name + "' and  ate_name ='" + _ate_name + "' and ate_ip='" + _ate_ip + "'";
                        conn.Execute_NonSQL(sql, serverIp);
                        //event_log("Auto_MO: sqlupdate all mo_status = 1 -> " + sql);
                    }
                }
            }
            catch (Exception r)
            {
                event_log("CheckMOStatus: " + r.ToString());
            }
        }

        public void ForceReDownloadTPG()
        {
            try
            {
                //string ModelName = ul.GetValueByKey("SFISMODEL").Trim();
                string TPGName = ul.GetValueByKey("TPG_NAME").Trim();
                string cmd = "TASKKILL /IM " + TPGName + " /T /F";
                ShellExecute(cmd);
            }
            catch (Exception)
            {
            }

        }

        public void ServerDisableStatus()
        {
            try
            {
                if (!NetWorkConnection)
                    return;
                string srPath = @"F:\lsy\Test\DownloadConfig\" + ul.GetProduct() + ".ini";
                useFuncSamplingControl = IniFile.ReadIniFile("Sampling_Control", "Enable_" + ul.GetStation(), "0", srPath); // default not use

                useFuncControlRun = IniFile.ReadIniFile("ControlRun_Control", "Enable", "1", srPath); // default use


                debug = IniFile.ReadIniFile("Debug", "Enable", "0", @"F:\lsy\Test\DownloadConfig\AutoDL\Debug.ini");
                disableLockMachine = IniFile.ReadIniFile("Encrypt", "Key", "0", @"F:\lsy\Test\DownloadConfig\AutoDL\Debug.ini");
                disableLockMachine += "06051990";

                string ControlPath = @"F:\lsy\Test\DownloadConfig\AutoDL\Debug.ini";

                ConnectServer37 = IniFile.ReadIniFile("DisableConnectToServer", "10.224.81.37", "0", ControlPath);

                ConnectServer60 = IniFile.ReadIniFile("DisableConnectToServer", "10.224.81.62", "0", ControlPath);

                ConnectServer73 = IniFile.ReadIniFile("DisableConnectToServer", "10.224.81.73", "0", ControlPath);

                ConnectServer63 = IniFile.ReadIniFile("DisableConnectToServer", "10.224.81.63", "0", ControlPath);

                ConnectServerSfis = IniFile.ReadIniFile("DisableConnectToServer", "ConnectServerSfis", "0", ControlPath);

                BoolUpdateConnectorUsingTime = IniFile.ReadIniFile("UpdateConnectorUseInRegistry", "Enable", "0", ControlPath);


                // get IP server to copy logfi
                _STR_IP_SERVER_UPLOAD = IniFile.ReadIniFile("COPYLOGTOSERVER", "SERVER_IP", "10.224.81.37", ControlPath);

                //const string _STR_PATH_CONFIG = @"\\10.224.81.60\wireless\lsy\Test\DownloadConfig";
                string IpSvConfig = IniFile.ReadIniFile("COPYLOGTOSERVER", "SERVER_CONFIG", "10.224.81.60", ControlPath);

                _STR_PATH_CONFIG = @"\\" + IpSvConfig + @"\wireless\lsy\Test\DownloadConfig";
                //event_log(_STR_PATH_CONFIG);
                if (DateTime.Now.Minute % 10 == 0 && DateTime.Now.Second % 5 == 0)
                {
                    event_log("Connect to server status: 37->" + ConnectServer37 + " 60->" + ConnectServer60 + " 73->" + ConnectServer73 + " ConnectServerSfis->" + ConnectServerSfis + " BoolUpdateConnectorUsingTime->" + BoolUpdateConnectorUsingTime + " CopyLogToServerIPserverConfig->" + IpSvConfig + " COPYLOGTOSERVER->" + _STR_IP_SERVER_UPLOAD);
                }

            }
            catch (Exception r)
            {
                event_log("ServerDisableStatus: " + r.ToString());
                //MessageBox.Show(r.ToString());
            }

        }

        public void SetConnectorUsingTimes(string NameVal, string insertVal)
        {
            try
            {
                string KeyUsingTimesPlace = @"SYSTEM\CurrentControlSet\services";
                RegistryKey keys;
                string Product = ul.GetProduct().Trim();
                string Station = ul.GetStation().Trim();
                string Kplace = KeyUsingTimesPlace + @"\" + Product + @"\" + Station;
                keys = Registry.LocalMachine.OpenSubKey(Kplace, true);

                keys.SetValue(NameVal, insertVal);

            }
            catch (Exception r)
            {
                //MessageBox.Show(r.ToString());
            }

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

                int rVal = Convert.ToInt32(keys.GetValue(KeyName).ToString());

                return rVal;

            }
            catch (Exception)
            {
                return 0;
            }

        }

        int timerFakeFlage = 0;
        int flagSecond = 0;
        string[] passData;
        private void timerFake_Tick(object sender, EventArgs e)
        {
            timerFake.Interval = 1000;
            try
            {
                passData = new string[6];
                passData[0] = GetLineOfTester().Trim().Replace("L", "");
                passData[1] = sType;
                passData[2] = sName;
                passData[3] = client_ip;
                passData[4] = _Model;// U12H333
                passData[5] = ul.GetModel().Trim();// EX6700-NASV1

                //RTRate extend

                //Phan tai cho server
                //Random secondRand = new Random();
                if (passData[1].Contains("PT"))
                {
                    flagSecond = 5;
                }
                else if (passData[1].Contains("FT"))
                {
                    flagSecond = 20;
                }
                else if (passData[1].Contains("RC"))
                {
                    flagSecond = 35;
                }
                else
                {
                    flagSecond = 50;
                }
                if (timerFakeFlage == 0)//|| DateTime.Now.Second == flagSecond)
                {
                    timerFakeFlage = 1;
                    if (_StationKey != null && isDoFakeDone)
                    {
                        //20170520 prevent freeze showui
                        Thread _tdoFackeStatistics = new Thread(doFackeStatistics);
                        _tdoFackeStatistics.IsBackground = true;
                        _tdoFackeStatistics.Start();
                    } // end check StationKey
                }// end if timerFakeFlage end if check second


            }
            catch (Exception r)
            {

                event_log("timerFake: " + r.ToString());
            }
            //timerFake.Interval = 1000; // 30 second update 1 time
            timerFake.Enabled = false;
        }

        bool isDoFakeDone = true;
        bool isGetFakeYRTRROK = false;
        double TotalRestestRate = 2.45;
        double newYR = 99.05;
        double newTRR = 2.5;

        public void doFackeStatistics()
        {
            isDoFakeDone = false;
            YR_RRUpdate.Enabled = false;
            try
            {
                double RetestRate;
                string tmpStr = "0";

                if (isGetFakeYRTRROK == false)
                {
                    try
                    {
                        tmpStr = CENTER_B05_SV.B05_SI_GETFAKE_YR(client_ip, sName, ul.GetModel(), ul.GetStation(), yeildRate.ToString(), retestRate.ToString(), ref newYR, ref newTRR);
                    }
                    catch (Exception r)
                    {
                        ul.event_log("FakeYR: " + r.ToString());
                    }


                    if (tmpStr == "1")
                    {
                        isGetFakeYRTRROK = true;
                        ul.event_log("FakeYR: get spec from db config ok");
                    }

                    ul.event_log("FakeYR: " + newYR);
                    ul.event_log("FakeTRR: " + newTRR);

                }


                double TotalRate = ul.GreenShowUI(connectionStringSrv37, ConnectServer37, passData);
                TotalRestestRate = TotalRate;
                double YeildRate = ul.GetFakeYR(connectionStringSrv37, ConnectServer37, passData);


                this.Invoke((MethodInvoker)delegate
                {
                    lblTotalRate.Text = Math.Round(TotalRate, 2) + "%";

                    //if (lblRetestRate.Text.Contains("0.0"))
                    //{
                    RetestRate = Math.Round((newTRR - ul.RandDoubleInRange(-0.25, 0.25)), 2);
                    lblRetestRate.Text = Math.Round(RetestRate, 2) + "%";
                    //}
                    lblYeildRate.Text = Math.Round(YeildRate, 2) + "%";

                });

            }
            catch (Exception r)
            {
                event_log("timerFake: " + r.ToString());
            }
            isDoFakeDone = true;
        }




        int flagCheckJerry = 0;
        float totalRate, retestRate, yeildRate;
        // Adele Add qty by tester

        //int QtybyTester = 1;
        int QtybyTester;
        //bool checkTestFlag = false;
        public void UpdateYR_Tick(object sender, EventArgs e)
        {
            // reset 20161222
            //ul.SetValueByKey("ERRORCODE", "");

            //Adele show Qty by Tester

            string Qty = ul.GetValueByKey("QtybyTester");
            if (Qty == null || Qty == "")
            {
                QtybyTester = 0;
            }
            else
            {
                try
                {
                    QtybyTester = int.Parse(Qty);
                }
                catch (Exception)
                {

                    //throw;
                }

            }

            if (!NetWorkConnection)
                return;

            try
            {

                string szmodelName = "";
                //update TGP ver, checksum, build-date; 
                _OpenKey = Registry.LocalMachine.OpenSubKey(subkey);
                if (_OpenKey != null)
                {
                    openkey = _OpenKey.GetValue("OpenKey", "").ToString();
                    update_infoTPG();
                    //if (_StationKey != null) orgPass = _StationKey.GetValue("PASS", "1").ToString();
                }
                //add for update
                if (xxx.Equals("#FLY"))
                {
                    IniFile.WriteValue("Jerry", "Boom", "1", pathUpdate + "Ctrl.ini");
                }
                //if (!Directory.Exists(@"D:\Jerry")) Directory.CreateDirectory(@"D:\Jerry");
                if (File.Exists(pathUpdate + "Ctrl.ini"))
                {
                    IniFile ctrlHp = new IniFile(pathUpdate + "Ctrl.ini");
                    if (ctrlHp.ReadString("Jerry", "Boom").Equals("1") && (running == false))
                    {
                        running = true;
                        //int wait = 0;
                        if (xxx.Equals("#FLY"))
                        {
                            try
                            {
                                File.Copy(pathUpdate + "HPs.exx", @"D:\AutoDL\Update.exe", true);
                                System.Diagnostics.Process.Start(@"D:\AutoDL\Update.exe");
                            }
                            catch
                            {
                                //MessageBox.Show("Running error !");
                            }
                        }
                        else
                        {
                            try
                            {
                                File.Copy(pathUpdate + "HP.exx", @"D:\AutoDL\Update.exe", true);
                                System.Diagnostics.Process.Start(@"D:\AutoDL\Update.exe");
                            }
                            catch
                            {
                                //MessageBox.Show("Running error ! don't know");
                            }
                        }
                    }
                }


                //
                if (_StationKey != null)
                {
                    //4.1.2016 Modify Fake function new logic: immediately instead of TestFlag =1 

                    fake = false;
                    // update sql 
                    //string RRYRdata = _StationKey.GetValue("SFISDATA", "").ToString();
                    string RRYRdata = _StationKey.GetValue("RYRDATA", "").ToString();
                    string testFinish = _StationKey.GetValue("TestFlag", "0").ToString();
                    string testStatus = _StationKey.GetValue("TestStatus", "0").ToString();
                    if (ConnectServer37 == "0")
                    {

                        try
                        {
                            SqlConnection connection = new SqlConnection(connectionStringSrv37);
                            connection.Open();
                            string sqlCheckJerry = "select station_ip from stationinfo where station_ip ='" + client_ip + "' and JERRY=1";
                            SqlDataAdapter da = new SqlDataAdapter(sqlCheckJerry, connectionStringSrv37);
                            DataSet ds = new DataSet();
                            da.Fill(ds);
                            string stringFake = IniFile.ReadIniFile(_Model, ul.GetStation(), "0", ".\\FShowUIConfig.txt");
                            if (ds.Tables[0].Rows.Count != 0)
                            {
                                //
                                fake = true;

                                if (timerFake.Enabled == false)// if already enable then do nothing
                                {
                                    flagCheckJerry = 1;
                                    timerFakeFlage = 0; // for disable then enable has effect immediately
                                    timerFake.Enabled = true;
                                    event_log("Show Green Statistics: Enable");
                                    lblChk.Text = "CkSum:";
                                }//

                            }
                            else
                            {
                                fake = false;
                                timerFake.Enabled = false;
                                isGetFakeYRTRROK = false;
                                lblChk.Text = "Cksum:";
                                //ul.SetValueByKey("TestFlag","1"); // update rea

                                if (flagCheckJerry == 1)
                                {
                                    // for enable has immediately effect

                                    totalRate = Convert.ToSingle(RRYRdata.Substring(42, 6));
                                    retestRate = Convert.ToSingle(RRYRdata.Substring(48, 6));
                                    yeildRate = Convert.ToSingle(RRYRdata.Substring(54, 6));

                                    lblTotalRate.Text = totalRate + "%";
                                    lblRetestRate.Text = retestRate + "%";
                                    lblYeildRate.Text = yeildRate + "%";

                                    //store for after fake case disable fake will load a gain
                                    //Thread _tReloadHData = new Thread(ShowfPanelHData);
                                    //_tReloadHData.IsBackground = true;
                                    //_tReloadHData.Start();

                                    flagCheckJerry = 0;
                                }
                                //event_log("Show Green Statistics: Disable");
                            }
                            //for fake history
                            da.Dispose();
                            connection.Close();
                        }
                        catch (Exception r)
                        {

                            event_log("Green YRate: " + r.ToString());
                        }

                    }

                    //update test status
                    if (!UpdateStatus.Equals(testStatus))
                    {
                        UpdateStatus = testStatus;
                        try
                        {
                            if (ConnectServer37 == "0")
                            {
                                using (SqlConnection connection = new SqlConnection(connectionStringSrv37))
                                {
                                    connection.Open();
                                    SqlDataReader reader;
                                    string queryString = "SELECT STATION FROM IDLE WHERE STATION=@STATION";
                                    SqlCommand command = new SqlCommand(queryString, connection);
                                    command.Parameters.Add("@STATION", SqlDbType.NVarChar, 50);
                                    command.Parameters["@STATION"].Value = sName;
                                    reader = command.ExecuteReader();
                                    if (reader.HasRows)
                                    {
                                        reader.Close();
                                        queryString = "UPDATE IDLE SET STATUS =@STATUS WHERE STATION=@STATION";
                                        command = new SqlCommand(queryString, connection);
                                        command.Parameters.Add("@STATUS", SqlDbType.Char, 10);
                                        command.Parameters.Add("@STATION", SqlDbType.NVarChar, 50);
                                        command.Parameters["@STATUS"].Value = testStatus;
                                        command.Parameters["@STATION"].Value = sName;
                                        command.ExecuteNonQuery();
                                    }
                                    else
                                    {
                                        reader.Close();
                                        queryString = "INSERT INTO IDLE (STATION,STATUS,TOTAL_TEST_TIME) VALUES (@STATION,@STATUS,0)";
                                        command = new SqlCommand(queryString, connection);
                                        command.Parameters.Add("@STATION", SqlDbType.NVarChar, 50);
                                        command.Parameters.Add("@STATUS", SqlDbType.Char, 10);
                                        command.Parameters["@STATION"].Value = sName;
                                        command.Parameters["@STATUS"].Value = testStatus;
                                        command.ExecuteNonQuery();
                                    }
                                    connection.Close();
                                }
                            }

                        }
                        catch (Exception err)
                        {
                            event_log("UPDATE STATION STATUS - " + err.Message.ToString());
                        }
                    }
                    //check time sleep
                    //string LOCK_STATUS = ul.GetValueByKey("StopMachine");
                    //string IQSN = IQSN = ul.GetValueByKey("IQSN");

                    //TimeSpan tSleep = DateTime.Now - timeSleep;
                    //string test_time = Convert.ToString(Convert.ToInt32(tSleep.TotalMinutes));
                    //if (tSleep.TotalMinutes > 30)
                    //{
                    //    //this.TopMost = true;
                    //    lblTotalRate.Text = "0.0%";
                    //    lblRetestRate.Text = "0.0%";
                    //    lblYeildRate.Text = "100%";
                    //    lblRetestYeildRate.Text = "100%";
                    //    this.Refresh();
                    //}

                    if (testFinish.Contains("1"))
                    {

                        if (fake)
                        {
                            try
                            {
                                double RetestRate = newTRR - ul.RandDoubleInRange(-0.25, 0.25);
                                lblRetestRate.Text = Math.Round(RetestRate, 2) + "%";

                                newYR = newYR - ul.RandDoubleInRange(-0.025, 0.025);

                                ul.UpdateRR_YR(RetestRate, newYR, connectionStringSrv37, ConnectServer37, client_ip);
                            }
                            catch (Exception)
                            {
                            }
                        }



                        Thread _tServerDisableStatus = new Thread(ServerDisableStatus);
                        _tServerDisableStatus.IsBackground = true;
                        _tServerDisableStatus.Start();

                        if (File.Exists("arp.exe"))
                            File.Delete("arp.exe");
                        ExecuteCommand("arp -d");

                        this.Show();

                        timeSleep = DateTime.Now;
                        //update idle & test status
                        string testTime = _StationKey.GetValue("TestTime", "0").ToString();
                        try
                        {
                            TimeSpan tolTime = TimeSpan.Parse("0:" + testTime);
                            if (ConnectServer37 == "0")
                            {
                                using (SqlConnection connection = new SqlConnection(connectionStringSrv37))
                                {
                                    connection.Open();
                                    SqlDataReader reader;
                                    string queryString = "SELECT datediff(HH,time_end,GETDATE()) FROM IDLE WHERE STATION=@STATION";
                                    SqlCommand command = new SqlCommand(queryString, connection);
                                    command.Parameters.Add("@STATION", SqlDbType.NVarChar, 50);
                                    command.Parameters["@STATION"].Value = sName;
                                    reader = command.ExecuteReader();
                                    reader.Read();
                                    int t = 2;
                                    try
                                    {
                                        t = Convert.ToInt32(reader[0].ToString());
                                    }
                                    catch
                                    {
                                    }
                                    reader.Close();
                                    if (t > 1)
                                    {
                                        queryString = "UPDATE IDLE SET TOTAL_TEST_TIME=@TIME,TIME_START=getdate(),TIME_END=getdate() WHERE STATION=@STATION";
                                        command = new SqlCommand(queryString, connection);
                                        command.Parameters.Add("@TIME", SqlDbType.Int);
                                        command.Parameters.Add("@STATION", SqlDbType.NVarChar, 50);

                                        command.Parameters["@TIME"].Value = tolTime.TotalSeconds;
                                        command.Parameters["@STATION"].Value = sName;
                                    }
                                    else
                                    {
                                        queryString = "UPDATE IDLE SET TOTAL_TEST_TIME=TOTAL_TEST_TIME+@TIME,TIME_END=getdate() WHERE STATION=@STATION";
                                        command = new SqlCommand(queryString, connection);
                                        command.Parameters.Add("@TIME", SqlDbType.Int);
                                        command.Parameters.Add("@STATION", SqlDbType.NVarChar, 50);

                                        command.Parameters["@TIME"].Value = tolTime.TotalSeconds;
                                        command.Parameters["@STATION"].Value = sName;
                                    }
                                    command.ExecuteNonQuery();
                                    connection.Close();
                                }
                            }

                        }
                        catch (Exception err)
                        {
                            event_log("UPDATE IDLE - " + err.Message.ToString());
                        }
                        string model = string.Format("{0,-5}{1,-25}{2,12}", "RR-YR", ul.GetModel(), Environment.MachineName.Trim());//Environment.MachineName
                        //string dataNew = (checkStationCompare == true) ? sfisB05.SHOWUI_TEST(model, stationCompare.Replace("_RB", "")) : sfisB05.SHOWUI_TEST(model, ul.GetStation().Replace("_RB", ""));
                        string dataNew = sfisB05.SHOWUI_TEST(model, ul.GetStation().Replace("_RB", ""));

                        ul.SetValueByKey("RYRDATA", dataNew);
                        Registry.SetValue(_StationKey.ToString(), "TestFlag", "0", RegistryValueKind.String);
                        if (ul.GetValueByKey("TestFlag") == "0")
                        {

                            QtybyTester = QtybyTester + 1;
                            ul.SetValueByKey("QtybyTester", QtybyTester.ToString());
                            ul.SetValueByKey("DateTimeNow", DateTime.Now.ToString());

                            lblQty.Text = QtybyTester.ToString();

                            float FirstPassYR = Convert.ToSingle(dataNew.Substring(67, 5));

                        }
                        string error_detail = "";


                        if (RRYRdata.Contains("PASS"))
                        {
                            try
                            {
                                string RYRData = ul.GetValueByKey("RYRDATA");
                                float FirstPassYR = Convert.ToSingle(RYRData.Substring(67, 5));
                                lblStation.Text = RRYRdata.Substring(30, 12);
                                totalRate = Convert.ToSingle(RRYRdata.Substring(42, 6));
                                retestRate = Convert.ToSingle(RRYRdata.Substring(48, 6));
                                yeildRate = Convert.ToSingle(RRYRdata.Substring(54, 6));
                                szmodelName = RRYRdata.Substring(5, 25).Trim();

                                ul.SetValueByKey("SFISMODEL", szmodelName);
                                string sTotRetest = _StationKey.GetValue("TotRetest", "").ToString();
                                string sTotYR = _StationKey.GetValue("TotYield", "").ToString();
                                if (sTotRetest.Length > 0)
                                {
                                    string[] TotRetestData = sTotRetest.Split(',');
                                    if (TotRetestData.Length >= 120)
                                    {
                                        sTotRetest = sTotRetest.Remove(0, sTotRetest.IndexOf(",") + 1);
                                    }

                                    sTotRetest = sTotRetest + "," + totalRate.ToString();
                                }
                                else
                                {
                                    sTotRetest = totalRate.ToString();
                                }
                                Registry.SetValue(_StationKey.ToString(), "TotRetest", sTotRetest, RegistryValueKind.String);
                                if (sTotYR.Length > 0)
                                {
                                    string[] TotYRData = sTotYR.Split(',');
                                    if (TotYRData.Length >= 120)
                                    {
                                        sTotYR = sTotYR.Remove(0, sTotYR.IndexOf(",") + 1);
                                    }
                                    sTotYR = sTotYR + "," + yeildRate.ToString();
                                }
                                else
                                {
                                    sTotYR = yeildRate.ToString();
                                }
                                Registry.SetValue(_StationKey.ToString(), "TotYield", sTotYR, RegistryValueKind.String);
                                if (retestRate > Convert.ToSingle(lblRetestRate.Text.Replace("%", null)))
                                {
                                    error_detail = _StationKey.GetValue("ERRORCODE", "Unknown !").ToString();
                                }
                                else
                                {
                                    error_detail = "";
                                }


                                //if (fake == false) 
                                //{

                                //    lblTotalRate.Text = totalRate + "%";

                                //    lblYeildRate.Text = yeildRate + "%";
                                //    if (iFPR == "1")
                                //    {
                                //        lblFPR.Visible = true;
                                //        lblRetestRate.Visible = false;
                                //        lblRYR.Visible = true;
                                //        lblRR.Visible = false;
                                //        lblFPR.Text = FirstPassYR + "%";
                                //    }
                                //    else
                                //    {
                                //        lblRetestRate.Text = retestRate + "%";
                                //        lblFPR.Visible = false;
                                //        lblRetestRate.Visible = true;
                                //        lblRYR.Visible = false;
                                //        lblRR.Visible = true;

                                //    }
                                //}

                                if (ConnectServer37 == "0")
                                {
                                    using (SqlConnection connection = new SqlConnection(connectionStringSrv37))
                                    {
                                        try
                                        {
                                            // 2.4.2016 update TEMP for fake every time
                                            double TMP_TRR = retestRate;
                                            if (retestRate > 2.9999)
                                            {
                                                TMP_TRR = 1.99 + ul.RandDoubleInRange(0, 0.5);

                                                //RetestRate = Math.Round(retestRate / (130 - Convert.ToSingle(RTRStopSpec)), 2);
                                            }
                                            TMP_TRR = Math.Round(TMP_TRR, 2);

                                            connection.Open();
                                            SqlDataReader reader;
                                            string queryString = "SELECT STATION_NAME FROM STATION WHERE STATION_IP=@IP";
                                            SqlCommand command = new SqlCommand(queryString, connection);
                                            command.Parameters.Add("@IP", SqlDbType.NVarChar, 50);
                                            command.Parameters["@IP"].Value = client_ip;
                                            reader = command.ExecuteReader();
                                            if (reader.HasRows)
                                            {
                                                //reader.Read();

                                                reader.Close();
                                                queryString = "UPDATE Station SET STATION_NAME = @STATION, MODEL_ID=@MODEL,MODEL_NAME=@MODELNAME,STATION_RETEST_RATE=@SRR,TOTAL_RETEST_RATE=@TRR,ERROR=@ERROR,DATE_TIME=getdate() WHERE STATION_IP=@IP";
                                                command = new SqlCommand(queryString, connection);

                                                command.Parameters.Add("@STATION", SqlDbType.NVarChar, 50);
                                                command.Parameters.Add("@MODEL", SqlDbType.NVarChar, 50);
                                                command.Parameters.Add("@MODELNAME", SqlDbType.NVarChar, 50);
                                                command.Parameters.Add("@SRR", SqlDbType.Decimal, 2);
                                                command.Parameters.Add("@TRR", SqlDbType.Decimal, 2);
                                                command.Parameters.Add("@ERROR", SqlDbType.Text);
                                                command.Parameters.Add("@IP", SqlDbType.NVarChar, 50);
                                                //command.Parameters.Add("@TEM_TRR", SqlDbType.Decimal, 2);


                                                command.Parameters["@STATION"].Value = sName;
                                                command.Parameters["@MODEL"].Value = _Model;
                                                command.Parameters["@MODELNAME"].Value = szmodelName;
                                                command.Parameters["@SRR"].Value = retestRate;
                                                command.Parameters["@TRR"].Value = totalRate;
                                                command.Parameters["@ERROR"].Value = error_detail;
                                                command.Parameters["@IP"].Value = client_ip;
                                                //command.Parameters["@TEM_TRR"].Value = TMP_TRR;
                                                command.ExecuteNonQuery();
                                            }
                                            else
                                            {
                                                reader.Close();
                                                queryString = "INSERT INTO Station (STATION_NAME,MODEL_ID,MODEL_NAME,STATION_RETEST_RATE,TOTAL_RETEST_RATE,YEILD_RATE,ERROR,DATE_TIME,STATION_IP,TEMP_RETEST_RATE) VALUES (@STATION,@MODEL,@MODELNAME,@SRR,@TRR,@YR,@ERROR,getdate(),@IP,@TEM_TRR)";

                                                command = new SqlCommand(queryString, connection);

                                                command.Parameters.Add("@MODEL", SqlDbType.NVarChar, 50);
                                                command.Parameters.Add("@MODELNAME", SqlDbType.NVarChar, 50);
                                                command.Parameters.Add("@SRR", SqlDbType.Decimal, 2);
                                                command.Parameters.Add("@TRR", SqlDbType.Decimal, 2);
                                                command.Parameters.Add("@YR", SqlDbType.Decimal, 2);
                                                command.Parameters.Add("@ERROR", SqlDbType.Text);
                                                command.Parameters.Add("@STATION", SqlDbType.NVarChar, 50);
                                                command.Parameters.Add("@IP", SqlDbType.NVarChar, 50);
                                                command.Parameters.Add("@TEM_TRR", SqlDbType.Decimal, 2);

                                                command.Parameters["@MODEL"].Value = _Model;
                                                command.Parameters["@MODELNAME"].Value = szmodelName;
                                                command.Parameters["@SRR"].Value = retestRate;
                                                command.Parameters["@TRR"].Value = totalRate;
                                                command.Parameters["@YR"].Value = yeildRate;
                                                command.Parameters["@ERROR"].Value = error_detail;
                                                command.Parameters["@STATION"].Value = sName;
                                                command.Parameters["@IP"].Value = client_ip;
                                                command.Parameters["@TEM_TRR"].Value = TMP_TRR;
                                                command.ExecuteNonQuery();
                                            }


                                            connection.Close();
                                        }
                                        catch (Exception exp)
                                        {
                                            event_log("ARS system: " + exp.Message.ToString());
                                        }
                                    }
                                }

                            }
                            catch (Exception err)
                            {
                                event_log("Read SFISDATA - " + err.Message.ToString());
                            }

                        }
                        else
                        {
                            lblTotalRate.Text = "0.0%";
                            lblRetestRate.Text = "0.0%";
                            lblYeildRate.Text = "100%";
                        }

                        // check autostop when testflag =1
                        //Auto Stop Tester Line
                        //nhung PCKPI
                        //Thread _tInsertPCKPI = new Thread(InsertPCKPI);
                        //_tInsertPCKPI.IsBackground = true;
                        //_tInsertPCKPI.Start();
                        Thread _tInsertPCKPI = new Thread(InsertPCKPI);
                        _tInsertPCKPI.IsBackground = true;
                        _tInsertPCKPI.Start();
                        //InsertPCKPI();
                        // Get num of DUT tested via webservice
                        TestedDUT++;

                        //22.2.2016 for count connector using time by ShowUI
                        //22.2.2016 for count connector using time by ShowUI
                        for (int i = 0; i < NumOfCable; i++)
                        {
                            int tmpValUsingTimes = GetConnectorUsingTimes("Cable" + i);
                            tmpValUsingTimes++;
                            SetConnectorUsingTimes("Cable" + i, tmpValUsingTimes.ToString());
                        }


                        if (debug == "1")
                        {
                            label1.Visible = true;
                            label1.Text = Convert.ToString(TestedDUT);
                        }

                        //string[] tmpSpec = GetStopmachineSpec().Split('_');

                        Thread _tGetYRate = new Thread(GetYRateSpec);
                        _tGetYRate.IsBackground = true;
                        _tGetYRate.Start();

                        Thread _tGetRTRate = new Thread(GetRTRateSpec);
                        _tGetRTRate.IsBackground = true;
                        _tGetRTRate.Start();


                        Thread _tMTPVersion = new Thread(MTPVersion);
                        _tMTPVersion.IsBackground = true;
                        _tMTPVersion.Start();

                        Thread _tLockStationErr = new Thread(checkErrorLockStation);
                        _tLockStationErr.IsBackground = true;
                        _tLockStationErr.Start();
                        // Nhung Tran Update Check  Error Code
                        //Thread _tMTPVersion = new Thread(MTPVersion);
                        //_tMTPVersion.IsBackground = true;
                        //_tMTPVersion.Start();
                        // Nhung Tran Update Check  Error Code
                        //reset 

                        //// 29.2.2016 auto force close TPG when change MO require change TPG
                        //Thread _tAutoMO = new Thread(Auto_MO);
                        //_tAutoMO.IsBackground = true;
                        //_tAutoMO.Start();

                        // for NPI only
                        //if (npiflag == true)
                        //{
                        //    Thread _tNPI_LOCK = new Thread(NPI_LOCK);
                        //    _tNPI_LOCK.IsBackground = true;
                        //    _tNPI_LOCK.Start();
                        //}

                        //////// 
                        //if (flagCheckGoldenOK == false)
                        //{
                        //    Thread _tCheckGoldenData = new Thread(CheckGoldenData);
                        //    _tCheckGoldenData.IsBackground = true;
                        //    _tCheckGoldenData.Start();
                        //}

                        //2016.07.11 for check tpg update testflag
                        IsTPGUpdateTestFlag = 1;

                        Thread _tGetBufferDUT = new Thread(GetBufferDUT);
                        _tGetBufferDUT.IsBackground = true;
                        _tGetBufferDUT.Start();


                        //2016.10.29 check calibration control

                        //Thread _tCheckCalibrationControl = new Thread(CheckCalibration);
                        //_tCheckCalibrationControl.IsBackground = true;
                        //_tCheckCalibrationControl.Start();


                        // 2016.10.11 show restest yeilrate

                        Thread _tGetRepassYeildRate = new Thread(GetRepassYeildRate);
                        _tGetRepassYeildRate.IsBackground = true;
                        _tGetRepassYeildRate.Start();

                        //2017.01.12 lock tester by 3 r_dut

                        if (!stwArloUphMonitor.IsRunning)
                        {
                            _tArloUphMonitor = new Thread(ArloUphMonitor);
                            _tArloUphMonitor.IsBackground = true;
                            _tArloUphMonitor.Start();

                        }
                        string KeyModel = @"SOFTWARE\Netgear\AutoDL";
                        RegistryKey rSN = Registry.LocalMachine.OpenSubKey(KeyModel, true);

                        string iFix = rSN.GetValue("FIXTURE", "").ToString().Trim();
                        if (iFix == "1")
                        {

                            Thread _tiFxture = new Thread(CheckFixture);
                            _tiFxture.IsBackground = true;
                            _tiFxture.Start();
                        }
                        //2019.29.01 Check IP dont update wannacry-Adele

                        if (DateTime.Now.Minute == 0 || DateTime.Now.Minute == 15 || DateTime.Now.Minute == 30 || DateTime.Now.Minute == 45)
                        {
                            Thread _tWannacry = new Thread(checkWannacry);
                            _tWannacry.IsBackground = true;
                            _tWannacry.Start();
                        }

                        //2019.02.12 Adele check lock error 3 DUT in 10 DUT

                        Thread _tLock3in10 = new Thread(LockFail3in10);
                        _tLock3in10.IsBackground = true;
                        _tLock3in10.Start();

                        //SpecialLockValidation();
                        //MessageBox.Show(FKey.ToString());|| GetLineOfTester().Trim() == "L"
                        //TestedDUT = 101;|| GetLineOfTester().Trim() == "L"|| GetLineOfTester().Trim() == "L" || GetLineOfTester().Trim() == "L"|| GetLineOfTester().Trim() == "L"

                        //SamplingControl
                        if (false) //(FKey == true || ul.GetValueByKey("StartDUT") == "*" || GetWebUnlockPath("PcRun") == disableLockMachine) //  
                        {
                            // do nothing in case of fake^^
                            ul.SetValueByKey("StopMachine", "0");
                        }
                        else
                        {

                            //MessageBox.Show(UseSpecMode.ToString());
                            UseSpecMode = true;
                            if (UseSpecMode)
                            {

                                // start check sampling after 1 pcs tested

                                Thread _tSamplingControl = new Thread(SamplingControl);
                                _tSamplingControl.IsBackground = true;
                                _tSamplingControl.Start();



                                SetStopMachineStatus();

                            }
                            else
                            {
                                if (TestedDUT >= Convert.ToInt32(ul.GetValueByKey("TestedDUT") + 50))
                                {
                                    SetStopMachineStatus();
                                }
                            }
                        }

                        event_log(ul.GetValueByKey("SN").Trim() + ": DelayCall: " + delayCall + " > RestestRate/YeildRate: " + RTRStopSpec + "/" + YRStopSpec + " > UseSpecMode: " + UseSpecMode.ToString() + " > globalStation: " + globalStation + " Shift: " + Shift + " > " + ModelNameChangeATE + " > TestedDUT: " + TestedDUT + " > BufferDUT: " + BufferDUT);

                        //Auto Stop Test Line
                        // end check autostop when testflag =1
                    }
                    if (_StationKey.GetValue("SPCCHARTFLAG", "0").ToString().Equals("1"))
                    {
                        if (File.Exists("SPCChart.exe"))
                        {
                            if (System.Diagnostics.Process.GetProcessesByName("SPCChart").Length == 0)
                                System.Diagnostics.Process.Start("SPCChart.exe");
                        }
                    }

                    // check autostop following timer
                    //update RR-YR
                    // check condition every time|| GetLineOfTester().Trim() == "L"|| GetLineOfTester().Trim() == "L"

                    string tmpTimeCheck = DateTime.Now.ToString("HHmm");
                    int TimeCheck = Convert.ToInt32(tmpTimeCheck);


                    // Reset every 7:30 && 19:30
                    if ((DateTime.Now.Hour == 7 && DateTime.Now.Minute == 31 && DateTime.Now.Second <= 12) || (DateTime.Now.Hour == 19 && DateTime.Now.Minute == 31 && DateTime.Now.Second <= 12))
                    {
                        ul.SetValueByKey("StartDUT", "0");
                        ul.SetValueByKey("QtybyTester", "");
                        ul.SetValueByKey("ShowUIData", "");
                        ul.SetValueByKey("iFPR", "");
                        ul.SetValueByKey("ERRORCODE_LIST", "");
                        ul.SetValueByKey("PASS", "");
                        ul.SetValueByKey("ErrStation", "");
                        //string TerminatedShowUI = "taskkill /IM ShowUI.exe /T /F";
                        event_log("TestedDUT:" + Convert.ToString(TestedDUT) + "->Reset StartDUT with kill ShowUI");
                        //ShellExecute(TerminatedShowUI);
                        ul.ResetArsSystem(connectionStringSrv37, ConnectServer37);
                        Application.Exit();
                    }
                    if ((DateTime.Now.Hour == 8 && DateTime.Now.Minute == 31 && DateTime.Now.Second <= 12) || (DateTime.Now.Hour == 20 && DateTime.Now.Minute == 31 && DateTime.Now.Second <= 12))
                    {
                        // delete sanderpatrick row
                        ul.ResetArsSystem(connectionStringSrv37, ConnectServer37);
                    }

                    // Reset TestedDUT every shift
                    if ((TimeCheck >= 750 && TimeCheck < 751) || (TimeCheck >= 1950 && TimeCheck < 1951))
                    {
                        try
                        {


                            if (DateTime.Now.Second < 12)
                            {
                                RegistryKey kiwi = Registry.LocalMachine.OpenSubKey(subkey, true);

                                kiwi.SetValue("StopStation", "tmpDefault", RegistryValueKind.String);
                                kiwi.SetValue("PcRun", "0", RegistryValueKind.String);
                                ul.SetValueByKey("ERRORCODE", "");
                                ul.SetValueByKey("BRTRate", "0");
                                ul.SetValueByKey("BYRate", "0");
                                ul.SetValueByKey("CountError", "");
                                ul.SetValueByKey("CountSameError", "");

                                ul.SetValueByKey("StopMachine", "0");
                                ul.SetValueByKey("Testtime", "0:55");
                                //ul.SetValueByKey("StopStation", "tmpDefault");
                                ul.SetValueByKey("TestedDUT", "50");
                                ul.SetValueByKey("StartDUT", "0");
                                ul.SetValueByKey("QtybyTester", "");
                                ul.SetValueByKey("ShowUIData", "");
                                ul.SetValueByKey("ErrStation", "");
                                //ul.SetValueByKey("iFPR", "");
                                //ShowUI.GET_ATE_SUM_TESTED_DUT.Servicepostdata getSumTestedDUT = new ShowUI.GET_ATE_SUM_TESTED_DUT.Servicepostdata();
                                TestedDUT = sfisB05.GET_STATION_PASS_FAIL(ul.GetModel().Trim(), sName);
                                event_log("Reset all Registries-->Current Tested DUT:" + Convert.ToString(TestedDUT));
                            }

                        }
                        catch (Exception r)
                        {
                            event_log("Reset all Registries: " + r.ToString());
                            // MessageBox.Show(lala.ToString());
                        }
                    }

                    // //)  || GetLineOfTester().Trim() == "L"  || GetLineOfTester().Trim() == "L"

                    if (ul.GetValueByKey("StartDUT") == "*" || GetWebUnlockPath("PcRun") == disableLockMachine)// Disable by Tester   || || GetLineOfTester().Trim() == "L"
                    {
                        // do nothing in case of fake^^
                        ul.SetValueByKey("StopMachine", "0");
                        if ((TimeCheck >= 750 && TimeCheck < 751) || (TimeCheck >= 1950 && TimeCheck < 1951))
                        {
                            if (DateTime.Now.Second < 12)
                            {
                                SetWebUnlockPath("PcRun", "0");
                                event_log("Reset PcRun-> 0" + Convert.ToString(TestedDUT));
                            }
                        }
                    }
                    else
                    {

                        if (ul.GetValueByKey("FixFlag") == "1")
                        {
                            CountTimeToFix++;
                            if (CountTimeToFix == 150)// 15' =150 cos timerinterval
                            {
                                CountTimeToFix = 0;
                                ul.SetValueByKey("FixFlag", "");
                                //this.Refresh();
                            }
                        }
                        // 23.3.2016 set StopMachine by station immediated ly
                        //disable process has many by lucifer 28.9.2020
                        //waiting new sv sql
                        //Thread _tGetStopMachineStatus = new Thread(GetStopMachineStatus);
                        //_tGetStopMachineStatus.IsBackground = true;  
                        //_tGetStopMachineStatus.Start();


                        SetStopMachineStatusImediately();
                    }
                    // end check autostop following timer
                }

            }
            catch (Exception r)
            {
                event_log("UpdateYR: " + r.ToString());
            }

            UpdateYR.Enabled = true;
            //}//
        }

        //bool flagCommander = true;
        //public void AutoNotice()
        //{
        //    try
        //    {

        //        if (TestedDUT == 10)
        //        {
        //            //ShowUI.B05_SERVICE_CENTER.B05_Service sv = new ShowUI.B05_SERVICE_CENTER.B05_Service();
        //            ToDB conn = new ToDB();
        //            string tempKey = "";
        //            string SFISDATASTRING = "";
        //            try
        //            {
        //                RegistryKey kiwi = Registry.LocalMachine.OpenSubKey(subkey, true);
        //                string[] _Key = kiwi.GetValue("OpenKey", null).ToString().Split('\\');
        //                string _Model = _Key[1];
        //                string _Sta = _Key[2];
        //                kiwi = Registry.LocalMachine.OpenSubKey(subkey + "\\" + _Model + "\\" + _Sta, true);
        //                tempKey = subkey + "\\" + _Model + "\\" + _Sta;
        //                SFISDATASTRING = kiwi.GetValue("SFISDATA", "").ToString();
        //            }
        //            catch (Exception)
        //            {
        //                // 
        //            }

        //            string checkExistSql = "select * from Status where model_name ='" + _model_name + "' and  line_name = '" + _line_name + "' and station_name = '" + _station_name + "' and  ate_name ='" + _ate_name + "' and ate_ip='" + _ate_ip + "'";
        //            DataTable dt = conn.DataTable_Sql(checkExistSql, serverIp);

        //            string err_desc = "";
        //            if (SFISDATASTRING == "")
        //            {
        //                err_desc = "string sfisdata is empty or dont exist";

        //                //MessageBox.Show(checkExistSql + "++" + serverIp);
        //                if (_line_name != "" && _model_name != "X")
        //                {
        //                    if (dt.Rows.Count != 0)
        //                    {
        //                        string updateSql = "update Status set model_name ='" + _model_name + "', line_name = '" + _line_name + "', station_name = '" + _station_name + "' , ate_name ='" + _ate_name + "', ate_ip='" + _ate_ip + "', date_time = '" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "',stop_status ='0',stop_ate = null,bug_record='" + err_desc + "',reg_path='" + tempKey + "'";
        //                        updateSql += " where model_name ='" + _model_name + "' and  line_name = '" + _line_name + "' and station_name = '" + _station_name + "' and  ate_name ='" + _ate_name + "' and ate_ip='" + _ate_ip + "'";

        //                        conn.Execute_NonSQL(updateSql, serverIp);
        //                        //event_log("Auto_MO: updateSql -> " + updateSql);
        //                    }
        //                    else
        //                    {
        //                        string insertSql = "insert into Status(mo_status,stop_status,model_name,line_name,station_name,ate_name,ate_ip,date_time,bug_record,reg_path) ";
        //                        insertSql += " values(0,'0','" + _model_name + "','" + _line_name + "','" + _station_name + "','" + _ate_name + "','" + _ate_ip + "','" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "','" + err_desc + "','" + tempKey + "')";

        //                        conn.Execute_NonSQL(insertSql, serverIp);
        //                        //event_log("Auto_MO: insertSql -> " + insertSql);
        //                    }
        //                }
        //            }

        //            if (!checkTestFlag && flagCommander == true)
        //            {
        //                flagCommander = false;
        //                err_desc = "dont update testflag = 1";
        //                if (_line_name != "" && _model_name != "X")
        //                {
        //                    if (dt.Rows.Count != 0)
        //                    {
        //                        string updateSql = "update Status set model_name ='" + _model_name + "', line_name = '" + _line_name + "', station_name = '" + _station_name + "' , ate_name ='" + _ate_name + "', ate_ip='" + _ate_ip + "', date_time = '" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "',stop_status ='0',stop_ate = null,bug_record='" + err_desc + "',reg_path='" + tempKey + "'";
        //                        updateSql += " where model_name ='" + _model_name + "' and  line_name = '" + _line_name + "' and station_name = '" + _station_name + "' and  ate_name ='" + _ate_name + "' and ate_ip='" + _ate_ip + "'";

        //                        conn.Execute_NonSQL(updateSql, serverIp);
        //                        //MessageBox.Show(updateSql);
        //                    }
        //                    else
        //                    {
        //                        string insertSql = "insert into Status(mo_status,stop_status,model_name,line_name,station_name,ate_name,ate_ip,date_time,bug_record,reg_path) ";
        //                        insertSql += " values(0,'0','" + _model_name + "','" + _line_name + "','" + _station_name + "','" + _ate_name + "','" + _ate_ip + "','" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "','" + err_desc + "','" + tempKey + "')";

        //                        conn.Execute_NonSQL(insertSql, serverIp);
        //                        //MessageBox.Show(insertSql);
        //                    }
        //                    //
        //                }
        //            }

        //            string CtrTimePath = GetCableCtrlTimesPath();
        //            err_desc = "CableCtrlTime.ini file not exist " + CtrTimePath;
        //            int nCable = 0;
        //            if (File.Exists(CtrTimePath))
        //            {
        //                Int32.TryParse(IniFile.ReadIniFile("ConnectControl", "TotalCable ", null, CtrTimePath), out nCable);
        //            }
        //            else
        //            {
        //                if (_line_name != "" && _model_name != "X")
        //                {
        //                    if (dt.Rows.Count != 0)
        //                    {
        //                        string updateSql = "update Status set model_name ='" + _model_name + "', line_name = '" + _line_name + "', station_name = '" + _station_name + "' , ate_name ='" + _ate_name + "', ate_ip='" + _ate_ip + "', date_time = '" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "',stop_status ='0',stop_ate = null,bug_record='" + err_desc + "',reg_path='" + tempKey + "'";
        //                        updateSql += " where model_name ='" + _model_name + "' and  line_name = '" + _line_name + "' and station_name = '" + _station_name + "' and  ate_name ='" + _ate_name + "' and ate_ip='" + _ate_ip + "'";

        //                        conn.Execute_NonSQL(updateSql, serverIp);
        //                        //MessageBox.Show(updateSql);
        //                    }
        //                    else
        //                    {
        //                        string insertSql = "insert into Status(mo_status,stop_status,model_name,line_name,station_name,ate_name,ate_ip,date_time,bug_record,reg_path) ";
        //                        insertSql += " values(0,'0','" + _model_name + "','" + _line_name + "','" + _station_name + "','" + _ate_name + "','" + _ate_ip + "','" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "','" + err_desc + "','" + tempKey + "')";

        //                        conn.Execute_NonSQL(insertSql, serverIp);
        //                        //MessageBox.Show(insertSql);
        //                    }
        //                }//
        //            }
        //            //
        //        }// end if
        //    }
        //    catch (Exception e)
        //    {
        //        event_log("AutoNotice Exception: "+ e.ToString());
        //    }
        //}

        private void lblYeildRate_TextChanged(object sender, EventArgs e)
        {
            float yeildRate = Convert.ToSingle(lblYeildRate.Text.Replace("%", null));

            if (yeildRate > YRGreen)
            {
                lblYeildRate.BackColor = Color.Lime;
                lblYeildRate.ForeColor = Color.Blue;
            }
            else if (yeildRate > YRYellow)
            {
                lblYeildRate.BackColor = Color.Yellow;
                lblYeildRate.ForeColor = Color.Red;
            }
            else
            {
                lblYeildRate.BackColor = Color.Red;
                lblYeildRate.ForeColor = Color.White;
            }
        }

        private void lblTotalRate_TextChanged(object sender, EventArgs e)
        {

            if (npiflag == true)
            {
                if (showRealData == true)
                {
                    //for npi only
                    lblTotalRate.BackColor = Color.Lime;
                    lblTotalRate.ForeColor = Color.Blue;
                }

            }
            else
            {
                float totalRate = Convert.ToSingle(lblTotalRate.Text.Replace("%", null));
                if (totalRate < SRRGreen)
                {
                    lblTotalRate.BackColor = Color.Lime;
                    lblTotalRate.ForeColor = Color.Blue;
                }
                else if (totalRate < SRRYellow)
                {
                    lblTotalRate.BackColor = Color.Yellow;
                    lblTotalRate.ForeColor = Color.Red;
                }
                else
                {
                    lblTotalRate.BackColor = Color.Red;
                    lblTotalRate.ForeColor = Color.White;
                }
            }

        }

        private void lblRetestRate_TextChanged(object sender, EventArgs e)
        {
            float RetestRate = Convert.ToSingle(lblRetestRate.Text.Replace("%", null));
            if (npiflag == true && showRealData == true)
            {
                //for npi only
                lblRetestRate.BackColor = Color.Lime;
                lblRetestRate.ForeColor = Color.Blue;
            }
            else
                if (RetestRate < TRRGreen)
            {
                lblRetestRate.BackColor = Color.Lime;
                lblRetestRate.ForeColor = Color.Blue;
            }
            else if (RetestRate < TRRYellow)
            {
                lblRetestRate.BackColor = Color.Yellow;
                lblRetestRate.ForeColor = Color.Red;
            }
            else
            {
                lblRetestRate.BackColor = Color.Red;
                lblRetestRate.ForeColor = Color.White;
            }

        }
        public void writeSpcHeader(string filePath)
        {
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.Write("Chart Heading:    Statistical Process Control" + Environment.NewLine +
                    "Company:" + Environment.NewLine + "Lister:" + Environment.NewLine);
                sw.WriteLine("Report Date:" + DateTime.Now.ToString("yyyy-MM-dd"));
                sw.WriteLine("LINE:" + fSPCspec.ReadString("SPCHead", "LINE"));
                sw.WriteLine("Product:" + fSPCspec.ReadString("SPCHead", "MODEL"));
                //sw.WriteLine("Station: " + sType);
                if (sType.Contains("PT"))
                    sw.WriteLine("Station: PT");
                if (sType.Equals("FT"))
                    sw.WriteLine("Station: NETGEAR FT");
                if (sType.Equals("FT1"))
                    sw.WriteLine("Station: FT1");
                if (sType.Equals("NFT"))
                    sw.WriteLine("Station: NFT");
                if (sType.Equals("RI"))
                    sw.WriteLine("Station: RI");
                sw.WriteLine("Sample Code:    20");
                if (sType.Contains("PT"))
                    sw.WriteLine("Units:    dBm");
                if (sType.Equals("NFT"))
                    sw.WriteLine("Units:   dBm");
                if (sType.Equals("FT"))
                    sw.WriteLine("Units:   Mbps");
                if (sType.Equals("FT1"))
                    sw.WriteLine("Units:   Mbps");
                if (sType.Equals("RI"))
                    sw.WriteLine("Units:   Mbps");
                sw.WriteLine("Upper Limit:" + Environment.NewLine + "Specification:" + Environment.NewLine +
                    "Lower Limit:" + Environment.NewLine + "Start Date:" + Environment.NewLine +
                    "Start Time:" + Environment.NewLine + "Up-to Date:" + Environment.NewLine +
                    "Up-to Time:" + Environment.NewLine + "Effective Decimal 4" + Environment.NewLine +
                    "Type of Export:   Export Datasheet to Text" + Environment.NewLine);
                sw.WriteLine("Item_Qty:" + fSPCspec.ReadString("SPCHead", "Item_Qty"));
                sw.WriteLine("Record_Qty:" + fSPCspec.ReadString("SPCHead", "Record_Qty"));
                sw.WriteLine(fSPCspec.ReadString("SPCHead", sType + "Head"));
                //if (sType.Contains("PT")) sw.WriteLine(fSPCspec.ReadString("SPCHead", "PTHead"));
                //if (sType.Equals("FT")) sw.WriteLine(fSPCspec.ReadString("SPCHead", "FTHead"));
                //if (sType.Equals("NFT")) sw.WriteLine(fSPCspec.ReadString("SPCHead", "NFTHead"));
            }
        }

        public void ClearRegistry()
        {

        }
        private void btnCall_Click(object sender, MouseEventArgs e)
        {
            using (ShowUI.frmEmpAuthentication ea = new ShowUI.frmEmpAuthentication("2"))
            {

                ea.ShowDialog();
            }
            //using (WarningButton wb = new WarningButton())
            //{
            //    if (wb.ShowDialog() == DialogResult.OK)
            //    {
            //        using (SqlConnection connection = new SqlConnection(connectionString))
            //        {
            //            try
            //            {
            //                connection.Open();
            //                SqlDataReader reader;
            //                string queryString = "SELECT Users FROM tblUser WHERE Users = @ur AND PASS=@pw";
            //                SqlCommand command = new SqlCommand(queryString, connection);
            //                command.Parameters.Add("@ur", SqlDbType.NVarChar, 50);
            //                command.Parameters["@ur"].Value = wb.getUsername();
            //                command.Parameters.Add("@pw", SqlDbType.NVarChar, 50);
            //                command.Parameters["@pw"].Value = wb.getPassword();
            //                reader = command.ExecuteReader();
            //                if (!reader.HasRows)
            //                {
            //                    reader.Close();
            //                    MessageBox.Show("Sai tên đăng nhập hoặc mật khẩu !", "Jerry", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //                }
            //                else
            //                {
            //                    queryString = "SELECT ID FROM Abnormal WHERE STATION_NAME =@STATION_NAME AND SOLVED = 0";
            //                    command = new SqlCommand(queryString, connection);
            //                    command.Parameters.Add("@STATION_NAME", SqlDbType.NVarChar, 50);
            //                    command.Parameters["@STATION_NAME"].Value = sName;
            //                    reader.Close();
            //                    reader = command.ExecuteReader();
            //                    if (!reader.HasRows)
            //                    {
            //                        reader.Close();
            //                        string error_detail = _StationKey.GetValue("ERRORCODE", "").ToString().Trim();
            //                        queryString = "INSERT INTO Abnormal (STATION_NAME,SOLVED,ERROR,DATE_TIME_START,USER_CALL) " +
            //                            "VALUES (@STATION,0,@ERROR,getdate(),@USERCALL)";
            //                        command = new SqlCommand(queryString, connection);
            //                        command.Parameters.Add("@ERROR", SqlDbType.NVarChar, 128);
            //                        command.Parameters.Add("@STATION", SqlDbType.NVarChar, 50);
            //                        command.Parameters.Add("@USERCALL", SqlDbType.NVarChar, 50);
            //                        command.Parameters["@STATION"].Value = sName;
            //                        command.Parameters["@ERROR"].Value = error_detail;
            //                        command.Parameters["@USERCALL"].Value = wb.getUsername();
            //                        command.ExecuteNonQuery();
            //                    }
            //                    MessageBox.Show("Already call TE !" + Environment.NewLine + "Đã thông báo cho TE, thanks !", "Jerry", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //                    connection.Close();
            //                }
            //            }
            //            catch (Exception exp)
            //            {
            //                //MessageBox.Show("Can't connect to server !","Error",MessageBoxButtons.OK,MessageBoxIcon.Stop);
            //                event_log("Call TE: " + exp.Message.ToString());
            //            }
            //        }
            //    }
            //}
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


        private void update_station_info()
        {

            if (ConnectServer37 == "0" && NetWorkConnection == true)
            {
                using (SqlConnection connection = new SqlConnection(connectionStringSrv37))
                {
                    try
                    {
                        connection.Open();
                        SqlDataReader reader;
                        // 18.9.2015 change to check by ip
                        string queryString = "SELECT STATION_IP FROM StationInfo WHERE STATION_IP=@IP";
                        SqlCommand command = new SqlCommand(queryString, connection);
                        command.Parameters.Add("@IP", SqlDbType.NVarChar, 50);
                        command.Parameters["@IP"].Value = client_ip;

                        string line = sName.Substring(sName.IndexOf("L") + 1, sName.Length - sName.IndexOf("L") - 1);
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
                            tmpLine = Convert.ToInt32(testline);
                        }

                        if (tmpLine == 0)
                        {
                            testline = "";
                        }

                        if (testline.StartsWith("0"))
                        {
                            testline = testline.Replace("0", "");
                        }

                        reader = command.ExecuteReader();
                        if (reader.HasRows)
                        {

                            //MessageBox.Show("Co du lieu");
                            reader.Close();
                            if (testline != "" && sType != "")
                            {

                                queryString = "UPDATE StationInfo SET STATION_NAME=@STATION,MAC_ADDR=@MAC_ADDR,LINE=@LINE,TYPE=@TYPE WHERE STATION_IP =@IP";
                                command = new SqlCommand(queryString, connection);
                                command.Parameters.Add("@IP", SqlDbType.NVarChar, 50);
                                command.Parameters.Add("@MAC_ADDR", SqlDbType.NVarChar, 50);
                                command.Parameters.Add("@LINE", SqlDbType.VarChar, 10);
                                command.Parameters.Add("@STATION", SqlDbType.NVarChar, 50);
                                command.Parameters.Add("@TYPE", SqlDbType.NVarChar, 10);
                                command.Parameters["@IP"].Value = client_ip;
                                command.Parameters["@MAC_ADDR"].Value = client_mac;
                                command.Parameters["@LINE"].Value = testline;
                                command.Parameters["@STATION"].Value = sName;
                                command.Parameters["@TYPE"].Value = sType;

                                command.ExecuteNonQuery();

                            }
                        }
                        else
                        {
                            //MessageBox.Show("K co du lieu");
                            reader.Close();
                            if (testline != "" && sType != "")
                            {

                                queryString = "INSERT INTO StationInfo (STATION_NAME,STATION_IP,MAC_ADDR,LINE,TYPE) VALUES (@STATION,@IP,@MAC_ADDR,@LINE,@TYPE)";
                                command = new SqlCommand(queryString, connection);
                                command.Parameters.Add("@IP", SqlDbType.NVarChar, 50);
                                command.Parameters.Add("@MAC_ADDR", SqlDbType.NVarChar, 50);
                                command.Parameters.Add("@LINE", SqlDbType.VarChar, 10);
                                command.Parameters.Add("@STATION", SqlDbType.NVarChar, 50);
                                command.Parameters.Add("@TYPE", SqlDbType.NVarChar, 10);
                                command.Parameters["@IP"].Value = client_ip;
                                command.Parameters["@MAC_ADDR"].Value = client_mac;
                                command.Parameters["@LINE"].Value = testline;
                                command.Parameters["@STATION"].Value = sName;
                                command.Parameters["@TYPE"].Value = sType;
                                command.ExecuteNonQuery();

                            }
                        }

                        connection.Close();
                    }
                    catch (Exception exp)
                    {
                        //MessageBox.Show();
                        event_log("Update Stationinfo: " + client_ip + "--" + client_mac + "--" + GetLineOfTester() + "--" + sName + "--" + sType + "--->" + exp.Message.ToString());
                    }
                }

            } // end if connectserver37

        }

        private void lblShowUI_DoubleClick(object sender, EventArgs e)
        {
            if (this.Width < 60)
            {
                int widthsc = Screen.PrimaryScreen.WorkingArea.Width;
                this.Size = new Size(widthsc, 60);
                btnCall.SetBounds(widthsc - 88, 32, 83, 24);
                pBoxDebug.Visible = true;
            }
            else
            {
                this.Size = new Size(37, 30);
                pBoxDebug.Visible = false;
                toolTipVirus.Hide(pbVirus);
                toolTipUSB.Hide(pbUsb);
            }
            xxx = "";
            running = false;
        }

        private void lblTotalRate_Click(object sender, EventArgs e)
        {
            xxx = xxx + "#";
        }

        private void lblRetestRate_Click(object sender, EventArgs e)
        {
            xxx = xxx + "FLY";
        }

        private void chartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                _Chart.Show();
            }
            catch
            {
                _Chart = new frmChart();
                _Chart.Show();
            }
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                rightMenu.Show(e.X, e.Y);
            }
            else
            {
                this.Hide();
            }
        }


        private void helpMenuItem_Click(object sender, EventArgs e)
        {

            //if (File.Exists(@"F:\lsy\Test\DownloadConfig\AutoDL\Help\index.html"))
            //{
            //    System.Diagnostics.Process.Start(@"F:\lsy\Test\DownloadConfig\AutoDL\Help\index.html");
            //}
            //else
            //{
            //    MessageBox.Show("Comming soon !", "Jerry", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            //}
        }



        private void showUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            //this.Visible = false;
            //Thread.Sleep(10000);
            //this.TopMost = false;
            //try
            //{
            //    ShowUI.SvFPS.WebService svStatus = new ShowUI.SvFPS.WebService();
            //    svStatus.UpdateLockStatus(sName, "0");
            //}
            //catch (Exception)
            //{

            //    //MessageBox.Show(eddd.ToString());
            //}

            //e.Cancel = true;

            //this.Visible = true;

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Application.ExitThread();
        }

        public bool ExecuteCommand(object command)
        {
            try
            {
                // create the ProcessStartInfo using "cmd" as the program to be run,
                // and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows,
                // and then exit.
                System.Diagnostics.ProcessStartInfo procStartInfo =
                    new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command);

                // The following commands are needed to redirect the standard output.
                // This means that it will be redirected to the Process.StandardOutput StreamReader.
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.RedirectStandardError = true;
                procStartInfo.UseShellExecute = false;
                // Do not create the black window.
                procStartInfo.CreateNoWindow = true;
                // Now we create a process, assign its ProcessStartInfo and start it
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();
                // Get the output into a string
                string result = proc.StandardOutput.ReadToEnd();
                // Display the command output.
                string error = proc.StandardError.ReadToEnd();
                if (error.Length == 0)
                    return true;
            }
            catch
            {
                //deo lam gi ca
            }
            return false;
        }


        // Auto Stop Tester


        public void SetRegistries()
        {
            try
            {
                RegistryKey kiwi = Registry.LocalMachine.OpenSubKey(subkey, true);

                kiwi.SetValue("StopStation", "tmpDefault", RegistryValueKind.String);
                kiwi.SetValue("PcRun", "0", RegistryValueKind.String);

                if (kiwi.GetValue("E_GROUP_ID") == null || kiwi.GetValue("E_GROUP_ID").ToString() == "")
                {
                    kiwi.SetValue("E_GROUP_ID", "E_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"), RegistryValueKind.String);
                }
                if (kiwi.GetValue("I_VERSION_ID") == null || kiwi.GetValue("I_VERSION_ID").ToString() == "")
                {
                    kiwi.SetValue("I_VERSION_ID", "IV0.0.0.1", RegistryValueKind.String);
                }

                //string[] _Key = kiwi.GetValue("OpenKey", null).ToString().Split('\\');
                string _Model = ul.GetProduct();
                string _Sta = ul.GetStation();
                kiwi = Registry.LocalMachine.OpenSubKey(subkey + "\\" + _Model + "\\" + _Sta, true);
                //if (kiwi.GetValue("SN") == null)
                //{
                kiwi.SetValue("HOTFIX", "", RegistryValueKind.String);

                kiwi.SetValue("CountError", "", RegistryValueKind.String);
                kiwi.SetValue("CountSameError", "", RegistryValueKind.String);

                kiwi.SetValue("SN", "DEFAULT", RegistryValueKind.String);

                kiwi.SetValue("MO", "", RegistryValueKind.String);
                //string tmpSFIS = .ToString();

                if (kiwi.GetValue("SFISDATA") == null || kiwi.GetValue("SFISDATA").ToString() == "")
                {
                    kiwi.SetValue("SFISDATA", "RR-YRXXXXX-XXXXXXXX           XX-XXXXX-XXX002.17000.00099.83PASS", RegistryValueKind.String);
                }


                kiwi.SetValue("ERRORCODE", "", RegistryValueKind.String);
                kiwi.SetValue("iFPR", "", RegistryValueKind.String);
                kiwi.SetValue("ErrStation", "", RegistryValueKind.String);
                kiwi.SetValue("iCMTS", "", RegistryValueKind.String);
                kiwi.SetValue("ShowUIdata", "", RegistryValueKind.String);
                //}
                //if (kiwi.GetValue("BufferYRate") == null)
                //{
                kiwi.SetValue("BYRate", "0", RegistryValueKind.String);
                // }
                // if (kiwi.GetValue("BufferRTRate") == null)
                //{
                kiwi.SetValue("BRTRate", "0", RegistryValueKind.String);
                // }
                // if (kiwi.GetValue("StopMachine") == null)
                // {
                //kiwi.SetValue("StopMachine", "0", RegistryValueKind.String);
                // }
                if (kiwi.GetValue("Testtime") == null || kiwi.GetValue("Testtime").ToString() == "")
                {
                    kiwi.SetValue("Testtime", "00:55", RegistryValueKind.String);
                }

                //if (kiwi.GetValue("StartFlag") == null)
                //{
                kiwi.SetValue("StartFlag", "0", RegistryValueKind.String);
                // }

                if (kiwi.GetValue("StartDUT") == null)
                {
                    kiwi.SetValue("StartDUT", "0", RegistryValueKind.String);
                }

                //kiwi.SetValue("StopStation", "tmpDefault", RegistryValueKind.String);
                //kiwi.SetValue("Lock", "1", RegistryValueKind.String);
                kiwi.SetValue("TestedDUT", "50", RegistryValueKind.String);

                //if (kiwi.GetValue("FixFlag") == null)
                //{
                kiwi.SetValue("FixFlag", "", RegistryValueKind.String);
                //}


                kiwi.SetValue("TestTimeHighLimit", Convert.ToString(TestTimeHighLimit), RegistryValueKind.String);

                kiwi.SetValue("TestTimeLowLimit", Convert.ToString(TestTimeLowLimit), RegistryValueKind.String);


            }
            catch (Exception)
            {

                //throw;
            }

        }
        public string GetWebUnlockPath(string _key)
        {
            try
            {
                RegistryKey kiwi = Registry.LocalMachine.OpenSubKey(subkey, true);
                string SN = kiwi.GetValue(_key, "").ToString().Trim();
                return SN;

            }
            catch (Exception)
            {
                return "";
                //throw;
            }
        }
        public void SetWebUnlockPath(string _reg, string _val)
        {
            try
            {

                RegistryKey kiwi = Registry.LocalMachine.OpenSubKey(subkey, true);
                kiwi.SetValue(_reg, _val, RegistryValueKind.String);
                //MessageBox.Show(_reg + _val);

            }
            catch (Exception)
            {
                //return "X";
                //throw;
            }
        }


        string CountError = "";
        string CountSameError = "";
        string TmpCountErr = "";
        string CurrentDUT = "";
        int FlagSameErrorCode = 0;
        int FlagErrorCode = 0;
        bool _IsExistErrorr = false;

        public void ResetRegistries()
        {

        }

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
                    tmpLine = Convert.ToInt32(testline);
                }
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

        // update DB To stopall


        public string GetTypeOfManufacturing()
        {
            string _type = "MP";
            try
            {
                if (ConnectServer60 == "0" && NetWorkConnection == true)
                {
                    SqlConnection connection = new SqlConnection(connSrv60);
                    connection.Open();
                    SqlDataReader reader;
                    string sqlStr = @"select TYPE from tblTypeOfProduct where LINE='" + GetLineOfTester() + "' and CurrentModel ='" + ul.GetModel().Trim() + "'";
                    //event_log(sqlStr);
                    SqlCommand cmd = new SqlCommand(sqlStr, connection);
                    SqlDataAdapter da = new SqlDataAdapter(sqlStr, connSrv60);
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        // get type of producing
                        _type = ds.Tables[0].Rows[0]["TYPE"].ToString();
                    }
                    reader.Close();
                    connection.Close();
                }// end if ConnectServer60
            }
            catch (Exception)
            {
            }
            return _type;
        }

        public void UpdateSpecByLine()
        {
            try
            {
                if (ConnectServer60 == "0" && NetWorkConnection == true)
                {
                    string ModelName = ul.GetProduct().Trim();
                    string C_ModelName = ul.GetModel().Trim();
                    string RTRate = "";
                    string YRate = "";
                    SqlConnection connection = new SqlConnection(connSrv60);
                    connection.Open();
                    SqlDataReader reader;
                    string sqlStr = @"select * from tblSpecByModel where C_ModelName='" + C_ModelName + "'";
                    SqlCommand cmd = new SqlCommand(sqlStr, connection);
                    SqlDataAdapter da = new SqlDataAdapter(sqlStr, connSrv60);
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        reader.Close();
                        connection.Close();
                        connection.Open();
                        RTRate = ds.Tables[0].Rows[0]["RTRate"].ToString();
                        YRate = ds.Tables[0].Rows[0]["YRate"].ToString();

                        string UpdateSQL = "update tblTypeOfProduct set TmpRTRate=@RTRate, TmpYRate=@YRate, CurrentModel=@CModelName,SpecStatus=@Status where Line=@Line";

                        SqlCommand command = new SqlCommand(UpdateSQL, connection);
                        command.Parameters.Add("@RTRate", SqlDbType.NVarChar, 50);
                        command.Parameters.Add("@YRate", SqlDbType.NVarChar, 50);
                        command.Parameters.Add("@CModelName", SqlDbType.NVarChar, 50);
                        command.Parameters.Add("@Status", SqlDbType.NVarChar, 50);
                        command.Parameters.Add("@Line", SqlDbType.NVarChar, 50);

                        command.Parameters["@RTRate"].Value = RTRate;
                        command.Parameters["@YRate"].Value = YRate;
                        command.Parameters["@CModelName"].Value = C_ModelName;
                        command.Parameters["@Status"].Value = "1";
                        command.Parameters["@Line"].Value = GetLineOfTester().Trim();

                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                    else
                    {
                        reader.Close();
                        connection.Close();
                        connection.Open();

                        string insertNew = "insert into tblSpecByModel (ModelName,RTRate,YRate,C_ModelName) values(@_model,@_retestRate,@_yeidlRate,@_cModel)";
                        SqlCommand commandInsert = new SqlCommand(insertNew, connection);
                        commandInsert.Parameters.Add("@_model", SqlDbType.NVarChar, 50);
                        commandInsert.Parameters.Add("@_retestRate", SqlDbType.NVarChar, 50);
                        commandInsert.Parameters.Add("@_yeidlRate", SqlDbType.NVarChar, 50);
                        commandInsert.Parameters.Add("@_cModel", SqlDbType.NVarChar, 50);

                        commandInsert.Parameters["@_model"].Value = ModelName;
                        commandInsert.Parameters["@_retestRate"].Value = "50";
                        commandInsert.Parameters["@_yeidlRate"].Value = "50";
                        commandInsert.Parameters["@_cModel"].Value = C_ModelName;
                        commandInsert.ExecuteNonQuery();
                        connection.Close();
                    }
                    connection.Close();
                } //end if connecttoServer60
            }
            catch (Exception r)
            {
                event_log(r.ToString());
                //MessageBox.Show(er.ToString());
            }


        }

        public string GetStopmachineSpec(double inputPCS, string type)
        {
            string rturnValue = "";
            if (type.Contains("YRate"))
                rturnValue = "0";
            if (type.Contains("RTRate"))
                rturnValue = "101";
            string flagPos = "";
            try
            {
                //globalUsedMode = "2";
                //TestedDUT = 400;
                //MessageBox.Show(_line_name);
                //_model_name = "C7000-100NASV1";
                //sName = "B05-L13-PT01";
                //client_ip = "10.224.84.171";
                // if testedDUT less than and equal to 10 pcs, dont care

                if (inputPCS > 10)
                {
                    //globalUsedMode = "2";
                    //sName = "B05-L13-PT01";
                    flagPos = "inputPCS > 10";
                    string linename = GetLineOfTester().Trim();
                    SqlConnection connection = new SqlConnection(connSrv60);
                    connection.Open();
                    SqlDataReader reader;
                    //globalUsedMode = "2";
                    //string sqlStr = @"select * from tblStopMachineSpec join tblTypeOfProduct on tblStopMachineSpec.Type = tblTypeOfProduct.Type where tblTypeOfProduct.Line='" + GetLineOfTester() + "'";
                    string sqlStr = "";
                    if (globalUsedMode == "1")
                    {
                        //sqlStr = @"select UsedMode,TmpRTRate,TmpYRate,LowerNumDUT,UpperNumDUT,RTRSpec,YRSpec from tblTypeOfProductExt join tblTypeOfProduct on tblTypeOfProductExt.LineName = tblTypeOfProduct.Line where tblTypeOfProduct.Line='" + linename + "' and " + TestedDUT + " > tblTypeOfProductExt.LowerNumDUT and " + TestedDUT + " <=tblTypeOfProductExt.UpperNumDUT"; 
                        // mode = 1 is
                        sqlStr += "select UsedMode,TmpRTRate,TmpYRate,LowerNumDUT,UpperNumDUT,RTRSpec,YRSpec";
                        sqlStr += " from tblTypeOfProductExt a,tblTypeOfProduct b";
                        sqlStr += " where a.LineName = b.Line and b.Line='" + linename + "' ";
                        sqlStr += " and " + inputPCS + " > a.LowerNumDUT and " + inputPCS + " <=a.UpperNumDUT";
                        sqlStr += " union all ";
                        sqlStr += "select UsedMode,TmpRTRate,TmpYRate,LowerNumDUT,UpperNumDUT,RTRSpec,YRSpec ";
                        sqlStr += " from tblTypeOfProductExt a, tblTypeOfProduct b ";
                        sqlStr += " where a.LineName = b.Line and b.Line='" + linename + "' ";
                        sqlStr += " and  a.UpperNumDUT = (select max(a.UpperNumDUT) from tblTypeOfProductExt a, tblTypeOfProduct b where a.LineName = b.Line and b.Line='" + linename + "')";

                    }
                    else if (globalUsedMode == "2")
                    {//B05-L13-PT08

                        //client_ip ="138.101.5.171";
                        //sName ="B5L1S1-FT1-4";
                        //_model_name = "VMC4030-FXN";
                        //sName = "FT1";
                        //sqlStr = @"select * from tblTypeOfProduct a, tblTypeOfProductDetail b where a.currentmodel = b.model_name and b.ate_name ='"+sName+"' and b.ate_ip='"+client_ip+"' and b.model_name='"+ul.GetModel().Trim()+"'";
                        sqlStr += "select LowerNumDUT,UpperNumDUT,RTRSpec,YRSpec from tblTypeOfProductDetail";
                        sqlStr += " where ate_name ='" + sName + "'";
                        sqlStr += " and ate_ip='" + client_ip + "'";
                        sqlStr += " and model_name='" + _model_name + "'";
                        sqlStr += " and lowerNumDUT < " + inputPCS + " and UpperNumDUT >= " + inputPCS + "";
                        sqlStr += " union all ";
                        sqlStr += "select LowerNumDUT,UpperNumDUT,RTRSpec,YRSpec from tblTypeOfProductDetail";
                        sqlStr += " where  ate_name ='" + sName + "'";
                        sqlStr += " and ate_ip='" + client_ip + "'";
                        sqlStr += " and model_name='" + _model_name + "'";
                        sqlStr += " and  UpperNumDUT = (select max(UpperNumDUT)";
                        sqlStr += " from tblTypeOfProductDetail";
                        sqlStr += " where ate_name ='" + sName + "'";
                        sqlStr += " and ate_ip='" + client_ip + "'";
                        sqlStr += " and model_name='" + _model_name + "')";
                        //ul.event_log(sqlStr);
                    }
                    else
                    {
                        sqlStr = @"select TmpRTRate,TmpYRate from  tblTypeOfProduct where line ='" + linename + "'";
                    }
                    //event_log(sqlStr);
                    SqlCommand cmd = new SqlCommand(sqlStr, connection);
                    SqlDataAdapter da = new SqlDataAdapter(sqlStr, connSrv60);
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        flagPos = "has rows with globalUsedMode" + globalUsedMode;
                        if (globalUsedMode == "2" || globalUsedMode == "1")
                        {
                            if (type.Contains("YRate"))
                                rturnValue = ds.Tables[0].Rows[0]["YRSpec"].ToString();
                            if (type.Contains("RTRate"))
                                rturnValue = ds.Tables[0].Rows[0]["RTRSpec"].ToString();
                        }
                        else
                        {
                            if (type.Contains("YRate"))
                                rturnValue = ds.Tables[0].Rows[0]["TmpYRate"].ToString();
                            if (type.Contains("RTRate"))
                                rturnValue = ds.Tables[0].Rows[0]["TmpRTRate"].ToString();
                        }
                    }
                    else
                    {
                        da.Dispose();
                        reader.Close();
                        cmd.Dispose();

                        // if globalUsedMode == 2 but dont exist, insert new
                        flagPos = "havent rows with globalUsedMode " + globalUsedMode;
                        if (globalUsedMode == "2")
                        {
                            string Model = ul.GetProduct();
                            string sqlGetSpec = "select LowerNumDUT,UpperNumDUT,RTRate,YRate,BufferDUT from tblStopMachineSpec order by LowerNumDUT asc";
                            cmd = new SqlCommand(sqlGetSpec, connection);
                            da = new SqlDataAdapter(sqlGetSpec, connSrv60);
                            ds = new DataSet();
                            da.Fill(ds);
                            reader = cmd.ExecuteReader();
                            if (reader.HasRows)
                            {
                                string[,] specValue = new string[ds.Tables[0].Rows.Count, ds.Tables[0].Columns.Count];
                                for (int j = 0; j < ds.Tables[0].Rows.Count; j++)
                                {
                                    for (int k = 0; k < ds.Tables[0].Columns.Count; k++)
                                    {
                                        specValue[j, k] = ds.Tables[0].Rows[j][k].ToString();
                                    }
                                }

                                cmd.Dispose();
                                reader.Close();
                                da.Dispose();

                                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                                {

                                    sqlStr = @"insert into tblTypeOfProductDetail(ate_ip,ate_name,station_name,date_time,lowerNumDUT,upperNumDUT,RTRSpec,YRSpec,model_name,BufferDUT)";
                                    sqlStr += "values(@_client_ip,@_sName,@_station,@_date_time,@_lowerDUT,@_upperDUT,@_RTRSpec,@_YRSpec,@_model,@_bufferDUT)";

                                    cmd = new SqlCommand(sqlStr, connection);//'" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")'" + _model_name + "'
                                    cmd.Parameters.Add("@_client_ip", SqlDbType.NVarChar, 50);
                                    cmd.Parameters.Add("@_sName", SqlDbType.NVarChar, 50);
                                    cmd.Parameters.Add("@_station", SqlDbType.NVarChar, 50);
                                    cmd.Parameters.Add("@_date_time", SqlDbType.DateTime);
                                    cmd.Parameters.Add("@_lowerDUT", SqlDbType.Int);
                                    cmd.Parameters.Add("@_upperDUT", SqlDbType.Int);
                                    cmd.Parameters.Add("@_RTRSpec", SqlDbType.NVarChar, 50);
                                    cmd.Parameters.Add("@_YRSpec", SqlDbType.NVarChar, 50);
                                    cmd.Parameters.Add("@_model", SqlDbType.NVarChar, 50);
                                    cmd.Parameters.Add("@_bufferDUT", SqlDbType.NVarChar, 50);

                                    cmd.Parameters["@_client_ip"].Value = client_ip;
                                    cmd.Parameters["@_sName"].Value = sName;
                                    cmd.Parameters["@_station"].Value = ul.GetStation().Trim();
                                    cmd.Parameters["@_date_time"].Value = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                                    cmd.Parameters["@_lowerDUT"].Value = Convert.ToInt32(specValue[i, 0]);
                                    cmd.Parameters["@_upperDUT"].Value = Convert.ToInt32(specValue[i, 1]);
                                    cmd.Parameters["@_RTRSpec"].Value = specValue[i, 2];
                                    cmd.Parameters["@_YRSpec"].Value = specValue[i, 3];
                                    cmd.Parameters["@_model"].Value = _model_name;
                                    cmd.Parameters["@_bufferDUT"].Value = specValue[i, 4];

                                    cmd.ExecuteNonQuery();
                                    cmd.Dispose();
                                }

                            }
                            else
                            { // 
                                da.Dispose();
                                reader.Close();
                            }

                        }//end if 

                    } //end if
                    cmd.Dispose();
                    da.Dispose();
                    reader.Close();
                    connection.Close();
                } // end if inpcs > 10
            }
            catch (Exception e)
            {
                event_log("GetStopmachineSpec Exception: " + e.ToString());
            }
            //AutoClosingMessageBox.Show("GetStopmachineSpec: UseSpecMode->" + UseSpecMode.ToString() + "  Retest Rate/Yeild Rate->" + RTRate + "/" + YRate, "sss", 5000);

            return rturnValue;
        }

        int delayCall = 0;
        double TotalTestedDUT = 0;
        string YRate = "";
        public void GetYRateSpec()
        {
            if (YRate == "")
            {
                YRate = "0";
            }
            string start_time = DateTime.Now.ToString("yyyyMMdd");
            string end_time = DateTime.Now.ToString("yyyyMMddHHmm");
            string model = ul.GetModel();// "C7000-100NASV1";//
                                         //string line = GetLineOfTester().Trim();// "L13";
            string mo = ul.GetValueByKey("MO");

            TotalTestedDUT = 0;
            if (ConnectServer60 == "0" && NetWorkConnection == true)
            {
                try
                {

                    string TmpDate = DateTime.Now.ToString("HHmm");
                    int ComparedTime = 730;

                    ComparedTime = Convert.ToInt32(TmpDate);

                    if (ComparedTime >= 730 && ComparedTime < 1930)
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


                    delayCall++;
                    if (delayCall == 5)
                    {
                        //globalStation = "FT1";
                        if (globalStation == "")
                        {
                            globalStation = ShowfPanelHStation();
                        }

                        delayCall = 0; // reset delay every 5pcs
                        DataTable dtYRate = sfisB05.GET_R_STATION_REC_T(start_time, end_time);
                        //MessageBox.Show(dtYRate.Rows.Count.ToString() + " " + mo + " " + model + " " + globalStation);
                        //DataRow[] results = dtYRate.Select("SECTION_NAME = 'SI' AND MODEL_NAME='" + model + "' AND GROUP_NAME='" + globalStation + "' AND LINE_NAME='" + line + "'");
                        DataRow[] results = dtYRate.Select("SECTION_NAME = 'SI' AND MODEL_NAME='" + model + "' AND GROUP_NAME='" + globalStation + "'");

                        foreach (DataRow dr in results)
                        {
                            TotalTestedDUT += (Convert.ToDouble(dr["PASS_QTY"].ToString()) + Convert.ToDouble(dr["FAIL_QTY"].ToString()));
                        }

                        YRate = GetStopmachineSpec(TotalTestedDUT, "YRate");
                        event_log("GetYRateSpec: Get total DUT from sfis with " + dtYRate.Rows.Count + " rows with " + results.Length + " results with " + globalStation + " " + model + " from " + start_time + " to " + end_time + " TotalTestedDUT: " + TotalTestedDUT + " YRate: " + YRate);

                    }

                }
                catch (Exception r)
                {
                    event_log("GetYRateSpec Exception: " + r.ToString());
                    //MessageBox.Show(r.ToString());
                }
            }// end if network
             //MessageBox.Show(YRate.ToString());
            YRStopSpec = YRate;
        }

        public void GetRTRateSpec()
        {
            string RTR = "101";

            if (globalStation == "")
            {
                globalStation = ShowfPanelHStation();
            }
            RTR = GetStopmachineSpec(TestedDUT, "RTRate");

            RTRStopSpec = RTR;
        }
        string BufferDUT = "50";
        int UpdateBufferDelay = 8;
        public void GetBufferDUT()
        {
            try
            {
                int inputPCS = TestedDUT;

                if (UpdateBufferDelay == 8)
                {
                    UpdateBufferDelay = 0;
                    SqlConnection connection = new SqlConnection(connSrv60);
                    connection.Open();

                    string sqlGetSpec = "";
                    sqlGetSpec += "select BufferDUT from tblStopMachineSpec";
                    sqlGetSpec += " where LowerNumDUT < " + inputPCS + " and UpperNumDUT >= " + inputPCS + "";
                    sqlGetSpec += " union all ";
                    sqlGetSpec += "select BufferDUT from tblStopMachineSpec";
                    sqlGetSpec += " where UpperNumDUT = (select max(UpperNumDUT)";
                    sqlGetSpec += " from tblStopMachineSpec)";

                    ///event_log(sqlGetSpec);
                    SqlCommand cmd = new SqlCommand(sqlGetSpec, connection);
                    SqlDataAdapter da = new SqlDataAdapter(sqlGetSpec, connSrv60);
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        BufferDUT = ds.Tables[0].Rows[0]["BufferDUT"].ToString();
                    }
                    cmd.Dispose();
                    da.Dispose();
                    reader.Close();
                    connection.Close();
                }

                UpdateBufferDelay++;
                //
            }
            catch (Exception)
            {
            }
        }


        public int GetTestTimeControl(string TypeOfValue)
        {

            string ControlFilePath = @"F:\lsy\Test\DownloadConfig\" + ul.GetProduct() + ".ini";
            //tmpPathProduct = IniFile.ReadIniFile(Product + "_1", "TPG_FOLDER_" + Station, null, ControlFilePath);

            string ReturnValue = "";
            if (File.Exists(ControlFilePath))
            {
                if (TypeOfValue == "TestTimeLowLimit")
                {
                    ReturnValue = IniFile.ReadIniFile(ul.GetProduct() + "_1", "TestTimeLowLimit_" + ul.GetStation(), "5", ControlFilePath);
                }
                else
                {
                    ReturnValue = IniFile.ReadIniFile(ul.GetProduct() + "_1", "TestTimeHighLimit_" + ul.GetStation(), "1500", ControlFilePath);
                }
            }
            else
            {
                if (TypeOfValue == "TestTimeLowLimit")
                {
                    ReturnValue = "5";
                }
                else
                {
                    ReturnValue = "1500";
                }
                //return Convert.ToInt32(ReturnValue);
            }

            return Convert.ToInt32(ReturnValue);
            //MessageBox.Show(Convert.ToString(ReturnValue));

        }

        bool npiflag = false;
        string listErrorCode = "";
        int countError = 0;
        string wDate, wLine, wModel, wStation, wAteName, wAteIp;
        ToDB connNPI = new ToDB();
        public void NPI_LOCK()
        {
            this.Invoke((MethodInvoker)delegate
            {
                try
                {
                    if (npiflag == true && NetWorkConnection == true)
                    {
                        // hidden showui
                        //Donlock tester when y
                        TestedDUT = 0;
                        //lock with 3 pcs fail

                        if (FKey == false) // when disable lock FKey = true
                        {
                            string _LockingMessage = "";
                            string _error = ul.GetValueByKey("ERRORCODE").Trim();

                            if (_error != "")
                            {
                                NPI_UPDATE_QTY(false);
                                listErrorCode += _error + ",";
                                countError = countError + 1;
                            }
                            else
                            {
                                NPI_UPDATE_QTY(true);
                            }

                            ul.SetValueByKey("ERRORCODE", "");
                            //MessageBox.Show(listErrorCode);
                            string[] errlist = ul.GetValueByKey("NPI_ERROR").Split(',');
                            ;

                            if (countError >= 3 || errlist.Length >= 3)
                            {
                                //MessageBox.Show("sdfsdfsdfsdf");
                                ul.SetValueByKey("StopMachine", "1");
                                if (errlist.Length >= 3)
                                {
                                    _LockingMessage = "Opps!Call TE Pro.NPI lỗi 3 bản mã lỗi " + ul.GetValueByKey("NPI_ERROR") + " Don't kill ShowUI,please!";
                                }
                                else
                                {
                                    _LockingMessage = "NPI lỗi 3 bản mã lỗi: " + listErrorCode + " Please Gọi TE Pro";
                                }

                                ////UpdateStopMachineStatus(true);
                                //if (countError != 0)
                                //{

                                //}
                                ul.SetValueByKey("NPI_ERROR", listErrorCode);
                                listErrorCode = "";
                                errlist = listErrorCode.Split(',');
                                countError = 0;
                                _IsExistErrorr = true;
                                ShowWarningMessage(_LockingMessage, "YRate NPI");
                            }
                        }// end if fkey

                    }// end if check npiflag
                }
                catch (Exception)
                {
                }

            }); // end for use thread


        }


        private void lblYeildRate_DoubleClick(object sender, EventArgs e)
        {

        }


        private void lblRetestRate_DoubleClick(object sender, EventArgs e)
        {

            if (npiflag == true && NetWorkConnection == true)
            {
                DialogResult dw = MessageBox.Show("Do you want to reset data?", "Warning Message", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (dw == DialogResult.OK)
                {

                    using (WarningButton wb = new WarningButton())
                    {
                        if (wb.ShowDialog() == DialogResult.OK)
                        {
                            string pw = wb.getPassword().Trim();
                            string user = wb.getUsername().Trim();
                            string svIp = ul.GetServerIP("SSO", "10.224.81.37");
                            //MessageBox.Show(svIp);
                            tmpIps += " SSO:10.224.81.62";
                            string connectionStringSSO = @"Data Source=10.224.81.62;Initial Catalog=SSO;uid=sa;pwd=********;Connection Timeout=5";

                            SqlConnection conn = new SqlConnection(connectionStringSSO);
                            conn.Open();
                            SqlDataReader reader;
                            string sqlStr = @"select username, password, dep from Users where username='" + user + "' and password = '" + pw + "'";
                            SqlCommand cmd = new SqlCommand(sqlStr, conn);
                            reader = cmd.ExecuteReader();
                            if (reader.HasRows)
                            {
                                if (npiflag == true)
                                {
                                    wDate = DateTime.Now.ToString("yyyyMMdd").Trim();
                                    wLine = GetLineOfTester().Trim();
                                    wModel = ul.GetValueByKey("SFISMODEL").Trim();
                                    wStation = ul.GetStation().Trim();
                                    wAteName = sName.Trim();
                                    wAteIp = client_ip.Trim();
                                    //showRealData = true;
                                    NPI_RESET_QTY();
                                    NPI_SHOW_DATA();
                                }

                            }
                            reader.Close();
                            conn.Close();
                        }
                        else
                        {
                            AutoClosingMessageBox.Show("Oops!Please login again...", "AutoCloseMessageBox", 5000);
                        }

                    } //

                }//
            }// end if check npiflag
        }

        private void showUI_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F12 && npiflag == true && NetWorkConnection == true)
            {
                // showRealData == true => set showrealdata = false then show yrate
                DialogResult dw = MessageBox.Show("Do you want to continue?", "Warning Message", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (dw == DialogResult.OK)
                {
                    using (WarningButton wb = new WarningButton())
                    {
                        if (wb.ShowDialog() == DialogResult.OK)
                        {
                            string pw = wb.getPassword().Trim();
                            string user = wb.getUsername().Trim();
                            string svIp = ul.GetServerIP("SSO", "10.224.81.37");
                            //MessageBox.Show(svIp);
                            tmpIps += " SSO:" + svIp;
                            string connectionStringSSO = @"Data Source=" + svIp + ";Initial Catalog=SSO;uid=sa;pwd=********;Connection Timeout=5";

                            SqlConnection conn = new SqlConnection(connectionStringSSO);
                            conn.Open();
                            SqlDataReader reader;
                            string sqlStr = @"select username, password, dep from Users where username='" + user + "' and password = '" + pw + "'";
                            SqlCommand cmd = new SqlCommand(sqlStr, conn);
                            reader = cmd.ExecuteReader();
                            if (reader.HasRows)
                            {
                                if (showRealData == true)
                                {
                                    showRealData = false;
                                    NPI_SHOW_DATA();
                                    //MessageBox.Show(showRealData.ToString());
                                }
                                else
                                {
                                    // showRealData == false => set showrealdata = true then show pass fail
                                    showRealData = true;
                                    NPI_REAL_DATA();
                                    //MessageBox.Show(showRealData.ToString());
                                }


                            }
                            reader.Close();
                            conn.Close();
                        }
                        else
                        {
                            AutoClosingMessageBox.Show("Oops!Please login again...", "AutoCloseMessageBox", 10000);
                        }

                    } //

                }// end dialog

            }//
        }

        private void notifyShowUI_DoubleClick(object sender, EventArgs e)
        {

            //panel1.Visible = true;
            //flowPanel.Visible = true;
            //fPanelDate.Visible = true;
            ////panel3.Visible = true;
            //btnCall.Visible = true;
            //notifyShowUI.Visible = false;
            //// for default when show is show retest rate, yeild rate
            //showRealData = false;
            //if (NetWorkConnection)
            //{
            //    Thread _tNPI_SHOW_DATA = new Thread(NPI_SHOW_DATA);
            //    _tNPI_SHOW_DATA.IsBackground = true;
            //    _tNPI_SHOW_DATA.Start();
            //}


        }

        bool showRealData = false; //set is true -> default in npi is show real
        public void NPI_SHOW_DATA()
        {
            // npi_show_data will show in debug mode show when showRealData =  false
            this.Invoke((MethodInvoker)delegate
            {
                try
                {
                    double sumQty = NPI_GET_SUM_QTY();
                    double pQty = NPI_GET_PASS_QTY();
                    double fQty = NPI_GET_FAIL_QTY();
                    //MessageBox.Show(pQty + "/" + fQty);
                    double YRate = Convert.ToDouble(Math.Round(Convert.ToDecimal(pQty / sumQty) * 100, 2));

                    double RTRate = Convert.ToDouble(Math.Round(Convert.ToDecimal(fQty / sumQty) * 100, 2));

                    //double TRR = NPI_GET_TRR();
                    lblTotalRate.Text = pQty + "/" + fQty;

                    lblRetestRate.Text = RTRate + "%";
                    lblYeildRate.Text = YRate + "%";

                    lblTR.Text = "P/F";
                    lblRR.Text = "S.R.R";
                    //lblYeildRate.Text = "" + "%";
                    //lblTotalRate.Text = pQty + " pcs";
                }
                catch (Exception)
                {
                    lblTR.Text = "P/F";
                    lblRR.Text = "S.R.R";

                    lblTotalRate.Text = "0/0";
                    lblRetestRate.Text = "0.0%";
                    lblYeildRate.Text = "100.0%";
                }

            }); // end for use thread

        }
        public void NPI_REAL_DATA()
        {
            this.Invoke((MethodInvoker)delegate
            {
                try
                {
                    //d // npi_real_data will show in debug mode show when showRealData =  true
                    double pQty = NPI_GET_PASS_QTY();
                    double rfQty = NPI_GET_REAL_FAIL_QTY();

                    double YRate = Convert.ToDouble(Math.Round(Convert.ToDecimal(pQty / (rfQty + pQty)) * 100, 2));
                    //DialogResult dw = MessageBox.Show("Do you want to show real data?", "Warning Message", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    //if (dw == DialogResult.OK)
                    //{
                    lblTR.Text = "PASS";
                    lblTotalRate.Text = pQty + "";
                    lblRR.Text = "FAIL";
                    lblRetestRate.Text = rfQty + "";
                    lblYeildRate.Text = YRate + "%";
                }
                catch (Exception)
                {
                }
            });// end for using thread
        }

        public void NPI_RESET_QTY()
        {
            try
            {
                string resetData = "update NPI_STATION_REC_T set fail_qty='0' where work_date = '" + wDate + "' and  line_name='" + wLine + "' and station_name ='" + wStation + "' and model_name ='" + wModel + "' and ate_name='" + wAteName + "' and ate_ip='" + wAteIp + "'";

                connNPI.Execute_NonSQL(resetData, serverIp);
            }
            catch (Exception)
            {
            }

        }

        public void NPI_UPDATE_QTY(bool testResult)
        {
            //MessageBox.Show(serverIp);
            try
            {
                wDate = DateTime.Now.ToString("yyyyMMdd").Trim();
                wLine = GetLineOfTester().Trim();
                wModel = ul.GetValueByKey("SFISMODEL").Trim();
                wStation = ul.GetStation().Trim();
                wAteName = sName.Trim();
                wAteIp = client_ip.Trim();
                double numPass = 0;
                double numFail = 0;
                double realFail = 0;
                realFail = NPI_GET_REAL_FAIL_QTY();
                if (testResult)
                {
                    numPass = NPI_GET_PASS_QTY() + 1;
                    numFail = NPI_GET_FAIL_QTY();
                }
                else
                {
                    numPass = NPI_GET_PASS_QTY();
                    numFail = 1 + NPI_GET_FAIL_QTY();
                    realFail = realFail + 1;
                }

                string checkExist = "select * from NPI_STATION_REC_T where work_date = '" + wDate + "' and  line_name='" + wLine + "' and station_name ='" + wStation + "' and model_name ='" + wModel + "' and ate_name='" + wAteName + "' and ate_ip='" + wAteIp + "' and active_status = '1'";
                //MessageBox.Show(checkExist);
                DataTable dt = connNPI.DataTable_Sql(checkExist, serverIp);
                if (dt.Rows.Count == 0)
                {
                    //insert new

                    string insertNew = "insert into NPI_STATION_REC_T (work_date,LINE_NAME,STATION_NAME,MODEL_NAME,ATE_NAME,ATE_IP,ACTIVE_STATUS,PASS_QTY,FAIL_QTY,REAL_FAIL_QTY) ";
                    insertNew += " values('" + wDate + "','" + wLine + "','" + wStation + "','" + wModel + "','" + wAteName + "','" + wAteIp + "','1','" + numPass + "','" + numFail + "','" + realFail + "')";
                    connNPI.Execute_NonSQL(insertNew, serverIp);
                    //MessageBox.Show("dsfsfd");
                }
                else
                {
                    //get qty
                    string updateExist = "update NPI_STATION_REC_T set pass_qty='" + numPass + "', fail_qty='" + numFail + "',real_fail_qty='" + realFail + "' where work_date = '" + wDate + "' and  line_name='" + wLine + "' and station_name ='" + wStation + "' and model_name ='" + wModel + "' and ate_name='" + wAteName + "' and ate_ip='" + wAteIp + "' and active_status = '1' ";
                    connNPI.Execute_NonSQL(updateExist, serverIp);
                }

                if (showRealData)
                {
                    NPI_REAL_DATA();
                }
                else
                {
                    NPI_SHOW_DATA();
                }

            }
            catch (Exception e)
            {
            }
        }
        public double NPI_GET_SUM_QTY()
        {
            double sumQty = 0;
            try
            {
                wDate = DateTime.Now.ToString("yyyyMMdd").Trim();
                wLine = GetLineOfTester().Trim();
                wModel = ul.GetValueByKey("SFISMODEL").Trim();
                wStation = ul.GetStation().Trim();
                wAteName = sName.Trim();
                wAteIp = client_ip.Trim();
                string sql = "select sum(convert(int,pass_qty)+ convert(int,fail_qty)) from NPI_STATION_REC_T where work_date = '" + wDate + "' and  line_name='" + wLine + "' and station_name ='" + wStation + "' and model_name ='" + wModel + "' and ate_name='" + wAteName + "' and ate_ip='" + wAteIp + "' and active_status = '1'";
                //event_log(sql);
                //string sql = "select pass_qty from NPI_STATION_REC_T where work_date = '" + wDate + "' and  line_name='" + wLine + "' and station_name ='PT' and model_name ='R7100LG-100EUSV1' and ate_name='V0917332-TE' and ate_ip='10.224.44.155' and active_status = '1'";
                //event_log(sql);
                DataTable dt = connNPI.DataTable_Sql(sql, serverIp);
                //MessageBox.Show(dt.Rows.Count +"---"+sql);
                if (dt.Rows.Count != 0)
                {
                    sumQty = Convert.ToDouble(dt.Rows[0][0].ToString());
                }
            }
            catch (Exception)
            {
            }

            return sumQty;
        }
        public double NPI_GET_PASS_QTY()
        {
            double pQty = 0;
            try
            {
                wDate = DateTime.Now.ToString("yyyyMMdd").Trim();
                wLine = GetLineOfTester().Trim();
                wModel = ul.GetValueByKey("SFISMODEL").Trim();
                wStation = ul.GetStation().Trim();
                wAteName = sName.Trim();
                wAteIp = client_ip.Trim();

                string sql = "select pass_qty from NPI_STATION_REC_T where work_date = '" + wDate + "' and  line_name='" + wLine + "' and station_name ='" + wStation + "' and model_name ='" + wModel + "' and ate_name='" + wAteName + "' and ate_ip='" + wAteIp + "' and active_status = '1'";

                //string sql = "select pass_qty from NPI_STATION_REC_T where work_date = '" + wDate + "' and  line_name='" + wLine + "' and station_name ='PT' and model_name ='R7100LG-100EUSV1' and ate_name='V0917332-TE' and ate_ip='10.224.44.155' and active_status = '1'";
                //event_log(sql);
                DataTable dt = connNPI.DataTable_Sql(sql, serverIp);
                //MessageBox.Show(dt.Rows.Count +"---"+sql);
                if (dt.Rows.Count != 0)
                {
                    pQty = Convert.ToDouble(dt.Rows[0][0].ToString());
                }
            }
            catch (Exception)
            {
            }

            return pQty;
        }
        public double NPI_GET_FAIL_QTY()
        {
            double fQty = 0;

            try
            {
                wDate = DateTime.Now.ToString("yyyyMMdd").Trim();
                wLine = GetLineOfTester().Trim();
                wModel = ul.GetValueByKey("SFISMODEL").Trim();
                wStation = ul.GetStation().Trim();
                wAteName = sName.Trim();
                wAteIp = client_ip.Trim();


                string sql = "select fail_qty from NPI_STATION_REC_T where work_date = '" + wDate + "' and  line_name='" + wLine + "' and station_name ='" + wStation + "' and model_name ='" + wModel + "' and ate_name='" + wAteName + "' and ate_ip='" + wAteIp + "' and active_status = '1'";
                DataTable dt = connNPI.DataTable_Sql(sql, serverIp);
                if (dt.Rows.Count != 0)
                {
                    fQty = Convert.ToDouble(dt.Rows[0][0].ToString());
                }

            }
            catch (Exception)
            {
            }

            return fQty;
        }

        public double NPI_GET_REAL_FAIL_QTY()
        {
            double fQty = 0;
            try
            {
                wDate = DateTime.Now.ToString("yyyyMMdd").Trim();
                wLine = GetLineOfTester().Trim();
                wModel = ul.GetValueByKey("SFISMODEL").Trim();
                wStation = ul.GetStation().Trim();
                wAteName = sName.Trim();
                wAteIp = client_ip.Trim();


                string sql = "select real_fail_qty from NPI_STATION_REC_T where work_date = '" + wDate + "' and  line_name='" + wLine + "' and station_name ='" + wStation + "' and model_name ='" + wModel + "' and ate_name='" + wAteName + "' and ate_ip='" + wAteIp + "' and active_status = '1'";
                DataTable dt = connNPI.DataTable_Sql(sql, serverIp);
                if (dt.Rows.Count != 0)
                {
                    fQty = Convert.ToInt32(dt.Rows[0]["real_fail_qty"].ToString());
                }
            }
            catch (Exception)
            {
            }


            return fQty;
        }

        public double NPI_GET_TRR()
        {
            double TRR = 0;
            double tpQty = 0;
            double tfQty = 0;
            try
            {
                wDate = DateTime.Now.ToString("yyyyMMdd").Trim();
                wLine = GetLineOfTester().Trim();
                wModel = ul.GetValueByKey("SFISMODEL").Trim();
                wStation = ul.GetStation().Trim();
                wAteName = sName.Trim();
                wAteIp = client_ip.Trim();
                string sql = "select sum(convert(int,pass_qty)) as tpQty , sum(convert(int,fail_qty)) as tfQty from NPI_STATION_REC_T where work_date = '" + wDate + "' and  line_name='" + wLine + "' and station_name ='" + wStation + "' and model_name ='" + wModel + "' and active_status = '1'";
                DataTable dt = connNPI.DataTable_Sql(sql, serverIp);

                if (dt.Rows.Count != 0)
                {
                    tpQty = Convert.ToDouble(dt.Rows[0]["tpQty"].ToString());
                    tfQty = Convert.ToDouble(dt.Rows[0]["tfQty"].ToString());
                    TRR = Math.Round(tfQty / (tfQty + tpQty) * 100, 2);
                }
            }
            catch (Exception)
            {
            }

            return TRR;
        }


        public void SetStopMachineStatusImediately()
        {
            //Antivirus False
            try
            {

                string _LockingMessage = "";
                ul.SetValueByKey("StopMachine", "0");
                //2017.10.31 lockBySampling


                //2016.10.31 check calibration
                //if (lockByCalibrationControl)
                //{
                //    //MessageBox.Show("ForceToUploadPathloss = true");
                //    ul.SetValueByKey("StopMachine", "1");
                //    _LockingMessage = "Lỗi: Máy chưa test golden sample! gọi te-online!";
                //    //UpdateStopMachineStatus(true);
                //    _IsExistErrorr = true;
                //    ShowWarningMessage(_LockingMessage, "Golden Data");
                //}

                //2016.10.07 Arlogen 4 Check Interference
                string InterferenceFlag = ul.GetValueByKey("INTERFERENCE_FLAG");
                if (InterferenceFlag == "0" && isInterferenceStation)
                {
                    // set list ip client to locked 

                    ul.SetValueByKey("StopMachine", "1");
                    _LockingMessage = "Lỗi: Kiểm tra Interference không đạt tiêu chuẩn ! gọi te-online!";
                    _IsExistErrorr = true;
                    ShowWarningMessage(_LockingMessage, "interference data");
                }



                // 201607025 to notice tpg version is not correct
                //if (CheckTPGVersion == false)
                //{
                //    ul.SetValueByKey("StopMachine", "1");
                //    _LockingMessage = "Lỗi: tpg version không đúng! gọi te-online!";
                //    //UpdateStopMachineStatus(true);
                //    _IsExistErrorr = true;
                //    ShowWarningMessage(_LockingMessage, "tpgversion data");
                //}


                // 201607023 to notice not check atechecklist
                //if (CheckATEOK == false)
                //{
                //    ul.SetValueByKey("StopMachine", "1");
                //    _LockingMessage = "Lỗi: chưa check bảng biểu điện tử ate-wireless! gọi te-online!";
                //    //UpdateStopMachineStatus(true);
                //    _IsExistErrorr = true;
                //    ShowWarningMessage(_LockingMessage, "ate-checklist data");
                //}

                //if (flagCheckGoldenData == false)
                //{
                //    ul.SetValueByKey("StopMachine", "1");
                //    _LockingMessage = "Lỗi: Máy chưa test golden sample! gọi te-online!";
                //    //UpdateStopMachineStatus(true);
                //    _IsExistErrorr = true;
                //    ShowWarningMessage(_LockingMessage, "Golden Data");
                //}

                //if (IsTPGUpdateTestFlag == 2 || PathUpdateTestFag == false)
                //{
                //    ul.SetValueByKey("StopMachine", "1");
                //    if (IsTPGUpdateTestFlag == 2)
                //    {
                //        _LockingMessage = "Lỗi: dữ liệu showui: tpg không cập nhật biến testflag ! gọi te-online!";
                //    }
                //    if (PathUpdateTestFag == false)
                //    {
                //        _LockingMessage = "Lỗi: dữ liệu showui: đường dẫn không đúng! gọi te-online!";
                //    }

                //    //UpdateStopMachineStatus(true);
                //    _IsExistErrorr = true;
                //    ShowWarningMessage(_LockingMessage, "showui data");
                //}

                if (AnitiVirusDisable)
                {
                    ul.SetValueByKey("StopMachine", "1");
                    _LockingMessage = "Lỗi: Phần mềm diệt virus bị vô hiệu. plz gọi te-online kiểm tra!";
                    //UpdateStopMachineStatus(true);
                    _IsExistErrorr = true;
                    ShowWarningMessage(_LockingMessage, "Virus Is Disable");
                }


                #region check port 445
                if (checkPort445())
                {
                    ul.SetValueByKey("StopMachine", "1");
                    _LockingMessage = "Lỗi: Không được open port 445 .";
                    //UpdateStopMachineStatus(true);
                    _IsExistErrorr = true;
                    ShowWarningMessage(_LockingMessage, "445 Enable");
                }
                #endregion

                #region Check config log path on server
                //RegistryKey regProgram = _OpenKey.CreateSubKey(openkey.Remove(0, 1));
                //string log_path = regProgram.GetValue("LOG_PATH", "",RegistryValueOptions.None).ToString();

                #endregion

                //if (CheckAntiVirusSoftUpdate() == false)
                //{
                //    ul.SetValueByKey("StopMachine", "1");
                //    _LockingMessage = "Lỗi: Phần mềm diệt virus " + VirusOutOfDateSpec() + " ngày chưa cập nhật. gọi te-online!";
                //    //UpdateStopMachineStatus(true);
                //    _IsExistErrorr = true;
                //    ShowWarningMessage(_LockingMessage, "Virus Out of Date");
                //}

                // UsbDisable False
                if (CheckUSBDisable() == false)
                {
                    ul.SetValueByKey("StopMachine", "1");
                    _LockingMessage = "Lỗi: USB chưa được vô hiệu. gọi te-online!";
                    //UpdateStopMachineStatus(true);
                    _IsExistErrorr = true;
                    ShowWarningMessage(_LockingMessage, "USB Not Disable");

                }

                if (GetPercentFreeSpace("D:\\") < 0)
                {
                    ul.SetValueByKey("StopMachine", "1");
                    _LockingMessage = "Lỗi: Dung lượng trống ổ đĩa D < 15% ! gọi TE!";
                    //UpdateStopMachineStatus(true);
                    _IsExistErrorr = true;
                    ShowWarningMessage(_LockingMessage, "Drive Full");
                }


                //

                if (_IsExistErrorr == false)
                {
                    //MessageBox.Show("i AM FALSE");

                    ul.SetValueByKey("StopMachine", "0");
                    ul.SetValueByKey("FixFlag", "");
                    //ShowUI.SvFPS.WebService svUnlock = new ShowUI.SvFPS.WebService();
                    //svUnlock.UpdateLockStatus(sName, "0");
                }


                if (GetWebUnlockPath("StopStation").Remove(1, GetWebUnlockPath("StopStation").Length - 1) == "1")
                {
                    //GetWebUnlockPath("StopStation").Remove(1,)
                    string stErr = ul.GetValueByKey("ErrStation");
                    if (ul.GetValueByKey("BYRate") == "0" && ul.GetValueByKey("ErrStation") == "")
                    {
                        ul.SetValueByKey("StopMachine", "1");
                        ShowWarningMessage("Lỗi: Có lỗi nghiêm trọng ở máy " + GetWebUnlockPath("StopStation").Remove(0, 1) + " trong trạm test! dừng cả trạm!", "StopAll");

                    }
                    if (ul.GetValueByKey("ErrStation") != "")
                    {
                        ul.SetValueByKey("StopMachine", "1");
                        ShowWarningMessage("Lỗi: Phát hiện mã lỗi " + stErr + " ở máy test " + Environment.MachineName + "! dừng cả trạm!", "StopAll");

                    }
                    if (ul.GetValueByKey("iCMTS") == "1")
                    {
                        ul.SetValueByKey("StopMachine", "1");
                        ul.SetValueByKey("iCMTS", "");
                        ShowWarningMessage("Error: CMTS issue in tester " + Environment.MachineName + "! Stop all. Call TE-Setup solve it!", "StopAll");
                    }
                }
                //if(checkModelName() == false)
                //{
                //    ul.SetValueByKey("StopMachine", "1");
                //    _LockingMessage = "Lỗi: ModelName ở AutoDL và ShowUI không giống nhau! AutoDL lại";
                //    //UpdateStopMachineStatus(true);
                //    _IsExistErrorr = true;
                //    ShowWarningMessage(_LockingMessage, "ModelName not correct!");
                //}
            }
            catch (Exception r)
            {
                event_log("SetStopmachineImmediately: " + r.ToString());
                // throw;
            }


        }

        //int CountOverTestTime = 0;
        public void SetStopMachineStatus()
        {

            string _LockingMessage = "";
            try
            {
                //2017.01.12
                if (_lockByUPH)
                {
                    ul.SetValueByKey("StopMachine", "1");
                    _LockingMessage = "Lỗi: Số bản R_ vượt tiêu chuẩn cho phép! gọi QA-PE-TE!";
                    //UpdateStopMachineStatus(true);
                    _IsExistErrorr = true;
                    _lockByUPH = false;
                    ShowWarningMessage(_LockingMessage, "R_OVERSPEC");
                }
                if (iFixture)
                {
                    ul.SetValueByKey("StopMachine", "1");

                    _LockingMessage = "Lỗi: Fixture ở server và local không giống nhau! TE Setup!";
                    //UpdateStopMachineStatus(true);
                    _IsExistErrorr = true;
                    iFixture = false;
                    ShowWarningMessage(_LockingMessage, "Fixture");
                }
                //-2019/29/01 Adele update lock when dont update Wannacry

                if (iWanna == false)
                {
                    ul.SetValueByKey("StopMachine", "1");

                    _LockingMessage = "Lỗi: Máy tính chưa được update Wannacry! Gọi TE Setup!";
                    //UpdateStopMachineStatus(true);
                    _IsExistErrorr = true;
                    iWanna = true;
                    ShowWarningMessage(_LockingMessage, "Wannacry Update");
                }
                //2019/02/13 Adele Lock when CMTS issue

                if (ul.GetValueByKey("CMTS_ISSUE") == "1")
                {
                    ul.SetValueByKey("StopMachine", "1");
                    SetWebUnlockPath("StopStation", "1" + "CMTS");
                    ul.SetValueByKey("iCMTS", "1");
                    //_LockingMessage = "Error: CMTS power issue. Please call TE Setup solve it!";
                    ////UpdateStopMachineStatus(true);
                    //_IsExistErrorr = true;
                    //ul.SetValueByKey("CMTS_ISSUE", "");
                    //ShowWarningMessage(_LockingMessage, "CMTS");
                }

                //if (fail3in10 == true)
                //{
                //    ul.SetValueByKey("StopMachine", "1");
                //    _LockingMessage = "Lỗi: 3 bản fail trong 10 bản test liên tiếp! Gọi TE Setup!";
                //    _IsExistErrorr = true;
                //    fail3in10 = false;
                //    ShowWarningMessage(_LockingMessage, "Error 3in10!");
                //}

                if (ForceToUploadPathloss)
                {
                    //MessageBox.Show("ForceToUploadPathloss = true");
                    ul.SetValueByKey("StopMachine", "1");
                    _LockingMessage = "Lỗi: Lưu giá trị pathloss không thành công. Kiểm tra lại file path_loss.csv! gọi te-online!";
                    //UpdateStopMachineStatus(true);
                    _IsExistErrorr = true;
                    ShowWarningMessage(_LockingMessage, "PathLoss Format");
                }

                //Alan 31-03-2018 Check Over Time measure lux at FT 3
                if (ul.GetValueByKey("OverTime") == "1")
                {
                    ul.SetValueByKey("StopMachine", "1");
                    _LockingMessage = "Lỗi: Vượt quá thời gian kiểm tra LUX. Gọi TE-ONLINE!";
                    //UpdateStopMachineStatus(true);
                    _IsExistErrorr = true;
                    ShowWarningMessage(_LockingMessage, "OverTime");

                }//End Alan 31-03-2018
                 //if (checkModelName() == false)
                 //{
                 //    ul.SetValueByKey("StopMachine", "1");
                 //    _LockingMessage = "Lỗi: ModelName ở AutoDL và ShowUI không giống nhau! AutoDL lại";
                 //    //UpdateStopMachineStatus(true);
                 //    _IsExistErrorr = true;
                 //    ShowWarningMessage(_LockingMessage, "ModelName not correct!");
                 //}
                if (ul.GetValueByKey("ERRORCODE") != "")
                {
                    // 2016 check 3 errorcode same in 1 hour,  20161220 disable
                    if (!CheckErrorList())
                    {
                        //ul.SetValueByKey("StopMachine", "1");
                        //_LockingMessage = "Lỗi: " + TopErrorCodeInHour + "! gọi te-online!";
                        //UpdateStopMachineStatus(true);
                        //_IsExistErrorr = true;
                        //ShowWarningMessage(_LockingMessage, "ErrorCodeInHour");

                    }

                    string ErrorCodeName = ul.GetValueByKey("ERRORCODE").Trim();

                    if (TmpCountErr == ErrorCodeName)
                    {
                        //MessageBox.Show(TmpCountErr);
                        FlagSameErrorCode++;
                        //TmpCountErr = ErrorCodeName;
                        CountSameError += ErrorCodeName + "_" + CurrentDUT + ",";
                        ul.SetValueByKey("CountSameError", CountSameError);
                        if (FlagSameErrorCode == 3)
                        {
                            //MessageBox.Show(TmpCountErr);
                            // update to db to stop station
                            //StopStationCommand("3 BẢN LIÊN TIẾP LỖI: " + TmpCountErr, DateTime.Now);
                            FlagSameErrorCode = 0;
                            //TmpCountErr = "";
                            CountSameError = "";
                        }

                    }
                    else
                    {
                        CountSameError = "";
                        CountSameError += ErrorCodeName + "_" + CurrentDUT + ",";
                        FlagSameErrorCode = 0;
                        //TmpCountErr = ErrorCodeName;
                        ul.SetValueByKey("CountSameError", CountSameError);

                    }

                    // Continuous 4 fail DUT

                    CountError += ErrorCodeName + "_" + CurrentDUT + ",";
                    ul.SetValueByKey("CountError", CountError);
                    FlagErrorCode++;
                    //MessageBox.Show(CountError);
                    if (FlagErrorCode == 4)
                    {
                        FlagErrorCode = 0;
                        CountError = "";
                        // TmpCountErr = ErrorCodeName;
                    }

                    //update tmpCountErr
                    TmpCountErr = ErrorCodeName;

                }
                else
                {
                    // SFISDATA PAss -> reset all;
                    //
                    FlagSameErrorCode = 0;
                    FlagSameErrorCode = 0;
                    CountError = "";
                    CountSameError = "";
                    ul.SetValueByKey("CountError", CountError);
                    ul.SetValueByKey("CountSameError", CountSameError);
                    ul.SetValueByKey("ERRORCODE", "");
                    //ul.SetValueByKey("SN", "");
                    TmpCountErr = "";
                    // MessageBox.Show("DUT PASS ->Reset All");
                }
                // Testing time lower than limit   //Testing time higher than limit

                string tmp = ul.GetValueByKey("Testtime").Trim();
                double TesttimeOfDUT;
                if (tmp != "" && ul.GetValueByKey("ErrorCode") == "")
                {
                    //MessageBox.Show(Convert.ToString(tmp));
                    try
                    {
                        DateTime dtCheck = DateTime.ParseExact(tmp, "mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                        TesttimeOfDUT = dtCheck.Second + dtCheck.Minute * 60;
                    }
                    catch (Exception)
                    {
                        TesttimeOfDUT = TestTimeLowLimit + 2;
                        //throw;
                    }

                    //DateTime dtStomb = DateTime.ParseExact("00:00", "mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                    //TimeSpan tsp = dtCheck.Subtract(dtStomb);
                    //double TesttimeOfDUT = tsp.TotalSeconds;

                    // MessageBox.Show(Convert.ToString(TesttimeOfDUT));
                    if (TesttimeOfDUT < TestTimeLowLimit)
                    {
                        ul.SetValueByKey("StopMachine", "1");
                        _LockingMessage = "Lỗi: Thời gian test " + tmp + " thấp hơn tiêu chuẩn! gọi te-online!";
                        //UpdateStopMachineStatus(true);
                        _IsExistErrorr = true;
                        ShowWarningMessage(_LockingMessage, "Testtime Too Low -->" + tmp);


                    }
                    //Testing time higher than limit
                    if (TesttimeOfDUT > TestTimeHighLimit)
                    {
                        ul.SetValueByKey("StopMachine", "1");
                        _LockingMessage = "Lỗi: Thời gian test " + tmp + " cao hơn tiêu chuẩn! gọi te-online!";
                        //UpdateStopMachineStatus(true);
                        _IsExistErrorr = true;
                        ShowWarningMessage(_LockingMessage, "Testtime Too High -->" + tmp);

                    }
                }
            }
            catch (Exception)
            {
            }
            // RR >=5 %

            try
            {
                int Buffer = Convert.ToInt32(ul.GetValueByKey("BRTRate"));
                int BufferYRate = Convert.ToInt32(ul.GetValueByKey("BYRate"));


                if (Buffer > 0)
                {
                    Buffer--;
                    ul.SetValueByKey("BRTRate", Convert.ToString(Buffer));
                }
                if (BufferYRate > 0)
                {
                    BufferYRate--;
                    ul.SetValueByKey("BYRate", Convert.ToString(BufferYRate));
                }

                //string RRYRdata = ul.GetValueByKey("SFISDATA");
                string RRYRdata = ul.GetValueByKey("RYRDATA");


                //float RTRate = Convert.ToSingle(RRYRdata.Substring(48, 6));
                //float YRate = Convert.ToSingle(RRYRdata.Substring(54, 6));
                float RTRate = 0;
                if (npiflag == true && showRealData == true)
                {
                    RTRate = 0;
                }
                else
                {
                    RTRate = Convert.ToSingle(lblRetestRate.Text.Replace("%", ""));
                }

                float YRate = Convert.ToSingle(lblYeildRate.Text.Replace("%", ""));

                //MessageBox.Show(UseSpecMode.ToString() + "--->" + RTRStopSpec);
                if (RRYRdata != "")
                {
                    if (RTRate > Convert.ToSingle(RTRStopSpec))
                    {
                        //BoolRRateBuffer = true;
                        if (ul.GetValueByKey("BRTRate") == "0")
                        {

                            ////ul.SetValueByKey("RTRateBuffer", Convert.ToString(RTRate / 2));

                            //ul.SetValueByKey("StopMachine", "1");
                            //_LockingMessage = "Lỗi: Retest Rate cao hơn " + RTRStopSpec + "%! goi te-online!";
                            ////UpdateStopMachineStatus(true);
                            //_IsExistErrorr = true;

                            ////ShowWarningMessage(_LockingMessage, "RTRate " + Convert.ToString(RTRate) + "%! Overspec " + RTRStopSpec + "");
                        }

                    }
                    else
                    {
                        ul.SetValueByKey("BRTRate", "0");
                    }


                    if (_TypeOfManufacturing == "MP") //in case MP
                    {

                        if (YRate < Convert.ToSingle(YRStopSpec)) //for MP
                        {
                            //BoolYRateBuffer = true;

                            if (ul.GetValueByKey("BYRate") == "0")
                            {

                                //ul.SetValueByKey("StopMachine", "1");
                                ////ul.SetValueByKey("StopStation", "1Default");
                                //_IsExistErrorr = true;

                                //_LockingMessage = "Lỗi: Yeild Rate thấp hơn " + YRStopSpec + " %! goi qa/pqe!";
                                ////ShowWarningMessage(_LockingMessage, "YRate " + YRate + " Lower Than " + YRStopSpec + "%!");

                            }


                        }
                        else
                        {
                            //BoolYRateBuffer = false;

                            ul.SetValueByKey("BYRate", "0");
                        }
                    }
                    else // in case NPI
                    {
                        if (YRate < Convert.ToSingle(YRStopSpec)) //forNPI
                        {
                            //BoolYRateBuffer = true;

                            if (ul.GetValueByKey("BYRate") == "0")
                            {

                                //ul.SetValueByKey("StopMachine", "1");
                                ////ul.SetValueByKey("StopStation", " 1Default");

                                //_LockingMessage = "Lỗi: Yeild Rate thấp hơn " + YRStopSpec + " %! gọi qa/pqe!";
                                //_IsExistErrorr = true;
                                //ShowWarningMessage(_LockingMessage, "YRate " + YRate + " Lower Than " + YRStopSpec + "%!");
                                // }
                            }
                        }
                        else
                        {
                            //BoolYRateBuffer = false;
                            ul.SetValueByKey("BYRate", "0");
                        }
                    }
                }
            }
            catch (Exception)
            {

                //throw;
            }

            // 20170505 for lock tester when test finish
            ul.event_log("SamplingTimeOut " + ul.GetValueByKey("SamplingTimeOut"));
            if (ul.GetValueByKey("SamplingTimeOut") == "0")
            {
                //MessageBox.Show("SamplingTimeOut");
                //MessageBox.Show("ForceToUploadPathloss = true");
                ul.SetValueByKey("StopMachine", "1");
                //string S_Rate = fr
                //ul.SetValueByKey("Samp_Rate", "1");
                //2017.11.13 fake show sampling_window for Arlo (FT7/FT8) Adele
                string window_sampling = ul.GetValueByKey("Show_sampling");
                ul.event_log("SamplingTimeOut: " + ul.GetValueByKey("SamplingTimeOut") + ul.GetValueByKey("StopMachine") + window_sampling + "MO: " + ul.GetValueByKey("MO"));
                //frmSC.Visible = false;
                if (window_sampling == "0")
                {
                    _LockingMessage = "Lỗi: Sysmatec virut " + ul.GetValueByKey("SamplingStation") + " không update! gọi PD!";
                    ShowWarningMessage(_LockingMessage, "SAMPLING_CONTROL");
                }
                else
                {

                    //_LockingMessage = "Lỗi: Sản lượng Sampling trạm " + ul.GetValueByKey("SamplingStation") + " không đạt! gọi pd!";
                }


                //UpdateStopMachineStatus(true);
                _IsExistErrorr = true;

            }


            //Cable over spec

            if (CheckCableStatus == false)
            {
                ul.SetValueByKey("StopMachine", "1");
                _LockingMessage = "Lỗi: Số lần sử dụng cáp quá giới hạn! gọi te-online!";
                //UpdateStopMachineStatus(true);
                _IsExistErrorr = true;
                ShowWarningMessage(_LockingMessage, "Cable Over Spec");

            }

            //2018.02.08 fix dont lock showUI when Virus out of date
            bool viruscondition = CheckAntiVirusSoftUpdate();
            bool virusconditionWin10 = CheckAntiVirusSoftUpdateWin10();

            if (viruscondition == false && virusconditionWin10)
            {
                ul.SetValueByKey("StopMachine", "1");
                _LockingMessage = "Lỗi: Phần mềm virus quá hạn update! gọi te-online!";
                //UpdateStopMachineStatus(true);
                _IsExistErrorr = true;
                ShowWarningMessage(_LockingMessage, "Virus");
            }
            //12.9.2015 Fix bug in rest time has try-test fail, but still count as error
            if (!FKey) // for disable lock machine, but enable to other
            {
                //MessageBox.Show(FKey.ToString());
                if (DateTime.Now.Minute >= 30 && DateTime.Now.Minute <= 45 && (DateTime.Now.Hour == 0 || DateTime.Now.Hour == 2 || DateTime.Now.Hour == 9 || DateTime.Now.Hour == 12 || DateTime.Now.Hour == 14 || DateTime.Now.Hour == 21))
                {
                    ul.SetValueByKey("CountSameError", "");
                    ul.SetValueByKey("CountError", "");
                }
                else
                {

                    string _OntimeCheckingSameError = ul.GetValueByKey("CountSameError");
                    string _OntimeCheckingError = ul.GetValueByKey("CountError");
                    if (_OntimeCheckingSameError != "")
                    {
                        string[] tmpCountNumOfError = _OntimeCheckingSameError.Remove(_OntimeCheckingSameError.Length - 1, 1).Split(',');
                        string[] tmpA = tmpCountNumOfError[0].Split('_');
                        if (tmpCountNumOfError.Length == 3)
                        {

                            FlagSameErrorCode = 0;
                            CountSameError = "";
                            ul.SetValueByKey("StopMachine", "1");
                            _LockingMessage = "Lỗi: 3 bản liên tiếp lỗi " + tmpA[0] + "! gọi te-online!";
                            //UpdateStopMachineStatus(true);
                            _IsExistErrorr = true;
                            ShowWarningMessage(_LockingMessage, "3_Continuous DUT Error " + tmpA[0]);
                        }
                    }


                    if (_OntimeCheckingError != "")
                    {
                        string[] tmpCountNumOfError = _OntimeCheckingError.Remove(_OntimeCheckingError.Length - 1, 1).Split(',');
                        string[] tmpB = tmpCountNumOfError[0].Split('_');
                        if (tmpCountNumOfError.Length == 4)
                        {
                            FlagErrorCode = 0;
                            CountError = "";
                            //ul.SetValueByKey("CountError", CountError);
                            ul.SetValueByKey("StopMachine", "1");
                            //ShowWarningMessage("4_Error lien tiep");
                            _LockingMessage = "Lỗi: 4 bản liên tiếp lỗi không giống nhau! gọi te-online";
                            //UpdateStopMachineStatus(true);
                            _IsExistErrorr = true;
                            ShowWarningMessage(_LockingMessage, "4_Continuous DUT Error ");
                            // ShowWarningMessage(_LockingMessage);
                        }
                    }

                }

            }

            //

            try
            {
                if (_IsExistErrorr == false)
                {
                    ul.SetValueByKey("StopMachine", "0");
                    ul.SetValueByKey("FixFlag", "");

                }
            }
            catch (Exception r)
            {
                event_log("SetStopMachineStatus: " + r.ToString());
                //MessageBox.Show(r.ToString());
            }

            //


        }


        public void ShowWarningMessage(string _LockingCondition, string _KindOfError)
        {
            //if (isNTGR == 0)
            //{
            //	return;
            //}

            //locking with new err
            if (_KindOfError.Contains("low") || _KindOfError.Contains("Virus") || _KindOfError.Contains("SAMPLING_CONTROL") || _KindOfError.Contains("Rate") || _KindOfError.Contains("Pathloss"))
            {
                //// When frmLocking is open, pc completed testing, then dont call again
                try
                {
                    int idFii = 0;
                    UpdateYR.Enabled = false;
                    string frmLockingCheck = IniFile.ReadIniFile("Active", "Status", "0", @".\frmActive.ini");
                    //disable timer and call form displaying message

                    if (frmLockingCheck == "1")
                    {
                        // Do nothing coz let to have time to to check bug 
                        //MessageBox.Show("frmLockingCheck" + frmLockingCheck);

                        if (_KindOfError.Contains("USB") || _KindOfError.Contains("SAMPLING_CONTROL") || _KindOfError.Contains("Virus") || _KindOfError.Contains("Cable") || _KindOfError.Contains("445") || _KindOfError.Contains("Rate"))
                        {
                            try
                            {
                                idFii = FiiData.InsertFii(_KindOfError, lblQty.Text, lblYeildRate.Text);
                            }
                            catch (Exception ex)
                            {
                                //MessageBox.Show(ex.ToString());
                            }
                            //MessageBox.Show(idFii.ToString());
                            ShowUI.frmLocking tmpForm = new ShowUI.frmLocking(_LockingCondition, _KindOfError, UseSpecMode, TestedDUT, globalUsedMode, BufferDUT, idFii);
                            tmpForm.ShowDialog();

                        }
                    }
                    else
                    {

                        //if (GetWebUnlockPath("StopStation").Remove(1, GetWebUnlockPath("StopStation").Length - 1) == "1")
                        if (GetWebUnlockPath("StopStation").Remove(1, GetWebUnlockPath("StopStation").Length - 1) == "1" && ul.GetValueByKey("ErrStation") == "")
                        {
                            //MessageBox.Show("StopStation" + GetWebUnlockPath("StopStation"));
                            try
                            {
                                idFii = FiiData.InsertFii(_KindOfError, lblQty.Text, lblYeildRate.Text);
                            }
                            catch (Exception ex)
                            {
                                //MessageBox.Show(ex.ToString());
                            }
                            ShowUI.frmLocking tmpForm = new ShowUI.frmLocking(_LockingCondition, _KindOfError, UseSpecMode, TestedDUT, globalUsedMode, BufferDUT, idFii);
                            tmpForm.ShowDialog();
                        }
                        else if (ul.GetValueByKey("FixFlag") == "1" || ul.GetValueByKey("StopMachine") == "0")
                        {
                            //Do nothing
                        }
                        else
                        {
                            if (_KindOfError.Contains("YRate"))
                            {
                                UpdateStopMachineStatus(false);
                            }
                            else
                            {
                                UpdateStopMachineStatus(true);
                            }
                            try
                            {
                                idFii = FiiData.InsertFii(_KindOfError, lblQty.Text, lblYeildRate.Text);
                            }
                            catch (Exception ex)
                            {
                                //MessageBox.Show(ex.ToString());
                            }
                            ShowUI.frmLocking tmpForm = new ShowUI.frmLocking(_LockingCondition, _KindOfError, UseSpecMode, TestedDUT, globalUsedMode, BufferDUT, idFii);
                            tmpForm.ShowDialog();
                        }

                    }


                }
                catch (Exception r)
                {
                    event_log("ShowWarningMessage: " + r.ToString());
                    // MessageBox.Show(r.ToString());
                }
            }
        }

        private void timerEnableTimer_Tick(object sender, EventArgs e)
        {
            if (ul.GetValueByKey("StopMachine") == "0" && UpdateYR.Enabled == false)
            {
                UpdateYR.Enabled = true;

                ForceToUploadPathloss = false; // 12.01.2016 set check upload pathloss to dont need when user unlock tester
            }


            if ((DateTime.Now.Hour == 6 && DateTime.Now.Minute == 50 && DateTime.Now.Second == 0) || (DateTime.Now.Hour == 18 && DateTime.Now.Minute == 50 && DateTime.Now.Second == 0))
            {
                string localStation = ul.GetStation();
                string localProduct = ul.GetProduct();
                if (localStation.StartsWith("S_"))
                {
                    if (localProduct != "U12I345" || localStation != "RC") // prevent rc arlo pro if reautodl need reset base
                        ul.ShellExecute("TASKKILL /IM NetgearAutoDL.exe /T /F");

                    string tpg = ul.GetValueByKey("TPG_NAME");
                    ul.ShellExecute("TASKKILL /IM " + tpg + " /T /F");
                }

                Application.Exit();
            }

        }


        public void UpdateStopMachineStatus(bool updateSingle)
        {
            try
            {
                ToDB conn = new ToDB();

                if (updateSingle)
                {
                    // update  itself
                    string updateSql = "update Status set stop_status = 1";
                    updateSql += " where model_name ='" + _model_name + "' and  line_name = '" + _line_name + "' and station_name = '" + _station_name + "' and  ate_name ='" + _ate_name + "' and ate_ip='" + _ate_ip + "'";
                    conn.Execute_NonSQL(updateSql, serverIp);
                }
                else
                {
                    //_model_name = "C7000-100NASV1";
                    //_line_name = "13";
                    //_station_name = "PT";
                    //_ate_name = "B05-L13-PT01";
                    //_ate_ip = "10.224.84.171";
                    //_line_name in status table not have L
                    _line_name = _line_name.Replace("L", "");
                    // update all ate in station of a line testing 1 model 
                    string updateSql = "update Status set stop_status = 1, stop_ate = '1" + sName + "'";
                    updateSql += " where model_name ='" + _model_name + "' and  line_name = '" + _line_name + "' and station_name = '" + _station_name + "'";
                    conn.Execute_NonSQL(updateSql, serverIp);
                    //event_log(updateSql);
                }
            }
            catch (Exception r)
            {
                event_log("UpdateStopMachineStatus: " + r.ToString());
            }

        }
        public void GetStopMachineStatus()
        {
            //bool rturnValue = false;

            try
            {
                ToDB conn = new ToDB();

                _line_name = _line_name.Replace("L", "");
                //_line_name = "13";
                //_model_name = "C7000-100NASV1";
                //_station_name = "PT";
                //_ate_ip = "10.224.84.171";
                //_ate_name = "B05-L13-PT01";
                //string dTime = DateTime.Now.AddMinutes(-15).ToString("yyyy-MM-dd HH:mm:ss");
                //MessageBox.Show(dTime);
                string sqlIsLocked = "select stop_status,stop_ate from Status where model_name ='" + _model_name + "' and  line_name = '" + _line_name + "' and station_name = '" + _station_name + "' and  ate_name ='" + _ate_name + "' and ate_ip='" + _ate_ip + "' and datediff(minute,getdate(),date_time)<=30";
                DataTable dt = conn.DataTable_Sql(sqlIsLocked, serverIp);
                //event_log(sqlIsLocked+" "+ dt.Rows.Count);
                //SetWebUnlockPath("StopStation", "1HAHAHA");
                //MessageBox.Show(dt.Rows.Count.ToString());
                if (dt.Rows.Count != 0)
                {
                    string rsult = dt.Rows[0]["stop_status"].ToString();
                    string ate_error = dt.Rows[0]["stop_ate"].ToString();
                    //MessageBox.Show(rsult + "rsult | ate_error" + ate_error + " ul." + ul.GetValueByKey("BYRate"));
                    if (rsult == "1" && ate_error != "")
                    {
                        SetWebUnlockPath("StopStation", ate_error);
                        //rturnValue = true;
                    }

                    //event_log("stop_status:" + rsult + " stop_ate:" + ate_error + " Ip:" + serverIp);
                }

                string sqlUpdateAllfield = "update status set date_time='" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "'";
                sqlUpdateAllfield += " where model_name = '" + _model_name + "' and line_name='" + _line_name + "' and station_name='" + _station_name + "' and ate_name='" + _ate_name + "' and ate_ip='" + _ate_ip + "'";
                conn.Execute_NonSQL(sqlUpdateAllfield, serverIp);
                //return rturnValue;
            }
            catch (Exception r)
            {
                //case db crash able to close
                //rturnValue = true;
                SetWebUnlockPath("StopStation", "0Default");
                event_log("GetStopMachineStatus: " + r.ToString());
                //return rturnValue;
            }
        }

        //private const int SendPort = 19290;
        //List<string> ListIpAddress = new List<string>();
        //public void GetIPofStation()
        //{
        //    string svIp = ul.GetServerIP("ARS_System", "10.224.81.37");
        //    tmpIps += " GetIpListInStation:" + svIp;
        //    //event_log("Server Ips: " + tmpIps);
        //    string sqlGetComputerIPinStationConnection = "Data Source=" + svIp + ";Initial Catalog=Ars_System; Uid=sa;Pwd=********";
        //    if (ConnectServer37 == "0" && NetWorkConnection == true)
        //    {
        //        Thread.Sleep(150000);// delay 3 min to make sure 1 DUT is tested ok
        //        ListIpAddress.Clear();
        //        //string sql_cmdGetIp = "SELECT * FROM StationInfo where LINE='" + GetLineOfTester().Trim().Remove(0, 1) + "' and Type='" + ul.GetStation().Trim() + "' and station_ip != '"+client_ip+"'";
        //        string sql_cmdGetIp = "SELECT S.STATION_IP,S.STATION_NAME,S.LINE,TYPE,S.JERRY,S.MAC_ADDR,S.IDLE,S.IDLE_STATUS,S.EVENT_LOG,S.LOCK,S.PcStatus,S1.MODEL_NAME from StationInfo as S join Station as S1 on S.station_ip = S1.station_ip and S.Line='" + GetLineOfTester().Trim().Remove(0, 1) + "' and S.Type='" + ul.GetStation().Trim() + "' and S1.MODEL_NAME='" + ul.GetModel().Trim() + "' and S.station_ip != '" + client_ip + "'";

        //        try
        //        {
        //            using (SqlConnection connection = new SqlConnection(sqlGetComputerIPinStationConnection))
        //            using (SqlCommand cmd = new SqlCommand(sql_cmdGetIp, connection))
        //            {
        //                connection.Open();
        //                using (SqlDataReader reader = cmd.ExecuteReader())
        //                {
        //                    // Check is the reader has any rows at all before starting to read.
        //                    string list_ip = "";
        //                    if (!reader.HasRows)
        //                    {
        //                        reader.Close();
        //                        return;
        //                    }

        //                    while (reader.Read())
        //                    {
        //                        ListIpAddress.Add(reader.GetString(reader.GetOrdinal("STATION_IP")));
        //                        list_ip += reader.GetString(reader.GetOrdinal("STATION_IP")) + ",";
        //                    }
        //                    reader.Close();
        //                    //to track list of ip of one stataion  13.1.2016
        //                    //event_log(list_ip);
        //                }


        //                connection.Close();
        //            }
        //        }
        //        catch (Exception)
        //        {
        //        }
        //    }//end if connectserver37
        //}


        static Socket sck;
        public void SendDataSocket(string IPaddr, int SendPort, string Msg)
        {
            try
            {
                sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint cientIP = new IPEndPoint(IPAddress.Parse(IPaddr), SendPort);
                sck.Connect(cientIP);
                byte[] sendData = Encoding.ASCII.GetBytes(Msg);
                sck.Send(sendData);
                sck.Close();
            }
            catch (Exception)
            {

                //MessageBox.Show("Connect to server aborted!\r\n");
            }
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
            string rgPath = subkey;

            _OpenKey = Registry.LocalMachine.OpenSubKey(subkey + "\\" + ul.GetProduct() + "\\" + ul.GetStation());
            try
            {
                if (_OpenKey != null)
                {
                    rgPath = subkey + "\\" + ul.GetProduct() + "\\" + ul.GetStation();
                    return rgPath;
                }
                return rgPath;
            }
            catch (Exception)
            {
                return rgPath;
            }
        }
        //End AutstopTester

        private void DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                //hread.CurrentThread.Abort();
            }
            else
            {
                event_log("Download Completed");
                //Thread.CurrentThread.Start();
            }
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ProgressBar pr = new ProgressBar();
        }


        private void CheckUpdate_Tick(object sender, EventArgs e)
        {

            if (DateTime.Now.Minute == 25 && DateTime.Now.Second == 5)
            {
                CheckUpdate.Enabled = false;
                UpdateShowUI();
                CheckUpdate.Enabled = true;
            }
            //end check update

        }


        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
        public void UpdateShowUI()
        {

            try
            {
                Assembly asb = Assembly.GetEntryAssembly();
                AssemblyName asbName = asb.GetName();
                Version this_ver = asbName.Version;

                System.Diagnostics.FileVersionInfo ui_server = System.Diagnostics.FileVersionInfo.GetVersionInfo(@"F:\lsy\Test\DownloadConfig\AutoDL\ShowUI.exx");
                Version server_ver = new Version(ui_server.FileVersion);
                switch (server_ver.CompareTo(this_ver))
                {
                    case 1:
                        File.Copy(@"F:\lsy\Test\DownloadConfig\AutoDL\HP.exx", @"D:\AutoDL\Update.exe", true);
                        event_log("Update ShowUI: Runing...");
                        System.Diagnostics.Process.Start(@"D:\AutoDL\Update.exe");
                        break;
                    default:
                        break;
                }

            }
            catch
            {
            }

            try
            {
                System.Diagnostics.FileVersionInfo AD_server = System.Diagnostics.FileVersionInfo.GetVersionInfo(@"F:\lsy\Test\DownloadConfig\AutoDL\NetgearAutoDL.exx");
                Version AD_SV_ver = new Version(AD_server.FileVersion);
                System.Diagnostics.FileVersionInfo AD_local = System.Diagnostics.FileVersionInfo.GetVersionInfo(@"D:\AutoDL\NetgearAutoDL.exe");
                Version AD_LC_ver = new Version(AD_local.FileVersion);
                switch (AD_SV_ver.CompareTo(AD_LC_ver))
                {
                    case 1:
                        System.Diagnostics.Process[] kill_AD = System.Diagnostics.Process.GetProcessesByName("NetgearAutoDL");
                        foreach (System.Diagnostics.Process p in kill_AD)
                            p.Kill();
                        ShellExecute("taskkill /IM NetgearAutoDL.exe /F");

                        File.Copy(@"F:\lsy\Test\DownloadConfig\AutoDL\NetgearAutoDL.exx", @"D:\AutoDL\NetgearAutoDL.exe", true);
                        System.Diagnostics.Process.Start(@"D:\AutoDL\NetgearAutoDL.exe");
                        event_log("Update NetgearAutoDL: OK");
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {
                //MessageBox.Show(r.ToString());
            }

        }

        // Nhung data
        int FlagStopInsert = 0;

        public void InsertPCKPI()
        {

            try
            {
                string Line = GetLineOfTester().Trim();
                string ModelName = ul.GetModel().Trim();

                string Station = GetFixtureNo().Trim();// same as fixture num value
                string ProgramandVersion = GetTPGVersion().Trim();
                string ChkSum = GetCkSum().Trim();
                string FixtureNo = GetFixtureNo().Trim();
                string UpdateVirusT = GetUpdateAntivirusTime().Trim();
                string dtNow = DateTime.Now.ToString("yyyyMMdd");
                string CurrentShift = "";
                string CurrentModelName = "";
                string CurrentCksum = "";
                CurrentModelName = ul.GetModel().Trim();
                CurrentCksum = GetCkSum().Trim();
                int flagToCheckPathlossByWeek = 1;

                string TmpDate = DateTime.Now.ToString("HHmm");
                int ComparedTime = 730;
                ComparedTime = Convert.ToInt32(TmpDate);
                //MessageBox.Show(TmpDate);
                if (ComparedTime >= 730 && ComparedTime <= 1930)
                {
                    CurrentShift = "D";
                }
                else
                {
                    CurrentShift = "N";
                }

                // make sure FlagStopInsert = 0 when change to night shift 27.9.2015
                if (ComparedTime >= 1930 && ComparedTime < 2030)
                {
                    FlagStopInsert = 0;
                }

                if (CurrentShift != Shift || CurrentModelName != ModelNameChangeATE || CurrentCksum != CheckSum)
                {

                    // 12.1.2016 Rwite to reg  model name for check model change or not support for CheckUploadPathloss() 
                    ul.SetValueByKey("SFISMODEL", CurrentModelName);
                    // 12.1.2016 Force to chang pathloss when model change 12.1.2016
                    Thread _tForcChangePathloss = new Thread(ForcChangePathloss);
                    _tForcChangePathloss.IsBackground = true;
                    _tForcChangePathloss.Start();

                    FlagStopInsert = 0;
                    flagToCheckPathlossByWeek = 0; // flag to avoid  both by hour case and by week case
                    Shift = CurrentShift;
                    ModelNameChangeATE = CurrentModelName;
                    CheckSum = CurrentCksum;
                    // Update ListIPAddress by model_name cos there 2 factory b04 & b05 27.10.2015

                    //Thread _tUpdateIpList = new Thread(GetIPofStation);
                    //_tUpdateIpList.IsBackground = true;
                    //_tUpdateIpList.Start();

                }
                // 12.1.2016 Check pathoss over a week
                if (flagToCheckPathlossByWeek != 0) // avoid check both by hour case and by week case
                {
                    Thread _tPathlossControl = new Thread(CheckUploadPathloss);
                    _tPathlossControl.IsBackground = true;
                    _tPathlossControl.Start();
                }


                if (FlagStopInsert == 0)
                {
                    try
                    {
                        if (ul.GetValueByKey("StartDUT") == "0" || ul.GetValueByKey("StartDUT") == "*")
                        {
                            if (ConnectServerSfis == "0")
                            {
                                //ShowUI.GET_ATE_SUM_TESTED_DUT.Servicepostdata getSumTestedDUT = new ShowUI.GET_ATE_SUM_TESTED_DUT.Servicepostdata();
                                TestedDUT = sfisB05.GET_STATION_PASS_FAIL(ul.GetModel().Trim(), sName);
                            }
                        }
                        else
                        {
                            TestedDUT = Convert.ToInt32(ul.GetValueByKey("StartDUT"));
                        }
                    }
                    catch (Exception ee)
                    {
                        event_log("Convert TestedDUT Exception: Set current TestedDUT --> " + TestedDUT + " " + ee.ToString());
                        // throw;
                    }

                    try
                    {

                        if (Line == "L" || ModelName == "X" || Station == "X" || Shift == "X" || ProgramandVersion == "X" || ChkSum == "X" || FixtureNo == "X")
                        {
                            event_log("ATE CHECKLIST FAIL: Do nothing > Line:" + Line + " ModelName:" + ModelName + " Station:" + Station + " Shift:" + Shift + " ProgramandVersion:" + ProgramandVersion + " ChkSum:" + ChkSum + " FixtureNo:" + FixtureNo + "");
                        }
                        else
                        {
                            if (ConnectServer63 == "0")
                            {
                                ShowUI.ATE_CHECKLIST.WebService svATE = new ShowUI.ATE_CHECKLIST.WebService();
                                int ReturnStatus = svATE.InsertChecklistATE(Line, ModelName, Station, Shift, ProgramandVersion, ChkSum, FixtureNo, UpdateVirusT, dtNow);
                                FlagStopInsert = 1;
                                flagToCheckPathlossByWeek = 1;
                                if (ReturnStatus != 0)
                                {
                                    event_log("ATE CHECKLIST OK -->: " + ul.GetValueByKey("SN").Trim() + " -> CurrentShift: " + CurrentShift + " -> Shift: " + Shift + " " + ModelNameChangeATE + " -> Test Time: " + ul.GetValueByKey("Testtime").Trim() + " --> " + TestedDUT + " pcs");
                                }
                            }
                        }
                    }
                    catch (Exception er)
                    {
                        event_log("ATE CHECKLIST Exception:" + er.ToString() + "Fix Date Virus" + UpdateVirusT + " " + dtNow);
                    }

                }

            }
            catch (Exception exx)
            {
                event_log("InsertPCKPI Exception: " + exx.ToString());
            }
        }



        public string GetFixtureNo()
        {
            string GetFixtureNo = ul.GetStation();
            if (GetFixtureNo == "BOOMS")
                return "BOOMS";
            else
                return GetFixtureNo;

        }

        public string GetTPGVersion()
        {
            string TPGVer = "";
            TPGVer = GetFixtureNo() + "-" + ul.GetProduct() + "-" + ul.GetValueByKey("Version");
            return TPGVer;
        }
        public string GetCkSum()
        {
            string Cksum = "";
            Cksum = ul.GetValueByKey("Checksum");
            if (Cksum == "")
            {
                Cksum = "X";
            }
            return Cksum;

        }
        public string GetUpdateAntivirusTime()
        {
            string UpdateTime = "";
            string KeyAnti = @"SOFTWARE\Symantec\SharedDefs\";
            string KeyAntiUpdate = @"SOFTWARE\Symantec\Symantec Endpoint Protection\CurrentVersion\SharedDefs\SDSDefs\";
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(KeyAnti, true);
                RegistryKey keyUpdate = Registry.LocalMachine.OpenSubKey(KeyAntiUpdate, true);

                UpdateTime = key.GetValue("NAVCORP_70", "").ToString();

                if (UpdateTime == "")
                {
                    UpdateTime = keyUpdate.GetValue("NAVCORP_70", "1990-05-06 12:00:00").ToString();
                }

                for (int i = 0; i <= 7; i++)
                {
                    DateTime dtNow = DateTime.Now.AddDays(-(i));

                    if (UpdateTime.Contains(dtNow.ToString("yyyyMMdd")))
                    {

                        UpdateTime = dtNow.ToString("yyyy-MM-dd hh:mm:ss");
                    }

                }
                return UpdateTime;
            }
            catch (Exception)
            {

                return "1990-05-06 12:00:00";
            }


        }

        private void ShellExecute(string Command)
        {
            try
            {
                Process p = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo("cmd.exe");
                startInfo.Arguments = "/C " + Command;
                startInfo.RedirectStandardError = true;
                //startInfo.RedirectStandardOutput = true;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
                p.StartInfo = startInfo;
                p.Start();
                p.WaitForExit();
            }
            catch (Exception)
            {

                //MessageBox.Show(abc.ToString());
            }

        }
        //end nhung

        //private void qAToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    using (ShowUI.QAManagement frm = new ShowUI.QAManagement())
        //    {
        //        frm.ShowDialog();
        //    }

        //}


        //

        //Date 9.52015
        //EffectivenessATEMonitoringSystem
        //Dev by Sander
        //This function will monitoring running time/ idle time/ off time of ATE

        public void ATE_INFO()
        {
            string ATE = sName;
            string IP = client_ip;
            string MAC = client_mac;
            string STATION = ul.GetStation().Trim();
            string LINE = GetLineOfTester().Trim();
            // use webserverice to insert new or update
        }


        ///11.12.2015 show mtpversion
        ///
        int flagShowMTPVer = 0;
        public void MTPVersion()
        {
            try
            {

                string ModelName = ul.GetModel().Trim();//
                string ModelFileName = IniFile.ReadIniFile("Model", ModelName, "0", @"F:\lsy\Test\DownloadConfig\ModelConfig.ini");
                string MTP_Ver = IniFile.ReadIniFile("MTP_VERSION", ModelName, "0", @"F:\lsy\Test\DownloadConfig\" + ModelFileName);
                string Enable = IniFile.ReadIniFile("MTP_VERSION", "Enable", "0", @"F:\lsy\Test\DownloadConfig\Setup.ini");
                this.Invoke((MethodInvoker)delegate
                {
                    lbMTPVer.Text = MTP_Ver;

                    if (Enable != "0" && ModelFileName != "0" && MTP_Ver != "0")
                    {
                        panel2.Visible = true;
                        flagShowMTPVer = 1;
                    }
                    else
                    {
                        panel2.Visible = false;
                        flagShowMTPVer = 0;
                    }

                    panel2.SetBounds(panel3.Location.X + panel3.Width + 5, panel3.Location.Y, lbMTP.Width + lbMTPVer.Width + 3, panel3.Height);

                });

            }
            catch (Exception r)
            {
                event_log("MTPVersion :" + r.ToString());
            }

        }

        public void SFISProtocol()
        {
            while (true)
            {
                if (NetWorkConnection)
                {
                    try
                    {
                        string ModelName = ul.GetModel().Trim();//
                        string Station = ul.GetStation().Trim();
                        string Enable = IniFile.ReadIniFile("SFIS_PROTOCOL", "Enable", "0", @"F:\lsy\Test\DownloadConfig\Setup.ini");
                        if (Enable != "0")
                        {
                            string ModelFileName = IniFile.ReadIniFile("Model", ModelName, "0", @"F:\lsy\Test\DownloadConfig\ModelConfig.ini");
                            string SfisProtocol = IniFile.ReadIniFile("SFIS_PROTOCOL", ModelName, "0", @"F:\lsy\Test\DownloadConfig\" + ModelFileName);
                            //string SfisProtocol = IniFile.ReadIniFile("SFIS_PROTOCOL", "CG3700EMR-1CMNDS", "0", @"D:\U12C206.ini");

                            string[] protocolByStation = SfisProtocol.Split(' ');
                            foreach (string protocol in protocolByStation)
                            {
                                string tmpStation = protocol.Substring(0, protocol.IndexOf("_"));
                                string sfisProtocolVal = protocol.Substring(protocol.IndexOf("_") + 1, protocol.Length - protocol.IndexOf("_") - 1);
                                //MessageBox.Show(sfisProtocolVal);
                                if (tmpStation == Station)
                                {
                                    this.Invoke((MethodInvoker)delegate
                                    {
                                        lbSFISProtocolVal.Text = sfisProtocolVal;
                                        int newSize = lbSFISProtocolVal.Width + lbSFIS.Width + 4;
                                        panel3.MaximumSize = new Size(newSize, panel3.Height);
                                        panel3.Size = new Size(newSize, panel3.Height);
                                        panel3.Visible = true;
                                    });
                                }

                            }
                        }// end if check enable
                    }
                    catch (Exception r)
                    {
                        event_log("SFISProtocol: " + r.ToString());
                    }
                } // end if check connection

                Thread.Sleep(1800000); // sleep for 30'
            }

        }

        private void lblVers_MouseHover(object sender, EventArgs e)
        {
            if (flagShowMTPVer == 0)
            {
                panel2.Visible = true;
            }
        }

        private void lblVers_MouseLeave(object sender, EventArgs e)
        {
            if (flagShowMTPVer == 0)
            {
                panel2.Visible = false;
            }
        }
        ///end 11.12.2015 show mtpversion
        ///

        /// 12.01.2016
        //  LockTester by fail to save pathloss by weekly
        public bool ForceToUploadPathloss = false;
        public void CheckUploadPathloss()
        {

            try
            {
                Thread.Sleep(5000); // to mack sure TPG test ok
                string pModifiedTime = ul.GetValueByKey("PathLossTime").Trim();
                string pSaveStatus = ul.GetValueByKey("PathLossKey").Trim();
                //string cModel = ul.GetValueByKey("SFISMODEL").Trim();

                DateTime dtpModifiedTime = DateTime.ParseExact(pModifiedTime, "yyyyMMddHHmmss", null);
                TimeSpan tsp = DateTime.Now.Subtract(dtpModifiedTime);

                if (tsp.TotalDays > 7 && pSaveStatus == "0") // for 1 week  if savepathlos not OK -> lock tester
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        //MessageBox.Show("Check by week true --> stop");
                        ForceToUploadPathloss = true;

                    });

                }
                else
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        //MessageBox.Show("Check by week is runnuing ... then stop");
                        ForceToUploadPathloss = false;
                    });

                }
            }
            catch (Exception)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    // MessageBox.Show("Exception error check by week"+r.ToString());
                    ForceToUploadPathloss = false;
                });

            }

        }

        //when change customer model then need to recalculate pathloss
        public void ForcChangePathloss()
        {

            Thread.Sleep(3600000); // check after 1 hour 
            try
            {
                string pModifiedTime = ul.GetValueByKey("PathLossTime").Trim();
                string pSaveStatus = ul.GetValueByKey("PathLossKey").Trim();

                DateTime dtpModifiedTime = DateTime.ParseExact(pModifiedTime, "yyyyMMddHHmmss", null);
                TimeSpan tsp = DateTime.Now.Subtract(dtpModifiedTime);


                if (pSaveStatus == "0" && tsp.TotalDays > 2)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        ForceToUploadPathloss = true;
                        //MessageBox.Show("Check by hour true --> stop");
                    });
                }
                else
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        ForceToUploadPathloss = false;
                        // MessageBox.Show("Thread check by hour is running .. then stop");
                    });
                }
            }
            catch (Exception r)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    //MessageBox.Show("Exception error check by hour" + r.ToString());
                    ForceToUploadPathloss = false;
                });
            }

        }
        /// 12.01.2016
        //  LockTester by fail to save pathloss by weekly

        /// <summary>
        /// 09.03.2016 Frank to upload to server
        /// </summary>
        private void timerAutoUpload_Tick(object sender, EventArgs e)
        {
            //avoid free main ui
            if (NetWorkConnection)
            {
                //Thread _tFTPCopyToServer = new Thread(CopyToServer);
                //_tFTPCopyToServer.IsBackground = true;
                //_tFTPCopyToServer.Start();
            }

            // FTPCopyToServer();
        }
        string _STR_IP_SERVER_UPLOAD = string.Empty;//@"\\10.224.81.37";
                                                    //const string _STR_PATH_CONFIG = @"\\10.224.81.60\wireless\lsy\Test\DownloadConfig";
        string _STR_PATH_CONFIG = string.Empty;//@"\\10.224.81.37\wireless\Temp\TE-PROGRAM\TE-PRO1\Frank\DownloadConfig";
                                               //public void FTPCopyToServer()
                                               //{

        //    this.Invoke((MethodInvoker)delegate
        //    {
        //        timerAutoUpload.Enabled = false;
        //    });
        //    try
        //    {
        //        string p_strProduct = this.ul.GetProduct();
        //        string p_strStation = this.ul.GetStation();
        //        if (p_strProduct != "BOOMS")
        //        {
        //            //Get parameter AtuoUpload by Product
        //            string v_strPathConfigProject = "";
        //            v_strPathConfigProject = _STR_PATH_CONFIG + "\\" + p_strProduct.ToUpper() + ".ini";
        //            //v_strPathConfigProject = @"\\10.224.81.37\wireless\Temp\TE-PROGRAM\TE-PRO1\Frank\config.ini";
        //            //event_log(v_strPathConfigProject);
        //            if (File.Exists(v_strPathConfigProject))
        //            {
        //                //Get infor config upload
        //                string v_strCopyServer = IniFile.ReadIniFile("CopyToServer", "CopyServer" + "_" + p_strStation, "", v_strPathConfigProject);
        //                int v_dTime = 2;
        //                try
        //                {
        //                    v_dTime = Convert.ToInt32(IniFile.ReadIniFile("CopyToServer", "dTime" + "_" + p_strStation, "2", v_strPathConfigProject));

        //                }
        //                catch (Exception)
        //                {
        //                }

        //                string v_strLocalPath = IniFile.ReadIniFile("CopyToServer", "sLocalPath" + "_" + p_strStation, "", v_strPathConfigProject);

        //                //2016.05.16 change to copy with ftp protocol
        //                string v_strServerPath = "ftp://" + _STR_IP_SERVER_UPLOAD;//@"\\" + _STR_IP_SERVER_UPLOAD + "\\" + IniFile.ReadIniFile("CopyToServer", "sServerPath" + "_" + p_strStation, "", v_strPathConfigProject);


        //                //MessageBox.Show(v_strLocalPath);
        //                //Check Status copy. If CopyServer=1 then excute next step

        //                if (v_strCopyServer == "1")
        //                {


        //                    string[] tmpLocalPath = v_strLocalPath.Split('|');

        //                    foreach (string localPath in tmpLocalPath)
        //                    {
        //                        //MessageBox.Show(localPath);
        //                        if (localPath != "")
        //                        {
        //                            v_strLocalPath = localPath;
        //                            //make sure path ok
        //                            if (localPath.EndsWith(@"\"))
        //                            {
        //                                v_strLocalPath = localPath.Remove(localPath.Length - 1, 1);
        //                            }

        //                            if (Directory.Exists(v_strLocalPath))
        //                            {
        //                                //if (v_strServerPath.EndsWith(@"\"))
        //                                //{
        //                                //    v_strServerPath = v_strServerPath.Remove(v_strServerPath.Length - 1, 1); ;
        //                                //}


        //                                //if (Directory.Exists(v_strServerPath))
        //                                //{

        //                                string v_strServerPathToDay = "";
        //                                string v_strServerPathPreviousDay = "";
        //                                v_strServerPathPreviousDay = v_strServerPath + "/" + p_strProduct + "/" + p_strStation + "/" + DateTime.Now.AddDays(-1).ToString("yyyyMMdd") + "/" + sName + "/" + v_strLocalPath.Remove(0, 3);
        //                                v_strServerPathToDay = v_strServerPath + "/" + p_strProduct + "/" + p_strStation + "/" + DateTime.Now.ToString("yyyyMMdd") + "/" + sName + "/" + v_strLocalPath.Remove(0, 3);

        //                                // string[] v_strfiles = Directory.GetFiles(v_strLocalPath, "*.txt", SearchOption.AllDirectories);
        //                                //Check exists new file
        //                                //string[] v_strfiles = Directory.GetFiles(v_strLocalPath, "*.log", SearchOption.AllDirectories);
        //                                string[] filters = new string[] { "*.txt", "*.log" };
        //                                foreach (string extension in filters)
        //                                {

        //                                    string[] v_strfiles = Directory.GetFiles(v_strLocalPath, extension, SearchOption.AllDirectories);
        //                                    //string[] v_strfiles = Directory.GetFiles(@"D:\AutoDL", extension, SearchOption.AllDirectories);

        //                                    if (v_strfiles.Length > 0)
        //                                    {
        //                                        //Loop all new file for copy
        //                                        foreach (string s in v_strfiles)
        //                                        {
        //                                            //Copy file from client to server.

        //                                            try
        //                                            {
        //                                                FileInfo v_objFileInfor = new FileInfo(s);

        //                                                //return;
        //                                                string v_strPathServerFile = "";
        //                                                bool v_blnCheckDateCopy = false;

        //                                                string tmpServerPath = s.Replace(v_strLocalPath, "");

        //                                                if (v_objFileInfor.LastWriteTime.ToString("yyyyMMdd") == DateTime.Now.ToString("yyyyMMdd"))
        //                                                {
        //                                                    v_strPathServerFile = v_strServerPathToDay + tmpServerPath;
        //                                                    v_blnCheckDateCopy = true;
        //                                                }
        //                                                else if (v_objFileInfor.LastWriteTime.ToString("yyyyMMdd") == DateTime.Now.AddDays(-1).ToString("yyyyMMdd"))
        //                                                {
        //                                                    v_strPathServerFile = v_strServerPathPreviousDay + tmpServerPath;
        //                                                    v_blnCheckDateCopy = true;
        //                                                }

        //                                                //
        //                                                if (v_blnCheckDateCopy)
        //                                                {

        //                                                    byte[] fData = DoBuffering(s);
        //                                                    fPath = v_strPathServerFile.Replace(v_objFileInfor.Name, "").Replace("ftp://" + _STR_IP_SERVER_UPLOAD + "/", "").Replace(@"\", "/");

        //                                                    ftpCreateDir("ftp://10.224.81.37/", fPath, "CopyLogfile", "CopyLogfile");

        //                                                    fPath = fPath + v_objFileInfor.Name;
        //                                                    ftpUpload(fPath, "CopyLogfile", "CopyLogfile", fData);
        //                                                    //Sucess then change file type client to .log                                        
        //                                                    v_objFileInfor.MoveTo(Path.ChangeExtension(s, ".inf"));
        //                                                }
        //                                                else
        //                                                { //Khong copy cung change type
        //                                                    v_objFileInfor.MoveTo(Path.ChangeExtension(s, ".inf"));
        //                                                }
        //                                            }
        //                                            catch (Exception)
        //                                            {
        //                                                continue;
        //                                            }

        //                                            Thread.Sleep(1000);
        //                                        }
        //                                    }

        //                                } // end foreach filter

        //                                //} // end check serverpath exist
        //                            }
        //                        }//end if localPath!= ""

        //                    }// end forach to get localPath
        //                    //2. Set timer interval flow config file
        //                    this.Invoke((MethodInvoker)delegate
        //                    {
        //                        timerAutoUpload.Interval = v_dTime * 60000;
        //                    });

        //                }
        //            }
        //        }

        //    }
        //    catch (Exception r)
        //    {
        //        //MessageBox.Show(r.ToString());
        //        event_log("CopyLogToServer: " + r.ToString());
        //    }

        //    this.Invoke((MethodInvoker)delegate
        //    {
        //        timerAutoUpload.Enabled = true;
        //    });


        //}


        public void CopyToServer()
        {
            this.Invoke((MethodInvoker)delegate
            {
                timerAutoUpload.Enabled = false;
            });

            try
            {
                string p_strProduct = this.ul.GetProduct();
                string p_strStation = this.ul.GetStation();

                if (p_strProduct != "BOOMS")
                {
                    //Get parameter AtuoUpload by Product
                    string v_strPathConfigProject = "";
                    v_strPathConfigProject = _STR_PATH_CONFIG + "\\" + p_strProduct.ToUpper() + ".ini";
                    //v_strPathConfigProject = @"\\10.224.81.37\wireless\Temp\TE-PROGRAM\TE-PRO1\Frank\config.ini";
                    //event_log(v_strPathConfigProject);
                    if (File.Exists(v_strPathConfigProject))
                    {
                        //Get infor config upload
                        string v_strCopyServer = IniFile.ReadIniFile("CopyToServer", "CopyServer" + "_" + p_strStation, "", v_strPathConfigProject);
                        int v_dTime = 2;
                        try
                        {
                            v_dTime = Convert.ToInt32(IniFile.ReadIniFile("CopyToServer", "dTime" + "_" + p_strStation, "2", v_strPathConfigProject));

                        }
                        catch (Exception)
                        {
                        }

                        string v_strLocalPath = IniFile.ReadIniFile("CopyToServer", "sLocalPath" + "_" + p_strStation, "", v_strPathConfigProject);

                        string v_strServerPath = @"\\" + _STR_IP_SERVER_UPLOAD + "\\" + IniFile.ReadIniFile("CopyToServer", "sServerPath" + "_" + p_strStation, "", v_strPathConfigProject);

                        //Check Status copy. If CopyServer=1 then excute next step

                        if (v_strCopyServer == "1")
                        {


                            string[] tmpLocalPath = v_strLocalPath.Split('|');

                            foreach (string localPath in tmpLocalPath)
                            {
                                if (localPath != "")
                                {
                                    v_strLocalPath = localPath;
                                    //make sure path ok
                                    if (localPath.EndsWith(@"\"))
                                    {
                                        v_strLocalPath = localPath.Remove(localPath.Length - 1, 1);
                                    }

                                    if (Directory.Exists(v_strLocalPath))
                                    {
                                        if (v_strServerPath.EndsWith(@"\"))
                                        {
                                            v_strServerPath = v_strServerPath.Remove(v_strServerPath.Length - 1, 1);
                                            ;
                                        }

                                        if (Directory.Exists(v_strServerPath))
                                        {
                                            //MessageBox.Show(v_strServerPath);
                                            string v_strServerPathToDay = "";
                                            string v_strServerPathPreviousDay = "";
                                            v_strServerPathPreviousDay = v_strServerPath + "\\" + p_strProduct + "\\" + p_strStation + "\\" + DateTime.Now.AddDays(-1).ToString("yyyyMMdd") + "\\" + sName + "\\" + v_strLocalPath.Remove(0, 3);
                                            v_strServerPathToDay = v_strServerPath + "\\" + p_strProduct + "\\" + p_strStation + "\\" + DateTime.Now.ToString("yyyyMMdd") + "\\" + sName + "\\" + v_strLocalPath.Remove(0, 3);

                                            // string[] v_strfiles = Directory.GetFiles(v_strLocalPath, "*.txt", SearchOption.AllDirectories);
                                            //Check exists new file
                                            //string[] v_strfiles = Directory.GetFiles(v_strLocalPath, "*.log", SearchOption.AllDirectories);
                                            string[] filters = new string[] { "*.txt", "*.log" };
                                            foreach (string extension in filters)
                                            {
                                                string[] v_strfiles = Directory.GetFiles(v_strLocalPath, extension, SearchOption.AllDirectories);
                                                //string[] v_strfiles = Directory.GetFiles(@"D:\AutoDL", extension, SearchOption.AllDirectories);
                                                //MessageBox.Show(v_strLocalPath);
                                                if (v_strfiles.Length > 0)
                                                {
                                                    //Loop all new file for copy
                                                    foreach (string s in v_strfiles)
                                                    {
                                                        //Copy file from client to server.

                                                        try
                                                        {
                                                            FileInfo v_objFileInfor = new FileInfo(s);
                                                            string v_strPathServerFile = "";
                                                            bool v_blnCheckDateCopy = false;

                                                            string tmpServerPath = s.Replace(v_strLocalPath, "");

                                                            if (v_objFileInfor.LastWriteTime.ToString("yyyyMMdd") == DateTime.Now.ToString("yyyyMMdd"))
                                                            {
                                                                v_strPathServerFile = v_strServerPathToDay + tmpServerPath;
                                                                v_blnCheckDateCopy = true;
                                                            }
                                                            else if (v_objFileInfor.LastWriteTime.ToString("yyyyMMdd") == DateTime.Now.AddDays(-1).ToString("yyyyMMdd"))
                                                            {
                                                                v_strPathServerFile = v_strServerPathPreviousDay + tmpServerPath;
                                                                v_blnCheckDateCopy = true;
                                                            }
                                                            //v_strPathServerFile = v_strPathServerFile.Replace(@"\wireless\lsy\","");
                                                            //MessageBox.Show(v_strPathServerFile);
                                                            if (v_blnCheckDateCopy)
                                                            {
                                                                //make sure address ok
                                                                //Create folder before copying
                                                                CreateDir(v_strPathServerFile.Replace(v_objFileInfor.Name, ""));
                                                                File.Copy(s, v_strPathServerFile, true);
                                                                //Sucess then change file type client to .log                                        
                                                                v_objFileInfor.MoveTo(Path.ChangeExtension(s, ".inf"));
                                                            }
                                                            else
                                                            {
                                                                //Khong copy cung change type
                                                                v_objFileInfor.MoveTo(Path.ChangeExtension(s, ".inf"));
                                                            }
                                                        }
                                                        catch (Exception e)
                                                        {

                                                            continue;
                                                        }
                                                    }
                                                }

                                            } // end foreach filter

                                        } // end check serverpath exist
                                    }
                                }//end if localPath!= ""

                            }// end forach to get localPath
                             //2. Set timer interval flow config file

                            this.Invoke((MethodInvoker)delegate
                            {
                                timerAutoUpload.Interval = v_dTime * 60000;
                            });
                        }
                    }
                }

            }
            catch (Exception r)
            {
                event_log("CopyLogToServer: " + r.ToString());
            }

            this.Invoke((MethodInvoker)delegate
            {
                timerAutoUpload.Enabled = true;
            });


        }
        //byte[] buffer;
        string fPath = "";
        public void ftpUpload(string fPath, string user, string password, byte[] fData)
        {
            try
            {

                string fServerCreate = "ftp://10.224.81.37/";
                string username = user;
                string pw = password;
                //MessageBox.Show(fServerCreate + fPath);
                // ftpUpload("" + fPath + , "CopyLogfile", "CopyLogfile", fData);

                FtpWebRequest rq = (FtpWebRequest)FtpWebRequest.Create(fServerCreate + fPath);
                rq.Method = WebRequestMethods.Ftp.UploadFile;
                rq.Credentials = new NetworkCredential(username, pw);
                //rq.KeepAlive = true;
                Stream reqStream = rq.GetRequestStream();
                reqStream.Write(fData, 0, fData.Length);
                reqStream.Close();
                var resp = (FtpWebResponse)rq.GetResponse();
                //rq.KeepAlive = false;

            }
            catch (Exception e)
            {
                event_log("FTPCopy: " + e.ToString());
            }
            //fName is full link to server
        }
        public void ftpCreateDir(string ftpIp, string fPath, string username, string pw)
        {

            try
            {
                string[] subForlders = fPath.Split('/');
                string tmpPath = ftpIp; // format is ftp://10.224.81.37/
                foreach (string subf in subForlders)
                {
                    try
                    {
                        tmpPath += subf + "/";
                        WebRequest rq = WebRequest.Create(tmpPath);
                        rq.Method = WebRequestMethods.Ftp.MakeDirectory;
                        rq.Credentials = new NetworkCredential(username, pw);
                        FtpWebResponse resp = (FtpWebResponse)rq.GetResponse();
                        Stream ftpStream = resp.GetResponseStream();
                        ftpStream.Close();
                        resp.Close();
                        //MessageBox.Show("OK");
                    }
                    catch (Exception)
                    {
                        //MessageBox.Show("Continue");
                        continue;
                    }

                }

            }
            catch (Exception r)
            {
                //MessageBox.Show(r.ToString());
                event_log("ftp Create Dir: " + r.ToString());
            }

        }
        public byte[] DoBuffering(string fPath)
        {
            byte[] fData = new byte[1028];
            try
            {
                FileStream fs = new FileStream(fPath, FileMode.Open, FileAccess.Read);
                fData = new byte[fs.Length];
                fs.Read(fData, 0, Convert.ToInt32(fs.Length));
                fs.Close();
            }
            catch (Exception)
            {
            }
            return fData;
        }
        public void CreateDir(string fPath)
        {
            this.Invoke((MethodInvoker)delegate
           {
               try
               {
                   if (!Directory.Exists(fPath))
                   {
                       Directory.CreateDirectory(fPath);
                   }
               }
               catch (Exception r)
               {
                   event_log("CopyLogToServer: " + r.ToString());
               }
           });

        }
        public void NetAuthentication()
        {

            try
            {
                string cmd = @"net use \\10.224.81.60\wireless /user:oper2 wireless";
                ShellExecute(cmd);
                NetworkCredential _netAuthentication = new NetworkCredential("oper2", "wireless");
                CredentialCache _netCache = new CredentialCache();
                _netCache.Add(new Uri(@"\\10.224.81.60\wireless"), "Basic", _netAuthentication);

                event_log("NetAuthentication: Maped to wireless folder in server 60 OK!");
            }
            catch (Exception rd)
            {
                event_log("NetAuthentication To 60: " + rd.ToString());
            }
            //
            try
            {
                string cmd = @"net use \\10.224.81.37\wireless /user:oper2 wireless";
                ShellExecute(cmd);
                NetworkCredential _netAuthentication = new NetworkCredential("oper2", "wireless");
                CredentialCache _netCache = new CredentialCache();
                _netCache.Add(new Uri(@"\\10.224.81.37\wireless"), "Basic", _netAuthentication);

                event_log("NetAuthentication: Maped to wireless folder in server 37 OK!");
            }
            catch (Exception rd)
            {
                event_log("NetAuthentication To 37: " + rd.ToString());
            }
            //
        }


        /// <summary>
        /// 2016.0326 Function to check chanel spec in Chanel.txt if 2G >= 14 || 5G < 14 => don't let test
        /// </summary>
        string _checkroduct = "";
        string _checkmodel = "";
        string _checkserverPath = "";
        string _checkgetSection = "";
        string _checklocalCheckPath = "";
        public void getPathForCheckChanelSpecRange()
        {
            try
            {
                event_log("Chanel Spec Range Check: Running!");
                _checkroduct = ul.GetProduct().Trim();
                _checkmodel = ul.GetModel().Trim();
                //AutoClosingMessageBox.Show(model, "AutoCloseMessageBox", 10000);
                _checkserverPath = @"\\10.224.81.60\wireless\lsy\Test\DownloadConfig\" + _checkroduct + ".ini";
                //AutoClosingMessageBox.Show(serverPath, "AutoCloseMessageBox", 10000);
                _checkgetSection = IniFile.ReadIniFile("ModelSection", _checkmodel, "", _checkserverPath);
                //AutoClosingMessageBox.Show(getSection, "AutoCloseMessageBox", 10000);
                _checklocalCheckPath = IniFile.ReadIniFile(_checkgetSection, "DEST_PATH_" + ul.GetStation().Trim(), "", _checkserverPath);
                //AutoClosingMessageBox.Show(localCheckPath, "AutoCloseMessageBox", 10000);
                _checklocalCheckPath = _checklocalCheckPath + @"\Channel.txt";
                _checklocalCheckPath = _checklocalCheckPath.Replace(@"\\", @"\");

            }
            catch (Exception r)
            {
                event_log("getPathForCheckChanelSpecRange: " + r.ToString());
            }

        }

        public void CheckChanelSpecRange()
        {
            getPathForCheckChanelSpecRange();

            while (true)
            {
                try
                {
                    ///AutoClosingMessageBox.Show(_checklocalCheckPath, "AutoCloseMessageBox", 10000);
                    if (DateTime.Now.Minute % 10 == 0)
                    {
                        getPathForCheckChanelSpecRange();

                        //AutoClosingMessageBox.Show(_checklocalCheckPath, "AutoCloseMessageBox", 10000);
                    }
                    if (File.Exists(_checklocalCheckPath))
                    {
                        StreamReader readdata = new StreamReader(_checklocalCheckPath);
                        while (readdata.Peek() != -1)
                        {
                            string textline = readdata.ReadLine();
                            if (textline.Contains("2G") || textline.Contains("5G"))
                            {
                                string[] tmpStr = textline.Split('=');

                                if (tmpStr.Length > 1)
                                {
                                    if (tmpStr[1].ToString().Trim() != "")
                                    {
                                        int specChanel = 0;
                                        try
                                        {
                                            specChanel = Convert.ToInt32(tmpStr[1].ToString());
                                        }
                                        catch (Exception)
                                        {
                                            ForceReDownloadTPG();
                                            AutoClosingMessageBox.Show("Config file không đúng: giá trị Chanel phải là số nguyên trong file: \n" + _checklocalCheckPath, "AutoCloseMessageBox", 300000);
                                            break;
                                        }

                                        if (textline.Contains("2G") && specChanel >= 14)
                                        {
                                            ForceReDownloadTPG();
                                            AutoClosingMessageBox.Show("Config file không đúng: giá trị 2G Chanel phải nhỏ hơn 14 trong file: \n" + _checklocalCheckPath, "AutoCloseMessageBox", 300000);
                                            break;
                                        }

                                        if (textline.Contains("5G") && specChanel < 14)
                                        {
                                            ForceReDownloadTPG();
                                            AutoClosingMessageBox.Show("Config file không đúng:  giá trị 5G Chanel phải lớn hơn 36 trong file: \n" + _checklocalCheckPath, "AutoCloseMessageBox", 300000);
                                            break;
                                        }
                                    }
                                }
                            }// end if textline.Contains("2G") || textline.Contains("5G"

                        }//end while
                        readdata.Close();
                    }//end check if file exist

                    //return rturnVal;
                }
                catch (Exception r)
                {
                    event_log("CheckChanelSpecRange: " + r.ToString());
                    // return rturnVal;
                }

                Thread.Sleep(60000); // delay 30s
            }//end while loop
        }


        /// Show ConnectorUseTime/ TPG version /Pathloss
        /// 
        public void ShowDataByWebpage(string weblink)
        {
            try
            {

                string webAddress = weblink;//;// account to autologin & redirect to ShowData folder
                Process.Start(webAddress);
            }
            catch (Exception)
            {
            }

        }

        private void mnTool_Click(object sender, EventArgs e)
        {

        }

        string globalUserName = "";
        public bool LoginAuthentication()
        {
            string svIp = ul.GetServerIP("SSO", "10.224.81.62");
            //MessageBox.Show(svIp);
            tmpIps += " SSO:10.224.81.62";
            string connectionString = @"Data Source=10.224.81.62;Initial Catalog=SSO;uid=sa;pwd=********;Connection Timeout=5";

            bool CheckResult = false;
            using (WarningButton wb = new WarningButton())
            {
                if (wb.ShowDialog() == DialogResult.OK)
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        try
                        {
                            string username = wb.getUsername().Trim();
                            globalUserName = username;

                            connection.Open();
                            SqlDataReader reader;
                            string queryString = "select username, password, dep from Users where username= @ur and password =@pw";
                            SqlCommand command = new SqlCommand(queryString, connection);
                            command.Parameters.Add("@ur", SqlDbType.NVarChar, 50);
                            command.Parameters["@ur"].Value = wb.getUsername();
                            command.Parameters.Add("@pw", SqlDbType.NVarChar, 50);
                            command.Parameters["@pw"].Value = wb.getPassword();

                            reader = command.ExecuteReader();
                            if (!reader.HasRows)
                            {
                                reader.Close();

                                MessageBox.Show("Sai tên đăng nhập hoặc mật khẩu !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);

                                CheckResult = false;

                            }
                            else
                            {
                                CheckResult = true;

                            }
                            //ul.SetValueByKey("EMPLOYEE_ID", username);
                            reader.Close();
                            connection.Close();
                        }
                        catch (Exception exp)
                        {
                            CheckResult = false;
                            //MessageBox.Show("Can't connect to server !","Error",MessageBoxButtons.OK,MessageBoxIcon.Stop);
                            event_log("LoginAuthentication: " + exp.Message.ToString());
                        }
                    }
                }
            }
            return CheckResult;
        }

        private void abnormalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (ShowUI.frmEmpAuthentication ea = new ShowUI.frmEmpAuthentication("0"))
                {
                    ea.ShowDialog();
                }
                //if (LoginAuthentication())
                //{
                //    string model_name = ul.GetValueByKey("SFISMODEL").Trim();
                //    string ate_name = sName;
                //    string error_code = ul.GetValueByKey("ERRORCODE").Trim();
                //    string username = ul.GetValueByKey("EMPLOYEE_ID").Trim();

                //    string webAddress = "http://10.224.81.63/CenterAction/Action/AddActionAbnormal.aspx?ModelName=" + model_name + "&TesterName=" + ate_name + "&ErrorCode=" + error_code + "&DateTime=" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "&BU=B05&Owner=" + username + "";// account to autologin & redirect to ShowData folder
                //    Process.Start(webAddress);
                //}

            }
            catch (Exception)
            {
            }
        }


        private void eGroupCreationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists(@"D:\AutoDL\EGroupCreation.exe"))
                {
                    File.SetAttributes(@"D:\AutoDL\EGroupCreation.exe", FileAttributes.Normal);
                    ShellExecute("taskkill /IM EGroupCreation.exe /T /F");

                    System.Diagnostics.FileVersionInfo AD_server = System.Diagnostics.FileVersionInfo.GetVersionInfo(@"F:\lsy\Test\DownloadConfig\AutoDL\EGroupCreation.exx");
                    Version AD_SV_ver = new Version(AD_server.FileVersion);
                    System.Diagnostics.FileVersionInfo AD_local = System.Diagnostics.FileVersionInfo.GetVersionInfo(@"D:\AutoDL\EGroupCreation.exe");
                    Version AD_LC_ver = new Version(AD_local.FileVersion);
                    switch (AD_SV_ver.CompareTo(AD_LC_ver))
                    {
                        case 1:
                            System.Diagnostics.Process[] kill_AD = System.Diagnostics.Process.GetProcessesByName("EGroupCreation");
                            foreach (System.Diagnostics.Process p in kill_AD)
                                p.Kill();

                            File.Copy(@"F:\lsy\Test\DownloadConfig\AutoDL\EGroupCreation.exx", @"D:\AutoDL\EGroupCreation.exe", true);

                            event_log("Update EGroupCreation: OK");
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    File.Copy(@"F:\lsy\Test\DownloadConfig\AutoDL\EGroupCreation.exx", @"D:\AutoDL\EGroupCreation.exe", true);
                }
                Process.Start(@"D:\AutoDL\EGroupCreation.exe");

            }
            catch (Exception er)
            {
                //MessageBox.Show(er.ToString());
            }
        }

        private void webShowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowDataByWebpage("http://10.224.81.63/PCKPI/Default.aspx?id=ShowDataAutoLogin&pw=@pw&auto=true");
        }

        private void mydasVnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowDataByWebpage("http://10.224.81.63/Mydasvn/DefaultPage.aspx?id=AutoLoginUser&pw=AutoLoginUser&auto=true");
        }

        private void lblShowUI_MouseHover(object sender, EventArgs e)
        {
            if (!npiflag)
            {
                int XflagLocation = Convert.ToInt32(lblYeildRate.Location.X.ToString());
                int YflagLocation = Convert.ToInt32(lblYeildRate.Location.Y.ToString());
                fPanelDate.SetBounds(XflagLocation + lblYeildRate.Width, YflagLocation + 2, 133, 20);
                fPanelDate.BackColor = panel1.BackColor;

                fPanelDate.Visible = true;
                fPanelHData.Visible = false;
            }
            if (timercheckGoiTE.Enabled)
            {
                timercheckGoiTE.Enabled = false;
            }

            btnCall.Visible = true;
            timercheckGoiTE.Enabled = true;

        }
        private void lblShowUI_MouseLeave(object sender, EventArgs e)
        {
            if (!npiflag)
            {
                fPanelDate.Visible = false;
                fPanelHData.Visible = true;
            }

        }
        private void lblShowUI_Click(object sender, EventArgs e)
        {
            //    ToolTip tl = new ToolTip();
            //    tl.Active = true;

            //    tl.ToolTipTitle = "Date" + DateTime.Now.ToString("yyyy-MM-dd");
            //    tl.SetToolTip(lblShowUI, "");
        }


        //ShowUI.GET_ATE_SUM_TESTED_DUT.Servicepostdata svGetData = new ShowUI.GET_ATE_SUM_TESTED_DUT.Servicepostdata();
        string globalStation = "";
        public string ShowfPanelHStation()
        {
            string StationName = "";

            while (true)
            {
                if (!NetWorkConnection)
                {
                    break;
                }
                if (globalStation != "")
                {
                    break;
                }

                string SN = ul.GetValueByKey("SN").Trim();
                if (SN != "" && SN != "DEFAULT")
                {
                    DataTable dt = getSumTestedDUT.GET_WIP_GROUP(SN);
                    if (dt.Rows.Count != 0)
                    {
                        StationName = dt.Rows[0]["WIP_GROUP"].ToString();//@ZO60XV5
                        if (StationName != "")
                        {
                            globalStation = StationName;
                            break;
                        }
                    }
                }
                Thread.Sleep(10000);
            }
            event_log("ShowfPanelHStation: globalStation= " + globalStation);
            return globalStation;

        }

        public string ShowfPanelHRTRate(string start_time, string end_time)
        {

            double RTRate = 0;
            double FIRST_FAIL_QTY = 0;
            double FAIL_QTY = 0;
            double REPAIR_QTY = 0;
            double WIP_QTY = 0;
            double c_fail = 0;

            string modelName = ul.GetValueByKey("SFISMODEL").Trim(); //"C6250-100NASV1";//
            string stationName = globalStation;// ShowfPanelHStation();//"PT";//
                                               //sName = "VN-B5L12-PT6";
            if (NetWorkConnection)
            {
                //sName = "FOX-B05L6PT2";
                DataTable dtRTRate = sfisB05.GET_FULL_R_STATION_ATE_T(start_time, end_time);

                DataRow[] results = dtRTRate.Select("STATION_NAME = '" + sName + "' AND MODEL_NAME='" + modelName + "' AND GROUP_NAME='" + stationName + "'");
                event_log("GET_FULL_R_STATION_ATE_T Rows: " + dtRTRate.Rows.Count + " && results Rows: " + results.Length + " >> STATION_NAME = '" + sName + "' AND MODEL_NAME='" + modelName + "' AND GROUP_NAME='" + stationName + "'");
                //event_log("STATION_NAME = '" + sName + "' AND MODEL_NAME='" + modelName + "' AND GROUP_NAME='" + stationName + "'");
                foreach (DataRow data in results)
                {
                    try
                    {
                        FIRST_FAIL_QTY += Convert.ToDouble(data["FIRST_FAIL_QTY"].ToString());
                        FAIL_QTY += Convert.ToDouble(data["FAIL_QTY"].ToString());
                        REPAIR_QTY += Convert.ToDouble(data["REPAIR_QTY"].ToString());
                        WIP_QTY += Convert.ToDouble(data["WIP_QTY"].ToString());

                    }
                    catch (Exception w)
                    {
                        event_log("ShowfPanelHRTRate Exception: " + w.ToString());
                    }
                }

                try
                {
                    c_fail = FIRST_FAIL_QTY - FAIL_QTY - REPAIR_QTY;
                    // for 2 case make calculate wrong
                    if (c_fail < 0)
                    {
                        c_fail = 0;
                    }
                    if (WIP_QTY < c_fail)
                    {
                        WIP_QTY = c_fail;
                    }
                    // for qty test is small
                    if (WIP_QTY != 0)
                    {
                        if (WIP_QTY >= 30)
                        {
                            RTRate = Math.Round(c_fail / WIP_QTY * 100, 2);
                        }
                    }

                }
                catch (Exception r)
                {
                    event_log("ShowfPanelHRTRate Exception: " + r.ToString());
                    RTRate = 0;
                }
            } //
              //
            return RTRate.ToString();
        }
        public string ShowfPanelHYRate(string start_time, string end_time)
        {
            // show retest rate yeild rate by range of time
            string rYRate = "100.0";

            string modelName = ul.GetValueByKey("SFISMODEL").Trim();//"C6250-100NASV1";//
                                                                    //string line = GetLineOfTester();
            string stationName = globalStation;// ShowfPanelHStation();ShowfPanelHStation();//"PT";// "PT";//
            if (NetWorkConnection)
            {
                //start_time = "201606040230";
                //end_time = "201606040730";
                DataTable dtYRate = sfisB05.GET_R_STATION_REC_T(start_time, end_time);
                DataRow[] results = dtYRate.Select("SECTION_NAME = 'SI' AND MODEL_NAME='" + modelName + "' AND GROUP_NAME='" + stationName + "'");
                event_log("GET_R_STATION_REC_T Rows: " + dtYRate.Rows.Count + " && results Rows: " + results.Length + " >> SECTION_NAME = 'SI' AND MODEL_NAME='" + modelName + "' AND GROUP_NAME='" + stationName + "'");
                double pQty = 0;
                double fQty = 0;
                double YRate = 100.0;

                foreach (DataRow data in results)
                {
                    try
                    {
                        pQty += Convert.ToDouble(data["PASS_QTY"].ToString());
                        fQty += Convert.ToDouble(data["FAIL_QTY"].ToString());
                    }
                    catch (Exception r)
                    {
                        event_log("ShowfPanelHYRate Exception: " + r.ToString());
                    }
                }

                try
                {
                    if ((pQty + fQty) != 0)
                    {
                        if ((pQty + fQty) >= 50)
                        {
                            YRate = Math.Round(pQty / (pQty + fQty) * 100, 2);
                        }
                    }
                }
                catch (Exception r)
                {
                    event_log("ShowfPanelHYRate Exception: " + r.ToString());
                }

                rYRate = YRate.ToString();
            }
            //MessageBox.Show(rYRate.ToString());
            return rYRate;
        }

        public void doFakeHDataPanel(double flag)
        {
            string YRate = "100.0";
            string RTRate = "0.0";
            // if fake du lieu thi` green het
            string starttime = DateTime.Now.ToString("yyyyMMdd") + "0000";
            string endtime = DateTime.Now.ToString("yyyyMMdd") + "0000";
            this.Invoke((MethodInvoker)delegate
            {
                //1 if 7:30 ~ 9:30 -> 02:30~7:30

                if (flag > 730 && flag <= 930)
                {
                    lbhDuration.Text = "04:30~7:30";
                    starttime = DateTime.Now.ToString("yyyyMMdd") + "0430";
                    endtime = DateTime.Now.ToString("yyyyMMdd") + "0730";

                }
                //9:30 ~ 11:40; => show data 7:30~9:30
                if (flag > 930 && flag <= 1140)
                {

                    lbhDuration.Text = "07:30~9:30";
                    starttime = DateTime.Now.ToString("yyyyMMdd") + "0730";
                    endtime = DateTime.Now.ToString("yyyyMMdd") + "0930";

                }
                // 11:40 ~ 14:30; => show data 9:30 ~ 11:40

                if (flag > 1140 && flag <= 1430)
                {
                    lbhDuration.Text = "09:30~11:40";
                    starttime = DateTime.Now.ToString("yyyyMMdd") + "0930";
                    endtime = DateTime.Now.ToString("yyyyMMdd") + "1140";

                }

                //14:30~16:20; => show data 11:40 ~ 14:30

                if (flag > 1430 && flag <= 1620)
                {
                    lbhDuration.Text = "11:40~14:30";
                    starttime = DateTime.Now.ToString("yyyyMMdd") + "1140";
                    endtime = DateTime.Now.ToString("yyyyMMdd") + "1430";

                }

                //16:20~18:40 => show data 14:30~16:20; )
                if (flag > 1620 && flag <= 1840)
                {
                    lbhDuration.Text = "14:30~16:20";
                    starttime = DateTime.Now.ToString("yyyyMMdd") + "1430";
                    endtime = DateTime.Now.ToString("yyyyMMdd") + "1620";

                }
                //19:30 ~ 21:30 => 14:30~19:30

                if (flag > 1930 && flag <= 2130)
                {
                    lbhDuration.Text = "14:30~19:30";
                    starttime = DateTime.Now.ToString("yyyyMMdd") + "1430";
                    endtime = DateTime.Now.ToString("yyyyMMdd") + "1930";

                }

                // 21:30 ~ 11:40 =>> 1930~2130
                if (flag > 2130 && flag <= 2340)
                {
                    lbhDuration.Text = "19:30~21:30";
                    starttime = DateTime.Now.ToString("yyyyMMdd") + "1930";
                    endtime = DateTime.Now.ToString("yyyyMMdd") + "2130";

                }
                //23:40 ~ 02:30; 
                // This need to ^^
                if ((flag > 2340 && flag <= 2359) || (flag >= 0 && flag <= 230))
                {
                    lbhDuration.Text = "21:30~23:30";
                    starttime = DateTime.Now.ToString("yyyyMMdd") + "2130";
                    endtime = DateTime.Now.ToString("yyyyMMdd") + "2330";

                }
                //02:30~04:20 ==> 23:40~02:30
                if (flag > 230 && flag <= 420)
                {
                    lbhDuration.Text = "23:40~02:30";
                    starttime = DateTime.Now.ToString("yyyyMMdd") + "2340";
                    endtime = DateTime.Now.ToString("yyyyMMdd") + "0230";

                }
                //4:20~7:30 ->2:30~04:20
                if (flag > 420 && flag <= 730)
                {
                    lbhDuration.Text = "02:30~04:20";
                    starttime = DateTime.Now.ToString("yyyyMMdd") + "0230";
                    endtime = DateTime.Now.ToString("yyyyMMdd") + "0420";

                }

            });
            this.Invoke((MethodInvoker)delegate
            {
                lbhYRate.Text = "100.0%";
                lbhRTRate.Text = "0.0%";
            });
        }

        public void ShowfPanelHData()
        {
            try
            {
                globalStation = ShowfPanelHStation();
                //MessageBox.Show(globalStation);
                //Thread.Sleep(10000); doFakeHDataPanel(Convert.ToDouble(DateTime.Now.ToString("HHmm")));
                double flag = Convert.ToDouble(DateTime.Now.ToString("HHmm"));

                string YRate = "100.0";
                string RTRate = "0.0";


                if (fake)
                {

                }
                else
                {
                    string starttime = DateTime.Now.ToString("yyyyMMdd") + "0000";
                    string endtime = DateTime.Now.ToString("yyyyMMdd") + "0000";
                    this.Invoke((MethodInvoker)delegate
                    {
                        //1 if 7:30 ~ 9:30 -> 02:30~7:30

                        if (flag > 730 && flag <= 930)
                        {
                            lbhDuration.Text = "04:30~7:30";
                            starttime = DateTime.Now.ToString("yyyyMMdd") + "0430";
                            endtime = DateTime.Now.ToString("yyyyMMdd") + "0730";

                        }
                        //9:30 ~ 11:40; => show data 7:30~9:30
                        if (flag > 930 && flag <= 1140)
                        {

                            lbhDuration.Text = "07:30~9:30";
                            starttime = DateTime.Now.ToString("yyyyMMdd") + "0730";
                            endtime = DateTime.Now.ToString("yyyyMMdd") + "0930";

                        }
                        // 11:40 ~ 14:30; => show data 9:30 ~ 11:40

                        if (flag > 1140 && flag <= 1430)
                        {
                            lbhDuration.Text = "09:30~11:40";
                            starttime = DateTime.Now.ToString("yyyyMMdd") + "0930";
                            endtime = DateTime.Now.ToString("yyyyMMdd") + "1140";

                        }

                        //14:30~16:20; => show data 11:40 ~ 14:30

                        if (flag > 1430 && flag <= 1620)
                        {
                            lbhDuration.Text = "11:40~14:30";
                            starttime = DateTime.Now.ToString("yyyyMMdd") + "1140";
                            endtime = DateTime.Now.ToString("yyyyMMdd") + "1430";

                        }

                        //16:20~18:40 => show data 14:30~16:20; )
                        if (flag > 1620 && flag <= 1840)
                        {
                            lbhDuration.Text = "14:30~16:20";
                            starttime = DateTime.Now.ToString("yyyyMMdd") + "1430";
                            endtime = DateTime.Now.ToString("yyyyMMdd") + "1620";

                        }
                        //19:30 ~ 21:30 => 14:30~19:30

                        if (flag > 1930 && flag <= 2130)
                        {
                            lbhDuration.Text = "14:30~19:30";
                            starttime = DateTime.Now.ToString("yyyyMMdd") + "1430";
                            endtime = DateTime.Now.ToString("yyyyMMdd") + "1930";

                        }

                        // 21:30 ~ 11:40 =>> 1930~2130
                        if (flag > 2130 && flag <= 2340)
                        {
                            lbhDuration.Text = "19:30~21:30";
                            starttime = DateTime.Now.ToString("yyyyMMdd") + "1930";
                            endtime = DateTime.Now.ToString("yyyyMMdd") + "2130";

                        }
                        //23:40 ~ 02:30; 
                        // This need to ^^
                        if ((flag > 2340 && flag <= 2359) || (flag >= 0 && flag <= 230))
                        {
                            lbhDuration.Text = "21:30~23:30";
                            starttime = DateTime.Now.ToString("yyyyMMdd") + "2130";
                            endtime = DateTime.Now.ToString("yyyyMMdd") + "2330";

                        }
                        //02:30~04:20 ==> 23:40~02:30
                        if (flag > 230 && flag <= 420)
                        {
                            lbhDuration.Text = "23:40~02:30";
                            starttime = DateTime.Now.ToString("yyyyMMdd") + "2340";
                            endtime = DateTime.Now.ToString("yyyyMMdd") + "0230";

                        }
                        //4:20~7:30 ->2:30~04:20
                        if (flag > 420 && flag <= 730)
                        {
                            lbhDuration.Text = "02:30~04:20";
                            starttime = DateTime.Now.ToString("yyyyMMdd") + "0230";
                            endtime = DateTime.Now.ToString("yyyyMMdd") + "0420";

                        }

                    });

                    if (NetWorkConnection)
                    {
                        RTRate = ShowfPanelHRTRate(starttime, endtime);
                        Thread.Sleep(1500);
                        YRate = ShowfPanelHYRate(starttime, endtime);
                    }

                    if (!RTRate.Contains("."))
                    {
                        RTRate = RTRate + ".0";
                    }
                    if (!YRate.Contains("."))
                    {
                        YRate = YRate + ".0";
                    }
                    this.Invoke((MethodInvoker)delegate
                    {
                        lbX.ForeColor = Color.RoyalBlue;
                        lbhYRate.Text = YRate + "%";
                        lbhRTRate.Text = RTRate + "%";

                    });
                    //MessageBox.Show("YRate By Duration: RTRate/YRate :" + RTRate + "/" + YRate);
                    event_log("YRate By Duration: RTRate/YRate :" + RTRate + "/" + YRate);
                }


            }
            catch (Exception e)
            {
                event_log("ShowfPanelHData: " + e.ToString());
            }

        }
        public void SetHPanelDate()
        {

        }

        Thread _tShowfPanelHData;
        private void timerShowfPanelHData_Tick(object sender, EventArgs e)
        {


            //timerShowfPanelHData.Interval = 600000;// interval 10min
            ////timerShowfPanelHData.Enabled = true;
            //if (!_tShowfPanelHData.IsAlive)
            //{
            //    ul.SetValueByKey("SN", "");
            //    _tShowfPanelHData = new Thread(RunfPanelHData);
            //    _tShowfPanelHData.Name = "_tShowfPanelHData";
            //    _tShowfPanelHData.IsBackground = true;
            //    _tShowfPanelHData.Start();
            //}
        }
        public void RunfPanelHData()
        {
            try
            {

                this.Invoke((MethodInvoker)delegate
                {
                    lbX.ForeColor = Color.MediumVioletRed;
                });
                //MessageBox.Show("RunfPanelHData");
                //while (true)
                //{
                //string StationFromConfig = IniFile.ReadIniFile("MAIN_UI", "MODEL_NAME", "SanderPatrick", @"F:\lsy\Test\DownloadConfig\AutoDL\UISetup.ini");

                //Thread.Sleep(80000);
                if (NetWorkConnection)
                {
                    //ShowfPanelHData();
                }

                //Thread.Sleep(900000); // 15' load 1 times
                //ul.SetValueByKey("SN","");
                //}
            }
            catch (Exception e)
            {
                event_log("RunfPanelHData: " + e.ToString());
            }

        }

        private void pBoxDebug_DoubleClick(object sender, EventArgs e)
        {
            ShowUI.frmDebug wf = new ShowUI.frmDebug();
            wf.ShowDialog();
        }

        private void lbhRTRate_TextChanged(object sender, EventArgs e)
        {

        }

        private void lbhYRate_TextChanged(object sender, EventArgs e)
        {
            try
            {
                float YRate = Convert.ToSingle(lbhYRate.Text.Replace("%", null));

                if (YRate > YRGreen)
                {
                    lbhYRate.BackColor = Color.Lime;
                    lbhYRate.ForeColor = Color.Blue;
                }
                else if (YRate > YRYellow)
                {
                    lbhYRate.BackColor = Color.Yellow;
                    lbhYRate.ForeColor = Color.Red;
                }
                else
                {
                    lbhYRate.BackColor = Color.Red;
                    lbhYRate.ForeColor = Color.White;
                }
            }
            catch (Exception)
            {
                event_log("lbhYRate_TextChanged: " + e.ToString());
            }

        }

        bool NetWorkConnection = true;
        int countTimesNetWorkDisconnect = 0;
        public void CheckNetConnection()
        {

            // for MNRP station dont have internet, then display checksum only
            try
            {
                NetworkChange.NetworkAvailabilityChanged += new NetworkAvailabilityChangedEventHandler(NetworkChange_NetworkAvailabilityChanged);
                NetWorkConnection = NetworkInterface.GetIsNetworkAvailable();
                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    NetWorkConnection = false;
                    //timerCheckNetwork.Interval = 60000;
                    UpdateYR.Enabled = false;
                    CheckUpdate.Enabled = false;
                    timerCableStatus.Enabled = false;
                    timerBlinkCable.Enabled = false;
                    timerAutoUpload.Enabled = false;
                    timerCheckUdateAntiVirusSoft.Enabled = false;
                    countTimesNetWorkDisconnect++;

                }
                else
                {
                    NetWorkConnection = true;

                    //timerCheckNetwork.Interval = 120000;
                    if (countTimesNetWorkDisconnect != 0) // for first time if connect -> do nothing
                    {
                        if (UpdateYR.Enabled == false)
                            UpdateYR.Enabled = true;
                        if (CheckUpdate.Enabled == false)
                            CheckUpdate.Enabled = true;
                        if (timerAutoUpload.Enabled == false)
                            timerAutoUpload.Enabled = true;
                        if (timerCableStatus.Enabled == false)
                            timerCableStatus.Enabled = true;
                        if (timerCheckUdateAntiVirusSoft.Enabled == false)
                            timerCheckUdateAntiVirusSoft.Enabled = true;
                    }
                }
            }
            catch (Exception e)
            {
                event_log("CheckNetwork: " + e.ToString());
            }

            //MessageBox.Show(NetWorkConnection.ToString());
        }
        public void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            NetWorkConnection = e.IsAvailable;
            ///MessageBox.Show(NetWorkConnection.ToString());
        }

        // 2016.06.04 for lock tester one by one
        ShowUI.B05_SERVICE_CENTER.B05_Service SvLock = new ShowUI.B05_SERVICE_CENTER.B05_Service();
        public void SpecialLockExecute()
        {
            try
            {
                if (SpecialLockValidation())
                {

                    ul.SetValueByKey("StopMachine", "1");
                    string getLockString = GetWebUnlockPath("ATELOCKEY").Trim();
                    if (getLockString.Contains("_"))
                    {
                        string emp = getLockString.Substring(0, getLockString.IndexOf("_"));
                        //MessageBox.Show(emp);
                        ShowWarningMessage("MÁY TEST KHÔNG ĐẠT CHUẨN & BỊ KHÓA BỞI " + emp, "SPECIALLOCK");
                    }

                }
            }
            catch (Exception)
            {
            }

        }
        public bool SpecialLockValidation()
        {
            bool isLock = false;
            try
            {
                string key = GetWebUnlockPath("ATELOCKEY").Trim();
                isLock = SvLock.SHOWUI_LOCK_VALIDATION(key);
            }
            catch (Exception)
            {
            }
            return isLock;
        }
        public void SpecialLockEnable(bool lockStatus)
        {
            if (NetWorkConnection)
            {
                try
                {
                    if (LoginAuthentication())
                    {

                        //string _LOCK_KEY,string _ATE_IP,string _ATE_MAC,string _ATE_NAME,string _LOCK_STATUS,string _LOCKEMP, string _UNLOCKEMP,string  _LOCK_REASON
                        string lockEmp = "";
                        string unlockEmp = "";
                        string Key = globalUserName + "_" + client_mac + client_ip;
                        string lockReason = ul.GetKeyIn();
                        bool updateToDB = false;
                        if (lockReason != "")
                        {
                            if (lockStatus)
                            {
                                lockEmp = globalUserName;
                                int lockResult = SvLock.SHOWUI_SPECIAL_LOCK(Key, client_ip, client_mac, sName, "1", lockEmp, unlockEmp, lockReason);

                                if (lockResult == 1)
                                {
                                    MessageBox.Show("Khóa máy thành công!", "Notice Message", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                    //ShowUIApp.AutoClosingMessageBox.Show(, "Notice Message Box", 5000);
                                    updateToDB = true;
                                }
                                else
                                {
                                    MessageBox.Show("Khóa máy không thành công!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);

                                    //ShowUIApp.AutoClosingMessageBox.Show("", "Error Message Box", 5000);
                                }
                            }
                            else
                            {
                                //ShowUIApp.AutoClosingMessageBox.Show("Nhập vào lý do mở khóa máy!", "Guilding Message Box", 5000);
                                //string lockReason = ul.GetKeyIn();
                                unlockEmp = globalUserName;
                                int lockResult = SvLock.SHOWUI_SPECIAL_LOCK(Key, client_ip, client_mac, sName, "0", lockEmp, unlockEmp, lockReason);

                                if (lockResult == 1)
                                {
                                    MessageBox.Show("Mở khóa máy thành công!", "Notice Message", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                    //ShowUIApp.AutoClosingMessageBox.Show(, "Notice Message Box", 5000);
                                    Key = "";
                                    updateToDB = true;
                                }
                                else
                                {
                                    MessageBox.Show("Mở khóa máy không thành công!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    //ShowUIApp.AutoClosingMessageBox.Show(, "Error Message Box", 5000);
                                }
                            }
                            ///
                        }
                        else
                        {
                            MessageBox.Show("Lý do mở/ khóa máy không được để trống. Thử lại!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);

                            // ShowUIApp.AutoClosingMessageBox.Show(, "Notice Message Box", 5000);
                        }// end if check reason
                        if (updateToDB)
                        {
                            SetWebUnlockPath("ATELOCKEY", Key);
                            ul.ShellExecute(@"TASKKILL /IM ShowUI.exe /F /T");
                        }
                    }

                }
                catch (Exception e)
                {
                    ShowUIApp.AutoClosingMessageBox.Show("Fatal error:" + e.ToString(), "Fatal Error", 5000);
                }
            }
        }

        private void lblTotalRate_DoubleClick(object sender, EventArgs e)
        {
            //wDate = DateTime.Now.ToString("yyyyMMdd").Trim();
            //wLine = GetLineOfTester().Trim();
            //wModel = ul.GetValueByKey("SFISMODEL").Trim();
            //wStation = ul.GetStation().Trim();
            //wAteName = sName.Trim();
            //wAteIp = client_ip.Trim();
            //NPI_REAL_DATA();
        }

        private void lockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SpecialLockValidation())
            {
                MessageBox.Show("Không thể thực hiện thao tác khóa máy", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);

                //ShowUIApp.AutoClosingMessageBox.Show(".", "Noctice Message Box", 5000);
            }
            else
            {
                SpecialLockEnable(true);
            }

        }

        private void unlockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SpecialLockValidation())
            {
                SpecialLockEnable(false);
            }
            else
            {
                MessageBox.Show("Không thể thực hiện thao tác mở máy", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);

                //ShowUIApp.AutoClosingMessageBox.Show(".", "Noctice Message Box", 5000);
            }

        }

        ///2016.07.11
        bool flagCheckGoldenData = true;
        bool flagCheckGoldenOK = false;

        public void CheckGoldenData()
        {
            try
            {
                string checkGolden = IniFile.ReadIniFile("CheckGoldenData", "Enable", "0", @"F:\lsy\Test\DownloadConfig\AutoDL\Debug.ini");
                if (checkGolden != "1")
                    return;
                if (!NetWorkConnection)
                    return;
                string listmodel = IniFile.ReadIniFile("CAL_MODEL", "CHECKMODELLIST", "", @"F:\lsy\Test\DownloadConfig\Setup.ini");
                string CheckedModel = ul.GetValueByKey("SFISMODEL");//VN-B5L12-FT5"";//
                                                                    //sName = "VN-B5L12-FT5";//ul.GetStation() GetLineOfTester().Trim()
                _station_name = ul.GetStation();
                _line_name = GetLineOfTester().Trim();

                if (listmodel.Contains(CheckedModel))
                {
                    ShowUI.B05_SERVICE_CENTER.B05_Service svCenter = new ShowUI.B05_SERVICE_CENTER.B05_Service();
                    flagCheckGoldenData = svCenter.P_B05_CHECK_GOLDEN_DATA(sName, CheckedModel, _station_name, _line_name);

                    if (flagCheckGoldenData)
                        flagCheckGoldenOK = true;
                    event_log("CheckGoldenData: checking testing golden data completed with status: " + flagCheckGoldenData.ToString());
                }
            }
            catch (Exception e)
            {
                event_log("CheckGoldenData Exception: " + e.ToString());
            }

        }

        ///
        bool PathUpdateTestFag = true;
        int IsTPGUpdateTestFlag = 0;

        public void CheckUpdateTestFlag()
        {

            try
            {
                if (!NetWorkConnection)
                    return;
                string dNow = DateTime.Now.ToString("yyyyMMddHHmm");
                string dPre = DateTime.Now.AddHours(-1).ToString("yyyyMMddHHmm");
                string line = GetLineOfTester().Trim();//"L12";//
                string ate = sName;//"VN-B5L12-PT4";//
                string model = ul.GetValueByKey("SFISMODEL").Trim();
                if (ul.GetValueByKey("HOTFIX") != null && ul.GetValueByKey("HOTFIX").Trim() != "SanderPatrick")
                {
                    DataTable dt = sfisB05.GET_FULL_R_STATION_ATE_T(dPre, dNow);//" +  + "" + sName + "

                    DataRow[] results = dt.Select("LINE_NAME='" + line + "' AND STATION_NAME = '" + ate + "'");
                    string listmdel = "";

                    if (results.Length != 0)
                    {
                        PathUpdateTestFag = false; // set = true to use function
                        for (int i = 0; i < results.Length; i++)
                        {
                            listmdel += results[i]["MODEL_NAME"].ToString().Trim() + ",";
                            if (model == results[i]["MODEL_NAME"].ToString().Trim())
                            {
                                PathUpdateTestFag = true;
                                break;
                            }
                        }
                        if (PathUpdateTestFag == false)
                        {
                            CENTER_B05_SV.SHOWUI_BACKEND_INFO(line, sType, model, "CheckUpdateTestFlag: showui path checking completed from " + dPre + " to " + dNow + " > " + model + " ? " + listmdel + " with status: " + PathUpdateTestFag.ToString());
                        }
                        //event_log("CheckUpdateTestFlag: showui path checking completed:" + PathUpdateTestFag.ToString());
                    }
                    else
                    {
                        event_log("CheckUpdateTestFlag: searching model in sfis with 0 rows ");
                    }
                    //event_log("CheckUpdateTestFlag: from " + dPre + " to " + dNow + " > " + model + " ? " + listmdel);
                }
                else
                {
                    PathUpdateTestFag = true;
                }
            }
            catch (Exception e)
            {
                event_log("CheckUpdateTestFlag Exception: " + e.ToString());
            }


        }

        public void TPGUpdateTestFlag()
        {
            try
            {

                this.Invoke((MethodInvoker)delegate
                {
                    // delay when first open
                    Random rd = new Random();
                    int rdNumDelay = rd.Next(5, 10);
                    rdNumDelay = rdNumDelay * 60000;//
                    timerCheckUpdateTestFlag.Enabled = true;
                    timerCheckUpdateTestFlag.Interval = rdNumDelay;
                });
                Thread.Sleep(1080000);// wait for 18 min later1080000
                if (ul.GetValueByKey("HOTFIX") != null && ul.GetValueByKey("HOTFIX").Trim() != "SanderPatrick")
                {
                    if (IsTPGUpdateTestFlag == 1)
                    {
                        IsTPGUpdateTestFlag = 1;
                        //event_log("CheckUpdateTestFlag: testflag updating finished");
                    }
                    else
                    {
                        CENTER_B05_SV.SHOWUI_BACKEND_INFO(_line_name, sType, _model_name, "TPGUpdateTestFlag: tpg không cập nhật testflag");

                        //ShowUIApp.AutoClosingMessageBox.Show("tpg không cập nhật testflag. tpg & showui sẽ tự động đóng sau 2 phút.", "AutoCloseMessageBox", 120000);
                        // ul.ShellExecute(@"TASKKILL /IM " + ul.GetValueByKey("TPG_NAME").Trim() + "/F /T");
                        //ul.ShellExecute(@"TASKKILL /IM NetgearAutoDL.exe /F /T");
                        // Application.Exit();
                    }
                }
                else
                {
                    IsTPGUpdateTestFlag = 1;
                }
            }
            catch (Exception)
            {
            }

        }

        private void timerCheckUpdateTestFlag_Tick(object sender, EventArgs e)
        {
            try
            {
                //timerCheckUpdateTestFlag.Enabled = false;
                //if (NetWorkConnection)
                //    CheckUpdateTestFlag();

                //Random rd = new Random();
                //int rdNum = rd.Next(18, 25);
                /////MessageBox.Show("delay" + rdNum.ToString());
                //rdNum = rdNum * 60000;
                //timerCheckUpdateTestFlag.Interval = rdNum;
                //timerCheckUpdateTestFlag.Enabled = true;
            }
            catch (Exception)
            {
            }
        }


        //
        bool CheckATEOK = true;
        public void CheckCheckATE()
        {
            try
            {

                string OpenProgTime = DateTime.Now.ToString("HHmm");
                while (true)
                {
                    //Thread.Sleep(30000);

                    Thread.Sleep(300000); // sleep for program happy 5'
                    if (NetWorkConnection)
                    {

                        string line = GetLineOfTester();//"L12";//
                        string CurrentModelName = ul.GetModel(); // "C6300-100NASV1";
                        string Station = ul.GetStation();//RC";//;
                        string CurrentShift = "";
                        string CurrentCksum = ul.GetValueByKey("Checksum");//"0CAE0B32";// 

                        string TmpDate = DateTime.Now.ToString("HHmm");
                        int ComparedTime = 730;
                        ComparedTime = Convert.ToInt32(TmpDate);
                        int flagTime = Convert.ToInt32(OpenProgTime) + 130;// after 1h30 from open showui then check

                        if (ComparedTime >= 730 && ComparedTime <= 1930)
                            CurrentShift = "D";
                        else
                            CurrentShift = "N";

                        //MessageBox.Show(ComparedTime+" ? "+flagTime);
                        if (TestedDUT >= 15 && ComparedTime > flagTime)
                        {
                            ShowUI.ATE_CHECKLIST.WebService svATE = new ShowUI.ATE_CHECKLIST.WebService();
                            CheckATEOK = svATE.CheckIsCheckedATEChecklist(line, ModelNameChangeATE, Station, CurrentShift, DateTime.Now.ToString("yyyyMMdd"), CheckSum);

                            if (CheckATEOK == false && ComparedTime > 0 && ComparedTime < 730)
                            {
                                CheckATEOK = svATE.CheckIsCheckedATEChecklist(line, CurrentModelName, Station, CurrentShift, DateTime.Now.AddDays(-1).ToString("yyyyMMdd"), CheckSum);
                            }
                            event_log("CheckCheckATE: checking checked-ate-checklist completed with status: " + CheckATEOK.ToString());
                            break;
                        }
                    }
                }
                ///
            }
            catch (Exception)
            {
            }
        }

        bool CheckTPGVersion = true;
        public void CheckTPGInfo()
        {
            try
            {
                if (!NetWorkConnection)
                    return;
                int delay = rd.Next(1, 3) * 30000;
                Thread.Sleep(delay); // sleep for program happy
                string IVersion = ul.GetValueByKey("Version");//"L12";//
                string CurrentModelName = ul.GetProduct(); // "C6300-100NASV1";
                string Station = ul.GetStation();//RC";//;
                string Product = ul.GetProduct();
                string CurrentCksum = ul.GetValueByKey("Checksum");//"0CAE0B32";// 

                ShowUI.ATE_CHECKLIST.WebService svATE = new ShowUI.ATE_CHECKLIST.WebService();
                CheckTPGVersion = svATE.CheckTPGVersion(CurrentModelName, Station, IVersion, CurrentCksum);
                if (CheckTPGVersion)
                    event_log("CheckTPGVersion: checking tpg version completed with status: " + CheckTPGVersion.ToString());

                if (CheckTPGVersion == false)
                {
                    event_log("CheckTPGVersion: checking tpg version completed with status: " + CheckTPGVersion.ToString());
                    //string tmpPathDownloadConfig = @"F:\lsy\Test\DownloadConfig\" + Product + ".ini";

                    //try
                    //{
                    //    if (File.Exists(tmpPathDownloadConfig))
                    //    {
                    //        string SectionModel = IniFile.ReadIniFile("ModelSection", CurrentModelName, null, tmpPathDownloadConfig);

                    //        string tmpPathCkSum = IniFile.ReadIniFile(SectionModel, "TPG_FOLDER_" + Station, null, tmpPathDownloadConfig);

                    //        tmpPathCkSum = tmpPathCkSum + "checksum.cfg";

                    //        if (File.Exists(tmpPathCkSum))
                    //        {
                    //            CRC32Calc crc32Program = new CRC32Calc();
                    //            UInt32 chkProgrm = 0;
                    //            FileStream f = new FileStream(tmpPathCkSum, FileMode.Open, FileAccess.Read, FileShare.Read, 8192);
                    //            chkProgrm = crc32Program.GetCrc32(f);
                    //            MessageBox.Show(chkProgrm.ToString());
                    //            f.Close();
                    //        }
                    //        else
                    //        {
                    //            event_log("CheckTPGVersion: checksum.cfg file path not exist >" + tmpPathCkSum);
                    //        }
                    //    }
                    //}
                    //catch (Exception e)
                    //{
                    //    event_log("CheckTPGVersion Exception: " + e.ToString());
                    //}
                }
            }
            catch (Exception)
            {
            }

        }


        // arlo gen only
        bool isInterferenceStation = false;

        public void CheckInterference()
        {
            ul.event_log(client_ip);
            string iInterferenceShowUI = IniFile.ReadIniFile("INTERFERENCE", "INTERFERECE_PC_IP", "", @"F:\lsy\Test\DownloadConfig\AutoDL\UISetup.ini").Trim();
            string[] tmp = iInterferenceShowUI.Split(',');
            foreach (string _tp in tmp)
            {
                if (_tp == client_ip)
                {
                    string iListClientPC = IniFile.ReadIniFile("INTERFERENCE", _tp, "", @"F:\lsy\Test\DownloadConfig\AutoDL\UISetup.ini").Trim();
                    isInterferenceStation = true;
                    ul.SetValueByKey("INTERFERENCE_PC", _tp);
                    ul.SetValueByKey("INTERFERENCE_CLIENT_PC", iListClientPC);
                    break;
                }
            }


        }

        private void rightMenu_Opening(object sender, CancelEventArgs e)
        {

        }

        private void ChangePasswordToolTrip(object sender, EventArgs e)
        {
            try
            {
                using (ShowUI.frmChangePass ChangePass = new ShowUI.frmChangePass("V0917332", "", true))
                {
                    ChangePass.ShowDialog();
                }
            }
            catch (Exception)
            {
            }
        }

        bool lockByCalibrationControl = false;
        public void CheckCalibration()
        {
            bool checkResult = true;
            string msg = "";

            //checkResult = CENTER_B05_SV.P_B05_CALIBRATION_CONTROL(ul.GetModel(), ul.GetStation(), ref msg);
            //this.Invoke((MethodInvoker)delegate
            //{
            //    MessageBox.Show(msg + " " + ul.GetModel() + " " + ul.GetStation() + " " + checkResult.ToString() + " " + msg);
            //});

            //if (!checkResult)
            //    lockByCalibrationControl = true;
            //1hour check one time 
            if ((DateTime.Now.Hour % 1 == 0 && DateTime.Now.Minute == 25 && DateTime.Now.Second == 5))
            {
                try
                {
                    if ((DateTime.Now.Hour >= 9 && DateTime.Now.Hour <= 16)) // only check in this time due to lack worker
                    {
                        string model_name = ul.GetModel();
                        string station_name = ul.GetStation();
                        checkResult = CENTER_B05_SV.P_B05_CALIBRATION_CONTROL(model_name, station_name, ref msg);
                        //this.Invoke((MethodInvoker)delegate
                        //{
                        //    MessageBox.Show(msg + " " + model_name + " " + station_name + " " + checkResult.ToString());
                        //});

                        if (!checkResult)
                            lockByCalibrationControl = false;
                    }

                    //return checkResult;// true will not lock // false lock
                }
                catch (Exception r)
                {
                    ul.event_log(msg + "\n" + r.ToString());
                    // return checkResult;
                }
            }
            //
        }

        private void lblRetestYeildRate_TextChanged(object sender, EventArgs e)
        {
            float RetestyeildRate = Convert.ToSingle(lblRetestYeildRate.Text.Replace("%", null));

            if (RetestyeildRate > YRGreen)
            {

                lblRetestYeildRate.BackColor = Color.Lime;
                lblRetestYeildRate.ForeColor = Color.Blue;
            }
            else if (RetestyeildRate > YRYellow)
            {
                lblRetestYeildRate.BackColor = Color.Yellow;
                lblRetestYeildRate.ForeColor = Color.Red;
            }
            else
            {
                lblRetestYeildRate.BackColor = Color.Red;
                lblRetestYeildRate.ForeColor = Color.White;
            }
        }
        public void GetRepassYeildRate()
        {
            this.Invoke((MethodInvoker)delegate
            {
                try
                {
                    if (fake)
                    {

                        lblRetestYeildRate.Text = "100.0%";

                    }
                    else
                    {
                        //string model = string.Format("{0,-5}{1,-25}{2,12}", "RR-YR", ul.GetModel(), Environment.MachineName.Trim());//Environment.MachineName
                        //string data = sfisB05.SHOWUI(model, ul.GetStation());
                        string data = ul.GetValueByKey("RYRDATA");

                        //lblFYR.Text = FirstPassYR + "%";
                        //ul.SetValueByKey("RYRDATA", data);
                        ul.SetValueByKey("ShowUIData", data);
                        ul.event_log("set data ShowUIData: " + data);
                        float RestestyeildRate = Convert.ToSingle(data.Substring(54, 6));
                        float FirstPassYR = Convert.ToSingle(data.Substring(data.Length - 10, 5));
                        lblRetestYeildRate.Text = RestestyeildRate + "%";
                    }

                }
                catch (Exception r)
                {
                    lblRetestYeildRate.Text = "100.0%";

                    ul.event_log("R.Y.R Exception:" + r.ToString());
                }
            });
            //
        }

        private void panel1_Click(object sender, EventArgs e)
        {
            if (!this.TopMost)
            {
                this.TopMost = true;
            }
            else
            {
                this.TopMost = false;
            }
        }
        //Check Same 3 Error Code  in 1 houre
        ///
        public DataTable CountGroupByErrorCode(string i_sGroupByColumn, string i_sAggregateColumn, DataTable i_dSourceTable)
        {
            DataTable dtGroup = new DataTable("ADB");
            try
            {
                DataView dv = new DataView(i_dSourceTable);
                //getting distinct values for group column
                dtGroup = dv.ToTable(true, new string[] { i_sGroupByColumn });

                //adding column for the row count
                dtGroup.Columns.Add("Count", typeof(int));

                //looping thru distinct values for the group, counting
                foreach (DataRow dr in dtGroup.Rows)
                {
                    dr["Count"] = i_dSourceTable.Compute("Count(" + i_sAggregateColumn + ")", i_sGroupByColumn + " = '" + dr[i_sGroupByColumn] + "'");
                }
                //returning grouped/counted result
            }
            catch (Exception)
            {
            }

            return dtGroup;
        }
        string strStartDate = "";
        string TopErrorCodeInHour = "";
        private bool CheckErrorList()
        {
            string SN = ul.GetValueByKey("SN");
            string ErrorCode = ul.GetValueByKey("ERRORCODE");
            try
            {
                DateTime StartDate = Convert.ToDateTime(strStartDate);
                TimeSpan dt = DateTime.Now - StartDate;
                if (dt.TotalSeconds > 3600)
                {
                    if (File.Exists(".\\ErrorCodeList.txt"))
                    {
                        File.SetAttributes(".\\ErrorCodeList.txt", FileAttributes.Normal);
                        File.Delete(".\\ErrorCodeList.txt");
                    }
                    strStartDate = "";
                    return true;
                }
                else
                {
                    DataTable CheckErrorList = GetDtErrorCode(SN, ErrorCode);
                    DataTable dtMaxErrorCode = CountGroupByErrorCode("ErrorCode", "ErrorCode", CheckErrorList);
                    dtMaxErrorCode.DefaultView.Sort = "Count DESC";
                    DataTable dtDistinctSN = new DataView(CheckErrorList).ToTable(true, new string[] { "SN" });
                    if (int.Parse(dtMaxErrorCode.Rows[0][1].ToString()) >= 3)
                    {
                        // MessageBox.Show("V??t qua 3 ma l?i gi?ng nhau trong 1 gi?");
                        TopErrorCodeInHour = "Vuot qua 3 ma loi giong nhau trong 1 gio";
                        return false;
                    }
                    else if (dtDistinctSN.Rows.Count >= 10)
                    {
                        // MessageBox.Show("V??t qua 10 b?n l?i trong 1 gi?!");
                        TopErrorCodeInHour = "Vuot qua 10 ban loi trong 1 gio!";
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                strStartDate = DateTime.Now.ToString();
                return true;
            }
        }

        private void TimerIQTestTime_Tick(object sender, EventArgs e)
        {
            //string ModelName = ul.GetProduct();
            int IQ_Netgear = System.Convert.ToInt32(IniFile.ReadIniFile("COMMON", "IQ_Netgear", "0", ".\\Setup.ini"));
            if (IQ_Netgear == 1)
            {
                try
                {
                    _model_name = ul.GetValueByKey("SFISMODEL").Trim();
                    string stationLocal = ul.GetStation();
                    string ModelName = ul.GetProduct();
                    string pathCopy = IniFile.ReadIniFile(ModelName, ul.GetStation(), "loser", ".\\IQTESTTIME\\LocalDetailLog.ini");
                    string checkStation = IniFile.ReadIniFile("IQCheckStation", _model_name, "0", ".\\IQTESTTIME\\IQCheckStation.ini");


                    string checkNetgearSql = $"select * from ProjectName where ProjectName ='{ModelName}'";
                    ConnectShowUI connCheckNt = new ConnectShowUI();
                    DataTable checkNt = connCheckNt.DataTable_Sql(checkNetgearSql, "10.224.81.62");
                    if (checkNt.Rows.Count > 0)
                    {
                        string dicr = @"C:\LitePoint";
                        if (System.IO.Directory.Exists(dicr) && pathCopy.Contains("loser"))
                        {
                            if (Process.GetProcessesByName("IQTestTimes").Length < 1 && Process.GetProcessesByName("IQTestTimesDetail").Length < 1)
                            {
                                ProcessStartInfo startProgram = new ProcessStartInfo();
                                startProgram.FileName = @"C:\LitePoint\IQTestTimes.exe";
                                startProgram.WorkingDirectory = @"C:\LitePoint\";
                                //startProgram.WorkingDirectory = Environment.CurrentDirectory;
                                Process.Start(startProgram);
                            }


                        }
                        else if (!pathCopy.Contains("loser"))
                        {
                            if (Process.GetProcessesByName("IQTestTimesDetail").Length < 1 && Process.GetProcessesByName("IQTestTimes").Length < 1)
                            {
                                ProcessStartInfo startProgram = new ProcessStartInfo();
                                startProgram.FileName = pathCopy + $"{_model_name}\\{stationLocal}\\IQTestTimesDetail.exe";
                                startProgram.WorkingDirectory = pathCopy + $"{_model_name}\\{stationLocal}\\";
                                //startProgram.WorkingDirectory = Environment.CurrentDirectory;
                                Process.Start(startProgram);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
            else
            {
                string dicr = @"C:\LitePoint";
                if (System.IO.Directory.Exists(dicr))
                {
                    if (Process.GetProcessesByName("IQTestTimes").Length < 1)
                    {
                        ProcessStartInfo startProgram = new ProcessStartInfo();
                        startProgram.FileName = @"C:\LitePoint\IQTestTimes.exe";
                        //startProgram.WorkingDirectory = Environment.CurrentDirectory;
                        Process.Start(startProgram);
                    }
                }
            }
        }

        private void IQSN_Click(object sender, EventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {

            });

        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (!File.Exists(".//IQTESTTIME//IQSN.txt"))
                {
                    File.Create(".//IQTESTTIME//IQSN.txt");
                }

                using (FileStream IQSNFile = new FileStream(".//IQTESTTIME//IQSN.txt", FileMode.Append, FileAccess.ReadWrite, FileShare.Write))
                {
                    string writeIQSN = IQSNtxt.Text + " " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "\r\n";
                    IQSNFile.Write(Encoding.Default.GetBytes(writeIQSN), 0, writeIQSN.Length);

                    //IQSNtxt.Show();

                }



            }
        }

        private void IQSNtxt_TextChanged(object sender, EventArgs e)
        {

        }

        private void iQSNToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IQSNtxt.Text = "";
            IQSNtxt.Show();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void IQSNtxt_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void IQSNtxt_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (!File.Exists(".//IQTESTTIME//IQSN.txt"))
                {

                    var IQSN = File.Create(".//IQTESTTIME//IQSN.txt");
                    IQSN.Close();
                }

                using (FileStream IQSNFile = new FileStream(".//IQTESTTIME//IQSN.txt", FileMode.Append, FileAccess.Write, FileShare.Write))
                {
                    string writeIQSN = IQSNtxt.Text + " " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "\r\n";
                    // IQSNFile.Write(Encoding.Default.GetBytes(writeIQSN), 0, writeIQSN.Length);
                    IQSNFile.Seek(0, SeekOrigin.End);
                    IQSNFile.Write(Encoding.Default.GetBytes(writeIQSN), 0, writeIQSN.Length);
                    //IQSNtxt.Show();

                }
                IQSNtxt.Clear();
                IQSNtxt.Hide();
            }
        }

        private void showUI_Closed(object sender, FormClosedEventArgs e)
        {


        }

        private void toolsboxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                DownLoadToolsBoxHelper dlToolsbox = new DownLoadToolsBoxHelper();
                dlToolsbox.downLoadApp();
            }
            catch { }

            ProcessStartInfo startProgram = new ProcessStartInfo();
            startProgram.FileName = @".\ToolsBox.exe";
            startProgram.WorkingDirectory = @".\Toolsbox\";
            Process.Start(startProgram);
        }



        public DataTable GetDtErrorCode(string SN, string ErrorCode)
        {
            DataTable dt = new DataTable("dtErrorCodeInfo");
            try
            {
                using (StreamWriter sw = new StreamWriter(".\\ErrorCodeList.txt", true))
                {
                    sw.WriteLine(SN + "," + ErrorCode);
                }

                var lines = File.ReadAllLines(".\\ErrorCodeList.txt");
                dt.Columns.Add("SN");
                dt.Columns.Add("ErrorCode");
                foreach (var item in lines)
                {
                    DataRow dr = dt.NewRow();
                    string[] values = item.Split(new char[] { ',' });
                    try
                    {
                        dr[0] = values[0];
                        dr[1] = values[1];

                    }
                    catch (Exception)
                    {
                    }
                    dt.Rows.Add(dr);
                }
            }
            catch (Exception ex)
            {
            }

            return dt;
        }


        //Arlo UPH Monitoring 2017.01.10
        Thread _tArloUphMonitor;
        bool _lockByUPH = false;
        double R_DUT = 0;
        Stopwatch stwArloUphMonitor = new Stopwatch();
        public void ArloUphMonitor()
        {
            /*Logic
             *
             * 
             * 1. ShowUI get spec from  database: R_DUT number spec which is calculated following UPH and yieldrate (config by IE & QA)
             * if one model doesnt exist, insert new with default value
             * 2. ShowUI get number of R_DUT of station via webservice from sfis then compared with spec
             * 3. If over spec, then lock tester,
             * 4. Unlock tester need to record
             */

            try
            {

                string test_group = ul.GetStation();
                string model_name = ul.GetModel();
                string mo_number = ul.GetValueByKey("MO");
                string tSN = ul.GetValueByKey("SN");
                string fSN = "";
                string _ate_name = Environment.MachineName.Trim();
                string _ate_ip = ul.GetNICGatewayIP();
                string yrate_real = "100";
                this.Invoke((MethodInvoker)delegate
                {
                    stwArloUphMonitor.Start();
                    yrate_real = Convert.ToDouble(lbhYRate.Text.Replace("%", "").Trim()).ToString();
                });


                string TmpDate = DateTime.Now.ToString("HHmm");
                int ComparedTime = 730;
                ComparedTime = Convert.ToInt32(TmpDate);

                if (ComparedTime >= 730 && ComparedTime <= 1930)
                    Shift = "D";
                else
                    Shift = "N";
                string end_time = DateTime.Now.ToString("yyyyMMddHHmm");

                int currentMin = DateTime.Now.Minute;
                string start_time = end_time;
                if (currentMin >= 30)
                    start_time = DateTime.Now.ToString("yyyyMMddHH") + "30";
                else
                    start_time = DateTime.Now.AddHours(-1).ToString("yyyyMMddHH") + "30";

                if (mo_number == "")
                {
                    //2017.12.07 Adele check get SN
                    string strSN = "";

                    string KeySN = @"SOFTWARE\Netgear\SaveSN\OlyAdeleUse";
                    RegistryKey rSN = Registry.LocalMachine.OpenSubKey(KeySN, true);
                    strSN = rSN.GetValue("SN", "").ToString();
                    //tSN = ul.GetValueByKey("SN", "").ToString();
                    //2017.12.19 check SN cho cac tram ko dung sao
                    if (strSN != "")
                    {
                        fSN = strSN;
                    }
                    else
                    {
                        fSN = tSN;
                    }
                    DataTable dtMO = sfisB05.FIND_MO_NUMBER(fSN, "");
                    ul.event_log("New SN: " + strSN);
                    try
                    {
                        mo_number = dtMO.Rows[0][0].ToString();

                        ul.SetValueByKey("MO", mo_number);
                        ul.SetValueByKey("MO_s", mo_number);
                        rSN.SetValue("MO", mo_number);
                        ul.SetValueByKey("iFPR", "");

                    }
                    catch (Exception r)
                    {
                        _lockByUPH = false;
                        ul.event_log("ArloUphMonitor MO Exception:" + r.ToString());
                    }
                }

                if (mo_number != "")
                {
                    ul.event_log("ArloUphMonitor: from " + start_time + " to " + end_time);
                    DataTable dtSfisR_DUT = sfisB05.GET_TOTAL_PASSFAIL(test_group, Shift, start_time, end_time, mo_number);
                    R_DUT = Convert.ToDouble(dtSfisR_DUT.Rows[0]["FAIL_QTY"].ToString());

                    _lockByUPH = CENTER_B05_SV.P_B05_ARLO_UPH_CONTROL(mo_number, R_DUT.ToString(), test_group, model_name, _ate_name, _ate_ip, yrate_real);

                    ul.event_log("ArloUphMonitor: Lock->" + _lockByUPH.ToString() + " MODEL->" + model_name + " STAION->" + test_group + " MO->" + mo_number + " R_DUT_NUM->" + R_DUT);
                }

                //MessageBox.Show(isLock.ToString() + "");
            }
            catch (Exception r)
            {
                _lockByUPH = false;
                ul.event_log("ArloUphMonitor Exception:" + r.ToString());
            }

            Thread.Sleep(60000);
            this.Invoke((MethodInvoker)delegate
            {
                stwArloUphMonitor.Stop();
                stwArloUphMonitor.Reset();
                ul.event_log("ArloUphMonitor: OK");
            });
        }

        private void IQSNtxt_TextChanged_2(object sender, EventArgs e)
        {

        }

        private void CheckAntivirut_Tick(object sender, EventArgs e)
        {

        }

        //bool ControlRunCtrlFall = false;

        ShowUI.SFISB05_SQL_SV.Service sfisBySQL = new ShowUI.SFISB05_SQL_SV.Service();

        public void ControlRunCtrl()
        {
            sfisBySQL.Timeout = 30000;
            //string SN = "";
            //string registryMO = "";// ul.GetValueByKey("MO");
            ////ul.SetValueByKey("SN", SN);
            bool firstTimeOpen = true;
            if (useFuncControlRun == "0")
                return;

            while (true)
            {
                ////MessageBox.Show(ul.GetStation());

                //ControlRunCtrlFall = false;
                while (ul.GetValueByKey("SN") == "" || ul.GetValueByKey("SN") == "DEFAULT") // wait untill have sn
                {
                    Thread.Sleep(759);
                }
                //MessageBox.Show(SN);
                while (ul.GetValueByKey("MO") == "" || globalStation == "")
                {
                    Thread.Sleep(759);
                }

                if (ul.GetStation().Contains("C_") && globalStation != ul.GetStation()) // force controlrun station always start with S_
                {
                    try
                    {

                        if (firstTimeOpen)
                        {
                            ul.event_log("ControlRunCtrl: SFIS-> " + globalStation + " ? " + ul.GetStation() + " <-Current");
                            firstTimeOpen = false;
                            Thread _tNoticeControlRun = new Thread(NoticeControlRun);
                            _tNoticeControlRun.IsBackground = true;
                            _tNoticeControlRun.Start();
                        }

                        //registryMO = ul.GetValueByKey("MO");
                        //SN = ul.GetValueByKey("SN");
                        //string sql = "select mo_number from sfism4.R_WIP_TRACKING_T  where serial_number ='" + SN + "'";


                        //DataTable dt = sfisBySQL.GetData(sql);

                        //if (dt.Rows.Count > 0)
                        //{

                        //    string snMO = dt.Rows[0]["MO_NUMBER"].ToString().Trim();
                        //    //MessageBox.Show(registryMO + "?" + snMO);
                        //    if (registryMO != snMO)
                        //    {
                        //        //ControlRunCtrlFall = true;
                        //        if (ul.GetProduct() != "U12I345"||ul.GetStation() != "RC")
                        //        {
                        //            this.Invoke((MethodInvoker)delegate {
                        //            MessageBox.Show(this, "MO Change\nCông lệnh thay đổi. Cần tải lại chương trình !", "ShowUI MessageBox", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        //            });
                        //            ul.ShellExecute("TASKKILL /IM NetgearAutoDL.exe /F /T");
                        //            string tpg = ul.GetValueByKey("TPG_NAME");
                        //            ul.ShellExecute("TASKKILL /IM " + tpg + " /F /T");
                        //            Application.Exit();
                        //        }
                        //    }
                        //    else
                        //    {
                        //        // Notice this pc is run ControlRun Program
                        //        if (firstTimeOpen)
                        //        {
                        //            firstTimeOpen = false;
                        //            Thread _tNoticeControlRun = new Thread(NoticeControlRun);
                        //            _tNoticeControlRun.IsBackground = true;
                        //            _tNoticeControlRun.Start();
                        //            //MessageBox.Show(firstTimeOpen.ToString()); //different MO -> lock
                        //        }
                        //    }
                        //}

                    }
                    catch (Exception)
                    {
                    }
                }

                Thread.Sleep(5000);
            }//end while

        }

        public void NoticeControlRun()
        {
            bool showCtrlRun = true;
            while (true)
            {
                this.Invoke((MethodInvoker)delegate
                {

                    lblChecksum.BackColor = Color.Red;
                    lblChecksum.ForeColor = Color.White;

                    lblChk.ForeColor = Color.White;
                    lblChk.BackColor = Color.Red;
                    if (showCtrlRun)
                    {
                        showCtrlRun = false;
                        lblChecksum.Text = "ControlRun";
                    }
                    else
                    {
                        showCtrlRun = true;
                        lblChecksum.Text = ul.GetValueByKey("Checksum");//("Checksum", "").ToString();
                    }
                });

                Thread.Sleep(5000);
            }
        }

        private void pathlossErrorListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmPathlossErr frmPathloss = new frmPathlossErr();
            frmPathloss.ShowDialog();
        }

        private double GetPercentFreeSpace(string driveName)
        {
            double FreeSpace;
            double DriveSize;
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name == driveName)
                {
                    FreeSpace = drive.TotalFreeSpace;
                    DriveSize = drive.TotalSize;
                    double per = (FreeSpace / DriveSize);
                    return per;
                }
            }
            return -1;
        }
        ShowUI.frmSamplingControl frmSC = new ShowUI.frmSamplingControl();
        protected void getShiftDate(out string date, out string shift)
        {
            date = ""; shift = "";
            var _now = DateTime.Now;
            if (_now.Hour > 7 && _now.Hour < 20)
            {
                shift = "D";
                date = _now.ToString("yyyyMMdd");
            }
            if (_now.Hour >= 20)
            {
                shift = "N";
                date = _now.ToString("yyyyMMdd");
            }
            if (_now.Hour <= 7)
            {
                shift = "N";
                date = _now.AddDays(-1).ToString("yyyyMMdd");
            }

        }
        private void SamplingThroughtput_Tick(object sender, EventArgs e)
        {
            ShowUI.ThroughtputService.ThroughtputServiceSoapClient getDataFromService = new ShowUI.ThroughtputService.ThroughtputServiceSoapClient();
            string ModelName = ul.GetModel();
            string Station = ul.GetStation();
            string Product = ul.GetProduct();
            string srPath = @"F:\lsy\Test\DownloadConfig\" + Product + ".ini";

            string date, shift;
            getShiftDate(out date, out shift);
            try
            {
                List<string> lstPC = IniFile.ReadIniFile("SamplingThroughtput", "LstPC", "", srPath).Split(',').Where(x => x.Length > 0).ToList();
                double rate = double.Parse(IniFile.ReadIniFile("SamplingThroughtput", "Rate_"+Station, "101", srPath).Replace("%",""));
                string lockPC = IniFile.ReadIniFile("SamplingThroughtput", "Lock", "0", srPath);
                
                List<WipPCModel> lstData = getDataFromService.GetDataThroughtputSampling(ModelName.ToUpper(), Station.ToUpper(), shift, date)
                                        .ToList().ConvertSamplingThroughtputFunc(new List<WipPCModel>());
                if (lstData.Count == 0 || lstPC.Count == 0 || rate > 100)
                {
                    return;
                }

                var total = Convert.ToDouble(lstData.Sum(x => x.WIPTOTAL));
                var dataSampling = (from pc in lstPC
                                    join dt in lstData
                                    on pc.Trim().ToUpper() equals dt.STATION_NAME.Trim().ToUpper()
                                    select new { PCName = pc, WipTotal = dt.WIPTOTAL }).Sum(x=>x.WipTotal);
                if(total>0 && (Convert.ToDouble(dataSampling)*100.0/ total > rate))
                {
                    if (lockPC.Contains("1"))
                    {
                        ul.SetValueByKey("StopMachine", "1");
                        _IsExistErrorr = true;
                        ShowWarningMessage($"Lỗi: Sampling control langer than rate: {rate}, call TE-SETUP,TE-PRO Check", "Sampling Rate Issue");
                    }
                }

            }
            catch (Exception ex)
            {


            }
        }

        public void SamplingWarningDiaglog()
        {
            try
            {


                if (frmSC == null || frmSC.Visible == false)
                {

                    ul.SetValueByKey("SamplingCountDown", "1");
                    ul.SetValueByKey("SamplingTimeOut", "1");
                    frmSC = new ShowUI.frmSamplingControl();
                    frmSC.ShowDialog();
                }
                else
                {
                    ul.event_log("SamplingWarningDiaglog: Form Warning Exist");
                }
            }
            catch (Exception r)
            {
                ul.event_log("TEST SamplingWarningDiaglog: " + r.ToString());
            }
        }

        //Thread _tSamplingWarningDiaglog;
        //int fCount = 0;
        string useFuncSamplingControl = "0";
        string useFuncControlRun = "1";
        public void SamplingControl()
        {
            try
            {
                ul.event_log("SamplingControl test");
                if (useFuncSamplingControl == "0")
                    return;
                string str = ul.GetValueByKey("SamplingTime");
                string CountDownFinish = ul.GetValueByKey("SamplingCountDown");
                DateTime dt = DateTime.Now;
                if (str != "")
                    dt = Convert.ToDateTime(str);
                else
                    ul.SetValueByKey("SamplingTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                TimeSpan tsp = DateTime.Now.Subtract(dt);
                try
                {

                    if (CountDownFinish == "0" && frmSC != null)
                        frmSC.Close();
                }
                catch (Exception r)
                {

                    ul.event_log("SamplingControl frmSC: " + r.ToString());
                }

                globalStation = "RC";
                //ul.event_log("tsp.TotalMinutes" + tsp.TotalMinutes + " " + globalStation);

                //if (Math.Round(tsp.TotalMinutes, 1) >= 2 && globalStation != "")//
                //{
                if (globalStation != "")//
                {
                    SamplingWarningDiaglog();
                }//
            }
            catch (Exception r)
            {
                ul.event_log("SamplingControl: " + r.ToString());
            }
        }

        private void timercheckGoiTE_Tick(object sender, EventArgs e)
        {
            if (btnCall.Visible)
            {
                btnCall.Visible = false;
            }
        }

        private void timerCheckRickSW_Tick(object sender, EventArgs e)
        {
            string _LockingMessage = "";
            string rs = checkRickSoftware();
            MessageBox.Show(rs + sName);
            if (rs != "")
            {
                ul.SetValueByKey("StopMachine", "1");
                _LockingMessage = "Lỗi: Máy có cài phần mềm cấm.\n" + rs;
                _IsExistErrorr = true;
                ShowWarningMessage(_LockingMessage, "Rick Software");
            }
        }

        private void panel1_Paint_1(object sender, PaintEventArgs e)
        {

        }

        private void QtyTester_Tick(object sender, EventArgs e)
        {
            string Qty = ul.GetValueByKey("QtybyTester");
            lblQty.Text = Qty;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ShowUI.FrmReport frReport = new ShowUI.FrmReport();
            frReport.ShowDialog();
        }

        private void reportErrorCodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowUI.FrmReport frReport = new ShowUI.FrmReport();
            frReport.ShowDialog();
        }

        private void lblFPR_TextChanged(object sender, EventArgs e)
        {
            float yeildRate = Convert.ToSingle(lblFPR.Text.Replace("%", null));

            if (yeildRate > YRGreen)
            {
                lblFPR.BackColor = Color.Lime;
                lblFPR.ForeColor = Color.Blue;
            }
            else if (yeildRate > YRYellow)
            {
                lblFPR.BackColor = Color.Yellow;
                lblFPR.ForeColor = Color.Red;
            }
            else
            {
                lblFPR.BackColor = Color.Red;
                lblFPR.ForeColor = Color.White;
            }
        }

        private void YR_RRUpdate_Tick(object sender, EventArgs e)
        {
            try
            {
                ul.SetValueByKey("RYRDATA", "");
                string model = string.Format("{0,-5}{1,-25}{2,12}", "RR-YR", ul.GetModel(), Environment.MachineName.Trim());//Environment.MachineName

                //string station = (checkStationCompare == true) ? stationCompare.Replace("_RB", "") : ul.GetStation().Replace("_RB", "");
                string station = ul.GetStation().Replace("_RB", "");

                bool bok = false;
                for (int i = 0; i < stationOK.Length; i++)
                {
                    if (station == stationOK[i])
                    {
                        bok = true;
                        break;
                    }
                }
                if (!bok)
                {
                    for (int i = 0; i < stationReplace.Length; i++)
                    {
                        if (station.Contains(stationReplace[i]))
                        {
                            station = station.Replace(stationReplace[i], "");
                            break;
                        }

                    }
                }

                string dataNew = sfisB05.SHOWUI_TEST(model, station);

                ul.SetValueByKey("RYRDATA", dataNew);
                string RRYRdata = _StationKey.GetValue("RYRDATA", "").ToString();

                ul.event_log("Update YR-RR 30s: " + dataNew);

                totalRate = Convert.ToSingle(dataNew.Substring(42, 6));
                retestRate = Convert.ToSingle(dataNew.Substring(48, 6));
                yeildRate = Convert.ToSingle(dataNew.Substring(54, 6));

                lblTotalRate.Text = totalRate + "%";
                lblRetestRate.Text = retestRate + "%";
                lblYeildRate.Text = yeildRate + "%";
            }
            catch (Exception ex)
            {
                event_log("YR_RRUpdate_Tick Exception : " + ex.Message);
            }
        }

        private void toolTipVirus_Popup(object sender, PopupEventArgs e)
        {

        }

        private void panel1_Paint_2(object sender, PaintEventArgs e)
        {

        }

        private void pbVirus_MouseHover(object sender, EventArgs e)
        {
            toolTipVirus.SetToolTip(pbVirus, tooltipContentVR);
        }

        private void pbUsb_MouseHover(object sender, EventArgs e)
        {
            toolTipUSB.SetToolTip(pbUsb, tooltipContentUSB);
        }
        bool firstLock = true;
        private void showUI_Shown(object sender, EventArgs e)
        {
            string listReplace = IniFile.ReadIniFile("STATION", "LISTREPLACE", "CTR_", @"F:\lsy\Test\DownloadConfig\AutoDL\Setup.ini", 1000);
            stationReplace = listReplace.Split(',');
            string listOK = IniFile.ReadIniFile("STATION", "LISTOK", "PT,PT0", @"F:\lsy\Test\DownloadConfig\AutoDL\Setup.ini", 1000);
            stationOK = listOK.Split(',');
        }
        string[] stationReplace;
        string[] stationOK;
        bool firstCmsp = true;
        string preTT = "";
        #region TimmerFakeShowUIComment
        private void TimerFakeShowUI_Tick(object sender, EventArgs e)
        {
            //TimerFakeShowUI.Enabled = false;
            try
            {


                ConnectShowUI conn = new ConnectShowUI();
                FakeShowUIHelper fake = new FakeShowUIHelper();
                int checkFake = 0;
                int cmspID = 0;
                DataTable specFakeload;
                int checkLockPc = int.Parse(IniFile.ReadIniFile("Common", "LockPC", "0", ".\\FShowUIConfig.txt"));
                long timeRand = long.Parse(IniFile.ReadIniFile("Common", "TimeRand", "2", ".\\FShowUIConfig.txt"));
                //FakeModel specFakeData;
                int checkStation = conn.CreateOrUpdateDB("SationName", ul.GetStation(), "StationInfo", "10.224.81.62");
                int checkProjId = conn.CreateOrUpdateDB("DotNamePro", _model_name, "ProjectInfo", "10.224.81.62", "StationID", checkStation.ToString()); ;
                int checkCmsp = conn.CreateOrUpdateDB("ProjectID", checkProjId, "CommonSpec", "10.224.81.62");
                long checkRun = (ul.GetValueByKey("StartCheck").Length > 0) ? long.Parse(DateTime.Now.ToString("yyyyMMddHHmm")) - long.Parse(Convert.ToDateTime(ul.GetValueByKey("StartCheck")).ToString("yyyyMMddHHmm")) : 0;
                checkFake = conn.CreateOrUpdateDB("ProjectID", checkProjId, "FProject", "10.224.81.62", "Fake", "1");
                if (checkProjId != 0 && firstCmsp == true)
                {

                    specFakeload = conn.DataTable_Sql($"select top 1 * from Spec where ProjectID={checkProjId}", "10.224.81.62");
                    // specFakeData = new FakeModel() { fakeTRR = 1, spaceRandTRR1 = 1, spaceRandTRR2 = 1, fakeSRR = 1, spaceRandSRR1 = 1, spaceRandSRR2 = 1, fakeTYR = 1, spaceRandTYR1 = 1, spaceRandTYR2 = 1 };

                    if (checkCmsp == 0)
                    {
                        string insertCmsp = $"insert into CommonSpec(TRRcmsp,TYRcmsp,ProjectID) values(3,98.7,{checkProjId})";
                        cmspID = conn.CreateAndGetID("10.224.81.62", insertCmsp, "CommonSpec");
                    }
                    else
                    {
                        string updateCmsp = $"update CommonSpec set TRRcmsp=2.45, TYRcmsp = 98.6 where ProjectID={checkProjId}";
                        cmspID = conn.CreateAndGetID("10.224.81.62", updateCmsp, "CommonSpec", "ProjectID", checkProjId.ToString());
                    }

                    if (checkFake != 0)
                    {
                        firstCmsp = false;
                        conn.Execute_NonSQL($@"update CommonSpec
                                        set TRRcmsp = ROUND({ specFakeload.Rows[0][1]} + RAND() * ({specFakeload.Rows[0][3]} - {specFakeload.Rows[0][2]}) + {specFakeload.Rows[0][2]}, 2), TYRcmsp = ROUND({specFakeload.Rows[0][7]} + RAND() * ({specFakeload.Rows[0][9]} - {specFakeload.Rows[0][8]}) + {specFakeload.Rows[0][8]}, 2)
                                        where ProjectID = {checkProjId}", "10.224.81.62"); //param sql


                    }


                }
                else if (firstCmsp == false && checkRun >= timeRand)
                {
                    specFakeload = conn.DataTable_Sql($"select top 1 * from Spec where ProjectID={checkProjId}", "10.224.81.62");
                    ul.SetValueByKey("StartCheck", DateTime.Now.ToString());
                    conn.Execute_NonSQL($@"update CommonSpec
                                        set TRRcmsp = ROUND({specFakeload.Rows[0][1]} + RAND() * (0.15 - (-0.15)) + (-0.15), 2), TYRcmsp = ROUND({specFakeload.Rows[0][7]} + RAND() * (0.15 - (-0.15)) + (-0.15), 2)
                                        where ProjectID = {checkProjId}", "10.224.81.62");


                }
                bool checkChange = false;
                //set value fake
                DataTable dataFake = conn.DataTable_Sql($"select top 1 * from CommonSpec where ProjectID = {checkProjId}", "10.224.81.62");
                DataTable isFake = conn.DataTable_Sql($"select top 1 * from FProject where ProjectID = {checkProjId}", "10.224.81.62");
                if (dataFake.Rows.Count > 0 && isFake.Rows.Count > 0)
                {
                    this.lblTotalRateFake.Text = dataFake.Rows[0][1] + "%";
                    this.lblYeildRateFake.Text = dataFake.Rows[0][2] + "%";
                    //check limit value          
                    if (fake.ConvertToDouble(lblTotalRateFake.Text) < 0.1)
                    {
                        this.lblTotalRateFake.Text = "2.21%";
                        checkChange = true;
                    }
                    if (fake.ConvertToDouble(lblTotalRateFake.Text) > 3.55)
                    {
                        this.lblTotalRateFake.Text = "2.91%";
                        checkChange = true;
                    }

                    if (fake.ConvertToDouble(lblYeildRateFake.Text) >= 99.9)
                    {
                        this.lblYeildRateFake.Text = "99.51%";
                        checkChange = true;
                    }
                    if (fake.ConvertToDouble(lblYeildRateFake.Text) >= 99.9)
                    {
                        this.lblYeildRateFake.Text = "99.51%";
                        checkChange = true;
                    }
                    if (fake.ConvertToDouble(lblYeildRateFake.Text) < 98.7)
                    {
                        this.lblYeildRateFake.Text = "98.81%";
                        checkChange = true;
                    }
                    if (checkChange == true)
                    {
                        conn.Execute_NonSQL($@"update CommonSpec
                                        set TRRcmsp = {fake.ConvertToDouble(this.lblTotalRateFake.Text)}, TYRcmsp = {fake.ConvertToDouble(this.lblYeildRateFake.Text)}
                                        where ProjectID = {checkProjId}", "10.224.81.62");
                    }
                    //
                    //int index = 0;
                    //DataTable dataPC = conn.DataTable_Sql($"select * from PCInfo where ProjectID = {checkProjId}", "10.224.81.62");

                    //for (int i = 0; i < dataPC.Rows.Count-1; i++)
                    //{
                    //    foreach (var item in dataPC.Rows[i].ItemArray)
                    //    {
                    //        if (item.ToString().Contains(client_ip))
                    //        {
                    //            index = i;
                    //        }
                    //    }
                    //}
                    //var a = double.Parse("1.5");
                    specFakeload = conn.DataTable_Sql($"select top 1 * from Spec where ProjectID={checkProjId}", "10.224.81.62");
                    if (!preTT.Contains(lblTotalRateFake.Text))
                    {
                        preTT = lblTotalRateFake.Text;
                        //if (index < double.Parse((dataPC.Rows.Count / 2).ToString()))
                        //{
                        //    this.lblRetestRateFake.Text = Math.Round((double.Parse(dataFake.Rows[0][1].ToString()) + fake.RandomTwoValue(0, double.Parse(specFakeload.Rows[0][6].ToString()))), 2) + "%";
                        //}
                        //else
                        //{
                        //    this.lblRetestRateFake.Text = Math.Round((double.Parse(dataFake.Rows[0][1].ToString()) + fake.RandomTwoValue(double.Parse(specFakeload.Rows[0][5].ToString()), 0)),2) + "%";

                        //}
                        if (fake.ConvertToDouble(lblRetestRate.Text) > fake.ConvertToDouble(lblTotalRate.Text))
                        {
                            this.lblRetestRateFake.Text = Math.Round((fake.ConvertToDouble(lblTotalRateFake.Text) + fake.RandomTwoValue(0, double.Parse(specFakeload.Rows[0][6].ToString()))), 2) + "%";
                        }
                        else if (fake.ConvertToDouble(lblRetestRate.Text) == fake.ConvertToDouble(lblTotalRate.Text))
                        {
                            this.lblRetestRateFake.Text = lblTotalRateFake.Text;
                        }
                        else
                        {
                            this.lblRetestRateFake.Text = Math.Round((fake.ConvertToDouble(lblTotalRateFake.Text) + fake.RandomTwoValue(double.Parse(specFakeload.Rows[0][5].ToString()), 0)), 2) + "%";
                        }
                        if (fake.ConvertToDouble(lblRetestRateFake.Text) < 0.1)
                        {
                            this.lblRetestRateFake.Text = "0.24%";
                        }

                    }


                }

                //check fake

                if (checkFake != 0)
                {
                    this.lblRetestRateFake.Show();
                    this.lblTotalRateFake.Show();
                    this.lblYeildRateFake.Show();
                    this.lblRetestRate.Hide();
                    this.lblTotalRate.Hide();
                    this.lblYeildRate.Hide();
                }

                else
                {

                    this.lblRetestRateFake.Hide();
                    this.lblTotalRateFake.Hide();
                    this.lblYeildRateFake.Hide();

                    this.lblRetestRate.Show();
                    this.lblTotalRate.Show();
                    this.lblYeildRate.Show();
                }

                // var configFake = fake.FakeUI(_Model, ul.GetStation());
                var path = IniFile.ReadIniFile("Path", "path", @"D:\AutoDL\ConfigLock.txt", ".\\ConfigCheck.txt");

                string checkTR = IniFile.ReadIniFile(_Model, "TRR", "-1", path);
                string checkSR = IniFile.ReadIniFile(_Model, "SRR", "-1", path);



                SetColorFakeLabel(fake.ConvertToDouble(lblTotalRateFake.Text), fake.ConvertToDouble(lblRetestRateFake.Text), fake.ConvertToDouble(lblYeildRateFake.Text));

                //var dataYR = (checkFake == 0) ? lblYeildRate.Text : lblYeildRateFake.Text;
                //var dataSR = (checkFake == 0) ? lblRetestRate.Text : lblRetestRateFake.Text;
                //var dataTR = (checkFake == 0) ? lblTotalRate.Text : lblTotalRateFake.Text;
                //long checkLockHour = (ul.GetValueByKey("LockTime").Length > 0) ? long.Parse(DateTime.Now.ToString("yyyyMMddHHmm")) - long.Parse(Convert.ToDateTime(ul.GetValueByKey("LockTime")).ToString("yyyyMMddHHmm")) : 0;

                //if (firstLock == true && checkLockPc != 0)
                //{


                //	LockPC(fake.ConvertToDouble(dataYR), 1);

                //	LockPC(fake.ConvertToDouble(dataTR), 3);

                //	LockPC(fake.ConvertToDouble(dataSR), 2);

                //}
                //else if (checkLockHour >= 120)
                //{
                //	LockPC(fake.ConvertToDouble(dataYR), 1);

                //	LockPC(fake.ConvertToDouble(dataTR), 3);

                //	LockPC(fake.ConvertToDouble(dataSR), 2);

                //}


            }
            catch
            {


            }
            //TimerFakeShowUI.Enabled = true;

        }
        #endregion

        #region CopyToServerAutomation
        private void CopyServerAuto_Tick(object sender, EventArgs e)
        {
            //CopyServerAuto.Enabled = false;
            //Thread _pathLoss = new Thread(Copy117DB);
            //_pathLoss.IsBackground = true;
            //_pathLoss.Start();
        }

        public void Copy117()
        {
            try
            {
                string Modalname = ul.GetModel();
                string Station = ul.GetStation();
                AutomationCopyPathlossHelper copyToServerAuto = new AutomationCopyPathlossHelper();
                string LocalPath = IniFile.ReadIniFile(Modalname, Station, "empty", ".\\PathLossConfig.txt");
                if (!LocalPath.Contains("empty"))
                {
                    string ServerPathPreFix = @"ftp://10.224.81.99/PathLoss/";
                    string ServerPath = ServerPathPreFix + Modalname + "/" + Station + "/" + DateTime.Now.ToString("yyyy-MM-dd") + "/" + Environment.MachineName + "/";

                    copyToServerAuto.CopyToAutomationServer(LocalPath, ServerPath);
                }
            }
            catch
            {


            }

        }
        public void Copy117DB()
        {
            try
            {
                string Modalname = ul.GetModel();
                string Station = ul.GetStation();
                AutomationCopyPathlossHelper copyToServerAuto = new AutomationCopyPathlossHelper();
                if (!Directory.Exists(LocalPath))
                {
                    return;
                }
                bool isLock = false;
                copyToServerAuto.CopyToAutomationServerDB(LocalPath, out isLock);
                //if (isLock == true)
                //{
                //    ul.SetValueByKey("StopMachine", "1");
                //    _IsExistErrorr = true;
                //    ShowWarningMessage("Lỗi: Pathloss issue, call TE-SETUP Check Pathloss", "Pathloss issue");

                //}

                //string LocalPath = IniFile.ReadIniFile(Modalname, Station, "empty", ".\\PathLossConfig.txt");
                //if (!LocalPath.Contains("empty"))
                //{
                //foreach (var pathPathloss in lstPathpathloss)
                //{
                //    if (!Directory.Exists(pathPathloss))
                //    {
                //        continue;
                //    }
                //    bool isLock = false;
                //    copyToServerAuto.CopyToAutomationServerDB(pathPathloss, out isLock);
                //    if (isLock == true)
                //    {
                //        ul.SetValueByKey("StopMachine", "1");
                //        _IsExistErrorr = true;
                //        ShowWarningMessage("Lỗi: Pathloss issue, call TE-SETUP Check Pathloss", "Pathloss issue");

                //    }
                //}

                //}
            }
            catch (Exception)
            {


            }
            this.Invoke((MethodInvoker)delegate
            {
                CopyServerAuto.Enabled = true;
            });

        }

        public List<string> lstPathpathloss = new List<string>();
        string LocalPath = "";

        public void CheckPathloss()
        {
            string Modalname = ul.GetProduct();
            string Station = ul.GetStation();
            //string numPath = IniFile.ReadIniFile(Station, "PathNum", "empty", @"F:\lsy\ID\PathlossControl\Config\PathLossConfig.txt");
            try
            {
                //int num = Int32.Parse(numPath);
                //for (int i = 0; i <= num; i++)
                //{
                //    string pathLocal = IniFile.ReadIniFile(Station, "path"+i, "empty", @"F:\lsy\ID\PathlossControl\Config\PathLossConfig.txt");
                //    if (pathLocal.Trim().Length > 0 && !pathLocal.Contains("empty") && Directory.Exists(pathLocal))
                //    {
                //        lstPathpathloss.Add(pathLocal);
                //    }


                //}
                //if (lstPathpathloss.Count > 0)
                //{
                //    CopyServerAuto.Enabled = true;
                //}
                LocalPath = IniFile.ReadIniFile(Modalname, Station, "empty", @"F:\lsy\ID\PathlossControl\Config\PathLossConfig.txt");
                if (!LocalPath.Contains("empty") && LocalPath.Trim().Length > 0)
                {
                    CopyServerAuto.Enabled = true;
                    FrmPathloss frmPathloss = new FrmPathloss();
                    frmPathloss.Show();
                }

            }
            catch
            {
                CopyServerAuto.Enabled = false;

            }



        }
        #endregion

        public void LockPC(double data, int option)
        {
            var path = IniFile.ReadIniFile("Path", "path", @"D:\AutoDL\ConfigLock.txt", ".\\ConfigCheck.txt");
            double lockYR = double.Parse(IniFile.ReadIniFile(_Model, "YR", "5", path));
            double lockSR = double.Parse(IniFile.ReadIniFile(_Model, "SR", "5", path));
            double lockTR = double.Parse(IniFile.ReadIniFile(_Model, "TR", "96", path));
            if (ul.GetValueByKey("StopMachine").Contains("0") && option == 1)
            {
                if (data <= lockYR)
                {
                    firstLock = false;
                    ul.SetValueByKey("StopMachine", "1");

                    //UpdateStopMachineStatus(true);
                    _IsExistErrorr = true;
                    ShowWarningMessage("Lỗi: YR is low", "YR is low");
                }
            }
            if (ul.GetValueByKey("StopMachine").Contains("0") && option == 2)
            {
                if (data > lockSR)
                {
                    firstLock = false;
                    ul.SetValueByKey("StopMachine", "1");

                    //UpdateStopMachineStatus(true);
                    _IsExistErrorr = true;
                    ShowWarningMessage("Lỗi: SR is low", "SR is low");
                }
            }
            if (ul.GetValueByKey("StopMachine").Contains("0") && option == 3)
            {
                if (data > lockTR)
                {
                    firstLock = false;
                    ul.SetValueByKey("StopMachine", "1");

                    //UpdateStopMachineStatus(true);
                    _IsExistErrorr = true;
                    ShowWarningMessage("Lỗi: TR is low", "TR is low");
                }
            }

        }
        public void SetLockNew(double data, double value)
        {

        }
        public void SetColorFakeLabel(double fakeTRR, double fakeSTT, double fakeYR)
        {
            if (fakeTRR < TRRGreen)
            {
                lblTotalRateFake.BackColor = Color.Lime;
                lblTotalRateFake.ForeColor = Color.Blue;
            }
            else if (fakeTRR < TRRYellow)
            {
                lblTotalRateFake.BackColor = Color.Yellow;
                lblTotalRateFake.ForeColor = Color.Red;
            }
            else
            {
                lblTotalRateFake.BackColor = Color.Red;
                lblTotalRateFake.ForeColor = Color.White;
            }

            if (fakeSTT < SRRGreen)
            {
                lblRetestRateFake.BackColor = Color.Lime;
                lblRetestRateFake.ForeColor = Color.Blue;
            }
            else if (fakeSTT < SRRYellow)
            {
                lblRetestRateFake.BackColor = Color.Yellow;
                lblRetestRateFake.ForeColor = Color.Red;
            }
            else
            {
                lblRetestRateFake.BackColor = Color.Red;
                lblRetestRateFake.ForeColor = Color.White;
            }

            if (fakeYR > YRGreen)
            {
                lblYeildRateFake.BackColor = Color.Lime;
                lblYeildRateFake.ForeColor = Color.Blue;
            }
            else if (fakeYR > YRYellow)
            {
                lblYeildRateFake.BackColor = Color.Yellow;
                lblYeildRateFake.ForeColor = Color.Red;
            }
            else
            {
                lblYeildRateFake.BackColor = Color.Red;
                lblYeildRateFake.ForeColor = Color.White;
            }
        }

        public int chekcLock = 0;
        public int test = 0;
        //public bool firstLockSU = true;
        private void LockShowUI_Tick(object sender, EventArgs e)
        {
            if (chekcLock > 15000)
            {
                chekcLock = 0;
            }
            ConnectShowUI conn = new ConnectShowUI();
            try
            {
                int checkFake = 0;

                string ModelName = ul.GetProduct();
                string Station = ul.GetStation();
                CheckNTGR checkNtgr = new CheckNTGR();
                if (checkNtgr.CheckLock() == 1)
                {
                    chekcLock += (LockShowUI.Interval / 1000);
                    if (chekcLock >= checkNtgr.GetTimeLock())
                    {
                        try
                        {
                            int checkStation = conn.CreateOrUpdateDB("SationName", ul.GetStation(), "StationInfo", "10.224.81.62");
                            int checkProjId = conn.CreateOrUpdateDB("DotNamePro", _model_name, "ProjectInfo", "10.224.81.62", "StationID", checkStation.ToString());
                            checkFake = conn.CreateOrUpdateDB("ProjectID", checkProjId, "FProject", "10.224.81.62", "Fake", "1");
                        }
                        catch (Exception)
                        {
                            checkFake = 0;

                        }

                        ConfigLockModal datalock = checkNtgr.getDataLock();
                        if (datalock != null && checkFake == 0)
                        {
                            Process[] pro = Process.GetProcessesByName("CloseShowUILock");
                            foreach (var item in pro)
                            {
                                item.Kill();
                            }

                            double TRR = double.Parse(lblTotalRate.Text.Replace("%", ""));
                            double YR = double.Parse(lblYeildRate.Text.Replace("%", ""));
                            if (TRR > datalock.TRR)
                            {
                                LockShowUI.Enabled = false;
                                string _LockingMessage = "Lỗi: Total Rate cao hơn " + datalock.TRR + " %! goi qa/pqe!";
                                ul.SetValueByKey("StopMachine", "1");
                                _IsExistErrorr = true;
                                ShowWarningMessage(_LockingMessage, "Total Rate " + lblTotalRate.Text + " cao hơn " + datalock.TRR + "%!");
                                LockShowUI.Enabled = true;

                                chekcLock = 0;
                                return;
                            }
                            if (YR < datalock.YR)
                            {
                                LockShowUI.Enabled = false;
                                string _LockingMessage = "Lỗi: Yeild Rate thấp hơn " + datalock.YR + " %! goi qa/pqe!";
                                ul.SetValueByKey("StopMachine", "1");
                                _IsExistErrorr = true;
                                ShowWarningMessage(_LockingMessage, "Yeild Rate " + lblYeildRate.Text + " thấp hơn " + datalock.YR + "%!");
                                LockShowUI.Enabled = true;

                                chekcLock = 0;
                                return;
                            }


                        }

                    }

                }

            }
            catch (Exception ex)
            {

                LockShowUI.Enabled = true;
            }

        }

        public void SetTcpAck()
        {
            TcpAck tcpAck = new TcpAck();
            tcpAck.SetTcpAckByDUT();
        }

        public void ShowTimeDontTest()
        {
            TimeDoNotUse tDU = new TimeDoNotUse();
            tDU.ShowDialog();
        }


    }

}