using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace AutoTrack.Database
{
    public class DatabaseHelper
    {
        private static readonly string ConnectionString =
            "Server=(localdb)\\MSSQLLocalDB;" +
            "Database=AutoTrackDB;" +
            "Integrated Security=True;" +
            "TrustServerCertificate=True;";

        public static SqlConnection GetConnection()
        {
            SqlConnection connection = new SqlConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        public static bool TestConnection()
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    return conn.State == ConnectionState.Open;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Database connection test failed: " + ex);
                return false;
            }
        }

        public static int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteNonQuery();
            }
        }

        public static object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (parameters != null)
                {
                    // Create new parameters to avoid reuse issues
                    foreach (SqlParameter param in parameters)
                    {
                        cmd.Parameters.Add(new SqlParameter(param.ParameterName, param.Value));
                    }
                }
                return cmd.ExecuteScalar();
            }
        }

        public static DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (parameters != null)
                {
                    // Create new parameters to avoid reuse issues
                    foreach (SqlParameter param in parameters)
                    {
                        cmd.Parameters.Add(new SqlParameter(param.ParameterName, param.Value));
                    }
                }

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(dt);
                }
            }
            return dt;
        }
    }
}
