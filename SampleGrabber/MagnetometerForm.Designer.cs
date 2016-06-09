namespace SampleGrabber
{
    public partial class MagnetometerForm
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea3 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.sampleNameLabel = new System.Windows.Forms.Label();
            this.sensorList = new System.Windows.Forms.ComboBox();
            this.recordButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.sensorLabel = new System.Windows.Forms.Label();
            this.refreshButton = new System.Windows.Forms.Button();
            this.name = new System.Windows.Forms.TextBox();
            this.dataGraph = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.uploadButton = new System.Windows.Forms.Button();
            this.averagingPeriodLabel = new System.Windows.Forms.Label();
            this.averagingPeriodMs = new System.Windows.Forms.NumericUpDown();
            this.maxPointsLabel = new System.Windows.Forms.Label();
            this.displayPoints = new System.Windows.Forms.NumericUpDown();
            this.previewBox = new System.Windows.Forms.GroupBox();
            this.powerLineFilter = new System.Windows.Forms.CheckBox();
            this.plotUpdateTimer = new System.Windows.Forms.Timer(this.components);
            this.gpsCloseButton = new System.Windows.Forms.Button();
            this.gpsStatusLabel = new System.Windows.Forms.Label();
            this.gpsOpenButton = new System.Windows.Forms.Button();
            this.gpsRefreshButton = new System.Windows.Forms.Button();
            this.gpsList = new System.Windows.Forms.ComboBox();
            this.gpsLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGraph)).BeginInit();
            this.statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.averagingPeriodMs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.displayPoints)).BeginInit();
            this.previewBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // sampleNameLabel
            // 
            this.sampleNameLabel.AutoSize = true;
            this.sampleNameLabel.Location = new System.Drawing.Point(30, 23);
            this.sampleNameLabel.Name = "sampleNameLabel";
            this.sampleNameLabel.Size = new System.Drawing.Size(90, 32);
            this.sampleNameLabel.TabIndex = 0;
            this.sampleNameLabel.Text = "Name";
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
            this.recordButton.Location = new System.Drawing.Point(854, 12);
            this.recordButton.Name = "recordButton";
            this.recordButton.Size = new System.Drawing.Size(138, 53);
            this.recordButton.TabIndex = 2;
            this.recordButton.Text = "Record";
            this.recordButton.UseVisualStyleBackColor = true;
            this.recordButton.Click += new System.EventHandler(this.recordButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(998, 12);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(131, 53);
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
            this.refreshButton.Location = new System.Drawing.Point(696, 12);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(152, 53);
            this.refreshButton.TabIndex = 5;
            this.refreshButton.Text = "Refresh";
            this.refreshButton.UseVisualStyleBackColor = true;
            this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
            // 
            // name
            // 
            this.name.Location = new System.Drawing.Point(130, 20);
            this.name.Name = "name";
            this.name.Size = new System.Drawing.Size(302, 38);
            this.name.TabIndex = 6;
            // 
            // dataGraph
            // 
            this.dataGraph.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea1.AxisX.Minimum = 0D;
            chartArea1.AxisX.Title = "s";
            chartArea1.AxisY.IsStartedFromZero = false;
            chartArea1.Name = "X";
            chartArea2.AxisX.Minimum = 0D;
            chartArea2.AxisX.Title = "s";
            chartArea2.AxisY.IsStartedFromZero = false;
            chartArea2.Name = "Y";
            chartArea3.AxisX.Minimum = 0D;
            chartArea3.AxisX.Title = "s";
            chartArea3.AxisY.IsStartedFromZero = false;
            chartArea3.Name = "Z";
            this.dataGraph.ChartAreas.Add(chartArea1);
            this.dataGraph.ChartAreas.Add(chartArea2);
            this.dataGraph.ChartAreas.Add(chartArea3);
            legend1.Name = "Legend1";
            this.dataGraph.Legends.Add(legend1);
            this.dataGraph.Location = new System.Drawing.Point(8, 105);
            this.dataGraph.Name = "dataGraph";
            series1.ChartArea = "X";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series1.Legend = "Legend1";
            series1.Name = "X";
            series2.ChartArea = "Y";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series2.Legend = "Legend1";
            series2.Name = "Y";
            series3.ChartArea = "Z";
            series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series3.Legend = "Legend1";
            series3.Name = "Z";
            this.dataGraph.Series.Add(series1);
            this.dataGraph.Series.Add(series2);
            this.dataGraph.Series.Add(series3);
            this.dataGraph.Size = new System.Drawing.Size(1369, 423);
            this.dataGraph.TabIndex = 7;
            this.dataGraph.Text = "Magnetic Data Sample";
            // 
            // statusStrip
            // 
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(40, 40);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 747);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1436, 46);
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
            this.uploadButton.Location = new System.Drawing.Point(1135, 12);
            this.uploadButton.Name = "uploadButton";
            this.uploadButton.Size = new System.Drawing.Size(140, 53);
            this.uploadButton.TabIndex = 9;
            this.uploadButton.Text = "Upload";
            this.uploadButton.UseVisualStyleBackColor = true;
            this.uploadButton.Click += new System.EventHandler(this.uploadButton_Click);
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
            this.averagingPeriodMs.ValueChanged += new System.EventHandler(this.averagingPeriodMs_ValueChanged);
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
            this.displayPoints.ValueChanged += new System.EventHandler(this.displayPoints_ValueChanged);
            // 
            // previewBox
            // 
            this.previewBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.previewBox.Controls.Add(this.powerLineFilter);
            this.previewBox.Controls.Add(this.dataGraph);
            this.previewBox.Controls.Add(this.displayPoints);
            this.previewBox.Controls.Add(this.averagingPeriodLabel);
            this.previewBox.Controls.Add(this.maxPointsLabel);
            this.previewBox.Controls.Add(this.averagingPeriodMs);
            this.previewBox.Location = new System.Drawing.Point(29, 186);
            this.previewBox.Name = "previewBox";
            this.previewBox.Size = new System.Drawing.Size(1392, 545);
            this.previewBox.TabIndex = 16;
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
            // plotUpdateTimer
            // 
            this.plotUpdateTimer.Tick += new System.EventHandler(this.plotUpdateTimer_Tick);
            // 
            // gpsCloseButton
            // 
            this.gpsCloseButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DimGray;
            this.gpsCloseButton.Location = new System.Drawing.Point(612, 125);
            this.gpsCloseButton.Name = "gpsCloseButton";
            this.gpsCloseButton.Size = new System.Drawing.Size(140, 53);
            this.gpsCloseButton.TabIndex = 23;
            this.gpsCloseButton.Text = "Close";
            this.gpsCloseButton.UseVisualStyleBackColor = true;
            this.gpsCloseButton.Click += new System.EventHandler(this.gpsCloseButton_Click);
            // 
            // gpsStatusLabel
            // 
            this.gpsStatusLabel.AutoSize = true;
            this.gpsStatusLabel.Location = new System.Drawing.Point(756, 136);
            this.gpsStatusLabel.Name = "gpsStatusLabel";
            this.gpsStatusLabel.Size = new System.Drawing.Size(188, 32);
            this.gpsStatusLabel.TabIndex = 22;
            this.gpsStatusLabel.Text = "Disconnected";
            // 
            // gpsOpenButton
            // 
            this.gpsOpenButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DimGray;
            this.gpsOpenButton.Location = new System.Drawing.Point(466, 125);
            this.gpsOpenButton.Name = "gpsOpenButton";
            this.gpsOpenButton.Size = new System.Drawing.Size(140, 53);
            this.gpsOpenButton.TabIndex = 21;
            this.gpsOpenButton.Text = "Open";
            this.gpsOpenButton.UseVisualStyleBackColor = true;
            this.gpsOpenButton.Click += new System.EventHandler(this.gpsOpenButton_Click);
            // 
            // gpsRefreshButton
            // 
            this.gpsRefreshButton.Location = new System.Drawing.Point(320, 125);
            this.gpsRefreshButton.Name = "gpsRefreshButton";
            this.gpsRefreshButton.Size = new System.Drawing.Size(140, 53);
            this.gpsRefreshButton.TabIndex = 20;
            this.gpsRefreshButton.Text = "Refresh";
            this.gpsRefreshButton.UseVisualStyleBackColor = true;
            this.gpsRefreshButton.Click += new System.EventHandler(this.gpsRefreshButton_Click);
            // 
            // gpsList
            // 
            this.gpsList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.gpsList.FormattingEnabled = true;
            this.gpsList.ItemHeight = 31;
            this.gpsList.Location = new System.Drawing.Point(130, 129);
            this.gpsList.Name = "gpsList";
            this.gpsList.Size = new System.Drawing.Size(169, 39);
            this.gpsList.TabIndex = 19;
            // 
            // gpsLabel
            // 
            this.gpsLabel.AutoSize = true;
            this.gpsLabel.Location = new System.Drawing.Point(31, 136);
            this.gpsLabel.Name = "gpsLabel";
            this.gpsLabel.Size = new System.Drawing.Size(75, 32);
            this.gpsLabel.TabIndex = 18;
            this.gpsLabel.Text = "GPS";
            // 
            // MagnetometerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1436, 793);
            this.Controls.Add(this.gpsCloseButton);
            this.Controls.Add(this.gpsStatusLabel);
            this.Controls.Add(this.gpsOpenButton);
            this.Controls.Add(this.gpsRefreshButton);
            this.Controls.Add(this.gpsList);
            this.Controls.Add(this.gpsLabel);
            this.Controls.Add(this.previewBox);
            this.Controls.Add(this.uploadButton);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.name);
            this.Controls.Add(this.refreshButton);
            this.Controls.Add(this.sensorLabel);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.recordButton);
            this.Controls.Add(this.sensorList);
            this.Controls.Add(this.sampleNameLabel);
            this.Name = "MagnetometerForm";
            this.Text = "Magnetic Sample Recorder";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MagnetometerForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGraph)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.averagingPeriodMs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.displayPoints)).EndInit();
            this.previewBox.ResumeLayout(false);
            this.previewBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected System.Windows.Forms.Label sampleNameLabel;
        protected System.Windows.Forms.ComboBox sensorList;
        protected System.Windows.Forms.Button recordButton;
        protected System.Windows.Forms.Button cancelButton;
        protected System.Windows.Forms.Label sensorLabel;
        protected System.Windows.Forms.Button refreshButton;
        protected System.Windows.Forms.TextBox name;
        protected System.Windows.Forms.DataVisualization.Charting.Chart dataGraph;
        protected System.Windows.Forms.StatusStrip statusStrip;
        protected System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        protected System.Windows.Forms.Button uploadButton;
        protected System.Windows.Forms.Label averagingPeriodLabel;
        protected System.Windows.Forms.NumericUpDown averagingPeriodMs;
        protected System.Windows.Forms.Label maxPointsLabel;
        protected System.Windows.Forms.NumericUpDown displayPoints;
        protected System.Windows.Forms.GroupBox previewBox;
        protected System.Windows.Forms.CheckBox powerLineFilter;
        protected System.Windows.Forms.Timer plotUpdateTimer;
        protected System.Windows.Forms.Button gpsCloseButton;
        protected System.Windows.Forms.Label gpsStatusLabel;
        protected System.Windows.Forms.Button gpsOpenButton;
        protected System.Windows.Forms.Button gpsRefreshButton;
        protected System.Windows.Forms.ComboBox gpsList;
        protected System.Windows.Forms.Label gpsLabel;
    }
}

