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
using System.IO.Compression;
using System.Security.Cryptography;
using FileSharingClient;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace FileSharingClient
{
    public partial class UploadView: UserControl
    {
        private string serverIp = ConfigurationManager.AppSettings["ServerIP"];
        private int serverPort = int.Parse(ConfigurationManager.AppSettings["ServerPort"]);
        private int chunkSize = int.Parse(ConfigurationManager.AppSettings["ChunkSize"]);
        private long maxFileSize = long.Parse(ConfigurationManager.AppSettings["MaxFileSizeMB"]) * 1024 * 1024;
        private string uploadsPath = ConfigurationManager.AppSettings["UploadsPath"];
        private string databasePath = ConfigurationManager.AppSettings["DatabasePath"];

        public event Func<Task> FileUploaded;
        private class PendingFile
        {
            public string FilePath { get; set; }
            public string RelativePath { get; set; }
        }
        
        private class PendingFolder
        {
            public string FolderPath { get; set; }
            public string FolderName { get; set; }
            public long TotalSize { get; set; }
            public int FileCount { get; set; }
            public List<PendingFile> Files { get; set; } = new List<PendingFile>();
            public List<PendingFolder> SubFolders { get; set; } = new List<PendingFolder>();
        }
        
        private class FolderNavigationItem
        {
            public string FolderPath { get; set; }
            public string FolderName { get; set; }
        }
        
        private List<PendingFile> pendingFiles = new List<PendingFile>();
        private List<PendingFolder> pendingFolders = new List<PendingFolder>();
        private long totalSizeBytes = 0;
        // Using chunkSize from configuration instead of hardcoded BUFFER_SIZE

        // Navigation properties
        private string currentFolderPath = null; // null = root level
        private Stack<FolderNavigationItem> navigationStack = new Stack<FolderNavigationItem>();

        public UploadView()
        {
            InitializeComponent();
            UploadFilePanel.FlowDirection = FlowDirection.TopDown;
            UploadFilePanel.AutoScroll = true;
            UploadFilePanel.WrapContents = false;
            AddHeaderRow();
            DisplayCurrentItems();
        }

        private string FormatFileSize(long bytes)
        {
            if (bytes >= 1024 * 1024) return $"{bytes / (1024 * 1024.0):0.##} MB";
            if(bytes >= 1024) return $"{bytes / 1024.0:0.##} KB";
            return $"{bytes} B";
        }



        private void DisplayCurrentItems()
        {
            // Clear current display (except header)
            ClearFileList();

            // Add back button if not at root
            if (currentFolderPath != null)
            {
                AddBackButton();
            }

            if (currentFolderPath == null)
            {
                // Root level - show all individual files and top-level folders
                foreach (var file in pendingFiles)
                {
                    var fileItem = new FileUploadItemControl(
                        Path.GetFileName(file.FilePath), 
                        Session.LoggedInUser, 
                        FormatFileSize(new FileInfo(file.FilePath).Length), 
                        file.FilePath
                    );
                    fileItem.FileRemoved += OnFileRemoved;
                    UploadFilePanel.Controls.Add(fileItem);
                }

                foreach (var folder in pendingFolders)
                {
                    var folderItem = new FolderUploadItemControl(
                        folder.FolderName, 
                        Session.LoggedInUser, 
                        FormatFileSize(folder.TotalSize), 
                        folder.FolderPath, 
                        folder.FileCount
                    );
                    folderItem.FolderRemoved += OnFolderRemoved;
                    folderItem.FolderNavigationRequested += OnFolderNavigationRequested;
                    UploadFilePanel.Controls.Add(folderItem);
                }
            }
            else
            {
                // Inside a folder - show contents of current folder
                var currentFolder = FindFolderByPath(currentFolderPath);
                if (currentFolder != null)
                {
                    // Show files in current folder
                    foreach (var file in currentFolder.Files)
                    {
                        var fileItem = new FileUploadItemControl(
                            Path.GetFileName(file.FilePath), 
                            Session.LoggedInUser, 
                            FormatFileSize(new FileInfo(file.FilePath).Length), 
                            file.FilePath
                        );
                        fileItem.FileRemoved += OnFileRemoved;
                        UploadFilePanel.Controls.Add(fileItem);
                    }

                    // Show subfolders
                    foreach (var subFolder in currentFolder.SubFolders)
                    {
                        var folderItem = new FolderUploadItemControl(
                            subFolder.FolderName, 
                            Session.LoggedInUser, 
                            FormatFileSize(subFolder.TotalSize), 
                            subFolder.FolderPath, 
                            subFolder.FileCount
                        );
                        folderItem.FolderRemoved += OnFolderRemoved;
                        folderItem.FolderNavigationRequested += OnFolderNavigationRequested;
                        UploadFilePanel.Controls.Add(folderItem);
                    }
                }
            }
        }

        private void AddBackButton()
        {
            var backButton = new Button
            {
                Text = "⬅ Quay lại",
                Width = 100,
                Height = 30,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.LightBlue,
                FlatStyle = FlatStyle.Flat
            };
            backButton.Click += (s, e) => NavigateBack();
            UploadFilePanel.Controls.Add(backButton);
        }

        private void NavigateBack()
        {
            if (navigationStack.Count > 0)
            {
                var previous = navigationStack.Pop();
                currentFolderPath = previous.FolderPath;
                DisplayCurrentItems();
            }
        }

        private void OnFolderNavigationRequested(string folderPath)
        {
            // Save current state to navigation stack
            navigationStack.Push(new FolderNavigationItem 
            { 
                FolderPath = currentFolderPath, 
                FolderName = currentFolderPath == null ? "Root" : Path.GetFileName(currentFolderPath)
            });
            
            currentFolderPath = folderPath;
            DisplayCurrentItems();
        }

        private PendingFolder FindFolderByPath(string folderPath)
        {
            return pendingFolders.FirstOrDefault(f => f.FolderPath == folderPath);
        }

        private void DragPanel_DragDrop(object sender, DragEventArgs e)
        {
            string[] items = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var item in items)
            {
                if (Directory.Exists(item))
                {
                    ProcessLocalFolder(item);
                }
                else if (File.Exists(item))
                {
                    ProcessLocalFile(item);
                }
            }
        }

        private void DragPanel_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private async void btnUpload_Click(object sender, EventArgs e)
        {
            try
            {
                int totalItems = pendingFiles.Count + pendingFolders.Count;
                if (totalItems == 0)
                {
                    MessageBox.Show("Không có file hoặc folder nào để upload!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Show progress panel and disable upload button
                progressPanel.Visible = true;
                btnUpload.Enabled = false;
                progressBarUpload.Value = 0;
                progressBarUpload.Maximum = totalItems;

                int completedItems = 0;

                // Upload individual files first
                foreach (var file in pendingFiles)
                {
                    lblProgressStatus.Text = $"Đang upload file: {Path.GetFileName(file.FilePath)} ({completedItems + 1}/{totalItems})";
                    Application.DoEvents(); // Update UI
                    
                    await UploadSingleFile(file.FilePath);
                    
                    completedItems++;
                    progressBarUpload.Value = completedItems;
                    Application.DoEvents(); // Update UI
                }

                // Upload folders
                foreach (var folder in pendingFolders)
                {
                    lblProgressStatus.Text = $"Đang upload folder: {folder.FolderName} ({completedItems + 1}/{totalItems})";
                    Application.DoEvents(); // Update UI
                    
                    await UploadFolderRecursive(folder);
                    
                    completedItems++;
                    progressBarUpload.Value = completedItems;
                    Application.DoEvents(); // Update UI
                }

                // Upload completed
                lblProgressStatus.Text = "Upload hoàn thành!";
                Application.DoEvents();
                
                await Task.Delay(1000); // Show completion message for 1 second
                
                MessageBox.Show("Upload thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Clear all pending items
                pendingFiles.Clear();
                pendingFolders.Clear();
                totalSizeBytes = 0;
                currentFolderPath = null;
                navigationStack.Clear();
                DisplayCurrentItems();
                UpdateFileSizeLabel();
                
                if (FileUploaded != null)
                    await FileUploaded.Invoke();
            }
            catch (Exception ex)
            {
                lblProgressStatus.Text = "Upload thất bại!";
                MessageBox.Show($"Lỗi upload: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Hide progress panel and re-enable upload button
                progressPanel.Visible = false;
                btnUpload.Enabled = true;
                progressBarUpload.Value = 0;
                lblProgressStatus.Text = "Đang chuẩn bị...";
            }
        }

        private async Task UploadSingleFile(string filePath)
        {
            try
            {
                lblProgressStatus.Text = $"Đang mã hóa file: {Path.GetFileName(filePath)}";
                Application.DoEvents();

                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync(serverIp, serverPort);
                using (sslStream)
                {
                    // Encrypt file before uploading
                    byte[] encryptedData = CryptoHelper.EncryptFileFromDisk(filePath, Session.UserPassword);
                    string fileName = Path.GetFileName(filePath);
                    int ownerId = Session.LoggedInUserId;
                    string uploadAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    string command = $"UPLOAD|{fileName}|{encryptedData.Length}|{ownerId}|{uploadAt}";
                    byte[] commandBytes = Encoding.UTF8.GetBytes(command + "\n");
                    
                    lblProgressStatus.Text = $"Đang gửi file: {fileName}";
                    Application.DoEvents();
                    
                    await sslStream.WriteAsync(commandBytes, 0, commandBytes.Length);
                    await sslStream.FlushAsync();
                    Console.WriteLine($"Đã gửi lệnh: {command.Trim()}");
                    
                    // Send encrypted file data
                    await sslStream.WriteAsync(encryptedData, 0, encryptedData.Length);
                    await sslStream.FlushAsync();
                    
                    lblProgressStatus.Text = $"Đang chờ phản hồi cho file: {fileName}";
                    Application.DoEvents();
                    
                    using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                    {
                        string response = await reader.ReadLineAsync();
                        Console.WriteLine($"Server trả về: {response}");
                        if (response.Trim() == "413")
                            throw new Exception("File quá lớn. Vui lòng thử lại với file nhỏ hơn.");
                        else if (response.Trim() != "200")
                            throw new Exception($"Lỗi server: {response.Trim()}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi upload file {Path.GetFileName(filePath)}: {ex.Message}");
            }
        }

        private async Task UploadFolderRecursive(PendingFolder folder)
        {
            try
            {
                await UploadFolder(folder);
                
                // Upload subfolders recursively
                foreach (var subFolder in folder.SubFolders)
                {
                    await UploadFolderRecursive(subFolder);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi upload folder {folder.FolderName}: {ex.Message}");
            }
        }

        private async Task UploadFolder(PendingFolder folder)
        {
            try
            {
                string folderName = folder.FolderName;
                int ownerId = Session.LoggedInUserId;
                
                int fileIndex = 0;
                foreach (var file in folder.Files)
                {
                    fileIndex++;
                    lblProgressStatus.Text = $"Đang upload file {fileIndex}/{folder.Files.Count} trong folder {folderName}: {Path.GetFileName(file.FilePath)}";
                    Application.DoEvents();
                    
                    // Encrypt file before uploading
                    byte[] encryptedData = CryptoHelper.EncryptFileFromDisk(file.FilePath, Session.UserPassword);
                    FileInfo fi = new FileInfo(file.FilePath);
                    string uploadAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    
                    // Calculate relative path from root folder
                    string rootFolderPath = GetRootFolderPath(folder.FolderPath);
                    string relativePath = GetRelativePathForUpload(file.FilePath, rootFolderPath);
                    
                    string command = $"UPLOAD_FILE_IN_FOLDER|{Path.GetFileName(rootFolderPath)}|{relativePath}|{fi.Name}|{encryptedData.Length}|{ownerId}|{uploadAt}";
                    byte[] commandBytes = Encoding.UTF8.GetBytes(command + "\n");
                    
                    var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync(serverIp, serverPort);
                    using (sslStream)
                    {
                        await sslStream.WriteAsync(commandBytes, 0, commandBytes.Length);
                        // Send encrypted file data
                        await sslStream.WriteAsync(encryptedData, 0, encryptedData.Length);
                        await sslStream.FlushAsync();
                        
                        using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                        {
                            string response = await reader.ReadLineAsync();
                            if (response.Trim() != "200")
                                throw new Exception($"Lỗi upload file {file.FilePath}: {response}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi upload folder {folder.FolderName}: {ex.Message}");
            }
        }

        private string GetRootFolderPath(string folderPath)
        {
            // Find the root folder for this path from pendingFolders
            var rootFolder = pendingFolders.FirstOrDefault(f => folderPath.StartsWith(f.FolderPath));
            return rootFolder?.FolderPath ?? folderPath;
        }

        private string GetRelativePathForUpload(string filePath, string rootFolderPath)
        {
            string relativePath = filePath.Substring(rootFolderPath.Length).TrimStart(Path.DirectorySeparatorChar);
            return Path.GetDirectoryName(relativePath) ?? "";
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                foreach (var filePath in dialog.FileNames)
                    ProcessLocalFile(filePath);
            }
        }

        private void btnBrowseFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Chọn folder để upload";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                ProcessLocalFolder(dialog.SelectedPath);
            }
        }

        private void ProcessLocalFile(string filePath)
        {
            const long MAX_TOTAL_SIZE = 10 * 1024 * 1024;
            const int MAX_FILENAME_LENGTH = 100; // Giới hạn tên file tối đa 100 ký tự
            
            if (!File.Exists(filePath)) return;

            string fileName = Path.GetFileName(filePath);
            
            // Kiểm tra độ dài tên file
            if (fileName.Length > MAX_FILENAME_LENGTH)
            {
                MessageBox.Show($"Tên file '{fileName}' quá dài (tối đa {MAX_FILENAME_LENGTH} ký tự). Vui lòng đổi tên file và thử lại.", 
                    "Tên file quá dài", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // Kiểm tra ký tự không hợp lệ trong tên file
            char[] invalidChars = Path.GetInvalidFileNameChars();
            if (fileName.IndexOfAny(invalidChars) >= 0)
            {
                MessageBox.Show($"Tên file '{fileName}' chứa ký tự không hợp lệ. Vui lòng đổi tên file và thử lại.", 
                    "Tên file không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // Check if file already exists in pending list
            if (pendingFiles.Any(f => f.FilePath == filePath))
            {
                MessageBox.Show($"File '{fileName}' đã có trong danh sách upload.", "File trùng lặp", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            string uploadAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string owner = Session.LoggedInUser;
            FileInfo fi = new FileInfo(filePath);
            long fileSizeBytes = fi.Length;
            
            if(totalSizeBytes + fileSizeBytes > MAX_TOTAL_SIZE)
            {
                MessageBox.Show($"Không thể thêm '{fileName}' vì tổng dung lượng vượt quá 10MB.");
                return;
            }
            totalSizeBytes += fileSizeBytes;

            pendingFiles.Add(new PendingFile { FilePath = filePath, RelativePath = "" });
            DisplayCurrentItems();
            UpdateFileSizeLabel();
        }

        private void ProcessLocalFolder(string folderPath)
        {
            const long MAX_TOTAL_SIZE = 50 * 1024 * 1024; // 50MB for folders
            const int MAX_FOLDERNAME_LENGTH = 100; // Giới hạn tên folder tối đa 100 ký tự
            
            if (!Directory.Exists(folderPath)) return;

            string folderName = Path.GetFileName(folderPath);
            
            // Kiểm tra độ dài tên folder
            if (folderName.Length > MAX_FOLDERNAME_LENGTH)
            {
                MessageBox.Show($"Tên folder '{folderName}' quá dài (tối đa {MAX_FOLDERNAME_LENGTH} ký tự). Vui lòng đổi tên folder và thử lại.", 
                    "Tên folder quá dài", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // Kiểm tra ký tự không hợp lệ trong tên folder
            char[] invalidChars = Path.GetInvalidFileNameChars();
            if (folderName.IndexOfAny(invalidChars) >= 0)
            {
                MessageBox.Show($"Tên folder '{folderName}' chứa ký tự không hợp lệ. Vui lòng đổi tên folder và thử lại.", 
                    "Tên folder không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check if folder already exists in pending list
            if (pendingFolders.Any(f => f.FolderPath == folderPath))
            {
                MessageBox.Show($"Folder '{folderName}' đã có trong danh sách upload.", "Folder trùng lặp", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // Build folder structure for navigation
                var pendingFolder = BuildFolderStructure(folderPath);
                
                if(totalSizeBytes + pendingFolder.TotalSize > MAX_TOTAL_SIZE)
                {
                    MessageBox.Show($"Không thể thêm folder '{folderName}' vì tổng dung lượng vượt quá 50MB.");
                    return;
                }
                
                totalSizeBytes += pendingFolder.TotalSize;
                pendingFolders.Add(pendingFolder);
                DisplayCurrentItems();
                UpdateFileSizeLabel();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private PendingFolder BuildFolderStructure(string folderPath)
        {
            const int MAX_FOLDERNAME_LENGTH = 100;
            
            string folderName = Path.GetFileName(folderPath);
            var pendingFolder = new PendingFolder
            {
                FolderPath = folderPath,
                FolderName = folderName,
                Files = new List<PendingFile>(),
                SubFolders = new List<PendingFolder>()
            };

            // Get files directly in this folder
            string[] files = Directory.GetFiles(folderPath, "*", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                string fileName = Path.GetFileName(file);
                
                // Kiểm tra độ dài tên file trong folder
                if (fileName.Length > MAX_FOLDERNAME_LENGTH)
                {
                    throw new Exception($"File '{fileName}' trong folder có tên quá dài (tối đa {MAX_FOLDERNAME_LENGTH} ký tự).");
                }
                
                pendingFolder.Files.Add(new PendingFile { FilePath = file, RelativePath = "" });
                FileInfo fi = new FileInfo(file);
                pendingFolder.TotalSize += fi.Length;
                pendingFolder.FileCount++;
            }

            // Get subfolders and build their structure recursively
            string[] subFolders = Directory.GetDirectories(folderPath);
            foreach (var subFolder in subFolders)
            {
                var subFolderStructure = BuildFolderStructure(subFolder);
                pendingFolder.SubFolders.Add(subFolderStructure);
                pendingFolder.TotalSize += subFolderStructure.TotalSize;
                pendingFolder.FileCount += subFolderStructure.FileCount;
            }

            return pendingFolder;
        }

        private void ClearFileList()
        {
            // Keep only the header (index 0)
            for (int i = UploadFilePanel.Controls.Count - 1; i >= 1; i--)
            {
                var control = UploadFilePanel.Controls[i];
                UploadFilePanel.Controls.RemoveAt(i);
                control.Dispose();
            }
        }
        
        private void UpdateFileSizeLabel()
        {
            TotalSizelbl.Text = $"Tổng kích thước: {FormatFileSize(totalSizeBytes)}";
        }
        
        private async void OnFileRemoved(string filePath)
        {
            var fileToRemove = pendingFiles.FirstOrDefault(pf => pf.FilePath == filePath);
            if(fileToRemove != null)
            {
                FileInfo fi = new FileInfo(filePath);
                totalSizeBytes -= fi.Length;

                pendingFiles.Remove(fileToRemove);
                DisplayCurrentItems();
                UpdateFileSizeLabel();
            }
        }
        
        private async void OnFolderRemoved(string folderPath)
        {
            var folderToRemove = pendingFolders.FirstOrDefault(pf => pf.FolderPath == folderPath);
            if(folderToRemove != null)
            {
                totalSizeBytes -= folderToRemove.TotalSize;

                pendingFolders.Remove(folderToRemove);
                
                // If we're currently inside the removed folder, go back to root
                if (currentFolderPath != null && currentFolderPath.StartsWith(folderPath))
                {
                    currentFolderPath = null;
                    navigationStack.Clear();
                }
                
                DisplayCurrentItems();
                UpdateFileSizeLabel();
            }
        }

        private void AddHeaderRow()
        {
            Panel headerPanel = new Panel();
            headerPanel.Height = 30;
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Width = UploadFilePanel.Width;
            headerPanel.BackColor = Color.LightGray;

            Font headerFont = new Font("Segoe UI", 9.75F, FontStyle.Bold);

            Label lblFileName = new Label()
            {
                Text = "Tên",
                Location = new Point(50, 5), // Khớp với lblFileName trong item (50, 3)
                Width = 180,
                Font = headerFont
            };

            Label lblOwner = new Label()
            {
                Text = "Chủ sở hữu",
                Location = new Point(240, 5), // Khớp với lblOwner trong item (240, 3)
                Width = 120,
                Font = headerFont
            };

            Label lblFileSize = new Label()
            {
                Text = "Kích thước",
                Location = new Point(370, 5), // Khớp với lblFileSize trong item (370, 3)
                Width = 100,
                Font = headerFont
            };

            Label lblFileType = new Label()
            {
                Text = "Loại",
                Location = new Point(480, 5), // Khớp với lblFileType trong item (480, 3)
                Width = 80,
                Font = headerFont
            };

            Label lblOption = new Label()
            {
                Text = "Thao tác",
                Location = new Point(570, 5), // Khớp với btnRemove trong item (570, 8)
                Width = 80,
                Font = headerFont
            };

            // Thêm các label vào header panel
            headerPanel.Controls.Add(lblFileName);
            headerPanel.Controls.Add(lblOwner);
            headerPanel.Controls.Add(lblFileSize);
            headerPanel.Controls.Add(lblFileType);
            headerPanel.Controls.Add(lblOption);

            // Thêm headerPanel vào đầu danh sách
            UploadFilePanel.Controls.Add(headerPanel);
            UploadFilePanel.Controls.SetChildIndex(headerPanel, 0); // Đảm bảo nó nằm trên đầu
        }
    }
}
