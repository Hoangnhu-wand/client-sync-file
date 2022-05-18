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

namespace WandSyncFile
{
    public partial class FormHome : Form
    {
        HubConnection connection;
        ProjectService projectService;
        CancellationToken cancellationToken;
        CancellationToken cancellationTokenRemoveProject;
        DisplayFolder displayFolder;
        public List<int> processingProject = new List<int>();
        public List<string> processingUploadProject = new List<string>();
        public List<string> processingUploadFixProject = new List<string>();

        public FormHome()
        {
            InitializeComponent();
            setupAutoRun();

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
            if (reg.GetValue("Wand-Developed") == null)
            {
                reg.SetValue("Wand-Developed", Application.ExecutablePath.ToString());
            }
        }

        public void addItem(DateTime created, string action, string projectName, int status)
        {
            try
            {
                var listItem = new CustomListView();
                listItem.CreatedDate = created.ToString("dd/M/yyyy");
                listItem.CreateTime = created.ToString("hh:mm:ss");
                listItem.ProjectName = projectName;
                listItem.ButtonText = action;

                if (action == "Download Do")
                {
                    listItem.ButtonColor = Color.FromArgb(178, 255, 212);
                }
                else if (action == "Upload Done" || action == "Upload Fix")
                {
                    listItem.ButtonColor = Color.FromArgb(255, 219, 150);
                } 
                else if(action == "Remove All")
                {
                    listItem.ButtonColor = Color.FromArgb(255, 219, 150);
                }

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
                Console.WriteLine(e.Message);
            }
        }

        public void addItem(DateTime created, string action, bool isConnected)
        {
            try
            {
                var listItem = new CustomViewConnectHrm1();
                listItem.CreatedDate = created.ToString("dd/mm/yyyy");
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
                Console.WriteLine(e.Message);
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

            }
        }

        public void SyncFix(string projectName, string projectPath)
        {
            var projectLocalPath = Properties.Settings.Default.ProjectLocalPath;
            var localProject = Path.Combine(projectLocalPath, projectName);
            var editorUserName = Properties.Settings.Default.Username;

            var projectDirectoties = Directory.GetDirectories(localProject).ToList();
            var localFixFolderLast = projectDirectoties.Where(item => Path.GetFileName(item).Trim().StartsWith(Options.PROJECT_FIX_PATH_NAME)).OrderByDescending(item => Path.GetFileName(item)).FirstOrDefault();

            if (localFixFolderLast == null)
            {
                return;
            }

            var folderFixName = Path.GetFileName(localFixFolderLast);
            var serverFolderFix = Path.Combine(projectPath, folderFixName);

            var isSyncFix = displayFolder.CheckFolderFixSync(localFixFolderLast, serverFolderFix, localFixFolderLast);

            if (!isSyncFix && !processingUploadFixProject.Any(pName => pName == projectName))
            {
                var project = projectService.RequestGetProjectByName(projectName);

                if (project == null || (project != null && (project.StatusId == (int)PROJECT_STATUS.CHECKED || project.StatusId == (int)PROJECT_STATUS.COMPLETED)))
                {
                    return;
                }

                Invoke((Action)(async () =>
                {
                    addItem(DateTime.Now, "Upload Fix", projectName, 0);
                }));

                processingUploadFixProject.Add(projectName);

                FileHelpers.CopyDirectoryToServer(localFixFolderLast, serverFolderFix);

                displayFolder.CheckFolderSync(localFixFolderLast, serverFolderFix, localFixFolderLast);

                Invoke((Action)(async () =>
                {
                    addItem(DateTime.Now, "Upload Fix", projectName, 1);
                }));
                processingUploadFixProject.Remove(projectName);
            }
        }

