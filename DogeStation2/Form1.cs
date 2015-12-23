using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GDriveWriter;

namespace DogeStation2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var writer = new GDriveWriter.GDriveWriter("nuri-station", "nuri-station.json");
                writer.ListFiles();
            }
            catch (Exception exception)
            {
                // Do nothing.
            }
        }
    }
}
