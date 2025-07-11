namespace FileSharingClient
{
    partial class TrashFileItemControl
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
            this.lblFileIcon = new System.Windows.Forms.Label();
            this.lblFileName = new System.Windows.Forms.Label();
            this.lblOwner = new System.Windows.Forms.Label();
            this.lblFileSize = new System.Windows.Forms.Label();
            this.lblFileType = new System.Windows.Forms.Label();
            this.btnRestore = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.lblDeletedAt = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblFileIcon
            // 
            this.lblFileIcon.AutoSize = true;
            this.lblFileIcon.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.lblFileIcon.Location = new System.Drawing.Point(12, 10);
            this.lblFileIcon.Name = "lblFileIcon";
            this.lblFileIcon.Size = new System.Drawing.Size(33, 25);
            this.lblFileIcon.TabIndex = 0;
            this.lblFileIcon.Text = "📄";
            // 
            // lblFileName
            // 
            this.lblFileName.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblFileName.Location = new System.Drawing.Point(50, 3);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(200, 39);
            this.lblFileName.TabIndex = 1;
            this.lblFileName.Text = "FileName";
            this.lblFileName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblOwner
            // 
            this.lblOwner.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblOwner.Location = new System.Drawing.Point(250, 3);
            this.lblOwner.Name = "lblOwner";
            this.lblOwner.Size = new System.Drawing.Size(150, 39);
            this.lblOwner.TabIndex = 2;
            this.lblOwner.Text = "Owner";
            this.lblOwner.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblFileSize
            // 
            this.lblFileSize.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblFileSize.Location = new System.Drawing.Point(550, 3);
            this.lblFileSize.Name = "lblFileSize";
            this.lblFileSize.Size = new System.Drawing.Size(150, 39);
            this.lblFileSize.TabIndex = 4;
            this.lblFileSize.Text = "FileSize";
            this.lblFileSize.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblFileType
            // 
            this.lblFileType.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblFileType.Location = new System.Drawing.Point(700, 3);
            this.lblFileType.Name = "lblFileType";
            this.lblFileType.Size = new System.Drawing.Size(100, 39);
            this.lblFileType.TabIndex = 5;
            this.lblFileType.Text = "FileType";
            this.lblFileType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnRestore
            // 
            this.btnRestore.BackColor = System.Drawing.Color.LightGreen;
            this.btnRestore.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRestore.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnRestore.Location = new System.Drawing.Point(810, 8);
            this.btnRestore.Name = "btnRestore";
            this.btnRestore.Size = new System.Drawing.Size(75, 29);
            this.btnRestore.TabIndex = 6;
            this.btnRestore.Text = "Phục hồi";
            this.btnRestore.UseVisualStyleBackColor = false;
            this.btnRestore.Click += new System.EventHandler(this.btnRestore_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.BackColor = System.Drawing.Color.LightCoral;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnDelete.Location = new System.Drawing.Point(890, 8);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(50, 29);
            this.btnDelete.TabIndex = 7;
            this.btnDelete.Text = "Xóa";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // lblDeletedAt
            // 
            this.lblDeletedAt.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblDeletedAt.Location = new System.Drawing.Point(400, 3);
            this.lblDeletedAt.Name = "lblDeletedAt";
            this.lblDeletedAt.Size = new System.Drawing.Size(150, 39);
            this.lblDeletedAt.TabIndex = 3;
            this.lblDeletedAt.Text = "DeletedAt";
            this.lblDeletedAt.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // TrashFileItemControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnRestore);
            this.Controls.Add(this.lblFileType);
            this.Controls.Add(this.lblFileSize);
            this.Controls.Add(this.lblDeletedAt);
            this.Controls.Add(this.lblOwner);
            this.Controls.Add(this.lblFileName);
            this.Controls.Add(this.lblFileIcon);
            this.Name = "TrashFileItemControl";
            this.Size = new System.Drawing.Size(1080, 45);
            this.Load += new System.EventHandler(this.TrashFileItemControl_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblFileIcon;
        private System.Windows.Forms.Label lblFileName;
        private System.Windows.Forms.Label lblOwner;
        private System.Windows.Forms.Label lblFileSize;
        private System.Windows.Forms.Label lblFileType;
        private System.Windows.Forms.Button btnRestore;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Label lblDeletedAt;
    }
} 