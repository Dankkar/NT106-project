using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;

namespace FileSharingServer
{

    public static class FileService
    {
        const long MAX_UPLOAD_SIZE = 1 * 1024 * 1024; // 1MB

        public static async Task<string> ReceiveFile(string fileName, int fileSize, string ownerId, string uploadTime, NetworkStream stream)
        {
            if (fileSize > MAX_UPLOAD_SIZE)
            {
                return "413\n"; // Payload too large
            }

            try
            {
                string uploadDir = Path.Combine(DatabaseHelper.projectRoot, "uploads");
                if (!Directory.Exists(uploadDir))
                    Directory.CreateDirectory(uploadDir);

                string filePath = Path.Combine(uploadDir, fileName);
                byte[] buffer = new byte[4096];
                int totalRead = 0;

                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    while (totalRead < fileSize)
                    {
                        int bytesRead = await stream.ReadAsync(buffer, 0, Math.Min(buffer.Length, fileSize - totalRead));
                        if (bytesRead == 0) break;
                        await fs.WriteAsync(buffer, 0, bytesRead);
                        totalRead += bytesRead;
                    }
                }

                if (totalRead == fileSize)
                {
                    string fileHash = CalculateSHA256(filePath);
                    using (SQLiteConnection conn = new SQLiteConnection(DatabaseHelper.connectionString))
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
                    return "200\n"; // Success
                }
                else
                {
                    return "400\n"; // Bad Request
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lưu file {ex.Message}");
                return "500\n"; // Internal Server Error
            }
        }

        private static string CalculateSHA256(string filePath)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            using (var stream = File.OpenRead(filePath))
            {
                var hash = sha256.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
