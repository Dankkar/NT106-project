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

namespace FileSharingClient
{
    public partial class Account : Form
    {
        public Account()
        {
            InitializeComponent();
        }

        private void lblUsername_Click(object sender, EventArgs e)
        {
        }

        public void SetAccountInfo(string username, string storageUsed)
        {
            lblUsername.Text = $"Tên đăng nhập: {username}";
            lblStorage.Text = $"Dung lượng đã sử dụng: {storageUsed}";
        }

        private async void btnChangePassword_Click(object sender, EventArgs e)
        {
            // Yêu cầu nhập mật khẩu cũ
            string oldPassword = Prompt.ShowDialog("Nhập mật khẩu cũ:", "Xác nhận mật khẩu");
            if (string.IsNullOrWhiteSpace(oldPassword))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu cũ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Yêu cầu nhập mật khẩu mới
            string newPassword = Prompt.ShowDialog("Nhập mật khẩu mới:", "Đổi mật khẩu");
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu mới!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Gửi yêu cầu đổi mật khẩu đến server
            string response = await ChangePassword(Session.LoggedInUser, oldPassword, newPassword);
            this.Invoke(new Action(() =>
            {
                switch (response)
                {
                    case "200":
                        MessageBox.Show("Đổi mật khẩu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    case "401":
                        MessageBox.Show("Mật khẩu cũ không đúng!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case "500":
                        MessageBox.Show("Lỗi server. Vui lòng thử lại sau!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    default:
                        MessageBox.Show($"Phản hồi không xác định từ server: {response}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                }
            }));
        }

        private async Task<string> ChangePassword(string username, string oldPassword, string newPassword)
        {
            try
            {
                using (TcpClient client = new TcpClient("127.0.0.1", 5000))
                using (NetworkStream stream = client.GetStream())
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    // Gửi yêu cầu đổi mật khẩu
                    string message = $"CHANGE_PASSWORD|{username}|{oldPassword}|{newPassword}\n";
                    await writer.WriteLineAsync(message);

                    // Nhận phản hồi từ server
                    string response = await reader.ReadLineAsync();
                    return response?.Trim() ?? "500";
                }
            }
            catch (Exception ex)
            {
                this.Invoke(new Action(() =>
                {
                    MessageBox.Show($"Lỗi kết nối server: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
                return "500";
            }
        }

        // Class để hiển thị hộp thoại nhập liệu
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
    }
}