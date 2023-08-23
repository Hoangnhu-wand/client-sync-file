using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WandSyncFile.Constants
{
    public static class Url
    {

        public static string ServerURI = "http://localhost:6688";
        public static string ServerFrequency = "http://172.16.0.20:6696";
        public static string ServerBlender = "http://localhost:6789";


        public static string Login = $"{ServerURI}/api/auth/login";
        public static string GetAccount = $"{ServerURI}/api/accounts";
        public static string RefreshToken = $"{ServerURI}/api/auth/refreshtoken";

        public static string GetDirectories = $"{ServerURI}/api/files/send-directories";
        public static string SyncFileStatus = ServerURI + "/api/projects/{id}/sync-file-status";
        public static string DowloadFileStatus = ServerURI + "/api/projects/{id}/download-file-status";

        public static string UpdateProjectPath = ServerURI + "/api/projects/{id}/update-path";

        public static string GetProject = $"{ServerURI}/api/projects/by-name";

        public static string GetBase64Guidance = $"{ServerFrequency}/api/v1/guidance";
        public static string GetBase64DodgeAndBurn = $"{ServerFrequency}/api/v1/dodge-and-burn";
        public static string GetBase64Frequency = $"{ServerBlender}/blend";
    }
}
