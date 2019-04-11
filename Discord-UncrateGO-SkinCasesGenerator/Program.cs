using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Discord_UncrateGO_SkinCasesGenerator
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var result = await HtmlParser.FetchCaseItemData();
            
            Logger.Log("Writing to file...");
//          Logger.LogToFile(JsonConvert.SerializeObject(caseResult), @"path");

            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
        }
    }
}
