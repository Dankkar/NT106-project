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
        private List<string> pendingFiles;
        private UploadView parentView;

        public string FileName { get; set; }
        public string CreateAt { get; set; }
        public string Owner { get; set; }
        public string FileSize { get; set; }
       
        // My File View
        public FileItemControl(string filename, string createAt, string owner, string filesize)
        {
            InitializeComponent();

            //Set thong tin file vao control
            lblFileName.Text = filename;
            lblFileSize.Text = filesize;
            lblOwner.Text = owner;
            lblCreateAt.Text = createAt;

            btnMore.Click += (s, e) => contextMenuStrip1.Show(btnMore, new Point(0, btnMore.Height));
        }

        // Upload View
        public FileItemControl(string filename, string createAt, string owner, string filesize, List<string> pendingFiles, UploadView parentView)
        {
            InitializeComponent();// Lưu danh sách pendingFiles để thao tác
            this.pendingFiles = pendingFiles ?? new List<string>();
            this.parentView = parentView ?? throw new ArgumentNullException(nameof(parentView), "UploadView không the null");

            // Set thông tin file vào control
            lblFileName.Text = filename;
            lblFileSize.Text = filesize;
            lblOwner.Text = owner;
            lblCreateAt.Text = createAt;

            // Xử lý sự kiện cho nút More (Menu Chuột phải)
            btnMore.Click += (s, e) => contextMenuStrip1.Show(btnMore, new Point(0, btnMore.Height));
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
                if (pendingFiles != null)
                {
                    var toRemove = pendingFiles.FirstOrDefault(p => Path.GetFileName(p) == FileName);
                    if (toRemove != null)
                    {
                        pendingFiles.Remove(toRemove);
                        UpdateTotalSize();
                    }
                    this.Dispose();
                    parentView.UpdateTotalSize();
                }
                else
                {
                    MessageBox.Show("Lỗi: Không thể xóa file, danh sách pendingFiles không hợp lệ.");
                }
            }
        }
       

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }
    }
}
