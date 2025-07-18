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
using System.Configuration;
using System.Security.Cryptography;

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
        private string serverIp = ConfigurationManager.AppSettings["ServerIP"];
        private int serverPort = int.Parse(ConfigurationManager.AppSettings["ServerPort"]);
        private int chunkSize = int.Parse(ConfigurationManager.AppSettings["ChunkSize"]);
        private async Task SendFile(string filePath)
        {
            try
            {
                var (sslStream, _) = await SecureChannelHelper.ConnectToLoadBalancerAsync(serverIp, serverPort);
                using (sslStream)
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: chunkSize, useAsync: true))
                {
                    long totalBytes = fileStream.Length;
                    byte[] buffer = new byte[chunkSize];
                    int bytesRead;
                    long totalSent = 0;
                    while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await sslStream.WriteAsync(buffer, 0, bytesRead);
                        totalSent += bytesRead;

                        //Cap nhat tien trinh
                        int progress = (int)((totalSent * 100) / totalBytes);
                    }
                    totalStorageUsed += totalSent;
                    MessageBox.Show("File đã gửi xong!");
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
        private TrashBinView trashbinView = new TrashBinView();
        private UploadView uploadView = new UploadView();

        private void Account_Click(object sender, EventArgs e)
        {
            Account accountForm = new Account();
            string username = Session.LoggedInUser ?? "Unknown";
            string storageUsed = GetTotalStorageUsed(); // H�m d� d?nh nghia tru?c
            accountForm.SetAccountInfo(username, storageUsed);
            accountForm.ShowDialog(); // Hi?n th? form Account v� ch? ngu?i d�ng thao t�c
        }

        private List<Control> dashboardButtons;

        private void HightlightSelectedDashboard(Button selectedButton)
        {
            Color selectedColor = Color.FromArgb(41, 121, 255); // Blue tươi
            Color selectedBg = Color.LightBlue;
            Color normalColor = Color.Gray;
            Color normalBg = Color.WhiteSmoke;
            foreach (Button btn in dashboardButtons)
            {
                btn.BackColor = normalBg;
                btn.ForeColor = normalColor;
                btn.Font = new Font(btn.Font, FontStyle.Bold);
                btn.Enabled = true;
                if (btn is FontAwesome.Sharp.IconButton iconBtn)
                {
                    iconBtn.IconColor = normalColor;
                    iconBtn.ForeColor = normalColor;
                }
                btn.MouseEnter -= DashboardButton_MouseEnter;
                btn.MouseLeave -= DashboardButton_MouseLeave;
                btn.MouseEnter += DashboardButton_MouseEnter;
                btn.MouseLeave += DashboardButton_MouseLeave;
                btn.Tag = null; // reset tag
            }
            selectedButton.BackColor = selectedBg;
            selectedButton.ForeColor = selectedColor;
            selectedButton.Font = new Font(selectedButton.Font, FontStyle.Bold);
            selectedButton.Enabled = false;
            if (selectedButton is FontAwesome.Sharp.IconButton selectedIconBtn)
            {
                selectedIconBtn.IconColor = selectedColor;
                selectedIconBtn.ForeColor = selectedColor;
            }
            selectedButton.Tag = "selected";
        }

        private void DashboardButton_MouseEnter(object sender, EventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;
            if (btn.Tag as string == "selected") return;
            btn.ForeColor = Color.LightBlue;
            btn.BackColor = Color.FromArgb(230, 240, 255); // nhạt hơn LightBlue
            if (btn is FontAwesome.Sharp.IconButton iconBtn)
                iconBtn.IconColor = Color.LightBlue;
        }

        private void DashboardButton_MouseLeave(object sender, EventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;
            if (btn.Tag as string == "selected") return;
            btn.ForeColor = Color.Gray;
            btn.BackColor = Color.WhiteSmoke;
            if (btn is FontAwesome.Sharp.IconButton iconBtn)
                iconBtn.IconColor = Color.Gray;
        }

        private void Main_Load(object sender, EventArgs e)
        {
            AddFilesToMyFileView();
            InitDashboardButtons();
            uploadView.FileUploaded += async () =>
            {
                await shareView.Reload();
                await trashbinView.RefreshTrashFiles();
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
                    TrashBin_Dashboard
                };
        }
        private async void Share_Dashboard_Click(object sender, EventArgs e)
        {
            await shareView.Reload();
            LoadView(shareView);
            HightlightSelectedDashboard(Share_Dashboard);

        }


        private void MyFile_Dashboard_Click(object sender, EventArgs e)
        {
            AddFilesToMyFileView();
            LoadView(myfileView);
            HightlightSelectedDashboard(MyFile_Dashboard);
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
