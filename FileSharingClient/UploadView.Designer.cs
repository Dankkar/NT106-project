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
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.ListFiles = new System.Windows.Forms.Label();
            this.DragPanel.SuspendLayout();
            this.UploadedPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // DragPanel
            // 
            this.DragPanel.Controls.Add(this.label1);
            this.DragPanel.Location = new System.Drawing.Point(3, 3);
            this.DragPanel.Name = "DragPanel";
            this.DragPanel.Size = new System.Drawing.Size(400, 748);
            this.DragPanel.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(45, 343);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(177, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "Kéo và thả File tại đây";
            // 
            // UploadedPanel
            // 
            this.UploadedPanel.Controls.Add(this.btnUpload);
            this.UploadedPanel.Controls.Add(this.progressBar1);
            this.UploadedPanel.Controls.Add(this.flowLayoutPanel1);
            this.UploadedPanel.Controls.Add(this.ListFiles);
            this.UploadedPanel.Location = new System.Drawing.Point(409, 3);
            this.UploadedPanel.Name = "UploadedPanel";
            this.UploadedPanel.Size = new System.Drawing.Size(790, 748);
            this.UploadedPanel.TabIndex = 1;
            // 
            // btnUpload
            // 
            this.btnUpload.Font = new System.Drawing.Font("Segoe UI", 16F);
            this.btnUpload.Location = new System.Drawing.Point(281, 634);
            this.btnUpload.Name = "btnUpload";
            this.btnUpload.Size = new System.Drawing.Size(221, 81);
            this.btnUpload.TabIndex = 0;
            this.btnUpload.Text = "Upload";
            this.btnUpload.UseVisualStyleBackColor = true;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(281, 575);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(221, 34);
            this.progressBar1.TabIndex = 2;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Location = new System.Drawing.Point(16, 103);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(760, 456);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // ListFiles
            // 
            this.ListFiles.AutoSize = true;
            this.ListFiles.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold);
            this.ListFiles.Location = new System.Drawing.Point(276, 42);
            this.ListFiles.Name = "ListFiles";
            this.ListFiles.Size = new System.Drawing.Size(226, 25);
            this.ListFiles.TabIndex = 0;
            this.ListFiles.Text = "Các File đã được Upload";
            // 
            // UploadView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.UploadedPanel);
            this.Controls.Add(this.DragPanel);
            this.Name = "UploadView";
            this.Size = new System.Drawing.Size(1202, 754);
            this.DragPanel.ResumeLayout(false);
            this.DragPanel.PerformLayout();
            this.UploadedPanel.ResumeLayout(false);
            this.UploadedPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel DragPanel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel UploadedPanel;
        private System.Windows.Forms.Label ListFiles;
        private System.Windows.Forms.Button btnUpload;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}
