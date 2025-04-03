using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileSharingClient
{
    public partial class Login : Form
    {
        private TcpClient client;
        private NetworkStream stream;
        private const string SERVER_IP = "127.0.0.1";
        private const int SERVER_PORT = 5000;
        public Login()
        {
            InitializeComponent();
        }


        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Hide();
            Register register = new Register();
            register.ShowDialog();
            this.Show();
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
                    MessageBox.Show("Dang nhap thanh cong!", "Thông báo ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Hide();

                    // Mo giao dien chinh
                    Main mainform = new Main();
                    mainform.Show();
                    this.Close();
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
}
