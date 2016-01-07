using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using Utils.DataReader;
using Utils.GDrive;
using Utils.DataManager;
using System.IO;

namespace LegacyDataUploader
{
    public partial class MainForm : Form
    {
        private IUploader google;
        private IUploadScheduler scheduler;
        private IStorage storage;
        private LegacyReader reader;

        enum UI_State
        {
            Start,
            FileOpened,
            Uploading,
            Done
        }

        public MainForm()
        {
            InitializeComponent();
            SetUI(UI_State.Start);
            try
            {
                var settings = ConfigurationManager.AppSettings;
                stationName.Text = settings["StationName"];
                int queueBound = Convert.ToInt32(settings["MaxActiveUploads"]);
                google = new GDrive("nuri-station.json");
                google.ProgressEvent += Google_ProgressEvent;
                scheduler = new UploadScheduler(google, queueBound);
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

        private void browseButton_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (reader != null)
                {
                    reader.Close();
                }

                try
                {
                    reader = new LegacyReader();
                    reader.OpenDataFiles(openFileDialog.FileName);
                    fileName.Text = openFileDialog.FileName;
                    startTime.Text = reader.DatasetStartTime.ToString();
                    chunksTotal.Text = reader.ChunksTotal.ToString();
                    pointsTotal.Text = reader.Points.ToString();
                    double size = reader.Size / 1073741824;
                    sizeTotalGB.Text = size.ToString();
                    toolStripStatusLabel.Text = "File opened successfully.";
                    SetUI(UI_State.FileOpened);
                }
                catch (Exception exception)
                {
                    toolStripStatusLabel.Text = exception.Message;
                    reader.Close();
                }
            }
        }

        private void Worker_ProgressChanged(object sender, 
            ProgressChangedEventArgs e)
        {
            var progress = Math.Min(100, e.ProgressPercentage);
            progress = Math.Max(0, progress);
            totalProgress.Value = e.ProgressPercentage;
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            LegacyReader reader = (LegacyReader)e.Argument;
            double kx = Convert.ToDouble(xSlope.Value);
            double ky = Convert.ToDouble(ySlope.Value);
            double kz = Convert.ToDouble(zSlope.Value);
            double bx = Convert.ToDouble(xOffset.Value);
            double by = Convert.ToDouble(yOffset.Value);
            double bz = Convert.ToDouble(zOffset.Value);
            double progress = 0, newprogress = 0;

            while (reader.HasNextChunk() && (!worker.CancellationPending))
            {
                var chunk = reader.GetNextChunk();
                double[] x, y, z;
                x = chunk.XData;
                y = chunk.YData;
                z = chunk.ZData;

                for (int i = 0; i < x.Length; i++)
                {
                    x[i] = kx * x[i] + bx;
                    y[i] = ky * y[i] + by;
                    z[i] = kz * z[i] + bz;
                }

                storage.Store(x, y, z, chunk.PerformanceCounter, chunk.Time);
                newprogress = 100 * reader.ChunkIndex / reader.ChunksTotal;
                if (newprogress != progress)
                {
                    worker.ReportProgress(Convert.ToInt32(progress));
                    progress = newprogress;
                }                
            }
            worker.ReportProgress(100);
            storage.Close();
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            reader.Rewind();
            SetUI(UI_State.Done);
            if (e.Cancelled)
            {
                toolStripStatusLabel.Text = "Cancelled.";
            }
            else if (e.Error != null)
            {
                toolStripStatusLabel.Text = e.Error.Message;
                Console.WriteLine(e.Error.Message);
            }      
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            if (worker.IsBusy)
            {
                worker.CancelAsync();                
            }                       
        }

        private void uploadButton_Click(object sender, EventArgs e)
        {
            if (!worker.IsBusy && (reader != null))
            {
                SetUI(UI_State.Uploading);
                worker.RunWorkerAsync(reader);
            }
        }


        void SetUI(UI_State state)
        {
            switch (state)
            {
                case UI_State.Start:
                    stationName.Enabled = true;
                    xOffset.Enabled = true;
                    yOffset.Enabled = true;
                    zOffset.Enabled = true;
                    xSlope.Enabled = true;
                    ySlope.Enabled = true;
                    zSlope.Enabled = true;
                    browseButton.Enabled = true;
                    uploadButton.Enabled = false;
                    cancelButton.Enabled = false;
                    return;
                case UI_State.FileOpened:
                case UI_State.Done:
                    stationName.Enabled = true;
                    xOffset.Enabled = true;
                    yOffset.Enabled = true;
                    zOffset.Enabled = true;
                    xSlope.Enabled = true;
                    ySlope.Enabled = true;
                    zSlope.Enabled = true;
                    browseButton.Enabled = true;
                    uploadButton.Enabled = true;
                    cancelButton.Enabled = false;
                    return;
                case UI_State.Uploading:
                    stationName.Enabled = false;
                    xOffset.Enabled = false;
                    yOffset.Enabled = false;
                    zOffset.Enabled = false;
                    xSlope.Enabled = false;
                    ySlope.Enabled = false;
                    zSlope.Enabled = false;
                    browseButton.Enabled = false;
                    uploadButton.Enabled = false;
                    cancelButton.Enabled = true;
                    return;
            }
        }
    }
}
