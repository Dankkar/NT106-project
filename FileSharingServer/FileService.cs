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
        const int MAX_UPLOAD_SIZE = 10 * 1024 * 1024; // 10MB
        const int BUFFER_SIZE = 8192; // Match client buffer size

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

                string userDir = Path.Combine(uploadDir, ownerId); // tạo thư mục uploads/ownerId
                if (!Directory.Exists(userDir))
                    Directory.CreateDirectory(userDir);

                string filePath = Path.Combine(userDir, fileName); // uploads/ownerId/filename
                byte[] buffer = new byte[BUFFER_SIZE];
                int totalRead = 0;


                // Luu file zip hoac file binh thuong vao server
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

                // Kiem tra neu la file zip
                if (fileName.EndsWith(".zip"))
                {
                    string extractDir = Path.Combine(userDir, Path.GetFileNameWithoutExtension(fileName));
                    ExtractZipFile(filePath, extractDir);

                    // Duyet qua cac file giai nen a luu thong tin vao DB
                    string[] extractedFiles = Directory.GetFiles(extractDir);
                    foreach (var extractedFile in extractedFiles)
                    {
                        string extractedFileName = Path.GetFileName(extractedFile);
                        long extractedFileSize = new FileInfo(extractedFile).Length;
                        string fileHash = CalculateSHA256(extractedFile);

                        // Luu thong tin vao DB voi duong dan la 'uploads/[ten file trong zip]'
                        string filePathInDb = Path.Combine("uploads", ownerId, extractedFileName);

                        // Di chuyen cac file da giai nen vao folder uploads (khong luu muc con)
                        string destFilePath = Path.Combine(userDir, extractedFileName);
                        try
                        {
                            File.Move(extractedFile, destFilePath); // Di chuyen file giai nen vao thu muc uploads
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine($"Loi khi di chuyen file {extractedFileName} : {ex.Message}");
                        }

                        using (SQLiteConnection conn = new SQLiteConnection(DatabaseHelper.connectionString))
                        {
                            await conn.OpenAsync();
                            string insertQuery = "INSERT INTO files (file_name, upload_at, owner_id, file_size, file_type, file_path, file_hash, folder_id) VALUES (@fileName, @uploadTime, @ownerId, @fileSize, @fileType, @filePath, @fileHash, @folderId)";
                            using (SQLiteCommand cmd = new SQLiteCommand(insertQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("@fileName", extractedFileName);
                                cmd.Parameters.AddWithValue("@uploadTime", uploadTime);
                                cmd.Parameters.AddWithValue("@ownerId", int.Parse(ownerId));
                                cmd.Parameters.AddWithValue("@fileSize", extractedFileSize);
                                cmd.Parameters.AddWithValue("@fileType", Path.GetExtension(extractedFile).TrimStart('.').ToLower());
                                cmd.Parameters.AddWithValue("@filePath", filePathInDb);
                                cmd.Parameters.AddWithValue("@fileHash", fileHash);
                                cmd.Parameters.AddWithValue("@folderId", DBNull.Value); // NULL for legacy file uploads
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }
                    }
                    // Sau khi giai nen va luu thong tin cac file, xoa file zip goc
                    File.Delete(filePath); // Xoa file zip goc de khong luu lai trong thu muc uploads
                    try
                    {
                        Directory.Delete(extractDir, true); // Xoa thu muc tam chua cac file giai nen
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($"Loi khi xoa thu muc {extractDir} : {ex.Message}");
                    }
                }

                else
                {
                    string fileHash = CalculateSHA256(filePath);

                    if (totalRead == fileSize)
                    {
                        using (SQLiteConnection conn = new SQLiteConnection(DatabaseHelper.connectionString))
                        {
                            await conn.OpenAsync();
                            string insertQuery = "INSERT INTO files (file_name, upload_at, owner_id, file_size, file_type, file_path, file_hash, folder_id) VALUES (@fileName, @uploadTime, @ownerId, @fileSize, @fileType, @filePath, @fileHash, @folderId)";
                            using (SQLiteCommand cmd = new SQLiteCommand(insertQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("@fileName", fileName);
                                cmd.Parameters.AddWithValue("@uploadTime", uploadTime);
                                cmd.Parameters.AddWithValue("@ownerId", int.Parse(ownerId));
                                cmd.Parameters.AddWithValue("@fileSize", fileSize);
                                cmd.Parameters.AddWithValue("@fileType", Path.GetExtension(fileName).TrimStart('.').ToLower());
                                cmd.Parameters.AddWithValue("@filePath", Path.Combine("uploads",ownerId, fileName));
                                cmd.Parameters.AddWithValue("@fileHash", fileHash);
                                cmd.Parameters.AddWithValue("@folderId", DBNull.Value); // NULL for legacy file uploads
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
                return "200\n";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Loi luu file {ex.Message}");
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

        private static void ExtractZipFile(string zipFilePath, string extractPath)
        {
            if (!Directory.Exists(extractPath))
            {
                Directory.CreateDirectory(extractPath);
            }
            ZipFile.ExtractToDirectory(zipFilePath, extractPath);
        }
    }
}
