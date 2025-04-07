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
        private ContextMenuStrip contextMenuStripOptions;
        public MyFileView()
        {
            InitializeComponent();
            MyFile_dataGridView.CellMouseClick += MyFile_dataGridView_CellMouseClick;
            contextMenuStripOptions = new ContextMenuStrip();
            contextMenuStripOptions.Items.Add("Chia sẻ File");
            contextMenuStripOptions.Items.Add("Xóa file");
            contextMenuStripOptions.Items.Add("Tải File về");
        }

        private void MyFile_dataGridView_CellMouseClick (object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && e.RowIndex >= 0 && e.ColumnIndex == 4)
            {

                //(Tùy chọn) Chọn dòng hiện tại
                MyFile_dataGridView.ClearSelection();
                MyFile_dataGridView.Rows[e.RowIndex].Selected = true;

                //Hiển thị ContextMenuStrip tại vị trí con trỏ chuột
                contextMenuStrip2.Show(Cursor.Position);
            }
        }
    }
}
