using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Discord_UncrateGO_SkinCasesGenerator
{
    /// <summary>
    /// Handles fetching html data from internet
    /// </summary>
    class HtmlFetcher
    {
        private readonly string _errorPage;

        public HtmlFetcher(string errorPage)
        {
            _errorPage = errorPage;
        }

        public HtmlFetcher()
        {
            _errorPage = null;
        }

        /// <summary>
        /// Fetches from the specified URL with an ascending index
        /// </summary>
        public async Task<List<string>> FetchAscending(string baseUrl, int delay = 1000, int stopIndex = 0)
        {
            List<string> fetchedPages = new List<string>();
            bool fetchCases = true;
            int index = 1;
            while (fetchCases)
            {
                string url = baseUrl + index;

                string result = await RetrieveFromUrl(url);

                //If stop index is not set and the result is a 404 then stop fetching
                if (_errorPage != null && stopIndex == 0 && result.Contains(_errorPage)) fetchCases = false;
                if (stopIndex != 0 && index >= stopIndex) fetchCases = false;

                Logger.Log($"Fetched site index {index}", Logger.LogLevel.Debug);

                //Add page to pages
                fetchedPages.Add(result);
                index++;

                
                await Task.Delay(delay);
            }

            return fetchedPages;
        }

        /// <summary>
        /// Fetches from specified URL with specified list of strings
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="urlFillers"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static async Task<List<string>> FetchUrlFillers(string baseUrl, List<string> urlFillers, int delay = 1000)
        {
            List<string> resultList = new List<string>();
            foreach (var urlFiller in urlFillers)
            {
                string result = await RetrieveFromUrl(baseUrl + urlFiller);

                resultList.Add(result);

                Logger.Log($"Fetched site with replacement {urlFiller}", Logger.LogLevel.Debug);

                await Task.Delay(delay);
            }

            return resultList;
        }

        public static async Task<List<string>> FetchUrls(List<string> urList, int delay = 1000)
        {
            List<string> resultList = new List<string>();
            foreach (var url in urList)
            {
                string result = await RetrieveFromUrl(url);
                Logger.Log($"Fetched site at URL {url}", Logger.LogLevel.Debug);

                resultList.Add(result);

                await Task.Delay(delay);
            }

            return resultList;
        }

        private static async Task<string> RetrieveFromUrl(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                using (HttpResponseMessage response = await client.GetAsync(url))
                using (HttpContent content = response.Content)
                {
                    string result = await content.ReadAsStringAsync();

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
