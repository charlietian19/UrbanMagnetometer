﻿using System;
using System.Windows.Forms;
using Utils.GDrive;
using Utils.DataManager;
using Utils.Configuration;
using Utils.Filters;
using Utils.GPS;
using Biomed_eMains_eFMx;
using System.IO;
using Utils.GPS.SerialGPS;
using Utils.GPS.Time;
using Utils.DataReader;

namespace SampleGrabber
{
    public partial class MagnetometerForm : Form
    {
        protected LegacyStorage storage;
        protected eMains sensor;
        protected double samplingRate;
        protected Range range = Range.NEG_10_TO_PLUS_10V;
        protected bool convertToMicroTesla;
        protected DateTime lastUpdated = DateTime.Now;

        protected IFilter[] averages = new MovingAverage[3];
        protected IFilter[] subsamples = new Subsample[3];
        protected RollingBuffer[] buffers = new RollingBuffer[3];
        protected LagSpikeFilter lagFilter;

        protected ITimeSource gps;
        protected ITimeEstimator interpolator = new NaiveTimeEstimator();

        protected IUploader google;
        protected IUploadScheduler scheduler;

        protected System.Timers.Timer gpsTimeoutTimer;


        /* Magnetometer UI status list */
        protected enum UiStateMagnetometer
        {
            NoSensorFound,
            Ready,
            Recording
        }

        /* Initialize form components */
        public MagnetometerForm()
        {
            InitializeComponent();
        }

        /* Initializes the objects that produce and handle the signals */
        protected virtual void InitializeResources()
        {
            try
            {
                eMains.LoadDLL();                              
                samplingRate = Convert.ToDouble(Settings.SamplingRate);
                var units = Settings.DataUnits;
                convertToMicroTesla = (units == "uT");
                google = new GDrive("nuri-station.json");
                google.ProgressEvent += Google_ProgressEvent;
                scheduler = new UploadScheduler(google);
                scheduler.StartedEvent += Scheduler_StartedEvent;
                scheduler.FinishedEvent += Scheduler_FinishedEvent;
                gpsTimeoutTimer = new System.Timers.Timer(5000);
                gpsTimeoutTimer.Elapsed += GpsTimeoutTimer_Elapsed;
                lagFilter = new LagSpikeFilter(Settings.LagFilterFitPoints);
                lagFilter.OnPop += LagFilter_OnPop;
                lagFilter.ToleranceHigh = Settings.LagFilterToleranceHigh;
                lagFilter.ToleranceLow = Settings.LagFilterToleranceLow;
                storage = new LegacySampleStorage(scheduler,
                    (start, now) => false);                
                RefreshGpsList();
            }
            catch (Exception exception)
            {
                MessageBox.Show("Can't initialize: " + exception.Message);
                Environment.Exit(-1);
            }
        }

        /* Resets the data display filters */
        protected void UpdateFilters()
        {
            int avgPoints = Convert.ToInt32(Convert.ToInt32(samplingRate)
                    * averagingPeriodMs.Value / 1000);
            InitializeFilters(avgPoints,
                Convert.ToInt32(displayPoints.Value));
        }

        /* Initializes a chain of data filters for X, Y, Z channels. */
        protected void InitializeFilters(int averagePoints, int bufferSize)
        {
            for (int i = 0; i < 3; i++)
            {
                InitializeFilter(averagePoints, bufferSize, i);
            }
        }

        /* Initializes a chain of data filters for a single channel. */
        protected void InitializeFilter(int averagePoints, int bufferSize, int i)
        {
            averagePoints = Math.Max(1, averagePoints);
            averages[i] = new MovingAverage(averagePoints);
            subsamples[i] = new Subsample(averagePoints);
            buffers[i] = new RollingBuffer(bufferSize, double.NaN);
            averages[i].output += new Action<double[]>(subsamples[i].InputData);
            subsamples[i].output += new Action<double[]>(buffers[i].InputData);
        }

