using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Discord_UncrateGO_SkinCasesGenerator
{
    /// <summary>
    /// Handles fetching html data from internet
    /// </summary>
    internal static class HtmlFetcher
    {
        public static async Task<string> RetrieveFromUrl(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                using (HttpResponseMessage response = await client.GetAsync(url))
                using (HttpContent content = response.Content)
                {
                    string result = await content.ReadAsStringAsync();

                    Logger.Log($"Fetched site at URL {url}", Logger.LogLevel.Debug);

                    return result;
                }
            }
            catch (Exception)
            {
                Logger.Log("Failed to retrieve from url | " + url, Logger.LogLevel.Error);
                return "";
            }
       
        }
    }
}
