using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSharingServer
{
    public static class AuthService
    {
        public static async Task<string> RegisterUser(string username, string email, string password)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();

                    // Kiểm tra username đã tồn tại chưa
                    string checkQuery = "SELECT COUNT(*) FROM users WHERE username = @username";
                    using (SQLiteCommand checkCmd = new SQLiteCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@username", username);
                        long userExists = Convert.ToInt64(await checkCmd.ExecuteScalarAsync());

                        if (userExists > 0)
                        {
                            return "409\n"; // Conflict: User đã tồn tại
                        }
                    }

                    // Thêm người dùng mới vào cơ sở dữ liệu
                    string insertQuery = "INSERT INTO users (username, email, password_hash) VALUES (@username, @email, @password_hash)";
                    using (SQLiteCommand insertCmd = new SQLiteCommand(insertQuery, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@username", username);
                        insertCmd.Parameters.AddWithValue("@email", email);
                        insertCmd.Parameters.AddWithValue("@password_hash", password);
                        await insertCmd.ExecuteNonQueryAsync();
                    }
                    return "201\n"; // Created: Đăng ký thành công
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi đăng ký: {ex.Message}");
                return "500\n"; // Internal Server Error
            }
        }
        public static async Task<string> LoginUser(string username, string password)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();
                    string query = "SELECT password_hash FROM users WHERE username = @username";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        string storedPassword = await cmd.ExecuteScalarAsync() as string;

                        if (storedPassword != null && storedPassword == password)
                        {
                            return "200\n"; // OK: Đăng nhập thành công
                        }
                        return "401\n"; // Unauthorized: Đăng nhập thất bại
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi đăng nhập: {ex.Message}");
                return "500\n"; // Internal server error
            }
        }
        public static async Task<string> ChangePassword(string username, string oldPassword, string newPassword)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(DatabaseHelper.connectionString))
                {
                    await conn.OpenAsync();

                    // Kiểm tra mật khẩu cũ
                    string checkQuery = "SELECT password_hash FROM users WHERE username = @username";
                    using (SQLiteCommand checkCmd = new SQLiteCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@username", username);
                        string storedPassword = await checkCmd.ExecuteScalarAsync() as string;

                        if (storedPassword != oldPassword)
                        {
                            return "401\n"; // Unauthorized: Mật khẩu cũ không chính xác
                        }
                    }

                    // Cập nhật mật khẩu mới
                    string updateQuery = "UPDATE users SET password_hash = @newPassword WHERE username = @username";
                    using (SQLiteCommand updateCmd = new SQLiteCommand(updateQuery, conn))
                    {
                        updateCmd.Parameters.AddWithValue("@username", username);
                        updateCmd.Parameters.AddWithValue("@newPassword", newPassword);
                        await updateCmd.ExecuteNonQueryAsync();
                    }
                    return "200\n"; // Đổi mật khẩu thành công
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi đổi mật khẩu: {ex.Message}");
                return "500\n"; // Internal Server Error
            }
        }
    }
}
