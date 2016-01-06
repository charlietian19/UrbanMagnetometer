namespace LegacyDataUploader
{
    partial class MainForm
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
            this.browseButton = new System.Windows.Forms.Button();
            this.xSlopeLabel = new System.Windows.Forms.Label();
            this.ySlopeLabel = new System.Windows.Forms.Label();
            this.zSlopeLabel = new System.Windows.Forms.Label();
            this.xSlope = new System.Windows.Forms.NumericUpDown();
            this.ySlope = new System.Windows.Forms.NumericUpDown();
            this.zSlope = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.xOffsetLabel = new System.Windows.Forms.Label();
            this.zOffset = new System.Windows.Forms.NumericUpDown();
            this.yOffsetLabel = new System.Windows.Forms.Label();
            this.yOffset = new System.Windows.Forms.NumericUpDown();
            this.zOffsetLabel = new System.Windows.Forms.Label();
            this.xOffset = new System.Windows.Forms.NumericUpDown();
            this.stationNameLabel = new System.Windows.Forms.Label();
            this.totalProgress = new System.Windows.Forms.ProgressBar();
            this.stationName = new System.Windows.Forms.TextBox();
            this.uploadButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.sizeTotalGB = new System.Windows.Forms.Label();
            this.pointsTotal = new System.Windows.Forms.Label();
            this.chunksTotal = new System.Windows.Forms.Label();
            this.startTime = new System.Windows.Forms.Label();
            this.fileName = new System.Windows.Forms.Label();
            this.progressLabel = new System.Windows.Forms.Label();
            this.sizeLabel = new System.Windows.Forms.Label();
            this.pointsLabel = new System.Windows.Forms.Label();
            this.chunksLabel = new System.Windows.Forms.Label();
            this.startLabel = new System.Windows.Forms.Label();
            this.fileNameLabel = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.cancelButton = new System.Windows.Forms.Button();
            this.worker = new System.ComponentModel.BackgroundWorker();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.xSlope)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ySlope)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.zSlope)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.zOffset)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.yOffset)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xOffset)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // browseButton
            // 
            this.browseButton.Location = new System.Drawing.Point(18, 279);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(165, 63);
            this.browseButton.TabIndex = 0;
            this.browseButton.Text = "Browse...";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // xSlopeLabel
            // 
            this.xSlopeLabel.AutoSize = true;
            this.xSlopeLabel.Location = new System.Drawing.Point(20, 45);
            this.xSlopeLabel.Name = "xSlopeLabel";
            this.xSlopeLabel.Size = new System.Drawing.Size(110, 32);
            this.xSlopeLabel.TabIndex = 2;
            this.xSlopeLabel.Text = "X slope";
            // 
            // ySlopeLabel
            // 
            this.ySlopeLabel.AutoSize = true;
            this.ySlopeLabel.Location = new System.Drawing.Point(20, 89);
            this.ySlopeLabel.Name = "ySlopeLabel";
            this.ySlopeLabel.Size = new System.Drawing.Size(110, 32);
            this.ySlopeLabel.TabIndex = 3;
            this.ySlopeLabel.Text = "Y slope";
            // 
            // zSlopeLabel
            // 
            this.zSlopeLabel.AutoSize = true;
            this.zSlopeLabel.Location = new System.Drawing.Point(20, 133);
            this.zSlopeLabel.Name = "zSlopeLabel";
            this.zSlopeLabel.Size = new System.Drawing.Size(108, 32);
            this.zSlopeLabel.TabIndex = 4;
            this.zSlopeLabel.Text = "Z slope";
            // 
            // xSlope
            // 
            this.xSlope.DecimalPlaces = 3;
            this.xSlope.Location = new System.Drawing.Point(147, 43);
            this.xSlope.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.xSlope.Name = "xSlope";
            this.xSlope.Size = new System.Drawing.Size(150, 38);
            this.xSlope.TabIndex = 6;
            this.xSlope.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // ySlope
            // 
            this.ySlope.DecimalPlaces = 3;
            this.ySlope.Location = new System.Drawing.Point(147, 88);
            this.ySlope.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.ySlope.Name = "ySlope";
            this.ySlope.Size = new System.Drawing.Size(150, 38);
            this.ySlope.TabIndex = 7;
            this.ySlope.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // zSlope
            // 
            this.zSlope.DecimalPlaces = 3;
            this.zSlope.Location = new System.Drawing.Point(147, 133);
            this.zSlope.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.zSlope.Name = "zSlope";
            this.zSlope.Size = new System.Drawing.Size(150, 38);
            this.zSlope.TabIndex = 8;
            this.zSlope.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.xOffsetLabel);
            this.groupBox1.Controls.Add(this.zOffset);
            this.groupBox1.Controls.Add(this.yOffsetLabel);
            this.groupBox1.Controls.Add(this.yOffset);
            this.groupBox1.Controls.Add(this.zOffsetLabel);
            this.groupBox1.Controls.Add(this.xOffset);
            this.groupBox1.Controls.Add(this.xSlopeLabel);
            this.groupBox1.Controls.Add(this.zSlope);
            this.groupBox1.Controls.Add(this.ySlopeLabel);
            this.groupBox1.Controls.Add(this.ySlope);
            this.groupBox1.Controls.Add(this.zSlopeLabel);
            this.groupBox1.Controls.Add(this.xSlope);
            this.groupBox1.Location = new System.Drawing.Point(18, 72);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(658, 191);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Calibration";
            // 
            // xOffsetLabel
            // 
            this.xOffsetLabel.AutoSize = true;
            this.xOffsetLabel.Location = new System.Drawing.Point(362, 45);
            this.xOffsetLabel.Name = "xOffsetLabel";
            this.xOffsetLabel.Size = new System.Drawing.Size(111, 32);
            this.xOffsetLabel.TabIndex = 9;
            this.xOffsetLabel.Text = "X offset";
            // 
            // zOffset
            // 
            this.zOffset.DecimalPlaces = 3;
            this.zOffset.Location = new System.Drawing.Point(489, 133);
            this.zOffset.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.zOffset.Name = "zOffset";
            this.zOffset.Size = new System.Drawing.Size(150, 38);
            this.zOffset.TabIndex = 14;
            // 
            // yOffsetLabel
            // 
            this.yOffsetLabel.AutoSize = true;
            this.yOffsetLabel.Location = new System.Drawing.Point(362, 89);
            this.yOffsetLabel.Name = "yOffsetLabel";
            this.yOffsetLabel.Size = new System.Drawing.Size(111, 32);
            this.yOffsetLabel.TabIndex = 10;
            this.yOffsetLabel.Text = "Y offset";
            // 
            // yOffset
            // 
            this.yOffset.DecimalPlaces = 3;
            this.yOffset.Location = new System.Drawing.Point(489, 88);
            this.yOffset.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.yOffset.Name = "yOffset";
            this.yOffset.Size = new System.Drawing.Size(150, 38);
            this.yOffset.TabIndex = 13;
            // 
            // zOffsetLabel
            // 
            this.zOffsetLabel.AutoSize = true;
            this.zOffsetLabel.Location = new System.Drawing.Point(362, 133);
            this.zOffsetLabel.Name = "zOffsetLabel";
            this.zOffsetLabel.Size = new System.Drawing.Size(109, 32);
            this.zOffsetLabel.TabIndex = 11;
            this.zOffsetLabel.Text = "Z offset";
            // 
            // xOffset
            // 
            this.xOffset.DecimalPlaces = 3;
            this.xOffset.Location = new System.Drawing.Point(489, 43);
            this.xOffset.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.xOffset.Name = "xOffset";
            this.xOffset.Size = new System.Drawing.Size(150, 38);
            this.xOffset.TabIndex = 12;
            // 
            // stationNameLabel
            // 
            this.stationNameLabel.AutoSize = true;
            this.stationNameLabel.Location = new System.Drawing.Point(12, 21);
            this.stationNameLabel.Name = "stationNameLabel";
            this.stationNameLabel.Size = new System.Drawing.Size(183, 32);
            this.stationNameLabel.TabIndex = 10;
            this.stationNameLabel.Text = "Station name";
            // 
            // totalProgress
            // 
            this.totalProgress.Location = new System.Drawing.Point(176, 295);
            this.totalProgress.Name = "totalProgress";
            this.totalProgress.Size = new System.Drawing.Size(625, 33);
            this.totalProgress.TabIndex = 11;
            // 
            // stationName
            // 
            this.stationName.Location = new System.Drawing.Point(203, 15);
            this.stationName.Name = "stationName";
            this.stationName.Size = new System.Drawing.Size(390, 38);
            this.stationName.TabIndex = 12;
            this.stationName.TextChanged += new System.EventHandler(this.stationName_TextChanged);
            // 
            // uploadButton
            // 
            this.uploadButton.Location = new System.Drawing.Point(203, 279);
            this.uploadButton.Name = "uploadButton";
            this.uploadButton.Size = new System.Drawing.Size(165, 63);
            this.uploadButton.TabIndex = 13;
            this.uploadButton.Text = "Upload";
            this.uploadButton.UseVisualStyleBackColor = true;
            this.uploadButton.Click += new System.EventHandler(this.uploadButton_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.sizeTotalGB);
            this.groupBox2.Controls.Add(this.pointsTotal);
            this.groupBox2.Controls.Add(this.chunksTotal);
            this.groupBox2.Controls.Add(this.startTime);
            this.groupBox2.Controls.Add(this.fileName);
            this.groupBox2.Controls.Add(this.progressLabel);
            this.groupBox2.Controls.Add(this.sizeLabel);
            this.groupBox2.Controls.Add(this.pointsLabel);
            this.groupBox2.Controls.Add(this.chunksLabel);
            this.groupBox2.Controls.Add(this.startLabel);
            this.groupBox2.Controls.Add(this.fileNameLabel);
            this.groupBox2.Controls.Add(this.totalProgress);
            this.groupBox2.Location = new System.Drawing.Point(27, 362);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(828, 352);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Dataset information";
            // 
            // sizeTotalGB
            // 
            this.sizeTotalGB.AutoSize = true;
            this.sizeTotalGB.Location = new System.Drawing.Point(170, 239);
            this.sizeTotalGB.Name = "sizeTotalGB";
            this.sizeTotalGB.Size = new System.Drawing.Size(39, 32);
            this.sizeTotalGB.TabIndex = 22;
            this.sizeTotalGB.Text = "...";
            // 
            // pointsTotal
            // 
            this.pointsTotal.AutoSize = true;
            this.pointsTotal.Location = new System.Drawing.Point(170, 194);
            this.pointsTotal.Name = "pointsTotal";
            this.pointsTotal.Size = new System.Drawing.Size(39, 32);
            this.pointsTotal.TabIndex = 21;
            this.pointsTotal.Text = "...";
            // 
            // chunksTotal
            // 
            this.chunksTotal.AutoSize = true;
            this.chunksTotal.Location = new System.Drawing.Point(170, 149);
            this.chunksTotal.Name = "chunksTotal";
            this.chunksTotal.Size = new System.Drawing.Size(39, 32);
            this.chunksTotal.TabIndex = 20;
            this.chunksTotal.Text = "...";
            // 
            // startTime
            // 
            this.startTime.AutoSize = true;
            this.startTime.Location = new System.Drawing.Point(170, 104);
            this.startTime.Name = "startTime";
            this.startTime.Size = new System.Drawing.Size(39, 32);
            this.startTime.TabIndex = 19;
            this.startTime.Text = "...";
            // 
            // fileName
            // 
            this.fileName.AutoSize = true;
            this.fileName.Location = new System.Drawing.Point(170, 59);
            this.fileName.Name = "fileName";
            this.fileName.Size = new System.Drawing.Size(39, 32);
            this.fileName.TabIndex = 18;
            this.fileName.Text = "...";
            // 
            // progressLabel
            // 
            this.progressLabel.AutoSize = true;
            this.progressLabel.Location = new System.Drawing.Point(11, 295);
            this.progressLabel.Name = "progressLabel";
            this.progressLabel.Size = new System.Drawing.Size(128, 32);
            this.progressLabel.TabIndex = 17;
            this.progressLabel.Text = "Progress";
            // 
            // sizeLabel
            // 
            this.sizeLabel.AutoSize = true;
            this.sizeLabel.Location = new System.Drawing.Point(11, 239);
            this.sizeLabel.Name = "sizeLabel";
            this.sizeLabel.Size = new System.Drawing.Size(130, 32);
            this.sizeLabel.TabIndex = 16;
            this.sizeLabel.Text = "size [GB]";
            // 
            // pointsLabel
            // 
            this.pointsLabel.AutoSize = true;
            this.pointsLabel.Location = new System.Drawing.Point(11, 194);
            this.pointsLabel.Name = "pointsLabel";
            this.pointsLabel.Size = new System.Drawing.Size(95, 32);
            this.pointsLabel.TabIndex = 15;
            this.pointsLabel.Text = "Points";
            // 
            // chunksLabel
            // 
            this.chunksLabel.AutoSize = true;
            this.chunksLabel.Location = new System.Drawing.Point(11, 149);
            this.chunksLabel.Name = "chunksLabel";
            this.chunksLabel.Size = new System.Drawing.Size(111, 32);
            this.chunksLabel.TabIndex = 14;
            this.chunksLabel.Text = "Chunks";
            // 
            // startLabel
            // 
            this.startLabel.AutoSize = true;
            this.startLabel.Location = new System.Drawing.Point(11, 104);
            this.startLabel.Name = "startLabel";
            this.startLabel.Size = new System.Drawing.Size(75, 32);
            this.startLabel.TabIndex = 13;
            this.startLabel.Text = "Start";
            // 
            // fileNameLabel
            // 
            this.fileNameLabel.AutoSize = true;
            this.fileNameLabel.Location = new System.Drawing.Point(11, 59);
            this.fileNameLabel.Name = "fileNameLabel";
            this.fileNameLabel.Size = new System.Drawing.Size(140, 32);
            this.fileNameLabel.TabIndex = 12;
            this.fileNameLabel.Text = "File name";
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(40, 40);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel,
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 747);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(870, 48);
            this.statusStrip1.TabIndex = 15;
            this.statusStrip1.Text = "statusStrip";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(752, 43);
            this.toolStripStatusLabel.Spring = true;
            this.toolStripStatusLabel.Text = "Ready";
            this.toolStripStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(25, 43);
            this.toolStripStatusLabel1.Text = ".";
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(386, 279);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(165, 63);
            this.cancelButton.TabIndex = 16;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // worker
            // 
            this.worker.WorkerReportsProgress = true;
            this.worker.WorkerSupportsCancellation = true;
            this.worker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.worker_DoWork);
            this.worker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.Worker_ProgressChanged);
            this.worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.Worker_RunWorkerCompleted);
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "time.bin";
            this.openFileDialog.Filter = "Magnetic field data (*.bin)|*.bin";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(870, 795);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.uploadButton);
            this.Controls.Add(this.stationName);
            this.Controls.Add(this.stationNameLabel);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.browseButton);
            this.Name = "MainForm";
            this.Text = "Legacy Uploader";
            ((System.ComponentModel.ISupportInitialize)(this.xSlope)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ySlope)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.zSlope)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.zOffset)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.yOffset)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xOffset)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.Label xSlopeLabel;
        private System.Windows.Forms.Label ySlopeLabel;
        private System.Windows.Forms.Label zSlopeLabel;
        private System.Windows.Forms.NumericUpDown xSlope;
        private System.Windows.Forms.NumericUpDown ySlope;
        private System.Windows.Forms.NumericUpDown zSlope;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label xOffsetLabel;
        private System.Windows.Forms.NumericUpDown zOffset;
        private System.Windows.Forms.Label yOffsetLabel;
        private System.Windows.Forms.NumericUpDown yOffset;
        private System.Windows.Forms.Label zOffsetLabel;
        private System.Windows.Forms.NumericUpDown xOffset;
        private System.Windows.Forms.Label stationNameLabel;
        private System.Windows.Forms.ProgressBar totalProgress;
        private System.Windows.Forms.TextBox stationName;
        private System.Windows.Forms.Button uploadButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label progressLabel;
        private System.Windows.Forms.Label sizeLabel;
        private System.Windows.Forms.Label pointsLabel;
        private System.Windows.Forms.Label chunksLabel;
        private System.Windows.Forms.Label startLabel;
        private System.Windows.Forms.Label fileNameLabel;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.Label sizeTotalGB;
        private System.Windows.Forms.Label pointsTotal;
        private System.Windows.Forms.Label chunksTotal;
        private System.Windows.Forms.Label startTime;
        private System.Windows.Forms.Label fileName;
        private System.Windows.Forms.Button cancelButton;
        private System.ComponentModel.BackgroundWorker worker;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
    }
}

