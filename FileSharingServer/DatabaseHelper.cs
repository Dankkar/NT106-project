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
        public static string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName ?? Environment.CurrentDirectory;
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
                
                // Create files table
                string createFilesTable = @"
                CREATE TABLE IF NOT EXISTS files (
                    file_id INTEGER NOT NULL UNIQUE,
                    owner_id INTEGER NOT NULL,
                    file_name TEXT NOT NULL,
                    file_size INTEGER NOT NULL,
                    file_type TEXT NOT NULL,
                    file_path TEXT NOT NULL,
                    upload_at TEXT NOT NULL DEFAULT (datetime('now')),
                    file_hash TEXT NOT NULL,
                    share_pass TEXT,
                    is_shared INTEGER NOT NULL DEFAULT 0 CHECK(is_shared IN (0, 1)),
                    status TEXT NOT NULL DEFAULT 'ACTIVE' CHECK(status IN ('ACTIVE', 'TRASH')),
                    deleted_at TEXT,
                    folder_id INTEGER,
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

                // Create file sharing table - updated to match actual schema
                string createFileSharesTable = @"
                CREATE TABLE IF NOT EXISTS files_share (
                    file_id INTEGER NOT NULL,
                    user_id INTEGER NOT NULL,
                    share_pass TEXT NOT NULL,
                    permission TEXT NOT NULL DEFAULT 'read' CHECK(permission IN ('read', 'write')),
                    shared_at TEXT NOT NULL DEFAULT (datetime('now')),
                    PRIMARY KEY(file_id, user_id),
                    FOREIGN KEY(file_id) REFERENCES files(file_id),
                    FOREIGN KEY(user_id) REFERENCES users(user_id)
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
                
                using (SQLiteCommand cmd = new SQLiteCommand(createFileSharesTable, conn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                // Migrate existing tables to add missing columns
                await MigrateDatabaseAsync(conn);
            }
        }
        
        private static async Task MigrateDatabaseAsync(SQLiteConnection conn)
        {
            try
            {
                // Check if share_pass column exists in files table
                string checkFilesColumns = @"
                    SELECT COUNT(*) FROM pragma_table_info('files') 
                    WHERE name IN ('share_pass', 'is_shared')";
                
                using (var cmd = new SQLiteCommand(checkFilesColumns, conn))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    int columnCount = Convert.ToInt32(result);
                    
                    if (columnCount < 2)
                    {
                        // Add missing columns to files table
                        string addSharePassColumn = "ALTER TABLE files ADD COLUMN share_pass TEXT";
                        string addIsSharedColumn = "ALTER TABLE files ADD COLUMN is_shared INTEGER NOT NULL DEFAULT 0 CHECK(is_shared IN (0, 1))";
                        
                        using (var addCmd = new SQLiteCommand(addSharePassColumn, conn))
                        {
                            await addCmd.ExecuteNonQueryAsync();
                        }
                        
                        using (var addCmd = new SQLiteCommand(addIsSharedColumn, conn))
                        {
                            await addCmd.ExecuteNonQueryAsync();
                        }
                    }
                }

                // Check if permission column exists in files_share table
                string checkFilesSharePermissionColumn = @"
                    SELECT COUNT(*) FROM pragma_table_info('files_share') 
                    WHERE name = 'permission'";
                
                using (var cmd = new SQLiteCommand(checkFilesSharePermissionColumn, conn))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    int columnCount = Convert.ToInt32(result);
                    
                    if (columnCount == 0)
                    {
                        // Add missing permission column to files_share table
                        string addPermissionColumn = "ALTER TABLE files_share ADD COLUMN permission TEXT NOT NULL DEFAULT 'read' CHECK(permission IN ('read', 'write'))";
                        
                        using (var addCmd = new SQLiteCommand(addPermissionColumn, conn))
                        {
                            await addCmd.ExecuteNonQueryAsync();
                        }
                    }
                }

                // Check if shared_at column exists in files_share table
                string checkFilesShareColumns = @"
                    SELECT COUNT(*) FROM pragma_table_info('files_share') 
                    WHERE name = 'shared_at'";
                
                using (var cmd = new SQLiteCommand(checkFilesShareColumns, conn))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    int columnCount = Convert.ToInt32(result);
                    
                    if (columnCount == 0)
                    {
                        // Add missing shared_at column to files_share table
                        string addSharedAtColumn = "ALTER TABLE files_share ADD COLUMN shared_at TEXT NOT NULL DEFAULT (datetime('now'))";
                        
                        using (var addCmd = new SQLiteCommand(addSharedAtColumn, conn))
                        {
                            await addCmd.ExecuteNonQueryAsync();
                        }
                    }
                }

                // Check if shared_at column exists in folder_shares table
                string checkFolderSharesColumns = @"
                    SELECT COUNT(*) FROM pragma_table_info('folder_shares') 
                    WHERE name = 'shared_at'";
                
                using (var cmd = new SQLiteCommand(checkFolderSharesColumns, conn))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    int columnCount = Convert.ToInt32(result);
                    
                    if (columnCount == 0)
                    {
                        // Add missing shared_at column to folder_shares table
                        string addSharedAtColumn = "ALTER TABLE folder_shares ADD COLUMN shared_at TEXT NOT NULL DEFAULT (datetime('now'))";
                        
                        using (var addCmd = new SQLiteCommand(addSharedAtColumn, conn))
                        {
                            await addCmd.ExecuteNonQueryAsync();
                        }
                    }
                }

                // Check if folder_id column exists in files table
                string checkFilesFolderColumn = @"
                    SELECT COUNT(*) FROM pragma_table_info('files') 
                    WHERE name = 'folder_id'";
                
                using (var cmd = new SQLiteCommand(checkFilesFolderColumn, conn))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    int columnCount = Convert.ToInt32(result);
                    
                    if (columnCount == 0)
                    {
                        // Add missing folder_id column to files table
                        string addFolderIdColumn = "ALTER TABLE files ADD COLUMN folder_id INTEGER";
                        
                        using (var addCmd = new SQLiteCommand(addFolderIdColumn, conn))
                        {
                            await addCmd.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during database migration: {ex.Message}");
            }
        }
    }
}
