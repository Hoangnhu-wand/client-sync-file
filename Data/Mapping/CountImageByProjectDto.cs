using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WandSyncFile.Data.Mapping
{

    public class CountImageByProjectDto
    {
        public string Folder { get; set; }
        public List<CountImageContentDto> Content { get; set; }
        public int Status { get; set; }
    }
    public class CountImageContentDto
    {
        public string Type { get; set; } 
        public int Number { get; set; }

    }
}
