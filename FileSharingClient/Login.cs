using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileSharingClient
{
    public partial class Login : Form
    {
        private bool isRegisterOpen = false;
        private string username = "Tên đăng nhập";
        private string password = "Mật khẩu";
        private TcpClient client;
        private NetworkStream stream;
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

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            await Task.Run(() => HandleLogin());
        }

        private void HandleLogin()
        {
            try
            {
                // Ket noi den server
                client = new TcpClient(SERVER_IP, SERVER_PORT);
                stream = client.GetStream();

                // Lay thong tin tu textbox
                string username = usernametxtBox.Text.Trim();
                string password = passtxtBox.Text.Trim();

                if(string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ thông tin", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                // Gui du lieu dang nhap
                string message = $"LOGIN|{username}|{password}";
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);

                // Nhan phan hoi tu server
                byte[] buffer = new byte[1024];
                int byteRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, byteRead);

                if(response == "SUCCESS")
                {
                    Session.LoggedInUser = username;
                    MessageBox.Show("Dang nhap thanh cong!", "Thông báo ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Hide();

                    // Mo giao dien chinh
                    Main mainform = new Main();
                    mainform.Show();
                    this.Close();
                }
                else if (response == "FAIL")
                {
                    MessageBox.Show("Sai tài khoản hoặc mật khẩu!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Sai tai khoan hoac mat khau", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Loi ket noi den server" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Dong ket noi
                stream?.Close();
                client.Close();
            }
        }

    }
    public static class Session
    {
        public static string LoggedInUser { get; set; } = "Anonymous";
    }
}
