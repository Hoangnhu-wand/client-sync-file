using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WandSyncFile.Data.Mapping
{

    public class DodgeAndBurnDto
    {
        public string base64_dodge { get; set; } 
        public string base64_burn { get; set; }

    }
}
