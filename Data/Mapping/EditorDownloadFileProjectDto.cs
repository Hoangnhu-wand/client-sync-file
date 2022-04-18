using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WandSyncFile.Data.Mapping
{
    public class EditorDownloadFileProjectDto
    {
        public string MessageId { get; set; }
        public int ProjectId { get; set; }
        public string ProjectPath { get; set; }
        public string FilePath { get; set; }
        public string ProjectName { get; set; }
        public int UserId { get; set; }
    }
}
