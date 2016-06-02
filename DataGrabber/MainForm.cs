using System;
using System.Windows.Forms;
using Utils.GDrive;
using Utils.DataManager;
using Utils.Configuration;
using Utils.GPS;
using Utils.GPS.SerialGPS;
using Utils.Filters;
using Biomed_eMains_eFMx;
using System.IO;
using System.Diagnostics;

namespace DataGrabber
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
        private ITimeSource gps;
        private ITimeEstimator interpolator = new NaiveTimeEstimator();
        private bool gpsValid = false;

        enum UiStateMagnetometer
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
            RefreshGpsList();
        }

        private void InitializeResources()
        {
            try
            {
                eMains.LoadDLL();
                stationName.Text = Settings.StationName;
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

        /* Resets the data filters */
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
            Settings.StationName = stationName.Text;
        }

        void SetUiMagnetometer(UiStateMagnetometer state)
        {
            switch (state)
            {
                case UiStateMagnetometer.Ready:
                    stationName.Enabled = true;
                    sensorList.Enabled = true;
                    refreshButton.Enabled = true;
                    recordButton.Enabled = true;
                    cancelButton.Enabled = false;
                    uploadButton.Enabled = true;
                    powerLineFilter.Enabled = false;
                    return;
                case UiStateMagnetometer.Recording:
                    stationName.Enabled = false;
                    sensorList.Enabled = false;
                    refreshButton.Enabled = false;
                    recordButton.Enabled = false;
                    cancelButton.Enabled = true;
                    uploadButton.Enabled = false;
                    powerLineFilter.Enabled = false;
                    return;
                case UiStateMagnetometer.NoSensorFound:
                    stationName.Enabled = true;
                    sensorList.Enabled = true;
                    refreshButton.Enabled = true;
                    recordButton.Enabled = false;
                    cancelButton.Enabled = false;
                    uploadButton.Enabled = true;
                    powerLineFilter.Enabled = false;
                    return;
                default:
                    throw new InvalidOperationException("Unknown UI state");
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
                SetUiMagnetometer(UiStateMagnetometer.NoSensorFound);
            }
        }

        private void SelectSensor(int serial)
        {
            try
            {
                sensor = new eMains(serial);
                SetUiMagnetometer(UiStateMagnetometer.Ready);
            }
            catch (eMainsException exception)
            {
                toolStripStatusLabel.Text = exception.Message;
                SetUiMagnetometer(UiStateMagnetometer.NoSensorFound);
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
                SetUiMagnetometer(UiStateMagnetometer.NoSensorFound);
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
                SetUiMagnetometer(UiStateMagnetometer.Recording);
            }
            catch (Exception exception)
            {
                toolStripStatusLabel.Text = exception.Message;
                SetUiMagnetometer(UiStateMagnetometer.Ready);
            }
        }

        /* Triggers the graph update. */
        private void plotUpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdatePlot("X", buffers[0].GetData());
            UpdatePlot("Y", buffers[1].GetData());
            UpdatePlot("Z", buffers[2].GetData());

            if (gpsValid)
            {
                gpsStatusLabel.BackColor = System.Drawing.Color.Green;
            }
            else
            {
                gpsStatusLabel.BackColor = System.Drawing.Color.Red;
            }
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
            var gpsData = interpolator.GetTimeStamp(Convert.ToInt64(
                systemSeconds * Stopwatch.Frequency));
            storage.Store(dataX, dataY, dataZ, gpsData);
            averages[0].InputData(dataX);
            averages[1].InputData(dataY);
            averages[2].InputData(dataZ);
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            if (sensor == null)
            {
                SetUiMagnetometer(UiStateMagnetometer.NoSensorFound);
                toolStripStatusLabel.Text 
                    = "Magnetic sensor is not initialized.";
            }

            try
            {
                sensor.DAQStop();
                sensor.NewDataHandler -= Sensor_NewDataHandler;
                SetUiMagnetometer(UiStateMagnetometer.Ready);
            }
            catch (Exception exception)
            {
                toolStripStatusLabel.Text = exception.Message;
                SetUiMagnetometer(UiStateMagnetometer.Ready);
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
                SetUiMagnetometer(UiStateMagnetometer.Ready);
            }

            if (storage.HasCachedData)
            {
                storage.Close();
                doneUploading = false;
            }

            if (!doneUploading)
            {
                e.Cancel = true;
                toolStripStatusLabel.Text
                    = "Please wait until the files are uploaded.";
            }
        }

        private void averagingPeriodMs_ValueChanged(object sender, EventArgs e)
        {
            UpdateFilters();
        }

        private void displayPoints_ValueChanged(object sender, EventArgs e)
        {
            UpdateFilters();
        }

        private void discardButton_Click(object sender, EventArgs e)
        {
            storage.Discard();
        }

        private void gpsRefreshButton_Click(object sender, EventArgs e)
        {
            RefreshGpsList();
        }

        private void RefreshGpsList()
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
                }
            }
            catch (Exception e)
            {
                toolStripStatusLabel.Text = e.Message;
            }            
        }

        private void gpsOpenButton_Click(object sender, EventArgs e)
        {
            try
            {
                gps = new SerialGps(gpsList.Text);
                gps.TimestampReceived += data =>
                {
                    interpolator.PutTimestamp(data);
                    gpsValid = data.valid;
                };                    
                gps.Open();
                SetUiGps(UiStateGps.Opened);
            }
            catch (Exception exception)
            {
                toolStripStatusLabel.Text = exception.Message;
            }
        }

        private void gpsCloseButton_Click(object sender, EventArgs e)
        {
            try
            {
                gps.Close();
                SetUiGps(UiStateGps.Closed);
            }
            catch (Exception exception)
            {
                toolStripStatusLabel.Text = exception.Message;
            }
        }

        enum UiStateGps
        {
            Opened,
            Closed,
            NoSensor
        }

        private void SetUiGps(UiStateGps state)
        {
            switch (state)
            {
                case UiStateGps.Opened:
                    gpsList.Enabled = false;
                    gpsOpenButton.Enabled = false;
                    gpsCloseButton.Enabled = true;
                    gpsRefreshButton.Enabled = false;
                    gpsStatusLabel.Text = "Connected";
                    break;
                case UiStateGps.Closed:
                    gpsList.Enabled = true;
                    gpsOpenButton.Enabled = true;
                    gpsCloseButton.Enabled = false;
                    gpsRefreshButton.Enabled = true;
                    gpsStatusLabel.Text = "Disconnected";
                    break;
                case UiStateGps.NoSensor:
                    gpsList.Enabled = true;
                    gpsOpenButton.Enabled = true;
                    gpsCloseButton.Enabled = false;
                    gpsRefreshButton.Enabled = true;
                    gpsStatusLabel.Text = "Disconnected";
                    break;
                default:
                    throw new InvalidOperationException("Unknown UI state");
            }
        }
    }
}
