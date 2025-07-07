using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Data.SQLite;
using System.Net.Sockets;

namespace FileSharingClient
{
    public partial class FilePreview : UserControl
    {
        private static string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName ?? Environment.CurrentDirectory;
        private static string dbPath = Path.Combine(projectRoot, "test.db");
        private static string connectionString = $"Data Source={dbPath};Version=3;Pooling=True";
        private int currentUserId = -1;
        private const string SERVER_IP = "localhost";
        private const int SERVER_PORT = 5000;

        public FilePreview()
        {
            InitializeComponent();
            // Add event handler for treeView node click
            treeView.NodeMouseClick += TreeView_NodeMouseClick;
            _ = InitAsync();
        }

        public async Task Reload()
        {
            await LoadTreeViewAsync();
        }

        private async Task InitAsync()
        {
            currentUserId = await GetUserIdFromSessionAsync();
            if (currentUserId != -1)
            {
                await LoadTreeViewAsync();
            }
        }

        private async Task LoadTreeViewAsync()
        {
            try
            {
                treeView.Nodes.Clear();

                // My Document node
                TreeNode myDocNode = new TreeNode("My Document");
                myDocNode.Tag = new NodeTag { IsFolder = true, IsRoot = true };
                await AddUserFoldersAndFiles(myDocNode, currentUserId, null);
                treeView.Nodes.Add(myDocNode);

                // Shared With Me node
                TreeNode sharedNode = new TreeNode("Shared With Me");
                sharedNode.Tag = new NodeTag { IsFolder = true, IsRoot = true };
                await AddSharedFoldersAndFiles(sharedNode, currentUserId);
                treeView.Nodes.Add(sharedNode);

                treeView.ExpandAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading tree view: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task AddUserFoldersAndFiles(TreeNode parentNode, int userId, int? parentFolderId)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToSecureServerAsync(SERVER_IP, SERVER_PORT);
                using (sslStream)
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    // Get user files
                    string message = $"GET_USER_FILES|{userId}|{parentFolderId?.ToString() ?? "null"}";
                    await writer.WriteLineAsync(message);

                    string response = await reader.ReadLineAsync();
                    response = response?.Trim();

                    if (response != null)
                    {
                        string[] parts = response.Split('|');
                        if (parts.Length >= 1 && parts[0] == "200")
                        {
                            // Parse folder and file data from response
                            // This is a simplified approach - in practice, you'd parse the JSON response
                            // For now, we'll keep the existing SQLite approach but update it to use API calls
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Fallback to direct database access for now
                using (var conn = new SQLiteConnection(connectionString))
                {
                    await conn.OpenAsync();

                    // Add folders first
                    string folderQuery = @"
                        SELECT folder_id, folder_name 
                        FROM folders 
                        WHERE owner_id = @userId 
                        AND ((@parentId IS NULL AND parent_folder_id IS NULL) OR parent_folder_id = @parentId)
                        AND status = 'ACTIVE'
                        ORDER BY folder_name";

                    using (var cmd = new SQLiteCommand(folderQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.Parameters.AddWithValue("@parentId", (object)parentFolderId ?? DBNull.Value);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int folderId = Convert.ToInt32(reader["folder_id"]);
                                string folderName = reader["folder_name"].ToString();
                                
                                TreeNode folderNode = new TreeNode($"📁 {folderName}");
                                folderNode.Tag = new NodeTag 
                                { 
                                    IsFolder = true, 
                                    Id = folderId, 
                                    IsShared = false,
                                    Name = folderName
                                };
                                
                                // Recursively add subfolders and files
                                await AddUserFoldersAndFiles(folderNode, userId, folderId);
                                parentNode.Nodes.Add(folderNode);
                            }
                        }
                    }

                    // Add files in current folder
                    string fileQuery = @"
                        SELECT file_id, file_name, file_type 
                        FROM files 
                        WHERE owner_id = @userId 
                        AND ((@parentId IS NULL AND folder_id IS NULL) OR folder_id = @parentId)
                        AND status = 'ACTIVE'
                        ORDER BY file_name";

                    using (var cmd = new SQLiteCommand(fileQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.Parameters.AddWithValue("@parentId", (object)parentFolderId ?? DBNull.Value);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int fileId = Convert.ToInt32(reader["file_id"]);
                                string fileName = reader["file_name"].ToString();
                                string fileType = reader["file_type"].ToString();
                                
                                string icon = GetFileIcon(fileType);
                                TreeNode fileNode = new TreeNode($"{icon} {fileName}");
                                fileNode.Tag = new NodeTag 
                                { 
                                    IsFolder = false, 
                                    Id = fileId, 
                                    IsShared = false,
                                    Name = fileName
                                };
                                
                                parentNode.Nodes.Add(fileNode);
                            }
                        }
                    }
                }
            }
        }

