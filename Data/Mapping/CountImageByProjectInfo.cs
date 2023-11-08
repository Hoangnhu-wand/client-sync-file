using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WandSyncFile.Data.Mapping
{

    public class CountImageByProjectInfo
    {
        public string ProjectName { get; set; } 
        public string ProjectPath { get; set; }
        public string UserName { get; set; }

    }
}
