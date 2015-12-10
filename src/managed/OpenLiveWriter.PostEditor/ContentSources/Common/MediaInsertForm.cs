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
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.CoreServices.Marketization;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.PostEditor.ContentSources.Common;
using System.Collections.Generic;

namespace OpenLiveWriter.PostEditor.ContentSources.Common
{
    /// <summary>
    /// Summary description for VideoBrowserForm.
    /// </summary>
    public class MediaInsertForm : ApplicationDialog, IRtlAware
    {
        private IContainer components = null;

        //top tab control
        private LightweightControlContainerControl mainTabControl;
        private TabLightweightControl tabs;

        private List<MediaTab> _sources;
        private MediaTab activeSource = null;

        private Button buttonInsert;
        private LinkLabel copyrightLinkLabel;
        private Button btnCancel;

        public const string EventName = "Inserting Media";

        public MediaInsertForm(List<MediaTab> sources, string blogID, int selectedTab, MediaSmartContent content, string title, bool showCopyright) :
    this(sources, blogID, selectedTab, content, title, showCopyright, false)
        {
        }

        public MediaInsertForm(List<MediaTab> sources, string blogID, int selectedTab, MediaSmartContent content, string title, bool showCopyright, bool isEdit)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            _sources = sources;

            //set strings
            btnCancel.Text = Res.Get(StringId.CancelButton);
            if (!isEdit)
                buttonInsert.Text = Res.Get(StringId.InsertButtonText);
            else
                buttonInsert.Text = Res.Get(StringId.OKButtonText);

            Text = title;

            if (!showCopyright || !MarketizationOptions.IsFeatureEnabled(MarketizationOptions.Feature.VideoCopyright))
            {
                copyrightLinkLabel.Visible = false;
            }
            else
            {
                copyrightLinkLabel.Font = Res.GetFont(FontSize.Small, FontStyle.Regular);
                copyrightLinkLabel.Text = Res.Get(StringId.Plugin_Video_Copyright_Notice);
                string link = MarketizationOptions.GetFeatureParameter(MarketizationOptions.Feature.VideoCopyright, "Glink");
                if (link == null || link == String.Empty)
                    copyrightLinkLabel.LinkArea = new LinkArea(0, 0);
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
            foreach (MediaTab mediaSource in _sources)
            {
                mediaSource.MediaSelected += videoSource_MediaSelected;
                TabPageControl tab = mediaSource;
                tab.TabStop = false;
                Controls.Add(tab);
                tabs.SetTab(i++, tab);
            }

            // initial appearance of editor

            if (!DesignMode)
                Icon = ApplicationEnvironment.ProductIcon;

            tabs.VirtualLocation = new Point(0, 5);
            tabs.VirtualSize = Size;

            Width = 510;
            Height = 570;

            foreach (MediaTab videoSource in _sources)
                videoSource.Init(content, blogID);

            SetActiveTab(selectedTab);
            tabs.SelectedTabNumber = selectedTab;

            tabs.SelectedTabNumberChanged += new EventHandler(tabs_SelectedTabNumberChanged);

            Closing += new CancelEventHandler(MediaInsertForm_Closing);

        }

        void MediaInsertForm_Closing(object sender, CancelEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                if (activeSource.ValidateSelection())
                {
                    ApplicationPerformance.StartEvent(EventName);
                    DialogResult = DialogResult.OK;
                }
                else
                {
                    DialogResult = DialogResult.Abort;
                    e.Cancel = true;
                }
            }
        }

        void videoSource_MediaSelected(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);

            if (tabs != null && mainTabControl != null)
                tabs.VirtualSize = new Size(mainTabControl.Width, mainTabControl.Height - 5);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LayoutHelper.FixupOKCancel(buttonInsert, btnCancel);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            tabs.CheckForTabSwitch(keyData);
            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

                copyrightLinkLabel.LinkClicked -= copyrightLinkLabel_LinkClicked;
                foreach (MediaTab videoSource in _sources)
                    videoSource.MediaSelected -= videoSource_MediaSelected;
                tabs.SelectedTabNumberChanged -= tabs_SelectedTabNumberChanged;
                Closing -= MediaInsertForm_Closing;

                if (components != null)
                {
                    components.Dispose();
                }
                foreach (MediaTab source in _sources)
                {
                    source.Dispose();
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
            this.mainTabControl = new OpenLiveWriter.Controls.LightweightControlContainerControl();
            this.buttonInsert = new System.Windows.Forms.Button();
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
            this.mainTabControl.Size = new System.Drawing.Size(450, 480);
            this.mainTabControl.TabIndex = 14;
            //
            // buttonInsert
            //
            this.buttonInsert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonInsert.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonInsert.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonInsert.Location = new System.Drawing.Point(282, 486);
            this.buttonInsert.Name = "buttonInsert";
            this.buttonInsert.Size = new System.Drawing.Size(75, 23);
            this.buttonInsert.TabIndex = 20;
            this.buttonInsert.Text = "Insert";
            //
            // btnCancel
            //
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnCancel.Location = new System.Drawing.Point(363, 486);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 21;
            this.btnCancel.Text = "Cancel";
            //
            // copyrightLinkLabel
            //
            this.copyrightLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.copyrightLinkLabel.AutoSize = true;
            this.copyrightLinkLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.copyrightLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.copyrightLinkLabel.LinkColor = System.Drawing.SystemColors.HotTrack;
            this.copyrightLinkLabel.Location = new System.Drawing.Point(7, 493);
            this.copyrightLinkLabel.Name = "copyrightLinkLabel";
            this.copyrightLinkLabel.Size = new System.Drawing.Size(140, 15);
            this.copyrightLinkLabel.TabIndex = 19;
            this.copyrightLinkLabel.TabStop = true;
            this.copyrightLinkLabel.Text = "Please Respect Copyright";
            //
            // MediaInsertForm
            //
            this.AcceptButton = this.buttonInsert;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(450, 520);
            this.Controls.Add(this.copyrightLinkLabel);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.buttonInsert);
            this.Controls.Add(this.mainTabControl);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MediaInsertForm";
            this.Text = "Insert Video";
            ((System.ComponentModel.ISupportInitialize)(this.mainTabControl)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private void SetActiveTab(int num)
        {
            activeSource = _sources[num];
            activeSource.TabSelected();

            mainTabControl.InitFocusManager(true);
            mainTabControl.AddFocusableControls(tabs.GetAccessibleControls());
            mainTabControl.AddFocusableControls(activeSource.GetAccessibleControls());
            mainTabControl.AddFocusableControl(copyrightLinkLabel);
            mainTabControl.AddFocusableControl(buttonInsert);
            mainTabControl.AddFocusableControl(btnCancel);
        }

        private void tabs_SelectedTabNumberChanged(object sender, EventArgs e)
        {
            int tabChosen = ((TabLightweightControl)sender).SelectedTabNumber;
            tabs.Update();
            SetActiveTab(tabChosen);
        }

        public void SaveContent(MediaSmartContent content)
        {
            activeSource.SaveContent(content);
        }

        private void copyrightLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string link = MarketizationOptions.GetFeatureParameter(MarketizationOptions.Feature.VideoCopyright, "Glink");
            if (link != null && link != String.Empty)
            {
                try
                {
                    ShellHelper.LaunchUrl(link);
                }
                catch (Exception ex)
                {
                    Trace.Fail("Unexpected exception navigating to copyright page: " + link + ", " + ex.ToString());
                }
            }
        }

        void IRtlAware.Layout()
        {
            BidiHelper.RtlLayoutFixup(this, true, true, Controls);
        }
    }
}
