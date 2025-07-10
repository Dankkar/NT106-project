using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace FileSharingClient
{
    public partial class FolderUploadItemControl : UserControl
    {
        public string FolderPath { get; private set; }
        public string FolderName { get; private set; }
        public int FileCount { get; private set; }

        public event Action<string> FolderRemoved;
        public event Action<string> FolderNavigationRequested;

        public FolderUploadItemControl(string folderName, string owner, string folderSize, string folderPath, int fileCount)
        {
            InitializeComponent();
            FolderPath = folderPath;
            FolderName = folderName;
            FileCount = fileCount;

            lblFolderName.Text = folderName;
            lblOwner.Text = owner;
            lblFolderSize.Text = folderSize;
            lblFolderType.Text = "Thư mục";

            // Set cursor to hand for clickable elements
            this.Cursor = Cursors.Hand;
            lblFolderName.Cursor = Cursors.Hand;
            lblOwner.Cursor = Cursors.Hand;
            lblFolderSize.Cursor = Cursors.Hand;
            lblFolderType.Cursor = Cursors.Hand;

            // Add hover effects
            this.MouseEnter += (s, e) => this.BackColor = Color.FromArgb(230, 240, 250);
            this.MouseLeave += (s, e) => this.BackColor = Color.White;
            lblFolderName.MouseEnter += (s, e) => this.BackColor = Color.FromArgb(230, 240, 250);
            lblFolderName.MouseLeave += (s, e) => this.BackColor = Color.White;
            lblOwner.MouseEnter += (s, e) => this.BackColor = Color.FromArgb(230, 240, 250);
            lblOwner.MouseLeave += (s, e) => this.BackColor = Color.White;
            lblFolderSize.MouseEnter += (s, e) => this.BackColor = Color.FromArgb(230, 240, 250);
            lblFolderSize.MouseLeave += (s, e) => this.BackColor = Color.White;
            lblFolderType.MouseEnter += (s, e) => this.BackColor = Color.FromArgb(230, 240, 250);
            lblFolderType.MouseLeave += (s, e) => this.BackColor = Color.White;

            // Add click events for navigation
            this.Click += FolderUploadItemControl_Click;
            lblFolderName.Click += FolderUploadItemControl_Click;
            lblOwner.Click += FolderUploadItemControl_Click;
            lblFolderSize.Click += FolderUploadItemControl_Click;
            lblFolderType.Click += FolderUploadItemControl_Click;
        }

        private void FolderUploadItemControl_Click(object sender, EventArgs e)
        {
            // Navigate into folder
            FolderNavigationRequested?.Invoke(FolderPath);
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show($"Bạn có chắc muốn xóa folder '{FolderName}' khỏi danh sách upload?", 
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                FolderRemoved?.Invoke(FolderPath);
            }
        }
    }
} 