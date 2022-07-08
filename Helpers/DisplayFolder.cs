using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WandSyncFile.Helpers
{
    public class DisplayFolder
    {
        public bool CheckFolderSyncCompleted(string fromPath, string toPath)
        {
            if (!FileHelpers.ExistsPathServer(toPath))
            {
                return false;
            }

            if (!Directory.Exists(fromPath))
            {
                return false;
            }

            var serverSizeDone = FileHelpers.DirSizeServer(toPath);
            var clientSizeDone = FileHelpers.DirSize(fromPath);

            if (serverSizeDone == clientSizeDone)
            {
                return true;
            }

            return false;
        }

        public bool CheckFolderSync(string fromPath, string toPath, string iconChangeFolder = null)
        {
            iconChangeFolder = iconChangeFolder ?? fromPath;
            if (!FileHelpers.ExistsPathServer(toPath))
            {
                return false;
            }

            if (!Directory.Exists(fromPath))
            {
                return false;
            }

            var serverSizeDone = FileHelpers.DirSizeServer(toPath);
            var clientSizeDone = FileHelpers.DirSize(fromPath);

            if (serverSizeDone == clientSizeDone)
            {
                ChangeFolderIconCompleted(iconChangeFolder);
                return true;
            }

            ChangeFolderIconLoading(iconChangeFolder);

            return false;
        }

        public bool CheckFolderFixSync(string clientFixpath, string serverFixPath, string iconChangeFolder = null)
        {
            iconChangeFolder = iconChangeFolder ?? clientFixpath;
            if (!FileHelpers.ExistsPathServer(serverFixPath))
            {
                return false;
            }

            if (!Directory.Exists(clientFixpath))
            {
                return false;
            }
            var clientFolderSize = FileHelpers.DirSize(clientFixpath);
            var listFileFixClient = FileHelpers.LocalGetListFile(clientFixpath);

            var listFilePath = new List<string>();

            foreach (var localPath in listFileFixClient)
            {
                var clientFileArr = localPath.Split(new string[] { clientFixpath }, StringSplitOptions.None);
                listFilePath.Add(clientFileArr.Last());
            }

            var listServerFixPathFile = listFilePath.Select(item => string.Concat(serverFixPath, item)).ToList();

            var serverFolderSize = FileHelpers.FilesSizeServer(listServerFixPathFile);

            if (clientFolderSize == serverFolderSize)
            {
                ChangeFolderIconCompleted(iconChangeFolder);
                return true;
            }

            ChangeFolderIconLoading(iconChangeFolder);

            return false;
        }

        public void ChangeFolderIconCompleted(string folderPath)
        {
            var desktopFile = Path.Combine(folderPath, "desktop.ini");
            if (File.Exists(desktopFile))
            {
                File.Delete(desktopFile);
            }

            var icon = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\icon-done.ico";

            LPSHFOLDERCUSTOMSETTINGS FolderSettings = new LPSHFOLDERCUSTOMSETTINGS();
            FolderSettings.dwMask = 0x10;
            FolderSettings.pszIconFile = icon;
            FolderSettings.iIconIndex = 0;

            UInt32 FCS_READ = 0x00000001;
            UInt32 FCS_FORCEWRITE = 0x00000002;
            UInt32 FCS_WRITE = FCS_READ | FCS_FORCEWRITE;

            string pszPath = folderPath;
            SHGetSetFolderCustomSettings(ref FolderSettings, pszPath, FCS_WRITE);
        }

        public void ChangeFolderIconLoading(string folderPath)
        {
            var desktopFile = Path.Combine(folderPath, "desktop.ini");
            if (File.Exists(desktopFile))
            {
                File.Delete(desktopFile);
            }

            var icon = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\icon-sync.ico";

            LPSHFOLDERCUSTOMSETTINGS FolderSettings = new LPSHFOLDERCUSTOMSETTINGS();
            FolderSettings.dwMask = 0x10;
            FolderSettings.pszIconFile = icon;
            FolderSettings.iIconIndex = 0;

            UInt32 FCS_READ = 0x00000001;
            UInt32 FCS_FORCEWRITE = 0x00000002;
            UInt32 FCS_WRITE = FCS_READ | FCS_FORCEWRITE;

            string pszPath = folderPath;
            SHGetSetFolderCustomSettings(ref FolderSettings, pszPath, FCS_WRITE);
        }

        public void ChangeFolderIconFail(string folderPath)
        {
            var desktopFile = Path.Combine(folderPath, "desktop.ini");
            if (File.Exists(desktopFile))
            {
                File.Delete(desktopFile);
            }

            var icon = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\icon-fail.ico";

            LPSHFOLDERCUSTOMSETTINGS FolderSettings = new LPSHFOLDERCUSTOMSETTINGS();
            FolderSettings.dwMask = 0x10;
            FolderSettings.pszIconFile = icon;
            FolderSettings.iIconIndex = 0;

            UInt32 FCS_READ = 0x00000001;
            UInt32 FCS_FORCEWRITE = 0x00000002;
            UInt32 FCS_WRITE = FCS_READ | FCS_FORCEWRITE;

            string pszPath = folderPath;
            SHGetSetFolderCustomSettings(ref FolderSettings, pszPath, FCS_WRITE);
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void SHChangeNotify(
            int wEventId, int uFlags, IntPtr dwItem1, IntPtr dwItem2);

        [DllImport("Shell32.dll", CharSet = CharSet.Auto)]
        static extern UInt32 SHGetSetFolderCustomSettings(ref LPSHFOLDERCUSTOMSETTINGS pfcs, string pszPath, UInt32 dwReadWrite);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct LPSHFOLDERCUSTOMSETTINGS
        {
            public UInt32 dwSize;
            public UInt32 dwMask;
            public IntPtr pvid;
            public string pszWebViewTemplate;
            public UInt32 cchWebViewTemplate;
            public string pszWebViewTemplateVersion;
            public string pszInfoTip;
            public UInt32 cchInfoTip;
            public IntPtr pclsid;
            public UInt32 dwFlags;
            public string pszIconFile;
            public UInt32 cchIconFile;
            public int iIconIndex;
            public string pszLogo;
            public UInt32 cchLogo;
        }

    }
}
