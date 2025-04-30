using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FileSharingServer
{
    public static class OTPService
    {
        private static readonly Dictionary<string, (string OTP, DateTime Expiry)> otpStorage = new Dictionary<string, (string, DateTime)>();
        private static readonly Random random = new Random();

        public static async Task<string> RequestOTP(string email)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    string query = "SELECT username FROM users WHERE email = @email";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", email);
                        string username = await cmd.ExecuteScalarAsync() as string;

                        if (username == null)
                        {
                            return "404\n"; // Email không tồn tại
                        }

                        // Tạo OTP 6 chữ số
                        string otp = random.Next(100000, 999999).ToString();
                        DateTime expiry = DateTime.Now.AddMinutes(5);
                        otpStorage[email] = (otp, expiry);

                        // Gửi OTP qua email
                        bool emailSent = await SendOTPEmail(email, otp);
                        if (!emailSent)
                        {
                            return "500\n"; // Lỗi gửi email
                        }

                        Console.WriteLine($"OTP cho {email}: {otp} (Hết hạn: {expiry})");
                        return "200\n"; // OTP gửi thành công
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi yêu cầu OTP: {ex.Message}");
                return "500\n"; // Internal Server Error
            }
        }
        public static async Task<bool> SendOTPEmail(string toEmail, string otp)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress("hieudinhle1204@gmail.com"); // Thay bằng email của bạn
                    mail.To.Add(toEmail);
                    mail.Subject = "Mã OTP để đặt lại mật khẩu";
                    mail.Body = $"Mã OTP của bạn là: {otp}\nMã này có hiệu lực trong 5 phút.";
                    mail.IsBodyHtml = false;

                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new NetworkCredential("hieudinhle1204@gmail.com", "inyuoohlwcqcklib"); // Thay bằng email và App Password
                        smtp.EnableSsl = true;
                        await smtp.SendMailAsync(mail);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi gửi email: {ex.Message}");
                return false;
            }
        }
        public static async Task<string> VerifyOTP(string email, string otp)
        {
            try
            {
                if (!otpStorage.ContainsKey(email))
                {
                    return "401\n"; // OTP không tồn tại
                }

                var (storedOTP, expiry) = otpStorage[email];
                if (DateTime.Now > expiry)
                {
                    otpStorage.Remove(email);
                    return "401\n"; // OTP hết hạn
                }

                if (storedOTP != otp)
                {
                    return "401\n"; // OTP không đúng
                }

                return "200\n"; // OTP hợp lệ
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xác nhận OTP: {ex.Message}");
                return "500\n"; // Internal Server Error
            }
        }
        public static async Task<string> ResetPassword(string email, string newPassword)
        {
            try
            {
                if (!otpStorage.ContainsKey(email))
                {
                    return "401\n"; // OTP chưa được xác nhận
                }

                using (SQLiteConnection conn = new SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    string updateQuery = "UPDATE users SET password_hash = @newPassword WHERE email = @email";
                    using (SQLiteCommand updateCmd = new SQLiteCommand(updateQuery, conn))
                    {
                        updateCmd.Parameters.AddWithValue("@email", email);
                        updateCmd.Parameters.AddWithValue("@newPassword", newPassword);
                        int rowsAffected = await updateCmd.ExecuteNonQueryAsync();

                        if (rowsAffected == 0)
                        {
                            return "404\n"; // Email không tồn tại
                        }
                    }
                }

                otpStorage.Remove(email); // Xóa OTP sau khi đặt lại mật khẩu
                return "200\n"; // Đặt lại mật khẩu thành công
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi đặt lại mật khẩu: {ex.Message}");
                return "500\n"; // Internal Server Error
            }
        }
    }
}
