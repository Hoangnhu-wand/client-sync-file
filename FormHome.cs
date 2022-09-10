using System;
using System.Net;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.AspNetCore.SignalR.Client;
using WandSyncFile.Data.Mapping;
using WandSyncFile.Helpers;
using WandSyncFile.Service;
using System.Threading;
using System.Drawing;
using static WandSyncFile.Constants.Values;
using WandSyncFile.CustomControls;
using Microsoft.Win32;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using WandSyncFile.Data.Mapping;
using WandSyncFile.Constants;
using System.Net.Http;

namespace WandSyncFile
{
    public partial class FormHome : Form
    {
        HubConnection connection;
        ProjectService projectService;
        CancellationToken cancellationToken;
        CancellationToken cancellationTokenRemoveProject;
        DisplayFolder displayFolder;
        public List<int> processingDoProject = new List<int>();
        public List<int> processingDoneProject = new List<int>();
        public List<int> processingFixProject = new List<int>();
        public bool _mouseDown;
        public Point _lastLocation;

        public FormHome()
        {
            InitializeComponent();
            setupAutoRun();

           /* HttpClientHelper.PostAsync("http://172.16.0.20:6696/api/v1/guidance", "");
*/
            displayFolder = new DisplayFolder();
            projectService = new ProjectService();
            cancellationToken = new CancellationToken();
            cancellationTokenRemoveProject = new CancellationToken();

            HandleHubConnection();
            DisplayAccountProfile();
            ReadFileChange(cancellationToken);
            RemoveCompletedProject(cancellationTokenRemoveProject);
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
                }
                catch (Exception e)
                {
                    Properties.Settings.Default.Reset();
                    return false;
                }
            }

