// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.PostEditor.Configuration.Wizard
{
    /// <summary>
    /// Summary description for WeblogHomepageUrlControl.
    /// </summary>
    public class WeblogHomepageUrlControl : UserControl
    {
        private TextBox textBoxHomepageUrl;
        private Label labeHomepageURL;
        private LinkLabel linkLabelViewWeblog;
        private HelpProvider helpProvider;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public WeblogHomepageUrlControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            this.linkLabelViewWeblog.Text = Res.Get(StringId.ViewWeblog);
            this.textBoxHomepageUrl.RightToLeft = RightToLeft.No;
            if (BidiHelper.IsRightToLeft)
                textBoxHomepageUrl.TextAlign = HorizontalAlignment.Right;

            HelpProviderContext.Bind(helpProvider, textBoxHomepageUrl);
            HelpProviderContext.Bind(helpProvider, linkLabelViewWeblog);

            // can't tab to the link-label
            linkLabelViewWeblog.Enabled = false;
            linkLabelViewWeblog.TabStop = false;

            linkLabelViewWeblog.UseCompatibleTextRendering = false;
        }

        public string HomepageUrl
        {
            get
            {
                return UrlHelper.FixUpUrl(textBoxHomepageUrl.Text);
            }
            set
            {
                textBoxHomepageUrl.Text = value;
            }
        }

        private void textBoxHomepageUrl_TextChanged(object sender, EventArgs e)
        {
            linkLabelViewWeblog.Enabled = UrlHelper.IsUrl(HomepageUrl);

            if (Changed != null)
                Changed(this, EventArgs.Empty);
        }

        private void linkLabelViewWeblog_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                ShellHelper.LaunchUrl(HomepageUrl);
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception navigating to weblog: " + HomepageUrl + ", " + ex.ToString());
            }
        }

        /// <summary>
        /// Notification that the value changed
        /// </summary>
        public event EventHandler Changed;


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
            this.textBoxHomepageUrl = new System.Windows.Forms.TextBox();
            this.labeHomepageURL = new System.Windows.Forms.Label();
            this.linkLabelViewWeblog = new System.Windows.Forms.LinkLabel();
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.SuspendLayout();
            //
            // textBoxHomepageUrl
            //
            this.textBoxHomepageUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.helpProvider.SetHelpString(this.textBoxHomepageUrl, "The URL for the homepage of your weblog");
            this.textBoxHomepageUrl.Location = new System.Drawing.Point(0, 16);
            this.textBoxHomepageUrl.Name = "textBoxHomepageUrl";
            this.helpProvider.SetShowHelp(this.textBoxHomepageUrl, true);
            this.textBoxHomepageUrl.Size = new System.Drawing.Size(315, 20);
            this.textBoxHomepageUrl.TabIndex = 1;
            this.textBoxHomepageUrl.Text = "";
            this.textBoxHomepageUrl.TextChanged += new System.EventHandler(this.textBoxHomepageUrl_TextChanged);
            //
            // labeHomepageURL
            //
            this.labeHomepageURL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                )));
            this.labeHomepageURL.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labeHomepageURL.Location = new System.Drawing.Point(0, 0);
            this.labeHomepageURL.Name = "labeHomepageURL";
            this.labeHomepageURL.Size = new System.Drawing.Size(311, 17);
            this.labeHomepageURL.TabIndex = 0;
            this.labeHomepageURL.Text = "Weblog &Homepage URL:";
            //
            // linkLabelViewWeblog
            //
            this.linkLabelViewWeblog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelViewWeblog.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.helpProvider.SetHelpString(this.linkLabelViewWeblog, "View the weblog in a browser");
            this.linkLabelViewWeblog.Location = new System.Drawing.Point(139, 38);
            this.linkLabelViewWeblog.Name = "linkLabelViewWeblog";
            this.helpProvider.SetShowHelp(this.linkLabelViewWeblog, true);
            this.linkLabelViewWeblog.Size = new System.Drawing.Size(175, 15);
            this.linkLabelViewWeblog.TabIndex = 2;
            this.linkLabelViewWeblog.TabStop = true;
            this.linkLabelViewWeblog.Text = "View Weblog";
            this.linkLabelViewWeblog.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.linkLabelViewWeblog.LinkBehavior = LinkBehavior.HoverUnderline;
            this.linkLabelViewWeblog.LinkColor = SystemColors.HotTrack;
            this.linkLabelViewWeblog.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelViewWeblog_LinkClicked);
            //
            // WeblogHomepageUrlControl
            //
            this.Controls.Add(this.textBoxHomepageUrl);
            this.Controls.Add(this.labeHomepageURL);
            this.Controls.Add(this.linkLabelViewWeblog);
            this.Name = "WeblogHomepageUrlControl";
            this.Size = new System.Drawing.Size(315, 55);
            this.ResumeLayout(false);

        }
        #endregion

        public string HomepageUrlLabel
        {
            get
            {
                return labeHomepageURL.Text;
            }
            set
            {
                labeHomepageURL.Text = value;
            }
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);
            HomepageUrl = UrlHelper.FixUpUrl(HomepageUrl);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            using (new AutoGrow(this, AnchorStyles.Bottom, false))
            {
                LayoutHelper.NaturalizeHeightAndDistribute(6, Controls);
            }

        }

    }
}
