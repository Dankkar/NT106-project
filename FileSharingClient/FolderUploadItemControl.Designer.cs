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
            this.lblFolderIcon.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFolderIcon.Location = new System.Drawing.Point(10, 10);
            this.lblFolderIcon.Name = "lblFolderIcon";
            this.lblFolderIcon.Size = new System.Drawing.Size(25, 21);
            this.lblFolderIcon.TabIndex = 0;
            this.lblFolderIcon.Text = "📁";
            // 
            // lblFolderName
            // 
            this.lblFolderName.AutoSize = true;
            this.lblFolderName.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFolderName.Location = new System.Drawing.Point(39, 12);
            this.lblFolderName.Name = "lblFolderName";
            this.lblFolderName.Size = new System.Drawing.Size(80, 17);
            this.lblFolderName.TabIndex = 1;
            this.lblFolderName.Text = "Folder Name";
            // 
            // lblOwner
            // 
            this.lblOwner.AutoSize = true;
            this.lblOwner.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOwner.Location = new System.Drawing.Point(248, 12);
            this.lblOwner.Name = "lblOwner";
            this.lblOwner.Size = new System.Drawing.Size(45, 17);
            this.lblOwner.TabIndex = 2;
            this.lblOwner.Text = "Owner";
            // 
            // lblFolderSize
            // 
            this.lblFolderSize.AutoSize = true;
            this.lblFolderSize.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFolderSize.Location = new System.Drawing.Point(565, 12);
            this.lblFolderSize.Name = "lblFolderSize";
            this.lblFolderSize.Size = new System.Drawing.Size(72, 17);
            this.lblFolderSize.TabIndex = 4;
            this.lblFolderSize.Text = "Folder Size";
            // 
            // lblFolderType
            // 
            this.lblFolderType.AutoSize = true;
            this.lblFolderType.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFolderType.Location = new System.Drawing.Point(733, 12);
            this.lblFolderType.Name = "lblFolderType";
            this.lblFolderType.Size = new System.Drawing.Size(35, 17);
            this.lblFolderType.TabIndex = 5;
            this.lblFolderType.Text = "Type";
            // 
            // btnRemove
            // 
            this.btnRemove.BackColor = System.Drawing.Color.LightCoral;
            this.btnRemove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRemove.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRemove.ForeColor = System.Drawing.Color.White;
            this.btnRemove.Location = new System.Drawing.Point(1253, 6);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(75, 28);
            this.btnRemove.TabIndex = 6;
            this.btnRemove.Text = "Remove";
            this.btnRemove.UseVisualStyleBackColor = false;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // FolderUploadItemControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.lblFolderType);
            this.Controls.Add(this.lblFolderSize);
            this.Controls.Add(this.lblOwner);
            this.Controls.Add(this.lblFolderName);
            this.Controls.Add(this.lblFolderIcon);
            this.Name = "FolderUploadItemControl";
            this.Size = new System.Drawing.Size(1340, 40);
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