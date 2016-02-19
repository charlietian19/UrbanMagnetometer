using System;
using System.Windows.Forms;
using Utils.GDrive;
using Utils.DataManager;
using Utils.Configuration;
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
                stationName.Text = Settings.StationName;
                samplingRate = Convert.ToInt32(Settings.SamplingRate);
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

        void SetUI(UI_State state)
        {
            switch (state)
            {
                case UI_State.Ready:
                    stationName.Enabled = true;
                    sensorList.Enabled = true;
                    refreshButton.Enabled = true;
                    recordButton.Enabled = true;
                    cancelButton.Enabled = false;
                    uploadButton.Enabled = true;
                    return;
                case UI_State.Recording:
                    stationName.Enabled = false;
                    sensorList.Enabled = false;
                    refreshButton.Enabled = false;
                    recordButton.Enabled = false;
                    cancelButton.Enabled = true;
                    uploadButton.Enabled = false;
                    return;
                case UI_State.NoSensorFound:
                    stationName.Enabled = true;
                    sensorList.Enabled = true;
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

        private void Sensor_NewDataHandler(double[] dataX, double[] dataY,
            double[] dataZ, double systemSeconds, DateTime time)
        {
            storage.Store(dataX, dataY, dataZ, systemSeconds, time);
            if (time.Subtract(lastUpdated).TotalSeconds > 1)
            {
                UpdateGraphThreadSafe(dataX, dataY, dataZ);
                lastUpdated = time;
            }
        }

        private void UpdateGraphThreadSafe(double[] dataX, double[] dataY, 
            double[] dataZ)
        {
            UpdatePlotThreadSafe("X", dataX);
            UpdatePlotThreadSafe("Y", dataY);
            UpdatePlotThreadSafe("Z", dataZ);
        }

        delegate void UpdatePlotCallback(string series, double[] data);
        private void UpdatePlotThreadSafe(string series, double[] data)
        {
            if (dataGraph.InvokeRequired)
            {
                var f = new UpdatePlotCallback(UpdatePlotThreadSafe);
                Invoke(f, new object[] { series, data });
            }
            else
            {
                var Points = dataGraph.Series.FindByName(series).Points;
                Points.Clear();
                foreach (var point in data)
                {
                    Points.Add(point);
                }
            }
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
    }
}
