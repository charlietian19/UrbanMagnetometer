using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utils.Configuration;

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
            storage.UploadOnClose = true;            
        }
    }
}
