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
        public string StationListCacheName { get { return "stationsInBoundary.shp"; } }

        /// <summary>
        /// All stations in given boundary read from given shapefile
        /// </summary>
        /// <remarks>
        /// All stations in boundary will be writen to a shapefile in temp folder with the name specified by StationListCacheName. 
        /// Existing shapefile will be re-write.</remarks>
        public List<ECStationInfo> Stations
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
                //default file path, in the exe folder
                string folder =
                    Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Shapefile");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                return folder;
            }
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
        public string getStationsShapefile(int startYear, int endYear)
        {            
            string stationShapefile = Path.Combine(ShapefileFolder,
                string.Format("huzelnut_{0}_{1}.shp", startYear, endYear));

            if (!File.Exists(stationShapefile))
            {
                if (Stations.Count == 0)
                    throw new System.Exception("There is no stations in given boundary!");

                //create the new shapefile to hold all the stations
                MultiPointShapefile newShapefile = new MultiPointShapefile();

                //get the projection
                Shapefile ref_sf = Shapefile.OpenFile(getShapefilePathInBoundary());
                newShapefile.Projection = ref_sf.Projection;
                ref_sf.Close();

                //remove some stations based on data availability
                foreach (ECStationInfo info in Stations)
                {
                    if (!info.IsAvailableForYear(startYear, endYear)) continue;

                    //add this station to the shapefile
                    newShapefile.Features.Add(info.Feature);
                }

                //save to file
                newShapefile.SaveAs(stationShapefile, true);

                //add criteria fields
                AddCriteriaFields(stationShapefile, startYear, endYear);
            }

            return stationShapefile;
        }

        #region Criteria

        private static string[] CRITERIA_FIELD_NAME = new string[] { "Avg", "Forstf", "Stemp", "Ltemp" };
        private static string FROST_FIELD_NAME = "Sf_W";

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
            }
            finally { sf.Close(); }
        }

        #endregion
    }
}
