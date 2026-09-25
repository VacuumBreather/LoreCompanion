using System.Net.Http;
using System.Net.Http.Headers;
using LoreCompanion.Utilities;

namespace LoreCompanion.Extensions
{
    public static class HttpClientExtensions
    {
        public static void Configure(this HttpClient client)
        {
            client.Timeout = TimeSpan.FromSeconds(30);

            client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("LoreCompanion", AppHelper.CurrentVersion.ToString()));
        }
    }
}