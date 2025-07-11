using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using FileSharingClient.Services;

namespace FileSharingClient
{
    public partial class Login : Form
    {
        private bool isRegisterOpen = false;
        private bool isForgotPasswordOpen = false;
        private string username = "Tên đăng nhập";
        private string password = "Mật khẩu";
        private string serverIp = ConfigurationManager.AppSettings["ServerIP"];
        private int serverPort = int.Parse(ConfigurationManager.AppSettings["ServerPort"]);
        private int chunkSize = int.Parse(ConfigurationManager.AppSettings["ChunkSize"]);
        private long maxFileSize = long.Parse(ConfigurationManager.AppSettings["MaxFileSizeMB"]) * 1024 * 1024;
        private string uploadsPath = ConfigurationManager.AppSettings["UploadsPath"];
        private string databasePath = ConfigurationManager.AppSettings["DatabasePath"];
        public Login()
        {
            InitializeComponent();
            usernametxtBox.Text = username;
            usernametxtBox.ForeColor = Color.Gray;
            usernametxtBox.Enter += usernametxtBox_Enter;
            usernametxtBox.Leave += usernametxtBox_Leave;

            passtxtBox.Text = password;
            passtxtBox.ForeColor = Color.Gray;
            passtxtBox.Enter += passtxtBox_Enter;
            passtxtBox.Leave += passtxtBox_Leave;
        }


        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Tag = "register";
            this.Close();
        }

        private void linkForgotPassword_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (isForgotPasswordOpen)
                return; // Nếu form ForgotPassword đã được mở, bỏ qua

