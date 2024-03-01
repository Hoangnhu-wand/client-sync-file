using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WandSyncFile.Data.Mapping
{
    public class AddonProjectRequestDto
    {
        [JsonPropertyName("result")]
        public AddonProjectResult Result { get; set; }
    }

    public class AddonProjectResult
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("statusId")]
        public int StatusId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }

        [JsonPropertyName("listEditors")]
        public List<EditorDto> ListEditors { get; set; }

        [JsonPropertyName("listImages")]
        public List<string> ListImages { get; set; }
    }

    public class EditorDto
    {
        public int Id { get; set; }
        public int EditorId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string FullName => String.Concat(LastName, " ", FirstName);
        public ProjectEditorStatus Status { get; set; }
    }

    public enum ProjectEditorStatus
    {
        [Display(Name = "Processing")]
        PROCESSING,

        [Display(Name = "Need check")]
        NEEDCHECK,

        [Display(Name = "Need fix")]
        NEEDFIX,

        [Display(Name = "Done")]
        DONE
    }
}
