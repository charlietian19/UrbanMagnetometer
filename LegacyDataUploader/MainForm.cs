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

namespace LegacyDataUploader
{
    public partial class MainForm : Form
    {
        private DatasetReader reader;
        public MainForm()
        {
            InitializeComponent();
            var settings = ConfigurationManager.AppSettings;
            stationName.Text = settings["StationName"];
        }

        private void stationName_TextChanged(object sender, EventArgs e)
        {
            var settings = ConfigurationManager.AppSettings;
            settings["StationName"] = stationName.Text;
        }

        private void browseButton_Click(object sender, EventArgs e)
        {

        }
    }
}
