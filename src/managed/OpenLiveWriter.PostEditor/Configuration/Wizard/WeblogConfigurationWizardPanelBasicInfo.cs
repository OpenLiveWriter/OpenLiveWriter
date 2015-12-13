// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework.Preferences;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.PostEditor.BlogProviderButtons;

namespace OpenLiveWriter.PostEditor.Configuration.Wizard
{
    /// <summary>
    /// Summary description for WelcomeToBlogControl.
    /// </summary>
    internal class WeblogConfigurationWizardPanelBasicInfo : WeblogConfigurationWizardPanel, IAccountBasicInfoProvider
    {
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.TextBox textBoxUsername;
        private System.Windows.Forms.Label labelUsername;
        private System.Windows.Forms.TextBox textBoxHomepageUrl;
        private System.Windows.Forms.Label labelHomepageUrl;
        private System.Windows.Forms.Label labelHomepageUrl2;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        private System.Windows.Forms.CheckBox checkBoxSavePassword;

        public WeblogConfigurationWizardPanelBasicInfo()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            this.textBoxHomepageUrl.RightToLeft = RightToLeft.No;
            if (BidiHelper.IsRightToLeft)
                textBoxHomepageUrl.TextAlign = HorizontalAlignment.Right;

            this.labelHeader.Text = Res.Get(StringId.CWBasicHeader);
            this.labelHomepageUrl.Text = Res.Get(StringId.CWBasicHomepage);
            this.labelHomepageUrl2.Text = Res.Get(StringId.CWBasicHomepage2);
            this.labelUsername.Text = Res.Get(StringId.UsernameLabel);
            this.labelPassword.Text = Res.Get(StringId.PasswordLabel);
            this.textBoxPassword.PasswordChar = Res.PasswordChar;
            this.checkBoxSavePassword.Text = Res.Get(StringId.RememberPassword);

            textBoxPassword.PasswordChar = Res.PasswordChar;

            textBoxHomepageUrl.AccessibleName = ControlHelper.ToAccessibleName(Res.Get(StringId.CWBasicHomepage));
        }

        public override void NaturalizeLayout()
        {
            if (!DesignMode)
            {
                MaximizeWidth(labelHomepageUrl);
                MaximizeWidth(labelHomepageUrl2);
                MaximizeWidth(checkBoxSavePassword);
                LayoutHelper.NaturalizeHeightAndDistribute(3, labelHomepageUrl, textBoxHomepageUrl, labelHomepageUrl2);
                LayoutHelper.NaturalizeHeightAndDistribute(3, labelUsername, textBoxUsername);
                LayoutHelper.NaturalizeHeightAndDistribute(3, labelPassword, textBoxPassword, checkBoxSavePassword);
                LayoutHelper.DistributeVertically(10, false,
                    new ControlGroup(labelHomepageUrl, textBoxHomepageUrl, labelHomepageUrl2),
                    new ControlGroup(labelUsername, textBoxUsername),
                    new ControlGroup(labelPassword, textBoxPassword, checkBoxSavePassword)
                    );
            }
        }

        public override ConfigPanelId? PanelId
        {
            get { return ConfigPanelId.OtherBasicInfo; }
        }

        public override bool ShowProxySettingsLink
        {
            get { return true; }
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
            get { return UrlHelper.FixUpUrl(textBoxHomepageUrl.Text); }
            set { textBoxHomepageUrl.Text = value; }
        }

