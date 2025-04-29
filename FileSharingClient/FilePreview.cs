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
using System.Data.SqlClient;
using System.Data.Common;
using System.Net.NetworkInformation;

namespace FileSharingClient
{
    public partial class FilePreview : UserControl
    {
        private static string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName;
        private static string dbPath = Path.Combine(projectRoot, "test.db");
        private static string connectionString = $"Data Source={dbPath};Version=3;Pooling=True";
        private int currentUserId = -1;

        public FilePreview()
        {
            InitializeComponent();
            _ = InitAsync();
        }

        private async Task InitAsync()
        {
            currentUserId = await GetUserIdFromSessionAsync();
            if (currentUserId != -1)
            {
                await LoadFileListAsync();
            }
        }

        private async Task<int> GetUserIdFromSessionAsync()
        {
            int userId = -1;

            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = "SELECT user_id FROM users WHERE username = @username";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", Session.LoggedInUser);
                        object result = await cmd.ExecuteScalarAsync();
                        if (result != null)
                        {
                            userId = Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting user_id: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return userId;
        }

        private async Task LoadFileListAsync()
        {

            try
            {
                cbBrowseFile.Items.Clear();
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = "SELECT file_name FROM files WHERE owner_id = @owner_Id";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@owner_Id", currentUserId);
                        using (DbDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string fileName = reader["file_name"].ToString();
                                cbBrowseFile.Items.Add(fileName);
                            }
                        }
                    }
                    string querySharedFiles = "SELECT f.file_name FROM files_share fs JOIN files f ON fs.file_id = f.file_id WHERE fs.user_id = @user_id";
                    using (SQLiteCommand cmd = new SQLiteCommand(querySharedFiles, conn))
                    {
                        cmd.Parameters.AddWithValue("@user_id", currentUserId);
                        using(DbDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while(await reader.ReadAsync())
                            {
                                string shareFileName = reader["file_name"].ToString();
                                cbBrowseFile.Items.Add(shareFileName);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách file: {ex.Message}");
            }
        }

        private async void btnPreview_Click(object sender, EventArgs e)
        {
            if (cbBrowseFile.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một file.");
                return;
            }

            string filename = cbBrowseFile.SelectedItem.ToString();

            try
            {
                using (var conn = new SQLiteConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"
                        SELECT f.file_path, f.file_type
                        FROM files f
                        WHERE f.file_name = @fileName
                        AND (
                            f.owner_id = @userId
                            OR f.file_id IN (
                                SELECT fs.file_id FROM files_share fs WHERE fs.user_id = @userId
                            )
                        )
                        LIMIT 1
                    ";
                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@fileName", filename);
                        cmd.Parameters.AddWithValue("@userId", currentUserId);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                string relativePath = reader["file_path"].ToString();
                                string fileType = reader["file_type"].ToString().ToLower();
                                string fullPath = Path.Combine(projectRoot, relativePath);

                                await ShowPreviewAsync(fullPath, fileType);
                            }
                            else
                            {
                                MessageBox.Show("Không tìm thấy đường dẫn file.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xem trước file: {ex.Message}");
            }
        }

        private async Task ShowPreviewAsync(string filePath, string fileType)
        {
            rtbContent.Visible = false;
            picImagePreview.Visible = false;
            wbPdfPreview.Visible = false;

            if (!File.Exists(filePath)) {
                MessageBox.Show("File khong ton tai");
                return;
            }

            if (fileType.Contains("text") || filePath.EndsWith(".txt"))
            {
                string content = await Task.Run(() => File.ReadAllText(filePath));
                rtbContent.Text = content;
                rtbContent.Visible = true;
            }

            else if (fileType.Contains("image") || filePath.EndsWith(".png") || filePath.EndsWith(".jpg") || filePath.EndsWith(".jpeg"))
            {
                picImagePreview.Image = Image.FromFile(filePath);
                picImagePreview.SizeMode = PictureBoxSizeMode.Zoom;
                picImagePreview.Visible = true;
            }

            else if(fileType.Contains("pdf") || filePath.EndsWith(".pdf"))
            {
                wbPdfPreview.Navigate(filePath);
                wbPdfPreview.Visible = true;
            }

            else
            {
                MessageBox.Show("Dinh dang file chua duoc ho tro");
            }
        }
    }
}
