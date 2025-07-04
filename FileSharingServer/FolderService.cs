using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;

namespace FileSharingServer
{
    public static class FolderService
    {
        const int MAX_UPLOAD_SIZE = 50 * 1024 * 1024; // 50MB
        const int BUFFER_SIZE = 8192;

        public static async Task<string> ReceiveFolder(string folderName, int totalSize, string ownerId, string uploadTime, NetworkStream stream)
        {
            if (totalSize > MAX_UPLOAD_SIZE)
            {
                return "413\n"; // Payload too large
            }

            try
            {
                string uploadDir = GetSharedUploadsPath();
                string userDir = Path.Combine(uploadDir, ownerId);
                if (!Directory.Exists(userDir))
                    Directory.CreateDirectory(userDir);

                // Create folder in database first
                int folderId = await CreateFolderInDatabase(folderName, ownerId, userDir);
                if (folderId == -1)
                {
                    return "500\n"; // Failed to create folder in database
                }

                string folderPath = Path.Combine(userDir, folderName);
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                // Receive zip file containing the folder
                string tempZipPath = Path.Combine(userDir, $"temp_{folderName}_{DateTime.Now.Ticks}.zip");
                byte[] buffer = new byte[BUFFER_SIZE];
                int totalRead = 0;

                using (FileStream fs = new FileStream(tempZipPath, FileMode.Create, FileAccess.Write))
                {
                    while (totalRead < totalSize)
                    {
                        int bytesRead = await stream.ReadAsync(buffer, 0, Math.Min(buffer.Length, totalSize - totalRead));
                        if (bytesRead == 0) break;
                        await fs.WriteAsync(buffer, 0, bytesRead);
                        totalRead += bytesRead;
                    }
                }

                if (totalRead != totalSize)
                {
                    // Clean up on failure
                    File.Delete(tempZipPath);
                    await DeleteFolderFromDatabase(folderId);
                    return "400\n"; // Incomplete upload
                }

                // Extract folder contents
                await ExtractAndProcessFolder(tempZipPath, folderPath, folderId, ownerId);

                // Clean up temp zip file
                File.Delete(tempZipPath);

                return "200\n"; // Success
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi nhận folder: {ex.Message}");
                return "500\n"; // Internal Server Error
            }
        }

        private static async Task<int> CreateFolderInDatabase(string folderName, string ownerId, string folderPath)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    string insertQuery = @"
                        INSERT INTO folders (folder_name, owner_id, folder_path, created_at) 
                        VALUES (@folderName, @ownerId, @folderPath, @createdAt)";
                    
                    using (SQLiteCommand cmd = new SQLiteCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@folderName", folderName);
                        cmd.Parameters.AddWithValue("@ownerId", int.Parse(ownerId));
                        cmd.Parameters.AddWithValue("@folderPath", Path.Combine("uploads", ownerId, folderName));
                        cmd.Parameters.AddWithValue("@createdAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        await cmd.ExecuteNonQueryAsync();
                    }

                    // Get the folder ID
                    string selectQuery = "SELECT last_insert_rowid()";
                    using (SQLiteCommand cmd = new SQLiteCommand(selectQuery, conn))
                    {
                        var result = await cmd.ExecuteScalarAsync();
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tạo folder trong database: {ex.Message}");
                return -1;
            }
        }

