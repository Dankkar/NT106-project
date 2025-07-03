using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileSharingClient
{
    public partial class FileItemControl : UserControl
    {
        public string FilePath { get; set; }
        public event Action<String> FileDeleted;
        public string FileName { get; set; }
        public string CreateAt { get; set; }
        public string Owner { get; set; }
        public string FileSize { get; set; }
        public FileItemControl(string filename, string createAt, string owner, string filesize, string filepath)
        {
            InitializeComponent();

            //Set thong tin file vao control
            this.FileName = filename;
            lblFileName.Text = filename;
            lblFileSize.Text = filesize;
            lblOwner.Text = owner;
            lblCreateAt.Text = createAt;
            FilePath = filepath;
            lblFilePath.Text = filepath;

            btnMore.Click += (s, e) => contextMenuStrip1.Show(btnMore, new Point(0, btnMore.Height));
        }



        private void shareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"Chia sẻ file {this.FileName}");
        }

        private async void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show($"Bạn có chắc muốn chuyển file '{this.FileName}' vào thùng rác?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                await MoveFileToTrash();
            }
        }

        private async Task MoveFileToTrash()
        {
            try
            {
                int fileId = await GetFileIdFromDatabase(this.FileName);
                if (fileId == -1)
                {
                    MessageBox.Show("Không tìm thấy file trong cơ sở dữ liệu!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                using (var client = new TcpClient("127.0.0.1", 5000))
                using (var stream = client.GetStream())
                using (var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    string command = $"MOVE_TO_TRASH|{fileId}|{Session.LoggedInUserId}\n";
                    await writer.WriteLineAsync(command);

                    string response = await reader.ReadLineAsync();
                    if (response?.Trim() == "200")
                    {
                        MessageBox.Show("File đã được chuyển vào thùng rác.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        FileDeleted?.Invoke(this.FilePath);
                        this.Dispose();
                    }
                    else
                    {
                        MessageBox.Show($"Không thể xóa file. Server phản hồi: {response}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa file: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<int> GetFileIdFromDatabase(string fileName)
        {
            try
            {
                string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName;
                string dbPath = Path.Combine(projectRoot, "test.db");
                string connectionString = $"Data Source={dbPath};Version=3;";

                using (var conn = new System.Data.SQLite.SQLiteConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = "SELECT file_id FROM files WHERE file_name = @fileName AND owner_id = @ownerId AND status = 'ACTIVE'";
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@fileName", fileName);
                        cmd.Parameters.AddWithValue("@ownerId", Session.LoggedInUserId);
                        var result = await cmd.ExecuteScalarAsync();
                        return result != null ? Convert.ToInt32(result) : -1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi truy vấn CSDL: {ex.Message}");
                return -1;
            }
        }
       

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }
    }
}
