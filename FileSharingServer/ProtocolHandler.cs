using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FileSharingServer
{
    public class ProtocolHandler
    {
        private readonly int Port = 5000;

        public async Task StartAsync()
        {
            // Initialize database with new folder-based schema
            await DatabaseHelper.InitializeDatabaseAsync();
            Console.WriteLine("Database initialized successfully");

            var server = new TcpListener(IPAddress.Any, Port);
            server.Start();
            Console.WriteLine($"Listening on port {Port}");
            while (true)
            {
                var client = await server.AcceptTcpClientAsync();
                _ = HandleClientAsync(client);
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            Console.WriteLine($"Client {client.Client.RemoteEndPoint} connected.");

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
                    return await AuthService.RegisterUser(username, email, password);
                case "LOGIN":
                    if (parts.Length != 3) return "400\n";
                    string loginUsername = parts[1];
                    string loginPassword = parts[2];
                    return await AuthService.LoginUser(loginUsername, loginPassword);
                case "CHANGE_PASSWORD":
                    if (parts.Length != 4) return "400\n";
                    string cpUsername = parts[1];
                    string oldPassword = parts[2];
                    string newPassword = parts[3];
                    return await AuthService.ChangePassword(cpUsername, oldPassword, newPassword);
                case "REQUEST_OTP":
                    if (parts.Length != 2) return "400\n";
                    string otpEmail = parts[1];
                    return await OTPService.RequestOTP(otpEmail);
                case "VERIFY_OTP":
                    if (parts.Length != 3) return "400\n";
                    string verifyEmail = parts[1];
                    string otp = parts[2];
                    return await OTPService.VerifyOTP(verifyEmail, otp);
                case "RESET_PASSWORD":
                    if (parts.Length != 3) return "400\n";
                    string resetEmail = parts[1];
                    string newPasswordReset = parts[2];
                    return await OTPService.ResetPassword(resetEmail, newPasswordReset);
                    
                // NEW: Folder-based operations
                case "UPLOAD_FOLDER":
                    if (parts.Length != 5) return "400\n";
                    string folderName = parts[1];
                    int folderSize = int.Parse(parts[2]);
                    string ownerId = parts[3];
                    string uploadTime = parts[4];
                    return await FolderService.ReceiveFolder(folderName, folderSize, ownerId, uploadTime, stream);
                    
                case "LIST_FOLDERS":
                    if (parts.Length != 2) return "400\n";
                    string userId = parts[1];
                    var folders = await FolderService.GetUserFolders(userId);
                    return FormatFolderList(folders);
                    
                // Legacy: Keep using original FileService for single file uploads
                case "UPLOAD":
                    if (parts.Length != 5) return "400\n";
                    string fileName = parts[1];
                    int fileSize = int.Parse(parts[2]);
                    string fileOwnerId = parts[3];
                    string fileUploadTime = parts[4];
                    
                    return await FileService.ReceiveFile(fileName, fileSize, fileOwnerId, fileUploadTime, stream);
                    
                case "UPLOAD_FILE_IN_FOLDER":
                    if (parts.Length != 7) return "400\n";
                    string upFolderName = parts[1];
                    string upRelativePath = parts[2];
                    string upFileName = parts[3];
                    int upFileSize = int.Parse(parts[4]);
                    string upOwnerId = parts[5];
                    string upUploadTime = parts[6];
                    return await FolderService.ReceiveFileInFolder(upFolderName, upRelativePath, upFileName, upFileSize, upOwnerId, upUploadTime, stream);
                    
                default:
                    return "400\n";
            }
        }

        private static string FormatFolderList(List<FolderInfo> folders)
        {
            if (folders.Count == 0)
            {
                return "200|NO_FOLDERS\n";
            }

            var result = new StringBuilder();
            result.Append("200|");
            
            foreach (var folder in folders)
            {
                result.Append($"{folder.FolderId}:{folder.FolderName}:{folder.CreatedAt}:{folder.IsShared};");
            }
            
            result.Append("\n");
            return result.ToString();
        }
    }
}
