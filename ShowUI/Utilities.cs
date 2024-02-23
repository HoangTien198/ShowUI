using Microsoft.Win32;
using ShowUIApp;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows.Forms;

namespace ShowUI
{
    internal class Utilities
    {
        public string checkDateShift()
        {
            string StartTime, EndTime;
            DateTime dtCheck = DateTime.Now;
            string StrShift;
            int iTimeCheck = int.Parse(DateTime.Now.ToString("HHmmss"));
            if (iTimeCheck > 73000 && iTimeCheck < 193000)
            {
                StrShift = "D";
                StartTime = dtCheck.ToString("yyyyMMdd") + "08";
                EndTime = dtCheck.ToString("yyyyMMdd") + "19";
            }
            else
            {
                StrShift = "N";

                if (iTimeCheck > 193000 && iTimeCheck < 235959)
                {
                    StartTime = dtCheck.ToString("yyyyMMdd") + "20";
                    EndTime = dtCheck.AddDays(+1).ToString("yyyyMMdd") + "07";
                }
                else
                {
                    StartTime = dtCheck.AddDays(-1).ToString("yyyyMMdd") + "20";
                    EndTime = dtCheck.ToString("yyyyMMdd") + "07";
                }
            }
            return StrShift + "_" + StartTime + "_" + EndTime;
        }

        public void ShellExecute(string Command)
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

        public string GetKeyIn()
        {
            string rString = "";
            using (frmTextBox wb = new frmTextBox())
            {
                if (wb.ShowDialog() == DialogResult.OK)
                {
                    rString = wb.getInput().Trim();
                }
            }
            return rString;
        }

        public string GetServerIP(string key_name, string defaultIp)
        {
            try
            {
                string SvIP = IniFile.ReadIniFile("ServerIP", key_name, defaultIp, @"D:\AutoDL\Setup.ini");

                return SvIP;
            }
            catch (Exception)
            {
                return defaultIp;
            }
        }

        public string GetModel()
        {
            string Model = GetValueByKey("SFISMODEL");
            return Model.Trim();
        }

        private RegistryKey _OpenKey;
        private string openkey;
        private string SubKey = @"SOFTWARE\Netgear\STATION";

        public string GetStation()
        {
            string Station = "";
            RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            _OpenKey = baseKey.OpenSubKey(SubKey);
            try
            {
                if (_OpenKey != null)
                {
                    openkey = _OpenKey.GetValue("OpenKey", "").ToString().ToUpper();
                    string[] tempProduct = openkey.Split('\\');
                    //int tempProductLength = ;
                    if (openkey.Contains("HIPOT"))
                    {
                        Station = "HIPOT";
                    }
                    if (openkey.Contains("NMRP"))
                    {
                        Station = "NMRP";
                    }

                    if (tempProduct.Length > 2)
                    {
                        Station = tempProduct[2];
                    }
                    else
                    {
                        if (openkey.Contains("HIPOT"))
                        {
                            Station = "HIPOT";
                        }
                        else if (openkey.Contains("NMRP"))
                        {
                            Station = "NMRP";
                        }
                        else
                        {
                            Station = "BOOMS";
                        }
                    }
                }
                return Station.Trim();
            }
            catch (Exception)
            {
                return "BOOMS";
            }
        }

        public string GetProduct()
        {
            string Product = "";
            RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            _OpenKey = baseKey.OpenSubKey(SubKey);
            try
            {
                if (_OpenKey != null)
                {
                    openkey = _OpenKey.GetValue("OpenKey", "").ToString().ToUpper();
                    string[] tempProduct = openkey.Split('\\');
                    if (tempProduct.Length > 1)
                    {
                        Product = tempProduct[1];
                        //MessageBox.Show(Product);
                    }
                    else
                    {
                        Product = "BOOMP";
                    }
                }
                return Product.Trim();
            }
            catch (Exception)
            {
                return "BOOMP";
            }
        }

        public string GetValueByKey(string _key)
        {
            try
            {
                RegistryKey kiwi = Registry.LocalMachine.OpenSubKey(SubKey, true);
                //string[] _Key = kiwi.GetValue("OpenKey", null).ToString().Split('\\');
                string _Model = GetProduct();
                string _Sta = GetStation();
                kiwi = Registry.LocalMachine.OpenSubKey(SubKey + "\\" + _Model + "\\" + _Sta, true);
                string SN = kiwi.GetValue(_key, "").ToString().Trim();
                return SN;
            }
            catch (Exception ex)
            {
                return "X";
                //throw;
            }
        }

