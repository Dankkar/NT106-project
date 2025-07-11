using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

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
                    // Handle multiple requests in the same connection
                    while (client.Connected)
                    {
                        string request = await reader.ReadLineAsync();
                        if (request == null || string.IsNullOrWhiteSpace(request))
                        {
                            Console.WriteLine("Client gửi dữ liệu trống hoặc ngắt kết nối.");
                            break;
                        }

                        Console.WriteLine($"Nhận từ Client: {request}");

                        string response = await ProcessRequest(request, stream);
                        Console.WriteLine($"Gửi response: {response.Replace("\n", "\\n")}");
                        await writer.WriteLineAsync(response);
                        await writer.FlushAsync();
                        Console.WriteLine("Response đã được gửi và flush");
                        
                        // For single-request commands, break after processing
                        if (request.StartsWith("GET_FILES_BY_PASSWORD") || 
                            request.StartsWith("GET_FOLDERS_BY_PASSWORD") ||
                            request.StartsWith("SHARE_FILE") ||
                            request.StartsWith("SHARE_FOLDER"))
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xử lý client: {ex.Message}");
            }
            finally
            {
                try
                {
                    client.Close();
                    Console.WriteLine("Client đã ngắt kết nối.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi đóng kết nối: {ex.Message}");
                }
            }
        }
        static async Task<string> ProcessRequest(string request, NetworkStream stream)
        {
            //Console.WriteLine($"[DEBUG] ProcessRequest - Raw request: '{request}'");
            
            string[] parts = request.Split('|');
            //Console.WriteLine($"[DEBUG] ProcessRequest - Split into {parts.Length} parts");
            
            if (parts.Length == 0)
            {
                Console.WriteLine($"[ERROR] ProcessRequest - No parts found in request");
                return "400\n"; // Bad Request
            }

            string command = parts[0];
            //Console.WriteLine($"[DEBUG] ProcessRequest - Command: '{command}'");

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
                case "GET_USER_FILES_DETAILED":
                    if (parts.Length != 3) return "400\n";
                    return await GetUserFilesDetailed(parts[1], parts[2]);
                case "GET_USER_FOLDERS":
                    if (parts.Length != 3) return "400\n";
                    return await GetUserFolders(parts[1], parts[2]);
                case "GET_SHARED_FILES":
                    if (parts.Length != 2) return "400\n";
                    return await GetSharedFiles(parts[1]);
                case "GET_SHARED_FILES_DETAILED":
                    if (parts.Length != 2) return "400\n";
                    return await GetSharedFilesDetailed(parts[1]);
                case "GET_SHARED_FOLDERS":
                    if (parts.Length != 2) return "400\n";
                    return await GetSharedFolders(parts[1]);
                case "CREATE_FOLDER":
                    if (parts.Length != 4) return "400\n";
                    return await CreateFolder(parts[1], parts[2], parts[3]);
                case "DELETE_FILE":
                    if (parts.Length != 2) return "400\n";
                    return await DeleteFile(parts[1]);
                case "DELETE_FOLDER":
                    if (parts.Length != 3) return "400\n";
                    return await DeleteFolder(parts[1], parts[2]);
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
                case "ADD_FILE_SHARE_ENTRY_WITH_PERMISSION":
                    if (parts.Length != 5) return "400\n";
                    return await AddFileShareEntryWithPermission(parts[1], parts[2], parts[3], parts[4]);
                case "GET_FILE_PERMISSION_BY_SHARE_PASS":
                    if (parts.Length != 2) return "400\n";
                    return await GetFilePermissionBySharePass(parts[1]);
                case "GET_FOLDER_PERMISSION_BY_SHARE_PASS":
                    if (parts.Length != 2) return "400\n";
                    return await GetFolderPermissionBySharePass(parts[1]);
                case "GET_FOLDER_INFO_BY_SHARE_PASS":
                    if (parts.Length != 2) return "400\n";
                    return await GetFolderInfoBySharePass(parts[1]);
                case "ADD_FOLDER_SHARE_ENTRY_WITH_PERMISSION":
                    if (parts.Length != 5) return "400\n";
                    return await AddFolderShareEntryWithPermission(parts[1], parts[2], parts[3], parts[4]);
                case "ADD_FILES_IN_FOLDER_TO_SHARE":
                    if (parts.Length != 5) return "400\n";
                    return await AddFilesInFolderToShare(parts[1], parts[2], parts[3], parts[4]);
                case "ADD_FOLDER_AND_FILES_SHARE":
                    if (parts.Length != 5) return "400\n";
                    return await AddFolderAndFilesShare(parts[1], parts[2], parts[3], parts[4]);
                case "DOWNLOAD_FILE":
                    if (parts.Length != 3) return "400\n";
                    return await DownloadFile(parts[1], parts[2]);
                    
                // NEW: Share commands
                case "SHARE_FILE":
                    if (parts.Length != 3) return "400\n";
                    return await ShareFile(parts[1], parts[2]);
                    
                case "SHARE_FOLDER":
                    if (parts.Length != 3) return "400\n";
                    return await ShareFolder(parts[1], parts[2]);
                    
                case "GET_FILE_SHARE_PASSWORD":
                    if (parts.Length != 2) return "400\n";
                    return await GetFileSharePassword(parts[1]);
                    
                case "GET_FOLDER_SHARE_PASSWORD":
                    if (parts.Length != 2) return "400\n";
                    return await GetFolderSharePassword(parts[1]);
                    
                case "GET_FILES_BY_PASSWORD":
                    if (parts.Length != 2) return "400\n";
                    return await GetFilesByPassword(parts[1]);
                    
                case "GET_FOLDERS_BY_PASSWORD":
                    if (parts.Length != 2) return "400\n";
                    return await GetFoldersByPassword(parts[1]);
                    
                case "GET_FOLDER_CONTENTS":
                    if (parts.Length != 2) return "400\n";
                    return await GetFolderContents(parts[1]);
                    
                case "GET_FILES_IN_FOLDER":
                    if (parts.Length != 2) return "400\n";
                    return await GetFilesInFolder(parts[1]);
                    
                case "DEBUG_LIST_SHARED_FILES":
                    return await DebugListSharedFiles();
                    
                case "DEBUG_LIST_SHARED_FOLDERS":
                    return await DebugListSharedFolders();
                    
                case "DEBUG_LIST_ALL_SHARED":
                    return await DebugListAllShared();
                    
                case "DEBUG_CHECK_DATABASE":
                    return await DebugCheckDatabase();
                    
                case "DEBUG_CHECK_USER_SHARES":
                    if (parts.Length != 2) return "400\n";
                    return await DebugCheckUserShares(parts[1]);
                    
                case "ADD_SHARED_FILE":
                    if (parts.Length != 6) return "400\n";
                    return await AddSharedFile(parts[1], parts[2], parts[3], parts[4], parts[5]);
                    
                case "ADD_SHARED_FOLDER":
                    if (parts.Length != 4) return "400\n";
                    return await AddSharedFolder(parts[1], parts[2], parts[3]);
                    
                case "GET_FILE_ID_BY_NAME":
                    if (parts.Length != 2) return "400\n";
                    return await GetFileIdByName(parts[1]);
                    
                case "GET_FOLDER_ID_BY_NAME":
                    if (parts.Length != 2) return "400\n";
                    return await GetFolderIdByName(parts[1]);
                    
                case "ADD_FOLDER_SHARE_ENTRY":
                    if (parts.Length != 4) return "400\n";
                    return await AddFolderShareEntry(parts[1], parts[2], parts[3]);
                    
                case "GET_SHARED_FOLDER_CONTENTS":
                    if (parts.Length != 3) return "400\n";
                    return await GetSharedFolderContents(parts[1], parts[2]);
                case "GET_TRASH_FILES":
                    //Console.WriteLine($"[DEBUG] ProcessRequest - GET_TRASH_FILES case reached");
                    if (parts.Length != 2) 
                    {
                        Console.WriteLine($"[ERROR] ProcessRequest - GET_TRASH_FILES: Invalid parts count: {parts.Length}");
                        return "400\n";
                    }
                    //Console.WriteLine($"[DEBUG] ProcessRequest - GET_TRASH_FILES: Calling GetTrashFiles with userId: {parts[1]}");
                    string trashResult = await GetTrashFiles(parts[1]);
                    //Console.WriteLine($"[DEBUG] ProcessRequest - GET_TRASH_FILES: GetTrashFiles returned: '{trashResult.Substring(0, Math.Min(100, trashResult.Length))}...'");
                    return trashResult;
                case "RESTORE_FILE":
                    if (parts.Length != 3) return "400\n";
                    return await RestoreFile(parts[1], parts[2]);
                case "PERMANENTLY_DELETE_FILE":
                    if (parts.Length != 3) return "400\n";
                    return await PermanentlyDeleteFile(parts[1], parts[2]);
                case "GET_TRASH_FOLDERS":
                    if (parts.Length != 2) return "400\n";
                    return await GetTrashFolders(parts[1]);
                case "RESTORE_FOLDER":
                    if (parts.Length != 3) return "400\n";
                    return await RestoreFolder(parts[1], parts[2]);
                case "PERMANENTLY_DELETE_FOLDER":
                    if (parts.Length != 3) return "400\n";
                    return await PermanentlyDeleteFolder(parts[1], parts[2]);
                    
                // HEAD branch features
                case "GET_USER_STORAGE":
                    if (parts.Length != 2) return "400\n";
                    return await GetUserStorage(parts[1]);
                case "DOWNLOAD_FILE_WITH_STREAM":
                    if (parts.Length != 3) return "400\n";
                    return await DownloadFile(parts[1], parts[2]);
                case "HEALTH_CHECK":
                    return "200|HEALTHY\n";
                case "DELETE_FILE_WITH_USER":
                    if (parts.Length != 3) return "400\n";
                    return await DeleteFile(parts[1]);
                    
                // NEW: Remove shared items commands
                case "REMOVE_SHARED_FILE":
                    if (parts.Length != 3) return "400\n";
                    return await RemoveSharedFile(parts[1], parts[2]);
                    
                case "REMOVE_SHARED_FOLDER":
                    if (parts.Length != 3) return "400\n";
                    return await RemoveSharedFolder(parts[1], parts[2]);
                    
                // CLIENT-SIDE RE-ENCRYPTION: Upload shared version
                case "UPLOAD_SHARED_VERSION":
                    if (parts.Length != 7) return "400\n";
                    return await UploadSharedVersion(parts[1], parts[2], int.Parse(parts[3]), parts[4], parts[5], parts[6], stream);
                    
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
                    string query = @"
                        SELECT f.file_id, f.file_name, f.file_type, f.file_size, f.upload_at, u.username as owner_name, f.file_path
                        FROM files_share fs
                        JOIN files f ON fs.file_id = f.file_id
                        JOIN users u ON f.owner_id = u.user_id
                        WHERE fs.user_id = @user_id 
                        AND f.status = 'ACTIVE'
                        AND f.folder_id IS NULL";
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@user_id", int.Parse(userId));
                        var files = new List<string>();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string id = reader["file_id"].ToString();
                                string name = reader["file_name"].ToString();
                                string type = reader["file_type"].ToString();
                                string size = reader["file_size"].ToString();
                                string uploadAt = reader["upload_at"].ToString();
                                string owner = reader["owner_name"].ToString();
                                string path = reader["file_path"].ToString();
                                files.Add($"{id}:{name}:{type}:{size}:{uploadAt}:{owner}:{path}");
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
                        SELECT f.file_path, f.file_type, f.file_size
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
                                long fileSize = Convert.ToInt64(reader["file_size"] ?? 0);
                                
                                // Clean file path - ensure it's safe
                                filePath = CleanFilePath(filePath);
                                fileType = CleanString(fileType);
                                
                                return $"200|{filePath}|{fileType}|{fileSize}\n";
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
                    string query = "SELECT file_id, owner_id FROM files WHERE share_pass = @share_pass AND is_shared = 1";
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@share_pass", sharePass);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                int fileId = Convert.ToInt32(reader["file_id"]);
                                int ownerId = Convert.ToInt32(reader["owner_id"]);
                                //Console.WriteLine($"[DEBUG] GetFileInfoBySharePass found: fileId={fileId}, ownerId={ownerId}");
                                return $"200|{fileId}|{ownerId}\n";
                            }
                            else
                            {
                                //Console.WriteLine($"[DEBUG] GetFileInfoBySharePass: No file found with share_pass={sharePass}");
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
                //Console.WriteLine($"[DEBUG] AddFileShareEntry called with: fileId={fileId}, userId={userId}, sharePass={sharePass}");
                
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
                            //Console.WriteLine($"[DEBUG] File share entry already exists");
                            return "200|ALREADY_SHARED\n";
                        }
                    }
                    
                    // Insert new share entry with permission field
                    string insertQuery = "INSERT INTO files_share (file_id, user_id, share_pass, permission) VALUES (@file_id, @user_id, @share_pass, @permission)";
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@file_id", int.Parse(fileId));
                        cmd.Parameters.AddWithValue("@user_id", int.Parse(userId));
                        cmd.Parameters.AddWithValue("@share_pass", sharePass);
                        cmd.Parameters.AddWithValue("@permission", "read"); // Default permission
                        
                        await cmd.ExecuteNonQueryAsync();
                        //Console.WriteLine($"[DEBUG] File share entry added successfully");
                        return "200|SHARE_ADDED\n";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddFileShareEntry: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static async Task<string> AddFileShareEntryWithPermission(string fileId, string userId, string sharePass, string permission)
        {
            try
            {
                //Console.WriteLine($"[DEBUG] AddFileShareEntryWithPermission called with: fileId={fileId}, userId={userId}, sharePass={sharePass}, permission={permission}");
                
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
                            //Console.WriteLine($"[DEBUG] File share entry already exists");
                            return "200|ALREADY_SHARED\n";
                        }
                    }
                    
                    // Insert new share entry with permission field
                    string insertQuery = "INSERT INTO files_share (file_id, user_id, share_pass, permission) VALUES (@file_id, @user_id, @share_pass, @permission)";
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@file_id", int.Parse(fileId));
                        cmd.Parameters.AddWithValue("@user_id", int.Parse(userId));
                        cmd.Parameters.AddWithValue("@share_pass", sharePass);
                        cmd.Parameters.AddWithValue("@permission", permission);
                        
                        await cmd.ExecuteNonQueryAsync();
                        //Console.WriteLine($"[DEBUG] File share entry added successfully with permission: {permission}");
                        return "200|SHARE_ADDED_WITH_PERMISSION\n";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddFileShareEntryWithPermission: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static async Task<string> GetFilePermissionBySharePass(string sharePass)
        {
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    
                    string query = @"
                        SELECT permission 
                        FROM files_share 
                        WHERE share_pass = @sharePass";
                    
                    using (var command = new System.Data.SQLite.SQLiteCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@sharePass", sharePass);
                        
                        var result = await command.ExecuteScalarAsync();
                        if (result != null)
                        {
                            return $"200|{result}\n";
                        }
                        else
                        {
                            return "404|File not shared or permission not found\n";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting file permission by share pass: {ex.Message}");
                return "500|Internal server error\n";
            }
        }

        private static async Task<string> GetFolderPermissionBySharePass(string sharePass)
        {
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    
                    string query = @"
                        SELECT permission 
                        FROM folder_shares 
                        WHERE share_pass = @sharePass";
                    
                    using (var command = new System.Data.SQLite.SQLiteCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@sharePass", sharePass);
                        
                        var result = await command.ExecuteScalarAsync();
                        if (result != null)
                        {
                            return $"200|{result}\n";
                        }
                        else
                        {
                            return "404|Folder not shared or permission not found\n";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting folder permission by share pass: {ex.Message}");
                return "500|Internal server error\n";
            }
        }

        private static async Task<string> GetFolderInfoBySharePass(string sharePass)
        {
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    
                    string query = @"
                        SELECT f.folder_id, f.folder_name, f.created_at, u.username as owner_name
                        FROM folders f
                        JOIN users u ON f.owner_id = u.user_id
                        WHERE f.share_pass = @sharePass";
                    
                    using (var command = new System.Data.SQLite.SQLiteCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@sharePass", sharePass);
                        
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                string folderInfo = $"{reader["folder_id"]}:{reader["folder_name"]}:{reader["created_at"]}:{reader["owner_name"]}";
                                return $"200|{folderInfo}\n";
                            }
                            else
                            {
                                return "404|Folder not found with this password\n";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting folder info by share pass: {ex.Message}");
                return "500|Internal server error\n";
            }
        }



        private static async Task<string> DownloadFile(string fileId, string userId)
        {
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    
                    // CLIENT-SIDE RE-ENCRYPTION: Check if user owns this file OR has access to shared file
                    string query = @"
                        SELECT f.file_path, f.file_size, f.owner_id, f.shared_file_path, f.share_pass
                        FROM files f 
                        WHERE f.file_id = @fileId 
                        AND f.status = 'ACTIVE'
                        AND (
                            f.owner_id = @userId
                            OR f.file_id IN (
                                SELECT fs.file_id FROM files_share fs WHERE fs.user_id = @userId
                            )
                        )";
                        
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@fileId", int.Parse(fileId));
                        cmd.Parameters.AddWithValue("@userId", int.Parse(userId));
                        
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (!await reader.ReadAsync())
                                return "404|FILE_NOT_FOUND_OR_NO_ACCESS\n";
                                
                            string filePath = reader["file_path"].ToString();
                            long fileSize = Convert.ToInt64(reader["file_size"]);
                            int ownerId = Convert.ToInt32(reader["owner_id"]);
                            string sharedFilePath = reader["shared_file_path"]?.ToString();
                            string sharePass = reader["share_pass"]?.ToString();
                            
                            bool isOwner = ownerId == int.Parse(userId);
                            bool isSharedFile = !isOwner && !string.IsNullOrEmpty(sharedFilePath);
                            
                            //Console.WriteLine($"[DEBUG] DownloadFile: fileId={fileId}, userId={userId}, ownerId={ownerId}, isOwner={isOwner}, isSharedFile={isSharedFile}");
                            
                            // Determine which file to serve
                            string fileToServe;
                            if (isSharedFile)
                            {
                                // User is accessing shared file → serve shared version (encrypted with share_pass)
                                fileToServe = sharedFilePath;
                                //Console.WriteLine($"[DEBUG] Serving shared version: {sharedFilePath}");
                            }
                            else
                            {
                                // User is owner → serve original version (encrypted with owner password)
                                fileToServe = filePath;
                                //Console.WriteLine($"[DEBUG] Serving original version: {filePath}");
                            }
                            
                            if (fileSize > 10 * 1024 * 1024) // 10MB
                                return "413|FILE_TOO_LARGE\n";
                                
                            string projectRoot = FindProjectRoot();
                            string absPath = Path.Combine(projectRoot, fileToServe.Replace("/", Path.DirectorySeparatorChar.ToString()).Replace("\\", Path.DirectorySeparatorChar.ToString()));
                            
                            if (!File.Exists(absPath))
                            {
                                Console.WriteLine($"[ERROR] File not found: {absPath}");
                                return $"404|FILE_NOT_FOUND ({absPath})\n";
                            }
                                
                            byte[] fileBytes = File.ReadAllBytes(absPath);
                            string base64 = Convert.ToBase64String(fileBytes);
                            
                            // Return file data with metadata about encryption type
                            string encryptionType = isSharedFile ? "SHARED" : "OWNER";
                            return $"200|{base64}|{encryptionType}|{sharePass ?? ""}\n";
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

        private static async Task<string> GetUserFilesDetailed(string userId, string folderId)
        {
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    bool isRoot = folderId == "null";
                    
                    // Only show user's own files, not files from shared folders
                    string query = @"
                        SELECT f.file_id, f.file_name, f.file_type, f.file_size, f.upload_at, f.file_path, u.username as owner_name
                        FROM files f
                        JOIN users u ON f.owner_id = u.user_id
                        WHERE f.status = 'ACTIVE'
                        AND f.owner_id = @userId 
                        AND (
                            (@isRoot = 1 AND f.folder_id IS NULL)
                            OR
                            (@isRoot = 0 AND f.folder_id = @folderId)
                        )
                        ORDER BY f.file_name";
                    
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", int.Parse(userId));
                        cmd.Parameters.AddWithValue("@isRoot", isRoot ? 1 : 0);
                        cmd.Parameters.AddWithValue("@folderId", isRoot ? (object)DBNull.Value : int.Parse(folderId));
                        
                        var files = new List<string>();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string fileInfo = $"{reader["file_id"]}:{reader["file_name"]}:{reader["file_type"]}:{reader["file_size"]}:{reader["upload_at"]}:{reader["owner_name"]}:{reader["file_path"]}";
                                files.Add(fileInfo);
                            }
                        }
                        
                        string result;
                        if (files.Count == 0)
                        {
                            result = "200|NO_FILES\n";
                        }
                        else
                        {
                            result = $"200|{string.Join(";", files)}\n";
                        }
                        //Console.WriteLine($"[DEBUG][GetUserFilesDetailed] Response: {result.Replace("\n", "\\n")}");
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUserFilesDetailed: {ex.Message}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static async Task<string> GetUserFolders(string userId, string folderId)
        {
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    bool isRoot = folderId == "null";
                    
                    // Only show user's own folders, not shared folders
                    string query = @"
                        SELECT f.folder_id, f.folder_name, f.created_at, u.username as owner_name
                        FROM folders f
                        JOIN users u ON f.owner_id = u.user_id
                        WHERE f.status = 'ACTIVE'
                        AND f.owner_id = @userId 
                        AND (
                            (@isRoot = 1 AND f.parent_folder_id IS NULL)
                            OR
                            (@isRoot = 0 AND f.parent_folder_id = @folderId)
                        )
                        ORDER BY f.folder_name";
                    
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", int.Parse(userId));
                        cmd.Parameters.AddWithValue("@isRoot", isRoot ? 1 : 0);
                        cmd.Parameters.AddWithValue("@folderId", isRoot ? (object)DBNull.Value : int.Parse(folderId));
                        
                        var folders = new List<string>();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string folderInfo = $"{reader["folder_id"]}:{reader["folder_name"]}:{reader["created_at"]}:{reader["owner_name"]}";
                                folders.Add(folderInfo);
                            }
                        }
                        
                        string result;
                        if (folders.Count == 0)
                        {
                            result = "200|NO_FOLDERS\n";
                        }
                        else
                        {
                            result = $"200|{string.Join(";", folders)}\n";
                        }
                        //Console.WriteLine($"[DEBUG][GetUserFolders] Response: {result.Replace("\n", "\\n")}");
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUserFolders: {ex.Message}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static async Task<string> GetSharedFilesDetailed(string userId)
        {
            try
            {
                //Console.WriteLine($"[DEBUG][GetSharedFilesDetailed] Called with userId: {userId}");
                
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    
                    // Only get shared files that are NOT in any shared folder
                    // This maintains folder structure by showing only root-level shared files
                    string query = @"
                        SELECT f.file_id, f.file_name, f.file_type, f.file_size, f.upload_at, u.username as owner_name, f.file_path
                        FROM files_share fs 
                        JOIN files f ON fs.file_id = f.file_id 
                        JOIN users u ON f.owner_id = u.user_id
                        WHERE fs.user_id = @userId 
                        AND f.status = 'ACTIVE'
                        AND (
                            f.folder_id IS NULL 
                            OR f.folder_id NOT IN (
                                SELECT folder_id FROM folder_shares WHERE shared_with_user_id = @userId
                            )
                        )
                        ORDER BY f.file_name";
                    
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", int.Parse(userId));
                        
                        var files = new List<string>();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string fileInfo = $"{reader["file_id"]}:{reader["file_name"]}:{reader["file_type"]}:{reader["file_size"]}:{reader["upload_at"]}:{reader["owner_name"]} (Shared):{reader["file_path"]}:shared";
                                files.Add(fileInfo);
                                //Console.WriteLine($"[DEBUG][GetSharedFilesDetailed] Found file: {fileInfo}");
                            }
                        }
                        
                        if (files.Count == 0)
                        {
                            string noFilesResult = "200|NO_SHARED_FILES\n";
                            //Console.WriteLine($"[DEBUG][GetSharedFilesDetailed] No shared files found, returning: {noFilesResult.Replace("\n", "\\n")}");
                            return noFilesResult;
                        }
                        
                        string filesResult = $"200|{string.Join(";", files)}\n";
                        //Console.WriteLine($"[DEBUG][GetSharedFilesDetailed] Response: {filesResult.Replace("\n", "\\n")}");
                        return filesResult;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetSharedFilesDetailed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static async Task<string> GetSharedFolders(string userId)
        {
            try
            {
                //Console.WriteLine($"[DEBUG][GetSharedFolders] Called with userId: {userId}");
                
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"
                        SELECT f.folder_id, f.folder_name, f.created_at, u.username as owner_name
                        FROM folder_shares fs 
                        JOIN folders f ON fs.folder_id = f.folder_id 
                        JOIN users u ON f.owner_id = u.user_id
                        WHERE fs.shared_with_user_id = @userId 
                        AND f.status = 'ACTIVE'
                        ORDER BY f.folder_name";
                    
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", int.Parse(userId));
                        
                        var folders = new List<string>();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string folderInfo = $"{reader["folder_id"]}:{reader["folder_name"]}:{reader["created_at"]}:{reader["owner_name"]} (Shared):shared";
                                folders.Add(folderInfo);
                                //Console.WriteLine($"[DEBUG][GetSharedFolders] Found folder: {folderInfo}");
                            }
                        }
                        
                        if (folders.Count == 0)
                        {
                            string noFoldersResult = "200|NO_SHARED_FOLDERS\n";
                            //Console.WriteLine($"[DEBUG][GetSharedFolders] No shared folders found, returning: {noFoldersResult.Replace("\n", "\\n")}");
                            return noFoldersResult;
                        }
                        
                        string foldersResult = $"200|{string.Join(";", folders)}\n";
                        //Console.WriteLine($"[DEBUG][GetSharedFolders] Response: {foldersResult.Replace("\n", "\\n")}");
                        return foldersResult;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetSharedFolders: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static async Task<string> CreateFolder(string userId, string folderName, string parentFolderId)
        {
            try
            {
                //Console.WriteLine($"[DEBUG] CreateFolder called: userId={userId}, folderName={folderName}, parentFolderId={parentFolderId}");
                
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    
                    // Generate folder path based on parent folder
                    string folderPath;
                    string physicalFolderPath;
                    
                    if (parentFolderId == "null" || string.IsNullOrEmpty(parentFolderId))
                    {
                        // Root level folder
                        folderPath = $"uploads\\{userId}\\{folderName}";
                        physicalFolderPath = Path.Combine(GetSharedUploadsPath(), userId, folderName);
                    }
                    else
                    {
                        // Get parent folder path from database
                        string parentQuery = "SELECT folder_path FROM folders WHERE folder_id = @parentId";
                        using (var parentCmd = new System.Data.SQLite.SQLiteCommand(parentQuery, conn))
                        {
                            parentCmd.Parameters.AddWithValue("@parentId", int.Parse(parentFolderId));
                            var parentPath = await parentCmd.ExecuteScalarAsync() as string;
                            
                            if (string.IsNullOrEmpty(parentPath))
                            {
                                Console.WriteLine($"[ERROR] Parent folder not found: {parentFolderId}");
                                return "404|PARENT_FOLDER_NOT_FOUND\n";
                            }
                            
                            folderPath = Path.Combine(parentPath, folderName).Replace('/', '\\');
                            physicalFolderPath = Path.Combine(GetSharedUploadsPath(), parentPath.Replace("uploads\\", ""), folderName);
                        }
                    }
                    
                    //Console.WriteLine($"[DEBUG] Generated folder_path: {folderPath}");
                    //Console.WriteLine($"[DEBUG] Physical folder path: {physicalFolderPath}");
                    
                    // Create physical directory
                    try
                    {
                        if (!Directory.Exists(physicalFolderPath))
                        {
                            Directory.CreateDirectory(physicalFolderPath);
                            //Console.WriteLine($"[DEBUG] Physical directory created: {physicalFolderPath}");
                        }
                        else
                        {
                            //Console.WriteLine($"[DEBUG] Physical directory already exists: {physicalFolderPath}");
                        }
                    }
                    catch (Exception dirEx)
                    {
                        Console.WriteLine($"[ERROR] Failed to create physical directory: {dirEx.Message}");
                        return "500|FAILED_TO_CREATE_DIRECTORY\n";
                    }
                    
                    // Insert into database
                    string query = @"
                        INSERT INTO folders (folder_name, owner_id, parent_folder_id, folder_path, created_at, status) 
                        VALUES (@folderName, @ownerId, @parentFolderId, @folderPath, datetime('now'), 'ACTIVE')";
                    
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@folderName", folderName);
                        cmd.Parameters.AddWithValue("@ownerId", int.Parse(userId));
                        cmd.Parameters.AddWithValue("@parentFolderId", parentFolderId == "null" ? DBNull.Value : (object)int.Parse(parentFolderId));
                        cmd.Parameters.AddWithValue("@folderPath", folderPath);
                        
                        //Console.WriteLine($"[DEBUG] Executing CreateFolder query with params: folderName={folderName}, ownerId={userId}, parentFolderId={parentFolderId}, folderPath={folderPath}");
                        
                        await cmd.ExecuteNonQueryAsync();
                        
                        //Console.WriteLine($"[DEBUG] Folder created successfully: {folderName}");
                        return "200|FOLDER_CREATED\n";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error in CreateFolder: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static async Task<string> DeleteFile(string fileId)
        {
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    string query = "UPDATE files SET status = 'TRASH', deleted_at = CURRENT_TIMESTAMP WHERE file_id = @fileId";
                    
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@fileId", int.Parse(fileId));
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        
                        if (rowsAffected > 0)
                        {
                            return "200|FILE_DELETED\n";
                        }
                        else
                        {
                            //Console.WriteLine($"[DEBUG] No rows updated - file might not have status='ACTIVE'");
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
                //Console.WriteLine($"[DEBUG] GetTrashFiles - Called with userId: {userId}");
                
                // Validate userId
                if (!int.TryParse(userId, out int userIdInt))
                {
                    Console.WriteLine($"[ERROR] GetTrashFiles - Invalid userId format: {userId}");
                    return "400|INVALID_USER_ID\n";
                }
                
                //Console.WriteLine($"[DEBUG] GetTrashFiles - Parsed userId as: {userIdInt}");
                
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    //Console.WriteLine($"[DEBUG] GetTrashFiles - Database connection opened");
                    
                    string query = "SELECT file_name, deleted_at FROM files WHERE owner_id = @owner_id AND status = 'TRASH' ORDER BY deleted_at DESC";
                    //Console.WriteLine($"[DEBUG] GetTrashFiles - Executing query: {query}");
                    
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@owner_id", userIdInt);
                        //Console.WriteLine($"[DEBUG] GetTrashFiles - Query parameter @owner_id = {userIdInt}");
                        
                        var files = new List<string>();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            int rowCount = 0;
                            while (await reader.ReadAsync())
                            {
                                rowCount++;
                                string fileName = reader["file_name"]?.ToString() ?? "";
                                string deletedAt = reader["deleted_at"]?.ToString() ?? "";
                                
                                //Console.WriteLine($"[DEBUG] GetTrashFiles - Row {rowCount}: fileName='{fileName}', deletedAt='{deletedAt}'");
                                
                                // Clean data before sending to client
                                fileName = CleanString(fileName);
                                deletedAt = CleanString(deletedAt);
                                
                                //Console.WriteLine($"[DEBUG] GetTrashFiles - Cleaned Row {rowCount}: fileName='{fileName}', deletedAt='{deletedAt}'");
                                
                                if (!string.IsNullOrWhiteSpace(fileName))
                                {
                                files.Add($"{fileName}:{deletedAt}");
                                    //Console.WriteLine($"[DEBUG] GetTrashFiles - Added to list: '{fileName}:{deletedAt}'");
                                }
                                else
                                {
                                    Console.WriteLine($"[WARNING] GetTrashFiles - Skipped row {rowCount} due to empty fileName");
                                }
                            }
                            
                            //Console.WriteLine($"[DEBUG] GetTrashFiles - Total rows found: {rowCount}");
                            //Console.WriteLine($"[DEBUG] GetTrashFiles - Valid files after cleaning: {files.Count}");
                        }
                        
                        if (files.Count == 0)
                        {
                            //Console.WriteLine($"[DEBUG] GetTrashFiles - No trash files found, returning NO_TRASH_FILES");
                            return "200|NO_TRASH_FILES\n";
                        }
                        
                        string result = $"200|{string.Join(";", files)}\n";
                        //Console.WriteLine($"[DEBUG] GetTrashFiles - Final result length: {result.Length}");
                        //Console.WriteLine($"[DEBUG] GetTrashFiles - Result starts with: '{result.Substring(0, Math.Min(50, result.Length))}'");
                        //Console.WriteLine($"[DEBUG] GetTrashFiles - Result ends with: '{result.Substring(Math.Max(0, result.Length - 20))}'");
                        //Console.WriteLine($"[DEBUG] GetTrashFiles - Files included: {files.Count}");
                        
                        // Validate result format
                        if (!result.StartsWith("200|"))
                        {
                            Console.WriteLine($"[ERROR] GetTrashFiles - Result doesn't start with '200|': '{result.Substring(0, Math.Min(20, result.Length))}'");
                        }
                        
                        // Additional validation
                        result = ValidateResponse(result);
                        //Console.WriteLine($"[DEBUG] GetTrashFiles - Final validated result: '{result.Substring(0, Math.Min(100, result.Length))}...'");
                        
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
                
                //Console.WriteLine($"[DEBUG] CleanString - Input: '{input}' -> Output: '{cleaned}'");
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

                //Console.WriteLine($"[DEBUG] ValidateResponse - Response validated successfully");
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
                //Console.WriteLine($"[DEBUG] RESTORE_FILE request received: fileName={fileName}, userId={userId}");
                
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
                        //Console.WriteLine($"[DEBUG] Trash files found with name '{fileName}' for user {userId}: {fileCount}");
                        
                        if (fileCount == 0)
                        {
                            //Console.WriteLine($"[DEBUG] File '{fileName}' not found in trash for user {userId}");
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
                            //Console.WriteLine($"[DEBUG] File '{fileName}' belongs to folder_id: {fileFolderId}");
                        }
                        else
                        {
                            //Console.WriteLine($"[DEBUG] File '{fileName}' is a standalone file (folder_id = NULL)");
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
                                //Console.WriteLine($"[DEBUG] Parent folder {fileFolderId.Value} is in trash, file will be moved to root level");
                            }
                            else
                            {
                                //Console.WriteLine($"[DEBUG] Parent folder {fileFolderId.Value} status: {folderStatus}");
                            }
                        }
                    }
                    
                    // Restore file - if parent folder is deleted, move file to root level (folder_id = NULL)
                    string updateQuery;
                    if (fileWasInDeletedFolder)
                    {
                        updateQuery = "UPDATE files SET status = 'ACTIVE', deleted_at = NULL, folder_id = NULL WHERE file_name = @file_name AND owner_id = @owner_id AND status = 'TRASH'";
                        //Console.WriteLine($"[DEBUG] Moving file to root level due to deleted parent folder");
                    }
                    else
                    {
                        updateQuery = "UPDATE files SET status = 'ACTIVE', deleted_at = NULL WHERE file_name = @file_name AND owner_id = @owner_id AND status = 'TRASH'";
                        //Console.WriteLine($"[DEBUG] Restoring file in its original folder");
                    }
                    
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@file_name", fileName);
                        cmd.Parameters.AddWithValue("@owner_id", int.Parse(userId));
                        
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        //Console.WriteLine($"[DEBUG] Rows affected by FILE RESTORE: {rowsAffected}");
                        
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
                            //Console.WriteLine($"[DEBUG] No rows updated during restore");
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
                //Console.WriteLine($"[DEBUG] PERMANENTLY_DELETE_FILE request received: fileName={fileName}, userId={userId}");
                
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
                            //Console.WriteLine($"[DEBUG] File '{fileName}' not found in trash for user {userId}");
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
                        //Console.WriteLine($"[DEBUG] Rows affected by PERMANENT DELETE: {rowsAffected}");
                        
                        if (rowsAffected > 0)
                        {
                            // Also delete physical file if it exists
                            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                            {
                                try
                                {
                                    File.Delete(filePath);
                                    //Console.WriteLine($"[DEBUG] Physical file deleted: {filePath}");
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
                            //Console.WriteLine($"[DEBUG] No rows deleted during permanent deletion");
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
                //Console.WriteLine($"[DEBUG] DELETE_FOLDER request received: folderId={folderId}, userId={userId}");
                
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
                        //Console.WriteLine($"[DEBUG] Folders found with id '{folderId}' for user {userId}: {folderCount}");
                        
                        if (folderCount == 0)
                        {
                            //Console.WriteLine($"[DEBUG] Folder '{folderId}' not found for user {userId}");
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
                        //Console.WriteLine($"[DEBUG] Rows affected by FOLDER DELETE: {rowsAffected}");
                        
                        if (rowsAffected > 0)
                        {
                            // Also move all files in this folder to trash
                            string moveFilesQuery = "UPDATE files SET status = 'TRASH', deleted_at = datetime('now') WHERE folder_id = @folder_id AND status = 'ACTIVE'";
                            using (var moveFilesCmd = new System.Data.SQLite.SQLiteCommand(moveFilesQuery, conn))
                            {
                                moveFilesCmd.Parameters.AddWithValue("@folder_id", int.Parse(folderId));
                                int filesAffected = await moveFilesCmd.ExecuteNonQueryAsync();
                                //Console.WriteLine($"[DEBUG] Files moved to trash: {filesAffected}");
                            }
                            
                            Console.WriteLine($"[SUCCESS] Folder '{folderId}' moved to trash by user {userId}");
                            return "200|FOLDER_DELETED\n";
                        }
                        else
                        {
                            //Console.WriteLine($"[DEBUG] No rows updated during folder deletion");
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
                //Console.WriteLine($"[DEBUG] GetTrashFolders - Called with userId: {userId}");
                
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
                            //Console.WriteLine($"[DEBUG] GetTrashFolders - No trash folders found, returning NO_TRASH_FOLDERS");
                            return "200|NO_TRASH_FOLDERS\n";
                        }
                        
                        string result = $"200|{string.Join(";", folders)}\n";
                        result = ValidateResponse(result);
                        
                        //Console.WriteLine($"[DEBUG] GetTrashFolders - Final result: '{result.Substring(0, Math.Min(100, result.Length))}...'");
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
                //Console.WriteLine($"[DEBUG] RESTORE_FOLDER request received: folderId={folderId}, userId={userId}");
                
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
                        //Console.WriteLine($"[DEBUG] Trash folders found with id '{folderId}' for user {userId}: {folderCount}");
                        
                        if (folderCount == 0)
                        {
                            //Console.WriteLine($"[DEBUG] Folder '{folderId}' not found in trash for user {userId}");
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
                        //Console.WriteLine($"[DEBUG] Rows affected by FOLDER RESTORE: {rowsAffected}");
                        
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
                                
                                //Console.WriteLine($"[DEBUG] Folder contains {totalFiles} total files");
                                //Console.WriteLine($"[DEBUG] Files already active (restored individually): {alreadyActiveFiles}");
                                //Console.WriteLine($"[DEBUG] Files restored with folder: {filesRestored}");
                                //Console.WriteLine($"[DEBUG] Final result: All {totalFiles} files are now active");
                                
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
                            //Console.WriteLine($"[DEBUG] No rows updated during folder restore");
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
                //Console.WriteLine($"[DEBUG] PERMANENTLY_DELETE_FOLDER request received: folderId={folderId}, userId={userId}");
                
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
                            //Console.WriteLine($"[DEBUG] Folder '{folderId}' not found in trash for user {userId}");
                            return "404|FOLDER_NOT_FOUND_IN_TRASH\n";
                        }
                    }
                    
                    // Delete all files in this folder first
                    string deleteFilesQuery = "DELETE FROM files WHERE folder_id = @folder_id AND status = 'TRASH'";
                    using (var deleteFilesCmd = new System.Data.SQLite.SQLiteCommand(deleteFilesQuery, conn))
                    {
                        deleteFilesCmd.Parameters.AddWithValue("@folder_id", int.Parse(folderId));
                        int filesDeleted = await deleteFilesCmd.ExecuteNonQueryAsync();
                        //Console.WriteLine($"[DEBUG] Files permanently deleted: {filesDeleted}");
                    }
                    
                    // Delete folder record from database
                    string deleteFolderQuery = "DELETE FROM folders WHERE folder_id = @folder_id AND owner_id = @owner_id AND status = 'TRASH'";
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(deleteFolderQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@folder_id", int.Parse(folderId));
                        cmd.Parameters.AddWithValue("@owner_id", int.Parse(userId));
                        
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        //Console.WriteLine($"[DEBUG] Rows affected by PERMANENT FOLDER DELETE: {rowsAffected}");
                        
                        if (rowsAffected > 0)
                        {
                            // Also delete physical folder if it exists
                            if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
                            {
                                try
                                {
                                    Directory.Delete(folderPath, true); // Delete recursively
                                    //Console.WriteLine($"[DEBUG] Physical folder deleted: {folderPath}");
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
                            //Console.WriteLine($"[DEBUG] No rows deleted during permanent folder deletion");
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

        private static string FindProjectRoot()
        {
            string currentDir = Directory.GetCurrentDirectory();
            //Console.WriteLine($"[DEBUG] FindProjectRoot: Starting from {currentDir}");
            
            // Navigate up until we find the project root (where test.db and uploads folder are)
            while (currentDir != null)
            {
                //Console.WriteLine($"[DEBUG] FindProjectRoot: Checking {currentDir}");
                
                // Check if this is the project root by looking for test.db and uploads folder
                string testDbPath = Path.Combine(currentDir, "test.db");
                string uploadsPath = Path.Combine(currentDir, "uploads");
                
                if (File.Exists(testDbPath) && Directory.Exists(uploadsPath))
                {
                    //Console.WriteLine($"[DEBUG] FindProjectRoot: Found project root at {currentDir}");
                    return currentDir;
                }
                
                string parentDir = Directory.GetParent(currentDir)?.FullName;
                if (parentDir == null || parentDir == currentDir)
                {
                    break;
                }
                currentDir = parentDir;
            }
            
            // Fallback: try to find by going up from current directory
            currentDir = Directory.GetCurrentDirectory();
            while (currentDir != null)
            {
                string parentDir = Directory.GetParent(currentDir)?.FullName;
                if (parentDir == null || parentDir == currentDir)
                {
                    break;
                }
                currentDir = parentDir;
                
                string testDbPath = Path.Combine(currentDir, "test.db");
                if (File.Exists(testDbPath))
                {
                    //Console.WriteLine($"[DEBUG] FindProjectRoot: Found project root (fallback) at {currentDir}");
                    return currentDir;
                }
            }
            
            //Console.WriteLine($"[DEBUG] FindProjectRoot: Could not find project root, using current directory: {Directory.GetCurrentDirectory()}");
            return Directory.GetCurrentDirectory();
        }

        // NEW: Share functionality methods
        private static async Task<string> ShareFile(string fileId, string permission)
        {
            try
            {
                using (var connection = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await connection.OpenAsync();
                    
                    // Generate hash-based password from file ID
                    string sharePassword = GenerateHashPassword(fileId);
                    //Console.WriteLine($"[DEBUG] Generated share password for file {fileId}: {sharePassword}");
                    
                    // Update file to set is_shared = 1 and share_pass (no permission column in files table)
                    string updateQuery = @"
                        UPDATE files 
                        SET is_shared = 1, share_pass = @sharePass 
                        WHERE file_id = @fileId";
                    
                    using (var command = new System.Data.SQLite.SQLiteCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@sharePass", sharePassword);
                        command.Parameters.AddWithValue("@fileId", fileId);
                        
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            //Console.WriteLine($"[DEBUG] Successfully updated file {fileId} with share password: {sharePassword}");
                            
                            // Store the default permission info for this shared file in a metadata table or use default
                            // Permission will be handled when users access the file via AddFileShareEntryWithPermission
                            //Console.WriteLine($"[DEBUG] File {fileId} shared with default permission: {permission}");
                            
                            return $"200|{sharePassword}\n";
                        }
                        else
                        {
                            //Console.WriteLine($"[DEBUG] File {fileId} not found for sharing");
                            return "404|File not found\n";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sharing file: {ex.Message}");
                return "500|Internal server error\n";
            }
        }

        private static async Task<string> ShareFolder(string folderId, string permission)
        {
            try
            {
                using (var connection = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await connection.OpenAsync();
                    
                    // Generate hash-based password from folder ID
                    string sharePassword = GenerateHashPassword(folderId);
                    //Console.WriteLine($"[DEBUG] Generated share password for folder {folderId}: {sharePassword}");
                    
                    // Update folder to set is_shared = 1 and share_pass (no permission column in folders table)
                    string updateFolderQuery = @"
                        UPDATE folders 
                        SET is_shared = 1, share_pass = @sharePass 
                        WHERE folder_id = @folderId";
                    
                    using (var command = new System.Data.SQLite.SQLiteCommand(updateFolderQuery, connection))
                    {
                        command.Parameters.AddWithValue("@sharePass", sharePassword);
                        command.Parameters.AddWithValue("@folderId", folderId);
                        
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            //Console.WriteLine($"[DEBUG] Successfully updated folder {folderId} with share password: {sharePassword}");
                            
                            // Also share all files in this folder with the same password
                            string updateFilesQuery = @"
                                UPDATE files 
                                SET is_shared = 1, share_pass = @sharePass 
                                WHERE folder_id = @folderId AND status = 'ACTIVE'";
                            
                            using (var filesCommand = new System.Data.SQLite.SQLiteCommand(updateFilesQuery, connection))
                            {
                                filesCommand.Parameters.AddWithValue("@sharePass", sharePassword);
                                filesCommand.Parameters.AddWithValue("@folderId", folderId);
                                
                                int filesUpdated = await filesCommand.ExecuteNonQueryAsync();
                                //Console.WriteLine($"[DEBUG] Updated {filesUpdated} files in folder {folderId} with share password: {sharePassword}");
                            }
                            
                            // Store the default permission info for this shared folder
                            // Permission will be handled when users access the folder via AddFolderShareEntryWithPermission
                            //Console.WriteLine($"[DEBUG] Folder {folderId} shared with default permission: {permission}");
                            
                            return $"200|{sharePassword}\n";
                        }
                        else
                        {
                            //Console.WriteLine($"[DEBUG] Folder {folderId} not found for sharing");
                            return "404|Folder not found\n";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sharing folder: {ex.Message}");
                return "500|Internal server error\n";
            }
        }

        private static async Task<string> GetFileSharePassword(string fileName)
        {
            try
            {
                using (var connection = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await connection.OpenAsync();
                    
                    string query = @"
                        SELECT share_pass 
                        FROM files 
                        WHERE file_name = @fileName AND is_shared = 1";
                    
                    using (var command = new System.Data.SQLite.SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@fileName", fileName);
                        
                        var result = await command.ExecuteScalarAsync();
                        if (result != null)
                        {
                            return $"200|{result}\n";
                        }
                        else
                        {
                            return "404|File not shared\n";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting file share password: {ex.Message}");
                return "500|Internal server error\n";
            }
        }

        private static async Task<string> GetFolderSharePassword(string folderName)
        {
            try
            {
                using (var connection = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await connection.OpenAsync();
                    
                    string query = @"
                        SELECT share_pass 
                        FROM folders 
                        WHERE folder_name = @folderName AND is_shared = 1";
                    
                    using (var command = new System.Data.SQLite.SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@folderName", folderName);
                        
                        var result = await command.ExecuteScalarAsync();
                        if (result != null)
                        {
                            return $"200|{result}\n";
                        }
                        else
                        {
                            return "404|Folder not shared\n";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting folder share password: {ex.Message}");
                return "500|Internal server error\n";
            }
        }

        private static async Task<string> GetFilesByPassword(string password)
        {
            try
            {
                //Console.WriteLine($"[DEBUG] Searching for files with password: '{password}'");
                using (var connection = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await connection.OpenAsync();
                    
                    // First, get all directly shared folders
                    var sharedFolderIds = new List<int>();
                    string folderQuery = @"
                        SELECT folder_id 
                        FROM folders 
                        WHERE share_pass = @password AND is_shared = 1";
                    
                    using (var folderCommand = new System.Data.SQLite.SQLiteCommand(folderQuery, connection))
                    {
                        folderCommand.Parameters.AddWithValue("@password", password);
                        using (var reader = await folderCommand.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int folderId = Convert.ToInt32(reader["folder_id"]);
                                sharedFolderIds.Add(folderId);
                                //Console.WriteLine($"[DEBUG] Found directly shared folder: {folderId}");
                            }
                        }
                    }
                    
                    //Console.WriteLine($"[DEBUG] Found {sharedFolderIds.Count} directly shared folders");
                    
                    // Then, recursively get all subfolders
                    var allFolderIds = new List<int>(sharedFolderIds);
                    var processedFolderIds = new HashSet<int>();
                    
                    while (sharedFolderIds.Count > 0)
                    {
                        var currentFolderIds = new List<int>(sharedFolderIds);
                        sharedFolderIds.Clear();
                        
                        if (currentFolderIds.Count > 0)
                        {
                            string subfolderQuery = @"
                                SELECT folder_id 
                                FROM folders 
                                WHERE parent_folder_id IN (" + string.Join(",", currentFolderIds) + ")";
                            
                            //Console.WriteLine($"[DEBUG] Looking for subfolders of: {string.Join(",", currentFolderIds)}");
                            
                            using (var subfolderCommand = new System.Data.SQLite.SQLiteCommand(subfolderQuery, connection))
                            {
                                using (var reader = await subfolderCommand.ExecuteReaderAsync())
                                {
                                    while (await reader.ReadAsync())
                                    {
                                        int folderId = Convert.ToInt32(reader["folder_id"]);
                                        if (!processedFolderIds.Contains(folderId))
                                        {
                                            allFolderIds.Add(folderId);
                                            sharedFolderIds.Add(folderId);
                                            processedFolderIds.Add(folderId);
                                            //Console.WriteLine($"[DEBUG] Found subfolder: {folderId}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    
                    //Console.WriteLine($"[DEBUG] Total folders found: {allFolderIds.Count}");
                    
                    // Finally, get all files in these folders plus directly shared files
                    if (allFolderIds.Count > 0)
                    {
                        string fileQuery = @"
                            SELECT DISTINCT f.file_id, f.file_name, f.file_type, f.file_size, f.upload_at, f.file_path, u.username as owner_name
                            FROM files f
                            JOIN users u ON f.owner_id = u.user_id
                            WHERE f.folder_id IN (" + string.Join(",", allFolderIds) + @") 
                               OR (f.share_pass = @password AND f.is_shared = 1 AND f.folder_id IS NULL)";
                        
                        using (var command = new System.Data.SQLite.SQLiteCommand(fileQuery, connection))
                        {
                            command.Parameters.AddWithValue("@password", password);
                            
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                var files = new List<string>();
                                while (await reader.ReadAsync())
                                {
                                    string fileInfo = $"{reader["file_id"]}:{reader["file_name"]}:{reader["file_type"]}:{reader["file_size"]}:{reader["upload_at"]}:{reader["owner_name"]}:{reader["file_path"]}";
                                    files.Add(fileInfo);
                                    //Console.WriteLine($"[DEBUG] Found file: {fileInfo}");
                                }
                                
                                if (files.Count > 0)
                                {
                                    //Console.WriteLine($"[DEBUG] Found {files.Count} files with password: {password}");
                                    return $"200|{string.Join(";", files)}\n";
                                }
                                else
                                {
                                    //Console.WriteLine($"[DEBUG] No files found with password: {password}");
                                    return "404|No files found with this password\n";
                                }
                            }
                        }
                    }
                    else
                    {
                        // Check for directly shared files only
                        string directFileQuery = @"
                            SELECT DISTINCT f.file_id, f.file_name, f.file_type, f.file_size, f.upload_at, f.file_path, u.username as owner_name
                            FROM files f
                            JOIN users u ON f.owner_id = u.user_id
                            WHERE f.share_pass = @password AND f.is_shared = 1";
                        
                        using (var command = new System.Data.SQLite.SQLiteCommand(directFileQuery, connection))
                        {
                            command.Parameters.AddWithValue("@password", password);
                            
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                var files = new List<string>();
                                while (await reader.ReadAsync())
                                {
                                    string fileInfo = $"{reader["file_id"]}:{reader["file_name"]}:{reader["file_type"]}:{reader["file_size"]}:{reader["upload_at"]}:{reader["owner_name"]}:{reader["file_path"]}";
                                    files.Add(fileInfo);
                                    //Console.WriteLine($"[DEBUG] Found directly shared file: {fileInfo}");
                                }
                                
                                if (files.Count > 0)
                                {
                                    //Console.WriteLine($"[DEBUG] Found {files.Count} directly shared files with password: {password}");
                                    return $"200|{string.Join(";", files)}\n";
                                }
                                else
                                {
                                    //Console.WriteLine($"[DEBUG] No files found with password: {password}");
                                    return "404|No files found with this password\n";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting files by password: {ex.Message}");
                return "500|Internal server error\n";
            }
        }

        private static async Task<string> GetFoldersByPassword(string password)
        {
            try
            {
                //Console.WriteLine($"[DEBUG] Searching for folders with password: '{password}'");
                using (var connection = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await connection.OpenAsync();
                    
                    // First, get all directly shared folders
                    var sharedFolderIds = new List<int>();
                    string folderQuery = @"
                        SELECT folder_id 
                        FROM folders 
                        WHERE share_pass = @password AND is_shared = 1";
                    
                    using (var folderCommand = new System.Data.SQLite.SQLiteCommand(folderQuery, connection))
                    {
                        folderCommand.Parameters.AddWithValue("@password", password);
                        using (var reader = await folderCommand.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int folderId = Convert.ToInt32(reader["folder_id"]);
                                sharedFolderIds.Add(folderId);
                                //Console.WriteLine($"[DEBUG] Found directly shared folder: {folderId}");
                            }
                        }
                    }
                    
                    //Console.WriteLine($"[DEBUG] Found {sharedFolderIds.Count} directly shared folders");
                    
                    // Then, recursively get all subfolders
                    var allFolderIds = new List<int>(sharedFolderIds);
                    var processedFolderIds = new HashSet<int>();
                    
                    while (sharedFolderIds.Count > 0)
                    {
                        var currentFolderIds = new List<int>(sharedFolderIds);
                        sharedFolderIds.Clear();
                        
                        if (currentFolderIds.Count > 0)
                        {
                            string subfolderQuery = @"
                                SELECT folder_id 
                                FROM folders 
                                WHERE parent_folder_id IN (" + string.Join(",", currentFolderIds) + ")";
                            
                            //Console.WriteLine($"[DEBUG] Looking for subfolders of: {string.Join(",", currentFolderIds)}");
                            
                            using (var subfolderCommand = new System.Data.SQLite.SQLiteCommand(subfolderQuery, connection))
                            {
                                using (var reader = await subfolderCommand.ExecuteReaderAsync())
                                {
                                    while (await reader.ReadAsync())
                                    {
                                        int folderId = Convert.ToInt32(reader["folder_id"]);
                                        if (!processedFolderIds.Contains(folderId))
                                        {
                                            allFolderIds.Add(folderId);
                                            sharedFolderIds.Add(folderId);
                                            processedFolderIds.Add(folderId);
                                            //Console.WriteLine($"[DEBUG] Found subfolder: {folderId}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    
                    //Console.WriteLine($"[DEBUG] Total folders found: {allFolderIds.Count}");
                    
                    // Finally, get all folders with their details
                    if (allFolderIds.Count > 0)
                    {
                        string finalQuery = @"
                            SELECT DISTINCT f.folder_id, f.folder_name, f.created_at, u.username as owner_name
                            FROM folders f
                            JOIN users u ON f.owner_id = u.user_id
                            WHERE f.folder_id IN (" + string.Join(",", allFolderIds) + ")";
                        
                        using (var command = new System.Data.SQLite.SQLiteCommand(finalQuery, connection))
                        {
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                var folders = new List<string>();
                                while (await reader.ReadAsync())
                                {
                                    string folderInfo = $"{reader["folder_id"]}:{reader["folder_name"]}:{reader["created_at"]}:{reader["owner_name"]}";
                                    folders.Add(folderInfo);
                                    //Console.WriteLine($"[DEBUG] Found folder: {folderInfo}");
                                }
                                
                                if (folders.Count > 0)
                                {
                                    //Console.WriteLine($"[DEBUG] Found {folders.Count} folders with password: {password}");
                                    return $"200|{string.Join(";", folders)}\n";
                                }
                                else
                                {
                                    //Console.WriteLine($"[DEBUG] No folders found with password: {password}");
                                    return "404|No folders found with this password\n";
                                }
                            }
                        }
                    }
                    else
                    {
                        //Console.WriteLine($"[DEBUG] No folders found with password: {password}");
                        return "404|No folders found with this password\n";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting folders by password: {ex.Message}");
                return "500|Internal server error\n";
            }
        }

        private static async Task<string> DebugListSharedFiles()
        {
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"
                        SELECT f.file_id, f.file_name, f.file_type, f.file_size, f.upload_at, f.file_path, u.username as owner_name, f.share_pass
                        FROM files f
                        JOIN users u ON f.owner_id = u.user_id
                        WHERE f.is_shared = 1
                        ORDER BY f.file_name";
                    
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(query, conn))
                    {
                        var files = new List<string>();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string fileInfo = $"{reader["file_id"]}:{reader["file_name"]}:{reader["file_type"]}:{reader["file_size"]}:{reader["upload_at"]}:{reader["owner_name"]}:{reader["file_path"]}:{reader["share_pass"]}";
                                files.Add(fileInfo);
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
                Console.WriteLine($"Error in DebugListSharedFiles: {ex.Message}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static async Task<string> DebugListSharedFolders()
        {
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"
                        SELECT f.folder_id, f.folder_name, f.created_at, u.username as owner_name, f.share_pass
                        FROM folders f
                        JOIN users u ON f.owner_id = u.user_id
                        WHERE f.is_shared = 1
                        ORDER BY f.folder_name";
                    
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(query, conn))
                    {
                        var folders = new List<string>();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string folderInfo = $"{reader["folder_id"]}:{reader["folder_name"]}:{reader["created_at"]}:{reader["owner_name"]}:{reader["share_pass"]}";
                                folders.Add(folderInfo);
                            }
                        }
                        
                        if (folders.Count == 0)
                        {
                            return "200|NO_SHARED_FOLDERS\n";
                        }
                        
                        return $"200|{string.Join(";", folders)}\n";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DebugListSharedFolders: {ex.Message}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static async Task<string> DebugListAllShared()
        {
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    

                    // Get all shared files
                    string fileQuery = @"
                        SELECT f.file_id, f.file_name, f.file_type, f.file_size, f.upload_at, f.file_path, u.username as owner_name, f.share_pass
                        FROM files f
                        JOIN users u ON f.owner_id = u.user_id
                        WHERE f.is_shared = 1
                        ORDER BY f.file_name";
                    
                    var files = new List<string>();
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(fileQuery, conn))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string fileInfo = $"{reader["file_id"]}:{reader["file_name"]}:{reader["file_type"]}:{reader["file_size"]}:{reader["upload_at"]}:{reader["owner_name"]}:{reader["file_path"]}:{reader["share_pass"]}";
                                files.Add(fileInfo);
                            }
                        }
                    }
                    
                    // Get all shared folders
                    string folderQuery = @"
                        SELECT f.folder_id, f.folder_name, f.created_at, u.username as owner_name, f.share_pass
                        FROM folders f
                        JOIN users u ON f.owner_id = u.user_id
                        WHERE f.is_shared = 1
                        ORDER BY f.folder_name";
                    
                    var folders = new List<string>();
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(folderQuery, conn))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string folderInfo = $"{reader["folder_id"]}:{reader["folder_name"]}:{reader["created_at"]}:{reader["owner_name"]}:{reader["share_pass"]}";
                                folders.Add(folderInfo);
                            }
                        }
                    }
                    
                    if (files.Count == 0 && folders.Count == 0)
                    {
                        return "200|NO_SHARED_ITEMS\n";
                    }
                    
                    string result = "";
                    if (files.Count > 0)
                    {
                        result += "FILES:" + string.Join(";", files) + "|";
                    }
                    if (folders.Count > 0)
                    {
                        result += "FOLDERS:" + string.Join(";", folders);
                    }
                    
                    return $"200|{result}\n";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DebugListAllShared: {ex.Message}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static async Task<string> GetFolderContents(string folderId)
        {
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    
                    var items = new List<string>();
                    
                    // Get all subfolders recursively
                    var allFolderIds = new List<int>();
                    var processedFolderIds = new HashSet<int>();
                    var foldersToProcess = new List<int> { int.Parse(folderId) };
                    
                    while (foldersToProcess.Count > 0)
                    {
                        var currentFolderIds = new List<int>(foldersToProcess);
                        foldersToProcess.Clear();
                        
                        if (currentFolderIds.Count > 0)
                        {
                            string subfolderQuery = "SELECT folder_id, folder_name, folder_path FROM folders WHERE parent_folder_id IN (" + string.Join(",", currentFolderIds) + ") AND status = 'ACTIVE'";
                            
                            using (var cmd = new System.Data.SQLite.SQLiteCommand(subfolderQuery, conn))
                            {
                                using (var reader = await cmd.ExecuteReaderAsync())
                                {
                                    while (await reader.ReadAsync())
                                    {
                                        int subFolderId = Convert.ToInt32(reader["folder_id"]);
                                        if (!processedFolderIds.Contains(subFolderId))
                                        {
                                            allFolderIds.Add(subFolderId);
                                            foldersToProcess.Add(subFolderId);
                                            processedFolderIds.Add(subFolderId);
                                            
                                            // Add folder to items list
                                            string folderPath = reader["folder_path"].ToString();
                                            string relativePath = GetRelativePath(folderPath, subFolderId.ToString());
                                            items.Add($"folder:{reader["folder_name"]}:{folderPath}:{relativePath}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    
                    // Get all files in these folders
                    var allFolderIdsWithRoot = new List<int>(allFolderIds) { int.Parse(folderId) };
                    
                    if (allFolderIdsWithRoot.Count > 0)
                    {
                        string fileQuery = "SELECT file_id, file_name, file_path, folder_id FROM files WHERE folder_id IN (" + string.Join(",", allFolderIdsWithRoot) + ") AND status = 'ACTIVE'";
                        
                        using (var cmd = new System.Data.SQLite.SQLiteCommand(fileQuery, conn))
                        {
                            using (var reader = await cmd.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    string filePath = reader["file_path"].ToString();
                                    string relativePath = GetRelativePath(filePath, reader["folder_id"].ToString());
                                    items.Add($"file:{reader["file_id"]}:{reader["file_name"]}:{filePath}:{relativePath}");
                                }
                            }
                        }
                    }
                    
                    if (items.Count == 0)
                    {
                        return "200|NO_CONTENTS\n";
                    }
                    
                    return $"200|{string.Join("|", items)}\n";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting folder contents: {ex.Message}");
                return "500|Internal server error\n";
            }
        }
        
        private static string GetRelativePath(string fullPath, string folderId)
        {
            try
            {
                // Extract the relative path from the full path
                // Assuming the path format is: uploads/{userId}/{folderName}/...
                string[] pathParts = fullPath.Split(Path.DirectorySeparatorChar);
                int uploadsIndex = Array.IndexOf(pathParts, "uploads");
                
                if (uploadsIndex >= 0 && uploadsIndex + 2 < pathParts.Length)
                {
                    // Skip uploads, userId, and folderName
                    return string.Join(Path.DirectorySeparatorChar.ToString(), pathParts, uploadsIndex + 3, pathParts.Length - uploadsIndex - 3);
                }
                
                return Path.GetFileName(fullPath);
            }
            catch
            {
                return Path.GetFileName(fullPath);
            }
        }

        private static async Task<string> DebugCheckDatabase()
        {
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    
                    // Check tables
                    string tableQuery = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name";
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(tableQuery, conn))
                    {
                        var tables = new List<string>();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                tables.Add(reader["name"].ToString());
                            }
                        }
                        //Console.WriteLine($"[DEBUG] Database tables: {string.Join(", ", tables)}");
                    }
                    
                    // Check users count
                    try
                    {
                        string userQuery = "SELECT COUNT(*) FROM users";
                        using (var cmd = new System.Data.SQLite.SQLiteCommand(userQuery, conn))
                        {
                            var userCount = await cmd.ExecuteScalarAsync();
                            //Console.WriteLine($"[DEBUG] Users count: {userCount}");
                        }
                    }
                    catch (Exception ex)
                    {
                        //Console.WriteLine($"[DEBUG] Error checking users: {ex.Message}");
                    }
                    
                    // Check files count
                    try
                    {
                        string fileQuery = "SELECT COUNT(*) FROM files";
                        using (var cmd = new System.Data.SQLite.SQLiteCommand(fileQuery, conn))
                        {
                            var fileCount = await cmd.ExecuteScalarAsync();
                            //Console.WriteLine($"[DEBUG] Files count: {fileCount}");
                        }
                    }
                    catch (Exception ex)
                    {
                        //Console.WriteLine($"[DEBUG] Error checking files: {ex.Message}");
                    }
                    
                    // Check folders count
                    try
                    {
                        string folderQuery = "SELECT COUNT(*) FROM folders";
                        using (var cmd = new System.Data.SQLite.SQLiteCommand(folderQuery, conn))
                        {
                            var folderCount = await cmd.ExecuteScalarAsync();
                            //Console.WriteLine($"[DEBUG] Folders count: {folderCount}");
                        }
                    }
                    catch (Exception ex)
                    {
                        //Console.WriteLine($"[DEBUG] Error checking folders: {ex.Message}");
                    }
                    
                    // Check shared folders
                    try
                    {
                        string sharedFolderQuery = "SELECT folder_id, folder_name, share_pass, is_shared FROM folders WHERE is_shared = 1";
                        using (var cmd = new System.Data.SQLite.SQLiteCommand(sharedFolderQuery, conn))
                        {
                            using (var reader = await cmd.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    //Console.WriteLine($"[DEBUG] Shared folder: ID={reader["folder_id"]}, Name={reader["folder_name"]}, Pass={reader["share_pass"]}, IsShared={reader["is_shared"]}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //Console.WriteLine($"[DEBUG] Error checking shared folders: {ex.Message}");
                    }
                    
                    // Check shared files
                    try
                    {
                        string sharedFileQuery = "SELECT file_id, file_name, share_pass, is_shared FROM files WHERE is_shared = 1";
                        using (var cmd = new System.Data.SQLite.SQLiteCommand(sharedFileQuery, conn))
                        {
                            using (var reader = await cmd.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    //Console.WriteLine($"[DEBUG] Shared file: ID={reader["file_id"]}, Name={reader["file_name"]}, Pass={reader["share_pass"]}, IsShared={reader["is_shared"]}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //Console.WriteLine($"[DEBUG] Error checking shared files: {ex.Message}");
                    }
                    
                    return "200|Database structure check complete.\n";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DebugCheckDatabase: {ex.Message}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static async Task<string> DebugCheckUserShares(string userId)
        {
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    
                    var result = new StringBuilder();
                    result.Append($"User {userId} shares:\n\n");
                    
                    // Check shared files for this user
                    string sharedFilesQuery = @"
                        SELECT f.file_id, f.file_name, f.file_type, fs.share_pass, COALESCE(fs.shared_at, 'Unknown') as shared_at
                        FROM files_share fs
                        JOIN files f ON fs.file_id = f.file_id
                        WHERE fs.user_id = @userId
                        ORDER BY f.file_name";
                    
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(sharedFilesQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", int.Parse(userId));
                        
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            result.Append("SHARED FILES:\n");
                            bool hasFiles = false;
                            while (await reader.ReadAsync())
                            {
                                hasFiles = true;
                                result.Append($"- {reader["file_name"]} ({reader["file_type"]}) - Shared: {reader["shared_at"]}\n");
                            }
                            if (!hasFiles)
                            {
                                result.Append("No shared files\n");
                            }
                        }
                    }
                    
                    // Check shared folders for this user
                    string sharedFoldersQuery = @"
                        SELECT f.folder_id, f.folder_name, fs.permission, COALESCE(fs.shared_at, 'Unknown') as shared_at
                        FROM folder_shares fs
                        JOIN folders f ON fs.folder_id = f.folder_id
                        WHERE fs.shared_with_user_id = @userId
                        ORDER BY f.folder_name";
                    
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(sharedFoldersQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", int.Parse(userId));
                        
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            result.Append("\nSHARED FOLDERS:\n");
                            bool hasFolders = false;
                            while (await reader.ReadAsync())
                            {
                                hasFolders = true;
                                result.Append($"- {reader["folder_name"]} (Permission: {reader["permission"]}) - Shared: {reader["shared_at"]}\n");
                            }
                            if (!hasFolders)
                            {
                                result.Append("No shared folders\n");
                            }
                        }
                    }
                    
                    return $"200|{result.ToString()}\n";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DebugCheckUserShares: {ex.Message}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static async Task<string> AddSharedFile(string fileName, string fileType, string fileSize, string userId, string filePath)
        {
            try
            {
                //Console.WriteLine($"[DEBUG] AddSharedFile called with: fileName={fileName}, fileType={fileType}, fileSize={fileSize}, userId={userId}, filePath={filePath}");
                
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    
                    // Generate file hash
                    string fileHash = GenerateFileHash(filePath);
                    //Console.WriteLine($"[DEBUG] Generated file hash: {fileHash}");
                    
                    string query = @"
                        INSERT INTO files (file_name, file_type, file_size, upload_at, owner_id, file_path, file_hash, status) 
                        VALUES (@fileName, @fileType, @fileSize, datetime('now'), @ownerId, @filePath, @fileHash, 'ACTIVE')";
                    
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@fileName", fileName);
                        cmd.Parameters.AddWithValue("@fileType", fileType);
                        cmd.Parameters.AddWithValue("@fileSize", int.Parse(fileSize));
                        cmd.Parameters.AddWithValue("@ownerId", int.Parse(userId));
                        cmd.Parameters.AddWithValue("@filePath", filePath);
                        cmd.Parameters.AddWithValue("@fileHash", fileHash);
                        
                        await cmd.ExecuteNonQueryAsync();
                        //Console.WriteLine($"[DEBUG] File added to database successfully: {fileName}");
                        return "200|FILE_ADDED_SHARED\n";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddSharedFile: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static string GenerateFileHash(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    using (var sha256 = System.Security.Cryptography.SHA256.Create())
                    {
                        using (var stream = File.OpenRead(filePath))
                        {
                            byte[] hash = sha256.ComputeHash(stream);
                            return Convert.ToBase64String(hash);
                        }
                    }
                }
                return "no_hash";
            }
            catch
            {
                return "no_hash";
            }
        }

        private static async Task<string> AddSharedFolder(string folderName, string userId, string folderPath)
        {
            try
            {
                //Console.WriteLine($"[DEBUG] AddSharedFolder called with: folderName={folderName}, userId={userId}, folderPath={folderPath}");
                
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    
                    // Convert to relative path for database
                    string relativePath = ConvertToRelativePath(folderPath);
                    //Console.WriteLine($"[DEBUG] Converted to relative path: {relativePath}");
                    
                    string query = @"
                        INSERT INTO folders (folder_name, owner_id, parent_folder_id, folder_path, created_at, status) 
                        VALUES (@folderName, @ownerId, NULL, @folderPath, datetime('now'), 'ACTIVE')";
                    
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@folderName", folderName);
                        cmd.Parameters.AddWithValue("@ownerId", int.Parse(userId));
                        cmd.Parameters.AddWithValue("@folderPath", relativePath);
                        
                        await cmd.ExecuteNonQueryAsync();
                        //Console.WriteLine($"[DEBUG] Folder added to database successfully: {folderName}");
                        return "200|FOLDER_ADDED_SHARED\n";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddSharedFolder: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static string ConvertToRelativePath(string fullPath)
        {
            try
            {
                // Extract the relative path from the full path
                // Assuming the path format is: uploads/{userId}/{folderName}/...
                string[] pathParts = fullPath.Split(Path.DirectorySeparatorChar);
                int uploadsIndex = Array.IndexOf(pathParts, "uploads");
                
                if (uploadsIndex >= 0 && uploadsIndex + 2 < pathParts.Length)
                {
                    // Skip uploads, userId, and folderName
                    return string.Join(Path.DirectorySeparatorChar.ToString(), pathParts, uploadsIndex + 3, pathParts.Length - uploadsIndex - 3);
                }
                
                return Path.GetFileName(fullPath);
            }
            catch
            {
                return Path.GetFileName(fullPath);
            }
        }

        private static async Task<string> GetFileIdByName(string fileName)
        {
            try
            {
                //Console.WriteLine($"[DEBUG] GetFileIdByName called with: fileName={fileName}");
                
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    string query = "SELECT file_id FROM files WHERE file_name = @fileName ORDER BY file_id DESC LIMIT 1";
                    
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@fileName", fileName);
                        
                        var result = await cmd.ExecuteScalarAsync();
                        if (result != null)
                        {
                            //Console.WriteLine($"[DEBUG] Found file ID: {result}");
                            return $"200|{result}\n";
                        }
                        else
                        {
                            //Console.WriteLine($"[DEBUG] File not found: {fileName}");
                            return "404|File not found\n";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting file ID by name: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return "500|Internal server error\n";
            }
        }

        private static async Task<string> GetFolderIdByName(string folderName)
        {
            try
            {
                //Console.WriteLine($"[DEBUG] GetFolderIdByName called with: folderName={folderName}");
                
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    string query = "SELECT folder_id FROM folders WHERE folder_name = @folderName ORDER BY folder_id DESC LIMIT 1";
                    
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@folderName", folderName);
                        
                        var result = await cmd.ExecuteScalarAsync();
                        if (result != null)
                        {
                            //Console.WriteLine($"[DEBUG] Found folder ID: {result}");
                            return $"200|{result}\n";
                        }
                        else
                        {
                            //Console.WriteLine($"[DEBUG] Folder not found: {folderName}");
                            return "404|Folder not found\n";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting folder ID by name: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return "500|Internal server error\n";
            }
        }

        private static async Task<string> AddFolderShareEntry(string folderId, string userId, string permission)
        {
            try
            {
                //Console.WriteLine($"[DEBUG] AddFolderShareEntry called with: folderId={folderId}, userId={userId}, permission={permission}");
                
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"
                        INSERT OR REPLACE INTO folder_shares (folder_id, shared_with_user_id, permission, shared_at) 
                        VALUES (@folderId, @userId, @permission, datetime('now'))";
                    
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@folderId", int.Parse(folderId));
                        cmd.Parameters.AddWithValue("@userId", int.Parse(userId));
                        cmd.Parameters.AddWithValue("@permission", permission);
                        
                        await cmd.ExecuteNonQueryAsync();
                        //Console.WriteLine($"[DEBUG] Folder share entry added successfully");
                        return "200|FOLDER_SHARE_ADDED\n";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding folder share entry: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return "500|Internal server error\n";
            }
        }

        private static async Task<string> AddFolderShareEntryWithPermission(string folderId, string userId, string sharePass, string permission)
        {
            try
            {
                //Console.WriteLine($"[DEBUG] AddFolderShareEntryWithPermission called with: folderId={folderId}, userId={userId}, sharePass={sharePass}, permission={permission}");
                
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    
                    // Check if the entry already exists
                    string checkQuery = "SELECT COUNT(*) FROM folder_shares WHERE folder_id = @folder_id AND shared_with_user_id = @user_id";
                    using (var checkCmd = new System.Data.SQLite.SQLiteCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@folder_id", int.Parse(folderId));
                        checkCmd.Parameters.AddWithValue("@user_id", int.Parse(userId));
                        
                        long count = (long)await checkCmd.ExecuteScalarAsync();
                        if (count > 0)
                        {
                            //Console.WriteLine($"[DEBUG] Folder share entry already exists");
                            return "200|ALREADY_SHARED\n";
                        }
                    }
                    
                    // Insert new share entry with permission field
                    string insertQuery = "INSERT INTO folder_shares (folder_id, shared_with_user_id, share_pass, permission, shared_at) VALUES (@folder_id, @user_id, @share_pass, @permission, datetime('now'))";
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@folder_id", int.Parse(folderId));
                        cmd.Parameters.AddWithValue("@user_id", int.Parse(userId));
                        cmd.Parameters.AddWithValue("@share_pass", sharePass);
                        cmd.Parameters.AddWithValue("@permission", permission);
                        
                        await cmd.ExecuteNonQueryAsync();
                        //Console.WriteLine($"[DEBUG] Folder share entry added successfully with permission: {permission}");
                        return "200|FOLDER_SHARE_ADDED_WITH_PERMISSION\n";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error in AddFolderShareEntryWithPermission: {ex.Message}");
                Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static string GenerateHashPassword(string id)
        {
            // Generate a hash-based password using only the ID (without timestamp)
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(id));
                return Convert.ToBase64String(hashBytes).Substring(0, 8); // Take first 8 characters
            }
        }

        private static async Task<string> AddFilesInFolderToShare(string folderId, string userId, string sharePass, string permission)
        {
            try
            {
                //Console.WriteLine($"[DEBUG] AddFilesInFolderToShare called with: folderId={folderId}, userId={userId}, sharePass={sharePass}, permission={permission}");
                
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    
                    // Get all files in the folder
                    string getFilesQuery = @"
                        SELECT file_id 
                        FROM files 
                        WHERE folder_id = @folderId AND status = 'ACTIVE'";
                    
                    var fileIds = new List<int>();
                    using (var getFilesCmd = new System.Data.SQLite.SQLiteCommand(getFilesQuery, conn))
                    {
                        getFilesCmd.Parameters.AddWithValue("@folderId", int.Parse(folderId));
                        
                        using (var reader = await getFilesCmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                fileIds.Add(Convert.ToInt32(reader["file_id"]));
                            }
                        }
                    }
                    
                    //Console.WriteLine($"[DEBUG] Found {fileIds.Count} files in folder {folderId}");
                    
                    // Add each file to files_share table
                    string insertQuery = @"
                        INSERT OR REPLACE INTO files_share (file_id, user_id, share_pass, permission, shared_at) 
                        VALUES (@fileId, @userId, @sharePass, @permission, datetime('now'))";
                    
                    foreach (int fileId in fileIds)
                    {
                        using (var insertCmd = new System.Data.SQLite.SQLiteCommand(insertQuery, conn))
                        {
                            insertCmd.Parameters.AddWithValue("@fileId", fileId);
                            insertCmd.Parameters.AddWithValue("@userId", int.Parse(userId));
                            insertCmd.Parameters.AddWithValue("@sharePass", sharePass);
                            insertCmd.Parameters.AddWithValue("@permission", permission);
                            
                            await insertCmd.ExecuteNonQueryAsync();
                        }
                    }
                    
                    //Console.WriteLine($"[DEBUG] Added {fileIds.Count} files to files_share table");
                    return "200|FILES_IN_FOLDER_SHARED\n";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error in AddFilesInFolderToShare: {ex.Message}");
                Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static async Task<string> GetSharedFolderContents(string folderId, string userId)
        {
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    
                    // Check if user has access to this folder
                    string checkQuery = @"
                        SELECT f.folder_id, f.folder_name, f.share_pass, fs.permission
                        FROM folders f
                        JOIN folder_shares fs ON f.folder_id = fs.folder_id
                        WHERE f.folder_id = @folderId
                        AND (
                            fs.shared_with_user_id = @userId
                            OR f.owner_id = @userId
                        )";
                    
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(checkQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@folderId", int.Parse(folderId));
                        cmd.Parameters.AddWithValue("@userId", int.Parse(userId));
                        
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (!await reader.ReadAsync())
                                return "404|FOLDER_NOT_FOUND_OR_NO_ACCESS\n";
                                
                            string folderName = reader["folder_name"].ToString();
                            string sharePass = reader["share_pass"].ToString();
                            string permission = reader["permission"].ToString();
                            
                            // Get all files in this folder that user has access to
                            string fileQuery = @"
                                SELECT f.file_id, f.file_name, f.file_type, f.file_size, f.upload_at, f.file_path, u.username as owner_name
                                FROM files f
                                JOIN users u ON f.owner_id = u.user_id
                                WHERE f.folder_id = @folderId
                                AND f.status = 'ACTIVE'
                                AND (
                                    f.owner_id = @userId
                                    OR f.file_id IN (
                                        SELECT fs.file_id FROM files_share fs WHERE fs.user_id = @userId
                                    )
                                )
                                ORDER BY f.file_name";
                            
                            using (var filesCmd = new System.Data.SQLite.SQLiteCommand(fileQuery, conn))
                            {
                                filesCmd.Parameters.AddWithValue("@folderId", int.Parse(folderId));
                                filesCmd.Parameters.AddWithValue("@userId", int.Parse(userId));
                                
                                using (var filesReader = await filesCmd.ExecuteReaderAsync())
                                {
                                    var files = new List<string>();
                                    while (await filesReader.ReadAsync())
                                    {
                                        // Format expected by client: file:<file_id>:<file_name>:<file_path>:<relative_path>:<file_size>
                                        string relativePath = GetRelativePath(filesReader["file_path"].ToString(), folderId);
                                        string fileSize = filesReader["file_size"].ToString();
                                        string fileInfo = $"file:{filesReader["file_id"]}:{filesReader["file_name"]}:{filesReader["file_path"]}:{relativePath}:{fileSize}";
                                        files.Add(fileInfo);
                                    }
                                    
                                    if (files.Count == 0)
                                    {
                                        return "200|NO_FILES_IN_FOLDER\n";
                                    }
                                    
                                    return $"200|{string.Join("|", files)}\n";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting shared folder contents: {ex.Message}");
                return "500|Internal server error\n";
            }
        }
        
        // Additional methods from HEAD branch
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

        // NEW: Remove shared items methods
        private static async Task<string> RemoveSharedFile(string fileId, string userId)
        {
            try
            {
                //Console.WriteLine($"[DEBUG] RemoveSharedFile called with: fileId={fileId}, userId={userId}");
                
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    
                    // Remove entry from files_share table
                    string deleteQuery = @"
                        DELETE FROM files_share 
                        WHERE file_id = @fileId AND user_id = @userId";
                    
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@fileId", int.Parse(fileId));
                        cmd.Parameters.AddWithValue("@userId", int.Parse(userId));
                        
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        
                        if (rowsAffected > 0)
                        {
                            //Console.WriteLine($"[DEBUG] Successfully removed shared file: fileId={fileId}, userId={userId}");
                            return "200|SHARED_FILE_REMOVED\n";
                        }
                        else
                        {
                            //Console.WriteLine($"[DEBUG] No shared file entry found: fileId={fileId}, userId={userId}");
                            return "404|SHARED_FILE_NOT_FOUND\n";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error in RemoveSharedFile: {ex.Message}");
                Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static async Task<string> AddFolderAndFilesShare(string folderId, string userId, string sharePass, string permission)
        {
            try
            {
                //Console.WriteLine($"[DEBUG] AddFolderAndFilesShare called with: folderId={folderId}, userId={userId}, sharePass={sharePass}, permission={permission}");
                
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    
                    // Start transaction to ensure atomicity
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // 1. Verify folder exists and is active
                            string verifyFolderQuery = "SELECT COUNT(*) FROM folders WHERE folder_id = @folder_id AND status = 'ACTIVE'";
                            using (var verifyCmd = new System.Data.SQLite.SQLiteCommand(verifyFolderQuery, conn, transaction))
                            {
                                verifyCmd.Parameters.AddWithValue("@folder_id", int.Parse(folderId));
                                long folderCount = (long)await verifyCmd.ExecuteScalarAsync();
                                if (folderCount == 0)
                                {
                                    Console.WriteLine($"[ERROR] Folder {folderId} not found or not active");
                                    transaction.Rollback();
                                    return "404|FOLDER_NOT_FOUND\n";
                                }
                            }

                            // 2. Check if share already exists
                            string checkQuery = "SELECT COUNT(*) FROM folder_shares WHERE folder_id = @folder_id AND shared_with_user_id = @user_id";
                            using (var checkCmd = new System.Data.SQLite.SQLiteCommand(checkQuery, conn, transaction))
                            {
                                checkCmd.Parameters.AddWithValue("@folder_id", int.Parse(folderId));
                                checkCmd.Parameters.AddWithValue("@user_id", int.Parse(userId));
                                
                                long count = (long)await checkCmd.ExecuteScalarAsync();
                                if (count > 0)
                                {
                                    //Console.WriteLine($"[DEBUG] Folder share entry already exists");
                                    transaction.Rollback();
                                    return "200|ALREADY_SHARED\n";
                                }
                            }
                            
                            // 3. Add folder to folder_shares table
                            string insertFolderQuery = "INSERT INTO folder_shares (folder_id, shared_with_user_id, share_pass, permission, shared_at) VALUES (@folder_id, @user_id, @share_pass, @permission, datetime('now'))";
                            using (var folderCmd = new System.Data.SQLite.SQLiteCommand(insertFolderQuery, conn, transaction))
                            {
                                folderCmd.Parameters.AddWithValue("@folder_id", int.Parse(folderId));
                                folderCmd.Parameters.AddWithValue("@user_id", int.Parse(userId));
                                folderCmd.Parameters.AddWithValue("@share_pass", sharePass);
                                folderCmd.Parameters.AddWithValue("@permission", permission);
                                
                                int rowsAffected = await folderCmd.ExecuteNonQueryAsync();
                                if (rowsAffected == 0)
                                {
                                    Console.WriteLine($"[ERROR] Failed to insert folder share entry");
                                    transaction.Rollback();
                                    return "500|FAILED_TO_ADD_FOLDER_SHARE\n";
                                }
                                //Console.WriteLine($"[DEBUG] Folder share entry added successfully");
                            }
                            
                            // 4. Get all files in the folder
                            string getFilesQuery = @"
                                SELECT file_id 
                                FROM files 
                                WHERE folder_id = @folderId AND status = 'ACTIVE'";
                            
                            var fileIds = new List<int>();
                            using (var getFilesCmd = new System.Data.SQLite.SQLiteCommand(getFilesQuery, conn, transaction))
                            {
                                getFilesCmd.Parameters.AddWithValue("@folderId", int.Parse(folderId));
                                
                                using (var reader = await getFilesCmd.ExecuteReaderAsync())
                                {
                                    while (await reader.ReadAsync())
                                    {
                                        fileIds.Add(Convert.ToInt32(reader["file_id"]));
                                    }
                                }
                            }
                            
                            //Console.WriteLine($"[DEBUG] Found {fileIds.Count} files in folder {folderId}");
                            
                            // 5. Add each file to files_share table
                            string insertFileQuery = @"
                                INSERT OR REPLACE INTO files_share (file_id, user_id, share_pass, permission, shared_at) 
                                VALUES (@fileId, @userId, @sharePass, @permission, datetime('now'))";
                            
                            foreach (int fileId in fileIds)
                            {
                                using (var insertCmd = new System.Data.SQLite.SQLiteCommand(insertFileQuery, conn, transaction))
                                {
                                    insertCmd.Parameters.AddWithValue("@fileId", fileId);
                                    insertCmd.Parameters.AddWithValue("@userId", int.Parse(userId));
                                    insertCmd.Parameters.AddWithValue("@sharePass", sharePass);
                                    insertCmd.Parameters.AddWithValue("@permission", permission);
                                    
                                    int rowsAffected = await insertCmd.ExecuteNonQueryAsync();
                                    if (rowsAffected == 0)
                                    {
                                        Console.WriteLine($"[ERROR] Failed to insert file share entry for file {fileId}");
                                        transaction.Rollback();
                                        return "500|FAILED_TO_ADD_FILE_SHARE\n";
                                    }
                                }
                            }
                            
                            //Console.WriteLine($"[DEBUG] Added {fileIds.Count} files to files_share table");
                            
                            // 6. Update is_shared flag in folders table
                            string updateFolderQuery = "UPDATE folders SET is_shared = 1 WHERE folder_id = @folder_id";
                            using (var updateCmd = new System.Data.SQLite.SQLiteCommand(updateFolderQuery, conn, transaction))
                            {
                                updateCmd.Parameters.AddWithValue("@folder_id", int.Parse(folderId));
                                await updateCmd.ExecuteNonQueryAsync();
                            }
                            
                            // Commit transaction
                            transaction.Commit();
                            return "200|FOLDER_AND_FILES_SHARED\n";
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[ERROR] Transaction error in AddFolderAndFilesShare: {ex.Message}");
                            Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                            transaction.Rollback();
                            return "500|INTERNAL_ERROR\n";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error in AddFolderAndFilesShare: {ex.Message}");
                Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static async Task<string> RemoveSharedFolder(string folderId, string userId)
        {
            try
            {
                //Console.WriteLine($"[DEBUG] RemoveSharedFolder called with: folderId={folderId}, userId={userId}");
                
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    
                    // Remove entry from folder_shares table
                    string deleteQuery = @"
                        DELETE FROM folder_shares 
                        WHERE folder_id = @folderId AND shared_with_user_id = @userId";
                    
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@folderId", int.Parse(folderId));
                        cmd.Parameters.AddWithValue("@userId", int.Parse(userId));
                        
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        
                        if (rowsAffected > 0)
                        {
                            // Also remove all files in this folder from files_share table
                            string deleteFilesQuery = @"
                                DELETE FROM files_share 
                                WHERE file_id IN (
                                    SELECT f.file_id 
                                    FROM files f 
                                    WHERE f.folder_id = @folderId
                                ) AND user_id = @userId";
                            
                            using (var filesCmd = new System.Data.SQLite.SQLiteCommand(deleteFilesQuery, conn))
                            {
                                filesCmd.Parameters.AddWithValue("@folderId", int.Parse(folderId));
                                filesCmd.Parameters.AddWithValue("@userId", int.Parse(userId));
                                await filesCmd.ExecuteNonQueryAsync();
                            }
                            
                            //Console.WriteLine($"[DEBUG] Successfully removed shared folder: folderId={folderId}, userId={userId}");
                            return "200|SHARED_FOLDER_REMOVED\n";
                        }
                        else
                        {
                            //Console.WriteLine($"[DEBUG] No shared folder entry found: folderId={folderId}, userId={userId}");
                            return "404|SHARED_FOLDER_NOT_FOUND\n";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error in RemoveSharedFolder: {ex.Message}");
                Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        // ================ CLIENT-SIDE RE-ENCRYPTION SUPPORT ================

        /// <summary>
        /// Upload shared version of file (encrypted with share_pass)
        /// Command: UPLOAD_SHARED_VERSION|fileId|sharedFileName|fileSize|ownerId|uploadAt|sharePass
        /// </summary>
        private static async Task<string> UploadSharedVersion(string fileId, string sharedFileName, int fileSize, 
            string ownerId, string uploadAt, string sharePass, NetworkStream stream)
        {
            try
            {
                //Console.WriteLine($"[DEBUG] UploadSharedVersion: fileId={fileId}, fileName={sharedFileName}, size={fileSize}, owner={ownerId}, sharePass={sharePass}");
                
                if (fileSize > 10 * 1024 * 1024) // 10MB limit
                {
                    return "413|FILE_TOO_LARGE\n";
                }

                // Create shared uploads directory
                string projectRoot = FindProjectRoot();
                string uploadsDir = Path.Combine(projectRoot, "uploads");
                string userDir = Path.Combine(uploadsDir, ownerId);
                string sharedDir = Path.Combine(userDir, "shared"); // Special subdirectory for shared versions
                
                if (!Directory.Exists(sharedDir))
                    Directory.CreateDirectory(sharedDir);

                // Save shared version to disk
                string sharedFilePath = Path.Combine(sharedDir, sharedFileName);
                byte[] buffer = new byte[8192];
                int totalRead = 0;

                using (FileStream fs = new FileStream(sharedFilePath, FileMode.Create, FileAccess.Write))
                {
                    while (totalRead < fileSize)
                    {
                        int bytesRead = await stream.ReadAsync(buffer, 0, Math.Min(buffer.Length, fileSize - totalRead));
                        if (bytesRead == 0) break;
                        await fs.WriteAsync(buffer, 0, bytesRead);
                        totalRead += bytesRead;
                    }
                }

                //Console.WriteLine($"[DEBUG] Received shared version: {totalRead}/{fileSize} bytes");

                if (totalRead == fileSize)
                {
                    // Update database: add shared_file_path to the original file record
                    using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                    {
                        await conn.OpenAsync();
                        
                        // Update files table with shared version path
                        string updateQuery = @"
                            UPDATE files 
                            SET shared_file_path = @sharedFilePath 
                            WHERE file_id = @fileId";
                        
                        using (var cmd = new System.Data.SQLite.SQLiteCommand(updateQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@sharedFilePath", Path.Combine("uploads", ownerId, "shared", sharedFileName));
                            cmd.Parameters.AddWithValue("@fileId", int.Parse(fileId));
                            
                            int rowsAffected = await cmd.ExecuteNonQueryAsync();
                            if (rowsAffected > 0)
                            {
                                //Console.WriteLine($"[DEBUG] Successfully updated file {fileId} with shared version path");
                                return "200|SHARED_VERSION_UPLOADED\n";
                            }
                            else
                            {
                                Console.WriteLine($"[ERROR] Failed to update file {fileId} with shared version");
                                return "404|ORIGINAL_FILE_NOT_FOUND\n";
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"[ERROR] Incomplete upload: received {totalRead}/{fileSize} bytes");
                    return "400|INCOMPLETE_UPLOAD\n";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error in UploadSharedVersion: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                return "500|INTERNAL_ERROR\n";
            }
        }

        private static async Task<string> GetFilesInFolder(string folderId)
        {
            try
            {
                //Console.WriteLine($"[DEBUG] GetFilesInFolder called with folderId: {folderId}");
                
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    
                    string query = @"
                        SELECT file_id, file_name, file_type, file_size, upload_at, file_path
                        FROM files 
                        WHERE folder_id = @folderId AND status = 'ACTIVE'
                        ORDER BY file_name";
                    
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@folderId", int.Parse(folderId));
                        
                        var files = new List<string>();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int fileId = Convert.ToInt32(reader["file_id"]);
                                string fileName = reader["file_name"].ToString();
                                string fileType = reader["file_type"].ToString();
                                int fileSize = Convert.ToInt32(reader["file_size"]);
                                string uploadAt = reader["upload_at"].ToString();
                                string filePath = reader["file_path"].ToString();
                                
                                // Format: fileId:fileName:fileType:fileSize:uploadAt:filePath
                                string fileInfo = $"{fileId}:{fileName}:{fileType}:{fileSize}:{uploadAt}:{filePath}";
                                files.Add(fileInfo);
                            }
                        }
                        
                        if (files.Count == 0)
                        {
                            //Console.WriteLine($"[DEBUG] No files found in folder {folderId}");
                            return "200|NO_FILES\n";
                        }
                        
                        string result = string.Join("|", files);
                        //Console.WriteLine($"[DEBUG] Found {files.Count} files in folder {folderId}");
                        return $"200|{result}\n";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error in GetFilesInFolder: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                return "500|INTERNAL_ERROR\n";
            }
        }
    }
}
