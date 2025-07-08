using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using FileSharingClient;

namespace FileSharingClient.Services
{
    public class ApiService
    {
        private const string SERVER_IP = "127.0.0.1";
        private const int SERVER_PORT = 5000; // Connect directly to backend server for testing
        private const int MAX_RETRY_ATTEMPTS = 3;
        private const int RETRY_DELAY_MS = 500;

        /// <summary>
        /// Retry helper method for API calls
        /// </summary>
        private static async Task<T> RetryApiCall<T>(Func<Task<T>> apiCall, T defaultValue)
        {
            for (int attempt = 0; attempt < MAX_RETRY_ATTEMPTS; attempt++)
            {
                try
                {
                    return await apiCall();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RETRY] Attempt {attempt + 1}/{MAX_RETRY_ATTEMPTS} failed: {ex.Message}");
                    
                    if (attempt == MAX_RETRY_ATTEMPTS - 1)
                    {
                        Console.WriteLine($"[ERROR] All {MAX_RETRY_ATTEMPTS} attempts failed for API call");
                        System.Windows.Forms.MessageBox.Show($"Không thể kết nối đến server sau {MAX_RETRY_ATTEMPTS} lần thử. Vui lòng kiểm tra kết nối mạng.", "Lỗi kết nối", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        return defaultValue;
                    }
                    
                    // Wait before retrying
                    await Task.Delay(RETRY_DELAY_MS * (attempt + 1)); // Exponential backoff
                }
            }
            return defaultValue;
        }

        public static async Task<int> GetUserIdAsync(string username)
        {
            return await RetryApiCall(async () =>
            {
                var connection = await SecureChannelHelper.ConnectToLoadBalancerAsync(SERVER_IP, SERVER_PORT);
                var sslStream = connection.sslStream;
                using (sslStream)
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    string message = $"GET_USER_ID|{username}";
                    await writer.WriteLineAsync(message);
                    await writer.FlushAsync();
                    
                    // Wait a bit for server to process
                    await Task.Delay(100);

                    string response = await reader.ReadLineAsync();
                    response = response?.Trim();

                    if (response != null)
                    {
                        string[] parts = response.Split('|');
                        if (parts.Length >= 2 && parts[0] == "200")
                        {
                            if (int.TryParse(parts[1], out int userId))
                            {
                                return userId;
                            }
                        }
                    }
                    return -1;
                }
            }, -1);
        }

        public static async Task<List<FileItem>> GetUserFilesAsync(int userId, int? folderId = null)
        {
            try
            {
                Console.WriteLine($"[DEBUG][GetUserFilesAsync] Calling with userId: {userId}, folderId: {folderId}");
                
                var connection = await SecureChannelHelper.ConnectToLoadBalancerAsync(SERVER_IP, SERVER_PORT);
                var sslStream = connection.sslStream;
                using (sslStream)
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    string folderParam = folderId.HasValue ? folderId.ToString() : "null";
                    string message = $"GET_USER_FILES_DETAILED|{userId}|{folderParam}";
                    Console.WriteLine($"[DEBUG][GetUserFilesAsync] Sending message: {message}");
                    await writer.WriteLineAsync(message);
                    await writer.FlushAsync();
                    
                    // Wait a bit for server to process
                    await Task.Delay(100);

                    // Try to read response with timeout
                    string response = null;
                    try
                    {
                        response = await reader.ReadLineAsync();
                        response = response?.Trim();
                        Console.WriteLine($"[DEBUG][GetUserFilesAsync] Received response: '{response}'");
                    }
                    catch (Exception readEx)
                    {
                        Console.WriteLine($"[ERROR][GetUserFilesAsync] Error reading response: {readEx.Message}");
                    }

                    if (response != null)
                    {
                        string[] parts = response.Split('|');
                        if (parts.Length >= 2 && parts[0] == "200")
                        {
                            Console.WriteLine($"[DEBUG][GetUserFilesAsync] Parsing data: {parts[1]}");
                            return ParseFileItems(parts[1]);
                        }
                        else
                        {
                            Console.WriteLine($"[DEBUG][GetUserFilesAsync] Invalid response format: {response}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[DEBUG][GetUserFilesAsync] No response received");
                    }
                    return new List<FileItem>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR][GetUserFilesAsync] Exception: {ex.Message}");
                Console.WriteLine($"[ERROR][GetUserFilesAsync] Stack trace: {ex.StackTrace}");
                return new List<FileItem>();
            }
        }

