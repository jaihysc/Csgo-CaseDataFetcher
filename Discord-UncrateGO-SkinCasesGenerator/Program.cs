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
            "Shadow+Daggers" //Commented out for testing with only 1 knife
        };

        static async Task Main()
        {
            //HtmlFetcher fetcher = new HtmlFetcher(@"Sorry, the page you are looking for could not be found.");
            //var returnList = await fetcher.FetchAscending("https://csgostash.com/case/");

            //var result = HtmlParser.ParseHtml(text, "//td/input");

            //var result = await HtmlParser.ParseCases();

            //var str = new List<string> {JsonConvert.SerializeObject(result)};

            var result = await HtmlParser.ParseKnives(GlobalKnifeList);
            Logger.LogToFile(JsonConvert.SerializeObject(result), @"path");
            //foreach (var s in result.Values)
            //{
            //    Console.WriteLine(s.CaseName);
            //    Console.WriteLine(s.CaseCollection);
            //    foreach (var item in s.CaseItems)
            //    {
            //        Console.WriteLine(item);
            //    }

            //    Console.WriteLine();
            //}

            Console.ReadLine();
        }
    }
}
