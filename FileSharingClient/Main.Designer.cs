namespace FileSharingClient
{
    partial class Main
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
            this.Account = new System.Windows.Forms.Button();
            this.DashboardPanel = new System.Windows.Forms.Panel();
            this.Settings_Dashboard = new FontAwesome.Sharp.IconButton();
            this.TrashBin_Dashboard = new FontAwesome.Sharp.IconButton();
            this.Upload_Dashboard = new FontAwesome.Sharp.IconButton();
            this.SharedWithMe_Dashboard = new FontAwesome.Sharp.IconButton();
            this.MyFile_Dashboard = new FontAwesome.Sharp.IconButton();
            this.NavbarPanel = new System.Windows.Forms.Panel();
            this.MainContentPanel = new System.Windows.Forms.Panel();
            this.DashboardPanel.SuspendLayout();
            this.NavbarPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // Account
            // 
            this.Account.Location = new System.Drawing.Point(1572, 45);
            this.Account.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Account.Name = "Account";
            this.Account.Size = new System.Drawing.Size(112, 35);
            this.Account.TabIndex = 12;
            this.Account.Text = "Account";
            this.Account.UseVisualStyleBackColor = true;
            this.Account.Click += new System.EventHandler(this.Account_Click);
            // 
            // DashboardPanel
            // 
<<<<<<< HEAD
            this.DashboardPanel.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.DashboardPanel.Controls.Add(this.Settings_Dashboard);
            this.DashboardPanel.Controls.Add(this.TrashBin_Dashboard);
            this.DashboardPanel.Controls.Add(this.Upload_Dashboard);
            this.DashboardPanel.Controls.Add(this.SharedWithMe_Dashboard);
            this.DashboardPanel.Controls.Add(this.MyFile_Dashboard);
            this.DashboardPanel.Location = new System.Drawing.Point(12, 88);
            this.DashboardPanel.Name = "DashboardPanel";
            this.DashboardPanel.Size = new System.Drawing.Size(160, 492);
            this.DashboardPanel.TabIndex = 15;
=======
            this.panel1.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.panel1.Controls.Add(this.File_Dashboard);
            this.panel1.Controls.Add(this.Upload_DashBoard);
            this.panel1.Location = new System.Drawing.Point(0, 123);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(300, 757);
            this.panel1.TabIndex = 15;
>>>>>>> fbd78884b7b4625040ce7540e7aa773ba12cccd1
            // 
            // Settings_Dashboard
            // 
<<<<<<< HEAD
            this.Settings_Dashboard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Settings_Dashboard.Font = new System.Drawing.Font("Segoe UI", 14.25F);
            this.Settings_Dashboard.IconChar = FontAwesome.Sharp.IconChar.None;
            this.Settings_Dashboard.IconColor = System.Drawing.Color.Black;
            this.Settings_Dashboard.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.Settings_Dashboard.Location = new System.Drawing.Point(0, 181);
            this.Settings_Dashboard.Name = "Settings_Dashboard";
            this.Settings_Dashboard.Size = new System.Drawing.Size(160, 42);
            this.Settings_Dashboard.TabIndex = 5;
            this.Settings_Dashboard.Text = "Settings";
            this.Settings_Dashboard.UseVisualStyleBackColor = true;
=======
            this.File_Dashboard.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.File_Dashboard.Dock = System.Windows.Forms.DockStyle.Top;
            this.File_Dashboard.FlatAppearance.BorderSize = 0;
            this.File_Dashboard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.File_Dashboard.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.File_Dashboard.IconChar = FontAwesome.Sharp.IconChar.File;
            this.File_Dashboard.IconColor = System.Drawing.Color.Black;
            this.File_Dashboard.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.File_Dashboard.IconSize = 40;
            this.File_Dashboard.Location = new System.Drawing.Point(0, 63);
            this.File_Dashboard.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.File_Dashboard.Name = "File_Dashboard";
            this.File_Dashboard.Size = new System.Drawing.Size(300, 63);
            this.File_Dashboard.TabIndex = 1;
            this.File_Dashboard.Text = "File";
            this.File_Dashboard.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.File_Dashboard.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.File_Dashboard.UseVisualStyleBackColor = false;
>>>>>>> fbd78884b7b4625040ce7540e7aa773ba12cccd1
            // 
            // TrashBin_Dashboard
            // 
<<<<<<< HEAD
            this.TrashBin_Dashboard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.TrashBin_Dashboard.Font = new System.Drawing.Font("Segoe UI", 14.25F);
            this.TrashBin_Dashboard.IconChar = FontAwesome.Sharp.IconChar.None;
            this.TrashBin_Dashboard.IconColor = System.Drawing.Color.Black;
            this.TrashBin_Dashboard.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.TrashBin_Dashboard.Location = new System.Drawing.Point(0, 142);
            this.TrashBin_Dashboard.Name = "TrashBin_Dashboard";
            this.TrashBin_Dashboard.Size = new System.Drawing.Size(160, 41);
            this.TrashBin_Dashboard.TabIndex = 4;
            this.TrashBin_Dashboard.Text = "Trash Bin";
            this.TrashBin_Dashboard.UseVisualStyleBackColor = true;
=======
            this.Upload_DashBoard.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.Upload_DashBoard.Dock = System.Windows.Forms.DockStyle.Top;
            this.Upload_DashBoard.Enabled = false;
            this.Upload_DashBoard.FlatAppearance.BorderSize = 0;
            this.Upload_DashBoard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Upload_DashBoard.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Upload_DashBoard.IconChar = FontAwesome.Sharp.IconChar.Upload;
            this.Upload_DashBoard.IconColor = System.Drawing.Color.Black;
            this.Upload_DashBoard.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.Upload_DashBoard.IconSize = 40;
            this.Upload_DashBoard.Location = new System.Drawing.Point(0, 0);
            this.Upload_DashBoard.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Upload_DashBoard.Name = "Upload_DashBoard";
            this.Upload_DashBoard.Size = new System.Drawing.Size(300, 63);
            this.Upload_DashBoard.TabIndex = 0;
            this.Upload_DashBoard.Text = "Upload";
            this.Upload_DashBoard.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Upload_DashBoard.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.Upload_DashBoard.UseVisualStyleBackColor = false;
>>>>>>> fbd78884b7b4625040ce7540e7aa773ba12cccd1
            // 
            // Upload_Dashboard
            // 
<<<<<<< HEAD
            this.Upload_Dashboard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Upload_Dashboard.Font = new System.Drawing.Font("Segoe UI", 14.25F);
            this.Upload_Dashboard.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Upload_Dashboard.IconChar = FontAwesome.Sharp.IconChar.None;
            this.Upload_Dashboard.IconColor = System.Drawing.Color.Black;
            this.Upload_Dashboard.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.Upload_Dashboard.Location = new System.Drawing.Point(0, 96);
            this.Upload_Dashboard.Name = "Upload_Dashboard";
            this.Upload_Dashboard.Size = new System.Drawing.Size(160, 50);
            this.Upload_Dashboard.TabIndex = 3;
            this.Upload_Dashboard.Text = "Upload";
            this.Upload_Dashboard.UseVisualStyleBackColor = true;
            this.Upload_Dashboard.Click += new System.EventHandler(this.Upload_Dashboard_Click);
=======
            this.panel2.Controls.Add(this.Account);
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1702, 126);
            this.panel2.TabIndex = 16;
