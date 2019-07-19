using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace bundlephobia
{
    public class SlackRequest
    {
        public string text { get; set; }
        public string response_url { get; set; }
    }
    public static class Main
    {
        static HttpClient Client = new HttpClient();
        [FunctionName("Main")]
        public static async Task Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] SlackRequest req,
            HttpResponse response,
            ILogger log)
        {
            response.StatusCode = 200;
            response.Clear();
            try
            {
                var resp = await Client.GetAsync($"https://bundlephobia.com/api/package-history?package=${req.text}");
                var respData = JsonConvert.DeserializeObject<Dictionary<string, object>>(await resp.Content.ReadAsStringAsync());
                var version = respData.Keys.OrderByDescending(a => a).First();
                await Client.PostAsJsonAsync(req.response_url, new { text = $"https://bundlephobia.com/result?p={req.text}@${version}" });
            }
            catch (Exception e)
            {
                await Client.PostAsJsonAsync(req.response_url, new { text = $"Something went wrong" });
            }

        }
    }
}
