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
using System.Security.Cryptography;

namespace FileSharingClient
{
    public partial class FileItemControl : UserControl
    {
        public string FilePath { get; set; }
        public event Action<String> FileDeleted;
        public event Action<String> FilePreviewRequested;
        public event Action<String> FileDownloadRequested;
        public event Action<String> FileShareRequested;
        
        public string FileName { get; set; }
        public string CreateAt { get; set; }
        public string Owner { get; set; }
        public string FileSize { get; set; }
        public int FileId { get; set; }
        
        private static string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName;
        private string serverIp = ConfigurationManager.AppSettings["ServerIP"];
        private int serverPort = int.Parse(ConfigurationManager.AppSettings["ServerPort"]);
        private int chunkSize = int.Parse(ConfigurationManager.AppSettings["ChunkSize"]);
        private long maxFileSize = long.Parse(ConfigurationManager.AppSettings["MaxFileSizeMB"]) * 1024 * 1024;
        private string uploadsPath = ConfigurationManager.AppSettings["UploadsPath"];
        private string databasePath = ConfigurationManager.AppSettings["DatabasePath"];

        public FileItemControl(string filename, string createAt, string owner, string filesize, string filepath, int fileId = -1)
        {
            InitializeComponent();

            //Set thong tin file vao control
            FileName = filename;
            lblFileName.Text = TruncateFileName(filename, 25); // Giới hạn tên file 25 ký tự
            lblFileSize.Text = filesize;
            lblOwner.Text = owner;
            lblCreateAt.Text = createAt;
            FilePath = filepath;
            lblFilePath.Text = filepath;
            CreateAt = createAt;
            Owner = owner;
            FileSize = filesize;
            FileId = fileId;

            Console.WriteLine($"[DEBUG][FileItemControl] Setting owner: '{owner}' for file: '{filename}'");

            // Set file type
            string fileExtension = Path.GetExtension(filename).ToLower();
            string fileType = GetFileType(fileExtension);
            lblFileType.Text = fileType;

            // Set file icon based on type
            lblFileIcon.Text = GetFileIcon(fileExtension);

            // Ẩn các thông tin không cần thiết - chỉ hiển thị Tên file, Người sở hữu, Kích thước, Type
            lblCreateAt.Visible = false;
            lblFilePath.Visible = false;

            // Thêm tooltip cho tên file nếu bị cắt ngắn
            if (filename.Length > 25)
            {
                ToolTip tooltip = new ToolTip();
                tooltip.SetToolTip(lblFileName, filename);
            }

            // Assign context menu to the control
            this.ContextMenuStrip = contextMenuStrip1;

            btnMore.Click += (s, e) => contextMenuStrip1.Show(btnMore, new Point(0, btnMore.Height));
            
            // Add click events to main controls for preview
            this.Click += FileItemControl_Click;
            lblFileName.Click += FileItemControl_Click;
            lblOwner.Click += FileItemControl_Click;
            lblFileSize.Click += FileItemControl_Click;
            
            // Make the control look clickable
            this.Cursor = Cursors.Hand;
            lblFileName.Cursor = Cursors.Hand;
            lblOwner.Cursor = Cursors.Hand;
            lblFileSize.Cursor = Cursors.Hand;
            
            // Add hover effects
            this.MouseEnter += (s, e) => this.BackColor = Color.LightBlue;
            this.MouseLeave += (s, e) => this.BackColor = Color.WhiteSmoke;
            lblFileName.MouseEnter += (s, e) => this.BackColor = Color.LightBlue;
            lblFileName.MouseLeave += (s, e) => this.BackColor = Color.WhiteSmoke;
            lblOwner.MouseEnter += (s, e) => this.BackColor = Color.LightBlue;
            lblOwner.MouseLeave += (s, e) => this.BackColor = Color.WhiteSmoke;
            lblFileSize.MouseEnter += (s, e) => this.BackColor = Color.LightBlue;
            lblFileSize.MouseLeave += (s, e) => this.BackColor = Color.WhiteSmoke;

            // Double click để preview
            this.DoubleClick += FileItemControl_DoubleClick;
            lblFileName.DoubleClick += FileItemControl_DoubleClick;
            lblOwner.DoubleClick += FileItemControl_DoubleClick;
            lblFileSize.DoubleClick += FileItemControl_DoubleClick;
        }

