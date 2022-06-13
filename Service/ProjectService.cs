using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using WandSyncFile.Data.Mapping;
using WandSyncFile.Helpers;
using WandSyncFile.Helpers;

namespace WandSyncFile.Service
{
    public class ProjectService
    {
        public string GetProjectPath(string rootProjectPath, string path)
        {
            var pathArr = (new FileInfo(path).Directory.FullName).Split(new string[] { rootProjectPath }, StringSplitOptions.None);
            if (pathArr.Length > 1)
            {
                var projectPathArr = pathArr[pathArr.Length - 1].Trim().Split(new string[] { "\\" }, StringSplitOptions.None);
                var listProjectPath = projectPathArr.Where(item => !string.IsNullOrEmpty(item)).ToList();
                var projectName = listProjectPath.First();

                return Path.Combine(rootProjectPath, projectName);
            }
            return null;
        }
        
        public string GetProjectName(string path)
        {
            var rootProjectPath = Properties.Settings.Default.ProjectLocalPath;
            var pathArr = (new FileInfo(path).Directory.FullName).Split(new string[] { rootProjectPath }, StringSplitOptions.None);
            if(pathArr.Length <= 0)
            {
                return null;
            }
            var projectPathArr = pathArr[pathArr.Length - 1].Trim().Split(new string[] { "\\" }, StringSplitOptions.None);
            var listProjectPath = projectPathArr.Where(item => !string.IsNullOrEmpty(item)).ToList();
            if (listProjectPath.Count > 0)
            {
                return listProjectPath.First();
            }
            return null;
        }

        public string GetProjectDoPath(string rootProjectPath, string filePath)
        {
            var projectPath = this.GetProjectPath(rootProjectPath, filePath);
            var projectDoPath = Path.Combine(projectPath, Options.PROJECT_DO_NAME);

            return projectDoPath;
        }

        public string GetProjectSamplePath(string rootProjectPath, string filePath)
        {
            var projectPath = this.GetProjectPath(rootProjectPath, filePath);
            var projectSamplePath = Path.Combine(projectPath, Options.PROJECT_SAMPLE_NAME);

            return projectSamplePath;
        }

        public Project CopyFolderProject(Project project)
        {
            try
            {
                var projectLocalPath = Path.Combine(Properties.Settings.Default.ProjectLocalPath, project.Name);
                var projectLocalPathDo = Path.Combine(projectLocalPath, Options.PROJECT_DO_NAME);
                var projectLocalPathSample = Path.Combine(projectLocalPath, Options.PROJECT_SAMPLE_NAME);
                var projectLocalPathDone = Path.Combine(projectLocalPath, Options.PROJECT_DONE_NAME);

                if(!Directory.Exists(projectLocalPathSample))
                {
                    FileHelpers.CreateFolder(projectLocalPathSample);
                }
                
                if(!Directory.Exists(projectLocalPathDone))
                {
                    FileHelpers.CreateFolder(projectLocalPathDone);
                }

                if(!string.IsNullOrEmpty(project.LocalPathSample))
                {
                    FileHelpers.CopyFolderProject(project.LocalPathSample, projectLocalPathSample, null);
                }
                
                FileHelpers.CopyFolderProject(project.LocalPath, projectLocalPathDo, project.LocalPathSample);

                project.LocalPath = projectLocalPath;
                project.LocalPathSample = projectLocalPathSample;

                return project;
            } catch(Exception e)
            {
                throw new Exception(e.Message, e);
            }
        }

        public ProjectResult RequestGetProjectByName(string name)
        {
            try
            {
                var standardizedName = StringHelpers.GetStandardizedName(name); 
                var url = Url.GetProject + $"?name={standardizedName}";
                var data = HttpRequest.GetAsync(url);
                var project = JsonConvert.DeserializeObject<ProjectRequestDto>(data);
                return project.Result;

            } catch(Exception e)
            {
                throw new Exception(e.Message, e);
            }
            
        }
    }
}
