using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            KillRunningInstances();

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

        private static void KillRunningInstances()
        {
            var current = Process.GetCurrentProcess();
            var others = Process.GetProcessesByName(current.ProcessName)
                .Where(p => p.Id != current.Id);

            foreach (var process in others)
            {
                try
                {
                    process.Kill();
                    process.WaitForExit(5000);
                }
                catch (Exception)
                {
                    // Process đã thoát hoặc không có quyền kill - bỏ qua
                }
                finally
                {
                    process.Dispose();
                }
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
