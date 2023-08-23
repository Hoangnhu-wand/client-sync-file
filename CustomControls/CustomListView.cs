using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WandSyncFile.CustomControls
{
    public partial class CustomListView : UserControl
    {
        private string createDate;
        private string createTime;
        private string projectName;
        private string action;
        private string count;
        private string status;
        private Color buttonColor = Color.FromArgb(174, 255, 210);
        private Color statusColor = Color.FromArgb(174, 255, 210);
        private string buttonText = null;

        public CustomListView()
        {
            InitializeComponent();
        }

        private void CustomListView_Load(object sender, EventArgs e)
        {

        }

        public string ProjectName
        {
            get { return projectName; }
            set
            {
                projectName = value;
                label3.Text = value;
            }
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

        public Color ButtonColor
        {
            get { return buttonColor; }
            set
            {
                buttonColor = value;
                buttonCustom1.BackColor = buttonColor;
            }
        }
        public Color StatusColor
        {
            get { return statusColor; }
            set
            {
                statusColor = value;
                buttonCustom2.BackColor = statusColor;
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

        public string ButtonText
        {
            get { return buttonText; }
            set
            {
                buttonText = value;
                buttonCustom1.Text = buttonText;
            }
        }

        public string Action
        {
            get { return action; }
            set
            {
                action = value;
                buttonCustom1.Text = value;
            }
        }

        public string Count
        {
            get { return count; }
            set
            {
                count = value;
                lblCount.Text = value;
            }
        }

        public string Status
        {
            get { return status; }
            set
            {
                status = value;
                label3.Text = value;
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void buttonCustom1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void buttonCustom2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click_1(object sender, EventArgs e)
        {

        }
    }
}
