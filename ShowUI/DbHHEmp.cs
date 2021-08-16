using System;
using System.Data;
using System.Data.SqlClient;

namespace ShowUI
{
    public class DbHHEmp
    {
        public string GetStringConn(string serverIp)
        {
            //MessageBox.Show(svIp);
            string stringConn = @"Data Source=" + serverIp + ";Initial Catalog=HHEmployee;uid=sa;pwd=********;";
            return stringConn;
        }

        public DataTable DataTable_Sql(string sql, string serverIp)
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

        public int Execute_NonSQL(string sql, string serverIp)
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
    }
}