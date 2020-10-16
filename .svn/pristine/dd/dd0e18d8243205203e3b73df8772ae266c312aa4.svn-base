using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Globalization;
using System.Text;
using ShowUI;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
namespace Fii
{
    class FiiData
    {
        public static int InsertFii(string lock_detail, string wip_qty, string yield_rate)
        {
            SQLConnect conn = new SQLConnect();
            string openkey = "";
            string model = "", station = "",err_loop="", first_fail = "0", repair_qty = "0", pass_qty = "0", shift = "", err_code = "", date_time = "", lock_status = "LOCK", why = lock_detail, lock_history = "";
            string[] lockA = { "YRate NPI", "Lỗi: Có lỗi nghiêm trọng", "Lỗi: Phát hiện mã lỗi", "R_OVERSPEC", "Error 3in10!", "RTRate", "Lower Than", "3_Continuous DUT Error", "4_Continuous DUT Error ABC12" };
            //ArrayList arr = new ArrayList();
            //arr.AddRange(lockA);
            //lock_history = (arr.Contains(lock_detail)) ? "LOCKA" : "LOCKB";
            bool check_lock = false;
            for (int i = 0; i < lockA.Length; i++)
            {
                string locka = lockA[i];
                if (lock_detail.Contains(locka))
                {
                    check_lock = true;
                    break;
                }
            }
            lock_history = (check_lock) ? "LOCKA" : "LOCKB";
            
           
            double d = 1.2;
            int n = 0;
            if (yield_rate.Contains("%"))
            {
                yield_rate = yield_rate.Replace("%", "");
            }
            
            int wip_ = 0; double yr_ = 0; double pass_ = 0; double fail_ = 0;
            if (int.TryParse(wip_qty, out n) & double.TryParse(yield_rate, out d))
            {
                wip_ = int.Parse(wip_qty);
                yr_ = double.Parse(yield_rate);
                pass_ = Math.Round((yr_ / 100) * wip_);
                fail_ = wip_ - pass_;

                pass_qty = pass_.ToString();
                first_fail = fail_.ToString();
            }
            if (!int.TryParse(wip_qty, out n))
            {
                wip_qty = "0";
            }
            
            date_time = DateTime.Now.ToString("dd/MM/yyyy");
            string hms = DateTime.Now.ToString("HH:mm:ss");
            string date_time_lock = date_time + " " + hms;
            string ate = Environment.MachineName;

            string subkey = @"SOFTWARE\Netgear\STATION";
            RegistryKey key;
            key = Registry.LocalMachine.OpenSubKey(subkey);
            if (key != null)
            {
                openkey = (string)key.GetValue("OpenKey");
                string[] temp = openkey.Split('\\');
                int leng = temp.Length;
                station = temp[leng - 1];
            }
            subkey += openkey;
            key = Registry.LocalMachine.OpenSubKey(subkey);
            if (key != null)
            {
                model = (string)key.GetValue("SFISMODEL");
                //pass_qty = "'" + (string)key.GetValue("PASS") + "'";
                //if (pass_qty=="''")
                //{
                //    pass_qty = "NULL";
                //}
                err_code = (string)key.GetValue("ERRORCODE");  
            }
            if (lock_detail.Contains("3_Continuous"))
            {
                err_loop = err_code + ":3";
            }
            if (lock_detail.Contains("4_Continuous"))
            {
                err_loop = err_code + ":4";
            }
            DateTime date = DateTime.ParseExact(hms, "HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime t1 = DateTime.ParseExact("07:30:00", "HH:mm:ss", CultureInfo.InvariantCulture), t2 = DateTime.ParseExact("19:30:00", "HH:mm:ss", CultureInfo.InvariantCulture);
            shift = (date > t1 & date < t2) ? "DAY" : "NIGHT";

            string sql = "SELECT MAX(ID) as ID FROM SPREADSHEETDATA";
            int id;
            if (conn.getDataTable(sql).Rows[0]["ID"].ToString() != "")
            {
                string idd = conn.getDataTable(sql).Rows[0]["ID"].ToString();
                id = int.Parse(idd);
                id += 1;
            }
            else id = 1;
            string sql_check = "SELECT*FROM SPREADSHEETDATA WHERE MODEL_NAME='"+model+"' AND STATION_NAME='"+station+"' AND ATE_NAME='"+ate+"' AND LOCK_STATUS='"+lock_status+"' AND LOCK_DETAIL='"+lock_detail+"' AND ERROR_CODE='"+err_code+"' AND LOCK_HISTORY='"+lock_history+"' AND DATE_TIME='"+date_time+"' ";
            if (conn.getDataTable(sql_check).Rows.Count==0)
            {
                sql = "INSERT INTO SPREADSHEETDATA(MODEL_NAME,STATION_NAME,ATE_NAME,ERROR_LOOP,WIP_QTY,FIRST_FAIL,REPAIR_QTY,PASS_QTY,WHY,LOCK_STATUS,LOCK_HISTORY,DATE_TIME_LOCK,DATE_TIME,SHIFT,LOCK_DETAIL,ERROR_CODE,ID) ";
                sql += "VALUES('" + model + "','" + station + "','" + ate + "','" + err_loop + "','" + wip_qty + "','" + first_fail + "','" + repair_qty + "','" + pass_qty + "','" + why + "','" + lock_status + "','" + lock_history + "',CONVERT(DATETIME,'" + date_time_lock + "',103),CONVERT(DATETIME,'" + date_time + "',103),'" + shift + "','" + lock_detail + "','" + err_code + "','" + id + "') ";
                conn.ExecuteSQL(sql);
            }
            
            return id;
        }
        public static void UpdateFii(int id, string action, string owner)
        {
            action = "'"+action+"'";
            SQLConnect conn = new SQLConnect();
            string lock_status = "UNLOCK";
            string date_unlock = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss tt");
            string sql = "UPDATE SPREADSHEETDATA SET ACTION=" + action + ",OWNER='" + owner + "',LOCK_STATUS='" + lock_status + "',DATE_TIME_UNLOCK=CONVERT(DATETIME,'" + date_unlock + "',22) WHERE ID='" + id + "'";
            conn.ExecuteSQL(sql);
        }
        public static void QRCode(PictureBox ptb)
        {
            var barcodeWriter = new BarcodeWriter();
            QrCodeEncodingOptions qr = new QrCodeEncodingOptions()
            {
                CharacterSet = "UTF-8",
                Height = 200,
                Width = 200,
                Margin = 0
            };
            barcodeWriter.Options = qr;
            barcodeWriter.Format = BarcodeFormat.QR_CODE;


            string content = Environment.MachineName;
            using (var bitmap = barcodeWriter.Write(content))
            {
                using (var stream = new MemoryStream())
                {
                    bitmap.Save(stream, ImageFormat.Png);
                    var image = Image.FromStream(stream);
                    ptb.Image = image;
                }
            }
        }
    }

