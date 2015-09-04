namespace HAWKLORRY.HuzelnutSuitability
{
    partial class FrmHuzelnutSuitability
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
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.nudEndYear = new System.Windows.Forms.NumericUpDown();
            this.nudStartYear = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.bOpen = new System.Windows.Forms.Button();
            this.bBrowseOutput = new System.Windows.Forms.Button();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudEndYear)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudStartYear)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.nudEndYear);
            this.groupBox2.Controls.Add(this.nudStartYear);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(188, 75);
            this.groupBox2.TabIndex = 10;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Time";
            // 
            // nudEndYear
            // 
            this.nudEndYear.Location = new System.Drawing.Point(90, 48);
            this.nudEndYear.Maximum = new decimal(new int[] {
            2015,
            0,
            0,
            0});
            this.nudEndYear.Minimum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.nudEndYear.Name = "nudEndYear";
            this.nudEndYear.Size = new System.Drawing.Size(81, 21);
            this.nudEndYear.TabIndex = 12;
            this.nudEndYear.Value = new decimal(new int[] {
            2014,
            0,
            0,
            0});
            // 
            // nudStartYear
            // 
            this.nudStartYear.Location = new System.Drawing.Point(90, 20);
            this.nudStartYear.Maximum = new decimal(new int[] {
            2015,
            0,
            0,
            0});
            this.nudStartYear.Minimum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.nudStartYear.Name = "nudStartYear";
            this.nudStartYear.Size = new System.Drawing.Size(81, 21);
            this.nudStartYear.TabIndex = 11;
            this.nudStartYear.Value = new decimal(new int[] {
            2010,
            0,
            0,
            0});
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
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(19, 48);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 4;
            this.label4.Text = "End Year";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(500, 48);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(91, 39);
            this.button1.TabIndex = 11;
            this.button1.Text = "Download";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.bOpen);
            this.groupBox3.Controls.Add(this.bBrowseOutput);
            this.groupBox3.Controls.Add(this.txtPath);
            this.groupBox3.Location = new System.Drawing.Point(206, 13);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(288, 75);
            this.groupBox3.TabIndex = 12;
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
            // bBrowseOutput
            // 
            this.bBrowseOutput.Location = new System.Drawing.Point(179, 42);
            this.bBrowseOutput.Name = "bBrowseOutput";
            this.bBrowseOutput.Size = new System.Drawing.Size(94, 21);
            this.bBrowseOutput.TabIndex = 6;
            this.bBrowseOutput.Text = "Change ...";
            this.bBrowseOutput.UseVisualStyleBackColor = true;
            // 
            // txtPath
            // 
            this.txtPath.Location = new System.Drawing.Point(22, 18);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(251, 21);
            this.txtPath.TabIndex = 5;
            // 
            // groupBox1
            // 
            this.groupBox1.Location = new System.Drawing.Point(13, 94);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(581, 269);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Map";
            // 
            // FrmHuzelnutSuitability
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(734, 375);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox2);
            this.Name = "FrmHuzelnutSuitability";
            this.Text = "FrmHuzelnutSuitability";
            this.Load += new System.EventHandler(this.FrmHuzelnutSuitability_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudEndYear)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudStartYear)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.NumericUpDown nudEndYear;
        private System.Windows.Forms.NumericUpDown nudStartYear;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button bOpen;
        private System.Windows.Forms.Button bBrowseOutput;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}