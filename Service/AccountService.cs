using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WandSyncFile.Data.Mapping;
using WandSyncFile.Helpers;

namespace WandSyncFile.Service
{
    public class AccountService
    {
        public UserDto GetAccount(string accesstoken)
        {
            try
            {
                WebClient clients = new WebClient();
                clients.Headers[HttpRequestHeader.ContentType] = "application/json";
                clients.Headers.Add("Authorization", "Bearer " + accesstoken);

                var accountResponse = clients.DownloadString(Url.GetAccount);

                var accountData = JsonSerializer.Deserialize<HrmUserResponse>(accountResponse);
                var account = accountData.Account;

                return account;
            } catch(Exception e)
            {
                throw new Exception(e.Message, e);
            }
        }

        public void SettingAccount(string token, UserDto account)
        {
            Properties.Settings.Default.Username = account.UserName;
            Properties.Settings.Default.Token = token;
            Properties.Settings.Default.Id = account.Id;
            Properties.Settings.Default.Avatar = account.Avatar;
            Properties.Settings.Default.FirstName = account.FirstName;
            Properties.Settings.Default.LastName = account.LastName;
            Properties.Settings.Default.ProjectLocalPath = account.ProjectLocalPath;
            Properties.Settings.Default.RefreshToken = account.RefreshToken;
            Properties.Settings.Default.Roles = JsonSerializer.Serialize(account.Roles);

            Properties.Settings.Default.Save();
        }
    }
}
