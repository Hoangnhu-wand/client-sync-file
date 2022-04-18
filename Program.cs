using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WandSyncFile.Service;

namespace WandSyncFile
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool logged = Logged();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (logged)
            {
                Application.Run(new FormHome());
            }
            else
            {
                Application.Run(new FormLogin());
            }
        }

        public static bool Logged()
        {
            var token = Properties.Settings.Default.Token;
            
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var account = new AccountService().GetAccount(token);
                    if (account != null)
                    {
                        var accountService = new AccountService();
                        accountService.SettingAccount(token, account);

                        return true;
                    }
                } catch(Exception e)
                {
                    Properties.Settings.Default.Reset();
                    return false;
                }
            }

            return false;
        }
    }
}
