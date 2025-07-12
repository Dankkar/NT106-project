namespace FileSharingClient
{
    partial class FolderUploadItemControl
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
            this.lblFolderIcon = new System.Windows.Forms.Label();
            this.lblFolderName = new System.Windows.Forms.Label();
            this.lblOwner = new System.Windows.Forms.Label();
            this.lblFolderSize = new System.Windows.Forms.Label();
            this.lblFolderType = new System.Windows.Forms.Label();
            this.btnRemove = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblFolderIcon
            // 
            this.lblFolderIcon.AutoSize = true;
            this.lblFolderIcon.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblFolderIcon.Location = new System.Drawing.Point(12, 10);
            this.lblFolderIcon.Name = "lblFolderIcon";
            this.lblFolderIcon.Size = new System.Drawing.Size(34, 25);
            this.lblFolderIcon.TabIndex = 9;
            this.lblFolderIcon.Text = "📁";
            // 
            // lblFolderName
            // 
            this.lblFolderName.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblFolderName.Location = new System.Drawing.Point(50, 3);
            this.lblFolderName.Name = "lblFolderName";
            this.lblFolderName.Size = new System.Drawing.Size(180, 42);
            this.lblFolderName.TabIndex = 0;
            this.lblFolderName.Text = "FolderName";
            this.lblFolderName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblOwner
            // 
            this.lblOwner.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblOwner.Location = new System.Drawing.Point(240, 3);
            this.lblOwner.Name = "lblOwner";
            this.lblOwner.Size = new System.Drawing.Size(120, 42);
            this.lblOwner.TabIndex = 1;
            this.lblOwner.Text = "Owner";
            this.lblOwner.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblFolderSize
            // 
            this.lblFolderSize.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblFolderSize.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblFolderSize.Location = new System.Drawing.Point(370, 3);
            this.lblFolderSize.Name = "lblFolderSize";
            this.lblFolderSize.Size = new System.Drawing.Size(100, 42);
            this.lblFolderSize.TabIndex = 3;
            this.lblFolderSize.Text = "FolderSize";
            this.lblFolderSize.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblFolderType
            // 
            this.lblFolderType.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblFolderType.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblFolderType.Location = new System.Drawing.Point(480, 3);
            this.lblFolderType.Name = "lblFolderType";
            this.lblFolderType.Size = new System.Drawing.Size(80, 42);
            this.lblFolderType.TabIndex = 4;
            this.lblFolderType.Text = "Folder";
            this.lblFolderType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnRemove
            // 
            this.btnRemove.BackColor = System.Drawing.Color.LightCoral;
            this.btnRemove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRemove.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.btnRemove.Location = new System.Drawing.Point(570, 8);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(80, 32);
            this.btnRemove.TabIndex = 5;
            this.btnRemove.Text = "Xóa";
            this.btnRemove.UseVisualStyleBackColor = false;
            // 
            // FolderUploadItemControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.lblFolderType);
            this.Controls.Add(this.lblFolderSize);
            this.Controls.Add(this.lblOwner);
            this.Controls.Add(this.lblFolderName);
            this.Controls.Add(this.lblFolderIcon);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "FolderUploadItemControl";
            this.Size = new System.Drawing.Size(790, 48);
            this.Click += new System.EventHandler(this.FolderUploadItemControl_Click);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblFolderIcon;
        private System.Windows.Forms.Label lblFolderName;
        private System.Windows.Forms.Label lblOwner;
        private System.Windows.Forms.Label lblFolderSize;
        private System.Windows.Forms.Label lblFolderType;
        private System.Windows.Forms.Button btnRemove;
    }
} 