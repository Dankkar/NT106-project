using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;
using System.Data.SQLite;

namespace FileSharingClient
{
    public partial class TrashBinView: UserControl
    {
        private static string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName;
        private static string dbPath = Path.Combine(projectRoot, "test.db");
        private static string connectionString = $"Data Source={dbPath};Version=3;Pooling=True";
        private int currentUserId = -1;
        private List<TrashFileItem> allTrashFiles = new List<TrashFileItem>();
        private List<TrashFolderItem> allTrashFolders = new List<TrashFolderItem>();
        
        // Event to notify when file is restored from trash
        public static event Action<string> FileRestoredFromTrash;
        
        // Event to notify when folder is restored from trash
        public static event Action<int> FolderRestoredFromTrash;

        public TrashBinView()
        {
            InitializeComponent();
            
            // Configure FlowLayoutPanel
            TrashFileLayoutPanel.FlowDirection = FlowDirection.TopDown;
            TrashFileLayoutPanel.AutoScroll = true;
            TrashFileLayoutPanel.WrapContents = false;
            
            // Set placeholder text style for search
            txtSearch.ForeColor = Color.Gray;
            
            // Subscribe to file deletion events from MyFileView
            MyFileView.FileMovedToTrash += OnFileMovedToTrash;
            MyFileView.FolderMovedToTrash += OnFolderMovedToTrash;
            Console.WriteLine("[DEBUG] TrashBinView - Subscribed to FileMovedToTrash and FolderMovedToTrash events");
            
            // Subscribe to visibility changes to refresh when view becomes visible
            this.VisibleChanged += OnVisibilityChanged;
            
            _ = InitAsync();
        }

