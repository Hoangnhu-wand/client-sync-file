using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using WandSyncFile.Service;
using static WandSyncFile.Constants.Values;
using WandSyncFile.Constants;
using System.Drawing;
using System.Threading;
using WandSyncFile.Data.Mapping;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Management;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using System.Windows.Forms.VisualStyles;
using System.Net.NetworkInformation;

namespace WandSyncFile.Helpers
{
    public static class FileHelpers
    {
        public static bool IsDoLocalPath(string filePath)
        {
            var projectService = new ProjectService();
            var doPath = projectService.GetProjectDoPath(Properties.Settings.Default.ProjectLocalPath, filePath);

            var isChangeDo = (filePath.Contains(doPath + "\\") || (filePath + "\\").Contains(doPath + "\\")) && filePath != doPath;

            return isChangeDo;
        }

        public static bool IsSampleLocalPath(string filePath)
        {
            var projectService = new ProjectService();
            var samplePath = projectService.GetProjectSamplePath(Properties.Settings.Default.ProjectLocalPath, filePath);
            var isChangeSample = (filePath.Contains(samplePath + "\\") || (filePath + "\\").Contains(samplePath + "\\")) && filePath != samplePath;

            return isChangeSample;
        }

        public static bool CheckHardDriveCanDowload()
        {
            var localPathProject = Properties.Settings.Default.ProjectLocalPath;
            var freeSpace = FreeSpace(localPathProject);
            var allSpace = GetTotalNumberOfBytes(localPathProject);
            var used = allSpace - freeSpace;
            var canUse = (allSpace * 100) / 100;

            return used < canUse;
        }

        public static string GetHardDriveCanUploadByTeam(List<string> allDrive)
        {
            var i = 0;
            string folder = String.Empty;
            while (i < allDrive.Count && string.IsNullOrEmpty(folder))
            {
                var currentFolder = allDrive[i];
                var freeSpace = FreeSpace(allDrive[i]);
                var allSpace = GetTotalNumberOfBytes(currentFolder);
                var used = allSpace - freeSpace;
                var canUse = (allSpace * 85) / 100;
                if (used < canUse)
                {
                    folder = currentFolder;
                    break;
                }
                i++;
            }

            return folder;
        }

        public static long FreeSpace(string folderName)
        {
            if (string.IsNullOrEmpty(folderName))
            {
                throw new ArgumentNullException("folderName");
            }

            if (!folderName.EndsWith("\\"))
            {
                folderName += '\\';
            }

            long free = 0, dummy1 = 0, dummy2 = 0;

            if (GetDiskFreeSpaceEx(folderName, ref free, ref dummy1, ref dummy2))
            {
                return free;
            }
            else
            {
                return -1;
            }
        }
        public static long GetTotalNumberOfBytes(string folderName)
        {
            if (string.IsNullOrEmpty(folderName))
            {
                throw new ArgumentNullException("folderName");
            }

            if (!folderName.EndsWith("\\"))
            {
                folderName += '\\';
            }

            long free = 0, dummy1 = 0, dummy2 = 0;

            if (GetDiskFreeSpaceEx(folderName, ref free, ref dummy1, ref dummy2))
            {
                return dummy1;
            }
            else
            {
                return -1;
            }
        }

        public static long GetTotalNumberOfFreeBytes(string folderName)
        {
            if (string.IsNullOrEmpty(folderName))
            {
                throw new ArgumentNullException("folderName");
            }

            if (!folderName.EndsWith("\\"))
            {
                folderName += '\\';
            }

            long free = 0, dummy1 = 0, dummy2 = 0;

            if (GetDiskFreeSpaceEx(folderName, ref free, ref dummy1, ref dummy2))
            {
                return dummy2;
            }
            else
            {
                return -1;
            }
        }

        public static long DirSize(string path)
        {
            var d = new DirectoryInfo(path);
            long size = 0;
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                FileAttributes attributes = File.GetAttributes(fi.FullName);
                var fromPathEx = Path.GetExtension(fi.FullName);

                if (!attributes.HasFlag(FileAttributes.Hidden) && fromPathEx.ToLower() != ".tmp")
                {
                    size += fi.Length;
                }
            }
            DirectoryInfo[] dis = d.GetDirectories();

            foreach (DirectoryInfo di in dis)
            {
                size += DirSize(di.FullName);
            }
            return size;
        }
        public static long FilesSize(List<string> files)
        {
            long size = 0;
            FileInfo[] fis = files.Select(item => new FileInfo(item)).ToArray();
            foreach (FileInfo fi in fis)
            {
                FileAttributes attributes = File.GetAttributes(fi.FullName);
                var fromPathEx = Path.GetExtension(fi.FullName);

                if (!attributes.HasFlag(FileAttributes.Hidden) && fromPathEx.ToLower() != ".tmp")
                {
                    size += fi.Length;
                }
            }

            return size;
        }

        public static List<string> LocalGetListFile(string path)
        {
            var listFileClient = Directory.GetFiles(path).Where(item => !File.GetAttributes(item).HasFlag(FileAttributes.Hidden)).Select(item => Path.GetFullPath(item)).ToList();

            var listFolders = Directory.GetDirectories(path);

            foreach (var folder in listFolders)
            {
                var listFileInFolder = Directory.GetFiles(path).Where(item => !File.GetAttributes(item).HasFlag(FileAttributes.Hidden)).Select(item => Path.GetFullPath(item)).ToList();
                listFileClient.AddRange(listFileInFolder);
            }

            return listFileClient;
        }

