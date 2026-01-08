using System;
using System.Data.SQLite; // Avem nevoie de SQLite
using System.Security.Cryptography; // Pentru hash-uirea parolelor
using System.Text;
using EncryptX.DataAccess; // Ca sa vedem DatabaseHelper

namespace EncryptX.Services
{
    public class AuthService
    {
        // 1. INREGISTRARE (Register)
        public bool RegisterUser(string username, string password)
        {
            using (var connection = new SQLiteConnection(DatabaseHelper.ConnectionString))
            {
                connection.Open();

                // Verificam intai daca userul exista deja
                string checkUserQuery = "SELECT COUNT(*) FROM Users WHERE Username = @u";
                using (var checkCmd = new SQLiteCommand(checkUserQuery, connection))
                {
                    checkCmd.Parameters.AddWithValue("@u", username);
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (count > 0) return false; // Userul exista deja
                }

                // Daca nu exista, il adaugam
                string insertQuery = "INSERT INTO Users (Username, PasswordHash) VALUES (@u, @p)";
                using (var insertCmd = new SQLiteCommand(insertQuery, connection))
                {
                    insertCmd.Parameters.AddWithValue("@u", username);
                    // Atentie: Nu salvam parola in clar! O hash-uim.
                    insertCmd.Parameters.AddWithValue("@p", HashPassword(password));
                    insertCmd.ExecuteNonQuery();
                }
                return true;
            }
        }

        // 2. AUTENTIFICARE (Login)
        public bool LoginUser(string username, string password)
        {
            using (var connection = new SQLiteConnection(DatabaseHelper.ConnectionString))
            {
                connection.Open();
                string query = "SELECT PasswordHash FROM Users WHERE Username = @u";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@u", username);
                    var result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        string dbHash = result.ToString();
                        // Verificam daca parola introdusa are acelasi hash cu cel din baza de date
                        if (dbHash == HashPassword(password))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        // Functie ajutatoare pentru securitate (Hash SHA256)
        private string HashPassword(string rawPassword)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawPassword));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}