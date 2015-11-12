using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using LumenWorks.Framework.IO.Csv;
using System.Data;

namespace HAWKLORRY.HuzelnutSuitability
{
    /// <summary>
    /// Daily temperature data
    /// </summary>
    class DailyTemperature
    {
        private DateTime _day;
        private double? _min = null;
        private double? _max = null;
        private double? _ave = null;
        private bool _fromHourly = false;
        
        public DateTime Day { get { return _day; } }
        public double Min { set { _min = value; } get { return _min.HasValue ? _min.Value : -99.0; } }
        public double Max { set { _max = value; } get { return _max.HasValue ? _max.Value : -99.0; } }
        public double Ave { set { _ave = value; } get { return _ave.HasValue ? _ave.Value : -99.0; } }
        public bool FromHourly { set { _fromHourly = value; } }
        public bool HasValue { get { return _min.HasValue && _max.HasValue && _ave.HasValue; } }

        private static List<DailyTemperature> GetInitialTemperatureForYear(int year)
        {
            List<DailyTemperature> temps = new List<DailyTemperature>();
            int numberOfDays = 365;
            if (DateTime.IsLeapYear(year)) numberOfDays = 366;
            DateTime baseDay = new DateTime(year - 1, 12, 31);
            for (int day = 1; day <= numberOfDays; day++)
            {
                temps.Add(new DailyTemperature(baseDay.AddDays((double)day)));
            }
            return temps;
        }

        public static List<DailyTemperature> GetTemperatureForYearHourlyFirst(string stationID, int year)
        {
            //get daily temperature first
            List<DailyTemperature> dailyTemps = GetTemperatureForYear(stationID, year);

            //see if there are some hourly data
            for (int month = 1; month <= 12; month++)
            {
                List<HourlyTemperature> hourlyTemps = HourlyTemperature.GetTemperatureForMonth(stationID, year, month);

                int numberOfDays = DateTime.DaysInMonth(year, month);
                for (int day = 1; day <= numberOfDays; day++)
                {
                    //get all hourly temperature data in current day
                    List<HourlyTemperature> tempsInCurrentDay =
                        hourlyTemps.Where(temps => temps.Time.Day == day).ToList();

                    //create the daily temperature from hourly temperatuer data
                    DailyTemperature newDailyTempFromHourly = new DailyTemperature(tempsInCurrentDay);

                    //replace the daily temperature with the one derived from hourly data
                    if (newDailyTempFromHourly.HasValue)
                    {
                        DailyTemperature currentDailyTemp =
                            dailyTemps.Where(temps => temps.Day.Equals(newDailyTempFromHourly.Day)).ToList()[0];
                        int index = dailyTemps.IndexOf(currentDailyTemp);
                        dailyTemps.RemoveAt(index);
                        dailyTemps.Insert(index, newDailyTempFromHourly);
                    }
                }
            }
            return dailyTemps;
        }

