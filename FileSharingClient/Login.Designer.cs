namespace FileSharingClient
{
    partial class Login
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Login));
            this.label3 = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.linkForgotPassword = new System.Windows.Forms.LinkLabel();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.LoginPanel = new Guna.UI2.WinForms.Guna2Panel();
            this.showPassword = new Guna.UI2.WinForms.Guna2ImageCheckBox();
            this.usernametxtBox = new Guna.UI2.WinForms.Guna2TextBox();
            this.passtxtBox = new Guna.UI2.WinForms.Guna2TextBox();
            this.btnLogin = new Guna.UI2.WinForms.Guna2Button();
            this.LoginPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.label3.Location = new System.Drawing.Point(78, 324);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(138, 15);
            this.label3.TabIndex = 5;
            this.label3.Text = "Bạn chưa có tài khoản ?";
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.linkLabel1.LinkColor = System.Drawing.Color.RoyalBlue;
            this.linkLabel1.Location = new System.Drawing.Point(222, 324);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(80, 15);
            this.linkLabel1.TabIndex = 6;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Đăng ký ngay";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // linkForgotPassword
            // 
            this.linkForgotPassword.AutoSize = true;
            this.linkForgotPassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.linkForgotPassword.LinkColor = System.Drawing.Color.RoyalBlue;
            this.linkForgotPassword.Location = new System.Drawing.Point(132, 350);
            this.linkForgotPassword.Name = "linkForgotPassword";
            this.linkForgotPassword.Size = new System.Drawing.Size(101, 15);
            this.linkForgotPassword.TabIndex = 10;
            this.linkForgotPassword.TabStop = true;
            this.linkForgotPassword.Text = "Quên mật khẩu ?";
            this.linkForgotPassword.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkForgotPassword_LinkClicked);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.label2.Location = new System.Drawing.Point(130, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(118, 30);
            this.label2.TabIndex = 8;
            this.label2.Text = "Đăng nhập";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 20F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.RoyalBlue;
            this.label1.Location = new System.Drawing.Point(74, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(217, 37);
            this.label1.TabIndex = 7;
            this.label1.Text = "File Sharing App";
            // 
            // LoginPanel
            // 
            this.LoginPanel.BorderColor = System.Drawing.Color.White;
            this.LoginPanel.BorderThickness = 1;
            this.LoginPanel.Controls.Add(this.showPassword);
            this.LoginPanel.Controls.Add(this.usernametxtBox);
            this.LoginPanel.Controls.Add(this.passtxtBox);
            this.LoginPanel.Controls.Add(this.btnLogin);
            this.LoginPanel.Controls.Add(this.linkForgotPassword);
            this.LoginPanel.Controls.Add(this.linkLabel1);
            this.LoginPanel.Controls.Add(this.label1);
            this.LoginPanel.Controls.Add(this.label2);
            this.LoginPanel.Controls.Add(this.label3);
            this.LoginPanel.Location = new System.Drawing.Point(230, 57);
            this.LoginPanel.Margin = new System.Windows.Forms.Padding(2);
            this.LoginPanel.Name = "LoginPanel";
            this.LoginPanel.Size = new System.Drawing.Size(370, 411);
            this.LoginPanel.TabIndex = 11;
            // 
            // showPassword
            // 
            this.showPassword.BackgroundImage = global::FileSharingClient.Properties.Resources.rsz_download;
            this.showPassword.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.showPassword.CheckedState.Image = ((System.Drawing.Image)(resources.GetObject("resource.Image")));
            this.showPassword.Image = ((System.Drawing.Image)(resources.GetObject("showPassword.Image")));
            this.showPassword.ImageOffset = new System.Drawing.Point(0, 0);
            this.showPassword.ImageRotate = 0F;
            this.showPassword.Location = new System.Drawing.Point(291, 189);
            this.showPassword.Name = "showPassword";
            this.showPassword.Size = new System.Drawing.Size(24, 24);
            this.showPassword.TabIndex = 16;
            this.showPassword.CheckedChanged += new System.EventHandler(this.showPassword_CheckedChanged);
            // 
            // usernametxtBox
            // 
            this.usernametxtBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.usernametxtBox.DefaultText = "";
            this.usernametxtBox.DisabledState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(208)))), ((int)(((byte)(208)))));
            this.usernametxtBox.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(226)))), ((int)(((byte)(226)))));
            this.usernametxtBox.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.usernametxtBox.DisabledState.PlaceholderForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.usernametxtBox.FocusedState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.usernametxtBox.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.usernametxtBox.HoverState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.usernametxtBox.Location = new System.Drawing.Point(60, 145);
            this.usernametxtBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.usernametxtBox.Name = "usernametxtBox";
            this.usernametxtBox.PlaceholderText = "Tên đăng nhập";
            this.usernametxtBox.SelectedText = "";
            this.usernametxtBox.Size = new System.Drawing.Size(265, 21);
            this.usernametxtBox.TabIndex = 14;
            this.usernametxtBox.Enter += new System.EventHandler(this.usernametxtBox_Enter);
            this.usernametxtBox.Leave += new System.EventHandler(this.usernametxtBox_Leave);
            // 
            // passtxtBox
            // 
            this.passtxtBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.passtxtBox.DefaultText = "";
            this.passtxtBox.DisabledState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(208)))), ((int)(((byte)(208)))));
            this.passtxtBox.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(226)))), ((int)(((byte)(226)))));
            this.passtxtBox.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.passtxtBox.DisabledState.PlaceholderForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.passtxtBox.FocusedState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.passtxtBox.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.passtxtBox.HoverState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.passtxtBox.Location = new System.Drawing.Point(60, 189);
            this.passtxtBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.passtxtBox.Name = "passtxtBox";
            this.passtxtBox.PlaceholderText = "";
            this.passtxtBox.SelectedText = "";
            this.passtxtBox.Size = new System.Drawing.Size(265, 21);
            this.passtxtBox.TabIndex = 13;
            this.passtxtBox.UseSystemPasswordChar = true;
            this.passtxtBox.Enter += new System.EventHandler(this.passtxtBox_Enter);
            this.passtxtBox.Leave += new System.EventHandler(this.passtxtBox_Leave);
            // 
            // btnLogin
            // 
            this.btnLogin.Animated = true;
            this.btnLogin.AutoRoundedCorners = true;
            this.btnLogin.BorderRadius = 18;
            this.btnLogin.DisabledState.BorderColor = System.Drawing.Color.DarkGray;
            this.btnLogin.DisabledState.CustomBorderColor = System.Drawing.Color.DarkGray;
            this.btnLogin.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(169)))), ((int)(((byte)(169)))), ((int)(((byte)(169)))));
            this.btnLogin.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(141)))), ((int)(((byte)(141)))));
            this.btnLogin.FillColor = System.Drawing.SystemColors.Highlight;
            this.btnLogin.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.btnLogin.ForeColor = System.Drawing.Color.White;
            this.btnLogin.Location = new System.Drawing.Point(60, 255);
            this.btnLogin.Margin = new System.Windows.Forms.Padding(2);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(265, 38);
            this.btnLogin.TabIndex = 11;
            this.btnLogin.Text = "Đăng nhập";
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // Login
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.BackgroundImage = global::FileSharingClient.Properties.Resources.blue_background;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(815, 545);
            this.Controls.Add(this.LoginPanel);
            this.Name = "Login";
            this.Text = "Login";
            this.LoginPanel.ResumeLayout(false);
            this.LoginPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.LinkLabel linkForgotPassword;
        private Guna.UI2.WinForms.Guna2Panel LoginPanel;
        private Guna.UI2.WinForms.Guna2Button btnLogin;
        private Guna.UI2.WinForms.Guna2TextBox passtxtBox;
        private Guna.UI2.WinForms.Guna2TextBox usernametxtBox;
        private Guna.UI2.WinForms.Guna2ImageCheckBox showPassword;
    }
}