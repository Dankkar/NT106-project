using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;



namespace FileSharingClient
{
    public partial class Register : Form
    {
        public Register()
        {
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Hide();
            Login login = new Login();
            login.ShowDialog();
            this.Show();
        }
        private Image LoadHighQualityImage(string path, int width, int height)
        {
            Bitmap originalImage = new Bitmap(path);
            Bitmap highQualityImage = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(highQualityImage))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawImage(originalImage, 0, 0, width, height);
            }
            return highQualityImage;
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            //Duong dan den file SQLite
            string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName;
            string dbPath = Path.Combine(projectRoot, "test.db");
            string connectionString = $"Data Source={dbPath};Version=3;";
            string username;
            string password;
            string conf_password;
            MessageBox.Show($"{dbPath}");
            if (!string.IsNullOrWhiteSpace(usernametxtBox.Text) && !string.IsNullOrWhiteSpace(passtxtBox.Text) && !string.IsNullOrWhiteSpace(confpasstxtBox.Text))
            {
                username = usernametxtBox.Text;
                password = passtxtBox.Text;
                conf_password = confpasstxtBox.Text;
                if(password == conf_password)
                {
                    using(SQLiteConnection conn = new SQLiteConnection(connectionString))
                    {
                        try
                        {
                            conn.Open();

                            //Kiem tra username da ton tai chua
                            string checkQuery = "SELECT COUNT(*) FROM users WHERE username = @username";
                            using (SQLiteCommand checkCmd = new SQLiteCommand(checkQuery, conn))
                            {
                                checkCmd.Parameters.AddWithValue("@username", username);
                                long userExists = (long)checkCmd.ExecuteScalar();

                                if (userExists > 0)
                                {
                                    MessageBox.Show("Tên người dùng đã tồn tại!");
                                    return;
                                }
                            }

                            //Them nguoi dung moi vao co so du lieu
                            string insertQuery = "INSERT INTO users (username, password_hash) VALUES (@username, @password_hash)";
                            using (SQLiteCommand insertCmd = new SQLiteCommand(insertQuery, conn))
                            {
                                insertCmd.Parameters.AddWithValue("@username", username);
                                insertCmd.Parameters.AddWithValue("@password_hash", password);
                                insertCmd.ExecuteNonQuery();
                            }

                            MessageBox.Show("Đăng ký thành công!");
                            usernametxtBox.Clear();
                            passtxtBox.Clear();
                            confpasstxtBox.Clear();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Lỗi: {ex.Message}");
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng không nhập dấu cách vào các ô và nhập đầy đủ các ô!");
                usernametxtBox.Clear();
                passtxtBox.Clear();
                confpasstxtBox.Clear();
                return;
            }
        }

        private void usernametxtBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Đảo trạng thái hiển thị mật khẩu
            if (passtxtBox.UseSystemPasswordChar)
            {
                passtxtBox.UseSystemPasswordChar = false;
                button1.Text = "hide password";
            }
            else
            {
                passtxtBox.UseSystemPasswordChar = true;
                button1.Text = "show password";
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