        public void SyncDo(string projectName, string projectPath)
        {
            var projectLocalPath = Properties.Settings.Default.ProjectLocalPath;
            var localProject = Path.Combine(projectLocalPath, projectName);
            var editorUserName = Properties.Settings.Default.Username;

            var localProjectDoPath = Path.Combine(localProject, Options.PROJECT_DO_NAME);
            var localEditorDoPath = Path.Combine(localProjectDoPath, editorUserName);

            var serverDoPath = Path.Combine(projectPath, Options.PROJECT_DO_NAME);
            var serverEditorDoPath = Path.Combine(serverDoPath, editorUserName);
            var isSyncDo = displayFolder.CheckFolderSyncCompleted(localEditorDoPath, serverEditorDoPath);
            if (isSyncDo)
            {
                return;
            }

            var project = projectService.RequestGetProjectByName(projectName);

            if (project == null || (project != null && (project.StatusId == (int)PROJECT_STATUS.CHECKED || project.StatusId == (int)PROJECT_STATUS.COMPLETED)))
            {
                return;
            }

            if (!isSyncDo && !processingProject.Any(pId => pId == project.Id))
            {
                displayFolder.CheckFolderSync(localEditorDoPath, serverEditorDoPath, localProjectDoPath);
                Invoke((Action)(async () =>
                {
                    addItem(DateTime.Now, "Download Do", projectName, 0);
                }));

                processingProject.Add(project.Id);

                FileHelpers.DownloadFolderFromServer(serverEditorDoPath, localEditorDoPath , null, true);

                // download folder Working - với dự án khác needFix
                if(project.StatusId != (int)PROJECT_STATUS.NEEDFIX)
                {
                    var localFolderWorking = Path.Combine(localProject, Options.PROJECT_WORKING_PATH_NAME);
                    var editorFolderWorking = Path.Combine(localFolderWorking, editorUserName);

                    displayFolder.CheckFolderSync(editorFolderWorking, localEditorDoPath, editorFolderWorking);

                    FileHelpers.DownloadFolder(localEditorDoPath, editorFolderWorking);

                    displayFolder.CheckFolderSync(editorFolderWorking, localEditorDoPath, editorFolderWorking);
                }

                Invoke((Action)(async () =>
                {
                    addItem(DateTime.Now, "Download Do", projectName, 1);
                }));

                displayFolder.CheckFolderSync(localEditorDoPath, serverEditorDoPath, localProjectDoPath);

                processingProject.Remove(project.Id);
            }
        }

        public void SyncDone(string projectName, string projectPath)
        {
            var projectLocalPath = Properties.Settings.Default.ProjectLocalPath;
            var editorUserName = Properties.Settings.Default.Username;

            var localProject = Path.Combine(projectLocalPath, projectName);
            var localProjectDonePath = Path.Combine(localProject, Options.PROJECT_DONE_NAME);
            var localEditorDonePath = Path.Combine(localProjectDonePath, editorUserName);


            var serverDonePath = Path.Combine(projectPath, Options.PROJECT_DONE_NAME);
            var serverEditorDonePath = Path.Combine(serverDonePath, editorUserName);
            var isSyncDone = displayFolder.CheckFolderSyncCompleted(localEditorDonePath, serverEditorDonePath);
            if (isSyncDone)
            {
                return;
            }

            var project = projectService.RequestGetProjectByName(projectName);

            if (project == null || (project != null && (project.StatusId == (int)PROJECT_STATUS.CHECKED || project.StatusId == (int)PROJECT_STATUS.COMPLETED)))
            {
                return;
            }

            if (!isSyncDone && !processingUploadProject.Any(pName => pName == projectName))
            {
                displayFolder.CheckFolderSync(localEditorDonePath, serverEditorDonePath, localProjectDonePath);

                Invoke((Action)(async () =>
                {
                    addItem(DateTime.Now, "Upload Done", projectName, 0);
                }));

                processingUploadProject.Add(projectName);

                FileHelpers.SyncDirectoryDoneToServer(localEditorDonePath, serverEditorDonePath);
                displayFolder.CheckFolderSync(localEditorDonePath, serverEditorDonePath, localProjectDonePath);

                Invoke((Action)(async () =>
                {
                    addItem(DateTime.Now, "Upload Done", projectName, 1);
                }));

                processingUploadProject.Remove(projectName);
            }
        }

