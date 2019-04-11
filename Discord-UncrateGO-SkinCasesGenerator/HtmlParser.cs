using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace Discord_UncrateGO_SkinCasesGenerator
{
    class HtmlParser
    {
        public static async Task<CsgoItemData> FetchCaseItemData()
        {
            
            return null;
        }
        public static async Task<Dictionary<string, CaseData>> ParseCases()
        {
            HtmlFetcher htmlFetcher = new HtmlFetcher("Sorry, the page you are looking for could not be found.");
        
            //Fetch
            List<string> pages = await htmlFetcher.FetchAscending("https://csgostash.com/case/", 30, 300);

            //This for debug only
            //pages = string.Join("", pages).Split(new[] { @"<!DOCTYPE html>" }, StringSplitOptions.None).ToList();

            Dictionary<string, CaseData> csgoData = new Dictionary<string, CaseData>();
            //Parse
            int index = 1; //one based to represent the website numbering scheme
            foreach (string page in pages)
            {
                try
                {
                    //Leave if the page is empty
                    if (!string.IsNullOrWhiteSpace(page))
                    {

                        //Separate single string with line breaks into array
                        string[] lines = page.Split(
                            new[] { "\r\n", "\r", "\n" },
                            StringSplitOptions.None
                        );

                        //Filter to lines containing " | "
                        List<string> fLines = lines.Where(l => l.Contains(" | ")).ToList();

                        CaseData caseData = new CaseData
                        {
                            CaseItems = new List<string>()
                        };
                        //Select the text out of the html
                        foreach (var dataDuoLines in fLines)
                        {
                            try
                            {
                                HtmlDocument duoHtml = new HtmlDocument();
                                duoHtml.LoadHtml(dataDuoLines);

                                //Get the data out of the <a3> tags
                                var duoLineData = duoHtml.DocumentNode.SelectNodes("/h3/a").ToList()
                                    .Select(i => i.InnerHtml);

                                //Concat into item name
                                caseData.CaseItems.Add(string.Join(" | ", duoLineData)); //Weapon name | skin name
                            }
                            catch (Exception)
                            {
                                Logger.Log("Error parsing html item data | Index " + index, Logger.LogLevel.Error);
                            }

                        }

                        try
                        {
                            HtmlDocument htmlDoc = new HtmlDocument();
                            htmlDoc.LoadHtml(page);


                            //Get the data out of the <a3> tags
                            HtmlNode caseNodeData =
                                htmlDoc.DocumentNode.SelectSingleNode(
                                    "//div[@class='inline-middle collapsed-top-margin']"); //inline-middle collapsed-top-margin

                            string caseName = caseNodeData.SelectSingleNode("h1").InnerText;
                            string caseCollection = caseNodeData.SelectSingleNode("h4").InnerText;

                            //Set the data for the return class
                            caseData.CaseName = caseName;
                            caseData.CaseCollection = caseCollection;

                            //Add items for case as list in dictionary if it does not exist
                            if (!csgoData.TryGetValue(caseName.ToLower(), out _)) csgoData.Add(caseName, caseData);
                        }
                        catch (Exception)
                        {
                            Logger.Log("Error parsing page name / collection data | Index " + index, Logger.LogLevel.Error);
                        }

                        index++;
                    }
                }
                catch (Exception)
                {
                    Logger.Log("Error parsing page | Index " + index, Logger.LogLevel.Error);
                }

            }

            return csgoData;
        }

        public static async Task<Dictionary<string, List<string>>> ParseKnives(List<string> knifeList)
        {
            HtmlFetcher htmlFetcher = new HtmlFetcher();

            Logger.Log("Fetching master knives HTML...");

            List<string> pages = await HtmlFetcher.FetchUrlFillers("https://csgostash.com/weapon/", knifeList, 50);

            Logger.Log("Extracting individual knife URLs from HTML...");

            //Pull the link <a></a> tags out of each page - store them to be used later
            List<string> knifeUrls = new List<string>();
            int pageindex = 0;
            foreach (var page in pages)
            {
                //Getting knife name
                // => /body/div[1]/div[2]/div[0]
                // => col-lg-12 text-center col-widen content-header => h1

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(page);

                var aTags =
                    doc.DocumentNode.SelectNodes("/html/body//a"); //Select the body before looking for all the a tags

                //Filter the A-Tags to only ones with href
                List<string> aTagHrefs = new List<string>();
                foreach (var aTag in aTags)
                {
                    string hrefVal = "";
                    if (aTag.Attributes["href"] != null) hrefVal = aTag.Attributes["href"].Value; //Try to extract href val
                    if (!string.IsNullOrWhiteSpace(hrefVal)) aTagHrefs.Add(hrefVal); //Log the https links for further processing       
                }

                //Filter to only entries containing the current knife name (E.G Navaja+Knife) which will be converted to navaja-knife
                string knifeName = knifeList[pageindex].Replace("+", "-"); //The generic name of the current knife whose page it is on

                //Process the aTag hrefs to only those which are valid links
                aTagHrefs = aTagHrefs.Where(t => t.Contains("http") && t.Contains("/skin/") && t.Contains(knifeName)).ToList(); //Only websites && only paths containing skin && has knife name

                //Delete duplicates
                aTagHrefs = aTagHrefs.Distinct().ToList();

                aTagHrefs.ForEach(t => knifeUrls.Add(t)); //add to master url list

                pageindex++;
            }

            Logger.Log("Fetching individual knife HTML...");

            //Follow links pulled above to get the names of the actual knives
            List<string> knifeDataList = await HtmlFetcher.FetchUrls(knifeUrls, 10);

            Logger.Log("Extracting individual knife data from HTML...");

            Dictionary<string, List<string>> knifeCaseData = new Dictionary<string, List<string>>();
            foreach (string knifeDataPage in knifeDataList)
            {
                HtmlDocument doc = new HtmlDocument();

                try
                {
                    doc.LoadHtml(knifeDataPage);
                }
                catch (Exception)
                {
                    Logger.Log("Unable to load HTML doc for knifeDataPage", Logger.LogLevel.Error);
                }

                ////Extracting knife cases <!--body/div1/div2/div1/div0/div0/div1/div1/div0--> <!--One with class "well skin-details-collection-container margin-top-med"-->
                //var selectedNode = doc.DocumentNode.SelectNodes("html/body/div/div/div/div/div/div/div/div") 
                //    .Where(n => n.InnerText.ToLower().Contains("expensive"))
                //    .Where(n => n.InnerText.ToLower().Contains("least"))
                //    .Where(n => n.InnerText.ToLower().Contains("most")).Select(i => i.InnerText).ToList();

                //if (selectedNode.Count <= 0) //Code above does not work if knife only exists in one case, thus if it does not find anything, run this
                //{

                //Extracting case data
                List<string> knifeCases = doc.DocumentNode.SelectNodes("html/body//p")
                    .Where(n => n.HasClass("collection-text-label")).Select(n => n.InnerHtml)
                    .ToList(); //Search for p with class "collection-text-label"

                //Get knife name
                string knifeName = doc.DocumentNode.SelectNodes("html/head/title").Select(n => n.InnerHtml)
                    .FirstOrDefault();
                knifeName = knifeName?.Replace(" - CS:GO Stash", ""); //Null prorogation

                if (knifeName != null)
                {
                    //Add to dictionary
                    knifeCaseData.Add(knifeName, knifeCases);
                }
                
            }

            Logger.Log("Done!");
            return knifeCaseData;
        }

        public static async Task<Dictionary<string, List<string>>> ParseSouvenirs()
        {
            HtmlFetcher htmlFetcher = new HtmlFetcher();

            List<string> pages = await htmlFetcher.FetchAscending("https://csgostash.com/containers/souvenir-packages?page=", 30, 2);
            
            Dictionary<string, List<string>> souvenirCollections = new Dictionary<string, List<string>>(); //Collection name -- Item list
            //Parse through the souvenirs for names, and the collection
            foreach (string page in pages)
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(page);
                
                //Filter to the class "well result-box nomargin"
                List<string> filteredDivs = doc.DocumentNode.SelectNodes("html/body/div/div/div/div").Where(n =>
                    n.Attributes["class"].Value.Contains("well result-box nomargin")).Select(n => n.InnerHtml).ToList();
                
                //Extract name and collection
                foreach (string filteredDiv in filteredDivs)
                {
                    /*
                    <a href="https://csgostash.com/item/13395/Katowice-2019-Nuke-Souvenir-Package">
                       <h4>Katowice 2019 Nuke Souvenir Package</h4>
                       <img class="img-responsive center-block" src="https://steamcommunity-a.akamaihd.net/economy/image/-9a81dlWLwJ2UUGcVs_nsVtzdOEdtWwKGZZLQHTxDZ7I56KU0Zwwo4NUX4oFJZEHLbXU5A1PIYQNqhpOSV-fRPasw8rsWFxgKhNetb_3e1Y57OPafjBN09izq46enPK6YurQk2hVvcYi2u-W84qhiQOx_kVvYDzyJdWUJA88Ml_U-VfoxuzsjYj84sqZEQQDQg/256fx256f" alt="Katowice 2019 Nuke Souvenir Package">
                       <div class="price margin-top-sm">
                          <p class="nomargin"><span data-toggle="tooltip" data-placement="right" title="1 Key">CDN$ 5.08</span></p>
                       </div>
                    </a>
                    <div class="btn-group-sm btn-group-justified">
                       <a class="btn btn-default market-button-item" href="https://steamcommunity.com/market/listings/730/Katowice%202019%20Nuke%20Souvenir%20Package" target="_blank" rel="nofollow" data-gaevent="Katowice 2019 Nuke Souvenir Package">3956 Steam Market Listings</a>
                    </div>
                    <div class="containers-details-link">
                       <p class="nomargin"><a href="https://csgostash.com/collection/The+2018+Nuke+Collection">
                          <img src="https://csgostash.com/img/collections/the_2018_nuke_collection.png" class="collection-icon" alt="The 2018 Nuke Collection">
                          The 2018 Nuke Collection</a>
                       </p>
                    </div>
                    */
                    HtmlDocument filteredDoc = new HtmlDocument();
                    filteredDoc.LoadHtml(filteredDiv); //Exception is likely from loading invalid HTML

                    string souvenirCaseName = "";
                    string souvenirCaseCollection = "";
                    try //Try catches because I can't be bothered to track down why this is causing an exception
                    {
                        //Extract case name
                        souvenirCaseName =  filteredDoc.DocumentNode?.SelectNodes("a/h4").FirstOrDefault()?.InnerText;
                        //Extract case collection
                        souvenirCaseCollection = filteredDoc.DocumentNode?.SelectNodes("div/p/a").FirstOrDefault()?.InnerText
                            .Replace("\n", ""); //Remove excess line break
                    }
                    catch
                    {
                        Logger.Log("Error parsing souvenir case info", Logger.LogLevel.Error);
                        Logger.Log("Information at time of error: " + filteredDiv, Logger.LogLevel.Debug);
                    }
                    
                    //Add to souvenirCollections
                    if (souvenirCaseCollection != null)
                    {
                        if (souvenirCollections.TryGetValue(souvenirCaseCollection, out _)) souvenirCollections[souvenirCaseCollection].Add(souvenirCaseName);
                        else
                        { //If souvenir collection does not exist, create one
                            souvenirCollections.Add(souvenirCaseCollection, new List<string>
                            {
                                souvenirCaseName
                            });
                        }
                    }
                }
            }

            return souvenirCollections;
        }

        public class CsgoItemData
        {
            public CsgoItemData()
            {
                CaseData = new List<CaseData>();
                CollectionData = new List<CollectionData>();
            }

            public List<CaseData> CaseData { get; set; }
            public List<CollectionData> CollectionData { get; set; }
        }
        public class CaseData
        {
            public string CaseName { get; set; }
            public string CaseCollection { get; set; }
            public List<string> CaseItems { get; set; }
        }

        public class CollectionData
        {
            public string ColletionName { get; set; }
            public bool IsSouvenir { get; set; }
            public List<string> CollectionItems { get; set; }
        }
    }
}
