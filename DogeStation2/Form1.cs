﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GDriveNURI;

namespace DogeStation2
{
    public partial class Form1 : Form
    {
        private GDriveNURI.Uploader GoogleWriter = null;

        public Form1()
        {
            InitializeComponent();
            TryAuthenticate();
        }

        private void TryAuthenticate()
        {
            try
            {
                GoogleWriter = new GDriveNURI.Uploader("nuri-station", "nuri-station.json");
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
