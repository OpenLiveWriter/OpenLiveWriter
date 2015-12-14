// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.PostEditor.Video.VideoService
{
    public class VideoPagingControl : UserControl, IRtlAware
    {
        private Label labelQueryStatus;
        private OpenLiveWriter.Controls.XPBitmapButton buttonPreviousPage;
        private OpenLiveWriter.Controls.XPBitmapButton buttonNextPage;
        private readonly Bitmap _previousDisabledImage;
        private readonly Bitmap _previousEnabledImage;
        private readonly Bitmap _nextDisabledImage;
        private readonly Bitmap _nextEnabledImage;
        private int _currentPage = 1;

        public VideoPagingControl()
        {
            InitializeComponent();

            _previousDisabledImage = new Bitmap(GetType(), "Images.PreviousVideosDisabled.png");
            _previousEnabledImage = new Bitmap(GetType(), "Images.PreviousVideosEnabled.png");
            _nextDisabledImage = new Bitmap(GetType(), "Images.NextVideosDisabled.png");
            _nextEnabledImage = new Bitmap(GetType(), "Images.NextVideosEnabled.png");

            buttonNextPage.Initialize(_nextEnabledImage, _nextDisabledImage);
            buttonPreviousPage.Initialize(_previousEnabledImage, _previousDisabledImage);

            buttonPreviousPage.Click += buttonPreviousPage_Click;
            buttonNextPage.Click += buttonNextPage_Click;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // We do this just to reverse the anchor directions of the children
            BidiHelper.RtlLayoutFixup(this, true, true, Controls);

            UpdateQueryStatus();
        }

        public int NumberOfVideosPerPage
        {
            get
            {
                return NUMBEROFVIDEOSPERPAGE;
            }
        }

        private const int NUMBEROFVIDEOSPERPAGE = 10;

        public int NumberOfVideos
        {
            get
            {
                return _numberOfVideos;
            }
            set
            {
                _numberOfVideos = value;
                UpdateQueryStatus();
            }
        }

        public void Reset()
        {
            NumberOfVideos = 0;
            _currentPage = 1;
        }

        private int _numberOfVideos = -1;

        public int Pages
        {
            get
            {
                return _pages;
            }
            set
            {
                _pages = value;
            }
        }

        private int _pages;

        public int CurrentPage
        {
            get
            {
                return _currentPage;
            }
        }

        public event EventHandler RefreshRequested;

        protected virtual void OnRefreshVideos()
        {
            if (RefreshRequested != null)
                RefreshRequested(this, EventArgs.Empty);
        }

        private void buttonPreviousPage_Click(object sender, EventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                OnRefreshVideos();
            }
            else
            {
                Trace.Fail("Invalid state: previous button enabled for first page");
            }
        }

        private void buttonNextPage_Click(object sender, EventArgs e)
        {
            _currentPage++;
            OnRefreshVideos();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _nextDisabledImage.Dispose();
                _nextEnabledImage.Dispose();
                _previousDisabledImage.Dispose();
                _previousEnabledImage.Dispose();

            }
            base.Dispose(disposing);
        }

        private void RefreshLayout()
        {
            //DisplayHelper.AutoFitSystemLabel(labelQueryStatus, int.MinValue, int.MaxValue);
            //Width = buttonPreviousPage.Width + buttonNextPage.Width + labelQueryStatus.Width + 3*PADDING_X;

            buttonNextPage.Top = 0;
            buttonPreviousPage.Top = 0;
            if (BidiHelper.IsRightToLeft)
            {
                DisplayHelper.AutoFitSystemLabel(labelQueryStatus, int.MinValue, int.MaxValue);
                buttonNextPage.Left = 0;
                buttonPreviousPage.Left = buttonNextPage.Right + PADDING_X;
                labelQueryStatus.Left = buttonPreviousPage.Right + PADDING_X;

            }
            else
            {
                buttonNextPage.Left = Width - buttonNextPage.Width;
                buttonPreviousPage.Left = buttonNextPage.Left - buttonPreviousPage.Width - PADDING_X;
                labelQueryStatus.Left = buttonPreviousPage.Left - labelQueryStatus.Width - PADDING_X;
                labelQueryStatus.TextAlign = ContentAlignment.MiddleRight;
            }

        }

        private static readonly int PADDING_X = 2;

        private void UpdateQueryStatus()
        {
            // we got no videos
            if (_numberOfVideos < 1)
            {
                labelQueryStatus.Text = "";
                buttonPreviousPage.Enabled = false;
                buttonNextPage.Enabled = false;
            }
            // we got all of the videos
            else if (_numberOfVideos < NUMBEROFVIDEOSPERPAGE)
            {
                labelQueryStatus.Text = FormatVideoString(1, _numberOfVideos) + " " + string.Format(CultureInfo.CurrentCulture, Res.Get(StringId.Plugin_Video_Soapbox_Total_Videos), _numberOfVideos);
                buttonPreviousPage.Enabled = false;
                buttonNextPage.Enabled = false;
            }
            // we didn't get all of the videos
            else
            {
                // calculate indexes and status text
                int beginIndex = ((_currentPage - 1) * NUMBEROFVIDEOSPERPAGE) + 1;
                int endIndex = Math.Min((beginIndex + NUMBEROFVIDEOSPERPAGE) - 1, _numberOfVideos);
                string statusText = FormatVideoString(beginIndex, endIndex) + " " + String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.Plugin_Video_Soapbox_Total_Videos), _numberOfVideos); ;

                // set status text

                labelQueryStatus.Visible = true;
                labelQueryStatus.Text = statusText;

                // determine enabled/disabled state of next/prev buttons
                buttonPreviousPage.Visible = true;
                buttonPreviousPage.Enabled = _currentPage > 1;
                buttonNextPage.Enabled = _numberOfVideos > endIndex;
            }
            RefreshLayout();
        }

        private static string FormatVideoString(int beginIndex, int endIndex)
        {
            if (beginIndex != endIndex)
                return String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.Plugin_Video_Soapbox_Result_Range), beginIndex, endIndex);
            else
                return String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.Plugin_Video_Soapbox_Result_Single), beginIndex);
        }

        private void InitializeComponent()
        {
            this.labelQueryStatus = new System.Windows.Forms.Label();
            this.buttonPreviousPage = new OpenLiveWriter.Controls.XPBitmapButton();
            this.buttonNextPage = new OpenLiveWriter.Controls.XPBitmapButton();
            this.SuspendLayout();
            //
            // labelQueryStatus
            //
            this.labelQueryStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelQueryStatus.BackColor = System.Drawing.SystemColors.Control;
            this.labelQueryStatus.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelQueryStatus.Location = new System.Drawing.Point(3, 1);
            this.labelQueryStatus.Name = "labelQueryStatus";
            this.labelQueryStatus.Size = new System.Drawing.Size(192, 18);
            this.labelQueryStatus.TabIndex = 9;
            this.labelQueryStatus.Text = "11 to 20 (of 121)";
            //
            // buttonPreviousPage
            //
            this.buttonPreviousPage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPreviousPage.ForeColor = System.Drawing.SystemColors.ControlText;
            this.buttonPreviousPage.Location = new System.Drawing.Point(201, -1);
            this.buttonPreviousPage.Name = "buttonPreviousPage";
            this.buttonPreviousPage.Size = new System.Drawing.Size(23, 21);
            this.buttonPreviousPage.TabIndex = 10;
            //
            // buttonNextPage
            //
            this.buttonNextPage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonNextPage.ForeColor = System.Drawing.SystemColors.ControlText;
            this.buttonNextPage.Location = new System.Drawing.Point(224, -1);
            this.buttonNextPage.Name = "buttonNextPage";
            this.buttonNextPage.Size = new System.Drawing.Size(23, 21);
            this.buttonNextPage.TabIndex = 11;
            //
            // VideoPagingControl
            //
            this.Controls.Add(this.labelQueryStatus);
            this.Controls.Add(this.buttonPreviousPage);
            this.Controls.Add(this.buttonNextPage);
            this.Name = "VideoPagingControl";
            this.Size = new System.Drawing.Size(250, 20);
            this.ResumeLayout(false);

        }

        void IRtlAware.Layout()
        {
        }
    }
}
