using System;
using System.Drawing;
using System.Windows.Forms;

namespace FileSharingClient
{
    public partial class TrashFolderItemControl : UserControl
    {
        public int FolderId { get; private set; }
        public string FolderName { get; private set; }
        public string DeletedAt { get; private set; }
        public string Owner { get; private set; }

        // Events for folder operations
        public event Action<int> FolderRestored;
        public event Action<int> FolderPermanentlyDeleted;

        public TrashFolderItemControl(int folderId, string folderName, string deletedAt, string owner)
        {
            InitializeComponent();
            
            FolderId = folderId;
            FolderName = folderName;
            DeletedAt = deletedAt;
            Owner = owner;
            
            // Set the data
            lblFolderName.Text = TruncateName(folderName, 30);
            var toolTip = new ToolTip();
            toolTip.SetToolTip(lblFolderName, folderName);
            lblOwner.Text = owner;
            lblDeletedAt.Text = deletedAt;
            lblType.Text = "Folder";
            lblFolderIcon.Text = "📁";

            // Set button text and color
            btnRestore.Text = "Phục hồi";
            btnPermanentDelete.Text = "Xóa";
            btnRestore.BackColor = Color.LightGreen;
            btnPermanentDelete.BackColor = Color.LightCoral;

            // Add hover effects
            this.MouseEnter += (s, e) => this.BackColor = Color.LightBlue;
            this.MouseLeave += (s, e) => this.BackColor = Color.WhiteSmoke;

            // Set layout
            this.Height = 45;
            this.Width = 1080;
            this.BorderStyle = BorderStyle.FixedSingle;
            this.BackColor = Color.WhiteSmoke;

            // Configure event handlers
            btnRestore.Click += BtnRestore_Click;
            btnPermanentDelete.Click += BtnPermanentDelete_Click;
        }

        private string TruncateName(string name, int maxLength)
        {
            if (string.IsNullOrEmpty(name) || name.Length <= maxLength)
                return name;
            return name.Substring(0, maxLength - 3) + "...";
        }

        private void BtnRestore_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show(
                    $"Bạn có chắc chắn muốn phục hồi folder '{FolderName}'?",
                    "Xác nhận phục hồi",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    //Console.WriteLine($"[DEBUG] TrashFolderItemControl - Restore folder clicked for: {FolderName} (ID: {FolderId})");
                    FolderRestored?.Invoke(FolderId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi phục hồi folder: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPermanentDelete_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show(
                    $"Bạn có chắc chắn muốn xóa vĩnh viễn folder '{FolderName}'?\n\nHành động này không thể hoàn tác!",
                    "Xác nhận xóa vĩnh viễn",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.Yes)
                {
                    //Console.WriteLine($"[DEBUG] TrashFolderItemControl - Permanent delete folder clicked for: {FolderName} (ID: {FolderId})");
                    FolderPermanentlyDeleted?.Invoke(FolderId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa vĩnh viễn folder: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 