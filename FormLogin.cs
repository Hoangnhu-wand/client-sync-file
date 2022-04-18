using Microsoft.AspNet.SignalR.Client;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using WandSyncFile.Helpers;
using WandSyncFile.Service;

namespace WandSyncFile
{
    public partial class FormLogin : Form
    {
        AccountService accountService = new AccountService();
        public FormLogin()
        {
            InitializeComponent();
            setupAutoRun();
            error.Hide();
           // txtUsername.IsFocus = true;
        }

        private void OnCloseFormClick(object sender, EventArgs e)
        {
            Application.Exit();
        }

        public void setupAutoRun()
        {
            RegistryKey reg = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (reg.GetValue("Wand-Developed") == null)
            {
                reg.SetValue("Wand-Developed", Application.ExecutablePath.ToString());
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Pen blackPen = new Pen(Color.Red, 3);
            PointF point1 = new PointF(100.0F, 180.0F);
            PointF point2 = new PointF(100.0F, 100.0F);
            //label1

            e.Graphics.DrawLine(blackPen, point1, point2);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textboxCustom1_Load(object sender, EventArgs e)
        {

        }

        private void headerpanel_Paint(object sender, PaintEventArgs e)
        {
            headerpanel.Location = new Point(0, 0);
        }

        private void minMaxButton1_Click(object sender, EventArgs e)
        {

        }

        private void signin_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        public void LoginToHrm()
        {
            var userName = txtUsername.TextBox1Value.Trim();
            var password = txtPassword.TextBox1Value.Trim();

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                error.Show();
                error.Text = "Username and Password is required!";
                return;
            }

            WebClient clients = new WebClient();
            clients.Headers[HttpRequestHeader.ContentType] = "application/json";
            string sendDirectorUrl = Url.Login;
            var postData = new
            {
                userName = userName,
                password = password
            };

            try
            {
                var test = JsonSerializer.Serialize(postData);
                var response = clients.UploadString(sendDirectorUrl, "POST", JsonSerializer.Serialize(postData));

                var token = JsonSerializer.Deserialize<Data.Mapping.AccountDto>(response);
                var accesstoken = token.accessToken.token;

                var account = accountService.GetAccount(accesstoken);
                accountService.SettingAccount(accesstoken, account);
                ShowIndex();
            }
            catch (Exception e)
            {
                error.Show();
                if (e.GetError().StatusCode == HttpStatusCode.Unauthorized)
                {
                    error.Text = "Invalid username or password.";
                }
                else
                {
                    error.Text = e.Message;
                    MessageBox.Show(e.Message);
                }
            }
        }

        public void ShowIndex()
        {
            var token = Properties.Settings.Default.Token;
            if (!string.IsNullOrEmpty(token))
            {
                var account = accountService.GetAccount(token);
                if (account != null)
                {
                    accountService.SettingAccount(token, account);

                    this.Hide();
                    FormHome frmHome = new FormHome();
                    frmHome.Show();
                }
                else
                {
                    this.Show();
                    Properties.Settings.Default.Reset();
                }
            }

        }
        private void buttonCustom1_Click(object sender, EventArgs e)
        {

        }

        private void textboxCustom1_Load_1(object sender, EventArgs e)
        {

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }

        private void buttonCustom2_Click(object sender, EventArgs e)
        {
            LoginToHrm();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textboxCustom1_Load_2(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            Hide();
            notifyIcon1.Visible = true;
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ShowInTaskbar = true;
            Show();
        }

        private void pictureBox2_Click_1(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void txtUsername_Load(object sender, EventArgs e)
        {

        }

        private void headerpanel_Paint_1(object sender, PaintEventArgs e)
        {

        }

        private void FrmLogin_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                //e.Cancel = true;
                //ShowInTaskbar = false;
                //Hide();
            }
        }

        private void OnUsernameKeyDown(object sender, KeyEventArgs e)
        {
            MessageBox.Show("123123");
        }
    }
}