>>>>>>> fbd78884b7b4625040ce7540e7aa773ba12cccd1
            // 
            // SharedWithMe_Dashboard
            // 
<<<<<<< HEAD
            this.SharedWithMe_Dashboard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SharedWithMe_Dashboard.Font = new System.Drawing.Font("Segoe UI", 14.25F);
            this.SharedWithMe_Dashboard.IconChar = FontAwesome.Sharp.IconChar.None;
            this.SharedWithMe_Dashboard.IconColor = System.Drawing.Color.Black;
            this.SharedWithMe_Dashboard.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.SharedWithMe_Dashboard.Location = new System.Drawing.Point(0, 47);
            this.SharedWithMe_Dashboard.Name = "SharedWithMe_Dashboard";
            this.SharedWithMe_Dashboard.Size = new System.Drawing.Size(160, 53);
            this.SharedWithMe_Dashboard.TabIndex = 2;
            this.SharedWithMe_Dashboard.Text = "Shared With Me";
            this.SharedWithMe_Dashboard.UseVisualStyleBackColor = true;
            this.SharedWithMe_Dashboard.Click += new System.EventHandler(this.SharedWithMe_Dashboard_Click);
=======
            this.upload_progress.Location = new System.Drawing.Point(693, 642);
            this.upload_progress.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.upload_progress.Name = "upload_progress";
            this.upload_progress.Size = new System.Drawing.Size(704, 35);
            this.upload_progress.TabIndex = 19;
            this.upload_progress.Visible = false;
