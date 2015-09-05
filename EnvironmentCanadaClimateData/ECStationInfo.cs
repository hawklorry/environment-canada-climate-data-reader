using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using HtmlAgilityPack;
using LumenWorks.Framework.IO.Csv;
using System.Data;
using SocialExplorer.IO.FastDBF;
using DotSpatial.Data;
using DotSpatial.Topology;
using HAWKLORRY.HuzelnutSuitability;

namespace HAWKLORRY
{
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
        private IFeature _fea = null;

        #region Constrcutor

        #region From Shapefile

        public static List<ECStationInfo> FromShapefile(string shapefilePath)
        {
            List<ECStationInfo> stations = new List<ECStationInfo>();
            if (!File.Exists(shapefilePath)) return stations;

            try
            {
                Shapefile sf = Shapefile.OpenFile(shapefilePath);
                try
                {
                    foreach (IFeature fea in sf.Features)
                        stations.Add(new ECStationInfo(fea));
                }
                catch { }
                finally { sf.Close(); }
            }
            catch { }

            return stations;
        }

        #endregion

        #region From CSV File

        public static List<ECStationInfo> FromCSVDataRows(DataRow[] rowsInCSV)
        {
            List<ECStationInfo> stations = new List<ECStationInfo>();
            foreach (DataRow r in rowsInCSV)
                stations.Add(new ECStationInfo(r));
            return stations;
        }

        public ECStationInfo(IFeature fea)
        {
            initializedFromDataRow(fea.DataRow);
            _fea = fea;
        }

        public ECStationInfo(DataRow row)
        {
            initializedFromDataRow(row);
        }

        private void initializedFromDataRow(DataRow row)
        {
            if (row == null) return;

            _id = row[0].ToString();
            _name = row[1].ToString();
            _province = row[2].ToString();
            _latitude = double.Parse(row[3].ToString());
            _longitude = double.Parse(row[4].ToString());
            _elevation = double.Parse(row[5].ToString());

            if (row.Table.Columns.Count >= 12)
            {
                _hasGotDataAvailability = true;
                _hourlyAvailability = new ECStationDataAvailability(
                    ECDataIntervalType.HOURLY, row[6].ToString(), row[7].ToString());
                _dailyAvailability = new ECStationDataAvailability(
                    ECDataIntervalType.DAILY, row[8].ToString(), row[9].ToString());
                _monthlyAvailability = new ECStationDataAvailability(
                    ECDataIntervalType.MONTHLY, row[10].ToString(), row[11].ToString());
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
            foreach (ECStationInfo info in stations)
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
            if (allDataDivNodes == null)
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
        public IFeature Feature { get { return _fea; } }

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
            return string.Format("{0},{1},{2},{3}", _name, _province, _elevation, _id);
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
                _id, _name, _province,
                _latitude, _longitude, _elevation,
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
                getFileName(1840, 1840, FormatType.ARCSWAT_TEXT, ECDataIntervalType.DAILY, isPrecipitation, false),
                Latitude, Longitude, Convert.ToInt32(Elevation));
        }

        public void ToArcSWAT2012CSVGageLocation(DbfFile dbf, bool isPrecipitation)
        {
            DbfRecord rec = new DbfRecord(dbf.Header);
            rec[0] = ID;
            rec[1] = getFileName(1840, 1840, FormatType.ARCSWAT_DBF, ECDataIntervalType.DAILY, isPrecipitation, false);
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
            if (obj.GetType() != typeof(ECStationInfo)) return false;

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

        public double getCriteria(int year, HuzelnutSuitabilityCriteriaType type)
        {
            if (!IsAvailableForYear(year)) return double.MinValue;

            if (type <= HuzelnutSuitabilityCriteriaType.Sf_W16)
                return getCriteriaFrostDays(year, (int)type);

            switch(type)
            {
                case HuzelnutSuitabilityCriteriaType.Avg:
                    return getCriteriaAverageTemperature(year);
                case HuzelnutSuitabilityCriteriaType.Forstf:
                    return getCriteriaFrostFreeDays(year);
                case HuzelnutSuitabilityCriteriaType.Ltemp:
                    return getCriteriaLowTemperature28Days(year);
                case HuzelnutSuitabilityCriteriaType.Stemp:
                    return getCriteriaLowTemperature40Days(year);
                default:
                    throw new Exception("Unknow Type!");
            }
        }

        public double getCriteriaAverageTemperature(int year)
        {
            DailyTemperatureStatisticsOneYear statics = new DailyTemperatureStatisticsOneYear(year, getTemperatureForOneYear(year));
            return statics.Average;
        }

        public int getCriteriaFrostFreeDays(int year)
        {
            DailyTemperatureStatisticsOneYear statics = new DailyTemperatureStatisticsOneYear(year, getTemperatureForOneYear(year));
            return statics.NumDayofFrostFree;
        }

        public int getCriteriaFrostDays(int year, int week)
        {
            DailyTemperatureStatisticsOneYear statics = new DailyTemperatureStatisticsOneYear(year, getTemperatureForOneYear(year));
            return statics.NumberofFrostInWeek(week);
        }

        public int getCriteriaLowTemperature40Days(int year)
        {
            DailyTemperatureStatisticsOneYear statics = new DailyTemperatureStatisticsOneYear(year, getTemperatureForOneYear(year));
            return statics.NumberofLowTemp40;
        }

        public int getCriteriaLowTemperature28Days(int year)
        {
            DailyTemperatureStatisticsOneYear statics = new DailyTemperatureStatisticsOneYear(year, getTemperatureForOneYear(year));
            return statics.NumberofLowTemp40;
        }

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
            string path = Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Temperature");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            path = Path.Combine(path, ID);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return Path.Combine(path, string.Format("{0}.csv", year));
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
            using (StreamReader reader = new StreamReader(cacheFile))
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
                else if (this.IsDailyAvailable)
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

                return save2Ascii(fields, startYear, endYear, destinationFolder, format, timeInterval);
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
                if (endYear > startYear)
                    return string.Format("{0}_{1}_{6}_{2}_{3}_{4}{5}",
                        _name.Replace(' ', '_'), _province, startYear, endYear, timeInterval, extension, ID);
                else
                    return string.Format("{0}_{1}_{5}_{2}_{3}{4}",
                        _name.Replace(' ', '_'), _province, startYear, timeInterval, extension, ID);
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
                Path.GetFullPath(destinationFolder), getFileName(startYear, endYear, format, timeInterval, true));  //precipitation

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
                getFileName(startYear, endYear, FormatType.ARCSWAT_TEXT, ECDataIntervalType.DAILY, true));  //precipitation
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
}
