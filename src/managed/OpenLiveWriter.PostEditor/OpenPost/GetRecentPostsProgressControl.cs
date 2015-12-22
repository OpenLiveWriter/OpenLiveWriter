// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.OpenPost
{

    public class GetRecentPostsProgressControl : UserControl
    {
        private Label labelCaption;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;
        private AnimatedBitmapControl progressAnimatedBitmap;

        private Control _parentControl;

        public GetRecentPostsProgressControl(Control parentControl)
        {
            // save reference to parent control
            _parentControl = parentControl;

            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
        }

        public void Initialize()
        {
            // set animation bitmaps, run animation for 2 seconds
            progressAnimatedBitmap.Bitmaps = AnimationBitmaps;
            progressAnimatedBitmap.Interval = 2000 / AnimationBitmaps.Length;

            // initialize properties
            TabStop = false;
            ForeColor = _parentControl.ForeColor;
            BackColor = _parentControl.BackColor;

            // add to outer context
            _parentControl.Parent.Controls.Add(this);

            // ensure centering
            CenterControlInControlBehavior centerControl = new CenterControlInControlBehavior(this, _parentControl);
        }

        public void Start(bool getPages)
        {
            labelCaption.Text = String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.RetrievingFromWeblog), getPages ? Res.Get(StringId.PagesLower) : Res.Get(StringId.PostsLower));

            if (!progressAnimatedBitmap.IsDisposed)
            {
                if (!progressAnimatedBitmap.Running)
                    progressAnimatedBitmap.Start();
            }

            BringToFront();
        }

        public void Stop()
        {
            if (!progressAnimatedBitmap.IsDisposed)
            {
                if (progressAnimatedBitmap.Running)
                    progressAnimatedBitmap.Stop();
            }

            SendToBack();
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

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelCaption = new System.Windows.Forms.Label();
            this.progressAnimatedBitmap = new OpenLiveWriter.Controls.AnimatedBitmapControl();
            this.SuspendLayout();
            //
            // labelCaption
            //
            this.labelCaption.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelCaption.Location = new System.Drawing.Point(0, 86);
            this.labelCaption.Name = "labelCaption";
            this.labelCaption.Size = new System.Drawing.Size(240, 33);
            this.labelCaption.TabIndex = 0;
            this.labelCaption.Text = "Retrieving posts from weblog...";
            this.labelCaption.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // progressAnimatedBitmap
            //
            this.progressAnimatedBitmap.Bitmaps = null;
            this.progressAnimatedBitmap.Interval = 100;
            this.progressAnimatedBitmap.Location = new System.Drawing.Point(0, 0);
            this.progressAnimatedBitmap.Name = "progressAnimatedBitmap";
            this.progressAnimatedBitmap.Running = false;
            this.progressAnimatedBitmap.Size = new System.Drawing.Size(240, 72);
            this.progressAnimatedBitmap.TabIndex = 3;
            //
            // GetRecentPostsProgressControl
            //
            this.Controls.Add(this.progressAnimatedBitmap);
            this.Controls.Add(this.labelCaption);
            this.Name = "GetRecentPostsProgressControl";
            this.Size = new System.Drawing.Size(240, 128);
            this.ResumeLayout(false);

        }
        #endregion
    }
}
