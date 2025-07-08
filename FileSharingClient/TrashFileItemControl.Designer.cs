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
            this.lblDeletedAt = new System.Windows.Forms.Label();
            this.lblFileSize = new System.Windows.Forms.Label();
            this.lblFileType = new System.Windows.Forms.Label();
            this.btnRestore = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblFileIcon
            // 
            this.lblFileIcon.AutoSize = true;
            this.lblFileIcon.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileIcon.Location = new System.Drawing.Point(5, 10);
            this.lblFileIcon.Name = "lblFileIcon";
            this.lblFileIcon.Size = new System.Drawing.Size(23, 20);
            this.lblFileIcon.TabIndex = 0;
            this.lblFileIcon.Text = "📄";
            // 
            // lblFileName
            // 
            this.lblFileName.AutoSize = true;
            this.lblFileName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileName.Location = new System.Drawing.Point(35, 12);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(61, 15);
            this.lblFileName.TabIndex = 1;
            this.lblFileName.Text = "FileName";
            this.lblFileName.Size = new System.Drawing.Size(150, 15);
            // 
            // lblOwner
            // 
            this.lblOwner.AutoSize = true;
            this.lblOwner.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOwner.Location = new System.Drawing.Point(200, 12);
            this.lblOwner.Name = "lblOwner";
            this.lblOwner.Size = new System.Drawing.Size(44, 15);
            this.lblOwner.TabIndex = 2;
            this.lblOwner.Text = "Owner";
            this.lblOwner.Size = new System.Drawing.Size(100, 15);
            // 
            // lblDeletedAt
            // 
            this.lblDeletedAt.AutoSize = true;
            this.lblDeletedAt.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDeletedAt.Location = new System.Drawing.Point(320, 12);
            this.lblDeletedAt.Name = "lblDeletedAt";
            this.lblDeletedAt.Size = new System.Drawing.Size(62, 15);
            this.lblDeletedAt.TabIndex = 3;
            this.lblDeletedAt.Text = "DeletedAt";
            this.lblDeletedAt.Size = new System.Drawing.Size(120, 15);
            // 
            // lblFileSize
            // 
            this.lblFileSize.AutoSize = true;
            this.lblFileSize.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileSize.Location = new System.Drawing.Point(450, 12);
            this.lblFileSize.Name = "lblFileSize";
            this.lblFileSize.Size = new System.Drawing.Size(51, 15);
            this.lblFileSize.TabIndex = 4;
            this.lblFileSize.Text = "FileSize";
            this.lblFileSize.Size = new System.Drawing.Size(80, 15);
            // 
            // lblFileType
            // 
            this.lblFileType.AutoSize = true;
            this.lblFileType.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileType.Location = new System.Drawing.Point(550, 12);
            this.lblFileType.Name = "lblFileType";
            this.lblFileType.Size = new System.Drawing.Size(54, 15);
            this.lblFileType.TabIndex = 5;
            this.lblFileType.Text = "FileType";
            this.lblFileType.Size = new System.Drawing.Size(70, 15);
            // 
            // btnRestore
            // 
            this.btnRestore.BackColor = System.Drawing.Color.LightGreen;
            this.btnRestore.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRestore.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRestore.Location = new System.Drawing.Point(640, 8);
            this.btnRestore.Name = "btnRestore";
            this.btnRestore.Size = new System.Drawing.Size(60, 23);
            this.btnRestore.TabIndex = 6;
            this.btnRestore.Text = "Phục hồi";
            this.btnRestore.UseVisualStyleBackColor = false;
            this.btnRestore.Click += new System.EventHandler(this.btnRestore_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.BackColor = System.Drawing.Color.LightCoral;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDelete.Location = new System.Drawing.Point(710, 8);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(60, 23);
            this.btnDelete.TabIndex = 7;
            this.btnDelete.Text = "Xóa";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // TrashFileItemControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
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
            this.Size = new System.Drawing.Size(780, 40);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblFileIcon;
        private System.Windows.Forms.Label lblFileName;
        private System.Windows.Forms.Label lblOwner;
        private System.Windows.Forms.Label lblDeletedAt;
        private System.Windows.Forms.Label lblFileSize;
        private System.Windows.Forms.Label lblFileType;
        private System.Windows.Forms.Button btnRestore;
        private System.Windows.Forms.Button btnDelete;
    }
} 