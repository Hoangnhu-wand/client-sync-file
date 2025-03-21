﻿using System;
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
using System.Security.Principal;
using System.Diagnostics;

namespace WandSyncFile
{
    public partial class FormHome : Form
    {
        HubConnection connection;
        ProjectService projectService;
        CancellationToken cancellationToken;
        CancellationToken cancellationTokenRemoveProject;
        DisplayFolder displayFolder;
        public List<int> processingDownLoad = new List<int>();
        public bool _mouseDown;
        public Point _lastLocation;

        public FormHome()
        {
            IntPtr token = UserHelpers.GetToken(Options.SERVER_FILE_15);
            IntPtr token08 = UserHelpers.GetToken(Options.SERVER_FILE_08);
            IntPtr token09 = UserHelpers.GetToken(Options.SERVER_FILE_09);
            IntPtr token16 = UserHelpers.GetToken(Options.SERVER_FILE_16);
            bool isConnected = FileHelpers.IsVpnConnected();
            using (WindowsImpersonationContext impersonatedUser = WindowsIdentity.Impersonate(token))
            {
                using (WindowsImpersonationContext impersonatedUser08 = WindowsIdentity.Impersonate(token08))
                {
                    using (WindowsImpersonationContext impersonatedUser09 = WindowsIdentity.Impersonate(token09))
                    {
                        using (WindowsImpersonationContext impersonatedUser16 = WindowsIdentity.Impersonate(token16))
                        {
                            InitializeComponent();
                            setupAutoRun();

                            displayFolder = new DisplayFolder();
                            projectService = new ProjectService();
                            cancellationToken = new CancellationToken();
                            cancellationTokenRemoveProject = new CancellationToken();

                            HandleHubConnection();
                            DisplayAccountProfile();
                            if (!isConnected)
                            {
                                ReadFileChange(cancellationToken);
                            }
                            RemoveCompletedProject(cancellationTokenRemoveProject);
                        }

                    }
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

        public void addItem(DateTime created, string action, string count, string projectName, int status)
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

                if (flowLayoutPanel.Controls.Count > 200)
                {
                    try
                    {
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
                    catch (Exception)
                    {

                    }
                }

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
                    Invoke((Action)(() =>
                    {
                        addItem(DateTime.Now, "Clear!", true);
                    }));
                }
                catch (Exception)
                {
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

                    if (project != null && project.StatusId == (int)PROJECT_STATUS.COMPLETED)
                    {
                        var localProject = Path.Combine(projectLocalPath, projectName);
                        if (Directory.Exists(localProject))
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

                    try
                    {
                        SyncDo(projectName, projectPath);
                    }
                    catch (Exception err)
                    {
                        Invoke((Action)(async () =>
                        {
                            addItem(DateTime.Now, "Error Sync Do_", projectName, err.Message, 2);
                        }));
                    }

                    try
                    {
                        SyncDone(projectName, projectPath);
                    }
                    catch (Exception err)
                    {
                        Invoke((Action)(async () =>
                        {
                            addItem(DateTime.Now, "Error Sync Done_", projectName, err.Message, 2);
                        }));
                    }

                    try
                    {
                        SyncFix(projectName, projectPath);
                    }
                    catch (Exception err)
                    {
                        Invoke((Action)(async () =>
                        {
                            addItem(DateTime.Now, "Error Sync Fix_", projectName, err.Message, 2);
                        }));
                    }
                }
            }
            catch (Exception e)
            {
                var errMessage = DateTime.Now.ToString() + "---" + e.Message + " ---- ";
                FileHelpers.WriteLog(errMessage);
                Invoke((Action)(async () =>
                {
                    addItem(DateTime.Now, "Error", null, "System not working", 2);
                }));
            }
        }

        public void SyncFix(string projectName, string projectPath)
        {
            try
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



                /*   var isSyncFix = displayFolder.CheckFolderFixSync(lastFixFolderLocalPath, lastFixFolderServerPath, folderDoneServer, lastFixFolderLocalPath);
                   if (isSyncFix)
                   {
                       Invoke((Action)(async () =>
                       {
                           addItem(DateTime.Now, "No Change Fix", null, projectName, 1);
                       }));
                       return;
                   }*/

                var project = projectService.RequestGetProjectByName(projectName);
                if (project == null || (project != null && (project.StatusId == (int)PROJECT_STATUS.CHECKED || project.StatusId == (int)PROJECT_STATUS.COMPLETED)))
                {
                    Invoke((Action)(async () =>
                    {
                        addItem(DateTime.Now, "Project No Change", null, projectName, 1);
                    }));
                    return;
                }


                if (processingDownLoad.Any(pId => pId == project.Id))
                {
                    Invoke((Action)(async () =>
                    {
                        addItem(DateTime.Now, "Downloading", null, projectName, 1);
                    }));
                    return;
                }

                bool isConnected = FileHelpers.IsVpnConnected();

                if (isConnected && processingDownLoad.Any())
                {
                    Invoke((Action)(() =>
                    {
                        addItem(DateTime.Now, "Download Fix", null, "Đang có dự án được tải", 2);
                    }));

                    return;
                }

                if (!processingDownLoad.Any(pId => pId == project.Id))
                {
                    displayFolder.CheckFolderSync(lastFixFolderLocalPath, lastFixFolderServerPath, lastFixFolderLocalPath);

                    //đếm số ảnh fix trước khi sync
                    var imageFixLocalFirst = FileHelpers.CountImageFolder(lastFixFolderLocalPath);
                    var imageFixServerFirst = FileHelpers.CountImageFolder(lastFixFolderServerPath);
                    var countFirts = imageFixLocalFirst + "/" + imageFixServerFirst;
                    Invoke((Action)(async () =>
                    {
                        addItem(DateTime.Now, "Upload Fix", countFirts, projectName, 0);
                    }));

                    processingDownLoad.Add(project.Id);

                    try
                    {
                        FileHelpers.CopyDirectoryToServer(lastFixFolderLocalPath, lastFixFolderServerPath);

                        //đếm số ảnh fix sau khi sync
                        var imageFixLocalLast = FileHelpers.CountImageFolder(lastFixFolderLocalPath);
                        var imageFixServerLast = FileHelpers.CountImageFolder(lastFixFolderServerPath);
                        var countLast = imageFixLocalLast + "/" + imageFixServerLast;
                        Invoke((Action)(async () =>
                        {
                            addItem(DateTime.Now, "Upload Fix", countLast, projectName, 1);
                        }));
                    }
                    catch (Exception e)
                    {
                        Invoke((Action)(async () =>
                        {
                            addItem(DateTime.Now, "Upload Fix", null, projectName, 2);
                        }));
                    }
                    finally
                    {
                        displayFolder.CheckFolderSync(lastFixFolderLocalPath, lastFixFolderServerPath, lastFixFolderLocalPath);

                        processingDownLoad.Remove(project.Id);
                    }
                }
            }
            catch (Exception err)
            {
                Invoke((Action)(async () =>
                {
                    addItem(DateTime.Now, "Error Sync Fix", projectName, err.Message, 2);
                }));
            }

        }

        public void SyncDo(string projectName, string projectPath)
        {

            try
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
                        addItem(DateTime.Now, "No Change Do", null, projectName, 1);
                    }));
                    return;
                }

                bool isConnected = FileHelpers.IsVpnConnected();

                if (isConnected && processingDownLoad.Any())
                {
                    Invoke((Action)(() =>
                    {
                        addItem(DateTime.Now, "Download Do", null, "Đang có dự án được tải", 2);
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

                if (processingDownLoad.Any(pId => pId == project.Id))
                {
                    Invoke((Action)(async () =>
                    {
                        addItem(DateTime.Now, "Downloading", null, projectName, 1);
                    }));
                    return;
                }


                if (!isSyncDo && !processingDownLoad.Any(pId => pId == project.Id))
                {
                    displayFolder.CheckFolderSync(projectDoEditorLocalPath, projectDoEditorServerPath, projectDoLocalPath);

                    var imageDoLocalFirst = FileHelpers.CountImageFolder(projectDoEditorLocalPath);
                    var imageDoServerFirst = FileHelpers.CountImageFolder(projectDoEditorServerPath);
                    var counDotFirst = imageDoLocalFirst + "/" + imageDoServerFirst;
                    Invoke((Action)(async () =>
                    {
                        addItem(DateTime.Now, "Download Do", counDotFirst, projectName, 0);
                    }));

                    processingDownLoad.Add(project.Id);

                    try
                    {
                        FileHelpers.DownloadFolderFromServer(projectDoEditorServerPath, projectDoEditorLocalPath, null, true);
                        FileHelpers.RemoveFolder(projectDoEditorLocalPath, projectDoEditorServerPath);
                        //đếm số ảnh do sau khi sync
                        var imageDoLocalLast = FileHelpers.CountImageFolder(projectDoEditorLocalPath);
                        var imageDoServerLast = FileHelpers.CountImageFolder(projectDoEditorServerPath);
                        var countDoLast = imageDoLocalLast + "/" + imageDoServerLast;
                        Invoke((Action)(async () =>
                        {
                            addItem(DateTime.Now, "Download Do", countDoLast, projectName, 1);
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
                                addItem(DateTime.Now, "Do => Working", countDoToWorkingFirst, projectName, 0);
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
                            addItem(DateTime.Now, "Download Do", null, projectName, 2);
                        }));
                    }
                    finally
                    {
                        displayFolder.CheckFolderSync(projectDoEditorLocalPath, projectDoEditorServerPath, projectDoLocalPath);

                        processingDownLoad.Remove(project.Id);
                    }
                }
            }
            catch (Exception err)
            {
                Invoke((Action)(async () =>
                {
                    addItem(DateTime.Now, "Error Sync Do", projectName, err.Message, 2);
                }));
            }


        }

        public void SyncDone(string projectName, string projectPath)
        {

            try
            {
                var editorUserName = Properties.Settings.Default.Username;
                var localPath = Properties.Settings.Default.ProjectLocalPath;

                var projectLocalPath = Path.Combine(localPath, projectName);
                var projectDoneLocalPath = Path.Combine(projectLocalPath, Options.PROJECT_DONE_NAME);
                var projectDoneEditorLocalPath = Path.Combine(projectDoneLocalPath, editorUserName);

                var projectDoneServerPath = Path.Combine(projectPath, Options.PROJECT_DONE_NAME);
                var projectDoneEditorServerPath = Path.Combine(projectDoneServerPath, editorUserName);


                var projectWorkingLocalPath = Path.Combine(projectLocalPath, Options.PROJECT_WORKING_PATH_NAME);
                var projectWorkingEditorLocalPath = Path.Combine(projectWorkingLocalPath, editorUserName);

                var isSyncDone = displayFolder.CheckFolderSyncCompleted(projectDoneEditorLocalPath, projectDoneEditorServerPath);

                if (isSyncDone)
                {
                    Invoke((Action)(async () =>
                    {
                        addItem(DateTime.Now, "No Change Done", null, projectName, 1);
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
                if (processingDownLoad.Any(pId => pId == project.Id))
                {
                    Invoke((Action)(async () =>
                    {
                        addItem(DateTime.Now, "Downloading", null, projectName, 1);
                    }));
                    return;
                }

                bool isConnected = FileHelpers.IsVpnConnected();

                if (isConnected && processingDownLoad.Any())
                {
                    Invoke((Action)(async () =>
                    {
                        addItem(DateTime.Now, "Download Done", null, "Đang có dự án được tải", 2);
                    }));

                    return;
                }

                if (!isSyncDone && !processingDownLoad.Any(pId => pId == project.Id))
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

                    processingDownLoad.Add(project.Id);

                    try
                    {
                        FileHelpers.SyncDirectoryDoneToServer(projectDoneEditorLocalPath, projectDoneEditorServerPath);

                        //DOne to Working
                        FileHelpers.DownloadFolder(projectDoneEditorLocalPath, projectWorkingEditorLocalPath);

                        //đếm số ảnh Done sau khi sync
                        var imageDoneLocalLast = FileHelpers.CountImageFolder(projectDoneEditorLocalPath);
                        var imageDoneServerLast = FileHelpers.CountImageFolder(projectDoneEditorServerPath);
                        var countDoneLast = imageDoneLocalLast + "/" + imageDoneServerLast;
                        Invoke((Action)(async () =>
                        {
                            addItem(DateTime.Now, "Upload Done", countDoneLast, projectName, 1);
                        }));
                    }
                    catch (Exception e)
                    {
                        Invoke((Action)(async () =>
                        {
                            addItem(DateTime.Now, "Upload Done", null, projectName, 2);
                        }));
                    }
                    finally
                    {
                        displayFolder.CheckFolderSync(projectDoneEditorLocalPath, projectDoneEditorServerPath, projectDoneLocalPath);

                        processingDownLoad.Remove(project.Id);
                    }
                }
            }
            catch (Exception err)
            {

                var errMessage = DateTime.Now.ToString() + "Sync Done -  " + err.Message + " ---- " + projectName;
                FileHelpers.WriteLog(errMessage);

                Invoke((Action)(async () =>
                {
                    addItem(DateTime.Now, "Error Sync Done", projectName, err.Message, 2);
                }));
            }
        }

        public void SyncDoneVPN(string projectName, string projectPath)
        {

            try
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
                        addItem(DateTime.Now, "No Change Done", null, projectName, 1);
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
                if (processingDownLoad.Any(pId => pId == project.Id))
                {
                    Invoke((Action)(async () =>
                    {
                        addItem(DateTime.Now, "Downloading", null, projectName, 1);
                    }));
                    return;
                }

                bool isConnected = FileHelpers.IsVpnConnected();

                if (isConnected && processingDownLoad.Any())
                {
                    Invoke((Action)(async () =>
                    {
                        addItem(DateTime.Now, "Download Done", null, "Đang có dự án được tải", 2);
                    }));

                    return;
                }

                if (!isSyncDone && !processingDownLoad.Any(pId => pId == project.Id))
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

                    processingDownLoad.Add(project.Id);

                    try
                    {
                        FileHelpers.SyncDirectoryDoneVPN(projectDoneEditorLocalPath, projectDoneEditorServerPath);

                        //đếm số ảnh Done sau khi sync
                        var imageDoneLocalLast = FileHelpers.CountImageFolder(projectDoneEditorLocalPath);
                        var imageDoneServerLast = FileHelpers.CountImageFolder(projectDoneEditorServerPath);
                        var countDoneLast = imageDoneLocalLast + "/" + imageDoneServerLast;
                        Invoke((Action)(async () =>
                        {
                            addItem(DateTime.Now, "Upload Done", countDoneLast, projectName, 1);
                        }));
                    }
                    catch (Exception e)
                    {
                        Invoke((Action)(async () =>
                        {
                            addItem(DateTime.Now, "Upload Done", null, projectName, 2);
                        }));
                    }
                    finally
                    {
                        displayFolder.CheckFolderSync(projectDoneEditorLocalPath, projectDoneEditorServerPath, projectDoneLocalPath);

                        processingDownLoad.Remove(project.Id);
                    }
                }
            }
            catch (Exception err)
            {
                Invoke((Action)(async () =>
                {
                    addItem(DateTime.Now, "Error Sync Done", projectName, err.Message, 2);
                }));
            }
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
                        try
                        {
                            var editorDownloadItem = JsonConvert.DeserializeObject<EditorDownloadFileProjectDto>(data);

                            if (editorDownloadItem == null || !FileHelpers.ExistsPathServer(editorDownloadItem.ProjectPath))
                            {
                                return;
                            }


                            var projectPath = editorDownloadItem.ProjectPath.Trim(); // ServerPath\\ProjectName"
                            var projectName = editorDownloadItem.ProjectName.Trim(); // ProjectName
                            var projectId = editorDownloadItem.ProjectId; // ProjectName


                            try
                            {
                                //Create folder
                                var pathProject = FileHelpers.GetProjectLocalPath(projectName);
                                var createDo = Path.Combine(pathProject, Options.PROJECT_DO_NAME);
                                var createDone = Path.Combine(pathProject, Options.PROJECT_DONE_NAME);
                                var createWorking = Path.Combine(pathProject, Options.PROJECT_WORKING_PATH_NAME);

                                if (!Directory.Exists(pathProject))
                                {
                                    Directory.CreateDirectory(pathProject);
                                }

                                if (!Directory.Exists(createDo))
                                {
                                    Directory.CreateDirectory(createDo);
                                }

                                if (!Directory.Exists(createDone))
                                {
                                    Directory.CreateDirectory(createDone);
                                }

                                if (!Directory.Exists(createWorking))
                                {
                                    Directory.CreateDirectory(createWorking);
                                }
                            }
                            catch (Exception ed)
                            {
                                var errMessageCreat = DateTime.Now.ToString() + "Err Create folder server -  " + ed.Message + " --- " + projectName;
                                FileHelpers.WriteLog(errMessageCreat);
                            }

                            if (processingDownLoad.Any(id => id == editorDownloadItem.ProjectId))
                            {
                                Invoke((Action)(async () =>
                                {
                                    addItem(DateTime.Now, "Downloading", null, projectName, 0);
                                }));

                                return;
                            }

                            bool isConnected = FileHelpers.IsVpnConnected();

                            if (isConnected && processingDownLoad.Any())
                            {
                                Invoke((Action)(() =>
                                {
                                    addItem(DateTime.Now, "Warning", null, "Đang có dự án được tải", 2);
                                }));


                                return;
                            }

                            var projectLocalPath = FileHelpers.GetProjectLocalPath(projectName); // LocalPath/ProjectName
                            var projectDoLocalPath = Path.Combine(projectLocalPath, Options.PROJECT_DO_NAME); // LocalPath/ProjectName/Do
                            var projectDoEditorLocalPath = FileHelpers.GetProjectDoEditorLocalPath(projectName); // LocalPath/ProjectName/Do/EditorName

                            //Kiếm tra kết nối VPN
                            //Có kết nối => Chỉ tải 1 dự án /1 lần

                            processingDownLoad.Add(editorDownloadItem.ProjectId);
                            Invoke((Action)(async () =>
                            {
                                addItem(DateTime.Now, "Start Download", null, projectName, 0);
                            }));
                            FileHelpers.CreateFolder(projectDoLocalPath);
                            FileHelpers.CreateFolder(projectDoEditorLocalPath);

                            FileHelpers.AddFileLogProjectPath(projectName, projectPath);

                            var imagesPriority = FileHelpers.ListImagePriority(projectPath, userName);

                            FileHelpers.AddFileLogProjectPath(projectName, projectPath);
                            await FileHelpers.CopyImagePriority(imagesPriority, projectPath, projectLocalPath, userName);
                            var listImagesNotPriority = FileHelpers.ListImageNotPriority(projectPath, userName, imagesPriority);
                            await FileHelpers.CopyImagePriority(listImagesNotPriority, projectPath, projectLocalPath, userName);

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

                            processingDownLoad.Remove(projectId);

                            await Task.Run(() => SyncDo(projectName, projectPath));
                            await Task.Run(() => SyncFix(projectName, projectPath));
                            await Task.Run(() => SyncDone(projectName, projectPath));

                            Invoke((Action)(async () =>
                            {
                                addItem(DateTime.Now, "Download Success", null, projectName, 1);
                            }));

                            await connection.SendAsync("ReceiverMessageAsync", "CLIENT_FILE", editorDownloadItem.MessageId, "REMOVE_PROJECT_QUEUE_MESSAGE", null);
                        }
                        catch (Exception e)
                        {
                            var errMessage = DateTime.Now.ToString() + "Err download server -  " + e.Message + " ---- " + data;
                            FileHelpers.WriteLog(errMessage);
                        }

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

                        if (processingDownLoad.Any(id => id == editorDownloadItem.ProjectId))
                        {
                            Invoke((Action)(async () =>
                            {
                                addItem(DateTime.Now, "Downloading", null, editorDownloadItem.ProjectName, 0);
                            }));

                            return;
                        }

                        bool isConnected = FileHelpers.IsVpnConnected();

                        if (isConnected && processingDownLoad.Any())
                        {
                            Invoke((Action)(() =>
                            {
                                addItem(DateTime.Now, "Warning", null, "Đang có dự án được tải", 2);
                            }));


                            return;
                        }

                        var projectName = editorDownloadItem.ProjectName;
                        var projectPath = editorDownloadItem.ProjectPath;
                        var projectId = editorDownloadItem.ProjectId;

                        processingDownLoad.Add(editorDownloadItem.ProjectId);

                        var localFolderFix = FileHelpers.GetProjectLocalPath(editorDownloadItem.ProjectName) + serverFileArr.Last(); //LocalPath\\ProjectName\\Fix_3
                        FileHelpers.CreateFolder(localFolderFix);

                        FileHelpers.AddFileLogProjectPath(editorDownloadItem.ProjectName, editorDownloadItem.ProjectPath);

                        var project = projectService.RequestGetProjectById(editorDownloadItem.ProjectId);
                        var localProjectPath = Path.Combine(localPath, editorDownloadItem.ProjectName);
                        if (project != null && project.StatusId == (int)PROJECT_STATUS.NEEDFIX)
                        {
                            var imagesPriority = new List<string>();
                            imagesPriority = project.ListImages;
                            await FileHelpers.CopyImagePriority(imagesPriority, editorDownloadItem.ProjectPath, localProjectPath, userName);
                            var listImagesNotPriority = FileHelpers.ListImageNotPriority(editorDownloadItem.ProjectPath, userName, imagesPriority);
                            await FileHelpers.CopyImagePriority(listImagesNotPriority, editorDownloadItem.ProjectPath, localProjectPath, userName);
                        }

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

                        processingDownLoad.Remove(projectId);

                        await Task.Run(() => SyncDo(projectName, projectPath));
                        await Task.Run(() => SyncFix(projectName, projectPath));
                        await Task.Run(() => SyncDone(projectName, projectPath));

                        Invoke((Action)(async () =>
                        {
                            addItem(DateTime.Now, "Download Success", null, projectName, 1);
                        }));

                        await connection.SendAsync("ReceiverMessageAsync", "CLIENT_FILE", editorDownloadItem.MessageId, "REMOVE_PROJECT_QUEUE_MESSAGE", null);
                    });
                }

                if (action == "UPDATE_PROJECT_PATH" && user == userId.ToString() && UserRoleHelpers.IsEditors())
                {

                    Task.Run(async () =>
                    {
                        try
                        {
                            var updateProjectPath = JsonConvert.DeserializeObject<UpdateProjectPathDto>(data);

                            var projectName = updateProjectPath.ProjectName;
                            var projectPath = updateProjectPath.ProjectPath;

                            var localProjectPath = Path.Combine(Properties.Settings.Default.ProjectLocalPath, projectName);

                            if (Directory.Exists(localProjectPath))
                            {
                                var createPath = Path.Combine(localProjectPath, Options.PROJECT_PATH_FILE_NAME);

                                if (File.Exists(createPath))
                                {
                                    File.Delete(createPath);
                                }
                                FileHelpers.AddFileLogProjectPath(projectName, projectPath);

                                Invoke((Action)(async () =>
                                    {
                                        addItem(DateTime.Now, "Update project path", null, projectName, 1);
                                    }));
                            }

                            await connection.SendAsync("ReceiverMessageAsync", "CLIENT_FILE", updateProjectPath.MessageId, "REMOVE_UPDATE_PROJECT_PATH_QUEUE_MESSAGE", null);
                        }
                        catch (Exception ex)
                        { }
                    });
                }
            });

            connection.On<string, string, string>("WAND_ADDON_MESSAGE", async (user, action, localProjectName) =>
            {
                if (action == "UPLOAD_DONE" && UserRoleHelpers.IsEditors() && user == userName)
                {
                    Task.Run(async () =>
                    {
                        var localProjectPath = Path.Combine(localPath, localProjectName);

                        var projectPath = FileHelpers.GetProjectPathByLog(localProjectPath);
                        var projectName = FileHelpers.GetProjectNameByLog(localProjectPath);

                        if (string.IsNullOrEmpty(projectPath) || string.IsNullOrEmpty(projectName))
                        {
                            return;
                        }
                        bool isConnected = FileHelpers.IsVpnConnected();

                        if (isConnected)
                        {
                            SyncDoneVPN(projectName, projectPath);
                        }
                        else
                        {
                            SyncDone(projectName, projectPath);
                        }

                    });
                }

                if (action == "UPLOAD_FIX" && UserRoleHelpers.IsEditors() && user == userName)
                {
                    Task.Run(async () =>
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
                    Task.Run(async () =>
                    {

                        var projectInfo = JsonConvert.DeserializeObject<DownloadProjectInfo>(localProjectName);

                        if (projectInfo == null || !FileHelpers.ExistsPathServer(projectInfo.Path))
                        {

                            Invoke((Action)(async () =>
                            {
                                addItem(DateTime.Now, "Error download", null, "Project or path not found", 2);
                            }));
                            return;
                        }
                        var projectId = projectInfo.Id;
                        var projectStatus = projectInfo.Status;
                        var projectPath = projectInfo.Path.Trim();
                        var projectName = projectInfo.Name.Trim();
                        var downloadAll = projectInfo.DownloadAll;
                        try
                        {
                            var localProjectPath = Path.Combine(localPath, projectInfo.Name);

                            try
                            {

                                var createDo = Path.Combine(localProjectPath, Options.PROJECT_DO_NAME);
                                var createDone = Path.Combine(localProjectPath, Options.PROJECT_DONE_NAME);
                                var createWorking = Path.Combine(localProjectPath, Options.PROJECT_WORKING_PATH_NAME);
                                if (!Directory.Exists(localProjectPath))
                                {
                                    Directory.CreateDirectory(localProjectPath);
                                }

                                if (!Directory.Exists(createDo))
                                {
                                    Directory.CreateDirectory(createDo);
                                }

                                if (!Directory.Exists(createDone))
                                {
                                    Directory.CreateDirectory(createDone);
                                }

                                if (!Directory.Exists(createWorking))
                                {
                                    Directory.CreateDirectory(createWorking);
                                }
                            }
                            catch (Exception ed)
                            {
                                var errMessageCreat = DateTime.Now.ToString() + "Err Create folder Addon -  " + ed.Message + " ---- " + localProjectPath;
                                FileHelpers.WriteLog(errMessageCreat);
                            }

                            var imagesPriority = new List<string>();

                            if (processingDownLoad.Any(id => id == projectId))
                            {
                                Invoke((Action)(async () =>
                                {
                                    addItem(DateTime.Now, "Downloading", null, projectName, 0);
                                }));

                                return;
                            }

                            //Kiếm tra kết nối VPN
                            //Có kết nối => Chỉ tải 1 dự án /1 lần
                            bool isConnected = FileHelpers.IsVpnConnected();

                            if (isConnected && processingDownLoad.Any())
                            {
                                Invoke((Action)(() =>
                                {
                                    addItem(DateTime.Now, "Warning", null, "Đang có dự án được tải", 2);
                                }));

                                return;
                            }


                            processingDownLoad.Add(projectId);
                            Invoke((Action)(async () =>
                            {
                                addItem(DateTime.Now, "Start Download", null, projectName, 0);
                            }));

                            var isProcessing = false;

                            if (projectStatus != Options.PROJECT_STATUS_PROCESSING)
                            {
                                var project = projectService.RequestGetProjectById(projectId);
                                if (project != null)
                                {
                                    if (project.ListEditors.Any(item => item.Status == ProjectEditorStatus.NEEDCHECK))
                                    {
                                        // Editor đã needcheck => lấy danh sách ảnh fix
                                        imagesPriority = project.ListImages;
                                    }
                                    else
                                    {
                                        // Editor chưa needcheck => Lấy danh sách không có trong Done
                                        imagesPriority = FileHelpers.ListImagePriority(projectPath, userName);
                                        isProcessing = true;
                                    }
                                }
                            }
                            else
                            {
                                // Editor chưa needcheck => Lấy danh sách không có trong Done
                                imagesPriority = FileHelpers.ListImagePriority(projectPath, userName);
                                isProcessing = true;
                            }
                            FileHelpers.AddFileLogProjectPath(projectName, projectPath);
                            await FileHelpers.CopyImagePriority(imagesPriority, projectPath, localProjectPath, userName);

                            if (downloadAll || !isConnected || !imagesPriority.Any())
                            {
                                var listImagesNotPriority = FileHelpers.ListImageNotPriority(projectPath, userName, imagesPriority);
                                await FileHelpers.CopyImagePriority(listImagesNotPriority, projectPath, localProjectPath, userName);

                            }


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

                            processingDownLoad.Remove(projectId);
                            await Task.Run(() => SyncFix(projectName, projectPath));
                            if (isProcessing || downloadAll || !isConnected)
                            {
                                await Task.Run(() => SyncDo(projectName, projectPath));
                                await Task.Run(() => SyncDone(projectName, projectPath));
                            }
                            else
                            {
                                await Task.Run(() => SyncDoneVPN(projectName, projectPath));
                            }

                            Invoke((Action)(async () =>
                            {
                                addItem(DateTime.Now, "Download Success", null, projectName, 1);
                            }));

                        }
                        catch (Exception e)
                        {

                            processingDownLoad.Remove(projectInfo.Id);
                            var errMessage = DateTime.Now.ToString() + "Err download -  " + e.Message + " ---- " + projectName;
                            FileHelpers.WriteLog(errMessage);
                            Invoke((Action)(async () =>
                            {
                                addItem(DateTime.Now, "Error download", projectName, e.Message, 2);
                            }));
                        }



                    });


                }

                if (action == "DOWNLOAD_PROJECT_FIX" && UserRoleHelpers.IsEditors() && user == userName)
                {

                    Task.Run(async () =>
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
                        var downloadAll = projectInfo.DownloadAll;
                        try
                        {
                            if (processingDownLoad.Any(id => id == projectId))
                            {
                                Invoke((Action)(async () =>
                                {
                                    addItem(DateTime.Now, "Downloading", null, projectName, 0);
                                }));

                                return;
                            }
                            //Có kết nối => Chỉ tải 1 dự án /1 lần
                            bool isConnected = FileHelpers.IsVpnConnected();

                            if (isConnected && processingDownLoad.Any())
                            {
                                Invoke((Action)(() =>
                                {
                                    addItem(DateTime.Now, "Warning", null, "Đang có dự án được tải", 2);
                                }));


                                return;
                            }

                            processingDownLoad.Add(projectId);

                            Invoke((Action)(async () =>
                            {
                                addItem(DateTime.Now, "Start download fix", null, projectName, 0);
                            }));

                            if (projectStatus == (int)PROJECT_STATUS.NEEDFIX)
                            {
                                FileHelpers.AddFileLogProjectPath(projectName, projectPath);
                                var project = projectService.RequestGetProjectById(projectId);
                                var localProjectPath = Path.Combine(localPath, localProjectName);
                                if (project != null)
                                {
                                    var imagesPriority = new List<string>();
                                    imagesPriority = project.ListImages;
                                    await FileHelpers.CopyImagePriority(imagesPriority, projectPath, localProjectPath, userName);

                                    if (downloadAll || !isConnected || !imagesPriority.Any())
                                    {
                                        var listImagesNotPriority = FileHelpers.ListImageNotPriority(projectPath, userName, imagesPriority);
                                        await FileHelpers.CopyImagePriority(listImagesNotPriority, projectPath, localProjectPath, userName);
                                    }

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
                                }
                            }

                            processingDownLoad.Remove(projectId);

                            await Task.Run(() => SyncFix(projectName, projectPath));
                            if (downloadAll || !isConnected)
                            {
                                await Task.Run(() => SyncDo(projectName, projectPath));
                                await Task.Run(() => SyncDoneVPN(projectName, projectPath));
                            }

                            Invoke((Action)(async () =>
                            {
                                addItem(DateTime.Now, "Download Success", null, projectName, 1);
                            }));

                            Invoke((Action)(async () =>
                            {
                                addItem(DateTime.Now, "Download fix success", null, projectName, 1);
                            }));
                        }
                        catch (Exception e)
                        {
                            Invoke((Action)(async () =>
                            {
                                addItem(DateTime.Now, "Error download fix", projectName, e.Message, 2);
                            }));
                        }
                    });

                }

                if (action == "COUNT_IMAGE_BY_PROJECT" && UserRoleHelpers.IsEditors() && user == userName)
                {
                    try
                    {
                        Task.Run(async () =>
                        {
                            var projectInfo = JsonConvert.DeserializeObject<CountImageByProjectInfo>(localProjectName);

                            if (projectInfo != null)
                            {

                                var countImage = new List<CountImageByProjectDto>();
                                var projectLocalPath = Path.Combine(localPath, projectInfo.ProjectName);

                                var projectDirectoties = Directory.GetDirectories(projectLocalPath);
                                var lastFixFolderLocalPath = projectDirectoties.Where(item => Path.GetFileName(item).Trim().StartsWith(Options.PROJECT_FIX_PATH_NAME)).OrderByDescending(item => Path.GetFileName(item)).FirstOrDefault();

                                var pathDoLocal = Path.Combine(projectLocalPath, Options.PROJECT_DO_NAME, projectInfo.UserName);
                                var pathDoServer = Path.Combine(projectInfo.ProjectPath, Options.PROJECT_DO_NAME, projectInfo.UserName);

                                var countDoLocal = await FileHelpers.CountImageByFolder(pathDoLocal, 0, Options.PROJECT_DO_NAME);
                                var countDoServer = await FileHelpers.CountImageByFolder(pathDoServer, 1, Options.PROJECT_DO_NAME);

                                countImage.Add(countDoLocal);
                                countImage.Add(countDoServer);

                                if (lastFixFolderLocalPath != null)
                                {
                                    var folderName = Path.GetFileName(lastFixFolderLocalPath);
                                    var pathFixLocal = Path.Combine(projectLocalPath, folderName);
                                    var pathFixServer = Path.Combine(projectInfo.ProjectPath, folderName);

                                    var countFixLocal = await FileHelpers.CountImageByFolder(pathFixLocal, 0, folderName);
                                    var countFixServer = await FileHelpers.CountImageByFolder(pathFixServer, 1, folderName);

                                    countImage.Add(countFixLocal);
                                    countImage.Add(countFixServer);

                                }
                                else
                                {
                                    var pathDoneLocal = Path.Combine(projectLocalPath, Options.PROJECT_DONE_NAME, projectInfo.UserName);
                                    var pathDoneServer = Path.Combine(projectInfo.ProjectPath, Options.PROJECT_DONE_NAME, projectInfo.UserName);

                                    var countDoneLocal = await FileHelpers.CountImageByFolder(pathDoneLocal, 0, Options.PROJECT_DONE_NAME);
                                    var countDoneServer = await FileHelpers.CountImageByFolder(pathDoneServer, 1, Options.PROJECT_DONE_NAME);

                                    countImage.Add(countDoneLocal);
                                    countImage.Add(countDoneServer);
                                }

                                projectService.RequestTrackingImage(projectInfo.UserId, projectInfo.ProjectId, countImage);

                            }
                        });
                    }
                    catch (Exception e)
                    {
                        Console.Write(e.Message);
                    }

                }

                if (action == "GET_VPN_CONNECTED" && UserRoleHelpers.IsEditors() && user == userName)
                {
                    Task.Run(async () =>
                    {
                        try
                        {
                            var tracking = JsonConvert.DeserializeObject<VPNConnectedDto>(localProjectName);
                            bool isConnected = FileHelpers.IsVpnConnected();
                            tracking.Status = isConnected;

                            projectService.RequestIsVPNConnected(tracking.UserId, tracking.UserName, tracking.Status);
                        }
                        catch (Exception e) { }
                    });
                }
            });

            connection.On<string, string, string>("HRM_PROJECT", async (users, action, projectName) =>
            {
                if (action == "EDITOR_REMOVE_COMPLETED")
                {
                    if (string.IsNullOrEmpty(users))
                    {
                        return;
                    }
                    var listUsers = JsonConvert.DeserializeObject<List<int>>(users);
                    if (listUsers.Any(u => u == userId))
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
                if (action == "DOWNLOAD_SAMPLE" && user == userId.ToString())
                {

                    var project = JsonConvert.DeserializeObject<DownloadProjectInfo>(data);
                    try
                    {
                        Task.Run(async () =>
                        {
                            var pathDone = Path.Combine(project.Path, Options.PROJECT_DONE_NAME, userName);
                            if (!Directory.Exists(pathDone))
                            {
                                return;
                            }

                            var localProjectPath = Path.Combine(localPath, project.Name);
                            if (!Directory.Exists(localProjectPath))
                            {
                                Directory.CreateDirectory(localProjectPath);
                            }


                            if (processingDownLoad.Any(id => id == project.Id))
                            {
                                Invoke((Action)(async () =>
                                {
                                    addItem(DateTime.Now, "Downloading ", null, project.Name, 0);
                                }));
                                return;
                            }
                            Invoke((Action)(async () =>
                            {
                                addItem(DateTime.Now, "Start download ", null, project.Name, 0);
                            }));

                            processingDownLoad.Add(project.Id);

                            FileHelpers.CopyDirectory(pathDone, localProjectPath);

                            processingDownLoad.Remove(project.Id);

                            Invoke((Action)(async () =>
                            {
                                addItem(DateTime.Now, "Download success ", null, project.Name, 1);
                            }));


                        });
                    }
                    catch (Exception e)
                    {
                        Invoke((Action)(async () =>
                        {
                            addItem(DateTime.Now, "Error ", null, project.Name, 2);
                        }));
                        processingDownLoad.Remove(project.Id);
                    }

                }
            });
        }

        private void FormHome_Load(object sender, EventArgs e)
        {

        }

        public void DisplayAccountProfile()
        {
            ovalPictureBox1.ImageLocation = Url.ServerURI + "/" + Properties.Settings.Default.Avatar;
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