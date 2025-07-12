using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace FileSharingClient
{
    public partial class FileUploadControl: UserControl
    {
        public FileUploadControl()
        {
            InitializeComponent();
        }
        public void DisplayFile(string filePath)
        {
            // Tạo một panel đại diện cho mỗi file
            Panel filePanel = new Panel
            {
                Width = 200,
                Height = 150,
                BackColor = System.Drawing.Color.LightGray,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(10)
            };

            // Tạo Label để hiển thị tên file
            Label fileNameLabel = new Label
            {
                Text = Path.GetFileName(filePath),
                Location = new System.Drawing.Point(10, 10),
                Size = new System.Drawing.Size(180, 20)
            };

            // Tạo Label để hiển thị kích thước file
            Label fileSizeLabel = new Label
            {
                Text = new FileInfo(filePath).Length + " bytes",
                Location = new System.Drawing.Point(10, 40),
                Size = new System.Drawing.Size(180, 20)
            };

            // Tạo PictureBox để hiển thị biểu tượng file
            PictureBox fileIcon = new PictureBox
            {
                Image = GetFileIcon(Path.GetExtension(filePath)),
                Location = new System.Drawing.Point(10, 70),
                Size = new System.Drawing.Size(40, 40)
            };

            // Tạo Label để hiển thị trạng thái
            Label statusLabel = new Label
            {
                Text = "Đang tải lên...",
                Location = new System.Drawing.Point(10, 110),
                Size = new System.Drawing.Size(180, 20)
            };

            // Thêm các điều khiển vào panel
            filePanel.Controls.Add(fileNameLabel);
            filePanel.Controls.Add(fileSizeLabel);
            filePanel.Controls.Add(fileIcon);
            filePanel.Controls.Add(statusLabel);

            // Thêm panel vào FlowLayoutPanel
            flowLayoutPanel1.Controls.Add(filePanel);
        }

        // Hàm lấy biểu tượng cho file (tùy thuộc vào đuôi file)
        private System.Drawing.Image GetFileIcon(string fileExtension)
        {
            if (fileExtension == ".jpg" || fileExtension == ".png")
            {
                return SystemIcons.Information.ToBitmap(); // Ví dụ cho hình ảnh
            }
            else if (fileExtension == ".txt")
            {
                return SystemIcons.Information.ToBitmap(); // Ví dụ cho văn bản
            }
            else
            {
                return SystemIcons.Warning.ToBitmap(); // Mặc định cho các loại file khác
            }
        }
    }
}
