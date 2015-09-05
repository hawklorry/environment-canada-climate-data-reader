using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HAWKLORRY.HuzelnutSuitability
{
    class DailyTemperatureStatisticsMultipleYear
    {
        private int _startYear;
        private int _endYear;
        private ECStationInfo _station;

        public DailyTemperatureStatisticsMultipleYear(int startYear, int endYear, ECStationInfo station)
        {
            _startYear = startYear;
            _endYear = endYear;
            _station = station;
        }

        private double getCriteriaStandard(HuzelnutSuitabilityCriteriaType type)
        {
            switch(type)
            {
                case HuzelnutSuitabilityCriteriaType.Avg: return 16.7;
                case HuzelnutSuitabilityCriteriaType.Forstf: return 120;
                default: return 0;
            }
        }

        public string getCriteriaString(HuzelnutSuitabilityCriteriaType type)
        {
            List<double> criteria = new List<double>();
            StringBuilder sb = new StringBuilder();
            sb.Append(_station.ID);
            for (int year = _startYear; year <= _endYear; year++)
            {
                double criteria_oneyear = _station.getCriteria(year, type);
                criteria.Add(criteria_oneyear);
                sb.Append(",");
                sb.Append(criteria_oneyear.Equals(double.MinValue) ? "" : criteria_oneyear.ToString());
            }

            //get the standard
            if(type > HuzelnutSuitabilityCriteriaType.Sf_W16)
            {
                sb.Append(",");

                List<double> newList = criteria.Where(item => !item.Equals(double.MinValue)).ToList();
                if (newList.Count == 0)
                    sb.Append("");
                else if (newList.Count() == newList.Count(item => item > getCriteriaStandard(type)))
                    sb.Append(1);
                else
                    sb.Append(0);
            }
            return sb.ToString();
        }         
    }
}