        private static List<DailyTemperature> GetTemperatureForYear(string stationID, int year)
        {
            List<DailyTemperature> temps = GetInitialTemperatureForYear(year);

            string ECDailyCSVString = ECRequestUtil.RequestAnnualDailyClimateData(stationID, year, true, true);
            if (string.IsNullOrEmpty(ECDailyCSVString)) return temps;

            using (CachedCsvReader csv = new CachedCsvReader(new StringReader(ECDailyCSVString), true))
            {
                csv.DefaultParseErrorAction = ParseErrorAction.AdvanceToNextLine;
                csv.MissingFieldAction = MissingFieldAction.ReplaceByEmpty; //treat missing value

                if (csv.FieldCount < 27) return temps;

                DateTime currentDay = DateTime.Now;
                while (csv.ReadNextRecord())
                {
                    if (string.IsNullOrEmpty(csv[0]) || string.IsNullOrEmpty(csv[5]) ||
                        string.IsNullOrEmpty(csv[7]) || string.IsNullOrEmpty(csv[9]))
                        continue;

                    //ignore 99 and -99
                    //some stations has data like this, like 4905 daily 12/27/2013 max temp = -99                    /
                    if (csv[5].Trim().Equals("99") || csv[7].Trim().Equals("99") || csv[9].Trim().Equals("99") ||
                        csv[5].Trim().Equals("-99") || csv[7].Trim().Equals("-99") || csv[9].Trim().Equals("-99"))
                        continue;

                    currentDay = DateTime.Parse(csv[0]);
                    var currentTemp = temps.Where(temp => temp.Day.Equals(currentDay));
                    DailyTemperature t = currentTemp.First<DailyTemperature>();

                    t.Max = double.Parse(csv[5]);
                    t.Min = double.Parse(csv[7]);
                    if(t.Min > t.Max)
                    {
                        double temp = t.Max;
                        t.Max = t.Min;
                        t.Min = temp;
                    }
                    t.Ave = double.Parse(csv[9]);
                }
            }

            return temps;
        }

        /// <summary>
        /// Get daily temperature from the cache file for analysis
        /// </summary>
        /// <param name="cacheFile"></param>
        /// <returns></returns>
        public static List<DailyTemperature> FromCacheFile(string cacheFile)
        {
            List<DailyTemperature> temps = new List<DailyTemperature>();
            if (!File.Exists(cacheFile)) return temps;
            using (StreamReader reader = new StreamReader(cacheFile))
            {
                using (CachedCsvReader csv = new CachedCsvReader(new StringReader(reader.ReadToEnd()), true))
                {
                    while (csv.ReadNextRecord())
                    {
                        if (string.IsNullOrEmpty(csv[1]) || string.IsNullOrEmpty(csv[2]) ||
                            string.IsNullOrEmpty(csv[3]))
                            continue;

                        //ignore 99 and -99
                        //some stations has data like this, like 4905 daily 12/27/2013 max temp = -99
                        //this would cause some normal line is ignored, like ave temp = 17.99
                        //if (csv[1].Contains("99") || csv[2].Contains("99") || csv[3].Contains("99"))
                        //    continue;

                        DailyTemperature t = new DailyTemperature(DateTime.Parse(csv[0]));
                        t.Min = double.Parse(csv[1]);
                        t.Max = double.Parse(csv[2]);
                        if (t.Min > t.Max)
                        {
                            double temp = t.Max;
                            t.Max = t.Min;
                            t.Min = temp;
                        }
                        t.Ave = double.Parse(csv[3]);
                        t.FromHourly = bool.Parse(csv[4]);

                        //ignore 99 and -99
                        if (t.Min == 99 || t.Min == -99 ||
                            t.Max == 99 || t.Max == -99 ||
                            t.Ave == 99 || t.Ave == -99) continue;

                        temps.Add(t);
                    }
                }
            }
            return temps;

        }

        public DailyTemperature(DateTime day)
        {
            _day = day;
        }

        public DailyTemperature(List<HourlyTemperature> hourlyTemp)
        {
            if (hourlyTemp.Count != 24)
                throw new Exception("One day should have 24 hours!");
            if (hourlyTemp.Count(temp => temp.HasValue) == 0) return; //no valid data today

            _day = hourlyTemp[0].Time.Date;
            _min = hourlyTemp.Min(temp => temp.Temperature);
            _max = hourlyTemp.Max(temp => temp.Temperature);
            _ave = hourlyTemp.Average(temp => temp.Temperature);
            _fromHourly = true;
        }

        public override string ToString()
        {
            if (HasValue)
                return string.Format("{0:yyyy-MM-dd},{1},{2},{3},{4}", _day, _min, _max, _ave, _fromHourly);
            else
                return string.Format("{0:yyyy-MM-dd},,,,false", _day);
        }

        public static string HEADER = "day,min,max,ave,hourly";
    }
}
