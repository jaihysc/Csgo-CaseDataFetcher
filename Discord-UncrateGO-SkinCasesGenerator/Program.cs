using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Discord_UncrateGO_SkinCasesGenerator
{
    internal static class Program
    {
        static async Task Main(string[] args)
        {
            var result = await HtmlParser.FetchCaseItemData();
            
            Logger.Log("Writing to file...");
            Logger.LogToFile(JsonConvert.SerializeObject(result), @"path");

            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
        }
    }
}
