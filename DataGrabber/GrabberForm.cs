using System;
using Utils.Configuration;
using Utils.DataManager;

namespace DataGrabber
{
    public partial class GrabberForm : SampleGrabber.MagnetometerForm
    {
        public GrabberForm()
        {
            InitializeComponent();
        }

        protected override void InitializeResources()
        {
            base.InitializeResources();
            name.Text = Settings.StationName;
            storage = new LegacySampleStorage(scheduler);
            storage.UploadOnClose = true;            
        }

        /* Called when user changes the station name changes */
        protected void name_TextChanged(object sender, EventArgs e)
        {
            Settings.StationName = name.Text;
        }
    }
}
