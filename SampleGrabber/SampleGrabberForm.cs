﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SampleGrabber;
using Utils.Configuration;

namespace DogeStation2
{
    public partial class SampleGrabberForm : SampleGrabber.MagnetometerForm
    {
        public SampleGrabberForm()
        {
            InitializeComponent();
        }

        protected override void InitializeResources()
        {
            base.InitializeResources();
            name.Text = Settings.SampleName;
            comment.Text = Settings.SampleComment;
            storage.UploadOnClose = true;            
        }

        protected override void SetUI(UiStateMagnetometer state)
        {
            switch (state)
            {
                case UiStateMagnetometer.Ready:
                    comment.Enabled = true;
                    discardButton.Enabled = true;
                    break;

                case UiStateMagnetometer.Recording:
                    comment.Enabled = false;
                    discardButton.Enabled = false;
                    break;

                case UiStateMagnetometer.NoSensorFound:
                    comment.Enabled = true;
                    discardButton.Enabled = true;
                    break;
            }
            base.SetUI(state);
        }

        protected void comment_TextChanged(object sender, EventArgs e)
        {
            Settings.SampleComment = comment.Text;
        }

        protected void discardButton_Click(object sender, EventArgs e)
        {
            storage.Discard();
        }

        /* Called when user changes the sample name changes */
        protected void name_TextChanged(object sender, EventArgs e)
        {
            Settings.SampleName = name.Text;
        }
    }
}
