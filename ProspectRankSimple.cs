namespace scrapysharp_dt2020
{
    public class ProspectRankSimple
    {
        public int rank;
        public string playerName;
        public string school;
        public string rankingDateString;

        public ProspectRankSimple(int rank, string name, string school, string rankingDate)
        {
            this.rank = rank;
            this.playerName = name;
            this.school = school;
            this.rankingDateString = rankingDate;
        }
    }
}