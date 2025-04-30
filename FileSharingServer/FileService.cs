using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FileSharingServer
{
    public static class FileService
    {
        public static async Task<string> ReceiveFile(string fileName, int fileSize, string ownerId, string uploadTime, NetworkStream stream)
        {
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
                Console.WriteLine($"Da nhan: {totalRead}/{fileSize} bytes");
                string fileHash = CalculateSHA256(filePath);

                if (totalRead == fileSize)
                {
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
                    return "200\n";
                }
                else
                {
                    return "400\n";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Loi luu file {ex.Message}");
                return "500\n";
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