        private static async Task DeleteFolderFromDatabase(int folderId)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    string deleteQuery = "DELETE FROM folders WHERE folder_id = @folderId";
                    using (SQLiteCommand cmd = new SQLiteCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@folderId", folderId);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xóa folder khỏi database: {ex.Message}");
            }
        }

        private static async Task ExtractAndProcessFolder(string zipPath, string extractPath, int folderId, string ownerId)
        {
            try
            {
                ZipFile.ExtractToDirectory(zipPath, extractPath);

                // Process all files in the extracted folder
                await ProcessFolderContents(extractPath, folderId, ownerId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi giải nén folder: {ex.Message}");
                throw;
            }
        }

        private static async Task ProcessFolderContents(string folderPath, int folderId, string ownerId)
        {
            try
            {
                string[] files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
                
                foreach (string filePath in files)
                {
                    FileInfo fileInfo = new FileInfo(filePath);
                    string relativePath = GetRelativePath(folderPath, filePath);
                    string fileHash = CalculateSHA256(filePath);

                    await AddFileToDatabase(fileInfo.Name, folderId, fileInfo.Length, 
                        Path.GetExtension(filePath).TrimStart('.').ToLower(), 
                        relativePath, fileHash, ownerId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xử lý nội dung folder: {ex.Message}");
                throw;
            }
        }

        private static async Task AddFileToDatabase(string fileName, int folderId, long fileSize, 
            string fileType, string relativePath, string fileHash, string ownerId)
        {
            try
            {
                Console.WriteLine($"[DEBUG] Adding to DB: {fileName}, FolderID: {folderId}, Size: {fileSize}");
                using (SQLiteConnection conn = new SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    string insertQuery = @"
                        INSERT INTO files (file_name, folder_id, owner_id, file_size, file_type, file_path, file_hash, upload_at) 
                        VALUES (@fileName, @folderId, @ownerId, @fileSize, @fileType, @filePath, @fileHash, @uploadAt)";
                    
                    using (SQLiteCommand cmd = new SQLiteCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@fileName", fileName);
                        cmd.Parameters.AddWithValue("@folderId", folderId);
                        cmd.Parameters.AddWithValue("@ownerId", int.Parse(ownerId));
                        cmd.Parameters.AddWithValue("@fileSize", fileSize);
                        cmd.Parameters.AddWithValue("@fileType", fileType);
                        cmd.Parameters.AddWithValue("@filePath", relativePath);
                        cmd.Parameters.AddWithValue("@fileHash", fileHash);
                        cmd.Parameters.AddWithValue("@uploadAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                Console.WriteLine($"[SUCCESS] File added to database: {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi thêm file vào database: {ex.Message}");
                Console.WriteLine($"[ERROR] Database error for {fileName}: {ex.StackTrace}");
            }
        }

        public static async Task<List<FolderInfo>> GetUserFolders(string userId)
        {
            var folders = new List<FolderInfo>();
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"
                        SELECT folder_id, folder_name, folder_path, created_at, is_shared, status
                        FROM folders 
                        WHERE owner_id = @userId AND status = 'ACTIVE'
                        ORDER BY created_at DESC";
                    
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", int.Parse(userId));
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                folders.Add(new FolderInfo
                                {
                                    FolderId = Convert.ToInt32(reader["folder_id"]),
                                    FolderName = reader["folder_name"].ToString(),
                                    FolderPath = reader["folder_path"].ToString(),
                                    CreatedAt = reader["created_at"].ToString(),
                                    IsShared = Convert.ToInt32(reader["is_shared"]) == 1,
                                    Status = reader["status"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy danh sách folder: {ex.Message}");
            }
            return folders;
        }

        private static string GetRelativePath(string basePath, string fullPath)
        {
            Uri baseUri = new Uri(basePath.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar);
            Uri fullUri = new Uri(fullPath);
            return Uri.UnescapeDataString(baseUri.MakeRelativeUri(fullUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        private static string CalculateSHA256(string filePath)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            using (var stream = File.OpenRead(filePath))
            {
                var hash = sha256.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        public static async Task<string> ReceiveFileInFolder(string folderName, string relativePath, string fileName, int fileSize, string ownerId, string uploadTime, NetworkStream stream)
        {
            try
            {
                Console.WriteLine($"[DEBUG] Starting upload: {folderName}/{relativePath}/{fileName} (Size: {fileSize}, Owner: {ownerId})");
                
                // 1. Kiểm tra/tạo folder trong DB
                int folderId = await EnsureFolderExists(folderName, ownerId);
                if (folderId == -1)
                {
                    Console.WriteLine($"[ERROR] Failed to ensure folder exists: {folderName}");
                    return "500\n";
                }
                Console.WriteLine($"[DEBUG] Folder ID: {folderId}");

                // 2. Tạo thư mục vật lý nếu chưa có
                string uploadDir = GetSharedUploadsPath();
                string userDir = Path.Combine(uploadDir, ownerId);
                string folderDir = Path.Combine(userDir, folderName);
                string fullDir = string.IsNullOrEmpty(relativePath) ? folderDir : Path.Combine(folderDir, relativePath);
                if (!Directory.Exists(fullDir))
                    Directory.CreateDirectory(fullDir);

                // 3. Nhận file
                string destFilePath = Path.Combine(fullDir, fileName);
                Console.WriteLine($"[DEBUG] Saving to: {destFilePath}");
                
                // Kiểm tra file đã tồn tại
                if (File.Exists(destFilePath))
                {
                    Console.WriteLine($"[WARNING] File already exists: {destFilePath}");
                    // Đọc hết data còn lại để tránh protocol error
                    byte[] skipBuffer = new byte[8192];
                    int remainingBytes = fileSize;
                    while (remainingBytes > 0)
                    {
                        int bytesToRead = Math.Min(skipBuffer.Length, remainingBytes);
                        int bytesRead = await stream.ReadAsync(skipBuffer, 0, bytesToRead);
                        if (bytesRead == 0) break;
                        remainingBytes -= bytesRead;
                    }
                    return "200\n"; // File đã tồn tại, coi như thành công
                }
                
                byte[] buffer = new byte[65536]; // 64KB buffer for faster upload
                int totalRead = 0;
                using (FileStream fs = new FileStream(destFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromMinutes(2)); // 2 minute timeout
                    while (totalRead < fileSize)
                    {
                        try
                        {
                            int bytesRead = await stream.ReadAsync(buffer, 0, Math.Min(buffer.Length, fileSize - totalRead), cts.Token);
                            if (bytesRead == 0) 
                            {
                                Console.WriteLine($"[WARNING] Stream ended early at {totalRead}/{fileSize} bytes");
                                break;
                            }
                            await fs.WriteAsync(buffer, 0, bytesRead, cts.Token);
                            totalRead += bytesRead;
                            
                                                         // Progress logging every 5MB or at completion
                             if (fileSize > 5 * 1024 * 1024 && (totalRead % (5 * 1024 * 1024) == 0 || totalRead == fileSize))
                             {
                                 Console.WriteLine($"[PROGRESS] {fileName}: {totalRead}/{fileSize} bytes ({(totalRead * 100 / fileSize)}%)");
                             }
                        }
                        catch (OperationCanceledException)
                        {
                            Console.WriteLine($"[ERROR] Upload timeout for {fileName}");
                            break;
                        }
                    }
                }
                Console.WriteLine($"[DEBUG] Received {totalRead}/{fileSize} bytes");
                if (totalRead != fileSize)
                {
                    Console.WriteLine($"[ERROR] Incomplete upload: {totalRead}/{fileSize} bytes");
                    return "400\n";
                }

                // 4. Lưu DB
                string fileHash = CalculateSHA256(destFilePath);
                Console.WriteLine($"[DEBUG] File hash: {fileHash}");
                await AddFileToDatabase(fileName, folderId, fileSize, Path.GetExtension(fileName).TrimStart('.').ToLower(), relativePath, fileHash, ownerId, uploadTime);
                Console.WriteLine($"[SUCCESS] Upload completed: {fileName}");
                return "200\n";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi ReceiveFileInFolder: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                return "500\n";
            }
        }

        private static async Task<int> EnsureFolderExists(string folderName, string ownerId)
        {
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    string selectQuery = "SELECT folder_id FROM folders WHERE folder_name = @folderName AND owner_id = @ownerId AND status = 'ACTIVE'";
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(selectQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@folderName", folderName);
                        cmd.Parameters.AddWithValue("@ownerId", int.Parse(ownerId));
                        var result = await cmd.ExecuteScalarAsync();
                        if (result != null && result != DBNull.Value)
                            return Convert.ToInt32(result);
                    }
                    // Chưa có, tạo mới
                    string insertQuery = "INSERT INTO folders (folder_name, owner_id, folder_path, created_at) VALUES (@folderName, @ownerId, @folderPath, @createdAt)";
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@folderName", folderName);
                        cmd.Parameters.AddWithValue("@ownerId", int.Parse(ownerId));
                        cmd.Parameters.AddWithValue("@folderPath", Path.Combine("uploads", ownerId, folderName));
                        cmd.Parameters.AddWithValue("@createdAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        await cmd.ExecuteNonQueryAsync();
                    }
                    // Lấy lại id
                    string getIdQuery = "SELECT last_insert_rowid()";
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(getIdQuery, conn))
                    {
                        var result = await cmd.ExecuteScalarAsync();
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi EnsureFolderExists: {ex.Message}");
                return -1;
            }
        }

        private static async Task AddFileToDatabase(string fileName, int folderId, long fileSize, string fileType, string relativePath, string fileHash, string ownerId, string uploadTime)
        {
            try
            {
                Console.WriteLine($"[DEBUG] Adding to DB: {fileName}, FolderID: {folderId}, Size: {fileSize}");
                
                // Lấy thông tin folder để tạo full path
                string fullFilePath = relativePath;
                using (var conn = new System.Data.SQLite.SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    
                    // Lấy folder_path từ bảng folders
                    string folderQuery = "SELECT folder_path FROM folders WHERE folder_id = @folderId";
                    using (var folderCmd = new System.Data.SQLite.SQLiteCommand(folderQuery, conn))
                    {
                        folderCmd.Parameters.AddWithValue("@folderId", folderId);
                        var folderPath = await folderCmd.ExecuteScalarAsync();
                        if (folderPath != null)
                        {
                            // Tạo full path: uploads\userid\foldername\relativepath\filename
                            if (string.IsNullOrEmpty(relativePath))
                            {
                                fullFilePath = Path.Combine(folderPath.ToString(), fileName);
                            }
                            else
                            {
                                fullFilePath = Path.Combine(folderPath.ToString(), relativePath, fileName);
                            }
                            // Normalize path separators for Windows
                            fullFilePath = fullFilePath.Replace('/', '\\');
                        }
                    }
                    
                    string insertQuery = @"INSERT INTO files (file_name, folder_id, owner_id, file_size, file_type, file_path, file_hash, upload_at) VALUES (@fileName, @folderId, @ownerId, @fileSize, @fileType, @filePath, @fileHash, @uploadAt)";
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@fileName", fileName);
                        cmd.Parameters.AddWithValue("@folderId", folderId);
                        cmd.Parameters.AddWithValue("@ownerId", int.Parse(ownerId));
                        cmd.Parameters.AddWithValue("@fileSize", fileSize);
                        cmd.Parameters.AddWithValue("@fileType", fileType);
                        cmd.Parameters.AddWithValue("@filePath", fullFilePath);
                        cmd.Parameters.AddWithValue("@fileHash", fileHash);
                        cmd.Parameters.AddWithValue("@uploadAt", uploadTime);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                Console.WriteLine($"[SUCCESS] File added to database: {fileName} with path: {fullFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi AddFileToDatabase (in-folder): {ex.Message}");
                Console.WriteLine($"[ERROR] Database error for {fileName}: {ex.StackTrace}");
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

    public class FolderInfo
    {
        public int FolderId { get; set; }
        public string FolderName { get; set; }
        public string FolderPath { get; set; }
        public string CreatedAt { get; set; }
        public bool IsShared { get; set; }
        public string Status { get; set; }
    }
} 