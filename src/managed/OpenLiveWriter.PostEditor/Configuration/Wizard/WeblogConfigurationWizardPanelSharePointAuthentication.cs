// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Configuration.Wizard
{
    /// <summary>
    /// Summary description for WeblogConfigurationWizardPanelAuthentication.
    /// </summary>
    internal class WeblogConfigurationWizardPanelSharePointAuthentication : WeblogConfigurationWizardPanel, IAccountBasicInfoProvider
    {
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.TextBox textBoxUsername;
        private System.Windows.Forms.Label labelUsername;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.CheckBox cbUseSystemLogin;
        private System.Windows.Forms.CheckBox checkBoxSavePassword;

        IAccountBasicInfoProvider _basicInfoProvider;

        public WeblogConfigurationWizardPanelSharePointAuthentication()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            labelHeader.Text = Res.Get(StringId.CWSharePointTitle2);

            labelUsername.Text = Res.Get(StringId.UsernameLabel);
            labelPassword.Text = Res.Get(StringId.PasswordLabel);
            cbUseSystemLogin.Text = Res.Get(StringId.CWSharePointUseSystemLogin);
            checkBoxSavePassword.Text = Res.Get(StringId.RememberPassword);

            textBoxPassword.PasswordChar = Res.PasswordChar;

            cbUseSystemLogin.Checked = true;
        }

        public WeblogConfigurationWizardPanelSharePointAuthentication(IAccountBasicInfoProvider basicInfoProvider)
            : this()
        {
            _basicInfoProvider = basicInfoProvider;
        }

        public override void NaturalizeLayout()
        {
            if (!DesignMode)
            {
                MaximizeWidth(labelUsername);
                MaximizeWidth(labelPassword);
                MaximizeWidth(checkBoxSavePassword);
                MaximizeWidth(cbUseSystemLogin);

                LayoutHelper.NaturalizeHeight(labelUsername, labelPassword, checkBoxSavePassword, cbUseSystemLogin);
                LayoutHelper.DistributeVertically(10, false, cbUseSystemLogin, labelUsername);
                LayoutHelper.DistributeVertically(3, false, labelUsername, textBoxUsername);
                LayoutHelper.DistributeVertically(10, false, textBoxUsername, labelPassword);
                LayoutHelper.DistributeVertically(3, false, labelPassword, textBoxPassword, checkBoxSavePassword);
            }
        }

        public override ConfigPanelId? PanelId
        {
            get { return ConfigPanelId.SharePointAuth; }
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
            this.labelPassword = new System.Windows.Forms.Label();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.textBoxUsername = new System.Windows.Forms.TextBox();
            this.labelUsername = new System.Windows.Forms.Label();
            this.cbUseSystemLogin = new System.Windows.Forms.CheckBox();
            this.checkBoxSavePassword = new System.Windows.Forms.CheckBox();
            this.panelMain.SuspendLayout();
            this.SuspendLayout();
            //
            // panelMain
            //
            this.panelMain.Controls.Add(this.labelUsername);
            this.panelMain.Controls.Add(this.textBoxUsername);
            this.panelMain.Controls.Add(this.labelPassword);
            this.panelMain.Controls.Add(this.textBoxPassword);
            this.panelMain.Controls.Add(this.cbUseSystemLogin);
            this.panelMain.Controls.Add(this.checkBoxSavePassword);
            //
            // labelPassword
            //
            this.labelPassword.Location = new System.Drawing.Point(20, 0);
            this.labelPassword.FlatStyle = FlatStyle.System;
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(168, 13);
            this.labelPassword.TabIndex = 3;
            this.labelPassword.Text = "&Password:";
            //
            // textBoxPassword
            //
            this.textBoxPassword.Location = new System.Drawing.Point(20, 16);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(168, 20);
            this.textBoxPassword.TabIndex = 4;
            this.textBoxPassword.Text = "";
            //
            // textBoxUsername
            //
            this.textBoxUsername.Location = new System.Drawing.Point(20, 16);
            this.textBoxUsername.Name = "textBoxUsername";
            this.textBoxUsername.Size = new System.Drawing.Size(168, 20);
            this.textBoxUsername.TabIndex = 2;
            this.textBoxUsername.Text = "";
            //
            // labelUsername
            //
            this.labelUsername.Location = new System.Drawing.Point(20, 0);
            this.labelUsername.FlatStyle = FlatStyle.System;
            this.labelUsername.Name = "labelUsername";
            this.labelUsername.Size = new System.Drawing.Size(168, 13);
            this.labelUsername.TabIndex = 1;
            this.labelUsername.Text = "&Username:";
            //
            // cbUseSystemLogin
            //
            this.cbUseSystemLogin.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cbUseSystemLogin.Location = new System.Drawing.Point(20, 0);
            this.cbUseSystemLogin.Name = "cbUseSystemLogin";
            this.cbUseSystemLogin.Size = new System.Drawing.Size(360, 24);
            this.cbUseSystemLogin.TabIndex = 0;
            this.cbUseSystemLogin.Text = "Use my &Windows username and password";
            this.cbUseSystemLogin.CheckedChanged += new System.EventHandler(this.cbUseSystemLogin_CheckedChanged);
            //
            // checkBoxSavePassword
            //
            this.checkBoxSavePassword.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxSavePassword.Location = new System.Drawing.Point(20, 40);
            this.checkBoxSavePassword.Name = "checkBoxSavePassword";
            this.checkBoxSavePassword.Size = new System.Drawing.Size(176, 26);
            this.checkBoxSavePassword.TabIndex = 5;
            this.checkBoxSavePassword.Text = "&Remember my password";
            this.checkBoxSavePassword.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            //
            // WeblogConfigurationWizardPanelSharePointAuthentication
            //
            this.Name = "WeblogConfigurationWizardPanelSharePointAuthentication";
            this.panelMain.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        /*public string Username
        {
            get{ return textBoxUsername.Text.Trim(); }
            set{ textBoxUsername.Text = value; }
        }

        public string Password
        {
            get{ return textBoxPassword.Text.Trim(); }
            set{ textBoxPassword.Text = value; }
        }*/

        public override bool ValidatePanel()
        {
            if (!cbUseSystemLogin.Checked)
            {
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
            }

            return true;
        }

        public bool IsDirty(TemporaryBlogSettings settings)
        {
            return _basicInfoProvider.IsDirty(settings) ||
                       Credentials.Username != settings.Credentials.Username;
        }

        private void cbUseSystemLogin_CheckedChanged(object sender, System.EventArgs e)
        {
            if (cbUseSystemLogin.Checked)
            {
                textBoxUsername.Text = "";
                textBoxPassword.Text = "";
            }
            textBoxUsername.Enabled =
                textBoxPassword.Enabled =
                checkBoxSavePassword.Enabled =
                !cbUseSystemLogin.Checked;
        }

        public IBlogProviderAccountWizardDescription ProviderAccountWizard
        {
            set
            {

            }
        }

        public string AccountId
        {
            set { _basicInfoProvider.AccountId = value; }
        }

        public string HomepageUrl
        {
            get { return _basicInfoProvider.HomepageUrl; }
            set { _basicInfoProvider.HomepageUrl = value; }
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
                cbUseSystemLogin.Checked = textBoxUsername.Text == String.Empty && textBoxPassword.Text == String.Empty;
            }
        }

        public BlogInfo BlogAccount
        {
            get { return _basicInfoProvider.BlogAccount; }
        }
    }
}
