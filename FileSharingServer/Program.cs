using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.Mail;
using System.Text;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FileSharingServer
{
    internal class Program
    {
        /*
        private static string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName;
        private static string dbPath = Path.Combine(projectRoot, "test.db");
        private static string connectionString = $"Data Source={dbPath};Version=3;";
        private static Dictionary<string, (string OTP, DateTime Expiry)> otpStorage = new Dictionary<string, (string, DateTime)>();
        private static Random random = new Random();

        static async Task Main(string[] args)
        {
            await CreateDatabase();
            TcpListener server = new TcpListener(IPAddress.Any, 5000);
            server.Start();
            Console.WriteLine("Server đang lắng nghe trên cổng 5000...");

            string uploadsPath = Path.Combine(projectRoot, "uploads");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
                Console.WriteLine("Đã tạo thư mục uploads.");
            }

            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                _ = HandleClientAsync(client);
            }
        }

        static async Task CreateDatabase()
        {
            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
            }

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                await conn.OpenAsync();
                string createTableQuery = "CREATE TABLE IF NOT EXISTS users (username TEXT PRIMARY KEY, email TEXT, password_hash TEXT)";
                using (SQLiteCommand cmd = new SQLiteCommand(createTableQuery, conn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        static async Task HandleClientAsync(TcpClient client)
        {
            Console.WriteLine($"Client {client.Client.RemoteEndPoint} đã kết nối.");

            try
            {
                using (NetworkStream stream = client.GetStream())
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
                {
                    string request = await reader.ReadLineAsync();
                    if (request == null)
                    {
                        Console.WriteLine("Client gửi dữ liệu trống.");
                        return;
                    }

                    Console.WriteLine($"Nhận từ Client: {request}");

                    string response = await ProcessRequest(request, stream);
                    await writer.WriteLineAsync(response);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xử lý client: {ex.Message}");
            }
            finally
            {
                client.Close();
                Console.WriteLine("Client đã ngắt kết nối.");
            }
        }

        static async Task<string> ProcessRequest(string request, NetworkStream stream)
        {
            string[] parts = request.Split('|');
            if (parts.Length == 0)
            {
                return "400\n"; // Bad Request
            }

            string command = parts[0];

            switch (command)
            {
                case "REGISTER":
                    if (parts.Length != 4) return "400\n";
                    string username = parts[1];
                    string email = parts[2];
                    string password = parts[3];
                    return await RegisterUser(username, email, password);
                case "LOGIN":
                    if (parts.Length != 3) return "400\n";
                    string loginUsername = parts[1];
                    string loginPassword = parts[2];
                    return await LoginUser(loginUsername, loginPassword);
                case "CHANGE_PASSWORD":
                    if (parts.Length != 4) return "400\n";
                    string cpUsername = parts[1];
                    string oldPassword = parts[2];
                    string newPassword = parts[3];
                    return await ChangePassword(cpUsername, oldPassword, newPassword);
                case "REQUEST_OTP":
                    if (parts.Length != 2) return "400\n";
                    string otpEmail = parts[1];
                    return await RequestOTP(otpEmail);
                case "VERIFY_OTP":
                    if (parts.Length != 3) return "400\n";
                    string verifyEmail = parts[1];
                    string otp = parts[2];
                    return await VerifyOTP(verifyEmail, otp);
                case "RESET_PASSWORD":
                    if (parts.Length != 3) return "400\n";
                    string resetEmail = parts[1];
                    string newPasswordReset = parts[2];
                    return await ResetPassword(resetEmail, newPasswordReset);
                case "UPLOAD":
                    if (parts.Length != 5) return "400\n";
                    return await ReceiveFile(parts[1], int.Parse(parts[2]), parts[3], parts[4], stream);
                default:
                    return "400\n";
            }
        }

        static async Task<string> RegisterUser(string username, string email, string password)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    await conn.OpenAsync();

                    // Kiểm tra username đã tồn tại chưa
                    string checkQuery = "SELECT COUNT(*) FROM users WHERE username = @username";
                    using (SQLiteCommand checkCmd = new SQLiteCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@username", username);
                        long userExists = Convert.ToInt64(await checkCmd.ExecuteScalarAsync());

                        if (userExists > 0)
                        {
                            return "409\n"; // Conflict: User đã tồn tại
                        }
                    }

                    // Thêm người dùng mới vào cơ sở dữ liệu
                    string insertQuery = "INSERT INTO users (username, email, password_hash) VALUES (@username, @email, @password_hash)";
                    using (SQLiteCommand insertCmd = new SQLiteCommand(insertQuery, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@username", username);
                        insertCmd.Parameters.AddWithValue("@email", email);
                        insertCmd.Parameters.AddWithValue("@password_hash", password);
                        await insertCmd.ExecuteNonQueryAsync();
                    }
                    return "201\n"; // Created: Đăng ký thành công
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi đăng ký: {ex.Message}");
                return "500\n"; // Internal Server Error
            }
        }

        static async Task<string> LoginUser(string username, string password)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = "SELECT password_hash FROM users WHERE username = @username";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        string storedPassword = await cmd.ExecuteScalarAsync() as string;

                        if (storedPassword != null && storedPassword == password)
                        {
                            return "200\n"; // OK: Đăng nhập thành công
                        }
                        return "401\n"; // Unauthorized: Đăng nhập thất bại
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi đăng nhập: {ex.Message}");
                return "500\n"; // Internal server error
            }
        }

        static async Task<string> ChangePassword(string username, string oldPassword, string newPassword)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    await conn.OpenAsync();

                    // Kiểm tra mật khẩu cũ
                    string checkQuery = "SELECT password_hash FROM users WHERE username = @username";
                    using (SQLiteCommand checkCmd = new SQLiteCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@username", username);
                        string storedPassword = await checkCmd.ExecuteScalarAsync() as string;

                        if (storedPassword != oldPassword)
                        {
                            return "401\n"; // Unauthorized: Mật khẩu cũ không chính xác
                        }
                    }

                    // Cập nhật mật khẩu mới
                    string updateQuery = "UPDATE users SET password_hash = @newPassword WHERE username = @username";
                    using (SQLiteCommand updateCmd = new SQLiteCommand(updateQuery, conn))
                    {
                        updateCmd.Parameters.AddWithValue("@username", username);
                        updateCmd.Parameters.AddWithValue("@newPassword", newPassword);
                        await updateCmd.ExecuteNonQueryAsync();
                    }
                    return "200\n"; // Đổi mật khẩu thành công
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi đổi mật khẩu: {ex.Message}");
                return "500\n"; // Internal Server Error
            }
        }

        static async Task<string> RequestOTP(string email)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
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

        static async Task<bool> SendOTPEmail(string toEmail, string otp)
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

        static async Task<string> VerifyOTP(string email, string otp)
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

        static async Task<string> ResetPassword(string email, string newPassword)
        {
            try
            {
                if (!otpStorage.ContainsKey(email))
                {
                    return "401\n"; // OTP chưa được xác nhận
                }

                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
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

        static async Task<string> ReceiveFile(string fileName, int fileSize, string ownerId, string uploadTime, NetworkStream stream)
        {
            try
            {
                string uploadDir = Path.Combine(projectRoot, "uploads");
                if (!Directory.Exists(uploadDir))
                    Directory.CreateDirectory(uploadDir);

                string filePath = Path.Combine(uploadDir, fileName);
                byte[] buffer = new byte[4096];
                int totalRead = 0;

                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    while(totalRead < fileSize)
                    {
                        int bytesRead = await stream.ReadAsync(buffer, 0, Math.Min(buffer.Length, fileSize - totalRead));
                        if (bytesRead == 0) break;
                        await fs.WriteAsync(buffer, 0, bytesRead);
                        totalRead += bytesRead;
                    }
                }
                Console.WriteLine($"Da nhan: {totalRead}/{fileSize} bytes");
                string fileHash = CalculateSHA256(filePath);

                if(totalRead == fileSize)
                {
                    using(SQLiteConnection conn = new SQLiteConnection(connectionString))
                    {
                        await conn.OpenAsync();
                        string insertQuery = "INSERT INTO files (file_name, upload_at, owner_id, file_size, file_type, file_path, file_hash) VALUES (@fileName, @uploadTime, @ownerId, @fileSize, @fileType, @filePath, @fileHash)";
                        using (SQLiteCommand cmd = new SQLiteCommand(insertQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@fileName", fileName);
                            cmd.Parameters.AddWithValue("@uploadTime", uploadTime);
                            cmd.Parameters.AddWithValue("@ownerId", ownerId);
                            cmd.Parameters.AddWithValue("@fileSize", fileSize);
                            cmd.Parameters.AddWithValue("@fileType", Path.GetExtension(fileName).TrimStart('.').ToLower());
                            cmd.Parameters.AddWithValue("@filePath", Path.Combine("uploads", fileName));
                            cmd.Parameters.AddWithValue("@fileHash", fileHash);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                    return "200\n";
                }
                else
                {
                    return "400\n";
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Loi luu file {ex.Message}");
                return "500\n";
            }
        }
        static string CalculateSHA256(string filePath)
        {
            using(var sha256 = System.Security.Cryptography.SHA256.Create())
            using(var stream = File.OpenRead(filePath))
            {
                var hash = sha256.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
        */
        static async Task Main()
        {
            // Tao thu muc Uploads
            var root = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName;
            Directory.CreateDirectory(Path.Combine(root, "uploads"));

            // Start server
            await new ProtocolHandler().StartAsync();
        }
    }
 }