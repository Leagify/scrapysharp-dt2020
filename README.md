# scrapysharp-dt2020

This is a webscraper that uses [ScrapySharp](https://github.com/rflechner/ScrapySharp) to obtain american football draft prospect information.

Output is stored as a CSV file, made with the help of LINQ and [CSVHelper](https://joshclose.github.io/CsvHelper/).

Some limited error checking is included to verify whether the school matches up to a pre-existing list of schools and the states they are found.

This program is written in .NET Core 3.1.

Part of this project was made possible by the help of Hacktoberfest.


[![Open in Gitpod](https://gitpod.io/button/open-in-gitpod.svg)](https://gitpod.io#https://github.com/Leagify/scrapysharp-dt2020
)

Note- I assume that you are going to run this in GitPod (which runs Ubuntu 18.04), so it uses the `linux-x64` by default.  If you're in Windows, make sure to use the `-r win-x64` parameter when building or running, since the .csproj file has a linux runtime set by default.
