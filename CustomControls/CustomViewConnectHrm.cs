using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WandSyncFile.CustomControls
{
    public partial class CustomViewConnectHrm1 : UserControl
    {
        private string createDate;
        private string createTime;
        private string action;

        private Color backgroundColor = Color.Red;

        public CustomViewConnectHrm1()
        {
            InitializeComponent();
        }

        public string CreateTime
        {
            get { return createTime; }
            set
            {
                createTime = value;
                label2.Text = value;
            }
        }
        public string CreatedDate
        {
            get { return createDate; }
            set
            {
                createDate = value;
                label1.Text = value;
            }
        }

        public Color BackgroundColor
        {
            get { return backgroundColor; }
            set
            {
                backgroundColor = value;
            }
        }

        public string Action
        {
            get { return action; }
            set
            {
                action = value;
                label3.Text = value;
            }
        }

        private void CustomViewConnectHrm_Load(object sender, EventArgs e)
        {
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}
