using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Forms;
using System.IO;

namespace ShowUIApp
{
    class ToDB
    {
        

        //public static string stringConn = "Data Source=(local)\\SQLEXPRESS;Initial Catalog=QLNS;Integrated Security=true";


        public SqlConnection open(string serverIp)
        {
            string stringConn = GetStringConn(serverIp);
            return new SqlConnection(stringConn);
            //MessageBox.Show("Connection Ok");
        }

        public string GetStringConn(string serverIp)
        {
           
            //MessageBox.Show(svIp);
            string stringConn = @"Data Source="+serverIp+";Initial Catalog=dbMO;uid=sa;pwd=********;Connection Timeout=5";
            return stringConn;
        }
        // select query

        // DataTable dt = new DataTable();

        public DataTable DataTable_Sql(string sql,string serverIp)
        {
            try
            {
                string stringConn = GetStringConn(serverIp);
                using (SqlConnection conn = new SqlConnection(stringConn))
                {
                    using (SqlDataAdapter dap = new SqlDataAdapter(sql, conn))
                    {
                        using (DataSet ds = new DataSet())
                        {
                            dap.Fill(ds);
                            conn.Close();
                            conn.Dispose();
                            return ds.Tables[0];
                        }
                    }
                }
            }
            catch (Exception)
            {

                // throw new Exception(ex.Message);
            }
            return new DataTable("NULL");
        }

        /**
         * ChuongNguyenVan
         * edit data( insert , update, delete)
         */

        public int Execute_NonSQL(string sql,string serverIp)
        {
            string stringConn = GetStringConn(serverIp);
            SqlConnection conn = new SqlConnection(stringConn);
            SqlCommand cmd = new SqlCommand(sql, conn);
            int row = 0;
            conn.Open();

            cmd.CommandType = CommandType.Text;
            cmd.CommandText = sql;
            row = cmd.ExecuteNonQuery();
            conn.Dispose();
            conn.Close();
            return row;
        }

        /**
         * ChuongNguyenVan
         * EXECUTE QUERY ( procedures)
         */
        public int Execute_SQL(string serverIp,string query_object, CommandType type, params object[] obj)
        {
            string stringConn = GetStringConn(serverIp);
            int row = 0;
            SqlConnection conn = new SqlConnection(stringConn);
            conn.Open();

            SqlCommand cmd = new SqlCommand(query_object, conn);
            cmd.CommandType = type;

            SqlCommandBuilder.DeriveParameters(cmd);
            for (int i = 1; i <= obj.Length; i++)
            {
                cmd.Parameters[i].Value = obj[i - 1];
            }
            cmd.ExecuteNonQuery();

            conn.Dispose();
            conn.Close();
            return row;
        }

        /**
         * ChuongNguyenVan
         * Check exists record
         */
        //public static bool CheckExist(string sql)
        //{
        //    DataTable dtb = new DataTable();
        //    dtb = DataTable_Sql(sql);
        //    if (dtb.Rows.Count > 0) return true;
        //    return false;
        //}
        public SqlDataReader ThucHienReader(string sql, string serverIp)
        {
          
            string stringConn = GetStringConn(serverIp);
            SqlConnection conn = new SqlConnection(stringConn);
            SqlCommand cmd = new SqlCommand(sql, conn);
            conn.Open();
            try
            {
                conn.Close();
                return cmd.ExecuteReader();
                
            }
            catch (SqlException)
            {
                conn.Close();
                return null;
            }
            

        }
        public DataSet getDataset(string select, string serverIp)
        {
            string stringConn = GetStringConn(serverIp);
            SqlConnection conn = new SqlConnection(stringConn);
            SqlDataAdapter da = new SqlDataAdapter(select, conn);
            DataSet ds = new DataSet();
            da.Fill(ds);
            return ds;
        }
    }
}
