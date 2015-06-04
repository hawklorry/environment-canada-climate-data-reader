As I was working on some watersheds in Canada, I realized that the climate data preparation is time-consuming and error-prone. To solve the problem, I created this tool to automatically download the climate data from Environment Canada website and generate climate data file in several format which could be directly used in hydrological models. More information could be found on my blog: http://wp.me/s2CzBq-325.

This project is a .NET 4.0 windows program. Three projects are included.

1. EnvironmentCanadaClimateData is the main project. The core function is in class Station. The private function retrieveAnnualDailyClimateData utilize HttpWebResponse to download the climate data in CSV format from http://climate.weather.gc.ca.

2. FastDBF is a .NET library developed by Ahmed Lacevic to read and write dbf files. Please find more information at http://fastdbf.codeplex.com/.

3. LumenWorks.Framework.IO is a .NET library developed by Sébastien Lorion to read csv files. Please find more information at http://www.codeproject.com/Articles/9258/A-Fast-CSV-Reader.