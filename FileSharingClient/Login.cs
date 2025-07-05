using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileSharingClient
{
    public partial class Login : Form
    {
        private bool isRegisterOpen = false;
        private bool isForgotPasswordOpen = false;
        private string username = "Tên đăng nhập";
        private string password = "Mật khẩu";
        private const string SERVER_IP = "127.0.0.1";
        private const int SERVER_PORT = 5000;
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
            try
            {
                using (TcpClient client = new TcpClient(SERVER_IP, SERVER_PORT))
                using (NetworkStream stream = client.GetStream())
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
                {
                    // Lấy thông tin từ TextBox và cắt khoảng trắng
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
                    
                    // Gửi dữ liệu đăng nhập theo định dạng: LOGIN|username|hashedPassword\n
                    string message = $"LOGIN|{username}|{hashedPassword}\n";
                    await writer.WriteLineAsync(message);

                    // Nhận phản hồi từ server (status code dạng số)
                    string response = await reader.ReadLineAsync();
                    response = response?.Trim();
                    int statusCode;

                    // Cập nhật giao diện theo status code nhận được
                    if (int.TryParse(response, out statusCode))
                    {
                        this.Invoke(new Action(async () =>
                        {
                            switch (statusCode)
                            {
                                case 200:
                                    Session.LoggedInUser = username;
                                    Session.LoggedInUserId = await GetUserIdFromLocalAsync(username);
                                    MessageBox.Show("Đăng nhập thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    }
                    else
                    {
                        this.Invoke(new Action(() =>
                        {
                            MessageBox.Show("Phản hồi không hợp lệ từ server: " + response, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                this.Invoke(new Action(() =>
                {
                    MessageBox.Show("Lỗi kết nối đến server: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
            }
        }
        private async Task<int> GetUserIdFromLocalAsync(string username)
        {
            try
            {
                using (TcpClient client = new TcpClient(SERVER_IP, SERVER_PORT))
                using (NetworkStream stream = client.GetStream())
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
                {
                    // Gửi request GET_USER_ID
                    string message = $"GET_USER_ID|{username}\n";
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
    }
}