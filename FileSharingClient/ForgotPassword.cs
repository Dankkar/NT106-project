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
using FileSharingClient;
using System.Security.Cryptography;
namespace FileSharingClient
{
    public partial class ForgotPassword : Form
    {
        private string serverIp = ConfigurationManager.AppSettings["ServerIP"];
        private int serverPort = int.Parse(ConfigurationManager.AppSettings["ServerPort"]);
        private int chunkSize = int.Parse(ConfigurationManager.AppSettings["ChunkSize"]);
        private long maxFileSize = long.Parse(ConfigurationManager.AppSettings["MaxFileSizeMB"]) * 1024 * 1024;
        private string uploadsPath = ConfigurationManager.AppSettings["UploadsPath"];
        private string databasePath = ConfigurationManager.AppSettings["DatabasePath"];

        public ForgotPassword()
        {
            InitializeComponent();
            InitializeControls();
        }

        private void InitializeControls()
        {
            // Ban d?u ch? hi?n th? � nh?p email v� n�t g?i OTP
            lblOTP.Visible = false;
            txtOTP.Visible = false;
            btnVerifyOTP.Visible = false;
            lblNewPassword.Visible = false;
            txtNewPassword.Visible = false;
            lblConfirmPassword.Visible = false;
            txtConfirmPassword.Visible = false;
            btnResetPassword.Visible = false;
        }

        private async void btnSendOTP_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Vui lòng nhập email!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!IsValidEmail(email))
            {
                MessageBox.Show("Vui lòng nhập email hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string response = await RequestOTP(email);
            this.Invoke(new Action(() =>
            {
                switch (response)
                {
                    case "200":
                        MessageBox.Show("Mã OTP đã được gửi đến email của bạn!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        // Hi?n th? nh?p OTP v nt xc nh?n
                        lblOTP.Visible = true;
                        txtOTP.Visible = true;
                        btnVerifyOTP.Visible = true;
                        // ?n email v nt g?i OTP
                        lblEmail.Visible = false;
                        txtEmail.Visible = false;
                        btnSendOTP.Visible = false;
                        break;
                    case "404":
                        MessageBox.Show("Email không tồn tại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case "500":
                        MessageBox.Show("Lỗi server. Vui lòng thử lại sau!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    default:
                        MessageBox.Show($"Phần hồi không xác định từ server: {response}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                }
            }));
        }

        private async void btnVerifyOTP_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string otp = txtOTP.Text.Trim();
            if (string.IsNullOrWhiteSpace(otp))
            {
                MessageBox.Show("Vui lòng nhập mã OTP!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string response = await VerifyOTP(email, otp);
            this.Invoke(new Action(() =>
            {
                switch (response)
                {
                    case "200":
                        MessageBox.Show("Xác nhận OTP thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        // Hi?n th? nh?p m?t kh?u m?i v xc nh?n
                        lblNewPassword.Visible = true;
                        txtNewPassword.Visible = true;
                        lblConfirmPassword.Visible = true;
                        txtConfirmPassword.Visible = true;
                        btnResetPassword.Visible = true;
                        // ?n � OTP v� n�t x�c nh?n
                        lblOTP.Visible = false;
                        txtOTP.Visible = false;
                        btnVerifyOTP.Visible = false;
                        break;
                    case "401":
                        MessageBox.Show("Mã OTP không đúng hoặc đã hết hạn!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case "500":
                        MessageBox.Show("Lỗi server. Vui lòng thử lại sau!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    default:
                        MessageBox.Show($"Phần hồi không xác định từ server: {response}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                }
            }));
        }

        private async void btnResetPassword_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string newPassword = txtNewPassword.Text.Trim();
            string confirmPassword = txtConfirmPassword.Text.Trim();

            if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu mới và xác nhận mật khẩu!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Mật khẩu mới và xác nhận mật khẩu không khớp!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string response = await ResetPassword(email, newPassword);
            this.Invoke(new Action(() =>
            {
                switch (response)
                {
                    case "200":
                        MessageBox.Show("Đặt lại mật khẩu thành công! Vui lòng đăng nhập lại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close();
                        break;
                    case "500":
                        MessageBox.Show("Lỗi server. Vui lòng thử lại sau!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    default:
                        MessageBox.Show($"Phần hồi không xác định từ server: {response}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                }
            }));
        }

        private async Task<string> RequestOTP(string email)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync(serverIp, serverPort);
                using (sslStream)
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                {
                    string message = $"REQUEST_OTP|{email}";
                    await writer.WriteLineAsync(message);
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

        private async Task<string> VerifyOTP(string email, string otp)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync(serverIp, serverPort);
                using (sslStream)
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                {
                    string message = $"VERIFY_OTP|{email}|{otp}";
                    await writer.WriteLineAsync(message);
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

        private async Task<string> ResetPassword(string email, string newPassword)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync(serverIp, serverPort);
                using (sslStream)
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                {
                    // Hash password b?ng SHA256 tru?c khi g?i
                    string hashedPassword;
                    using (SHA256 sha256Hash = SHA256.Create())
                    {
                        byte[] data = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(newPassword));
                        StringBuilder sb = new StringBuilder();
                        foreach (byte b in data)
                        {
                            sb.Append(b.ToString("x2"));
                        }
                        hashedPassword = sb.ToString();
                    }
                    
                    string message = $"RESET_PASSWORD|{email}|{hashedPassword}";
                    await writer.WriteLineAsync(message);
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

        private bool IsValidEmail(string email)
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