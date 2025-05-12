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
            this.PanelDashboard = new System.Windows.Forms.Panel();
            this.FilePreview_Dashboard = new FontAwesome.Sharp.IconButton();
            this.TrashBin_Dashboard = new FontAwesome.Sharp.IconButton();
            this.Upload_Dashboard = new FontAwesome.Sharp.IconButton();
            this.Share_Dashboard = new FontAwesome.Sharp.IconButton();
            this.MyFile_Dashboard = new FontAwesome.Sharp.IconButton();
            this.MainContentPanel = new System.Windows.Forms.Panel();
            this.PanelDashboard.SuspendLayout();
            this.SuspendLayout();
            // 
            // Account
            // 
            this.Account.Location = new System.Drawing.Point(24, 917);
            this.Account.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Account.Name = "Account";
            this.Account.Size = new System.Drawing.Size(144, 46);
            this.Account.TabIndex = 12;
            this.Account.Text = "Account";
            this.Account.UseVisualStyleBackColor = true;
            this.Account.Click += new System.EventHandler(this.Account_Click);
            // 
            // PanelDashboard
            // 
            this.PanelDashboard.BackColor = System.Drawing.Color.RosyBrown;
            this.PanelDashboard.Controls.Add(this.Account);
            this.PanelDashboard.Controls.Add(this.FilePreview_Dashboard);
            this.PanelDashboard.Controls.Add(this.TrashBin_Dashboard);
            this.PanelDashboard.Controls.Add(this.Upload_Dashboard);
            this.PanelDashboard.Controls.Add(this.Share_Dashboard);
            this.PanelDashboard.Controls.Add(this.MyFile_Dashboard);
            this.PanelDashboard.Dock = System.Windows.Forms.DockStyle.Left;
            this.PanelDashboard.Location = new System.Drawing.Point(0, 0);
            this.PanelDashboard.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.PanelDashboard.Name = "PanelDashboard";
            this.PanelDashboard.Size = new System.Drawing.Size(200, 985);
            this.PanelDashboard.TabIndex = 15;
            // 
            // FilePreview_Dashboard
            // 
            this.FilePreview_Dashboard.Dock = System.Windows.Forms.DockStyle.Top;
            this.FilePreview_Dashboard.FlatAppearance.BorderSize = 0;
            this.FilePreview_Dashboard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.FilePreview_Dashboard.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.FilePreview_Dashboard.ForeColor = System.Drawing.Color.White;
            this.FilePreview_Dashboard.IconChar = FontAwesome.Sharp.IconChar.FileCircleCheck;
            this.FilePreview_Dashboard.IconColor = System.Drawing.Color.White;
            this.FilePreview_Dashboard.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.FilePreview_Dashboard.Location = new System.Drawing.Point(0, 192);
            this.FilePreview_Dashboard.Name = "FilePreview_Dashboard";
            this.FilePreview_Dashboard.Size = new System.Drawing.Size(200, 47);
            this.FilePreview_Dashboard.TabIndex = 5;
            this.FilePreview_Dashboard.Text = "File Preview";
            this.FilePreview_Dashboard.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.FilePreview_Dashboard.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.FilePreview_Dashboard.UseVisualStyleBackColor = true;
            this.FilePreview_Dashboard.Click += new System.EventHandler(this.FilePreview_Dashboard_Click);
            // 
            // TrashBin_Dashboard
            // 
            this.TrashBin_Dashboard.Dock = System.Windows.Forms.DockStyle.Top;
            this.TrashBin_Dashboard.FlatAppearance.BorderSize = 0;
            this.TrashBin_Dashboard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.TrashBin_Dashboard.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.TrashBin_Dashboard.ForeColor = System.Drawing.Color.White;
            this.TrashBin_Dashboard.IconChar = FontAwesome.Sharp.IconChar.Trash;
            this.TrashBin_Dashboard.IconColor = System.Drawing.Color.White;
            this.TrashBin_Dashboard.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.TrashBin_Dashboard.Location = new System.Drawing.Point(0, 145);
            this.TrashBin_Dashboard.Name = "TrashBin_Dashboard";
            this.TrashBin_Dashboard.Size = new System.Drawing.Size(200, 47);
            this.TrashBin_Dashboard.TabIndex = 4;
            this.TrashBin_Dashboard.Text = "Trash Bin";
            this.TrashBin_Dashboard.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.TrashBin_Dashboard.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.TrashBin_Dashboard.UseVisualStyleBackColor = true;
            this.TrashBin_Dashboard.Click += new System.EventHandler(this.TrashBin_Dashboard_Click);
            // 
            // Upload_Dashboard
            // 
            this.Upload_Dashboard.Dock = System.Windows.Forms.DockStyle.Top;
            this.Upload_Dashboard.FlatAppearance.BorderSize = 0;
            this.Upload_Dashboard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Upload_Dashboard.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.Upload_Dashboard.ForeColor = System.Drawing.Color.White;
            this.Upload_Dashboard.IconChar = FontAwesome.Sharp.IconChar.Upload;
            this.Upload_Dashboard.IconColor = System.Drawing.Color.White;
            this.Upload_Dashboard.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.Upload_Dashboard.Location = new System.Drawing.Point(0, 98);
            this.Upload_Dashboard.Name = "Upload_Dashboard";
            this.Upload_Dashboard.Size = new System.Drawing.Size(200, 47);
            this.Upload_Dashboard.TabIndex = 3;
            this.Upload_Dashboard.Text = "Upload";
            this.Upload_Dashboard.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Upload_Dashboard.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.Upload_Dashboard.UseVisualStyleBackColor = true;
            this.Upload_Dashboard.Click += new System.EventHandler(this.Upload_Dashboard_Click);
            // 
            // Share_Dashboard
            // 
            this.Share_Dashboard.Dock = System.Windows.Forms.DockStyle.Top;
            this.Share_Dashboard.FlatAppearance.BorderSize = 0;
            this.Share_Dashboard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Share_Dashboard.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.Share_Dashboard.ForeColor = System.Drawing.Color.White;
            this.Share_Dashboard.IconChar = FontAwesome.Sharp.IconChar.Share;
            this.Share_Dashboard.IconColor = System.Drawing.Color.White;
            this.Share_Dashboard.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.Share_Dashboard.Location = new System.Drawing.Point(0, 49);
            this.Share_Dashboard.Name = "Share_Dashboard";
            this.Share_Dashboard.Size = new System.Drawing.Size(200, 49);
            this.Share_Dashboard.TabIndex = 2;
            this.Share_Dashboard.Text = "Share";
            this.Share_Dashboard.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Share_Dashboard.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.Share_Dashboard.UseVisualStyleBackColor = true;
            this.Share_Dashboard.Click += new System.EventHandler(this.Share_Dashboard_Click);
            // 
            // MyFile_Dashboard
            // 
            this.MyFile_Dashboard.BackColor = System.Drawing.Color.DodgerBlue;
            this.MyFile_Dashboard.Dock = System.Windows.Forms.DockStyle.Top;
            this.MyFile_Dashboard.FlatAppearance.BorderSize = 0;
            this.MyFile_Dashboard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.MyFile_Dashboard.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.MyFile_Dashboard.ForeColor = System.Drawing.Color.Black;
            this.MyFile_Dashboard.IconChar = FontAwesome.Sharp.IconChar.File;
            this.MyFile_Dashboard.IconColor = System.Drawing.Color.White;
            this.MyFile_Dashboard.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.MyFile_Dashboard.IconSize = 40;
            this.MyFile_Dashboard.Location = new System.Drawing.Point(0, 0);
            this.MyFile_Dashboard.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.MyFile_Dashboard.Name = "MyFile_Dashboard";
            this.MyFile_Dashboard.Size = new System.Drawing.Size(200, 49);
            this.MyFile_Dashboard.TabIndex = 1;
            this.MyFile_Dashboard.Text = "My File";
            this.MyFile_Dashboard.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MyFile_Dashboard.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.MyFile_Dashboard.UseVisualStyleBackColor = false;
            this.MyFile_Dashboard.Click += new System.EventHandler(this.MyFile_Dashboard_Click);
            // 
            // MainContentPanel
            // 
            this.MainContentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainContentPanel.Location = new System.Drawing.Point(200, 0);
            this.MainContentPanel.Name = "MainContentPanel";
            this.MainContentPanel.Size = new System.Drawing.Size(1704, 985);
            this.MainContentPanel.TabIndex = 17;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.AntiqueWhite;
            this.ClientSize = new System.Drawing.Size(1904, 985);
            this.Controls.Add(this.MainContentPanel);
            this.Controls.Add(this.PanelDashboard);
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Name = "Main";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Main_Load);
            this.PanelDashboard.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button Account;
        private System.Windows.Forms.Panel PanelDashboard;
        private FontAwesome.Sharp.IconButton MyFile_Dashboard;
        private FontAwesome.Sharp.IconButton Upload_Dashboard;
        private FontAwesome.Sharp.IconButton Share_Dashboard;
        private FontAwesome.Sharp.IconButton FilePreview_Dashboard;
        private FontAwesome.Sharp.IconButton TrashBin_Dashboard;
        private System.Windows.Forms.Panel MainContentPanel;
    }
}

