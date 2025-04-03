using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Data.SQLite;

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

            while(true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                _ = HandleClientAsync(client);
            }
        }
        static async Task HandleClientAsync(TcpClient client)
        {
            Console.WriteLine($"Client {client.Client.RemoteEndPoint} đa ket noi.");

            using (NetworkStream stream = client.GetStream())
            {
                byte[] buffer = new byte[1024];

                //Doc du lieu bat dong bo
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Nhan tu Client: {request}");

                string response = await ProcessRequest(request);
                byte[] responseData = Encoding.UTF8.GetBytes(response);
                await stream.WriteAsync(responseData, 0, responseData.Length);
            }
            Console.WriteLine($"Client da ngat ket noi.");
            client.Close();
        }

        //
        static async Task<string> ProcessRequest(string request)
        {
            string[] parts = request.Split('|');

            if(parts.Length != 3)
            {
                return "INVALID_REQUEST";
            }
            string username = parts[1];
            string password = parts[2];
            switch (parts[0])
            {
                case "REGISTER":
                    {
                        using(SQLiteConnection conn = new SQLiteConnection(connectionString))
                        {
                            try
                            {
                                await conn.OpenAsync();

                                //Kiem tra username da ton tai chua
                                string checkQuery = "SELECT COUNT(*) FROM users WHERE username = @username";
                                using (SQLiteCommand checkCmd = new SQLiteCommand(checkQuery, conn))
                                {
                                    checkCmd.Parameters.AddWithValue("@username", username);
                                    long userExists = Convert.ToInt64(await checkCmd.ExecuteScalarAsync());

                                    if (userExists > 0)
                                    {
                                        return "USER_HAS_EXISTED";
                                    }
                                }
                                //Them nguoi dung moi vao co so du lieu
                                string insertQuery = "INSERT INTO users (username, password_hash) VALUES (@username, @password_hash)";
                                using (SQLiteCommand insertCmd = new SQLiteCommand(insertQuery, conn))
                                {
                                    insertCmd.Parameters.AddWithValue("@username", username);
                                    insertCmd.Parameters.AddWithValue("@password_hash", password);
                                    await insertCmd.ExecuteNonQueryAsync();
                                }
                                return "REGISTER_COMPLETE";
                                
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Lỗi: {ex.Message}");
                                return "ERROR";
                            }
                        }
                    }
                default:
                    break;
            }
            return "UNKNOWN_COMMAND";
        }
    }
}
