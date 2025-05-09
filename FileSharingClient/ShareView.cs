using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.IO;
using System.Data.Common;

namespace FileSharingClient
{
    public partial class ShareView: UserControl
    {
        private static string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName;
        private static string dbPath = Path.Combine(projectRoot, "test.db");
        private static string connectionString = $"Data Source={dbPath};Version=3;Pooling=False";
        private bool isBusy = false;

        public ShareView()
        {
            InitializeComponent();
            LoadUserFiles();
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

        private async void LoadUserFiles()
        {
            int userId = await GetUserIdFromSessionAsync();
            if (userId != -1)
            {
                await LoadUploadedFilesAsync(userId);  // Load files for the logged-in user
            }
            else
            {
                MessageBox.Show("User information not found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadUploadedFilesAsync(int userID)
        {
            try
            {
                cbBrowseFile.Items.Clear();

                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    await conn.OpenAsync();

                    // Lay cac file cua nguoi dung tu bang files
                    string queryUserFiles = "SELECT file_name FROM files WHERE owner_id = @owner_id";
                    using (SQLiteCommand cmd = new SQLiteCommand(queryUserFiles, conn))
                    {
                        cmd.Parameters.AddWithValue("@owner_id", userID);

                        using(DbDataReader reader = await cmd.ExecuteReaderAsync())
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
                        cmd.Parameters.AddWithValue("@user_id", userID);
                        using (DbDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string sharedFileName = reader["file_name"].ToString();
                                cbBrowseFile.Items.Add(sharedFileName);
                            }
                        }
                    }

                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Loi tai danh sach file: {ex.Message}", "Loi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void ShareView_Load(object sender, EventArgs e)
        {
            PasswordPanel.Visible = false;
            LoadUserFiles();
        }

        private async void btnShare_Click(object sender, EventArgs e)
        {
            if (isBusy) return;
            isBusy = true;

            PasswordPanel.Visible = true;

            string selectedFile = cbBrowseFile.SelectedItem?.ToString();
            string sharePass = tbPassword.Text;

            if (string.IsNullOrEmpty(selectedFile))
            {
                MessageBox.Show("Vui lòng chọn file trước khi chia sẻ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                isBusy = false;
                return;
            }



            try
            {
                bool result = await UpdateFileShareStatusAsync(selectedFile, sharePass);
                if (result)
                {
                    MessageBox.Show("File đã được chia sẻ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Lỗi khi chia sẻ file!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                isBusy = false;
            }
        }

        private async Task<bool> UpdateFileShareStatusAsync(string fileName, string sharePass)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string updateQuery = "UPDATE files SET share_pass = @sharePass, is_shared = 1 WHERE file_name = @file_name";
                    using (SQLiteCommand cmd = new SQLiteCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@file_name", fileName);
                        cmd.Parameters.AddWithValue("@sharePass", sharePass);
                        int rowAffected = await cmd.ExecuteNonQueryAsync();

                        return rowAffected > 0;
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Loi khi cap nhat trang thai chia se: {ex.Message}", "Loi", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
        }

        private async void cbBrowseFile_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedFile = cbBrowseFile.SelectedItem.ToString();

            string sharePass = await GetSharePass(selectedFile);
            tbPassword.Text = sharePass;
        }

        private async Task<string> GetSharePass(string fileName)
        {
            string sharePass = string.Empty;

            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = "SELECT share_pass FROM files WHERE file_name = @file_name";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@file_name", fileName);
                        object result = await cmd.ExecuteScalarAsync();

                        if (result != null)
                        {
                            sharePass = result.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lấy share_pass: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return sharePass;
        }

        private async void btnGet_Click(object sender, EventArgs e)
        {
            string sharePass = tbInputPassword.Text;

            // Kiem tra neu mat khau trong
            if (string.IsNullOrEmpty(sharePass))
            {
                MessageBox.Show("Vui long nhap mat khau chia se", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Lay thong tin file_id va owner_id tu share_pass
            (int fileId, int ownerId) = await GetFileInfoFromSharePassAsync(sharePass);

            if(fileId == -1)
            {
                MessageBox.Show("Mat khau chia se khong hop le", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Lay user_id cua nguoi dung hien tai
            int userId = await GetUserIdFromSessionAsync();

            // Kiem tra neu client da la owner cua file, khong them ban ghi vao db
            if(userId == ownerId)
            {
                MessageBox.Show("Ban la chu so huu cua file nay. Khong can chia se lai.");
                return;
            }


            // Them ban ghi vao "files_share" de chia se file voi nguoi dung
            bool result = await AddFileShareEntryAsync(fileId, userId, sharePass);
            if (result)
            {
                MessageBox.Show("Ban da co quyen truy cap vao file nay", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadUserFiles();
            }
            else
            {
                MessageBox.Show("Loi khi chia se file 1", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Truy van thong tin file_od va owner_id tu mat khau chia se
        private async Task<(int, int)> GetFileInfoFromSharePassAsync(string sharePass)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = "SELECT file_id, owner_id, share_pass FROM files WHERE share_pass = @share_pass";
                    using(SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@share_pass", sharePass);
                        using(DbDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if(await reader.ReadAsync())
                            {
                                int fileId = Convert.ToInt32(reader["file_id"]);
                                int ownerId = Convert.ToInt32(reader["owner_id"]);
                                string dbSharePass = reader["share_pass"].ToString();

                                // Neu mat khau chia trong db trung voi mat khau nguoi dung nhap, tra ve file_id va owner_id
                                if(dbSharePass == sharePass)
                                {
                                    return (fileId, ownerId);
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Loi khi lay thong tin file: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return (-1, -1); // Tra ve (-1, -1_) neu khong tim thay hoac mat khau khong khop
        }

        // Them ban ghi vao "files_share" de chia se file voi nguoi dung
        private async Task<bool> AddFileShareEntryAsync(int fileId, int userId, string sharePass)
        {
            try
            {
                using(SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = "INSERT INTO files_share (file_id, user_id, share_pass) VALUES (@file_id, @user_id, @share_pass)";
                    using(SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@file_id", fileId);
                        cmd.Parameters.AddWithValue("@user_id", userId);
                        cmd.Parameters.AddWithValue("@share_pass", sharePass);

                        await cmd.ExecuteNonQueryAsync(); // Them thogn tin chia se vao co so du lieu
                    }
                }
                return true;
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Loi khi chia se file 2 {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
