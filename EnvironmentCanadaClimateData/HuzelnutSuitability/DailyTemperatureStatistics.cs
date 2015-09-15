using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HAWKLORRY.HuzelnutSuitability
{
    class DailyTemperatureStatisticsOneYear
    {
        private List<DailyTemperature> _temps = null;
        private int _year = 2000;

        public DailyTemperatureStatisticsOneYear(int year, List<DailyTemperature> temps)
        {
            _temps = temps;
            _year = year;
        }


        /// <summary>
        /// The number of valid data points
        /// </summary>
        public double NumberOfValidData
        {
            get
            {
                if (_temps.Count == 0) return double.MinValue; //missing data all year
                return _temps.Count(item => item.HasValue);
            }
        }

        /// <summary>
        /// Lowest min temperature
        /// </summary>
        public double LowestMin
        {
            get
            {
                if (_temps.Count == 0) return double.MinValue;//missing value
                return _temps.Min(tmp => tmp.Min);
            }
        }

        /// <summary>
        /// The average temp from May 1st to September 30
        /// </summary>
        public double Average
        {
            get 
            {
                if (_temps.Count == 0) return double.MinValue;//missing value
                IEnumerable<DailyTemperature> temps_in_range = _temps.Where(tmp => tmp.HasValue && tmp.Day.Month >= 5 && tmp.Day.Month <= 9);
                if (temps_in_range.Count() == 0) return double.MinValue;
                return temps_in_range.Average(tmp => tmp.Ave);
            }
        }

        /// <summary>
        /// Number of days with temperature continuously larger than -2
        /// </summary>
        /// <remarks>continuously is the key</remarks>
        public double NumDayofFrostFree
        {
            get
            {
                if (_temps.Count == 0) return double.MinValue; //missing value

                int days = 0;
                foreach(DailyTemperature tmp in _temps)
                {
                    if (!tmp.HasValue) continue;
                    if (tmp.Min > -2.0) days++;
                    if (tmp.Min < -2.0 && tmp.Day.Month <= 7) days = 0;  //use July to separate the spring and fall
                    if (tmp.Min < -2.0 && tmp.Day.Month > 7) break;     //already going to the fall
                }
                return days;
            }
        }

        /// <summary>
        /// Number of days with min temperature less thant -2
        /// </summary>
        /// <param name="week">Week between 1 to 16, starting from March 1st</param>
        /// <returns></returns>
        public double NumberofFrostInWeek(int week)
        {
            if (_temps.Count == 0) return double.MinValue; //missing value

            DateTime startingDay = new DateTime(_year, 5, 1);
            DateTime firstDay = startingDay.AddDays((week - 1) * 7);
            DateTime lastDay = firstDay.AddDays(7);
            if (_temps.Count(tmp => tmp.HasValue && tmp.Day >= firstDay && tmp.Day <= lastDay) == 0) return double.MinValue; //no data points in this week, treat it as missing value

            return _temps.Where(tmp => tmp.HasValue && tmp.Day >= firstDay && tmp.Day <= lastDay).Count(tmp => tmp.Min < -2.0);
        }

        /// <summary>
        /// Number of days with min temperature less than -40
        /// </summary>
        public double NumberofLowTemp40
        {
            get
            {
                if (_temps.Count == 0) return double.MinValue; //missing value
                return _temps.Where(tmp => tmp.HasValue).Count(tmp => tmp.Min <= -40);
            }
        }

        /// <summary>
        /// Number of days with min temperature less than -28
        /// </summary>
        public double NumberofLowTemp28
        {
            get
            {
                if (_temps.Count == 0) return double.MinValue; //missing value
                return _temps.Where(tmp => tmp.HasValue).Count(tmp => tmp.Min <= -28);
            }
        }
    }
}
