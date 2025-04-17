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
            this.MyFileLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.lblFileName = new System.Windows.Forms.Label();
            this.lblOwner = new System.Windows.Forms.Label();
            this.lblCreateAt = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // MyFileLayoutPanel
            // 
            this.MyFileLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.MyFileLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.MyFileLayoutPanel.Location = new System.Drawing.Point(0, 50);
            this.MyFileLayoutPanel.MaximumSize = new System.Drawing.Size(601, 342);
            this.MyFileLayoutPanel.Name = "MyFileLayoutPanel";
            this.MyFileLayoutPanel.Size = new System.Drawing.Size(601, 342);
            this.MyFileLayoutPanel.TabIndex = 1;
            // 
            // lblFileName
            // 
            this.lblFileName.AutoSize = true;
            this.lblFileName.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileName.Location = new System.Drawing.Point(32, 12);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(53, 17);
            this.lblFileName.TabIndex = 2;
            this.lblFileName.Text = "Tên File";
            // 
            // lblOwner
            // 
            this.lblOwner.AutoSize = true;
            this.lblOwner.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOwner.Location = new System.Drawing.Point(181, 12);
            this.lblOwner.Name = "lblOwner";
            this.lblOwner.Size = new System.Drawing.Size(91, 17);
            this.lblOwner.TabIndex = 3;
            this.lblOwner.Text = "Người sở hữu";
            // 
            // lblCreateAt
            // 
            this.lblCreateAt.AutoSize = true;
            this.lblCreateAt.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCreateAt.Location = new System.Drawing.Point(306, 12);
            this.lblCreateAt.Name = "lblCreateAt";
            this.lblCreateAt.Size = new System.Drawing.Size(51, 17);
            this.lblCreateAt.TabIndex = 4;
            this.lblCreateAt.Text = "Tạo lúc";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(447, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 17);
            this.label1.TabIndex = 5;
            this.label1.Text = "Kích thước";
            // 
            // MyFileView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblCreateAt);
            this.Controls.Add(this.lblOwner);
            this.Controls.Add(this.lblFileName);
            this.Controls.Add(this.MyFileLayoutPanel);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MyFileView";
            this.Size = new System.Drawing.Size(601, 392);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.FlowLayoutPanel MyFileLayoutPanel;
        private System.Windows.Forms.Label lblFileName;
        private System.Windows.Forms.Label lblOwner;
        private System.Windows.Forms.Label lblCreateAt;
        private System.Windows.Forms.Label label1;
    }
}
