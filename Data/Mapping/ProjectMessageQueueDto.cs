using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WandSyncFile.Data.Mapping
{
    public class ProjectMessageQueueDto
    {
        public string MessageId { get; set; }
        public string ProjectPath { get; set; }
        public string FilePath { get; set; }
        public string UserId { get; set; }
        public string UserProjectPath { get; set; }
        public string ProjectSamplePath { get; set; }
    }
}
