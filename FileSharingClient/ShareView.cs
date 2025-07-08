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
                // Note: Shared folders cannot be deleted by current user
                SharedFileLayoutPanel.Controls.Add(folderControl);
            }
            
            // Add shared files
            foreach (var file in allSharedFiles)
            {
                Console.WriteLine($"[DEBUG][ShareView] Creating shared file control: {file.Name}, Owner: '{file.Owner}'");
                
                var fileItemControl = new FileItemControl(file.Name, file.CreatedAt, file.Owner, file.Size, file.FilePath, file.Id);
                // Note: Shared files cannot be deleted by current user, but can be previewed
                SharedFileLayoutPanel.Controls.Add(fileItemControl);
            }
        }

        private Control CreateBackButton()
        {
            var backButton = new Button()
            {
                Text = "? Back",
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
                            var files = ParseFileItems(data);
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
            if (data == "NO_FILES" || data == "NO_SHARED_FILES" || data == "NO_FILES_IN_FOLDER") return files;
            
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
                    
                    Console.WriteLine($"[DEBUG][ParseFileItems] Parsed shared file: {name}, Owner: {owner}");
                    
                    long sizeBytes = 0;
                    long.TryParse(size, out sizeBytes);
                    
                    files.Add(new Services.FileItem
                    {
                        Id = int.Parse(id),
                        Name = name,
                        Type = type,
                        Size = FormatFileSize(sizeBytes),
                        CreatedAt = uploadAt,
                        Owner = owner,
                        FilePath = path,
                        IsShared = true
                    });
                }
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
            string searchTerm = txtSearch.Text;
            if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm == "T�m ki?m file...")
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

        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            await LoadSharedFoldersAndFilesAsync();
        }

        // X�A: btnDebug_Click, DebugListSharedFiles
        // (Kh�ng c�n code debug ? d�y)

        private void txtSearch_Enter(object sender, EventArgs e)
        {
            if (txtSearch.Text == "T�m ki?m file...")
            {
                txtSearch.Text = "";
                txtSearch.ForeColor = Color.Black;
            }
        }

        private void txtSearch_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.Text = "T�m ki?m file...";
                txtSearch.ForeColor = Color.Gray;
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
                        MessageBox.Show("B?n l� ch? s? h?u c?a file n�y. Kh�ng c?n chia s? l?i.", "Th�ng b�o", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return (null, null, false);
                    }
                    
                    // Add reference to files_share table
                    bool shareResult = await AddFileReferenceAsync(fileId.ToString(), currentUserId.ToString(), password);
                    Console.WriteLine($"[DEBUG] AddFileReferenceAsync result: {shareResult}");
                    
                    if (shareResult)
                    {
                        Console.WriteLine($"[DEBUG] File reference added successfully");
                        MessageBox.Show("B?n d� c� quy?n truy c?p v�o file n�y!", "Th�ng b�o", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return (new List<Services.FileItem>(), new List<Services.FolderItem>(), true);
                    }
                    else
                    {
                        Console.WriteLine($"[DEBUG] Failed to add file reference");
                        MessageBox.Show("L?i khi th�m quy?n truy c?p file!", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                        MessageBox.Show("B?n l� ch? s? h?u c?a thu m?c n�y. Kh�ng c?n chia s? l?i.", "Th�ng b�o", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return (null, null, false);
                    }
                    
                    // Add reference to folder_shares table
                    bool shareResult = await AddFolderReferenceAsync(folderId.ToString(), currentUserId.ToString(), password);
                    Console.WriteLine($"[DEBUG] AddFolderReferenceAsync result: {shareResult}");
                    
                    if (shareResult)
                    {
                        Console.WriteLine($"[DEBUG] Folder reference added successfully");
                        MessageBox.Show("B?n d� c� quy?n truy c?p v�o thu m?c n�y!", "Th�ng b�o", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return (new List<Services.FileItem>(), new List<Services.FolderItem>(), true);
                    }
                    else
                    {
                        Console.WriteLine($"[DEBUG] Failed to add folder reference");
                        MessageBox.Show("L?i khi th�m quy?n truy c?p thu m?c!", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return (null, null, false);
                    }
                }
                
                Console.WriteLine($"[DEBUG] Invalid password, no file or folder found");
                MessageBox.Show("M?t kh?u kh�ng h?p l?!", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    
                    // Add entry to folder_shares table with permission
                    string message = $"ADD_FOLDER_SHARE_ENTRY_WITH_PERMISSION|{folderId}|{userId}|{sharePass}|{permission}";
                    Console.WriteLine($"[DEBUG] Adding folder share entry: {message}");
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
                Console.WriteLine($"Error adding folder reference: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> AddFilesInFolderToShareAsync(string folderId, string userId, string sharePass, string permission)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync("127.0.0.1", 5000);
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
                Console.WriteLine($"Error adding files in folder to share: {ex.Message}");
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
    }
}

