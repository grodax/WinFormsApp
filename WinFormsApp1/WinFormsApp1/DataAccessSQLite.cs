using System;
using System.Data;
using System.Data.SQLite;

public class DataAccessSQLite
{
    public DataTable ExecuteQuery(string sql, SQLiteParameter[] parameters = null)
    {
        DataTable dt = new DataTable();

        using (var connection = DatabaseHelper.GetConnection())
        {
            connection.Open();

            using (var command = new SQLiteCommand(sql, connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                using (var adapter = new SQLiteDataAdapter(command))
                {
                    adapter.Fill(dt);
                }
            }
        }

        return dt;
    }

    public int ExecuteNonQuery(string sql, SQLiteParameter[] parameters = null)
    {
        using (var connection = DatabaseHelper.GetConnection())
        {
            connection.Open();

            using (var command = new SQLiteCommand(sql, connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                return command.ExecuteNonQuery();
            }
        }
    }

    public object ExecuteScalar(string sql, SQLiteParameter[] parameters = null)
    {
        using (var connection = DatabaseHelper.GetConnection())
        {
            connection.Open();

            using (var command = new SQLiteCommand(sql, connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                return command.ExecuteScalar();
            }
        }
    }
}