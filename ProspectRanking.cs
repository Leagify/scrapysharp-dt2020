using System;

namespace scrapysharp_dt2020
{

    class ProspectRanking
    {
        public int rank;
        public string change;
        public string playerName;
        public string school;
        public string position1;
        public string height;
        public int weight;
        public string position2;
        public DateTime rankingDate;

        public ProspectRanking(DateTime date, int rank, string chg, string name, string school, string pos1, string height = "0", int weight = 0, string pos2 = "")
        {
            this.rankingDate = date;
            this.rank = rank;
            this.change = chg;
            this.playerName = name;
            this.school = school;
            this.position1 = pos1;
            this.height = height;
            this.weight = weight;
            this.position2 = pos2;
        }

    }
}