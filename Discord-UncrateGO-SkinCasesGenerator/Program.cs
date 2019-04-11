using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Discord_UncrateGO_SkinCasesGenerator
{
    internal class Program
    {
        private static readonly List<string> GlobalKnifeList = new List<string>
        {
            "Navaja+Knife",
            "Stiletto+Knife",
            "Talon+Knife",
            "Ursus+Knife",
            "Bayonet",
            "Bowie+Knife",
            "Butterfly+Knife",
            "Falchion+Knife",
            "Flip+Knife",
            "Gut+Knife",
            "Huntsman+Knife",
            "Karambit",
            "M9+Bayonet",
            "Shadow+Daggers"
        };

        static async Task Main(string[] args)
        {
//            Dictionary<string, HtmlParser.CaseData> caseResult = await HtmlParser.ParseCases(); //Index by lowercase case name
//            Dictionary<string, List<string>> knifeResult = await HtmlParser.ParseKnives(GlobalKnifeList);
//            
//            //Sort the items from knife result into the distinctive caseResult cases
//            Logger.Log("Sorting knife data into cases...");
//            foreach (KeyValuePair<string,List<string>> knife in knifeResult)
//            {
//                foreach (string knifeCases in knife.Value) //Iterate through each knife's cases and add it to the master case list
//                {
//                    if (caseResult.TryGetValue(knifeCases, out _)) //Ensure the case exists before trying to add to it
//                    {
//                        caseResult[knifeCases].CaseItems.Add(knife.Key);
//                    } 
//                }
//            }

            //TODO fetch all collections
            var result = await HtmlParser.ParseSouvenirs();
            //TODO generate a dictionary containing all the collections and whether they are souvenirs
            
            
            Logger.Log("Writing to file...");
//            Logger.LogToFile(JsonConvert.SerializeObject(caseResult), @"path");

            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
        }
    }
}
