using System;
using System.Windows.Forms;
using Utils.GDrive;
using Utils.DataManager;


namespace DogeStation2
{
    public partial class Form1 : Form
    {
        private IStorage storage;

        public Form1()
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
