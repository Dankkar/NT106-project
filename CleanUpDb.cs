using System;
using System.Data.SQLite;
using System.Collections.Generic;

class CleanUpDb
{
    static void Main(string[] args)
    {
        string dbPath = @"D:\NT106_Project\NT106-project\test.db";
        string connectionString = $"Data Source={dbPath};Version=3;";

        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();

            // Lấy danh sách tất cả các bảng (trừ bảng hệ thống)
            var tableNames = new List<string>();
            using (var cmd = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';", connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    tableNames.Add(reader.GetString(0));
                }
            }

            // Xóa dữ liệu trong từng bảng
            foreach (var table in tableNames)
            {
                string deleteSql = $"DELETE FROM [{table}]";
                using (var deleteCmd = new SQLiteCommand(deleteSql, connection))
                {
                    deleteCmd.ExecuteNonQuery();
                }
                Console.WriteLine($"Đã xóa dữ liệu trong bảng: {table}");
            }

            Console.WriteLine("Hoàn thành làm sạch database!");
        }
    }
} 