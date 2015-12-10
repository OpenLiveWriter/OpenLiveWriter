// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework.Preferences;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Configuration.Wizard
{
    /// <summary>
    /// Summary description for WelcomeToBlogControl.
    /// </summary>
    internal class WeblogConfigurationWizardPanelSharePointBasicInfo : WeblogConfigurationWizardPanel, IAccountBasicInfoProvider
    {
        private TextBox textBoxHomepageUrl;
        private Label labelHomepageUrl;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public WeblogConfigurationWizardPanelSharePointBasicInfo()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            labelHeader.Text = Res.Get(StringId.CWBasicHeader);
            labelHomepageUrl.Text = Res.Get(StringId.CWSharePointHomepageUrl);
        }

        public override bool ShowProxySettingsLink
        {
            get { return true; }
        }

        public override ConfigPanelId? PanelId
        {
            get { return ConfigPanelId.SharePointBasicInfo; }
        }

        public override void NaturalizeLayout()
        {
            if (!DesignMode)
            {
                MaximizeWidth(labelHomepageUrl);
                LayoutHelper.NaturalizeHeightAndDistribute(3, labelHomepageUrl, textBoxHomepageUrl);
            }
        }

        public IBlogProviderAccountWizardDescription ProviderAccountWizard
        {
            set
            {
            }
        }

        public string AccountId
        {
            set
            {
            }
        }

        public string HomepageUrl
        {
            get { return UrlHelper.FixUpUrl(textBoxHomepageUrl.Text); }
            set { textBoxHomepageUrl.Text = value; }
        }

        public bool SavePassword
        {
            get { return false; }
            set { }
        }

        public bool ForceManualConfiguration
        {
            get { return false; }
            set { }
        }

        public IBlogCredentials Credentials
        {
            get
            {
                if (credentials == null)
                {
                    credentials = new TemporaryBlogCredentials();
                    credentials.Username = "";
                    credentials.Password = "";
                }
                return credentials;
            }
            set
            {
                credentials = new TemporaryBlogCredentials();
                credentials.Username = value.Username;
                credentials.Password = value.Password;
            }
        }
        private TemporaryBlogCredentials credentials;

        public bool IsDirty(TemporaryBlogSettings settings)
        {
            return
                !UrlHelper.UrlsAreEqual(HomepageUrl, settings.HomepageUrl) ||
                !BlogCredentialsHelper.CredentialsAreEqual(Credentials, settings.Credentials);
        }

        public BlogInfo BlogAccount
        {
            get
            {
                return null;
            }
        }

        public override bool ValidatePanel()
        {
            if (HomepageUrl == String.Empty)
            {
                ShowValidationError(textBoxHomepageUrl, MessageId.HomepageUrlRequired);
                return false;
            }
            return true;
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
            this.labelHomepageUrl = new Label();
            this.textBoxHomepageUrl = new TextBox();
            this.panelMain.SuspendLayout();
            this.SuspendLayout();
            //
            // panelMain
            //
            this.panelMain.Controls.Add(this.labelHomepageUrl);
            this.panelMain.Controls.Add(this.textBoxHomepageUrl);
            this.panelMain.Location = new System.Drawing.Point(48, 8);
            this.panelMain.Size = new System.Drawing.Size(352, 224);
            //
            // textBoxHomepageUrl
            //
            this.textBoxHomepageUrl.Location = new System.Drawing.Point(20, 74);
            this.textBoxHomepageUrl.Name = "textBoxHomepageUrl";
            this.textBoxHomepageUrl.Size = new System.Drawing.Size(275, 22);
            this.textBoxHomepageUrl.TabIndex = 2;
            this.textBoxHomepageUrl.Enter += new System.EventHandler(this.textBoxHomepageUrl_Enter);
            this.textBoxHomepageUrl.Leave += new System.EventHandler(this.textBoxHomepageUrl_Leave);
            //
            // labelHomepageUrl
            //
            this.labelHomepageUrl.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelHomepageUrl.Location = new System.Drawing.Point(20, 0);
            this.labelHomepageUrl.Name = "labelHomepageUrl";
            this.labelHomepageUrl.Size = new System.Drawing.Size(167, 13);
            this.labelHomepageUrl.TabIndex = 1;
            this.labelHomepageUrl.Text = "Web &address of your blog:";
            //
            // WeblogConfigurationWizardPanelSharePointBasicInfo
            //
            this.Name = "WeblogConfigurationWizardPanelSharePointBasicInfo";
            this.Size = new System.Drawing.Size(432, 244);
            this.panelMain.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private void textBoxHomepageUrl_Enter(object sender, EventArgs e)
        {
            textBoxHomepageUrl.SelectAll();
        }

        private void textBoxHomepageUrl_Leave(object sender, EventArgs e)
        {
            // adds http:// if necessary
            HomepageUrl = HomepageUrl;
        }

    }
}
