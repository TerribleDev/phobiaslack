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
    class SlackPost
    {
        public string text { get; set; }
        public bool unfurl_links { get; set; } = true;
        public bool unfurl_media { get; set; } = true;
        public string response_type { get; set; } = "in_channel";

    }
    public static class Main
    {
        static HttpClient Client = new HttpClient();
        [FunctionName("Main")]
        public static async Task Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest httpReq,
            ILogger log)
        {

            var text = httpReq.Form["text"];
            var responseUrl = httpReq.Form["response_url"];
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentNullException(nameof(text));
            }
            if (string.IsNullOrWhiteSpace(responseUrl))
            {
                throw new ArgumentNullException(nameof(text));
            }
            httpReq.HttpContext.Response.Clear();
            try
            {
                var resp = await Client.GetAsync($"https://bundlephobia.com/api/package-history?package={text}");
                var respData = JsonConvert.DeserializeObject<Dictionary<string, object>>(await resp.Content.ReadAsStringAsync());
                var version = respData.Keys.OrderByDescending(a => a).First();
                var payload = new SlackPost()
                {
                    text = $"https://bundlephobia.com/result?p={text}@{version}",
                };
                await Client.PostAsJsonAsync(responseUrl, payload);
            }
            catch (Exception e)
            {
                await Client.PostAsJsonAsync(responseUrl, new { text = $"😢 Something went wrong" });
                log.LogError(e, "error");
            }

        }
    }
}
