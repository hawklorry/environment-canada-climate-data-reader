using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using LumenWorks.Framework.IO.Csv;
using SocialExplorer.IO.FastDBF;

namespace HAWKLORRY
{
    partial class FrmDownloadEnvrionmentCanadaClimateData : Form
    {
        private int[] _fields = null;
        private string _path = "";
        private int _startYear = 1840;
        private int _endYear = 2013;        

        private List<ECStationInfo> _stations = null;
        private bool _isDownloadAllStations = false;


        private static int[] DATA_FIELD_INDEX_DAILY_TEMP = { 5, 7, 9 };
        private static int[] DATA_FIELD_INDEX_DAILY = { 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25 };
        private static string[] DATA_FIELD_NAME_DAILY = 
        {   "Max Temp (°C)","Min Temp (°C)","Mean Temp (°C)","Heat Deg Days (°C)",
            "Cool Deg Days (°C)","Total Rain (mm)","Total Snow (cm)","Total Precip (mm)",
            "Snow on Grnd (cm)","Dir of Max Gust (10s deg)","Spd of Max Gust (km/h)"};

        private static int[] DATA_FIELD_INDEX_HOURLY_TEMP = { 6 };
        private static int[] DATA_FIELD_INDEX_HOURLY = { 6, 8, 10, 12, 14, 16, 18, 20, 22 };
        private static string[] DATA_FIELD_NAME_HOURLY = 
        {   "Temp (°C)", "Dew Point Temp (°C)", "Rel Hum (%)", "Wind Dir (10s deg)",
            "Wind Spd (km/h)", "Visibility (km)", "Stn Press (kPa)", "Hmdx", "Wind Chill"
        };

        private int[] DataFieldIndex
        {
            get
            {
                if (TimeInterval == ECDataIntervalType.DAILY) return DATA_FIELD_INDEX_DAILY;
                if (TimeInterval == ECDataIntervalType.HOURLY) return DATA_FIELD_INDEX_HOURLY;
                return new int[] { };
            }
        }

        public List<ECStationInfo> SelectedStations
        {
            get
            {
                return _stations;
            }
            set
            {
                _stations = value;

                //add handler
                foreach (ECStationInfo info in _stations)
                    info.ProgressChanged += onStaionClimateDataDownloadingProgressChanged;

                if (_stations.Count == 0)
                    lblSelectedStations.Text = "No station is seleted.";
                else
                    lblSelectedStations.Text = _stations.Count.ToString() + " stations are seleted.";

            }
        }

