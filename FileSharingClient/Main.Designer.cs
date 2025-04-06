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
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.Account = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.File_Dashboard = new FontAwesome.Sharp.IconButton();
            this.Upload_DashBoard = new FontAwesome.Sharp.IconButton();
            this.panel2 = new System.Windows.Forms.Panel();
            this.upload_progress = new System.Windows.Forms.ProgressBar();
            this.btnSendFile = new System.Windows.Forms.Button();
            this.panelFile = new System.Windows.Forms.Panel();
            this.iconButton1 = new FontAwesome.Sharp.IconButton();
            this.lblFileExtension = new System.Windows.Forms.Label();
            this.lblFileSize = new System.Windows.Forms.Label();
            this.lblFileName = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panelFile.SuspendLayout();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // Account
            // 
            this.Account.Location = new System.Drawing.Point(1048, 29);
            this.Account.Name = "Account";
            this.Account.Size = new System.Drawing.Size(75, 23);
            this.Account.TabIndex = 12;
            this.Account.Text = "Account";
            this.Account.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.panel1.Controls.Add(this.File_Dashboard);
            this.panel1.Controls.Add(this.Upload_DashBoard);
            this.panel1.Location = new System.Drawing.Point(0, 80);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 492);
            this.panel1.TabIndex = 15;
            // 
            // File_Dashboard
            // 
            this.File_Dashboard.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.File_Dashboard.Dock = System.Windows.Forms.DockStyle.Top;
            this.File_Dashboard.FlatAppearance.BorderSize = 0;
            this.File_Dashboard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.File_Dashboard.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.File_Dashboard.IconChar = FontAwesome.Sharp.IconChar.File;
            this.File_Dashboard.IconColor = System.Drawing.Color.Black;
            this.File_Dashboard.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.File_Dashboard.IconSize = 40;
            this.File_Dashboard.Location = new System.Drawing.Point(0, 41);
            this.File_Dashboard.Name = "File_Dashboard";
            this.File_Dashboard.Size = new System.Drawing.Size(200, 41);
            this.File_Dashboard.TabIndex = 1;
            this.File_Dashboard.Text = "File";
            this.File_Dashboard.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.File_Dashboard.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.File_Dashboard.UseVisualStyleBackColor = false;
            // 
            // Upload_DashBoard
            // 
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
            this.Upload_DashBoard.Name = "Upload_DashBoard";
            this.Upload_DashBoard.Size = new System.Drawing.Size(200, 41);
            this.Upload_DashBoard.TabIndex = 0;
            this.Upload_DashBoard.Text = "Upload";
            this.Upload_DashBoard.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Upload_DashBoard.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.Upload_DashBoard.UseVisualStyleBackColor = false;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.Account);
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1135, 82);
            this.panel2.TabIndex = 16;
            // 
            // upload_progress
            // 
            this.upload_progress.Location = new System.Drawing.Point(462, 417);
            this.upload_progress.Name = "upload_progress";
            this.upload_progress.Size = new System.Drawing.Size(469, 23);
            this.upload_progress.TabIndex = 19;
            this.upload_progress.Visible = false;
            // 
            // btnSendFile
            // 
            this.btnSendFile.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSendFile.Location = new System.Drawing.Point(462, 138);
            this.btnSendFile.Name = "btnSendFile";
            this.btnSendFile.Size = new System.Drawing.Size(469, 104);
            this.btnSendFile.TabIndex = 20;
            this.btnSendFile.Text = "Nhấn vào đây để tải file lên";
            this.btnSendFile.UseVisualStyleBackColor = true;
            this.btnSendFile.Click += new System.EventHandler(this.btnSendFile_Click_1);
            // 
            // panelFile
            // 
            this.panelFile.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.panelFile.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.panelFile.Controls.Add(this.iconButton1);
            this.panelFile.Controls.Add(this.lblFileExtension);
            this.panelFile.Controls.Add(this.lblFileSize);
            this.panelFile.Controls.Add(this.lblFileName);
            this.panelFile.Location = new System.Drawing.Point(462, 373);
            this.panelFile.Name = "panelFile";
            this.panelFile.Size = new System.Drawing.Size(469, 25);
            this.panelFile.TabIndex = 21;
            this.panelFile.Visible = false;
            // 
            // iconButton1
            // 
            this.iconButton1.FlatAppearance.BorderSize = 0;
            this.iconButton1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.iconButton1.IconChar = FontAwesome.Sharp.IconChar.X;
            this.iconButton1.IconColor = System.Drawing.Color.Black;
            this.iconButton1.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.iconButton1.IconSize = 20;
            this.iconButton1.Location = new System.Drawing.Point(445, 1);
            this.iconButton1.Name = "iconButton1";
            this.iconButton1.Size = new System.Drawing.Size(24, 23);
            this.iconButton1.TabIndex = 3;
            this.iconButton1.UseVisualStyleBackColor = true;
            // 
            // lblFileExtension
            // 
            this.lblFileExtension.AutoSize = true;
            this.lblFileExtension.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileExtension.Location = new System.Drawing.Point(287, 0);
            this.lblFileExtension.Name = "lblFileExtension";
            this.lblFileExtension.Size = new System.Drawing.Size(100, 21);
            this.lblFileExtension.TabIndex = 2;
            this.lblFileExtension.Text = "FileExtension";
            // 
            // lblFileSize
            // 
            this.lblFileSize.AutoSize = true;
            this.lblFileSize.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileSize.Location = new System.Drawing.Point(137, 1);
            this.lblFileSize.Name = "lblFileSize";
            this.lblFileSize.Size = new System.Drawing.Size(62, 21);
            this.lblFileSize.TabIndex = 1;
            this.lblFileSize.Text = "FileSize";
            // 
            // lblFileName
            // 
            this.lblFileName.AutoSize = true;
            this.lblFileName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFileName.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileName.Location = new System.Drawing.Point(0, 0);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(76, 21);
            this.lblFileName.TabIndex = 0;
            this.lblFileName.Text = "FileName";
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1135, 570);
            this.Controls.Add(this.panelFile);
            this.Controls.Add(this.btnSendFile);
            this.Controls.Add(this.upload_progress);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "Main";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Text = "Form1";
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panelFile.ResumeLayout(false);
            this.panelFile.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button Account;
        private System.Windows.Forms.Panel panel1;
        private FontAwesome.Sharp.IconButton Upload_DashBoard;
        private FontAwesome.Sharp.IconButton File_Dashboard;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ProgressBar upload_progress;
        private System.Windows.Forms.Button btnSendFile;
        private System.Windows.Forms.Panel panelFile;
        private System.Windows.Forms.Label lblFileName;
        private System.Windows.Forms.Label lblFileSize;
        private FontAwesome.Sharp.IconButton iconButton1;
        private System.Windows.Forms.Label lblFileExtension;
    }
}

