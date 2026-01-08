using System;
using System.IO;
using System.Data.SQLite; // Aici folosim pachetul instalat

namespace EncryptX.DataAccess
{
    public static class DatabaseHelper
    {
        // Numele fisierului de baza de date
        private const string DbName = "EncryptX.db";

        // String-ul de conectare (Ii spune aplicatiei unde e fisierul)
        public static string ConnectionString = $"Data Source={DbName};Version=3;";

        // Aceasta functie va fi apelata cand porneste aplicatia
        public static void InitializeDatabase()
        {
            // Verificam daca fisierul exista. Daca nu, il va crea SQLite automat la conectare.
            if (!File.Exists(DbName))
            {
                SQLiteConnection.CreateFile(DbName);
                Console.WriteLine("Fisierul bazei de date a fost creat!");
            }

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                // 1. Tabelul pentru Utilizatori
                string tableUsers = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Username TEXT UNIQUE NOT NULL,
                        PasswordHash TEXT NOT NULL
                    );";

                // 2. Tabelul pentru Istoric (Log-uri)
                string tableHistory = @"
                    CREATE TABLE IF NOT EXISTS History (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Username TEXT NOT NULL,
                        FileName TEXT,
                        ActionType TEXT NOT NULL, 
                        Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
                    );";

                // Executam comenzile SQL
                using (var command = new SQLiteCommand(tableUsers, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SQLiteCommand(tableHistory, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}