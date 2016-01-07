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
                var settings = ConfigurationManager.AppSettings;
                stationName.Text = settings["StationName"];
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
            var msg = string.Format("Uploading {0}, {1}% done", name, progress);
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
            var msg = string.Format("Uploading dataset from {0}", info.StartDate);
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
                    sensor = new eMains(sensors[0]);
                }
            }
            catch (eMainsException exception)
            {
                toolStripStatusLabel.Text = exception.Message;
            }

        }

        private void sensorList_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var serial = Convert.ToInt32(sensorList.Text);
                sensor = new eMains(serial);
            }
            catch (eMainsException exception)
            {
                toolStripStatusLabel.Text = exception.Message;
            }
        }
    }
}
