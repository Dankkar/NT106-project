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
    public partial class ShareView: UserControl
    {

        private bool isBusy = false;

        public ShareView()
        {
            InitializeComponent();
            tbPassword.Visible = false;
        }

        private async Task<int> GetUserIdFromSessionAsync()
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToSecureServerAsync("localhost", 5000);
                using (sslStream)
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                    {
                    // Gửi request GET_USER_ID
                    string message = $"GET_USER_ID|{Session.LoggedInUser}";
                    await writer.WriteLineAsync(message);

                    // Nhận response từ server
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
        public async Task Reload()
        {
            await LoadUserFilesAsync();
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
            catch(Exception ex)
            {
                MessageBox.Show($"Loi tai danh sach file: {ex.Message}", "Loi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<List<string>> GetUserFilesFromServer(int userId)
        {
            List<string> files = new List<string>();
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToSecureServerAsync("localhost", 5000);
                using (sslStream)
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    string message = $"GET_USER_FILES|{userId}";
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
                var (sslStream, _) = await SecureChannelHelper.ConnectToSecureServerAsync("localhost", 5000);
                using (sslStream)
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    string message = $"GET_SHARED_FILES|{userId}";
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



        

        private async void btnShare_Click(object sender, EventArgs e)
        {
            if (isBusy) return;
            isBusy = true;

            PasswordPanel.Visible = true;

            string selectedFile = cbBrowseFile.SelectedItem?.ToString();
            string sharePass = tbPassword.Text;

            if (string.IsNullOrEmpty(selectedFile))
            {
                MessageBox.Show("Vui lòng chọn file trước khi chia sẻ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                isBusy = false;
                return;
            }



            try
            {
                bool result = await UpdateFileShareStatusAsync(selectedFile, sharePass);
                if (result)
                {
                    MessageBox.Show("File đã được chia sẻ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    tbPassword.Visible = true;
                }
                else
                {
                    MessageBox.Show("Lỗi khi chia sẻ file!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                isBusy = false;
            }
        }

        private async Task<bool> UpdateFileShareStatusAsync(string fileName, string sharePass)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToSecureServerAsync("localhost", 5000);
                using (sslStream)
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                    {
                    string message = $"UPDATE_FILE_SHARE|{fileName}|{sharePass}";
                    await writer.WriteLineAsync(message);

                    string response = await reader.ReadLineAsync();
                    response = response?.Trim();

                    if (response != null)
                    {
                        string[] parts = response.Split('|');
                        return parts.Length >= 2 && parts[0] == "200" && parts[1] == "FILE_SHARED";
                    }
                    return false;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Loi khi cap nhat trang thai chia se: {ex.Message}", "Loi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private async void cbBrowseFile_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedFile = cbBrowseFile.SelectedItem.ToString();

            string sharePass = await GetSharePass(selectedFile);
            tbPassword.Text = sharePass;
        }

        private async Task<string> GetSharePass(string fileName)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToSecureServerAsync("localhost", 5000);
                using (sslStream)
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                    {
                    string message = $"GET_SHARE_PASS|{fileName}";
                    await writer.WriteLineAsync(message);

                    string response = await reader.ReadLineAsync();
                    response = response?.Trim();

                    if (response != null)
                        {
                        string[] parts = response.Split('|');
                        if (parts.Length >= 2 && parts[0] == "200")
                        {
                            return parts[1];
                        }
                    }
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lấy share_pass: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.Empty;
            }
        }

        private async void btnGet_Click(object sender, EventArgs e)
        {
            string sharePass = tbInputPassword.Text;

            // Kiem tra neu mat khau trong
            if (string.IsNullOrEmpty(sharePass))
            {
                MessageBox.Show("Vui long nhap mat khau chia se", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Lay thong tin file_id va owner_id tu share_pass
            (int fileId, int ownerId) = await GetFileInfoFromSharePassAsync(sharePass);

            if(fileId == -1)
            {
                MessageBox.Show("Mat khau chia se khong hop le", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Lay user_id cua nguoi dung hien tai
            int userId = await GetUserIdFromSessionAsync();

            // Kiem tra neu client da la owner cua file, khong them ban ghi vao db
            if(userId == ownerId)
            {
                MessageBox.Show("Ban la chu so huu cua file nay. Khong can chia se lai.");
                return;
            }


            // Them ban ghi vao "files_share" de chia se file voi nguoi dung
            bool result = await AddFileShareEntryAsync(fileId, userId, sharePass);
            if (result)
            {
                MessageBox.Show("Ban da co quyen truy cap vao file nay", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadUserFilesAsync();
            }
            else
            {
                MessageBox.Show("Loi khi chia se file 1", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Truy van thong tin file_od va owner_id tu mat khau chia se
        private async Task<(int, int)> GetFileInfoFromSharePassAsync(string sharePass)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToSecureServerAsync("localhost", 5000);
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
                MessageBox.Show($"Loi khi lay thong tin file: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return (-1, -1);
            }
        }

        // Them ban ghi vao "files_share" de chia se file voi nguoi dung
        private async Task<bool> AddFileShareEntryAsync(int fileId, int userId, string sharePass)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToSecureServerAsync("localhost", 5000);
                using (sslStream)
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                    {
                    string message = $"ADD_FILE_SHARE_ENTRY|{fileId}|{userId}|{sharePass}";
                    await writer.WriteLineAsync(message);

                    string response = await reader.ReadLineAsync();
                    response = response?.Trim();

                    if (response != null)
                    {
                        string[] parts = response.Split('|');
                        return parts.Length >= 2 && parts[0] == "200" && (parts[1] == "SHARE_ADDED" || parts[1] == "ALREADY_SHARED");
                }
                    return false;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Loi khi chia se file 2 {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}