        /* Called when a Google upload progress is updated */
        protected void Google_ProgressEvent(string fullPath, long bytesSent,
            long bytesTotal)
        {
            double progress = 100 * bytesSent / bytesTotal;
            var name = Path.GetFileName(fullPath);
            var msg = string.Format("Uploading {0}, {1}% done", name, 
                progress);
            SetTextThreadSafe(msg);
        }

        /* Thread-safe method to set the status label text */
        delegate void SetTextCallback(string text);
        protected void SetTextThreadSafe(string text)
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

        /* Called when a Google upload has finished */
        protected void Scheduler_FinishedEvent(IDatasetInfo info, bool success,
            string message)
        {
            SetTextThreadSafe(message);
        }

        /* Called when a Google upload has started */
        protected void Scheduler_StartedEvent(IDatasetInfo info)
        {
            var msg = string.Format("Uploading dataset from {0}", 
                info.StartDate);
            SetTextThreadSafe(msg);
        }

        /* Updates the magnetometer UI */
        protected virtual void SetUI(UiStateMagnetometer state)
        {
            switch (state)
            {
                case UiStateMagnetometer.Ready:
                    name.Enabled = true;
                    sensorList.Enabled = true;
                    averagingPeriodMs.Enabled = true;
                    powerLineFilter.Enabled = false;
                    plotUpdateTimer.Enabled = false;
                    displayPoints.Enabled = true;
                    refreshButton.Enabled = true;
                    recordButton.Enabled = true;
                    cancelButton.Enabled = false;
                    uploadButton.Enabled = true;
                    break;

                case UiStateMagnetometer.Recording:
                    name.Enabled = false;
                    sensorList.Enabled = false;
                    averagingPeriodMs.Enabled = false;
                    powerLineFilter.Enabled = false;
                    plotUpdateTimer.Enabled = true;
                    displayPoints.Enabled = false;
                    refreshButton.Enabled = false;
                    recordButton.Enabled = false;
                    cancelButton.Enabled = true;
                    uploadButton.Enabled = false;
                    break;

                case UiStateMagnetometer.NoSensorFound:
                    name.Enabled = true;
                    sensorList.Enabled = true;
                    averagingPeriodMs.Enabled = true;
                    powerLineFilter.Enabled = false;
                    plotUpdateTimer.Enabled = false;
                    displayPoints.Enabled = true;
                    refreshButton.Enabled = true;
                    recordButton.Enabled = false;
                    cancelButton.Enabled = false;
                    uploadButton.Enabled = true;
                    break;
            }
        }

        /* Called when refresh magnetometer list button is clicked */
        protected void refreshButton_Click(object sender, EventArgs e)
        {
            RefreshSensorList();
        }

