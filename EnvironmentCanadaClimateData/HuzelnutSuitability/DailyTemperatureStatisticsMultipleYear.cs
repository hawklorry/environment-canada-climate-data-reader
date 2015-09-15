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
            bool hasData = false;   //if the station has data in given year range
            sb.Append(_station.ID);
            
            for (int year = _startYear; year <= _endYear; year++)
            {
                double criteria_oneyear = _station.getCriteria(year, type);
                criteria.Add(criteria_oneyear);
                sb.Append(",");
                sb.Append(criteria_oneyear.Equals(double.MinValue) ? "" : criteria_oneyear.ToString());
                if (!hasData) hasData = !criteria_oneyear.Equals(double.MinValue);
            }

            if (!hasData) return string.Empty;

            //get the standard        
            else if(type > HuzelnutSuitabilityCriteriaType.Sf_W16 && type != HuzelnutSuitabilityCriteriaType.Num)
            {
                sb.Append(",");

                List<double> newList = criteria.Where(item => !item.Equals(double.MinValue)).ToList();
                if (newList.Count == 0)                                     //all empty
                    sb.Append("");
                else if (type == HuzelnutSuitabilityCriteriaType.Lowest)    //lowest min
                    sb.Append(newList.Min());
                else if (type == HuzelnutSuitabilityCriteriaType.Forstf || 
                        type == HuzelnutSuitabilityCriteriaType.Avg)
                {
                    //all year must meet the standard
                    if (newList.Count() == newList.Count(item => item > getCriteriaStandard(type)))
                        sb.Append(1);
                    else
                        sb.Append(0);
                }
                else if (type == HuzelnutSuitabilityCriteriaType.Stemp||
                        type == HuzelnutSuitabilityCriteriaType.Ltemp)
                {
                    //only require one year to meet the standard
                    if (newList.Count(item => item > getCriteriaStandard(type)) > 0)
                        sb.Append(1);
                    else
                        sb.Append(0);
                }

                //add number of years with ave temp > 16.7
                if (type == HuzelnutSuitabilityCriteriaType.Avg)
                {
                    sb.Append(",");
                    sb.Append(newList.Count(item => item > getCriteriaStandard(type)));
                }
            }

            return sb.ToString();
        }         
    }
}
