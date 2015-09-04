using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Data;
using System.Data.OleDb;
using DotSpatial.Data;
using DotSpatial.Topology;


namespace HAWKLORRY
{  
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
        public static List<ECStationInfo> SearchByShapefile(string shapefilePath, Func<string> selectedStationsShapefilePath = null)
        {
            if (!System.IO.File.Exists(shapefilePath))
                throw new Exception(shapefilePath + " doesn't exit!");

            string allStationShapeFile = GetAllStationShapeFile();
            Shapefile allsf = Shapefile.OpenFile(allStationShapeFile);
            Shapefile sf = Shapefile.OpenFile(shapefilePath);

            try
            {
                //check the projection, make sure it's using WGS1984
                if (!sf.Projection.GeographicInfo.Name.Equals("GCS_North_American_1983"))
                    throw new Exception("The boundary shapefile should use GCS_North_American_1983 projection!");
                
                if(sf.FeatureType != FeatureType.Polygon)
                    throw new Exception("The boundary shapefile is not polygon!");

                //return value
                List<ECStationInfo> selectedStations = new List<ECStationInfo>();

                //try to find all the climate stations located in boundary
                List<IFeature> stations = allsf.Select(sf.Extent);
                List<IFeature> stationsInBoundary = new List<IFeature>();
                foreach (IFeature station in stations)
                {
                    foreach (IFeature boundary in sf.Features)
                    {
                        if ((boundary.BasicGeometry as IPolygon).Contains(station.BasicGeometry as IGeometry))
                        {
                            selectedStations.Add(new ECStationInfo(station));
                            stationsInBoundary.Add(station);
                            break;
                        }
                    }                    
                }

                //save to shapefile if necessary
                //shapefile for all stations in boundary
                //the existing file will be overwrite                
                if (selectedStationsShapefilePath != null)
                {
                    //MultiPointShapefile selectedStationsShapefile = new MultiPointShapefile();
                    //selectedStationsShapefile.Projection = allsf.Projection;
                    //selectedStationsShapefile.CopyFeatures(new FeatureSet(stationsInBoundary), true);

                    FeatureSet newShapefile = new FeatureSet(stationsInBoundary);
                    newShapefile.Projection = allsf.Projection;
                    newShapefile.SaveAs(selectedStationsShapefilePath(), true);
                }

                return selectedStations;
            }
            finally
            {
                allsf.Close();
                sf.Close();
            }               

        }

    }
}
