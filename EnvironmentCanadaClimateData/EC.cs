using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using System.Diagnostics;
using HtmlAgilityPack;
using LumenWorks.Framework.IO.Csv;
using System.Data;
using System.Data.OleDb;
using SocialExplorer.IO.FastDBF;
using DotSpatial.Data;
using DotSpatial.Topology;

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
                string.Format(STATION_NAME_SEARCH_FORMAT,stationName),
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
                (string.Format(DAILY_REPORT_FORMAT,id));
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
        public static string RequestHourlyClimateData(string stationID, int year, int month, bool keepHeader = true,bool savedCacheFile = false)
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
                sb.AppendLine(RequestHourlyClimateData(stationID,year,i,i == 1));
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

    /// <summary>
    /// Environment Canada website
    /// </summary>
    class EC
    {
        private static string STATIONS_CSV_HEADER =
            "ID,NAME,PROVINCE,LATITUDE,LONGITUDE,ELEVATION,HOURLY_FIRST_DAY,HOURLY_LAST_DAY,DAILY_FIRST_DAY,DAILY_LAST_DAY,MONTHLY_FIRST_DAY,MONTHLY_LAST_DAY";

        /// <summary>
        /// Save given stations to given file. Used to save user defined station list
        /// </summary>
        /// <param name="csvFilePath"></param>
        /// <param name="stations"></param>
        public static void SaveStations(string csvFilePath,
            List<ECStationInfo> stations)
        {
            if (stations == null || stations.Count == 0) return;

            using (StreamWriter writer = new StreamWriter(csvFilePath))
            {
                writer.WriteLine(STATIONS_CSV_HEADER);

                foreach (ECStationInfo info in stations)
                    writer.WriteLine(info.ToCSVString());
            }
        }

        /// <summary>
        /// Save given stations to temp folder. Used to automatically save the selected stations.
        /// </summary>
        /// <param name="stations"></param>
        public static void SaveStations(List<ECStationInfo> stations)
        {            
            SaveStations(GetSavedSelectedStationCSVFile(), stations);
        }

        /// <summary>
        /// get automatically saved stations
        /// </summary>
        public static List<ECStationInfo> SavedStations
        {
            get
            {
                return ReadStations(GetSavedSelectedStationCSVFile());
            }
        }

        /// <summary>
        /// read stations from given csv file
        /// </summary>
        /// <param name="csv"></param>
        /// <returns></returns>
        public static List<ECStationInfo> ReadStations(string csv)
        {
            List<ECStationInfo> stations = new List<ECStationInfo>();            
            if (!System.IO.File.Exists(csv)) return stations;

            try
            {
                DataTable dt = ReadCSV(csv);
                stations = ECStationInfo.FromCSVDataRows(dt.Select());
                return stations;
            }
            catch
            {
                return stations;
            }
        }

        /// <summary>
        /// Retrieve all stations from EC and save into a csv file
        /// </summary>
        /// <param name="csvFilePath"></param>
        public static void RetrieveAndSaveAllStations(string csvFilePath,
            System.ComponentModel.BackgroundWorker worker = null)
        {
            using (StreamWriter writer = new StreamWriter(csvFilePath))
            {
                writer.WriteLine(STATIONS_CSV_HEADER);
 
                int num = 100;
                int startRow = 1;
                List<ECStationInfo> stations = new List<ECStationInfo>();

                do
                {
                    string request = ECRequestUtil.RequestAllStations(num, startRow);
                    stations = ECStationInfo.FromEC(request,worker);

                    foreach (ECStationInfo info in stations)
                        writer.WriteLine(info.ToCSVString());

                    startRow += num;
                } while (stations.Count > 0); 
            }
        }

        private static string FILE_NAME_EC_STATIONS_CSV = "ecstations_with_timeRange.csv";
        private static string FILE_NAME_SELECTED_STATIONS_CSV = "ecstations_selected.csv";
        private static string FILE_NAME_EC_STATIONS_ZIP = "EC_Stations.zip"; //this is the shapefile
        private static string FILE_NAME_EC_STATIONS_SHAPEFILE_IN_BOUNDARY = "stationsInBoundary.shp"; //this is the shapefile

        public static string GetStationsInBoundary()
        {
            string path = System.IO.Path.GetTempPath();
            path += @"ECReader\";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            path += @"StationsInBoundary\";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path + FILE_NAME_EC_STATIONS_SHAPEFILE_IN_BOUNDARY;
        }

        /// <summary>
        /// get default ecstations.csv in system temp folder. If doesn't exist, create using the resource file.
        /// </summary>
        /// <returns></returns>
        public static string GetAllStationShapeFile()
        {
            string path = System.IO.Path.GetTempPath();
            path += @"ECReader\";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            string file = path + FILE_NAME_EC_STATIONS_ZIP;

            if (!System.IO.File.Exists(file))
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(file, FileMode.Create)))
                    writer.Write(Properties.Resources.EC_Stations);

                //try to extrac the shapefile
                using (Ionic.Zip.ZipFile zip = Ionic.Zip.ZipFile.Read(file))
                {
                    foreach (Ionic.Zip.ZipEntry e in zip)
                        e.Extract(path);
                }
            }
            
            //find the shapefile
            IEnumerable<string> files = Directory.EnumerateFiles(path, "*.shp");
            if (files.Count<string>() == 0)
                throw new Exception("Couldn't fined climate station shapefile!");

            return files.First<string>();
        }

        /// <summary>
        /// get default ecstations.csv in system temp folder. If doesn't exist, create using the resource file.
        /// </summary>
        /// <returns></returns>
        private static string GetAllStationCSVFile()
        {
            string file = System.IO.Path.GetTempPath();
            file += @"ECReader\";
            if (!Directory.Exists(file)) Directory.CreateDirectory(file);
            file += FILE_NAME_EC_STATIONS_CSV;

            if (!System.IO.File.Exists(file))
            {
                using (StreamWriter writer = new StreamWriter(file))
                    writer.Write(Properties.Resources.ecstations);
            }
            return file;
        }

        private static string GetSavedSelectedStationCSVFile()
        {
            string file = System.IO.Path.GetTempPath();
            file += @"ECReader\";
            if (!Directory.Exists(file)) Directory.CreateDirectory(file);
            file += FILE_NAME_SELECTED_STATIONS_CSV;

            return file;
        }

        /// <summary>
        /// read csv as datatable to search
        /// </summary>
        /// <param name="csvFile"></param>
        /// <returns></returns>
        private static DataTable ReadCSV(string csvFile)
        {
            FileInfo info = new FileInfo(csvFile);
            using (OleDbDataAdapter d = new OleDbDataAdapter(
                "select * from " + info.Name,
                "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" +
                info.DirectoryName + ";Extended Properties='text;HDR=Yes;FMT=Delimited'"))
            {
                DataTable dt = new DataTable();
                d.Fill(dt);
                return dt;
            }
        }

        /// <summary>
        /// read all stations information from ecstations.csv
        /// </summary>
        /// <returns></returns>
        private static DataTable ReadAllStations()
        {
            return ReadCSV(GetAllStationCSVFile());            
        }

        private static DataTable ALL_STATIONS_TABLE = ReadAllStations();

        public static List<ECStationInfo> SearchByName(string name)
        {
            return ECStationInfo.FromCSVDataRows(ALL_STATIONS_TABLE.Select("NAME like '*"+name+"*'"));
        }

        public static List<ECStationInfo> Search(string SQL)
        {
            return ECStationInfo.FromCSVDataRows(ALL_STATIONS_TABLE.Select(SQL));
        }

        /// <summary>
        /// Find all climate stations inside a given boundary by shapefile
        /// </summary>
        /// <param name="shapefilePath"></param>
        /// <returns></returns>
        public static List<ECStationInfo> SearchByShapefile(string shapefilePath)
        {
            if (!System.IO.File.Exists(shapefilePath))
                throw new Exception(shapefilePath + " doesn't exit!");

            string allStationShapeFile = GetAllStationShapeFile();
            Shapefile allsf = Shapefile.OpenFile(allStationShapeFile);
            Shapefile sf = Shapefile.OpenFile(shapefilePath);

            try
            {
                //check the projection, make sure it's use WGS1984
                if (!sf.Projection.GeographicInfo.Name.Equals("GCS_North_American_1983"))
                    throw new Exception("The boundary shapefile should use GCS_North_American_1983 projection!");
                
                if(sf.FeatureType != FeatureType.Polygon)
                    throw new Exception("The boundary shapefile is not polygon!");

                //return value
                List<ECStationInfo> selectedStations = new List<ECStationInfo>();

                //shapefile for all stations in boundary
                //the existing file will be overwrite
                MultiPointShapefile selectedStationsShapefile = new MultiPointShapefile();
                selectedStationsShapefile.Projection = allsf.Projection;

                //try to find all the climate stations located in boundary
                List<IFeature> stations = allsf.Select(sf.Extent);
                foreach (IFeature station in stations)
                {
                    foreach (IFeature boundary in sf.Features)
                    {
                        if ((boundary.BasicGeometry as IPolygon).Contains(station.BasicGeometry as IGeometry))
                        {
                            selectedStations.Add(new ECStationInfo(station.DataRow));
                            selectedStationsShapefile.Features.Add(station);
                            break;
                        }
                    }                    
                }

                //save stations in boundary to the shapefile
                selectedStationsShapefile.SaveAs(GetStationsInBoundary(), true);

                return selectedStations;
            }
            finally
            {
                allsf.Close();
                sf.Close();
            }               

        }

    }

    enum ECSearchType
    {
        StationName = 0,
        Province = 1,
    }

    enum ECDataIntervalType
    {
        HOURLY = 1,
        DAILY = 2,
        MONTHLY = 3
    }

    class ECStationDataAvailability
    {
        private static string[] INTERVAL_NAME_IN_HTML = {"hlyRange","dlyRange","mlyRange"};
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
            _intervalType = (ECDataIntervalType)(Array.IndexOf(INTERVAL_NAME_IN_HTML,dataRangeType) + 1);

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

            DateTime firstDay_TestYear = new DateTime(year,1,1);
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

            return string.Format("{2}: From {0} To {1}", _firstDay, _lastDay,_intervalType);
        }
    }

    /// <summary>
    /// Util class to read html data retrieved from EC
    /// </summary>
    class ECHtmlUtil
    {
        /// <summary>
        /// Read hidden input tag. The basic information of stations are in this format, especially the station id.
        /// </summary>
        /// <param name="inputHiddenNode"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void ReadInputHiddenNode(HtmlNode inputHiddenNode,
            out string name, out string value)
        {
            name = "";
            value = "";
            if (inputHiddenNode.Attributes.Contains("name") && inputHiddenNode.Attributes.Contains("value"))
            {
                name = inputHiddenNode.Attributes["name"].Value;
                value = inputHiddenNode.Attributes["value"].Value;
            }
        }

        public static HtmlNodeCollection ReadAllNodes(string html, string xpath)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc.DocumentNode.SelectNodes(xpath);
        }

        public static HtmlNodeCollection ReadAllNodes(HtmlNode node, string xpath)
        {
            return ReadAllNodes(node.InnerHtml, xpath);
        }

        public static double ReadLatitudeLongitude(HtmlNode node)
        {
            if (node.ChildNodes.Count < 5)
            {
                Debug.WriteLine("Don't have latitude information.");
                return 0.0;
            }

            double degree = 0.0;
            double.TryParse(node.ChildNodes[0].InnerText.Trim(), out degree);

            double minute = 0.0;
            double.TryParse(node.ChildNodes[2].InnerText.Trim(), out minute);

            double second = 0.0;
            double.TryParse(node.ChildNodes[4].InnerText.Trim(), out second);

            return degree + (minute + second/60.0) / 60.0;
        }
    }

    class ECStationInfo : BaseWithProcess
    {
        private string _name;
        private string _province;
        private string _id;
        private double _latitude = 0;
        private double _longitude = 0;
        private double _elevation = 0;
        private ECStationDataAvailability _hourlyAvailability = null;
        private ECStationDataAvailability _dailyAvailability = null;
        private ECStationDataAvailability _monthlyAvailability = null;

        #region Constrcutor

        #region From CSV File

        public static List<ECStationInfo> FromCSVDataRows(DataRow[] rowsInCSV)
        {
            List<ECStationInfo> stations = new List<ECStationInfo>();
            foreach (DataRow r in rowsInCSV)
                stations.Add(new ECStationInfo(r));
            return stations;
        }
             

        public ECStationInfo(DataRow oneRowInCSV)
        {
            if (oneRowInCSV == null) return;

            _id = oneRowInCSV[0].ToString();
            _name = oneRowInCSV[1].ToString();
            _province = oneRowInCSV[2].ToString();
            _latitude = double.Parse(oneRowInCSV[3].ToString());
            _longitude = double.Parse(oneRowInCSV[4].ToString());
            _elevation = double.Parse(oneRowInCSV[5].ToString());

            if (oneRowInCSV.Table.Columns.Count >= 12)
            {
                _hasGotDataAvailability = true;
                _hourlyAvailability = new ECStationDataAvailability(
                    ECDataIntervalType.HOURLY, oneRowInCSV[6].ToString(), oneRowInCSV[7].ToString());
                _dailyAvailability = new ECStationDataAvailability(
                    ECDataIntervalType.DAILY, oneRowInCSV[8].ToString(), oneRowInCSV[9].ToString());
                _monthlyAvailability = new ECStationDataAvailability(
                    ECDataIntervalType.MONTHLY, oneRowInCSV[10].ToString(), oneRowInCSV[11].ToString());
            }
        }
        
        private bool _hasGotDataAvailability = false;

        /// <summary>
        /// get data availability though search by name in EC website
        /// </summary>
        public void readDataAvailability()
        {
            if (_hasGotDataAvailability) return;

            _hasGotDataAvailability = true;
            List<ECStationInfo> stations = FromEC(ECRequestUtil.RequestOneStation(_name));
            foreach(ECStationInfo info in stations)
                if (info.Equals(this))
                {
                    _dailyAvailability = info.DailyAvailability;
                    _monthlyAvailability = info.MonthlyAvailability;
                    _hourlyAvailability = info.HourlyAvailability;
                }
        }

        #endregion

        #region From EC Html response

        public static List<ECStationInfo> FromEC(string htmlRequest,
            System.ComponentModel.BackgroundWorker worker = null)
        {
            HtmlNodeCollection nodes = ECHtmlUtil.ReadAllNodes(htmlRequest, "//form[@action='/lib/climateData/Interform.php']");
            List<ECStationInfo> stations = new List<ECStationInfo>();
            if (nodes == null || nodes.Count == 0) return stations;
            foreach (HtmlNode node in nodes)
            {
                ECStationInfo info = new ECStationInfo(node);                
                Debug.WriteLine(info);
                if (worker != null)
                    worker.ReportProgress(0, info);
                stations.Add(info);
            }
            return stations;
        }

        public ECStationInfo(HtmlNode stationFormNode)
        {
            //read basic information from hidden inputs
            HtmlNodeCollection allHiddenInputNodes =
                ECHtmlUtil.ReadAllNodes(stationFormNode, "//input[@type='hidden']");
            if (allHiddenInputNodes == null)
            {
                //try to find parent div of hidden inputs
                while (stationFormNode != null && stationFormNode.Name != "div")
                    stationFormNode = stationFormNode.NextSibling;
                if (stationFormNode == null) return;
                allHiddenInputNodes = ECHtmlUtil.ReadAllNodes(stationFormNode, "//input[@type='hidden']");
                if (allHiddenInputNodes == null) return;
            }

            _hourlyAvailability = new ECStationDataAvailability(allHiddenInputNodes[0]);
            _dailyAvailability = new ECStationDataAvailability(allHiddenInputNodes[1]);
            _monthlyAvailability = new ECStationDataAvailability(allHiddenInputNodes[2]);
            _hasGotDataAvailability = true;

            string key = "";
            ECHtmlUtil.ReadInputHiddenNode(allHiddenInputNodes[3],  //station id
                out key, out _id);
            ECHtmlUtil.ReadInputHiddenNode(allHiddenInputNodes[4],  //province
                out key, out _province);

            //read station name and available years from divs
            HtmlNodeCollection allDataDivNodes =
                ECHtmlUtil.ReadAllNodes(stationFormNode, "//div[@class='divTableRowOdd']");
            if(allDataDivNodes == null)
                allDataDivNodes =
                ECHtmlUtil.ReadAllNodes(stationFormNode, "//div[@class='divTableRowEven']");
            if (allDataDivNodes == null) return;

            HtmlNode tableDataNode = allDataDivNodes[0];
            allDataDivNodes = ECHtmlUtil.ReadAllNodes(tableDataNode, "//div[@class]");
            if (allDataDivNodes == null) return;

            _name = allDataDivNodes[0].InnerText.Trim(); //just read station name right now
            if (_name.Contains(',')) //some name has comma, like KEY LAKE, SK, just need first part, orelse it will conflict with csv format and would has problem when import in ArcMap
                _name = _name.Split(',')[0].Trim();


            //try to retrieve latitude, longitude and elevation
            HtmlNodeCollection tdNodes = ECHtmlUtil.ReadAllNodes(ECRequestUtil.RequestLatLongElevation(_id), "//td"); 
            if (tdNodes == null) return;

            _latitude = ECHtmlUtil.ReadLatitudeLongitude(tdNodes[0]);
            _longitude = -ECHtmlUtil.ReadLatitudeLongitude(tdNodes[1]);

            double.TryParse(tdNodes[2].ChildNodes[0].InnerText.Trim(), out _elevation);
        }

        #endregion

        #endregion
        
        /// <summary>
        /// read data interval types or available years from select-option tage
        /// </summary>
        /// <param name="html"></param>
        /// <remarks>may be used in the future</remarks>
        private void getOptions(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//option");
            foreach (HtmlNode node in nodes)
            {
                Debug.Write(node.Attributes[0].Value + ",");
            }
        }

        public string Name { get { return _name; } }
        public string Province { get { return _province; } }
        public string ID { get { return _id; } }
        public double Latitude { get { return _latitude; } }
        public double Longitude { get { return _longitude; } }
        public double Elevation { get { return _elevation; } }
        public bool IsHourlyAvailable { get { return HourlyAvailability != null && HourlyAvailability.IsAvailable; } }
        public bool IsDailyAvailable { get { return DailyAvailability != null && DailyAvailability.IsAvailable; } }
        public bool IsMonthlyAvailable { get { return MonthlyAvailability != null && MonthlyAvailability.IsAvailable; } }
        public ECStationDataAvailability HourlyAvailability { get { readDataAvailability(); return _hourlyAvailability; } }
        public ECStationDataAvailability DailyAvailability { get { readDataAvailability(); return _dailyAvailability; } }
        public ECStationDataAvailability MonthlyAvailability { get { readDataAvailability(); return _monthlyAvailability; } }

        public bool IsAvailableForYear(int year)
        {
            if (!IsDailyAvailable && !IsHourlyAvailable) return false;
            bool daily = IsDailyAvailable && DailyAvailability.IsAvailableForYear(year);
            bool hourly = IsHourlyAvailable && HourlyAvailability.IsAvailableForYear(year);
            return daily || hourly;
        }

        public bool IsAvailableForYear(int startYear, int endYear)
        {
            bool available = false;
            for (int year = startYear; year <= endYear; year++)
            {
                available |= IsAvailableForYear(year);
                if (available) break;
            }
            return available;
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3}", _name, _province,_elevation,_id);
            //return string.Format("Name={0},Province={1},ID={2},Latitude={6},Longitude={7},Elevation={8},{3},{4},{5}",
            //    _name,_province,_id,
            //    _hourlyAvailability == null ? "" : _hourlyAvailability.ToString(),
            //    _dailyAvailability == null ? "" : _dailyAvailability.ToString(),
            //    _monthlyAvailability == null ? "" : _monthlyAvailability.ToString(),
            //    _latitude,_longitude,_elevation);
        }

        /// <summary>
        /// Used to save in CSV format
        /// </summary>
        /// <returns></returns>
        public string ToCSVString()
        {
            return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}",
                _id,_name,_province,
                _latitude,_longitude,_elevation,
                HourlyAvailability != null ? HourlyAvailability.ToCSVString() : "null,null",
                DailyAvailability != null ? DailyAvailability.ToCSVString() : "null,null",
                MonthlyAvailability != null ? MonthlyAvailability.ToCSVString() : "null,null");
        }

        /// <summary>
        /// Gage location format for ArcSWAT 2012
        /// </summary>
        /// <returns></returns>
        public string ToArcSWAT2012CSVGageLocation(bool isPrecipitation)
        {
            return string.Format("{0},{1},{2:F3},{3:F3},{4}",
                ID,
                getFileName(1840, 1840, FormatType.ARCSWAT_TEXT, ECDataIntervalType.DAILY,isPrecipitation, false),
                Latitude,Longitude,Convert.ToInt32(Elevation));
        }

        public void ToArcSWAT2012CSVGageLocation(DbfFile dbf, bool isPrecipitation)
        {
            DbfRecord rec = new DbfRecord(dbf.Header);
            rec[0] = ID;
            rec[1] = getFileName(1840, 1840, FormatType.ARCSWAT_DBF,ECDataIntervalType.DAILY, isPrecipitation, false);
            rec[2] = Latitude.ToString("F3");
            rec[3] = Longitude.ToString("F3");
            rec[4] = Convert.ToInt32(Elevation).ToString();
            dbf.Write(rec, true);
        }

        /// <summary>
        /// Compare to other station
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if(obj.GetType() != typeof(ECStationInfo)) return false;

            ECStationInfo info = obj as ECStationInfo;
            if (info == null) return false;

            return info.ID.Equals(ID);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #region Download Data

        #region Data Cache

        #region Temperature

        public List<DailyTemperature> getTemperatureForOneYear(int year)
        {
            List<DailyTemperature> temps = null;
            string cacheFile = getCachFileNameTemperature(year);
            if (File.Exists(cacheFile))
                temps = DailyTemperature.FromCacheFile(cacheFile);
            else
            {
                temps = DailyTemperature.GetTemperatureForYearHourlyFirst(ID, year);
                using (StreamWriter writer = new StreamWriter(cacheFile))
                {
                    writer.WriteLine(DailyTemperature.HEADER);
                    foreach (DailyTemperature tep in temps)
                        writer.WriteLine(tep);
                }
            }
            return temps;
        }

        private string getCachFileNameTemperature(int year)
        {
            string path = Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),"Temperature");
            if(!Directory.Exists(path)) Directory.CreateDirectory(path);
            path = Path.Combine(path, ID);
            if(!Directory.Exists(path)) Directory.CreateDirectory(path);
            return Path.Combine(path, string.Format("{0}.csv",year));
        }

        #endregion



        private string getDataForOneYear(int year, ECDataIntervalType timeInterval, int processPercent)
        {
            string cacheFile = getCacheFileName(year, timeInterval);

            //not in cache, go to download and then write into cache file
            if (!System.IO.File.Exists(cacheFile))
            {
                setProgress(processPercent, 
                    string.Format("Downloading data for station: {0}, year: {1}", this, year));
                string resultsForOneYear = "";
                if (timeInterval == ECDataIntervalType.DAILY)
                    resultsForOneYear = ECRequestUtil.RequestAnnualDailyClimateData(ID, year);
                else if (timeInterval == ECDataIntervalType.HOURLY)
                {
                    System.Text.StringBuilder sb = new StringBuilder();
                    for (int month = 1; month <= 12; month++)
                    {
                        setProgress(processPercent,
                            string.Format("Month: {0}", month));
                        sb.AppendLine(ECRequestUtil.RequestHourlyClimateData(ID, year, month, month == 1));
                    }
                    resultsForOneYear = sb.ToString();
                }

                //write to cache file
                using (StreamWriter writer = new StreamWriter(cacheFile))
                {
                    writer.Write(resultsForOneYear);
                }

                return resultsForOneYear;
            }

            //read from cache file
            setProgress(processPercent,
                string.Format("Reading data from cache for station: {0}, year: {1}", this, year));
            using(StreamReader reader = new StreamReader(cacheFile))
            {
                return reader.ReadToEnd();
            }
        }

        private string getCacheFileName(int year, ECDataIntervalType timeInterval)
        {
            return CachePath + string.Format("{0}_{1}_{2}.csv", ID, year, timeInterval);
        }

        private string CachePath
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

        #endregion

        private static int TOTAL_PRECIPITATION_COL_INDEX = 19;
        private static int MAX_T_COL_INDEX = 5;
        private static int MIN_T_COL_INDEX = 7;
        private static string WARNING_FORMAT = "*** Warning: {0} ***";

        #region Warning Message

        private List<int> _failureYears = null;
        private List<int> _uncompletedYears = null;

        #region No data Year

        /// <summary>
        /// clear no data year array
        /// </summary>
        private void clearFailureYears()
        {
            if (_failureYears == null) _failureYears = new List<int>();
            _failureYears.Clear();
        }

        /// <summary>
        /// record one no data year
        /// </summary>
        /// <param name="year"></param>
        private void addFailureYear(int year)
        {
            setProgress(ProcessPercentage, string.Format(WARNING_FORMAT, "No data is available for year " + year.ToString()));
            _failureYears.Add(year);
        }

        /// <summary>
        /// output all no data years
        /// </summary>
        private void outputFailureYear()
        {
            if (_failureYears == null && _failureYears.Count == 0) return;

            setProgress(ProcessPercentage, "Following years don't have data");
            foreach (int year in _failureYears)
                setProgress(ProcessPercentage, year.ToString());
        }

        #endregion

        #region Uncompleted Years

        private void clearUncompletedYears()
        {
            if (_uncompletedYears == null) _uncompletedYears = new List<int>();
            this._uncompletedYears.Clear();
        }

        private void addUncompletedYear(int year)
        {
            setProgress(ProcessPercentage, string.Format(WARNING_FORMAT, "The data of year " + year.ToString() + " is not completed"));
            _failureYears.Add(year);
        }

        private void checkLastDayofYear(string date)
        {
            DateTime lastDay = DateTime.Now;
            if (DateTime.TryParse(date, out lastDay))
                if (lastDay.Month != 12 || lastDay.Day != 31)
                    addUncompletedYear(lastDay.Year);
        }

        #endregion

        private static int NUM_OF_COLUMN_OUTPUT_YEAR = 5;

        public string WarningMessage
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if (_failureYears != null && _failureYears.Count > 0)
                {
                    sb.AppendLine("There is no data in following year.");
                    for (int i = 0; i < _failureYears.Count; i++)
                    {
                        sb.Append(_failureYears[i]);
                        if (i % NUM_OF_COLUMN_OUTPUT_YEAR < NUM_OF_COLUMN_OUTPUT_YEAR - 1)
                            sb.Append("\t");
                        else
                            sb.Append(Environment.NewLine);
                    }
                    sb.AppendLine();
                }
                if (_uncompletedYears != null && _uncompletedYears.Count > 0)
                {
                    if (sb.Length > 0) sb.AppendLine();
                    sb.AppendLine("The data of following year is uncompleted.");
                    for (int i = 0; i < _uncompletedYears.Count; i++)
                    {
                        sb.Append(_uncompletedYears[i]);
                        if (i % NUM_OF_COLUMN_OUTPUT_YEAR < NUM_OF_COLUMN_OUTPUT_YEAR - 1)
                            sb.Append("\t");
                        else
                            sb.Append(Environment.NewLine);
                    }
                }

                return sb.ToString();
            }
        }

        #endregion

        /// <summary>
        /// Save temperature data into an ascii file for later use
        /// </summary>
        /// <param name="startYear"></param>
        /// <param name="endYear"></param>
        /// <param name="destinationFolder"></param>
        /// <returns></returns>
        public bool saveTemperature(int startYear, int endYear, string destinationFolder)
        {
            int[] hourly_index = new int[] { 6 };
            int[] daily_index = new int[] { 5, 7, 9 };

            for (int year = startYear; year <= endYear; year++)
            {
                if (this.IsHourlyAvailable && HourlyAvailability.IsAvailableForYear(year))
                    save2Ascii(hourly_index, year, year, destinationFolder, FormatType.SIMPLE_CSV, ECDataIntervalType.HOURLY);
                else if(this.IsDailyAvailable)
                    save2Ascii(daily_index, year, year, destinationFolder, FormatType.SIMPLE_CSV, ECDataIntervalType.DAILY);
            }
            return true;
        }

        public bool save(int[] fields,
            int startYear, int endYear, string destinationFolder, FormatType format,
            ECDataIntervalType timeInterval = ECDataIntervalType.DAILY)
        {
            if (timeInterval == ECDataIntervalType.MONTHLY) return false;
            if (timeInterval == ECDataIntervalType.HOURLY)
            {
                if (format != FormatType.SIMPLE_CSV && format != FormatType.SIMPLE_TEXT) return false;
                if (!IsHourlyAvailable) return false;

                return save2Ascii(fields, startYear, endYear, destinationFolder, format,timeInterval);
            }

            if (!IsDailyAvailable) return false;
                
            //shorten the time range if possible
            //only apply for free foramt csv and txt format which is usually to do data analysis
            //for SWAT/ArcSWAT format, this is checked in the calling function. For years without data, 
            //program will just add -99 to it to make sure all input files have the same time range.
            if ((format == FormatType.SIMPLE_CSV || format == FormatType.SIMPLE_TEXT)
                && DailyAvailability.IsAvailable)
            {
                if (startYear < DailyAvailability.FirstYear)
                {
                    startYear = DailyAvailability.FirstYear;
                    setProgress(0, "Start year is changed to " + startYear);
                }
                if (endYear > DailyAvailability.LastYear)
                {
                    endYear = DailyAvailability.LastYear;
                    setProgress(0, "End year is changed to " + endYear);
                }
            }

            if (format == FormatType.ARCSWAT_DBF)
                return save2ArcSWATdbf(startYear, endYear, destinationFolder);
            else if (format == FormatType.ARCSWAT_TEXT)
                return save2ArcSWATAscii(startYear, endYear, destinationFolder);
            else if (format == FormatType.SIMPLE_CSV || format == FormatType.SIMPLE_TEXT)
                return save2Ascii(fields, startYear, endYear, destinationFolder, format);
            else
                return false;
        }

        private string getTimeAffix()
        {
            return "";
            //return string.Format("_{0:yyyyMMddHHmmss}",DateTime.Now);
        }

        /// <summary>
        /// get result file extension (txt, csv or dbf) from result format
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string getExtentionFromType(FormatType type)
        {
            if (type == FormatType.ARCSWAT_DBF) return ".dbf";
            else if (type == FormatType.SIMPLE_CSV) return ".csv";
            return ".txt";
        }

        public string getFileName(int startYear, int endYear, FormatType type, 
            ECDataIntervalType timeInterval, bool isPrecipitation, bool containExtension = true)
        {
            string extension = containExtension ? getExtentionFromType(type) : "";
            if (type == FormatType.SIMPLE_CSV || type == FormatType.SIMPLE_TEXT)
            {
                if(endYear > startYear)
                    return string.Format("{0}_{1}_{6}_{2}_{3}_{4}{5}",
                        _name.Replace(' ', '_'), _province, startYear, endYear, timeInterval, extension,ID);
                else
                    return string.Format("{0}_{1}_{5}_{2}_{3}{4}",
                        _name.Replace(' ', '_'), _province, startYear, timeInterval, extension,ID);
            }
            else
            {
                string affix = "T";
                if (isPrecipitation) affix = "P";
                return affix + ID.PadLeft(7, '0') + extension; //Limitation of file name in ArcSWAT: max 8 chars 
            }                   
        }

        private bool save2Ascii(int[] fields,
            int startYear, int endYear, string destinationFolder, FormatType format, ECDataIntervalType timeInterval = ECDataIntervalType.DAILY)
        {
            //get the file name using station name
            string fileName = string.Format("{0}\\{1}",
                Path.GetFullPath(destinationFolder), getFileName(startYear,endYear,format, timeInterval, true));  //precipitation

            this.setProgress(0, string.Format("Processing station {0}", this));
            this.setProgress(0, fileName);

            //open the file and write the data
            int processPercent = 0;
            bool hasResults = false;
            clearFailureYears();
            clearUncompletedYears();
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                for (int i = startYear; i <= endYear; i++)
                {
                    string resultsForOneYear = getDataForOneYear(i, timeInterval, processPercent);

                    if (resultsForOneYear.Length == 0)
                    {
                        addFailureYear(i);
                        continue;
                    }

                    processPercent += 1;
                    setProgress(processPercent, "Writing data");

                    if (format == FormatType.SIMPLE_CSV || format == FormatType.SIMPLE_TEXT)
                        hasResults = write2FreeFormat(resultsForOneYear, fields, writer, i == startYear, format, timeInterval);

                    processPercent += 1;
                }
            }

            return hasResults;
        }

        #region Write Free Format File, Simple Text and CSV

        private static int FIXED_FIELD_WIDTH = 20; //the width of each field in text format

        private string formatFreeFormatData(string v, FormatType format, bool isDate)
        {
            if (format == FormatType.SIMPLE_CSV)
                return v;
            else if (format == FormatType.SIMPLE_TEXT)
            {
                if (isDate)
                    return v.PadRight(FIXED_FIELD_WIDTH);
                else
                    return v.PadLeft(FIXED_FIELD_WIDTH);
            }
            return "";
        }

        private bool write2FreeFormat(string resultsForOneYear, int[] fields, StreamWriter writer, bool needWriteHeader, FormatType format, ECDataIntervalType timeInterval = ECDataIntervalType.DAILY)
        {
            StringBuilder sb = new StringBuilder();
            int numofColumn = 27;
            if (timeInterval == ECDataIntervalType.HOURLY) numofColumn = 25;

            //make sure field is valid
            foreach (int field in fields)
                if (field >= numofColumn || field < 0) return false;

            using (CachedCsvReader csv = new CachedCsvReader(new StringReader(resultsForOneYear), true))
            {
                if (csv.FieldCount < numofColumn) return false;

                string date = "";
                if (needWriteHeader)
                {
                    string[] fieldNames = csv.GetFieldHeaders();
                    sb.Append(formatFreeFormatData(fieldNames[0], format, true));

                    foreach (int field in fields)
                    {
                        if (format == FormatType.SIMPLE_CSV)
                            sb.Append(",");
                        sb.Append(formatFreeFormatData(fieldNames[field], format, false));
                    }
                    sb.AppendLine();
                }
                while (csv.ReadNextRecord())
                {
                    date = csv[0];
                    sb.Append(formatFreeFormatData(date, format, true));

                    foreach (int field in fields)
                    {
                        if (format == FormatType.SIMPLE_CSV)
                            sb.Append(",");
                        sb.Append(formatFreeFormatData(csv[field], format, false));
                    }
                    sb.AppendLine();
                }

                checkLastDayofYear(date);
            }
            if (sb.Length > 0)
                writer.Write(sb.ToString());

            return sb.Length > 0;
        }

        #endregion

        private bool hasDataAvailable(int year)
        {
            return
                IsDailyAvailable && DailyAvailability.FirstYear <= year && DailyAvailability.LastYear >= year;
        }

        /// <summary>
        /// Write data in given time range as ArcSWAT txt file
        /// </summary>
        /// <param name="startYear"></param>
        /// <param name="endYear"></param>
        /// <param name="destinationFolder"></param>
        /// <returns></returns>
        private bool save2ArcSWATAscii(int startYear, int endYear, string destinationFolder)
        {
            //get the file name using station name
            string timeAffix = getTimeAffix();
            string pFile = string.Format("{0}\\{1}", 
                Path.GetFullPath(destinationFolder), 
                getFileName(startYear,endYear,FormatType.ARCSWAT_TEXT, ECDataIntervalType.DAILY, true));  //precipitation
            string tFile = string.Format("{0}\\{1}",
                Path.GetFullPath(destinationFolder),
                getFileName(startYear, endYear, FormatType.ARCSWAT_TEXT, ECDataIntervalType.DAILY, false));  //temperature

            this.setProgress(0, string.Format("Processing station {0}", this));
            this.setProgress(0, pFile);
            this.setProgress(0, tFile);

            int processPercent = 0;
            bool hasResults = false;
            string numberForamt = "F1";
            string temperatureFormat = "{0:" + numberForamt + "},{1:" + numberForamt + "}";
            StringBuilder pSb = new StringBuilder();
            StringBuilder tSb = new StringBuilder();
            clearFailureYears();
            clearUncompletedYears();
            for (int i = startYear; i <= endYear; i++)
            {
                //there is data, try to download
                string resultsForOneYear = getDataForOneYear(i, ECDataIntervalType.DAILY, processPercent);
                if (resultsForOneYear.Length == 0)
                {
                    addFailureYear(i);
                    continue;
                }

                processPercent += 1;
                setProgress(processPercent, "Writing data");

                using (CachedCsvReader csv = new CachedCsvReader(new StringReader(resultsForOneYear), true))
                {
                    if (csv.FieldCount >= 27)
                    {
                        hasResults = true;

                        string lastDay = "";
                        while (csv.ReadNextRecord())
                        {
                            //add starting date
                            if (pSb.Length == 0)
                            {
                                DateTime date = DateTime.Now;
                                if (DateTime.TryParse(csv[0], out date))
                                {
                                    string startDate = string.Format("{0:yyyyMMdd}, Generated by Environment Canada Climate Data Reader, hawklorry@gmail.com", date);
                                    pSb.AppendLine(startDate);
                                    tSb.AppendLine(startDate);
                                }
                            }
                            lastDay = csv[0];

                            //write data                            
                            double p = ClimateString2Double(csv[TOTAL_PRECIPITATION_COL_INDEX]);
                            pSb.AppendLine(p.ToString(numberForamt));

                            double t_max = ClimateString2Double(csv[MAX_T_COL_INDEX]);
                            double t_min = ClimateString2Double(csv[MIN_T_COL_INDEX]);
                            tSb.AppendLine(string.Format(temperatureFormat, t_max, t_min));
                        }
                        checkLastDayofYear(lastDay);
                    }
                }
                processPercent += 1;
            }

            if (pSb.Length > 0)
                using (StreamWriter writer = new StreamWriter(pFile))
                    writer.Write(pSb.ToString());
            if (tSb.Length > 0)
                using (StreamWriter writer = new StreamWriter(tFile))
                    writer.Write(tSb.ToString());

            return hasResults;
        }

        /// <summary>
        /// Write data in given time range as ArcSWAT dbf file
        /// </summary>
        /// <param name="startYear"></param>
        /// <param name="endYear"></param>
        /// <param name="destinationFolder"></param>
        /// <returns></returns>
        private bool save2ArcSWATdbf(int startYear, int endYear, string destinationFolder)
        {
            string timeAffix = getTimeAffix();
            string pFile = string.Format("{0}\\{1}",
                Path.GetFullPath(destinationFolder),
                getFileName(startYear, endYear, FormatType.ARCSWAT_DBF, ECDataIntervalType.DAILY, true));  //precipitation
            string tFile = string.Format("{0}\\{1}",
                Path.GetFullPath(destinationFolder),
                getFileName(startYear, endYear, FormatType.ARCSWAT_DBF, ECDataIntervalType.DAILY, false));  //temperature

            this.setProgress(0, string.Format("Processing station {0}", this));
            this.setProgress(0, pFile);
            this.setProgress(0, tFile);

            //create the dbf structure based on ArcSWAT document
            DbfFile pDBF = new DbfFile();
            pDBF.Open(pFile, FileMode.Create);
            pDBF.Header.AddColumn(new DbfColumn("DATE", DbfColumn.DbfColumnType.Date));
            pDBF.Header.AddColumn(new DbfColumn("PCP", DbfColumn.DbfColumnType.Number, 5, 1));


            DbfFile tDBF = new DbfFile();
            tDBF.Open(tFile, FileMode.Create);
            tDBF.Header.AddColumn(new DbfColumn("DATE", DbfColumn.DbfColumnType.Date));
            tDBF.Header.AddColumn(new DbfColumn("MAX", DbfColumn.DbfColumnType.Number, 5, 1));
            tDBF.Header.AddColumn(new DbfColumn("MIN", DbfColumn.DbfColumnType.Number, 5, 1));

            DbfRecord pRec = new DbfRecord(pDBF.Header);
            DbfRecord tRec = new DbfRecord(tDBF.Header);

            int processPercent = 0;
            bool hasResults = false;
            clearFailureYears();
            clearUncompletedYears();
            for (int i = startYear; i <= endYear; i++)
            {
                string resultsForOneYear = getDataForOneYear(i, ECDataIntervalType.DAILY, processPercent);
                if (resultsForOneYear.Length == 0)
                {
                    addFailureYear(i);
                    continue;
                }

                processPercent += 1;
                setProgress(processPercent, "Writing data");

                using (CachedCsvReader csv = new CachedCsvReader(new StringReader(resultsForOneYear), true))
                {
                    if (csv.FieldCount >= 27)
                    {
                        hasResults = true;

                        string date = "";
                        while (csv.ReadNextRecord())
                        {
                            date = csv[0];
                            double p = ClimateString2Double(csv[TOTAL_PRECIPITATION_COL_INDEX]);
                            pRec[0] = date;
                            pRec[1] = p.ToString();
                            pDBF.Write(pRec, true);

                            double t_max = ClimateString2Double(csv[MAX_T_COL_INDEX]);
                            double t_min = ClimateString2Double(csv[MIN_T_COL_INDEX]);
                            tRec[0] = date;
                            tRec[1] = t_max.ToString();
                            tRec[2] = t_min.ToString();
                            tDBF.Write(tRec, true);
                        }
                        checkLastDayofYear(date);
                    }
                }
                processPercent += 1;
            }
            pDBF.Close();
            tDBF.Close();

            return hasResults;
        }

        /// <summary>
        /// replace missing data using -99
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private double ClimateString2Double(string data)
        {
            double d = -99.0;
            if (double.TryParse(data, out d)) return d;
            return -99.0;
        }

        #endregion
    }

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
        public double Min { set { _min = value; } }
        public double Max { set { _max = value; } }
        public double Ave { set { _ave = value; } }
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
                        string.IsNullOrEmpty(csv[7]) || string.IsNullOrEmpty(csv[9])) continue;

                    currentDay = DateTime.Parse(csv[0]);
                    var currentTemp = temps.Where(temp => temp.Day.Equals(currentDay));
                    DailyTemperature t = currentTemp.First<DailyTemperature>();

                    t.Min = double.Parse(csv[5]);
                    t.Max = double.Parse(csv[7]);
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
                            string.IsNullOrEmpty(csv[3])) continue;

                        DailyTemperature t = new DailyTemperature(DateTime.Parse(csv[0]));
                        t.Min = double.Parse(csv[1]);
                        t.Max = double.Parse(csv[2]);
                        t.Ave = double.Parse(csv[3]);
                        t.FromHourly = bool.Parse(csv[4]);
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

            _day = hourlyTemp[0].Time.Date;
            _min = hourlyTemp.Min(temp => temp.Temperature);
            _max = hourlyTemp.Max(temp => temp.Temperature);
            _ave = hourlyTemp.Average(temp => temp.Temperature);
            _fromHourly = true;
        }

        public override string ToString()
        {
            if(HasValue)
                return string.Format("{0:yyyy-MM-dd},{1},{2},{3},{4}", _day, _min,_max,_ave,_fromHourly);
            else
                return string.Format("{0:yyyy-MM-dd},,,,false", _day);
        }

        public static string HEADER = "day,min,max,ave,hourly";
    }

    /// <summary>
    /// A project to deal with EC climate data. It may have a shapefile boundary and has a group of shapefiles
    /// </summary>
    class ECClimateProject
    {
        private string _shapefileName = string.Empty;
        private List<ECStationInfo> _stations = null;

        public ECClimateProject(string shapefileName)
        {
            _shapefileName = shapefileName;
        }

        public List<ECStationInfo> Stations
        {
            get
            {
                if (_stations == null) readECStationsInBoundary();
                return _stations;
            }
        }

        /// <summary>
        /// A new shapefile will be generated for each given start and end year. They may have different stations available
        /// </summary>
        /// <param name="startYear"></param>
        /// <param name="endYear"></param>
        /// <returns></returns>
        //public string getStationsShapefile(int startYear, int endYear)
        //{
        //    //default file path, in the exe folder
        //    string stationShapefile =
        //        Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Shapefile");
        //    if (!Directory.Exists(stationShapefile)) Directory.CreateDirectory(stationShapefile);
        //    stationShapefile = Path.Combine(stationShapefile,
        //        string.Format("huzelnut_{0}_{1}.shp", startYear, endYear));

        //    if (!File.Exists(stationShapefile))
        //    {
        //        //copy the shapefile for all stations
        //        string allStationShapefile = EC.GetAllStationShapeFile();
        //        File.Copy(allStationShapefile, stationShapefile);

        //        //only keep stations in boundary
        //        Shapefile sf = Shapefile.OpenFile(stationShapefile);


        //        //remove some stations based on data availability
        //        foreach (ECStationInfo info in Stations)
        //        {
        //            if (!info.IsAvailableForYear(startYear, endYear)) continue;

        //            //add this station to the shapefile

        //        }
        //    }
        //}

        public string StationListCacheName { get { return "stationsInBoundary.csv"; } }

        private void readECStationsInBoundary()
        {
            if (_stations != null) return;

            //load from save stations
            string path = System.IO.Path.GetTempPath();
            path += @"ECReader\";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            string file = path + StationListCacheName;

            if (!File.Exists(file))
            {
                if (!string.IsNullOrEmpty(_shapefileName))
                {
                    _stations = EC.SearchByShapefile(_shapefileName);
                    EC.SaveStations(file, _stations);
                }
                _stations = new List<ECStationInfo>();
            }

            _stations = EC.ReadStations(file);
        }
    }

    class HuzelnutSuitabilityProject : ECClimateProject
    {
        public HuzelnutSuitabilityProject(string shapefileName) : base(shapefileName) { }
    }
}
