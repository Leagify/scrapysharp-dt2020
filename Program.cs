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
using System.Text.RegularExpressions;

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
            var document4 = webGet.Load("https://www.drafttek.com/2020-NFL-Draft-Big-Board/Top-NFL-Draft-Prospects-2020-Page-4.asp");
            var document5 = webGet.Load("https://www.drafttek.com/2020-NFL-Draft-Big-Board/Top-NFL-Draft-Prospects-2020-Page-5.asp");

            //Get ranking date
            var dateOfRanks = document1.DocumentNode.SelectSingleNode("//*[@id='HeadlineInfo1']").InnerText.Replace(" EST", "").Trim();
            //Change date to proper date. The original format should be like this:
            //" May 21, 2019 2:00 AM EST"
            DateTime parsedDate;
            DateTime.TryParse(dateOfRanks, out parsedDate);
            string dateInNiceFormat = parsedDate.ToString("yyyy-MM-dd");

            List<ProspectRanking> list1 = GetProspects(document1, parsedDate, 1);
            List<ProspectRanking> list2 = GetProspects(document2, parsedDate, 2);
            List<ProspectRanking> list3 = GetProspects(document3, parsedDate, 3);
            List<ProspectRanking> list4 = GetProspects(document4, parsedDate, 4);
            List<ProspectRanking> list5 = GetProspects(document5, parsedDate, 5);

            //This is the file name we are going to write.
            var csvFileName = $"ranks{Path.DirectorySeparatorChar}{dateInNiceFormat}-ranks.csv";

            //Write projects to csv with date.
            using (var writer = new StreamWriter(csvFileName))
            using (var csv = new CsvWriter(writer))
            {    
                csv.Configuration.RegisterClassMap<ProspectRankingMap>();
                csv.WriteRecords(list1);
                csv.WriteRecords(list2);
                csv.WriteRecords(list3);
                if (list4.Count > 0)
                {
                    csv.WriteRecords(list4);
                }
                if (list5.Count > 0)
                {
                    csv.WriteRecords(list5);
                }
            }

            CheckForMismatches(csvFileName);
            CreateCombinedCSV();
            CheckForMismatches($"ranks{Path.DirectorySeparatorChar}combinedRanks2020.csv");
            CreateCombinedCSVWithExtras();
        }

        private static void CreateCombinedCSV()
        {
            //Combine ranks from CSV files to create a master CSV.
            var filePaths = Directory.GetFiles($"ranks{Path.DirectorySeparatorChar}", "20??-??-??-ranks.csv").ToList<String>();
            //The results are probably already sorted, but I don't trust that, so I'm going to sort manually.
            filePaths.Sort();
            string destinationFile = $"ranks{Path.DirectorySeparatorChar}combinedRanks2020.csv";
            
            // Specify wildcard search to match CSV files that will be combined
            StreamWriter fileDest = new StreamWriter(destinationFile, false);
            
            int i;
            for (i = 0; i < filePaths.Count; i++)
            {
                string file = filePaths[i];
            
                string[] lines = File.ReadAllLines(file);
            
                if (i > 0)
                {
                    lines = lines.Skip(1).ToArray(); // Skip header row for all but first file
                }
            
                foreach (string line in lines)
                {
                    fileDest.WriteLine(line);
                }
            }
            
            fileDest.Close();
        }

        private static void CreateCombinedCSVWithExtras()
        {
            System.Console.WriteLine("Creating the big CSV.....");
            // Get Schools and the States where they are located.
            var schoolsAndConferences = System.IO.File.ReadAllLines($"info{Path.DirectorySeparatorChar}SchoolStatesAndConferences.csv")
                                        .Skip(1)
                                        .Where(s => s.Length > 1)
                                        .Select( s =>
                                        {
                                            var columns = s.Split(',');
                                            return new School(columns[0], columns[1], columns[2]);
                                        })
                                        .ToList();
            
            //Get position types
            var positionsAndTypes = System.IO.File.ReadAllLines($"info{Path.DirectorySeparatorChar}PositionInfo.csv")
                                        .Skip(1)
                                        .Where(s => s.Length > 1)
                                        .Select( s =>
                                        {
                                            var columns = s.Split(',');
                                            return new PositionType(columns[0], columns[1], columns[2]);
                                        })
                                        .ToList();
            // Let's assign these ranks point values.
            var ranksToProjectedPoints = System.IO.File.ReadAllLines($"info{Path.DirectorySeparatorChar}RanksToProjectedPoints.csv")
                                        .Skip(1)
                                        .Where(s => s.Length > 1)
                                        .Select( s =>
                                        {
                                            var columns = s.Split(',');
                                            return new PointProjection(columns[0], columns[1]);
                                        })
                                        .ToList();

            //Combine ranks from CSV files to create a master CSV.
            var filePaths = Directory.GetFiles($"ranks{Path.DirectorySeparatorChar}", "20??-??-??-ranks.csv").ToList<String>();
            //The results are probably already sorted, but I don't trust that, so I'm going to sort manually.
            filePaths.Sort();
            string destinationFile = $"ranks{Path.DirectorySeparatorChar}joinedRanks2020.csv";
            
            // Specify wildcard search to match CSV files that will be combined
            StreamWriter fileDest = new StreamWriter(destinationFile, false);
            
            int i;
            for (i = 0; i < filePaths.Count; i++)
            {
                string file = filePaths[i];
            
                string[] lines = File.ReadAllLines(file);
            
                if (i > 0)
                {
                    lines = lines.Skip(1).ToArray(); // Skip header row for all but first file
                }
            
                foreach (string line in lines)
                {
                    fileDest.WriteLine(line);
                }
            }
            
            fileDest.Close();

            // Get ranks from the newly created CSV file.
            var prospectRanks = System.IO.File.ReadAllLines($"ranks{Path.DirectorySeparatorChar}joinedRanks2020.csv")
                                        .Skip(1)
                                        .Where(s => s.Length > 1)
                                        .Select( s =>
                                        {
                                            var columns = s.Split(',');
                                            string heightInInches = convertHeightToInches(columns[5]).ToString();
                                            return new ExistingProspectRanking(columns[0], columns[1], columns[2], columns[3], columns[4], heightInInches, columns[6], columns[7], columns[8]);
                                        })
                                        .ToList();


            
            // Use linq to join the stuff back together, then write it out again.
            var combinedHistoricalRanks = from r in prospectRanks
                                    join school in schoolsAndConferences on r.school equals school.schoolName
                                    join positions in positionsAndTypes on r.position1 equals positions.positionName
                                    join rank in ranksToProjectedPoints on r.rank equals rank.rank
                                    select new {
                                        Rank = r.rank,
                                        Change = r.change,
                                        Name = r.playerName,
                                        Position = r.position1,
                                        College = r.school,
                                        Conference = school.conference,
                                        State = school.state,
                                        Height = r.height,
                                        Weight = r.weight,
                                        Position2 = r.position2,
                                        PositionGroup = positions.positionGroup,
                                        PositionAspect = positions.positionAspect,
                                        Date = r.rankingDateString,
                                        Points = rank.projectedPoints
                                    };

            
            
            //Write everything back to CSV, only better!
            using (var writer = new StreamWriter($"ranks{Path.DirectorySeparatorChar}joinedRanks2020.csv"))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(combinedHistoricalRanks);
            }
        }

        private static void CheckForMismatches(string csvFileName)
        {
            System.Console.WriteLine("Checking for mismatches in " + csvFileName + ".....");
            // Read in data from a different project.
            var schoolsAndConferences = System.IO.File.ReadAllLines($"info{Path.DirectorySeparatorChar}SchoolStatesAndConferences.csv")
                                        .Skip(1)
                                        .Where(s => s.Length > 1)
                                        .Select( s =>
                                        {
                                            var columns = s.Split(',');
                                            return new School(columns[0], columns[1], columns[2]);
                                        })
                                        .ToList();
            

            
            var ranks = System.IO.File.ReadAllLines(csvFileName)
                                        .Skip(1)
                                        .Where(r => r.Length > 1)
                                        .Select(r =>
                                        {
                                            var columns = r.Split(',');
                                            int rank = Int32.Parse(columns[0]);
                                            string name = columns[2];
                                            string college = columns[3];
                                            string dateString = columns[8];

                                            return new ProspectRankSimple(rank, name, college, dateString);
                                        }
                                        )
                                        .ToList();
            var schoolMismatches = from r in ranks
                                    join school in schoolsAndConferences on r.school equals school.schoolName into mm
                                    from school in mm.DefaultIfEmpty()
                                    where school is null
                                    select new {
                                        rank = r.rank,
                                        name = r.playerName,
                                        college = r.school 
                                    }
                                    ;
            
            bool noMismatches = true;
            
            foreach(var s in schoolMismatches){
                noMismatches = false;
                Console.WriteLine($"{s.rank}, {s.name}, {s.college}");
            }

            if(noMismatches)
            {
                Console.WriteLine("All good!");
            }
        }

        public static List<ProspectRanking> GetProspects(HtmlDocument document, DateTime dateOfRanks, int pageNumber)
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
                
                if (tbl == null)
                {
                    Console.WriteLine($"No prospects on page {pageNumber}");
                    return prospectList;
                }

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
                                    change = change.Replace("&nbsp;","");
                                    break;
                                case '3':
                                    // td[3]= Player
                                    playerName = cell.InnerText;
                                    Console.WriteLine("Player: " + cell.InnerText);
                                    break;
                                case '4':
                                    // td[4]= School
                                    school = checkSchool(cell.InnerText);
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
                Console.WriteLine($"Prospect count on page {pageNumber}: {prospectList.Count}");
            }
            return prospectList;
        }

        public static string checkSchool(string school)
        {
            switch(school)
            {
                case "Miami":
                    return "Miami (FL)";
                case "Mississippi":
                    return "Ole Miss";
                case "Central Florida":
                    return "UCF";
                case "MTSU":
                    return "Middle Tennessee";
                case "Eastern Carolina":
                    return "East Carolina";
                case "Pittsburgh":
                    return "Pitt";
                case "FIU":
                    return "Florida International";
                case "Florida St":
                    return "Florida State";
                case "Penn St":
                    return "Penn State";
                case "Minneosta":
                    return "Minnesota";
                case "Mississippi St.":
                    return "Mississippi State";
                case "Mississippi St":
                    return "Mississippi State";
                case "Oklahoma St":
                    return "Oklahoma State";
                case "Boise St":
                    return "Boise State";
                case "Lenoir-Rhyne":
                    return "Lenoir–Rhyne";
                case "NCState":
                    return "NC State";
                case "W Michigan":
                    return "Western Michigan";
                case "UL Lafayette":
                    return "Louisiana-Lafayette";
                default:
                    return school;
            }
        }
        public static int convertHeightToInches(string height)
        {
            // Height might look something like "\"6'1\"\"\"" - convert to inches to look less awful.
            string regexHeight = Regex.Match(height, @"\d'\d+").Value;
            string[] feetAndInches = regexHeight.Split("'");
            
            bool parseFeet = Int32.TryParse(feetAndInches[0], out int feet);
            bool parseInches = Int32.TryParse(feetAndInches[0], out int inches);

            if (parseFeet && parseInches)
            {
                int heightInInches = (feet*12)+inches;
                return heightInInches;
            }
            else
            {
                return 0;
            }
        }
    }
}