        public static async Task<List<FolderItem>> GetUserFoldersAsync(int userId, int? folderId = null)
        {
            try
            {
                Console.WriteLine($"[DEBUG][GetUserFoldersAsync] Calling with userId: {userId}, folderId: {folderId}");
                
                var connection = await SecureChannelHelper.ConnectToLoadBalancerAsync(SERVER_IP, SERVER_PORT);
                var sslStream = connection.sslStream;
                using (sslStream)
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    string folderParam = folderId.HasValue ? folderId.ToString() : "null";
                    string message = $"GET_USER_FOLDERS|{userId}|{folderParam}";
                    Console.WriteLine($"[DEBUG][GetUserFoldersAsync] Sending message: {message}");
                    await writer.WriteLineAsync(message);
                    await writer.FlushAsync();
                    
                    // Wait a bit for server to process
                    await Task.Delay(100);

                    // Try to read response with timeout
                    string response = null;
                    try
                    {
                        response = await reader.ReadLineAsync();
                        response = response?.Trim();
                        Console.WriteLine($"[DEBUG][GetUserFoldersAsync] Received response: '{response}'");
                    }
                    catch (Exception readEx)
                    {
                        Console.WriteLine($"[ERROR][GetUserFoldersAsync] Error reading response: {readEx.Message}");
                    }

                    if (response != null)
                    {
                        string[] parts = response.Split('|');
                        if (parts.Length >= 2 && parts[0] == "200")
                        {
                            Console.WriteLine($"[DEBUG][GetUserFoldersAsync] Parsing data: {parts[1]}");
                            return ParseFolderItems(parts[1]);
                        }
                        else
                        {
                            Console.WriteLine($"[DEBUG][GetUserFoldersAsync] Invalid response format: {response}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[DEBUG][GetUserFoldersAsync] No response received");
                    }
                    return new List<FolderItem>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR][GetUserFoldersAsync] Exception: {ex.Message}");
                Console.WriteLine($"[ERROR][GetUserFoldersAsync] Stack trace: {ex.StackTrace}");
                return new List<FolderItem>();
            }
        }

        public static async Task<List<FileItem>> GetSharedFilesAsync(int userId)
        {
            try
            {
                Console.WriteLine($"[DEBUG][GetSharedFilesAsync] Calling with userId: {userId}");
                
                var connection = await SecureChannelHelper.ConnectToLoadBalancerAsync(SERVER_IP, SERVER_PORT);
                var sslStream = connection.sslStream;
                using (sslStream)
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    string message = $"GET_SHARED_FILES|{userId}";
                    Console.WriteLine($"[DEBUG][GetSharedFilesAsync] Sending message: {message}");
                    await writer.WriteLineAsync(message);
                    await writer.FlushAsync();
                    
                    // Wait a bit for server to process
                    await Task.Delay(100);

                    // Try to read response with timeout
                    string response = null;
                    try
                    {
                        response = await reader.ReadLineAsync();
                        response = response?.Trim();
                        Console.WriteLine($"[DEBUG][GetSharedFilesAsync] Received response: '{response}'");
                    }
                    catch (Exception readEx)
                    {
                        Console.WriteLine($"[ERROR][GetSharedFilesAsync] Error reading response: {readEx.Message}");
                    }

                    if (response != null)
                    {
                        string[] parts = response.Split('|');
                        if (parts.Length >= 2 && parts[0] == "200")
                        {
                            Console.WriteLine($"[DEBUG][GetSharedFilesAsync] Parsing data: {parts[1]}");
                            return ParseFileItems(parts[1]);
                        }
                        else
                        {
                            Console.WriteLine($"[DEBUG][GetSharedFilesAsync] Invalid response format: {response}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[DEBUG][GetSharedFilesAsync] No response received");
                    }
                    return new List<FileItem>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR][GetSharedFilesAsync] Exception: {ex.Message}");
                Console.WriteLine($"[ERROR][GetSharedFilesAsync] Stack trace: {ex.StackTrace}");
                return new List<FileItem>();
            }
        }

