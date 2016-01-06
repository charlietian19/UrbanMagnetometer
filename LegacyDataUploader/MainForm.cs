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

namespace LegacyDataUploader
{
    public partial class MainForm : Form
    {
        private IUploader google;
        private IUploadScheduler scheduler;
        private IStorage storage;
        private LegacyReader reader;

        public MainForm()
        {
            InitializeComponent();
            var settings = ConfigurationManager.AppSettings;
            stationName.Text = settings["StationName"];            
            google = new GDrive("nuri-station.json");
            google.ProgressEvent += Google_ProgressEvent;
            scheduler = new UploadScheduler(google);
            scheduler.StartedEvent += Scheduler_StartedEvent;
            scheduler.FinishedEvent += Scheduler_FinishedEvent;
            storage = new Storage(scheduler);
        }

        private void Google_ProgressEvent(string name, long bytesSent, 
            long bytesTotal)
        {
            double progress = 100 * bytesSent / bytesTotal;
            uploadProgressBar.Value = Convert.ToInt32(progress);
            toolStripStatusLabel.Text = "Uploading " + name;
        }

        private void Scheduler_FinishedEvent(IDatasetInfo info, bool success, 
            string message)
        {
            toolStripStatusLabel.Text = message;
        }

        private void Scheduler_StartedEvent(IDatasetInfo info)
        {
            toolStripStatusLabel.Text = "Uploading...";
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
                    chunksTotal.Text = reader.Chunks.ToString();
                    pointsTotal.Text = reader.Points.ToString();
                    toolStripStatusLabel.Text = "File opened successfully.";
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
            totalProgress.Value = e.ProgressPercentage;
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            LegacyReader reader = (LegacyReader)e.Argument;           
            while (reader.HasNextChunk() && (!worker.CancellationPending))
            {
                var chunk = reader.GetNextChunk();
                storage.Store(chunk.XData, chunk.XData, chunk.ZData, 
                    chunk.PerformanceCounter, chunk.Time);
                var progress = 100 * chunk.Index / reader.Chunks;
                worker.ReportProgress(Convert.ToInt32(progress));
            }
            reader.Close();
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                toolStripStatusLabel.Text = "Cancelled.";
            }
            else if (e.Error == null)
            {
                uploadProgressBar.Value = 100;
            }
            else
            {
                toolStripStatusLabel.Text = e.Error.Message;
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            worker.CancelAsync();            
        }

        private void uploadButton_Click(object sender, EventArgs e)
        {
            if (!worker.IsBusy && (reader != null))
            {
                worker.RunWorkerAsync(reader);
            }
        }
    }
}
