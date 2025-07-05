using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileSharingClient
{
    public partial class FolderItemControl : UserControl
    {
        public int FolderId { get; set; }
        public string FolderName { get; set; }
        public string CreatedAt { get; set; }
        public string Owner { get; set; }
        public bool IsShared { get; set; }
        
        public event Action<int> FolderClicked;
        public event Action<int> FolderDeleted;
        public event Action<int> FolderShared;

        public FolderItemControl(string folderName, string createdAt, string owner, bool isShared, int folderId)
        {
            InitializeComponent();

            // Set thông tin folder vào control
            FolderId = folderId;
            FolderName = folderName;
            CreatedAt = createdAt;
            Owner = owner;
            IsShared = isShared;

            lblFolderName.Text = folderName;
            lblOwner.Text = owner;
            lblType.Text = "Folder";
            lblCreatedAt.Text = createdAt;

            // Ẩn các thông tin không cần thiết - chỉ hiển thị Tên folder, Người sở hữu, Type
            lblCreatedAt.Visible = false;

            btnMore.Click += (s, e) => contextMenuStrip1.Show(btnMore, new Point(0, btnMore.Height));
            
            // Add click events to main controls for folder navigation
            this.Click += FolderItemControl_Click;
            lblFolderName.Click += FolderItemControl_Click;
            lblOwner.Click += FolderItemControl_Click;
            lblType.Click += FolderItemControl_Click;
            
            // Make the control look clickable
            this.Cursor = Cursors.Hand;
            lblFolderName.Cursor = Cursors.Hand;
            lblOwner.Cursor = Cursors.Hand;
            lblType.Cursor = Cursors.Hand;
            
            // Add hover effects
            this.MouseEnter += (s, e) => this.BackColor = Color.LightBlue;
            this.MouseLeave += (s, e) => this.BackColor = SystemColors.ButtonHighlight;
            lblFolderName.MouseEnter += (s, e) => this.BackColor = Color.LightBlue;
            lblFolderName.MouseLeave += (s, e) => this.BackColor = SystemColors.ButtonHighlight;
            lblOwner.MouseEnter += (s, e) => this.BackColor = Color.LightBlue;
            lblOwner.MouseLeave += (s, e) => this.BackColor = SystemColors.ButtonHighlight;
            lblType.MouseEnter += (s, e) => this.BackColor = Color.LightBlue;
            lblType.MouseLeave += (s, e) => this.BackColor = SystemColors.ButtonHighlight;
        }

        private void FolderItemControl_Click(object sender, EventArgs e)
        {
            // When clicked, navigate into folder
            FolderClicked?.Invoke(FolderId);
        }

        private void shareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderShared?.Invoke(FolderId);
            MessageBox.Show($"Chia sẻ folder {FolderName}");
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show($"Bạn có chắc muốn xóa folder {FolderName}?", "Xác nhận", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                FolderDeleted?.Invoke(FolderId);
                this.Dispose(); // Xóa FolderItemControl khỏi giao diện
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }
    }
} 