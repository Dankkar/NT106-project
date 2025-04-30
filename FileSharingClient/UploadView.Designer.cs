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
            this.label1 = new System.Windows.Forms.Label();
            this.UploadedPanel = new System.Windows.Forms.Panel();
            this.btnUpload = new System.Windows.Forms.Button();
            this.UploadFilePanel = new System.Windows.Forms.FlowLayoutPanel();
            this.ListFiles = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.DragPanel.SuspendLayout();
            this.UploadedPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // DragPanel
            // 
            this.DragPanel.AllowDrop = true;
            this.DragPanel.AutoScroll = true;
            this.DragPanel.Controls.Add(this.label1);
            this.DragPanel.Location = new System.Drawing.Point(3, 3);
            this.DragPanel.Name = "DragPanel";
            this.DragPanel.Size = new System.Drawing.Size(367, 517);
            this.DragPanel.TabIndex = 0;
            this.DragPanel.DragDrop += new System.Windows.Forms.DragEventHandler(this.DragPanel_DragDrop);
            this.DragPanel.DragEnter += new System.Windows.Forms.DragEventHandler(this.DragPanel_DragEnter);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(30, 146);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(308, 38);
            this.label1.TabIndex = 0;
            this.label1.Text = "Kéo và thả File tại đây";
            // 
            // UploadedPanel
            // 
            this.UploadedPanel.Controls.Add(this.btnUpload);
            this.UploadedPanel.Controls.Add(this.UploadFilePanel);
            this.UploadedPanel.Controls.Add(this.ListFiles);
            this.UploadedPanel.Location = new System.Drawing.Point(375, 3);
            this.UploadedPanel.Name = "UploadedPanel";
            this.UploadedPanel.Size = new System.Drawing.Size(724, 718);
            this.UploadedPanel.TabIndex = 1;
            // 
            // btnUpload
            // 
            this.btnUpload.Font = new System.Drawing.Font("Segoe UI", 16F);
            this.btnUpload.Location = new System.Drawing.Point(258, 609);
            this.btnUpload.Name = "btnUpload";
            this.btnUpload.Size = new System.Drawing.Size(203, 78);
            this.btnUpload.TabIndex = 0;
            this.btnUpload.Text = "Upload";
            this.btnUpload.UseVisualStyleBackColor = true;
            this.btnUpload.Click += new System.EventHandler(this.btnUpload_Click);
            // 
            // UploadFilePanel
            // 
            this.UploadFilePanel.Location = new System.Drawing.Point(15, 99);
            this.UploadFilePanel.Name = "UploadFilePanel";
            this.UploadFilePanel.Size = new System.Drawing.Size(697, 438);
            this.UploadFilePanel.TabIndex = 1;
            // 
            // ListFiles
            // 
            this.ListFiles.AutoSize = true;
            this.ListFiles.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold);
            this.ListFiles.Location = new System.Drawing.Point(253, 40);
            this.ListFiles.Name = "ListFiles";
            this.ListFiles.Size = new System.Drawing.Size(405, 46);
            this.ListFiles.TabIndex = 0;
            this.ListFiles.Text = "Các File đã được Upload";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.label2.Location = new System.Drawing.Point(40, 541);
            this.label2.Name = "label2";
            this.label2.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.label2.Size = new System.Drawing.Size(290, 38);
            this.label2.TabIndex = 2;
            this.label2.Text = "Hoặc chọn File tại đây";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.btnBrowse.Location = new System.Drawing.Point(40, 601);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(283, 55);
            this.btnBrowse.TabIndex = 3;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // UploadView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.UploadedPanel);
            this.Controls.Add(this.DragPanel);
            this.Name = "UploadView";
            this.Size = new System.Drawing.Size(1102, 724);
            this.DragPanel.ResumeLayout(false);
            this.DragPanel.PerformLayout();
            this.UploadedPanel.ResumeLayout(false);
            this.UploadedPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

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
    }
}
