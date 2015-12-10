// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.PostEditor;
using OpenLiveWriter.ApplicationFramework.Preferences;
using OpenLiveWriter.PostEditor.Configuration.Wizard;

namespace OpenLiveWriter.PostEditor.Configuration.Settings
{
    /// <summary>
    /// Summary description for AccountPanel.
    /// </summary>
    public class AccountPanel : WeblogSettingsPanel
    {
        private System.Windows.Forms.TextBox textBoxWeblogName;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.GroupBox groupBoxConfiguration;
        private System.Windows.Forms.GroupBox groupBoxName;
        private System.Windows.Forms.Button buttonEditConfiguration;
        private System.Windows.Forms.Label labelWeblogProvider;
        private System.Windows.Forms.Label labelHomepageUrl;
        private System.Windows.Forms.Label labelUsername;
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.TextBox textBoxWeblogProvider;
        private System.Windows.Forms.TextBox textBoxHomepageUrl;
        private System.Windows.Forms.TextBox textBoxUsername;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.LinkLabel linkLabelViewCapabilities;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public AccountPanel()
            : base()
        {
            InitializeComponent();
            UpdateStrings();
        }

        public AccountPanel(TemporaryBlogSettings targetBlogSettings, TemporaryBlogSettings editableBlogSettings)
            : base(targetBlogSettings, editableBlogSettings)
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            UpdateStrings();

            PanelBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Configuration.Settings.Images.AccountPanelBitmap.png");
            textBoxPassword.PasswordChar = Res.PasswordChar;

            InitializeSettings();

            // event handlers
            textBoxWeblogName.TextChanged += new EventHandler(textBoxWeblogName_TextChanged);
            buttonEditConfiguration.Click += new EventHandler(buttonEditConfiguration_Click);
            textBoxHomepageUrl.RightToLeft = System.Windows.Forms.RightToLeft.No;
            if (BidiHelper.IsRightToLeft)
                textBoxHomepageUrl.TextAlign = HorizontalAlignment.Right;
        }

        private void UpdateStrings()
        {
            this.groupBoxName.Text = Res.Get(StringId.Name);
            this.groupBoxConfiguration.Text = Res.Get(StringId.Configuration);
            this.labelName.Text = Res.Get(StringId.WeblogName);
            this.labelWeblogProvider.Text = Res.Get(StringId.WeblogProvider);
            this.labelHomepageUrl.Text = Res.Get(StringId.HomepageUrl);
            this.labelUsername.Text = Res.Get(StringId.Username);
            this.labelPassword.Text = Res.Get(StringId.Password);
            //this.labelCapabilities.Text = Res.Get(StringId.Capabilities);
            this.linkLabelViewCapabilities.Text = Res.Get(StringId.ViewWeblogCapabilities);
            this.buttonEditConfiguration.Text = Res.Get(StringId.UpdateAccountConfiguration);
            this.PanelName = Res.Get(StringId.AccountPanel);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            DisplayHelper.AutoFitSystemButton(buttonEditConfiguration);
        }

        private void InitializeSettings()
        {
            WeblogName = TemporaryBlogSettings.BlogName;

            textBoxWeblogProvider.Text = TemporaryBlogSettings.ServiceName;
            textBoxHomepageUrl.Text = TemporaryBlogSettings.HomepageUrl;
            textBoxUsername.Text = TemporaryBlogSettings.Credentials.Username;

            textBoxPassword.Text = TemporaryBlogSettings.Credentials.Password;
        }

        public override bool PrepareSave(SwitchToPanel switchToPanel)
        {
            // validate that we have a post api url
            if (WeblogName == String.Empty)
            {
                switchToPanel();
                DisplayMessage.Show(MessageId.RequiredFieldOmitted, FindForm(), TextHelper.StripAmpersands(Res.Get(StringId.WeblogName)));
                textBoxWeblogName.Focus();
                return false;
            }

            return true;
        }

        private void textBoxWeblogName_TextChanged(object sender, EventArgs e)
        {
            TemporaryBlogSettings.BlogName = WeblogName;
            TemporaryBlogSettingsModified = true;
        }

        private void buttonEditConfiguration_Click(object sender, EventArgs e)
        {
            // make a copy of the temporary settings to edit
            TemporaryBlogSettings blogSettings = TemporaryBlogSettings.Clone() as TemporaryBlogSettings;

            // edit account info
            if (WeblogConfigurationWizardController.EditTemporarySettings(FindForm(), blogSettings))
            {
                // go ahead and save the settings back
                TemporaryBlogSettings.CopyFrom(blogSettings);

                // note that settings have been modified
                TemporaryBlogSettingsModified = true;

                // reset ui
                InitializeSettings();
            }
        }

