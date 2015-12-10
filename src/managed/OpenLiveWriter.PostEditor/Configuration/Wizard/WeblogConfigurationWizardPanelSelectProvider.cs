// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.BlogClient.Detection;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.BlogClient.Providers;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.CoreServices.Marketization;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Api;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.PostEditor.Configuration.Wizard
{
    /// <summary>
    /// Summary description for WeblogConfigurationWizardPanelSelectProvider.
    /// </summary>
    public class WeblogConfigurationWizardPanelSelectProvider : WeblogConfigurationWizardPanel
    {
        private System.Windows.Forms.Label labelSelectProvider;
        private System.Windows.Forms.ComboBox comboBoxSelectProvider;
        private System.Windows.Forms.Label labelServerAPIUrl;
        private System.Windows.Forms.TextBox textBoxServerApiUrl;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private string _homepageUrl;
        private IBlogCredentials _credentials;
        private string _accountId;

        private BlogInfo _targetBlog = null;
        private System.Windows.Forms.Label labelText;
        private BlogInfo[] _usersBlogs = new BlogInfo[] { };

        public override void NaturalizeLayout()
        {
            MaximizeWidth(labelText);
            MaximizeWidth(labelServerAPIUrl);
            MaximizeWidth(labelSelectProvider);
            LayoutHelper.NaturalizeHeightAndDistribute(3, labelSelectProvider, comboBoxSelectProvider);
            LayoutHelper.NaturalizeHeightAndDistribute(3, labelServerAPIUrl, textBoxServerApiUrl);
            LayoutHelper.NaturalizeHeightAndDistribute(10,
                labelText,
                new ControlGroup(labelSelectProvider, comboBoxSelectProvider),
                new ControlGroup(labelServerAPIUrl, textBoxServerApiUrl));
        }

        public WeblogConfigurationWizardPanelSelectProvider()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            this.labelHeader.Text = Res.Get(StringId.ConfigWizardSelectProvider);
            this.labelSelectProvider.Text = Res.Get(StringId.CWSelectProviderWeblogTypeLabel);
            this.labelServerAPIUrl.Text = Res.Get(StringId.CWSelectProviderApiUrlLabel);
            this.labelText.Text = Res.Get(StringId.CWSelectProviderText);

            if (BidiHelper.IsRightToLeft)
                textBoxServerApiUrl.TextAlign = HorizontalAlignment.Right;

            this.textBoxServerApiUrl.RightToLeft = RightToLeft.No;

            // Load up the combo and select the first item
            //adding marketization--only show providers for this market
            HashSet marketSupportedIds = new HashSet();
            marketSupportedIds.AddAll(
                StringHelper.Split(
                    MarketizationOptions.GetFeatureParameter(MarketizationOptions.Feature.BlogProviders, "supported"), ";"));
            foreach (IBlogProvider provider in BlogProviderManager.Providers)
                if (provider.Visible && marketSupportedIds.Contains(provider.Id))
                    comboBoxSelectProvider.Items.Add(new BlogProviderDescriptionProxy(provider));

            comboBoxSelectProvider.SelectedIndex = 0;

            labelText.Text = string.Format(CultureInfo.CurrentCulture, labelText.Text, ApplicationEnvironment.ProductNameQualified);
        }

        public override ConfigPanelId? PanelId
        {
            get { return ConfigPanelId.SelectProvider; }
        }

        public void ShowPanel(string defaultServiceName, string homepageUrl, string accountId, IBlogCredentials credentials)
        {
            // save reference to settings
            _homepageUrl = homepageUrl;
            _accountId = accountId;
            _credentials = credentials;

            // find provider and select it (add it to the combo if necessary)
            IBlogProviderDescription provider = BlogProviderManager.FindProviderByName(defaultServiceName);
            if (provider != null)
            {
                BlogProviderDescriptionProxy providerProxy = new BlogProviderDescriptionProxy(provider);

                if (!comboBoxSelectProvider.Items.Contains(providerProxy))
                    comboBoxSelectProvider.Items.Add(providerProxy);

                comboBoxSelectProvider.SelectedItem = providerProxy;
            }
            else
            {
                // add the special 'select provider' entry and select it
                if (!comboBoxSelectProvider.Items.Contains(BlogProviderDescriptionProxy.SelectProvider))
                    comboBoxSelectProvider.Items.Add(BlogProviderDescriptionProxy.SelectProvider);

                comboBoxSelectProvider.SelectedItem = BlogProviderDescriptionProxy.SelectProvider;
            }

            // reset results
            _targetBlog = null;
            _usersBlogs = new BlogInfo[] { };
        }

        public IBlogProviderDescription SelectedBlogProvider
        {
            get
            {
                return new BlogProviderDescriptionProxy(
                    comboBoxSelectProvider.SelectedItem as IBlogProviderDescription,
                    textBoxServerApiUrl.Text.Trim());
            }
        }

        public BlogInfo TargetBlog
        {
            get { return _targetBlog; }
        }

        public BlogInfo[] UsersBlogs
        {
            get { return _usersBlogs; }
        }

        public override bool ValidatePanel()
        {
            // validate we have select a provider
            if (comboBoxSelectProvider.SelectedItem == BlogProviderDescriptionProxy.SelectProvider)
            {
                DisplayMessage.Show(MessageId.RequiredFieldOmitted, FindForm(), Res.Get(StringId.CWSelectProviderWeblogProvider));
                comboBoxSelectProvider.Focus();
                return false;
            }
            // validate that we have a post api url
            else if (textBoxServerApiUrl.Text.Trim() == String.Empty)
            {
                DisplayMessage.Show(MessageId.RequiredFieldOmitted, FindForm(), Res.Get(StringId.CWSelectProviderApiUrl));
                textBoxServerApiUrl.Focus();
                return false;
            }
            else if (!ValidateNoParameters(textBoxServerApiUrl))
            {
                return false;
            }
            else
            {
                using (new WaitCursor())
                {
                    IBlogProviderDescription provider = SelectedBlogProvider;
                    BlogAccountDetector blogAccountDetector = new BlogAccountDetector(
                        provider.ClientType, provider.PostApiUrl, new BlogCredentialsAccessor(_accountId, _credentials));

                    if (blogAccountDetector.ValidateService())
                    {
                        BlogInfo blogInfo = blogAccountDetector.DetectAccount(_homepageUrl, null);
                        if (blogInfo != null)
                            _targetBlog = blogInfo;
                        _usersBlogs = blogAccountDetector.UsersBlogs;
                        return true;
                    }
                    else
                    {
                        if (blogAccountDetector.ErrorMessageType != MessageId.None)
                            DisplayMessage.Show(blogAccountDetector.ErrorMessageType, FindForm(), blogAccountDetector.ErrorMessageParams);
                        textBoxServerApiUrl.Focus();
                        return false;
                    }
                }
            }
        }

        private bool ValidateNoParameters(TextBox textBoxEntry)
        {
            // pick out the <param> values in the server url string
            string parameters = BlogProviderParameters.ExtractParameterList(textBoxEntry.Text.Trim());

            // if there are values then tell the user they must fill them in
            if (parameters.Length > 0)
            {
                DisplayMessage.Show(MessageId.ServerParameterNotSpecified, textBoxEntry.FindForm(), parameters.ToString());
                textBoxEntry.Focus();
                return false;
            }
            else
            {
                return true;
            }
        }

        private void comboBoxSelectProvider_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            BlogProviderDescriptionProxy blogProvider = comboBoxSelectProvider.SelectedItem as BlogProviderDescriptionProxy;

            if (blogProvider != BlogProviderDescriptionProxy.SelectProvider)
            {
                textBoxServerApiUrl.Text = blogProvider.PostApiUrl;
                labelServerAPIUrl.Text = Res.Get(blogProvider.PostApiUrlLabel);

                // remove the special SelectProvider entry if it exists
                if (comboBoxSelectProvider.Items.Contains(BlogProviderDescriptionProxy.SelectProvider))
                    comboBoxSelectProvider.Items.Remove(BlogProviderDescriptionProxy.SelectProvider);
            }
            else
            {
                textBoxServerApiUrl.Text = String.Empty;
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
            this.labelSelectProvider = new System.Windows.Forms.Label();
            this.comboBoxSelectProvider = new System.Windows.Forms.ComboBox();
            this.labelServerAPIUrl = new System.Windows.Forms.Label();
            this.textBoxServerApiUrl = new System.Windows.Forms.TextBox();
            this.labelText = new System.Windows.Forms.Label();
            this.panelMain.SuspendLayout();
            //
            // panelMain
            //
            this.panelMain.Controls.Add(this.labelText);
            this.panelMain.Controls.Add(this.textBoxServerApiUrl);
            this.panelMain.Controls.Add(this.labelServerAPIUrl);
            this.panelMain.Controls.Add(this.comboBoxSelectProvider);
            this.panelMain.Controls.Add(this.labelSelectProvider);
            //
            // labelSelectProvider
            //
            this.labelSelectProvider.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelSelectProvider.Location = new System.Drawing.Point(20, 48);
            this.labelSelectProvider.Name = "labelSelectProvider";
            this.labelSelectProvider.Size = new System.Drawing.Size(344, 16);
            this.labelSelectProvider.TabIndex = 0;
            this.labelSelectProvider.Text = "&Type of weblog that you are using:";
            //
            // comboBoxSelectProvider
            //
            this.comboBoxSelectProvider.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSelectProvider.Location = new System.Drawing.Point(20, 64);
            this.comboBoxSelectProvider.MaxDropDownItems = 20;
            this.comboBoxSelectProvider.Name = "comboBoxSelectProvider";
            this.comboBoxSelectProvider.Size = new System.Drawing.Size(256, 21);
            this.comboBoxSelectProvider.TabIndex = 1;
            this.comboBoxSelectProvider.SelectedIndexChanged += new System.EventHandler(this.comboBoxSelectProvider_SelectedIndexChanged);
            //
            // labelServerAPIUrl
            //
            this.labelServerAPIUrl.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelServerAPIUrl.Location = new System.Drawing.Point(20, 104);
            this.labelServerAPIUrl.Name = "labelServerAPIUrl";
            this.labelServerAPIUrl.Size = new System.Drawing.Size(344, 16);
            this.labelServerAPIUrl.TabIndex = 2;
            this.labelServerAPIUrl.Text = "Remote posting &URL for your weblog:";
            //
            // textBoxServerApiUrl
            //
            this.textBoxServerApiUrl.Location = new System.Drawing.Point(20, 120);
            this.textBoxServerApiUrl.Name = "textBoxServerApiUrl";
            this.textBoxServerApiUrl.Size = new System.Drawing.Size(256, 20);
            this.textBoxServerApiUrl.TabIndex = 3;
            this.textBoxServerApiUrl.Text = "";
            //
            // labelText
            //
            this.labelText.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelText.Location = new System.Drawing.Point(20, 0);
            this.labelText.Name = "labelText";
            this.labelText.Size = new System.Drawing.Size(350, 40);
            this.labelText.TabIndex = 4;
            this.labelText.Text = "{0} was not able to automatically detect your blog settings. Please select the ty" +
                "pe of blog that you are using to continue.";
            //
            // WeblogConfigurationWizardPanelSelectProvider
            //
            this.Name = "WeblogConfigurationWizardPanelSelectProvider";
            this.Size = new System.Drawing.Size(432, 244);
            this.panelMain.ResumeLayout(false);

        }
        #endregion

        private class BlogProviderDescriptionProxy : IBlogProviderDescription
        {
            public BlogProviderDescriptionProxy(IBlogProviderDescription provider)
                : this(provider, null)
            {
            }

            public BlogProviderDescriptionProxy(IBlogProviderDescription provider, string postApiUrl)
            {
                _provider = provider;
                _postApiUrl = postApiUrl;
            }

            public virtual string Id
            {
                get { return _provider.Id; }
            }

            public virtual string Name
            {
                get { return _provider.Name; }
            }

            public string Description
            {
                get { return _provider.Description; }
            }

            public string Link
            {
                get { return _provider.Link; }
            }

            public string ClientType
            {
                get { return _provider.ClientType; }
            }

            public string PostApiUrl
            {
                get
                {
                    if (_postApiUrl != null)
                        return _postApiUrl;
                    else
                        return _provider.PostApiUrl;
                }
            }

            public StringId PostApiUrlLabel
            {
                get { return _provider.PostApiUrlLabel; }
            }

            public string AppId
            {
                get { return _provider.AppId; }
            }

            public override string ToString()
            {
                return Name;
            }

            public override bool Equals(object obj)
            {
                return (obj as BlogProviderDescriptionProxy).Id == Id;
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }

            private IBlogProviderDescription _provider;
            private string _postApiUrl;

            // special "select provider" proxy

            internal static readonly BlogProviderDescriptionProxy SelectProvider = new SelectProviderProxy();

            private BlogProviderDescriptionProxy()
            {
            }

            private class SelectProviderProxy : BlogProviderDescriptionProxy
            {
                public override string Id
                {
                    get
                    {
                        return "8E0868BF-E1FA-487C-B519-8724F969E26A";
                    }
                }

                public override string Name
                {
                    get
                    {
                        return Res.Get(StringId.SelectWeblogProvider);
                    }
                }
            }

        }

    }

}
