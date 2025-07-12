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
    public partial class FolderItemControl : UserControl
    {
        private string serverIp = ConfigurationManager.AppSettings["ServerIP"];
        private int serverPort = int.Parse(ConfigurationManager.AppSettings["ServerPort"]);
        private int chunkSize = int.Parse(ConfigurationManager.AppSettings["ChunkSize"]);
        private long maxFileSize = long.Parse(ConfigurationManager.AppSettings["MaxFileSizeMB"]) * 1024 * 1024;
        private string uploadsPath = ConfigurationManager.AppSettings["UploadsPath"];
        private string databasePath = ConfigurationManager.AppSettings["DatabasePath"];

        public int FolderId { get; set; }
        public string FolderName { get; set; }
        public string CreatedAt { get; set; }
        public string Owner { get; set; }
        public bool IsShared { get; set; }
        
        public event Action<int> FolderClicked;
        public event Action<int> FolderDeleted;
        public event Action<int> FolderShared;
        public event Action<string> FolderShareRequested;
        public event Action<int> FolderDownloadRequested;

        public FolderItemControl(string folderName, string createdAt, string owner, bool isShared, int folderId)
        {
            InitializeComponent();

            // Set thông tin folder vào control
            FolderId = folderId;
            FolderName = folderName;
            CreatedAt = createdAt;
            Owner = owner;
            IsShared = isShared;

            //Console.WriteLine($"[DEBUG][FolderItemControl] Setting owner: '{owner}' for folder: '{folderName}'");

            lblFolderName.Text = TruncateFolderName(folderName, 25); // Giới hạn tên folder 25 ký tự
            lblOwner.Text = owner;
            lblType.Text = "Folder";
            lblCreatedAt.Text = createdAt;

            // Ẩn các thông tin không cần thiết - chỉ hiển thị Tên folder, Người sở hữu, Type
            lblCreatedAt.Visible = false;

            // Thêm tooltip cho tên folder nếu bị cắt ngắn
            if (folderName.Length > 25)
            {
                ToolTip tooltip = new ToolTip();
                tooltip.SetToolTip(lblFolderName, folderName);
            }

            // Assign context menu to the control
            this.ContextMenuStrip = contextMenuStrip1;

            btnMore.Click += (s, e) => contextMenuStrip1.Show(btnMore, new Point(0, btnMore.Height));
            
            // Add click events to main controls for folder navigation
            this.Click += FolderItemControl_Click;
            lblFolderName.Click += FolderItemControl_Click;
            lblOwner.Click += FolderItemControl_Click;
            lblType.Click += FolderItemControl_Click;
            
            // Make the control look clickable
            this.Cursor = Cursors.Hand;
            lblFolderName.Cursor = Cursors.Hand;
            lblOwner.Cursor = Cursors.Hand;
            lblType.Cursor = Cursors.Hand;
            
            // Add hover effects
            this.MouseEnter += (s, e) => this.BackColor = Color.LightBlue;
            this.MouseLeave += (s, e) => this.BackColor = SystemColors.ButtonHighlight;
            lblFolderName.MouseEnter += (s, e) => this.BackColor = Color.LightBlue;
            lblFolderName.MouseLeave += (s, e) => this.BackColor = SystemColors.ButtonHighlight;
            lblOwner.MouseEnter += (s, e) => this.BackColor = Color.LightBlue;
            lblOwner.MouseLeave += (s, e) => this.BackColor = SystemColors.ButtonHighlight;
            lblType.MouseEnter += (s, e) => this.BackColor = Color.LightBlue;
            lblType.MouseLeave += (s, e) => this.BackColor = SystemColors.ButtonHighlight;

            // Double click để mở folder
            this.DoubleClick += FolderItemControl_DoubleClick;
            lblFolderName.DoubleClick += FolderItemControl_DoubleClick;
            lblOwner.DoubleClick += FolderItemControl_DoubleClick;
            lblType.DoubleClick += FolderItemControl_DoubleClick;
        }

        /// <summary>
        /// Cắt ngắn tên folder nếu quá dài và thêm dấu "..."
        /// </summary>
        /// <param name="folderName">Tên folder gốc</param>
        /// <param name="maxLength">Độ dài tối đa</param>
        /// <returns>Tên folder đã được cắt ngắn</returns>
        private string TruncateFolderName(string folderName, int maxLength)
        {
            if (string.IsNullOrEmpty(folderName) || folderName.Length <= maxLength)
                return folderName;

            return folderName.Substring(0, maxLength - 3) + "...";
        }

        private void FolderItemControl_Click(object sender, EventArgs e)
        {
            // Navigate vào folder khi click
            FolderClicked?.Invoke(FolderId);
        }

        private void FolderItemControl_DoubleClick(object sender, EventArgs e)
        {
            // Double click cũng navigate vào folder
            FolderClicked?.Invoke(FolderId);
        }

        private void shareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Query current share password from server
            try
            {
                // Trigger the share event to get current share password
                FolderShareRequested?.Invoke(FolderId.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting share password: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show($"Bạn có chắc muốn xóa folder {FolderName}?", "Xác nhận", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                FolderDeleted?.Invoke(FolderId);
                this.Dispose(); // Xóa FolderItemControl khỏi giao diện
            }
        }

        private void downloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Trigger the download event
            FolderDownloadRequested?.Invoke(FolderId);
        }

        private async Task DownloadFolderAsync(string targetPath)
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
                    string message = $"GET_FOLDER_CONTENTS|{FolderId}";
                    await writer.WriteLineAsync(message);

                    string response = await reader.ReadLineAsync();
                    response = response?.Trim();

                    if (response != null && response.StartsWith("200|"))
                    {
                        string data = response.Substring(4);
                        if (data != "NO_FILES_IN_FOLDER")
                        {
                            // Parse danh sách files
                            string[] fileStrings = data.Split(';');
                            foreach (string fileString in fileStrings)
                            {
                                if (string.IsNullOrEmpty(fileString)) continue;

                                string[] parts = fileString.Split(':');
                                if (parts.Length >= 7)
                                {
                                    int fileId = int.Parse(parts[0]);
                                    string fileName = parts[1];
                                    string relativePath = parts[6]; // file_path chứa đường dẫn tương đối

                                    // Tạo thư mục con nếu cần
                                    string fileDir = Path.GetDirectoryName(relativePath);
                                    if (!string.IsNullOrEmpty(fileDir))
                                    {
                                        string fullDir = Path.Combine(targetPath, fileDir);
                                        if (!Directory.Exists(fullDir))
                                        {
                                            Directory.CreateDirectory(fullDir);
                                        }
                                    }

                                    // Download file
                                    string filePath = Path.Combine(targetPath, relativePath);
                                    await DownloadFileAsync(fileId, filePath);
                                }
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
                                    //Console.WriteLine($"[DEBUG] FolderItemControl: Downloading shared file, using share_pass for decryption");
                                }
                                else
                                {
                                    // Owner file → decrypt with user password
                                    decryptionKey = Session.UserPassword;
                                    //Console.WriteLine($"[DEBUG] FolderItemControl: Downloading owner file, using user password for decryption");
                                }
                            }
                            else
                            {
                                // Legacy format → assume owner file
                                decryptionKey = Session.UserPassword;
                                //Console.WriteLine($"[DEBUG] FolderItemControl: Legacy download format, using user password for decryption");
                            }
                            
                            // Decrypt and save file
                            CryptoHelper.DecryptFileToLocal(encryptedData, decryptionKey, savePath);
                            //Console.WriteLine($"[DEBUG] FolderItemControl: Successfully downloaded and decrypted file to: {savePath}");
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
                Console.WriteLine($"[ERROR] FolderItemControl: Error in DownloadFileAsync: {ex.Message}");
                throw new Exception($"Lỗi khi tải file {Path.GetFileName(savePath)}: {ex.Message}");
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        /// <summary>
        /// Override context menu for shared folders (used in ShareView)
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