        public void CopyDoneAndFixFromServer(string projectName, string projectPath, int projectId)
        {
            var projectLocalPath = Properties.Settings.Default.ProjectLocalPath;
            var editorUserName = Properties.Settings.Default.Username;
            var localProject = Path.Combine(projectLocalPath, projectName);

            if(!Directory.Exists(localProject))
            {
                Directory.CreateDirectory(localProject);
            }

            var folderProjectHasFile = FileHelpers.HasFileInFolder(localProject);
            if (folderProjectHasFile)
            {
                return;
            }

            // download do 
            var localDoPath = Path.Combine(localProject, Options.PROJECT_DO_NAME);
            var localEditorDoPath = Path.Combine(localDoPath, editorUserName);

            var serverDoPath = Path.Combine(projectPath, Options.PROJECT_DO_NAME);
            var serverEditorDoPath = Path.Combine(serverDoPath, editorUserName);

            Invoke((Action)(async () =>
            {
                addItem(DateTime.Now, "Download Do", projectName, 0);
            }));

            processingProject.Add(projectId);

            FileHelpers.DownloadFolderFromServer(serverEditorDoPath, localEditorDoPath);

            Invoke((Action)(async () =>
            {
                addItem(DateTime.Now, "Download Do Completed", projectName, 0);
            }));

            processingProject.Remove(projectId);

            // download done
            var localProjectDonePath = Path.Combine(localProject, Options.PROJECT_DONE_NAME);
            var localEditorDonePath = Path.Combine(localProjectDonePath, editorUserName);

            // Neu Done đã có ảnh -> Không tải nữa
            FileHelpers.CreateFolder(localEditorDonePath);
            var folderDoneHasFile = FileHelpers.HasFileInFolder(localEditorDonePath);
            if (folderDoneHasFile)
            {
                return;
            }

            Invoke((Action)(async () =>
            {
                addItem(DateTime.Now, "Download Done", projectName, 0);
            }));

            // download done
            var serverDonePath = Path.Combine(projectPath, Options.PROJECT_DONE_NAME);
            var serverEditorDonePath = Path.Combine(serverDonePath, editorUserName);

            FileHelpers.DownloadFolderFromServer(serverEditorDonePath, localEditorDonePath);

            Invoke((Action)(async () =>
            {
                addItem(DateTime.Now, "Download Done", projectName, 1);
            }));

            // download folder Working from done
            var localFolderWorking = Path.Combine(localProject, Options.PROJECT_WORKING_PATH_NAME);
            var editorFolderWorking = Path.Combine(localFolderWorking, editorUserName);

            FileHelpers.CreateFolder(editorFolderWorking);

            var checkWorking = displayFolder.CheckFolderSync(editorFolderWorking, localEditorDonePath, editorFolderWorking);
            if (!checkWorking)
            {
                FileHelpers.DownloadFolder(localEditorDonePath, editorFolderWorking);
            }

            // download fix
            var allFolderFix = FileHelpers.ServerGetListFix(projectPath);
            if (allFolderFix == null)
            {
                return;
            }

            Invoke((Action)(async () =>
            {
                addItem(DateTime.Now, "Download Fix", projectName, 0);
            }));

            foreach (var folderFixItem in allFolderFix)
            {
                var folderFixName = FileHelpers.ServerGetFolderName(folderFixItem);
                var localEditorFixPath = Path.Combine(localProject, folderFixName);

                // chỉ lấy các file fix có trong Done

                var allFileDoneName = FileHelpers.GetListFileNameByFolder(serverEditorDonePath);
                var allFixByDoneName = FileHelpers.ServerGetListFixPathByDoneName(allFileDoneName, folderFixItem);

                foreach (var fixItem in allFixByDoneName)
                {
                    var clientPath = Path.Combine(localEditorFixPath, Path.GetFileName(fixItem));
                    FileHelpers.CopyFileFromServer(fixItem, clientPath);
                }

                // copy fix to working
                FileHelpers.DownloadFolder(localEditorFixPath, editorFolderWorking);
            }

            Invoke((Action)(async () =>
            {
                addItem(DateTime.Now, "Download Fix", projectName, 1);
            }));
        }

