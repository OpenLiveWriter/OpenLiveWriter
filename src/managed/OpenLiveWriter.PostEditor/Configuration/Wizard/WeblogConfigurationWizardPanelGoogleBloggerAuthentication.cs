// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.BlogClient.Clients;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Localization;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Blogger.v3;
using Google.Apis.Blogger.v3.Data;
using Google.Apis.Services;
using System.Threading;
using OpenLiveWriter.BlogClient.Detection;
using Google.Apis.Util.Store;

namespace OpenLiveWriter.PostEditor.Configuration.Wizard
{
    /// <summary>
    /// Summary description for WeblogConfigurationWizardPanelAuthentication.
    /// </summary>
    internal class WeblogConfigurationWizardPanelGoogleBloggerAuthentication : WeblogConfigurationWizardPanel, IAccountBasicInfoProvider
    {
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.Button buttonLogin;
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private string _blogId;
        private CancellationTokenSource _cancellationTokenSource;
        private UserCredential _userCredentials;

        public WeblogConfigurationWizardPanelGoogleBloggerAuthentication(string blogId)
        {
            _blogId = blogId;

            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            
            labelHeader.Text = Res.Get(StringId.CWGoogleBloggerTitle);
            labelDescription.Text = Res.Get(StringId.CWGoogleBloggerDescription);
            buttonLogin.Text = Res.Get(StringId.Login);
        }

        private async void buttonLogin_Click(object sender, EventArgs e)
        {
            buttonLogin.Enabled = false;

            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _userCredentials = await BloggerAtomClient.GetOAuth2AuthorizationAsync(_blogId, _cancellationTokenSource.Token);
                _cancellationTokenSource = null;
            }
            finally
            {
                buttonLogin.Enabled = true;
            }
        }
        
        public void CancelAuthorization()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }
        }

        public override void NaturalizeLayout()
        {
            if (!DesignMode)
            {
                MaximizeWidth(labelDescription);

                LayoutHelper.NaturalizeHeight(labelDescription);
                LayoutHelper.DistributeVertically(10, false, labelDescription, buttonLogin);
            }
        }

        public override ConfigPanelId? PanelId
        {
            get { return ConfigPanelId.GoogleBloggerAuth; }
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
            this.buttonLogin = new System.Windows.Forms.Button();
            this.labelDescription = new System.Windows.Forms.Label();
            this.panelMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.buttonLogin);
            this.panelMain.Controls.Add(this.labelDescription);
            // 
            // buttonLogin
            // 
            this.buttonLogin.AutoSize = true;
            this.buttonLogin.Location = new System.Drawing.Point(20, 0);
            this.buttonLogin.FlatStyle = FlatStyle.System;
            this.buttonLogin.Name = "buttonLogin";
            this.buttonLogin.Size = new System.Drawing.Size(168, 13);
            this.buttonLogin.TabIndex = 0;
            this.buttonLogin.Text = "Sign in";
            this.buttonLogin.Click += buttonLogin_Click;
            // 
            // labelDescription
            // 
            this.labelDescription.FlatStyle = FlatStyle.System;
            this.labelDescription.Location = new System.Drawing.Point(20, 0);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(360, 24);
            this.labelDescription.TabIndex = 1;
            this.labelDescription.Text = "To configure Google Blogger please sign in.";
            // 
            // WeblogConfigurationWizardPanelGoogleBloggerAuthentication
            // 
            this.Name = "WeblogConfigurationWizardPanelGoogleBloggerAuthentication";
            this.panelMain.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        public override bool ValidatePanel()
        {
            if (_userCredentials == null || _userCredentials.Token == null)
            {
                ShowValidationError(buttonLogin, MessageId.GoogleBloggerLoginRequired);
                return false;
            }

            return true;
        }

        public bool IsDirty(TemporaryBlogSettings settings)
        {
            return false;

            // TODO:OLW
            // return Credentials.OAuthCredentials != settings.Credentials.OAuthCredentials;
        }

        public IBlogProviderAccountWizardDescription ProviderAccountWizard
        {
            set { }
        }

        public string AccountId
        {
            set { }
        }

        public string HomepageUrl
        {
            get { return string.Empty; }
            set { }
        }

        public bool SavePassword
        {
            get { return true; }
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
                // The Google Blogger credentials don't use the normal IBlogCredentials storage and are instead 
                // automatically written to disk by the Google APIs in the GetOAuth2AuthorizationAsync() call.
                TemporaryBlogCredentials credentials = new TemporaryBlogCredentials();
                credentials.Username = "user";
                return credentials;
            }
            set { }
        }

        public BlogInfo BlogAccount
        {
            get { return null; }
        }
    }
}
