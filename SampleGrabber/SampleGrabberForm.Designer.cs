namespace DogeStation2
{
    partial class SampleGrabberForm
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
            this.comment = new System.Windows.Forms.TextBox();
            this.commentLabel = new System.Windows.Forms.Label();
            this.discardButton = new System.Windows.Forms.Button();
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
            // dataGraph
            // 
            this.dataGraph.Size = new System.Drawing.Size(1369, 391);
            // 
            // previewBox
            // 
            this.previewBox.Size = new System.Drawing.Size(1392, 518);
            // 
            // gpsCloseButton
            // 
            this.gpsCloseButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DimGray;
            // 
            // gpsOpenButton
            // 
            this.gpsOpenButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DimGray;
            // 
            // gpsList
            // 
            this.gpsList.Size = new System.Drawing.Size(169, 39);
            // 
            // comment
            // 
            this.comment.Location = new System.Drawing.Point(173, 73);
            this.comment.Name = "comment";
            this.comment.Size = new System.Drawing.Size(1248, 38);
            this.comment.TabIndex = 11;
            this.comment.TextChanged += new System.EventHandler(this.comment_TextChanged);
            // 
            // commentLabel
            // 
            this.commentLabel.AutoSize = true;
            this.commentLabel.Location = new System.Drawing.Point(30, 76);
            this.commentLabel.Name = "commentLabel";
            this.commentLabel.Size = new System.Drawing.Size(137, 32);
            this.commentLabel.TabIndex = 10;
            this.commentLabel.Text = "Comment";
            // 
            // discardButton
            // 
            this.discardButton.Location = new System.Drawing.Point(1281, 12);
            this.discardButton.Name = "discardButton";
            this.discardButton.Size = new System.Drawing.Size(140, 53);
            this.discardButton.TabIndex = 17;
            this.discardButton.Text = "Discard";
            this.discardButton.UseVisualStyleBackColor = true;
            this.discardButton.Click += new System.EventHandler(this.discardButton_Click);
            // 
            // SampleGrabberForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.ClientSize = new System.Drawing.Size(1436, 768);
            this.Controls.Add(this.comment);
            this.Controls.Add(this.commentLabel);
            this.Controls.Add(this.discardButton);
            this.Name = "SampleGrabberForm";
            this.Controls.SetChildIndex(this.discardButton, 0);
            this.Controls.SetChildIndex(this.commentLabel, 0);
            this.Controls.SetChildIndex(this.comment, 0);
            this.Controls.SetChildIndex(this.sampleNameLabel, 0);
            this.Controls.SetChildIndex(this.sensorList, 0);
            this.Controls.SetChildIndex(this.recordButton, 0);
            this.Controls.SetChildIndex(this.cancelButton, 0);
            this.Controls.SetChildIndex(this.sensorLabel, 0);
            this.Controls.SetChildIndex(this.refreshButton, 0);
            this.Controls.SetChildIndex(this.name, 0);
            this.Controls.SetChildIndex(this.uploadButton, 0);
            this.Controls.SetChildIndex(this.previewBox, 0);
            this.Controls.SetChildIndex(this.gpsLabel, 0);
            this.Controls.SetChildIndex(this.gpsList, 0);
            this.Controls.SetChildIndex(this.gpsRefreshButton, 0);
            this.Controls.SetChildIndex(this.gpsOpenButton, 0);
            this.Controls.SetChildIndex(this.gpsStatusLabel, 0);
            this.Controls.SetChildIndex(this.gpsCloseButton, 0);
            ((System.ComponentModel.ISupportInitialize)(this.dataGraph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.averagingPeriodMs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.displayPoints)).EndInit();
            this.previewBox.ResumeLayout(false);
            this.previewBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        protected System.Windows.Forms.TextBox comment;
        protected System.Windows.Forms.Label commentLabel;
        protected System.Windows.Forms.Button discardButton;
    }
}
