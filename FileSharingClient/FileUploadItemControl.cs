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
    public partial class FileUploadItemControl : UserControl
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string Owner { get; set; }
        public string FileSize { get; set; }
        public string FileType { get; set; }
        
        public event Action<string> FileRemoved;
        public event Action<string> FilePreviewRequested; // For navigation/preview

        private Color originalBackColor = Color.White;
        private Color hoverBackColor = Color.LightGray;

        public FileUploadItemControl(string fileName, string owner, string fileSize, string filePath)
        {
            InitializeComponent();

            // Set file information
            FileName = fileName;
            Owner = owner;
            FileSize = fileSize;
            FilePath = filePath;
            FileType = GetFileType(fileName);

            // Set labels
            lblFileName.Text = TruncateFileName(fileName, 25);
            lblOwner.Text = owner;
            lblFileSize.Text = fileSize;
            lblFileType.Text = FileType;

            // Set file icon
            string extension = Path.GetExtension(fileName).ToLower();
            lblFileIcon.Text = GetFileIcon(extension);

            // Set tooltip for full file name if truncated
            if (fileName.Length > 25)
            {
                var toolTip = new ToolTip();
                toolTip.SetToolTip(lblFileName, fileName);
            }

            // Set up hover effects
            this.BackColor = originalBackColor;
            SetupHoverEffects();
            
            // Wire up button click event
            btnRemove.Click += btnRemove_Click;
        }

        private void SetupHoverEffects()
        {
            // Add hover effects to the main control and all child controls
            this.MouseEnter += (s, e) => OnControlMouseEnter();
            this.MouseLeave += (s, e) => OnControlMouseLeave();

            // Add hover effects to all child controls
            foreach (Control control in this.Controls)
            {
                if (control != btnRemove) // Don't apply to button
                {
                    control.MouseEnter += (s, e) => OnControlMouseEnter();
                    control.MouseLeave += (s, e) => OnControlMouseLeave();
                }
            }
        }

        private void OnControlMouseEnter()
        {
            this.BackColor = hoverBackColor;
            this.Cursor = Cursors.Hand;
        }

        private void OnControlMouseLeave()
        {
            this.BackColor = originalBackColor;
            this.Cursor = Cursors.Default;
        }

        private string TruncateFileName(string fileName, int maxLength)
        {
            if (fileName.Length <= maxLength)
                return fileName;
            return fileName.Substring(0, maxLength - 3) + "...";
        }

        private string GetFileType(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLower();
            switch (extension)
            {
                case ".txt":
                case ".md":
                case ".log":
                    return "Text";
                case ".pdf":
                    return "PDF";
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".gif":
                case ".bmp":
                    return "Image";
                case ".mp4":
                case ".avi":
                case ".mov":
                case ".wmv":
                case ".mkv":
                    return "Video";
                case ".mp3":
                case ".wav":
                case ".flac":
                    return "Audio";
                case ".docx":
                case ".doc":
                    return "Word";
                case ".xlsx":
                case ".xls":
                    return "Excel";
                case ".pptx":
                case ".ppt":
                    return "PowerPoint";
                case ".zip":
                case ".rar":
                case ".7z":
                    return "Archive";
                default:
                    return "File";
            }
        }

        private string GetFileIcon(string extension)
        {
            switch (extension)
            {
                case ".txt":
                case ".md":
                case ".log":
                    return "📄";
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
                    return "🎬";
                case ".mp3":
                case ".wav":
                case ".flac":
                    return "🎵";
                case ".docx":
                case ".doc":
                    return "📝";
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

        private void btnRemove_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa file '{FileName}' khỏi danh sách upload?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                FileRemoved?.Invoke(FilePath);
            }
        }

        private void FileUploadItemControl_Click(object sender, EventArgs e)
        {
            // Preview file when clicked (navigation support)
            PreviewFile();
        }

        private void PreviewFile()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    string extension = Path.GetExtension(FileName).ToLower();
                    
                    // Display the file based on type
                    if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".gif" || extension == ".bmp")
                    {
                        var img = Image.FromFile(FilePath);
                        Form frm = new Form { Width = img.Width + 40, Height = img.Height + 60, Text = FileName };
                        var pb = new PictureBox { Image = img, Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom };
                        frm.Controls.Add(pb);
                        frm.ShowDialog();
                    }
                    else if (extension == ".txt" || extension == ".md" || extension == ".log")
                    {
                        string text = File.ReadAllText(FilePath);
                        Form frm = new Form { Width = 800, Height = 600, Text = FileName };
                        var rtb = new RichTextBox { Text = text, Dock = DockStyle.Fill, ReadOnly = true, Font = new Font("Consolas", 11) };
                        frm.Controls.Add(rtb);
                        frm.ShowDialog();
                    }
                    else
                    {
                        // Mở bằng app mặc định
                        System.Diagnostics.Process.Start(FilePath);
                    }
                }
                else
                {
                    MessageBox.Show("File không tồn tại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi preview file: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 