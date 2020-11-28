
using ShowUIApp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Data;


namespace ShowUI.AutomationHelper
{
    public class AutomationCopyPathlossHelper
    {
        ShowUI.Utilities ul = new ShowUI.Utilities();
        public void CreateIni()
        {
            try
            {
                string Modalname = ul.GetModel();
                string Station = ul.GetStation();
                if (!File.Exists(".\\PathLossConfig.txt"))
                {
                    File.Create(".\\PathLossConfig.txt");
                }
                Thread.Sleep(200);
                string PathAutomationLocal = IniFile.ReadIniFile(Modalname, Station, "empty", ".\\PathLossConfig.txt");
                if (PathAutomationLocal.Contains("empty") || String.IsNullOrEmpty(PathAutomationLocal))
                {
                    IniFile.WriteValue(Modalname, Station, "empty", ".\\PathLossConfig.txt");

                }
            }
            catch
            {

            }
        }

        public bool FtpDirectoryExists(string directoryPath, string ftpUser, string ftpPassword)
        {
            bool isexist = false;

            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(directoryPath);
                request.Credentials = new NetworkCredential(ftpUser, ftpPassword);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    isexist = true;
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    FtpWebResponse response = (FtpWebResponse)ex.Response;
                    if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    {
                        return false;
                    }
                }
            }
            return isexist;
        }
        public void CreateDirecFtp(string path)
        {
            FtpWebRequest reqFTP = null;
            Stream ftpStream = null;
            reqFTP = (FtpWebRequest)FtpWebRequest.Create(path);
            reqFTP.Method = WebRequestMethods.Ftp.MakeDirectory;
            reqFTP.UseBinary = true;
            reqFTP.Credentials = new NetworkCredential("oper", "123");
            FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
            ftpStream = response.GetResponseStream();
            ftpStream.Close();
            response.Close();
            //WebRequest request = WebRequest.Create(path);// "ftp://host.com/directory");
            //request.Method = WebRequestMethods.Ftp.MakeDirectory;
            //request.Credentials = new NetworkCredential("oper", "123");
        }
        public void UpdateDb(string Model, string Station, string PCName, string Data, string FilePathloss, DateTime datePathlossFile)
        {
            Connect117 conn = new Connect117();
            string sql = $@"INSERT INTO dbo.PathLoss(Dotname,Station,PCName,DateTest,DataPathLoss,FilePathLoss,TimeUpload,TimePathloss)
                            VALUES('{Model}','{Station}','{PCName}','{DateTime.Now.ToString("yyyyMMdd")}','{Data}','{FilePathloss}','{DateTime.Now}','{datePathlossFile}')";
            int a = conn.Execute_NonSQL(sql, "10.224.81.49,1434");
        }
        public void UpdateRecordDb(string Model, string Station, string Data, string FileName)
        {
            Connect117 conn = new Connect117();
            string sql = $@"INSERT INTO RecordPathloss(Dotname,Station,PCName,DateTest,TimeUpload,FileName,RecordPathloss) 
                            VALUES('{Model}','{Station}','{Environment.MachineName}','{DateTime.Now.ToString("yyyyMMdd")}','{DateTime.Now}','{FileName}','{Data}')";
            int a = conn.Execute_NonSQL(sql, "10.224.81.49,1434");
        }
        public void UploadFile(string Localpath, string Serverpath, string UserId, string Password)
        {
            try
            {
                string From = Localpath;
                string To = Serverpath;

                using (WebClient client = new WebClient())
                {
                    client.Credentials = new NetworkCredential(UserId, Password);
                    client.UploadFile(To, WebRequestMethods.Ftp.UploadFile, From);
                    client.Dispose();
                }
            }
            catch (Exception)
            {


            }

        }
        public bool EqualsUpToSeconds(DateTime dt1, DateTime dt2)
        {
            return dt1.Year == dt2.Year && dt1.Month == dt2.Month && dt1.Day == dt2.Day &&
                   dt1.Hour == dt2.Hour && dt1.Minute == dt2.Minute && dt1.Second == dt2.Second;
        }
        public void CompareByAntena(List<DataPathloss> dtPathloss, out List<string> lstErr)
        {
            string Station = ul.GetStation();

            string Product = ul.GetProduct();
            string delta = IniFile.ReadIniFile(Product, "DeltaAtena_"+Station, "3", @"F:\lsy\ID\PathlossControl\Config\PathLossConfig.txt");
            double deltaVal;
            try
            {
                deltaVal = double.Parse(delta);
            }
            catch (Exception)
            {

                deltaVal = 1;
            }

            lstErr = new List<string>();
            foreach (var data in dtPathloss)
            {
                var lstValuePl = data.Data.Split(',').Where(x => x.Length > 0).Take(5).ToList();
                for (int i = 1; i < lstValuePl.Count; i++)
                {
                    try
                    {
                        if (double.Parse(lstValuePl[i]) == 0)
                        {
                            continue;
                        }
                        if (Math.Round(Math.Abs(double.Parse(lstValuePl[i + 1]) - double.Parse(lstValuePl[i])), 2) > deltaVal)
                        {
                            lstErr.Add($"key {data.Key}: index {i} and index {i + 1} lager than delta {deltaVal}");
                        }
                    }
                    catch (Exception)
                    {


                    }

                }

            }

        }
        public string GetTimeModifyFile(string ModelName, string Station, string PCName, string PathLossFileName, string DateTest)
        {
            try
            {
                Connect117 conn = new Connect117();
                string sql = $@"select top 1 TimePathloss from PathLoss where
                            DateTest = '{DateTest}' 
                            and Dotname = '{ModelName}'
                            and Station= '{Station}'
                            and PCName= '{PCName}'
                            and FilePathLoss= '{PathLossFileName}'
                            order by TimeUpload desc";
                var dt = conn.DataTable_Sql(sql, "10.224.81.49,1434");
                string hutt = "";
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        foreach (DataColumn dc in dt.Columns)
                        {
                            hutt = dr[dc].ToString();
                        }
                    }
                    return hutt;
                }
                else
                {
                    return "";
                }
            }
            catch
            {
                return "";

            }

        }
        public void CompareByBand(List<DataPathloss> dtPathloss, out List<string> lstErr)
        {
            string Station = ul.GetStation();

            string Product = ul.GetProduct();
            string delta2g = IniFile.ReadIniFile(Product, "Delta2g_"+Station, "3", @"F:\lsy\ID\PathlossControl\Config\PathLossConfig.txt");
            string delta5gl = IniFile.ReadIniFile(Product, "Delta5gl_"+ Station, "3", @"F:\lsy\ID\PathlossControl\Config\PathLossConfig.txt");
            string delta5gh = IniFile.ReadIniFile(Product, "Delta5gh_"+ Station, "3", @"F:\lsy\ID\PathlossControl\Config\PathLossConfig.txt");
            double delta2gVal, delta5glVal, delta5ghVal;
            try
            {
                delta2gVal = double.Parse(delta2g);
            }
            catch (Exception)
            {

                delta2gVal = 3;
            }
            try
            {
                delta5glVal = double.Parse(delta5gl);
            }
            catch (Exception)
            {

                delta5glVal = 3;
            }
            try
            {
                delta5ghVal = double.Parse(delta5gh);
            }
            catch (Exception)
            {

                delta5ghVal = 3;
            }
            lstErr = new List<string>();
            List<string> lstDataTop = dtPathloss.First().Data.Split(',').Where(x => x.Trim().Length > 0).Take(5).ToList();
            List<DataPathloss> lst2g = dtPathloss.Where(x => Int32.Parse(x.Key) < 5000).ToList();
            List<DataPathloss> lst5gl = dtPathloss.Where(x => Int32.Parse(x.Key) < 5500 && Int32.Parse(x.Key) >= 5000).ToList();
            List<string> lstDataTop5gl = lst5gl.First().Data.Split(',').Where(x => x.Trim().Length > 0).Take(5).ToList();
            List<DataPathloss> lst5gh = dtPathloss.Where(x => Int32.Parse(x.Key) >= 5500).ToList();
            List<string> lstDataTop5gh = lst5gh.First().Data.Split(',').Where(x => x.Trim().Length > 0).Take(5).ToList();

            List<string> Result = new List<string>();
            //2g
            for (int i = 1; i < lstDataTop.Count; i++)
            {
                List<double> dataTmp = new List<double>();
                foreach (var item in lst2g)
                {
                    try
                    {
                        var dataPathlossTmp = item.Data.Split(',').Where(x => x.Trim().Length > 0).Take(5).ToList();
                        if (double.Parse(dataPathlossTmp[i]) > 0)
                        {

                            dataTmp.Add(double.Parse(dataPathlossTmp[i]));
                        }
                    }
                    catch (Exception)
                    {


                    }

                }
                if (dataTmp.Count > 2)
                {
                    var max = dataTmp.Max();
                    var min = dataTmp.Min();
                    if (max - min > delta2gVal)
                    {
                        lstErr.Add($"2g index {i} deltal max {max} and min {min} lager than {delta2gVal}");
                    }
                }

            }
            //5gl
            for (int i = 1; i < lstDataTop5gl.Count; i++)
            {
                List<double> dataTmp = new List<double>();
                foreach (var item in lst5gl)
                {
                    try
                    {
                        var dataPathlossTmp = item.Data.Split(',').Where(x => x.Trim().Length > 0).Take(5).ToList();
                        if (double.Parse(dataPathlossTmp[i]) > 0)
                        {
                            dataTmp.Add(double.Parse(dataPathlossTmp[i]));
                        }
                    }
                    catch (Exception)
                    {


                    }

                }
                if (dataTmp.Count > 2)
                {
                    var max = dataTmp.Max();
                    var min = dataTmp.Min();
                    if (max - min > delta5glVal)
                    {
                        lstErr.Add($"5gl index {i} deltal max {max} and min {min} lager than {delta5glVal}");
                    }
                }

            }
            //5gh
            for (int i = 1; i < lstDataTop5gh.Count; i++)
            {
                List<double> dataTmp = new List<double>();
                foreach (var item in lst5gh)
                {
                    try
                    {
                        var dataPathlossTmp = item.Data.Split(',').Where(x => x.Trim().Length > 0).Take(5).ToList();
                        if (double.Parse(dataPathlossTmp[i]) > 0)
                        {
                            dataTmp.Add(double.Parse(dataPathlossTmp[i]));
                        }
                    }
                    catch (Exception)
                    {


                    }

                }
                if (dataTmp.Count > 2)
                {
                    var max = dataTmp.Max();
                    var min = dataTmp.Min();
                    if (max - min > delta5ghVal)
                    {
                        lstErr.Add($"5gh index {i} deltal max {max} and min {min} lager than {delta5ghVal}");
                    }
                }

            }

        }
        public void CopyToAutomationServer(string LocalPath, string ServerPath)
        {
            try
            {
                if (FtpDirectoryExists(ServerPath, "oper", "123") == false)
                {
                    CreateDirecFtp(ServerPath);
                }

                string[] files = Directory.GetFiles(LocalPath, "*.csv");
                foreach (var item in files)
                {


                    FileInfo fInfo = new FileInfo(item);
                    UploadFile(fInfo.FullName, ServerPath + "/" + fInfo.Name, "oper", "123");
                }
            }
            catch (Exception)
            {


            }


            //ImpersonatedUser IPUser = new ImpersonatedUser("oper", "TE-TPG-SERVER", "123");
            //try
            //{

            //	string Modalname = ul.GetModel();
            //	string Station = ul.GetStation();
            //	string PathAutomationLocal = IniFile.ReadIniFile(Modalname, Station, "empty", ".\\PathLossConfig.txt");
            //	if (!PathAutomationLocal.Contains("empty"))
            //	{

            //		if (!Directory.Exists(ServerPath))
            //		{
            //			Directory.CreateDirectory(ServerPath);
            //		}
            //		string[] files = Directory.GetFiles(LocalPath, "*.csv");
            //		foreach (var item in files)
            //		{
            //			FileInfo fInfo = new FileInfo(item);
            //			var path = ServerPath + Path.GetFileName(fInfo.Name);
            //			File.Copy(fInfo.FullName, ServerPath + Path.GetFileName(fInfo.Name), true);
            //		}
            //		IPUser.Dispose();


            //	}

            //}
            //catch (Exception ex)
            //{
            //	MessageBox.Show("err copy: "+ ex.ToString());
            //	IPUser.Dispose();
            //}

        }
        public string ConvertCsvFileToJsonObject(string path)
        {

            var lines = File.ReadAllLines(path);
            var listObjResult = new List<Dictionary<string, string>>();

            for (int i = 0; i < lines.Length; i++)
            {
                var objResult = new Dictionary<string, string>();
                objResult.Add("Key", lines[i].Split(',').First());
                objResult.Add("Data", lines[i]);
                listObjResult.Add(objResult);
            }

            return JsonConvert.SerializeObject(listObjResult);
        }
        public string GetDataNew(string ModelName, string Station, string PCName, string PathLossFileName, string DateTest)
        {
            Connect117 conn = new Connect117();
            string sql = $@"select top 1 DataPathLoss from dbo.PathLoss where 
                            DateTest = '{DateTest}' 
                            and Dotname = '{ModelName}'
                            and Station= '{Station}'
                            and PCName= '{PCName}'
                            and FilePathLoss= '{PathLossFileName}'
                            order by TimeUpload desc";
            var dt = conn.DataTable_Sql(sql, "10.224.81.49,1434");
            //var data = dt.AsEnumerable();//.Select(x => x.Field<int>("DataPathLoss")).FirstOrDefault();
            string hutt = "";
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    foreach (DataColumn dc in dt.Columns)
                    {
                        hutt = dr[dc].ToString();
                    }
                }
            }

            return hutt;

        }

        public List<string> CompareTwoData(List<DataPathloss> data1, List<DataPathloss> data2, out List<string> lstErr)
        {
            lstErr = new List<string>();
            try
            {
                foreach (var pl1 in data1)
                {
                    var pl2 = data2.Where(x => x.Key == pl1.Key).FirstOrDefault();
                    if (pl2 != null)
                    {
                        var dtPl1 = pl1.Data.Split(',').Where(x => x.Trim().Length > 0).Take(5).ToList();
                        var dtPl2 = pl2.Data.Split(',').Where(x => x.Trim().Length > 0).Take(5).ToList();
                        for (int i = 1; i < dtPl1.Count; i++)
                        {
                            if (double.Parse(dtPl1[i]) - double.Parse(dtPl2[i]) >= 1 || double.Parse(dtPl1[i]) - double.Parse(dtPl2[i]) <= -1)
                            {
                                lstErr.Add($"diff in {pl1.Key} ==> index: {i + 1}");
                            }
                        }
                    }
                    else
                    {
                        lstErr.Add($"Empty key {pl1.Key}");
                    }

                }
            }
            catch (Exception)
            {


            }


            return new List<string>();
        }
        public int GetShift()
        {
            var _now = DateTime.Now.Hour;
            if (_now > 7 && _now < 20)
            {
                return 1;
            }

            return 0;

        }
        public void UpdateData(string DotName, string Station, string Status)
        {
            int shift = GetShift();
            string _now = DateTime.Now.ToString("yyyy-MM-dd");
            if (DateTime.Now.Hour <= 7)
            {
                _now = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            }
            Connect117 conn = new Connect117();
            string _sql = $@"update PathlossByShift
                            set Status = '{Status}'
                            where Dotname='{DotName}' and Station='{Station}' 
                            and DateTest='{_now}' and Shift={shift} and PCName='{Environment.MachineName}'";
            conn.Execute_NonSQL(_sql, "10.224.81.49,1434");
        }
        public int CheckDataShift(string DotName, string Station)
        {
            int shift = GetShift();
            string _now = DateTime.Now.ToString("yyyy-MM-dd");
            if (DateTime.Now.Hour <= 7)
            {
                _now = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            }
            string sql = $@"select * from PathlossByShift where Dotname='{DotName}'
                            and Station='{Station}' and PCName='{Environment.MachineName}'
                            and Shift={shift}
                            and DateTest ='{_now}'";
            Connect117 conn = new Connect117();
            DataTable dt = conn.DataTable_Sql(sql, "10.224.81.49,1434");
            if (dt.Rows.Count > 0)
            {

                return 1;
            }
            else
            {
                string _sqlInsert = $@"INSERT INTO PathlossByShift (Dotname,Station,PCName,DateTest,Shift,Status)
                                     VALUES('{DotName}','{Station}','{Environment.MachineName}','{_now}',{shift},'Pass')";
                conn.Execute_NonSQL(_sqlInsert, "10.224.81.49,1434");
                return 0;
            }
        }

        public void CopyToAutomationServerDB(string LocalPath, out bool isLock)
        {
            try
            {
                if (!File.Exists("ErrorPathloss.txt"))
                {
                    var fs = File.Create("ErrorPathloss.txt");
                    fs.Close();
                    fs.Dispose();
                }

            }
            catch (Exception)
            {


            }
            bool isModify = false;
            List<ErrorPathloss> lstSumErr = new List<ErrorPathloss>();
            string currentDate = DateTime.Now.ToString("yyyyMMdd_HH_mm_ss");
            try
            {
                string[] files = Directory.GetFiles(LocalPath, "*.csv", SearchOption.TopDirectoryOnly);
                string pattern = @"^path_loss(1?[0-9]?|20)\.csv$";
                
                files = (from x in files
                         where Regex.IsMatch(x.Split('\\').Last(), pattern) == true
                         select x
                         ).ToArray();

                foreach (var item in files)
                {
                    FileInfo fInfo = new FileInfo(item);
                    string dataUpload = ConvertCsvFileToJsonObject(fInfo.FullName);
                    string Modalname = ul.GetModel();
                    string Product = ul.GetProduct();
                    string Station = ul.GetStation();
                    string fileName = fInfo.Name;
                    string pathSave = $@"F:\lsy\ID\PathlossControl\PathLoss\{Product}\{Modalname}\{Environment.MachineName}\{Station}\{currentDate}\{fInfo.Name}";
                    string dateModifyPathlossFile = GetTimeModifyFile(Modalname, Station, Environment.MachineName, fileName, DateTime.Now.ToString("yyyyMMdd"));
                    if (dateModifyPathlossFile.Trim().Length > 0)
                    {
                        if (EqualsUpToSeconds(fInfo.LastWriteTime, DateTime.Parse(dateModifyPathlossFile)))
                        {
                            continue;
                        }
                    }
                    isModify = true;
                    string newData = GetDataNew(Modalname, Station, Environment.MachineName, fileName, DateTime.Now.ToString("yyyyMMdd"));
                    if (!String.IsNullOrEmpty(newData))
                    {
                        List<string> lstErrOL = new List<string>();
                        List<string> lstErrAntena = new List<string>();
                        List<string> lstErrBand = new List<string>();
                        var dataNew = JsonConvert.DeserializeObject<List<DataPathloss>>(dataUpload);
                        var dataOld = JsonConvert.DeserializeObject<List<DataPathloss>>(newData);

                        CompareTwoData(dataOld, dataNew, out lstErrOL);
                        CompareByAntena(dataNew, out lstErrAntena);
                        CompareByBand(dataNew, out lstErrBand);
                        string summury = string.Format("{0,20}{1,20}{2,6}{3,22}", Environment.MachineName, Modalname, Station, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                        try
                        {
                            List<string> errPathLoss = new List<string>();
                            if (lstErrOL.Count > 0)
                            {
                                errPathLoss.Add("Old and new pathloss war error");
                            }
                            if (lstErrAntena.Count > 0)
                            {
                                errPathLoss.Add("Antena pathloss war error");
                            }
                            if (lstErrAntena.Count > 0)
                            {
                                lstErrBand.Add("Band pathloss war error");
                            }
                            if (errPathLoss.Count > 0)
                            {
                                summury += string.Format("{0,7}{1,130}", "FAIL", pathSave);
                                File.WriteAllText("ErrorPathloss.txt", string.Empty);
                                using (TextWriter tw = new StreamWriter("ErrorPathloss.txt"))
                                {
                                   // tw.WriteLine($"Path: {fInfo.FullName}");
                                    foreach (var err in errPathLoss)
                                    {
                                        tw.WriteLine($"{err}");
                                    }
                                    tw.Dispose();
                                    tw.Close();
                                }


                            }
                            else
                            {
                                summury += string.Format("{0,7}{1,130}", "PASS", pathSave);
                                File.WriteAllText("ErrorPathloss.txt", string.Empty);
                            }
                            try
                            {
                                if (!Directory.Exists($@"F:\lsy\ID\PathlossControl\Summary\{DateTime.Now.ToString("yyyy_MM_dd")}"))
                                {
                                    Directory.CreateDirectory($@"F:\lsy\ID\PathlossControl\Summary\{DateTime.Now.ToString("yyyy_MM_dd")}");
                                }
                                if (!File.Exists($@"F:\lsy\ID\PathlossControl\Summary\{DateTime.Now.ToString("yyyy_MM_dd")}\Sumary.txt"))
                                {
                                    var fs = File.Create($@"F:\lsy\ID\PathlossControl\Summary\{DateTime.Now.ToString("yyyy_MM_dd")}\Sumary.txt");
                                    fs.Dispose();
                                    fs.Close();
                                }
                                using (TextWriter tw = new StreamWriter($@"F:\lsy\ID\PathlossControl\Summary\{DateTime.Now.ToString("yyyy_MM_dd")}\Sumary.txt", true))
                                {
                                    tw.WriteLine(summury);
                                    tw.Dispose();
                                    tw.Close();
                                }
                            }
                            catch
                            {


                            }
                            using (TextWriter tw = new StreamWriter("SavedList.txt"))
                            {
                                tw.WriteLine($"Path: {fInfo.FullName}");
                                foreach (var err in errPathLoss)
                                {
                                    tw.WriteLine($"{err}");
                                }
                                tw.Dispose();
                                tw.Close();
                            }


                        }
                        catch (Exception)
                        {


                        }


                        var sumErr = new ErrorPathloss() { FileName = fileName, lstErr = lstErrOL.Concat(lstErrBand).Concat(lstErrAntena).ToList() };
                        lstSumErr.Add(sumErr);
                        if (sumErr.lstErr.Count > 0)
                        {

                            break;
                        }
                        else
                        {
                            UpdateDb(Modalname, Station, Environment.MachineName, dataUpload, fileName, fInfo.LastWriteTime);
                        }
                    }
                    else
                    {
                        UpdateDb(Modalname, Station, Environment.MachineName, dataUpload, fileName, fInfo.LastWriteTime);
                    }
                    if (!Directory.Exists($@"F:\lsy\ID\PathlossControl\PathLoss\{Product}\{Modalname}\{Environment.MachineName}\{Station}\{currentDate}"))
                    {
                        Directory.CreateDirectory($@"F:\lsy\ID\PathlossControl\PathLoss\{Product}\{Modalname}\{Environment.MachineName}\{Station}\{currentDate}");
                    }
                    File.Copy(fInfo.FullName, $@"F:\lsy\ID\PathlossControl\PathLoss\{Product}\{Modalname}\{Environment.MachineName}\{Station}\{currentDate}\{fInfo.Name}", true);

                }
            }
            catch (Exception ex)
            {

                if (!File.Exists("SavedList.txt"))
                {
                    var fs = File.Create("SavedList.txt");
                    fs.Close();
                    fs.Dispose();

                }
                File.WriteAllText("SavedList.txt", string.Empty);
                using (TextWriter tw = new StreamWriter("SavedList.txt"))
                {
                    tw.WriteLine($"ex: {ex.Message}");
                    tw.Close();
                    tw.Dispose();
                }

            }

            if (lstSumErr.Where(x => x.lstErr.Count > 0).ToList().Count > 0)
            {
                isLock = true;
            }
            else
            {
                isLock = false;
            }
            //save to db
            try
            {
                string Modalname = ul.GetModel();
                string Station = ul.GetStation();
                CheckDataShift(Modalname, Station);
                if (lstSumErr.Count > 0)
                {
                    UpdateData(Modalname, Station, "Fail and modify");
                }
                else
                {
                    if (isModify)
                    {
                        UpdateData(Modalname, Station, "Pass and modify");
                    }
                }
            }
            catch 
            { 
            }
            try
            {
                if (!File.Exists("SavedList.txt"))
                {
                    var fs = File.Create("SavedList.txt");
                    fs.Close();
                    fs.Dispose();

                }
                File.WriteAllText("SavedList.txt", string.Empty);
                using (TextWriter tw = new StreamWriter("SavedList.txt"))
                {
                    tw.WriteLine($"Folder: {LocalPath}");
                    foreach (var item in lstSumErr)
                    {
                        foreach (var s in item.lstErr)
                        {
                            tw.WriteLine($"File: {item.FileName}, Err = {s}");

                        }
                    }
                    if (lstSumErr.Count < 1)
                    {
                        tw.WriteLine($"==> Pass");
                    }
                    else
                    {
                        tw.WriteLine($"==> Fail");
                    }
                    tw.Close();
                    tw.Dispose();

                }
                string Modalname = ul.GetModel();
                string Product = ul.GetProduct();
                string Station = ul.GetStation();

                if (!Directory.Exists($@"F:\lsy\ID\PathlossControl\Log\{Product}\{Modalname}\{Environment.MachineName}\{Station}\{currentDate}"))
                {
                    Directory.CreateDirectory($@"F:\lsy\ID\PathlossControl\Log\{Product}\{Modalname}\{Environment.MachineName}\{Station}\{currentDate}");
                }
                File.Copy("SavedList.txt", $@"F:\lsy\ID\PathlossControl\Log\{Product}\{Modalname}\{Environment.MachineName}\{Station}\{currentDate}\SavedList.txt", true);

            }


            catch (Exception ex)
            {

                using (TextWriter tw = new StreamWriter("SavedList.txt"))
                {
                    tw.WriteLine($"ex: {ex.Message}");
                    tw.Close();
                    tw.Dispose();
                }
            }




        }
    }
}
