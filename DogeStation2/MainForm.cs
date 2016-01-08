using System;
using System.Windows.Forms;
using Utils.GDrive;
using Utils.DataManager;
using Biomed_eMains_eFMx;
using System.Configuration;
using System.IO;

namespace DogeStation2
{
    public partial class MainForm : Form
    {
        private IStorage storage;
        private eMains sensor;
        private double samplingRate;
        private Range range = Range.NEG_10_TO_PLUS_10V;
        private bool convertToMicroTesla;
        private DateTime lastUpdated = DateTime.Now;

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
                var settings = ConfigurationManager.AppSettings;
                stationName.Text = settings["StationName"];
                samplingRate = Convert.ToInt32(settings["SamplingRate"]);
                var units = settings["DataUnits"];
                convertToMicroTesla = (units == "uT");
                var google = new GDrive("nuri-station.json");
                google.ProgressEvent += Google_ProgressEvent;
                var scheduler = new UploadScheduler(google);
                scheduler.StartedEvent += Scheduler_StartedEvent;
                scheduler.FinishedEvent += Scheduler_FinishedEvent;
                storage = new Storage(scheduler);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Can't initialize: " + exception.Message);
                Application.Exit();
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
        }

        private void Scheduler_StartedEvent(IDatasetInfo info)
        {
            var msg = string.Format("Uploading dataset from {0}", 
                info.StartDate);
            SetTextThreadSafe(msg);
        }

        private void stationName_TextChanged(object sender, EventArgs e)
        {
            var settings = ConfigurationManager.AppSettings;
            settings["StationName"] = stationName.Text;
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
                    return;
                case UI_State.Recording:
                    stationName.Enabled = false;
                    sensorList.Enabled = false;
                    refreshButton.Enabled = false;
                    recordButton.Enabled = false;
                    cancelButton.Enabled = true;
                    return;
                case UI_State.NoSensorFound:
                    stationName.Enabled = true;
                    sensorList.Enabled = true;
                    refreshButton.Enabled = true;
                    recordButton.Enabled = false;
                    cancelButton.Enabled = false;
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
                sensorList.Items.Add(sensors);
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
            throw new NotImplementedException();
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
                Points.Add(data);
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
    }
}
