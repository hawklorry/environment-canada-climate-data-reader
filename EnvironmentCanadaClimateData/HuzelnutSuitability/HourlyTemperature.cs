using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using LumenWorks.Framework.IO.Csv;
using System.Data;

namespace HAWKLORRY.HuzelnutSuitability
{
    /// <summary>
    /// Hourly temperature data
    /// </summary>
    class HourlyTemperature
    {
        private DateTime _time;
        private double? _temp = null;

        public DateTime Time { get { return _time; } }
        public double? Temperature { set { _temp = value; } get { if (_temp.HasValue) return _temp.Value; else return null; } }
        public bool HasValue { get { return _temp.HasValue; } }

        private static List<HourlyTemperature> GetInitialTemperatureForMonth(int year, int month)
        {
            List<HourlyTemperature> temps = new List<HourlyTemperature>();

            DateTime baseTime = new DateTime(year, month, 1);
            int numberOfHours = DateTime.DaysInMonth(year, month) * 24;

            for (int i = 1; i <= numberOfHours; i++)
            {
                temps.Add(new HourlyTemperature(baseTime.AddHours(i - 1)));
            }
            return temps;
        }

        public static List<HourlyTemperature> GetTemperatureForMonth(string stationID, int year, int month)
        {
            List<HourlyTemperature> temps = GetInitialTemperatureForMonth(year, month);

            string ECHourlyCSVString = ECRequestUtil.RequestHourlyClimateData(stationID, year, month, true, true);
            if (string.IsNullOrEmpty(ECHourlyCSVString)) return temps;

            using (CachedCsvReader csv = new CachedCsvReader(new StringReader(ECHourlyCSVString), true))
            {
                csv.DefaultParseErrorAction = ParseErrorAction.AdvanceToNextLine;
                csv.MissingFieldAction = MissingFieldAction.ReplaceByEmpty; //treat missing value

                if (csv.FieldCount < 25) return temps;

                DateTime currentTime = DateTime.Now;
                int line = 0;
                while (csv.ReadNextRecord())
                {
                    if (string.IsNullOrEmpty(csv[0]) || string.IsNullOrEmpty(csv[6])) continue;

                    currentTime = DateTime.Parse(csv[0]);
                    currentTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, 0, 0);
                    var currentTemp = temps.Where(temp => temp.Time.Equals(currentTime));
                    HourlyTemperature t = currentTemp.First<HourlyTemperature>();

                    t.Temperature = double.Parse(csv[6]);
                    line++;
                }
            }

            return temps;
        }

        public HourlyTemperature(DateTime time)
        {
            _time = time;
        }
    }
}