>>>>>>> fbd78884b7b4625040ce7540e7aa773ba12cccd1
            // 
            // MyFile_Dashboard
            // 
<<<<<<< HEAD
            this.MyFile_Dashboard.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.MyFile_Dashboard.Dock = System.Windows.Forms.DockStyle.Top;
            this.MyFile_Dashboard.FlatAppearance.BorderSize = 0;
            this.MyFile_Dashboard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.MyFile_Dashboard.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MyFile_Dashboard.IconChar = FontAwesome.Sharp.IconChar.File;
            this.MyFile_Dashboard.IconColor = System.Drawing.Color.Black;
            this.MyFile_Dashboard.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.MyFile_Dashboard.IconSize = 40;
            this.MyFile_Dashboard.Location = new System.Drawing.Point(0, 0);
            this.MyFile_Dashboard.Name = "MyFile_Dashboard";
            this.MyFile_Dashboard.Size = new System.Drawing.Size(160, 48);
            this.MyFile_Dashboard.TabIndex = 1;
            this.MyFile_Dashboard.Text = "My File";
            this.MyFile_Dashboard.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MyFile_Dashboard.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.MyFile_Dashboard.UseVisualStyleBackColor = false;
            this.MyFile_Dashboard.Click += new System.EventHandler(this.File_Dashboard_Click);
=======
            this.btnSendFile.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSendFile.Location = new System.Drawing.Point(693, 212);
            this.btnSendFile.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnSendFile.Name = "btnSendFile";
            this.btnSendFile.Size = new System.Drawing.Size(704, 160);
            this.btnSendFile.TabIndex = 20;
            this.btnSendFile.Text = "Nhấn vào đây để tải file lên";
            this.btnSendFile.UseVisualStyleBackColor = true;
            this.btnSendFile.Click += new System.EventHandler(this.btnSendFile_Click_1);
>>>>>>> fbd78884b7b4625040ce7540e7aa773ba12cccd1
            // 
            // NavbarPanel
            // 
<<<<<<< HEAD
            this.NavbarPanel.Controls.Add(this.Account);
            this.NavbarPanel.Location = new System.Drawing.Point(0, 0);
            this.NavbarPanel.Name = "NavbarPanel";
            this.NavbarPanel.Size = new System.Drawing.Size(1135, 82);
            this.NavbarPanel.TabIndex = 16;
=======
            this.panelFile.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.panelFile.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.panelFile.Controls.Add(this.iconButton1);
            this.panelFile.Controls.Add(this.lblFileExtension);
            this.panelFile.Controls.Add(this.lblFileSize);
            this.panelFile.Controls.Add(this.lblFileName);
            this.panelFile.Location = new System.Drawing.Point(693, 574);
            this.panelFile.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panelFile.Name = "panelFile";
            this.panelFile.Size = new System.Drawing.Size(704, 38);
            this.panelFile.TabIndex = 21;
            this.panelFile.Visible = false;
