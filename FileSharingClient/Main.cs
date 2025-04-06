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
            SetUpListView();
        }
        private void SetUpListView()
        {
            listView1.View = View.Details;
            listView1.GridLines = true;
            listView1.FullRowSelect = true;
            listView1.OwnerDraw = true;
            listView1.Columns.Add("Tên file", 200);
            listView1.Columns.Add("Kích thước (KB)", 100);
            listView1.Columns.Add("Tiến trình", 100);
            listView1.Columns.Add("Loại file", 100);
            listView1.Columns.Add("Chủ sở hũu", 150);
            listView1.Columns.Add("Ngày chỉnh sửa", 150);
            listView1.DrawColumnHeader += ListView1_DrawColumnHeader;
            listView1.DrawSubItem += ListView1_DrawSubItem;
        }
        // Vẽ tiêu đề cột
        private void ListView1_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        // Vẽ từng ô trong ListView (cột tiến trình sẽ có ProgressBar)
        private void ListView1_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            if (e.ColumnIndex == 2) // Cột tiến trình
            {
                int progress;
                if (int.TryParse(e.Item.SubItems[2].Text.Replace("%", ""), out progress))
                {
                    Rectangle rect = e.Bounds;
                    rect.Inflate(-2, -6); // Giảm kích thước để không bị lấn ra ngoài
                    ProgressBarRenderer.DrawHorizontalBar(e.Graphics, rect);

                    if (progress > 0)
                    {
                        rect.Inflate(-1, -1);
                        rect.Width = (int)(rect.Width * (progress / 100.0));
                        e.Graphics.FillRectangle(Brushes.Blue, rect);
                    }

                    // Vẽ số %
                    TextRenderer.DrawText(e.Graphics, $"{progress}%", e.Item.Font, e.Bounds, Color.Black, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                }
            }
            else
            {
                e.DrawDefault = true;
            }
        }
        private async void btnSendFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                FileInfo fileInfo = new FileInfo(filePath);

                string extension = Path.GetExtension(filePath).ToLower().TrimStart('.');
                string owner = Session.LoggedInUser;
                string dateModified = fileInfo.LastWriteTime.ToString("dd/MM/yyyy HH:mm");
                ListViewItem item = new ListViewItem(fileInfo.Name);
                item.SubItems.Add((fileInfo.Length / 1024).ToString("N0") + " KB");
                item.SubItems.Add("0%");
                item.SubItems.Add(extension);
                item.SubItems.Add(owner);
                item.SubItems.Add(dateModified);
                listView1.Items.Add(item);
                await SendFile(filePath, item);
            }
        }
        private async Task SendFile(string filePath, ListViewItem item)
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
                            item.SubItems[2].Text = $"{progress}%";

                            //Yeu cau ListView ve lai
                            listView1.Invalidate();
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

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnCreateLink_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void btnArrange_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
