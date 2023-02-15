using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WandSyncFile.Data.Mapping
{
    public class ProjectRequestDto
    {
        [JsonPropertyName("result")]
        public ProjectResult Result { get; set; }
    }

    public class ProjectResult
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("statusId")]
        public int StatusId { get; set; }
    }
}
