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
using System.Drawing.Drawing2D;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using FontAwesome.Sharp;
using FileSharingClient;

namespace FileSharingClient
{
    public partial class Register : Form
    {
        private bool isLoginOpen = false;
        private string username = "Tên dang nh?p";
        private string password = "M?t kh?u";
        private string conf_pass = "Xác nh?n m?t kh?u";
        private string gmail = "Gmail";
        private const string SERVER_IP = "localhost";
        private const int SERVER_PORT = 5000;
        public Register()
        {
            InitializeComponent();
            usernametxtBox.Text = username;
            usernametxtBox.ForeColor = Color.Gray;
            usernametxtBox.Enter += usernametxtBox_Enter;
            usernametxtBox.Leave += usernametxtBox_Leave;

            passtxtBox.Text = password;
            passtxtBox.ForeColor = Color.Gray;
            passtxtBox.Enter += passtxtBox_Enter;
            passtxtBox.Leave += passtxtBox_Leave;

            confpasstxtBox.Text = conf_pass;
            confpasstxtBox.ForeColor = Color.Gray;
            confpasstxtBox.Enter += confpasstxtBox_Enter;
            confpasstxtBox.Leave += confpasstxtBox_Leave;

            gmailtxtBox.Text = gmail;
            gmailtxtBox.ForeColor = Color.Gray;
            gmailtxtBox.Enter += gmailtxtBox_Enter;
            gmailtxtBox.Leave += gmailtxtBox_Leave;
        }
        private void usernametxtBox_Enter(object sender, EventArgs e)
        {
            if (usernametxtBox.Text == username)
            {
                usernametxtBox.Text = "";
                usernametxtBox.ForeColor = Color.Black;
            }
        }
        private void usernametxtBox_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(usernametxtBox.Text))
            {
                usernametxtBox.Text = username;
                usernametxtBox.ForeColor = Color.Gray;
            }
        }
        private void passtxtBox_Enter(object sender, EventArgs e)
        {
            if (passtxtBox.Text == password)
            {
                passtxtBox.Text = "";
                passtxtBox.ForeColor = Color.Black;
                passtxtBox.UseSystemPasswordChar = true;
            }
        }
        private void passtxtBox_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(passtxtBox.Text))
            {
                passtxtBox.Text = password;
                passtxtBox.ForeColor = Color.Gray;
                passtxtBox.UseSystemPasswordChar = false;
            }
        }
        private void confpasstxtBox_Enter(object sender, EventArgs e)
        {
            if (confpasstxtBox.Text == conf_pass)
            {
                confpasstxtBox.Text = "";
                confpasstxtBox.ForeColor = Color.Black;
                confpasstxtBox.UseSystemPasswordChar = true;
            }
        }
        private void confpasstxtBox_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(confpasstxtBox.Text))
            {
                confpasstxtBox.Text = conf_pass;
                confpasstxtBox.ForeColor = Color.Gray;
                confpasstxtBox.UseSystemPasswordChar = false;
            }
        }
        private void gmailtxtBox_Enter(object sender, EventArgs e)
        {
            if (gmailtxtBox.Text == gmail)
            {
                gmailtxtBox.Text = "";
                gmailtxtBox.ForeColor = Color.Black;
            }
        }
        private void gmailtxtBox_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(gmailtxtBox.Text))
            {
                gmailtxtBox.Text = gmail;
                gmailtxtBox.ForeColor = Color.Gray;
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

            this.Tag = "login";
            this.Close();
          
        }

        private async void btnRegister_Click(object sender, EventArgs e)
        {
            await HandleRegister();
        }   
        private async Task HandleRegister()
        {
            try
            {
                // K?t n?i d?n server
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync(SERVER_IP, SERVER_PORT);
                using (sslStream)
                using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                {
                    // L?y thông tin t? các TextBox
                    string username = usernametxtBox.Text;
                    string email = gmailtxtBox.Text;
                    string password = passtxtBox.Text;
                    string conf_pass = confpasstxtBox.Text;

                    if (string.IsNullOrWhiteSpace(username) ||
                        string.IsNullOrWhiteSpace(password) ||
                        string.IsNullOrWhiteSpace(conf_pass) ||
                        string.IsNullOrWhiteSpace(email))
                    {
                        this.Invoke(new Action(() =>
                        {
                            MessageBox.Show("Vui lòng nh?p d?y d? thông tin", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }));
                        return;
                    }
                    if (conf_pass != password)
                    {
                        this.Invoke(new Action(() =>
                        {
                            MessageBox.Show("M?t kh?u và xác nh?n m?t kh?u không trùng kh?p!", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }));
                        return;
                    }
                    if(!IsValidEmail(email))
                    {
                        this.Invoke(new Action(() =>
                        {
                            MessageBox.Show("Vui lòng nh?p dúng d?nh d?ng email!", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }));
                        return;
                    }

                    // Hash password b?ng SHA256 tru?c khi g?i
                    string hashedPassword;
                    using (SHA256 sha256Hash = SHA256.Create())
                    {
                        byte[] data = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                        StringBuilder sb = new StringBuilder();
                        foreach (byte b in data)
                        {
                            sb.Append(b.ToString("x2"));
                        }
                        hashedPassword = sb.ToString();
                    }
                    
                    // G?i d? li?u dang ký theo d?nh d?ng: REGISTER|username|email|hashedPassword
                    string message = $"REGISTER|{username}|{email}|{hashedPassword}";
                    await writer.WriteLineAsync(message);

                    // Nh?n ph?n h?i t? server (status code d?ng s?)
                    string response = await reader.ReadLineAsync();
                    response = response?.Trim(); // c?t b? kho?ng tr?ng th?a, newline, ...
                    int statusCode;
                    if (int.TryParse(response, out statusCode))
                    {
                        this.Invoke(new Action(() =>
                        {
                            switch (statusCode)
                            {
                                case 201:
                                    MessageBox.Show("Ðang ký thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    this.Tag= "login"; // Ð?t tag d? bi?t form này dã dang ký thành công
                                    this.Close();
                                    break;
                                case 409:
                                    MessageBox.Show("Username dã t?n t?i!", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    break;
                                case 400:
                                    MessageBox.Show("Yêu c?u không h?p l?!", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    break;
                                case 500:
                                    MessageBox.Show("L?i t? server. Vui lòng th? l?i sau!", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    break;
                                default:
                                    MessageBox.Show("Ph?n h?i không xác d?nh t? server: " + statusCode, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    break;
                            }
                        }));
                    }
                    else
                    {
                        this.Invoke(new Action(() =>
                        {
                            MessageBox.Show("Ph?n h?i không h?p l? t? server: " + response, "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                this.Invoke(new Action(() =>
                {
                    MessageBox.Show("L?i k?t n?i d?n server: " + ex.Message, "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
           if(checkBox1.Checked == true)
            {
                passtxtBox.UseSystemPasswordChar = false;
                confpasstxtBox.UseSystemPasswordChar = false;
            }
           else
            {
                passtxtBox.UseSystemPasswordChar = true;
                confpasstxtBox.UseSystemPasswordChar = true;
            }
        }
        bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

    }
}
