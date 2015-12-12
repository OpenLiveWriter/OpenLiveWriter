// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.CoreServices.Marketization;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Video
{
    /// <summary>
    /// Summary description for VideoBrowserForm.
    /// </summary>
    public class VideoBrowserForm : ApplicationDialog
    {
        private IContainer components = null;

        //top tab control
        private LightweightControlContainerControl mainTabControl;
        private TabLightweightControl tabs;

        //video source info for non local file images
        private ArrayList _videoSources;
        private VideoSource activeVideoSource = null;

        private Button btnInsert;
        private LinkLabel copyrightLinkLabel;
        private Button btnCancel;

        public VideoBrowserForm(ArrayList videoSources, string blogID, int selectedTab)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            _videoSources = videoSources;

            //set strings
            btnInsert.Text = Res.Get(StringId.InsertButtonText);
            btnCancel.Text = Res.Get(StringId.CancelButton);
            Text = Res.Get(StringId.Plugin_Videos_Select_Video_Form);

            if (!MarketizationOptions.IsFeatureEnabled(MarketizationOptions.Feature.VideoCopyright))
            {
                copyrightLinkLabel.Visible = false;
            }
            else
            {
                copyrightLinkLabel.Font = Res.GetFont(FontSize.Small, FontStyle.Regular);
                copyrightLinkLabel.Text = Res.Get(StringId.Plugin_Video_Copyright_Notice);
                string link = MarketizationOptions.GetFeatureParameter(MarketizationOptions.Feature.VideoCopyright, "Glink");
                if (link == null || link == String.Empty)
                    copyrightLinkLabel.LinkArea = new LinkArea(0,0);
                else
                    copyrightLinkLabel.LinkClicked += copyrightLinkLabel_LinkClicked;
            }

            copyrightLinkLabel.LinkColor = SystemColors.HotTrack;

            //
            // tabs
            //
            tabs = new TabLightweightControl();
            tabs.VirtualBounds = new Rectangle(0, 5, 450, 485);
            tabs.LightweightControlContainerControl = mainTabControl;
            tabs.DrawSideAndBottomTabPageBorders = false;
            tabs.ColorizeBorder = false;

            int i = 0;
            foreach (VideoSource videoSource in _videoSources)
            {
                videoSource.BlogId = blogID;
                videoSource.Init();
                videoSource.VideoSelected += new EventHandler(videoSource_VideoSelected);
                TabPageControl tab = (TabPageControl)videoSource;
                tab.TabStop = false;
                Controls.Add(tab);
                tabs.SetTab(i++, tab);
            }

            // initial appearance of editor

            SetActiveTab(selectedTab);
            tabs.SelectedTabNumber = selectedTab;

            tabs.SelectedTabNumberChanged += new EventHandler(tabs_SelectedTabNumberChanged);

            if ( !DesignMode )
                Icon = ApplicationEnvironment.ProductIcon ;

            tabs.VirtualLocation = new Point(0, 5);
            tabs.VirtualSize = Size;

            Width = 510;
            Height = 570;
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout (levent);

            if (tabs != null && mainTabControl != null)
                tabs.VirtualSize = new Size(mainTabControl.Width, mainTabControl.Height - 5);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad (e);
            LayoutHelper.FixupOKCancel(btnInsert, btnCancel);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            tabs.CheckForTabSwitch(keyData);
            return base.ProcessCmdKey(ref msg, keyData);
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
                foreach(VideoSource source in _videoSources)
                {
                    source.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.mainTabControl = new OpenLiveWriter.Controls.LightweightControlContainerControl();
            this.btnInsert = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.copyrightLinkLabel = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.mainTabControl)).BeginInit();
            this.SuspendLayout();
            //
            // mainTabControl
            //
            this.mainTabControl.AllowDragDropAutoScroll = false;
            this.mainTabControl.AllPaintingInWmPaint = true;
            this.mainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.mainTabControl.BackColor = System.Drawing.SystemColors.Control;
            this.mainTabControl.Location = new System.Drawing.Point(0, 0);
            this.mainTabControl.Name = "mainTabControl";
            this.mainTabControl.Size = new System.Drawing.Size(450, 485);
            this.mainTabControl.TabIndex = 14;
            //
            // btnInsert
            //
            this.btnInsert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnInsert.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnInsert.Location = new System.Drawing.Point(288, 490);
            this.btnInsert.Name = "buttonInsert";
            this.btnInsert.TabIndex = 20;
            this.btnInsert.Text = "Insert";
            this.btnInsert.Click += new System.EventHandler(this.btnInsert_Click);
            //
            // btnCancel
            //
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnCancel.Location = new System.Drawing.Point(368, 490);
            this.btnCancel.Name = "buttonCancel";
            this.btnCancel.TabIndex = 21;
            this.btnCancel.Text = "Cancel";
            //
            // copyrightLinkLabel
            //
            this.copyrightLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.copyrightLinkLabel.AutoSize = true;
            this.copyrightLinkLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.copyrightLinkLabel.Location = new System.Drawing.Point(6, 496);
            this.copyrightLinkLabel.Name = "copyrightLinkLabel";
            this.copyrightLinkLabel.Size = new System.Drawing.Size(208, 18);
            this.copyrightLinkLabel.TabIndex = 19;
            this.copyrightLinkLabel.TabStop = true;
            this.copyrightLinkLabel.Text = "Please Respect Copyright";
            //
            // VideoBrowserForm
            //
            this.AcceptButton = this.btnInsert;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(450, 520);
            this.Controls.Add(this.copyrightLinkLabel);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnInsert);
            this.Controls.Add(this.mainTabControl);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VideoBrowserForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Insert Video";
            ((System.ComponentModel.ISupportInitialize)(this.mainTabControl)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        private void SetActiveTab(int num)
        {
            activeVideoSource = (VideoSource)_videoSources[num];
            activeVideoSource.TabSelected();
        }

        private void tabs_SelectedTabNumberChanged(object sender, EventArgs e)
        {
            int tabChosen = ((TabLightweightControl)sender).SelectedTabNumber;
            SetActiveTab(tabChosen);
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            if (activeVideoSource.ValidateSelection())
            {
                DialogResult = DialogResult.OK;
            }
        }

        public Video SelectedVideo
        {
            get
            {
                return activeVideoSource.GetVideoForInsert();
            }
        }

        private void videoSource_VideoSelected(object sender, EventArgs e)
        {
            if (activeVideoSource.ValidateSelection())
            {
                DialogResult = DialogResult.OK;
            }
        }

        private void copyrightLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string link = MarketizationOptions.GetFeatureParameter(MarketizationOptions.Feature.VideoCopyright, "Glink");
            if (link != null && link != String.Empty)
            {
                try
                {
                    ShellHelper.LaunchUrl( link );
                }
                catch( Exception ex )
                {
                    Trace.Fail( "Unexpected exception navigating to copyright page: " + link + ", " + ex.ToString() ) ;
                }
            }
        }
    }
}