        /* Updates the list of magnetometer sensors */
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
                SetUI(UiStateMagnetometer.NoSensorFound);
            }
        }

        /* Selects a magnetometer sensor given device ID */
        protected void SelectSensor(int serial)
        {
            try
            {
                sensor = new eMains(serial);
                SetUI(UiStateMagnetometer.Ready);
            }
            catch (eMainsException exception)
            {
                toolStripStatusLabel.Text = exception.Message;
                SetUI(UiStateMagnetometer.NoSensorFound);
            }
        }

        /* Called when a user selects a magnetometer from the list */
        protected void sensorList_SelectedIndexChanged(object sender, 
            EventArgs e)
        {
            var serial = Convert.ToInt32(sensorList.Text);
            SelectSensor(serial);
        }

        /* Called when record button is clicked */
        protected void recordButton_Click(object sender, EventArgs e)
        {
            StartRecording();
        }

        protected void StartRecording()
        {
            if (sensor == null)
            {
                SetUI(UiStateMagnetometer.NoSensorFound);
                toolStripStatusLabel.Text
                    = "Magnetic sensor is not initialized.";
            }

            try
            {
                sensor.DAQInitialize(samplingRate, range, 1, 1);
                sensor.NewDataHandler -= Sensor_NewDataHandler;
                sensor.NewDataHandler += Sensor_NewDataHandler;
                UpdateFilters();
                lagFilter.Clear();
                sensor.DAQStart(convertToMicroTesla);
                SetUI(UiStateMagnetometer.Recording);
            }
            catch (Exception exception)
            {
                toolStripStatusLabel.Text = exception.Message;
                SetUI(UiStateMagnetometer.Ready);
            }
        }

        /* Triggers the graph update. */
        protected void plotUpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdatePlot("X", buffers[0].GetData());
            UpdatePlot("Y", buffers[1].GetData());
            UpdatePlot("Z", buffers[2].GetData());
        }

        /* Updates the given series in the graph. */
        protected void UpdatePlot(string series, double[] data)
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

        /* Called when new magnetic data arrives */
        protected void Sensor_NewDataHandler(double[] dataX, double[] dataY,
            double[] dataZ, long ticks, DateTime time)
        {
            var gpsData = interpolator.GetTimeStamp(ticks);
            var chunk = new GpsDatasetChunk(gpsData, interpolator, 0, 
                dataX, dataY, dataZ);
            lagFilter.InputData(chunk);
            averages[0].InputData(dataX);
            averages[1].InputData(dataY);
            averages[2].InputData(dataZ);
        }

        /* Called when filtered data pops out of the spike filter */
        private void LagFilter_OnPop(GpsDatasetChunk chunk)
        {
            storage.Store(chunk.XData, chunk.YData, chunk.ZData, 
                chunk.Gps);
        }

        /* Called when user clicks cancel recording button */
        protected void cancelButton_Click(object sender, EventArgs e)
        {
            StopRecording();
        }

        protected void StopRecording()
        {
            if (sensor == null)
            {
                SetUI(UiStateMagnetometer.NoSensorFound);
                toolStripStatusLabel.Text
                    = "Magnetic sensor is not initialized.";
            }

            try
            {
                sensor.DAQStop();
                sensor.NewDataHandler -= Sensor_NewDataHandler;
                SetUI(UiStateMagnetometer.Ready);
                lagFilter.Flush();
            }
            catch (Exception exception)
            {
                toolStripStatusLabel.Text = exception.Message;
                SetUI(UiStateMagnetometer.Ready);
            }
        }

        /* Called when user clicks upload button */
        protected void uploadButton_Click(object sender, EventArgs e)
        {
            storage.Upload();
        }

        /* Called when the form is being closed */
        protected void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = FinalizeUploads();
            CloseGps();
            StopRecording();
        }

        /* Returns true if there are uploads to be finished.
        Tells the scheduler to hurry up is that's the case. */
        private bool FinalizeUploads()
        {
            /* This condition will sometimes fail because of racing,
            but this shouldn't really matter */
            if (scheduler.ActiveUploads + scheduler.QueuedUploads > 0)
            {
                toolStripStatusLabel.Text = "Please wait until the files are uploaded.";
                scheduler.Flush();
                return true;
            }
            return false;
        }

        /* Called when user selects a new time constant for the display 
        average filter */
        protected void averagingPeriodMs_ValueChanged(object sender, EventArgs e)
        {
            UpdateFilters();
        }

        /* Called when user selects a new maximum number of points to display */
        protected void displayPoints_ValueChanged(object sender, EventArgs e)
        {
            UpdateFilters();
        }

        /* Called when user clicks refresh GPS list button */
        protected void gpsRefreshButton_Click(object sender, EventArgs e)
        {
            RefreshGpsList();
        }

        /* Updates the list of serial ports present in the system */
        protected void RefreshGpsList()
        {
            try
            {
                gpsList.Items.Clear();
                var portNames = System.IO.Ports.SerialPort.GetPortNames();
                foreach (var name in portNames)
                {
                    gpsList.Items.Add(name);
                }

                if (portNames.Length == 0)
                {
                    SetUiGps(UiStateGps.NoSensor);
                }
                else
                {
                    SetUiGps(UiStateGps.Closed);
                    gpsList.Text = portNames[0];
                }
            }
            catch (Exception e)
            {
                toolStripStatusLabel.Text = e.Message;
            }
        }

        /* Called when user clicks open GPS button */
        protected void gpsOpenButton_Click(object sender, EventArgs e)
        {
            try
            {
                gps = new SerialGps(gpsList.Text, 4800);
                gps.TimestampReceived += Gps_TimestampReceived;

                gps.Open();
                gpsTimeoutTimer.Enabled = true;
                SetUiGps(UiStateGps.Opened);
            }
            catch (Exception exception)
            {
                toolStripStatusLabel.Text = exception.Message;
            }
        }

        /* Called when new GPS data is received */
        private void Gps_TimestampReceived(GpsData data)
        {
            gpsTimeoutTimer.Enabled = false;
            gpsTimeoutTimer.Enabled = true;
            interpolator.PutTimestamp(data);
            var msg = BuildGpsStatusMessage(data);
            var color = GetGpsStatusColor(data);
            UpdateGpsStatusThreadSafe(msg, color);
        }

        /* Returns GPS status message given the GPS data */
        private string BuildGpsStatusMessage(GpsData data)
        {
            string active;
            if (data.valid)
            {
                active = "ACTIVE";
            }
            else
            {
                active = "No PPS or VOID";
            }
            return string.Format("UTC: {0} ({1})",
                data.timestamp, active);
        }

        /* Returns GPS status message color given the data */
        System.Drawing.Color GetGpsStatusColor(GpsData data)
        {
            if (data.valid)
            {
                return System.Drawing.Color.LightGreen;
            }
            else
            {
                return System.Drawing.Color.Red;
            }
        }

        /* Called when the GPS data hasn't been received for too long */
        private void GpsTimeoutTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            UpdateGpsStatusThreadSafe("No data", System.Drawing.Color.Red);
        }

        /* Thread-safe function to update GPS status */
        delegate void UpdateGpsStatusCallback(string msg, 
            System.Drawing.Color color);
        protected void UpdateGpsStatusThreadSafe(string msg,
            System.Drawing.Color color)
        {           
            if (InvokeRequired)
            {
                UpdateGpsStatusCallback f = new UpdateGpsStatusCallback(
                    UpdateGpsStatusThreadSafe);
                Invoke(f, new object[] { msg, color });
            }
            else
            {
                gpsStatusLabel.Text = msg;
                gpsStatusLabel.BackColor = color;
            }
        }

        /* Called when user clicks close GPS button */
        protected void gpsCloseButton_Click(object sender, EventArgs e)
        {
            CloseGps();
        }

        protected void CloseGps()
        {
            try
            {
                gpsTimeoutTimer.Enabled = false;
                SetUiGps(UiStateGps.Closed);
                gps.Close();
            }
            catch (Exception exception)
            {
                toolStripStatusLabel.Text = exception.Message;
            }
        }

        /* GPS UI status list */
        protected enum UiStateGps
        {
            Opened,
            Closed,
            NoSensor
        }

        /* Update GPS UI part */
        protected virtual void SetUiGps(UiStateGps state)
        {
            switch (state)
            {
                case UiStateGps.Opened:
                    gpsList.Enabled = false;
                    gpsOpenButton.Enabled = false;
                    gpsCloseButton.Enabled = true;
                    gpsRefreshButton.Enabled = false;
                    gpsStatusLabel.Text = "Connected";
                    gpsStatusLabel.BackColor = System.Drawing.Color.LightGray;
                    break;

                case UiStateGps.Closed:
                    gpsList.Enabled = true;
                    gpsOpenButton.Enabled = true;
                    gpsCloseButton.Enabled = false;
                    gpsRefreshButton.Enabled = true;
                    gpsStatusLabel.Text = "Disconnected";
                    gpsStatusLabel.BackColor = System.Drawing.Color.Red;
                    break;

                case UiStateGps.NoSensor:
                    gpsList.Enabled = true;
                    gpsOpenButton.Enabled = true;
                    gpsCloseButton.Enabled = false;
                    gpsRefreshButton.Enabled = true;
                    gpsStatusLabel.Text = "Disconnected";
                    gpsStatusLabel.BackColor = System.Drawing.Color.Red;
                    break;
            }
        }

        /* Called when the form is loaded */
        protected void MagnetometerForm_Load(object sender, EventArgs e)
        {            
            if (DesignMode) 
            {
                /* Needed for the Visual Studio form designer to work */
                return;
            }
            
            InitializeResources();
            RefreshSensorList();
        }
    }
}
