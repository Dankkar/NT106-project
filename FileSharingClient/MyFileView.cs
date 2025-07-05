using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.IO;

namespace FileSharingClient
{
    public partial class MyFileView: UserControl
    {
        private static string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName;
        private static string dbPath = Path.Combine(projectRoot, "test.db");
        private static string connectionString = $"Data Source={dbPath};Version=3;Pooling=True";
        private int currentUserId = -1;
        private List<FileItem> allFiles = new List<FileItem>();
        private List<FolderItem> allFolders = new List<FolderItem>();
        private int? currentFolderId = null; // null = root level
        private Stack<FolderNavigationItem> navigationStack = new Stack<FolderNavigationItem>();

        public MyFileView()
        {
            InitializeComponent();
            MyFileLayoutPanel.FlowDirection = FlowDirection.TopDown;
            MyFileLayoutPanel.AutoScroll = true;
            MyFileLayoutPanel.WrapContents = false;
            
            // Set placeholder text style
            txtSearch.ForeColor = Color.Gray;
            
            _ = InitAsync();
        }

        private async Task InitAsync()
        {
            currentUserId = await GetUserIdFromSessionAsync();
            if (currentUserId != -1)
            {
                await LoadFoldersAndFilesAsync();
            }
        }

        private async Task LoadFoldersAndFilesAsync()
        {
            try
            {
                allFiles.Clear();
                allFolders.Clear();
                MyFileLayoutPanel.Controls.Clear();

                using (var conn = new SQLiteConnection(connectionString))
                {
                    await conn.OpenAsync();

                    // Load folders in current directory
                    string foldersQuery = @"
                        SELECT folder_id, folder_name, created_at, 'Me' as owner_type, 0 as is_shared
                        FROM folders 
                        WHERE owner_id = @userId 
                        AND (@currentFolderId IS NULL AND parent_folder_id IS NULL OR parent_folder_id = @currentFolderId)
                        AND status = 'ACTIVE'
                        ORDER BY folder_name";

                    using (var cmd = new SQLiteCommand(foldersQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", currentUserId);
                        cmd.Parameters.AddWithValue("@currentFolderId", (object)currentFolderId ?? DBNull.Value);
                        
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var folderItem = new FolderItem
                                {
                                    Id = Convert.ToInt32(reader["folder_id"]),
                                    Name = reader["folder_name"].ToString(),
                                    CreatedAt = reader["created_at"].ToString(),
                                    Owner = "Me",
                                    IsShared = false
                                };
                                allFolders.Add(folderItem);
                            }
                        }
                    }

                    // Load shared folders (only at root level)
                    if (currentFolderId == null)
                    {
                        string sharedFoldersQuery = @"
                            SELECT f.folder_id, f.folder_name, f.created_at, u.username as owner_name
                            FROM folder_shares fs 
                            JOIN folders f ON fs.folder_id = f.folder_id 
                            JOIN users u ON f.owner_id = u.user_id
                            WHERE fs.shared_with_user_id = @userId 
                            AND f.status = 'ACTIVE'
                            ORDER BY f.folder_name";

                        using (var cmd = new SQLiteCommand(sharedFoldersQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@userId", currentUserId);
                            using (var reader = await cmd.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    var folderItem = new FolderItem
                                    {
                                        Id = Convert.ToInt32(reader["folder_id"]),
                                        Name = reader["folder_name"].ToString(),
                                        CreatedAt = reader["created_at"].ToString(),
                                        Owner = reader["owner_name"].ToString() + " (Shared)",
                                        IsShared = true
                                    };
                                    allFolders.Add(folderItem);
                                }
                            }
                        }
                    }

                    // Load files in current directory
                    string myFilesQuery = @"
                        SELECT file_id, file_name, file_type, file_size, upload_at, file_path, 'Me' as owner_type
                        FROM files 
                        WHERE owner_id = @userId 
                        AND (@currentFolderId IS NULL AND folder_id IS NULL OR folder_id = @currentFolderId)
                        AND status = 'ACTIVE'
                        ORDER BY file_name";

                    using (var cmd = new SQLiteCommand(myFilesQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", currentUserId);
                        cmd.Parameters.AddWithValue("@currentFolderId", (object)currentFolderId ?? DBNull.Value);
                        
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var fileItem = new FileItem
                                {
                                    Id = Convert.ToInt32(reader["file_id"]),
                                    Name = reader["file_name"].ToString(),
                                    Type = reader["file_type"].ToString(),
                                    Size = FormatFileSize(Convert.ToInt64(reader["file_size"])),
                                    CreatedAt = reader["upload_at"].ToString(),
                                    Owner = "Me",
                                    FilePath = reader["file_path"].ToString(),
                                    IsShared = false,
                                    IsFolder = false
                                };
                                allFiles.Add(fileItem);
                            }
                        }
                    }

                    // Load shared files (only at root level)
                    if (currentFolderId == null)
                    {
                        string sharedFilesQuery = @"
                            SELECT f.file_id, f.file_name, f.file_type, f.file_size, f.upload_at, f.file_path, u.username as owner_name
                            FROM files_share fs 
                            JOIN files f ON fs.file_id = f.file_id 
                            JOIN users u ON f.owner_id = u.user_id
                            WHERE fs.user_id = @userId 
                            AND f.status = 'ACTIVE'
                            ORDER BY f.file_name";

                        using (var cmd = new SQLiteCommand(sharedFilesQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@userId", currentUserId);
                            using (var reader = await cmd.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    var fileItem = new FileItem
                                    {
                                        Id = Convert.ToInt32(reader["file_id"]),
                                        Name = reader["file_name"].ToString(),
                                        Type = reader["file_type"].ToString(),
                                        Size = FormatFileSize(Convert.ToInt64(reader["file_size"])),
                                        CreatedAt = reader["upload_at"].ToString(),
                                        Owner = reader["owner_name"].ToString() + " (Shared)",
                                        FilePath = reader["file_path"].ToString(),
                                        IsShared = true,
                                        IsFolder = false
                                    };
                                    allFiles.Add(fileItem);
                                }
                            }
                        }
                    }
                }

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
                MyFileLayoutPanel.Controls.Add(folderControl);
            }
            
