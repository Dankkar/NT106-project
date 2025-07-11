namespace FileSharingClient
{
    partial class TrashFolderItemControl
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
            this.lblFolderName = new System.Windows.Forms.Label();
            this.lblOwner = new System.Windows.Forms.Label();
            this.lblDeletedAt = new System.Windows.Forms.Label();
            this.lblType = new System.Windows.Forms.Label();
            this.btnRestore = new System.Windows.Forms.Button();
            this.btnPermanentDelete = new System.Windows.Forms.Button();
            this.lblFolderIcon = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblFolderIcon
            // 
            this.lblFolderIcon.AutoSize = true;
            this.lblFolderIcon.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Regular);
            this.lblFolderIcon.Location = new System.Drawing.Point(12, 10);
            this.lblFolderIcon.Name = "lblFolderIcon";
            this.lblFolderIcon.Size = new System.Drawing.Size(32, 25);
            this.lblFolderIcon.TabIndex = 10;
            this.lblFolderIcon.Text = "📁";
            // 
            // lblFolderName
            // 
            this.lblFolderName.AutoSize = false;
            this.lblFolderName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblFolderName.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblFolderName.Location = new System.Drawing.Point(50, 3);
            this.lblFolderName.Size = new System.Drawing.Size(200, 39);
            this.lblFolderName.Name = "lblFolderName";
            this.lblFolderName.TabIndex = 0;
            this.lblFolderName.Text = "Folder Name";
            // 
            // lblOwner
            // 
            this.lblOwner.AutoSize = false;
            this.lblOwner.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblOwner.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            this.lblOwner.Location = new System.Drawing.Point(250, 3);
            this.lblOwner.Size = new System.Drawing.Size(150, 39);
            this.lblOwner.Name = "lblOwner";
            this.lblOwner.TabIndex = 1;
            this.lblOwner.Text = "Owner";
            // 
            // lblDeletedAt
            // 
            this.lblDeletedAt.AutoSize = false;
            this.lblDeletedAt.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblDeletedAt.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            this.lblDeletedAt.Location = new System.Drawing.Point(400, 3);
            this.lblDeletedAt.Size = new System.Drawing.Size(300, 39);
            this.lblDeletedAt.Name = "lblDeletedAt";
            this.lblDeletedAt.TabIndex = 2;
            this.lblDeletedAt.Text = "Deleted At";
            // 
            // lblType
            // 
            this.lblType.AutoSize = false;
            this.lblType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblType.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            this.lblType.Location = new System.Drawing.Point(700, 3);
            this.lblType.Size = new System.Drawing.Size(100, 39);
            this.lblType.Name = "lblType";
            this.lblType.TabIndex = 3;
            this.lblType.Text = "Folder";
            // 
            // btnRestore
            // 
            this.btnRestore.BackColor = System.Drawing.Color.LightGreen;
            this.btnRestore.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRestore.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            this.btnRestore.Location = new System.Drawing.Point(810, 8);
            this.btnRestore.Name = "btnRestore";
            this.btnRestore.Size = new System.Drawing.Size(75, 29);
            this.btnRestore.TabIndex = 4;
            this.btnRestore.Text = "Phục hồi";
            this.btnRestore.UseVisualStyleBackColor = false;
            // 
            // btnPermanentDelete
            // 
            this.btnPermanentDelete.BackColor = System.Drawing.Color.LightCoral;
            this.btnPermanentDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPermanentDelete.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            this.btnPermanentDelete.Location = new System.Drawing.Point(890, 8);
            this.btnPermanentDelete.Name = "btnPermanentDelete";
            this.btnPermanentDelete.Size = new System.Drawing.Size(50, 29);
            this.btnPermanentDelete.TabIndex = 5;
            this.btnPermanentDelete.Text = "Xóa";
            this.btnPermanentDelete.UseVisualStyleBackColor = false;
            // 
            // TrashFolderItemControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightBlue;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.btnPermanentDelete);
            this.Controls.Add(this.btnRestore);
            this.Controls.Add(this.lblType);
            this.Controls.Add(this.lblDeletedAt);
            this.Controls.Add(this.lblOwner);
            this.Controls.Add(this.lblFolderName);
            this.Controls.Add(this.lblFolderIcon);
            this.Name = "TrashFolderItemControl";
            this.Size = new System.Drawing.Size(1080, 45);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblFolderName;
        private System.Windows.Forms.Label lblOwner;
        private System.Windows.Forms.Label lblDeletedAt;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.Button btnRestore;
        private System.Windows.Forms.Button btnPermanentDelete;
        private System.Windows.Forms.Label lblFolderIcon;
    }
} 