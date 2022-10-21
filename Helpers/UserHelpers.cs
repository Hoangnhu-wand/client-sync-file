using System;
using WandSyncFile.Constants;

namespace WandSyncFile.Helpers
{
    public static class UserHelpers
    {
        public static IntPtr GetToken(string path)
        {
            var isFolder05 = path.Contains(Options.SERVER_FILE_05);
            var isFolder06 = path.Contains(Options.SERVER_FILE_06);
            var isFolder07 = path.Contains(Options.SERVER_FILE_07);
            var isFolder08 = path.Contains(Options.SERVER_FILE_08);
            var isFolder09 = path.Contains(Options.SERVER_FILE_09);

            ServerImpersonate cls = new ServerImpersonate();

            IntPtr token = IntPtr.Zero;

            if (isFolder07)
            {
                token = cls.ImpersonateUser(Options.SERVER_USERNAME_07, Options.SERVER_FILE_07, Options.SERVER_PASSWORD_07);
            }
            else if (isFolder08)
            {
                token = cls.ImpersonateUser(Options.SERVER_USERNAME_08, Options.SERVER_FILE_08, Options.SERVER_PASSWORD_08);
            }
            else if (isFolder05)
            {
                token = cls.ImpersonateUser(Options.SERVER_USERNAME_05, Options.SERVER_FILE_05, Options.SERVER_PASSWORD_05);
            }
            else if (isFolder06)
            {
                token = cls.ImpersonateUser(Options.SERVER_USERNAME_06, Options.SERVER_FILE_06, Options.SERVER_PASSWORD_06);
            }
            else if (isFolder09)
            {
                token = cls.ImpersonateUser(Options.SERVER_USERNAME_09, Options.SERVER_FILE_09, Options.SERVER_PASSWORD_09);
            }
            else
            {
                token = cls.ImpersonateUser(Options.SERVER_USERNAME_07, Options.SERVER_FILE_07, Options.SERVER_PASSWORD_07);
            }

            return token;
        }
    }
}
