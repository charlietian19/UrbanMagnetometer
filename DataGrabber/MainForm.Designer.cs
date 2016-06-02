namespace DataGrabber
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea10 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea11 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea12 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend4 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series10 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series11 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series12 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.stationNameLabel = new System.Windows.Forms.Label();
            this.sensorList = new System.Windows.Forms.ComboBox();
            this.recordButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.sensorLabel = new System.Windows.Forms.Label();
            this.refreshButton = new System.Windows.Forms.Button();
            this.stationName = new System.Windows.Forms.TextBox();
            this.dataGraph = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.uploadButton = new System.Windows.Forms.Button();
            this.gpsLabel = new System.Windows.Forms.Label();
            this.gpsList = new System.Windows.Forms.ComboBox();
            this.gpsRefreshButton = new System.Windows.Forms.Button();
            this.gpsOpenButton = new System.Windows.Forms.Button();
            this.gpsStatusLabel = new System.Windows.Forms.Label();
            this.gpsCloseButton = new System.Windows.Forms.Button();
            this.previewBox = new System.Windows.Forms.GroupBox();
            this.powerLineFilter = new System.Windows.Forms.CheckBox();
            this.displayPoints = new System.Windows.Forms.NumericUpDown();
            this.averagingPeriodLabel = new System.Windows.Forms.Label();
            this.maxPointsLabel = new System.Windows.Forms.Label();
            this.averagingPeriodMs = new System.Windows.Forms.NumericUpDown();
            this.plotUpdateTimer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dataGraph)).BeginInit();
            this.statusStrip.SuspendLayout();
            this.previewBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.displayPoints)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.averagingPeriodMs)).BeginInit();
            this.SuspendLayout();
            // 
            // stationNameLabel
            // 
            this.stationNameLabel.AutoSize = true;
            this.stationNameLabel.Location = new System.Drawing.Point(30, 23);
            this.stationNameLabel.Name = "stationNameLabel";
            this.stationNameLabel.Size = new System.Drawing.Size(90, 32);
            this.stationNameLabel.TabIndex = 0;
            this.stationNameLabel.Text = "Name";
            // 
            // sensorList
            // 
            this.sensorList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.sensorList.FormattingEnabled = true;
            this.sensorList.ItemHeight = 31;
            this.sensorList.Location = new System.Drawing.Point(569, 15);
            this.sensorList.Name = "sensorList";
            this.sensorList.Size = new System.Drawing.Size(121, 39);
            this.sensorList.TabIndex = 1;
            this.sensorList.SelectedIndexChanged += new System.EventHandler(this.sensorList_SelectedIndexChanged);
            // 
            // recordButton
            // 
            this.recordButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DimGray;
            this.recordButton.Location = new System.Drawing.Point(858, 12);
            this.recordButton.Name = "recordButton";
            this.recordButton.Size = new System.Drawing.Size(140, 53);
            this.recordButton.TabIndex = 2;
            this.recordButton.Text = "Record";
            this.recordButton.UseVisualStyleBackColor = true;
            this.recordButton.Click += new System.EventHandler(this.recordButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(1005, 12);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(140, 53);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // sensorLabel
            // 
            this.sensorLabel.AutoSize = true;
            this.sensorLabel.Location = new System.Drawing.Point(447, 23);
            this.sensorLabel.Name = "sensorLabel";
            this.sensorLabel.Size = new System.Drawing.Size(105, 32);
            this.sensorLabel.TabIndex = 4;
            this.sensorLabel.Text = "Sensor";
            // 
            // refreshButton
            // 
            this.refreshButton.Location = new System.Drawing.Point(711, 12);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(140, 53);
            this.refreshButton.TabIndex = 5;
            this.refreshButton.Text = "Refresh";
            this.refreshButton.UseVisualStyleBackColor = true;
            this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
            // 
            // stationName
            // 
            this.stationName.Location = new System.Drawing.Point(130, 20);
            this.stationName.Name = "stationName";
            this.stationName.Size = new System.Drawing.Size(302, 38);
            this.stationName.TabIndex = 6;
            this.stationName.TextChanged += new System.EventHandler(this.stationName_TextChanged);
            // 
            // dataGraph
            // 
            this.dataGraph.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea10.AxisY.IsStartedFromZero = false;
            chartArea10.Name = "X";
            chartArea11.AxisY.IsStartedFromZero = false;
            chartArea11.Name = "Y";
            chartArea12.AxisY.IsStartedFromZero = false;
            chartArea12.Name = "Z";
            this.dataGraph.ChartAreas.Add(chartArea10);
            this.dataGraph.ChartAreas.Add(chartArea11);
            this.dataGraph.ChartAreas.Add(chartArea12);
            legend4.Name = "Legend1";
            this.dataGraph.Legends.Add(legend4);
            this.dataGraph.Location = new System.Drawing.Point(28, 91);
            this.dataGraph.Name = "dataGraph";
            series10.ChartArea = "X";
            series10.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series10.Legend = "Legend1";
            series10.Name = "X";
            series11.ChartArea = "Y";
            series11.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series11.Legend = "Legend1";
            series11.Name = "Y";
            series12.ChartArea = "Z";
            series12.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series12.Legend = "Legend1";
            series12.Name = "Z";
            this.dataGraph.Series.Add(series10);
            this.dataGraph.Series.Add(series11);
            this.dataGraph.Series.Add(series12);
            this.dataGraph.Size = new System.Drawing.Size(1243, 629);
            this.dataGraph.TabIndex = 7;
            this.dataGraph.Text = "Magnetic Data Sample";
            // 
            // statusStrip
            // 
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(40, 40);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 861);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1309, 46);
            this.statusStrip.TabIndex = 8;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(99, 41);
            this.toolStripStatusLabel.Text = "Ready";
            // 
            // uploadButton
            // 
            this.uploadButton.Location = new System.Drawing.Point(1152, 12);
            this.uploadButton.Name = "uploadButton";
            this.uploadButton.Size = new System.Drawing.Size(140, 53);
            this.uploadButton.TabIndex = 9;
            this.uploadButton.Text = "Upload";
            this.uploadButton.UseVisualStyleBackColor = true;
            this.uploadButton.Click += new System.EventHandler(this.uploadButton_Click);
            // 
            // gpsLabel
            // 
            this.gpsLabel.AutoSize = true;
            this.gpsLabel.Location = new System.Drawing.Point(31, 87);
            this.gpsLabel.Name = "gpsLabel";
            this.gpsLabel.Size = new System.Drawing.Size(75, 32);
            this.gpsLabel.TabIndex = 10;
            this.gpsLabel.Text = "GPS";
            // 
            // gpsList
            // 
            this.gpsList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.gpsList.FormattingEnabled = true;
            this.gpsList.ItemHeight = 31;
            this.gpsList.Location = new System.Drawing.Point(130, 80);
            this.gpsList.Name = "gpsList";
            this.gpsList.Size = new System.Drawing.Size(121, 39);
            this.gpsList.TabIndex = 11;
            // 
            // gpsRefreshButton
            // 
            this.gpsRefreshButton.Location = new System.Drawing.Point(269, 72);
            this.gpsRefreshButton.Name = "gpsRefreshButton";
            this.gpsRefreshButton.Size = new System.Drawing.Size(140, 53);
            this.gpsRefreshButton.TabIndex = 12;
            this.gpsRefreshButton.Text = "Refresh";
            this.gpsRefreshButton.UseVisualStyleBackColor = true;
            this.gpsRefreshButton.Click += new System.EventHandler(this.gpsRefreshButton_Click);
            // 
            // gpsOpenButton
            // 
            this.gpsOpenButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DimGray;
            this.gpsOpenButton.Location = new System.Drawing.Point(415, 72);
            this.gpsOpenButton.Name = "gpsOpenButton";
            this.gpsOpenButton.Size = new System.Drawing.Size(140, 53);
            this.gpsOpenButton.TabIndex = 13;
            this.gpsOpenButton.Text = "Open";
            this.gpsOpenButton.UseVisualStyleBackColor = true;
            this.gpsOpenButton.Click += new System.EventHandler(this.gpsOpenButton_Click);
            // 
            // gpsStatusLabel
            // 
            this.gpsStatusLabel.AutoSize = true;
            this.gpsStatusLabel.Location = new System.Drawing.Point(705, 83);
            this.gpsStatusLabel.Name = "gpsStatusLabel";
            this.gpsStatusLabel.Size = new System.Drawing.Size(188, 32);
            this.gpsStatusLabel.TabIndex = 14;
            this.gpsStatusLabel.Text = "Disconnected";
            // 
            // gpsCloseButton
            // 
            this.gpsCloseButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DimGray;
            this.gpsCloseButton.Location = new System.Drawing.Point(561, 72);
            this.gpsCloseButton.Name = "gpsCloseButton";
            this.gpsCloseButton.Size = new System.Drawing.Size(140, 53);
            this.gpsCloseButton.TabIndex = 15;
            this.gpsCloseButton.Text = "Close";
            this.gpsCloseButton.UseVisualStyleBackColor = true;
            this.gpsCloseButton.Click += new System.EventHandler(this.gpsCloseButton_Click);
            // 
            // previewBox
            // 
            this.previewBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.previewBox.Controls.Add(this.powerLineFilter);
            this.previewBox.Controls.Add(this.displayPoints);
            this.previewBox.Controls.Add(this.averagingPeriodLabel);
            this.previewBox.Controls.Add(this.maxPointsLabel);
            this.previewBox.Controls.Add(this.averagingPeriodMs);
            this.previewBox.Controls.Add(this.dataGraph);
            this.previewBox.Location = new System.Drawing.Point(7, 126);
            this.previewBox.Name = "previewBox";
            this.previewBox.Size = new System.Drawing.Size(1290, 720);
            this.previewBox.TabIndex = 17;
            this.previewBox.TabStop = false;
            this.previewBox.Text = "Sample preview";
            // 
            // powerLineFilter
            // 
            this.powerLineFilter.AutoSize = true;
            this.powerLineFilter.Enabled = false;
            this.powerLineFilter.Location = new System.Drawing.Point(865, 46);
            this.powerLineFilter.Name = "powerLineFilter";
            this.powerLineFilter.Size = new System.Drawing.Size(254, 36);
            this.powerLineFilter.TabIndex = 16;
            this.powerLineFilter.Text = "Filter power line";
            this.powerLineFilter.UseVisualStyleBackColor = true;
            // 
            // displayPoints
            // 
            this.displayPoints.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.displayPoints.Location = new System.Drawing.Point(687, 46);
            this.displayPoints.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.displayPoints.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.displayPoints.Name = "displayPoints";
            this.displayPoints.Size = new System.Drawing.Size(155, 38);
            this.displayPoints.TabIndex = 15;
            this.displayPoints.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
            // 
            // averagingPeriodLabel
            // 
            this.averagingPeriodLabel.AutoSize = true;
            this.averagingPeriodLabel.Location = new System.Drawing.Point(22, 46);
            this.averagingPeriodLabel.Name = "averagingPeriodLabel";
            this.averagingPeriodLabel.Size = new System.Drawing.Size(291, 32);
            this.averagingPeriodLabel.TabIndex = 12;
            this.averagingPeriodLabel.Text = "Averaging period [ms]";
            // 
            // maxPointsLabel
            // 
            this.maxPointsLabel.AutoSize = true;
            this.maxPointsLabel.Location = new System.Drawing.Point(488, 46);
            this.maxPointsLabel.Name = "maxPointsLabel";
            this.maxPointsLabel.Size = new System.Drawing.Size(193, 32);
            this.maxPointsLabel.TabIndex = 14;
            this.maxPointsLabel.Text = "Display points";
            // 
            // averagingPeriodMs
            // 
            this.averagingPeriodMs.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.averagingPeriodMs.Location = new System.Drawing.Point(319, 46);
            this.averagingPeriodMs.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.averagingPeriodMs.Minimum = new decimal(new int[] {
            25,
            0,
            0,
            131072});
            this.averagingPeriodMs.Name = "averagingPeriodMs";
            this.averagingPeriodMs.Size = new System.Drawing.Size(145, 38);
            this.averagingPeriodMs.TabIndex = 13;
            this.averagingPeriodMs.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // plotUpdateTimer
            // 
            this.plotUpdateTimer.Tick += new System.EventHandler(this.plotUpdateTimer_Tick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1309, 907);
            this.Controls.Add(this.previewBox);
            this.Controls.Add(this.gpsCloseButton);
            this.Controls.Add(this.gpsStatusLabel);
            this.Controls.Add(this.gpsOpenButton);
            this.Controls.Add(this.gpsRefreshButton);
            this.Controls.Add(this.gpsList);
            this.Controls.Add(this.gpsLabel);
            this.Controls.Add(this.uploadButton);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.stationName);
            this.Controls.Add(this.refreshButton);
            this.Controls.Add(this.sensorLabel);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.recordButton);
            this.Controls.Add(this.sensorList);
            this.Controls.Add(this.stationNameLabel);
            this.Name = "MainForm";
            this.Text = "Magnetic Data Recorder";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.dataGraph)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.previewBox.ResumeLayout(false);
            this.previewBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.displayPoints)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.averagingPeriodMs)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label stationNameLabel;
        private System.Windows.Forms.ComboBox sensorList;
        private System.Windows.Forms.Button recordButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label sensorLabel;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.TextBox stationName;
        private System.Windows.Forms.DataVisualization.Charting.Chart dataGraph;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.Button uploadButton;
        private System.Windows.Forms.Label gpsLabel;
        private System.Windows.Forms.ComboBox gpsList;
        private System.Windows.Forms.Button gpsRefreshButton;
        private System.Windows.Forms.Button gpsOpenButton;
        private System.Windows.Forms.Label gpsStatusLabel;
        private System.Windows.Forms.Button gpsCloseButton;
        private System.Windows.Forms.GroupBox previewBox;
        private System.Windows.Forms.CheckBox powerLineFilter;
        private System.Windows.Forms.NumericUpDown displayPoints;
        private System.Windows.Forms.Label averagingPeriodLabel;
        private System.Windows.Forms.Label maxPointsLabel;
        private System.Windows.Forms.NumericUpDown averagingPeriodMs;
        private System.Windows.Forms.Timer plotUpdateTimer;
    }
}

