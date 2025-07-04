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
    public partial class FileItemControl : UserControl
    {
        public string FilePath { get; set; }
        public event Action<String> FileDeleted;
        public event Action<String> FilePreviewRequested;
        public event Action<String> FileDownloadRequested;
        
        public string FileName { get; set; }
        public string CreateAt { get; set; }
        public string Owner { get; set; }
        public string FileSize { get; set; }
        
        private static string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName;

        public FileItemControl(string filename, string createAt, string owner, string filesize, string filepath)
        {
            InitializeComponent();

            //Set thong tin file vao control
            FileName = filename;
            lblFileName.Text = filename;
            lblFileSize.Text = filesize;
            lblOwner.Text = owner;
            lblCreateAt.Text = createAt;
            FilePath = filepath;
            lblFilePath.Text = filepath;
            CreateAt = createAt;
            Owner = owner;
            FileSize = filesize;

            btnMore.Click += (s, e) => contextMenuStrip1.Show(btnMore, new Point(0, btnMore.Height));
            
            // Add click events to main controls for preview
            this.Click += FileItemControl_Click;
            lblFileName.Click += FileItemControl_Click;
            lblOwner.Click += FileItemControl_Click;
            lblCreateAt.Click += FileItemControl_Click;
            lblFileSize.Click += FileItemControl_Click;
            lblFilePath.Click += FileItemControl_Click;
            
            // Make the control look clickable
            this.Cursor = Cursors.Hand;
            lblFileName.Cursor = Cursors.Hand;
            lblOwner.Cursor = Cursors.Hand;
            lblCreateAt.Cursor = Cursors.Hand;
            lblFileSize.Cursor = Cursors.Hand;
            lblFilePath.Cursor = Cursors.Hand;
            
            // Add hover effects
            this.MouseEnter += (s, e) => this.BackColor = Color.LightGray;
            this.MouseLeave += (s, e) => this.BackColor = SystemColors.ButtonHighlight;
            lblFileName.MouseEnter += (s, e) => this.BackColor = Color.LightGray;
            lblFileName.MouseLeave += (s, e) => this.BackColor = SystemColors.ButtonHighlight;
            lblOwner.MouseEnter += (s, e) => this.BackColor = Color.LightGray;
            lblOwner.MouseLeave += (s, e) => this.BackColor = SystemColors.ButtonHighlight;
            lblCreateAt.MouseEnter += (s, e) => this.BackColor = Color.LightGray;
            lblCreateAt.MouseLeave += (s, e) => this.BackColor = SystemColors.ButtonHighlight;
            lblFileSize.MouseEnter += (s, e) => this.BackColor = Color.LightGray;
            lblFileSize.MouseLeave += (s, e) => this.BackColor = SystemColors.ButtonHighlight;
            lblFilePath.MouseEnter += (s, e) => this.BackColor = Color.LightGray;
            lblFilePath.MouseLeave += (s, e) => this.BackColor = SystemColors.ButtonHighlight;
        }

        private void FileItemControl_Click(object sender, EventArgs e)
        {
            // When clicked, automatically preview the file
            btnPreview_Click(sender, e);
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            try
            {
                string fullPath = Path.Combine(projectRoot, FilePath);
                
                if (!File.Exists(fullPath))
                {
                    MessageBox.Show("File not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Determine file type and open appropriate preview
                string extension = Path.GetExtension(fullPath).ToLower();
                
                if (extension == ".txt" || extension == ".md" || extension == ".log")
                {
                    // Open text files in notepad
                    System.Diagnostics.Process.Start("notepad.exe", fullPath);
                }
                else if (extension == ".pdf")
                {
                    // Open PDF with default application
                    System.Diagnostics.Process.Start(fullPath);
                }
                else if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || 
                         extension == ".gif" || extension == ".bmp")
                {
                    // Open images with default application
                    System.Diagnostics.Process.Start(fullPath);
                }
                else if (extension == ".mp4" || extension == ".avi" || extension == ".mov" || 
                         extension == ".wmv" || extension == ".mkv")
                {
                    // Open videos with default application
                    System.Diagnostics.Process.Start(fullPath);
                }
                else if (extension == ".docx" || extension == ".doc")
                {
                    // Open Word documents
                    System.Diagnostics.Process.Start(fullPath);
                }
                else if (extension == ".xlsx" || extension == ".xls")
                {
                    // Open Excel files
                    System.Diagnostics.Process.Start(fullPath);
                }
                else if (extension == ".pptx" || extension == ".ppt")
                {
                    // Open PowerPoint files
                    System.Diagnostics.Process.Start(fullPath);
                }
                else
                {
                    // For other files, try to open with default application
                    try
                    {
                        System.Diagnostics.Process.Start(fullPath);
                    }
                    catch
                    {
                        MessageBox.Show("Cannot preview this file type. Please download to view.", 
                            "Preview Not Available", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error previewing file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            try
            {
                string fullPath = Path.Combine(projectRoot, FilePath);
                
                if (!File.Exists(fullPath))
                {
                    MessageBox.Show("File not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = FileName;
                saveFileDialog.Title = "Save file as...";
                saveFileDialog.Filter = "All Files (*.*)|*.*";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        File.Copy(fullPath, saveFileDialog.FileName, true);
                        MessageBox.Show("File downloaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error downloading file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error downloading file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void shareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"Chia sẻ file {FileName}");
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show($"Bạn có chắc muốn xóa file {FileName}?", "Xác nhận", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                FileDeleted?.Invoke(FilePath);
                this.Dispose(); // Xóa FileItemControl khỏi giao diện
            }
        }
       
        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }
    }
}