        public static bool ExistsPathServer(string path)
        {
            try
            {
                    return Directory.Exists(path);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return false;
        }

        public static long DirSizeServer(string path)
        {
            try
            {
                    var size = DirSize(path);

                    return size;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return 0;
        }

        public static long FilesSizeServer(List<string> files)
        {
            if (files.Count() == 0) return 0;

            try
            {
                    var size = FilesSize(files);

                    return size;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return 0;
        }


        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage"), SuppressUnmanagedCodeSecurity]
        [DllImport("Kernel32", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]

        private static extern bool GetDiskFreeSpaceEx
        (
            string lpszPath,                    // Must name a folder, must end with '\'.
            ref long lpFreeBytesAvailable,
            ref long lpTotalNumberOfBytes,
            ref long lpTotalNumberOfFreeBytes
        );

        public static void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public static List<string> GetAllFolder(string path)
        {
            try
            {
                string[] subdirectoryEntries = Directory.GetDirectories(path);

                var directorName = new List<string>();
                foreach (var director in subdirectoryEntries)
                {
                    if (HasFileInFolder(director))
                    {
                        directorName.Add(new DirectoryInfo(director).Name);
                    }
                }

                return directorName;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
        }

        public static bool HasFileInFolder(string path)
        {
            try
            {
                if (Directory.GetFiles(path).Where(item => !File.GetAttributes(item).HasFlag(FileAttributes.Hidden)).Count() > 0)
                {
                    return true;
                }

                string[] folders = Directory.GetDirectories(path);
                var i = 0;
                bool hasFile = false;

                while (i < folders.Length && !hasFile)
                {
                    hasFile = HasFileInFolder(folders[i]);
                    i++;
                }

                return hasFile;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
        }

        public static void SalesSyncFileFromServer(string projectServerPath, string projectLocalPath, string path)
        {
            try
            {
                // không xóa file / fo ở DO / Sample
                var serverFileArr = path.Split(new string[] { projectServerPath }, StringSplitOptions.None);
                if (serverFileArr.Length <= 0)
                {
                    return;
                }
                var updateLocalPath = projectLocalPath + serverFileArr.Last();
                updateLocalPath = Path.GetFullPath(updateLocalPath);

                var isFolder = Directory.Exists(updateLocalPath) && Directory.GetFiles(updateLocalPath).Length > 0;

                if (isFolder && !Directory.Exists(path) && !IsDoLocalPath(updateLocalPath) && !IsSampleLocalPath(updateLocalPath))
                {
                    Directory.Delete(updateLocalPath, true);
                }

                if (!File.Exists(path) && File.Exists(updateLocalPath) && !IsDoLocalPath(updateLocalPath) && !IsSampleLocalPath(updateLocalPath))
                {
                    File.Delete(updateLocalPath);
                }

                // Thêm file
                if (File.Exists(path))
                {
                    CopyFile(path, updateLocalPath);
                }

                var isServerFolder = Directory.Exists(path) && Directory.GetFiles(path).Length > 0;
                // Thêm folder
                if (isServerFolder && Directory.Exists(path))
                {
                    DownloadFolder(path, updateLocalPath);
                }

            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
        }

        // Update các file khác thư mục Do/Sample
        public static void SaleSyncUpdateFromServer(string serverPath, string localPath)
        {
            if (IsDoLocalPath(localPath) || IsSampleLocalPath(localPath))
            {
                return;
            }

            FileInfo[] localFileInfo = new DirectoryInfo(serverPath).GetFiles();
            var fromFiles = localFileInfo.Select(item => item.Name).ToList();

            if (Directory.Exists(serverPath) && Directory.Exists(localPath))
            {
                FileInfo[] fileInfo = new DirectoryInfo(localPath).GetFiles();
                var toFiles = fileInfo.Select(item => item.Name).ToList();

                var updateFiles = toFiles.Where(toFile => fromFiles.Any(fromFile => fromFile == toFile)).ToList();
                if (updateFiles != null && updateFiles.Count > 0)
                {
                    foreach (var updateFile in updateFiles)
                    {
                        var fileFromPath = Path.Combine(serverPath, updateFile);
                        var fileToPath = Path.Combine(localPath, updateFile);
                        CopyFile(fileFromPath, fileToPath);
                    }
                }

                // update ảnh trong folder
                string[] folders = Directory.GetDirectories(serverPath);
                foreach (string folder in folders)
                {
                    string name = Path.GetFileName(folder);
                    string dest = Path.Combine(localPath, name);
                    SaleSyncUpdateFromServer(folder, dest);
                }
            }
        }

        // Chỉ xóa file ở các thư mục # do / Sample
        public static void SaleSyncDeleteFromServer(string serverPath, string localPath)
        {
            if (IsDoLocalPath(localPath) || IsSampleLocalPath(localPath))
            {
                return;
            }

            FileInfo[] localFileInfo = new DirectoryInfo(serverPath).GetFiles();
            var fromFiles = localFileInfo.Select(item => item.Name).ToList();

            if (Directory.Exists(serverPath) && Directory.Exists(localPath))
            {
                FileInfo[] fileInfo = new DirectoryInfo(localPath).GetFiles();
                var toFiles = fileInfo.Select(item => item.Name).ToList();

                var deleteFiles = toFiles.Where(toFile => !fromFiles.Any(fromFile => fromFile == toFile)).ToList();
                if (deleteFiles != null && deleteFiles.Count > 0)
                {
                    foreach (var deleteFile in deleteFiles)
                    {
                        var file = Path.Combine(localPath, deleteFile);
                        File.Delete(file);
                    }
                }

                // Xóa ảnh trong folder
                string[] folders = Directory.GetDirectories(serverPath);
                foreach (string folder in folders)
                {
                    string name = Path.GetFileName(folder);
                    string dest = Path.Combine(localPath, name);
                    SaleSyncDeleteFromServer(folder, dest);
                }

                // xóa folders
                string[] foldersToPath = Directory.GetDirectories(localPath);
                var deleteFolders = foldersToPath.Where(toFolder => !folders.Any(fromFolder => fromFolder == toFolder)).ToList();
                foreach (var dir in deleteFolders)
                {
                    new DirectoryInfo(dir).Delete(true);
                }
            }
        }

        public static void DownloadFolder(string fromPath, string toPath, string withoutFolder = null, bool isRemoveNotExists = false, bool isReplaceExists = false, bool isCopyAllFile = false)
        {
            try
            {
                if (!Directory.Exists(toPath))
                {
                    Directory.CreateDirectory(toPath);
                }

                FileInfo[] fromFileInfo = new DirectoryInfo(fromPath).GetFiles();
                var fromFiles = fromFileInfo.Select(item => item.Name).ToList();

                FileInfo[] toFileInfo = new DirectoryInfo(toPath).GetFiles();
                var toFiles = toFileInfo.Select(item => item.Name).ToList();

                var newFiles = fromFiles.Where(fromFile => !toFiles.Any(toFile => fromFile == toFile)).ToList();
                string projectPath = null;
 
                if (isRemoveNotExists)
                {
                    // Xóa file khong co trong fromPath
                    var removeFiles = toFiles.Where(toFile => !fromFiles.Any(fromFile => fromFile == toFile)).ToList();
                    foreach (var deleteFile in removeFiles)
                    {
                        var deleteFileItem = Path.Combine(toPath, deleteFile);
                        if (File.Exists(deleteFileItem))
                        {
                            File.Delete(deleteFileItem);
                        }
                    }
                }

                if (isReplaceExists)
                {
                    // Ghi đè file
                    var existsFiles = fromFiles.Where(fromFile => toFiles.Any(toFile => fromFile == toFile)).ToList();
                    foreach (var existsFile in existsFiles)
                    {
                        var fromExistsFileFile = Path.Combine(fromPath, existsFile);
                        string toExistsFileFile = toPath + "/" + existsFile;
                        CopyFile(fromExistsFileFile, toExistsFileFile, 1024 * 1024 * 5);
                    }
                }

                if (newFiles != null && newFiles.Count > 0)
                {
                    foreach (var newFile in newFiles)
                    {
                        var fileFromPath = Path.Combine(fromPath, newFile);
                        string fileToPath = toPath + "/" + newFile;
                        CopyFile(fileFromPath, fileToPath, 1024 * 1024 * 5);
                    }
                }

                // copy folders
                string[] folders = Directory.GetDirectories(fromPath);
                if (!string.IsNullOrEmpty(withoutFolder))
                {
                    withoutFolder = withoutFolder.Replace(@"/", "\\");
                }

                foreach (string folder in folders)
                {
                    if (string.IsNullOrEmpty(withoutFolder) || (withoutFolder != folder))
                    {
                        string name = Path.GetFileName(folder);
                        string dest = Path.Combine(toPath, name);
                        DownloadFolder(folder, dest, withoutFolder, isRemoveNotExists, isReplaceExists);
                    }
                }

            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
        }

        public static void DownloadFolderFromServer(string fromPath, string toPath, string withoutFolder = null, bool isRemoveNotExists = false, bool isReplaceExists = false, bool isCopyAllFile = false)
        {
            try
            {
                    DownloadFolder(fromPath, toPath, withoutFolder, isRemoveNotExists, isReplaceExists, isCopyAllFile);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static List<string> ServerGetListFileNameByFolder(string path)
        {
            try
            {
                    return GetListFileNameByFolder(path);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }


        public static List<string> GetListFileNameByFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                return null;
            }

            var files = Directory.GetFiles(path).Where(item => !File.GetAttributes(item).HasFlag(FileAttributes.Hidden)).Select(item => Path.GetFileName(item)).ToList();

            var directories = Directory.GetDirectories(path);

            foreach (var dir in directories)
            {
                var fileInDir = GetListFileNameByFolder(dir);
                files.AddRange(fileInDir);
            }

            return files;
        }

        public static List<string> ServerGetListFixPathByDoneName(List<string> listDoneName, string fixPath)
        {

            try
            {
                    return GetListFixPathByDoneName(listDoneName, fixPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }

        public static List<string> GetListFixPathByDoneName(List<string> listDoneName, string fixPath)
        {
            try
            {
                if (!Directory.Exists(fixPath))
                {
                    return null;
                }

                var fixFiles = Directory.GetFiles(fixPath).Where(item => !File.GetAttributes(item).HasFlag(FileAttributes.Hidden))
                    .Select(item => Path.GetFullPath(item)).ToList();

                if (listDoneName != null && listDoneName.Count() > 0)
                {
                    fixFiles = fixFiles.Where(itemFixPath => listDoneName.Any(doneName => itemFixPath.Trim().ToLower().Contains(doneName.Trim().ToLower()))).ToList();
                }

                var dir = Directory.GetDirectories(fixPath);
                foreach (var dirItem in dir)
                {
                    var files = GetListFixPathByDoneName(listDoneName, Path.GetFullPath(dirItem));

                    fixFiles.AddRange(files);
                }

                return fixFiles;

            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }

            return null;
        }

        public static async void CopyFile(string fromPath, string toPath, int eachReadLength = 1024 * 1024)
        {
            try
            {             
                FileAttributes attributes = File.GetAttributes(fromPath);

                if (attributes.HasFlag(FileAttributes.Hidden) || (File.Exists(fromPath) && File.Exists(toPath) && DateTime.Compare(File.GetLastWriteTime(fromPath), File.GetLastWriteTime(toPath)) <= 0))
                {
                    return;
                }

                var fromPathEx = Path.GetExtension(fromPath);
                if (fromPathEx.ToLower() == ".tmp")
                {
                    return;
                }

                var directoryName = new FileInfo(toPath).Directory.FullName;
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }


                File.Copy(fromPath, toPath, true);

                var getListWrite = File.GetLastWriteTime(fromPath);
                File.SetLastWriteTime(toPath, getListWrite);

                var localPath = Properties.Settings.Default.ProjectLocalPath;
                if (toPath.IndexOf(Options.PROJECT_DONE_NAME) > -1 && toPath.IndexOf(localPath) > -1)
                {
                    var workingPath = toPath.Replace("\\" + Options.PROJECT_DONE_NAME + "\\", "\\" + Options.PROJECT_WORKING_PATH_NAME + "\\");
                    await CopyFileAsync(toPath, workingPath);
                }
              
            }
            catch (Exception e)
            {
                var errMessage = DateTime.Now.ToString() + " " + e.Message + " ---- " + fromPath;
                WriteLog(errMessage);
            }
        }

        // Đồng bộ những file đã xóa ở from -> to và có trong file delete của dự án: Chỉ lấy file Do
        // FromPath:Thư mục dự án Máy nhân viên
        // toPath: Thư mục dự án Máy server

        public static void SyncFolderDelete(string fromPath, string toPath)
        {
            try
            {
                // get all File in delete
                var deleteFilePath = Path.Combine(fromPath, Options.FILE_DELETE);
                if (File.Exists(deleteFilePath))
                {
                    Uri localPathUri = new Uri(Properties.Settings.Default.ProjectLocalPath);
                    var projectLocalPath = localPathUri.LocalPath;
                    var allFileDelete = new List<string>();
                    foreach (string line in System.IO.File.ReadLines(deleteFilePath))
                    {
                        fromPath = Path.GetFullPath(fromPath);
                        var projectPathFile = line.Split(new string[] { fromPath }, StringSplitOptions.RemoveEmptyEntries);
                        var fileName = projectPathFile[projectPathFile.Length - 1];
                        allFileDelete.Add(fileName);
                    }

                    foreach (var deleteFile in allFileDelete)
                    {

                        var serverFile = Path.GetFullPath(toPath + deleteFile);
                        var localFile = Path.GetFullPath(fromPath + deleteFile);
                        if (File.Exists(serverFile) && !File.Exists(localFile))
                        {
                            File.Delete(serverFile);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
        }

        public static void WriteToFileDeleteProject(string fileDelete)
        {
            try
            {
                var projectService = new ProjectService();
                var curentDate = DateTime.Now.ToString("yyyyMMddHHmmss");
                var localProjectName = projectService.GetProjectName(fileDelete);
                if (string.IsNullOrEmpty(localProjectName))
                {
                    return;
                }
                var deletePath = Path.Combine(Properties.Settings.Default.ProjectLocalPath, localProjectName + "_" + Options.FILE_DELETE + "_" + curentDate + ".txt");
                if (!File.Exists(deletePath))
                {
                    using (var file = File.Create(deletePath))
                    {
                        File.SetAttributes(deletePath, FileAttributes.Hidden);
                    }
                }
                File.AppendAllText(deletePath, fileDelete + Environment.NewLine);

            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
        }


        public static void CreateFolder(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
        }

        public static void CopyDirectoryToServer(string fromPath, string toPath)
        {

            try
            {
                    CopyDirectory(fromPath, toPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }


        public static void SyncDirectoryToServer(string fromPath, string toPath)
        {
            try
            {
                    SyncDirectory(fromPath, toPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void SyncDirectoryDoneToServer(string fromPath, string toPath)
        {
            try
            {
                    SyncDirectoryDone(fromPath, toPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void SyncDirectoryDone(string fromPath, string toPath)
        {
            // sync done from client to server
            DownloadFolder(fromPath, toPath, null, false, true);

            Thread.Sleep(2000);
         
            // sync done from server to client
            DownloadFolder(toPath, fromPath, null, false, true);

            Thread.Sleep(2000);

        }

        public static void SyncDirectory(string fromPath, string toPath, string notMoveFolder = null)
        {
            if (!Directory.Exists(toPath))
            {
                Directory.CreateDirectory(toPath);
            }
            var files = Directory.GetFiles(fromPath);
            var directories = Directory.GetDirectories(fromPath);
            directories = directories.Where(di => di != toPath).ToArray();

            if (!string.IsNullOrEmpty(notMoveFolder))
            {
                notMoveFolder = Path.GetFullPath(notMoveFolder);
                directories = directories.Where(di => di != notMoveFolder).ToArray();
            }
            var toFiles = Directory.GetFiles(toPath);
            var toDirs = Directory.GetDirectories(toPath);
            var removeFiles = toFiles.Where(toFile => !files.Any(fromFile => new FileInfo(fromFile).Name == new FileInfo(toFile).Name)).ToList();

            foreach (var deleteFile in removeFiles)
            {
                if (File.Exists(deleteFile))
                {
                    File.Delete(deleteFile);
                }
            }

            var removeFolders = toDirs.Where(toDir => !directories.Any(fromFile => new FileInfo(fromFile).Name == new FileInfo(toDir).Name)).ToList();
            foreach (var deleteDir in removeFolders)
            {
                if (Directory.Exists(deleteDir))
                {
                    Directory.Delete(deleteDir, true);
                }
            }

            foreach (string s in files)
            {
                CopyFile(s, Path.Combine(toPath, Path.GetFileName(s)));
            }
            foreach (string d in directories)
            {
                CopyDirectory(Path.Combine(fromPath, Path.GetFileName(d)), Path.Combine(toPath, Path.GetFileName(d)));
            }
        }

        public static void CopyDirectory(string fromPath, string toPath, string notMoveFolder = null)
        {
            if (!Directory.Exists(toPath))
            {
                Directory.CreateDirectory(toPath);
            }
            if (!Directory.Exists(fromPath))
            {
                return;
            }
            var files = Directory.GetFiles(fromPath);
            var directories = Directory.GetDirectories(fromPath);
            directories = directories.Where(di => di != toPath).ToArray();

            if (!string.IsNullOrEmpty(notMoveFolder))
            {
                notMoveFolder = Path.GetFullPath(notMoveFolder);
                directories = directories.Where(di => di != notMoveFolder).ToArray();
            }

            foreach (string s in files)
            {
                CopyFile(s, Path.Combine(toPath, Path.GetFileName(s)));
            }
            foreach (string d in directories)
            {
                CopyDirectory(Path.Combine(fromPath, Path.GetFileName(d)), Path.Combine(toPath, Path.GetFileName(d)));
            }
        }

        public static void CopyFolderProject(string fromPath, string toPath, string sampleFolder = null)
        {
            try
            {
                if (!Directory.Exists(fromPath))
                {
                    return;
                }

                // Neu Do chi có 1 folder Do và không có ảnh ngoài Do => Lấy luôn thư mục DO
                string[] folderLocal = Directory.GetDirectories(fromPath);

                var localIsDo = false;

                if (!string.IsNullOrEmpty(sampleFolder))
                {
                    localIsDo = folderLocal.Length > 0 && folderLocal.Any(f => Path.GetFileName(f) == Options.PROJECT_DO_NAME)
                    && !folderLocal.Any(f => Path.GetFileName(f) != Options.PROJECT_DO_NAME && f != sampleFolder)
                    && (new DirectoryInfo(fromPath).GetFiles().Length <= 0);
                }
                else
                {
                    localIsDo = folderLocal.Length > 0 && folderLocal.Any(f => Path.GetFileName(f) == Options.PROJECT_DO_NAME)
                    && (new DirectoryInfo(fromPath).GetFiles().Length <= 0);
                }

                if (!string.IsNullOrEmpty(fromPath) && Directory.Exists(fromPath) && localIsDo)
                {
                    fromPath = Path.Combine(fromPath, Options.PROJECT_DO_NAME);
                }

                if (!string.IsNullOrEmpty(fromPath) && Directory.Exists(fromPath) && fromPath != toPath)
                {
                    var notCopyFolder = Path.GetFileName(toPath) != Options.PROJECT_SAMPLE_NAME ? sampleFolder : null;
                    CopyDirectory(fromPath, toPath, sampleFolder);
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
        }

        public static void EditorCreateFolderDonePath(string projectPath, string projectName)
        {
            var editorUserName = Properties.Settings.Default.Username;

            var projectLocalPath = Path.Combine(Properties.Settings.Default.ProjectLocalPath, projectName);
            var projectLocalDonePath = Path.Combine(projectLocalPath, Options.PROJECT_DONE_NAME);

            var editorLocalDonePath = Path.Combine(projectLocalDonePath, editorUserName);

            var serverDonePath = Path.Combine(projectPath, Options.PROJECT_DONE_NAME);
            var serverEditorDonePath = Path.Combine(serverDonePath, editorUserName);

            SyncCreateFolderName(serverEditorDonePath, editorLocalDonePath);

        }

        public static void SyncCreateFolderName(string fromDir, string toDir)
        {
            if (!Directory.Exists(fromDir))
            {
                return;
            }

            if (!Directory.Exists(toDir))
            {
                CreateFolder(toDir);
            }

            var doneDirectories = Directory.GetDirectories(fromDir);
            foreach (var dir in doneDirectories)
            {
                // parent folder
                var dirInfo = new DirectoryInfo(dir);
                var dirName = dirInfo.Name;
                var toDirName = Path.Combine(toDir, dirName);

                CreateFolder(toDirName);

                var subDirectories = Directory.GetDirectories(dir);
                if (subDirectories.Count() <= 0)
                {
                    continue;
                }
                SyncCreateFolderName(dirInfo.FullName, toDirName);
            }
        }

        public static string GetProjectLocalPath(string projectName)
        {
            var localPath = Properties.Settings.Default.ProjectLocalPath;

            if (string.IsNullOrEmpty(localPath))
            {
                return "";
            }
            var editorProjectPath = Path.Combine(localPath, projectName);

            return editorProjectPath;
        }

        public static string GetProjectDoEditorLocalPath(string projectName)
        {
            var editorUserName = Properties.Settings.Default.Username;

            var projectLocalPath = GetProjectLocalPath(projectName);
            var editorDoPath = Path.Combine(projectLocalPath, Options.PROJECT_DO_NAME);

            return Path.Combine(editorDoPath, editorUserName);
        }

        public static string GetProjectDoneEditorLocalPath(string projectName)
        {
            var editorUserName = Properties.Settings.Default.Username;
            var localPath = Properties.Settings.Default.ProjectLocalPath;

            var projectLocalPath = Path.Combine(localPath, projectName);
            var projectDoneLocalPath = Path.Combine(projectLocalPath, Options.PROJECT_DONE_NAME);

            return Path.Combine(projectDoneLocalPath, editorUserName);
        }

        public static string GetProjectSampleLocalPath(string projectName)
        {
            var localPath = Properties.Settings.Default.ProjectLocalPath;
            var projectLocalPath = Path.Combine(localPath, projectName);

            var sampleLocalPath = Path.Combine(projectLocalPath, Options.PROJECT_SAMPLE_NAME);

            return sampleLocalPath;
        }

        public static string GetEditorProjectDoServerPath(string projectPath)
        {
            var editorUserName = Properties.Settings.Default.Username;
            var editorDoPath = Path.Combine(projectPath, Options.PROJECT_DO_NAME);

            return Path.Combine(editorDoPath, editorUserName);
        }

        public static string GetEditorProjectFixLocalPath(string projectName)
        {
            var editorLocalPath = Properties.Settings.Default.ProjectLocalPath;
            var editorProjectPath = Path.Combine(editorLocalPath, projectName);
            var path = Path.Combine(editorProjectPath, Options.PROJECT_FIX_PATH_NAME);

            return path;
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword,
    int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        [DllImport("kernel32.dll")]
        private static extern Boolean CloseHandle(IntPtr hObject);


        public static void CopyFileFromServer(string serverPath, string localPath)
        {
            try
            {           
                    CopyFile(serverPath, localPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static bool DeleteFileFromServer(string path)
        {
            try
            {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                    return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public static bool DeleteFolderFromServer(string path)
        {
            try
            {
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                    }
                    return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public static string EditorLocalDonePath(string projectName)
        {
            var editorLocalPath = Properties.Settings.Default.ProjectLocalPath;
            var editorUserName = Properties.Settings.Default.Username;

            var editorProjectPath = Path.Combine(editorLocalPath, projectName);
            var projectLocalDonePath = Path.Combine(editorProjectPath, Options.PROJECT_DONE_NAME);
            var editorProjectDonePath = Path.Combine(projectLocalDonePath, editorUserName);

            return editorProjectDonePath;
        }

        public static bool ProjectServerHasFix(string projectPath)
        {
            try
            {
                    var projectDirectoties = Directory.GetDirectories(projectPath).Select(item => item.ToLower()).ToList();
                    var folderFixName = Options.PROJECT_FIX_PATH_NAME.ToLower();
                    var fixFolders = projectDirectoties.Any(item => Path.GetFileName(item).Trim().StartsWith(folderFixName));

                    return fixFolders;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return false;
        }

        public static List<string> GetListServerFolderFix(string projectPath)
        {
            try
            {
                    var projectDirectoties = Directory.GetDirectories(projectPath).ToList();
                    var folderFixName = Options.PROJECT_FIX_PATH_NAME;
                    var fixFolders = projectDirectoties.Where(item => Path.GetFileName(item).Trim().ToLower().StartsWith(folderFixName.ToLower())).Select(item => Path.GetFullPath(item)).ToList();

                    return fixFolders;            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }

        public static string ServerGetFolderName(string path)
        {

            try
            {
           
                    if (!Directory.Exists(path))
                    {
                        return null;
                    }

                    return Path.GetFileName(path);
            
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }

        public static string AddFileLogProjectPath(string projectName, string projectPath)
        {
            try
            {
                var localProjectPath = Path.Combine(Properties.Settings.Default.ProjectLocalPath, projectName);

                CreateFolder(localProjectPath);

                var createPath = Path.Combine(localProjectPath, Options.PROJECT_PATH_FILE_NAME);

                if (!File.Exists(createPath))
                {
                    using (var file = File.Create(createPath))
                    {
                        File.SetAttributes(createPath, FileAttributes.Hidden);
                    }
                    File.AppendAllText(createPath, projectPath);
                }

                var createPathName = Path.Combine(localProjectPath, Options.PROJECT_FILE_NAME);
                if (!File.Exists(createPathName))
                {
                    using (var file = File.Create(createPathName))
                    {
                        File.SetAttributes(createPathName, FileAttributes.Hidden);
                    }
                    File.AppendAllText(createPathName, projectName);
                }

                return createPath;
            }
            catch (Exception e)
            {
                var errMessage = DateTime.Now.ToString() + " " + e.Message;
                WriteLog(errMessage);
                Console.WriteLine(e.Message);
            }

            return null;

        }

        public static string WriteLog(string content)
        {
            try
            {
                var localProjectPath = Properties.Settings.Default.ProjectLocalPath;
                var createPath = Path.Combine(localProjectPath, Options.PROJECT_LOG_NAME);

                CreateFolder(localProjectPath);

                if (!File.Exists(createPath))
                {
                    using (var file = File.Create(createPath))
                    {
                        File.SetAttributes(createPath, FileAttributes.Hidden);
                    }
                    File.AppendAllText(createPath, content + "\n");
                }
                else
                {
                    File.AppendAllText(createPath, content + "\n");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }

        public static string GetProjectPathByLog(string localProjectPath)
        {
            if (!Directory.Exists(localProjectPath))
            {
                return null;
            }

            DirectoryInfo projectDirInfo = new DirectoryInfo(localProjectPath);

            var logPath = projectDirInfo.GetFiles().Where(p => p.Name == Options.PROJECT_PATH_FILE_NAME).FirstOrDefault();

            if (logPath == null)
            {
                return null;
            }

            var projectPath = File.ReadLines(logPath.FullName).FirstOrDefault();

            return projectPath;
        }

        public static string GetProjectNameByLog(string localProjectPath)
        {
            if (!Directory.Exists(localProjectPath))
            {
                return null;
            }

            DirectoryInfo projectDirInfo = new DirectoryInfo(localProjectPath);

            var logProjectName = projectDirInfo.GetFiles().Where(p => p.Name == Options.PROJECT_FILE_NAME).FirstOrDefault();

            if (logProjectName == null)
            {
                return null;
            }

            var projectName = File.ReadLines(logProjectName.FullName).FirstOrDefault();

            return projectName;
        }

        public static void FolderSetAttributeNormal(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);

            di.Attributes = FileAttributes.Normal;

            var allFolders = di.GetDirectories();

            foreach (var folderItem in allFolders)
            {
                FolderSetAttributeNormal(folderItem.FullName);
            }
        }

        public static bool ExistsServer(string path)
        {
 
            try
            {
               
                    return Directory.Exists(path);
              
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public static int CountImageFolder(string path)
        {
            var listImage = GetListFileNameByFolder(path);
            var countImage = 0;

            if (listImage != null)
            {
                countImage = listImage.Count;
            }

            return countImage;
        }

        public static void RemoveFolder(string localPath, string serverpath)
        {

            try
            {
                DirectoryInfo directoryLocal = new DirectoryInfo(localPath);
                var directoriesLocal = directoryLocal.GetDirectories();

                if (directoriesLocal.Length <= 0)
                {
                    return;
                }
                foreach (var folderName in directoriesLocal)
                {

                    var newServerPath = Path.Combine(serverpath, folderName.Name);
                    if (!Directory.Exists(newServerPath))
                    {
                        Directory.Delete(folderName.FullName, true);
                    }

                    RemoveFolder(folderName.FullName, newServerPath);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static string GetBase64StringForImage(string imgPath)
        {
            byte[] imageBytes = System.IO.File.ReadAllBytes(imgPath);
            string base64String = Convert.ToBase64String(imageBytes);
            return base64String;
        }

        public static void Base64ToImage(string base64String, string path, string nameImage, string user)
        {
            try
            {
                // Convert Base64 String to byte[]
                string fullpath = path.Replace("Working\\" + user, "Frequency");
                if (!Directory.Exists(fullpath))
                {
                    Directory.CreateDirectory(fullpath);
                }
                string filePath = Path.Combine(fullpath, nameImage);
                File.WriteAllBytes(filePath, Convert.FromBase64String(base64String));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        // Start VPN

        //Check VPN
        public static bool IsVpnConnected()
        {
            try
            {

                NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

                foreach (NetworkInterface nic in networkInterfaces)
                {
                    if (nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType == NetworkInterfaceType.Ppp)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            return false;
        }

        //Editor processing => lấy danh sách có trong Do mà không có trong Done
        public static List<string> ListImagePriority(string projectPath, string userName)
        {

            try {
                var pathDo = Path.Combine(projectPath, Options.PROJECT_DO_NAME, userName);
                var pathDone = Path.Combine(projectPath, Options.PROJECT_DONE_NAME, userName);

                var allFileDo = FileHelpers.GetListFileNameAndFolder(pathDo);
                var allFileDone = FileHelpers.GetListFileNameAndFolder(pathDone);

                var folderDo = "\\" + Options.PROJECT_DO_NAME + "\\";
                var folderDone = "\\" + Options.PROJECT_DONE_NAME + "\\";

                var listFiles =  allFileDo.Where(fileDo =>
                     !allFileDone.Any(fileDone => fileDo == fileDone.Replace(folderDone, folderDo))).ToList();

                var listImage = new List<string>();

                foreach (var item in listFiles)
                {
                    try {
                        var startIndex = item.IndexOf("\\" + userName + "\\") + userName.Length + 2;
                        var endIndex = item.Length - 1;

                        var file = item.Substring(startIndex);

                        listImage.Add(file);
                    }
                    catch(Exception e) {
                    
                    }
                  
                }

                return listImage;

            } catch (Exception ex)
            {
                return new List<string> ();
            }
        }

        public static List<string> ListImageNotPriority(string projectPath, string userName, List<string> imagesPriority)
        {

            try {
                var pathDo = Path.Combine(projectPath, Options.PROJECT_DO_NAME, userName);
                var allFileDo = FileHelpers.GetListFileNameAndFolder(pathDo);

                var allImageDo = new List<string>();

                foreach (var item in allFileDo)
                {
                    try
                    {
                        var startIndex = item.IndexOf("\\" + userName + "\\") + userName.Length + 2;
                        var endIndex = item.Length - 1;

                        var file = item.Substring(startIndex);

                        allImageDo.Add(file);
                    }
                    catch (Exception e){}
                }

                if (!imagesPriority.Any())
                {
                    return allImageDo;
                }

                allImageDo = allImageDo.Where(fileDo =>
                       !imagesPriority.Any(image => fileDo == image)).ToList();

                return allImageDo;
            }
            catch(Exception ex) {
                return new List<string>();
            }

      
        }

        public static List<string> GetListFileNameAndFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                return null;
            }

            var files = Directory.GetFiles(path).Where(item => !File.GetAttributes(item).HasFlag(FileAttributes.Hidden)).Select(item => Path.GetFullPath(item)).ToList();

            var directories = Directory.GetDirectories(path);

            foreach (var dir in directories)
            {
                var fileInDir = GetListFileNameAndFolder(dir);
                files.AddRange(fileInDir);
            }

            return files;
        }


        public static async Task CopyImagePriority(List<string> listImage, string projectPath,string localPath, string userName)
        {

            string[] subdirectoryEntries = Directory.GetDirectories(projectPath);
            var listFolderFix = new List<string>();

            foreach (var director in subdirectoryEntries)
            {
                var dirInfo = new DirectoryInfo(director);
                if (dirInfo.Name.StartsWith(Options.PROJECT_FIX_PATH_NAME))
                {
                    listFolderFix.Add(dirInfo.Name);
                }
            }

            listFolderFix = listFolderFix.OrderByDescending(str => int.Parse(str.Replace(Options.PROJECT_FIX_PATH_NAME, ""))).ToList();

            foreach (var image in listImage)
            {
                var fileName  = Path.GetFileName(image);
                var subFolder = image.Replace(fileName, "");
               
                var pathDoServer = Path.Combine(projectPath, Options.PROJECT_DO_NAME, userName, subFolder);
                var pathDoLocal = Path.Combine(localPath, Options.PROJECT_DO_NAME, userName, subFolder);

                var pathDoneServer = Path.Combine(projectPath, Options.PROJECT_DONE_NAME, userName, subFolder);
                var pathDoneLocal = Path.Combine(localPath, Options.PROJECT_DONE_NAME, userName, subFolder);

                var pathWorking = Path.Combine(localPath, Options.PROJECT_WORKING_PATH_NAME, userName, subFolder);

                await GetFileAsync(pathDoServer, pathDoLocal, pathWorking, fileName);
                
                foreach (var folderFix in listFolderFix)
                {
                    var pathFixServer = Path.Combine(projectPath, folderFix, subFolder);
                    var pathFixLocal = Path.Combine(localPath, folderFix, subFolder);
                    if (!Directory.Exists(pathFixLocal))
                    {
                        Directory.CreateDirectory(pathFixLocal);
                    }
                    await GetFileAsync(pathFixServer, pathFixLocal, pathWorking, fileName);
                }

                if (!Directory.Exists(pathDoneLocal))
                {
                    Directory.CreateDirectory(pathDoneLocal);
                }

                await GetFileAsync(pathDoneServer, pathDoneLocal, pathWorking, fileName);
            }
        }
        public static async Task GetFileAsync(string formFolder, string toFolder,string workingFolder, string name)
        {
            try {
                name = Path.GetFileNameWithoutExtension(name);
                var files = Directory.GetFiles(formFolder).Where(item => !File.GetAttributes(item).HasFlag(FileAttributes.Hidden) && Path.GetFileNameWithoutExtension(item).Equals(name)).Select(item => Path.GetFileName(item)).ToList();

                foreach (var image in files)
                {
                    var fromPath = Path.Combine(formFolder, image);
                    var toPath = Path.Combine(toFolder, image);
                    var workingPath = Path.Combine(workingFolder, image);
                    await CopyFileAsync(fromPath, toPath);
                    await CopyFileAsync(toPath, workingPath);
                }
            }
            catch (Exception e) {
            }
        }
        public static async Task CopyFileAsync(string fromPath, string toPath)
        {
            try
            {
                FileAttributes attributes = File.GetAttributes(fromPath);

                if (attributes.HasFlag(FileAttributes.Hidden) || (File.Exists(fromPath) && File.Exists(toPath) && DateTime.Compare(File.GetLastWriteTime(fromPath), File.GetLastWriteTime(toPath)) <= 0))
                {
                    return;
                }

                var fromPathEx = Path.GetExtension(fromPath);
                if (fromPathEx.ToLower() == ".tmp")
                {
                    return;
                }

                var directoryName = new FileInfo(toPath).Directory.FullName;
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                 await Task.Run(() => File.Copy(fromPath, toPath, true));
                
                var getListWrite = File.GetLastWriteTime(fromPath);
                File.SetLastWriteTime(toPath, getListWrite);

            }
            catch (Exception e)
            {
                var errMessage = DateTime.Now.ToString() + " " + e.Message + " ---- " + fromPath;
                 WriteLog(errMessage);
            }
        }

        public static async Task<CountImageByProjectDto> CountImageByFolder(string path,int status,  string folderName)
        {
            var countImage = new CountImageByProjectDto();
            countImage.Status = status;
            countImage.Folder = folderName;

            var file = await CountImageByType(path);

            var contentImage = new List<CountImageContentDto>();

            var fileByType = file.GroupBy(item => Path.GetExtension(item).ToLower()).ToList();

            foreach (var item in fileByType)
            {
                var content = new CountImageContentDto();
                content.Type = item.Key;
                content.Number = item.Count();
                contentImage.Add(content);
            }

            countImage.Content = contentImage;

            return countImage;
        }

        public static async Task<List<string>> CountImageByType(string path)
        {
            var file = new List<string>();
            try { 
                 file = Directory.GetFiles(path).Where(item => !File.GetAttributes(item).HasFlag(FileAttributes.Hidden)).ToList();

                var directory = Directory.GetDirectories(path);

                foreach (var dir in directory)
                {
                    var name = new DirectoryInfo(dir).Name;
                    var newPath = Path.Combine(path, name);
                    var newFile = await CountImageByType(newPath);
                    file.AddRange(newFile);
                }
            }
            catch (Exception e) { }
           
            return file;
        }


    }
}