>>>>>>> fbd78884b7b4625040ce7540e7aa773ba12cccd1
            // 
            // MainContentPanel
            // 
<<<<<<< HEAD
            this.MainContentPanel.Location = new System.Drawing.Point(190, 88);
            this.MainContentPanel.Margin = new System.Windows.Forms.Padding(2);
            this.MainContentPanel.Name = "MainContentPanel";
            this.MainContentPanel.Size = new System.Drawing.Size(752, 492);
            this.MainContentPanel.TabIndex = 17;
            this.MainContentPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.UploadPanel_Paint);
=======
            this.iconButton1.FlatAppearance.BorderSize = 0;
            this.iconButton1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.iconButton1.IconChar = FontAwesome.Sharp.IconChar.X;
            this.iconButton1.IconColor = System.Drawing.Color.Black;
            this.iconButton1.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.iconButton1.IconSize = 20;
            this.iconButton1.Location = new System.Drawing.Point(668, 2);
            this.iconButton1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.iconButton1.Name = "iconButton1";
            this.iconButton1.Size = new System.Drawing.Size(36, 35);
            this.iconButton1.TabIndex = 3;
            this.iconButton1.UseVisualStyleBackColor = true;
            // 
            // lblFileExtension
            // 
            this.lblFileExtension.AutoSize = true;
            this.lblFileExtension.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileExtension.Location = new System.Drawing.Point(430, 0);
            this.lblFileExtension.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFileExtension.Name = "lblFileExtension";
            this.lblFileExtension.Size = new System.Drawing.Size(153, 32);
            this.lblFileExtension.TabIndex = 2;
            this.lblFileExtension.Text = "FileExtension";
            // 
            // lblFileSize
            // 
            this.lblFileSize.AutoSize = true;
            this.lblFileSize.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileSize.Location = new System.Drawing.Point(206, 2);
            this.lblFileSize.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFileSize.Name = "lblFileSize";
            this.lblFileSize.Size = new System.Drawing.Size(94, 32);
            this.lblFileSize.TabIndex = 1;
            this.lblFileSize.Text = "FileSize";
            // 
            // lblFileName
            // 
            this.lblFileName.AutoSize = true;
            this.lblFileName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFileName.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileName.Location = new System.Drawing.Point(0, 0);
            this.lblFileName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(115, 32);
            this.lblFileName.TabIndex = 0;
            this.lblFileName.Text = "FileName";
>>>>>>> fbd78884b7b4625040ce7540e7aa773ba12cccd1
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
<<<<<<< HEAD
            this.ClientSize = new System.Drawing.Size(941, 572);
            this.Controls.Add(this.DashboardPanel);
            this.Controls.Add(this.NavbarPanel);
            this.Controls.Add(this.MainContentPanel);
=======
            this.ClientSize = new System.Drawing.Size(1702, 877);
            this.Controls.Add(this.panelFile);
            this.Controls.Add(this.btnSendFile);
            this.Controls.Add(this.upload_progress);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
>>>>>>> fbd78884b7b4625040ce7540e7aa773ba12cccd1
            this.Name = "Main";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Text = "Form1";
            this.DashboardPanel.ResumeLayout(false);
            this.NavbarPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button Account;
        private System.Windows.Forms.Panel DashboardPanel;
        private FontAwesome.Sharp.IconButton MyFile_Dashboard;
        private System.Windows.Forms.Panel NavbarPanel;
        private System.Windows.Forms.Panel MainContentPanel;
        private FontAwesome.Sharp.IconButton SharedWithMe_Dashboard;
        private FontAwesome.Sharp.IconButton Upload_Dashboard;
        private FontAwesome.Sharp.IconButton TrashBin_Dashboard;
        private FontAwesome.Sharp.IconButton Settings_Dashboard;
    }
}

