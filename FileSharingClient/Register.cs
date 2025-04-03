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



namespace FileSharingClient
{
    public partial class Register : Form
    {
        private TcpClient client;
        private NetworkStream stream;
        private const string SERVER_IP = "127.0.0.1";
        private const int SERVER_PORT = 5000;
        public Register()
        {
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Hide();
            Login login = new Login();
            login.ShowDialog();
            this.Show();
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



        private void button1_Click(object sender, EventArgs e)
        {
            // Đảo trạng thái hiển thị mật khẩu
            if (passtxtBox.UseSystemPasswordChar)
            {
                passtxtBox.UseSystemPasswordChar = false;
                button1.Text = "hide password";
            }
            else
            {
                passtxtBox.UseSystemPasswordChar = true;
                button1.Text = "show password";
            }
        }


    }
}
