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

        [JsonPropertyName("teamId")]
        public int TeamId { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("localPath")]
        public string LocalPath { get; set; }

        [JsonPropertyName("creatorId")]
        public int CreatorId { get; set; }

        [JsonPropertyName("salerId")]
        public int SalerId { get; set; }

        [JsonPropertyName("pathDo")]
        public string PathDo { get; set; }

        [JsonPropertyName("localPathSample")]
        public string LocalPathSample { get; set; }

        [JsonPropertyName("pathSample")]
        public string PathSample { get; set; }

        [JsonPropertyName("statusId")]
        public int StatusId { get; set; }
    }
}
