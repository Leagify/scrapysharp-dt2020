using CsvHelper.Configuration;

namespace scrapysharp_dt2020
{
    public class School
    {
        public string schoolName;
        public string conference;
        public string state;

        public School () {}
        public School (string schoolName, string conference, string state)
        {
            this.schoolName = schoolName;
            this.conference = conference;
            this.state = state;
        }
    }

    public sealed class SchoolCsvMap : ClassMap<School>
    {
        public SchoolCsvMap()
        {
            Map(m => m.schoolName).Name("School");
            Map(m => m.conference).Name("Conference");
            Map(m => m.state).Name("State");
        }
    }
}
