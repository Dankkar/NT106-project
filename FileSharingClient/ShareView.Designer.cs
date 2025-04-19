namespace FileSharingClient
{
    partial class ShareView
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
            this.ShareWithPanel = new System.Windows.Forms.Panel();
            this.btnShare = new System.Windows.Forms.Button();
            this.PasswordPanel = new System.Windows.Forms.Panel();
            this.tbPassword = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.cbBrowseFile = new System.Windows.Forms.ComboBox();
            this.lblFileToShare = new System.Windows.Forms.Label();
            this.lblShareWith = new System.Windows.Forms.Label();
            this.GetSharePanel = new System.Windows.Forms.Panel();
            this.btnGet = new System.Windows.Forms.Button();
            this.tbInputPassword = new System.Windows.Forms.TextBox();
            this.lblInputPassword = new System.Windows.Forms.Label();
            this.lblGetFile = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.ShareWithPanel.SuspendLayout();
            this.PasswordPanel.SuspendLayout();
            this.GetSharePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // ShareWithPanel
            // 
            this.ShareWithPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ShareWithPanel.Controls.Add(this.btnShare);
            this.ShareWithPanel.Controls.Add(this.PasswordPanel);
            this.ShareWithPanel.Controls.Add(this.cbBrowseFile);
            this.ShareWithPanel.Controls.Add(this.lblFileToShare);
            this.ShareWithPanel.Controls.Add(this.lblShareWith);
            this.ShareWithPanel.Location = new System.Drawing.Point(6, 0);
            this.ShareWithPanel.Margin = new System.Windows.Forms.Padding(6);
            this.ShareWithPanel.Name = "ShareWithPanel";
            this.ShareWithPanel.Size = new System.Drawing.Size(548, 716);
            this.ShareWithPanel.TabIndex = 0;
            // 
            // btnShare
            // 
            this.btnShare.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.btnShare.Location = new System.Drawing.Point(169, 338);
            this.btnShare.Margin = new System.Windows.Forms.Padding(6);
            this.btnShare.Name = "btnShare";
            this.btnShare.Size = new System.Drawing.Size(191, 89);
            this.btnShare.TabIndex = 4;
            this.btnShare.Text = "Share";
            this.btnShare.UseVisualStyleBackColor = true;
            this.btnShare.Click += new System.EventHandler(this.btnShare_Click);
            // 
            // PasswordPanel
            // 
            this.PasswordPanel.Controls.Add(this.tbPassword);
            this.PasswordPanel.Controls.Add(this.lblPassword);
            this.PasswordPanel.Location = new System.Drawing.Point(40, 462);
            this.PasswordPanel.Margin = new System.Windows.Forms.Padding(6);
            this.PasswordPanel.Name = "PasswordPanel";
            this.PasswordPanel.Size = new System.Drawing.Size(447, 214);
            this.PasswordPanel.TabIndex = 3;
            // 
            // tbPassword
            // 
            this.tbPassword.Location = new System.Drawing.Point(226, 52);
            this.tbPassword.Margin = new System.Windows.Forms.Padding(6);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.ReadOnly = true;
            this.tbPassword.Size = new System.Drawing.Size(180, 29);
            this.tbPassword.TabIndex = 1;
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lblPassword.Location = new System.Drawing.Point(44, 52);
            this.lblPassword.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(132, 38);
            this.lblPassword.TabIndex = 0;
            this.lblPassword.Text = "Password";
            // 
            // cbBrowseFile
            // 
            this.cbBrowseFile.FormattingEnabled = true;
            this.cbBrowseFile.Location = new System.Drawing.Point(266, 159);
            this.cbBrowseFile.Margin = new System.Windows.Forms.Padding(6);
            this.cbBrowseFile.Name = "cbBrowseFile";
            this.cbBrowseFile.Size = new System.Drawing.Size(218, 32);
            this.cbBrowseFile.TabIndex = 2;
            this.cbBrowseFile.SelectedIndexChanged += new System.EventHandler(this.cbBrowseFile_SelectedIndexChanged);
            // 
            // lblFileToShare
            // 
            this.lblFileToShare.AutoSize = true;
            this.lblFileToShare.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lblFileToShare.Location = new System.Drawing.Point(33, 159);
            this.lblFileToShare.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblFileToShare.Name = "lblFileToShare";
            this.lblFileToShare.Size = new System.Drawing.Size(171, 38);
            this.lblFileToShare.TabIndex = 1;
            this.lblFileToShare.Text = "File to Share";
            // 
            // lblShareWith
            // 
            this.lblShareWith.AutoSize = true;
            this.lblShareWith.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold);
            this.lblShareWith.Location = new System.Drawing.Point(160, 61);
            this.lblShareWith.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblShareWith.Name = "lblShareWith";
            this.lblShareWith.Size = new System.Drawing.Size(195, 46);
            this.lblShareWith.TabIndex = 0;
            this.lblShareWith.Text = "Share With";
            // 
            // GetSharePanel
            // 
            this.GetSharePanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.GetSharePanel.Controls.Add(this.btnGet);
            this.GetSharePanel.Controls.Add(this.tbInputPassword);
            this.GetSharePanel.Controls.Add(this.lblInputPassword);
            this.GetSharePanel.Controls.Add(this.lblGetFile);
            this.GetSharePanel.Location = new System.Drawing.Point(566, 0);
            this.GetSharePanel.Margin = new System.Windows.Forms.Padding(6);
            this.GetSharePanel.Name = "GetSharePanel";
            this.GetSharePanel.Size = new System.Drawing.Size(524, 716);
            this.GetSharePanel.TabIndex = 1;
            // 
            // btnGet
            // 
            this.btnGet.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.btnGet.Location = new System.Drawing.Point(167, 338);
            this.btnGet.Margin = new System.Windows.Forms.Padding(6);
            this.btnGet.Name = "btnGet";
            this.btnGet.Size = new System.Drawing.Size(191, 89);
            this.btnGet.TabIndex = 5;
            this.btnGet.Text = "Get";
            this.btnGet.UseVisualStyleBackColor = true;
            this.btnGet.Click += new System.EventHandler(this.btnGet_Click);
            // 
            // tbInputPassword
            // 
            this.tbInputPassword.Location = new System.Drawing.Point(280, 155);
            this.tbInputPassword.Margin = new System.Windows.Forms.Padding(6);
            this.tbInputPassword.Name = "tbInputPassword";
            this.tbInputPassword.Size = new System.Drawing.Size(180, 29);
            this.tbInputPassword.TabIndex = 2;
            // 
            // lblInputPassword
            // 
            this.lblInputPassword.AutoSize = true;
            this.lblInputPassword.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lblInputPassword.Location = new System.Drawing.Point(40, 153);
            this.lblInputPassword.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblInputPassword.Name = "lblInputPassword";
            this.lblInputPassword.Size = new System.Drawing.Size(204, 38);
            this.lblInputPassword.TabIndex = 5;
            this.lblInputPassword.Text = "Input Password";
            // 
            // lblGetFile
            // 
            this.lblGetFile.AutoSize = true;
            this.lblGetFile.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold);
            this.lblGetFile.Location = new System.Drawing.Point(191, 61);
            this.lblGetFile.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblGetFile.Name = "lblGetFile";
            this.lblGetFile.Size = new System.Drawing.Size(140, 46);
            this.lblGetFile.TabIndex = 5;
            this.lblGetFile.Text = "Get File";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(730, 68);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 25);
            this.label1.TabIndex = 2;
            // 
            // ShareView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.GetSharePanel);
            this.Controls.Add(this.ShareWithPanel);
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "ShareView";
            this.Size = new System.Drawing.Size(1102, 724);
            this.Load += new System.EventHandler(this.ShareView_Load);
            this.ShareWithPanel.ResumeLayout(false);
            this.ShareWithPanel.PerformLayout();
            this.PasswordPanel.ResumeLayout(false);
            this.PasswordPanel.PerformLayout();
            this.GetSharePanel.ResumeLayout(false);
            this.GetSharePanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel ShareWithPanel;
        private System.Windows.Forms.Panel GetSharePanel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblShareWith;
        private System.Windows.Forms.Label lblFileToShare;
        private System.Windows.Forms.Button btnShare;
        private System.Windows.Forms.Panel PasswordPanel;
        private System.Windows.Forms.TextBox tbPassword;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.ComboBox cbBrowseFile;
        private System.Windows.Forms.Label lblGetFile;
        private System.Windows.Forms.Button btnGet;
        private System.Windows.Forms.TextBox tbInputPassword;
        private System.Windows.Forms.Label lblInputPassword;
    }
}
