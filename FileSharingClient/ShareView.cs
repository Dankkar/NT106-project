using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using FileSharingClient.Services;

namespace FileSharingClient
{
    public partial class ShareView: UserControl
    {
        private List<Services.FileItem> allSharedFiles = new List<Services.FileItem>();
        private List<Services.FolderItem> allSharedFolders = new List<Services.FolderItem>();
        private int? currentSharedFolderId = null; // null = root level of shared items
        private Stack<FolderNavigationItem> navigationStack = new Stack<FolderNavigationItem>();

        public ShareView()
        {
            InitializeComponent();
            SharedFileLayoutPanel.FlowDirection = FlowDirection.TopDown;
            SharedFileLayoutPanel.AutoScroll = true;
            SharedFileLayoutPanel.WrapContents = false;
            
            // Set placeholder text style
            txtSearch.ForeColor = Color.Gray;
            
            // Add KeyPress event handler for search length validation
            txtSearch.KeyPress += txtSearch_KeyPress;
            
            _ = InitAsync();
        }

        private async Task InitAsync()
        {
            await LoadSharedFoldersAndFilesAsync();
        }

        public async Task Reload()
        {
            await LoadSharedFoldersAndFilesAsync();
        }

        private async Task LoadSharedFoldersAndFilesAsync()
        {
            try
            {
                Console.WriteLine($"[DEBUG] LoadSharedFoldersAndFilesAsync called for user {Session.LoggedInUserId}");
                
                allSharedFiles.Clear();
                allSharedFolders.Clear();
                SharedFileLayoutPanel.Controls.Clear();

                // Debug: Check currentUserId
                if (Session.LoggedInUserId == -1)
                {
                    MessageBox.Show("currentUserId is -1, cannot load shared files", "Debug", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Load shared folders (from folder_shares table where shared_with_user_id = current user)
                var sharedFolders = await ApiService.GetSharedFoldersAsync(Session.LoggedInUserId);
                Console.WriteLine($"[DEBUG] Loaded {sharedFolders.Count} shared folders");
                allSharedFolders.AddRange(sharedFolders);

                // Load shared files (from files_share table where user_id = current user)
                var sharedFiles = await ApiService.GetSharedFilesAsync(Session.LoggedInUserId);
                Console.WriteLine($"[DEBUG] Loaded {sharedFiles.Count} shared files");
                allSharedFiles.AddRange(sharedFiles);

                // Debug: Show counts
                Console.WriteLine($"[DEBUG] Total: {allSharedFolders.Count} shared folders and {allSharedFiles.Count} shared files for user {Session.LoggedInUserId}");

                // Display shared folders and files
                DisplaySharedFoldersAndFiles();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] LoadSharedFoldersAndFilesAsync error: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Error loading shared folders and files: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplaySharedFoldersAndFiles()
        {
            SharedFileLayoutPanel.Controls.Clear();
            
            // Add back button if not at root (for future folder navigation)
            if (currentSharedFolderId != null)
            {
                var backControl = CreateBackButton();
                SharedFileLayoutPanel.Controls.Add(backControl);
            }
            
            // Add shared folders first
            foreach (var folder in allSharedFolders)
            {
                Console.WriteLine($"[DEBUG][ShareView] Creating shared folder control: {folder.Name}, Owner: '{folder.Owner}'");
                
                var folderControl = new FolderItemControl(folder.Name, folder.CreatedAt, folder.Owner, folder.IsShared, folder.Id);
                folderControl.FolderClicked += async (folderId) => await NavigateToSharedFolderById(folderId);
                // Remove default context menu and create custom one for shared folders
                CreateCustomFolderContextMenu(folderControl, folder.Id, folder.Name);
                SharedFileLayoutPanel.Controls.Add(folderControl);
            }
            
            // Add shared files
            foreach (var file in allSharedFiles)
            {
                Console.WriteLine($"[DEBUG][ShareView] Creating shared file control: {file.Name}, Owner: '{file.Owner}'");
                
                var fileItemControl = new FileItemControl(file.Name, file.CreatedAt, file.Owner, file.Size, file.FilePath, file.Id);
                // Remove default context menu and create custom one for shared files
                CreateCustomFileContextMenu(fileItemControl, file.Id, file.Name);
                SharedFileLayoutPanel.Controls.Add(fileItemControl);
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

        private async Task NavigateToSharedFolderById(int folderId)
        {
            // Find the folder in allSharedFolders
            var folder = allSharedFolders.FirstOrDefault(f => f.Id == folderId);
            if (folder != null)
            {
                await NavigateToSharedFolder(folder);
            }
        }

        private async Task NavigateToSharedFolder(Services.FolderItem folder)
        {
            // Save current state to navigation stack
            navigationStack.Push(new FolderNavigationItem 
            { 
                FolderId = currentSharedFolderId, 
                FolderName = currentSharedFolderId == null ? "Shared Root" : GetCurrentFolderName() 
            });
            
            currentSharedFolderId = folder.Id;
            
            // Load shared folder contents
            await LoadSharedFolderContentsAsync(folder.Id);
        }

        private async Task LoadSharedFolderContentsAsync(int folderId)
        {
            try
            {
                Console.WriteLine($"[DEBUG] LoadSharedFolderContentsAsync called for folder {folderId}");
                
                allSharedFiles.Clear();
                allSharedFolders.Clear();
                SharedFileLayoutPanel.Controls.Clear();

                // Add back button
                var backControl = CreateBackButton();
                SharedFileLayoutPanel.Controls.Add(backControl);

                // Get shared folder contents from server
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync("127.0.0.1", 5000);
                using (sslStream)
                using (var reader = new StreamReader(sslStream, Encoding.UTF8))
                using (var writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    string message = $"GET_SHARED_FOLDER_CONTENTS|{folderId}|{Session.LoggedInUserId}";
                    Console.WriteLine($"[DEBUG] Getting shared folder contents: {message}");
                    await writer.WriteLineAsync(message);

                    string response = await reader.ReadLineAsync();
                    response = response?.Trim();
                    Console.WriteLine($"[DEBUG] Shared folder contents response: '{response}'");

                    if (response != null && response.StartsWith("200|"))
                    {
                        string data = response.Substring(4);
                        if (data != "NO_FILES_IN_FOLDER")
                        {
                            var files = ParseSharedFolderFiles(data);
                            allSharedFiles.AddRange(files);
                        }
                    }
                }

                // Display files
                DisplaySharedFoldersAndFiles();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] LoadSharedFolderContentsAsync error: {ex.Message}");
                MessageBox.Show($"Error loading shared folder contents: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task NavigateBack()
        {
            if (navigationStack.Count > 0)
            {
                var previous = navigationStack.Pop();
                currentSharedFolderId = previous.FolderId;
                
                if (currentSharedFolderId == null)
                {
                    // Back to root - load all shared items
                    await LoadSharedFoldersAndFilesAsync();
                }
                else
                {
                    // Navigate to parent folder
                    await LoadSharedFolderContentsAsync(currentSharedFolderId.Value);
                }
            }
        }

        private string GetCurrentFolderName()
        {
            // This would need to query the database to get folder name by ID
            return "Current Shared Folder";
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

        private List<Services.FileItem> ParseSharedFolderFiles(string data)
        {
            var files = new List<Services.FileItem>();
            if (string.IsNullOrWhiteSpace(data) || data == "NO_FILES_IN_FOLDER" || data == "NO_CONTENTS") return files;
            
            Console.WriteLine($"[DEBUG][ParseSharedFolderFiles] Raw data: {data}");

            string[] items = data.Split('|');
            foreach (string item in items)
            {
                if (string.IsNullOrWhiteSpace(item)) continue;
                if (item.StartsWith("file:"))
                {
                    // Format from server: file:<file_id>:<file_name>:<file_path>:<relative_path>
                    var parts = item.Split(':');
                    if (parts.Length >= 5)
                    {
                        if (int.TryParse(parts[1], out int fileId))
                        {
                            string name = parts[2];
                            string filePath = parts[3];
                            string relativePath = parts[4];
                            
                            // Get file extension for type
                            string fileType = Path.GetExtension(name).ToLower();
                            
                            files.Add(new Services.FileItem
                            {
                                Id = fileId,
                                Name = name,
                                Type = fileType,
                                FilePath = filePath,
                                Size = "Unknown", // Server doesn't provide size in this format
                                CreatedAt = DateTime.Now.ToString(), // Placeholder
                                Owner = "Shared" // Placeholder for shared files
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
            }
            
            Console.WriteLine($"[DEBUG][ParseSharedFolderFiles] Parsed {files.Count} files");
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

        // Toolbar event handlers
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
                        Console.WriteLine($"[DEBUG] Success! Refreshing shared file list");
                        
                        // Refresh the list to show new shared items
                        await LoadSharedFoldersAndFilesAsync();
                        
                        Console.WriteLine($"[DEBUG] Shared file list refreshed");
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
                    MessageBox.Show($"Error getting files: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                Console.WriteLine($"[DEBUG] No password entered");
            }
        }

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            await PerformSearch();
        }

        private async Task PerformSearch()
        {
            try
            {
                string searchTerm = txtSearch.Text;
                if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm == "Tìm kiếm file...")
                {
                    DisplaySharedFoldersAndFiles();
                    return;
                }

                var filteredFiles = allSharedFiles.Where(f => 
                    f.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    f.Type.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    f.Owner.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0
                ).ToList();

                var filteredFolders = allSharedFolders.Where(f =>
                    f.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    f.Owner.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0
                ).ToList();

                // Display filtered results
                SharedFileLayoutPanel.Controls.Clear();
                
                foreach (var folder in filteredFolders)
                {
                    var folderControl = new FolderItemControl(folder.Name, folder.CreatedAt, folder.Owner, folder.IsShared, folder.Id);
                    folderControl.FolderClicked += async (folderId) => await NavigateToSharedFolderById(folderId);
                    SharedFileLayoutPanel.Controls.Add(folderControl);
                }
                
                foreach (var file in filteredFiles)
                {
                    var fileItemControl = new FileItemControl(file.Name, file.CreatedAt, file.Owner, file.Size, file.FilePath, file.Id);
                    SharedFileLayoutPanel.Controls.Add(fileItemControl);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error during search: {ex.Message}");
                MessageBox.Show($"Error during search: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            await LoadSharedFoldersAndFilesAsync();
        }

        // XA: btnDebug_Click, DebugListSharedFiles
        // (Kh�ng c�n code debug ? d�y)

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

        // Password dialog for GetFile functionality
        private string ShowPasswordDialog()
        {
            Form prompt = new Form()
            {
                Width = 400,
                Height = 200,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Get File from Other User",
                StartPosition = FormStartPosition.CenterScreen,
                MaximizeBox = false,
                MinimizeBox = false
            };
            
            Label textLabel = new Label() { Left = 20, Top = 20, Width = 350, Text = "Enter password to get files from other users:" };
            TextBox passwordBox = new TextBox() { Left = 20, Top = 50, Width = 350, UseSystemPasswordChar = true };
            Button confirmation = new Button() { Text = "Get Files", Left = 200, Width = 80, Top = 100, DialogResult = DialogResult.OK };
            Button cancel = new Button() { Text = "Cancel", Left = 290, Width = 80, Top = 100, DialogResult = DialogResult.Cancel };
            
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
                MessageBox.Show($"Error getting items by password: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return (null, null, false);
            }
        }

        private async Task<bool> AddFileReferenceAsync(string fileId, string userId, string sharePass)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync("127.0.0.1", 5000);
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
                Console.WriteLine($"Error adding file reference: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> AddFolderReferenceAsync(string folderId, string userId, string sharePass)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync("127.0.0.1", 5000);
                using (sslStream)
                using (var reader = new StreamReader(sslStream, Encoding.UTF8))
                using (var writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    // First get the permission from the share pass
                    string permission = await GetPermissionFromSharePassAsync(sharePass, "folder");
                    
                    // Add folder and all its files to share tables in one atomic transaction
                    string message = $"ADD_FOLDER_AND_FILES_SHARE|{folderId}|{userId}|{sharePass}|{permission}";
                    Console.WriteLine($"[DEBUG] Adding folder and files share: {message}");
                    await writer.WriteLineAsync(message);

                    string response = await reader.ReadLineAsync();
                    Console.WriteLine($"[DEBUG] Add folder and files response: '{response}'");
                    
                    return response != null && response.StartsWith("200|");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding folder reference: {ex.Message}");
                return false;
            }
        }



        private async Task<string> GetPermissionFromSharePassAsync(string sharePass, string itemType)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync("127.0.0.1", 5000);
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
                Console.WriteLine($"Error getting permission from share pass: {ex.Message}");
                return "read"; // Default to read permission
            }
        }

        private async Task<(int, int)> GetFolderInfoFromSharePassAsync(string sharePass)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync("localhost", 5000);
                using (sslStream)
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    string message = $"GET_FOLDER_INFO_BY_SHARE_PASS|{sharePass}";
                    Console.WriteLine($"[DEBUG] GetFolderInfoFromSharePassAsync: Sending message: {message.Trim()}");
                    await writer.WriteLineAsync(message);

                    string response = await reader.ReadLineAsync();
                    response = response?.Trim();
                    Console.WriteLine($"[DEBUG] GetFolderInfoFromSharePassAsync response: '{response}'");

                    if (response != null)
                    {
                        string[] parts = response.Split('|');
                        if (parts.Length >= 2 && parts[0] == "200")
                        {
                            // Parse folder info: folder_id:folder_name:created_at:owner_name
                            string[] folderInfo = parts[1].Split(':');
                            if (folderInfo.Length >= 4)
                            {
                                if (int.TryParse(folderInfo[0], out int folderId))
                                {
                                    // Get owner_id from owner_name (we need to query this)
                                    int ownerId = await GetOwnerIdFromUsernameAsync(folderInfo[3]);
                                    Console.WriteLine($"[DEBUG] Parsed folder: id={folderId}, owner={folderInfo[3]}, ownerId={ownerId}");
                                    return (folderId, ownerId);
                                }
                            }
                        }
                    }
                    return (-1, -1);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error getting folder info from share pass: {ex.Message}");
                return (-1, -1);
            }
        }

        private async Task<int> GetOwnerIdFromUsernameAsync(string username)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync("localhost", 5000);
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
                Console.WriteLine($"Error getting user ID from username: {ex.Message}");
                return -1;
            }
        }

        // Get file info from share password
        private async Task<(int, int)> GetFileInfoFromSharePassAsync(string sharePass)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync("localhost", 5000);
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
                Console.WriteLine($"Error getting file info from share pass: {ex.Message}");
                return (-1, -1);
            }
        }

        // Helper classes
        private class FolderNavigationItem
        {
            public int? FolderId { get; set; }
            public string FolderName { get; set; }
        }

        private void CreateCustomFileContextMenu(FileItemControl fileControl, int fileId, string fileName)
        {
            // Create a completely new context menu to override the default one
            var contextMenu = new ContextMenuStrip();
            
            // Download option
            var downloadItem = new ToolStripMenuItem("Download");
            downloadItem.Click += async (s, e) => await DownloadSharedFile(fileControl, fileId, fileName);
            contextMenu.Items.Add(downloadItem);
            
            // Remove from list option (instead of delete)
            var removeItem = new ToolStripMenuItem("Remove from my list");
            removeItem.Click += async (s, e) => await RemoveSharedFile(fileId, fileName);
            contextMenu.Items.Add(removeItem);
            
            // Use the new OverrideContextMenu method
            fileControl.OverrideContextMenu(contextMenu);
        }

        private void CreateCustomFolderContextMenu(FolderItemControl folderControl, int folderId, string folderName)
        {
            // Create a completely new context menu to override the default one
            var contextMenu = new ContextMenuStrip();
            
            // Download option for shared folders
            var downloadItem = new ToolStripMenuItem("Download");
            downloadItem.Click += async (s, e) => await DownloadSharedFolder(folderControl, folderId, folderName);
            contextMenu.Items.Add(downloadItem);
            
            // Remove from list option (instead of delete)
            var removeItem = new ToolStripMenuItem("Remove from my list");
            removeItem.Click += async (s, e) => await RemoveSharedFolder(folderId, folderName);
            contextMenu.Items.Add(removeItem);
            
            // Use the new OverrideContextMenu method
            folderControl.OverrideContextMenu(contextMenu);
        }

        private async Task DownloadSharedFile(FileItemControl fileControl, int fileId, string fileName)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = fileName;
                saveFileDialog.Title = "Save file as...";
                saveFileDialog.Filter = "All Files (*.*)|*.*";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    await fileControl.DownloadFileAsync(saveFileDialog.FileName);
                    MessageBox.Show("File downloaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error downloading file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task DownloadSharedFolder(FolderItemControl folderControl, int folderId, string folderName)
        {
            try
            {
                FolderBrowserDialog folderDialog = new FolderBrowserDialog();
                folderDialog.Description = "Chọn thư mục để lưu folder";
                folderDialog.ShowNewFolderButton = true;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    string targetPath = Path.Combine(folderDialog.SelectedPath, folderName);
                    
                    // Tạo thư mục đích
                    if (!Directory.Exists(targetPath))
                    {
                        Directory.CreateDirectory(targetPath);
                    }

                    // Download folder và tất cả files bên trong
                    await DownloadSharedFolderContentsAsync(folderId, targetPath);
                    
                    MessageBox.Show($"Folder '{folderName}' đã được tải về thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải folder: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task DownloadSharedFolderContentsAsync(int folderId, string targetPath)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync("127.0.0.1", 5000);
                using (sslStream)
                using (var reader = new StreamReader(sslStream, Encoding.UTF8))
                using (var writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    string message = $"GET_SHARED_FOLDER_CONTENTS|{folderId}|{Session.LoggedInUserId}";
                    Console.WriteLine($"[DEBUG] Getting shared folder contents for download: {message}");
                    await writer.WriteLineAsync(message);

                    string response = await reader.ReadLineAsync();
                    response = response?.Trim();
                    Console.WriteLine($"[DEBUG] Shared folder contents response for download: '{response}'");

                    if (response != null && response.StartsWith("200|"))
                    {
                        string data = response.Substring(4);
                        if (data != "NO_FILES_IN_FOLDER")
                        {
                            var files = ParseSharedFolderFiles(data);
                            foreach (var file in files)
                            {
                                try
                                {
                                    string filePath = Path.Combine(targetPath, file.Name);
                                    await DownloadSharedFileAsync(file.Id, filePath);
                                    Console.WriteLine($"[DEBUG] Downloaded file: {file.Name}");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"[ERROR] Failed to download file {file.Name}: {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tải nội dung folder: {ex.Message}");
            }
        }

        private async Task DownloadSharedFileAsync(int fileId, string savePath)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync("localhost", 5000);
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

        private async Task RemoveSharedFile(int fileId, string fileName)
        {
            DialogResult result = MessageBox.Show($"Bạn có chắc muốn xóa file '{fileName}' khỏi danh sách chia sẻ của bạn?", 
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                try
                {
                    bool success = await RemoveFileFromSharedListAsync(fileId);
                    if (success)
                    {
                        MessageBox.Show("File đã được xóa khỏi danh sách chia sẻ.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadSharedFoldersAndFilesAsync(); // Refresh the list
                    }
                    else
                    {
                        MessageBox.Show("Không thể xóa file khỏi danh sách chia sẻ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa file: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async Task RemoveSharedFolder(int folderId, string folderName)
        {
            DialogResult result = MessageBox.Show($"Bạn có chắc muốn xóa folder '{folderName}' khỏi danh sách chia sẻ của bạn?", 
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                try
                {
                    bool success = await RemoveFolderFromSharedListAsync(folderId);
                    if (success)
                    {
                        MessageBox.Show("Folder đã được xóa khỏi danh sách chia sẻ.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadSharedFoldersAndFilesAsync(); // Refresh the list
                    }
                    else
                    {
                        MessageBox.Show("Không thể xóa folder khỏi danh sách chia sẻ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa folder: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async Task<bool> RemoveFileFromSharedListAsync(int fileId)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync("localhost", 5000);
                using (sslStream)
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    string message = $"REMOVE_SHARED_FILE|{fileId}|{Session.LoggedInUserId}";
                    Console.WriteLine($"[DEBUG] Removing shared file: {message}");
                    await writer.WriteLineAsync(message);

                    string response = await reader.ReadLineAsync();
                    response = response?.Trim();
                    Console.WriteLine($"[DEBUG] Remove shared file response: '{response}'");

                    if (response != null)
                    {
                        string[] parts = response.Split('|');
                        return parts.Length >= 2 && parts[0] == "200" && parts[1] == "SHARED_FILE_REMOVED";
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error removing shared file: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> RemoveFolderFromSharedListAsync(int folderId)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync("localhost", 5000);
                using (sslStream)
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    string message = $"REMOVE_SHARED_FOLDER|{folderId}|{Session.LoggedInUserId}";
                    Console.WriteLine($"[DEBUG] Removing shared folder: {message}");
                    await writer.WriteLineAsync(message);

                    string response = await reader.ReadLineAsync();
                    response = response?.Trim();
                    Console.WriteLine($"[DEBUG] Remove shared folder response: '{response}'");

                    if (response != null)
                    {
                        string[] parts = response.Split('|');
                        return parts.Length >= 2 && parts[0] == "200" && parts[1] == "SHARED_FOLDER_REMOVED";
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error removing shared folder: {ex.Message}");
                return false;
            }
        }
    }
}