        public FrmDownloadEnvrionmentCanadaClimateData()
        {
            InitializeComponent();

            //time
            txtStartYear.TextChanged += (s, ee) => { int.TryParse(txtStartYear.Text, out _startYear); };
            txtEndYear.TextChanged += (s, ee) => { int.TryParse(txtEndYear.Text, out _endYear); };

            //stations
            bDefineStations.Click += (s, ee) => 
            {
                FrmDefineStations frm = new FrmDefineStations();

                //remove handler
                if (_stations != null && _stations.Count > 0)
                {
                    foreach (ECStationInfo info in _stations)
                        info.ProgressChanged -= onStaionClimateDataDownloadingProgressChanged;
                }

                //set to frm
                frm.SelectedStations = _stations;
                frm.DataType = TimeInterval;

                if (frm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    SelectedStations = frm.SelectedStations;
                }
                else
                    SelectedStations = SelectedStations; //for case when user doesn't click OK to add event handler.
            };
            bLoadSavedStations.Click += (s, ee) =>
                {
                    if (dlgLoadSaveStations.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        SelectedStations = EC.ReadStations(dlgLoadSaveStations.FileName);
                        MessageBox.Show(SelectedStations.Count.ToString() + " stations are loaded.","ECReader");
                    }
                };

            lblLatestVersion.LinkClicked += (s, ee) => { System.Diagnostics.Process.Start("http://wp.me/s2CzBq-325"); };
            lblStationLocation.LinkClicked += (s, ee) => { System.Diagnostics.Process.Start("http://wp.me/p2CzBq-68"); };
            lblFeedback.LinkClicked += (s, ee) => 
            {
                try
                {
                    System.Diagnostics.Process.Start("mailto:hawklorry@gmail.com");
                }
                catch
                {
                    System.Windows.Forms.Clipboard.SetText("hawklorry@gmail.com");
                    showInformationWindow("My email address has been copyied to clipboard.");
                }                
            };
            
            //the output time interval
            this.rdbTimeIntervalDaily.CheckedChanged += (s, ee) => { this.updateTimeIntervalSelectionControl(); };
            this.rdbTimeIntervalHourly.CheckedChanged += (s, ee) => { this.updateTimeIntervalSelectionControl(); };
            this.rdbTimeIntervalMonthly.CheckedChanged += (s, ee) => { this.updateTimeIntervalSelectionControl(); };

            //the output format
            this.rdbFormatArcSWATDbf.CheckedChanged += (s, ee) => { updateFormatSelectionControl(); };
            this.rdbFormatArcSWATTxt.CheckedChanged += (s, ee) => { updateFormatSelectionControl(); };
            this.rdbFormatFreeCSV.CheckedChanged += (s, ee) => { updateFormatSelectionControl(); };
            this.rdbFormatFreeText.CheckedChanged += (s, ee) => { updateFormatSelectionControl(); };
            this.rdbFormatSWATInput.CheckedChanged += (s, ee) => { updateFormatSelectionControl(); };
            
            //select output fields
            listFields.ItemCheck += (s, ee) =>                
                {
                    List<int> f = new List<int>();

                    foreach(int index in listFields.CheckedIndices)
                    {
                        if (index != ee.Index) 
                            f.Add(DataFieldIndex[index]);
                    }
                    if (ee.NewValue == CheckState.Checked) f.Add(DataFieldIndex[ee.Index]);

                    _fields = null;

                    if(f.Count > 0)
                        _fields = f.ToArray();
                };
            bSelectAll.Click += (s, ee) => 
            {
                for (int i = 0; i < listFields.Items.Count; i++)
                    listFields.SetItemCheckState(i, CheckState.Checked);
            };

            //select output folder
            bBrowseOutput.Click +=
                (s, ee) =>
                {
                    FolderBrowserDialog dlg = new FolderBrowserDialog();
                    dlg.SelectedPath = _path;
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        txtPath.Text = dlg.SelectedPath;
                        _path = dlg.SelectedPath;
                    }
                };
            txtPath.TextChanged += (s, ee) => { if (System.IO.Directory.Exists(txtPath.Text)) _path = txtPath.Text; else txtPath.Text = _path; };

            bGetAllStations.Click += (s, ee) =>
                {
                    if (backgroundWorker1.IsBusy)
                    {
                        showInformationWindow("Please wait current process to finish.");
                        return;
                    }

                    _isDownloadAllStations = true;
                    backgroundWorker1.RunWorkerAsync();
                };

            //download button
            bDownload.Click += (s, ee) =>
                {
                    if (backgroundWorker1.IsBusy)
                    {
                        showInformationWindow("Please wait current process to finish.");
                        return;
                    }

                    //check stations
                    if(_stations == null || _stations.Count == 0)
                    {
                        showInformationWindow("Please define stations first.");
                        return;
                    }

                    //check field list
                    if (listFields.Enabled && _fields == null)
                    {
                        showInformationWindow("Please select output fields.");
                        return;
                    }

                    if (Format == FormatType.SWAT_TEXT)
                    {
                        showInformationWindow("Coming soon. Stay tuned:)");
                        return;
                    }

                    if (_endYear < _startYear)
                    {
                        showInformationWindow("The end year couldn't be earlier than start year.");
                        return;
                    }

                    progressBar1.Maximum = (_endYear - _startYear + 1) * 2;
                    _maxValueofProgressBar = progressBar1.Maximum;

                    backgroundWorker1.RunWorkerAsync();
                };

