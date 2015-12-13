// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor
{

    public class RecentPostProgressForm : DelayedAnimatedProgressDialog
    {
        private Label labelRetrievingPost;
        private AnimatedBitmapControl progressAnimatedBitmap;
        private PictureBox pictureBoxSeparator;
        private System.Windows.Forms.Button buttonCancelForm;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public RecentPostProgressForm(string entityName)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.labelRetrievingPost.Text = Res.Get(StringId.RetrievingFromWeblog);
            this.buttonCancelForm.Text = Res.Get(StringId.CancelButton);
            this.Text = Res.Get(StringId.Retrieving);

            Text = String.Format(CultureInfo.CurrentCulture, Text, entityName);
            labelRetrievingPost.Text = String.Format(CultureInfo.CurrentCulture, labelRetrievingPost.Text, entityName.ToLower(CultureInfo.CurrentCulture));

            progressAnimatedBitmap.Bitmaps = AnimationBitmaps;
            progressAnimatedBitmap.Interval = 2000 / AnimationBitmaps.Length;
            SetAnimatatedBitmapControl(progressAnimatedBitmap);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            DisplayHelper.AutoFitSystemButton(buttonCancelForm, buttonCancelForm.Width, int.MaxValue);
            using (new AutoGrow(this, AnchorStyles.Bottom, false))
            {
                LayoutHelper.NaturalizeHeightAndDistribute(8, labelRetrievingPost, buttonCancelForm);
            }
        }

        private void buttonCancelForm_Click(object sender, System.EventArgs e)
        {
            Cancel();
        }

        private Bitmap[] AnimationBitmaps
        {
            get
            {
                if (_animationBitmaps == null)
                {
                    ArrayList list = new ArrayList();
                    for (int i = 0; i < 12; i++)
                    {
                        string resourceName = String.Format(CultureInfo.InvariantCulture, "OpenPost.Images.GetRecentPostsAnimation.GetRecentPostsAnimation{0:00}.png", i);
                        list.Add(ResourceHelper.LoadAssemblyResourceBitmap(resourceName));
                    }
                    _animationBitmaps = (Bitmap[])list.ToArray(typeof(Bitmap));
                }
                return _animationBitmaps;
            }
        }
        private Bitmap[] _animationBitmaps;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
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
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.ClientSize = new System.Drawing.Size(317, 141);
            this.Controls.Add(this.buttonCancelForm);
            this.Controls.Add(this.pictureBoxSeparator);
            this.Controls.Add(this.progressAnimatedBitmap);
            this.Controls.Add(this.labelRetrievingPost);
            this.Name = "RecentPostProgressForm";
            this.Text = "Retrieving {0}";
            this.CancelButton = buttonCancelForm;
            this.ResumeLayout(false);

        }
        #endregion


    }
}