            isForgotPasswordOpen = true;
            this.Hide();
            using (ForgotPassword forgotPassword = new ForgotPassword())
            {
                forgotPassword.ShowDialog();
            }
            this.Show();
            isForgotPasswordOpen = false;
        }


        private async Task HandleLogin()
        {
            const int MAX_LOGIN_ATTEMPTS = 3;
            
            // Lấy thông tin từ TextBox và cắt khoảng trắng - kiểm tra trước khi thử kết nối
            string username = usernametxtBox.Text.Trim();
            string password = passtxtBox.Text;
            
            // Kiểm tra placeholder text
            if (username == this.username || string.IsNullOrWhiteSpace(username))
            {
                this.Invoke(new Action(() =>
                {
                    MessageBox.Show("Vui lòng nhập tên đăng nhập", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
                return;
            }
            if (password == this.password || string.IsNullOrWhiteSpace(password))
            {
                this.Invoke(new Action(() =>
                {
                    MessageBox.Show("Vui lòng nhập mật khẩu", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
                return;
            }
            
            for (int attempt = 0; attempt < MAX_LOGIN_ATTEMPTS; attempt++)
            {
                try
                {
                    Console.WriteLine($"[LOGIN] Attempt {attempt + 1}/{MAX_LOGIN_ATTEMPTS}");
                    var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync(serverIp, serverPort);
                    using (sslStream)
                    using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                    using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                    {
                        // Hash password bằng SHA256 trước khi gửi
                        string hashedPassword;
                        using (SHA256 sha256Hash = SHA256.Create())
                        {
                            byte[] data = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                            StringBuilder sb = new StringBuilder();
                            foreach (byte b in data)
                            {
                                sb.Append(b.ToString("x2"));
                            }
                            hashedPassword = sb.ToString();
                        }
                        // Gửi dữ liệu đăng nhập theo định dạng: LOGIN|username|hashedPassword
                        string message = $"LOGIN|{username}|{hashedPassword}";
                        await writer.WriteLineAsync(message);
                        // Nhận phản hồi từ server (status code dạng số)
                        string response = await reader.ReadLineAsync();
                        response = response?.Trim();
                        int statusCode;
                        // Cập nhật giao diện theo status code nhận được
                        if (int.TryParse(response, out statusCode))
                        {
                            int userId = -1;
                            if (statusCode == 200)
                            {
                                // Lấy userId ngay sau khi đăng nhập thành công
                                userId = await ApiService.GetUserIdAsync(username);
                                
                                // Debug: Check if userId is valid
                                if (userId == -1)
                                {
                                    Console.WriteLine($"[ERROR] Failed to get userId for user: {username}");
                                    // If getUserId fails, retry the whole login process
                                    if (attempt < MAX_LOGIN_ATTEMPTS - 1)
                                    {
                                        Console.WriteLine($"[RETRY] Retrying login due to getUserId failure");
                                        await Task.Delay(500);
                                        continue;
                                    }
                                    else
                                    {
                                        MessageBox.Show($"Không thể lấy thông tin user sau {MAX_LOGIN_ATTEMPTS} lần thử. Vui lòng kiểm tra server.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        return;
                                    }
                                }
                                //Console.WriteLine($"[DEBUG] Login successful - userId: {userId}");
                            }

                            this.Invoke(new Action(() =>
                            {
                                switch (statusCode)
                                {
                                    case 200:
                                        Session.LoggedInUser = username;
                                        Session.LoggedInUserId = userId;
                                        Session.UserPassword = password; // Store original password for encryption
                                        System.Windows.Forms.MessageBox.Show("Đăng nhập thành công!", "Thông báo", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                                        this.Hide();
                                        // Mở giao diện chính
                                        Main mainform = new Main();
                                        mainform.Show();
                                        break;
                                    case 401:
                                        MessageBox.Show("Sai tài khoản hoặc mật khẩu!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        break;
                                    case 400:
                                        MessageBox.Show("Yêu cầu không hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        break;
                                    case 500:
                                        MessageBox.Show("Lỗi từ server. Vui lòng thử lại sau!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        break;
                                    default:
                                        MessageBox.Show("Phản hồi không xác định từ server: " + statusCode, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                        break;
                                }
                            }));
                            
                            // If login was successful (200) or failed due to auth issues (401), don't retry
                            if (statusCode == 200 || statusCode == 401 || statusCode == 400)
                            {
                                return;
                            }
                        }
                        else
                        {
                            if (attempt == MAX_LOGIN_ATTEMPTS - 1)
                            {
                                this.Invoke(new Action(() =>
                                {
                                    MessageBox.Show("Phản hồi không hợp lệ từ server: " + response, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[LOGIN ERROR] Attempt {attempt + 1}/{MAX_LOGIN_ATTEMPTS} failed: {ex.Message}");
                    
                    if (attempt == MAX_LOGIN_ATTEMPTS - 1)
                    {
                        this.Invoke(new Action(() =>
                        {
                            MessageBox.Show($"Lỗi kết nối đến server sau {MAX_LOGIN_ATTEMPTS} lần thử: {ex.Message}", "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }));
                    }
                    else
                    {
                        // Wait before retrying
                        await Task.Delay(500 * (attempt + 1));
                    }
                }
            }
        }
        private async Task<int> GetUserIdFromLocalAsync(string username)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync(serverIp, serverPort);
                using (sslStream)
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    // Gửi request GET_USER_ID
                    string message = $"GET_USER_ID|{username}";
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
            catch
            {
                return -1;
            }
        }

        private void passtxtBox_Enter(object sender, EventArgs e)
        {
            if (passtxtBox.Text == password)
            {
                passtxtBox.Text = "";
                passtxtBox.ForeColor = Color.Black;
                passtxtBox.UseSystemPasswordChar = true;
            }
        }

        private void passtxtBox_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(passtxtBox.Text))
            {
                passtxtBox.Text = password;
                passtxtBox.ForeColor = Color.Gray;
                passtxtBox.UseSystemPasswordChar = false;
            }
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            await HandleLogin();
        }

        private void usernametxtBox_Enter(object sender, EventArgs e)
        {
            if (usernametxtBox.Text == username)
            {
                usernametxtBox.Text = "";
                usernametxtBox.ForeColor = Color.Black;
            }
        }

        private void usernametxtBox_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(usernametxtBox.Text))
            {
                usernametxtBox.Text = username;
                usernametxtBox.ForeColor = Color.Gray;
            }
        }

        private void showPassword_CheckedChanged(object sender, EventArgs e)
        {
            if(passtxtBox.UseSystemPasswordChar == true)
            {
                passtxtBox.UseSystemPasswordChar = false;
            }
            else
            {
                passtxtBox.UseSystemPasswordChar = true;
            }
        }
    }

    public static class Session
    {
        public static string LoggedInUser { get; set; } = "Anonymous";
        public static int LoggedInUserId { get; set; } = -1;
        public static string UserPassword { get; set; } = ""; // Store original password for encryption
    }
}