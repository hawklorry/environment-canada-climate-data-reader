using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using DotSpatial.Data;
using System.Data;

namespace HAWKLORRY.HuzelnutSuitability
{
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

        /// <summary>
        /// The name of the shapefile used to save all stations in the boundary. 
        /// </summary>
        /// <remarks>If multiple project will be supported, each project should have their own name.</remarks>
        private string StationListCacheName { get { return "stationsInBoundary.shp"; } }

        /// <summary>
        /// All stations in given boundary read from given shapefile
        /// </summary>
        /// <remarks>
        /// All stations in boundary will be writen to a shapefile in temp folder with the name specified by StationListCacheName. 
        /// Existing shapefile will be re-write.</remarks>
        public List<ECStationInfo> StationsInBoundary
        {
            get
            {
                if (_stations == null) readECStationsInBoundary();
                return _stations;
            }
        }
        
        /// <summary>
        /// This is the function do the real work for property Stations: Read all stations in given boundary and same them to the shapefile.
        /// </summary>
        private void readECStationsInBoundary()
        {
            if (_stations != null) return;

            //load from save stations
            if (!File.Exists(getShapefilePathInBoundary()))
            {
                if (!string.IsNullOrEmpty(_shapefileName))
                {
                    _stations = EC.SearchByShapefile(_shapefileName, getShapefilePathInBoundary);
                }
            }

            _stations = ECStationInfo.FromShapefile(getShapefilePathInBoundary());
        }

        /// <summary>
        /// Get the path of the shapefile for the stations in the given boundary. It's located at the shapefile folder
        /// </summary>
        /// <returns></returns>
        private string getShapefilePathInBoundary()
        {
            return Path.Combine(ShapefileFolder, StationListCacheName);
        }

        /// <summary>
        /// The folder to save all stations in given boundary and has data between given start and end year. These shapefiles will be the 
        /// final product.
        /// </summary>
        private string ShapefileFolder
        {
            get
            {
                return getSpecificFolder("Shapefile");
            }
        }

        private string FinalTableFolder
        {
            get
            {
                return getSpecificFolder("FinalTable");
            }
        }

