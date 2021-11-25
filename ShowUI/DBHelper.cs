using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ShowUI
{
    public class DBHelper
    {
        public SqlConnection open(string serverIp, string DbName)
        {
            string stringConn = GetStringConn(serverIp, DbName);
            return new SqlConnection(stringConn);
            //MessageBox.Show("Connection Ok");
        }

        public string GetStringConn(string serverIp, string DbName)
        {
            //MessageBox.Show(svIp);
            string stringConn = @"Data Source=" + serverIp + ";Initial Catalog=" + DbName + ";uid=sa;pwd=********;";
            return stringConn;
        }

        public int Execute_NonSQL(string sql, string serverIp, string DbName)
        {
            try
            {
                string stringConn = GetStringConn(serverIp, DbName);
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
            catch
            {
                return 0;
            }
        }

        public DataTable DataTable_Sql(string sql, string serverIp, string DbName)
        {
            try
            {
                string stringConn = GetStringConn(serverIp, DbName);
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
    }
}