using CsvHelper;
using HtmlAgilityPack;
using ScrapySharp.Extensions;
using ScrapySharp.Html;
using ScrapySharp.Html.Forms;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace scrapysharp_dt2020
{
    class Program
    {
        static void Main(string[] args)
        {
            var webGet = new HtmlWeb();
            webGet.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:31.0) Gecko/20100101 Firefox/31.0";
            var document1 = webGet.Load("https://www.drafttek.com/2020-NFL-Draft-Big-Board/Top-NFL-Draft-Prospects-2020-Page-1.asp");
            var document2 = webGet.Load("https://www.drafttek.com/2020-NFL-Draft-Big-Board/Top-NFL-Draft-Prospects-2020-Page-2.asp");
            var document3 = webGet.Load("https://www.drafttek.com/2020-NFL-Draft-Big-Board/Top-NFL-Draft-Prospects-2020-Page-3.asp");

            //Get ranking date
            var dateOfRanks = document1.DocumentNode.SelectSingleNode("//*[@id='HeadlineInfo1']").InnerText.Replace(" EST", "").Trim();
            //Change date to proper date. The original format should be like this:
            //" May 21, 2019 2:00 AM EST"
            DateTime parsedDate;
            DateTime.TryParse(dateOfRanks, out parsedDate);
            string dateInNiceFormat = parsedDate.ToString("yyyy-MM-dd");

            List<ProspectRanking> list1 = GetProspects(document1, parsedDate);
            List<ProspectRanking> list2 = GetProspects(document2, parsedDate);
            List<ProspectRanking> list3 = GetProspects(document3, parsedDate);

            //Write projects to csv with date.
            using (var writer = new StreamWriter($"ranks\\{dateInNiceFormat}-ranks.csv"))
            using (var csv = new CsvWriter(writer))
            {    
                csv.Configuration.RegisterClassMap<ProspectRankingMap>();
                csv.WriteRecords(list1);
                csv.WriteRecords(list2);
                csv.WriteRecords(list3);
            }
        }

        public static List<ProspectRanking> GetProspects(HtmlDocument document, DateTime dateOfRanks)
        {
            // Create variables to store prospect rankings.
            int rank = 0;
            string change = "";
            string playerName = "";
            string school = "";
            string position1 = "";
            string height = "";
            int weight = 0;
            string position2 = "";
            
            List<ProspectRanking> prospectList = new List<ProspectRanking>();

            if (document.DocumentNode != null)
            {
                // "/html[1]/body[1]/div[1]/div[3]/div[1]/table[1]"
                var tbl = document.DocumentNode.SelectNodes("/html[1]/body[1]/div[1]/div[3]/div[1]/table[1]");
                
                foreach (HtmlNode table in tbl) {
                    foreach (HtmlNode row in table.SelectNodes("tr")) {
                        foreach (HtmlNode cell in row.SelectNodes("th|td")) {
                            
                            string Xpath = cell.XPath;
                            int locationOfColumnNumber = cell.XPath.Length - 2 ;
                            char dataIndicator = Xpath[locationOfColumnNumber];
                            bool isRank = (dataIndicator == '1');
                            switch (dataIndicator)
                            {
                                case '1':
                                    // td[1]= Rank
                                    if (Int32.TryParse(cell.InnerText, out int rankNumber))
                                        rank = rankNumber;
                                        Console.WriteLine("Rank: " + cell.InnerText);
                                    break;
                                case '2':
                                    // td[2]= Change
                                    change = cell.InnerText;
                                    break;
                                case '3':
                                    // td[3]= Player
                                    playerName = cell.InnerText;
                                    Console.WriteLine("Player: " + cell.InnerText);
                                    break;
                                case '4':
                                    // td[4]= School
                                    school = cell.InnerText;
                                    break;
                                case '5':
                                    // td[5]= Pos1
                                    position1 = cell.InnerText;
                                    break;
                                case '6':
                                    // td[6]= Ht
                                    height = cell.InnerText;
                                    break;
                                case '7':
                                    // td[7]= Weight
                                    if (Int32.TryParse(cell.InnerText, out int weightNumber))
                                        weight = weightNumber;
                                    break;
                                case '8':
                                    // td[8]= Pos2 (Often blank)
                                    position2 = cell.InnerText;
                                    break;
                                case '9':
                                    // td[9]= Link to Bio (not used)
                                    continue;
                                default:
                                    break;
                            }
                        }
                        // The header is in the table, so I need to ignore it here.
                        if (change != "CNG")
                        {
                            prospectList.Add(new ProspectRanking(dateOfRanks, rank, change, playerName, school, position1, height, weight, position2));
                        }
                    }
                }
                Console.WriteLine($"Prospect count: {prospectList.Count}");
            }
            return prospectList;
        }
    }
}