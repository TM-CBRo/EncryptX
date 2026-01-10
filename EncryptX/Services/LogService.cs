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
        public DataTable GetLogs(string username, bool isAdmin)
        {
            using (var conn = new SQLiteConnection(DatabaseHelper.ConnectionString))
            {
                conn.Open();
                string query;
                SQLiteCommand cmd = new SQLiteCommand(conn);

                // LOGICA DE FILTRARE
                if (isAdmin)
                {
                    // Daca e admin, luam TOT, de la cel mai recent la cel mai vechi
                    query = "SELECT Username, FileName, ActionType, Timestamp FROM History ORDER BY Timestamp DESC";
                }
                else
                {
                    // Daca e user simplu, luam DOAR randurile lui
                    query = "SELECT Username, FileName, ActionType, Timestamp FROM History WHERE Username = @user ORDER BY Timestamp DESC";
                    cmd.Parameters.AddWithValue("@user", username);
                }

                cmd.CommandText = query;

                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }

        // 3. STERGE USER SI ISTORIC (Metoda Noua)
        public void DeleteUserAndHistory(string usernameToDelete)
        {
            using (var conn = new SQLiteConnection(DatabaseHelper.ConnectionString))
            {
                conn.Open();

                // Folosim o tranzactie ca sa fim siguri ca se sterg AMBELE sau niciuna
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // A. Stergem din tabelul de Useri
                        string cmdUsers = "DELETE FROM Users WHERE Username = @user";
                        using (var cmd1 = new SQLiteCommand(cmdUsers, conn))
                        {
                            cmd1.Parameters.AddWithValue("@user", usernameToDelete);
                            cmd1.Transaction = transaction;
                            cmd1.ExecuteNonQuery();
                        }

                        // B. Stergem din tabelul de Istoric
                        string cmdHistory = "DELETE FROM History WHERE Username = @user";
                        using (var cmd2 = new SQLiteCommand(cmdHistory, conn))
                        {
                            cmd2.Parameters.AddWithValue("@user", usernameToDelete);
                            cmd2.Transaction = transaction;
                            cmd2.ExecuteNonQuery();
                        }

                        // Confirmam modificarile
                        transaction.Commit();
                    }
                    catch
                    {
                        // Daca apare o eroare, anulam tot (nu stergem nimic)
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}