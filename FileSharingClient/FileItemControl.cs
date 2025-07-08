using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        public FileItemControl(string filename, string createAt, string owner, string filesize, string filepath, int fileId = -1)
        {
            InitializeComponent();

            //Set thong tin file vao control
            FileName = filename;
            lblFileName.Text = filename;
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

            // ?n các thông tin không c?n thi?t - ch? hi?n th? Tên file, Ngu?i s? h?u, Kích thu?c, Type
            lblCreateAt.Visible = false;
            lblFilePath.Visible = false;

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
            this.MouseEnter += (s, e) => this.BackColor = Color.LightGray;
            this.MouseLeave += (s, e) => this.BackColor = SystemColors.ButtonHighlight;
            lblFileName.MouseEnter += (s, e) => this.BackColor = Color.LightGray;
            lblFileName.MouseLeave += (s, e) => this.BackColor = SystemColors.ButtonHighlight;
            lblOwner.MouseEnter += (s, e) => this.BackColor = Color.LightGray;
            lblOwner.MouseLeave += (s, e) => this.BackColor = SystemColors.ButtonHighlight;
            lblFileSize.MouseEnter += (s, e) => this.BackColor = Color.LightGray;
            lblFileSize.MouseLeave += (s, e) => this.BackColor = SystemColors.ButtonHighlight;
        }

        private void FileItemControl_Click(object sender, EventArgs e)
        {
            // When clicked, automatically preview the file
            PreviewFile();
        }

        private async void PreviewFile()
        {
            try
            {
                string extension = Path.GetExtension(FileName).ToLower();
                if (FileId > 0)
                {
                    var (fileBytes, error) = await Services.ApiService.DownloadFileForPreviewAsync(FileId);
                    if (error == "FILE_TOO_LARGE")
                    {
                        MessageBox.Show("File quá l?n, vui lòng t?i v? d? xem.", "File quá l?n", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                        return;
                    }
                    if (fileBytes == null)
                    {
                        MessageBox.Show(error ?? "Không th? t?i file t? server.", "L?i", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        return;
                    }
                    if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".gif" || extension == ".bmp")
                    {
                        using (var ms = new MemoryStream(fileBytes))
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
                        string text = Encoding.UTF8.GetString(fileBytes);
                        Form frm = new Form { Width = 800, Height = 600, Text = FileName };
                        var rtb = new RichTextBox { Text = text, Dock = DockStyle.Fill, ReadOnly = true, Font = new Font("Consolas", 11) };
                        frm.Controls.Add(rtb);
                        frm.ShowDialog();
                    }
                    else
                    {
                        // Các lo?i khác: luu file t?m r?i m? b?ng app m?c d?nh
                        string tempPath = Path.Combine(Path.GetTempPath(), FileName);
                        File.WriteAllBytes(tempPath, fileBytes);
                        System.Diagnostics.Process.Start(tempPath);
                    }
                }
                else
                {
                    MessageBox.Show("Không xác d?nh du?c fileId d? preview!", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync("localhost", 5000);
            using (sslStream)
            using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
            using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
            {
                // Send download request
                string message = $"DOWNLOAD_FILE|{fileName}|{Session.LoggedInUserId}";
                await writer.WriteLineAsync(message);

                // Read response header
                string response = await reader.ReadLineAsync();
                response = response?.Trim();

                if (response != null)
                {
                    string[] parts = response.Split('|');
                    if (parts.Length >= 2 && parts[0] == "200")
                    {
                        long fileSize = long.Parse(parts[1]);
                        
                        // Read encrypted file data
                        byte[] encryptedData = new byte[fileSize];
                        int totalRead = 0;
                        byte[] buffer = new byte[8192];
                        
                        while (totalRead < fileSize)
                        {
                            int bytesRead = await sslStream.ReadAsync(buffer, 0, Math.Min(buffer.Length, (int)(fileSize - totalRead)));
                            if (bytesRead == 0) break;
                            
                            Array.Copy(buffer, 0, encryptedData, totalRead, bytesRead);
                            totalRead += bytesRead;
                        }
                        
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

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show($"B?n có ch?c mu?n xóa file {FileName}?", "Xác nh?n", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                FileDeleted?.Invoke(FilePath);
                this.Dispose(); // Xóa FileItemControl kh?i giao di?n
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
                    return "??";
                case ".pdf":
                    return "??";
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".gif":
                case ".bmp":
                    return "???";
                case ".mp4":
                case ".avi":
                case ".mov":
                case ".wmv":
                case ".mkv":
                    return "??";
                case ".mp3":
                case ".wav":
                case ".flac":
                    return "??";
                case ".docx":
                case ".doc":
                    return "??";
                case ".xlsx":
                case ".xls":
                    return "??";
                case ".pptx":
                case ".ppt":
                    return "??";
                case ".zip":
                case ".rar":
                case ".7z":
                    return "??";
                default:
                    return "??";
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }
    }
}
