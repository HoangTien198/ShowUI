using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ShowUI.AutomationHelper
{
	public class Connect117
	{
		public SqlConnection open(string serverIp)
		{
			string stringConn = GetStringConn(serverIp);
			return new SqlConnection(stringConn);
			//MessageBox.Show("Connection Ok");
		}
		public string GetStringConn(string serverIp)
		{

			//MessageBox.Show(svIp);
			string stringConn = @"Data Source=" + serverIp + ";Initial Catalog=PathLoss;uid=sa;pwd=********;";
			return stringConn;
		}
		public int Execute_NonSQL(string sql, string serverIp)
		{
			try
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
			catch (Exception ex)
			{
				return 0;
			}
		}
		public dynamic ExecuteAndGetData(string sql, string serverIp)
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
		public int Execute_ProcRandDom(string proc, string delayTime, string serverIp, params double[] param)
		{
			string stringConn = GetStringConn(serverIp);
			SqlConnection conn = new SqlConnection(stringConn);
			//string sql = $@"declare @return_value int
			//               exec @return_value = [dbo].[RandDom]

			//               @TYR ={param[0]},
			//               @YRlow={param[1]},
			//               @YRhight={param[2]},
			//               @projID={param[3]},
			//               @TRR={param[4]},
			//               @TRlow={param[5]},
			//               @TRhight={param[6]}

			//               select 'Return value'= @return_value
			//               ";
			SqlCommand cmd = new SqlCommand(proc, conn);
			int row = 0;
			conn.Open();

			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = proc;
			cmd.Parameters.Add(new SqlParameter("@delay", delayTime));
			cmd.Parameters.Add(new SqlParameter("@TYR", param[0]));
			cmd.Parameters.Add(new SqlParameter("@YRlow", param[1]));
			cmd.Parameters.Add(new SqlParameter("@YRhight", param[2]));
			cmd.Parameters.Add(new SqlParameter("@TRR", param[3]));
			cmd.Parameters.Add(new SqlParameter("@TRlow", param[4]));
			cmd.Parameters.Add(new SqlParameter("@TRhight", param[5]));
			cmd.Parameters.Add(new SqlParameter("@projID", param[6]));
			cmd.ExecuteNonQuery();



			conn.Dispose();
			conn.Close();
			return row;
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
		public int CreateOrUpdateDB(string check, dynamic valueCheck, string table, string serverIp, params string[] check2)
		{
			string sql = $"select top 1 * from {table} where {check}='{valueCheck}'";
			if (check2.Length > 1)
			{
				sql = $"select top 1 * from {table} where {check}='{valueCheck}' and {check2[0]} ='{check2[1]}'";
			}
			var row = DataTable_Sql(sql, serverIp);
			if (row.Rows.Count == 0)
			{
				return 0;
			}
			else
			{
				return Int32.Parse(row.Rows[row.Rows.Count - 1][0].ToString());
			}

		}
		public int CreateAndGetID(string serverIp, string sql, string table, params string[] check)
		{
			Execute_NonSQL(sql, serverIp);
			if (check.Length > 0)
			{
				sql = $"select top 1 * from {table} where {check[0]} = '{check[1]}'";
			}
			else
			{
				sql = $"select * from {table}";
			}
			var row = DataTable_Sql(sql, serverIp);
			if (row.Rows.Count > 0)
			{
				return Int32.Parse(row.Rows[row.Rows.Count - 1][0].ToString());
			}
			else
			{
				return 0;
			}

		}
		public string checkExit(string serverIp, string modal, string station, string pcName,string dataPathLoss,string fileName)
		{

			string stringConn = GetStringConn(serverIp);
			SqlConnection conn = new SqlConnection(stringConn);
			conn.Open();
			try
			{
				SqlCommand cmd = new SqlCommand("CreateOrUpdateProc", conn);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add(new SqlParameter("@dot_name", modal));
				cmd.Parameters.Add(new SqlParameter("@station", station));
				cmd.Parameters.Add(new SqlParameter("@pc_Name", pcName));
				cmd.Parameters.Add(new SqlParameter("@data_pathloss", dataPathLoss));
				cmd.Parameters.Add(new SqlParameter("@file_name", fileName));
				//cmd.Parameters.Add(new SqlParameter("@CreateOrUpdate",ParameterDirection.Output));
				cmd.Parameters.Add("@CreateOrUpdate",SqlDbType.NVarChar,20).Direction = ParameterDirection.Output;
				cmd.ExecuteNonQuery();
				conn.Dispose();
				conn.Close();
				string CreateOrUpdate = cmd.Parameters["@CreateOrUpdate"].Value.ToString();
				return CreateOrUpdate;
			}
			catch (Exception ex)
			{
				conn.Dispose();
				conn.Close();
				return "ERR";
			}
		}

    }
}
