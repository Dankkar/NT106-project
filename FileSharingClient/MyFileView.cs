using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using FileSharingClient.Services;
using System.Security.Cryptography;

namespace FileSharingClient
{
    public partial class MyFileView : UserControl
    {
        private string serverIp = ConfigurationManager.AppSettings["ServerIP"];
        private int serverPort = int.Parse(ConfigurationManager.AppSettings["ServerPort"]);
        private int chunkSize = int.Parse(ConfigurationManager.AppSettings["ChunkSize"]);
        private long maxFileSize = long.Parse(ConfigurationManager.AppSettings["MaxFileSizeMB"]) * 1024 * 1024;
        private string uploadsPath = ConfigurationManager.AppSettings["UploadsPath"];
        private string databasePath = ConfigurationManager.AppSettings["DatabasePath"];

        private List<Services.FileItem> allFiles = new List<Services.FileItem>();
        private List<Services.FolderItem> allFolders = new List<Services.FolderItem>();
        private int? currentFolderId = null; // null = root level
        private Stack<FolderNavigationItem> navigationStack = new Stack<FolderNavigationItem>();

        // Thêm event để main form bắt chuyển tab
        public event Action RequestUploadTab;
        public event Action<int> FolderDownloadRequested;

        public MyFileView()
        {
            InitializeComponent();
            MyFileLayoutPanel.FlowDirection = FlowDirection.TopDown;
            MyFileLayoutPanel.AutoScroll = true;
            MyFileLayoutPanel.WrapContents = false;
            // Set placeholder text style
            txtSearch.ForeColor = Color.Gray;
            
            // Add KeyPress event handler for search length validation
            txtSearch.KeyPress += txtSearch_KeyPress;
            
            // Subscribe to TrashBinView events
            TrashBinView.FileRestoredFromTrash += OnFileRestoredFromTrash;
            TrashBinView.FolderRestoredFromTrash += OnFolderRestoredFromTrash;
            _ = InitAsync();
        }

        private async Task InitAsync()
        {
            await LoadFoldersAndFilesAsync();
        }

        private async Task LoadFoldersAndFilesAsync()
        {
            try
            {
                allFiles.Clear();
                allFolders.Clear();
                MyFileLayoutPanel.Controls.Clear();
                if (Session.LoggedInUserId == -1)
                {
                    MessageBox.Show("currentUserId is -1, cannot load files", "Debug", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                // Load ONLY user's own folders (from folders table where owner_id = current user)
                var userFolders = await ApiService.GetUserFoldersAsync(Session.LoggedInUserId, currentFolderId);
                allFolders.AddRange(userFolders);
                // Load ONLY user's own files (from files table where owner_id = current user)
                var userFiles = await ApiService.GetUserFilesAsync(Session.LoggedInUserId, currentFolderId);
                allFiles.AddRange(userFiles);
                // Display folders and files
                DisplayFoldersAndFiles();
                UpdateBreadcrumb();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading folders and files: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayFoldersAndFiles()
        {
            MyFileLayoutPanel.Controls.Clear();
            // Add back button if not at root
            if (currentFolderId != null)
            {
                var backControl = CreateBackButton();
                MyFileLayoutPanel.Controls.Add(backControl);
            }
            // Add folders first
            foreach (var folder in allFolders)
            {
                var folderControl = new FolderItemControl(folder.Name, folder.CreatedAt, folder.Owner, folder.IsShared, folder.Id);
                folderControl.FolderClicked += async (folderId) => await NavigateToFolderById(folderId);
                folderControl.FolderDeleted += async (folderId) => await OnFolderDeleted(folderId);
                folderControl.FolderShareRequested += async (folderIdStr) => await OnFolderShareRequested(int.Parse(folderIdStr), folder.Name);
                folderControl.FolderDownloadRequested += async (folderId) => await OnFolderDownloadRequested(folderId);
                MyFileLayoutPanel.Controls.Add(folderControl);
            }
            // Add files
            foreach (var file in allFiles)
            {
                var fileItemControl = new FileItemControl(file.Name, file.CreatedAt, file.Owner, file.Size, file.FilePath, file.Id);
                fileItemControl.FileDeleted += async (filePath) => await OnFileDeleted(filePath);
                fileItemControl.FileShareRequested += async (fileIdStr) => await OnFileShareRequested(int.Parse(fileIdStr), file.Name);
                MyFileLayoutPanel.Controls.Add(fileItemControl);
            }
        }

        private Control CreateBackButton()
        {
            var backButton = new Button()
            {
                Text = "← Back",
                Size = new Size(100, 40),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat
            };
            
            backButton.Click += async (s, e) => await NavigateBack();
            return backButton;
        }

        private Control CreateFolderControl(Services.FolderItem folder)
        {
            var panel = new Panel()
            {
                Size = new Size(1000, 40),
                BorderStyle = BorderStyle.FixedSingle,
                //BackColor = Color.LightBlue,
                Cursor = Cursors.Hand
            };
            
            var iconLabel = new Label()
            {
                Text = "📁",
                Font = new Font("Segoe UI", 12),
                Location = new Point(10, 10),
                Size = new Size(30, 20),
                BackColor = Color.Transparent
            };
            
            var nameLabel = new Label()
            {
                Text = folder.Name,
                Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold),
                Location = new Point(50, 12),
                Size = new Size(200, 17),
                BackColor = Color.Transparent
            };
            
            var ownerLabel = new Label()
            {
                Text = folder.Owner,
                Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold),
                Location = new Point(260, 12),
                Size = new Size(150, 17),
                BackColor = Color.Transparent
            };
            
            var dateLabel = new Label()
            {
                Text = folder.CreatedAt,
                Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold),
                Location = new Point(420, 12),
                Size = new Size(100, 17),
                BackColor = Color.Transparent
            };
            
            var typeLabel = new Label()
            {
                Text = "Folder",
                Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold),
                Location = new Point(530, 12),
                Size = new Size(60, 17),
                BackColor = Color.Transparent
            };
            
            panel.Controls.Add(iconLabel);
            panel.Controls.Add(nameLabel);
            panel.Controls.Add(ownerLabel);
            panel.Controls.Add(dateLabel);
            panel.Controls.Add(typeLabel);
            
            // Click event to navigate into folder
            panel.Click += async (s, e) => await NavigateToFolder(folder);
            iconLabel.Click += async (s, e) => await NavigateToFolder(folder);
            nameLabel.Click += async (s, e) => await NavigateToFolder(folder);
            ownerLabel.Click += async (s, e) => await NavigateToFolder(folder);
            dateLabel.Click += async (s, e) => await NavigateToFolder(folder);
            typeLabel.Click += async (s, e) => await NavigateToFolder(folder);
            
            return panel;
        }

