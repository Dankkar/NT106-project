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

namespace FileSharingClient
{
    public partial class ShareView: UserControl
    {
        private static string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName;
        private static string dbPath = Path.Combine(projectRoot, "test.db");
        private static string connectionString = $"Data Source={dbPath};Version=3;";

        public ShareView()
        {
            InitializeComponent();
            LoadUploadedFiles(4);
        }

        private void LoadUploadedFiles(int userID)
        {
            try
            {
                cbBrowseFile.Items.Clear();

                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT file_name FROM files WHERE owner_id = @owner_id AND is_shared = 0";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@owner_id", userID);

                        using(SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string fileName = reader["file_name"].ToString();
                                cbBrowseFile.Items.Add(fileName);
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
        }

        private void btnShare_Click(object sender, EventArgs e)
        {
            PasswordPanel.Visible = true;

            string selectedFile = cbBrowseFile.SelectedItem.ToString();
            string sharePass = tbPassword.Text;

            if (UpdateFileShareStatus(selectedFile, sharePass))
            {
                MessageBox.Show("File đã được chia sẻ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Lỗi khi chia sẻ file!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool UpdateFileShareStatus(string fileName, string sharePass)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    string updateQuery = "UPDATE files SET share_pass = @sharePass, is_shared = 1 WHERE file_name = @file_name";
                    using (SQLiteCommand cmd = new SQLiteCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@file_name", fileName);
                        cmd.Parameters.AddWithValue("@sharePass", sharePass);
                        int rowAffected = cmd.ExecuteNonQuery();

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

        private void cbBrowseFile_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedFile = cbBrowseFile.SelectedItem.ToString();

            string sharePass = GetSharePass(selectedFile);
            tbPassword.Text = sharePass;
        }

        private string GetSharePass(string fileName)
        {
            string sharePass = string.Empty;

            try
            {
                using(SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT share_pass FROM files WHERE file_name = @file_name";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@file_name", fileName);

                        object result = cmd.ExecuteScalar();
                        if(result != null)
                        {
                            sharePass = result.ToString();
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Loi lay share_pass: {ex.Message}", "Loi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return sharePass;
        }
    }
}
