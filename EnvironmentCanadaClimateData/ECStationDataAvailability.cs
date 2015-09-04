using System;
using System.Linq;
using HtmlAgilityPack;
using DotSpatial.Data;
using DotSpatial.Topology;

namespace HAWKLORRY
{
    class ECStationDataAvailability
    {
        private static string[] INTERVAL_NAME_IN_HTML = { "hlyRange", "dlyRange", "mlyRange" };
        private ECDataIntervalType _intervalType = ECDataIntervalType.DAILY;
        private bool _isAvailable = false;
        private string _firstDay = "";
        private int _firstYear = 9999;
        private string _lastDay = "";
        private bool _isValid = false;
        private int _lastYear = 9999;

        /// <summary>
        /// Initialize with htmlNode corresponding to hidden input tag
        /// </summary>
        /// <param name="inputHiddenNode"></param>
        public ECStationDataAvailability(HtmlNode inputHiddenNode)
        {
            string dataRangeType = "";
            string dataRange = "";
            ECHtmlUtil.ReadInputHiddenNode(inputHiddenNode,
                out dataRangeType, out dataRange);

            if (dataRangeType.Length == 0 || !INTERVAL_NAME_IN_HTML.Contains(dataRangeType)) return;

            _isValid = true;
            _intervalType = (ECDataIntervalType)(Array.IndexOf(INTERVAL_NAME_IN_HTML, dataRangeType) + 1);

            string[] range = dataRange.Split('|');
            if (range.Length == 2)
            {
                _firstDay = range[0].Trim();
                _lastDay = range[1].Trim();
                _isAvailable = _firstDay.Length > 0 || _lastDay.Length > 0;
                readFirstLastYear();
            }
        }

        public ECStationDataAvailability(ECDataIntervalType type, string firstDay, string lastDay)
        {
            _intervalType = type;
            _isValid = true;
            if (firstDay.Length == 0 || firstDay == "null" ||
                lastDay.Length == 0 || lastDay == "null")
                return;

            _isAvailable = true;
            _firstDay = DateTime.Parse(firstDay).ToShortDateString();
            _lastDay = DateTime.Parse(lastDay).ToShortDateString();
            readFirstLastYear();
        }

        private void readFirstLastYear()
        {
            if (IsAvailable)
            {
                DateTime d;
                if (DateTime.TryParse(_firstDay, out d)) _firstYear = d.Year;
                if (DateTime.TryParse(_lastDay, out d)) _lastYear = d.Year;
            }
        }

        public bool IsAvailable { get { return _isValid && _isAvailable; } }
        public string FirstDay { get { return _firstDay; } }
        public string LastDay { get { return _lastDay; } }
        public int FirstYear { get { return _firstYear; } }
        public int LastYear { get { return _lastYear; } }

        public bool IsAvailableForYear(int year)
        {
            if (!IsAvailable) return false;
            if (FirstYear > year || LastYear < year) return false;

            DateTime firstDay_TestYear = new DateTime(year, 1, 1);
            DateTime lastDay_TestYear = new DateTime(year, 12, 31);
            DateTime firstDay = DateTime.Parse(_firstDay);
            DateTime lastDay = DateTime.Parse(_lastDay);

            return firstDay_TestYear >= firstDay && lastDay_TestYear <= lastDay;
        }

        public override string ToString()
        {
            if (!IsAvailable) return _intervalType + " data not available";
            return string.Format("Type={2},FirstDay={0},LastDay={1}", _firstDay, _lastDay, _intervalType);
        }
        public string ToCSVString()
        {
            if (!IsAvailable) return "null,null";
            return string.Format("{0},{1}", _firstDay, _lastDay);
        }
        public string ToTimeRangeString()
        {
            if (!IsAvailable) return "";

            return string.Format("{2}: From {0} To {1}", _firstDay, _lastDay, _intervalType);
        }
    }
}
