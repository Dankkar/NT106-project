namespace FileSharingClient
{
    partial class FileUploadItemControl
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
            this.btnRemove = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblFileIcon
            // 
            this.lblFileIcon.AutoSize = true;
            this.lblFileIcon.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblFileIcon.Location = new System.Drawing.Point(12, 10);
            this.lblFileIcon.Name = "lblFileIcon";
            this.lblFileIcon.Size = new System.Drawing.Size(32, 25);
            this.lblFileIcon.TabIndex = 9;
            this.lblFileIcon.Text = "📄";
            // 
            // lblFileName
            // 
            this.lblFileName.AutoSize = false;
            this.lblFileName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblFileName.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblFileName.Location = new System.Drawing.Point(50, 3);
            this.lblFileName.Size = new System.Drawing.Size(180, 42);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.TabIndex = 0;
            this.lblFileName.Text = "FileName";
            // 
            // lblOwner
            // 
            this.lblOwner.AutoSize = false;
            this.lblOwner.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblOwner.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblOwner.Location = new System.Drawing.Point(240, 3);
            this.lblOwner.Size = new System.Drawing.Size(120, 42);
            this.lblOwner.Name = "lblOwner";
            this.lblOwner.TabIndex = 1;
            this.lblOwner.Text = "Owner";
            // 
            // lblFileSize
            // 
            this.lblFileSize.AutoSize = false;
            this.lblFileSize.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblFileSize.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblFileSize.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblFileSize.Location = new System.Drawing.Point(370, 3);
            this.lblFileSize.Size = new System.Drawing.Size(100, 42);
            this.lblFileSize.Name = "lblFileSize";
            this.lblFileSize.TabIndex = 3;
            this.lblFileSize.Text = "FileSize";
            // 
            // lblFileType
            // 
            this.lblFileType.AutoSize = false;
            this.lblFileType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblFileType.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblFileType.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblFileType.Location = new System.Drawing.Point(480, 3);
            this.lblFileType.Size = new System.Drawing.Size(80, 42);
            this.lblFileType.Name = "lblFileType";
            this.lblFileType.TabIndex = 4;
            this.lblFileType.Text = "FileType";
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
            // FileUploadItemControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = false;
            this.Height = 48;
            this.Margin = new System.Windows.Forms.Padding(0);
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.lblFileType);
            this.Controls.Add(this.lblFileSize);
            this.Controls.Add(this.lblOwner);
            this.Controls.Add(this.lblFileName);
            this.Controls.Add(this.lblFileIcon);
            this.Name = "FileUploadItemControl";
            this.Size = new System.Drawing.Size(790, 48);
            this.Click += new System.EventHandler(this.FileUploadItemControl_Click);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblFileIcon;
        private System.Windows.Forms.Label lblFileName;
        private System.Windows.Forms.Label lblOwner;
        private System.Windows.Forms.Label lblFileSize;
        private System.Windows.Forms.Label lblFileType;
        private System.Windows.Forms.Button btnRemove;
    }
} 