namespace FileSharingClient
{
    partial class ChangePassword
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.old_passtxtBox = new System.Windows.Forms.TextBox();
            this.new_passtxtBox = new System.Windows.Forms.TextBox();
            this.conf_new_passtxtBox = new System.Windows.Forms.TextBox();
            this.view_pass = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.view_pass);
            this.panel1.Controls.Add(this.conf_new_passtxtBox);
            this.panel1.Controls.Add(this.new_passtxtBox);
            this.panel1.Controls.Add(this.old_passtxtBox);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(328, 363);
            this.panel1.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.label2.Location = new System.Drawing.Point(93, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(142, 30);
            this.label2.TabIndex = 9;
            this.label2.Text = "Đổi mật khẩu";
            // 
            // old_passtxtBox
            // 
            this.old_passtxtBox.Location = new System.Drawing.Point(48, 86);
            this.old_passtxtBox.Name = "old_passtxtBox";
            this.old_passtxtBox.Size = new System.Drawing.Size(237, 20);
            this.old_passtxtBox.TabIndex = 10;
            // 
            // new_passtxtBox
            // 
            this.new_passtxtBox.Location = new System.Drawing.Point(48, 131);
            this.new_passtxtBox.Name = "new_passtxtBox";
            this.new_passtxtBox.Size = new System.Drawing.Size(237, 20);
            this.new_passtxtBox.TabIndex = 11;
            // 
            // conf_new_passtxtBox
            // 
            this.conf_new_passtxtBox.Location = new System.Drawing.Point(48, 172);
            this.conf_new_passtxtBox.Name = "conf_new_passtxtBox";
            this.conf_new_passtxtBox.Size = new System.Drawing.Size(237, 20);
            this.conf_new_passtxtBox.TabIndex = 12;
            // 
            // view_pass
            // 
            this.view_pass.AutoSize = true;
            this.view_pass.Location = new System.Drawing.Point(48, 213);
            this.view_pass.Name = "view_pass";
            this.view_pass.Size = new System.Drawing.Size(94, 17);
            this.view_pass.TabIndex = 13;
            this.view_pass.Text = "Xem mật khẩu";
            this.view_pass.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(98, 257);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(137, 49);
            this.button1.TabIndex = 14;
            this.button1.Text = "Đổi mật khẩu";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // ChangePassword
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.AntiqueWhite;
            this.ClientSize = new System.Drawing.Size(352, 387);
            this.Controls.Add(this.panel1);
            this.Name = "ChangePassword";
            this.Text = "ChangePassword";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox conf_new_passtxtBox;
        private System.Windows.Forms.TextBox new_passtxtBox;
        private System.Windows.Forms.TextBox old_passtxtBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox view_pass;
        private System.Windows.Forms.Button button1;
    }
}