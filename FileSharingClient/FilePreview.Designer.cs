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
            this.treeView = new System.Windows.Forms.TreeView();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.previewImage = new System.Windows.Forms.PictureBox();
            this.previewText = new System.Windows.Forms.RichTextBox();
            this.previewPdf = new System.Windows.Forms.WebBrowser();
            ((System.ComponentModel.ISupportInitialize)(this.previewImage)).BeginInit();
            this.SuspendLayout();
            // 
            // treeView
            // 
            this.treeView.Location = new System.Drawing.Point(2, 3);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(400, 552);
            this.treeView.TabIndex = 0;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // previewImage
            // 
            this.previewImage.Location = new System.Drawing.Point(408, 3);
            this.previewImage.Name = "previewImage";
            this.previewImage.Size = new System.Drawing.Size(748, 552);
            this.previewImage.TabIndex = 3;
            this.previewImage.TabStop = false;
            // 
            // previewText
            // 
            this.previewText.Location = new System.Drawing.Point(408, 3);
            this.previewText.Name = "previewText";
            this.previewText.ReadOnly = true;
            this.previewText.Size = new System.Drawing.Size(748, 552);
            this.previewText.TabIndex = 4;
            this.previewText.Text = "";
            // 
            // previewPdf
            // 
            this.previewPdf.Location = new System.Drawing.Point(408, 3);
            this.previewPdf.MinimumSize = new System.Drawing.Size(20, 20);
            this.previewPdf.Name = "previewPdf";
            this.previewPdf.Size = new System.Drawing.Size(748, 552);
            this.previewPdf.TabIndex = 1;
            // 
            // FilePreview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::FileSharingClient.Properties.Resources.blue_background;
            this.Controls.Add(this.previewPdf);
            this.Controls.Add(this.previewText);
            this.Controls.Add(this.previewImage);
            this.Controls.Add(this.treeView);
            this.Name = "FilePreview";
            this.Size = new System.Drawing.Size(1159, 558);
            ((System.ComponentModel.ISupportInitialize)(this.previewImage)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.PictureBox previewImage;
        private System.Windows.Forms.RichTextBox previewText;
        private System.Windows.Forms.WebBrowser previewPdf;
    }
}