        public void SetValueByKey(string _key, string _Val)
        {
            try
            {
                RegistryKey kiwi = Registry.LocalMachine.OpenSubKey(SubKey, true);
                //string[] _Key = kiwi.GetValue("OpenKey", null).ToString().Split('\\');
                string _Model = GetProduct();
                string _Sta = GetStation();
                kiwi = Registry.LocalMachine.OpenSubKey(SubKey + "\\" + _Model + "\\" + _Sta, true);
                if (kiwi == null)
                    kiwi = Registry.LocalMachine.CreateSubKey(SubKey + "\\" + _Model + "\\" + _Sta, RegistryKeyPermissionCheck.ReadWriteSubTree);
                kiwi.SetValue(_key, _Val, RegistryValueKind.String);
            }
            catch (Exception r)
            {
                //MessageBox.Show(r.ToString());
            }
        }

        public string GetNICGatewayIP()
        {
            string client_ip = "";
            using (Socket sck = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                try
                {
                    sck.Connect("10.224.80.37", 1008);
                    IPEndPoint endPoint = sck.LocalEndPoint as IPEndPoint;
                    client_ip = endPoint.Address.ToString();
                    sck.Close();
                }
                catch (Exception)
                {
                    sck.Close();
                }
            }
            //NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            //foreach (NetworkInterface adapter in nics)
            //{
            //    IPInterfaceProperties properties = adapter.GetIPProperties();
            //    Console.WriteLine();
            //    Console.WriteLine(adapter.Description);
            //    Console.WriteLine(String.Empty.PadLeft(adapter.Description.Length, '='));
            //    Console.WriteLine("  Interface type .......................... : {0}", adapter.NetworkInterfaceType);
            //    Console.WriteLine("  Physical Address ........................ : {0}",
            //               adapter.GetPhysicalAddress().ToString());
            //    if (properties.GatewayAddresses.Count > 0)
            //    {
            //        Console.WriteLine("  Gateway ip .............................. : {0}", properties.GatewayAddresses[0].Address);
            //        if (properties.UnicastAddresses.Count > 0)
            //        {
            //            foreach (UnicastIPAddressInformation ipInfo in properties.UnicastAddresses)
            //            {
            //                if (ipInfo.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            //                {
            //                    Console.WriteLine("  Ip address............................... : {0}", ipInfo.Address);
            //                    return ipInfo.Address.ToString();
            //                }
            //            }
            //        }
            //    }
            //}
            if (client_ip != "")
                return client_ip;
            else
                return "No IP Prefix: 138 !";
        }

        public string WorkSection()
        {
            double tmpDate = Convert.ToDouble(DateTime.Now.ToString("HHmm"));

            if (0 <= tmpDate && tmpDate <= 30)
                return "0";
            else if (30 < tmpDate && tmpDate <= 130)
                return "1";
            else if (130 < tmpDate && tmpDate <= 230)
                return "2";
            else if (230 < tmpDate && tmpDate <= 330)
                return "3";
            else if (330 < tmpDate && tmpDate <= 430)
                return "4";
            else if (430 < tmpDate && tmpDate <= 530)
                return "5";
            else if (530 < tmpDate && tmpDate <= 630)
                return "6";
            else if (630 < tmpDate && tmpDate <= 730)
                return "7";
            else if (730 < tmpDate && tmpDate <= 830)
                return "8";
            else if (830 < tmpDate && tmpDate <= 930)
                return "9";
            else if (930 < tmpDate && tmpDate <= 1030)
                return "10";
            else if (1030 < tmpDate && tmpDate <= 1130)
                return "11";
            else if (1130 < tmpDate && tmpDate <= 1230)
                return "12";
            else if (1230 < tmpDate && tmpDate <= 1330)
                return "13";
            else if (1330 < tmpDate && tmpDate <= 1430)
                return "14";
            else if (1430 < tmpDate && tmpDate <= 1530)
                return "15";
            else if (1530 < tmpDate && tmpDate <= 1630)
                return "16";
            else if (1630 < tmpDate && tmpDate <= 1730)
                return "17";
            else if (1730 < tmpDate && tmpDate <= 1830)
                return "18";
            else if (1830 < tmpDate && tmpDate <= 1930)
                return "19";
            else if (1930 < tmpDate && tmpDate <= 2030)
                return "20";
            else if (2030 < tmpDate && tmpDate <= 2130)
                return "21";
            else if (2130 < tmpDate && tmpDate <= 2230)
                return "22";
            else if (2230 < tmpDate && tmpDate <= 2330)
                return "23";
            else if (2330 < tmpDate && tmpDate <= 2359)
                return "24";
            else
                return "0";
        }

