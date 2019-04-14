using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Discord_UncrateGO_SkinCasesGenerator
{
    internal static class HtmlParser
    {
        public static async Task<CsgoItemData> FetchCaseItemData()
        {
            CsgoItemData csgoItemData = new CsgoItemData();

            SiteData data = await GetSiteData();

            //TODO method for gathering data could be less memory intensive, instead of grab all urls and parse, grab one and parse so it can then be disposed of
            Dictionary<string, DataCollection> caseResult = await ParseGridBlocks(data.CaseUrLs); 
            Dictionary<string, List<string>> knifeResult = await ParseKnives(data.KnifeUrLs);

            //sort the items from knife result into the distinctive caseResult cases
            Logger.Log("sorting knife data into cases...");
            foreach (KeyValuePair<string, List<string>> knife in knifeResult)
            {
                foreach (string knifeCases in knife.Value) //iterate through each knife's cases and add it to the master case list
                {
                    if (caseResult.TryGetValue(knifeCases, out _)) //ensure the case exists before trying to add to it
                    {
                        caseResult[knifeCases].Items.Add(knife.Key);
                    }
                }
            }

            csgoItemData.CaseData = caseResult.Values.ToList();

            csgoItemData.CollectionData = (await ParseGridBlocks(data.CollectionUrLs)).Values.ToList(); //Get collections
            csgoItemData.SouvenirData = await ParseSouvenirs(data.SouvenirUrLs);

            //TODO Get stickers

            return csgoItemData;
        }

        #region Get site metadata for methods

        /// <summary>
        /// Fetches and returns information about the site that would later be used to parse cases
        /// </summary>
        /// <returns></returns>
        private static async Task<SiteData> GetSiteData()
        {
            string site = await HtmlFetcher.RetrieveFromUrl("https://csgostash.com/");

            var doc = new HtmlDocument();
            doc.LoadHtml(site);

            //Target the li tags with a class of dropdown
            IEnumerable<HtmlNode> liData =
                doc.DocumentNode.SelectNodes("//li[@class='dropdown']");

            //Generate the urLs for souvenirs
            string baseSouvenirUrL = GetDropdownOption(liData, "newest cases", "souvenir-package").FirstOrDefault();
            var souvenirUrLs = new List<string>();
            int pageCount = await GetPaginationPagesCount(baseSouvenirUrL);
            for (int i = 1; i < pageCount + 1; i++) //add 1 to be 1 based
            {
                souvenirUrLs.Add($"{baseSouvenirUrL}?page={i}");
            }

            var siteData = new SiteData
            {
                //Get URL data from dropdowns
                CaseUrLs = GetDropdownOption(liData, "newest cases", "/case/"),
                KnifeUrLs = GetDropdownOption(liData, "newest knives", "/weapon/"),
                CollectionUrLs = GetDropdownOption(liData, "newest collections", "/collection/"),
                SouvenirUrLs = souvenirUrLs
            };

            return siteData;
        }

        /// <summary>
        /// Filters the html nodes in the dropdown class to ones specified by filter
        /// </summary>
        /// <param name="htmlNodes"></param>
        /// <param name="dropdownFilter"></param>
        /// <param name="urLFilter"></param>
        /// <returns></returns>
        private static IEnumerable<string> GetDropdownOption(IEnumerable<HtmlNode> htmlNodes, string dropdownFilter, string urLFilter)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlNodes.Where(i => i.InnerHtml.ToLower().Contains(dropdownFilter)).Select(i => i.InnerHtml).FirstOrDefault());

            return doc.DocumentNode.SelectNodes("//a").Select(n => n.Attributes["href"].Value).Where(n => n.Contains(urLFilter));
        }

        private static async Task<int> GetPaginationPagesCount(string url)
        {
            var page = await HtmlFetcher.RetrieveFromUrl(url);

            var doc = new HtmlDocument();
            doc.LoadHtml(page);

            doc.LoadHtml(doc.DocumentNode.SelectNodes("//ul[@class='pagination']").FirstOrDefault().InnerHtml); //Select and load the first pagination button
            IEnumerable<string> innerText = doc.DocumentNode.SelectNodes("//li").Select(n => n.InnerText); //Extract the inner text from li options

            //Cycle through each li and get the highest one
            int highestVal = 1;
            foreach (string s in innerText)
            {
                if (int.TryParse(s, out int val)) if (val > highestVal) highestVal = val;
            }

            return highestVal;
        }

        private class SiteData
        {
            internal SiteData()
            {
                CaseUrLs = new List<string>();
                KnifeUrLs = new List<string>();
                CollectionUrLs = new List<string>();
                SouvenirUrLs = new List<string>();
            }

            public IEnumerable<string> CaseUrLs { get; set; }
            public IEnumerable<string> KnifeUrLs { get; set; }
            public IEnumerable<string> CollectionUrLs { get; set; }
            public IEnumerable<string> SouvenirUrLs { get; set; }
        }

        #endregion

        /// <summary>
        /// Parses through the block structure for item data and case name+collection
        /// </summary>
        /// <param name="caseUrLs"></param>
        /// <returns></returns>
        private static async Task<Dictionary<string, DataCollection>> ParseGridBlocks(IEnumerable<string> caseUrLs)
        {
            List<string> pages = await HtmlFetcher.FetchUrls(caseUrLs);

            var csgoData = new Dictionary<string, DataCollection>();
            foreach (string page in pages)
            {
                //Get case name and collection
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(page);

                //Get the data out of the <a3> tags
                HtmlNode caseNodeData =
                    htmlDoc.DocumentNode.SelectSingleNode(
                        "//div[@class='inline-middle collapsed-top-margin']"); //inline-middle collapsed-top-margin

                string caseName = ExtractStringFromTags(caseNodeData, "h1");
                string caseCollection = ExtractStringFromTags(caseNodeData, "h4"); //img-responsive center-block content-header-img-margin

                string iconUrL = ExtractImgSrc(htmlDoc, "img-responsive center-block content-header-img-margin");

                var caseData = new DataCollection {Name = caseName, CaseCollection = caseCollection, IconUrL = iconUrL };

                //If case name already exists, pass
                if (csgoData.ContainsKey(caseName)) continue;
                //Separate single string with line breaks into array
                string[] lines = page.Split(
                    new[] { "\r\n", "\r", "\n" },
                    StringSplitOptions.None
                );

                //Filter to lines containing " | "
                List<string> fLines = lines.Where(l => l.Contains(" | ")).ToList();
                //Select the text out of the html
                foreach (var dataDuoLines in fLines) //The 2 lines containing the item name and collection
                {
                    HtmlDocument duoHtml = new HtmlDocument();
                    duoHtml.LoadHtml(dataDuoLines);

                    //Get the data out of the <a3> tags
                    HtmlNodeCollection duoLineDataNodes = duoHtml.DocumentNode.SelectNodes("/h3/a");
                    if (duoLineDataNodes != null && duoLineDataNodes.Any())
                    {
                        List<string> duoLineData = duoLineDataNodes.Select(i => i.InnerHtml).ToList();

                        //Concat into item name
                        if (duoLineData.Any()) caseData.Items.Add(string.Join(" | ", duoLineData)); //Weapon name | skin name
                    }
                }

                if (!csgoData.TryGetValue(caseName, out _)) csgoData.Add(caseName, caseData);
            }

            return csgoData;
        }

        private static async Task<Dictionary<string, List<string>>> ParseKnives(IEnumerable<string> knifeUrLs)
        {
            Logger.Log("Fetching master knives HTML...");

            List<string> knifeUrLsList = knifeUrLs.ToList();
            List<string> pages = await HtmlFetcher.FetchUrls(knifeUrLsList);

            Logger.Log("Extracting individual knife URLs from HTML...");

            var knifeNames = new List<string>();
            //Extract the knife names by themselves from the list
            foreach (string knifeUrL in knifeUrLsList)
            {
                for (int i = knifeUrL.Length - 1; i > 0; i--) //subtract 1 since array is 0 based
                {
                    if (knifeUrL[i] == '/')
                    {
                        knifeNames.Add(knifeUrL.Substring(i + 1, knifeUrL.Length - i - 1) //Offset forwards by 1 since this is 1 based
                            .Replace("+", "-")); //the current knife name (E.G Navaja+Knife) which will be converted to navaja-knife
                        break;
                    }
                }
            }

            //Pull the link <a></a> tags out of each page - store them to be used later
            var knifeUrls = new List<string>();
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
                var aTagHrefs = new List<string>();
                foreach (var aTag in aTags)
                {
                    string hrefVal = "";
                    if (aTag.Attributes["href"] != null) hrefVal = aTag.Attributes["href"].Value; //Try to extract href val
                    if (!string.IsNullOrWhiteSpace(hrefVal)) aTagHrefs.Add(hrefVal); //Log the https links for further processing       
                }

                //Process the aTag hrefs to only those which are valid links
                aTagHrefs = aTagHrefs.Where(t => t.Contains("http") && t.Contains("/skin/") 
                                                                    && t.Contains(knifeNames[pageindex])).ToList(); //Only websites && only paths containing skin && has knife name

                //Delete duplicates
                aTagHrefs = aTagHrefs.Distinct().ToList();

                aTagHrefs.ForEach(t => knifeUrls.Add(t)); //add to master url list

                pageindex++;
            }

            Logger.Log("Fetching individual knife HTML...");

            //Follow links pulled above to get the names of the actual knives
            List<string> knifeDataList = await HtmlFetcher.FetchUrls(knifeUrls);

            Logger.Log("Extracting individual knife data from HTML...");

            var knifeCaseData = new Dictionary<string, List<string>>();
            foreach (string knifeDataPage in knifeDataList)
            {
                HtmlDocument doc = new HtmlDocument();

                doc.LoadHtml(knifeDataPage);

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

            return knifeCaseData;
        }

        private static async Task<List<DataCollection>> ParseSouvenirs(IEnumerable<string> souvenirUrLs)
        {
            Logger.Log("Fetching Souvenirs...");

            List<string> pages = await HtmlFetcher.FetchUrls(souvenirUrLs);
            
            var souvenirCollections = new List<DataCollection>();
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
                    HtmlDocument filteredDoc = new HtmlDocument();
                    filteredDoc.LoadHtml(filteredDiv); //Exception is likely from loading invalid HTML

                    //Extract case name
                    string souvenirCaseName = ExtractStringFromTags(filteredDoc, "a/h4");

                    //Extract case collection
                    string souvenirCaseCollection = ExtractStringFromTags(filteredDoc, "div/p/a").Replace("\n", "");

                    string iconUrL = ExtractImgSrc(doc, "img-responsive center-block");

                    //Add to souvenirCollections
                    var souvenirCollection = new DataCollection()
                    {
                        Name = souvenirCaseName,
                        CaseCollection = souvenirCaseCollection,
                        IconUrL = iconUrL
                    };

                    souvenirCollections.Add(souvenirCollection);
                }
            }

            return souvenirCollections;
        }

        #region Helper Methods

        /// <summary>
        /// Extracts information from desired parts of specified html node in HtmlDocument
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="xpath"></param>
        /// <param name="nodeSection"></param>
        /// <returns></returns>
        private static string ExtractStringFromTags(HtmlDocument doc, string xpath, NodeSection nodeSection = NodeSection.InnerText)
        {
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes(xpath);

            string returnStr = "";
            if (nodes != null && nodes.Count > 0)
            {
                HtmlNode nodeEntry = nodes.FirstOrDefault();
                if (nodeEntry != null)
                {
                    switch (nodeSection)
                    {
                        case NodeSection.InnerText:
                            returnStr = nodeEntry.InnerText;
                            break;
                        case NodeSection.InnerHtml:
                            returnStr = nodeEntry.InnerHtml;
                            break;
                        case NodeSection.OuterHtml:
                            returnStr = nodeEntry.OuterHtml;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(nodeSection), nodeSection, "Desired nodeSection not found");
                    }
                }
            }

            return returnStr;
        }
        private static string ExtractStringFromTags(HtmlNode node, string xpath, NodeSection nodeSection = NodeSection.InnerText)
        {
            HtmlNodeCollection nodes = node.SelectNodes(xpath);

            string returnStr = "";
            if (nodes != null && nodes.Count > 0)
            {
                HtmlNode nodeEntry = nodes.FirstOrDefault();
                if (nodeEntry != null)
                {
                    switch (nodeSection)
                    {
                        case NodeSection.InnerText:
                            returnStr = nodeEntry.InnerText;
                            break;
                        case NodeSection.InnerHtml:
                            returnStr = nodeEntry.InnerHtml;
                            break;
                        case NodeSection.OuterHtml:
                            returnStr = nodeEntry.OuterHtml;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(nodeSection), nodeSection, "Desired nodeSection not found");
                    }
                }
            }

            return returnStr;
        }

        private static string ExtractImgSrc(HtmlDocument htmlDoc, string className)
        {
            return htmlDoc.DocumentNode
                .SelectNodes($"//img[@class='{className}']")
                .Select(n => n.Attributes["src"].Value).FirstOrDefault();
        }

        private enum NodeSection { InnerText, InnerHtml, OuterHtml}

        #endregion
        public class CsgoItemData
        {
            public CsgoItemData()
            {
                CaseData = new List<DataCollection>();
                CollectionData = new List<DataCollection>();
                SouvenirData = new List<DataCollection>();
            }

            public List<DataCollection> CaseData { get; set; }
            public List<DataCollection> CollectionData { get; set; }
            public List<DataCollection> SouvenirData { get; set; }
        }

        public class DataCollection
        {
            public DataCollection()
            {
                Items = new List<string>();
            }

            public string Name { get; set; }
            public string CaseCollection { get; set; }
            public string IconUrL{ get; set; }
            public List<string> Items { get; set; }
        }
    }
}
