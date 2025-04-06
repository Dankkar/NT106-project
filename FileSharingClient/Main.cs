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
            if (myfileView == null)
                myfileView = new MyFileView();

            LoadView(myfileView);
            HightlightSelectedDashboard(MyFile_Dashboard);

        }    
       
        private void LoadView(UserControl view)
        {
            MainContentPanel.Controls.Clear();
            view.Dock = DockStyle.Fill;
            MainContentPanel.Controls.Add(view);
        }

        private void ResetDashboardColors()
        {
            MyFile_Dashboard.BackColor = Color.LightGray;
            SharedWithMe_Dashboard.BackColor = Color.LightGray;
            Upload_Dashboard.BackColor = Color.LightGray;
            TrashBin_Dashboard.BackColor = Color.LightGray;
            Settings_Dashboard.BackColor = Color.LightGray;
        }

        private void HightlightSelectedDashboard(Control selected)
        {
            ResetDashboardColors();
            selected.BackColor = Color.SteelBlue;
        }

        private void UploadPanel_Paint(object sender, PaintEventArgs e)
        {
            
        }


        private UploadView uploadView;
        private MyFileView myfileView;
        private SharedWithMeView sharewithmeView;
        private TrashBinView trashbinView;
        private SettingsView settingsView;
        
        private void File_Dashboard_Click(object sender, EventArgs e)
        {
            if (myfileView == null)
                myfileView = new MyFileView();

            LoadView(myfileView);
            HightlightSelectedDashboard(MyFile_Dashboard);

        }

        private void lblFileName_Click(object sender, EventArgs e)
        {

        }

        private void SharedWithMe_Dashboard_Click(object sender, EventArgs e)
        {
            if (sharewithmeView == null)
                sharewithmeView = new SharedWithMeView();
            LoadView(sharewithmeView);
            HightlightSelectedDashboard(SharedWithMe_Dashboard);
        }

        private void Upload_Dashboard_Click(object sender, EventArgs e)
        {
            if (uploadView == null)
                uploadView = new UploadView();
            LoadView(uploadView);
            HightlightSelectedDashboard(Upload_Dashboard);
        }
    }
}
