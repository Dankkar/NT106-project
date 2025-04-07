namespace FileSharingClient
{
    partial class TrashBinView
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.FileName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FileSize = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FileExtension = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Delete_at = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Recovery = new System.Windows.Forms.DataGridViewButtonColumn();
            this.PermanentDelete = new System.Windows.Forms.DataGridViewButtonColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.FileName,
            this.FileSize,
            this.FileExtension,
            this.Delete_at,
            this.Recovery,
            this.PermanentDelete});
            this.dataGridView1.Location = new System.Drawing.Point(0, 3);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 33;
            this.dataGridView1.Size = new System.Drawing.Size(1202, 754);
            this.dataGridView1.TabIndex = 0;
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
            this.FileSize.HeaderText = "Kích thước";
            this.FileSize.Name = "FileSize";
            this.FileSize.ReadOnly = true;
            this.FileSize.Width = 50;
            // 
            // FileExtension
            // 
            this.FileExtension.HeaderText = "Đuôi File";
            this.FileExtension.Name = "FileExtension";
            this.FileExtension.ReadOnly = true;
            // 
            // Delete_at
            // 
            this.Delete_at.HeaderText = "Ngày Xóa";
            this.Delete_at.Name = "Delete_at";
            this.Delete_at.ReadOnly = true;
            // 
            // Recovery
            // 
            this.Recovery.HeaderText = "Phục hồi";
            this.Recovery.Name = "Recovery";
            this.Recovery.Width = 70;
            // 
            // PermanentDelete
            // 
            this.PermanentDelete.HeaderText = "Xóa";
            this.PermanentDelete.Name = "PermanentDelete";
            this.PermanentDelete.Width = 50;
            // 
            // TrashBinView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dataGridView1);
            this.Name = "TrashBinView";
            this.Size = new System.Drawing.Size(1202, 754);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn FileName;
        private System.Windows.Forms.DataGridViewTextBoxColumn FileSize;
        private System.Windows.Forms.DataGridViewTextBoxColumn FileExtension;
        private System.Windows.Forms.DataGridViewTextBoxColumn Delete_at;
        private System.Windows.Forms.DataGridViewButtonColumn Recovery;
        private System.Windows.Forms.DataGridViewButtonColumn PermanentDelete;
    }
}
