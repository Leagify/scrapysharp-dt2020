using System;
using CsvHelper.Configuration;

namespace scrapysharp_dt2020
{

    public class ExistingProspectRanking
    {
        public string rank;
        public string change;
        public string playerName;
        public string school;
        public string position1;
        public string height;
        public string weight;
        public string collegeClass;
        public string rankingDateString;
        public string draftStatus;

        public ExistingProspectRanking() {}
        public ExistingProspectRanking(string rank, string chg, string name, string school, string pos1, string height, string weight, string year, string date, string status)
        {
            this.rank = rank;
            this.change = chg;
            this.playerName = name;
            this.school = school;
            this.position1 = pos1;
            this.height = height;
            this.weight = weight;
            this.collegeClass = year;
            this.rankingDateString = date;
            this.draftStatus = status;
        }
    }

    public sealed class ExistingProspectRankingCsvMap : ClassMap<ExistingProspectRanking>
    {
        public ExistingProspectRankingCsvMap()
        {
            Map(m => m.rank).Name("Rank");
            Map(m => m.change).Name("Change");
            Map(m => m.playerName).Name("Player");
            Map(m => m.school).Name("School");
            Map(m => m.position1).Name("Position");
            Map(m => m.height).Name("Height").ConvertUsing(map => Program.convertHeightToInches(map.GetField("Height"), map.GetField("Player")).ToString());
            Map(m => m.weight).Name("Weight");
            Map(m => m.collegeClass).Name("CollegeClass");
            Map(m => m.rankingDateString).Name("Date");
            Map(m => m.draftStatus).Name("DraftStatus");
        }
    }
}