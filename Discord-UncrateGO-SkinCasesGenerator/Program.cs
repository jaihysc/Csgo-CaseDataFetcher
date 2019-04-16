using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Discord_UncrateGO_SkinCasesGenerator
{
    internal static class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("File output folder location: ");
            string writePath = Console.ReadLine();
            writePath += "/CsgoCosmeticData.json";
            
            HtmlParser.CsgoItemData result = await HtmlParser.FetchCaseItemData();
            
            Logger.Log("Writing to " + writePath);
            Logger.LogToFile(JsonConvert.SerializeObject(result), writePath);

            Console.WriteLine("DONE -- Press ENTER to exit");
            Console.ReadLine();
        }
    }
}
