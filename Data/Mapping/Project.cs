using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WandSyncFile.Data.Mapping
{
    public class Project
    {
        public int Id { get; set; }
        public int TeamId { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public string LocalPath { get; set; }
        public int CreatorId { get; set; }
        public int SalerId { get; set; }
        public string PathDo { get; set; }
        public string LocalPathSample { get; set; }
        public string PathSample { get; set; }
    }
}