        private string getSpecificFolder(string folderName)
        {
            //default file path, in the exe folder
            string folder =
                Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), folderName);
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            return folder;
        }

        private string getFinalTableFileName(HuzelnutSuitabilityCriteriaType type)
        {
            return Path.Combine(FinalTableFolder, string.Format("{0}.csv", type));
        }

        private string getFinalTableHeader(int startYear, int endYear, HuzelnutSuitabilityCriteriaType type)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("station ID");
            for(int year = startYear;year <= endYear;year ++)
            {
                sb.Append(",");
                sb.Append(string.Format("{0}_{1}",type,year));                   
            }
            if(type > HuzelnutSuitabilityCriteriaType.Sf_W16)
            {
                sb.Append(",");
                sb.Append(string.Format("{0}_{1}", type, "All"));
            }
            if(type == HuzelnutSuitabilityCriteriaType.Avg)
            {
                sb.Append(",");
                sb.Append("Num>16.7"); //number of year with ave temp > 16.7
            }
            return sb.ToString();
        }

        public void generateTable(int startYear, int endYear)
        {
            for (int i = (int)(HuzelnutSuitabilityCriteriaType.Sf_W1); i <= (int)(HuzelnutSuitabilityCriteriaType.Lowest); i++)
                generateTable(startYear, endYear, (HuzelnutSuitabilityCriteriaType)i);
        }

        public void generateTable(int startYear, int endYear, HuzelnutSuitabilityCriteriaType type)
        {
            using (StreamWriter writer = new StreamWriter(getFinalTableFileName(type)))
            {
                writer.WriteLine(getFinalTableHeader(startYear, endYear, type));
                List<ECStationInfo> stations = getStationsAvailableInYearRange(startYear, endYear);
                for (int i = 0; i < stations.Count; i++)
                {
                    DailyTemperatureStatisticsMultipleYear statistics =
                        new DailyTemperatureStatisticsMultipleYear(startYear, endYear, stations[i]);

                    //get the criteria string for all years
                    string criteria = statistics.getCriteriaString(type);

                    //only write line when station has temperature data
                    if(!string.IsNullOrEmpty(criteria))
                        writer.WriteLine(criteria);
                }
            }
        }

        /// <summary>
        /// Cache all stations in a given year range
        /// </summary>
        private Dictionary<string, List<ECStationInfo>> _stationsAvailableInYearRange = new Dictionary<string, List<ECStationInfo>>();

        /// <summary>
        /// Get all station info in given year range 
        /// </summary>
        /// <param name="startYear"></param>
        /// <param name="endYear"></param>
        /// <returns></returns>
        public List<ECStationInfo> getStationsAvailableInYearRange(int startYear, int endYear)
        {
            string key = string.Format("{0}_{1}", startYear, endYear);

            if (!_stationsAvailableInYearRange.ContainsKey(key))
                _stationsAvailableInYearRange.Add(key, ECStationInfo.FromShapefile(getStationShapefile(startYear,endYear)));
            return _stationsAvailableInYearRange[key];
        }

        /// <summary>
        /// A new shapefile will be generated for each given start and end year. They may have different stations available. 
        /// </summary>
        /// <param name="startYear"></param>
        /// <param name="endYear"></param>
        /// <returns></returns>
        /// <remarks>
        /// The shapefile is generated based on the cached shapefile which has all stations in boundary. So the shapefile must be generated 
        /// first before this function could be used.
        /// The criteria fields are also added.
        /// </remarks>
        private string getStationShapefile(int startYear, int endYear)
        {
            string stationShapefile = Path.Combine(ShapefileFolder,
                string.Format("huzelnut_{0}_{1}.shp", startYear, endYear));

            if (!File.Exists(stationShapefile))
            {
                if (StationsInBoundary.Count == 0)
                    throw new System.Exception("There is no stations in given boundary!");

                //stations has data in given year range
                List<IFeature> stationsAvailable = new List<IFeature>();

                //remove some stations based on data availability
                foreach (ECStationInfo info in StationsInBoundary)
                {
                    if (!info.IsAvailableForYear(startYear, endYear)) continue;
                    stationsAvailable.Add(info.Feature);
                }

                //create the shapefile
                FeatureSet newShapefile = new FeatureSet(stationsAvailable);

                //get the projection
                Shapefile ref_sf = Shapefile.OpenFile(getShapefilePathInBoundary());
                newShapefile.Projection = ref_sf.Projection;
                ref_sf.Close();

                //save to file
                newShapefile.SaveAs(stationShapefile, true);

                //add criteria fields
                AddCriteriaFields(stationShapefile, startYear, endYear);
            }
            return stationShapefile;
        }

        #region Criteria

        private static string[] CRITERIA_FIELD_NAME = new string[] { "Avg", "Forst", "Stemp", "Ltemp" };
        private static string FROST_FIELD_NAME = "Sf";

        /// <summary>
        /// Add all criterias to the shapefile
        /// </summary>
        /// <param name="shapefileName"></param>
        /// <param name="startYear"></param>
        /// <param name="endYear"></param>
        private static void AddCriteriaFields(string shapefileName, int startYear, int endYear)
        {
            Shapefile sf = Shapefile.OpenFile(shapefileName);
            try
            {
                DataTable dt = sf.DataTable;

                //get all the names
                //don't exceed the max length 10
                StringCollection names = new StringCollection();
                foreach (string criteria in CRITERIA_FIELD_NAME) names.Add(criteria);
                for (int i = 0; i <= 16; i++) names.Add(string.Format("{0}{1}", FROST_FIELD_NAME, i));

                //add all fields
                foreach (string criteria in names)
                {
                    for(int year = startYear;year <= endYear;year ++)
                    {
                        string fieldName = string.Format("{0}_{1}",criteria,year);
                        if (dt.Columns.IndexOf(fieldName) > -1) continue;
                                          
                        if(criteria.Equals("Avg"))
                            dt.Columns.Add(fieldName, typeof(double));  //double
                        else
                            dt.Columns.Add(fieldName, typeof(int));     //int
                    }
                }

                //save the change
                sf.Save();
            }
            finally { sf.Close(); }
        }

        #endregion
    }
}
