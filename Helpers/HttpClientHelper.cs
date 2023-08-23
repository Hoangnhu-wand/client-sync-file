using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WandSyncFile.Helpers
{
    public static class HttpClientHelper
    {
        public static string callAPIGuidance(string url, dynamic data)
        {
            var base64_guidacne = "";
            using (var client = new HttpClient())
            {
                MultipartFormDataContent content = new MultipartFormDataContent();
                content.Add(new StringContent(data), "image_base64");

                try
                {
                   HttpResponseMessage response = client.PostAsync(url, content).GetAwaiter().GetResult();
                    base64_guidacne = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                  
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            };

            return base64_guidacne;
        }

        public static string callAPIDodgeAndBurn(string url, dynamic image_base64, dynamic guidance_base64)
        {
            var base64_dodge_burn = "";
            using (var client = new HttpClient())
            {
                MultipartFormDataContent content = new MultipartFormDataContent();
                content.Add(new StringContent(image_base64), "base64_image");
                content.Add(new StringContent(guidance_base64), "base64_guidance");
                try
                {
                    HttpResponseMessage response = client.PostAsync(url, content).GetAwaiter().GetResult();
                    base64_dodge_burn = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
              
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            };

            return base64_dodge_burn;
        }

        public static string exportBase64(string url, dynamic image_base64, dynamic base64_dodge_mask, dynamic base64_burn_mask, double dodge_level, double burn_level, double blend_level)
        {
            var base64_export = "";
            var number_dodge = dodge_level.ToString();
            var number_burn = burn_level.ToString();
            var number_blend = blend_level.ToString();
            using (var client = new HttpClient())
            {
                MultipartFormDataContent content = new MultipartFormDataContent();
                content.Add(new StringContent(image_base64), "base64_image");
                content.Add(new StringContent(base64_dodge_mask), "base64_dodge_mask");
                content.Add(new StringContent(base64_burn_mask), "base64_burn_mask");
                content.Add(new StringContent(number_dodge), "dodge_level");
                content.Add(new StringContent(number_burn), "burn_level");
                content.Add(new StringContent(number_blend), "blend_level");
                try
                {
                    HttpResponseMessage response = client.PostAsync(url, content).GetAwaiter().GetResult();
                    base64_export = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            };

            return base64_export;
        }
    }
}
