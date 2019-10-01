using CsvHelper.Configuration;

namespace scrapysharp_dt2020
{
    public class ProspectRankSimple
    {
        public int rank;
        public string playerName;
        public string school;
        public string rankingDateString;

        public ProspectRankSimple() {}
        public ProspectRankSimple(int rank, string name, string school, string rankingDate)
        {
            this.rank = rank;
            this.playerName = name;
            this.school = school;
            this.rankingDateString = rankingDate;
        }
    }

    public sealed class ProspectRankSimpleCsvMap : ClassMap<ProspectRankSimple>
    {
        public ProspectRankSimpleCsvMap()
        {
            Map(m => m.rank).Name("Rank");
            Map(m => m.playerName).Name("Player");
            Map(m => m.school).Name("School");
            Map(m => m.rankingDateString).Name("Date");
        }
    }
}