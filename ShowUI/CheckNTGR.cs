using Newtonsoft.Json;
using ShowUI.Common;
using System.Data;

namespace ShowUI
{
    public class CheckNTGR
    {
        private ShowUI.Utilities ul = new ShowUI.Utilities();

        public int CheckModel()
        {
            try
            {
                string ModelName = ul.GetProduct();
                string checkNetgearSql = $"select * from ProjectName where ProjectName ='{ModelName}'";
                ConnectShowUI connCheckNt = new ConnectShowUI();
                DataTable checkNt = connCheckNt.DataTable_Sql(checkNetgearSql, "10.224.81.162,1734");
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
                string ModelName = ul.GetProduct();
                string Station = ul.GetStation();
                string checkLockSql = $"select * from LockShowUITE where Model ='{ModelName}' and Station='{Station}' and Lock = 1";
                ConnectShowUI connCheckNt = new ConnectShowUI();
                DataTable checkNt = connCheckNt.DataTable_Sql(checkLockSql, "10.224.81.162,1734");
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
                string ModelName = ul.GetProduct();
                string Station = ul.GetStation();
                string checkLockSql = $"select * from LockShowUITE where Model ='{ModelName}' and Station='{Station}' and LockSampling = 1 ";
                ConnectShowUI connCheckNt = new ConnectShowUI();
                DataTable checkNt = connCheckNt.DataTable_Sql(checkLockSql, "10.224.81.162,1734");
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
                string ModelName = ul.GetProduct();
                string Station = ul.GetStation();
                string checkLockSql = $"select top 1 TimeLock from LockShowUITE where Model ='{ModelName}' and Station='{Station}' and Lock = 1";
                ConnectShowUI connCheckNt = new ConnectShowUI();
                DataTable checkNt = connCheckNt.DataTable_Sql(checkLockSql, "10.224.81.162,1734");

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
                string ModelName = ul.GetProduct();
                string Station = ul.GetStation();
                string checkLockSql = $"select top 1 ConfigLock from LockShowUITE where Model ='{ModelName}' and Station='{Station}' and Lock = 1";
                ConnectShowUI connCheckNt = new ConnectShowUI();
                DataTable checkNt = connCheckNt.DataTable_Sql(checkLockSql, "10.224.81.162,1734");
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