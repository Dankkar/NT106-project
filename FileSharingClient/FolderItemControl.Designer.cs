namespace FileSharingClient
{
    partial class FolderItemControl
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
            this.lblFolderName = new System.Windows.Forms.Label();
            this.lblOwner = new System.Windows.Forms.Label();
            this.lblCreatedAt = new System.Windows.Forms.Label();
            this.lblType = new System.Windows.Forms.Label();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.shareToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnMore = new System.Windows.Forms.Button();
            this.lblFolderIcon = new System.Windows.Forms.Label();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblFolderName
            // 
            this.lblFolderName.AutoSize = true;
            this.lblFolderName.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFolderName.Location = new System.Drawing.Point(39, 12);
            this.lblFolderName.Name = "lblFolderName";
            this.lblFolderName.Size = new System.Drawing.Size(64, 17);
            this.lblFolderName.TabIndex = 0;
            this.lblFolderName.Text = "FolderName";
            // 
            // lblOwner
            // 
            this.lblOwner.AutoSize = true;
            this.lblOwner.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOwner.Location = new System.Drawing.Point(248, 12);
            this.lblOwner.Name = "lblOwner";
            this.lblOwner.Size = new System.Drawing.Size(48, 17);
            this.lblOwner.TabIndex = 1;
            this.lblOwner.Text = "Owner";
            // 
            // lblCreatedAt
            // 
            this.lblCreatedAt.AutoSize = true;
            this.lblCreatedAt.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCreatedAt.Location = new System.Drawing.Point(420, 12);
            this.lblCreatedAt.Name = "lblCreatedAt";
            this.lblCreatedAt.Size = new System.Drawing.Size(37, 17);
            this.lblCreatedAt.TabIndex = 2;
            this.lblCreatedAt.Text = "Time";
            // 
            // lblType
            // 
            this.lblType.AutoSize = true;
            this.lblType.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblType.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblType.Location = new System.Drawing.Point(450, 12);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(51, 17);
            this.lblType.TabIndex = 3;
            this.lblType.Text = "Folder";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.shareToolStripMenuItem,
            this.deleteToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(108, 48);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // shareToolStripMenuItem
            // 
            this.shareToolStripMenuItem.Name = "shareToolStripMenuItem";
            this.shareToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.shareToolStripMenuItem.Text = "Share";
            this.shareToolStripMenuItem.Click += new System.EventHandler(this.shareToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // btnMore
            // 
            this.btnMore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMore.AutoSize = true;
            this.btnMore.FlatAppearance.BorderSize = 0;
            this.btnMore.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMore.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.btnMore.Location = new System.Drawing.Point(550, 10);
            this.btnMore.Name = "btnMore";
            this.btnMore.Size = new System.Drawing.Size(75, 23);
            this.btnMore.TabIndex = 5;
            this.btnMore.Text = "...";
            this.btnMore.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.btnMore.UseVisualStyleBackColor = true;
            // 
            // lblFolderIcon
            // 
            this.lblFolderIcon.AutoSize = true;
            this.lblFolderIcon.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFolderIcon.Location = new System.Drawing.Point(10, 12);
            this.lblFolderIcon.Name = "lblFolderIcon";
            this.lblFolderIcon.Size = new System.Drawing.Size(23, 21);
            this.lblFolderIcon.TabIndex = 6;
            this.lblFolderIcon.Text = "📁";
            // 
            // FolderItemControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.Controls.Add(this.lblFolderIcon);
            this.Controls.Add(this.btnMore);
            this.Controls.Add(this.lblType);
            this.Controls.Add(this.lblCreatedAt);
            this.Controls.Add(this.lblOwner);
            this.Controls.Add(this.lblFolderName);
            this.Name = "FolderItemControl";
            this.Size = new System.Drawing.Size(650, 43);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblFolderName;
        private System.Windows.Forms.Label lblOwner;
        private System.Windows.Forms.Label lblCreatedAt;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem shareToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.Button btnMore;
        private System.Windows.Forms.Label lblFolderIcon;
    }
} 