using System;
using System.Windows.Forms;
using Utils.GDrive;
using Utils.DataManager;
using Biomed_eMains_eFMx;


namespace DogeStation2
{
    public partial class MainForm : Form
    {
        private IStorage storage;
        private eMains sensor;

        public MainForm()
        {
            InitializeComponent();
            TryAuthenticate();
        }

        private void TryAuthenticate()
        {
            try
            {
                var google = new GDrive("nuri-station.json");
                var scheduler = new UploadScheduler(google);
                storage = new Storage(scheduler);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Can't initialize: " + exception.Message);
            }
        }

        private void AuthButton_Click(object sender, EventArgs e)
        {
            TryAuthenticate();
        }
        
    }
}
