using System;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace FileSharingClient
{
    public class SharePasswordForm : Form
    {
        private Label lblTitle;
        private Label lblPermission;
        private Label lblPassword;
        private TextBox txtPassword;
        private Button btnCopy;
        private Button btnClose;
        private Label lblNote;

        public SharePasswordForm(string name, string permission, string password, bool isFolder)
        {
            this.Text = "Chia sẻ thành công";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Width = 420;
            this.Height = 250;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;

            lblTitle = new Label
            {
                Text = (isFolder ? "Folder" : "File") + $" '{name}' đã được chia sẻ!",
                Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold),
                AutoSize = false,
                Width = 380,
                Height = 30,
                Top = 15,
                Left = 20
            };
            lblPermission = new Label
            {
                Text = $"Quyền chia sẻ: {permission}",
                Font = new System.Drawing.Font("Segoe UI", 10F),
                AutoSize = false,
                Width = 380,
                Height = 25,
                Top = 50,
                Left = 20
            };
            lblPassword = new Label
            {
                Text = "Mật khẩu chia sẻ:",
                Font = new System.Drawing.Font("Segoe UI", 10F),
                AutoSize = false,
                Width = 120,
                Height = 25,
                Top = 85,
                Left = 20
            };
            txtPassword = new TextBox
            {
                Text = password,
                ReadOnly = true,
                Width = 180,
                Height = 25,
                Top = 82,
                Left = 150,
                Font = new System.Drawing.Font("Segoe UI", 10F)
            };
            btnCopy = new Button
            {
                Text = "Copy",
                Width = 60,
                Height = 25,
                Top = 82,
                Left = 340
            };
            btnCopy.Click += (s, e) =>
            {
                Clipboard.SetText(txtPassword.Text);
                btnCopy.Text = "Đã copy!";
                Timer timer = new Timer { Interval = 1200 };
                timer.Tick += (s2, e2) => { btnCopy.Text = "Copy"; timer.Stop(); };
                timer.Start();
            };
            lblNote = new Label
            {
                Text = "* Hãy gửi mật khẩu này cho người nhận để họ truy cập.",
                Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic),
                ForeColor = System.Drawing.Color.Gray,
                AutoSize = false,
                Width = 380,
                Height = 30,
                Top = 120,
                Left = 20
            };
            btnClose = new Button
            {
                Text = "Đóng",
                Width = 80,
                Height = 30,
                Top = 160,
                Left = 160,
                DialogResult = DialogResult.OK
            };

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblPermission);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnCopy);
            this.Controls.Add(lblNote);
            this.Controls.Add(btnClose);
            this.AcceptButton = btnClose;
        }
    }
} 