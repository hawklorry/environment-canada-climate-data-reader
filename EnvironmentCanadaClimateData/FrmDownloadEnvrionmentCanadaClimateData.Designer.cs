namespace HAWKLORRY
{
    partial class FrmDownloadEnvrionmentCanadaClimateData
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmDownloadEnvrionmentCanadaClimateData));
            this.bDownload = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.listFields = new System.Windows.Forms.CheckedListBox();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.bBrowseOutput = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.txtStartYear = new System.Windows.Forms.TextBox();
            this.txtEndYear = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.bLoadSavedStations = new System.Windows.Forms.Button();
            this.lblStationLocation = new System.Windows.Forms.LinkLabel();
            this.lblSelectedStations = new System.Windows.Forms.Label();
            this.bDefineStations = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.bOpen = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.rdbFormatSWATInput = new System.Windows.Forms.RadioButton();
            this.rdbFormatFreeCSV = new System.Windows.Forms.RadioButton();
            this.rdbFormatFreeText = new System.Windows.Forms.RadioButton();
            this.rdbFormatArcSWATTxt = new System.Windows.Forms.RadioButton();
            this.rdbFormatArcSWATDbf = new System.Windows.Forms.RadioButton();
            this.bSelectAll = new System.Windows.Forms.Button();
            this.dlgLoadSaveStations = new System.Windows.Forms.OpenFileDialog();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.lblLatestVersion = new System.Windows.Forms.LinkLabel();
            this.lblFeedback = new System.Windows.Forms.LinkLabel();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.rdbTimeIntervalMonthly = new System.Windows.Forms.RadioButton();
            this.rdbTimeIntervalDaily = new System.Windows.Forms.RadioButton();
            this.rdbTimeIntervalHourly = new System.Windows.Forms.RadioButton();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.bGetAllStations = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.SuspendLayout();
            // 
            // bDownload
            // 
            this.bDownload.Location = new System.Drawing.Point(308, 221);
            this.bDownload.Name = "bDownload";
            this.bDownload.Size = new System.Drawing.Size(97, 21);
            this.bDownload.TabIndex = 0;
            this.bDownload.Text = "Download";
            this.bDownload.UseVisualStyleBackColor = true;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(12, 308);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(744, 242);
            this.richTextBox1.TabIndex = 2;
            this.richTextBox1.Text = "";
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            // 
            // listFields
            // 
            this.listFields.FormattingEnabled = true;
            this.listFields.Items.AddRange(new object[] {
            "Max Temp (°C)",
            "Min Temp (°C)",
            "Mean Temp (°C)",
            "Heat Deg Days (°C)",
            "Cool Deg Days (°C)",
            "Total Rain (mm)",
            "Total Snow (cm)",
            "Total Precip (mm)",
            "Snow on Grnd (cm)",
            "Dir of Max Gust (10s deg)",
            "Spd of Max Gust (km/h)"});
            this.listFields.Location = new System.Drawing.Point(16, 20);
            this.listFields.Name = "listFields";
            this.listFields.Size = new System.Drawing.Size(191, 132);
            this.listFields.TabIndex = 3;
            // 
            // txtPath
            // 
            this.txtPath.Location = new System.Drawing.Point(22, 18);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(251, 21);
            this.txtPath.TabIndex = 5;
            // 
            // bBrowseOutput
            // 
            this.bBrowseOutput.Location = new System.Drawing.Point(179, 42);
            this.bBrowseOutput.Name = "bBrowseOutput";
            this.bBrowseOutput.Size = new System.Drawing.Size(94, 21);
            this.bBrowseOutput.TabIndex = 6;
            this.bBrowseOutput.Text = "Change ...";
            this.bBrowseOutput.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "Start Year";
            // 
            // txtStartYear
            // 
            this.txtStartYear.Location = new System.Drawing.Point(116, 21);
            this.txtStartYear.Name = "txtStartYear";
            this.txtStartYear.Size = new System.Drawing.Size(157, 21);
            this.txtStartYear.TabIndex = 1;
            this.txtStartYear.Text = "2000";
            // 
            // txtEndYear
            // 
            this.txtEndYear.Location = new System.Drawing.Point(116, 45);
            this.txtEndYear.Name = "txtEndYear";
            this.txtEndYear.Size = new System.Drawing.Size(157, 21);
            this.txtEndYear.TabIndex = 1;
            this.txtEndYear.Text = "2010";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(19, 48);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 4;
            this.label4.Text = "End Year";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.bLoadSavedStations);
            this.groupBox1.Controls.Add(this.lblStationLocation);
            this.groupBox1.Controls.Add(this.lblSelectedStations);
            this.groupBox1.Controls.Add(this.bDefineStations);
            this.groupBox1.Location = new System.Drawing.Point(12, 11);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(288, 107);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Stations";
            // 
            // bLoadSavedStations
            // 
            this.bLoadSavedStations.Location = new System.Drawing.Point(145, 18);
            this.bLoadSavedStations.Name = "bLoadSavedStations";
            this.bLoadSavedStations.Size = new System.Drawing.Size(128, 23);
            this.bLoadSavedStations.TabIndex = 16;
            this.bLoadSavedStations.Text = "Load Saved Stations";
            this.bLoadSavedStations.UseVisualStyleBackColor = true;
            // 
            // lblStationLocation
            // 
            this.lblStationLocation.AutoSize = true;
            this.lblStationLocation.Location = new System.Drawing.Point(8, 74);
            this.lblStationLocation.Name = "lblStationLocation";
            this.lblStationLocation.Size = new System.Drawing.Size(227, 12);
            this.lblStationLocation.TabIndex = 15;
            this.lblStationLocation.TabStop = true;
            this.lblStationLocation.Text = "Download EC Stations (Shapefile & kmz)";
            // 
            // lblSelectedStations
            // 
            this.lblSelectedStations.AutoSize = true;
            this.lblSelectedStations.Location = new System.Drawing.Point(8, 49);
            this.lblSelectedStations.Name = "lblSelectedStations";
            this.lblSelectedStations.Size = new System.Drawing.Size(119, 12);
            this.lblSelectedStations.TabIndex = 1;
            this.lblSelectedStations.Text = "lblSelectedStations";
            // 
            // bDefineStations
            // 
            this.bDefineStations.Location = new System.Drawing.Point(11, 18);
            this.bDefineStations.Name = "bDefineStations";
            this.bDefineStations.Size = new System.Drawing.Size(128, 21);
            this.bDefineStations.TabIndex = 0;
            this.bDefineStations.Text = "Define Stations ...";
            this.bDefineStations.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.txtStartYear);
            this.groupBox2.Controls.Add(this.txtEndYear);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Location = new System.Drawing.Point(12, 124);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(288, 75);
            this.groupBox2.TabIndex = 9;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Time";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.bOpen);
            this.groupBox3.Controls.Add(this.bBrowseOutput);
            this.groupBox3.Controls.Add(this.txtPath);
            this.groupBox3.Location = new System.Drawing.Point(12, 204);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(288, 72);
            this.groupBox3.TabIndex = 10;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Output Folder";
            // 
            // bOpen
            // 
            this.bOpen.Location = new System.Drawing.Point(22, 42);
            this.bOpen.Name = "bOpen";
            this.bOpen.Size = new System.Drawing.Size(92, 21);
            this.bOpen.TabIndex = 4;
            this.bOpen.Text = "Open Folder";
            this.bOpen.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.rdbFormatSWATInput);
            this.groupBox4.Controls.Add(this.rdbFormatFreeCSV);
            this.groupBox4.Controls.Add(this.rdbFormatFreeText);
            this.groupBox4.Controls.Add(this.rdbFormatArcSWATTxt);
            this.groupBox4.Controls.Add(this.rdbFormatArcSWATDbf);
            this.groupBox4.Location = new System.Drawing.Point(308, 68);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(219, 131);
            this.groupBox4.TabIndex = 11;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Format";
            // 
            // rdbFormatSWATInput
            // 
            this.rdbFormatSWATInput.AutoSize = true;
            this.rdbFormatSWATInput.Location = new System.Drawing.Point(7, 103);
            this.rdbFormatSWATInput.Name = "rdbFormatSWATInput";
            this.rdbFormatSWATInput.Size = new System.Drawing.Size(161, 16);
            this.rdbFormatSWATInput.TabIndex = 0;
            this.rdbFormatSWATInput.TabStop = true;
            this.rdbFormatSWATInput.Text = "SWAT Input (.pcp, .tmp)";
            this.rdbFormatSWATInput.UseVisualStyleBackColor = true;
            // 
            // rdbFormatFreeCSV
            // 
            this.rdbFormatFreeCSV.AutoSize = true;
            this.rdbFormatFreeCSV.Location = new System.Drawing.Point(6, 82);
            this.rdbFormatFreeCSV.Name = "rdbFormatFreeCSV";
            this.rdbFormatFreeCSV.Size = new System.Drawing.Size(167, 16);
            this.rdbFormatFreeCSV.TabIndex = 0;
            this.rdbFormatFreeCSV.TabStop = true;
            this.rdbFormatFreeCSV.Text = "Free Format CSV (.csv)  ";
            this.rdbFormatFreeCSV.UseVisualStyleBackColor = true;
            // 
            // rdbFormatFreeText
            // 
            this.rdbFormatFreeText.AutoSize = true;
            this.rdbFormatFreeText.Location = new System.Drawing.Point(6, 61);
            this.rdbFormatFreeText.Name = "rdbFormatFreeText";
            this.rdbFormatFreeText.Size = new System.Drawing.Size(173, 16);
            this.rdbFormatFreeText.TabIndex = 0;
            this.rdbFormatFreeText.TabStop = true;
            this.rdbFormatFreeText.Text = "Free Format Text (.txt)  ";
            this.rdbFormatFreeText.UseVisualStyleBackColor = true;
            // 
            // rdbFormatArcSWATTxt
            // 
            this.rdbFormatArcSWATTxt.AutoSize = true;
            this.rdbFormatArcSWATTxt.Location = new System.Drawing.Point(7, 40);
            this.rdbFormatArcSWATTxt.Name = "rdbFormatArcSWATTxt";
            this.rdbFormatArcSWATTxt.Size = new System.Drawing.Size(185, 16);
            this.rdbFormatArcSWATTxt.TabIndex = 0;
            this.rdbFormatArcSWATTxt.TabStop = true;
            this.rdbFormatArcSWATTxt.Text = "ArcSWAT 2012 ASCII (.txt)  ";
            this.rdbFormatArcSWATTxt.UseVisualStyleBackColor = true;
            // 
            // rdbFormatArcSWATDbf
            // 
            this.rdbFormatArcSWATDbf.AutoSize = true;
            this.rdbFormatArcSWATDbf.Location = new System.Drawing.Point(7, 18);
            this.rdbFormatArcSWATDbf.Name = "rdbFormatArcSWATDbf";
            this.rdbFormatArcSWATDbf.Size = new System.Drawing.Size(185, 16);
            this.rdbFormatArcSWATDbf.TabIndex = 0;
            this.rdbFormatArcSWATDbf.TabStop = true;
            this.rdbFormatArcSWATDbf.Text = "ArcSWAT 2009 dBase (.dbf)  ";
            this.rdbFormatArcSWATDbf.UseVisualStyleBackColor = true;
            // 
            // bSelectAll
            // 
            this.bSelectAll.Location = new System.Drawing.Point(16, 174);
            this.bSelectAll.Name = "bSelectAll";
            this.bSelectAll.Size = new System.Drawing.Size(191, 21);
            this.bSelectAll.TabIndex = 4;
            this.bSelectAll.Text = "Select All";
            this.bSelectAll.UseVisualStyleBackColor = true;
            // 
            // dlgLoadSaveStations
            // 
            this.dlgLoadSaveStations.DefaultExt = "csv";
            this.dlgLoadSaveStations.Filter = "Station Information CSV files|*.csv";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(12, 282);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(744, 21);
            this.progressBar1.TabIndex = 12;
            // 
            // lblLatestVersion
            // 
            this.lblLatestVersion.AutoSize = true;
            this.lblLatestVersion.Location = new System.Drawing.Point(306, 246);
            this.lblLatestVersion.Name = "lblLatestVersion";
            this.lblLatestVersion.Size = new System.Drawing.Size(167, 12);
            this.lblLatestVersion.TabIndex = 13;
            this.lblLatestVersion.TabStop = true;
            this.lblLatestVersion.Text = "Get Lastest Porgram Package";
            // 
            // lblFeedback
            // 
            this.lblFeedback.AutoSize = true;
            this.lblFeedback.Location = new System.Drawing.Point(480, 246);
            this.lblFeedback.Name = "lblFeedback";
            this.lblFeedback.Size = new System.Drawing.Size(83, 12);
            this.lblFeedback.TabIndex = 14;
            this.lblFeedback.TabStop = true;
            this.lblFeedback.Text = "Send Feedback";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.rdbTimeIntervalMonthly);
            this.groupBox5.Controls.Add(this.rdbTimeIntervalDaily);
            this.groupBox5.Controls.Add(this.rdbTimeIntervalHourly);
            this.groupBox5.Location = new System.Drawing.Point(308, 14);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(219, 48);
            this.groupBox5.TabIndex = 5;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Time Interval";
            // 
            // rdbTimeIntervalMonthly
            // 
            this.rdbTimeIntervalMonthly.AutoSize = true;
            this.rdbTimeIntervalMonthly.Location = new System.Drawing.Point(148, 21);
            this.rdbTimeIntervalMonthly.Name = "rdbTimeIntervalMonthly";
            this.rdbTimeIntervalMonthly.Size = new System.Drawing.Size(65, 16);
            this.rdbTimeIntervalMonthly.TabIndex = 0;
            this.rdbTimeIntervalMonthly.TabStop = true;
            this.rdbTimeIntervalMonthly.Text = "Monthly";
            this.rdbTimeIntervalMonthly.UseVisualStyleBackColor = true;
            // 
            // rdbTimeIntervalDaily
            // 
            this.rdbTimeIntervalDaily.AutoSize = true;
            this.rdbTimeIntervalDaily.Location = new System.Drawing.Point(73, 20);
            this.rdbTimeIntervalDaily.Name = "rdbTimeIntervalDaily";
            this.rdbTimeIntervalDaily.Size = new System.Drawing.Size(53, 16);
            this.rdbTimeIntervalDaily.TabIndex = 0;
            this.rdbTimeIntervalDaily.TabStop = true;
            this.rdbTimeIntervalDaily.Text = "Daily";
            this.rdbTimeIntervalDaily.UseVisualStyleBackColor = true;
            // 
            // rdbTimeIntervalHourly
            // 
            this.rdbTimeIntervalHourly.AutoSize = true;
            this.rdbTimeIntervalHourly.Location = new System.Drawing.Point(7, 21);
            this.rdbTimeIntervalHourly.Name = "rdbTimeIntervalHourly";
            this.rdbTimeIntervalHourly.Size = new System.Drawing.Size(59, 16);
            this.rdbTimeIntervalHourly.TabIndex = 0;
            this.rdbTimeIntervalHourly.TabStop = true;
            this.rdbTimeIntervalHourly.Text = "Hourly";
            this.rdbTimeIntervalHourly.UseVisualStyleBackColor = true;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.listFields);
            this.groupBox6.Controls.Add(this.bSelectAll);
            this.groupBox6.Location = new System.Drawing.Point(533, 14);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(223, 206);
            this.groupBox6.TabIndex = 15;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Column";
            // 
            // bGetAllStations
            // 
            this.bGetAllStations.Location = new System.Drawing.Point(411, 221);
            this.bGetAllStations.Name = "bGetAllStations";
            this.bGetAllStations.Size = new System.Drawing.Size(116, 21);
            this.bGetAllStations.TabIndex = 0;
            this.bGetAllStations.Text = "GetAllStations";
            this.bGetAllStations.UseVisualStyleBackColor = true;
            // 
            // FrmDownloadEnvrionmentCanadaClimateData
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(770, 558);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.lblFeedback);
            this.Controls.Add(this.lblLatestVersion);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.bGetAllStations);
            this.Controls.Add(this.bDownload);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.richTextBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmDownloadEnvrionmentCanadaClimateData";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Environment Canada Climate Data Reader";
            this.Load += new System.EventHandler(this.FrmDownloadEnvrionmentCanadaClimateData_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bDownload;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.CheckedListBox listFields;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.Button bBrowseOutput;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtStartYear;
        private System.Windows.Forms.TextBox txtEndYear;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.RadioButton rdbFormatArcSWATTxt;
        private System.Windows.Forms.RadioButton rdbFormatArcSWATDbf;
        private System.Windows.Forms.RadioButton rdbFormatFreeText;
        private System.Windows.Forms.RadioButton rdbFormatSWATInput;
        private System.Windows.Forms.RadioButton rdbFormatFreeCSV;
        private System.Windows.Forms.OpenFileDialog dlgLoadSaveStations;
        private System.Windows.Forms.Button bOpen;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button bSelectAll;
        private System.Windows.Forms.LinkLabel lblLatestVersion;
        private System.Windows.Forms.LinkLabel lblFeedback;
        private System.Windows.Forms.LinkLabel lblStationLocation;
        private System.Windows.Forms.Button bDefineStations;
        private System.Windows.Forms.Label lblSelectedStations;
        private System.Windows.Forms.Button bLoadSavedStations;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.RadioButton rdbTimeIntervalMonthly;
        private System.Windows.Forms.RadioButton rdbTimeIntervalDaily;
        private System.Windows.Forms.RadioButton rdbTimeIntervalHourly;
        private System.Windows.Forms.Button bGetAllStations;
    }
}