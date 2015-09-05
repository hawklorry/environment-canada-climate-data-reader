using System;
using System.Text;
using System.Net;
using System.IO;

namespace HAWKLORRY
{
    class ECRequestUtil
    {
        private static string DOMAIN = "http://climate.weather.gc.ca";

        private static int HEADER_LINE_HOURLY = 17;
        private static int HEADER_LINE_DAILY = 26;

        /// <summary>
        /// hourly and daily is defined by timefram
        /// </summary>
        private static string DATA_REQUEST_URL_FORMAT =
            DOMAIN +
            "/climateData/bulkdata_e.html?" +
            "format=csv&stationID={0}&Year={1}&Month={2}&Day=1&timeframe={3}&submit=Download+Data";

        private static string[] SEARCH_TYPE = { "stnName", "stnProv" };
        private static string STATION_NAME_SEARCH_FORMAT = "txtStationName={0}&searchMethod=contains&";
        private static string SEARCH_FORMAT =
            DOMAIN +
            "/advanceSearch/searchHistoricDataStations_e.html?" +
            "searchType={0}&timeframe=1&{1}" +
            "optLimit=yearRange&StartYear=1840&EndYear={2}&Year={2}&Month={3}&Day={4}&" +
            "selRowPerPage={6}&cmdStnSubmit=Search&startRow={5}";


        /// <summary>
        /// to request daily report for given station
        /// </summary>
        private static string DAILY_REPORT_FORMAT =
            DOMAIN + "/climateData/dailydata_e.html?timeframe=2&StationID={0}";//to get latitude,Longitude and elevation

        private static string sendRequest(string requestURL)
        {
            HttpWebRequest r = WebRequest.Create(requestURL) as HttpWebRequest;
            r.Method = "GET";            
            using (HttpWebResponse response = r.GetResponse() as HttpWebResponse)
            {
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }

        public static string RequestAllStations(int numInOnePage, int startRow)
        {
            return sendRequest
                (string.Format(SEARCH_FORMAT,
                "stnProv", "", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, startRow, numInOnePage));
        }

        /// <summary>
        /// Search station using station name to get its information
        /// </summary>
        /// <param name="stationName"></param>
        /// <returns></returns>
        public static string RequestOneStation(string stationName)
        {
            return sendRequest
               (string.Format(SEARCH_FORMAT,
                "stnProv",
                string.Format(STATION_NAME_SEARCH_FORMAT, stationName),
                DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 1, 25));
        }

        /// <summary>
        /// request daily report for given station
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string RequestLatLongElevation(string id)
        {
            return sendRequest
                (string.Format(DAILY_REPORT_FORMAT, id));
        }

        /// <summary>
        /// request climate data
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        private static string RequestClimateData(string stationID, int year, int month,
            ECDataIntervalType interval, bool keepHeader = true, bool savedCacheFile = false)
        {
            //read from cache if it exists
            string cache = getCachePath(stationID, interval, year, month);
            if (File.Exists(cache))
            {
                using (StreamReader reader = new StreamReader(cache))
                {
                    return reader.ReadToEnd();
                }
            }

            //request from website if the cache file doesn't exist
            string csv = sendRequest(
                string.Format(DATA_REQUEST_URL_FORMAT, stationID, year, month, Convert.ToInt32(interval)));

            System.Text.StringBuilder sb = new StringBuilder();
            using (StringReader reader = new StringReader(csv))
            {
                int lineNum = 0;
                int headLine = HEADER_LINE_DAILY;
                if (interval == ECDataIntervalType.HOURLY)
                    headLine = HEADER_LINE_HOURLY;
                if (!keepHeader) headLine += 1;
                while (reader.Peek() >= 0)
                {
                    string line = reader.ReadLine();
                    lineNum++;

                    if (lineNum == 1 && !line.ToLower().Contains("station name")) break; //no data for this year
                    if (lineNum < headLine) continue;

                    sb.AppendLine(line);
                    sb.AppendLine(reader.ReadToEnd()); //read all other contents
                    break;
                }
            }

            //save to cache file even nothing is there to avoid request to server next time
            if (savedCacheFile)
            {
                using (StreamWriter writer = new StreamWriter(cache))
                {
                    writer.Write(sb);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// request hourly climate data
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public static string RequestHourlyClimateData(string stationID, int year, int month, bool keepHeader = true, bool savedCacheFile = false)
        {
            return RequestClimateData(stationID, year, month, ECDataIntervalType.HOURLY, keepHeader, savedCacheFile);
        }

        /// <summary>
        /// request annual climate data
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public static string RequestAnnualClimateData(string stationID, int year, ECDataIntervalType timeInterval)
        {
            if (timeInterval == ECDataIntervalType.HOURLY) return RequestAnnualHourlyClimateData(stationID, year);
            if (timeInterval == ECDataIntervalType.DAILY) return RequestAnnualDailyClimateData(stationID, year);
            return "";
        }

        /// <summary>
        /// request annual hourly climate data
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public static string RequestAnnualHourlyClimateData(string stationID, int year)
        {
            System.Text.StringBuilder sb = new StringBuilder();
            for (int i = 1; i <= 12; i++)
                sb.AppendLine(RequestHourlyClimateData(stationID, year, i, i == 1));
            return sb.ToString();
        }

        /// <summary>
        /// request annual daily climate data
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public static string RequestAnnualDailyClimateData(string stationID, int year, bool keepHeader = true, bool savedCacheFile = false)
        {
            return RequestClimateData(stationID, year, 8, ECDataIntervalType.DAILY, keepHeader, savedCacheFile);
        }

        public static string getCachePath(string stationID, ECDataIntervalType interval, int year, int month)
        {
            //create the cache folder if necessary
            string cache = Path.Combine(CACHE_PATH, stationID);
            if (!Directory.Exists(cache)) Directory.CreateDirectory(cache);
            cache = Path.Combine(cache, interval.ToString().ToLower());
            if (!Directory.Exists(cache)) Directory.CreateDirectory(cache);

            //save to the file
            cache = Path.Combine(cache,
                interval == ECDataIntervalType.DAILY ? string.Format("{0}.csv", year) : string.Format("{0}_{1}.csv", year, month));

            return cache;
        }

        private static string CACHE_PATH = CachePath;

        private static string CachePath
        {
            get
            {
                string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\cache\";
                if (!System.IO.Directory.Exists(path))
                {
                    try
                    {
                        System.IO.Directory.CreateDirectory(path);
                    }
                    catch (System.Exception e)
                    {
                        throw e;
                    }
                }
                return path;
            }
        }

    }
}
