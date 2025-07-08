using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;
using FileSharingClient;

namespace FileSharingClient
{
    public partial class Account : Form
    {
        private const long TOTAL_STORAGE_BYTES = 1024 * 1024 * 1024; // 1GB in bytes

        public Account()
        {
            InitializeComponent();
            LoadAccountInfo();
        }

        private async void LoadAccountInfo()
        {
            try
            {
                // Get storage info from server
                long usedStorageBytes = await GetUserStorageFromServer();
                
                // Format and display
                string usedStorage = FormatFileSize(usedStorageBytes);
                string totalStorage = FormatFileSize(TOTAL_STORAGE_BYTES);
                
                // Calculate percentage for progress bar
                double usagePercentage = (double)usedStorageBytes / TOTAL_STORAGE_BYTES * 100;
                
                SetAccountInfo(Session.LoggedInUser, usedStorage, totalStorage, usagePercentage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải thông tin tài khoản: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetAccountInfo(Session.LoggedInUser, "0 B", "1 GB", 0);
            }
        }

        private async Task<long> GetUserStorageFromServer()
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync("localhost", 5000);
                using (sslStream)
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    string message = $"GET_USER_STORAGE|{Session.LoggedInUserId}";
                    await writer.WriteLineAsync(message);

                    string response = await reader.ReadLineAsync();
                    response = response?.Trim();

                    if (response != null)
                    {
                        string[] parts = response.Split('|');
                        if (parts.Length >= 2 && parts[0] == "200")
                        {
                            if (long.TryParse(parts[1], out long storageBytes))
                            {
                                return storageBytes;
                            }
                        }
                    }
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user storage: {ex.Message}");
                return 0;
            }
        }

        private string FormatFileSize(long bytes)
        {
            if (bytes == 0) return "0 B";

            const int unit = 1024;
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            
            double size = bytes;
            int suffixIndex = 0;

            while (size >= unit && suffixIndex < suffixes.Length - 1)
            {
                size /= unit;
                suffixIndex++;
            }

            return $"{size:F1} {suffixes[suffixIndex]}";
        }

        private void lblUsername_Click(object sender, EventArgs e)
        {
        }

        public void SetAccountInfo(string username, string storageUsed, string totalStorage)
        {
            lblUsername.Text = $"Tên đăng nhập: {username}";
            lblStorage.Text = $"Dung lượng: {storageUsed} / {totalStorage}";
        }

        // Overload with progress bar support
        public void SetAccountInfo(string username, string storageUsed, string totalStorage, double usagePercentage)
        {
            lblUsername.Text = $"Tên đăng nhập: {username}";
            lblStorage.Text = $"Dung lượng: {storageUsed} / {totalStorage} ({usagePercentage:F1}%)";
            
            // Update progress bar
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = Math.Min(100, Math.Max(0, (int)Math.Round(usagePercentage)));
            
            // Set progress bar color based on usage
            SetProgressBarColor(usagePercentage);
        }

        private void SetProgressBarColor(double percentage)
        {
            // Change progress bar style based on usage percentage
            if (percentage < 70)
            {
                // Green for low usage (default)
                progressBar1.ForeColor = Color.Green;
            }
            else if (percentage < 90)
            {
                // Yellow/Orange for medium usage  
                progressBar1.ForeColor = Color.Orange;
            }
            else
            {
                // Red for high usage
                progressBar1.ForeColor = Color.Red;
            }
        }

        // Backward compatibility overload - calls LoadAccountInfo to get real data
        public void SetAccountInfo(string username, string storageUsed)
        {
            lblUsername.Text = $"Tên đăng nhập: {username}";
            // Trigger reload to get accurate data from server
            LoadAccountInfo();
        }

        private async void btnChangePassword_Click(object sender, EventArgs e)
        {
            // Yêu cầu nhập mật khẩu cũ
            string oldPassword = Prompt.ShowDialog("Nhập mật khẩu cũ:", "Xác nhận mật khẩu");
            if (string.IsNullOrWhiteSpace(oldPassword))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu cũ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Yêu cầu nhập mật khẩu mới
            string newPassword = Prompt.ShowDialog("Nhập mật khẩu mới:", "Đổi mật khẩu");
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu mới!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Gọi yêu cầu đổi mật khẩu đến server
            string response = await ChangePassword(Session.LoggedInUser, oldPassword, newPassword);
            this.Invoke(new Action(() =>
            {
                switch (response)
                {
                    case "200":
                        MessageBox.Show("Đổi mật khẩu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        // Update stored password for encryption
                        Session.UserPassword = newPassword;
                        break;
                    case "401":
                        MessageBox.Show("Mật khẩu cũ không đúng!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case "500":
                        MessageBox.Show("Lỗi server. Vui lòng thử lại sau!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    default:
                        MessageBox.Show($"Phản hồi không xác định từ server: {response}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                }
            }));
        }

        private async Task<string> ChangePassword(string username, string oldPassword, string newPassword)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync("localhost", 5000);
                using (sslStream)
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                {
                    // Gọi yêu cầu đổi mật khẩu
                    string message = $"CHANGE_PASSWORD|{username}|{oldPassword}|{newPassword}";
                    await writer.WriteLineAsync(message);

                    // Nhận phản hồi từ server
                    string response = await reader.ReadLineAsync();
                    return response?.Trim() ?? "500";
                }
            }
            catch (Exception ex)
            {
                this.Invoke(new Action(() =>
                {
                    MessageBox.Show($"Lỗi kết nối server: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
                return "500";
            }
        }

        // Class dể hiển thị hộp thoại nhập liệu
        public static class Prompt
        {
            public static string ShowDialog(string text, string caption)
            {
                Form prompt = new Form()
                {
                    Width = 300,
                    Height = 150,
                    Text = caption,
                    StartPosition = FormStartPosition.CenterScreen
                };
                Label textLabel = new Label() { Left = 20, Top = 20, Text = text };
                TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 240 };
                Button confirmation = new Button() { Text = "OK", Left = 20, Top = 80, DialogResult = DialogResult.OK };
                confirmation.Click += (sender, e) => { prompt.Close(); };
                prompt.Controls.Add(textLabel);
                prompt.Controls.Add(textBox);
                prompt.Controls.Add(confirmation);
                prompt.AcceptButton = confirmation;

                return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
            }
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận đăng xuất",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Clear session data
                Session.LoggedInUser = "Anonymous";
                Session.LoggedInUserId = -1;
                Session.UserPassword = "";

                // Close current form and Main form
                this.Hide();

                // Find and close Main form
                foreach (Form form in Application.OpenForms.Cast<Form>().ToArray())
                {
                    if (form is Main)
                    {
                        form.Close();
                        break;
                    }
                }

                // Restart application to restore proper navigation flow
                Application.Restart();
            }
        }

    }
}