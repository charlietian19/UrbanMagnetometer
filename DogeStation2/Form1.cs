using System;
using System.Windows.Forms;
using GDriveNURI;

namespace DogeStation2
{
    public partial class Form1 : Form
    {
        private GDriveNURI.UploadScheduler GoogleWriter = null;

        public Form1()
        {
            InitializeComponent();
            TryAuthenticate();
        }

        private void TryAuthenticate()
        {
            try
            {
                IUploader google = new GDrive("nuri-station", "nuri-station.json");
                GoogleWriter = new GDriveNURI.UploadScheduler(google);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Can't authenticate: " + exception.Message);
            }
        }

        private void AuthButton_Click(object sender, EventArgs e)
        {
            TryAuthenticate();
        }
        
    }
}
