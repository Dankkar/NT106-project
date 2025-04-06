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
            this.btnSendFile.Location = new System.Drawing.Point(24, 48);
            this.btnSendFile.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.btnSendFile.Name = "btnSendFile";
            this.btnSendFile.Size = new System.Drawing.Size(150, 44);
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
            this.listView1.Location = new System.Drawing.Point(24, 204);
            this.listView1.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.listView1.Name = "listView1";
            this.listView1.OwnerDraw = true;
            this.listView1.Size = new System.Drawing.Size(1390, 618);
            this.listView1.TabIndex = 1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            // 
            // btnDownloadFile
            // 
            this.btnDownloadFile.Location = new System.Drawing.Point(432, 48);
            this.btnDownloadFile.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.btnDownloadFile.Name = "btnDownloadFile";
            this.btnDownloadFile.Size = new System.Drawing.Size(150, 44);
            this.btnDownloadFile.TabIndex = 2;
            this.btnDownloadFile.Text = "Download";
            this.btnDownloadFile.UseVisualStyleBackColor = true;
            // 
            // btnDeleteFile
            // 
            this.btnDeleteFile.Location = new System.Drawing.Point(224, 48);
            this.btnDeleteFile.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.btnDeleteFile.Name = "btnDeleteFile";
            this.btnDeleteFile.Size = new System.Drawing.Size(150, 44);
            this.btnDeleteFile.TabIndex = 3;
            this.btnDeleteFile.Text = "Delete";
            this.btnDeleteFile.UseVisualStyleBackColor = true;
            // 
            // btnMakeDirectory
            // 
            this.btnMakeDirectory.Location = new System.Drawing.Point(830, 125);
            this.btnMakeDirectory.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.btnMakeDirectory.Name = "btnMakeDirectory";
            this.btnMakeDirectory.Size = new System.Drawing.Size(179, 44);
            this.btnMakeDirectory.TabIndex = 4;
            this.btnMakeDirectory.Text = "Make Directory";
            this.btnMakeDirectory.UseVisualStyleBackColor = true;
            // 
            // btnFindFile
            // 
            this.btnFindFile.Location = new System.Drawing.Point(630, 48);
            this.btnFindFile.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.btnFindFile.Name = "btnFindFile";
            this.btnFindFile.Size = new System.Drawing.Size(150, 44);
            this.btnFindFile.TabIndex = 5;
            this.btnFindFile.Text = "Find";
            this.btnFindFile.UseVisualStyleBackColor = true;
            // 
            // btnCreateLink
            // 
            this.btnCreateLink.Location = new System.Drawing.Point(840, 48);
            this.btnCreateLink.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.btnCreateLink.Name = "btnCreateLink";
            this.btnCreateLink.Size = new System.Drawing.Size(150, 44);
            this.btnCreateLink.TabIndex = 6;
            this.btnCreateLink.Text = "Create Link";
            this.btnCreateLink.UseVisualStyleBackColor = true;
            this.btnCreateLink.Click += new System.EventHandler(this.btnCreateLink_Click);
            // 
            // btnSettings
            // 
            this.btnSettings.Location = new System.Drawing.Point(1264, 125);
            this.btnSettings.Margin = new System.Windows.Forms.Padding(6);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(150, 44);
            this.btnSettings.TabIndex = 7;
            this.btnSettings.Text = "Settings";
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnShareFile
            // 
            this.btnShareFile.Location = new System.Drawing.Point(1057, 48);
            this.btnShareFile.Margin = new System.Windows.Forms.Padding(6);
            this.btnShareFile.Name = "btnShareFile";
            this.btnShareFile.Size = new System.Drawing.Size(150, 44);
            this.btnShareFile.TabIndex = 8;
            this.btnShareFile.Text = "Share";
            this.btnShareFile.UseVisualStyleBackColor = true;
            // 
            // btnMoveFile
            // 
            this.btnMoveFile.Location = new System.Drawing.Point(1264, 48);
            this.btnMoveFile.Margin = new System.Windows.Forms.Padding(6);
            this.btnMoveFile.Name = "btnMoveFile";
            this.btnMoveFile.Size = new System.Drawing.Size(150, 44);
            this.btnMoveFile.TabIndex = 9;
            this.btnMoveFile.Text = "Move To";
            this.btnMoveFile.UseVisualStyleBackColor = true;
            // 
            // btnRename
            // 
            this.btnRename.Location = new System.Drawing.Point(24, 125);
            this.btnRename.Margin = new System.Windows.Forms.Padding(6);
            this.btnRename.Name = "btnRename";
            this.btnRename.Size = new System.Drawing.Size(150, 44);
            this.btnRename.TabIndex = 10;
            this.btnRename.Text = "Rename";
            this.btnRename.UseVisualStyleBackColor = true;
            // 
            // btnPermissions
            // 
            this.btnPermissions.Location = new System.Drawing.Point(224, 125);
            this.btnPermissions.Margin = new System.Windows.Forms.Padding(6);
            this.btnPermissions.Name = "btnPermissions";
            this.btnPermissions.Size = new System.Drawing.Size(150, 44);
            this.btnPermissions.TabIndex = 11;
            this.btnPermissions.Text = "Permissions";
            this.btnPermissions.UseVisualStyleBackColor = true;
            // 
            // Information
            // 
            this.Information.Location = new System.Drawing.Point(432, 125);
            this.Information.Margin = new System.Windows.Forms.Padding(6);
            this.Information.Name = "Information";
            this.Information.Size = new System.Drawing.Size(150, 44);
            this.Information.TabIndex = 12;
            this.Information.Text = "Information";
            this.Information.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(1057, 125);
            this.button1.Margin = new System.Windows.Forms.Padding(6);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(150, 44);
            this.button1.TabIndex = 13;
            this.button1.Text = "Account";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // btnArrange
            // 
            this.btnArrange.Location = new System.Drawing.Point(630, 125);
            this.btnArrange.Margin = new System.Windows.Forms.Padding(6);
            this.btnArrange.Name = "btnArrange";
            this.btnArrange.Size = new System.Drawing.Size(150, 44);
            this.btnArrange.TabIndex = 14;
            this.btnArrange.Text = "Arrange";
            this.btnArrange.UseVisualStyleBackColor = true;
            this.btnArrange.Click += new System.EventHandler(this.btnArrange_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1444, 865);
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
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Name = "Main";
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

