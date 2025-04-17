using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Threading;

namespace FileSharingServer
{
    internal class Program
    {
        private static string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName;
        private static string dbPath = Path.Combine(projectRoot, "test.db");
        private static string SERVER_IP = "127.0.0.1";
        private static int SERVER_PORT = 5000;
        private static TcpListener server;
        private static CancellationTokenSource cts;
        private static string connectionString = $"Data Source={dbPath};Version=3;";

        static async Task Main(string[] args)
        {
            cts = new CancellationTokenSource();
            Task serverTask = StartServerAsync(cts.Token);
            Console.WriteLine("Nhan Enter de dung server.");
            Console.ReadLine();
            cts.Cancel();
            try
            {
                await serverTask;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Server da dung.");
            }
        }
        private static async Task StartServerAsync(CancellationToken token)
        {
            server = new TcpListener(IPAddress.Parse(SERVER_IP), SERVER_PORT);
            server.Start();
            Console.WriteLine("Server dang lang nghe tren cong 5000...");

            try
            {
                while(!token.IsCancellationRequested)
                {
                    //Cho client ket noi
                    TcpClient client = await server.AcceptTcpClientAsync();
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                    //Xu ly client ket noi
                    _ = HandleClientAsync(client);
                }
            }
            catch (Exception ex) 
            {
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine("Server dừng vì yêu cầu hủy.");
                }
                else
                {
                    Console.WriteLine($"Lỗi server: {ex.Message}");
                }
            }
            finally
            {
                server.Stop();
                Console.WriteLine("Server da ngung.");
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
                string createTableQuery = "CREATE TABLE IF NOT EXISTS users (username TEXT PRIMARY KEY, password_hash TEXT)";
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

                    string response = await ProcessRequest(request);
                    
                    // Thêm "\n" để client đọc được
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

        static async Task<string> ProcessRequest(string request)
        {
            if (request == null || !request.Contains("|"))
            {
                return "400\n"; // Bad Request: Dữ liệu không hợp lệ
            }
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
                    string R_username = parts[1];
                    string R_email = parts[2];
                    string R_password = parts[3];
                    return await RegisterUser(R_username,R_email, R_password);
                case "LOGIN":
                    if (parts.Length != 3) return "400\n";
                    string L_Username = parts[1];
                    string L_Password = parts[2];
                    return await LoginUser(L_Username, L_Password);
                case "CHANGE_PASSWORD":
                    if (parts.Length != 4) return "400\n";
                    string CP_Username = parts[1];
                    string oldPassword = parts[2];
                    string newPassword = parts[3];
                    return await ChangePassword(CP_Username, oldPassword, newPassword);
                /*case "FORGOT_PASSWORD":
                    if (parts.Length != 2) return "400\n";
                    string FP_email = parts[1];
                    return await ForgotPassword(FP_email); */
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
                            return "409\n"; // Conflict: User da ton tai
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
                    return "201\n"; //Created: Dang ky thanh cong
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi đăng ký: {ex.Message}");
                return "500\n"; //Internal Server Error
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
                            return "200\n"; //OK: dang nhap thanh cong
                        }
                        return "401\n"; //Unauthorized: Dang nhap that bai
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi đăng nhập: {ex.Message}");
                return "500\n"; //Internal server error
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
                            return "401\n"; //Unauthorized: Mat khau cu khong chinh xac
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
                    return "200\n"; //Doi mat khau thanh cong
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi đổi mật khẩu: {ex.Message}");
                return "500\n"; //Internal Server Error
            }
        }
        /*
        static async Task<string> ForgotPassword(string email)
        {
            
        }
        */
    }
}

