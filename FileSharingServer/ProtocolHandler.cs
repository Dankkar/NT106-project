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
                    Console.WriteLine($"[DEBUG] HandleClientAsync - About to send response: '{response.Substring(0, Math.Min(200, response.Length))}...'");
                    Console.WriteLine($"[DEBUG] HandleClientAsync - Response length: {response.Length}");
                    Console.WriteLine($"[DEBUG] HandleClientAsync - Response ends with newline: {response.EndsWith("\n")}");
                    
                    // Don't use WriteLineAsync since our methods already include \n
                    await writer.WriteAsync(response);
                    await writer.FlushAsync(); // Ensure data is sent immediately
                    Console.WriteLine($"[DEBUG] HandleClientAsync - Response sent and flushed");
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
            Console.WriteLine($"[DEBUG] ProcessRequest - Raw request: '{request}'");
            
            string[] parts = request.Split('|');
            Console.WriteLine($"[DEBUG] ProcessRequest - Split into {parts.Length} parts");
            
            if (parts.Length == 0)
            {
                Console.WriteLine($"[ERROR] ProcessRequest - No parts found in request");
                return "400\n"; // Bad Request
            }

            string command = parts[0];
            Console.WriteLine($"[DEBUG] ProcessRequest - Command: '{command}'");

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
                case "DELETE_FILE":
                    if (parts.Length != 3) return "400\n";
                    return await DeleteFile(parts[1], parts[2]);
                case "GET_TRASH_FILES":
                    Console.WriteLine($"[DEBUG] ProcessRequest - GET_TRASH_FILES case reached");
                    if (parts.Length != 2) 
                    {
                        Console.WriteLine($"[ERROR] ProcessRequest - GET_TRASH_FILES: Invalid parts count: {parts.Length}");
                        return "400\n";
                    }
                    Console.WriteLine($"[DEBUG] ProcessRequest - GET_TRASH_FILES: Calling GetTrashFiles with userId: {parts[1]}");
                    string trashResult = await GetTrashFiles(parts[1]);
                    Console.WriteLine($"[DEBUG] ProcessRequest - GET_TRASH_FILES: GetTrashFiles returned: '{trashResult.Substring(0, Math.Min(100, trashResult.Length))}...'");
                    return trashResult;
                case "RESTORE_FILE":
                    if (parts.Length != 3) return "400\n";
                    return await RestoreFile(parts[1], parts[2]);
                case "PERMANENTLY_DELETE_FILE":
                    if (parts.Length != 3) return "400\n";
                    return await PermanentlyDeleteFile(parts[1], parts[2]);
                case "DELETE_FOLDER":
                    if (parts.Length != 3) return "400\n";
                    return await DeleteFolder(parts[1], parts[2]);
                case "GET_TRASH_FOLDERS":
                    if (parts.Length != 2) return "400\n";
                    return await GetTrashFolders(parts[1]);
                case "RESTORE_FOLDER":
                    if (parts.Length != 3) return "400\n";
                    return await RestoreFolder(parts[1], parts[2]);
                case "PERMANENTLY_DELETE_FOLDER":
                    if (parts.Length != 3) return "400\n";
                    return await PermanentlyDeleteFolder(parts[1], parts[2]);
                    
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
                                string filePath = reader["file_path"]?.ToString() ?? "";
                                string fileType = reader["file_type"]?.ToString() ?? "";
                                
                                // Clean file path - ensure it's safe
                                filePath = CleanFilePath(filePath);
                                fileType = CleanString(fileType);
                                
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
                Console.WriteLine($"[DEBUG] GetTrashFiles - Called with userId: {userId}");
                
                // Validate userId
                if (!int.TryParse(userId, out int userIdInt))
                {
                    Console.WriteLine($"[ERROR] GetTrashFiles - Invalid userId format: {userId}");
                    return "400|INVALID_USER_ID\n";
                }
                
                Console.WriteLine($"[DEBUG] GetTrashFiles - Parsed userId as: {userIdInt}");
                
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    Console.WriteLine($"[DEBUG] GetTrashFiles - Database connection opened");
                    
                    string query = "SELECT file_name, deleted_at FROM files WHERE owner_id = @owner_id AND status = 'TRASH' ORDER BY deleted_at DESC";
                    Console.WriteLine($"[DEBUG] GetTrashFiles - Executing query: {query}");
                    
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@owner_id", userIdInt);
                        Console.WriteLine($"[DEBUG] GetTrashFiles - Query parameter @owner_id = {userIdInt}");
                        
                        var files = new List<string>();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            int rowCount = 0;
                            while (await reader.ReadAsync())
                            {
                                rowCount++;
                                string fileName = reader["file_name"]?.ToString() ?? "";
                                string deletedAt = reader["deleted_at"]?.ToString() ?? "";
                                
                                Console.WriteLine($"[DEBUG] GetTrashFiles - Row {rowCount}: fileName='{fileName}', deletedAt='{deletedAt}'");
                                
                                // Clean data before sending to client
                                fileName = CleanString(fileName);
                                deletedAt = CleanString(deletedAt);
                                
                                Console.WriteLine($"[DEBUG] GetTrashFiles - Cleaned Row {rowCount}: fileName='{fileName}', deletedAt='{deletedAt}'");
                                
                                if (!string.IsNullOrWhiteSpace(fileName))
                                {
                                files.Add($"{fileName}:{deletedAt}");
                                    Console.WriteLine($"[DEBUG] GetTrashFiles - Added to list: '{fileName}:{deletedAt}'");
                                }
                                else
                                {
                                    Console.WriteLine($"[WARNING] GetTrashFiles - Skipped row {rowCount} due to empty fileName");
                                }
                            }
                            
                            Console.WriteLine($"[DEBUG] GetTrashFiles - Total rows found: {rowCount}");
                            Console.WriteLine($"[DEBUG] GetTrashFiles - Valid files after cleaning: {files.Count}");
                        }
                        
                        if (files.Count == 0)
                        {
                            Console.WriteLine($"[DEBUG] GetTrashFiles - No trash files found, returning NO_TRASH_FILES");
                            return "200|NO_TRASH_FILES\n";
                        }
                        
                        string result = $"200|{string.Join(";", files)}\n";
                        Console.WriteLine($"[DEBUG] GetTrashFiles - Final result length: {result.Length}");
                        Console.WriteLine($"[DEBUG] GetTrashFiles - Result starts with: '{result.Substring(0, Math.Min(50, result.Length))}'");
                        Console.WriteLine($"[DEBUG] GetTrashFiles - Result ends with: '{result.Substring(Math.Max(0, result.Length - 20))}'");
                        Console.WriteLine($"[DEBUG] GetTrashFiles - Files included: {files.Count}");
                        
                        // Validate result format
                        if (!result.StartsWith("200|"))
                        {
                            Console.WriteLine($"[ERROR] GetTrashFiles - Result doesn't start with '200|': '{result.Substring(0, Math.Min(20, result.Length))}'");
                        }
                        
                        // Additional validation
                        result = ValidateResponse(result);
                        Console.WriteLine($"[DEBUG] GetTrashFiles - Final validated result: '{result.Substring(0, Math.Min(100, result.Length))}...'");
                        
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetTrashFiles - Exception: {ex.Message}");
                Console.WriteLine($"[ERROR] GetTrashFiles - StackTrace: {ex.StackTrace}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static string CleanFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return "";

            try
            {
                // Remove invalid characters and normalize path
                char[] invalidChars = System.IO.Path.GetInvalidPathChars();
                foreach (char c in invalidChars)
                {
                    filePath = filePath.Replace(c.ToString(), "");
                }
                
                // Normalize path separators
                filePath = filePath.Replace('\\', '/');
                
                return filePath;
            }
            catch
            {
                return "";
            }
        }

        private static string CleanString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "";

            try
            {
                // Remove control characters and other problematic characters
                var result = new StringBuilder();
                foreach (char c in input)
                {
                    if (!char.IsControl(c) && c != '|' && c != '\n' && c != '\r' && c != ';')
                    {
                        result.Append(c);
                    }
                }
                string cleaned = result.ToString().Trim();
                
                // Extra safety: replace any remaining problematic characters
                cleaned = cleaned.Replace("|", "_").Replace(";", "_").Replace("\n", "_").Replace("\r", "_");
                
                Console.WriteLine($"[DEBUG] CleanString - Input: '{input}' -> Output: '{cleaned}'");
                return cleaned;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] CleanString - Exception: {ex.Message}");
                return "CLEANED_ERROR";
            }
        }

        private static string ValidateResponse(string response)
        {
            if (string.IsNullOrEmpty(response))
            {
                Console.WriteLine("[ERROR] ValidateResponse - Empty response");
                return "500|EMPTY_RESPONSE\n";
            }

            try
            {
                // Ensure response ends with newline
                if (!response.EndsWith("\n"))
                {
                    Console.WriteLine("[WARNING] ValidateResponse - Adding missing newline");
                    response = response + "\n";
                }

                // Check for valid status code format
                if (!response.StartsWith("200|") && !response.StartsWith("404|") && !response.StartsWith("500|") && !response.StartsWith("400|"))
                {
                    Console.WriteLine($"[ERROR] ValidateResponse - Invalid status code format: '{response.Substring(0, Math.Min(20, response.Length))}'");
                    return "500|INVALID_FORMAT\n";
                }

                // Check for problematic characters that could break parsing
                if (response.Contains("\r\n") || response.Contains("\n\r"))
                {
                    Console.WriteLine("[WARNING] ValidateResponse - Found CRLF combinations, cleaning...");
                    response = response.Replace("\r\n", "\n").Replace("\n\r", "\n");
                }

                Console.WriteLine($"[DEBUG] ValidateResponse - Response validated successfully");
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] ValidateResponse - Exception: {ex.Message}");
                return "500|VALIDATION_ERROR\n";
            }
        }

        private static async Task<string> RestoreFile(string fileName, string userId)
        {
            try
            {
                Console.WriteLine($"[DEBUG] RESTORE_FILE request received: fileName={fileName}, userId={userId}");
                
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    
                    // First, check if file exists in trash
                    string checkExistenceQuery = "SELECT COUNT(*) FROM files WHERE file_name = @file_name AND owner_id = @owner_id AND status = 'TRASH'";
                    using (var checkCmd = new System.Data.SQLite.SQLiteCommand(checkExistenceQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@file_name", fileName);
                        checkCmd.Parameters.AddWithValue("@owner_id", int.Parse(userId));
                        
                        long fileCount = (long)await checkCmd.ExecuteScalarAsync();
                        Console.WriteLine($"[DEBUG] Trash files found with name '{fileName}' for user {userId}: {fileCount}");
                        
                        if (fileCount == 0)
                        {
                            Console.WriteLine($"[DEBUG] File '{fileName}' not found in trash for user {userId}");
                            return "404|FILE_NOT_FOUND_IN_TRASH\n";
                        }
                    }
                    
                    // Then, get the folder_id (can be NULL for standalone files)
                    string getFolderIdQuery = "SELECT folder_id FROM files WHERE file_name = @file_name AND owner_id = @owner_id AND status = 'TRASH'";
                    int? fileFolderId = null;
                    bool fileWasInDeletedFolder = false;
                    
                    using (var getFolderCmd = new System.Data.SQLite.SQLiteCommand(getFolderIdQuery, conn))
                    {
                        getFolderCmd.Parameters.AddWithValue("@file_name", fileName);
                        getFolderCmd.Parameters.AddWithValue("@owner_id", int.Parse(userId));
                        
                        var result = await getFolderCmd.ExecuteScalarAsync();
                        
                        if (result != null && result != DBNull.Value && int.TryParse(result.ToString(), out int folderId))
                        {
                            fileFolderId = folderId;
                            Console.WriteLine($"[DEBUG] File '{fileName}' belongs to folder_id: {fileFolderId}");
                        }
                        else
                        {
                            Console.WriteLine($"[DEBUG] File '{fileName}' is a standalone file (folder_id = NULL)");
                        }
                    }
                    
                    // If file belongs to a folder, check if the folder is also in trash
                    if (fileFolderId.HasValue)
                    {
                        string checkFolderQuery = "SELECT status FROM folders WHERE folder_id = @folder_id";
                        using (var checkFolderCmd = new System.Data.SQLite.SQLiteCommand(checkFolderQuery, conn))
                        {
                            checkFolderCmd.Parameters.AddWithValue("@folder_id", fileFolderId.Value);
                            var folderStatusResult = await checkFolderCmd.ExecuteScalarAsync();
                            var folderStatus = folderStatusResult?.ToString();
                            
                            if (folderStatus == "TRASH")
                            {
                                fileWasInDeletedFolder = true;
                                Console.WriteLine($"[DEBUG] Parent folder {fileFolderId.Value} is in trash, file will be moved to root level");
                            }
                            else
                            {
                                Console.WriteLine($"[DEBUG] Parent folder {fileFolderId.Value} status: {folderStatus}");
                            }
                        }
                    }
                    
                    // Restore file - if parent folder is deleted, move file to root level (folder_id = NULL)
                    string updateQuery;
                    if (fileWasInDeletedFolder)
                    {
                        updateQuery = "UPDATE files SET status = 'ACTIVE', deleted_at = NULL, folder_id = NULL WHERE file_name = @file_name AND owner_id = @owner_id AND status = 'TRASH'";
                        Console.WriteLine($"[DEBUG] Moving file to root level due to deleted parent folder");
                    }
                    else
                    {
                        updateQuery = "UPDATE files SET status = 'ACTIVE', deleted_at = NULL WHERE file_name = @file_name AND owner_id = @owner_id AND status = 'TRASH'";
                        Console.WriteLine($"[DEBUG] Restoring file in its original folder");
                    }
                    
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@file_name", fileName);
                        cmd.Parameters.AddWithValue("@owner_id", int.Parse(userId));
                        
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        Console.WriteLine($"[DEBUG] Rows affected by FILE RESTORE: {rowsAffected}");
                        
                        if (rowsAffected > 0)
                        {
                            string successMessage = $"[SUCCESS] File '{fileName}' restored by user {userId}";
                            if (fileWasInDeletedFolder)
                            {
                                successMessage += $" (moved to root level due to deleted parent folder {fileFolderId.Value})";
                            }
                            Console.WriteLine(successMessage);
                            
                            return "200|FILE_RESTORED\n";
                        }
                        else
                        {
                            Console.WriteLine($"[DEBUG] No rows updated during restore");
                            return "404|FILE_NOT_FOUND_IN_TRASH\n";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error in RestoreFile: {ex.Message}");
                Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static async Task<string> PermanentlyDeleteFile(string fileName, string userId)
        {
            try
            {
                Console.WriteLine($"[DEBUG] PERMANENTLY_DELETE_FILE request received: fileName={fileName}, userId={userId}");
                
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    
                    // Get file info before deletion
                    string getFileQuery = "SELECT file_path FROM files WHERE file_name = @file_name AND owner_id = @owner_id AND status = 'TRASH'";
                    string filePath = null;
                    
                    using (var getCmd = new System.Data.SQLite.SQLiteCommand(getFileQuery, conn))
                    {
                        getCmd.Parameters.AddWithValue("@file_name", fileName);
                        getCmd.Parameters.AddWithValue("@owner_id", int.Parse(userId));
                        
                        var result = await getCmd.ExecuteScalarAsync();
                        if (result != null)
                        {
                            filePath = result.ToString();
                        }
                        else
                        {
                            Console.WriteLine($"[DEBUG] File '{fileName}' not found in trash for user {userId}");
                            return "404|FILE_NOT_FOUND_IN_TRASH\n";
                        }
                    }
                    
                    // Delete file record from database
                    string deleteQuery = "DELETE FROM files WHERE file_name = @file_name AND owner_id = @owner_id AND status = 'TRASH'";
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@file_name", fileName);
                        cmd.Parameters.AddWithValue("@owner_id", int.Parse(userId));
                        
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        Console.WriteLine($"[DEBUG] Rows affected by PERMANENT DELETE: {rowsAffected}");
                        
                        if (rowsAffected > 0)
                        {
                            // Also delete physical file if it exists
                            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                            {
                                try
                                {
                                    File.Delete(filePath);
                                    Console.WriteLine($"[DEBUG] Physical file deleted: {filePath}");
                                }
                                catch (Exception fileEx)
                                {
                                    Console.WriteLine($"[WARNING] Could not delete physical file {filePath}: {fileEx.Message}");
                                }
                            }
                            
                            Console.WriteLine($"[SUCCESS] File '{fileName}' permanently deleted by user {userId}");
                            return "200|FILE_PERMANENTLY_DELETED\n";
                        }
                        else
                        {
                            Console.WriteLine($"[DEBUG] No rows deleted during permanent deletion");
                            return "404|FILE_NOT_FOUND_IN_TRASH\n";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error in PermanentlyDeleteFile: {ex.Message}");
                Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static async Task<string> DeleteFolder(string folderId, string userId)
        {
            try
            {
                Console.WriteLine($"[DEBUG] DELETE_FOLDER request received: folderId={folderId}, userId={userId}");
                
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    
                    // First, check if folder exists and belongs to user
                    string checkQuery = "SELECT COUNT(*) FROM folders WHERE folder_id = @folder_id AND owner_id = @owner_id AND status = 'ACTIVE'";
                    using (var checkCmd = new System.Data.SQLite.SQLiteCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@folder_id", int.Parse(folderId));
                        checkCmd.Parameters.AddWithValue("@owner_id", int.Parse(userId));
                        
                        long folderCount = (long)await checkCmd.ExecuteScalarAsync();
                        Console.WriteLine($"[DEBUG] Folders found with id '{folderId}' for user {userId}: {folderCount}");
                        
                        if (folderCount == 0)
                        {
                            Console.WriteLine($"[DEBUG] Folder '{folderId}' not found for user {userId}");
                            return "404|FOLDER_NOT_FOUND\n";
                        }
                    }
                    
                    // Move folder to trash
                    string updateQuery = "UPDATE folders SET status = 'TRASH', deleted_at = datetime('now') WHERE folder_id = @folder_id AND owner_id = @owner_id AND status = 'ACTIVE'";
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@folder_id", int.Parse(folderId));
                        cmd.Parameters.AddWithValue("@owner_id", int.Parse(userId));
                        
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        Console.WriteLine($"[DEBUG] Rows affected by FOLDER DELETE: {rowsAffected}");
                        
                        if (rowsAffected > 0)
                        {
                            // Also move all files in this folder to trash
                            string moveFilesQuery = "UPDATE files SET status = 'TRASH', deleted_at = datetime('now') WHERE folder_id = @folder_id AND status = 'ACTIVE'";
                            using (var moveFilesCmd = new System.Data.SQLite.SQLiteCommand(moveFilesQuery, conn))
                            {
                                moveFilesCmd.Parameters.AddWithValue("@folder_id", int.Parse(folderId));
                                int filesAffected = await moveFilesCmd.ExecuteNonQueryAsync();
                                Console.WriteLine($"[DEBUG] Files moved to trash: {filesAffected}");
                            }
                            
                            Console.WriteLine($"[SUCCESS] Folder '{folderId}' moved to trash by user {userId}");
                            return "200|FOLDER_DELETED\n";
                        }
                        else
                        {
                            Console.WriteLine($"[DEBUG] No rows updated during folder deletion");
                            return "404|FOLDER_NOT_FOUND\n";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error in DeleteFolder: {ex.Message}");
                Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static async Task<string> GetTrashFolders(string userId)
        {
            try
            {
                Console.WriteLine($"[DEBUG] GetTrashFolders - Called with userId: {userId}");
                
                if (!int.TryParse(userId, out int userIdInt))
                {
                    Console.WriteLine($"[ERROR] GetTrashFolders - Invalid userId format: {userId}");
                    return "400|INVALID_USER_ID\n";
                }
                
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    
                    string query = "SELECT folder_id, folder_name, deleted_at FROM folders WHERE owner_id = @owner_id AND status = 'TRASH' ORDER BY deleted_at DESC";
                    
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@owner_id", userIdInt);
                        
                        var folders = new List<string>();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string folderId = reader["folder_id"]?.ToString() ?? "";
                                string folderName = reader["folder_name"]?.ToString() ?? "";
                                string deletedAt = reader["deleted_at"]?.ToString() ?? "";
                                
                                // Clean data before sending to client
                                folderName = CleanString(folderName);
                                deletedAt = CleanString(deletedAt);
                                
                                if (!string.IsNullOrWhiteSpace(folderName))
                                {
                                    folders.Add($"{folderId}:{folderName}:{deletedAt}");
                                }
                            }
                        }
                        
                        if (folders.Count == 0)
                        {
                            Console.WriteLine($"[DEBUG] GetTrashFolders - No trash folders found, returning NO_TRASH_FOLDERS");
                            return "200|NO_TRASH_FOLDERS\n";
                        }
                        
                        string result = $"200|{string.Join(";", folders)}\n";
                        result = ValidateResponse(result);
                        
                        Console.WriteLine($"[DEBUG] GetTrashFolders - Final result: '{result.Substring(0, Math.Min(100, result.Length))}...'");
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetTrashFolders - Exception: {ex.Message}");
                Console.WriteLine($"[ERROR] GetTrashFolders - StackTrace: {ex.StackTrace}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static async Task<string> RestoreFolder(string folderId, string userId)
        {
            try
            {
                Console.WriteLine($"[DEBUG] RESTORE_FOLDER request received: folderId={folderId}, userId={userId}");
                
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    
                    // Check if folder exists in trash
                    string checkQuery = "SELECT COUNT(*) FROM folders WHERE folder_id = @folder_id AND owner_id = @owner_id AND status = 'TRASH'";
                    using (var checkCmd = new System.Data.SQLite.SQLiteCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@folder_id", int.Parse(folderId));
                        checkCmd.Parameters.AddWithValue("@owner_id", int.Parse(userId));
                        
                        long folderCount = (long)await checkCmd.ExecuteScalarAsync();
                        Console.WriteLine($"[DEBUG] Trash folders found with id '{folderId}' for user {userId}: {folderCount}");
                        
                        if (folderCount == 0)
                        {
                            Console.WriteLine($"[DEBUG] Folder '{folderId}' not found in trash for user {userId}");
                            return "404|FOLDER_NOT_FOUND_IN_TRASH\n";
                        }
                    }
                    
                    // Restore folder by changing status back to ACTIVE and clearing deleted_at
                    string updateQuery = "UPDATE folders SET status = 'ACTIVE', deleted_at = NULL WHERE folder_id = @folder_id AND owner_id = @owner_id AND status = 'TRASH'";
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@folder_id", int.Parse(folderId));
                        cmd.Parameters.AddWithValue("@owner_id", int.Parse(userId));
                        
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        Console.WriteLine($"[DEBUG] Rows affected by FOLDER RESTORE: {rowsAffected}");
                        
                        if (rowsAffected > 0)
                        {
                            // First, check how many files are in this folder (both TRASH and ACTIVE)
                            string countAllFilesQuery = "SELECT COUNT(*) FROM files WHERE folder_id = @folder_id";
                            int totalFiles = 0;
                            using (var countAllCmd = new System.Data.SQLite.SQLiteCommand(countAllFilesQuery, conn))
                            {
                                countAllCmd.Parameters.AddWithValue("@folder_id", int.Parse(folderId));
                                totalFiles = Convert.ToInt32(await countAllCmd.ExecuteScalarAsync());
                            }
                            
                            // Check how many files are already ACTIVE (restored individually before)
                            string countActiveFilesQuery = "SELECT COUNT(*) FROM files WHERE folder_id = @folder_id AND status = 'ACTIVE'";
                            int alreadyActiveFiles = 0;
                            using (var countActiveCmd = new System.Data.SQLite.SQLiteCommand(countActiveFilesQuery, conn))
                            {
                                countActiveCmd.Parameters.AddWithValue("@folder_id", int.Parse(folderId));
                                alreadyActiveFiles = Convert.ToInt32(await countActiveCmd.ExecuteScalarAsync());
                            }
                            
                            // Also restore all files in this folder that are still in TRASH
                            string restoreFilesQuery = "UPDATE files SET status = 'ACTIVE', deleted_at = NULL WHERE folder_id = @folder_id AND status = 'TRASH'";
                            using (var restoreFilesCmd = new System.Data.SQLite.SQLiteCommand(restoreFilesQuery, conn))
                            {
                                restoreFilesCmd.Parameters.AddWithValue("@folder_id", int.Parse(folderId));
                                int filesRestored = await restoreFilesCmd.ExecuteNonQueryAsync();
                                
                                Console.WriteLine($"[DEBUG] Folder contains {totalFiles} total files");
                                Console.WriteLine($"[DEBUG] Files already active (restored individually): {alreadyActiveFiles}");
                                Console.WriteLine($"[DEBUG] Files restored with folder: {filesRestored}");
                                Console.WriteLine($"[DEBUG] Final result: All {totalFiles} files are now active");
                                
                                if (alreadyActiveFiles > 0)
                                {
                                    Console.WriteLine($"[INFO] {alreadyActiveFiles} file(s) were already restored individually and remain unchanged (no duplicates created)");
                                }
                            }
                            
                            Console.WriteLine($"[SUCCESS] Folder '{folderId}' restored by user {userId}");
                            return "200|FOLDER_RESTORED\n";
                        }
                        else
                        {
                            Console.WriteLine($"[DEBUG] No rows updated during folder restore");
                            return "404|FOLDER_NOT_FOUND_IN_TRASH\n";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error in RestoreFolder: {ex.Message}");
                Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static async Task<string> PermanentlyDeleteFolder(string folderId, string userId)
        {
            try
            {
                Console.WriteLine($"[DEBUG] PERMANENTLY_DELETE_FOLDER request received: folderId={folderId}, userId={userId}");
                
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    
                    // Get folder info before deletion
                    string getFolderQuery = "SELECT folder_path FROM folders WHERE folder_id = @folder_id AND owner_id = @owner_id AND status = 'TRASH'";
                    string folderPath = null;
                    
                    using (var getCmd = new System.Data.SQLite.SQLiteCommand(getFolderQuery, conn))
                    {
                        getCmd.Parameters.AddWithValue("@folder_id", int.Parse(folderId));
                        getCmd.Parameters.AddWithValue("@owner_id", int.Parse(userId));
                        
                        var result = await getCmd.ExecuteScalarAsync();
                        if (result != null)
                        {
                            folderPath = result.ToString();
                        }
                        else
                        {
                            Console.WriteLine($"[DEBUG] Folder '{folderId}' not found in trash for user {userId}");
                            return "404|FOLDER_NOT_FOUND_IN_TRASH\n";
                        }
                    }
                    
                    // Delete all files in this folder first
                    string deleteFilesQuery = "DELETE FROM files WHERE folder_id = @folder_id AND status = 'TRASH'";
                    using (var deleteFilesCmd = new System.Data.SQLite.SQLiteCommand(deleteFilesQuery, conn))
                    {
                        deleteFilesCmd.Parameters.AddWithValue("@folder_id", int.Parse(folderId));
                        int filesDeleted = await deleteFilesCmd.ExecuteNonQueryAsync();
                        Console.WriteLine($"[DEBUG] Files permanently deleted: {filesDeleted}");
                    }
                    
                    // Delete folder record from database
                    string deleteFolderQuery = "DELETE FROM folders WHERE folder_id = @folder_id AND owner_id = @owner_id AND status = 'TRASH'";
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(deleteFolderQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@folder_id", int.Parse(folderId));
                        cmd.Parameters.AddWithValue("@owner_id", int.Parse(userId));
                        
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        Console.WriteLine($"[DEBUG] Rows affected by PERMANENT FOLDER DELETE: {rowsAffected}");
                        
                        if (rowsAffected > 0)
                        {
                            // Also delete physical folder if it exists
                            if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
                            {
                                try
                                {
                                    Directory.Delete(folderPath, true); // Delete recursively
                                    Console.WriteLine($"[DEBUG] Physical folder deleted: {folderPath}");
                                }
                                catch (Exception folderEx)
                                {
                                    Console.WriteLine($"[WARNING] Could not delete physical folder {folderPath}: {folderEx.Message}");
                                }
                            }
                            
                            Console.WriteLine($"[SUCCESS] Folder '{folderId}' permanently deleted by user {userId}");
                            return "200|FOLDER_PERMANENTLY_DELETED\n";
                        }
                        else
                        {
                            Console.WriteLine($"[DEBUG] No rows deleted during permanent folder deletion");
                            return "404|FOLDER_NOT_FOUND_IN_TRASH\n";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error in PermanentlyDeleteFolder: {ex.Message}");
                Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
                return "500|INTERNAL_ERROR\n";
            }
        }
    }
}