        private async Task NavigateToFolderById(int folderId)
        {
            // Find the folder in allFolders
            var folder = allFolders.FirstOrDefault(f => f.Id == folderId);
            if (folder != null)
            {
                await NavigateToFolder(folder);
            }
        }

        private async Task NavigateToFolder(Services.FolderItem folder)
        {
            // Save current state to navigation stack
            navigationStack.Push(new FolderNavigationItem 
            { 
                FolderId = currentFolderId, 
                FolderName = currentFolderId == null ? "Root" : GetCurrentFolderName() 
            });
            
            currentFolderId = folder.Id;
            
            // Load user's own folder contents only
            await LoadFoldersAndFilesAsync();
        }

        private async Task OnFolderDeleted(int folderId)
        {
            try
            {
                int userId = await GetUserIdFromSessionAsync();
                
                Console.WriteLine($"[DEBUG] OnFolderDeleted called with folderId: {folderId}");
                Console.WriteLine($"[DEBUG] Current userId: {userId}");
                
                if (userId == -1)
                {
                    MessageBox.Show("Không thể xác định người dùng hiện tại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Send DELETE_FOLDER request to server
                bool success = await DeleteFolderOnServer(folderId.ToString(), userId);
                
                if (success)
                {
                    MessageBox.Show("Folder đã được chuyển vào thùng rác.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Refresh the folder list after successful deletion
                    await LoadFoldersAndFilesAsync();
                    
                    // Refresh TrashBin to show the deleted folder immediately
                    await RefreshTrashBinAsync();
                }
                else
                {
                    MessageBox.Show("Không thể xóa folder. Vui lòng thử lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa folder: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task NavigateBack()
        {
            if (navigationStack.Count > 0)
            {
                var previous = navigationStack.Pop();
                currentFolderId = previous.FolderId;
                await LoadFoldersAndFilesAsync();
            }
        }

        private string GetCurrentFolderName()
        {
            // This would need to query the database to get folder name by ID
            return "Current Folder";
        }

        private void UpdateBreadcrumb()
        {
            // Update the UI to show current path
            // You could add a Label at the top to show: Home > Folder1 > Subfolder2
        }

        private async Task OnFileDeleted(string fileName)
        {
            try
            {
                // Tìm fileId từ allFiles
                var file = allFiles.FirstOrDefault(f => f.FilePath == fileName || f.Name == fileName);
                if (file == null)
                {
                    MessageBox.Show("Không tìm thấy file để xóa.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                int fileId = file.Id;
                // Gọi API xóa file
                bool success = await Services.ApiService.DeleteFileAsync(fileId);
                if (success)
                {
                    MessageBox.Show("File đã được chuyển vào thùng rác.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadFoldersAndFilesAsync();
                    
                    // Refresh TrashBin to show the deleted file immediately
                    await RefreshTrashBinAsync();
                }
                else
                {
                    MessageBox.Show("Không thể xóa file. Vui lòng thử lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa file: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void OnFileRestoredFromTrash(string fileName)
        {
            try
            {
                Console.WriteLine($"[DEBUG] MyFileView - Received FileRestoredFromTrash event for: {fileName}");
                // Refresh the file list to show the restored file
                await LoadFoldersAndFilesAsync();
                Console.WriteLine($"[DEBUG] MyFileView - Refreshed file list after restore of: {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] MyFileView - Error in OnFileRestoredFromTrash: {ex.Message}");
            }
        }

        private async void OnFolderRestoredFromTrash(int folderId)
        {
            try
            {
                Console.WriteLine($"[DEBUG] MyFileView - Received FolderRestoredFromTrash event for: {folderId}");
                // Refresh the folder list to show the restored folder
                await LoadFoldersAndFilesAsync();
                Console.WriteLine($"[DEBUG] MyFileView - Refreshed folder list after restore of: {folderId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] MyFileView - Error in OnFolderRestoredFromTrash: {ex.Message}");
            }
        }

        private async Task OnFileShareRequested(int fileId, string fileName)
        {
            try
            {
                // Show permission selection dialog
                string permission = ShowPermissionDialog("file", fileName);
                if (string.IsNullOrEmpty(permission))
                {
                    return; // User cancelled
                }

                // Send share request to server with permission
                bool shareSuccess = await ShareFileAsync(fileId, fileName, permission);
                if (shareSuccess)
                {
                    // Query the generated share password
                    string sharePassword = await GetFileSharePasswordAsync(fileName);
                    if (!string.IsNullOrEmpty(sharePassword))
                    {
                        using (var form = new SharePasswordForm(fileName, permission, sharePassword, isFolder: false))
                        {
                            form.ShowDialog();
                        }
                    }
                    else
                    {
                        MessageBox.Show($"File '{fileName}' đã được chia sẻ thành công với quyền {permission}!", 
                            "Thành công chia sẻ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    
                    // Refresh the list to show updated share status
                    await LoadFoldersAndFilesAsync();
                }
                else
                {
                    MessageBox.Show($"Không thể chia sẻ file '{fileName}'.", 
                        "Thất bại chia sẻ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi chia sẻ file: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task OnFolderShareRequested(int folderId, string folderName)
        {
            try
            {
                // Show permission selection dialog
                string permission = ShowPermissionDialog("folder", folderName);
                if (string.IsNullOrEmpty(permission))
                {
                    return; // User cancelled
                }

                // Send share request to server with permission
                bool shareSuccess = await ShareFolderAsync(folderId, folderName, permission);
                if (shareSuccess)
                {
                    // Query the generated share password
                    string sharePassword = await GetFolderSharePasswordAsync(folderName);
                    if (!string.IsNullOrEmpty(sharePassword))
                    {
                        using (var form = new SharePasswordForm(folderName, permission, sharePassword, isFolder: true))
                        {
                            form.ShowDialog();
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Folder '{folderName}' đã được chia sẻ thành công với quyền {permission}!", 
                            "Thành công chia sẻ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    
                    // Refresh the list to show updated share status
                    await LoadFoldersAndFilesAsync();
                }
                else
                {
                    MessageBox.Show($"Không thể chia sẻ folder '{folderName}'.", 
                        "Thất bại chia sẻ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi chia sẻ folder: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task OnFolderDownloadRequested(int folderId)
        {
            try
            {
                // Find the folder in allFolders
                var folder = allFolders.FirstOrDefault(f => f.Id == folderId);
                if (folder == null)
                {
                    MessageBox.Show("Không tìm thấy folder để tải về.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                FolderBrowserDialog folderDialog = new FolderBrowserDialog();
                folderDialog.Description = "Chọn thư mục để lưu folder";
                folderDialog.ShowNewFolderButton = true;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    string targetPath = Path.Combine(folderDialog.SelectedPath, folder.Name);
                    
                    // Tạo thư mục đích
                    if (!Directory.Exists(targetPath))
                    {
                        Directory.CreateDirectory(targetPath);
                    }

                    // Download folder và tất cả files bên trong
                    await DownloadFolderContentsAsync(folderId, targetPath);
                    
                    MessageBox.Show($"Folder '{folder.Name}' đã được tải về thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải folder: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task DownloadFolderContentsAsync(int folderId, string targetPath)
        {
            try
            {
                // Kết nối đến server để lấy danh sách files trong folder
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync(serverIp, serverPort);
                using (sslStream)
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    // Lấy danh sách files trong folder
                    string message = $"GET_FOLDER_CONTENTS|{folderId}";
                    await writer.WriteLineAsync(message);

                    string response = await reader.ReadLineAsync();
                    response = response?.Trim();

                    Console.WriteLine($"[DEBUG] DownloadFolderContentsAsync response: '{response}'");

                    if (response != null && response.StartsWith("200|"))
                    {
                        string data = response.Substring(4);
                        if (data != "NO_FILES_IN_FOLDER")
                        {
                            // Sử dụng ParseFileItems method đã được cập nhật
                            var files = ParseFileItems(response);
                            
                            foreach (var file in files)
                            {
                                if (file == null) continue;

                                // Tạo thư mục con nếu cần (dựa trên filename structure)
                                string fileName = file.Name;
                                string fileDir = Path.GetDirectoryName(fileName);
                                if (!string.IsNullOrEmpty(fileDir))
                                {
                                    string fullDir = Path.Combine(targetPath, fileDir);
                                    if (!Directory.Exists(fullDir))
                                    {
                                        Directory.CreateDirectory(fullDir);
                                    }
                                }

                                // Download file
                                string filePath = Path.Combine(targetPath, fileName);
                                await DownloadFileAsync(file.Id, filePath);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tải folder: {ex.Message}");
            }
        }

        private async Task DownloadFileAsync(int fileId, string savePath)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync(serverIp, serverPort);
                using (sslStream)
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    // Send download request
                    string message = $"DOWNLOAD_FILE|{fileId}|{Session.LoggedInUserId}";
                    await writer.WriteLineAsync(message);

                    // Read response header
                    string response = await reader.ReadLineAsync();
                    response = response?.Trim();

                    if (response != null)
                    {
                        string[] parts = response.Split('|');
                        if (parts.Length >= 2 && parts[0] == "200")
                        {
                            // Parse base64 data
                            byte[] encryptedData = Convert.FromBase64String(parts[1]);
                            
                            // Decrypt and save file
                            CryptoHelper.DecryptFileToLocal(encryptedData, Session.UserPassword, savePath);
                        }
                        else
                        {
                            throw new Exception($"Server error: {response}");
                        }
                    }
                    else
                    {
                        throw new Exception("No response from server");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tải file {Path.GetFileName(savePath)}: {ex.Message}");
            }
        }

        private async Task<bool> ShareFileAsync(int fileId, string fileName, string permission = "read")
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync(serverIp, serverPort);
                using (sslStream)
                using (var reader = new StreamReader(sslStream, Encoding.UTF8))
                using (var writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    // Send SHARE_FILE command with file ID and permission
                    // This will:
                    // 1. Generate a share_pass for the file
                    // 2. Update files table: set share_pass and is_shared=1
                    // 3. Store the permission info for later use
                    string message = $"SHARE_FILE|{fileId}|{permission}";
                    Console.WriteLine($"[DEBUG] Sharing file with permission: {message.Trim()}");
                    await writer.WriteLineAsync(message);

                    string response = await reader.ReadLineAsync();
                    response = response?.Trim();
                    Console.WriteLine($"[DEBUG] Share file response: '{response}'");

                    if (response != null)
                    {
                        string[] parts = response.Split('|');
                        if (parts.Length >= 2 && parts[0] == "200")
                        {
                            // Success - file shared with permission
                            return true;
                        }
                        else
                        {
                            // Error
                            Console.WriteLine($"[ERROR] Share file failed: {response}");
                            return false;
                        }
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error sharing file: {ex.Message}");
                System.Windows.Forms.MessageBox.Show($"Lỗi chia sẻ file: {ex.Message}", "Lỗi", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
        }

        private async Task<bool> ShareFolderAsync(int folderId, string folderName, string permission = "read")
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync(serverIp, serverPort);
                using (sslStream)
                using (var reader = new StreamReader(sslStream, Encoding.UTF8))
                using (var writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    // Send SHARE_FOLDER command with folder ID and permission
                    // This will:
                    // 1. Generate a share_pass for the folder
                    // 2. Update folders table: set share_pass and is_shared=1
                    // 3. Store the permission info for later use
                    string message = $"SHARE_FOLDER|{folderId}|{permission}";
                    Console.WriteLine($"[DEBUG] Sharing folder with permission: {message}");
                    await writer.WriteLineAsync(message);

                    string response = await reader.ReadLineAsync();
                    response = response?.Trim();
                    Console.WriteLine($"[DEBUG] Share folder response: '{response}'");

                    if (response != null)
                    {
                        string[] parts = response.Split('|');
                        if (parts.Length >= 2 && parts[0] == "200")
                        {
                            // Success - folder shared with permission
                            return true;
                        }
                        else
                        {
                            // Error
                            Console.WriteLine($"[ERROR] Share folder failed: {response}");
                            return false;
                        }
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error sharing folder: {ex.Message}");
                MessageBox.Show($"Lỗi chia sẻ folder: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private List<Services.FileItem> ParseFileItems(string data)
        {
            var files = new List<Services.FileItem>();
            if (string.IsNullOrWhiteSpace(data) || data == "NO_FILES" || data == "NO_SHARED_FILES" || data == "NO_FILES_IN_FOLDER" || data == "NO_CONTENTS") return files;

            Console.WriteLine($"[DEBUG][ParseFileItems] Raw data: {data}");

            // Nếu có prefix 200| thì bỏ đi
            if (data.StartsWith("200|"))
                data = data.Substring(4);

            string[] items = data.Split('|');
            foreach (string item in items)
            {
                if (string.IsNullOrWhiteSpace(item)) continue;
                if (item.StartsWith("file:"))
                {
                    // New format: file:<file_id>:<file_name>:<file_path>:<relative_path>
                    var parts = item.Split(':');
                    if (parts.Length >= 5)
                    {
                        if (int.TryParse(parts[1], out int fileId))
                        {
                            string name = parts[2];
                            string filePath = parts[3];
                            string relativePath = parts[4];
                            files.Add(new Services.FileItem
                            {
                                Id = fileId,
                                Name = name,
                                FilePath = filePath,
                                // Có thể bổ sung các trường khác nếu cần
                            });
                        }
                        else
                        {
                            Console.WriteLine($"[ERROR] Failed to parse file_id from: {item}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[ERROR] Invalid file format: {item}");
                    }
                }
                // Nếu cần parse folder thì thêm else if (item.StartsWith("folder:"))
            }
            return files;
        }

        private string FormatFileSize(long bytes)
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

        private async Task<int> GetUserIdFromSessionAsync()
        {
            // Use the user ID that was already retrieved during login
            if (Session.LoggedInUserId != -1)
            {
                return Session.LoggedInUserId;
            }
            // Fallback: try to get from server if not available
            try
            {
                int userId = await ApiService.GetUserIdAsync(Session.LoggedInUser);
                return userId;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lấy user_id: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }

        private async Task<bool> DeleteFileOnServer(string fileName, int userId)
        {
            try
            {
                Console.WriteLine($"[DEBUG] Sending DELETE_FILE request: fileName={fileName}, userId={userId}");
                
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync(serverIp, serverPort);
                using (sslStream)
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    string message = $"DELETE_FILE|{fileName}|{userId}";
                    Console.WriteLine($"[DEBUG] Sending message: {message}");
                    await writer.WriteLineAsync(message);

                    string response = await reader.ReadLineAsync();
                    response = response?.Trim();
                    Console.WriteLine($"[DEBUG] Server response: {response}");

                    if (response != null)
                    {
                        string[] parts = response.Split('|');
                        bool success = parts.Length >= 2 && parts[0] == "200" && parts[1] == "FILE_DELETED";
                        Console.WriteLine($"[DEBUG] Delete success: {success}");
                        
                        if (!success)
                        {
                            MessageBox.Show($"Server response: {response}", "Thông tin debug", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        
                        return success;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error deleting file on server: {ex.Message}");
                MessageBox.Show($"Lỗi kết nối: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private async Task<bool> DeleteFolderOnServer(string folderId, int userId)
        {
            try
            {
                Console.WriteLine($"[DEBUG] Sending DELETE_FOLDER request: folderId={folderId}, userId={userId}");
                
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync(serverIp, serverPort);
                using (sslStream)
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    string message = $"DELETE_FOLDER|{folderId}|{userId}";
                    Console.WriteLine($"[DEBUG] Sending message: {message}");
                    await writer.WriteLineAsync(message);

                    string response = await reader.ReadLineAsync();
                    response = response?.Trim();
                    Console.WriteLine($"[DEBUG] Server response: {response}");

                    if (response != null)
                    {
                        string[] parts = response.Split('|');
                        bool success = parts.Length >= 2 && parts[0] == "200" && parts[1] == "FOLDER_DELETED";
                        Console.WriteLine($"[DEBUG] Folder delete success: {success}");
                        
                        if (!success)
                        {
                            MessageBox.Show($"Server response: {response}", "Thông tin debug", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        
                        return success;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error deleting folder on server: {ex.Message}");
                MessageBox.Show($"Lỗi kết nối: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // Toolbar event handlers
        private async void btnNewFolder_Click(object sender, EventArgs e)
        {
            // Simple input dialog using InputDialog
            string folderName = ShowInputDialog("Nhập tên thư mục:", "Thư mục mới", "Thư mục mới");

            if (!string.IsNullOrWhiteSpace(folderName))
            {
                try
                {
                    bool success = await ApiService.CreateFolderAsync(Session.LoggedInUserId, folderName, currentFolderId);
                    
                    if (success)
                    {
                        MessageBox.Show("Thư mục đã được tạo thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadFoldersAndFilesAsync();
                    }
                    else
                    {
                        MessageBox.Show("Không thể tạo thư mục. Vui lòng thử lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi tạo thư mục: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            await PerformSearch();
            }

        private async void btnGetFile_Click(object sender, EventArgs e)
        {
            Console.WriteLine($"[DEBUG] btnGetFile_Click called");
            
            // Show dialog to enter password for getting files
            string password = ShowPasswordDialog();
            Console.WriteLine($"[DEBUG] Password entered: '{password}'");
            
            if (!string.IsNullOrEmpty(password))
            {
                try
                {
                    Console.WriteLine($"[DEBUG] Calling GetItemsByPasswordAsync with password: '{password}'");
                    
                    // Get files and folders using password
                    var (files, folders, success) = await GetItemsByPasswordAsync(password);
                    Console.WriteLine($"[DEBUG] GetItemsByPasswordAsync returned: success={success}");
                    
                    if (success)
                    {
                        Console.WriteLine($"[DEBUG] Success! Note: Files/folders will not be shown here as this tab only shows your own files.");
                        
                        // Don't refresh the list since MyFileView only shows user's own files
                        // The shared files will be accessible through "Shared With Me" tab
                        
                        Console.WriteLine($"[DEBUG] Get file operation completed");
                    }
                    else
                    {
                        Console.WriteLine($"[DEBUG] No items found or invalid password");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Exception in btnGetFile_Click: {ex.Message}");
                    Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                    MessageBox.Show($"Lỗi khi lấy file: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                Console.WriteLine($"[DEBUG] No password entered");
            }
        }

        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            await LoadFoldersAndFilesAsync();
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

        private void txtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            const int MAX_SEARCH_LENGTH = 50; // Giới hạn tìm kiếm tối đa 50 ký tự
            
            // Cho phép backspace, delete và control keys
            if (char.IsControl(e.KeyChar))
                return;
                
            // Kiểm tra độ dài
            if (txtSearch.Text.Length >= MAX_SEARCH_LENGTH && txtSearch.Text != "Tìm kiếm file...")
            {
                e.Handled = true; // Ngăn không cho nhập thêm ký tự
                MessageBox.Show($"Từ khóa tìm kiếm không được vượt quá {MAX_SEARCH_LENGTH} ký tự.", 
                    "Giới hạn tìm kiếm", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public void AddFileToView(string fileName, string createAt, string owner, string filesize, string filePath)
        {
            var fileItem = new FileItemControl(fileName, createAt, owner, filesize, filePath);
            MyFileLayoutPanel.Controls.Add(fileItem);
        }

        private void MyFileLayoutPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void CleanupEvents()
        {
            try
            {
                // Unsubscribe from events to prevent memory leaks
                TrashBinView.FileRestoredFromTrash -= OnFileRestoredFromTrash;
                TrashBinView.FolderRestoredFromTrash -= OnFolderRestoredFromTrash;
                Console.WriteLine("[DEBUG] MyFileView - Unsubscribed from FileRestoredFromTrash and FolderRestoredFromTrash events");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] MyFileView - Error unsubscribing from events: {ex.Message}");
            }
        }

        // Simple input dialog method
        private string ShowInputDialog(string text, string caption, string defaultValue = "")
        {
            Form prompt = new Form()
            {
                Width = 400,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen,
                MaximizeBox = false,
                MinimizeBox = false
            };
            
            Label textLabel = new Label() { Left = 20, Top = 20, Width = 350, Text = text };
            TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 350, Text = defaultValue };
            Button confirmation = new Button() { Text = "OK", Left = 200, Width = 80, Top = 80, DialogResult = DialogResult.OK };
            Button cancel = new Button() { Text = "Cancel", Left = 290, Width = 80, Top = 80, DialogResult = DialogResult.Cancel };
            
            confirmation.Click += (sender, e) => { prompt.Close(); };
            cancel.Click += (sender, e) => { prompt.Close(); };
            
            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(cancel);
            prompt.AcceptButton = confirmation;
            prompt.CancelButton = cancel;
            
            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }

        private string ShowPasswordDialog()
        {
            Form prompt = new Form()
            {
                Width = 400,
                Height = 200,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Lấy file từ người dùng khác",
                StartPosition = FormStartPosition.CenterScreen,
                MaximizeBox = false,
                MinimizeBox = false
            };
            
            Label textLabel = new Label() { Left = 20, Top = 20, Width = 350, Text = "Nhập mật khẩu để lấy file từ người dùng khác:" };
            TextBox passwordBox = new TextBox() { Left = 20, Top = 50, Width = 350, UseSystemPasswordChar = true };
            Button confirmation = new Button() { Text = "Lấy file", Left = 200, Width = 80, Top = 100, DialogResult = DialogResult.OK };
            Button cancel = new Button() { Text = "Hủy", Left = 290, Width = 80, Top = 100, DialogResult = DialogResult.Cancel };
            
            confirmation.Click += (sender, e) => { prompt.Close(); };
            cancel.Click += (sender, e) => { prompt.Close(); };
            
            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(passwordBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(cancel);
            prompt.AcceptButton = confirmation;
            prompt.CancelButton = cancel;
            
            return prompt.ShowDialog() == DialogResult.OK ? passwordBox.Text : "";
        }

        private string ShowPermissionDialog(string itemType, string itemName)
        {
            Form prompt = new Form()
            {
                Width = 450,
                Height = 220,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = $"Chia sẻ {itemType}",
                StartPosition = FormStartPosition.CenterScreen,
                MaximizeBox = false,
                MinimizeBox = false
            };
            
            Label textLabel = new Label() 
            { 
                Left = 20, 
                Top = 20, 
                Width = 400, 
                Text = $"Chọn quyền chia sẻ '{itemName}':" 
            };
            
            ComboBox permissionCombo = new ComboBox() 
            { 
                Left = 20, 
                Top = 50, 
                Width = 350,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            
                         if (itemType == "file")
             {
                 permissionCombo.Items.AddRange(new string[] { "read", "write" });
             }
             else // folder
             {
                 permissionCombo.Items.AddRange(new string[] { "read", "write", "admin" });
             }
            permissionCombo.SelectedIndex = 0; // Default to "read"
            
                         Label descLabel = new Label() 
             { 
                 Left = 20, 
                 Top = 80, 
                 Width = 400, 
                 Height = 40,
                 Text = "• read: View only\n• write: View and edit\n• admin: Full control (folders only)"
             };
            
            Button confirmation = new Button() 
            { 
                Text = "Chia sẻ", 
                Left = 200, 
                Width = 80, 
                Top = 130, 
                DialogResult = DialogResult.OK 
            };
            
            Button cancel = new Button() 
            { 
                Text = "Hủy", 
                Left = 290, 
                Width = 80, 
                Top = 130, 
                DialogResult = DialogResult.Cancel 
            };
            
            confirmation.Click += (sender, e) => { prompt.Close(); };
            cancel.Click += (sender, e) => { prompt.Close(); };
            
            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(permissionCombo);
            prompt.Controls.Add(descLabel);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(cancel);
            prompt.AcceptButton = confirmation;
            prompt.CancelButton = cancel;
            
            return prompt.ShowDialog() == DialogResult.OK ? permissionCombo.SelectedItem.ToString() : "";
        }

        // Helper classes


        private class FolderNavigationItem
        {
            public int? FolderId { get; set; }
            public string FolderName { get; set; }
        }

        private async Task<string> GetFileSharePasswordAsync(string fileName)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync(serverIp, serverPort);
                using (sslStream)
                using (var reader = new StreamReader(sslStream, Encoding.UTF8))
                using (var writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    // Send GET_FILE_SHARE_PASSWORD command
                    string message = $"GET_FILE_SHARE_PASSWORD|{fileName}";
                    await writer.WriteLineAsync(message);

                    string response = await reader.ReadLineAsync();
                    response = response?.Trim();

                    if (response != null)
                    {
                        string[] parts = response.Split('|');
                        if (parts.Length >= 2 && parts[0] == "200")
                        {
                            return parts[1]; // Return the share password
                        }
                    }
                    return null; // No share password found
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lấy mật khẩu chia sẻ file: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private async Task<string> GetFolderSharePasswordAsync(string folderName)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync(serverIp, serverPort);
                using (sslStream)
                using (var reader = new StreamReader(sslStream, Encoding.UTF8))
                using (var writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    // Send GET_FOLDER_SHARE_PASSWORD command
                    string message = $"GET_FOLDER_SHARE_PASSWORD|{folderName}";
                    await writer.WriteLineAsync(message);

                    string response = await reader.ReadLineAsync();
                    response = response?.Trim();

                    if (response != null)
                    {
                        string[] parts = response.Split('|');
                        if (parts.Length >= 2 && parts[0] == "200")
                        {
                            return parts[1]; // Return the share password
                        }
                    }
                    return null; // No share password found
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lấy mật khẩu chia sẻ thư mục: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private async Task<(List<Services.FileItem> files, List<Services.FolderItem> folders, bool success)> GetItemsByPasswordAsync(string password)
        {
            try
            {
                Console.WriteLine($"[DEBUG] GetItemsByPasswordAsync called with password: '{password}'");
                
                // First try to get file info from share password
                (int fileId, int ownerId) = await GetFileInfoFromSharePassAsync(password);
                Console.WriteLine($"[DEBUG] GetFileInfoFromSharePassAsync returned: fileId={fileId}, ownerId={ownerId}");
                
                if (fileId != -1)
                {
                    // Check if current user is the owner
                    int currentUserId = Session.LoggedInUserId;
                    if (currentUserId == ownerId)
                    {
                        Console.WriteLine($"[DEBUG] Current user is the owner, no need to share");
                        MessageBox.Show("Bạn là chủ sở hữu của file này. Không cần chia sẻ lại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return (null, null, false);
                    }
                    
                    // Add reference to files_share table
                    bool shareResult = await AddFileReferenceAsync(fileId.ToString(), currentUserId.ToString(), password);
                    Console.WriteLine($"[DEBUG] AddFileReferenceAsync result: {shareResult}");
                    
                    if (shareResult)
                    {
                        Console.WriteLine($"[DEBUG] File reference added successfully");
                        MessageBox.Show("Bạn đã có quyền truy cập vào file này!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return (new List<Services.FileItem>(), new List<Services.FolderItem>(), true);
                    }
                    else
                    {
                        Console.WriteLine($"[DEBUG] Failed to add file reference");
                        MessageBox.Show("Lỗi khi thêm quyền truy cập file!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return (null, null, false);
                    }
                }
                
                // If no file found, try to get folder info from share password
                Console.WriteLine($"[DEBUG] No file found, trying folder...");
                (int folderId, int folderOwnerId) = await GetFolderInfoFromSharePassAsync(password);
                Console.WriteLine($"[DEBUG] GetFolderInfoFromSharePassAsync returned: folderId={folderId}, ownerId={folderOwnerId}");
                
                if (folderId != -1)
                {
                    // Check if current user is the owner
                    int currentUserId = Session.LoggedInUserId;
                    Console.WriteLine($"[DEBUG] Current user ID: {currentUserId}, Folder owner ID: {folderOwnerId}");
                    
                    if (currentUserId == folderOwnerId)
                    {
                        Console.WriteLine($"[DEBUG] Current user is the folder owner, no need to share");
                        MessageBox.Show("Bạn là chủ sở hữu của thư mục này. Không cần chia sẻ lại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return (null, null, false);
                    }
                    
                    // Add reference to folder_shares table
                    bool shareResult = await AddFolderReferenceAsync(folderId.ToString(), currentUserId.ToString(), password);
                    Console.WriteLine($"[DEBUG] AddFolderReferenceAsync result: {shareResult}");
                    
                    if (shareResult)
                    {
                        Console.WriteLine($"[DEBUG] Folder reference added successfully");
                        MessageBox.Show("Bạn đã có quyền truy cập vào thư mục này!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return (new List<Services.FileItem>(), new List<Services.FolderItem>(), true);
                    }
                    else
                    {
                        Console.WriteLine($"[DEBUG] Failed to add folder reference");
                        MessageBox.Show("Lỗi khi thêm quyền truy cập thư mục!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return (null, null, false);
                    }
                }
                
                Console.WriteLine($"[DEBUG] Invalid password, no file or folder found");
                MessageBox.Show("Mật khẩu không hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return (null, null, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Exception in GetItemsByPasswordAsync: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Lỗi khi lấy mục theo mật khẩu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return (null, null, false);
            }
        }

        // COMMENTED OUT: Moved to "Shared With Me" tab  
        private async Task<bool> AddFileReferenceAsync(string fileId, string userId, string sharePass)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync(serverIp, serverPort);
                using (sslStream)
                using (var reader = new StreamReader(sslStream, Encoding.UTF8))
                using (var writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    // First get the permission from the share pass
                    string permission = await GetPermissionFromSharePassAsync(sharePass, "file");
                    
                    // Add entry to files_share table with permission
                    string message = $"ADD_FILE_SHARE_ENTRY_WITH_PERMISSION|{fileId}|{userId}|{sharePass}|{permission}";
                    Console.WriteLine($"[DEBUG] Adding file share entry: {message}");
                    await writer.WriteLineAsync(message);

                    string response = await reader.ReadLineAsync();
                    Console.WriteLine($"[DEBUG] Add file reference response: '{response}'");
                    
                    return response != null && response.StartsWith("200|");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi thêm quyền truy cập file: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> AddFolderReferenceAsync(string folderId, string userId, string sharePass)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync(serverIp, serverPort);
                using (sslStream)
                using (var reader = new StreamReader(sslStream, Encoding.UTF8))
                using (var writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    // First get the permission from the share pass
                    string permission = await GetPermissionFromSharePassAsync(sharePass, "folder");
                    
                    // Add entry to folder_shares table with permission
                    string message = $"ADD_FOLDER_SHARE_ENTRY_WITH_PERMISSION|{folderId}|{userId}|{sharePass}|{permission}";
                    Console.WriteLine($"[DEBUG] Adding folder share entry: {message.Trim()}");
                    await writer.WriteLineAsync(message);

                    string response = await reader.ReadLineAsync();
                    Console.WriteLine($"[DEBUG] Add folder reference response: '{response}'");
                    
                    if (response != null && response.StartsWith("200|"))
                    {
                        // Also add all files in this folder to files_share table
                        bool filesAdded = await AddFilesInFolderToShareAsync(folderId, userId, sharePass, permission);
                        Console.WriteLine($"[DEBUG] Files in folder added to share: {filesAdded}");
                        
                        return true;
                    }
                    
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi thêm quyền truy cập thư mục: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> AddFilesInFolderToShareAsync(string folderId, string userId, string sharePass, string permission)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync(serverIp, serverPort);
                using (sslStream)
                using (var reader = new StreamReader(sslStream, Encoding.UTF8))
                using (var writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    // Send command to add all files in folder to files_share table
                    string message = $"ADD_FILES_IN_FOLDER_TO_SHARE|{folderId}|{userId}|{sharePass}|{permission}";
                    Console.WriteLine($"[DEBUG] Adding files in folder to share: {message}");
                    await writer.WriteLineAsync(message);

                    string response = await reader.ReadLineAsync();
                    Console.WriteLine($"[DEBUG] Add files in folder response: '{response}'");
                    
                    return response != null && response.StartsWith("200|");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi thêm file trong thư mục vào chia sẻ: {ex.Message}");
                return false;
            }
        }

        private async Task<string> GetPermissionFromSharePassAsync(string sharePass, string itemType)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync(serverIp, serverPort);
                using (sslStream)
                using (var reader = new StreamReader(sslStream, Encoding.UTF8))
                using (var writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    string command = itemType == "file" ? "GET_FILE_PERMISSION_BY_SHARE_PASS" : "GET_FOLDER_PERMISSION_BY_SHARE_PASS";
                    string message = $"{command}|{sharePass}";
                    await writer.WriteLineAsync(message);

                    string response = await reader.ReadLineAsync();
                    response = response?.Trim();

                    if (response != null && response.StartsWith("200|"))
                    {
                        string[] parts = response.Split('|');
                        if (parts.Length >= 2)
                        {
                            return parts[1]; // Return the permission
                        }
                    }
                                         return "read"; // Default to read permission
                 }
             }
             catch (Exception ex)
             {
                 Console.WriteLine($"Lỗi khi lấy quyền từ mật khẩu chia sẻ: {ex.Message}");
                 return "read"; // Default to read permission
             }
        }

        private async Task<(int, int)> GetFolderInfoFromSharePassAsync(string sharePass)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync(serverIp, serverPort);
                using (sslStream)
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    string message = $"GET_FOLDER_INFO_BY_SHARE_PASS|{sharePass}";
                    Console.WriteLine($"[DEBUG] GetFolderInfoFromSharePassAsync: Sending message: {message}");
                    await writer.WriteLineAsync(message);

                    string response = await reader.ReadLineAsync();
                    response = response?.Trim();
                    Console.WriteLine($"[DEBUG] GetFolderInfoFromSharePassAsync response: '{response}'");

                    if (response != null)
                    {
                        string[] parts = response.Split('|');
                        Console.WriteLine($"[DEBUG] GetFolderInfoFromSharePassAsync: Response parts count: {parts.Length}");
                        for (int i = 0; i < parts.Length; i++)
                        {
                            Console.WriteLine($"[DEBUG] GetFolderInfoFromSharePassAsync: parts[{i}] = '{parts[i]}'");
                        }
                        
                        if (parts.Length >= 2 && parts[0] == "200")
                        {
                            // Parse folder info: folder_id:folder_name:created_at:owner_name
                            string[] folderInfo = parts[1].Split(':');
                            Console.WriteLine($"[DEBUG] GetFolderInfoFromSharePassAsync: Folder info parts count: {folderInfo.Length}");
                            for (int i = 0; i < folderInfo.Length; i++)
                            {
                                Console.WriteLine($"[DEBUG] GetFolderInfoFromSharePassAsync: folderInfo[{i}] = '{folderInfo[i]}'");
                            }
                            
                            if (folderInfo.Length >= 4)
                            {
                                if (int.TryParse(folderInfo[0], out int folderId))
                                {
                                    // Get owner_id from owner_name (we need to query this)
                                    int ownerId = await GetOwnerIdFromUsernameAsync(folderInfo[3]);
                                    Console.WriteLine($"[DEBUG] Parsed folder: id={folderId}, owner={folderInfo[3]}, ownerId={ownerId}");
                                    return (folderId, ownerId);
                                }
                                else
                                {
                                    Console.WriteLine($"[DEBUG] Failed to parse folder ID: '{folderInfo[0]}'");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"[DEBUG] Invalid folder info format: expected >=4 parts, got {folderInfo.Length}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[DEBUG] Invalid response format: expected '200|...', got '{response}'");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[DEBUG] No response received");
                    }
                    return (-1, -1);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy thông tin thư mục từ mật khẩu chia sẻ: {ex.Message}");
                return (-1, -1);
            }
        }

        private async Task<int> GetOwnerIdFromUsernameAsync(string username)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync(serverIp, serverPort);
                using (sslStream)
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    string message = $"GET_USER_ID|{username}";
                    await writer.WriteLineAsync(message);

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
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy ID người dùng từ tên người dùng: {ex.Message}");
                return -1;
            }
        }

        // Get file info from share password (copied from ShareView)
        private async Task<(int, int)> GetFileInfoFromSharePassAsync(string sharePass)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync(serverIp, serverPort);
                using (sslStream)
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    string message = $"GET_FILE_INFO_BY_SHARE_PASS|{sharePass}";
                    await writer.WriteLineAsync(message);

                    string response = await reader.ReadLineAsync();
                    response = response?.Trim();

                    if (response != null)
                    {
                        string[] parts = response.Split('|');
                        if (parts.Length >= 3 && parts[0] == "200")
                        {
                            if (int.TryParse(parts[1], out int fileId) && int.TryParse(parts[2], out int ownerId))
                            {
                                return (fileId, ownerId);
                            }
                        }
                    }
                    return (-1, -1);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy thông tin file từ mật khẩu chia sẻ: {ex.Message}");
                return (-1, -1);
            }
        }

        // COMMENTED OUT: Moved to "Shared With Me" tab
        
        private List<Services.FolderItem> ParseFolderListFromResponse(string folderListData)
        {
            var folders = new List<Services.FolderItem>();
            
            try
            {
                Console.WriteLine($"[DEBUG][ParseFolderListFromResponse] Raw data: '{folderListData}'");
                
                // Check if the data contains semicolons (multiple folders) or is a single folder entry
                if (folderListData.Contains(';'))
                {
                    // Multiple folders separated by semicolons
                    string[] folderEntries = folderListData.Split(';');
                    Console.WriteLine($"[DEBUG][ParseFolderListFromResponse] Found {folderEntries.Length} folder entries");
                    
                    foreach (string folderEntry in folderEntries)
                    {
                        Console.WriteLine($"[DEBUG][ParseFolderListFromResponse] Processing entry: '{folderEntry}'");
                        if (!string.IsNullOrEmpty(folderEntry))
                        {
                            // Parse folder entry: folder_id:folder_name:created_at:owner_name
                            string[] parts = folderEntry.Split(':');
                            if (parts.Length >= 4)
                            {
                                var folderItem = new Services.FolderItem
                                {
                                    Id = int.Parse(parts[0]),
                                    Name = parts[1],
                                    CreatedAt = parts[2],
                                    Owner = parts[3],
                                    IsShared = true
                                };
                                folders.Add(folderItem);
                                Console.WriteLine($"[DEBUG][ParseFolderListFromResponse] Successfully parsed folder: {parts[1]}");
                            }
                            else
                            {
                                Console.WriteLine($"[DEBUG][ParseFolderListFromResponse] Failed to parse folder entry: '{folderEntry}'");
                            }
                        }
                    }
                }
                else
                {
                    // Single folder entry - treat the entire data as one folder
                    Console.WriteLine($"[DEBUG][ParseFolderListFromResponse] Single folder entry detected");
                    string[] parts = folderListData.Split(':');
                    if (parts.Length >= 4)
                    {
                        var folderItem = new Services.FolderItem
                        {
                            Id = int.Parse(parts[0]),
                            Name = parts[1],
                            CreatedAt = parts[2],
                            Owner = parts[3],
                            IsShared = true
                        };
                        folders.Add(folderItem);
                        Console.WriteLine($"[DEBUG][ParseFolderListFromResponse] Successfully parsed single folder: {parts[1]}");
                    }
                    else
                    {
                        Console.WriteLine($"[DEBUG][ParseFolderListFromResponse] Failed to parse single folder entry: '{folderListData}'");
                    }
                }
                
                Console.WriteLine($"[DEBUG][ParseFolderListFromResponse] Total folders parsed: {folders.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi phân tích danh sách thư mục: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            
            return folders;
        }
        
        private async Task PerformSearch()
        {
            string searchTerm = txtSearch.Text;
            if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm == "Tìm kiếm file...")
            {
                DisplayFoldersAndFiles();
                return;
            }

            var filteredFiles = allFiles.Where(f =>
                (f.Name != null && f.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (f.Type != null && f.Type.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (f.Owner != null && f.Owner.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
            ).ToList();

            var filteredFolders = allFolders.Where(f =>
                (f.Name != null && f.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (f.Owner != null && f.Owner.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
            ).ToList();

            MyFileLayoutPanel.Controls.Clear();
            foreach (var folder in filteredFolders)
            {
                var folderControl = new FolderItemControl(folder.Name, folder.CreatedAt, folder.Owner, folder.IsShared, folder.Id);
                folderControl.FolderClicked += async (folderId) => await NavigateToFolderById(folderId);
                folderControl.FolderDeleted += async (folderId) => await OnFolderDeleted(folderId);
                folderControl.FolderShareRequested += async (folderIdStr) => await OnFolderShareRequested(int.Parse(folderIdStr), folder.Name);
                MyFileLayoutPanel.Controls.Add(folderControl);
            }
            foreach (var file in filteredFiles)
            {
                var fileItemControl = new FileItemControl(file.Name, file.CreatedAt, file.Owner, file.Size, file.FilePath, file.Id);
                fileItemControl.FileDeleted += async (filePath) => await OnFileDeleted(filePath);
                fileItemControl.FileShareRequested += async (fileIdStr) => await OnFileShareRequested(int.Parse(fileIdStr), file.Name);
                MyFileLayoutPanel.Controls.Add(fileItemControl);
            }
        }

        private async Task RefreshTrashBinAsync()
        {
            try
            {
                // Find TrashBinView in the parent form and refresh it
                var parentForm = this.FindForm();
                if (parentForm != null)
                {
                    var trashBinView = parentForm.Controls.OfType<TrashBinView>().FirstOrDefault();
                    if (trashBinView != null)
                    {
                        await trashBinView.RefreshTrashFiles();
                        Console.WriteLine("[DEBUG] TrashBin refreshed after delete operation");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to refresh TrashBin: {ex.Message}");
            }
        }
    }
}