        public static async Task<List<FolderItem>> GetSharedFoldersAsync(int userId)
        {
            try
            {
                Console.WriteLine($"[DEBUG][GetSharedFoldersAsync] Calling with userId: {userId}");
                
                var connection = await SecureChannelHelper.ConnectToLoadBalancerAsync(SERVER_IP, SERVER_PORT);
                var sslStream = connection.sslStream;
                using (sslStream)
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    string message = $"GET_SHARED_FOLDERS|{userId}";
                    Console.WriteLine($"[DEBUG][GetSharedFoldersAsync] Sending message: {message}");
                    await writer.WriteLineAsync(message);
                    await writer.FlushAsync();
                    
                    // Wait a bit for server to process
                    await Task.Delay(100);

                    // Try to read response with timeout
                    string response = null;
                    try
                    {
                        response = await reader.ReadLineAsync();
                        response = response?.Trim();
                        Console.WriteLine($"[DEBUG][GetSharedFoldersAsync] Received response: '{response}'");
                    }
                    catch (Exception readEx)
                    {
                        Console.WriteLine($"[ERROR][GetSharedFoldersAsync] Error reading response: {readEx.Message}");
                    }

                    if (response != null)
                    {
                        string[] parts = response.Split('|');
                        if (parts.Length >= 2 && parts[0] == "200")
                        {
                            Console.WriteLine($"[DEBUG][GetSharedFoldersAsync] Parsing data: {parts[1]}");
                            return ParseFolderItems(parts[1]);
                        }
                        else
                        {
                            Console.WriteLine($"[DEBUG][GetSharedFoldersAsync] Invalid response format: {response}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[DEBUG][GetSharedFoldersAsync] No response received");
                    }
                    return new List<FolderItem>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR][GetSharedFoldersAsync] Exception: {ex.Message}");
                Console.WriteLine($"[ERROR][GetSharedFoldersAsync] Stack trace: {ex.StackTrace}");
                return new List<FolderItem>();
            }
        }

