using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WandSyncFile.Data.Mapping
{
    public class AccountDto
    {
        public accessToken accessToken { get; set; }
    }

    public class accessToken
    {
        public string token { get; set; }
    }

    public class RefreshTokenResponse
    {
        public string accessToken { get; set; }
        public string refreshToken { get; set; }
    }
}
