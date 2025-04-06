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
        private TcpClient client;
        private NetworkStream stream;
        private const string SERVER_IP = "127.0.0.1";
        private const int SERVER_PORT = 5000;
        public Register()
        {
            InitializeComponent();
            usernametxtBox.Text = username;
            usernametxtBox.ForeColor = Color.Gray;
            usernametxtBox.Enter += usernametxtBox_Enter;
            usernametxtBox.Enter += usernametxtBox_Leave;

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
            await Task.Run(() => HandleLogin());
        }   
        private async void HandleLogin()
        {
            try
            {
                //Ket noi den server
                client = new TcpClient(SERVER_IP, SERVER_PORT);
                stream = client.GetStream();
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                //Lay thong tin tu textbox
                string username = usernametxtBox.Text;
                string password = passtxtBox.Text;
                string conf_pass = confpasstxtBox.Text;

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(conf_pass))
                {
                    this.Invoke(new Action(() =>
                    {
                        MessageBox.Show("Vui lòng nhập đầy đủ thông tin", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                    return;
                }
                //Gui du lieu dang ky
                if(conf_pass != password)
                {
                    this.Invoke(new Action(() =>
                    {
                        MessageBox.Show("Mật khẩu và xác nhận mật khẩu không trùng khớp!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                    return;
                }
                string message = $"REGISTER|{username}|{password}\n";
                await writer.WriteLineAsync(message);

                //Nhan phan hoi tu server
                string response = await reader.ReadLineAsync();
                this.Invoke(new Action(() =>
                {
                    if (response == "SUCCESS")
                    {
                        MessageBox.Show("Đăng ký thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (response == "USER_HAS_EXISTED")
                    {
                        MessageBox.Show("Username đã tồn tại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }));
            }
            catch (Exception ex)
            {
                this.Invoke(new Action(() =>
                {
                    MessageBox.Show("Lỗi kết nối đến server: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
            }
            finally
            {
                //Dong ket noi
                stream?.Close();
                client?.Close();
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

    }
}
