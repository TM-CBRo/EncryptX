using System;
using System.Data;
using System.Data.SQLite; // Avem nevoie de SQLite
using EncryptX.DataAccess; // Ca sa vedem DatabaseHelper

namespace EncryptX.Services
{
    public class LogService
    {
        // 1. ADAUGA UN RAND NOU IN ISTORIC
        public void AddLog(string username, string fileName, string action)
        {
            using (var connection = new SQLiteConnection(DatabaseHelper.ConnectionString))
            {
                connection.Open();
                string query = "INSERT INTO History (Username, FileName, ActionType, Timestamp) VALUES (@u, @f, @a, @t)";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@u", username);
                    cmd.Parameters.AddWithValue("@f", fileName);
                    cmd.Parameters.AddWithValue("@a", action);
                    cmd.Parameters.AddWithValue("@t", DateTime.Now);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // 2. CITESTE TOT ISTORICUL (Pentru a-l afisa in tabel)
        public DataTable GetLogs()
        {
            using (var connection = new SQLiteConnection(DatabaseHelper.ConnectionString))
            {
                connection.Open();
                // Luam ultimele 50 de actiuni, ordonate descrescator (cele mai noi sus)
                string query = "SELECT Username, FileName, ActionType, Timestamp FROM History ORDER BY Timestamp DESC LIMIT 50";

                using (var adapter = new SQLiteDataAdapter(query, connection))
                {
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    return table;
                }
            }
        }
    }
}