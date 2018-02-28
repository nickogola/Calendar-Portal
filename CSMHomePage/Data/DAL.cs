using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace CSMHomePage.Data
{
    public class DAL
    {
        //string connection = System.Configuration.ConfigurationManager.ConnectionStrings["csm_cdw_db_ConnectionString"].ConnectionString;
        public string Connection { get; set; }

        public DAL()
        {

        }

        public DAL(string connectionString)
        {
            this.Connection = connectionString;
        }

        SqlConnection con;

        public DataSet execute_sp_dataset_with_param(string procname, SqlParameter[] param)
        {
            try
            {
                using (SqlConnection con = new SqlConnection())
                {
                    con.ConnectionString = Connection;
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = procname;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;
                    cmd.Parameters.AddRange(param);//.Add(pr);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    con.Open();
                    da.Fill(ds);

                    if (ds != null)
                    {
                        return ds;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        public DataSet execute_sp_dataset_without_param(string procname)
        {
            try
            {
                using (SqlConnection con = new SqlConnection())
                {
                    con.ConnectionString = Connection;
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = procname;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;
                 //   cmd.Parameters.AddRange(param);//.Add(pr);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    con.Open();
                    da.Fill(ds);

                    if (ds != null)
                    {
                        return ds;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        public void execute_sp_saving_with_param(string procname, SqlParameter[] param)
        {
            try
            {
                using (SqlConnection con = new SqlConnection())
                {
                    con.ConnectionString = Connection;
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = con;
                    con.Open();
                    cmd.CommandText = procname;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;
                    foreach (SqlParameter pr in param)
                    {
                        cmd.Parameters.Add(pr);
                    }
                    cmd.ExecuteNonQuery();
                }


            }
            catch (Exception ex)
            {
                //return 1;
            }

        }

        public DataSet SelectPolicyList(string procname, SqlParameter[] param)
        {
            DataSet ds = new DataSet();
            ds = execute_sp_dataset_with_param(procname, param);
            return ds;
        }
        public DataSet GetDataNoParams(string procname)
        {
            DataSet ds = new DataSet();
            ds = execute_sp_dataset_without_param(procname);
            return ds;
        }
        public DataTable ExecuteScalar(string procname, SqlParameter[] param)
        {
            DataSet ds = new DataSet();
            ds = execute_sp_dataset_with_param(procname, param);
            return ds.Tables[0];
        }

        public void SaveItems(string procname, SqlParameter[] param)
        {
            //int i=0;
            execute_sp_saving_with_param(procname, param);
            //return i;
        }

    }
}