        public async void HandleHubConnection()
        {
            string connectedServer = DateTime.Now.ToString();

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

            var userId = Properties.Settings.Default.Id;
            var userName = Properties.Settings.Default.Username;
            var localPath = Properties.Settings.Default.ProjectLocalPath;

            connection.On<string, string, string>("SERVER_QUEUE_MESSAGE", async (user, action, data) =>
            {
                if (action == "EDITOR_DOWNLOAD_FILE" && user == userId.ToString() && UserRoleHelpers.IsEditors())
                {
                    Task.Run(async () =>
                    {
                        var editorDownloadItem = JsonConvert.DeserializeObject<EditorDownloadFileProjectDto>(data);

                        if (editorDownloadItem == null || !FileHelpers.ExitServerPath(editorDownloadItem.ProjectPath))
                        {
                            return;
                        }
                        
                        var projectPath = editorDownloadItem.ProjectPath.Trim();
                        var projectName = editorDownloadItem.ProjectName.Trim();
                        
                        var editorLocalPathDo = FileHelpers.GetEditorProjectDoLocalPath(projectName);
                        var localProjectPath = FileHelpers.GetEditorProjectLocalPath(projectName);
                        var localProjectDoPath = Path.Combine(localProjectPath, Options.PROJECT_DO_NAME);

                        FileHelpers.CreateFolder(localProjectDoPath);

                        FileHelpers.CreateFolder(editorLocalPathDo);
                        FileHelpers.AddFileLogProjectPath(projectName, projectPath);

                        // Tạo thư mục Done
                        var editorLocalDonepath = FileHelpers.GetEditorProjectDoneLocalPath(projectName);
                        FileHelpers.CreateFolder(editorLocalDonepath);

                        // check sync do path
                        SyncDo(projectName, projectPath);

                        var editorServerPathDo = FileHelpers.GetEditorProjectDoServerPath(projectPath);
                        var editorLocalProjectPath = Path.Combine(Properties.Settings.Default.ProjectLocalPath, projectName);

                        var localSamplePath = FileHelpers.GetEditorProjectSampleLocalPath(projectName);
                        var projectPathSample = Path.Combine(projectPath, Options.PROJECT_SAMPLE_NAME);

                        if (Directory.Exists(projectPathSample))
                        {
                            var sampleAlreadySync = displayFolder.CheckFolderSync(localSamplePath, projectPathSample);

                            if (!sampleAlreadySync)
                            {
                                FileHelpers.DownloadFolderFromServer(projectPathSample, localSamplePath, null, true);
                            }
                        }

                        // create folder Done by server Done

                        FileHelpers.EditorCreateFolderDonePath(projectPath, projectName);

                        displayFolder.CheckFolderSync(localSamplePath, projectPathSample);
                    });
                }

                if (action == "EDITOR_CREATE_FOLDER_FIX" && UserRoleHelpers.IsEditors() && user == userId.ToString())
                {
                    Task.Run(async () =>
                    {

                        var editorDownloadItem = JsonConvert.DeserializeObject<EditorDownloadFileProjectDto>(data);
                        if (editorDownloadItem == null)
                        {
                            return;
                        }

                        var folderFix = editorDownloadItem.FilePath;
                        var serverFileArr = Path.GetFullPath(folderFix).Split(new string[] { Path.GetFullPath(editorDownloadItem.ProjectPath) }, StringSplitOptions.None).Where(item => !string.IsNullOrEmpty(item)).ToList();
                        if (serverFileArr.Count() <= 0)
                        {
                            return;
                        }

                        var localFolderFix = FileHelpers.GetEditorProjectLocalPath(editorDownloadItem.ProjectName) + serverFileArr.Last();
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
                                addItem(DateTime.Now, "Remove All", projectName, 0);
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
                                        addItem(DateTime.Now, "Remove All", projectName, 1);
                                    }));
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.Message);
                                    Invoke((Action)(() =>
                                    {
                                        addItem(DateTime.Now, "Remove All", projectName, 2);
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

        private void ovalPictureBox1_Click(object sender, EventArgs e)
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

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
