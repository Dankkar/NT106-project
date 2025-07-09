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
            if (disposing)
            {
                // Call our cleanup method
                CleanupEvents();
                
                if (components != null)
                {
                    components.Dispose();
                }
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
            this.TrashFileLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.lblFileName = new System.Windows.Forms.Label();
            this.lblOwner = new System.Windows.Forms.Label();
            this.lblDeletedAt = new System.Windows.Forms.Label();
            this.lblSize = new System.Windows.Forms.Label();
            this.lblType = new System.Windows.Forms.Label();
            this.lblActions = new System.Windows.Forms.Label();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnRestoreAll = new System.Windows.Forms.ToolStripButton();
            this.btnEmptyTrash = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.txtSearch = new System.Windows.Forms.ToolStripTextBox();
            this.btnSearch = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnRefresh = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // TrashFileLayoutPanel
            // 
            this.TrashFileLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TrashFileLayoutPanel.AutoScroll = true;
            this.TrashFileLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.TrashFileLayoutPanel.Location = new System.Drawing.Point(0, 50);
            this.TrashFileLayoutPanel.Name = "TrashFileLayoutPanel";
            this.TrashFileLayoutPanel.Size = new System.Drawing.Size(800, 400);
            this.TrashFileLayoutPanel.TabIndex = 1;
            this.TrashFileLayoutPanel.WrapContents = false;
            // 
            // lblFileName
            // 
            this.lblFileName.AutoSize = true;
            this.lblFileName.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileName.Location = new System.Drawing.Point(32, 32);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(53, 17);
            this.lblFileName.TabIndex = 2;
            this.lblFileName.Text = "Tên File";
            // 
            // lblOwner
            // 
            this.lblOwner.AutoSize = true;
            this.lblOwner.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOwner.Location = new System.Drawing.Point(200, 32);
            this.lblOwner.Name = "lblOwner";
            this.lblOwner.Size = new System.Drawing.Size(91, 17);
            this.lblOwner.TabIndex = 3;
            this.lblOwner.Text = "Người sở hữu";
            // 
            // lblDeletedAt
            // 
            this.lblDeletedAt.AutoSize = true;
            this.lblDeletedAt.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDeletedAt.Location = new System.Drawing.Point(320, 32);
            this.lblDeletedAt.Name = "lblDeletedAt";
            this.lblDeletedAt.Size = new System.Drawing.Size(66, 17);
            this.lblDeletedAt.TabIndex = 4;
            this.lblDeletedAt.Text = "Ngày xóa";
            // 
            // lblSize
            // 
            this.lblSize.AutoSize = true;
            this.lblSize.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSize.Location = new System.Drawing.Point(450, 32);
            this.lblSize.Name = "lblSize";
            this.lblSize.Size = new System.Drawing.Size(72, 17);
            this.lblSize.TabIndex = 5;
            this.lblSize.Text = "Kích thước";
            // 
            // lblType
            // 
            this.lblType.AutoSize = true;
            this.lblType.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblType.Location = new System.Drawing.Point(550, 32);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(32, 17);
            this.lblType.TabIndex = 6;
            this.lblType.Text = "Loại";
            // 
            // lblActions
            // 
            this.lblActions.AutoSize = true;
            this.lblActions.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblActions.Location = new System.Drawing.Point(650, 32);
            this.lblActions.Name = "lblActions";
            this.lblActions.Size = new System.Drawing.Size(60, 17);
            this.lblActions.TabIndex = 7;
            this.lblActions.Text = "Thao tác";
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnRestoreAll,
            this.btnEmptyTrash,
            this.toolStripSeparator1,
            this.txtSearch,
            this.btnSearch,
            this.toolStripSeparator2,
            this.btnRefresh});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(800, 25);
            this.toolStrip1.TabIndex = 8;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnRestoreAll
            // 
            this.btnRestoreAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRestoreAll.Name = "btnRestoreAll";
            this.btnRestoreAll.Size = new System.Drawing.Size(90, 22);
            this.btnRestoreAll.Text = "Phục hồi tất cả";
            this.btnRestoreAll.Click += new System.EventHandler(this.btnRestoreAll_Click);
            // 
            // btnEmptyTrash
            // 
            this.btnEmptyTrash.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnEmptyTrash.Name = "btnEmptyTrash";
            this.btnEmptyTrash.Size = new System.Drawing.Size(87, 22);
            this.btnEmptyTrash.Text = "Dọn thùng rác";
            this.btnEmptyTrash.Click += new System.EventHandler(this.btnEmptyTrash_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // txtSearch
            // 
            this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(111, 25);
            this.txtSearch.Text = "Tìm kiếm file...";
            this.txtSearch.ToolTipText = "Nhập tên file để tìm kiếm";
            this.txtSearch.Enter += new System.EventHandler(this.txtSearch_Enter);
            this.txtSearch.Leave += new System.EventHandler(this.txtSearch_Leave);
            // 
            // btnSearch
            // 
            this.btnSearch.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(61, 22);
            this.btnSearch.Text = "Tìm kiếm";
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // btnRefresh
            // 
            this.btnRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(58, 22);
            this.btnRefresh.Text = "Làm mới";
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // TrashBinView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.lblActions);
            this.Controls.Add(this.lblType);
            this.Controls.Add(this.lblSize);
            this.Controls.Add(this.lblDeletedAt);
            this.Controls.Add(this.lblOwner);
            this.Controls.Add(this.lblFileName);
            this.Controls.Add(this.TrashFileLayoutPanel);
            this.Name = "TrashBinView";
            this.Size = new System.Drawing.Size(800, 450);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel TrashFileLayoutPanel;
        private System.Windows.Forms.Label lblFileName;
        private System.Windows.Forms.Label lblOwner;
        private System.Windows.Forms.Label lblDeletedAt;
        private System.Windows.Forms.Label lblSize;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.Label lblActions;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnRestoreAll;
        private System.Windows.Forms.ToolStripButton btnEmptyTrash;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripTextBox txtSearch;
        private System.Windows.Forms.ToolStripButton btnSearch;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton btnRefresh;
    }
}
