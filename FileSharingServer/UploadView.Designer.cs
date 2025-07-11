namespace FileSharingClient
{
    partial class UploadView
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
            this.DragPanel = new System.Windows.Forms.Panel();
            this.UploadedPanel = new System.Windows.Forms.Panel();
            this.progressPanel = new System.Windows.Forms.Panel();
            this.lblProgressStatus = new System.Windows.Forms.Label();
            this.progressBarUpload = new System.Windows.Forms.ProgressBar();
            this.TotalSizelbl = new System.Windows.Forms.Label();
            this.btnUpload = new System.Windows.Forms.Button();
            this.UploadFilePanel = new System.Windows.Forms.FlowLayoutPanel();
            this.ListFiles = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.btnBrowseFolder = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.DragPanel.SuspendLayout();
            this.UploadedPanel.SuspendLayout();
            this.progressPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // DragPanel
            // 
            this.DragPanel.AllowDrop = true;
            this.DragPanel.AutoScroll = true;
            this.DragPanel.Controls.Add(this.UploadedPanel);
            this.DragPanel.Controls.Add(this.btnBrowse);
            this.DragPanel.Controls.Add(this.btnBrowseFolder);
            this.DragPanel.Controls.Add(this.label1);
            this.DragPanel.Controls.Add(this.label2);
            this.DragPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.DragPanel.Location = new System.Drawing.Point(0, 0);
            this.DragPanel.Margin = new System.Windows.Forms.Padding(2);
            this.DragPanel.Name = "DragPanel";
            this.DragPanel.Size = new System.Drawing.Size(1102, 595);
            this.DragPanel.TabIndex = 0;
            this.DragPanel.DragDrop += new System.Windows.Forms.DragEventHandler(this.DragPanel_DragDrop);
            this.DragPanel.DragEnter += new System.Windows.Forms.DragEventHandler(this.DragPanel_DragEnter);
            // 
            // UploadedPanel
            // 
            this.UploadedPanel.Controls.Add(this.progressPanel);
            this.UploadedPanel.Controls.Add(this.TotalSizelbl);
            this.UploadedPanel.Controls.Add(this.btnUpload);
            this.UploadedPanel.Controls.Add(this.UploadFilePanel);
            this.UploadedPanel.Controls.Add(this.ListFiles);
            this.UploadedPanel.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.UploadedPanel.Location = new System.Drawing.Point(299, 0);
            this.UploadedPanel.Margin = new System.Windows.Forms.Padding(2);
            this.UploadedPanel.Name = "UploadedPanel";
            this.UploadedPanel.Size = new System.Drawing.Size(803, 591);
            this.UploadedPanel.TabIndex = 1;
            // 
            // progressPanel
            // 
            this.progressPanel.Controls.Add(this.lblProgressStatus);
            this.progressPanel.Controls.Add(this.progressBarUpload);
            this.progressPanel.Location = new System.Drawing.Point(4, 389);
            this.progressPanel.Name = "progressPanel";
            this.progressPanel.Size = new System.Drawing.Size(798, 60);
            this.progressPanel.TabIndex = 4;
            this.progressPanel.Visible = false;
            // 
            // lblProgressStatus
            // 
            this.lblProgressStatus.AutoSize = true;
            this.lblProgressStatus.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblProgressStatus.Location = new System.Drawing.Point(3, 5);
            this.lblProgressStatus.Name = "lblProgressStatus";
            this.lblProgressStatus.Size = new System.Drawing.Size(107, 19);
            this.lblProgressStatus.TabIndex = 1;
            this.lblProgressStatus.Text = "Đang chuẩn bị...";
            // 
            // progressBarUpload
            // 
            this.progressBarUpload.Location = new System.Drawing.Point(3, 30);
            this.progressBarUpload.Name = "progressBarUpload";
            this.progressBarUpload.Size = new System.Drawing.Size(792, 25);
            this.progressBarUpload.TabIndex = 0;
            // 
            // TotalSizelbl
            // 
            this.TotalSizelbl.AutoSize = true;
            this.TotalSizelbl.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
            this.TotalSizelbl.Location = new System.Drawing.Point(3, 454);
            this.TotalSizelbl.Name = "TotalSizelbl";
            this.TotalSizelbl.Size = new System.Drawing.Size(145, 21);
            this.TotalSizelbl.TabIndex = 2;
            this.TotalSizelbl.Text = "Tổng dung lượng: ";
            // 
            // btnUpload
            // 
            this.btnUpload.Font = new System.Drawing.Font("Segoe UI", 15F);
            this.btnUpload.Location = new System.Drawing.Point(7, 503);
            this.btnUpload.Margin = new System.Windows.Forms.Padding(2);
            this.btnUpload.Name = "btnUpload";
            this.btnUpload.Size = new System.Drawing.Size(792, 40);
            this.btnUpload.TabIndex = 0;
            this.btnUpload.Text = "Upload";
            this.btnUpload.UseVisualStyleBackColor = true;
            this.btnUpload.Click += new System.EventHandler(this.btnUpload_Click);
            // 
            // UploadFilePanel
            // 
            this.UploadFilePanel.Location = new System.Drawing.Point(4, 54);
            this.UploadFilePanel.Margin = new System.Windows.Forms.Padding(2);
            this.UploadFilePanel.Name = "UploadFilePanel";
            this.UploadFilePanel.Size = new System.Drawing.Size(800, 330);
            this.UploadFilePanel.TabIndex = 1;
            // 
            // ListFiles
            // 
            this.ListFiles.AutoSize = true;
            this.ListFiles.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold);
            this.ListFiles.Location = new System.Drawing.Point(238, 18);
            this.ListFiles.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ListFiles.Name = "ListFiles";
            this.ListFiles.Size = new System.Drawing.Size(299, 25);
            this.ListFiles.TabIndex = 0;
            this.ListFiles.Text = "Các Files/Folder đã được Upload";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.btnBrowse.Location = new System.Drawing.Point(59, 361);
            this.btnBrowse.Margin = new System.Windows.Forms.Padding(2);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(154, 51);
            this.btnBrowse.TabIndex = 3;
            this.btnBrowse.Text = "Browse Files";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // btnBrowseFolder
            // 
            this.btnBrowseFolder.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.btnBrowseFolder.Location = new System.Drawing.Point(59, 427);
            this.btnBrowseFolder.Margin = new System.Windows.Forms.Padding(2);
            this.btnBrowseFolder.Name = "btnBrowseFolder";
            this.btnBrowseFolder.Size = new System.Drawing.Size(154, 51);
            this.btnBrowseFolder.TabIndex = 4;
            this.btnBrowseFolder.Text = "Browse Folder";
            this.btnBrowseFolder.UseVisualStyleBackColor = true;
            this.btnBrowseFolder.Click += new System.EventHandler(this.btnBrowseFolder_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(55, 46);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(172, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "Kéo và thả file/folder";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.label2.Location = new System.Drawing.Point(34, 322);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.label2.Size = new System.Drawing.Size(212, 21);
            this.label2.TabIndex = 2;
            this.label2.Text = "Hoặc chọn files/folder tại đây";
            // 
            // UploadView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.DragPanel);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "UploadView";
            this.Size = new System.Drawing.Size(1104, 595);
            this.DragPanel.ResumeLayout(false);
            this.DragPanel.PerformLayout();
            this.UploadedPanel.ResumeLayout(false);
            this.UploadedPanel.PerformLayout();
            this.progressPanel.ResumeLayout(false);
            this.progressPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel DragPanel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel UploadedPanel;
        private System.Windows.Forms.Label ListFiles;
        private System.Windows.Forms.Button btnUpload;
        private System.Windows.Forms.FlowLayoutPanel UploadFilePanel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnBrowseFolder;
        private System.Windows.Forms.Label TotalSizelbl;
        private System.Windows.Forms.Panel progressPanel;
        private System.Windows.Forms.Label lblProgressStatus;
        private System.Windows.Forms.ProgressBar progressBarUpload;
    }
}
