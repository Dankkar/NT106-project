using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace FileSharingServer
{
    public static class DatabaseHelper
    {
        public static string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName;
        private static string dbPath = GetDatabasePath();
        public static readonly string connectionString = $"Data Source={dbPath};Version=3;";

        private static string GetDatabasePath()
        {
            // Ensure all server instances use the same database
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            
            // Navigate up from bin\Debug\ to project root
            DirectoryInfo current = new DirectoryInfo(baseDir);
            while (current != null && !File.Exists(Path.Combine(current.FullName, "test.db")))
            {
                current = current.Parent;
                if (current?.Parent?.Parent == null) break; // Safety check
            }
            
            if (current != null && File.Exists(Path.Combine(current.FullName, "test.db")))
            {
                return Path.Combine(current.FullName, "test.db");
            }
            
            // Fallback: use original logic
            string fallbackRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName;
            return Path.Combine(fallbackRoot ?? Environment.CurrentDirectory, "test.db");
        }

        public static async Task InitializeDatabaseAsync()
        {
            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
            }

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                await conn.OpenAsync();
                
                // Create users table
                string createUsersTable = @"
                CREATE TABLE IF NOT EXISTS users (
                    user_id INTEGER NOT NULL UNIQUE,
                    username TEXT NOT NULL UNIQUE,
                    email TEXT NOT NULL UNIQUE,
                    password_hash TEXT NOT NULL,
                    created_at TEXT NOT NULL DEFAULT (datetime('now')),
                    updated_at TEXT NOT NULL DEFAULT (datetime('now')),
                    otp INTEGER,
                    PRIMARY KEY(user_id AUTOINCREMENT)
                )";
                
                // Create folders table
                string createFoldersTable = @"
                CREATE TABLE IF NOT EXISTS folders (
                    folder_id INTEGER NOT NULL UNIQUE,
                    folder_name TEXT NOT NULL,
                    owner_id INTEGER NOT NULL,
                    parent_folder_id INTEGER,
                    folder_path TEXT NOT NULL,
                    created_at TEXT NOT NULL DEFAULT (datetime('now')),
                    updated_at TEXT NOT NULL DEFAULT (datetime('now')),
                    share_pass TEXT,
                    is_shared INTEGER NOT NULL DEFAULT 0 CHECK(is_shared IN (0, 1)),
                    status TEXT NOT NULL DEFAULT 'ACTIVE' CHECK(status IN ('ACTIVE', 'TRASH')),
                    deleted_at TEXT,
                    PRIMARY KEY(folder_id AUTOINCREMENT),
                    FOREIGN KEY(owner_id) REFERENCES users(user_id) ON DELETE CASCADE,
                    FOREIGN KEY(parent_folder_id) REFERENCES folders(folder_id) ON DELETE CASCADE
                )";
                
                // Create files table with backward compatibility
                string createFilesTable = @"
                CREATE TABLE IF NOT EXISTS files (
                    file_id INTEGER NOT NULL UNIQUE,
                    file_name TEXT NOT NULL,
                    folder_id INTEGER,
                    owner_id INTEGER NOT NULL,
                    file_size INTEGER NOT NULL,
                    file_type TEXT NOT NULL,
                    file_path TEXT NOT NULL,
                    upload_at TEXT NOT NULL DEFAULT (datetime('now')),
                    file_hash TEXT NOT NULL,
                    status TEXT NOT NULL DEFAULT 'ACTIVE' CHECK(status IN ('ACTIVE', 'TRASH')),
                    deleted_at TEXT,
                    PRIMARY KEY(file_id AUTOINCREMENT),
                    FOREIGN KEY(folder_id) REFERENCES folders(folder_id) ON DELETE CASCADE,
                    FOREIGN KEY(owner_id) REFERENCES users(user_id) ON DELETE CASCADE
                )";
                
                // Create folder sharing table
                string createFolderSharesTable = @"
                CREATE TABLE IF NOT EXISTS folder_shares (
                    folder_id INTEGER NOT NULL,
                    shared_with_user_id INTEGER NOT NULL,
                    share_pass TEXT,
                    permission TEXT NOT NULL DEFAULT 'read' CHECK(permission IN ('read', 'write', 'admin')),
                    shared_at TEXT NOT NULL DEFAULT (datetime('now')),
                    PRIMARY KEY(folder_id, shared_with_user_id),
                    FOREIGN KEY(folder_id) REFERENCES folders(folder_id) ON DELETE CASCADE,
                    FOREIGN KEY(shared_with_user_id) REFERENCES users(user_id) ON DELETE CASCADE
                )";

                using (SQLiteCommand cmd = new SQLiteCommand(createUsersTable, conn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                
                using (SQLiteCommand cmd = new SQLiteCommand(createFoldersTable, conn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                
                using (SQLiteCommand cmd = new SQLiteCommand(createFilesTable, conn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                
                using (SQLiteCommand cmd = new SQLiteCommand(createFolderSharesTable, conn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
