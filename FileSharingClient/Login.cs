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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;

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
        private static string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName;
        private static string dbPath = Path.Combine(projectRoot, "test.db");
        private static string connectionString = $"Data Source={dbPath};Version=3;";
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
            SetWALModeAsync();
        }

        private async Task SetWALModeAsync()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = "PRAGMA journal_mode = WAL;";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    await cmd.ExecuteNonQueryAsync();  // Apply WAL mode to improve concurrency
                }
            }
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

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (isRegisterOpen)
                return; // Nếu form Register đã được mở, bỏ qua

            isRegisterOpen = true;
            this.Hide();
            using (Register register = new Register())
            {
                register.ShowDialog();
            }
            this.Show();
            isRegisterOpen = false;
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

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            await HandleLogin();
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
                    string username = usernametxtBox.Text;
                    string password = passtxtBox.Text;

                    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                    {
                        this.Invoke(new Action(() =>
                        {
                            MessageBox.Show("Vui lòng nhập đầy đủ thông tin", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }));
                        return;
                    }

                    // Gửi dữ liệu đăng nhập theo định dạng: LOGIN|username|password\n
                    string message = $"LOGIN|{username}|{password}\n";
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
                using (var conn = new SQLiteConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = "SELECT user_id FROM users WHERE username = @username";
                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        object result = await cmd.ExecuteScalarAsync();
                        return result != null ? Convert.ToInt32(result) : -1;
                    }
                }
            }
            catch
            {
                return -1;
            }
        }
    }

    public static class Session
    {
        public static string LoggedInUser { get; set; } = "Anonymous";
        public static int LoggedInUserId { get; set; } = -1;
    }
}