// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Collections;
using System.Diagnostics;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using System.Threading;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.ContentSources
{
    internal class UrlContentRetreivalWithProgressDialog : ApplicationDialog
    {
        private UrlContentRetreivalAsyncOperation _asyncOperation;
        private OpenLiveWriter.Controls.AnimatedBitmapControl _animatedBitmapControl;
        private System.Windows.Forms.Label labelCaption;
        private System.Windows.Forms.Label labelDetails;
        private System.Windows.Forms.Button buttonCancel;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public UrlContentRetreivalWithProgressDialog(ContentSourceInfo contentSourceInfo)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.buttonCancel.Text = Res.Get(StringId.CancelButton);

            this.labelCaption.Font = Res.GetFont(FontSize.Normal, FontStyle.Bold);
            // initialize text
            Text = contentSourceInfo.Name;
            labelCaption.Text = contentSourceInfo.UrlContentSourceProgressCaption != String.Empty ? contentSourceInfo.UrlContentSourceProgressCaption : Res.Get(StringId.UrlRetrieveProgressCaption);
            labelDetails.Text = contentSourceInfo.UrlContentSourceProgressMessage != String.Empty ? contentSourceInfo.UrlContentSourceProgressMessage : Res.Get(StringId.UrlRetrieveProgressMessage);

            // initialize animated bitmap
            _animatedBitmapControl.Bitmaps = AnimationBitmaps;
            _animatedBitmapControl.Interval = 75;

            Icon = ApplicationEnvironment.ProductIcon;
        }

        public void ShowProgress(IWin32Window owner, UrlContentRetreivalAsyncOperation asyncOperation)
        {
            // got past the delay interval, need to signup for events and show the dialog
            _asyncOperation = asyncOperation;
            _asyncOperation.Completed += new EventHandler(_asyncOperation_Completed);
            _asyncOperation.Cancelled += new EventHandler(_asyncOperation_Cancelled);
            _asyncOperation.Failed += new ThreadExceptionEventHandler(_asyncOperation_Failed);

            // show the dialog
            ShowDialog(owner);
        }

        private void buttonCancel_Click(object sender, System.EventArgs e)
        {
            Cancel();
        }

        protected void Cancel()
        {
            _asyncOperation.Cancel();
            Close();
        }

        private void _asyncOperation_Failed(object sender, ThreadExceptionEventArgs e)
        {
            Close();
        }

        private void _asyncOperation_Completed(object sender, EventArgs e)
        {
            Close();
        }

        private void _asyncOperation_Cancelled(object sender, EventArgs e)
        {
            Close();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            Debug.Assert(_animatedBitmapControl != null);

            if (!_animatedBitmapControl.Running)
                _animatedBitmapControl.Start();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_animatedBitmapControl.Running)
                    _animatedBitmapControl.Stop();

                if (_asyncOperation != null)
                {
                    _asyncOperation.Completed -= new EventHandler(_asyncOperation_Completed);
                    _asyncOperation.Cancelled -= new EventHandler(_asyncOperation_Cancelled);
                    _asyncOperation.Failed -= new ThreadExceptionEventHandler(_asyncOperation_Failed);
                }

                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private Bitmap[] AnimationBitmaps
        {
            get
            {
                if (_animationBitmaps == null)
                {
                    ArrayList list = new ArrayList();
                    for (int i = 1; i <= 22; i++)
                    {
                        string resourceName = String.Format(CultureInfo.InvariantCulture, "ContentSources.Images.NetworkProgress.Progress{0:00}.png", i);
                        list.Add(ResourceHelper.LoadAssemblyResourceBitmap(resourceName));
                    }
                    _animationBitmaps = (Bitmap[])list.ToArray(typeof(Bitmap));
                }
                return _animationBitmaps;
            }
        }
        private Bitmap[] _animationBitmaps;

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._animatedBitmapControl = new OpenLiveWriter.Controls.AnimatedBitmapControl();
            this.labelCaption = new System.Windows.Forms.Label();
            this.labelDetails = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // _animatedBitmapControl
            //
            this._animatedBitmapControl.Bitmaps = null;
            this._animatedBitmapControl.Interval = 100;
            this._animatedBitmapControl.Location = new System.Drawing.Point(192, 56);
            this._animatedBitmapControl.Name = "_animatedBitmapControl";
            this._animatedBitmapControl.Running = false;
            this._animatedBitmapControl.Size = new System.Drawing.Size(123, 32);
            this._animatedBitmapControl.TabIndex = 5;
            //
            // labelCaption
            //
            this.labelCaption.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelCaption.Font = Res.GetFont(FontSize.Normal, FontStyle.Bold);
            this.labelCaption.Location = new System.Drawing.Point(8, 16);
            this.labelCaption.Name = "labelCaption";
            this.labelCaption.Size = new System.Drawing.Size(312, 23);
            this.labelCaption.TabIndex = 6;
            this.labelCaption.Text = "Downloading Video Details from YouTube";
            //
            // labelDetails
            //
            this.labelDetails.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelDetails.Location = new System.Drawing.Point(8, 56);
            this.labelDetails.Name = "labelDetails";
            this.labelDetails.Size = new System.Drawing.Size(168, 56);
            this.labelDetails.TabIndex = 7;
            this.labelDetails.Text = "Downloading additional details for video \"Harlem\'s last stand\"...";
            //
            // buttonCancel
            //
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCancel.Location = new System.Drawing.Point(240, 120);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.TabIndex = 8;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            //
            // UrlContentRetreivalWithProgressDialog
            //
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.ClientSize = new System.Drawing.Size(330, 152);
            this.ControlBox = false;
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.labelDetails);
            this.Controls.Add(this.labelCaption);
            this.Controls.Add(this._animatedBitmapControl);
            this.Location = new System.Drawing.Point(0, 0);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UrlContentRetreivalWithProgressDialog";
            this.Text = "YouTube Video";
            this.ResumeLayout(false);

        }
        #endregion

    }
}
