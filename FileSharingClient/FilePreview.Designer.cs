namespace FileSharingClient
{
    partial class FilePreview
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
            this.cbBrowseFile = new System.Windows.Forms.ComboBox();
            this.lblBrowseFile = new System.Windows.Forms.Label();
            this.FilePreviewPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // cbBrowseFile
            // 
            this.cbBrowseFile.FormattingEnabled = true;
            this.cbBrowseFile.Location = new System.Drawing.Point(188, 72);
            this.cbBrowseFile.Name = "cbBrowseFile";
            this.cbBrowseFile.Size = new System.Drawing.Size(173, 21);
            this.cbBrowseFile.TabIndex = 0;
            // 
            // lblBrowseFile
            // 
            this.lblBrowseFile.AutoSize = true;
            this.lblBrowseFile.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold);
            this.lblBrowseFile.Location = new System.Drawing.Point(215, 29);
            this.lblBrowseFile.Name = "lblBrowseFile";
            this.lblBrowseFile.Size = new System.Drawing.Size(112, 25);
            this.lblBrowseFile.TabIndex = 1;
            this.lblBrowseFile.Text = "Browse File";
            // 
            // FilePreviewPanel
            // 
            this.FilePreviewPanel.Location = new System.Drawing.Point(13, 120);
            this.FilePreviewPanel.Name = "FilePreviewPanel";
            this.FilePreviewPanel.Size = new System.Drawing.Size(576, 257);
            this.FilePreviewPanel.TabIndex = 2;
            // 
            // FilePreview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.FilePreviewPanel);
            this.Controls.Add(this.lblBrowseFile);
            this.Controls.Add(this.cbBrowseFile);
            this.Name = "FilePreview";
            this.Size = new System.Drawing.Size(601, 392);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbBrowseFile;
        private System.Windows.Forms.Label lblBrowseFile;
        private System.Windows.Forms.Panel FilePreviewPanel;
    }
}
