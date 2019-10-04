using CsvHelper.Configuration;

namespace scrapysharp_dt2020
{
	public class Region
	{
		public string state;
		public string region;

		public Region () { }
		public Region (string state, string region)
		{
			this.region = region;
			this.state = state;
		}
	}

	public sealed class RegionCsvMap : ClassMap<Region>
	{
		public RegionCsvMap ()
		{
			Map(m => m.state).Name("State");
			Map(m => m.region).Name("Region");
		}
	}
}
