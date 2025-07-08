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
                MessageBox.Show($"L?i khi t?i thông tin tài kho?n: {ex.Message}", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            lblUsername.Text = $"Tên dang nh?p: {username}";
            lblStorage.Text = $"Dung lu?ng: {storageUsed} / {totalStorage}";
        }

        // Overload with progress bar support
        public void SetAccountInfo(string username, string storageUsed, string totalStorage, double usagePercentage)
        {
            lblUsername.Text = $"Tên dang nh?p: {username}";
            lblStorage.Text = $"Dung lu?ng: {storageUsed} / {totalStorage} ({usagePercentage:F1}%)";
            
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
            lblUsername.Text = $"Tên dang nh?p: {username}";
            // Trigger reload to get accurate data from server
            LoadAccountInfo();
        }

        private async void btnChangePassword_Click(object sender, EventArgs e)
        {
            // Yêu c?u nh?p m?t kh?u cu
            string oldPassword = Prompt.ShowDialog("Nh?p m?t kh?u cu:", "Xác nh?n m?t kh?u");
            if (string.IsNullOrWhiteSpace(oldPassword))
            {
                MessageBox.Show("Vui lòng nh?p m?t kh?u cu!", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Yêu c?u nh?p m?t kh?u m?i
            string newPassword = Prompt.ShowDialog("Nh?p m?t kh?u m?i:", "Ð?i m?t kh?u");
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                MessageBox.Show("Vui lòng nh?p m?t kh?u m?i!", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // G?i yêu c?u d?i m?t kh?u d?n server
            string response = await ChangePassword(Session.LoggedInUser, oldPassword, newPassword);
            this.Invoke(new Action(() =>
            {
                switch (response)
                {
                    case "200":
                        MessageBox.Show("Ð?i m?t kh?u thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        // Update stored password for encryption
                        Session.UserPassword = newPassword;
                        break;
                    case "401":
                        MessageBox.Show("M?t kh?u cu không dúng!", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case "500":
                        MessageBox.Show("L?i server. Vui lòng th? l?i sau!", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    default:
                        MessageBox.Show($"Ph?n h?i không xác d?nh t? server: {response}", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    // G?i yêu c?u d?i m?t kh?u
                    string message = $"CHANGE_PASSWORD|{username}|{oldPassword}|{newPassword}";
                    await writer.WriteLineAsync(message);

                    // Nh?n ph?n h?i t? server
                    string response = await reader.ReadLineAsync();
                    return response?.Trim() ?? "500";
                }
            }
            catch (Exception ex)
            {
                this.Invoke(new Action(() =>
                {
                    MessageBox.Show($"L?i k?t n?i server: {ex.Message}", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
                return "500";
            }
        }

        // Class d? hi?n th? h?p tho?i nh?p li?u
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
            var result = MessageBox.Show("B?n có ch?c ch?n mu?n dang xu?t?", "Xác nh?n dang xu?t",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Clear session data
                Session.LoggedInUser = "Anonymous";
                Session.LoggedInUserId = -1;
                Session.UserPassword = "";

                // Close current form and show login
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

                // Show login form
                Login loginForm = new Login();
                loginForm.Show();
            }
        }

    }
}