            //worker
            backgroundWorker1.ProgressChanged += (s, ee) => 
            {
                if (richTextBox1.Text.Length > 0)
                    richTextBox1.AppendText(Environment.NewLine);
                richTextBox1.AppendText(ee.UserState.ToString());
                richTextBox1.SelectionStart = richTextBox1.Text.Length; 
                richTextBox1.ScrollToCaret();

                progressBar1.Value =
                    ee.ProgressPercentage > progressBar1.Maximum ? progressBar1.Maximum : ee.ProgressPercentage;
            };
            backgroundWorker1.RunWorkerCompleted += (s, ee) => 
            {
                _isDownloadAllStations = false;
                //this.progressBar1.Value = this.progressBar1.Maximum;
                //this.richTextBox1.Text += "finished";                                   
            };
            backgroundWorker1.DoWork +=
                (s, ee) =>
                {
                    backgroundWorker1.ReportProgress(0, "--------------------------------------------");

                    if (_isDownloadAllStations) //download all stations, update ecstations.csv
                    {
                        backgroundWorker1.ReportProgress(0, "Downloading all EC stations.");
                        EC.RetrieveAndSaveAllStations(
                            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\ecstations.csv",
                            backgroundWorker1);
                        backgroundWorker1.ReportProgress(100,
                            "Downloading all EC stations finished. Location: " + Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\ecstations.csv");
                    }
                    else//download daily, hourly data
                    {
                        createGageLocationFile();
                        string msg = "";
                        if (_stations.Count == 1)        //one station 
                        {
                            ECStationInfo station = _stations[0];                            
                            downloadOneStation(station, out msg);
                            backgroundWorker1.ReportProgress(_maxValueofProgressBar, msg);
                            showInformationWindow(msg);
                        }
                        else                    //multiple stations
	                    {
                            //needs to write into one file
                            if (Format == FormatType.SWAT_TEXT)
                            {

                            }
                            else
                            {
                                bool totalStatus = false;
                                StringBuilder sb = new StringBuilder();
                                sb.AppendLine("------------ Summary ------------");
                                foreach (ECStationInfo onestation in _stations)
                                {
                                    bool status = downloadOneStation(onestation, out msg);
                                    totalStatus |= status;

                                    if (msg.Length > 0)
                                        sb.AppendLine(msg);
                                }

                                showInformationWindow("Finished." + (totalStatus ? "" : "Please check messages in the message window."));
                                backgroundWorker1.ReportProgress(_maxValueofProgressBar, sb.ToString());                            
                            }
	                    }
                        
                    }
                };

            //open the output folder
            bOpen.Click += (s, ee) => { if (System.IO.Directory.Exists(_path)) System.Diagnostics.Process.Start(_path); };

            this.FormClosing += (s, ee) =>
                {
                    EC.SaveStations(SelectedStations);
                };
        }

