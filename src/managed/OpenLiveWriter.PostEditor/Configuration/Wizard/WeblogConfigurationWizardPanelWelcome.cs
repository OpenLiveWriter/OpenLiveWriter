// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework.Skinning;
using OpenLiveWriter.Controls.Wizard;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Configuration.Wizard
{
    /// <summary>
    /// Summary description for WeblogConfigurationWizardPanelEditingWithStyle.
    /// </summary>
    public class WeblogConfigurationWizardPanelWelcome : WeblogConfigurationWizardPanel
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;
        private System.Windows.Forms.Label labelWelcomeText;
        private LinkLabel linkLabelLearnMore;

        public WeblogConfigurationWizardPanelWelcome()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            this.labelHeader.Text = string.Format(CultureInfo.InvariantCulture, Res.Get(StringId.ConfigureProduct), ApplicationEnvironment.ProductNameQualified);
            this.labelWelcomeText.Text = Res.Get(StringId.CWWelcomeText);
            this.linkLabelLearnMore.Text = Res.Get(StringId.ConfigWizardLearnMore);
            this.linkLabelLearnMore.LinkClicked += new LinkLabelLinkClickedEventHandler(linkLabelLearnMore_Click);
            this.linkLabelLearnMore.LinkColor = SystemInformation.HighContrast ? SystemColors.HotTrack : Color.FromArgb(0, 102, 204);

            if (!DesignMode)
            {
                // customize default provider text
                labelWelcomeText.Text = String.Format(CultureInfo.CurrentCulture, labelWelcomeText.Text, ApplicationEnvironment.ProductNameQualified);
                linkLabelLearnMore.Text = String.Format(CultureInfo.CurrentCulture, linkLabelLearnMore.Text, ApplicationEnvironment.ProductNameQualified);

                // is there a custom account wizard with a welcome page installed?
                IBlogProviderAccountWizardDescription providerAccountWizard = null;
                foreach (IBlogProviderAccountWizardDescription wizard in BlogProviderAccountWizard.InstalledAccountWizards)
                {
                    if (wizard.WelcomePage != null)
                    {
                        providerAccountWizard = wizard;
                        break;
                    }
                }
            }
        }

        void linkLabelLearnMore_Click(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ShellHelper.LaunchUrl(GLink.Instance.LearnMore);
        }

        public override ConfigPanelId? PanelId
        {
            get { return ConfigPanelId.Welcome; }
        }

        public override void NaturalizeLayout()
        {
            if (!DesignMode)
            {
                MaximizeWidth(labelWelcomeText);
                MaximizeWidth(linkLabelLearnMore);
                LayoutHelper.NaturalizeHeightAndDistribute(10, labelWelcomeText, linkLabelLearnMore);
            }
        }

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WeblogConfigurationWizardPanelWelcome));
            this.labelWelcomeText = new System.Windows.Forms.Label();
            this.linkLabelLearnMore = new LinkLabel();
            this.panelMain.SuspendLayout();
            this.SuspendLayout();
            //
            // panelMain
            //
            this.panelMain.Controls.Add(this.labelWelcomeText);
            this.panelMain.Controls.Add(this.linkLabelLearnMore);
            //
            // labelWelcomeText
            //
            this.labelWelcomeText.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelWelcomeText.Location = new System.Drawing.Point(20, 0);
            this.labelWelcomeText.Name = "labelWelcomeText";
            this.labelWelcomeText.Size = new System.Drawing.Size(344, 55);
            this.labelWelcomeText.Text = resources.GetString("labelWelcomeText.Text");
            //
            // linkLabelLearnMore
            //
            this.linkLabelLearnMore.Location = new Point(17, 65);
            this.linkLabelLearnMore.Name = "linkLabelLearnMore";
            this.linkLabelLearnMore.LinkBehavior = LinkBehavior.HoverUnderline;
            //
            // WeblogConfigurationWizardPanelWelcome
            //
            this.Name = "WeblogConfigurationWizardPanelWelcome";
            this.Size = new System.Drawing.Size(432, 244);
            this.panelMain.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

    }
}