        private async Task InitAsync()
        {
            try
            {
                // Use Session class instead of database query
                currentUserId = Session.LoggedInUserId;
                Console.WriteLine($"[DEBUG] TrashBinView - Current User ID: {currentUserId}");
                Console.WriteLine($"[DEBUG] TrashBinView - Session.LoggedInUser: {Session.LoggedInUser}");
                
                if (currentUserId != -1)
                {
                    Console.WriteLine($"[DEBUG] TrashBinView - Starting to load trash files...");
                    await LoadTrashFilesAsync();
                }
                else
                {
                    Console.WriteLine($"[ERROR] TrashBinView - Invalid user ID, showing error message");
                    MessageBox.Show("Không thể lấy thông tin user từ session. Vui lòng đăng nhập lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] TrashBinView - Exception in InitAsync: {ex.Message}");
                Console.WriteLine($"[ERROR] TrashBinView - StackTrace: {ex.StackTrace}");
                MessageBox.Show($"Lỗi khởi tạo TrashBinView: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadTrashFilesAsync()
        {
            try
            {
                TrashFileLayoutPanel.Controls.Clear();
                allTrashFiles.Clear();
                allTrashFolders.Clear();
                
                Console.WriteLine($"[DEBUG] TrashBinView - Loading trash files and folders for user {currentUserId}");
                
                // Test server connection first
                Console.WriteLine("[DEBUG] TrashBinView - Testing server connection...");
                
                // Get trash files from server
                string requestString = $"GET_TRASH_FILES|{currentUserId}";
                Console.WriteLine($"[DEBUG] TrashBinView - Sending request: {requestString}");
                
                string response = await SendRequestToServer(requestString);
                
                Console.WriteLine($"[DEBUG] TrashBinView - Server response: '{response}'");
                Console.WriteLine($"[DEBUG] TrashBinView - Response length: {response?.Length ?? 0}");
                
                if (string.IsNullOrEmpty(response))
                {
                    Console.WriteLine("[ERROR] TrashBinView - Server returned empty response");
                    MessageBox.Show("Server không phản hồi. Vui lòng kiểm tra kết nối.", "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                if (response.StartsWith("200|"))
                {
                    string filesData = response.Substring(4).Trim();
                    Console.WriteLine($"[DEBUG] TrashBinView - Extracted files data: '{filesData}'");
                    
                    if (filesData == "NO_TRASH_FILES")
                    {
                        Console.WriteLine("[DEBUG] TrashBinView - No trash files found");
                        ShowNoFilesMessage();
                        return;
                    }
                    
                    // Parse files data: fileName:deletedAt;fileName:deletedAt;...
                    string[] files = filesData.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    Console.WriteLine($"[DEBUG] TrashBinView - Found {files.Length} files");
                    Console.WriteLine($"[DEBUG] TrashBinView - Files array: [{string.Join(", ", files)}]");
                    
                    foreach (string file in files)
                    {
                        if (string.IsNullOrEmpty(file)) continue;
                        
                        try
                        {
                            string[] parts = file.Split(':');
                            if (parts.Length >= 2)
                            {
                                string fileName = parts[0];
                                string deletedAt = parts[1];
                                
                                Console.WriteLine($"[DEBUG] TrashBinView - Processing file: {fileName}");
                                
                                // Validate fileName
                                if (string.IsNullOrWhiteSpace(fileName))
                                {
                                    Console.WriteLine("[DEBUG] TrashBinView - Skipping invalid fileName");
                                    continue;
                                }
                                
                                // Get file info for size and extension
                                var fileInfo = await GetFileInfoFromServer(fileName);
                                
                                // Parse deleted date safely
                                string formattedDeletedAt = "N/A";
                                try
                                {
                                    formattedDeletedAt = DateTime.Parse(deletedAt).ToString("dd/MM/yyyy HH:mm");
                                }
                                catch
                                {
                                    formattedDeletedAt = deletedAt; // Use raw date if parsing fails
                                }
                                
                                var trashFile = new TrashFileItem
                                {
                                    FileName = fileName,
                                    DeletedAt = formattedDeletedAt,
                                    Owner = "Me",
                                    FileSize = fileInfo.Size,
                                    FileType = fileInfo.Extension
                                };
                                
                                allTrashFiles.Add(trashFile);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[ERROR] TrashBinView - Error processing file '{file}': {ex.Message}");
                            // Continue with next file instead of crashing
                        }
                    }
                    
                    Console.WriteLine($"[DEBUG] TrashBinView - Successfully loaded {allTrashFiles.Count} files");
                }
                else if (response.StartsWith("404|"))
                {
                    Console.WriteLine($"[DEBUG] TrashBinView - No files found (404)");
                    // Still load folders even if no files found
                }
                else if (response.StartsWith("500|"))
                {
                    Console.WriteLine($"[ERROR] TrashBinView - Server internal error: {response}");
                    MessageBox.Show("Lỗi server khi tải danh sách file đã xóa: " + response, "Lỗi server", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else
                {
                    Console.WriteLine($"[ERROR] TrashBinView - Unexpected server response: {response}");
                    MessageBox.Show("Phản hồi không mong đợi từ server: " + response, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                // Always load folders regardless of files result
                await LoadTrashFoldersAsync();
                DisplayTrashFiles();
                
                Console.WriteLine($"[DEBUG] TrashBinView - Final counts: {allTrashFiles.Count} files, {allTrashFolders.Count} folders");
                Console.WriteLine($"[DEBUG] TrashBinView - Added {TrashFileLayoutPanel.Controls.Count} controls to FlowLayoutPanel");
                
                // Show no items message if both files and folders are empty
                if (allTrashFiles.Count == 0 && allTrashFolders.Count == 0)
                {
                    ShowNoFilesMessage();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] TrashBinView - Exception in LoadTrashFilesAsync: {ex.Message}");
                Console.WriteLine($"[ERROR] TrashBinView - Exception type: {ex.GetType().Name}");
                Console.WriteLine($"[ERROR] TrashBinView - StackTrace: {ex.StackTrace}");
                
                string detailedError = $"Chi tiết lỗi:\n- Type: {ex.GetType().Name}\n- Message: {ex.Message}";
                if (ex.InnerException != null)
                {
                    detailedError += $"\n- Inner: {ex.InnerException.Message}";
                }
                
                MessageBox.Show("Lỗi tải danh sách file đã xóa:\n" + detailedError, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadTrashFoldersAsync()
        {
            try
            {
                Console.WriteLine($"[DEBUG] TrashBinView - Loading trash folders for user {currentUserId}");
                
                string requestString = $"GET_TRASH_FOLDERS|{currentUserId}";
                Console.WriteLine($"[DEBUG] TrashBinView - Sending folder request: {requestString}");
                
                string response = await SendRequestToServer(requestString);
                Console.WriteLine($"[DEBUG] TrashBinView - Folder response: '{response}'");
                
                if (string.IsNullOrEmpty(response))
                {
                    Console.WriteLine("[ERROR] TrashBinView - Server returned empty response for folders");
                    return;
                }
                
                if (response.StartsWith("200|"))
                {
                    string foldersData = response.Substring(4).Trim();
                    Console.WriteLine($"[DEBUG] TrashBinView - Extracted folders data: '{foldersData}'");
                    
                    if (foldersData == "NO_TRASH_FOLDERS")
                    {
                        Console.WriteLine("[DEBUG] TrashBinView - No trash folders found");
                        return;
                    }
                    
                    // Parse folders data: folderId:folderName:deletedAt;folderId:folderName:deletedAt;...
                    string[] folders = foldersData.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    Console.WriteLine($"[DEBUG] TrashBinView - Found {folders.Length} folders");
                    
                    foreach (string folder in folders)
                    {
                        if (string.IsNullOrEmpty(folder)) continue;
                        
                        try
                        {
                            string[] parts = folder.Split(':');
                            if (parts.Length >= 3)
                            {
                                int folderId = int.Parse(parts[0]);
                                string folderName = parts[1];
                                string deletedAt = parts[2];
                                
                                Console.WriteLine($"[DEBUG] TrashBinView - Processing folder: {folderName} (ID: {folderId})");
                                
                                // Validate folderName
                                if (string.IsNullOrWhiteSpace(folderName))
                                {
                                    Console.WriteLine("[DEBUG] TrashBinView - Skipping invalid folderName");
                                    continue;
                                }
                                
                                // Parse deleted date safely
                                string formattedDeletedAt = "N/A";
                                try
                                {
                                    formattedDeletedAt = DateTime.Parse(deletedAt).ToString("dd/MM/yyyy HH:mm");
                                }
                                catch
                                {
                                    formattedDeletedAt = deletedAt; // Use raw date if parsing fails
                                }
                                
                                var trashFolder = new TrashFolderItem
                                {
                                    FolderId = folderId,
                                    FolderName = folderName,
                                    DeletedAt = formattedDeletedAt,
                                    Owner = "Me"
                                };
                                
                                allTrashFolders.Add(trashFolder);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[ERROR] TrashBinView - Error processing folder '{folder}': {ex.Message}");
                            // Continue with next folder instead of crashing
                        }
                    }
                    
                    Console.WriteLine($"[DEBUG] TrashBinView - Successfully loaded {allTrashFolders.Count} folders");
                }
                else if (response.StartsWith("404|"))
                {
                    Console.WriteLine($"[DEBUG] TrashBinView - No folders found (404)");
                }
                else if (response.StartsWith("500|"))
                {
                    Console.WriteLine($"[ERROR] TrashBinView - Server internal error for folders: {response}");
                }
                else
                {
                    Console.WriteLine($"[ERROR] TrashBinView - Unexpected server response for folders: {response}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] TrashBinView - Exception in LoadTrashFoldersAsync: {ex.Message}");
                Console.WriteLine($"[ERROR] TrashBinView - StackTrace: {ex.StackTrace}");
            }
        }

        private void DisplayTrashFiles()
        {
            TrashFileLayoutPanel.Controls.Clear();
            
            // Display folders first (with light blue background)
            foreach (var folder in allTrashFolders)
            {
                var folderControl = new TrashFolderItemControl(
                    folder.FolderId,
                    folder.FolderName,
                    folder.DeletedAt,
                    folder.Owner
                );
                
                // Subscribe to folder events
                folderControl.FolderRestored += OnFolderRestored;
                folderControl.FolderPermanentlyDeleted += OnFolderPermanentlyDeleted;
                
                TrashFileLayoutPanel.Controls.Add(folderControl);
            }
            
            // Then display files
            foreach (var file in allTrashFiles)
            {
                var control = new TrashFileItemControl(
                    file.FileName,
                    file.DeletedAt,
                    file.Owner,
                    file.FileSize,
                    file.FileType
                );
                
                // Subscribe to events
                control.FileRestored += OnFileRestored;
                control.FilePermanentlyDeleted += OnFilePermanentlyDeleted;
                
                TrashFileLayoutPanel.Controls.Add(control);
            }
        }

        private void ShowNoFilesMessage()
        {
            // Only show empty message if both files and folders are empty
            if (allTrashFiles.Count == 0 && allTrashFolders.Count == 0)
            {
                var noFilesLabel = new Label
                {
                    Text = "Thùng rác trống",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.Gray,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill,
                    Height = 100
                };
                TrashFileLayoutPanel.Controls.Add(noFilesLabel);
            }
        }

        private async void OnFileRestored(string fileName)
        {
            try
            {
                string response = await SendRequestToServer($"RESTORE_FILE|{fileName}|{currentUserId}");
                
                if (response.StartsWith("200|"))
                {
                    MessageBox.Show("Phục hồi file thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Console.WriteLine($"[DEBUG] TrashBinView - File '{fileName}' restored, refreshing list");
                    await LoadTrashFilesAsync(); // Refresh the list
                    
                    // Notify MyFileView that file has been restored
                    FileRestoredFromTrash?.Invoke(fileName);
                    Console.WriteLine($"[DEBUG] TrashBinView - FileRestoredFromTrash event fired for: {fileName}");
                }
                else
                {
                    MessageBox.Show("Lỗi phục hồi file: " + response, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi phục hồi file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void OnFolderRestored(int folderId)
        {
            try
            {
                string response = await SendRequestToServer($"RESTORE_FOLDER|{folderId}|{currentUserId}");
                
                if (response.StartsWith("200|"))
                {
                    MessageBox.Show("Phục hồi folder thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Console.WriteLine($"[DEBUG] TrashBinView - Folder '{folderId}' restored, refreshing list");
                    await LoadTrashFilesAsync(); // Refresh the list
                    
                    // Notify MyFileView that folder has been restored
                    FolderRestoredFromTrash?.Invoke(folderId);
                    Console.WriteLine($"[DEBUG] TrashBinView - FolderRestoredFromTrash event fired for: {folderId}");
                }
                else
                {
                    MessageBox.Show("Lỗi phục hồi folder: " + response, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi phục hồi folder: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void OnFolderPermanentlyDeleted(int folderId)
        {
            try
            {
                string response = await SendRequestToServer($"PERMANENTLY_DELETE_FOLDER|{folderId}|{currentUserId}");
                
                if (response.StartsWith("200|"))
                {
                    MessageBox.Show("Xóa vĩnh viễn folder thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Console.WriteLine($"[DEBUG] TrashBinView - Folder '{folderId}' permanently deleted, refreshing list");
                    await LoadTrashFilesAsync(); // Refresh the list
                }
                else
                {
                    MessageBox.Show("Lỗi xóa vĩnh viễn folder: " + response, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa vĩnh viễn folder: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void OnFilePermanentlyDeleted(string fileName)
        {
            try
            {
                string response = await SendRequestToServer($"PERMANENTLY_DELETE_FILE|{fileName}|{currentUserId}");
                
                if (response.StartsWith("200|"))
                {
                    MessageBox.Show("Xóa vĩnh viễn file thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Console.WriteLine($"[DEBUG] TrashBinView - File '{fileName}' permanently deleted, refreshing list");
                    await LoadTrashFilesAsync(); // Refresh the list
                }
                else
                {
                    MessageBox.Show("Lỗi xóa vĩnh viễn file: " + response, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa vĩnh viễn file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<(string Size, string Extension)> GetFileInfoFromServer(string fileName)
        {
            try
            {
                string response = await SendRequestToServer($"GET_FILE_INFO|{fileName}|{currentUserId}");
                
                if (response.StartsWith("200|"))
                {
                    string[] parts = response.Substring(4).Split('|');
                    if (parts.Length >= 2)
                    {
                        string filePath = parts[0];
                        string fileType = parts[1];
                        
                        // Get file size if file exists and path is valid
                        string size = "N/A";
                        if (IsValidPath(filePath))
                        {
                            try
                            {
                                if (File.Exists(filePath))
                                {
                                    FileInfo fileInfo = new FileInfo(filePath);
                                    size = FormatFileSize(fileInfo.Length);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[DEBUG] TrashBinView - Error accessing file {filePath}: {ex.Message}");
                                // File path may have issues, but we can continue without size
                            }
                        }
                        
                        return (size, fileType);
                    }
                }
                
                return ("N/A", GetSafeFileExtension(fileName));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] TrashBinView - Error in GetFileInfoFromServer: {ex.Message}");
                return ("N/A", GetSafeFileExtension(fileName));
            }
        }

        private bool IsValidPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            try
            {
                // Check for invalid characters
                char[] invalidChars = Path.GetInvalidPathChars();
                if (path.IndexOfAny(invalidChars) >= 0)
                    return false;

                // Try to get full path - this will throw if path is invalid
                Path.GetFullPath(path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string GetSafeFileExtension(string fileName)
        {
            try
            {
                return Path.GetExtension(fileName);
            }
            catch
            {
                // If fileName has invalid characters, try to extract extension manually
                int lastDot = fileName.LastIndexOf('.');
                if (lastDot >= 0 && lastDot < fileName.Length - 1)
                {
                    return fileName.Substring(lastDot);
                }
                return "";
            }
        }

        private async Task<string> SendRequestToServer(string request)
        {
            try
            {
                Console.WriteLine($"[DEBUG] SendRequestToServer - Connecting to 127.0.0.1:5000");
                
                using (TcpClient client = new TcpClient())
                {
                    // Set timeout
                    client.ReceiveTimeout = 10000; // 10 seconds
                    client.SendTimeout = 10000; // 10 seconds
                    
                    await client.ConnectAsync("127.0.0.1", 5000);
                    Console.WriteLine($"[DEBUG] SendRequestToServer - Connected successfully");
                    
                    NetworkStream stream = client.GetStream();
                    
                    // Send request
                    string fullRequest = request + "\n";
                    byte[] requestData = Encoding.UTF8.GetBytes(fullRequest);
                    Console.WriteLine($"[DEBUG] SendRequestToServer - Sending: '{fullRequest.Trim()}'");
                    Console.WriteLine($"[DEBUG] SendRequestToServer - Request bytes: {requestData.Length}");
                    
                    await stream.WriteAsync(requestData, 0, requestData.Length);
                    Console.WriteLine($"[DEBUG] SendRequestToServer - Request sent");
                    
                    // Read response using StreamReader for proper text handling
                    using (StreamReader responseReader = new StreamReader(stream, Encoding.UTF8))
                    {
                        Console.WriteLine($"[DEBUG] SendRequestToServer - Waiting for response...");
                        
                        // Read the full response - could be multiline
                        StringBuilder responseBuilder = new StringBuilder();
                        string line;
                        bool firstLine = true;
                        
                        while ((line = await responseReader.ReadLineAsync()) != null)
                        {
                            if (!firstLine)
                            {
                                responseBuilder.Append("\n");
                            }
                            responseBuilder.Append(line);
                            firstLine = false;
                            
                            // For most responses, we expect just one line
                            // But allow for potential multiline responses
                            break;
                        }
                        
                        string response = responseBuilder.ToString();
                        Console.WriteLine($"[DEBUG] SendRequestToServer - Received {response.Length} characters");
                        Console.WriteLine($"[DEBUG] SendRequestToServer - Response: '{response}'");
                        
                        if (string.IsNullOrEmpty(response))
                        {
                            Console.WriteLine("[ERROR] SendRequestToServer - Empty response from server");
                            return "";
                        }
                        
                        return response;
                    }
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"[ERROR] SendRequestToServer - Socket error: {ex.Message}");
                Console.WriteLine($"[ERROR] SendRequestToServer - Error Code: {ex.ErrorCode}");
                throw new Exception($"Không thể kết nối đến server (Socket): {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] SendRequestToServer - General error: {ex.Message}");
                Console.WriteLine($"[ERROR] SendRequestToServer - Exception type: {ex.GetType().Name}");
                throw new Exception($"Lỗi kết nối server: {ex.Message}");
            }
        }

        // DEPRECATED: Use Session.LoggedInUserId instead
        // private async Task<int> GetUserIdFromSessionAsync()
        // {
        //     try
        //     {
        //         Console.WriteLine($"[DEBUG] TrashBinView - Getting user ID from session");
        //         Console.WriteLine($"[DEBUG] TrashBinView - Connection string: {connectionString}");
        //         
        //         using (var conn = new SQLiteConnection(connectionString))
        //         {
        //             await conn.OpenAsync();
        //             string query = "SELECT user_id FROM session WHERE is_active = 1 ORDER BY created_at DESC LIMIT 1";
        //             Console.WriteLine($"[DEBUG] TrashBinView - Query: {query}");
        //             
        //             using (var cmd = new SQLiteCommand(query, conn))
        //             {
        //                 var result = await cmd.ExecuteScalarAsync();
        //                 Console.WriteLine($"[DEBUG] TrashBinView - Query result: {result}");
        //                 
        //                 if (result != null)
        //                 {
        //                     int userId = Convert.ToInt32(result);
        //                     Console.WriteLine($"[DEBUG] TrashBinView - User ID: {userId}");
        //                     return userId;
        //                 }
        //             }
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"[ERROR] TrashBinView - Error getting user ID from session: {ex.Message}");
        //         Console.WriteLine($"[ERROR] TrashBinView - Stack trace: {ex.StackTrace}");
        //     }
        //     return -1;
        // }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;
            
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            
            return $"{size:0.##} {sizes[order]}";
        }

        public async Task RefreshTrashFiles()
        {
            await LoadTrashFilesAsync();
        }

        private async void OnFileMovedToTrash(string fileName)
        {
            try
            {
                Console.WriteLine($"[DEBUG] TrashBinView - Received FileMovedToTrash event for: {fileName}");
                
                // Always refresh to ensure files are loaded, regardless of visibility
                Console.WriteLine("[DEBUG] TrashBinView - Auto-refreshing trash files due to new deletion");
                await LoadTrashFilesAsync();
                Console.WriteLine($"[DEBUG] TrashBinView - Refresh completed after file {fileName} deletion");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] TrashBinView - Error in OnFileMovedToTrash: {ex.Message}");
            }
        }

        private async void OnFolderMovedToTrash(int folderId)
        {
            try
            {
                Console.WriteLine($"[DEBUG] TrashBinView - Received FolderMovedToTrash event for: {folderId}");
                
                // Always refresh to ensure folders are loaded, regardless of visibility
                Console.WriteLine("[DEBUG] TrashBinView - Auto-refreshing trash items due to new folder deletion");
                await LoadTrashFilesAsync();
                Console.WriteLine($"[DEBUG] TrashBinView - Refresh completed after folder {folderId} deletion");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] TrashBinView - Error in OnFolderMovedToTrash: {ex.Message}");
            }
        }

        private async void OnVisibilityChanged(object sender, EventArgs e)
        {
            try
            {
                // Only refresh if we're becoming visible and have been initialized
                if (this.Visible && currentUserId != -1)
                {
                    Console.WriteLine("[DEBUG] TrashBinView - Becoming visible, refreshing trash files");
                    await LoadTrashFilesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] TrashBinView - Error in OnVisibilityChanged: {ex.Message}");
            }
        }

        // Toolbar event handlers
        private async void btnRestoreAll_Click(object sender, EventArgs e)
        {
            int totalItems = allTrashFiles.Count + allTrashFolders.Count;
            if (totalItems == 0)
            {
                MessageBox.Show("Không có file hoặc folder nào trong thùng rác.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show(
                $"Bạn có chắc chắn muốn phục hồi tất cả {totalItems} item ({allTrashFiles.Count} file, {allTrashFolders.Count} folder)?",
                "Xác nhận phục hồi tất cả",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                int successCount = 0;
                List<string> restoredFiles = new List<string>();
                List<int> restoredFolders = new List<int>();
                
                // Restore files
                foreach (var file in allTrashFiles)
                {
                    try
                    {
                        string response = await SendRequestToServer($"RESTORE_FILE|{file.FileName}|{currentUserId}");
                        if (response.StartsWith("200|"))
                        {
                            successCount++;
                            restoredFiles.Add(file.FileName);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error restoring file {file.FileName}: {ex.Message}");
                    }
                }
                
                // Restore folders
                foreach (var folder in allTrashFolders)
                {
                    try
                    {
                        string response = await SendRequestToServer($"RESTORE_FOLDER|{folder.FolderId}|{currentUserId}");
                        if (response.StartsWith("200|"))
                        {
                            successCount++;
                            restoredFolders.Add(folder.FolderId);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error restoring folder {folder.FolderName}: {ex.Message}");
                    }
                }

                MessageBox.Show($"Đã phục hồi {successCount} item thành công!", "Kết quả", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadTrashFilesAsync();
                
                // Fire events for all successfully restored items
                foreach (string fileName in restoredFiles)
                {
                    FileRestoredFromTrash?.Invoke(fileName);
                    Console.WriteLine($"[DEBUG] TrashBinView - FileRestoredFromTrash event fired for: {fileName}");
                }
                
                foreach (int folderId in restoredFolders)
                {
                    FolderRestoredFromTrash?.Invoke(folderId);
                    Console.WriteLine($"[DEBUG] TrashBinView - FolderRestoredFromTrash event fired for: {folderId}");
                }
            }
        }

        private async void btnEmptyTrash_Click(object sender, EventArgs e)
        {
            int totalItems = allTrashFiles.Count + allTrashFolders.Count;
            if (totalItems == 0)
            {
                MessageBox.Show("Thùng rác đã trống.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa vĩnh viễn tất cả {totalItems} item ({allTrashFiles.Count} file, {allTrashFolders.Count} folder)?\n\nHành động này không thể hoàn tác!",
                "Xác nhận dọn thùng rác",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                int successCount = 0;
                
                // Delete files permanently
                foreach (var file in allTrashFiles)
                {
                    try
                    {
                        string response = await SendRequestToServer($"PERMANENTLY_DELETE_FILE|{file.FileName}|{currentUserId}");
                        if (response.StartsWith("200|"))
                        {
                            successCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error permanently deleting file {file.FileName}: {ex.Message}");
                    }
                }
                
                // Delete folders permanently
                foreach (var folder in allTrashFolders)
                {
                    try
                    {
                        string response = await SendRequestToServer($"PERMANENTLY_DELETE_FOLDER|{folder.FolderId}|{currentUserId}");
                        if (response.StartsWith("200|"))
                        {
                            successCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error permanently deleting folder {folder.FolderName}: {ex.Message}");
                    }
                }

                MessageBox.Show($"Đã xóa vĩnh viễn {successCount} item!", "Kết quả", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadTrashFilesAsync();
            }
        }

        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            await LoadTrashFilesAsync();
        }

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            await PerformSearch();
        }

        private async Task PerformSearch()
        {
            string searchTerm = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(searchTerm) || searchTerm == "Tìm kiếm file...")
            {
                DisplayTrashFiles(); // Show all files and folders
                return;
            }

            var filteredFiles = allTrashFiles.Where(f => 
                f.FileName.ToLower().Contains(searchTerm.ToLower())
            ).ToList();
            
            var filteredFolders = allTrashFolders.Where(f => 
                f.FolderName.ToLower().Contains(searchTerm.ToLower())
            ).ToList();

            TrashFileLayoutPanel.Controls.Clear();

            if (filteredFiles.Count == 0 && filteredFolders.Count == 0)
            {
                var noResultsLabel = new Label
                {
                    Text = $"Không tìm thấy file hoặc folder nào chứa '{searchTerm}'",
                    Font = new Font("Segoe UI", 10, FontStyle.Italic),
                    ForeColor = Color.Gray,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Height = 50
                };
                TrashFileLayoutPanel.Controls.Add(noResultsLabel);
                return;
            }

            // Display filtered folders first
            foreach (var folder in filteredFolders)
            {
                var folderControl = new TrashFolderItemControl(
                    folder.FolderId,
                    folder.FolderName,
                    folder.DeletedAt,
                    folder.Owner
                );
                
                folderControl.FolderRestored += OnFolderRestored;
                folderControl.FolderPermanentlyDeleted += OnFolderPermanentlyDeleted;
                
                TrashFileLayoutPanel.Controls.Add(folderControl);
            }
            
            // Then display filtered files
            foreach (var file in filteredFiles)
            {
                var control = new TrashFileItemControl(
                    file.FileName,
                    file.DeletedAt,
                    file.Owner,
                    file.FileSize,
                    file.FileType
                );
                
                control.FileRestored += OnFileRestored;
                control.FilePermanentlyDeleted += OnFilePermanentlyDeleted;
                
                TrashFileLayoutPanel.Controls.Add(control);
            }
        }

        private void txtSearch_Enter(object sender, EventArgs e)
        {
            if (txtSearch.Text == "Tìm kiếm file...")
            {
                txtSearch.Text = "";
                txtSearch.ForeColor = Color.Black;
            }
        }

        private void txtSearch_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.Text = "Tìm kiếm file...";
                txtSearch.ForeColor = Color.Gray;
            }
        }

        private void CleanupEvents()
        {
            try
            {
                // Unsubscribe from events to prevent memory leaks
                MyFileView.FileMovedToTrash -= OnFileMovedToTrash;
                MyFileView.FolderMovedToTrash -= OnFolderMovedToTrash;
                this.VisibleChanged -= OnVisibilityChanged;
                Console.WriteLine("[DEBUG] TrashBinView - Unsubscribed from events");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] TrashBinView - Error unsubscribing events: {ex.Message}");
            }
        }

        private class TrashFileItem
        {
            public string FileName { get; set; }
            public string DeletedAt { get; set; }
            public string Owner { get; set; }
            public string FileSize { get; set; }
            public string FileType { get; set; }
        }

        private class TrashFolderItem
        {
            public int FolderId { get; set; }
            public string FolderName { get; set; }
            public string DeletedAt { get; set; }
            public string Owner { get; set; }
        }
    }
}
