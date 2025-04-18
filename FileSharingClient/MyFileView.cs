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
    public partial class MyFileView: UserControl
    {
        public MyFileView()
        {
            InitializeComponent();
            MyFileLayoutPanel.FlowDirection = FlowDirection.LeftToRight;
            MyFileLayoutPanel.AutoScroll = true;
            

        }

        public void AddFileToView(string fileName, string createAt, string owner, string filesize)
        {
            var fileItem = new FileItemControl(fileName, createAt, owner, filesize);
            MyFileLayoutPanel.Controls.Add(fileItem);
        }

        
    }
}
