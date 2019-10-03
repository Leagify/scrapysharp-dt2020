# scrapysharp-dt2020

This is a webscraper that uses [ScrapySharp](https://github.com/rflechner/ScrapySharp) to obtain american football draft prospect information.

Output is stored as a CSV file, made with the help of LINQ and [CSVHelper](https://joshclose.github.io/CsvHelper/).

Some limited error checking is included to verify whether the school matches up to a pre-existing list of schools and the states they are found.

This program is written in .NET Core 3.0.

Part of this project was made possible by the help of Hacktoberfest.