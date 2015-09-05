using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using DotSpatial.Data;

namespace HAWKLORRY.HuzelnutSuitability
{
    public partial class FrmHuzelnutSuitability : Form
    {
        public FrmHuzelnutSuitability()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //HuzelnutSuitabilityProject project = new HuzelnutSuitabilityProject(
            //    @"D:\Michael\git\fromwanhongreneedyourhelpwithprogrammingforclimate\County_Southern_ONT.shp");

            HuzelnutSuitabilityProject project = new HuzelnutSuitabilityProject(
                @"C:\dev\hezel_suitability\doc\gis\County_Southern_ONT.shp");

            project.generateTable(2000, 2014);

            //List<ECStationInfo> stations = project.getStationsAvailableInYearRange(2000, 2014);
            //for (int i = 0; i < stations.Count; i++)
            //{
            //    ECStationInfo info = stations[i];
            //    DailyTemperatureStatisticsMultipleYear statistics =
            //        new DailyTemperatureStatisticsMultipleYear(2000, 2014, info);



                //System.Diagnostics.Debug.WriteLine(string.Format("\n#{0},{1}", i, info.Name));
                //for (int year = 2000; year <= 2014; year++)
                //{
                //    System.Diagnostics.Debug.Write(year);
                //    if (!info.IsAvailableForYear(year))
                //    {
                //        System.Diagnostics.Debug.Write(",skip");
                //        continue;
                //    }
                //    System.Diagnostics.Debug.Write(",");
                //    info.getCateriaAverageTemperature
                //}
            //}
        }

        private void FrmHuzelnutSuitability_Load(object sender, EventArgs e)
        {

        }
    }
}