        private string WeblogName
        {
            get
            {
                return textBoxWeblogName.Text.Trim();
            }
            set
            {
                textBoxWeblogName.Text = value;
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
            this.textBoxWeblogName = new System.Windows.Forms.TextBox();
            this.labelName = new System.Windows.Forms.Label();
            this.labelWeblogProvider = new System.Windows.Forms.Label();
            this.labelHomepageUrl = new System.Windows.Forms.Label();
            this.labelUsername = new System.Windows.Forms.Label();
            this.groupBoxConfiguration = new System.Windows.Forms.GroupBox();
            this.linkLabelViewCapabilities = new System.Windows.Forms.LinkLabel();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.textBoxUsername = new System.Windows.Forms.TextBox();
            this.textBoxHomepageUrl = new System.Windows.Forms.TextBox();
            this.textBoxWeblogProvider = new System.Windows.Forms.TextBox();
            this.buttonEditConfiguration = new System.Windows.Forms.Button();
            this.labelPassword = new System.Windows.Forms.Label();
            this.groupBoxName = new System.Windows.Forms.GroupBox();
            this.groupBoxConfiguration.SuspendLayout();
            this.groupBoxName.SuspendLayout();
            this.SuspendLayout();
            //
            // textBoxWeblogName
            //
            this.textBoxWeblogName.Location = new System.Drawing.Point(16, 37);
            this.textBoxWeblogName.Name = "textBoxWeblogName";
            this.textBoxWeblogName.Size = new System.Drawing.Size(316, 20);
            this.textBoxWeblogName.TabIndex = 1;
            this.textBoxWeblogName.Text = "";
            //
            // labelName
            //
            this.labelName.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelName.Location = new System.Drawing.Point(16, 19);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(144, 16);
            this.labelName.TabIndex = 0;
            this.labelName.Text = "&Weblog Name:";
            //
            // labelWeblogProvider
            //
            this.labelWeblogProvider.AutoSize = true;
            this.labelWeblogProvider.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelWeblogProvider.Location = new System.Drawing.Point(16, 24);
            this.labelWeblogProvider.Name = "labelWeblogProvider";
            this.labelWeblogProvider.Size = new System.Drawing.Size(50, 16);
            this.labelWeblogProvider.TabIndex = 0;
            this.labelWeblogProvider.Text = "Provider:";
            //
            // labelHomepageUrl
            //
            this.labelHomepageUrl.AutoSize = true;
            this.labelHomepageUrl.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelHomepageUrl.Location = new System.Drawing.Point(16, 72);
            this.labelHomepageUrl.Name = "labelHomepageUrl";
            this.labelHomepageUrl.Size = new System.Drawing.Size(63, 16);
            this.labelHomepageUrl.TabIndex = 2;
            this.labelHomepageUrl.Text = "Homepage:";
            //
            // labelUsername
            //
            this.labelUsername.AutoSize = true;
            this.labelUsername.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelUsername.Location = new System.Drawing.Point(16, 120);
            this.labelUsername.Name = "labelUsername";
            this.labelUsername.Size = new System.Drawing.Size(60, 16);
            this.labelUsername.TabIndex = 4;
            this.labelUsername.Text = "Username:";
            //
            // groupBoxConfiguration
            //
            this.groupBoxConfiguration.Controls.Add(this.linkLabelViewCapabilities);
            this.groupBoxConfiguration.Controls.Add(this.textBoxPassword);
            this.groupBoxConfiguration.Controls.Add(this.textBoxUsername);
            this.groupBoxConfiguration.Controls.Add(this.textBoxHomepageUrl);
            this.groupBoxConfiguration.Controls.Add(this.textBoxWeblogProvider);
            this.groupBoxConfiguration.Controls.Add(this.buttonEditConfiguration);
            this.groupBoxConfiguration.Controls.Add(this.labelPassword);
            this.groupBoxConfiguration.Controls.Add(this.labelWeblogProvider);
            this.groupBoxConfiguration.Controls.Add(this.labelHomepageUrl);
            this.groupBoxConfiguration.Controls.Add(this.labelUsername);
            this.groupBoxConfiguration.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxConfiguration.Location = new System.Drawing.Point(8, 115);
            this.groupBoxConfiguration.Name = "groupBoxConfiguration";
            this.groupBoxConfiguration.Size = new System.Drawing.Size(345, 291);
            this.groupBoxConfiguration.TabIndex = 2;
            this.groupBoxConfiguration.TabStop = false;
            this.groupBoxConfiguration.Text = "Configuration";
            //
            // linkLabelViewCapabilities
            //
            this.linkLabelViewCapabilities.AutoSize = true;
            this.linkLabelViewCapabilities.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.linkLabelViewCapabilities.Location = new System.Drawing.Point(16, 216);
            this.linkLabelViewCapabilities.Name = "linkLabelViewCapabilities";
            this.linkLabelViewCapabilities.Size = new System.Drawing.Size(91, 16);
            this.linkLabelViewCapabilities.TabIndex = 9;
            this.linkLabelViewCapabilities.TabStop = true;
            this.linkLabelViewCapabilities.Text = "View Capabilities";
            this.linkLabelViewCapabilities.LinkBehavior = LinkBehavior.HoverUnderline;
            this.linkLabelViewCapabilities.LinkColor = SystemColors.HotTrack;
            this.linkLabelViewCapabilities.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelViewCapabilities_LinkClicked);
            //
            // textBoxPassword
            //
            this.textBoxPassword.Location = new System.Drawing.Point(16, 184);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.ReadOnly = true;
            this.textBoxPassword.Size = new System.Drawing.Size(316, 20);
            this.textBoxPassword.TabIndex = 7;
            this.textBoxPassword.TabStop = false;
            this.textBoxPassword.Text = "xxxxxxxxxx";
            //
            // textBoxUsername
            //
            this.textBoxUsername.Location = new System.Drawing.Point(16, 136);
            this.textBoxUsername.Name = "textBoxUsername";
            this.textBoxUsername.ReadOnly = true;
            this.textBoxUsername.Size = new System.Drawing.Size(316, 20);
            this.textBoxUsername.TabIndex = 5;
            this.textBoxUsername.TabStop = false;
            this.textBoxUsername.Text = "";
            //
            // textBoxHomepageUrl
            //
            this.textBoxHomepageUrl.Location = new System.Drawing.Point(16, 88);
            this.textBoxHomepageUrl.Name = "textBoxHomepageUrl";
            this.textBoxHomepageUrl.ReadOnly = true;
            this.textBoxHomepageUrl.Size = new System.Drawing.Size(316, 20);
            this.textBoxHomepageUrl.TabIndex = 3;
            this.textBoxHomepageUrl.TabStop = false;
            this.textBoxHomepageUrl.Text = "";
            //
            // textBoxWeblogProvider
            //
            this.textBoxWeblogProvider.Location = new System.Drawing.Point(16, 40);
            this.textBoxWeblogProvider.Name = "textBoxWeblogProvider";
            this.textBoxWeblogProvider.ReadOnly = true;
            this.textBoxWeblogProvider.Size = new System.Drawing.Size(316, 20);
            this.textBoxWeblogProvider.TabIndex = 1;
            this.textBoxWeblogProvider.TabStop = false;
            this.textBoxWeblogProvider.Text = "Blogger";
            //
            // buttonEditConfiguration
            //
            this.buttonEditConfiguration.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonEditConfiguration.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonEditConfiguration.Location = new System.Drawing.Point(154, 255);
            this.buttonEditConfiguration.Name = "buttonEditConfiguration";
            this.buttonEditConfiguration.Size = new System.Drawing.Size(178, 23);
            this.buttonEditConfiguration.TabIndex = 10;
            this.buttonEditConfiguration.Text = "&Update Account Configuration...";
            //
            // labelPassword
            //
            this.labelPassword.AutoSize = true;
            this.labelPassword.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelPassword.Location = new System.Drawing.Point(16, 168);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(57, 16);
            this.labelPassword.TabIndex = 6;
            this.labelPassword.Text = "Password:";
            //
            // groupBoxName
            //
            this.groupBoxName.Controls.Add(this.textBoxWeblogName);
            this.groupBoxName.Controls.Add(this.labelName);
            this.groupBoxName.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxName.Location = new System.Drawing.Point(8, 32);
            this.groupBoxName.Name = "groupBoxName";
            this.groupBoxName.Size = new System.Drawing.Size(345, 77);
            this.groupBoxName.TabIndex = 1;
            this.groupBoxName.TabStop = false;
            this.groupBoxName.Text = "Name";
            //
            // AccountPanel
            //
            this.Controls.Add(this.groupBoxName);
            this.Controls.Add(this.groupBoxConfiguration);
            this.Name = "AccountPanel";
            this.PanelName = "Account";
            this.Size = new System.Drawing.Size(370, 425);
            this.Controls.SetChildIndex(this.groupBoxConfiguration, 0);
            this.Controls.SetChildIndex(this.groupBoxName, 0);
            this.groupBoxConfiguration.ResumeLayout(false);
            this.groupBoxName.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private void linkLabelViewCapabilities_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            using (Blog blog = new Blog(TemporaryBlogSettings))
            {
                using (WeblogCapabilitiesForm form = new WeblogCapabilitiesForm(blog.ClientOptions))
                    form.ShowDialog(FindForm());
            }
        }

    }
}
