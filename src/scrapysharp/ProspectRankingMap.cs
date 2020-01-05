using CsvHelper;
using CsvHelper.Configuration;

namespace scrapysharp_dt2020
{
    public sealed class ProspectRankingMap : ClassMap<ProspectRanking>
    {
        public ProspectRankingMap()
        {
            //AutoMap();
        //     public int rank;
        // public string change;
        // public string playerName;
        // public string school;
        // public string position1;
        // public string height;
        // public int weight;
        // public string position2;
        // public DateTime rankingDate;
            Map(m => m.rank).Index(0).Name("Rank");
            Map(m => m.change).Index(1).Name("Change");
            Map(m => m.playerName).Index(2).Name("Player");
            Map(m => m.school).Index(3).Name("School");
            Map(m => m.position1).Index(4).Name("Position");
            Map(m => m.height).Index(5).Name("Height");
            Map(m => m.weight).Index(6).Name("Weight");
            Map(m => m.collegeClass).Index(7).Name("CollegeClass");
            Map(m => m.rankingDateString).Index(8).Name("Date");
            Map(m => m.draftStatus).Index(9).Name("DraftStatus");

        }
    }
}