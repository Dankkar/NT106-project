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
    public partial class TrashFileItemControl : UserControl
    {
        public string FileName { get; set; }
        public string DeletedAt { get; set; }
        public string Owner { get; set; }
        public string FileSize { get; set; }
        public string FileType { get; set; }
        
        public event Action<string> FileRestored;
        public event Action<string> FilePermanentlyDeleted;

        public TrashFileItemControl(string fileName, string deletedAt, string owner, string fileSize, string fileType)
        {
            InitializeComponent();

            // Set file information
            FileName = fileName;
            DeletedAt = deletedAt;
            Owner = owner;
            FileSize = fileSize;
            FileType = fileType;

            // Set labels
            lblFileName.Text = fileName;
            lblOwner.Text = owner;
            lblDeletedAt.Text = deletedAt;
            lblFileSize.Text = fileSize;
            lblFileType.Text = fileType;

            // Set file icon based on type
            string fileExtension = Path.GetExtension(fileName).ToLower();
            lblFileIcon.Text = GetFileIcon(fileExtension);

            // Configure buttons
            btnRestore.Text = "Phục hồi";
            btnDelete.Text = "Xóa";
            btnRestore.BackColor = Color.LightGreen;
            btnDelete.BackColor = Color.LightCoral;

            // Add hover effects
            this.MouseEnter += (s, e) => this.BackColor = Color.LightBlue;
            this.MouseLeave += (s, e) => this.BackColor = Color.LightGray;

            // Set layout giống folder
            this.Height = 45;
            this.Width = 790;
            this.BorderStyle = BorderStyle.FixedSingle;
            this.BackColor = Color.LightGray;
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                $"Bạn có chắc chắn muốn phục hồi file '{FileName}'?",
                "Xác nhận phục hồi",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                FileRestored?.Invoke(FileName);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa vĩnh viễn file '{FileName}'?\n\nHành động này không thể hoàn tác!",
                "Xác nhận xóa vĩnh viễn",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                FilePermanentlyDeleted?.Invoke(FileName);
            }
        }

        private string GetFileIcon(string extension)
        {
            switch (extension)
            {
                case ".txt":
                case ".md":
                case ".log":
                    return "📝";
                case ".pdf":
                    return "📕";
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".gif":
                case ".bmp":
                    return "🖼️";
                case ".mp4":
                case ".avi":
                case ".mov":
                case ".wmv":
                case ".mkv":
                    return "🎥";
                case ".mp3":
                case ".wav":
                case ".flac":
                    return "🎵";
                case ".docx":
                case ".doc":
                    return "📄";
                case ".xlsx":
                case ".xls":
                    return "📊";
                case ".pptx":
                case ".ppt":
                    return "📋";
                case ".zip":
                case ".rar":
                case ".7z":
                    return "📦";
                default:
                    return "📄";
            }
        }

        private void TrashFileItemControl_Load(object sender, EventArgs e)
        {

        }
    }
} 