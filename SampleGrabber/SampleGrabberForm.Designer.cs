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
            this.components = new System.ComponentModel.Container();
            this.comment = new System.Windows.Forms.TextBox();
            this.commentLabel = new System.Windows.Forms.Label();
            // 
            // comment
            // 
            this.comment.Location = new System.Drawing.Point(173, 74);
            this.comment.Name = "comment";
            this.comment.Size = new System.Drawing.Size(1248, 38);
            this.comment.TabIndex = 11;
            this.comment.TextChanged += new System.EventHandler(this.comment_TextChanged);
            // 
            // commentLabel
            // 
            this.commentLabel.AutoSize = true;
            this.commentLabel.Location = new System.Drawing.Point(30, 77);
            this.commentLabel.Name = "commentLabel";
            this.commentLabel.Size = new System.Drawing.Size(137, 32);
            this.commentLabel.TabIndex = 10;
            this.commentLabel.Text = "Comment";
            //
            // SimpleGrabberForm
            //
            this.Controls.Add(this.comment);
            this.Controls.Add(this.commentLabel);
        }

        #endregion
        protected System.Windows.Forms.TextBox comment;
        protected System.Windows.Forms.Label commentLabel;
    }
}
