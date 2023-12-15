using Newtonsoft.Json;
using ShowUI.Common;
using System.Data;
using ShowUIApp;

namespace ShowUI
{
    public class CheckNTGR
    {
        private ShowUI.Utilities ul = new ShowUI.Utilities();

        string serverIp = "";
        public int CheckModel()
        {
            try
            {
                serverIp = IniFile.ReadIniFile("DATABASE", "SERVER_NAME", "10.220.130.103,1734", @"F:\lsy\Test\DownloadConfig\AutoDL\SOURCE.ini");
                string ModelName = ul.GetProduct();
                string checkNetgearSql = $"select * from ProjectName where ProjectName ='{ModelName}'";
                ConnectShowUI connCheckNt = new ConnectShowUI();
                DataTable checkNt = connCheckNt.DataTable_Sql(checkNetgearSql, serverIp);
                if (checkNt.Rows.Count > 0)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        public int CheckLock()
        {
            try
            {
                serverIp = IniFile.ReadIniFile("DATABASE", "SERVER_NAME", "10.220.130.103,1734", @"F:\lsy\Test\DownloadConfig\AutoDL\SOURCE.ini");
                string ModelName = ul.GetProduct();
                string Station = ul.GetStation();
                string checkLockSql = $"select * from LockShowUITE where Model ='{ModelName}' and Station='{Station}' and Lock = 1";
                ConnectShowUI connCheckNt = new ConnectShowUI();
                DataTable checkNt = connCheckNt.DataTable_Sql(checkLockSql, serverIp);
                if (checkNt.Rows.Count > 0)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        public bool CheckLockSP()
        {
            try
            {
                serverIp = IniFile.ReadIniFile("DATABASE", "SERVER_NAME", "10.220.130.103,1734", @"F:\lsy\Test\DownloadConfig\AutoDL\SOURCE.ini");
                string ModelName = ul.GetProduct();
                string Station = ul.GetStation();
                string checkLockSql = $"select * from LockShowUITE where Model ='{ModelName}' and Station='{Station}' and LockSampling = 1 ";
                ConnectShowUI connCheckNt = new ConnectShowUI();
                DataTable checkNt = connCheckNt.DataTable_Sql(checkLockSql, serverIp);
                if (checkNt.Rows.Count > 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch
            {
                return true;
            }
        }

        public int GetTimeLock()
        {
            try
            {
                serverIp = IniFile.ReadIniFile("DATABASE", "SERVER_NAME", "10.220.130.103,1734", @"F:\lsy\Test\DownloadConfig\AutoDL\SOURCE.ini");
                string ModelName = ul.GetProduct();
                string Station = ul.GetStation();
                string checkLockSql = $"select top 1 TimeLock from LockShowUITE where Model ='{ModelName}' and Station='{Station}' and Lock = 1";
                ConnectShowUI connCheckNt = new ConnectShowUI();
                DataTable checkNt = connCheckNt.DataTable_Sql(checkLockSql, serverIp);

                if (checkNt.Rows.Count > 0)
                {
                    var time = int.Parse(checkNt.Rows[0][0].ToString());
                    return time;
                }
                else
                {
                    return -1;
                }
            }
            catch
            {
                return -1;
            }
        }

        public ConfigLockModal getDataLock()
        {
            try
            {
                serverIp = IniFile.ReadIniFile("DATABASE", "SERVER_NAME", "10.220.130.103,1734", @"F:\lsy\Test\DownloadConfig\AutoDL\SOURCE.ini");
                string ModelName = ul.GetProduct();
                string Station = ul.GetStation();
                string checkLockSql = $"select top 1 ConfigLock from LockShowUITE where Model ='{ModelName}' and Station='{Station}' and Lock = 1";
                ConnectShowUI connCheckNt = new ConnectShowUI();
                DataTable checkNt = connCheckNt.DataTable_Sql(checkLockSql, serverIp);
                if (checkNt.Rows.Count > 0)
                {
                    var data = JsonConvert.DeserializeObject<ConfigLockModal>(checkNt.Rows[0][0].ToString());
                    return data;
                }
                else
                {
                    return null;// new ConfigLockModal();
                }
            }
            catch
            {
                return null;// new ConfigLockModal();
            }
        }
    }
}