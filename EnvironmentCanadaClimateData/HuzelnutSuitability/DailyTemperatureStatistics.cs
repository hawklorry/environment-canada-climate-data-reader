using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HAWKLORRY.HuzelnutSuitability
{
    class DailyTemperatureStatistics
    {
        private List<DailyTemperature> _temps = null;
        private int _year = 2000;

        public DailyTemperatureStatistics(int year, List<DailyTemperature> temps)
        {
            _temps = temps;
            _year = year;
        }

        public double Average
        {
            get 
            {
                return _temps.Where(tmp=>tmp.HasValue).Average(tmp => tmp.Ave);
            }
        }

        public int NumDayofFrostFree
        {
            get
            {
                return _temps.Where(tmp => tmp.HasValue).Count(tmp => tmp.Ave > -2.0);
            }
        }

        /// <summary>
        /// Number of days with temperature less thant -2
        /// </summary>
        /// <param name="week">Week between 1 to 16, starting from March 1st</param>
        /// <returns></returns>
        public int NumberofFrostInWeek(int week)
        {
            DateTime startingDay = new DateTime(_year, 5, 1);
            DateTime firstDay = startingDay.AddDays((week - 1) * 7);
            DateTime lastDay = firstDay.AddDays(7);
            return _temps.Where(tmp => tmp.HasValue && tmp.Day >= firstDay && tmp.Day <= lastDay).Count(tmp => tmp.Ave < -2.0);
        }

        public int NumberofLowTemp
        {
            get
            {
                return _temps.Where(tmp => tmp.HasValue).Count(tmp => tmp.Min < 40);
            }
        }
    }
}
