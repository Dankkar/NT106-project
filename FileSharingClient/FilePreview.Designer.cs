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
            this.btnPreview = new System.Windows.Forms.Button();
            this.rtbContent = new System.Windows.Forms.RichTextBox();
            this.picImagePreview = new System.Windows.Forms.PictureBox();
            this.wbPdfPreview = new System.Windows.Forms.WebBrowser();
            ((System.ComponentModel.ISupportInitialize)(this.picImagePreview)).BeginInit();
            this.SuspendLayout();
            // 
            // cbBrowseFile
            // 
            this.cbBrowseFile.FormattingEnabled = true;
            this.cbBrowseFile.Location = new System.Drawing.Point(345, 133);
            this.cbBrowseFile.Margin = new System.Windows.Forms.Padding(6);
            this.cbBrowseFile.Name = "cbBrowseFile";
            this.cbBrowseFile.Size = new System.Drawing.Size(314, 32);
            this.cbBrowseFile.TabIndex = 0;
            // 
            // lblBrowseFile
            // 
            this.lblBrowseFile.AutoSize = true;
            this.lblBrowseFile.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold);
            this.lblBrowseFile.Location = new System.Drawing.Point(394, 54);
            this.lblBrowseFile.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblBrowseFile.Name = "lblBrowseFile";
            this.lblBrowseFile.Size = new System.Drawing.Size(202, 46);
            this.lblBrowseFile.TabIndex = 1;
            this.lblBrowseFile.Text = "Browse File";
            // 
            // btnPreview
            // 
            this.btnPreview.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.btnPreview.Location = new System.Drawing.Point(345, 193);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(314, 43);
            this.btnPreview.TabIndex = 3;
            this.btnPreview.Text = "Preview";
            this.btnPreview.UseVisualStyleBackColor = true;
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
            // 
            // rtbContent
            // 
            this.rtbContent.Location = new System.Drawing.Point(63, 299);
            this.rtbContent.Name = "rtbContent";
            this.rtbContent.ReadOnly = true;
            this.rtbContent.Size = new System.Drawing.Size(967, 368);
            this.rtbContent.TabIndex = 4;
            this.rtbContent.Text = "";
            // 
            // picImagePreview
            // 
            this.picImagePreview.Location = new System.Drawing.Point(63, 299);
            this.picImagePreview.Name = "picImagePreview";
            this.picImagePreview.Size = new System.Drawing.Size(967, 368);
            this.picImagePreview.TabIndex = 5;
            this.picImagePreview.TabStop = false;
            // 
            // wbPdfPreview
            // 
            this.wbPdfPreview.Location = new System.Drawing.Point(63, 299);
            this.wbPdfPreview.MinimumSize = new System.Drawing.Size(20, 20);
            this.wbPdfPreview.Name = "wbPdfPreview";
            this.wbPdfPreview.Size = new System.Drawing.Size(967, 368);
            this.wbPdfPreview.TabIndex = 6;
            // 
            // FilePreview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.wbPdfPreview);
            this.Controls.Add(this.picImagePreview);
            this.Controls.Add(this.rtbContent);
            this.Controls.Add(this.btnPreview);
            this.Controls.Add(this.lblBrowseFile);
            this.Controls.Add(this.cbBrowseFile);
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "FilePreview";
            this.Size = new System.Drawing.Size(1102, 724);
            ((System.ComponentModel.ISupportInitialize)(this.picImagePreview)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbBrowseFile;
        private System.Windows.Forms.Label lblBrowseFile;
        private System.Windows.Forms.Button btnPreview;
        private System.Windows.Forms.RichTextBox rtbContent;
        private System.Windows.Forms.PictureBox picImagePreview;
        private System.Windows.Forms.WebBrowser wbPdfPreview;
    }
}
