using System;

namespace WandSyncFile.Helpers
{
    public static class UserHelpers
    {
        public static IntPtr GetToken(string path)
        {
            var isFolder07 = path.Contains(Options.SERVER_FILE_07);
            var isFolder08 = path.Contains(Options.SERVER_FILE_08);

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
            else
            {
                token = cls.ImpersonateUser(Options.SERVER_USERNAME_07, Options.SERVER_FILE_07, Options.SERVER_PASSWORD_07);
            }

            return token;
        }
    }
}
