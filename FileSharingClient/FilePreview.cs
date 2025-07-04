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
using System.Data.SqlClient;
using System.Data.Common;
using System.Net.NetworkInformation;

namespace FileSharingClient
{
    public partial class FilePreview : UserControl
    {
        private static string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName;
        private static string dbPath = Path.Combine(projectRoot, "test.db");
        private static string connectionString = $"Data Source={dbPath};Version=3;Pooling=True";
        private int currentUserId = -1;

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

        private async Task AddSharedFoldersAndFiles(TreeNode parentNode, int userId)
        {
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
            if (string.IsNullOrEmpty(fileType)) return "📄";
            
            fileType = fileType.ToLower();
            
            if (fileType.Contains("image") || fileType == "jpg" || fileType == "png" || fileType == "gif" || fileType == "jpeg")
                return "🖼️";
            else if (fileType.Contains("pdf") || fileType == "pdf")
                return "📕";
            else if (fileType.Contains("text") || fileType == "txt")
                return "📝";
            else if (fileType.Contains("video") || fileType == "mp4" || fileType == "avi" || fileType == "mov")
                return "🎥";
            else if (fileType.Contains("audio") || fileType == "mp3" || fileType == "wav" || fileType == "m4a")
                return "🎵";
            else if (fileType == "zip" || fileType == "rar" || fileType == "7z")
                return "📦";
            else
                return "📄";
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
                using (var conn = new SQLiteConnection(connectionString))
                {
                    await conn.OpenAsync();
                    
                    string query = @"
                        SELECT file_path, file_type, file_name
                        FROM files 
                        WHERE file_id = @fileId";

                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@fileId", fileId);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                string relativePath = reader["file_path"].ToString();
                                string fileType = reader["file_type"].ToString().ToLower();
                                string fileName = reader["file_name"].ToString();
                                string fullPath = Path.Combine(projectRoot, relativePath);

                                await ShowPreviewAsync(fullPath, fileType, fileName);
                            }
                            else
                            {
                                MessageBox.Show("File not found in database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
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
                if (fileType.Contains("text") || filePath.EndsWith(".txt") || filePath.EndsWith(".md"))
                {
                    string content = await Task.Run(() => File.ReadAllText(filePath));
                    previewText.Text = content;
                    previewText.Visible = true;
                }
                else if (fileType.Contains("image") || filePath.EndsWith(".png") || filePath.EndsWith(".jpg") || 
                         filePath.EndsWith(".jpeg") || filePath.EndsWith(".gif") || filePath.EndsWith(".bmp"))
                {
                    using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        previewImage.Image = Image.FromStream(fs);
                        previewImage.Visible = true;
                    }
                }
                else if (fileType.Contains("pdf") || filePath.EndsWith(".pdf"))
                {
                    previewPdf.Navigate(filePath);
                    previewPdf.Visible = true;
                }
                else
                {
                    previewText.Text = $"Preview not supported for this file type.\nFile: {fileName}\nType: {fileType}\nPath: {filePath}";
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
            int userId = -1;

            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = "SELECT user_id FROM users WHERE username = @username";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", Session.LoggedInUser);
                        object result = await cmd.ExecuteScalarAsync();
                        if (result != null)
                        {
                            userId = Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting user_id: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return userId;
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
