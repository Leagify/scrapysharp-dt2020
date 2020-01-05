using System;

namespace scrapysharp_dt2020
{

    public class ProspectRanking
    {
        public int rank;
        public string change;
        public string playerName;
        public string school;
        public string position1;
        public string height;
        public int weight;
        public string collegeClass;
        public DateTime rankingDate;
        public string rankingDateString;
        public string draftStatus;

        public ProspectRanking(DateTime date, int rank, string chg, string name, string school, string pos1, string height = "0", int weight = 0, string year = "", string draftstatus = "")
        {
            this.rankingDate = date;
            this.rank = rank;
            this.change = chg;
            this.playerName = name;
            this.school = school;
            this.position1 = pos1;
            this.height = height;
            this.weight = weight;
            this.collegeClass = year;
            this.rankingDateString = date.ToString("yyyy-MM-dd");
            this.draftStatus = draftstatus;
        }
    }
}