        private async Task AddSharedFoldersAndFiles(TreeNode parentNode, int userId)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToSecureServerAsync(SERVER_IP, SERVER_PORT);
                using (sslStream)
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    // Get shared files
                    string message = $"GET_SHARED_FILES|{userId}";
                    await writer.WriteLineAsync(message);

                    string response = await reader.ReadLineAsync();
                    response = response?.Trim();

                    if (response != null)
                    {
                        string[] parts = response.Split('|');
                        if (parts.Length >= 1 && parts[0] == "200")
                        {
                            // Parse shared file data from response
                            // For now, fallback to database approach
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Fallback to direct database access
                using (var conn = new SQLiteConnection(connectionString))
                {
                    await conn.OpenAsync();

                    // Add shared folders
                    string sharedFolderQuery = @"
                        SELECT f.folder_id, f.folder_name 
                        FROM folder_shares fs 
                        JOIN folders f ON fs.folder_id = f.folder_id 
                        WHERE fs.shared_with_user_id = @userId 
                        AND f.status = 'ACTIVE'
                        ORDER BY f.folder_name";

                    using (var cmd = new SQLiteCommand(sharedFolderQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int folderId = Convert.ToInt32(reader["folder_id"]);
                                string folderName = reader["folder_name"].ToString();
                                
                                TreeNode folderNode = new TreeNode($"📁 {folderName} (Shared)");
                                folderNode.Tag = new NodeTag 
                                { 
                                    IsFolder = true, 
                                    Id = folderId, 
                                    IsShared = true,
                                    Name = folderName
                                };
                                
                                // Add files in shared folder
                                await AddFilesInSharedFolder(folderNode, folderId);
                                parentNode.Nodes.Add(folderNode);
                            }
                        }
                    }

                    // Add directly shared files (not in folders)
                    string sharedFileQuery = @"
                        SELECT f.file_id, f.file_name, f.file_type 
                        FROM files_share fs 
                        JOIN files f ON fs.file_id = f.file_id 
                        WHERE fs.user_id = @userId 
                        AND f.status = 'ACTIVE'
                        ORDER BY f.file_name";

                    using (var cmd = new SQLiteCommand(sharedFileQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int fileId = Convert.ToInt32(reader["file_id"]);
                                string fileName = reader["file_name"].ToString();
                                string fileType = reader["file_type"].ToString();
                                
                                string icon = GetFileIcon(fileType);
                                TreeNode fileNode = new TreeNode($"{icon} {fileName} (Shared)");
                                fileNode.Tag = new NodeTag 
                                { 
                                    IsFolder = false, 
                                    Id = fileId, 
                                    IsShared = true,
                                    Name = fileName
                                };
                                
                                parentNode.Nodes.Add(fileNode);
                            }
                        }
                    }
                }
            }
        }

        private async Task AddFilesInSharedFolder(TreeNode folderNode, int folderId)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                await conn.OpenAsync();

                string fileQuery = @"
                    SELECT file_id, file_name, file_type 
                    FROM files 
                    WHERE folder_id = @folderId 
                    AND status = 'ACTIVE'
                    ORDER BY file_name";

                using (var cmd = new SQLiteCommand(fileQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@folderId", folderId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int fileId = Convert.ToInt32(reader["file_id"]);
                            string fileName = reader["file_name"].ToString();
                            string fileType = reader["file_type"].ToString();
                            
                            string icon = GetFileIcon(fileType);
                            TreeNode fileNode = new TreeNode($"{icon} {fileName}");
                            fileNode.Tag = new NodeTag 
                            { 
                                IsFolder = false, 
                                Id = fileId, 
                                IsShared = true,
                                Name = fileName
                            };
                            
                            folderNode.Nodes.Add(fileNode);
                        }
                    }
                }
            }
        }

        private string GetFileIcon(string fileType)
        {
            switch (fileType.ToLower())
            {
                case "text":
                case ".txt":
                case ".md":
                case ".log":
                    return "📝";
                case "pdf":
                case ".pdf":
                    return "📕";
                case "image":
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".gif":
                case ".bmp":
                    return "🖼️";
                case "video":
                case ".mp4":
                case ".avi":
                case ".mov":
                case ".wmv":
                case ".mkv":
                    return "🎥";
                case "audio":
                case ".mp3":
                case ".wav":
                case ".flac":
                    return "🎵";
                case "document":
                case ".docx":
                case ".doc":
                    return "📄";
                case "spreadsheet":
                case ".xlsx":
                case ".xls":
                    return "📊";
                case "presentation":
                case ".pptx":
                case ".ppt":
                    return "📋";
                case "archive":
                case ".zip":
                case ".rar":
                case ".7z":
                    return "📦";
                default:
                    return "📄";
            }
        }

        private async void TreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node?.Tag is NodeTag nodeTag && !nodeTag.IsFolder && !nodeTag.IsRoot)
            {
                await PreviewFile(nodeTag.Id, nodeTag.IsShared);
            }
        }

        private async Task PreviewFile(int fileId, bool isShared)
        {
            try
            {
                // Get file info via API
                var (sslStream, _) = await SecureChannelHelper.ConnectToSecureServerAsync(SERVER_IP, SERVER_PORT);
                using (sslStream)
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    string message = $"GET_FILE_INFO|{fileId}";
                    await writer.WriteLineAsync(message);

                    string response = await reader.ReadLineAsync();
                    response = response?.Trim();

                    if (response != null)
                    {
                        string[] parts = response.Split('|');
                        if (parts.Length >= 4 && parts[0] == "200")
                        {
                            string fileName = parts[1];
                            string fileType = parts[2];
                            string filePath = parts[3];
                            
                            string fullPath = Path.Combine(projectRoot, filePath);
                            await ShowPreviewAsync(fullPath, fileType, fileName);
                        }
                        else
                        {
                            MessageBox.Show("File not found or access denied.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error previewing file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task ShowPreviewAsync(string filePath, string fileType, string fileName)
        {
            // Hide all preview controls first
            previewText.Visible = false;
            previewImage.Visible = false;
            previewPdf.Visible = false;
            
            if (!File.Exists(filePath))
            {
                previewText.Text = $"File not found: {fileName}\nPath: {filePath}";
                previewText.Visible = true;
                return;
            }

            try
            {
                // Check if we have user password for decryption
                if (string.IsNullOrEmpty(Session.UserPassword))
                {
                    previewText.Text = $"Cannot preview encrypted file: User password not available.\nPlease re-login to preview encrypted files.";
                    previewText.Visible = true;
                    return;
                }

                // Read and decrypt the file
                byte[] encryptedData = await Task.Run(() => File.ReadAllBytes(filePath));
                byte[] decryptedData = null;

                try
                {
                    decryptedData = CryptoHelper.DecryptFile(encryptedData, Session.UserPassword);
                }
                catch (Exception decryptEx)
                {
                    // If decryption fails, the file might not be encrypted yet (legacy files)
                    // Try to read as plain text
                    previewText.Text = $"Decryption failed: {decryptEx.Message}\n\nThis file may not be encrypted yet. Raw content preview:\n\n{Encoding.UTF8.GetString(encryptedData, 0, Math.Min(1000, encryptedData.Length))}...";
                    previewText.Visible = true;
                    return;
                }

                // Display decrypted content based on file type
                if (fileType.Contains("text") || fileName.EndsWith(".txt") || fileName.EndsWith(".md"))
                {
                    string content = Encoding.UTF8.GetString(decryptedData);
                    previewText.Text = content;
                    previewText.Visible = true;
                }
                else if (fileType.Contains("image") || fileName.EndsWith(".png") || fileName.EndsWith(".jpg") || 
                         fileName.EndsWith(".jpeg") || fileName.EndsWith(".gif") || fileName.EndsWith(".bmp"))
                {
                    using (var ms = new MemoryStream(decryptedData))
                    {
                        previewImage.Image = Image.FromStream(ms);
                        previewImage.Visible = true;
                    }
                }
                else if (fileType.Contains("pdf") || fileName.EndsWith(".pdf"))
                {
                    // For PDF, we need to save decrypted content to a temporary file
                    string tempDir = Path.Combine(Path.GetTempPath(), "FileSharingPreview");
                    
                    // Create temp directory if it doesn't exist
                    if (!Directory.Exists(tempDir))
                    {
                        Directory.CreateDirectory(tempDir);
                    }
                    
                    // Create temporary file with proper name and extension
                    string tempFileName = $"preview_{DateTime.Now:yyyyMMdd_HHmmss}_{Path.GetRandomFileName()}.pdf";
                    string tempPath = Path.Combine(tempDir, tempFileName);
                    
                    await Task.Run(() => File.WriteAllBytes(tempPath, decryptedData));
                    
                    // Set file attributes to normal to avoid signature issues
                    File.SetAttributes(tempPath, FileAttributes.Normal);
                    
                    previewPdf.Navigate(tempPath);
                    previewPdf.Visible = true;
                    
                    // Clean up temp file after a longer delay (30 seconds)
                    _ = Task.Delay(30000).ContinueWith(_ => 
                    {
                        try 
                        { 
                            File.Delete(tempPath);
                            
                            // Also clean up temp directory if empty
                            if (Directory.Exists(tempDir) && !Directory.EnumerateFileSystemEntries(tempDir).Any())
                            {
                                Directory.Delete(tempDir);
                            }
                        } 
                        catch 
                        { 
                            // Ignore cleanup errors
                        }
                    });
                }
                else
                {
                    // For other file types, show hex preview
                    string hexPreview = BitConverter.ToString(decryptedData, 0, Math.Min(500, decryptedData.Length)).Replace("-", " ");
                    previewText.Text = $"Binary file preview (first 500 bytes in hex):\nFile: {fileName}\nType: {fileType}\nSize: {decryptedData.Length} bytes\n\n{hexPreview}";
                    previewText.Visible = true;
                }
            }
            catch (Exception ex)
            {
                previewText.Text = $"Error loading file preview: {ex.Message}\nFile: {fileName}";
                previewText.Visible = true;
            }
        }

        private async Task<int> GetUserIdFromSessionAsync()
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToSecureServerAsync(SERVER_IP, SERVER_PORT);
                using (sslStream)
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    // Get user ID from session
                    string message = $"GET_USER_ID|{Session.LoggedInUser}";
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
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting user ID: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return -1;
        }

        // Class to store node information
        private class NodeTag
        {
            public bool IsFolder { get; set; }
            public int Id { get; set; }
            public bool IsShared { get; set; }
            public bool IsRoot { get; set; }
            public string Name { get; set; }
        }
    }
}

