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
            this.Settings_Dashboard = new FontAwesome.Sharp.IconButton();
            this.TrashBin_Dashboard = new FontAwesome.Sharp.IconButton();
            this.Upload_Dashboard = new FontAwesome.Sharp.IconButton();
            this.Share_Dashboard = new FontAwesome.Sharp.IconButton();
            this.MyFile_Dashboard = new FontAwesome.Sharp.IconButton();
            this.NavBarPanel = new System.Windows.Forms.Panel();
            this.MainContentPanel = new System.Windows.Forms.Panel();
            this.PanelDashboard.SuspendLayout();
            this.NavBarPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // Account
            // 
            this.Account.Location = new System.Drawing.Point(1332, 52);
            this.Account.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.Account.Name = "Account";
            this.Account.Size = new System.Drawing.Size(148, 44);
            this.Account.TabIndex = 12;
            this.Account.Text = "Account";
            this.Account.UseVisualStyleBackColor = true;
            this.Account.Click += new System.EventHandler(this.Account_Click);
            // 
            // PanelDashboard
            // 
            this.PanelDashboard.BackColor = System.Drawing.Color.RosyBrown;
            this.PanelDashboard.Controls.Add(this.Settings_Dashboard);
            this.PanelDashboard.Controls.Add(this.TrashBin_Dashboard);
            this.PanelDashboard.Controls.Add(this.Upload_Dashboard);
            this.PanelDashboard.Controls.Add(this.Share_Dashboard);
            this.PanelDashboard.Controls.Add(this.MyFile_Dashboard);
            this.PanelDashboard.Location = new System.Drawing.Point(0, 169);
            this.PanelDashboard.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.PanelDashboard.Name = "PanelDashboard";
            this.PanelDashboard.Size = new System.Drawing.Size(400, 754);
            this.PanelDashboard.TabIndex = 15;
            // 
            // Settings_Dashboard
            // 
            this.Settings_Dashboard.Dock = System.Windows.Forms.DockStyle.Top;
            this.Settings_Dashboard.FlatAppearance.BorderSize = 0;
            this.Settings_Dashboard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Settings_Dashboard.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.Settings_Dashboard.ForeColor = System.Drawing.Color.White;
            this.Settings_Dashboard.IconChar = FontAwesome.Sharp.IconChar.Shapes;
            this.Settings_Dashboard.IconColor = System.Drawing.Color.White;
            this.Settings_Dashboard.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.Settings_Dashboard.Location = new System.Drawing.Point(0, 368);
            this.Settings_Dashboard.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Settings_Dashboard.Name = "Settings_Dashboard";
            this.Settings_Dashboard.Size = new System.Drawing.Size(400, 90);
            this.Settings_Dashboard.TabIndex = 5;
            this.Settings_Dashboard.Text = "Settings";
            this.Settings_Dashboard.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Settings_Dashboard.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.Settings_Dashboard.UseVisualStyleBackColor = true;
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
            this.TrashBin_Dashboard.Location = new System.Drawing.Point(0, 278);
            this.TrashBin_Dashboard.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.TrashBin_Dashboard.Name = "TrashBin_Dashboard";
            this.TrashBin_Dashboard.Size = new System.Drawing.Size(400, 90);
            this.TrashBin_Dashboard.TabIndex = 4;
            this.TrashBin_Dashboard.Text = "Trash Bin";
            this.TrashBin_Dashboard.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.TrashBin_Dashboard.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.TrashBin_Dashboard.UseVisualStyleBackColor = true;
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
            this.Upload_Dashboard.Location = new System.Drawing.Point(0, 188);
            this.Upload_Dashboard.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Upload_Dashboard.Name = "Upload_Dashboard";
            this.Upload_Dashboard.Size = new System.Drawing.Size(400, 90);
            this.Upload_Dashboard.TabIndex = 3;
            this.Upload_Dashboard.Text = "Upload";
            this.Upload_Dashboard.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Upload_Dashboard.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.Upload_Dashboard.UseVisualStyleBackColor = true;
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
            this.Share_Dashboard.Location = new System.Drawing.Point(0, 94);
            this.Share_Dashboard.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Share_Dashboard.Name = "Share_Dashboard";
            this.Share_Dashboard.Size = new System.Drawing.Size(400, 94);
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
            this.MyFile_Dashboard.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.MyFile_Dashboard.Name = "MyFile_Dashboard";
            this.MyFile_Dashboard.Size = new System.Drawing.Size(400, 94);
            this.MyFile_Dashboard.TabIndex = 1;
            this.MyFile_Dashboard.Text = "My File";
            this.MyFile_Dashboard.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MyFile_Dashboard.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.MyFile_Dashboard.UseVisualStyleBackColor = false;
            this.MyFile_Dashboard.Click += new System.EventHandler(this.MyFile_Dashboard_Click);
            // 
            // NavBarPanel
            // 
            this.NavBarPanel.Controls.Add(this.Account);
            this.NavBarPanel.Location = new System.Drawing.Point(4, 0);
            this.NavBarPanel.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.NavBarPanel.Name = "NavBarPanel";
            this.NavBarPanel.Size = new System.Drawing.Size(1610, 158);
            this.NavBarPanel.TabIndex = 16;
            this.NavBarPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.NavBarPanel_Paint);
            // 
            // MainContentPanel
            // 
            this.MainContentPanel.Location = new System.Drawing.Point(412, 169);
            this.MainContentPanel.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.MainContentPanel.Name = "MainContentPanel";
            this.MainContentPanel.Size = new System.Drawing.Size(1202, 754);
            this.MainContentPanel.TabIndex = 17;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.AntiqueWhite;
            this.ClientSize = new System.Drawing.Size(1618, 935);
            this.Controls.Add(this.MainContentPanel);
            this.Controls.Add(this.NavBarPanel);
            this.Controls.Add(this.PanelDashboard);
            this.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.Name = "Main";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Main_Load);
            this.PanelDashboard.ResumeLayout(false);
            this.NavBarPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button Account;
        private System.Windows.Forms.Panel PanelDashboard;
        private FontAwesome.Sharp.IconButton MyFile_Dashboard;
        private System.Windows.Forms.Panel NavBarPanel;
        private FontAwesome.Sharp.IconButton Upload_Dashboard;
        private FontAwesome.Sharp.IconButton Share_Dashboard;
        private FontAwesome.Sharp.IconButton Settings_Dashboard;
        private FontAwesome.Sharp.IconButton TrashBin_Dashboard;
        private System.Windows.Forms.Panel MainContentPanel;
    }
}

