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
            this.Account.Location = new System.Drawing.Point(1048, 29);
            this.Account.Name = "Account";
            this.Account.Size = new System.Drawing.Size(75, 23);
            this.Account.TabIndex = 12;
            this.Account.Text = "Account";
            this.Account.UseVisualStyleBackColor = true;
            // 
            // DashboardPanel
            // 
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
            // 
            // Settings_Dashboard
            // 
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
            // 
            // TrashBin_Dashboard
            // 
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
            // 
            // Upload_Dashboard
            // 
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
            // 
            // SharedWithMe_Dashboard
            // 
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
            // 
            // MyFile_Dashboard
            // 
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
            // 
            // NavbarPanel
            // 
            this.NavbarPanel.Controls.Add(this.Account);
            this.NavbarPanel.Location = new System.Drawing.Point(0, 0);
            this.NavbarPanel.Name = "NavbarPanel";
            this.NavbarPanel.Size = new System.Drawing.Size(1135, 82);
            this.NavbarPanel.TabIndex = 16;
            // 
            // MainContentPanel
            // 
            this.MainContentPanel.Location = new System.Drawing.Point(190, 88);
            this.MainContentPanel.Margin = new System.Windows.Forms.Padding(2);
            this.MainContentPanel.Name = "MainContentPanel";
            this.MainContentPanel.Size = new System.Drawing.Size(752, 492);
            this.MainContentPanel.TabIndex = 17;
            this.MainContentPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.UploadPanel_Paint);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(941, 572);
            this.Controls.Add(this.DashboardPanel);
            this.Controls.Add(this.NavbarPanel);
            this.Controls.Add(this.MainContentPanel);
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

