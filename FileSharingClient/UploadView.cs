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
using System.IO.Compression;
using System.Security.Cryptography;
using FileSharingClient;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace FileSharingClient
{
    public partial class UploadView: UserControl
    {
        public event Func<Task> FileUploaded;
        private class PendingFile
        {
            public string FilePath { get; set; }
            public string RelativePath { get; set; }
        }
        private List<PendingFile> pendingFiles = new List<PendingFile>();
        private string pendingFolder = null; // For folder upload
        private bool isUploadingFolder = false;
        private long totalSizeBytes = 0;
        private const int BUFFER_SIZE = 8192; // Match server buffer size

        public UploadView()
        {
            InitializeComponent();
            UploadFilePanel.FlowDirection = FlowDirection.LeftToRight;
            UploadFilePanel.AutoScroll = true;
            AddHeaderRow();
        }



        private string FormatFileSize(long bytes)
        {
            if (bytes >= 1024 * 1024) return $"{bytes / (1024 * 1024.0):0.##} MB";
            if(bytes >= 1024) return $"{bytes / 1024.0:0.##} KB";
            return $"{bytes} B";
        }

        public void AddFileToView(string fileName, string createAt, string owner, string filesize, string filePath)
        {
            var fileItem = new FileItemControl(fileName, createAt, owner, filesize, filePath);
            fileItem.FileDeleted += OnFileDeleted;
            UploadFilePanel.Controls.Add(fileItem);

        }

        private void DragPanel_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var file in files)
                ProcessLocalFile(file);
        }

        private void DragPanel_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private async void btnUpload_Click(object sender, EventArgs e)
        {
            if (isUploadingFolder && pendingFiles.Count > 0)
            {
                string folderName = Path.GetFileName(pendingFolder);
                int ownerId = Session.LoggedInUserId;
                foreach (var pf in pendingFiles)
                {
                    // Encrypt file before uploading
                    byte[] encryptedData = CryptoHelper.EncryptFileFromDisk(pf.FilePath, Session.UserPassword);
                    FileInfo fi = new FileInfo(pf.FilePath);
                    string uploadAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    string command = $"UPLOAD_FILE_IN_FOLDER|{folderName}|{pf.RelativePath}|{fi.Name}|{encryptedData.Length}|{ownerId}|{uploadAt}";
                    byte[] commandBytes = Encoding.UTF8.GetBytes(command + "\n");
                    var (sslStream, _) = await SecureChannelHelper.ConnectToSecureServerAsync("localhost", 5000);
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
                                MessageBox.Show($"Lỗi upload file {pf.FilePath}: {response}");
                        }
                    }
                }
                MessageBox.Show("Upload folder thành công!");
                pendingFiles.Clear();
                totalSizeBytes = 0;
                pendingFolder = null;
                isUploadingFolder = false;
                ClearFileList();
                UpdateFileSizeLabel();
                if (FileUploaded != null)
                    await FileUploaded.Invoke();
            }
            else
            {
                await UploadFiles(); // Xử lý upload file lẻ như cũ
            }
        }

        private async Task UploadFiles()
        {
            var filesToUpload = new List<string>(pendingFiles.Select(pf => pf.FilePath));
            // Neu co nhieu file -> nen lai
            if(filesToUpload.Count > 1)
            {
                string zipFilePath = CompressFiles(filesToUpload);
                filesToUpload = new List<string> { zipFilePath };
            }
            foreach(var filePath in filesToUpload)
            {
                try
                {
                    var (sslStream, _) = await SecureChannelHelper.ConnectToSecureServerAsync("localhost", 5000);
                    using (sslStream)
                    {
                        // Encrypt file before uploading
                        byte[] encryptedData = CryptoHelper.EncryptFileFromDisk(filePath, Session.UserPassword);
                        string fileName = Path.GetFileName(filePath);
                        int ownerId = Session.LoggedInUserId;
                        string uploadAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        string command = $"UPLOAD|{fileName}|{encryptedData.Length}|{ownerId}|{uploadAt}";
                        byte[] commandBytes = Encoding.UTF8.GetBytes(command + "\n");
                        await sslStream.WriteAsync(commandBytes, 0, commandBytes.Length);
                        await sslStream.FlushAsync();
                        Console.WriteLine($"Đã gửi lệnh: {command.Trim()}");
                        // Send encrypted file data
                        await sslStream.WriteAsync(encryptedData, 0, encryptedData.Length);
                        await sslStream.FlushAsync();
                        using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                        {
                            string response = await reader.ReadLineAsync();
                            Console.WriteLine($"Server trả về: {response}");
                            if (response.Trim() == "413")
                                MessageBox.Show("File quá lớn. Vui lòng thử lại với file nhỏ hơn.");
                            else if (response.Trim() == "200")
                            {
                                MessageBox.Show("Tải lên thành công");
                                if (FileUploaded != null)
                                    await FileUploaded.Invoke();
                            }
                            else
                                MessageBox.Show($"Lỗi: {response.Trim()}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi upload file {filePath}: {ex.Message}");
                }
            }
            pendingFiles.Clear();
            totalSizeBytes = 0;
            ClearFileList();
            UpdateFileSizeLabel();
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
            if (!File.Exists(filePath)) return;

            string fileName = Path.GetFileName(filePath);
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

            string fileSize = FormatFileSize(fileSizeBytes);
            AddFileToView(fileName, uploadAt, owner, fileSize, filePath);
            pendingFiles.Add(new PendingFile { FilePath = filePath, RelativePath = "" });
            UpdateFileSizeLabel();
        }

        private void ProcessLocalFolder(string folderPath)
        {
            const long MAX_TOTAL_SIZE = 50 * 1024 * 1024; // 50MB for folders
            if (!Directory.Exists(folderPath)) return;

            pendingFiles.Clear();
            totalSizeBytes = 0;
            ClearFileList();

            string folderName = Path.GetFileName(folderPath);
            string[] files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                string relativePath = Path.GetDirectoryName(file.Substring(folderPath.Length).TrimStart(Path.DirectorySeparatorChar)) ?? "";
                pendingFiles.Add(new PendingFile { FilePath = file, RelativePath = relativePath });
                FileInfo fi = new FileInfo(file);
                totalSizeBytes += fi.Length;
                string fileSize = FormatFileSize(fi.Length);
                AddFileToView(fi.Name, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Session.LoggedInUser, fileSize, file);
            }
            pendingFolder = folderPath;
            isUploadingFolder = true;
            UpdateFileSizeLabel();
        }

        private void ClearFileList()
        {
            for (int i = UploadFilePanel.Controls.Count - 1; i >= 1; i--)
            {
                var control = UploadFilePanel.Controls[i];
                control.Dispose();
            }
        }
        private void UpdateFileSizeLabel()
        {
            TotalSizelbl.Text = $"Tổng kích thước: {FormatFileSize(totalSizeBytes)}";
        }
        private void OnFileDeleted(string filePath)
        {
            if(pendingFiles.Any(pf => pf.FilePath == filePath))
            {
                FileInfo fi = new FileInfo(filePath);
                totalSizeBytes -= fi.Length;

                pendingFiles.RemoveAll(pf => pf.FilePath == filePath);
                UpdateFileSizeLabel();
            }
        }

        private string CompressFiles(List<string> filesToCompress)
        {
            string zipFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".zip");
            using (var zip = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
            {
                foreach (var file in filesToCompress)
                {
                    zip.CreateEntryFromFile(file, Path.GetFileName(file));
                }
            }
            return zipFilePath;
        }

        private string CompressFolderToZip(string folderPath, string folderName)
        {
            string zipFilePath = Path.Combine(Path.GetTempPath(), $"{folderName}_{Guid.NewGuid()}.zip");
            
            try
            {
                using (var zip = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                {
                    string[] files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
                    
                    foreach (string file in files)
                    {
                        string relativePath = file.Substring(folderPath.Length + 1);
                        zip.CreateEntryFromFile(file, relativePath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi nén folder: {ex.Message}");
                throw;
            }
            
            return zipFilePath;
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
                Text = "Tên file",
                Location = new Point(39, 5),
                Width = 180,
                Font = headerFont
            };

            Label lblOwner = new Label()
            {
                Text = "Chủ sở hữu",
                Location = new Point(248, 5),
                Width = 140,
                Font = headerFont
            };

            Label lblCreateAt = new Label()
            {
                Text = "Ngày upload",
                Location = new Point(420, 5),
                Width = 120,
                Font = headerFont
            };

            Label lblFileSize = new Label()
            {
                Text = "Dung lượng",
                Location = new Point(565, 5),
                Width = 130,
                Font = headerFont
            };

            Label lblFilePath = new Label()
            {
                Text = "Đường dẫn",
                Location = new Point(733, 5),
                Width = 400,
                Font = headerFont,
                AutoEllipsis = true
            };

            Label lblOption = new Label()
            {
                Text = "Tuỳ chọn",
                Location = new Point(1253, 5), // Khớp với btnMore
                Width = 80,
                Font = headerFont
            };

            // Thêm các label vào header panel
            headerPanel.Controls.Add(lblFileName);
            headerPanel.Controls.Add(lblOwner);
            headerPanel.Controls.Add(lblCreateAt);
            headerPanel.Controls.Add(lblFileSize);
            headerPanel.Controls.Add(lblFilePath);
            headerPanel.Controls.Add(lblOption);

            // Thêm headerPanel vào đầu danh sách
            UploadFilePanel.Controls.Add(headerPanel);
            UploadFilePanel.Controls.SetChildIndex(headerPanel, 0); // Đảm bảo nó nằm trên đầu
        }
    }
}
