using System;
using System.Windows.Forms;
using Utils.GDrive;
using Utils.DataManager;
using Utils.Configuration;
using Utils.Filters;
using Biomed_eMains_eFMx;
using System.IO;

namespace SampleGrabber
{
    public partial class MainForm : Form
    {
        private LegacyStorage storage;
        private eMains sensor;
        private double samplingRate;
        private Range range = Range.NEG_10_TO_PLUS_10V;
        private bool convertToMicroTesla;
        private DateTime lastUpdated = DateTime.Now;
        private bool doneUploading = true;

        private IFilter[] averages = new MovingAverage[3];
        private IFilter[] subsamples = new Subsample[3];
        private RollingBuffer[] buffers = new RollingBuffer[3];

        enum UI_State
        {
            NoSensorFound,
            Ready,
            Recording
        }

        public MainForm()
        {
            InitializeComponent();
            InitializeResources();
            RefreshSensorList();
        }

        private void InitializeResources()
        {
            try
            {
                eMains.LoadDLL();
                sampleName.Text = Settings.SampleName;
                samplingRate = Convert.ToDouble(Settings.SamplingRate);
                var units = Settings.DataUnits;
                convertToMicroTesla = (units == "uT");
                var google = new GDrive("nuri-station.json");
                google.ProgressEvent += Google_ProgressEvent;
                var scheduler = new UploadScheduler(google);
                scheduler.StartedEvent += Scheduler_StartedEvent;
                scheduler.FinishedEvent += Scheduler_FinishedEvent;
                storage = new LegacyStorage(scheduler);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Can't initialize: " + exception.Message);
                Environment.Exit(-1);
            }
        }

        /* Updates the data filters */
        private void UpdateFilters()
        {
            int avgPoints = Convert.ToInt32(Convert.ToInt32(samplingRate)
                    * averagingPeriodMs.Value / 1000);
            InitializeFilters(avgPoints,
                Convert.ToInt32(displayPoints.Value));
        }

        /* Initializes a chain of data filters for X, Y, Z channels. */
        private void InitializeFilters(int averagePoints, int bufferSize)
        {
            for (int i = 0; i < 3; i++)
            {
                InitializeFilter(averagePoints, bufferSize, i);
            }
        }

        /* Initializes a chain of data filters for a single channel. */
        private void InitializeFilter(int averagePoints, int bufferSize, int i)
        {
            averagePoints = Math.Max(1, averagePoints);
            averages[i] = new MovingAverage(averagePoints);
            subsamples[i] = new Subsample(averagePoints);
            buffers[i] = new RollingBuffer(bufferSize, double.NaN);
            averages[i].output += new FilterEvent(subsamples[i].InputData);
            subsamples[i].output += new FilterEvent(buffers[i].InputData);
        }

        private void Google_ProgressEvent(string fullPath, long bytesSent,
            long bytesTotal)
        {
            double progress = 100 * bytesSent / bytesTotal;
            var name = Path.GetFileName(fullPath);
            var msg = string.Format("Uploading {0}, {1}% done", name, 
                progress);
            SetTextThreadSafe(msg);
        }

        delegate void SetTextCallback(string text);
        private void SetTextThreadSafe(string text)
        {
            if (InvokeRequired)
            {
                SetTextCallback f = new SetTextCallback(SetTextThreadSafe);
                Invoke(f, new object[] { text });
            }
            else
            {
                toolStripStatusLabel.Text = text;
            }
        }

        private void Scheduler_FinishedEvent(IDatasetInfo info, bool success,
            string message)
        {
            SetTextThreadSafe(message);
            doneUploading = true;
        }

        private void Scheduler_StartedEvent(IDatasetInfo info)
        {
            var msg = string.Format("Uploading dataset from {0}", 
                info.StartDate);
            SetTextThreadSafe(msg);
            doneUploading = false;
        }

        private void stationName_TextChanged(object sender, EventArgs e)
        {
            Settings.SampleName = sampleName.Text;
        }

        void SetUI(UI_State state)
        {
            switch (state)
            {
                case UI_State.Ready:
                    sampleName.Enabled = true;
                    sensorList.Enabled = true;
                    averagingPeriodMs.Enabled = true;
                    powerLineFilter.Enabled = false;
                    comment.Enabled = true;
                    plotUpdateTimer.Enabled = false;
                    displayPoints.Enabled = true;
                    refreshButton.Enabled = true;
                    recordButton.Enabled = true;
                    cancelButton.Enabled = false;
                    uploadButton.Enabled = true;
                    return;
                case UI_State.Recording:
                    sampleName.Enabled = false;
                    sensorList.Enabled = false;
                    averagingPeriodMs.Enabled = false;
                    powerLineFilter.Enabled = false;
                    comment.Enabled = false;
                    plotUpdateTimer.Enabled = true;
                    displayPoints.Enabled = false;
                    refreshButton.Enabled = false;
                    recordButton.Enabled = false;
                    cancelButton.Enabled = true;
                    uploadButton.Enabled = false;
                    return;
                case UI_State.NoSensorFound:
                    sampleName.Enabled = true;
                    sensorList.Enabled = true;
                    averagingPeriodMs.Enabled = true;
                    powerLineFilter.Enabled = false;
                    comment.Enabled = true;
                    plotUpdateTimer.Enabled = false;
                    displayPoints.Enabled = true;
                    refreshButton.Enabled = true;
                    recordButton.Enabled = false;
                    cancelButton.Enabled = false;
                    uploadButton.Enabled = true;
                    return;
            }
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            RefreshSensorList();
        }