    class SQLConnect
    {
        public static string StringConnect = "Server=10.224.81.92;Uid=B05FII;Pwd=Foxconn168#;Database=B05_FII;";
        System.Data.SqlClient.SqlConnection connect;
        System.Data.SqlClient.SqlDataAdapter sda;
        System.Data.SqlClient.SqlCommand cmd;
        public void OpenConnect()
        {
            //connect = new OracleConnection(StringConnect);
            connect = new System.Data.SqlClient.SqlConnection(StringConnect);
            if (connect.State != System.Data.ConnectionState.Open)
            {
                try
                {
                    connect.Open();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }
        public void CloseConnect()
        {
            if (connect.State != System.Data.ConnectionState.Closed)
            {
                try
                {
                    connect.Close();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }
        public System.Data.SqlClient.SqlConnection GetConn()
        {
            connect = new System.Data.SqlClient.SqlConnection(StringConnect);
            OpenConnect();
            return connect;
        }
        public void ExecuteSQL(string sql)
        {
            cmd = new System.Data.SqlClient.SqlCommand();
            sda = new System.Data.SqlClient.SqlDataAdapter();
            cmd.CommandText = sql;
            cmd.Connection = GetConn();
            //   transaction = connect.BeginTransaction();
            cmd.CommandType = CommandType.Text;
            try
            {
                cmd.ExecuteNonQuery();
                //    transaction.Commit();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
                //   transaction.Rollback();
            }
            finally
            {
                cmd.Dispose();
                CloseConnect();
            }
        }
        public DataTable getDataTable(string sql)
        {
            try
            {
                GetConn();
                System.Data.SqlClient.SqlDataAdapter dap = new System.Data.SqlClient.SqlDataAdapter(sql, connect);
                DataSet ds = new DataSet();
                dap.Fill(ds);
                connect.Dispose();
                connect.Close();
                return ds.Tables[0];

            }
            catch (Exception)
            {
                //throw new Exception(ex.Message);
                return new DataTable();
            }

        }
    }
    class OracleConnect
    {
        public OracleConnect()
        {
            //
            // TODO: Add constructor logic here
            //
        }
        public bool Connected = false;
        OracleConnection connection = null;
        OracleCommand command = null;
        public OracleConnection GetConnection()
        {

            string DBName = "User ID=B05FII;Pwd=Foxconn168#;data source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=10.224.81.92)(PORT=1521))(CONNECT_DATA=(SID=orclb05me)));unicode=true";
            OracleConnection myConn = new OracleConnection(DBName);
            return myConn;
        }
        public void sqlEx(string StrSQL)
        {
            OracleConnection con = GetConnection();
            con.Open();
            OracleCommand cmd = new OracleCommand(StrSQL, con);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            con.Close();
        }

        public DataTable reDt(string StrSQL)
        {
            OracleConnection con = GetConnection();
            con.Open();
            OracleDataAdapter da = new OracleDataAdapter(StrSQL, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            con.Close();
            return (ds.Tables[0]);
        }
    }

}
