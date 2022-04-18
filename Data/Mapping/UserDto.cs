using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WandSyncFile.Data.Mapping
{
    public class HrmUserResponse
    {
        [JsonPropertyName("account")]
        public UserDto Account {get; set;}
    }

    public class UserDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("avatar")]
        public string Avatar { get; set; }

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [JsonPropertyName("identityId")]
        public string IdentityId { get; set; }

        [JsonPropertyName("userName")]
        public string UserName { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("departmentId")]
        public int? DepartmentId { get; set; }

        [JsonPropertyName("projectLocalPath")]
        public string ProjectLocalPath { get; set; }

        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; }

        [JsonPropertyName("roles")]
        public List<string> Roles { get; set; }
    }
}
