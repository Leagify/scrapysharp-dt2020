using CsvHelper;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace scrapysharp_dt2020
{
	class Program
    {
        static void Main(string[] args)
        {
            File.WriteAllText($"logs{Path.DirectorySeparatorChar}Status.log", "");
            File.WriteAllText($"logs{Path.DirectorySeparatorChar}Prospects.log", "");

            Console.WriteLine("Getting data...");

            var webGet = new HtmlWeb();
            webGet.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:31.0) Gecko/20100101 Firefox/31.0";
            var document1 = webGet.Load("https://www.drafttek.com/2020-NFL-Draft-Big-Board/Top-NFL-Draft-Prospects-2020-Page-1.asp");
            var document2 = webGet.Load("https://www.drafttek.com/2020-NFL-Draft-Big-Board/Top-NFL-Draft-Prospects-2020-Page-2.asp");
            var document3 = webGet.Load("https://www.drafttek.com/2020-NFL-Draft-Big-Board/Top-NFL-Draft-Prospects-2020-Page-3.asp");
            var document4 = webGet.Load("https://www.drafttek.com/2020-NFL-Draft-Big-Board/Top-NFL-Draft-Prospects-2020-Page-4.asp");
            var document5 = webGet.Load("https://www.drafttek.com/2020-NFL-Draft-Big-Board/Top-NFL-Draft-Prospects-2020-Page-5.asp");

            Console.WriteLine("Parsing data...");

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

            Console.WriteLine("Creating csv...");

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

            Console.WriteLine("Completed.");
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
            File.AppendAllText($"logs{Path.DirectorySeparatorChar}Status.log", "Creating the big CSV....." + Environment.NewLine);

            // Get Schools and the States where they are located.
            List<School> schoolsAndConferences;
            using (var reader = new StreamReader($"info{Path.DirectorySeparatorChar}SchoolStatesAndConferences.csv"))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.RegisterClassMap<SchoolCsvMap>();
                schoolsAndConferences = csv.GetRecords<School>().ToList();
            }

			List<Region> statesAndRegions;
			using(var reader = new StreamReader($"info{Path.DirectorySeparatorChar}StatesToRegions.csv"))
			using(var csv = new CsvReader(reader))
			{
				csv.Configuration.RegisterClassMap<RegionCsvMap>();
				statesAndRegions = csv.GetRecords<Region>().ToList();
			}

            //Get position types
            List<PositionType> positionsAndTypes;
            using (var reader = new StreamReader($"info{Path.DirectorySeparatorChar}PositionInfo.csv"))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.RegisterClassMap<PositionTypeCsvMap>();
                positionsAndTypes = csv.GetRecords<PositionType>().ToList();
            }

            // Let's assign these ranks point values.
            List<PointProjection> ranksToProjectedPoints;
            using (var reader = new StreamReader($"info{Path.DirectorySeparatorChar}RanksToProjectedPoints.csv"))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.RegisterClassMap<PointProjectionCsvMap>();
                ranksToProjectedPoints = csv.GetRecords<PointProjection>().ToList();
            }

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
            List<ExistingProspectRanking> prospectRanks;
            using (var reader = new StreamReader($"ranks{Path.DirectorySeparatorChar}joinedRanks2020.csv"))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.RegisterClassMap<ExistingProspectRankingCsvMap>();
                prospectRanks = csv.GetRecords<ExistingProspectRanking>().ToList();
            }

            // Use linq to join the stuff back together, then write it out again.
            var combinedHistoricalRanks = from r in prospectRanks
                                    join school in schoolsAndConferences on r.school equals school.schoolName
									join region in statesAndRegions on school.state equals region.state
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
										Region = region.region,
                                        Height = r.height,
                                        Weight = r.weight,
                                        CollegeClass = r.collegeClass,
                                        PositionGroup = positions.positionGroup,
                                        PositionAspect = positions.positionAspect,
                                        ProspectStatus = r.draftStatus,
                                        Date = r.rankingDateString,
                                        Points = rank.projectedPoints
                                    };



            //Write everything back to CSV, only better!
            using (var writer = new StreamWriter($"ranks{Path.DirectorySeparatorChar}joinedRanks2020.csv"))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(combinedHistoricalRanks);
            }

            File.AppendAllText($"logs{Path.DirectorySeparatorChar}Status.log", "Creating the big CSV completed." + Environment.NewLine);
        }

        private static void CheckForMismatches(string csvFileName)
        {
            File.AppendAllText($"logs{Path.DirectorySeparatorChar}Status.log", "Checking for mismatches in " + csvFileName + "....." + Environment.NewLine);

            // Read in data from a different project.
            List<School> schoolsAndConferences;
            using (var reader = new StreamReader($"info{Path.DirectorySeparatorChar}SchoolStatesAndConferences.csv"))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.RegisterClassMap<SchoolCsvMap>();
                schoolsAndConferences = csv.GetRecords<School>().ToList();
            }

            List<ProspectRankSimple> ranks;
            using (var reader = new StreamReader(csvFileName))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.RegisterClassMap<ProspectRankSimpleCsvMap>();
                ranks = csv.GetRecords<ProspectRankSimple>().ToList();
            }

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

            if (schoolMismatches.Count() > 0)
            {
                File.WriteAllText($"logs{Path.DirectorySeparatorChar}Mismatches.log", "");
            }

            foreach (var s in schoolMismatches){
                noMismatches = false;
                File.AppendAllText($"logs{Path.DirectorySeparatorChar}Mismatches.log", $"{s.rank}, {s.name}, {s.college}" + Environment.NewLine);
            }

            if (noMismatches)
            {
                File.AppendAllText($"logs{Path.DirectorySeparatorChar}Status.log", "No mismatches in " + csvFileName + "....." + Environment.NewLine);
            }
            else
            {
                File.AppendAllText($"logs{Path.DirectorySeparatorChar}Status.log", schoolMismatches.Count() + " mismatches in " + csvFileName + ".....Check Mismatches.log." + Environment.NewLine);
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
            string collegeClass = "";

            List<ProspectRanking> prospectList = new List<ProspectRanking>();

            if (document.DocumentNode != null)
            {
                // "/html[1]/body[1]/div[1]/div[3]/div[1]/table[1]"
                var tbl = document.DocumentNode.SelectNodes("/html[1]/body[1]/div[1]/div[3]/div[1]/table[1]");

                if (tbl == null)
                {
                    File.AppendAllText($"logs{Path.DirectorySeparatorChar}Status.log", $"No prospects on page {pageNumber}" + Environment.NewLine);
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
                                    File.AppendAllText($"logs{Path.DirectorySeparatorChar}Prospects.log", "Rank: " + cell.InnerText + Environment.NewLine);
                                    break;
                                case '2':
                                    // td[2]= Change
                                    change = cell.InnerText;
                                    change = change.Replace("&nbsp;","");
                                    break;
                                case '3':
                                    // td[3]= Player
                                    playerName = cell.InnerText;
                                    File.AppendAllText($"logs{Path.DirectorySeparatorChar}Prospects.log", "Player: " + cell.InnerText + Environment.NewLine);
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
                                    // College Class- used to be Pos2 (which was often blank)
                                    collegeClass = cell.InnerText;
                                    break;
                                case '9':
                                    // td[9]= Link to Bio (not used)
                                    continue;
                                default:
                                    break;
                            }
                        }
                        // Handle draft eligibility and declarations (done via row color)
                        string draftStatus = "";
                        if (row.Attributes.Contains("style") && row.Attributes["style"].Value.Contains("background-color"))
                        {
                            string rowStyle = row.Attributes["style"].Value;
                            string backgroundColor = Regex.Match(rowStyle, @"background-color: \w*").Value.Substring(18);
                            switch (backgroundColor)
                            {
                                case "white":
                                    draftStatus = "Eligible";
                                    break;
                                case "lightblue":
                                    draftStatus = "Underclassman";
                                    break;
                                case "palegoldenrod":
                                    draftStatus = "Declared";
                                    break;
                                default:
                                    draftStatus = "";
                                    break;
                            }
                            File.AppendAllText($"logs{Path.DirectorySeparatorChar}Prospects.log", "Draft Status: " + draftStatus + Environment.NewLine);
                        }
                        // The header is in the table, so I need to ignore it here.
                        if (change != "CNG")
                        {
                            prospectList.Add(new ProspectRanking(dateOfRanks, rank, change, playerName, school, position1, height, weight, collegeClass, draftStatus));
                        }
                    }
                }
                File.AppendAllText($"logs{Path.DirectorySeparatorChar}Status.log", $"Prospect count on page {pageNumber}: {prospectList.Count}" + Environment.NewLine);
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
                case "Cal":
                    return "California";
                case "S. Illinois":
                    return "Southern Illinois";
                case "UConn":
                    return "Connecticut";
                case "LA Tech":
                    return "Louisiana Tech";
                case "Louisiana":
                    return "Louisiana-Lafayette";
                case "San Diego St":
                    return "San Diego State";
                case "South Carolina St":
                    return "South Carolina State";
                case "Wake Forrest":
                    return "Wake Forest";
                case "NM State":
                    return "New Mexico State";
                case "New Mexico St":
                    return "New Mexico State";
                case "Southern Cal":
                    return "USC";
                case "Mempis":
                    return "Memphis";
                case "Southeast Missouri":
                    return "Southeast Missouri State";
                case "Berry College":
                    return "Berry";
                case "USF":
                    return "South Florida";
                case "N Dakota State":
                    return "North Dakota State";
                case "SE Missouri State":
                    return "Southeast Missouri State";
                default:
                    return school;
            }
        }
        public static int convertHeightToInches(string height, string playerName)
        {
            // Height might look something like "\"6'1\"\"\"" - convert to inches to look less awful.
            string regexHeight = Regex.Match(height, @"\d'\d+").Value;
            string[] feetAndInches = regexHeight.Split("'");

            bool parseFeet = Int32.TryParse(feetAndInches[0], out int feet);
            int inches = 0;
            bool parseInches = false;
            if (feetAndInches.Length > 1 && feetAndInches[1] != null)
            {
                parseInches = Int32.TryParse(feetAndInches[1], out inches);
            }

            if (parseFeet && parseInches)
            {
                int heightInInches = (feet*12)+inches;
                return heightInInches;
            }
            else
            {
                File.AppendAllText($"logs{Path.DirectorySeparatorChar}Mismatches.log", $"Player {playerName} height of {height} not converted properly, entring 0 instead" + Environment.NewLine);
                return 0;
            }
        }
    }
}