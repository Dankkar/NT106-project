using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Data.SQLite;
using System.Net.Sockets;
using System.IO.Compression;

namespace FileSharingClient
{
    public partial class UploadView: UserControl
    {
        private static string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName;
        private static string dbPath = Path.Combine(projectRoot, "test.db");
        private static string connectionString = $"Data Source={dbPath};Version=3;Pooling=True";
        private List<string> pendingFiles = new List<string>();
        public UploadView()
        {
            InitializeComponent();
            UploadFilePanel.FlowDirection = FlowDirection.LeftToRight;
            UploadFilePanel.AutoScroll = true;
        }



        private string FormatFileSize(long bytes)
        {
            if (bytes >= 1024 * 1024) return $"{bytes / (1024 * 1024.0):0.##} MB";
            if(bytes >= 1024) return $"{bytes / 1024.0:0.##} KB";
            return $"{bytes} B";
        }

        public void AddFileToView(string fileName, string createAt, string owner, string filesize)
        {
            var fileItem = new FileItemControl(fileName, createAt, owner, filesize);
            UploadFilePanel.Controls.Add(fileItem);
        }

        private void DragPanel_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var file in files)
                ProcessLocalFile(file);
        }

        private void DragPanel_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private async void btnUpload_Click(object sender, EventArgs e)
        {
            var filesToUpload = new List<string>(pendingFiles);

            // Neu co nhieu file -> nen lai
            if(filesToUpload.Count > 1)
            {
                string zipFilePath = CompressFiles(filesToUpload);
                filesToUpload = new List<string> { zipFilePath };
            }

            foreach(var filePath in filesToUpload)
            {
                try
                {
                    using(TcpClient client = new TcpClient("127.0.0.1", 5000))
                    {
                        using (NetworkStream stream = client.GetStream())
                        {
                            byte[] fileBytes = File.ReadAllBytes(filePath);
                            string fileName = Path.GetFileName(filePath);
                            int ownerId = Session.LoggedInUserId;
                            string uploadAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                            string command = $"UPLOAD|{fileName}|{fileBytes.Length}|{ownerId}|{uploadAt}\n";
                            byte[] commandBytes = Encoding.UTF8.GetBytes(command);

                            await stream.WriteAsync(commandBytes, 0, commandBytes.Length);
                            await stream.FlushAsync();
                            Console.WriteLine($"Đã gửi lệnh: {command.Trim()}");

                            await stream.WriteAsync(fileBytes, 0, fileBytes.Length);
                            await stream.FlushAsync();
                            Console.WriteLine($"Đã gửi file {fileName} ({fileBytes.Length} bytes)");

                            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                            {
                                string response = await reader.ReadLineAsync();
                                Console.WriteLine($"Server tra ve: {response}");

                                // Kiem tra phan hoi va thong bao (neu can)
                                if (response.Trim() == "413")
                                {
                                    MessageBox.Show($"File {fileName} qua lon. Vui long thu lai voi file nho hon,");
                                }
                                else
                                {
                                    MessageBox.Show($"Tai len thanh cong {fileName}");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Thêm thông báo chi tiết hơn về lỗi
                    MessageBox.Show($"Lỗi upload file {filePath}: {ex.Message}\nStack Trace: {ex.StackTrace}");
                }

            }
            pendingFiles.Clear();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                foreach (var filePath in dialog.FileNames)
                    ProcessLocalFile(filePath);
            }
        }

        private void ProcessLocalFile(string filePath)
        {
            if (!File.Exists(filePath)) return;

            string fileName = Path.GetFileName(filePath);
            string uploadAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string owner = Session.LoggedInUser;
            string fileSize = FormatFileSize(new FileInfo(filePath).Length);

            AddFileToView(fileName, uploadAt, owner, fileSize);
            pendingFiles.Add(filePath);
        }

        private string CompressFiles(List<string> filesToCompress)
        {
            string zipFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".zip");
            using (var zip = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
            {
                foreach (var file in filesToCompress)
                {
                    zip.CreateEntryFromFile(file, Path.GetFileName(file));
                }
            }
            return zipFilePath;
        }
    }
}
