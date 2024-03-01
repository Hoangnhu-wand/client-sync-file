using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WandSyncFile.Data.Mapping
{
    internal class DownloadProjectInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public int Status { get; set; }
        public bool DownloadAll { get; set; }
    }
}
