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
        public event Func<Task> FileUploaded;
        private List<string> pendingFiles = new List<string>();
        private long totalSizeBytes = 0;
        private const int BUFFER_SIZE = 8192; // Match server buffer size

        public UploadView()
        {
            InitializeComponent();
            UploadFilePanel.FlowDirection = FlowDirection.LeftToRight;
            UploadFilePanel.AutoScroll = true;
            AddHeaderRow();
        }



        private string FormatFileSize(long bytes)
        {
            if (bytes >= 1024 * 1024) return $"{bytes / (1024 * 1024.0):0.##} MB";
            if(bytes >= 1024) return $"{bytes / 1024.0:0.##} KB";
            return $"{bytes} B";
        }

        public void AddFileToView(string fileName, string createAt, string owner, string filesize, string filePath)
        {
            var fileItem = new FileItemControl(fileName, createAt, owner, filesize, filePath);
            fileItem.FileDeleted += OnFileDeleted;
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
                            long filesize = new FileInfo(filePath).Length;
                            string fileName = Path.GetFileName(filePath);
                            int ownerId = Session.LoggedInUserId;
                            string uploadAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            string command = $"UPLOAD|{fileName}|{filesize}|{ownerId}|{uploadAt}\n";
                            byte[] commandBytes = Encoding.UTF8.GetBytes(command);
                            await stream.WriteAsync(commandBytes, 0, commandBytes.Length);
                            await stream.FlushAsync();
                            Console.WriteLine($"Đã gửi lệnh: {command.Trim()}");

                            //Gui file theo tung phan
                            byte[] buffer = new byte[BUFFER_SIZE];
                            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                            {
                                int bytesRead;
                                while ((bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                {
                                    await stream.WriteAsync(buffer, 0, bytesRead);
                                }
                            }
                            await stream.FlushAsync();
                            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                            {
                                string response = await reader.ReadLineAsync();
                                Console.WriteLine($"Server trả về: {response}");

                                if (response.Trim() == "413")
                                    MessageBox.Show("File quá lớn. Vui lòng thử lại với file nhỏ hơn.");
                                else if (response.Trim() == "200")
                                {
                                    MessageBox.Show("Tải lên thành công");
                                    if (FileUploaded != null)
                                        await FileUploaded.Invoke();
                                }
                                else
                                    MessageBox.Show($"Lỗi: {response.Trim()}");
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
            totalSizeBytes = 0;
            for (int i = UploadFilePanel.Controls.Count - 1; i >= 1; i--)
            {
                var control = UploadFilePanel.Controls[i];
                control.Dispose();
            }
            UpdateFileSizeLabel();
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
            const long MAX_TOTAL_SIZE = 10 * 1024 * 1024;
            if (!File.Exists(filePath)) return;

            string fileName = Path.GetFileName(filePath);
            string uploadAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string owner = Session.LoggedInUser;
            FileInfo fi = new FileInfo(filePath);
            long fileSizeBytes = fi.Length;
            
            if(totalSizeBytes + fileSizeBytes > MAX_TOTAL_SIZE)
            {
                MessageBox.Show($"Không thể thêm '{fileName}' vì tổng dung lượng vượt quá 10MB.");
                return;
            }
            totalSizeBytes += fileSizeBytes;

            string fileSize = FormatFileSize(fileSizeBytes);
            AddFileToView(fileName, uploadAt, owner, fileSize, filePath);
            pendingFiles.Add(filePath);
            UpdateFileSizeLabel();
        }
        private void UpdateFileSizeLabel()
        {
            TotalSizelbl.Text = $"Tổng kích thước: {FormatFileSize(totalSizeBytes)}";
        }
        private void OnFileDeleted(string filePath)
        {
            if(pendingFiles.Contains(filePath))
            {
                FileInfo fi = new FileInfo(filePath);
                totalSizeBytes -= fi.Length;

                pendingFiles.Remove(filePath);
                UpdateFileSizeLabel();
            }
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
        private void AddHeaderRow()
        {
            Panel headerPanel = new Panel();
            headerPanel.Height = 30;
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Width = UploadFilePanel.Width;
            headerPanel.BackColor = Color.LightGray;

            Font headerFont = new Font("Segoe UI", 9.75F, FontStyle.Bold);

            Label lblFileName = new Label()
            {
                Text = "Tên file",
                Location = new Point(39, 5),
                Width = 180,
                Font = headerFont
            };

            Label lblOwner = new Label()
            {
                Text = "Chủ sở hữu",
                Location = new Point(248, 5),
                Width = 140,
                Font = headerFont
            };

            Label lblCreateAt = new Label()
            {
                Text = "Ngày upload",
                Location = new Point(420, 5),
                Width = 120,
                Font = headerFont
            };

            Label lblFileSize = new Label()
            {
                Text = "Dung lượng",
                Location = new Point(565, 5),
                Width = 130,
                Font = headerFont
            };

            Label lblFilePath = new Label()
            {
                Text = "Đường dẫn",
                Location = new Point(733, 5),
                Width = 400,
                Font = headerFont,
                AutoEllipsis = true
            };

            Label lblOption = new Label()
            {
                Text = "Tuỳ chọn",
                Location = new Point(1253, 5), // Khớp với btnMore
                Width = 80,
                Font = headerFont
            };

            // Thêm các label vào header panel
            headerPanel.Controls.Add(lblFileName);
            headerPanel.Controls.Add(lblOwner);
            headerPanel.Controls.Add(lblCreateAt);
            headerPanel.Controls.Add(lblFileSize);
            headerPanel.Controls.Add(lblFilePath);
            headerPanel.Controls.Add(lblOption);

            // Thêm headerPanel vào đầu danh sách
            UploadFilePanel.Controls.Add(headerPanel);
            UploadFilePanel.Controls.SetChildIndex(headerPanel, 0); // Đảm bảo nó nằm trên đầu
        }
    }
}
