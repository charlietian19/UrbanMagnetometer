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
    }
}
