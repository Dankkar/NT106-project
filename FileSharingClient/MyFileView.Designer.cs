namespace FileSharingClient
{
    partial class MyFileView
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
            this.MyFile_dataGridView = new System.Windows.Forms.DataGridView();
            this.FileName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FileSize = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FileExtension = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DateUpload = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Option = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.MyFile_dataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // MyFile_dataGridView
            // 
            this.MyFile_dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.MyFile_dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.FileName,
            this.FileSize,
            this.FileExtension,
            this.DateUpload,
            this.Option});
            this.MyFile_dataGridView.Location = new System.Drawing.Point(122, 19);
            this.MyFile_dataGridView.Name = "MyFile_dataGridView";
            this.MyFile_dataGridView.Size = new System.Drawing.Size(544, 424);
            this.MyFile_dataGridView.TabIndex = 0;
            // 
            // FileName
            // 
            this.FileName.HeaderText = "Tên File";
            this.FileName.Name = "FileName";
            this.FileName.ReadOnly = true;
            this.FileName.Width = 200;
            // 
            // FileSize
            // 
            this.FileSize.HeaderText = "Kích thước File";
            this.FileSize.Name = "FileSize";
            this.FileSize.ReadOnly = true;
            this.FileSize.Width = 75;
            // 
            // FileExtension
            // 
            this.FileExtension.HeaderText = "Đuôi mở rộng";
            this.FileExtension.Name = "FileExtension";
            this.FileExtension.ReadOnly = true;
            this.FileExtension.Width = 75;
            // 
            // DateUpload
            // 
            this.DateUpload.HeaderText = "Ngày tải lên";
            this.DateUpload.Name = "DateUpload";
            this.DateUpload.ReadOnly = true;
            // 
            // Option
            // 
            this.Option.HeaderText = "Tùy Chọn";
            this.Option.Name = "Option";
            this.Option.Width = 50;
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.Name = "contextMenuStrip2";
            this.contextMenuStrip2.Size = new System.Drawing.Size(61, 4);
            // 
            // MyFileView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.MyFile_dataGridView);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "MyFileView";
            this.Size = new System.Drawing.Size(806, 460);
            ((System.ComponentModel.ISupportInitialize)(this.MyFile_dataGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView MyFile_dataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn FileName;
        private System.Windows.Forms.DataGridViewTextBoxColumn FileSize;
        private System.Windows.Forms.DataGridViewTextBoxColumn FileExtension;
        private System.Windows.Forms.DataGridViewTextBoxColumn DateUpload;
        private System.Windows.Forms.DataGridViewTextBoxColumn Option;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
    }
}
