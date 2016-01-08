namespace DogeStation2
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea7 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea8 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea9 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend3 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series7 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series8 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series9 = new System.Windows.Forms.DataVisualization.Charting.Series();
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
            ((System.ComponentModel.ISupportInitialize)(this.dataGraph)).BeginInit();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // stationNameLabel
            // 
            this.stationNameLabel.AutoSize = true;
            this.stationNameLabel.Location = new System.Drawing.Point(30, 23);
            this.stationNameLabel.Name = "stationNameLabel";
            this.stationNameLabel.Size = new System.Drawing.Size(183, 32);
            this.stationNameLabel.TabIndex = 0;
            this.stationNameLabel.Text = "Station name";
            // 
            // sensorList
            // 
            this.sensorList.FormattingEnabled = true;
            this.sensorList.ItemHeight = 31;
            this.sensorList.Location = new System.Drawing.Point(821, 15);
            this.sensorList.Name = "sensorList";
            this.sensorList.Size = new System.Drawing.Size(121, 39);
            this.sensorList.TabIndex = 1;
            this.sensorList.SelectedIndexChanged += new System.EventHandler(this.sensorList_SelectedIndexChanged);
            // 
            // recordButton
            // 
            this.recordButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DimGray;
            this.recordButton.Location = new System.Drawing.Point(1198, 13);
            this.recordButton.Name = "recordButton";
            this.recordButton.Size = new System.Drawing.Size(150, 53);
            this.recordButton.TabIndex = 2;
            this.recordButton.Text = "Record";
            this.recordButton.UseVisualStyleBackColor = true;
            this.recordButton.Click += new System.EventHandler(this.recordButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(1354, 13);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(150, 53);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // sensorLabel
            // 
            this.sensorLabel.AutoSize = true;
            this.sensorLabel.Location = new System.Drawing.Point(699, 23);
            this.sensorLabel.Name = "sensorLabel";
            this.sensorLabel.Size = new System.Drawing.Size(105, 32);
            this.sensorLabel.TabIndex = 4;
            this.sensorLabel.Text = "Sensor";
            // 
            // refreshButton
            // 
            this.refreshButton.Location = new System.Drawing.Point(948, 13);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(150, 53);
            this.refreshButton.TabIndex = 5;
            this.refreshButton.Text = "Refresh";
            this.refreshButton.UseVisualStyleBackColor = true;
            this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
            // 
            // stationName
            // 
            this.stationName.Location = new System.Drawing.Point(230, 20);
            this.stationName.Name = "stationName";
            this.stationName.Size = new System.Drawing.Size(418, 38);
            this.stationName.TabIndex = 6;
            this.stationName.TextChanged += new System.EventHandler(this.stationName_TextChanged);
            // 
            // dataGraph
            // 
            this.dataGraph.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea7.Name = "X";
            chartArea8.Name = "Y";
            chartArea9.Name = "Z";
            this.dataGraph.ChartAreas.Add(chartArea7);
            this.dataGraph.ChartAreas.Add(chartArea8);
            this.dataGraph.ChartAreas.Add(chartArea9);
            legend3.Name = "Legend1";
            this.dataGraph.Legends.Add(legend3);
            this.dataGraph.Location = new System.Drawing.Point(37, 90);
            this.dataGraph.Name = "dataGraph";
            series7.ChartArea = "X";
            series7.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series7.Legend = "Legend1";
            series7.Name = "X";
            series8.ChartArea = "Y";
            series8.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series8.Legend = "Legend1";
            series8.Name = "Y";
            series9.ChartArea = "Z";
            series9.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series9.Legend = "Legend1";
            series9.Name = "Z";
            this.dataGraph.Series.Add(series7);
            this.dataGraph.Series.Add(series8);
            this.dataGraph.Series.Add(series9);
            this.dataGraph.Size = new System.Drawing.Size(1466, 1213);
            this.dataGraph.TabIndex = 7;
            this.dataGraph.Text = "Magnetic Data Sample";
            // 
            // statusStrip
            // 
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(40, 40);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 1333);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1535, 46);
            this.statusStrip.TabIndex = 8;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(100, 41);
            this.toolStripStatusLabel.Text = "Ready";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1535, 1379);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.dataGraph);
            this.Controls.Add(this.stationName);
            this.Controls.Add(this.refreshButton);
            this.Controls.Add(this.sensorLabel);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.recordButton);
            this.Controls.Add(this.sensorList);
            this.Controls.Add(this.stationNameLabel);
            this.Name = "MainForm";
            this.Text = "Magnetic Data Recorder";
            ((System.ComponentModel.ISupportInitialize)(this.dataGraph)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
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
    }
}