        public static async Task<bool> CreateFolderAsync(int userId, string folderName, int? parentFolderId = null)
        {
            try
            {
                Console.WriteLine($"[DEBUG] CreateFolderAsync called: userId={userId}, folderName={folderName}, parentFolderId={parentFolderId}");
                
                var connection = await SecureChannelHelper.ConnectToLoadBalancerAsync(SERVER_IP, SERVER_PORT);
                var sslStream = connection.sslStream;
                using (sslStream)
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    string parentParam = parentFolderId.HasValue ? parentFolderId.ToString() : "null";
                    string message = $"CREATE_FOLDER|{userId}|{folderName}|{parentParam}";
                    
                    Console.WriteLine($"[DEBUG] Sending message: {message}");
                    await writer.WriteLineAsync(message);

                    string response = await reader.ReadLineAsync();
                    response = response?.Trim();
                    
                    Console.WriteLine($"[DEBUG] CreateFolder response: '{response}'");

                    bool success = response == "200|FOLDER_CREATED";
                    Console.WriteLine($"[DEBUG] CreateFolder success: {success}");
                    
                    return success;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Exception in CreateFolderAsync: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public static async Task<bool> DeleteFileAsync(int fileId)
        {
            try
            {
                var connection = await SecureChannelHelper.ConnectToLoadBalancerAsync(SERVER_IP, SERVER_PORT);
                var sslStream = connection.sslStream;
                using (sslStream)
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    string message = $"DELETE_FILE|{fileId}";
                    await writer.WriteLineAsync(message);

                    string response = await reader.ReadLineAsync();
                    response = response?.Trim();

                    return response == "200|FILE_DELETED";
                }
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> DeleteFolderAsync(int folderId)
        {
            try
            {
                var connection = await SecureChannelHelper.ConnectToLoadBalancerAsync(SERVER_IP, SERVER_PORT);
                var sslStream = connection.sslStream;
                using (sslStream)
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    string message = $"DELETE_FOLDER|{folderId}";
                    await writer.WriteLineAsync(message);

                    string response = await reader.ReadLineAsync();
                    response = response?.Trim();

                    return response == "200|FOLDER_DELETED";
                }
            }
            catch
            {
                return false;
            }
        }

        public static async Task<(byte[] fileBytes, string error)> DownloadFileForPreviewAsync(int fileId)
        {
            try
            {
                var connection = await SecureChannelHelper.ConnectToLoadBalancerAsync(SERVER_IP, SERVER_PORT);
                var sslStream = connection.sslStream;
                using (sslStream)
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    // Get current user ID from session
                    int userId = Session.LoggedInUserId;
                    if (userId == -1)
                    {
                        return (null, "User not logged in");
                    }
                    
                    string message = $"DOWNLOAD_FILE|{fileId}|{userId}";
                    await writer.WriteLineAsync(message);
                    string response = await reader.ReadLineAsync();
                    response = response?.Trim();
                    if (response != null)
                    {
                        string[] parts = response.Split('|');
                        if (parts.Length >= 2 && parts[0] == "200")
                        {
                            byte[] fileBytes = Convert.FromBase64String(parts[1]);
                            return (fileBytes, null);
                        }
                        else if (parts[0] == "404")
                        {
                            return (null, "File not found on server or no access");
                        }
                        else if (parts[0] == "413")
                        {
                            return (null, "FILE_TOO_LARGE");
                        }
                    }
                    return (null, "Invalid response from server");
                }
            }
            catch (Exception ex)
            {
                return (null, $"Download failed: {ex.Message}");
            }
        }

        private static List<FolderItem> ParseFolderItems(string data)
        {
            var folders = new List<FolderItem>();
            if (data == "NO_FOLDERS" || data == "NO_SHARED_FOLDERS") 
            {
                Console.WriteLine($"[DEBUG][ParseFolderItems] No folders found: {data}");
                return folders;
            }
            
            Console.WriteLine($"[DEBUG][ParseFolderItems] Raw data: {data}");
            
            string[] folderStrings = data.Split(';');
            Console.WriteLine($"[DEBUG][ParseFolderItems] Found {folderStrings.Length} folder strings");
            
            foreach (string folderString in folderStrings)
            {
                if (string.IsNullOrEmpty(folderString)) continue;
                
                Console.WriteLine($"[DEBUG][ParseFolderItems] FolderString: {folderString}");
                
                // Format: id:name:created_at:owner_name
                // created_at format: "yyyy-MM-dd HH:mm:ss" (contains colons)
                string[] parts = folderString.Split(':');
                Console.WriteLine($"[DEBUG][ParseFolderItems] Total parts: {parts.Length}");
                for (int i = 0; i < parts.Length; i++)
                {
                    Console.WriteLine($"[DEBUG][ParseFolderItems] parts[{i}]: {parts[i]}");
                }
                
                if (parts.Length >= 4)
                {
                    string id = parts[0];
                    string name = parts[1];
                    
                    // Handle created_at which contains colons: "yyyy-MM-dd HH:mm:ss"
                    // We need to reconstruct the datetime from parts[2], parts[3], parts[4]
                    string createdAt;
                    string owner;
                    
                    if (parts.Length == 4)
                    {
                        // Simple case: id:name:datetime:owner
                        createdAt = parts[2];
                        owner = parts[3];
                    }
                    else
                    {
                        // Complex case: id:name:yyyy-MM-dd:HH:mm:ss:owner
                        // Reconstruct datetime from parts[2], parts[3], parts[4]
                        createdAt = $"{parts[2]}:{parts[3]}:{parts[4]}";
                        owner = parts[5];
                    }
                    
                    Console.WriteLine($"[DEBUG][ParseFolderItems] Parsed folder: {name}, Owner: {owner}");
                    
                    folders.Add(new FolderItem
                    {
                        Id = int.Parse(id),
                        Name = name,
                        CreatedAt = createdAt,
                        Owner = owner,
                        IsShared = false
                    });
                }
                else
                {
                    Console.WriteLine($"[DEBUG][ParseFolderItems] Invalid folder string format: {folderString}");
                }
            }
            
            Console.WriteLine($"[DEBUG][ParseFolderItems] Parsed {folders.Count} folders");
            return folders;
        }

        private static List<FileItem> ParseFileItems(string data)
        {
            var files = new List<FileItem>();
            if (data == "NO_FILES" || data == "NO_SHARED_FILES") return files;
            
            Console.WriteLine($"[DEBUG][ParseFileItems] Raw data: {data}");
            
            string[] fileStrings = data.Split(';');
            foreach (string fileString in fileStrings)
            {
                if (string.IsNullOrEmpty(fileString)) continue;
                
                Console.WriteLine($"[DEBUG][ParseFileItems] FileString: {fileString}");
                
                // Format: id:name:type:size:upload_at:owner_name:path
                // upload_at format: "yyyy-MM-dd HH:mm:ss" (contains colons)
                string[] parts = fileString.Split(':');
                Console.WriteLine($"[DEBUG][ParseFileItems] Total parts: {parts.Length}");
                for (int i = 0; i < parts.Length; i++)
                {
                    Console.WriteLine($"[DEBUG][ParseFileItems] parts[{i}]: {parts[i]}");
                }
                
                if (parts.Length >= 7)
                {
                    string id = parts[0];
                    string name = parts[1];
                    string type = parts[2];
                    string size = parts[3];
                    
                    // Handle upload_at which contains colons: "yyyy-MM-dd HH:mm:ss"
                    string uploadAt;
                    string owner;
                    string path;
                    
                    if (parts.Length == 7)
                    {
                        // Simple case: id:name:type:size:datetime:owner:path
                        uploadAt = parts[4];
                        owner = parts[5];
                        path = parts[6];
                    }
                    else
                    {
                        // Complex case: id:name:type:size:yyyy-MM-dd:HH:mm:ss:owner:path
                        // Reconstruct datetime from parts[4], parts[5], parts[6]
                        uploadAt = $"{parts[4]}:{parts[5]}:{parts[6]}";
                        owner = parts[7];
                        path = parts[8];
                    }
                    
                    Console.WriteLine($"[DEBUG][ParseFileItems] Parsed file: {name}, Owner: {owner}");
                    
                    long sizeBytes = 0;
                    long.TryParse(size, out sizeBytes);
                    
                    files.Add(new FileItem
                    {
                        Id = int.Parse(id),
                        Name = name,
                        Type = type,
                        Size = FormatFileSize(sizeBytes),
                        CreatedAt = uploadAt,
                        Owner = owner,
                        FilePath = path
                    });
                }
            }
            return files;
        }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }

    public class FileItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Size { get; set; }
        public string CreatedAt { get; set; }
        public string Owner { get; set; }
        public string FilePath { get; set; }
        public bool IsShared { get; set; }
        public bool IsFolder { get; set; }
    }

    public class FolderItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CreatedAt { get; set; }
        public string Owner { get; set; }
        public bool IsShared { get; set; }
    }
} 