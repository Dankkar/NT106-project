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
            switch (response)
            {
                case "SUCCESS":
                    MessageBox.Show("Đổi mật khẩu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case "WRONG_PASSWORD":
                    MessageBox.Show("Mật khẩu cũ không đúng!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case "ERROR":
                    MessageBox.Show("Lỗi khi đổi mật khẩu!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                default:
                    MessageBox.Show("Phản hồi không xác định từ server!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

        private async Task<string> ChangePassword(string username, string oldPassword, string newPassword)
        {
            try
            {
                using (TcpClient client = new TcpClient("127.0.0.1", 5000))
                using (NetworkStream stream = client.GetStream())
                {
                    // Gửi yêu cầu đổi mật khẩu
                    string message = $"CHANGE_PASSWORD|{username}|{oldPassword}|{newPassword}";
                    byte[] data = Encoding.UTF8.GetBytes(message + "\n");
                    await stream.WriteAsync(data, 0, data.Length);

                    // Nhận phản hồi từ server
                    byte[] buffer = new byte[1024];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    return Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối server: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "ERROR";
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
