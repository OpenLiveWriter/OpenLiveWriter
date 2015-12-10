using System.Windows.Forms;
using OpenLiveWriter.Controls;

namespace OpenLiveWriter.PostEditor.Video
{
    partial class VideoPublishProgressForm
    {
        private Label labelRetrievingPost;
        private AnimatedBitmapControl progressAnimatedBitmap;
        private PictureBox pictureBoxSeparator;
        private System.Windows.Forms.Button buttonCancelForm;
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(RecentPostProgressForm));
            this.labelRetrievingPost = new System.Windows.Forms.Label();
            this.progressAnimatedBitmap = new OpenLiveWriter.Controls.AnimatedBitmapControl();
            this.pictureBoxSeparator = new System.Windows.Forms.PictureBox();
            this.buttonCancelForm = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // labelRetrievingPost
            //
            this.labelRetrievingPost.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.labelRetrievingPost.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelRetrievingPost.Location = new System.Drawing.Point(10, 91);
            this.labelRetrievingPost.Name = "labelRetrievingPost";
            this.labelRetrievingPost.Size = new System.Drawing.Size(295, 17);
            this.labelRetrievingPost.TabIndex = 1;
            this.labelRetrievingPost.Text = "Retrieving {0} from weblog...";
            //
            // progressAnimatedBitmap
            //
            this.progressAnimatedBitmap.Bitmaps = null;
            this.progressAnimatedBitmap.Interval = 100;
            this.progressAnimatedBitmap.Location = new System.Drawing.Point(38, 3);
            this.progressAnimatedBitmap.Name = "progressAnimatedBitmap";
            this.progressAnimatedBitmap.Running = false;
            this.progressAnimatedBitmap.Size = new System.Drawing.Size(240, 72);
            this.progressAnimatedBitmap.TabIndex = 4;
            this.progressAnimatedBitmap.UseVirtualTransparency = false;
            //
            // pictureBoxSeparator
            //
            this.pictureBoxSeparator.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxSeparator.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxSeparator.Image")));
            this.pictureBoxSeparator.Location = new System.Drawing.Point(10, 84);
            this.pictureBoxSeparator.Name = "pictureBoxSeparator";
            this.pictureBoxSeparator.Size = new System.Drawing.Size(293, 3);
            this.pictureBoxSeparator.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBoxSeparator.TabIndex = 5;
            this.pictureBoxSeparator.TabStop = false;
            //
            // buttonCancelForm
            //
            this.buttonCancelForm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancelForm.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCancelForm.Location = new System.Drawing.Point(228, 112);
            this.buttonCancelForm.Name = "buttonCancelForm";
            this.buttonCancelForm.TabIndex = 6;
            this.buttonCancelForm.Text = "Cancel";
            this.buttonCancelForm.Click += new System.EventHandler(this.buttonCancelForm_Click);
            //
            // RecentPostProgressForm
            //
            this.CancelButton = buttonCancelForm;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.ClientSize = new System.Drawing.Size(317, 141);
            this.Controls.Add(this.buttonCancelForm);
            this.Controls.Add(this.pictureBoxSeparator);
            this.Controls.Add(this.progressAnimatedBitmap);
            this.Controls.Add(this.labelRetrievingPost);
            this.Name = "RecentPostProgressForm";
            this.Text = "Retrieving {0}";
            this.ResumeLayout(false);

        }

        #endregion
    }
}
