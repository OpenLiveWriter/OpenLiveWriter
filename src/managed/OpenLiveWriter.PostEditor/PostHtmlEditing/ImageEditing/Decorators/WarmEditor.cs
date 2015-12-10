// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.ImageEditing;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{
    public class WarmEditor : ImageDecoratorEditor, ISupportsRightAndLeftArrows
    {
        private TrackBar trackBarWarmth;
        private Label label1;
        private Label label2;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public WarmEditor()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            this.label1.Text = Res.Get(StringId.ColorTempCooler);
            this.label2.Text = Res.Get(StringId.ColorTempWarmer);
        }

        protected override void LoadEditor()
        {
            base.LoadEditor ();
            WarmSettings = new WarmDecorator.WarmDecoratorSettings(Settings);

            this.trackBarWarmth.Value = (int) (WarmSettings.WarmthPosition * 500f);
            trackBarWarmth.SmallChange = trackBarWarmth.Maximum/20;
            trackBarWarmth.LargeChange = trackBarWarmth.Maximum / 5;

            BidiHelper.RtlLayoutFixup(this);

            label1.Top = label2.Top = trackBarWarmth.Bottom ;
            AccessibleName = ControlHelper.ToAccessibleName(Res.Get(StringId.DecoratorColorTemp));
            trackBarWarmth.AccessibleName = ControlHelper.ToAccessibleName(Res.Get(StringId.DecoratorColorTemp));
        }

        private WarmDecorator.WarmDecoratorSettings WarmSettings;

        protected override void OnSaveSettings()
        {
            base.OnSaveSettings();
            float newWarmth = trackBarWarmth.Value/500f;
            if(WarmSettings.WarmthPosition != newWarmth)
            {
                WarmSettings.WarmthPosition = newWarmth;
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if(components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        #region Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.trackBarWarmth = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarWarmth)).BeginInit();
            this.SuspendLayout();
            //
            // trackBarWarmth
            //
            this.trackBarWarmth.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBarWarmth.Location = new System.Drawing.Point(0, 16);
            this.trackBarWarmth.Maximum = 100;
            this.trackBarWarmth.Minimum = -100;
            this.trackBarWarmth.Name = "trackBarWarmth";
            this.trackBarWarmth.Size = new System.Drawing.Size(215, 45);
            this.trackBarWarmth.TabIndex = 1;
            this.trackBarWarmth.TickFrequency = 50;
            this.trackBarWarmth.KeyUp += new System.Windows.Forms.KeyEventHandler(this.trackBarWarmth_KeyUp);
            this.trackBarWarmth.ValueChanged += new System.EventHandler(this.trackBarWarmth_ValueChanged);
            //
            // label1
            //
            this.label1.Location = new System.Drawing.Point(0, 61);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 43);
            this.label1.TabIndex = 2;
            this.label1.Text = "Cooler";
            //
            // label2
            //
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.Location = new System.Drawing.Point(120, 61);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 43);
            this.label2.TabIndex = 3;
            this.label2.Text = "Warmer";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            //
            // WarmEditor
            //
            this.Controls.Add(this.trackBarWarmth);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Name = "WarmEditor";
            this.RightToLeft = BidiHelper.IsRightToLeft ? RightToLeft.Yes : RightToLeft.No;
            this.Size = new System.Drawing.Size(216, 160);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarWarmth)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        private void trackBarWarmth_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Left)
            {
                trackBarWarmth.Value =
                    Math.Max(trackBarWarmth.Value - trackBarWarmth.SmallChange, trackBarWarmth.Minimum);
            }
        }

        private void trackBarWarmth_ValueChanged(object sender, EventArgs e)
        {
            SaveSettingsAndApplyDecorator();
        }

    }
}
