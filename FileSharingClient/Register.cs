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
using System.Drawing.Drawing2D;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using FontAwesome.Sharp;
using FileSharingClient;

namespace FileSharingClient
{
    public partial class Register : Form
    {
        private bool isLoginOpen = false;
        private string username = "Tên đăng nhập";
        private string password = "Mật khẩu";
        private string conf_pass = "Xác nhận mật khẩu";
        private string gmail = "Gmail";
        private string serverIp = ConfigurationManager.AppSettings["ServerIP"];
        private int serverPort = int.Parse(ConfigurationManager.AppSettings["ServerPort"]);
        private int chunkSize = int.Parse(ConfigurationManager.AppSettings["ChunkSize"]);
        private long maxFileSize = long.Parse(ConfigurationManager.AppSettings["MaxFileSizeMB"]) * 1024 * 1024;
        private string uploadsPath = ConfigurationManager.AppSettings["UploadsPath"];
        private string databasePath = ConfigurationManager.AppSettings["DatabasePath"];
        public Register()
        {
            InitializeComponent();
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            // Nếu có panel chứa nội dung chính, set panel.BackColor = Color.White;
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is Panel panel)
                {
                    panel.BackColor = System.Drawing.Color.White;
                    foreach (Control child in panel.Controls)
                    {
                        if (child is TextBox tb)
                        {
                            tb.BackColor = System.Drawing.Color.White;
                            tb.ForeColor = System.Drawing.Color.Gray;
                        }
                        if (child is Label lbl)
                        {
                            lbl.ForeColor = System.Drawing.Color.Gray;
                        }
                    }
                }
            }
            btnRegister.BackColor = System.Drawing.Color.LightBlue;
            btnRegister.ForeColor = System.Drawing.Color.White;
            btnRegister.FlatStyle = FlatStyle.Flat;
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.MouseEnter += (s, e) => {
                btnRegister.BackColor = System.Drawing.Color.FromArgb(41, 121, 255);
                btnRegister.ForeColor = System.Drawing.Color.White;
            };
            btnRegister.MouseLeave += (s, e) => {
                btnRegister.BackColor = System.Drawing.Color.LightBlue;
                btnRegister.ForeColor = System.Drawing.Color.White;
            };
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

            this.Tag = "login";
            this.Close();
          
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
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync(serverIp, serverPort);
                using (sslStream)
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
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
                    if (password.Length < 8)
                    {
                        this.Invoke(new Action(() =>
                        {
                            MessageBox.Show("Mật khẩu phải có ít nhất 8 ký tự!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                            MessageBox.Show("Vui lòng nhập định dạng email hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    
                    // Gửi dữ liệu đăng ký theo định dạng: REGISTER|username|email|hashedPassword
                    string message = $"REGISTER|{username}|{email}|{hashedPassword}";
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
                                    this.Tag= "login"; // Đặt tag để biết form này đã đăng ký thành công
                                    this.Close();
                                    break;
                                case 409:
                                    MessageBox.Show("Username đã tồn tại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    break;
                                case 400:
                                    MessageBox.Show("Yêu cầu không hợp lý!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
