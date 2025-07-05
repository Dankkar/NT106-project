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

namespace FileSharingClient
{
    public partial class FilePreview : UserControl
    {
        private static string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName;
        private int currentUserId = -1;

        public FilePreview()
        {
            InitializeComponent();
            _ = InitAsync();
        }
        public async Task Reload()
        {
            await LoadUserFilesAsync();
        }
        private async Task InitAsync()
        {
            currentUserId = await GetUserIdFromSessionAsync();
            if (currentUserId != -1)
            {
                await LoadUserFilesAsync();
            }
        }
        public async Task LoadUserFilesAsync()
        {
            int userId = await GetUserIdFromSessionAsync();
            if (userId != -1)
            {
                await LoadUploadedFilesAsync(userId);  // Load files for the logged-in user
            }
            else
            {
                MessageBox.Show("User information not found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async Task LoadUploadedFilesAsync(int userID)
        {
            try
            {
                cbBrowseFile.Items.Clear();

                // Get user files from server
                List<string> userFiles = await GetUserFilesFromServer(userID);
                foreach (string fileName in userFiles)
                {
                    cbBrowseFile.Items.Add(fileName);
                }

                // Get shared files from server
                List<string> sharedFiles = await GetSharedFilesFromServer(userID);
                foreach (string fileName in sharedFiles)
                {
                    cbBrowseFile.Items.Add(fileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi tai danh sach file: {ex.Message}", "Loi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<List<string>> GetUserFilesFromServer(int userId)
        {
            List<string> files = new List<string>();
            try
            {
                using (TcpClient client = new TcpClient("127.0.0.1", 5000))
                using (NetworkStream stream = client.GetStream())
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
                {
                    string message = $"GET_USER_FILES|{userId}\n";
                    await writer.WriteLineAsync(message);

                    string response = await reader.ReadLineAsync();
                    response = response?.Trim();

                    if (response != null)
                    {
                        string[] parts = response.Split('|');
                        if (parts.Length >= 2 && parts[0] == "200")
                        {
                            if (parts[1] != "NO_FILES")
                            {
                                files.AddRange(parts[1].Split(';'));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting user files: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return files;
        }

        private async Task<List<string>> GetSharedFilesFromServer(int userId)
        {
            List<string> files = new List<string>();
            try
            {
                using (TcpClient client = new TcpClient("127.0.0.1", 5000))
                using (NetworkStream stream = client.GetStream())
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
                {
                    string message = $"GET_SHARED_FILES|{userId}\n";
                    await writer.WriteLineAsync(message);

                    string response = await reader.ReadLineAsync();
                    response = response?.Trim();

                    if (response != null)
                    {
                        string[] parts = response.Split('|');
                        if (parts.Length >= 2 && parts[0] == "200")
                        {
                            if (parts[1] != "NO_SHARED_FILES")
                            {
                                files.AddRange(parts[1].Split(';'));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting shared files: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return files;
        }
        private async Task<int> GetUserIdFromSessionAsync()
        {
            try
            {
                using (TcpClient client = new TcpClient("127.0.0.1", 5000))
                using (NetworkStream stream = client.GetStream())
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
                {
                    string message = $"GET_USER_ID|{Session.LoggedInUser}\n";
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
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting user_id: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }


        private async void btnPreview_Click(object sender, EventArgs e)
        {
            if (cbBrowseFile.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một file.");
                return;
            }

            string filename = cbBrowseFile.SelectedItem.ToString();

            try
            {
                using (TcpClient client = new TcpClient("127.0.0.1", 5000))
                using (NetworkStream stream = client.GetStream())
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
                {
                    string message = $"GET_FILE_INFO|{filename}|{currentUserId}\n";
                    await writer.WriteLineAsync(message);

                    string response = await reader.ReadLineAsync();
                    response = response?.Trim();

                    if (response != null)
                    {
                        string[] parts = response.Split('|');
                        if (parts.Length >= 3 && parts[0] == "200")
                        {
                            string relativePath = parts[1];
                            string fileType = parts[2].ToLower();
                            string fullPath = Path.Combine(projectRoot, relativePath);

                            await ShowPreviewAsync(fullPath, fileType);
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy đường dẫn file.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Không nhận được phản hồi từ server.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xem trước file: {ex.Message}");
            }
        }

        private async Task ShowPreviewAsync(string filePath, string fileType)
        {
            rtbContent.Visible = false;
            picImagePreview.Visible = false;
            wbPdfPreview.Visible = false;

            if (!File.Exists(filePath)) {
                MessageBox.Show("File khong ton tai");
                return;
            }

            if (fileType.Contains("text") || filePath.EndsWith(".txt"))
            {
                string content = await Task.Run(() => File.ReadAllText(filePath));
                rtbContent.Text = content;
                rtbContent.Visible = true;
            }

            else if (fileType.Contains("image") || filePath.EndsWith(".png") || filePath.EndsWith(".jpg") || filePath.EndsWith(".jpeg"))
            {
                picImagePreview.Image = Image.FromFile(filePath);
                picImagePreview.SizeMode = PictureBoxSizeMode.Zoom;
                picImagePreview.Visible = true;
            }

            else if(fileType.Contains("pdf") || filePath.EndsWith(".pdf"))
            {
                wbPdfPreview.Navigate(filePath);
                wbPdfPreview.Visible = true;
            }

            else
            {
                MessageBox.Show("Dinh dang file chua duoc ho tro");
            }
        }
    }
}