        public bool CheckNetworkAvailable()
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public double RandDoubleInRange(double min, double max)
        {
            Random rd = new Random();
            return rd.NextDouble() * (max - min) + min;
        }

        public double GreenShowUI(string connStr, string EnableConnectServer37, string[] nData)
        {
            double returnVal = 2.45;
            if (EnableConnectServer37 == "0")
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connStr))
                    {
                        //connection.Open();
                        //string sql = "update Station set TEMP_RETEST_RATE = @retestRate where station_ip = @ip";
                        //SqlCommand command = new SqlCommand(sql, connection);
                        //command.Parameters.Add("@retestRate", SqlDbType.Decimal, 2);
                        //command.Parameters["@retestRate"].Value = retestRate;

                        //command.Parameters.Add("@ip", SqlDbType.NVarChar, 50);
                        //command.Parameters["@ip"].Value = nData[3]; // index 3 for client ip
                        //command.ExecuteNonQuery();
                        //connection.Close();

                        connection.Open();
                        // by model as well
                        //19-05-2018 Adele fix error all station show TRR = 2.45

                        string sqlAvgRTR = "select AVG(cast(a.TEMP_RETEST_RATE as decimal(16,4))) as TRR from Station a, stationinfo b";
                        sqlAvgRTR += " where a.STATION_IP = b.STATION_IP and datediff(HH,a.DATE_TIME,GETDATE()) < 0.15 and b.LINE = '" + nData[0] + "' and b.TYPE= '" + nData[1] + "' and a.MODEL_NAME ='" + nData[5] + "'";
                        //datediff(HH,a.DATE_TIME,GETDATE()) < 0.15 to get avg of recent 10' having testing
                        //AutoClosingMessageBox.Show(sqlAvgRTR+"", "", 10000);
                        SqlDataAdapter da = new SqlDataAdapter(sqlAvgRTR, connStr);
                        DataSet ds = new DataSet();
                        da.Fill(ds);

                        if (ds.Tables[0].Rows[0]["TRR"].ToString() != "")
                        {
                            returnVal = Convert.ToDouble(ds.Tables[0].Rows[0]["TRR"].ToString());
                        }
                        da.Dispose();
                        connection.Close();
                    }