        /// <summary>
        /// create precipitation and temperature gage location file in txt or dbf file
        /// </summary>
        private void createGageLocationFile()
        {
            FormatType type = Format;
            if (type != FormatType.ARCSWAT_DBF && type != FormatType.ARCSWAT_TEXT) return;
            if (_stations == null || _stations.Count == 0) return;

            //for ArcSWAT 2012 text format
            if (type == FormatType.ARCSWAT_TEXT)
            {
                StringBuilder sb_p = new StringBuilder();
                StringBuilder sb_t = new StringBuilder();
                sb_p.AppendLine("ID,NAME,LAT,LONG,ELEVATION");
                sb_t.AppendLine("ID,NAME,LAT,LONG,ELEVATION");
                foreach (ECStationInfo info in _stations)
                {
                    sb_p.AppendLine(info.ToArcSWAT2012CSVGageLocation(true)); //precipitation
                    sb_t.AppendLine(info.ToArcSWAT2012CSVGageLocation(false));//temperature
                }
                string pFileName = "pcp.txt";
                string tFileName = "tmp.txt";
                using (StreamWriter writer = new StreamWriter(_path + @"\" + pFileName))
                    writer.Write(sb_p.ToString());
                using (StreamWriter writer = new StreamWriter(_path + @"\" + tFileName))
                    writer.Write(sb_t.ToString());
            }
            else if (type == FormatType.ARCSWAT_DBF)
            {
                string pFileName = "pcp.dbf";
                string tFileName = "tmp.dbf";
                DbfFile pDBF = createDBFGageLocationFile(_path + @"\" + pFileName);
                DbfFile tDBF = createDBFGageLocationFile(_path + @"\" + tFileName);

                DbfRecord pRec = new DbfRecord(pDBF.Header);
                DbfRecord tRec = new DbfRecord(tDBF.Header);

                foreach (ECStationInfo info in _stations)
                {
                    info.ToArcSWAT2012CSVGageLocation(pDBF, true);
                    info.ToArcSWAT2012CSVGageLocation(tDBF, false);
                }
                pDBF.Close();
                tDBF.Close();
            }
        }

        private DbfFile createDBFGageLocationFile(string file)
        {
            DbfFile dbf = new DbfFile();
            dbf.Open(file, FileMode.Create);
            dbf.Header.AddColumn(new DbfColumn("ID", DbfColumn.DbfColumnType.Number, 8, 0)); //integer is a binary Integer type
            dbf.Header.AddColumn(new DbfColumn("NAME", DbfColumn.DbfColumnType.Character,8,0));
            dbf.Header.AddColumn(new DbfColumn("LAT", DbfColumn.DbfColumnType.Number,8,3));
            dbf.Header.AddColumn(new DbfColumn("LONG", DbfColumn.DbfColumnType.Number,8,3));
            dbf.Header.AddColumn(new DbfColumn("ELEVATION", DbfColumn.DbfColumnType.Number, 4, 0));//integer is a binary Integer type
            return dbf;
        }

        private bool downloadOneStation(ECStationInfo station, out string message)
        {
            message = "";
            try
            {
                bool status = station.save(_fields, _startYear, _endYear, _path, Format,TimeInterval);
                if (!status)
                    message = station.Name + ": There is no data between "
                        + _startYear.ToString() + " and " + _endYear.ToString() + ". Please check output messages.";
                else
                {
                    message = station.WarningMessage;
                    if (message.Length == 0)
                        message = station.Name + ": Finished.";
                }
                return status;
            }
            catch (System.Exception e)
            {
                message = station.Name + ": " + e.Message;
                return false;
            }
        }

        private void onStaionClimateDataDownloadingProgressChanged(object sender, EventArgs e)
        {
            BaseWithProcess p = sender as BaseWithProcess;
            backgroundWorker1.ReportProgress(p.ProcessPercentage, p.ProcessMessage);
        }

        private int _maxValueofProgressBar = 100;

        private void FrmDownloadEnvrionmentCanadaClimateData_Load(object sender, EventArgs e)
        {
            rdbFormatArcSWATTxt.Checked = true;
            rdbTimeIntervalDaily.Checked = true;
            rdbTimeIntervalMonthly.Enabled = false;

            updateFormatSelectionControl();

            _path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            txtPath.Text = _path;

            txtStartYear.Text = _startYear.ToString();
            txtEndYear.Text = _endYear.ToString();

            lblSelectedStations.Text = "No station is seleted.";

            //load automatically save stations
            SelectedStations = EC.SavedStations;
         }

        private void updateFormatSelectionControl()
        {
            listFields.Enabled =
                rdbFormatFreeCSV.Checked || rdbFormatFreeText.Checked;
            bSelectAll.Enabled = listFields.Enabled;
        }

        private FormatType Format
        {
            get
            {
                if (rdbFormatArcSWATDbf.Checked) return FormatType.ARCSWAT_DBF;
                if (rdbFormatArcSWATTxt.Checked) return FormatType.ARCSWAT_TEXT;
                if (rdbFormatFreeCSV.Checked) return FormatType.SIMPLE_CSV;
                if (rdbFormatFreeText.Checked) return FormatType.SIMPLE_TEXT;
                if (rdbFormatSWATInput.Checked) return FormatType.SWAT_TEXT;
                return FormatType.SIMPLE_TEXT;
            }
        }

        private void updateTimeIntervalSelectionControl()
        {
            listFields.Items.Clear();
            string[] fields = null;

            if (rdbTimeIntervalDaily.Checked)
                fields = DATA_FIELD_NAME_DAILY;
            else if (this.rdbTimeIntervalHourly.Checked)
                fields = DATA_FIELD_NAME_HOURLY;

            if (fields == null) return;

            foreach (string f in fields)
                listFields.Items.Add(f);

            rdbFormatArcSWATDbf.Enabled = rdbTimeIntervalDaily.Checked;
            rdbFormatArcSWATTxt.Enabled = rdbTimeIntervalDaily.Checked;
            rdbFormatSWATInput.Enabled = rdbTimeIntervalDaily.Checked;

            if (!rdbFormatFreeCSV.Checked && !rdbFormatFreeText.Checked)
                rdbFormatFreeCSV.Checked = true;

            _fields = null;
        }

        private ECDataIntervalType TimeInterval
        {
            get
            {
                if (rdbTimeIntervalDaily.Checked) return ECDataIntervalType.DAILY;
                if (rdbTimeIntervalHourly.Checked) return ECDataIntervalType.HOURLY;
                if (rdbTimeIntervalMonthly.Checked) return ECDataIntervalType.MONTHLY;
                return ECDataIntervalType.DAILY;
            }
        }

        private void showInformationWindow(string msg)
        {
            MessageBox.Show(msg, "Environment Canada Climte Data");
        }
    }
}
