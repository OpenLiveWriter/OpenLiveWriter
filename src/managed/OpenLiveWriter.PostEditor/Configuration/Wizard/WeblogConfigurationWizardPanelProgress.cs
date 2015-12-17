// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Controls.Wizard;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Configuration.Wizard
{
    public class WeblogConfigurationWizardPanelProgress : WeblogConfigurationWizardPanel
    {
        private OpenLiveWriter.Controls.AnimatedBitmapControl progressAnimatedBitmap;
        private ProgressBar progressBar;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private System.Windows.Forms.Label labelProgress;
        private IProgressProvider _progressProvider;

        public WeblogConfigurationWizardPanelProgress()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            labelHeader.Text = Res.Get(StringId.CWProgressHeader);
            // This will flip the direction of progress bar if we are RTL
            progressBar.RightToLeftLayout = true;
        }

        public override void NaturalizeLayout()
        {
            MaximizeWidth(progressBar);
            MaximizeWidth(labelProgress);
            progressBar.Top = labelProgress.Bottom + Convert.ToInt32(DisplayHelper.ScaleY(10));
            if (progressAnimatedBitmap.Bitmaps.Length > 0)
            {
                progressAnimatedBitmap.Size = progressAnimatedBitmap.Bitmaps[0].Size;
                progressAnimatedBitmap.Left = progressBar.Left + (progressBar.Width - progressAnimatedBitmap.Width) / 2;
            }
        }

        protected void Init(int animationTimeMs, Bitmap[] animationBitmaps)
        {
            progressAnimatedBitmap.Bitmaps = animationBitmaps;
            progressAnimatedBitmap.Interval = animationTimeMs / animationBitmaps.Length;
        }

        public void Start(IProgressProvider progressProvider, WizardController wizard)
        {
            // save reference
            _progressProvider = progressProvider;

            // reset the progress UR and start the animation
            labelProgress.Text = String.Empty;
            progressBar.Value = 0;
            progressAnimatedBitmap.Start();

            // subscribe to progress events
            _progressProvider.ProgressUpdated += new ProgressUpdatedEventHandler(_progressProvider_ProgressUpdated);

            // focus the back button
            BeginInvoke(new InvokeInUIThreadDelegate(wizard.FocusBackButton));
        }

        public void Stop()
        {
            if (_progressProvider != null)
            {
                _progressProvider.ProgressUpdated -= new ProgressUpdatedEventHandler(_progressProvider_ProgressUpdated);
                _progressProvider = null;
            }

            if (progressAnimatedBitmap.Running)
                progressAnimatedBitmap.Stop();
        }

        private void _progressProvider_ProgressUpdated(object sender, ProgressUpdatedEventArgs progressUpdatedHandler)
        {
            if (progressUpdatedHandler.ProgressMessage != "")
            {
                labelProgress.Text = progressUpdatedHandler.ProgressMessage;
                panelMain.Invalidate(labelProgress.Bounds);
            }
            progressBar.Maximum = progressUpdatedHandler.Total;
            progressBar.Value = progressUpdatedHandler.Completed;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();

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
            this.progressAnimatedBitmap = new OpenLiveWriter.Controls.AnimatedBitmapControl();
            this.progressBar = new ProgressBar();
            this.labelProgress = new System.Windows.Forms.Label();
            this.panelMain.SuspendLayout();
            this.SuspendLayout();
            //
            // panelMain
            //
            this.panelMain.Controls.Add(this.labelProgress);
            this.panelMain.Controls.Add(this.progressBar);
            this.panelMain.Controls.Add(this.progressAnimatedBitmap);
            this.panelMain.Location = new System.Drawing.Point(48, 8);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(352, 224);
            //
            // progressAnimatedBitmap
            //
            this.progressAnimatedBitmap.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.progressAnimatedBitmap.Bitmaps = null;
            this.progressAnimatedBitmap.Interval = 100;
            this.progressAnimatedBitmap.Location = new System.Drawing.Point(16, 16);
            this.progressAnimatedBitmap.Name = "progressAnimatedBitmap";
            this.progressAnimatedBitmap.Running = false;
            this.progressAnimatedBitmap.Size = new System.Drawing.Size(320, 88);
            this.progressAnimatedBitmap.TabIndex = 3;
            this.progressAnimatedBitmap.UseVirtualTransparency = false;
            //
            // progressBar
            //
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(20, 144);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(352, 15);
            this.progressBar.TabIndex = 4;
            //
            // labelProgress
            //
            this.labelProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.labelProgress.FlatStyle = FlatStyle.Standard;
            this.labelProgress.Location = new System.Drawing.Point(17, 128);
            this.labelProgress.Name = "labelProgress";
            this.labelProgress.Size = new System.Drawing.Size(352, 16);
            this.labelProgress.TabIndex = 5;
            this.labelProgress.Text = "Detecting weblog settings...";
            this.labelProgress.UseCompatibleTextRendering = false;
            this.labelProgress.AutoSize = false;
            this.labelProgress.AutoEllipsis = true;
            this.labelProgress.TextAlign = ContentAlignment.TopLeft;
            //
            // WeblogConfigurationWizardPanelProgress
            //
            this.Name = "WeblogConfigurationWizardPanelProgress";
            this.Size = new System.Drawing.Size(432, 244);
            this.panelMain.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        #endregion
    }
}