            // Add files
            foreach (var file in allFiles)
            {
                var fileItemControl = new FileItemControl(file.Name, file.CreatedAt, file.Owner, file.Size, file.FilePath);
                fileItemControl.FileDeleted += async (filePath) => await OnFileDeleted(filePath);
                MyFileLayoutPanel.Controls.Add(fileItemControl);
            }
        }

        private Control CreateBackButton()
        {
            var backButton = new Button()
            {
                Text = "⬅ Back",
                Size = new Size(100, 40),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat
            };
            
            backButton.Click += async (s, e) => await NavigateBack();
            return backButton;
        }

        private Control CreateFolderControl(FolderItem folder)
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

        private async Task NavigateToFolder(FolderItem folder)
        {
            // Save current state to navigation stack
            navigationStack.Push(new FolderNavigationItem 
            { 
                FolderId = currentFolderId, 
                FolderName = currentFolderId == null ? "Root" : GetCurrentFolderName() 
            });
            
            currentFolderId = folder.Id;
            await LoadFoldersAndFilesAsync();
        }

        private async Task OnFolderDeleted(int folderId)
        {
            // Refresh the folder list after deletion
            await LoadFoldersAndFilesAsync();
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

        private async Task OnFileDeleted(string filePath)
        {
            // Refresh the file list after deletion
            await LoadFoldersAndFilesAsync();
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

        // Toolbar event handlers
        private async void btnNewFolder_Click(object sender, EventArgs e)
        {
            // Simple input dialog using InputDialog
            string folderName = ShowInputDialog("Enter folder name:", "New Folder", "New Folder");

            if (!string.IsNullOrWhiteSpace(folderName))
            {
                try
                {
                    // Build correct folder path
                    string folderPath;
                    string parentFolderPath = "";
                    
                    using (var conn = new SQLiteConnection(connectionString))
                    {
                        await conn.OpenAsync();
                        
                        // Get parent folder path if we're inside a folder
                        if (currentFolderId != null)
                        {
                            string parentQuery = "SELECT folder_path FROM folders WHERE folder_id = @folderId";
                            using (var parentCmd = new SQLiteCommand(parentQuery, conn))
                            {
                                parentCmd.Parameters.AddWithValue("@folderId", currentFolderId);
                                var result = await parentCmd.ExecuteScalarAsync();
                                if (result != null)
                                {
                                    parentFolderPath = result.ToString();
                                }
                            }
                        }
                        
                        // Build correct folder path
                        if (currentFolderId == null)
                        {
                            // Root level folder
                            folderPath = $"uploads/{currentUserId}/{folderName}";
                        }
                        else
                        {
                            // Subfolder - append to parent path
                            folderPath = $"{parentFolderPath}/{folderName}";
                        }
                        
                        string insertQuery = @"
                            INSERT INTO folders (folder_name, owner_id, parent_folder_id, folder_path, created_at, updated_at)
                            VALUES (@folderName, @ownerId, @parentFolderId, @folderPath, datetime('now'), datetime('now'))";

                        using (var cmd = new SQLiteCommand(insertQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@folderName", folderName);
                            cmd.Parameters.AddWithValue("@ownerId", currentUserId);
                            cmd.Parameters.AddWithValue("@parentFolderId", (object)currentFolderId ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@folderPath", folderPath);
                            
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }

                    // Create physical folder using the folder path from database
                    string physicalFolderPath = Path.Combine(projectRoot ?? Environment.CurrentDirectory, folderPath);
                    
                    Directory.CreateDirectory(physicalFolderPath);

                    MessageBox.Show("Folder created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadFoldersAndFilesAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error creating folder: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Title = "Select files to upload";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show($"Selected {openFileDialog.FileNames.Length} files. Upload functionality will be implemented.", 
                    "Upload", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // TODO: Implement upload functionality
            }
        }

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            await PerformSearch();
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
                f.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0 ||
                f.Type.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0 ||
                f.Owner.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0
            ).ToList();

            var filteredFolders = allFolders.Where(f =>
                f.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0 ||
                f.Owner.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0
            ).ToList();

            // Display filtered results
            MyFileLayoutPanel.Controls.Clear();
            
            foreach (var folder in filteredFolders)
            {
                var folderControl = CreateFolderControl(folder);
                MyFileLayoutPanel.Controls.Add(folderControl);
            }
            
            foreach (var file in filteredFiles)
            {
                var fileItemControl = new FileItemControl(file.Name, file.CreatedAt, file.Owner, file.Size, file.FilePath);
                fileItemControl.FileDeleted += async (filePath) => await OnFileDeleted(filePath);
                MyFileLayoutPanel.Controls.Add(fileItemControl);
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

        public void AddFileToView(string fileName, string createAt, string owner, string filesize, string filePath)
        {
            var fileItem = new FileItemControl(fileName, createAt, owner, filesize, filePath);
            MyFileLayoutPanel.Controls.Add(fileItem);
        }

        private void MyFileLayoutPanel_Paint(object sender, PaintEventArgs e)
        {

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

        // Helper classes
        private class FileItem
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

        private class FolderItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string CreatedAt { get; set; }
            public string Owner { get; set; }
            public bool IsShared { get; set; }
        }

        private class FolderNavigationItem
        {
            public int? FolderId { get; set; }
            public string FolderName { get; set; }
        }
    }
}