            return false;
        }

        public void setupAutoRun()
        {
            RegistryKey reg = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (reg.GetValue("WandSyncFile") == null)
            {
                reg.SetValue("WandSyncFile", Application.ExecutablePath.ToString());
            }
        }

        public void addItem(DateTime created, string action,string count, string projectName, int status)
        {
            try
            {
                var listItem = new CustomListView();
                listItem.CreatedDate = created.ToString("dd/M/yyyy");
                listItem.CreateTime = created.ToString("hh:mm:ss");
                listItem.ProjectName = projectName;
                listItem.Count = count;
                listItem.ButtonText = action;

                var buttonColor = Color.FromArgb(178, 255, 212);


                switch (action)
                {
                    case "Download Do":
                    case "Do => Working":
                        buttonColor = Color.FromArgb(194, 245, 233);
                        break;
                    case "Download Done":
                    case "Upload Done":
                    case "Done => Working":
                        buttonColor = Color.FromArgb(178, 255, 212);
                        break;
                    case "Download Fix":
                    case "Download Fix_1":
                    case "Download Fix_2":
                    case "Download Fix_3":
                    case "Download Fix_4":
                    case "Download Fix_5":
                    case "Fix_1 => Working":
                    case "Fix_2 => Working":
                    case "Fix_3 => Working":
                    case "Fix_4 => Working":
                    case "Fix_5 => Working":
                    case "Upload Fix":
                        buttonColor = Color.FromArgb(255, 218, 246);
                        break;
                    case "Completed":
                        buttonColor = Color.FromArgb(255, 219, 150);
                        break;
                    case "Get Sample":
                        buttonColor = Color.FromArgb(207, 223, 255);
                        break;
                    default:
                        buttonColor = Color.FromArgb(178, 255, 212);
                        break;
                }

                listItem.ButtonColor = buttonColor;

                listItem.StatusColor = Color.Red;

                // processing
                if (status == 0)
                {
                    listItem.StatusColor = Color.FromArgb(255, 219, 150);
                }

                // done
                if (status == 1)
                {
                    listItem.StatusColor = Color.FromArgb(178, 255, 212);
                }

                // fail
                if (status == 2)
                {
                    listItem.StatusColor = Color.FromArgb(255, 173, 173);
                }

                listItem.Width = flowLayoutPanel.Width;
                flowLayoutPanel.Controls.Add(listItem);
                flowLayoutPanel.Controls.SetChildIndex(listItem, 1);
            }
            catch (Exception e)
            {
                try {
                    for (int i = flowLayoutPanel.Controls.Count - 1; i >= 0; --i)
                    {
                        var ctl = flowLayoutPanel.Controls[i];
                        ctl.Dispose();
                    }
                    Invoke((Action)(() =>
                    {
                        addItem(DateTime.Now, "Clear!", true);
                    }));
                }
                catch(Exception) {
                    Console.WriteLine(e.Message);
                }  
            }
        }

        public void addItem(DateTime created, string action, bool isConnected)
        {
            try
            {
                var listItem = new CustomViewConnectHrm1();
                listItem.CreatedDate = created.ToString("dd/M/yyyy");
                listItem.CreateTime = created.ToString("hh:mm:ss");
                listItem.Action = action;
                listItem.Width = flowLayoutPanel.Width;
                if (isConnected)
                {
                    listItem.BackColor = Color.FromArgb(255, 82, 56);
                }
                else
                {
                    listItem.BackColor = Color.FromArgb(44, 44, 46);
                }

                flowLayoutPanel.Controls.Add(listItem);
                flowLayoutPanel.Controls.SetChildIndex(listItem, 0);
            }
            catch (Exception e)
            {
                try
                {
                    for (int i = flowLayoutPanel.Controls.Count - 1; i >= 0; --i)
                    {
                        var ctl = flowLayoutPanel.Controls[i];
                        ctl.Dispose();
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public void ReadFileChange(CancellationToken cancellationToken)
        {
            Task.Factory.StartNew(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    ReadAllFileChange();
                    await Task.Delay(TimeSpan.FromSeconds(Options.TIME_SPAN_READ_FILE_CHANGE), cancellationToken);
                }
            });
        }

        public void RemoveCompletedProject(CancellationToken cancellationToken)
        {
            Task.Factory.StartNew(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    RemoveCompletedProjectFolder();
                    await Task.Delay(TimeSpan.FromSeconds(Options.TIME_SPAN_REMOVE_COMPLETED_PROJECT));
                }
            });
        }

        public void RemoveCompletedProjectFolder()
        {
            var displayFolder = new DisplayFolder();
            if (!UserRoleHelpers.IsEditors())
            {
                return;
            }

            // get all folder in project path
            var projectLocalPath = Properties.Settings.Default.ProjectLocalPath;
            var editorUserName = Properties.Settings.Default.Username;

            if (!Directory.Exists(projectLocalPath))
            {
                return;
            }

            try
            {
                DirectoryInfo info = new DirectoryInfo(projectLocalPath);
                var directories = info.GetDirectories().OrderBy(p => p.LastWriteTime).ToArray();

                foreach (var projectDir in directories)
                {
                    DirectoryInfo projectDirInfo = new DirectoryInfo(projectDir.FullName);
                    var logPath = projectDirInfo.GetFiles().Where(p => p.Name == Options.PROJECT_PATH_FILE_NAME).FirstOrDefault();
                    var logProjectName = projectDirInfo.GetFiles().Where(p => p.Name == Options.PROJECT_FILE_NAME).FirstOrDefault();

                    if (logPath == null)
                    {
                        continue;
                    }

                    var projectPath = File.ReadLines(logPath.FullName).FirstOrDefault();
                    var projectName = File.ReadLines(logProjectName.FullName).FirstOrDefault();

                    var project = projectService.RequestGetProjectByName(projectName);

                    if (project != null &&  project.StatusId == (int)PROJECT_STATUS.COMPLETED)
                    {
                        var localProject = Path.Combine(projectLocalPath, projectName);
                        if(Directory.Exists(localProject))
                        {
                            FileHelpers.FolderSetAttributeNormal(localProject);

                            Directory.Delete(localProject, true);
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        public void ReadAllFileChange()
        {
            var displayFolder = new DisplayFolder();
            if (!UserRoleHelpers.IsEditors())
            {
                return;
            }

            // get all folder in project path
            var projectLocalPath = Properties.Settings.Default.ProjectLocalPath;
            var editorUserName = Properties.Settings.Default.Username;
    

            if (!Directory.Exists(projectLocalPath))
            {
                return;
            }

            try
            {
                DirectoryInfo info = new DirectoryInfo(projectLocalPath);     
                var directories = info.GetDirectories().OrderBy(p => p.LastWriteTime).ToArray();          
                        foreach (var projectDir in directories)
                        {
                            DirectoryInfo projectDirInfo = new DirectoryInfo(projectDir.FullName);
                            var logPath = projectDirInfo.GetFiles().Where(p => p.Name == Options.PROJECT_PATH_FILE_NAME).FirstOrDefault();
                            var logProjectName = projectDirInfo.GetFiles().Where(p => p.Name == Options.PROJECT_FILE_NAME).FirstOrDefault();

                            if (logPath == null)
                            {
                                continue;
                            }

                            var projectPath = File.ReadLines(logPath.FullName).FirstOrDefault();
                            var projectName = File.ReadLines(logProjectName.FullName).FirstOrDefault();

                            SyncDo(projectName, projectPath);

                            SyncDone(projectName, projectPath);

                            SyncFix(projectName, projectPath);
                        }
            }
            catch (Exception e)
            {
                Invoke((Action)(async () =>
                {
                    addItem(DateTime.Now, "Error", null,"System not working", 2);
                }));
            }
        }

        public void SyncFix(string projectName, string projectPath)
        {
            var editorUserName = Properties.Settings.Default.Username;
            var localPath = Properties.Settings.Default.ProjectLocalPath;

            var projectLocalPath = Path.Combine(localPath, projectName);

            var projectDirectoties = Directory.GetDirectories(projectLocalPath).ToList();
            var lastFixFolderLocalPath = projectDirectoties.Where(item => Path.GetFileName(item).Trim().StartsWith(Options.PROJECT_FIX_PATH_NAME)).OrderByDescending(item => Path.GetFileName(item)).FirstOrDefault();

            if (lastFixFolderLocalPath == null)
            {
                return;
            }

            var lastFixFolderName = Path.GetFileName(lastFixFolderLocalPath);
            var lastFixFolderServerPath = Path.Combine(projectPath, lastFixFolderName);

            var folderDoneServer = Path.Combine(projectPath, "Done", editorUserName);

            var isSyncFix = displayFolder.CheckFolderFixSync(lastFixFolderLocalPath, lastFixFolderServerPath,  folderDoneServer, lastFixFolderLocalPath);
            if (isSyncFix)
            {
                Invoke((Action)(async () =>
                {
                    addItem(DateTime.Now, "No Change Fix", null, projectName, 1);
                }));
                return;
            }

            var project = projectService.RequestGetProjectByName(projectName);
            if (project == null || (project != null && (project.StatusId == (int)PROJECT_STATUS.CHECKED || project.StatusId == (int)PROJECT_STATUS.COMPLETED)))
            {
                Invoke((Action)(async () =>
                {
                    addItem(DateTime.Now, "Project No Change", null, projectName, 1);
                }));
                return;
            }

            if (!isSyncFix && !processingFixProject.Any(pId => pId == project.Id))
            {
                displayFolder.CheckFolderSync(lastFixFolderLocalPath, lastFixFolderServerPath, lastFixFolderLocalPath);

                //đếm số ảnh fix trước khi sync
                var imageFixLocalFirst = FileHelpers.CountImageFolder(lastFixFolderLocalPath);
                var imageFixServerFirst = FileHelpers.CountImageFolder(lastFixFolderServerPath);
                var countFirts = imageFixLocalFirst + "/" + imageFixServerFirst;
                Invoke((Action)(async () =>
                {
                    addItem(DateTime.Now, "Upload Fix" , countFirts, projectName, 0);
                }));

                processingFixProject.Add(project.Id);

                try {
                    FileHelpers.CopyDirectoryToServer(lastFixFolderLocalPath, lastFixFolderServerPath);

                    //đếm số ảnh fix sau khi sync
                    var imageFixLocalLast = FileHelpers.CountImageFolder(lastFixFolderLocalPath);
                    var imageFixServerLast = FileHelpers.CountImageFolder(lastFixFolderServerPath);
                    var countLast = imageFixLocalLast + "/" + imageFixServerLast;
                    Invoke((Action)(async () =>
                    {
                        addItem(DateTime.Now, "Upload Fix" , countLast, projectName, 1);
                    }));
                }
                catch(Exception e) {
                    Invoke((Action)(async () =>
                    {
                        addItem(DateTime.Now, "Upload Fix",null, projectName, 2);
                    }));
                }
                finally {
                    displayFolder.CheckFolderSync(lastFixFolderLocalPath, lastFixFolderServerPath, lastFixFolderLocalPath);

                    processingFixProject.Remove(project.Id);
                }
            }
        }

        public void SyncDo(string projectName, string projectPath)
        {
            var editorUserName = Properties.Settings.Default.Username;
            var localPath = Properties.Settings.Default.ProjectLocalPath;

            var projectLocalPath = Path.Combine(localPath, projectName);
            var projectDoLocalPath = Path.Combine(projectLocalPath, Options.PROJECT_DO_NAME);
            var projectDoEditorLocalPath = Path.Combine(projectDoLocalPath, editorUserName);

            var projectDoServerPath = Path.Combine(projectPath, Options.PROJECT_DO_NAME);
            var projectDoEditorServerPath = Path.Combine(projectDoServerPath, editorUserName);

            var isSyncDo = displayFolder.CheckFolderSyncCompleted(projectDoEditorLocalPath, projectDoEditorServerPath);

            if (isSyncDo)
            {
                Invoke((Action)(async () =>
                {
                    addItem(DateTime.Now, "No Change Do",null, projectName, 1);
                }));
                return;
            }

            var project = projectService.RequestGetProjectByName(projectName);

            if (project == null || (project != null && (project.StatusId == (int)PROJECT_STATUS.CHECKED || project.StatusId == (int)PROJECT_STATUS.COMPLETED)))
            {
                Invoke((Action)(async () =>
                {
                    addItem(DateTime.Now, "Project No Change", null, projectName, 1);
                }));
                return;
            }

            if (!isSyncDo && !processingDoProject.Any(pId => pId == project.Id))
            {
                displayFolder.CheckFolderSync(projectDoEditorLocalPath, projectDoEditorServerPath, projectDoLocalPath);

                var imageDoLocalFirst = FileHelpers.CountImageFolder(projectDoEditorLocalPath);
                var imageDoServerFirst = FileHelpers.CountImageFolder(projectDoEditorServerPath);
                var counDotFirst = imageDoLocalFirst + "/" + imageDoServerFirst;
                Invoke((Action)(async () =>
                {
                    addItem(DateTime.Now, "Download Do" , counDotFirst, projectName, 0);
                }));

                processingDoProject.Add(project.Id);

                try {
                    FileHelpers.DownloadFolderFromServer(projectDoEditorServerPath, projectDoEditorLocalPath, null, true);
                    FileHelpers.RemoveFolder(projectDoEditorLocalPath, projectDoEditorServerPath);
                    //đếm số ảnh do sau khi sync
                    var imageDoLocalLast = FileHelpers.CountImageFolder(projectDoEditorLocalPath);
                    var imageDoServerLast = FileHelpers.CountImageFolder(projectDoEditorServerPath);
                    var countDoLast = imageDoLocalLast + "/" + imageDoServerLast;
                    Invoke((Action)(async () =>
                    {
                        addItem(DateTime.Now, "Download Do" , countDoLast, projectName, 1);
                    }));
                    // Copy Do => Working
                    if (project.StatusId != (int)PROJECT_STATUS.NEEDFIX)
                    {
                        var localFolderWorking = Path.Combine(projectLocalPath, Options.PROJECT_WORKING_PATH_NAME);
                        var editorFolderWorking = Path.Combine(localFolderWorking, editorUserName);

                        displayFolder.CheckFolderSync(editorFolderWorking, projectDoEditorLocalPath, editorFolderWorking);

               
                        var imageWorkingServerFirst = FileHelpers.CountImageFolder(editorFolderWorking);
                        var countDoToWorkingFirst = imageDoLocalLast + "/" + imageWorkingServerFirst;
                        Invoke((Action)(async () =>
                        {
                            addItem(DateTime.Now, "Do => Working" , countDoToWorkingFirst, projectName, 0);
                        }));

                        FileHelpers.DownloadFolder(projectDoEditorLocalPath, editorFolderWorking);
                       /* FileHelpers.RemoveFolder(editorFolderWorking, projectDoEditorLocalPath);*/
                        var imageWorkingServerLast = FileHelpers.CountImageFolder(editorFolderWorking);
                        var countDoToWorkingLast = imageDoLocalLast + "/" + imageWorkingServerLast;
                        Invoke((Action)(async () =>
                        {
                            addItem(DateTime.Now, "Do => Working", countDoToWorkingLast, projectName, 1);
                        }));

                        displayFolder.CheckFolderSync(editorFolderWorking, projectDoEditorLocalPath, editorFolderWorking);
                    }
                }
                catch (Exception e)
                {
                    Invoke((Action)(async () =>
                    {
                        addItem(DateTime.Now, "Download Do",null, projectName, 2);
                    }));
                }
                finally
                {
                    displayFolder.CheckFolderSync(projectDoEditorLocalPath, projectDoEditorServerPath, projectDoLocalPath);

                    processingDoProject.Remove(project.Id);
                }
            }
        }

        public void SyncDone(string projectName, string projectPath)
        {
            var editorUserName = Properties.Settings.Default.Username;
            var localPath = Properties.Settings.Default.ProjectLocalPath;

            var projectLocalPath = Path.Combine(localPath, projectName);
            var projectDoneLocalPath = Path.Combine(projectLocalPath, Options.PROJECT_DONE_NAME);
            var projectDoneEditorLocalPath = Path.Combine(projectDoneLocalPath, editorUserName);

            var projectDoneServerPath = Path.Combine(projectPath, Options.PROJECT_DONE_NAME);
            var projectDoneEditorServerPath = Path.Combine(projectDoneServerPath, editorUserName);

            var isSyncDone = displayFolder.CheckFolderSyncCompleted(projectDoneEditorLocalPath, projectDoneEditorServerPath);

            if (isSyncDone)
            {
                Invoke((Action)(async () =>
                {
                    addItem(DateTime.Now, "No Change Done",null, projectName, 1);
                }));
                return;
            }

            var project = projectService.RequestGetProjectByName(projectName);
            if (project == null || (project != null && (project.StatusId == (int)PROJECT_STATUS.CHECKED || project.StatusId == (int)PROJECT_STATUS.COMPLETED)))
            {
                Invoke((Action)(async () =>
                {
                    addItem(DateTime.Now, "Project No Change",null, projectName, 1);
                }));
                return;
            }

            if (!isSyncDone && !processingDoneProject.Any(pId => pId == project.Id))
            {
                displayFolder.CheckFolderSync(projectDoneEditorLocalPath, projectDoneEditorServerPath, projectDoneLocalPath);

                //đếm số ảnh Done trước khi sync
                var imageDoneLocalFirst = FileHelpers.CountImageFolder(projectDoneEditorLocalPath);
                var imageDoneServerFirst = FileHelpers.CountImageFolder(projectDoneEditorServerPath);
                var countDoneFirst = imageDoneLocalFirst + "/" + imageDoneServerFirst;
                Invoke((Action)(async () =>
                {
                    addItem(DateTime.Now, "Upload Done", countDoneFirst, projectName, 0);
                }));

                processingDoneProject.Add(project.Id);

                try {
                    FileHelpers.SyncDirectoryDoneToServer(projectDoneEditorLocalPath, projectDoneEditorServerPath);

                    //đếm số ảnh Done sau khi sync
                    var imageDoneLocalLast = FileHelpers.CountImageFolder(projectDoneEditorLocalPath);
                    var imageDoneServerLast = FileHelpers.CountImageFolder(projectDoneEditorServerPath);
                    var countDoneLast = imageDoneLocalLast + "/" + imageDoneServerLast;
                    Invoke((Action)(async () =>
                    {
                        addItem(DateTime.Now, "Upload Done", countDoneLast, projectName, 1);
                    }));
                }
                catch (Exception e ) {
                    Invoke((Action)(async () =>
                    {
                        addItem(DateTime.Now, "Upload Done",null, projectName, 2);
                    }));
                }
                finally {
                    displayFolder.CheckFolderSync(projectDoneEditorLocalPath, projectDoneEditorServerPath, projectDoneLocalPath);

                    processingDoneProject.Remove(project.Id);
                }
            }
        }

        public void CopyDoneAndFixFromServer(string projectName, string projectPath, int projectId)
        {
            var editorUserName = Properties.Settings.Default.Username; // EditorName
            var localPath = Properties.Settings.Default.ProjectLocalPath; //LocalPath

            var projectLocalPath = Path.Combine(localPath, projectName); // LocalPath\\ProjectName

            if (!Directory.Exists(projectLocalPath))
            {
                Directory.CreateDirectory(projectLocalPath);
            }

            var isFolderProjectLocalPathHasFile = FileHelpers.HasFileInFolder(projectLocalPath);
            if (isFolderProjectLocalPathHasFile)
            {
                return;
            }

            // Download Do 
            var projectDoLocalPath = Path.Combine(projectLocalPath, Options.PROJECT_DO_NAME); // LocalPath\\ProjectName\\Do
            var projectDoEditorLocalPath = Path.Combine(projectDoLocalPath, editorUserName); // LocalPath\\ProjectName\\Do\\EditorName

            var projectDoServerPath = Path.Combine(projectPath, Options.PROJECT_DO_NAME); // ServerPath\\James\\Do
            var projectDoEditorServerPath = Path.Combine(projectDoServerPath, editorUserName); // ServerPath\\James\\Do\\EditorName

            //đếm số ảnh Do trước khi tải xuống
              
            var imageDoLocalFirst = FileHelpers.CountImageFolder(projectDoEditorLocalPath);
            var imageDoServerFirst = FileHelpers.CountImageFolder(projectDoEditorServerPath);
            var countDoFirst = imageDoLocalFirst + "/" + imageDoServerFirst;

            Invoke((Action)(async () =>
            {
                addItem(DateTime.Now, "Download Do" , countDoFirst, projectName, 0);
            }));

            processingDoProject.Add(projectId);

            FileHelpers.DownloadFolderFromServer(projectDoEditorServerPath, projectDoEditorLocalPath);
            FileHelpers.RemoveFolder(projectDoEditorLocalPath, projectDoEditorServerPath);
            //Đếm số ảnh Do sau khi tải xuống
            var imageDoLocalLast = FileHelpers.CountImageFolder(projectDoEditorLocalPath);
            var imageDoServerLast = FileHelpers.CountImageFolder(projectDoEditorServerPath);
            var countDoLast = imageDoLocalLast + "/" + imageDoServerLast;

            Invoke((Action)(async () =>
            {
                addItem(DateTime.Now, "Download Do" , countDoLast, projectName, 1);
            }));

            processingDoProject.Remove(projectId);

            // Download Done
            var projectDoneLocalPath = Path.Combine(projectLocalPath, Options.PROJECT_DONE_NAME); // LocalPath\\ProjectName\\Done
            var projectDoneEditorLocalPath = Path.Combine(projectDoneLocalPath, editorUserName); // LocalPath\\ProjectName\\Done\\EditorName

            var projectDoneServerPath = Path.Combine(projectPath, Options.PROJECT_DONE_NAME); // ServerPath\\James\\Done
            var projectDoneEditorServerPath = Path.Combine(projectDoneServerPath, editorUserName); // ServerPath\\James\\Done\\EditorName

            FileHelpers.CreateFolder(projectDoneEditorLocalPath);
            var folderDoneHasFile = FileHelpers.HasFileInFolder(projectDoneEditorLocalPath);
            if (folderDoneHasFile)
            {
                return;
            }
            //Đếm số ảnh Done trước khi tải xuống
            var imageDoneLocalFirst = FileHelpers.CountImageFolder(projectDoneEditorLocalPath);
            var imageDoneServerFirst = FileHelpers.CountImageFolder(projectDoneEditorServerPath);
            var countDoneFirst = imageDoneLocalFirst + "/" + imageDoneServerFirst;

            Invoke((Action)(async () =>
            {
                addItem(DateTime.Now, "Download Done" , countDoneFirst, projectName, 0);
            }));

            processingDoneProject.Add(projectId);

            FileHelpers.DownloadFolderFromServer(projectDoneEditorServerPath, projectDoneEditorLocalPath);

            //Đếm số ảnh Done sau khi tải xuống
            var imageDoneLocalLast = FileHelpers.CountImageFolder(projectDoneEditorLocalPath);
            var imageDoneServerLast = FileHelpers.CountImageFolder(projectDoneEditorServerPath);
            var countDoneLast = imageDoneLocalLast + "/" + imageDoneServerLast;

            Invoke((Action)(async () =>
            {
                addItem(DateTime.Now, "Download Done", countDoneLast, projectName, 1);
            }));

            processingDoneProject.Remove(projectId);

            // Copy Done -> Working
            var projectWorkingLocalPath = Path.Combine(projectLocalPath, Options.PROJECT_WORKING_PATH_NAME); // LocalPath\\ProjectName\\Working
            var projectWorkingEditorLocalPath = Path.Combine(projectWorkingLocalPath, editorUserName); // LocalPath\\ProjectName\\Working\\EditorName

            FileHelpers.CreateFolder(projectWorkingEditorLocalPath);

            var checkWorking = displayFolder.CheckFolderSync(projectWorkingEditorLocalPath, projectDoneEditorLocalPath, projectWorkingEditorLocalPath);
            if (!checkWorking)
            {

                var imageWorkingLocalFirst = FileHelpers.CountImageFolder(projectWorkingEditorLocalPath);
                var countDoneToWorkingFirst = imageDoneLocalLast + "/" + imageWorkingLocalFirst;

                Invoke((Action)(async () =>
                {
                    addItem(DateTime.Now, "Done => Working", countDoneToWorkingFirst, projectName, 0);
                }));

                FileHelpers.DownloadFolder(projectDoneEditorLocalPath, projectWorkingEditorLocalPath);

                var imageWorkingLocalLast = FileHelpers.CountImageFolder(projectWorkingEditorLocalPath);
                var countDoneToWorkingLast = imageDoneLocalLast + "/" + imageWorkingLocalLast;
                Invoke((Action)(async () =>
                {
                    addItem(DateTime.Now, "Done => Working" , countDoneToWorkingLast, projectName, 1);
                }));
            }

            // Download Fix
            var allFolderFix = FileHelpers.GetListServerFolderFix(projectPath);
            if (allFolderFix == null)
            {
                return;
            }

            Invoke((Action)(async () =>
            {
                addItem(DateTime.Now, "Download Fix",null, projectName, 0);
            }));

            foreach (var folderFixItem in allFolderFix)
            {
                var folderFixName = FileHelpers.ServerGetFolderName(folderFixItem); // fix_1
                var localEditorFixPath = Path.Combine(projectLocalPath, folderFixName); // LocalPath\\ProjectName\\fix_1


                var imageFixLocalFirst = FileHelpers.CountImageFolder(localEditorFixPath);
                var imageFixServerFirst = FileHelpers.CountImageFolder(folderFixItem);
                var countFixFirst = imageFixLocalFirst + "/" + imageFixServerFirst;
                Invoke((Action)(async () =>
                {
                    addItem(DateTime.Now, "Download " + folderFixName, countFixFirst, projectName, 0);
                }));

                FileHelpers.CreateFolder(localEditorFixPath);

                // chỉ lấy các file fix có trong Done
                var allFileDoneName = FileHelpers.GetListFileNameByFolder(projectDoneEditorServerPath);
                var allFixByDoneName = FileHelpers.ServerGetListFixPathByDoneName(allFileDoneName, folderFixItem);

                foreach (var fixItem in allFixByDoneName)
                {
                    var nextFolderAfterFolderFix = fixItem.Replace(folderFixItem + "\\", "");
                    var clientPath = Path.Combine(localEditorFixPath, nextFolderAfterFolderFix); // LocalPath\\ProjectName\\fix_1\\Year 12 Formal (Photographer James)--4.jpg
                    FileHelpers.CopyFileFromServer(fixItem, clientPath);
                }

                var imageFixLocalLast = FileHelpers.CountImageFolder(localEditorFixPath);
                var imageFixServerLast = FileHelpers.CountImageFolder(folderFixItem);
                var countFixLast = imageFixLocalLast + "/" + imageFixLocalLast;
                Invoke((Action)(async () =>
                {
                    addItem(DateTime.Now, "Download " + folderFixName, countFixLast, projectName, 1);
                }));

                // Copy Fix -> Working


                var imageWorkingLocalFirst = FileHelpers.CountImageFolder(projectWorkingEditorLocalPath);
                var countFixToWorkingFirst = imageFixLocalLast + "/" + imageWorkingLocalFirst;

                Invoke((Action)(async () =>
                {
                    addItem(DateTime.Now, folderFixName + " => Working", countFixToWorkingFirst, projectName, 0);
                }));

                FileHelpers.DownloadFolder(localEditorFixPath, projectWorkingEditorLocalPath, null, false, true);

                var imageWorkingLocalLast = FileHelpers.CountImageFolder(projectWorkingEditorLocalPath);
                var countFixToWorkingLast = imageFixLocalLast + "/" + imageWorkingLocalLast;


                Invoke((Action)(async () =>
                {
                    addItem(DateTime.Now, folderFixName + " => Working" , countFixToWorkingLast, projectName, 1);
                }));
            }

            Invoke((Action)(async () =>
            {
                addItem(DateTime.Now, "Download Fix",null, projectName, 1);
            }));
        }

        public async void HandleHubConnection()
        {
            string connectedServer = DateTime.Now.ToString();

            var userId = Properties.Settings.Default.Id;
            var userName = Properties.Settings.Default.Username;
            var localPath = Properties.Settings.Default.ProjectLocalPath;

            var reconnectSeconds = new List<TimeSpan> { TimeSpan.Zero, TimeSpan.Zero, TimeSpan.FromSeconds(5) };

            var i = 5;
            while (i <= 7200)
            {
                reconnectSeconds.Add(TimeSpan.FromSeconds(i));
                i++;
            }

            connection = new HubConnectionBuilder()
               .WithUrl($"{Url.ServerURI}/appHub")
               .WithAutomaticReconnect(reconnectSeconds.ToArray())
               .Build();
            try
            {
                await connection.StartAsync();
                Invoke((Action)(() =>
                {
                    addItem(DateTime.Now, "Connected!", true);
                }));

                if (!Directory.Exists(localPath))
                {
                    Invoke((Action)(() =>
                    {
                        addItem(DateTime.Now, "Error - Folder does not exist: " + localPath, false);
                    }));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Invoke((Action)(() =>
                {
                    addItem(DateTime.Now, "Disconnect!", false);
                }));
            }

            connection.Reconnecting += connectionId =>
            {
                Invoke((Action)(() =>
                {
                    addItem(DateTime.Now, "Reconnecting....", false);
                }));
                return Task.CompletedTask;
            };

            connection.Reconnected += connectionId =>
            {
                Invoke((Action)(() =>
                {
                    addItem(DateTime.Now, "Connected!", true);
                }));

                return Task.CompletedTask;
            };


            connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };

            

            connection.On<string, string, string>("SERVER_QUEUE_MESSAGE", async (user, action, data) =>
            {
                if (action == "EDITOR_DOWNLOAD_FILE" && user == userId.ToString() && UserRoleHelpers.IsEditors())
                {
                    Task.Run(async () =>
                    {
                        var editorDownloadItem = JsonConvert.DeserializeObject<EditorDownloadFileProjectDto>(data);

                        if (editorDownloadItem == null || !FileHelpers.ExistsPathServer(editorDownloadItem.ProjectPath))
                        {
                            return;
                        }
                        
                        var projectPath = editorDownloadItem.ProjectPath.Trim(); // ServerPath\\ProjectName"
                        var projectName = editorDownloadItem.ProjectName.Trim(); // ProjectName

                        var projectLocalPath = FileHelpers.GetProjectLocalPath(projectName); // LocalPath/ProjectName
                        var projectDoLocalPath = Path.Combine(projectLocalPath, Options.PROJECT_DO_NAME); // LocalPath/ProjectName/Do
                        var projectDoEditorLocalPath = FileHelpers.GetProjectDoEditorLocalPath(projectName); // LocalPath/ProjectName/Do/EditorName

                        FileHelpers.CreateFolder(projectDoLocalPath);
                        FileHelpers.CreateFolder(projectDoEditorLocalPath);

                        FileHelpers.AddFileLogProjectPath(projectName, projectPath);

                        // Tạo thư mục Done
                        var projectDoneEditorLocalPath = FileHelpers.GetProjectDoneEditorLocalPath(projectName);
                        FileHelpers.CreateFolder(projectDoneEditorLocalPath);

                        // check sync do path
                        SyncDo(projectName, projectPath);

                        var sampleLocalPath = FileHelpers.GetProjectSampleLocalPath(projectName);
                        var sampleServerPath = Path.Combine(projectPath, Options.PROJECT_SAMPLE_NAME);

                        if (FileHelpers.ExistsServer(sampleServerPath))
                        {
                            var isSyncSample = displayFolder.CheckFolderSync(sampleLocalPath, sampleServerPath);

                            if (!isSyncSample)
                            {
                                Invoke((Action)(() =>
                                {
                                    addItem(DateTime.Now, "Get Sample", null, projectName, 0);
                                }));

                                FileHelpers.DownloadFolderFromServer(sampleServerPath, sampleLocalPath, null, true, true);

                                Invoke((Action)(() =>
                                {
                                    addItem(DateTime.Now, "Get Sample", null, projectName, 1);
                                }));
                            }
                        }

                        // create folder Done by server Done
                        FileHelpers.EditorCreateFolderDonePath(projectPath, projectName);

                        displayFolder.CheckFolderSync(sampleLocalPath, sampleServerPath);

                        await connection.SendAsync("ReceiverMessageAsync", "CLIENT_FILE", editorDownloadItem.MessageId, "REMOVE_PROJECT_QUEUE_MESSAGE", null);
                    });
                }

                if (action == "EDITOR_CREATE_FOLDER_FIX" && UserRoleHelpers.IsEditors() && user == userId.ToString())
                {
                    Task.Run(async () =>
                    {

                        var editorDownloadItem = JsonConvert.DeserializeObject<EditorDownloadFileProjectDto>(data); // get QUEUE
                        if (editorDownloadItem == null)
                        {
                            return;
                        }

                        var folderFix = editorDownloadItem.FilePath; // ServerPath\\James\\Fix_3
                        var serverFileArr = Path.GetFullPath(folderFix).Split(new string[] { Path.GetFullPath(editorDownloadItem.ProjectPath) }, StringSplitOptions.None).Where(item => !string.IsNullOrEmpty(item)).ToList();
                        if (serverFileArr.Count() <= 0)
                        {
                            return;
                        }

                        var localFolderFix = FileHelpers.GetProjectLocalPath(editorDownloadItem.ProjectName) + serverFileArr.Last(); //LocalPath\\ProjectName\\Fix_3
                        FileHelpers.CreateFolder(localFolderFix);

                        FileHelpers.AddFileLogProjectPath(editorDownloadItem.ProjectName, editorDownloadItem.ProjectPath);

                        CopyDoneAndFixFromServer(editorDownloadItem.ProjectName, editorDownloadItem.ProjectPath, editorDownloadItem.ProjectId);

                        await connection.SendAsync("ReceiverMessageAsync", "CLIENT_FILE", editorDownloadItem.MessageId, "REMOVE_PROJECT_QUEUE_MESSAGE", null);
                    });
                }
            });

            connection.On<string, string, string>("WAND_ADDON_MESSAGE", async (user, action, localProjectName) =>
            {
                if (action == "UPLOAD_DONE" && UserRoleHelpers.IsEditors() && user == userName)
                {
                    Task.Run(() =>
                    {
                        var localProjectPath = Path.Combine(localPath, localProjectName);

                        var projectPath = FileHelpers.GetProjectPathByLog(localProjectPath);
                        var projectName = FileHelpers.GetProjectNameByLog(localProjectPath);

                        if (string.IsNullOrEmpty(projectPath) || string.IsNullOrEmpty(projectName))
                        {
                            return;
                        }

                        SyncDone(projectName, projectPath);
                    });
                }

                if (action == "UPLOAD_FIX" && UserRoleHelpers.IsEditors() && user == userName)
                {
                    Task.Run(() =>
                    {
                        var localProjectPath = Path.Combine(localPath, localProjectName);

                        var projectPath = FileHelpers.GetProjectPathByLog(localProjectPath);
                        var projectName = FileHelpers.GetProjectNameByLog(localProjectPath);

                        if (string.IsNullOrEmpty(projectPath) || string.IsNullOrEmpty(projectName))
                        {
                            return;
                        }

                        SyncFix(projectName, projectPath);
                    });
                }

                if (action == "DOWNLOAD_PROJECT" && UserRoleHelpers.IsEditors() && user == userName)
                {
                    Task.Run(() =>
                    {
                        var projectInfo = JsonConvert.DeserializeObject<DownloadProjectInfo>(localProjectName);

                        if (projectInfo == null || !FileHelpers.ExistsPathServer(projectInfo.Path))
                        {
                            return;
                        }

                        var projectId = projectInfo.Id; // ProjectName
                        var projectStatus = projectInfo.Status; // ProjectName
                        var projectPath = projectInfo.Path.Trim(); // ServerPath\\ProjectName"
                        var projectName = projectInfo.Name.Trim(); // ProjectName

                        if (projectStatus == (int)PROJECT_STATUS.NEEDFIX)
                        {
                            FileHelpers.AddFileLogProjectPath(projectName, projectPath);

                            CopyDoneAndFixFromServer(projectName, projectPath, projectId);
                        }
                        else
                        {
                            var projectLocalPath = FileHelpers.GetProjectLocalPath(projectName); // LocalPath/ProjectName
                            var projectDoLocalPath = Path.Combine(projectLocalPath, Options.PROJECT_DO_NAME); // LocalPath/ProjectName/Do
                            var projectDoEditorLocalPath = FileHelpers.GetProjectDoEditorLocalPath(projectName); // LocalPath/ProjectName/Do/EditorName

                            FileHelpers.CreateFolder(projectDoLocalPath);
                            FileHelpers.CreateFolder(projectDoEditorLocalPath);

                            FileHelpers.AddFileLogProjectPath(projectName, projectPath);

                            // Tạo thư mục Done
                            var projectDoneEditorLocalPath = FileHelpers.GetProjectDoneEditorLocalPath(projectName);
                            FileHelpers.CreateFolder(projectDoneEditorLocalPath);

                            // check sync do path
                            SyncDo(projectName, projectPath);

                            var sampleLocalPath = FileHelpers.GetProjectSampleLocalPath(projectName);
                            var sampleServerPath = Path.Combine(projectPath, Options.PROJECT_SAMPLE_NAME);

                            if (Directory.Exists(sampleServerPath))
                            {
                                var isSyncSample = displayFolder.CheckFolderSync(sampleLocalPath, sampleServerPath);

                                if (!isSyncSample)
                                {
                                    Invoke((Action)(() =>
                                    {
                                        addItem(DateTime.Now, "Get Sample",null, projectName, 0);
                                    }));

                                    FileHelpers.DownloadFolderFromServer(sampleServerPath, sampleLocalPath, null, true, true);

                                    Invoke((Action)(() =>
                                    {
                                        addItem(DateTime.Now, "Get Sample", null, projectName, 1);
                                    }));
                                }
                            }

                            // create folder Done by server Done
                            FileHelpers.EditorCreateFolderDonePath(projectPath, projectName);

                            displayFolder.CheckFolderSync(sampleLocalPath, sampleServerPath);
                        }
                    });
                }
            });

            connection.On<string, string, string>("HRM_PROJECT", async (users, action, projectName) =>
            {
                if(action == "EDITOR_REMOVE_COMPLETED")
                {
                    if(string.IsNullOrEmpty(users))
                    {
                        return;
                    }
                    var listUsers = JsonConvert.DeserializeObject<List<int>>(users);
                    if(listUsers.Any(u => u == userId))
                    {
                        Task.Run(() =>
                        {
                            Invoke((Action)(() =>
                            {
                                addItem(DateTime.Now, "Completed", null, projectName, 0);
                            }));

                            var localProjectPath = Path.Combine(localPath, projectName);
                            if (!Directory.Exists(localProjectPath))
                            {
                                return;
                            }

                            var project = projectService.RequestGetProjectByName(projectName);

                            if (project != null && project.StatusId == (int)PROJECT_STATUS.COMPLETED)
                            {
                                DirectoryInfo di = new DirectoryInfo(localProjectPath);
                                var allFolders = di.GetDirectories();

                                FileHelpers.FolderSetAttributeNormal(localProjectPath);

                                try
                                {
                                    Directory.Delete(localProjectPath, true);
                                    Invoke((Action)(() =>
                                    {
                                        addItem(DateTime.Now, "Completed", null, projectName, 1);
                                    }));
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.Message);
                                    Invoke((Action)(() =>
                                    {
                                        addItem(DateTime.Now, "Completed", null, projectName, 2);
                                    }));
                                }
                                finally { }
                            }
                        });
                    }
                }
            });

            connection.On<string, string, string>("HRM_USER", async (user, action, data) =>
            {
                if (action == "UPDATE_LOCAL_PATH" && UserRoleHelpers.IsEditors() && user == userId.ToString())
                {
                    var userDetail = JsonConvert.DeserializeObject<UserDto>(data);
                    if (!string.IsNullOrEmpty(userDetail.ProjectLocalPath))
                    {
                        Properties.Settings.Default.ProjectLocalPath = userDetail.ProjectLocalPath;
                        Properties.Settings.Default.Save();
                    }
                }
            });

            connection.On<string, string, string, double, double, double, string>("WAND_ADDON_MESSAGE_FREQUENCY", async (user, action, path, blend, dodge, burn, filter) =>
            {
                if (action == "FREQUENCY_ALL_IMAGE")
                {
                    Task.Run( async() =>
                    {
                        try
                        {                        
                            DirectoryInfo directory = new DirectoryInfo(path);
                            var files = directory.GetFiles()
                 .Where(s => Options.PROJECT_IMAGE_FILE_TYPE_JPG.Contains(Path.GetExtension(s.Extension).TrimStart('.').ToUpper()))
                 .OrderBy(p => p.FullName)
                 .Select(item => item.FullName).ToList();
                            if(filter == "all") {
                                foreach (var item in files)
                                {
                                    try
                                    {
                                        var name = Path.GetFileName(item);
                                        var base64Image = FileHelpers.GetBase64StringForImage(item);

                                        var base64_guidance = HttpClientHelper.callAPIGuidance(Url.GetBase64Guidance, base64Image);
                                        var base64_dodge_burn = HttpClientHelper.callAPIDodgeAndBurn(Url.GetBase64DodgeAndBurn, base64Image, base64_guidance);
                                        DodgeAndBurnDto base64_dodge_burn_obj = System.Text.Json.JsonSerializer.Deserialize<DodgeAndBurnDto>(base64_dodge_burn);
                                        var basse64_layer = HttpClientHelper.exportBase64(Url.GetBase64Frequency, base64Image, base64_dodge_burn_obj.base64_dodge, base64_dodge_burn_obj.base64_burn, dodge, burn, blend);
                                        FileHelpers.Base64ToImage(basse64_layer, path, name, user);
                                        var filesnew = directory.GetFiles()
                .Where(s => Options.PROJECT_IMAGE_FILE_TYPE_JPG.Contains(Path.GetExtension(s.Extension).TrimStart('.').ToUpper()))
                .OrderBy(p => p.FullName)
                .Select(p => p.FullName).ToList();
                                        if (filesnew.Count != files.Count)
                                        {
                                            FrequencyNewImage(user, action, path, blend, dodge, burn);
                                            break;
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e.Message);
                                    }
                                }

                            }
                            else
                            {
                                FrequencyNewImage(user, action, path, blend, dodge, burn);
                            }

                          }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    });
                }
            });


        }

        public void FrequencyNewImage(string user,string action,string path,double blend,double dodge,double burn)
        {
            Task.Run(async () =>
            {
                try
                {
                    DirectoryInfo directory = new DirectoryInfo(path);
                    var files = directory.GetFiles()
         .Where(s => Options.PROJECT_IMAGE_FILE_TYPE_JPG.Contains(Path.GetExtension(s.Extension).TrimStart('.').ToUpper()))
         .OrderBy(p => p.FullName)
         .Select(item => item.FullName).ToList();

                    foreach (var item in files)
                    {
                        try
                        {
                            string pathImage = item.Replace("Working\\" + user, "Frequency");
               
                            if (!File.Exists(pathImage))
                            {
                                var name = Path.GetFileName(item);
                                var base64Image = FileHelpers.GetBase64StringForImage(item);

                                var base64_guidance = HttpClientHelper.callAPIGuidance(Url.GetBase64Guidance, base64Image);
                                var base64_dodge_burn = HttpClientHelper.callAPIDodgeAndBurn(Url.GetBase64DodgeAndBurn, base64Image, base64_guidance);
                                DodgeAndBurnDto base64_dodge_burn_obj = System.Text.Json.JsonSerializer.Deserialize<DodgeAndBurnDto>(base64_dodge_burn);
                                var basse64_layer = HttpClientHelper.exportBase64(Url.GetBase64Frequency, base64Image, base64_dodge_burn_obj.base64_dodge, base64_dodge_burn_obj.base64_burn, dodge, burn, blend);
                                FileHelpers.Base64ToImage(basse64_layer, path, name, user);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            });
        }


            private void FormHome_Load(object sender, EventArgs e)
        {

        }

        public void DisplayAccountProfile()
        {
            ovalPictureBox1.ImageLocation = "https://hrm.wand.vn/" + Properties.Settings.Default.Avatar;
            lbHello.Text = "Hi, " + String.Concat(Properties.Settings.Default.LastName, " ", Properties.Settings.Default.FirstName);
        }

        private void label3_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reset();
            this.Close();
            FormLogin login = new FormLogin();
            login.Show();
            return;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void notifyIcon2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ShowInTaskbar = true;
            Show();
        }

        private void pnlHeader_MouseUp(object sender, MouseEventArgs e)
        {
            _mouseDown = false;
        }

        private void pnlHeader_MouseDown(object sender, MouseEventArgs e)
        {
            _mouseDown = true;
            _lastLocation = e.Location;
        }

        private void pnlHeader_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_mouseDown) return;
            Location = new Point(
                (Location.X - _lastLocation.X) + e.X, (Location.Y - _lastLocation.Y) + e.Y);

            Update();
        }
    }
}
