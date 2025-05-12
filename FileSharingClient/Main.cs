using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Principal;
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
        private long totalStorageUsed = 0;
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
                        while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await stream.WriteAsync(buffer, 0, bytesRead);
                            totalSent += bytesRead;

                            //Cap nhat tien trinh
                            int progress = (int)((totalSent * 100) / totalBytes);
                        }
                        totalStorageUsed += totalSent;
                        MessageBox.Show("File đã gửi xong!");
                    }

                }
            }


            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private string FormatSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return string.Format("{0:n1} {1}", number, suffixes[counter]);
        }
        private string GetTotalStorageUsed()
        {
            return FormatSize(totalStorageUsed);
        }


        private void LoadView(UserControl view)
        {
            MainContentPanel.Controls.Clear();
            view.Dock = DockStyle.Fill;
            MainContentPanel.Controls.Add(view);
        }

        private MyFileView myfileView = new MyFileView();
        private ShareView shareView = new ShareView();
        private FilePreview filepreviewView = new FilePreview();
        private TrashBinView trashbinView = new TrashBinView();
        private UploadView uploadView = new UploadView();

        private void Account_Click(object sender, EventArgs e)
        {
            Account accountForm = new Account();
            string username = Session.LoggedInUser ?? "Unknown";
            string storageUsed = GetTotalStorageUsed(); // Hàm đã định nghĩa trước
            accountForm.SetAccountInfo(username, storageUsed);
            accountForm.ShowDialog(); // Hiển thị form Account và chờ người dùng thao tác
        }

        private List<Control> dashboardButtons;

        private void HightlightSelectedDashboard(Button selectedButton)
        {
            foreach (Button btn in dashboardButtons)
            {
                btn.BackColor = Color.RosyBrown;
                btn.ForeColor = Color.White;
                btn.Font = new Font(btn.Font, FontStyle.Bold);
                btn.Enabled = true;
            }
            selectedButton.BackColor = Color.DodgerBlue;
            selectedButton.ForeColor = Color.Black;
            selectedButton.Font = new Font(selectedButton.Font, FontStyle.Bold);
            selectedButton.Enabled = false;
        }

        private void Main_Load(object sender, EventArgs e)
        {
            AddFilesToMyFileView();
            InitDashboardButtons();
            uploadView.FileUploaded += async () =>
            {
                await shareView.Reload();
                await filepreviewView.Reload();
            };
            LoadView(myfileView);
            
        }

        private void InitDashboardButtons()
        {
            dashboardButtons = new List<Control>
                {
                    MyFile_Dashboard,
                    Share_Dashboard,
                    Upload_Dashboard,
                    TrashBin_Dashboard,
                    FilePreview_Dashboard
                };
        }
        private void Share_Dashboard_Click(object sender, EventArgs e)
        {
            LoadView(shareView);
            HightlightSelectedDashboard(Share_Dashboard);

        }


        private void MyFile_Dashboard_Click(object sender, EventArgs e)
        {
            AddFilesToMyFileView();
            LoadView(myfileView);
            HightlightSelectedDashboard(MyFile_Dashboard);
        }


        private void FilePreview_Dashboard_Click(object sender, EventArgs e)
        {
            LoadView(filepreviewView);
            HightlightSelectedDashboard(FilePreview_Dashboard);
        }

        private void TrashBin_Dashboard_Click(object sender, EventArgs e)
        {
            LoadView(trashbinView);
            HightlightSelectedDashboard(TrashBin_Dashboard);
        }

        private void Upload_Dashboard_Click(object sender, EventArgs e)
        {
            LoadView(uploadView);
            HightlightSelectedDashboard(Upload_Dashboard);
        }
        private void AddFilesToMyFileView()
        {
            //myfileView.AddFileToView("File1.txt", "Aug 10, 2024", "me", "500 KB");
            //myfileView.AddFileToView("File2.pdf", "Jul 25, 2024", "me", "1.2 MB"); 
            //myfileView.AddFileToView("File3.pdf", "Jul 25, 2024", "hieu", "1.2 MB");
        }
    }
}
