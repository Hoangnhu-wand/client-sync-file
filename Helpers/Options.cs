﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WandSyncFile.Helpers
{
    public static class Options
    {
        public static string SERVER_FILE_105 = @"192.168.1.105";
        public static string FILE_DELETE = "DELETE";
        public static string FILE_RENAME = "RENAME";
        public static string FILE_CREATE = "CREATE";

        public static string SEVER_USERNAME105 = @"Administrator";
        public static string SERVER_PASSWORD105 = "Wand12345";
        public static string SERVER_FILE105 = @"\\192.168.1.105";

        // dropbox
        public static string SERVER_USERNAME101 = @"leader";
        public static string SERVER_PASSWORD101 = "wand@ld1234";
        public static string SERVER_FILE101 = @"\\192.168.1.101";

        public static string PROJECT_PATH_FILE_NAME = "Path";
        public static string PROJECT_FILE_NAME = "Name";

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
    }
}