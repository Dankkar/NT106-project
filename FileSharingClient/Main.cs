using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileSharingClient
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();

        }

        private async void btnSendFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                FileInfo fileInfo = new FileInfo(filePath);
                await SendFile(filePath);
            }
        }
        private async Task SendFile(string filePath)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    await client.ConnectAsync("172.20.10.3", 5000);
                    using (NetworkStream stream = client.GetStream())
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
                    {
                        long totalBytes = fileStream.Length;
                        byte[] buffer = new byte[4096];
                        int bytesRead;
                        long totalSent = 0;
                        while((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await stream.WriteAsync(buffer, 0, bytesRead);
                            totalSent += bytesRead;

                            //Cap nhat tien trinh
                            int progress = (int)((totalSent * 100) / totalBytes);
                        }
                        MessageBox.Show("File đã gửi xong!");
                    }
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void btnSendFile_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                FileInfo fileInfo = new FileInfo(filePath);

                // Lấy tên file (không gồm đường dẫn)
                lblFileName.Text =  fileInfo.Name;

                // Lấy kích thước file (theo byte)
                lblFileSize.Text =  fileInfo.Length + " bytes";

                // Lấy extension (đuôi file)
                lblFileExtension.Text = fileInfo.Extension;
                panelFile.Visible = true;
                upload_progress.Visible = true;
                // Nếu cần upload lên server, bạn có thể viết thêm logic upload ở đây
            }
        }
    }
}
