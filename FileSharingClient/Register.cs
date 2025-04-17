using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using FontAwesome.Sharp;



namespace FileSharingClient
{
    public partial class Register : Form
    {
        private bool isLoginOpen = false;
        private string username = "Tên đăng nhập";
        private string password = "Mật khẩu";
        private string conf_pass = "Xác nhận mật khẩu";
        private string gmail = "Gmail";
        private const string SERVER_IP = "127.0.0.1";
        private const int SERVER_PORT = 5000;
        public Register()
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

            confpasstxtBox.Text = conf_pass;
            confpasstxtBox.ForeColor = Color.Gray;
            confpasstxtBox.Enter += confpasstxtBox_Enter;
            confpasstxtBox.Leave += confpasstxtBox_Leave;

            gmailtxtBox.Text = gmail;
            gmailtxtBox.ForeColor = Color.Gray;
            gmailtxtBox.Enter += gmailtxtBox_Enter;
            gmailtxtBox.Leave += gmailtxtBox_Leave;
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
        private void confpasstxtBox_Enter(object sender, EventArgs e)
        {
            if (confpasstxtBox.Text == conf_pass)
            {
                confpasstxtBox.Text = "";
                confpasstxtBox.ForeColor = Color.Black;
                confpasstxtBox.UseSystemPasswordChar = true;
            }
        }
        private void confpasstxtBox_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(confpasstxtBox.Text))
            {
                confpasstxtBox.Text = conf_pass;
                confpasstxtBox.ForeColor = Color.Gray;
                confpasstxtBox.UseSystemPasswordChar = false;
            }
        }
        private void gmailtxtBox_Enter(object sender, EventArgs e)
        {
            if (gmailtxtBox.Text == gmail)
            {
                gmailtxtBox.Text = "";
                gmailtxtBox.ForeColor = Color.Black;
            }
        }
        private void gmailtxtBox_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(gmailtxtBox.Text))
            {
                gmailtxtBox.Text = gmail;
                gmailtxtBox.ForeColor = Color.Gray;
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (isLoginOpen)
                return;

            isLoginOpen = true;

            // Vô hiệu hóa LinkLabel để tránh click liên tục
            linkLabel1.Enabled = false;

            this.Hide();
            using (Login login = new Login())
            {
                login.ShowDialog();
            }
            this.Show();

            // Kích hoạt lại LinkLabel và reset cờ
            linkLabel1.Enabled = true;
            isLoginOpen = false;
        }

        private async void btnRegister_Click(object sender, EventArgs e)
        {
            await HandleRegister();
        }   
        private async Task HandleRegister()
        {
            try
            {
                // Kết nối đến server
                using (TcpClient client = new TcpClient(SERVER_IP, SERVER_PORT))
                using (NetworkStream stream = client.GetStream())
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
                {
                    // Lấy thông tin từ các TextBox
                    string username = usernametxtBox.Text;
                    string email = gmailtxtBox.Text;
                    string password = passtxtBox.Text;
                    string conf_pass = confpasstxtBox.Text;

                    if (string.IsNullOrWhiteSpace(username) ||
                        string.IsNullOrWhiteSpace(password) ||
                        string.IsNullOrWhiteSpace(conf_pass) ||
                        string.IsNullOrWhiteSpace(email))
                    {
                        this.Invoke(new Action(() =>
                        {
                            MessageBox.Show("Vui lòng nhập đầy đủ thông tin", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }));
                        return;
                    }
                    if (conf_pass != password)
                    {
                        this.Invoke(new Action(() =>
                        {
                            MessageBox.Show("Mật khẩu và xác nhận mật khẩu không trùng khớp!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }));
                        return;
                    }
                    if(!IsValidEmail(email))
                    {
                        this.Invoke(new Action(() =>
                        {
                            MessageBox.Show("Vui lòng nhập đúng định dạng email!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }));
                        return;
                    }

                    // Gửi dữ liệu đăng ký theo định dạng: REGISTER|username|email|password
                    string message = $"REGISTER|{username}|{email}|{password}\n";
                    await writer.WriteLineAsync(message);

                    // Nhận phản hồi từ server (status code dạng số)
                    string response = await reader.ReadLineAsync();
                    response = response?.Trim(); // cắt bỏ khoảng trắng thừa, newline, ...
                    int statusCode;
                    if (int.TryParse(response, out statusCode))
                    {
                        this.Invoke(new Action(() =>
                        {
                            switch (statusCode)
                            {
                                case 201:
                                    MessageBox.Show("Đăng ký thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    this.Hide();
                                    Main mainform = new Main();
                                    mainform.Show();
                                    this.Close();
                                    break;
                                case 409:
                                    MessageBox.Show("Username đã tồn tại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
           if(checkBox1.Checked == true)
            {
                passtxtBox.UseSystemPasswordChar = false;
                confpasstxtBox.UseSystemPasswordChar = false;
            }
           else
            {
                passtxtBox.UseSystemPasswordChar = true;
                confpasstxtBox.UseSystemPasswordChar = true;
            }
        }
        bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

    }
}
