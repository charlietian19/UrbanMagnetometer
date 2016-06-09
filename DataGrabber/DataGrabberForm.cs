using System;
using Utils.Configuration;
using Utils.DataManager;

namespace DataGrabber
{
    public partial class DataGrabberForm : SampleGrabber.MagnetometerForm
    {
        public DataGrabberForm()
        {
            InitializeComponent();
        }

        /* Called when the form is initialized */
        protected override void InitializeResources()
        {
            base.InitializeResources();
            name.Text = Settings.StationName;
            storage = new LegacyStorage(scheduler);
            storage.UploadOnClose = true;            
        }

        /* Called when user changes the station name changes */
        protected void name_TextChanged(object sender, EventArgs e)
        {
            Settings.StationName = name.Text;
        }

        /* Called when user closes the form */
        private void DataGrabberForm_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            StopRecording();
            if (storage.HasCachedData)
            {
                storage.Close();
                e.Cancel = true;
            }            
        }
    }
}
