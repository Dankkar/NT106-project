using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Threading;

namespace FileSharingServer
{
    public class CleanupService
    {
        private readonly Timer _cleanupTimer;
        private readonly int _cleanupIntervalHours = 24; // Run cleanup every 24 hours
        private readonly int _trashRetentionDays = 30; // Keep files in trash for 30 days

        public CleanupService()
        {
            // Set up timer to run cleanup every 24 hours
            _cleanupTimer = new Timer(async _ => await RunCleanupAsync(), null, TimeSpan.Zero, TimeSpan.FromHours(_cleanupIntervalHours));
        }

        public async Task RunCleanupAsync()
        {
            try
            {
                Console.WriteLine($"[CLEANUP] Starting automatic cleanup process...");
                
                using (var conn = new SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    
                    // Find files that have been in trash for more than 30 days
                    string findOldTrashQuery = @"
                        SELECT file_id, file_name, file_path, owner_id, deleted_at 
                        FROM files 
                        WHERE status = 'TRASH' 
                        AND deleted_at IS NOT NULL 
                        AND datetime(deleted_at) < datetime('now', '-30 days')";
                    
                    var filesToDelete = new List<(int fileId, string fileName, string filePath, int ownerId, string deletedAt)>();
                    
                    using (var cmd = new SQLiteCommand(findOldTrashQuery, conn))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                filesToDelete.Add((
                                    Convert.ToInt32(reader["file_id"]),
                                    reader["file_name"].ToString(),
                                    reader["file_path"].ToString(),
                                    Convert.ToInt32(reader["owner_id"]),
                                    reader["deleted_at"].ToString()
                                ));
                            }
                        }
                    }
                    
                    if (filesToDelete.Count == 0)
                    {
                        Console.WriteLine("[CLEANUP] No files found for cleanup");
                        return;
                    }
                    
                    Console.WriteLine($"[CLEANUP] Found {filesToDelete.Count} files to permanently delete");
                    
                    // Delete files from database and filesystem
                    foreach (var file in filesToDelete)
                    {
                        try
                        {
                            // Delete from database
                            string deleteQuery = "DELETE FROM files WHERE file_id = @file_id";
                            using (var deleteCmd = new SQLiteCommand(deleteQuery, conn))
                            {
                                deleteCmd.Parameters.AddWithValue("@file_id", file.fileId);
                                await deleteCmd.ExecuteNonQueryAsync();
                            }
                            
                            // Delete physical file
                            if (!string.IsNullOrEmpty(file.filePath) && File.Exists(file.filePath))
                            {
                                try
                                {
                                    File.Delete(file.filePath);
                                    Console.WriteLine($"[CLEANUP] Deleted physical file: {file.filePath}");
                                }
                                catch (Exception fileEx)
                                {
                                    Console.WriteLine($"[CLEANUP] Warning: Could not delete physical file {file.filePath}: {fileEx.Message}");
                                }
                            }
                            
                            Console.WriteLine($"[CLEANUP] Permanently deleted file: {file.fileName} (owner: {file.ownerId}, deleted: {file.deletedAt})");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[CLEANUP] Error deleting file {file.fileName}: {ex.Message}");
                        }
                    }
                    
                    Console.WriteLine($"[CLEANUP] Cleanup completed. Processed {filesToDelete.Count} files");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CLEANUP] Error during cleanup process: {ex.Message}");
                Console.WriteLine($"[CLEANUP] StackTrace: {ex.StackTrace}");
            }
        }

        // Method to manually trigger cleanup (can be called from admin interface)
        public async Task TriggerManualCleanupAsync()
        {
            Console.WriteLine("[CLEANUP] Manual cleanup triggered");
            await RunCleanupAsync();
        }

        // Method to get statistics about trash files
        public async Task<(int totalTrashFiles, int expiredFiles, int activeFiles)> GetTrashStatisticsAsync()
        {
            try
            {
                using (var conn = new SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    
                    // Count total trash files
                    string totalQuery = "SELECT COUNT(*) FROM files WHERE status = 'TRASH'";
                    int totalTrash = 0;
                    using (var cmd = new SQLiteCommand(totalQuery, conn))
                    {
                        totalTrash = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    }
                    
                    // Count expired files (older than 30 days)
                    string expiredQuery = @"
                        SELECT COUNT(*) FROM files 
                        WHERE status = 'TRASH' 
                        AND deleted_at IS NOT NULL 
                        AND datetime(deleted_at) < datetime('now', '-30 days')";
                    int expired = 0;
                    using (var cmd = new SQLiteCommand(expiredQuery, conn))
                    {
                        expired = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    }
                    
                    // Count active files
                    string activeQuery = "SELECT COUNT(*) FROM files WHERE status = 'ACTIVE'";
                    int active = 0;
                    using (var cmd = new SQLiteCommand(activeQuery, conn))
                    {
                        active = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    }
                    
                    return (totalTrash, expired, active);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CLEANUP] Error getting trash statistics: {ex.Message}");
                return (0, 0, 0);
            }
        }

        public void Dispose()
        {
            _cleanupTimer?.Dispose();
        }
    }
} 