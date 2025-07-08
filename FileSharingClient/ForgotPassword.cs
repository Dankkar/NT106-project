using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FileSharingClient;

namespace FileSharingClient
{
    public partial class ForgotPassword : Form
    {
        private const string SERVER_IP = "localhost";
        private const int SERVER_PORT = 5000;

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
                MessageBox.Show("Vui l�ng nh?p email!", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!IsValidEmail(email))
            {
                MessageBox.Show("Vui l�ng nh?p email h?p l?!", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string response = await RequestOTP(email);
            this.Invoke(new Action(() =>
            {
                switch (response)
                {
                    case "200":
                        MessageBox.Show("M� OTP d� du?c g?i d?n email c?a b?n!", "Th�ng b�o", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        // Hi?n th? � nh?p OTP v� n�t x�c nh?n
                        lblOTP.Visible = true;
                        txtOTP.Visible = true;
                        btnVerifyOTP.Visible = true;
                        // ?n � email v� n�t g?i OTP
                        lblEmail.Visible = false;
                        txtEmail.Visible = false;
                        btnSendOTP.Visible = false;
                        break;
                    case "404":
                        MessageBox.Show("Email kh�ng t?n t?i!", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case "500":
                        MessageBox.Show("L?i server. Vui l�ng th? l?i sau!", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    default:
                        MessageBox.Show($"Ph?n h?i kh�ng x�c d?nh t? server: {response}", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show("Vui l�ng nh?p m� OTP!", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string response = await VerifyOTP(email, otp);
            this.Invoke(new Action(() =>
            {
                switch (response)
                {
                    case "200":
                        MessageBox.Show("X�c nh?n OTP th�nh c�ng!", "Th�ng b�o", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        // Hi?n th? � nh?p m?t kh?u m?i v� x�c nh?n
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
                        MessageBox.Show("M� OTP kh�ng d�ng ho?c d� h?t h?n!", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case "500":
                        MessageBox.Show("L?i server. Vui l�ng th? l?i sau!", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    default:
                        MessageBox.Show($"Ph?n h?i kh�ng x�c d?nh t? server: {response}", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show("Vui l�ng nh?p m?t kh?u m?i v� x�c nh?n m?t kh?u!", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("M?t kh?u m?i v� x�c nh?n m?t kh?u kh�ng kh?p!", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string response = await ResetPassword(email, newPassword);
            this.Invoke(new Action(() =>
            {
                switch (response)
                {
                    case "200":
                        MessageBox.Show("�?t l?i m?t kh?u th�nh c�ng! Vui l�ng dang nh?p l?i.", "Th�ng b�o", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close();
                        break;
                    case "500":
                        MessageBox.Show("L?i server. Vui l�ng th? l?i sau!", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    default:
                        MessageBox.Show($"Ph?n h?i kh�ng x�c d?nh t? server: {response}", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                }
            }));
        }

        private async Task<string> RequestOTP(string email)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync(SERVER_IP, SERVER_PORT);
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
                    MessageBox.Show($"L?i k?t n?i server: {ex.Message}", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
                return "500";
            }
        }

        private async Task<string> VerifyOTP(string email, string otp)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync(SERVER_IP, SERVER_PORT);
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
                    MessageBox.Show($"L?i k?t n?i server: {ex.Message}", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
                return "500";
            }
        }

        private async Task<string> ResetPassword(string email, string newPassword)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync(SERVER_IP, SERVER_PORT);
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
                    MessageBox.Show($"L?i k?t n?i server: {ex.Message}", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
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