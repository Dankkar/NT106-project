using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FileSharingServer
{
    public class ProtocolHandler
    {
        private readonly int _port;

        public ProtocolHandler(int port = 5000)
        {
            _port = port;
        }

        public async Task StartAsync()
        {
            // Initialize database with new folder-based schema
            await DatabaseHelper.InitializeDatabaseAsync();
            Console.WriteLine("Database initialized successfully");

            var server = new TcpListener(IPAddress.Any, _port);
            server.Start();
            Console.WriteLine($"Listening on port {_port}");
            while (true)
            {
                var client = await server.AcceptTcpClientAsync();
                _ = HandleClientAsync(client);
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            Console.WriteLine($"Client {client.Client.RemoteEndPoint} connected.");

            try
            {
                using (NetworkStream stream = client.GetStream())
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
                {
                    string request = await reader.ReadLineAsync();
                    if (request == null)
                    {
                        Console.WriteLine("Client gửi dữ liệu trống.");
                        return;
                    }

                    Console.WriteLine($"Nhận từ Client: {request}");

                    string response = await ProcessRequest(request, stream);
                    await writer.WriteLineAsync(response);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xử lý client: {ex.Message}");
            }
            finally
            {
                client.Close();
                Console.WriteLine("Client đã ngắt kết nối.");
            }
        }
        static async Task<string> ProcessRequest(string request, NetworkStream stream)
        {
            string[] parts = request.Split('|');
            if (parts.Length == 0)
            {
                return "400\n"; // Bad Request
            }

            string command = parts[0];

            switch (command)
            {
                case "REGISTER":
                    if (parts.Length != 4) return "400\n";
                    string username = parts[1];
                    string email = parts[2];
                    string password = parts[3];
                    return await AuthService.RegisterUser(username, email, password);
                case "LOGIN":
                    if (parts.Length != 3) return "400\n";
                    string loginUsername = parts[1];
                    string loginPassword = parts[2];
                    return await AuthService.LoginUser(loginUsername, loginPassword);
                case "CHANGE_PASSWORD":
                    if (parts.Length != 4) return "400\n";
                    string cpUsername = parts[1];
                    string oldPassword = parts[2];
                    string newPassword = parts[3];
                    return await AuthService.ChangePassword(cpUsername, oldPassword, newPassword);
                case "REQUEST_OTP":
                    if (parts.Length != 2) return "400\n";
                    string otpEmail = parts[1];
                    return await OTPService.RequestOTP(otpEmail);
                case "VERIFY_OTP":
                    if (parts.Length != 3) return "400\n";
                    string verifyEmail = parts[1];
                    string otp = parts[2];
                    return await OTPService.VerifyOTP(verifyEmail, otp);
                case "RESET_PASSWORD":
                    if (parts.Length != 3) return "400\n";
                    string resetEmail = parts[1];
                    string newPasswordReset = parts[2];
                    return await OTPService.ResetPassword(resetEmail, newPasswordReset);
                    
                // NEW: Folder-based operations
                case "UPLOAD_FOLDER":
                    if (parts.Length != 5) return "400\n";
                    string folderName = parts[1];
                    int folderSize = int.Parse(parts[2]);
                    string ownerId = parts[3];
                    string uploadTime = parts[4];
                    return await FolderService.ReceiveFolder(folderName, folderSize, ownerId, uploadTime, stream);
                    
                case "LIST_FOLDERS":
                    if (parts.Length != 2) return "400\n";
                    string userId = parts[1];
                    var folders = await FolderService.GetUserFolders(userId);
                    return FormatFolderList(folders);
                    
                // Legacy: Keep using original FileService for single file uploads
                case "UPLOAD":
                    if (parts.Length != 5) return "400\n";
                    string fileName = parts[1];
                    int fileSize = int.Parse(parts[2]);
                    string fileOwnerId = parts[3];
                    string fileUploadTime = parts[4];
                    
                    return await FileService.ReceiveFile(fileName, fileSize, fileOwnerId, fileUploadTime, stream);
                    
                case "UPLOAD_FILE_IN_FOLDER":
                    if (parts.Length != 7) return "400\n";
                    string upFolderName = parts[1];
                    string upRelativePath = parts[2];
                    string upFileName = parts[3];
                    int upFileSize = int.Parse(parts[4]);
                    string upOwnerId = parts[5];
                    string upUploadTime = parts[6];
                    return await FolderService.ReceiveFileInFolder(upFolderName, upRelativePath, upFileName, upFileSize, upOwnerId, upUploadTime, stream);
                    
                // API endpoints for client database replacement
                case "GET_USER_ID":
                    if (parts.Length != 2) return "400\n";
                    return await GetUserId(parts[1]);
                case "GET_USER_FILES":
                    if (parts.Length != 2) return "400\n";
                    return await GetUserFiles(parts[1]);
                case "GET_SHARED_FILES":
                    if (parts.Length != 2) return "400\n";
                    return await GetSharedFiles(parts[1]);
                case "UPDATE_FILE_SHARE":
                    if (parts.Length != 3) return "400\n";
                    return await UpdateFileShare(parts[1], parts[2]);
                case "GET_SHARE_PASS":
                    if (parts.Length != 2) return "400\n";
                    return await GetSharePass(parts[1]);
                case "GET_FILE_INFO":
                    if (parts.Length != 3) return "400\n";
                    return await GetFileInfo(parts[1], parts[2]);
                case "GET_FILE_INFO_BY_SHARE_PASS":
                    if (parts.Length != 2) return "400\n";
                    return await GetFileInfoBySharePass(parts[1]);
                case "ADD_FILE_SHARE_ENTRY":
                    if (parts.Length != 4) return "400\n";
                    return await AddFileShareEntry(parts[1], parts[2], parts[3]);
                case "GET_USER_STORAGE":
                    if (parts.Length != 2) return "400\n";
                    return await GetUserStorage(parts[1]);
                case "DOWNLOAD_FILE":
                    if (parts.Length != 3) return "400\n";
                    return await DownloadFile(parts[1], parts[2], stream);
                case "HEALTH_CHECK":
                    return "200|HEALTHY\n";
                case "DELETE_FILE":
                    if (parts.Length != 3) return "400\n";
                    return await DeleteFile(parts[1], parts[2]);
                case "GET_TRASH_FILES":
                    if (parts.Length != 2) return "400\n";
                    return await GetTrashFiles(parts[1]);
                    
                default:
                    return "400\n";
            }
        }

        private static string FormatFolderList(List<FolderInfo> folders)
        {
            if (folders.Count == 0)
            {
                return "200|NO_FOLDERS\n";
            }

            var result = new StringBuilder();
            result.Append("200|");
            
            foreach (var folder in folders)
            {
                result.Append($"{folder.FolderId}:{folder.FolderName}:{folder.CreatedAt}:{folder.IsShared};");
            }
            
            result.Append("\n");
            return result.ToString();
        }

        // API methods for client database replacement
        private static async Task<string> GetUserId(string username)
        {
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    string query = "SELECT user_id FROM users WHERE username = @username";
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        var result = await cmd.ExecuteScalarAsync();
                        
                        if (result != null)
                        {
                            return $"200|{result}\n";
                        }
                        else
                        {
                            return "404|USER_NOT_FOUND\n";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUserId: {ex.Message}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static async Task<string> GetUserFiles(string userId)
        {
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    string query = "SELECT file_name FROM files WHERE owner_id = @owner_id AND status = 'ACTIVE'";
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@owner_id", int.Parse(userId));
                        
                        var files = new List<string>();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                files.Add(reader["file_name"].ToString());
                            }
                        }
                        
                        if (files.Count == 0)
                        {
                            return "200|NO_FILES\n";
                        }
                        
                        return $"200|{string.Join(";", files)}\n";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUserFiles: {ex.Message}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static async Task<string> GetSharedFiles(string userId)
        {
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    string query = "SELECT f.file_name FROM files_share fs JOIN files f ON fs.file_id = f.file_id WHERE fs.user_id = @user_id AND f.status = 'ACTIVE'";
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@user_id", int.Parse(userId));
                        
                        var files = new List<string>();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                files.Add(reader["file_name"].ToString());
                            }
                        }
                        
                        if (files.Count == 0)
                        {
                            return "200|NO_SHARED_FILES\n";
                        }
                        
                        return $"200|{string.Join(";", files)}\n";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetSharedFiles: {ex.Message}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static async Task<string> UpdateFileShare(string fileName, string sharePass)
        {
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    string updateQuery = "UPDATE files SET share_pass = @sharePass, is_shared = 1 WHERE file_name = @file_name";
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@file_name", fileName);
                        cmd.Parameters.AddWithValue("@sharePass", sharePass);
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        
                        if (rowsAffected > 0)
                        {
                            return "200|FILE_SHARED\n";
                        }
                        else
                        {
                            return "404|FILE_NOT_FOUND\n";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateFileShare: {ex.Message}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static async Task<string> GetSharePass(string fileName)
        {
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    string query = "SELECT share_pass FROM files WHERE file_name = @file_name";
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@file_name", fileName);
                        var result = await cmd.ExecuteScalarAsync();
                        
                        if (result != null)
                        {
                            string sharePass = result.ToString();
                            return $"200|{sharePass}\n";
                        }
                        else
                        {
                            return "404|FILE_NOT_FOUND\n";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetSharePass: {ex.Message}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static async Task<string> GetFileInfo(string fileName, string userId)
        {
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"
                        SELECT f.file_path, f.file_type
                        FROM files f
                        WHERE f.file_name = @fileName
                        AND (
                            f.owner_id = @userId
                            OR f.file_id IN (
                                SELECT fs.file_id FROM files_share fs WHERE fs.user_id = @userId
                            )
                        )
                        LIMIT 1
                    ";
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@fileName", fileName);
                        cmd.Parameters.AddWithValue("@userId", int.Parse(userId));
                        
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                string filePath = reader["file_path"].ToString();
                                string fileType = reader["file_type"].ToString();
                                return $"200|{filePath}|{fileType}\n";
                            }
                            else
                            {
                                return "404|FILE_NOT_FOUND\n";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetFileInfo: {ex.Message}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static async Task<string> GetFileInfoBySharePass(string sharePass)
        {
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    string query = "SELECT file_id, owner_id FROM files WHERE share_pass = @share_pass";
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@share_pass", sharePass);
                        
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                int fileId = Convert.ToInt32(reader["file_id"]);
                                int ownerId = Convert.ToInt32(reader["owner_id"]);
                                return $"200|{fileId}|{ownerId}\n";
                            }
                            else
                            {
                                return "404|FILE_NOT_FOUND\n";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetFileInfoBySharePass: {ex.Message}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static async Task<string> AddFileShareEntry(string fileId, string userId, string sharePass)
        {
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    
                    // Check if the entry already exists
                    string checkQuery = "SELECT COUNT(*) FROM files_share WHERE file_id = @file_id AND user_id = @user_id";
                    using (var checkCmd = new System.Data.SQLite.SQLiteCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@file_id", int.Parse(fileId));
                        checkCmd.Parameters.AddWithValue("@user_id", int.Parse(userId));
                        
                        long count = (long)await checkCmd.ExecuteScalarAsync();
                        if (count > 0)
                        {
                            return "200|ALREADY_SHARED\n";
                        }
                    }
                    
                    // Insert new share entry
                    string insertQuery = "INSERT INTO files_share (file_id, user_id, share_pass) VALUES (@file_id, @user_id, @share_pass)";
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@file_id", int.Parse(fileId));
                        cmd.Parameters.AddWithValue("@user_id", int.Parse(userId));
                        cmd.Parameters.AddWithValue("@share_pass", sharePass);
                        
                        await cmd.ExecuteNonQueryAsync();
                        return "200|SHARE_ADDED\n";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddFileShareEntry: {ex.Message}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static async Task<string> DeleteFile(string fileName, string userId)
        {
            try
            {
                Console.WriteLine($"[DEBUG] DELETE_FILE request received: fileName={fileName}, userId={userId}");
                
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    
                    // First, check if file exists
                    string checkQuery = "SELECT COUNT(*) FROM files WHERE file_name = @file_name AND owner_id = @owner_id";
                    using (var checkCmd = new System.Data.SQLite.SQLiteCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@file_name", fileName);
                        checkCmd.Parameters.AddWithValue("@owner_id", int.Parse(userId));
                        
                        long fileCount = (long)await checkCmd.ExecuteScalarAsync();
                        Console.WriteLine($"[DEBUG] Files found with name '{fileName}' for user {userId}: {fileCount}");
                        
                        if (fileCount == 0)
                        {
                            Console.WriteLine($"[DEBUG] File '{fileName}' not found for user {userId}");
                            return "404|FILE_NOT_FOUND\n";
                        }
                    }
                    
                    // Update file status to TRASH and set deleted_at timestamp
                    string updateQuery = "UPDATE files SET status = 'TRASH', deleted_at = datetime('now') WHERE file_name = @file_name AND owner_id = @owner_id AND status = 'ACTIVE'";
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@file_name", fileName);
                        cmd.Parameters.AddWithValue("@owner_id", int.Parse(userId));
                        
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        Console.WriteLine($"[DEBUG] Rows affected by UPDATE: {rowsAffected}");
                        
                        if (rowsAffected > 0)
                        {
                            Console.WriteLine($"[SUCCESS] File '{fileName}' moved to trash by user {userId}");
                            return "200|FILE_DELETED\n";
                        }
                        else
                        {
                            Console.WriteLine($"[DEBUG] No rows updated - file might not have status='ACTIVE'");
                            return "404|FILE_NOT_FOUND_OR_ALREADY_DELETED\n";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error in DeleteFile: {ex.Message}");
                Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static async Task<string> GetTrashFiles(string userId)
        {
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    string query = "SELECT file_name, deleted_at FROM files WHERE owner_id = @owner_id AND status = 'TRASH' ORDER BY deleted_at DESC";
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@owner_id", int.Parse(userId));
                        
                        var files = new List<string>();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string fileName = reader["file_name"].ToString();
                                string deletedAt = reader["deleted_at"].ToString();
                                files.Add($"{fileName}:{deletedAt}");
                            }
                        }
                        
                        if (files.Count == 0)
                        {
                            return "200|NO_TRASH_FILES\n";
                        }
                        
                        return $"200|{string.Join(";", files)}\n";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTrashFiles: {ex.Message}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static async Task<string> DownloadFile(string fileName, string userId, NetworkStream stream)
        {
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    
                    // Get file path and verify user access
                    string query = @"
                        SELECT f.file_path, f.file_size, f.owner_id
                        FROM files f
                        WHERE f.file_name = @fileName
                        AND f.status = 'ACTIVE'
                        AND (
                            f.owner_id = @userId
                            OR f.file_id IN (
                                SELECT fs.file_id FROM files_share fs WHERE fs.user_id = @userId
                            )
                        )
                        LIMIT 1
                    ";
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@fileName", fileName);
                        cmd.Parameters.AddWithValue("@userId", int.Parse(userId));
                        
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                string filePath = reader["file_path"].ToString();
                                long fileSize = Convert.ToInt64(reader["file_size"]);
                                int ownerId = Convert.ToInt32(reader["owner_id"]);
                                
                                // Build full file path
                                string fullFilePath = Path.Combine(GetSharedUploadsPath(), filePath);
                                
                                if (File.Exists(fullFilePath))
                                {
                                    // Send file size first
                                    byte[] sizeBytes = Encoding.UTF8.GetBytes($"200|{fileSize}\n");
                                    await stream.WriteAsync(sizeBytes, 0, sizeBytes.Length);
                                    await stream.FlushAsync();
                                    
                                    // Send encrypted file data
                                    byte[] encryptedData = File.ReadAllBytes(fullFilePath);
                                    await stream.WriteAsync(encryptedData, 0, encryptedData.Length);
                                    await stream.FlushAsync();
                                    
                                    return ""; // Don't return anything more since we already sent data
                                }
                                else
                                {
                                    return "404|FILE_NOT_FOUND_ON_DISK\n";
                                }
                            }
                            else
                            {
                                return "404|FILE_NOT_FOUND\n";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DownloadFile: {ex.Message}");
                return "500|INTERNAL_ERROR\n";
            }
        }
        
        private static async Task<string> GetUserStorage(string userId)
        {
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    string query = "SELECT COALESCE(SUM(file_size), 0) as total_size FROM files WHERE owner_id = @owner_id AND status = 'ACTIVE'";
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@owner_id", int.Parse(userId));
                        var result = await cmd.ExecuteScalarAsync();
                        
                        long totalSizeBytes = Convert.ToInt64(result);
                        return $"200|{totalSizeBytes}\n";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUserStorage: {ex.Message}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static string GetSharedUploadsPath()
        {
            // Ensure all server instances use the same uploads directory
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
                return Path.Combine(current.FullName, "uploads");
            }
            
            // Fallback: use original logic
            return Path.Combine(DatabaseHelper.projectRoot ?? Environment.CurrentDirectory, "uploads");
        }
    }
}
