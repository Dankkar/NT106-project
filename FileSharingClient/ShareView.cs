using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileSharingClient
{
    public partial class ShareView: UserControl
    {
        public ShareView()
        {
            InitializeComponent();
        }

        

        private void ShareView_Load(object sender, EventArgs e)
        {
            PasswordPanel.Visible = false;
        }

        private void btnShare_Click(object sender, EventArgs e)
        {
            PasswordPanel.Visible = true;
        }

        
    }
}
