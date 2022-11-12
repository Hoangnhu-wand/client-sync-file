using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WandSyncFile.Constants
{
    public static class Options
    {
        public static string FILE_DELETE = "DELETE";
        public static string FILE_RENAME = "RENAME";
        public static string FILE_CREATE = "CREATE";

        // Server 172.16.0.5
        public static string SERVER_FILE_05 = @"172.16.0.5";
        public static string SERVER_USERNAME_05 = @"leader";
        public static string SERVER_PASSWORD_05 = "wand@ld1";

        // Server 172.16.0.6
        public static string SERVER_FILE_06 = @"172.16.0.6";
        public static string SERVER_USERNAME_06 = @"leader";
        public static string SERVER_PASSWORD_06 = "wand@ld1234";

        // Server 172.16.0.7
        public static string SERVER_FILE_07 = @"172.16.0.7";
        public static string SERVER_USERNAME_07 = @"Administrator";
        public static string SERVER_PASSWORD_07 = "Wand12345";

        // Server 172.16.0.8
        public static string SERVER_FILE_08 = @"172.16.0.8";
        public static string SERVER_USERNAME_08 = @"hrm";
        public static string SERVER_PASSWORD_08 = "wand@hrm1";

        // Server 172.16.0.9
        public static string SERVER_FILE_09 = @"172.16.0.9";
        public static string SERVER_USERNAME_09 = @"hrm";
        public static string SERVER_PASSWORD_09 = "wand@hrm1";

        public static string PROJECT_PATH_FILE_NAME = "Path";
        public static string PROJECT_FILE_NAME = "Name";
        public static string PROJECT_LOG_NAME = "Log";

        public static string PROJECT_DO_NAME = "Do";
        public static string PROJECT_WORKING_PATH_NAME = "Working";
        public static string PROJECT_SAMPLE_NAME = "Sample";
        public static string PROJECT_DONE_NAME = "Done";
        public static string PROJECT_FIX_PATH_NAME = "Fix_";

        public static string ROLE_SALES = "Salers";
        public static string ROLE_CUSTOMER_SUPPORT = "Customer Support";
        public static string ROLE_MARKETING = "Marketing";
        public static string ROLE_LEADER = "Leaders";
        public static string ROLE_SUBLEADER = "Sub-leader";
        public static string ROLE_ADMIN = "Administrators";
        public static string ROLE_EDITOR = "Editors";

        // TimeSpan
        public static int TIME_SPAN_READ_FILE_CHANGE = 10;
        public static int TIME_SPAN_REMOVE_COMPLETED_PROJECT = 10;

        public static readonly List<string> PROJECT_IMAGE_FILE_TYPE_JPG = new List<string> { "JPG", "JPEG" };

        
    }
}