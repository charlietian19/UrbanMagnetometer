namespace DataGrabber
{
    partial class DataGrabberForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            ((System.ComponentModel.ISupportInitialize)(this.dataGraph)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.averagingPeriodMs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.displayPoints)).BeginInit();
            this.previewBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // sensorList
            // 
            this.sensorList.Size = new System.Drawing.Size(121, 39);
            // 
            // recordButton
            // 
            this.recordButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DimGray;
            // 
            // name
            // 
            this.name.TextChanged += new System.EventHandler(this.name_TextChanged);
            // 
            // previewBox
            // 
            this.previewBox.Location = new System.Drawing.Point(29, 134);
            // 
            // gpsCloseButton
            // 
            this.gpsCloseButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DimGray;
            this.gpsCloseButton.Location = new System.Drawing.Point(612, 73);
            // 
            // gpsStatusLabel
            // 
            this.gpsStatusLabel.Location = new System.Drawing.Point(756, 84);
            // 
            // gpsOpenButton
            // 
            this.gpsOpenButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DimGray;
            this.gpsOpenButton.Location = new System.Drawing.Point(466, 73);
            // 
            // gpsRefreshButton
            // 
            this.gpsRefreshButton.Location = new System.Drawing.Point(320, 73);
            // 
            // gpsList
            // 
            this.gpsList.Location = new System.Drawing.Point(130, 77);
            this.gpsList.Size = new System.Drawing.Size(169, 39);
            // 
            // gpsLabel
            // 
            this.gpsLabel.Location = new System.Drawing.Point(31, 84);
            // 
            // GrabberForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1436, 963);
            this.Name = "GrabberForm";
            this.Text = "Magnetic Data Logger";
            ((System.ComponentModel.ISupportInitialize)(this.dataGraph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.averagingPeriodMs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.displayPoints)).EndInit();
            this.previewBox.ResumeLayout(false);
            this.previewBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }
}