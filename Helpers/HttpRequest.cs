using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WandSyncFile.Helpers
{
    public static class HttpRequest
    {
        public static string PostAsync(string url, dynamic data)
        {
            var token = Properties.Settings.Default.Token;
            WebClient clients = new WebClient();
            clients.Headers[HttpRequestHeader.ContentType] = "application/json";
            
            if (!string.IsNullOrEmpty(token))
            {
                clients.Headers.Add("Authorization", "Bearer " + Properties.Settings.Default.Token);
            }

            var result = clients.UploadString(url, "POST", JsonSerializer.Serialize(data));

            return result;
        }
        
        public static void PutAsync(string url, dynamic data)
        {
            var token = Properties.Settings.Default.Token;
            WebClient clients = new WebClient();
            clients.Headers[HttpRequestHeader.ContentType] = "application/json";
            
            if (!string.IsNullOrEmpty(token))
            {
                clients.Headers.Add("Authorization", "Bearer " + Properties.Settings.Default.Token);
            }

            clients.UploadString(url, "PUT", JsonSerializer.Serialize(data));
        }
        
        public static string GetAsync(string url)
        {
            var token = Properties.Settings.Default.Token;
            WebClient clients = new WebClient();
            clients.Headers[HttpRequestHeader.ContentType] = "application/json";
            
            if (!string.IsNullOrEmpty(token))
            {
                clients.Headers.Add("Authorization", "Bearer " + Properties.Settings.Default.Token);
            }

            return clients.DownloadString(url);
        }
    }
}