                    return returnVal;
                }
                catch (Exception r)
                {
                    event_log("TRR Exception: " + r.ToString());
                    return returnVal;
                }
            }
            else
            {
                return returnVal;
            }
        }

        public double GetFakeYR(string connStr, string EnableConnectServer37, string[] nData)
        {
            double returnVal = 99.1;
            if (EnableConnectServer37 == "0")
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connStr))
                    {
                        connection.Open();

                        string sqlAvgRTR = "select AVG(cast(a.YEILD_RATE as decimal(16,4))) as YR from Station a, stationinfo b";
                        sqlAvgRTR += " where a.STATION_IP = b.STATION_IP and datediff(HH,a.DATE_TIME,GETDATE()) < 0.15 and b.LINE = '" + nData[0] + "' and b.TYPE= '" + nData[1] + "' and a.MODEL_NAME ='" + nData[5] + "'";

                        SqlDataAdapter da = new SqlDataAdapter(sqlAvgRTR, connStr);
                        DataSet ds = new DataSet();
                        da.Fill(ds);

                        if (ds.Tables[0].Rows[0]["YR"].ToString() != "")
                        {
                            returnVal = Convert.ToDouble(ds.Tables[0].Rows[0]["YR"].ToString());
                        }
                        da.Dispose();
                        connection.Close();
                    }

                    return returnVal;
                }
                catch (Exception r)
                {
                    event_log("YR Exception: " + r.ToString());
                    return returnVal;
                }
            }
            else
            {
                return returnVal;
            }
        }

        //

        public int UpdateRR_YR(double retestRate, double yeild_rate, string connStr, string EnableConnectServer37, string ip)
        {
            int rs = 0;
            if (EnableConnectServer37 == "0")
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connStr))
                    {
                        connection.Open();
                        string sql = "update Station set TEMP_RETEST_RATE = @retestRate, YEILD_RATE = @yeild_rate where station_ip = @ip";
                        SqlCommand command = new SqlCommand(sql, connection);
                        command.Parameters.Add("@retestRate", SqlDbType.Decimal, 2);
                        command.Parameters["@retestRate"].Value = retestRate;

                        command.Parameters.Add("@yeild_rate", SqlDbType.Decimal, 2);
                        command.Parameters["@yeild_rate"].Value = yeild_rate;

                        command.Parameters.Add("@ip", SqlDbType.NVarChar, 50);
                        command.Parameters["@ip"].Value = ip; // index 3 for client ip
                        rs = command.ExecuteNonQuery();
                        connection.Close();
                    }

                    return rs;
                }
                catch (Exception r)
                {
                    event_log("TRR Exception: " + r.ToString());
                    return rs;
                }
            }
            else
            {
                return rs;
            }
        }

        public void ResetArsSystem(string connStr, string EnableConnectServer37)
        {
            try
            {
                if (EnableConnectServer37 == "0" && (DateTime.Now.Hour == 8 || DateTime.Now.Hour == 20))
                {
                    using (SqlConnection connection = new SqlConnection(connStr))
                    {
                        connection.Open();
                        string sql = "delete from StationInfo where STATION_NAME = 'SanderPatrick'";
                        SqlCommand command = new SqlCommand(sql, connection);
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                }

                if (EnableConnectServer37 == "0" && (DateTime.Now.Hour == 7 || DateTime.Now.Hour == 19))
                {
                    bool isDeleteAll = false;
                    try
                    {
                        SqlConnection connection = new SqlConnection(connStr);
                        connection.Open();
                        SqlDataReader reader;
                        string sqlStr = @"select * from StationInfo  where STATION_NAME = 'SanderPatrick'";

                        SqlCommand cmd = new SqlCommand(sqlStr, connection);
                        SqlDataAdapter da = new SqlDataAdapter(sqlStr, connStr);
                        DataSet ds = new DataSet();
                        da.Fill(ds);
                        reader = cmd.ExecuteReader();
                        if (!reader.HasRows)
                        {
                            isDeleteAll = true;
                        }
                        reader.Dispose();
                        //event_log("UseSpecMode: " + UseSpecMode.ToString());
                    }
                    catch (Exception)
                    {
                    }

                    if (isDeleteAll)
                    {
                        using (SqlConnection connection = new SqlConnection(connStr))
                        {
                            connection.Open();
                            string sql = "delete from StationInfo";
                            SqlCommand command = new SqlCommand(sql, connection);
                            command.ExecuteNonQuery();
                            connection.Close();
                        }
                        using (SqlConnection connection = new SqlConnection(connStr))
                        {
                            connection.Open();
                            string sql = "delete from Station";
                            SqlCommand command = new SqlCommand(sql, connection);
                            command.ExecuteNonQuery();
                            connection.Close();
                        }
                        using (SqlConnection connection = new SqlConnection(connStr))
                        {
                            connection.Open();
                            string sql = "INSERT INTO StationInfo (STATION_NAME,STATION_IP,MAC_ADDR,LINE,TYPE) VALUES (@STATION,@IP,@MAC_ADDR,@LINE,@TYPE)";
                            SqlCommand command = new SqlCommand(sql, connection);
                            command.Parameters.Add("@IP", SqlDbType.NVarChar, 50);
                            command.Parameters.Add("@MAC_ADDR", SqlDbType.NVarChar, 50);
                            command.Parameters.Add("@LINE", SqlDbType.VarChar, 10);
                            command.Parameters.Add("@STATION", SqlDbType.NVarChar, 50);
                            command.Parameters.Add("@TYPE", SqlDbType.NVarChar, 10);
                            command.Parameters["@IP"].Value = "192.168.111.111";
                            command.Parameters["@MAC_ADDR"].Value = "SanderPatrick";
                            command.Parameters["@LINE"].Value = "SanderPatrick";
                            command.Parameters["@STATION"].Value = "SanderPatrick";
                            command.Parameters["@TYPE"].Value = "SanderPatrick";
                            command.ExecuteNonQuery();
                            connection.Close();
                        }
                    }//
                }
            }
            catch (Exception r)
            {
                event_log("ResetArsSystem: " + r.ToString());
            }
        }///
    }
}