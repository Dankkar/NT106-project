using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace FileSharingServer
{
    internal class Program
    {
        private static string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName;
        private static string dbPath = Path.Combine(projectRoot, "test.db");
        private static string connectionString = $"Data Source={dbPath};Version=3;";

        static async Task Main(string[] args)
        {
            TcpListener server = new TcpListener(IPAddress.Any, 5000);
            server.Start();
            Console.WriteLine("Server đang lắng nghe trên cổng 5000...");

            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                _ = HandleClientAsync(client);
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
            string[] parts = request.Split('|');

            if (parts.Length != 3)
            {
                return "ERROR\n"; // Thêm "\n" để client đọc được phản hồi đầy đủ
            }

            string command = parts[0];
            string username = parts[1];
            string password = parts[2];

            if (command == "REGISTER")
            {
                return await RegisterUser(username, password);
            }

            return "ERROR\n";
        }

        static async Task<string> RegisterUser(string username, string password)
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
                            return "USER_HAS_EXISTED\n"; // Thêm "\n" để client đọc được phản hồi đầy đủ
                        }
                    }

                    // Thêm người dùng mới vào cơ sở dữ liệu
                    string insertQuery = "INSERT INTO users (username, password_hash) VALUES (@username, @password_hash)";
                    using (SQLiteCommand insertCmd = new SQLiteCommand(insertQuery, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@username", username);
                        insertCmd.Parameters.AddWithValue("@password_hash", password);
                        await insertCmd.ExecuteNonQueryAsync();
                    }
                    return "SUCCESS\n";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi đăng ký: {ex.Message}");
                return "ERROR\n";
            }
        }
    }
}