        public bool SavePassword
        {
            get { return checkBoxSavePassword.Checked; }
            set { checkBoxSavePassword.Checked = value; }
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
                TemporaryBlogCredentials credentials = new TemporaryBlogCredentials();
                credentials.Username = textBoxUsername.Text.Trim();
                credentials.Password = textBoxPassword.Text.Trim();
                return credentials;
            }
            set
            {
                textBoxUsername.Text = value.Username;
                textBoxPassword.Text = value.Password;
            }
        }

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
            string homepageUrl = HomepageUrl;

            if (homepageUrl == String.Empty)
            {
                ShowValidationError(textBoxHomepageUrl, MessageId.HomepageUrlRequired);
                return false;
            }

            if (!UrlHelper.IsUrl(homepageUrl))
            {
                ShowValidationError(textBoxHomepageUrl, MessageId.HomepageUrlInvalid);
                return false;
            }

            if (textBoxUsername.Text.Trim() == String.Empty)
            {
                ShowValidationError(textBoxUsername, MessageId.UsernameAndPasswordRequired);
                return false;
            }

            if (textBoxPassword.Text.Trim() == String.Empty)
            {
                ShowValidationError(textBoxPassword, MessageId.UsernameAndPasswordRequired);
                return false;
            }

            if (IsWordPress(homepageUrl))
            {
                ShowValidationError(textBoxHomepageUrl, MessageId.WordpressHomepageWrong, ApplicationEnvironment.ProductNameQualified);
                return false;
            }

            return true;
        }

        private static bool IsWordPress(string url)
        {
            try
            {
                return Regex.IsMatch(
                    new Uri(url).Host,
                    @"^(www\.)?wordpress\.com$",
                    RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
            }
            catch (Exception e)
            {
                Trace.Fail(e.ToString());
                return false;
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
            this.checkBoxSavePassword = new System.Windows.Forms.CheckBox();
            this.labelPassword = new System.Windows.Forms.Label();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.textBoxUsername = new System.Windows.Forms.TextBox();
            this.labelUsername = new System.Windows.Forms.Label();
            this.textBoxHomepageUrl = new System.Windows.Forms.TextBox();
            this.labelHomepageUrl = new System.Windows.Forms.Label();
            this.labelHomepageUrl2 = new System.Windows.Forms.Label();
            this.panelMain.SuspendLayout();
            this.SuspendLayout();
            panelMain.Controls.Add(labelHomepageUrl);
            panelMain.Controls.Add(textBoxHomepageUrl);
            panelMain.Controls.Add(labelHomepageUrl2);
            panelMain.Controls.Add(labelUsername);
            panelMain.Controls.Add(textBoxUsername);
            panelMain.Controls.Add(labelPassword);
            panelMain.Controls.Add(textBoxPassword);
            panelMain.Controls.Add(checkBoxSavePassword);
            //
            // checkBoxSavePassword
            //
            this.checkBoxSavePassword.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxSavePassword.Location = new System.Drawing.Point(20, 98);
            this.checkBoxSavePassword.Name = "checkBoxSavePassword";
            this.checkBoxSavePassword.Size = new System.Drawing.Size(165, 26);
            this.checkBoxSavePassword.TabIndex = 5;
            this.checkBoxSavePassword.Text = "&Remember my password";
            this.checkBoxSavePassword.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            //
            // labelPassword
            //
            this.labelPassword.AutoSize = true;
            this.labelPassword.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelPassword.Location = new System.Drawing.Point(20, 57);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(73, 17);
            this.labelPassword.TabIndex = 3;
            this.labelPassword.Text = "&Password:";
            //
            // textBoxPassword
            //
            this.textBoxPassword.Location = new System.Drawing.Point(20, 74);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(208, 22);
            this.textBoxPassword.TabIndex = 4;
            this.textBoxPassword.Enter += new System.EventHandler(this.textBoxPassword_Enter);
            //
            // textBoxUsername
            //
            this.textBoxUsername.Location = new System.Drawing.Point(20, 74);
            this.textBoxUsername.Name = "textBoxUsername";
            this.textBoxUsername.Size = new System.Drawing.Size(208, 22);
            this.textBoxUsername.TabIndex = 2;
            this.textBoxUsername.Enter += new System.EventHandler(this.textBoxUsername_Enter);
            //
            // labelUsername
            //
            this.labelUsername.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelUsername.Location = new System.Drawing.Point(20, 57);
            this.labelUsername.Name = "labelUsername";
            this.labelUsername.Size = new System.Drawing.Size(167, 13);
            this.labelUsername.TabIndex = 1;
            this.labelUsername.Text = "&Username:";
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
            // labelHomepageUrl2
            //
            this.labelHomepageUrl2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelHomepageUrl2.ForeColor = SystemColors.GrayText;
            this.labelHomepageUrl2.Location = new System.Drawing.Point(20, 57);
            this.labelHomepageUrl2.Name = "labelHomepageUrl2";
            this.labelHomepageUrl2.Size = new System.Drawing.Size(167, 13);
            this.labelHomepageUrl2.TabIndex = 1;
            this.labelHomepageUrl2.Text = "This is the URL that visitors use to read your blog";
            //
            // WeblogConfigurationWizardPanelBasicInfo
            //
            this.Name = "WeblogConfigurationWizardPanelBasicInfo";
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

        private void textBoxUsername_Enter(object sender, EventArgs e)
        {
            textBoxUsername.SelectAll();
        }

        private void textBoxPassword_Enter(object sender, EventArgs e)
        {
            textBoxPassword.SelectAll();
        }


    }

}
