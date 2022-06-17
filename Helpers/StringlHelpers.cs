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

namespace WandSyncFile.Helpers
{
    public static class StringHelpers
    {
        public static string GetStandardizedName(string name)
        {
            return name.Replace("#", "%23").Replace("&", "%26");
        }
    }
}
