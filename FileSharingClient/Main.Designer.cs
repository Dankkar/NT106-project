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
            this.btnSendFile = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.listView1 = new System.Windows.Forms.ListView();
            this.btnDownloadFile = new System.Windows.Forms.Button();
            this.btnDeleteFile = new System.Windows.Forms.Button();
            this.btnMakeDirectory = new System.Windows.Forms.Button();
            this.btnFindFile = new System.Windows.Forms.Button();
            this.btnCreateLink = new System.Windows.Forms.Button();
            this.btnSettings = new System.Windows.Forms.Button();
            this.btnShareFile = new System.Windows.Forms.Button();
            this.btnMoveFile = new System.Windows.Forms.Button();
            this.btnRename = new System.Windows.Forms.Button();
            this.btnPermissions = new System.Windows.Forms.Button();
            this.Information = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btnArrange = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnSendFile
            // 
            this.btnSendFile.Location = new System.Drawing.Point(18, 38);
            this.btnSendFile.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnSendFile.Name = "btnSendFile";
            this.btnSendFile.Size = new System.Drawing.Size(112, 35);
            this.btnSendFile.TabIndex = 0;
            this.btnSendFile.Text = "Upload";
            this.btnSendFile.UseVisualStyleBackColor = true;
            this.btnSendFile.Click += new System.EventHandler(this.btnSendFile_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // listView1
            // 
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(219, 192);
            this.listView1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.listView1.Name = "listView1";
            this.listView1.OwnerDraw = true;
            this.listView1.Size = new System.Drawing.Size(1232, 467);
            this.listView1.TabIndex = 1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            // 
            // btnDownloadFile
            // 
            this.btnDownloadFile.Location = new System.Drawing.Point(363, 38);
            this.btnDownloadFile.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnDownloadFile.Name = "btnDownloadFile";
            this.btnDownloadFile.Size = new System.Drawing.Size(112, 35);
            this.btnDownloadFile.TabIndex = 2;
            this.btnDownloadFile.Text = "Download";
            this.btnDownloadFile.UseVisualStyleBackColor = true;
            // 
            // btnDeleteFile
            // 
            this.btnDeleteFile.Location = new System.Drawing.Point(188, 38);
            this.btnDeleteFile.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnDeleteFile.Name = "btnDeleteFile";
            this.btnDeleteFile.Size = new System.Drawing.Size(112, 35);
            this.btnDeleteFile.TabIndex = 3;
            this.btnDeleteFile.Text = "Delete";
            this.btnDeleteFile.UseVisualStyleBackColor = true;
            // 
            // btnMakeDirectory
            // 
            this.btnMakeDirectory.Location = new System.Drawing.Point(18, 265);
            this.btnMakeDirectory.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnMakeDirectory.Name = "btnMakeDirectory";
            this.btnMakeDirectory.Size = new System.Drawing.Size(142, 35);
            this.btnMakeDirectory.TabIndex = 4;
            this.btnMakeDirectory.Text = "Make Directory";
            this.btnMakeDirectory.UseVisualStyleBackColor = true;
            // 
            // btnFindFile
            // 
            this.btnFindFile.Location = new System.Drawing.Point(18, 192);
            this.btnFindFile.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnFindFile.Name = "btnFindFile";
            this.btnFindFile.Size = new System.Drawing.Size(142, 35);
            this.btnFindFile.TabIndex = 5;
            this.btnFindFile.Text = "Find";
            this.btnFindFile.UseVisualStyleBackColor = true;
            // 
            // btnCreateLink
            // 
            this.btnCreateLink.Location = new System.Drawing.Point(546, 38);
            this.btnCreateLink.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnCreateLink.Name = "btnCreateLink";
            this.btnCreateLink.Size = new System.Drawing.Size(112, 35);
            this.btnCreateLink.TabIndex = 6;
            this.btnCreateLink.Text = "Create Link";
            this.btnCreateLink.UseVisualStyleBackColor = true;
            this.btnCreateLink.Click += new System.EventHandler(this.btnCreateLink_Click);
            // 
            // btnSettings
            // 
            this.btnSettings.Location = new System.Drawing.Point(18, 477);
            this.btnSettings.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(142, 35);
            this.btnSettings.TabIndex = 7;
            this.btnSettings.Text = "Settings";
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnShareFile
            // 
            this.btnShareFile.Location = new System.Drawing.Point(732, 38);
            this.btnShareFile.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnShareFile.Name = "btnShareFile";
            this.btnShareFile.Size = new System.Drawing.Size(112, 35);
            this.btnShareFile.TabIndex = 8;
            this.btnShareFile.Text = "Share";
            this.btnShareFile.UseVisualStyleBackColor = true;
            // 
            // btnMoveFile
            // 
            this.btnMoveFile.Location = new System.Drawing.Point(912, 38);
            this.btnMoveFile.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnMoveFile.Name = "btnMoveFile";
            this.btnMoveFile.Size = new System.Drawing.Size(112, 35);
            this.btnMoveFile.TabIndex = 9;
            this.btnMoveFile.Text = "Move To";
            this.btnMoveFile.UseVisualStyleBackColor = true;
            // 
            // btnRename
            // 
            this.btnRename.Location = new System.Drawing.Point(1080, 38);
            this.btnRename.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnRename.Name = "btnRename";
            this.btnRename.Size = new System.Drawing.Size(112, 35);
            this.btnRename.TabIndex = 10;
            this.btnRename.Text = "Rename";
            this.btnRename.UseVisualStyleBackColor = true;
            // 
            // btnPermissions
            // 
            this.btnPermissions.Location = new System.Drawing.Point(1242, 38);
            this.btnPermissions.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnPermissions.Name = "btnPermissions";
            this.btnPermissions.Size = new System.Drawing.Size(112, 35);
            this.btnPermissions.TabIndex = 11;
            this.btnPermissions.Text = "Permissions";
            this.btnPermissions.UseVisualStyleBackColor = true;
            // 
            // Information
            // 
            this.Information.Location = new System.Drawing.Point(1404, 38);
            this.Information.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Information.Name = "Information";
            this.Information.Size = new System.Drawing.Size(112, 35);
            this.Information.TabIndex = 12;
            this.Information.Text = "Information";
            this.Information.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(18, 408);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(142, 35);
            this.button1.TabIndex = 13;
            this.button1.Text = "Account";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // btnArrange
            // 
            this.btnArrange.Location = new System.Drawing.Point(18, 332);
            this.btnArrange.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnArrange.Name = "btnArrange";
            this.btnArrange.Size = new System.Drawing.Size(142, 35);
            this.btnArrange.TabIndex = 14;
            this.btnArrange.Text = "Arrange";
            this.btnArrange.UseVisualStyleBackColor = true;
            this.btnArrange.Click += new System.EventHandler(this.btnArrange_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1443, 692);
            this.Controls.Add(this.btnArrange);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.Information);
            this.Controls.Add(this.btnPermissions);
            this.Controls.Add(this.btnRename);
            this.Controls.Add(this.btnMoveFile);
            this.Controls.Add(this.btnShareFile);
            this.Controls.Add(this.btnSettings);
            this.Controls.Add(this.btnCreateLink);
            this.Controls.Add(this.btnFindFile);
            this.Controls.Add(this.btnMakeDirectory);
            this.Controls.Add(this.btnDeleteFile);
            this.Controls.Add(this.btnDownloadFile);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.btnSendFile);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "Main";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnSendFile;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Button btnDownloadFile;
        private System.Windows.Forms.Button btnDeleteFile;
        private System.Windows.Forms.Button btnMakeDirectory;
        private System.Windows.Forms.Button btnFindFile;
        private System.Windows.Forms.Button btnCreateLink;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.Button btnShareFile;
        private System.Windows.Forms.Button btnMoveFile;
        private System.Windows.Forms.Button btnRename;
        private System.Windows.Forms.Button btnPermissions;
        private System.Windows.Forms.Button Information;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnArrange;
    }
}