        void RefreshSensorList()
        {
            try
            {
                sensorList.Items.Clear();
                var sensors = eMains.GetAvailableSerials();
                sensorList.Items.Clear();
                foreach (var item in sensors)
                {
                    sensorList.Items.Add(item);
                }
                
                if (sensors.Count > 0)
                {
                    sensorList.Text = sensors[0].ToString();
                    SelectSensor(sensors[0]);
                }
            }
            catch (eMainsException exception)
            {
                toolStripStatusLabel.Text = exception.Message;
                SetUI(UI_State.NoSensorFound);
            }
        }

        private void SelectSensor(int serial)
        {
            try
            {
                sensor = new eMains(serial);
                SetUI(UI_State.Ready);
            }
            catch (eMainsException exception)
            {
                toolStripStatusLabel.Text = exception.Message;
                SetUI(UI_State.NoSensorFound);
            }
        }

        private void sensorList_SelectedIndexChanged(object sender, 
            EventArgs e)
        {
            var serial = Convert.ToInt32(sensorList.Text);
            SelectSensor(serial);
        }

        private void recordButton_Click(object sender, EventArgs e)
        {
            if (sensor == null)
            {
                SetUI(UI_State.NoSensorFound);
                toolStripStatusLabel.Text 
                    = "Magnetic sensor is not initialized.";
            }

            try
            {
                sensor.DAQInitialize(samplingRate, range, 1, 1);
                sensor.NewDataHandler -= Sensor_NewDataHandler;
                sensor.NewDataHandler += Sensor_NewDataHandler;
                UpdateFilters();
                sensor.DAQStart(convertToMicroTesla);
                doneUploading = false;
                SetUI(UI_State.Recording);
            }
            catch (Exception exception)
            {
                toolStripStatusLabel.Text = exception.Message;
                SetUI(UI_State.Ready);
            }
        }

        /* Triggers the graph update. */
        private void plotUpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdatePlot("X", buffers[0].GetData());
            UpdatePlot("Y", buffers[1].GetData());
            UpdatePlot("Z", buffers[2].GetData());
        }

        /* Updates the given series in the graph. */
        private void UpdatePlot(string series, double[] data)
        {
            var Points = dataGraph.Series.FindByName(series).Points;
            Points.Clear();
            int i = 0;
            double dx = Convert.ToDouble(averagingPeriodMs.Value) / 1000;
            foreach (var point in data)
            {
                if (!double.IsNaN(point))
                {
                    Points.AddXY(i * dx, point);
                    i++;
                }
            }
        }

        private void Sensor_NewDataHandler(double[] dataX, double[] dataY,
            double[] dataZ, double systemSeconds, DateTime time)
        {
            storage.Store(dataX, dataY, dataZ, systemSeconds, time);
            averages[0].InputData(dataX);
            averages[1].InputData(dataY);
            averages[2].InputData(dataZ);
        }        

        private void cancelButton_Click(object sender, EventArgs e)
        {
            if (sensor == null)
            {
                SetUI(UI_State.NoSensorFound);
                toolStripStatusLabel.Text 
                    = "Magnetic sensor is not initialized.";
            }

            try
            {
                sensor.DAQStop();
                sensor.NewDataHandler -= Sensor_NewDataHandler;
                SetUI(UI_State.Ready);
            }
            catch (Exception exception)
            {
                toolStripStatusLabel.Text = exception.Message;
                SetUI(UI_State.Ready);
            }
        }

        private void uploadButton_Click(object sender, EventArgs e)
        {
            storage.Close();
            doneUploading = false;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sensor != null)
            {
                sensor.DAQStop();
                SetUI(UI_State.Ready);
            }

            //if (storage.HasCachedData)
            //{
            //    storage.Close();
            //    doneUploading = false;
            //}

            //if (!doneUploading)
            //{
            //    e.Cancel = true;
            //    toolStripStatusLabel.Text
            //        = "Please wait until the files are uploaded.";
            //}
        }

        private void averagingPeriodMs_ValueChanged(object sender, EventArgs e)
        {
            UpdateFilters();
        }

        private void displayPoints_ValueChanged(object sender, EventArgs e)
        {
            UpdateFilters();
        }

        private void dataGraph_Click(object sender, EventArgs e)
        {

        }
    }
}
