namespace FileSharingClient
{
    partial class FileItemControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lblFileName = new System.Windows.Forms.Label();
            this.lblOwner = new System.Windows.Forms.Label();
            this.lblCreateAt = new System.Windows.Forms.Label();
            this.lblFileSize = new System.Windows.Forms.Label();
            this.lblFileType = new System.Windows.Forms.Label();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.shareToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.downloadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnMore = new System.Windows.Forms.Button();
            this.lblFilePath = new System.Windows.Forms.Label();
            this.lblFileIcon = new System.Windows.Forms.Label();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblFileName
            // 
            this.lblFileName.AutoSize = true;
            this.lblFileName.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileName.Location = new System.Drawing.Point(50, 12);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(64, 17);
            this.lblFileName.TabIndex = 0;
            this.lblFileName.Text = "FileName";
            // 
            // lblOwner
            // 
            this.lblOwner.AutoSize = true;
            this.lblOwner.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOwner.Location = new System.Drawing.Point(260, 12);
            this.lblOwner.Name = "lblOwner";
            this.lblOwner.Size = new System.Drawing.Size(48, 17);
            this.lblOwner.TabIndex = 1;
            this.lblOwner.Text = "Owner";
            // 
            // lblCreateAt
            // 
            this.lblCreateAt.AutoSize = true;
            this.lblCreateAt.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCreateAt.Location = new System.Drawing.Point(600, 12);
            this.lblCreateAt.Name = "lblCreateAt";
            this.lblCreateAt.Size = new System.Drawing.Size(37, 17);
            this.lblCreateAt.TabIndex = 2;
            this.lblCreateAt.Text = "Time";
            this.lblCreateAt.Visible = false;
            // 
            // lblFileSize
            // 
            this.lblFileSize.AutoSize = true;
            this.lblFileSize.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblFileSize.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileSize.Location = new System.Drawing.Point(420, 12);
            this.lblFileSize.Name = "lblFileSize";
            this.lblFileSize.Size = new System.Drawing.Size(51, 17);
            this.lblFileSize.TabIndex = 3;
            this.lblFileSize.Text = "FileSize";
            // 
            // lblFileType
            // 
            this.lblFileType.AutoSize = true;
            this.lblFileType.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblFileType.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileType.Location = new System.Drawing.Point(650, 12);
            this.lblFileType.Name = "lblFileType";
            this.lblFileType.Size = new System.Drawing.Size(51, 17);
            this.lblFileType.TabIndex = 4;
            this.lblFileType.Text = "FileType";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.shareToolStripMenuItem,
            this.downloadToolStripMenuItem,
            this.deleteToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(128, 70);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // shareToolStripMenuItem
            // 
            this.shareToolStripMenuItem.Name = "shareToolStripMenuItem";
            this.shareToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.shareToolStripMenuItem.Text = "Share";
            this.shareToolStripMenuItem.Click += new System.EventHandler(this.shareToolStripMenuItem_Click);
            // 
            // downloadToolStripMenuItem
            // 
            this.downloadToolStripMenuItem.Name = "downloadToolStripMenuItem";
            this.downloadToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.downloadToolStripMenuItem.Text = "Download";
            this.downloadToolStripMenuItem.Click += new System.EventHandler(this.downloadToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // btnMore
            // 
            this.btnMore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMore.AutoSize = true;
            this.btnMore.FlatAppearance.BorderSize = 0;
            this.btnMore.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMore.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.btnMore.Location = new System.Drawing.Point(750, 10);
            this.btnMore.Name = "btnMore";
            this.btnMore.Size = new System.Drawing.Size(75, 23);
            this.btnMore.TabIndex = 5;
            this.btnMore.Text = "...";
            this.btnMore.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.btnMore.UseVisualStyleBackColor = true;
            // 
            // lblFilePath
            // 
            this.lblFilePath.AutoEllipsis = true;
            this.lblFilePath.AutoSize = true;
            this.lblFilePath.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblFilePath.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFilePath.Location = new System.Drawing.Point(733, 12);
            this.lblFilePath.Name = "lblFilePath";
            this.lblFilePath.Size = new System.Drawing.Size(56, 17);
            this.lblFilePath.TabIndex = 6;
            this.lblFilePath.Text = "FilePath";
            // 
            // lblFileIcon
            // 
            this.lblFileIcon.AutoSize = true;
            this.lblFileIcon.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileIcon.Location = new System.Drawing.Point(10, 12);
            this.lblFileIcon.Name = "lblFileIcon";
            this.lblFileIcon.Size = new System.Drawing.Size(23, 21);
            this.lblFileIcon.TabIndex = 9;
            this.lblFileIcon.Text = "📄";
            // 
            // FileItemControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = false;
            this.Height = 43;
            this.Margin = new System.Windows.Forms.Padding(0);
            this.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.Controls.Add(this.lblFilePath);
            this.Controls.Add(this.btnMore);
            this.Controls.Add(this.lblFileType);
            this.Controls.Add(this.lblFileSize);
            this.Controls.Add(this.lblCreateAt);
            this.Controls.Add(this.lblOwner);
            this.Controls.Add(this.lblFileName);
            this.Controls.Add(this.lblFileIcon);
            this.Name = "FileItemControl";
            this.Size = new System.Drawing.Size(800, 43);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblFileName;
        private System.Windows.Forms.Label lblOwner;
        private System.Windows.Forms.Label lblCreateAt;
        private System.Windows.Forms.Label lblFileSize;
        private System.Windows.Forms.Label lblFileType;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem shareToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem downloadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.Button btnMore;
        private System.Windows.Forms.Label lblFilePath;
        private System.Windows.Forms.Label lblFileIcon;
    }
}
