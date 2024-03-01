using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WandSyncFile.Data.Mapping
{


    public class VPNConnectedDto
    {
        public string UserName { get; set; }
        public int UserId { get; set; }
        public bool Status { get; set; }
    }
}