        /// <summary>
        /// Cắt ngắn tên file nếu quá dài và thêm dấu "..."
        /// </summary>
        /// <param name="fileName">Tên file gốc</param>
        /// <param name="maxLength">Độ dài tối đa</param>
        /// <returns>Tên file đã được cắt ngắn</returns>
        private string TruncateFileName(string fileName, int maxLength)
        {
            if (string.IsNullOrEmpty(fileName) || fileName.Length <= maxLength)
                return fileName;

            string extension = Path.GetExtension(fileName);
            string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            
            // Nếu extension quá dài, cắt cả extension
            if (extension.Length > maxLength / 2)
            {
                return fileName.Substring(0, maxLength - 3) + "...";
            }
            
            // Cắt phần tên file, giữ lại extension
            int availableLength = maxLength - extension.Length - 3; // 3 cho "..."
            if (availableLength <= 0)
            {
                return "..." + extension;
            }
            
            return nameWithoutExtension.Substring(0, availableLength) + "..." + extension;
        }

        private void FileItemControl_Click(object sender, EventArgs e)
        {
            // Preview file on single click
            PreviewFile();
        }

        private async void PreviewFile()
        {
            try
            {
                string extension = Path.GetExtension(FileName).ToLower();
                if (FileId > 0)
                {
                    var (fileBytes, error, encryptionType, sharePass) = await Services.ApiService.DownloadFileForPreviewAsync(FileId);
                    if (error == "FILE_TOO_LARGE")
                    {
                        MessageBox.Show("File quá lớn, vui lòng tải về để xem.", "File quá lớn", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                        return;
                    }
                    if (fileBytes == null)
                    {
                        MessageBox.Show(error ?? "Không thể tải file từ server.", "Lỗi", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        return;
                    }

                    // CLIENT-SIDE RE-ENCRYPTION: Determine decryption key based on file type
                    string decryptionKey;
                    if (encryptionType == "SHARED" && !string.IsNullOrEmpty(sharePass))
                    {
                        // Shared file → decrypt with share_pass
                        decryptionKey = sharePass;
                        Console.WriteLine($"[DEBUG] FileItemControl: Previewing shared file, using share_pass for decryption");
                    }
                    else
                    {
                        // Owner file → decrypt with user password
                        if (string.IsNullOrEmpty(Session.UserPassword))
                        {
                            MessageBox.Show("Không thể preview file đã mã hóa: Mật khẩu người dùng không khả dụng.\nVui lòng đăng nhập lại để preview file đã mã hóa.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        decryptionKey = Session.UserPassword;
                        Console.WriteLine($"[DEBUG] FileItemControl: Previewing owner file, using user password for decryption");
                    }

                    // Decrypt the file data before displaying
                    byte[] decryptedData = null;
                    try
                    {
                        decryptedData = CryptoHelper.DecryptFile(fileBytes, decryptionKey);
                        Console.WriteLine($"[DEBUG] FileItemControl: Successfully decrypted file for preview");
                    }
                    catch (Exception decryptEx)
                    {
                        // If decryption fails, show detailed error
                        MessageBox.Show($"Giải mã thất bại: {decryptEx.Message}\n\nEncryption Type: {encryptionType}\nFile này có thể chưa được mã hóa hoặc key không đúng.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Console.WriteLine($"[ERROR] FileItemControl: Decryption failed - {decryptEx.Message}");
                        return;
                    }

                    // Display the decrypted file data
                    if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".gif" || extension == ".bmp")
                    {
                        using (var ms = new MemoryStream(decryptedData))
                        {
                            var img = Image.FromStream(ms);
                            Form frm = new Form { Width = img.Width + 40, Height = img.Height + 60, Text = FileName };
                            var pb = new PictureBox { Image = img, Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom };
                            frm.Controls.Add(pb);
                            frm.ShowDialog();
                        }
                    }
                    else if (extension == ".txt" || extension == ".md" || extension == ".log")
                    {
                        string text = Encoding.UTF8.GetString(decryptedData);
                        Form frm = new Form { Width = 800, Height = 600, Text = FileName };
                        var rtb = new RichTextBox { Text = text, Dock = DockStyle.Fill, ReadOnly = true, Font = new Font("Consolas", 11) };
                        frm.Controls.Add(rtb);
                        frm.ShowDialog();
                    }
                    else
                    {
                        // Các loại khác: luu file tạm rồi mở bằng app mặc định
                        string tempPath = Path.Combine(Path.GetTempPath(), FileName);
                        File.WriteAllBytes(tempPath, decryptedData);
                        System.Diagnostics.Process.Start(tempPath);
                    }
                }
                else
                {
                    MessageBox.Show("Không xác định được fileId để preview!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error previewing file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void shareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Query current share password from server
            try
            {
                // Trigger the share event to get current share password
                FileShareRequested?.Invoke(FileId.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting share password: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void downloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = FileName;
                saveFileDialog.Title = "Save file as...";
                saveFileDialog.Filter = "All Files (*.*)|*.*";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        await DownloadEncryptedFile(FileName, saveFileDialog.FileName);
                        MessageBox.Show("File downloaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error downloading file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error downloading file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task DownloadEncryptedFile(string fileName, string savePath)
        {
            var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync(serverIp, serverPort);
            using (sslStream)
            using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
            using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
            {
                // Send download request with FileId instead of fileName
                string message = $"DOWNLOAD_FILE|{FileId}|{Session.LoggedInUserId}";
                await writer.WriteLineAsync(message);

                // Read response header
                string response = await reader.ReadLineAsync();
                response = response?.Trim();

                if (response != null)
                {
                    string[] parts = response.Split('|');
                    if (parts.Length >= 2 && parts[0] == "200")
                    {
                        // Parse base64 data directly from response (same as ApiService)
                        byte[] encryptedData = Convert.FromBase64String(parts[1]);
                        
                        // CLIENT-SIDE RE-ENCRYPTION: Determine decryption key based on file type
                        string decryptionKey;
                        if (parts.Length >= 4)
                        {
                            // New format with encryption type
                            string encryptionType = parts[2];
                            string sharePass = parts[3];
                            
                            if (encryptionType == "SHARED" && !string.IsNullOrEmpty(sharePass))
                            {
                                // Shared file → decrypt with share_pass
                                decryptionKey = sharePass;
                                Console.WriteLine($"[DEBUG] FileItemControl: Downloading shared file, using share_pass for decryption");
                            }
                            else
                            {
                                // Owner file → decrypt with user password
                                decryptionKey = Session.UserPassword;
                                Console.WriteLine($"[DEBUG] FileItemControl: Downloading owner file, using user password for decryption");
                            }
                        }
                        else
                        {
                            // Legacy format → assume owner file
                            decryptionKey = Session.UserPassword;
                            Console.WriteLine($"[DEBUG] FileItemControl: Legacy download format, using user password for decryption");
                        }
                        
                        // Decrypt and save file
                        CryptoHelper.DecryptFileToLocal(encryptedData, decryptionKey, savePath);
                        Console.WriteLine($"[DEBUG] FileItemControl: Successfully downloaded and decrypted file to: {savePath}");
                    }
                    else if (parts[0] == "404")
                    {
                        throw new Exception("File not found on server or no access");
                    }
                    else if (parts[0] == "413")
                    {
                        throw new Exception("File too large for download");
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

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show($"Bạn có chắc muốn xóa file {FileName}?", "Xác nhận", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                FileDeleted?.Invoke(FilePath);
                this.Dispose(); // Xa FileItemControl kh?i giao di?n
            }
        }
       
        private string GetFileType(string extension)
        {
            switch (extension)
            {
                case ".txt":
                case ".md":
                case ".log":
                    return "Text";
                case ".pdf":
                    return "PDF";
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".gif":
                case ".bmp":
                    return "Image";
                case ".mp4":
                case ".avi":
                case ".mov":
                case ".wmv":
                case ".mkv":
                    return "Video";
                case ".mp3":
                case ".wav":
                case ".flac":
                    return "Audio";
                case ".docx":
                case ".doc":
                    return "Word";
                case ".xlsx":
                case ".xls":
                    return "Excel";
                case ".pptx":
                case ".ppt":
                    return "PowerPoint";
                case ".zip":
                case ".rar":
                case ".7z":
                    return "Archive";
                default:
                    return "File";
            }
        }

        private string GetFileIcon(string extension)
        {
            switch (extension)
            {
                case ".txt":
                case ".md":
                case ".log":
                    return "📄";
                case ".pdf":
                    return "📕";
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".gif":
                case ".bmp":
                    return "🖼️";
                case ".mp4":
                case ".avi":
                case ".mov":
                case ".wmv":
                case ".mkv":
                    return "🎬";
                case ".mp3":
                case ".wav":
                case ".flac":
                    return "🎵";
                case ".docx":
                case ".doc":
                    return "📝";
                case ".xlsx":
                case ".xls":
                    return "📊";
                case ".pptx":
                case ".ppt":
                    return "📋";
                case ".zip":
                case ".rar":
                case ".7z":
                    return "📦";
                default:
                    return "📄";
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void FileItemControl_DoubleClick(object sender, EventArgs e)
        {
            // Double click also previews file
            PreviewFile();
        }

        public async Task DownloadFileAsync(string savePath)
        {
            try
            {
                await DownloadEncryptedFile(FileName, savePath);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tải file {FileName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Override context menu for shared files (used in ShareView)
        /// </summary>
        /// <param name="customContextMenu">Custom context menu to use</param>
        public void OverrideContextMenu(ContextMenuStrip customContextMenu)
        {
            this.ContextMenuStrip = customContextMenu;
            // Override btnMore to show custom context menu
            btnMore.Click -= (s, e) => contextMenuStrip1.Show(btnMore, new Point(0, btnMore.Height));
            btnMore.Click += (s, e) => customContextMenu.Show(btnMore, new Point(0, btnMore.Height));
        }
    }
}
