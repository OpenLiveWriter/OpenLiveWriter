// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
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
    internal class WeblogConfigurationWizardPanelStaticSiteConfig : WeblogConfigurationWizardPanel
    {
        private System.Windows.Forms.Label labelLocalSitePath;
        private System.Windows.Forms.Label labelPostsPath;
        private System.Windows.Forms.Label labelPagesPath;
        private System.Windows.Forms.Label labelBuildCmd;
        private System.Windows.Forms.Label labelPublishCmd;

        private System.Windows.Forms.TextBox textBoxLocalSitePath;
        private System.Windows.Forms.TextBox textBoxPostsPath;
        private System.Windows.Forms.TextBox textBoxPagesPath;
        private System.Windows.Forms.TextBox textBoxBuildCmd;
        private System.Windows.Forms.TextBox textBoxPublishCmd;

        private System.Windows.Forms.Button btnLocalSiteBrowse;

        private System.Windows.Forms.CheckBox checkBoxEnableBuilding;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public WeblogConfigurationWizardPanelStaticSiteConfig()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            this.labelHeader.Text = Res.Get(StringId.CWStaticSiteAddSiteTitle);
            this.labelLocalSitePath.Text = Res.Get(StringId.CWStaticSiteLocalSitePath);
            this.labelPostsPath.Text = Res.Get(StringId.CWStaticSitePostsPath);
            this.labelPagesPath.Text = Res.Get(StringId.CWStaticSitePagesPath);
            this.labelBuildCmd.Text = Res.Get(StringId.CWStaticSiteBuildCommand);
            this.labelPublishCmd.Text = Res.Get(StringId.CWStaticSitePublishCommand);
            this.checkBoxEnableBuilding.Text = Res.Get(StringId.CWStaticSiteEnableBuilding);
        }

        public override void NaturalizeLayout()
        {
            // Wizard views are very broken in the VS Form Designer, due to runtime control layout.
            if (DesignMode) return;
            
            MaximizeWidth(labelLocalSitePath);
            MaximizeWidth(labelBuildCmd);
            MaximizeWidth(labelPublishCmd);
            //MaximizeWidth(checkBoxEnableBuilding);

            LayoutHelper.NaturalizeHeightAndDistribute(3, labelLocalSitePath, textBoxLocalSitePath);
            LayoutHelper.NaturalizeHeightAndDistribute(3, labelPostsPath, textBoxPostsPath);
            LayoutHelper.NaturalizeHeightAndDistribute(3, labelPagesPath, textBoxPagesPath);
            LayoutHelper.NaturalizeHeightAndDistribute(3, labelBuildCmd, textBoxBuildCmd);
            LayoutHelper.NaturalizeHeightAndDistribute(3, labelPublishCmd, textBoxPublishCmd);
            LayoutHelper.DistributeVertically(10, false,
                new ControlGroup(labelLocalSitePath, textBoxLocalSitePath),
                new ControlGroup(labelPostsPath, textBoxPostsPath, labelPagesPath, textBoxPagesPath),
                new ControlGroup(labelBuildCmd, textBoxBuildCmd),
                new ControlGroup(labelPublishCmd, textBoxPublishCmd)
                );

            // Align Pages path label and input next to Posts path
            labelPagesPath.Left = textBoxPagesPath.Left = textBoxPostsPath.Left + textBoxPostsPath.Width + 20;

            // Align Browse button with Local Site Path top and right
            btnLocalSiteBrowse.Left = textBoxLocalSitePath.Left + textBoxLocalSitePath.Width + 3;
            btnLocalSiteBrowse.Top = textBoxLocalSitePath.Top;
            btnLocalSiteBrowse.Height = textBoxLocalSitePath.Height;
        }

        public override ConfigPanelId? PanelId
        {
            get { return ConfigPanelId.StaticSiteConfig; }
        }

        public override bool ShowProxySettingsLink
        {
            get { return false; }
        }

        public IBlogProviderAccountWizardDescription ProviderAccountWizard
        {
            set { }
        }

        public string AccountId
        {
            set { }
        }

        public string LocalSitePath
        {
            get => PathHelper.RemoveLeadingAndTrailingSlash(textBoxLocalSitePath.Text);
            set { textBoxLocalSitePath.Text = value; }
        }

        public string PostsPath
        {
            get => PathHelper.RemoveLeadingAndTrailingSlash(textBoxPostsPath.Text);
            set { textBoxPostsPath.Text = value; }
        }

        public string PagesPath
        {
            get => PathHelper.RemoveLeadingAndTrailingSlash(textBoxPagesPath.Text);
            set { textBoxPagesPath.Text = value; }
        }

        public string BuildCmd
        {
            get => textBoxBuildCmd.Text;
            set { textBoxBuildCmd.Text = value; }
        }

        public string PublishCmd
        {
            get => textBoxPublishCmd.Text;
            set { textBoxPublishCmd.Text = value; }
        }

        public bool BuildEnabled
        {
            get => checkBoxEnableBuilding.Checked;
            set { checkBoxEnableBuilding.Checked = value; }
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
                // TODO
                return new TemporaryBlogCredentials();
            }
            set
            {
                // TODO
            }
        }

        public bool IsDirty(TemporaryBlogSettings settings)
        {
            return true; // TODO

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
            if (!Directory.Exists(LocalSitePath))
            {
                ShowValidationError(textBoxLocalSitePath, MessageId.FolderNotFound, LocalSitePath);
                return false;
            }

            var postsPathFull = $"{LocalSitePath}\\{PostsPath}";
            var pagesPathFull = $"{LocalSitePath}\\{PagesPath}";

            // If the Posts path is empty or doesn't exist, display an error
            if (PagesPath.Trim().Length == 0 || !Directory.Exists(postsPathFull))
            {
                ShowValidationError(textBoxPostsPath, MessageId.FolderNotFound, postsPathFull);
                return false;
            }

            // If the Pages path is over 0 (pages enabled) and the path doesn't exist, display an error
            if (PagesPath.Trim().Length > 0 && !Directory.Exists(pagesPathFull))
            {
                ShowValidationError(textBoxPagesPath, MessageId.FolderNotFound, pagesPathFull);
                return false;
            }

            // Publish commands are required
            if(PublishCmd.Trim().Length == 0)
            {
                ShowValidationError(textBoxPublishCmd, MessageId.SSGPublishCommandRequired);
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
            this.labelLocalSitePath = new System.Windows.Forms.Label();
            this.labelPostsPath = new System.Windows.Forms.Label();
            this.labelPagesPath = new System.Windows.Forms.Label();
            this.labelBuildCmd = new System.Windows.Forms.Label();
            this.labelPublishCmd = new System.Windows.Forms.Label();

            this.checkBoxEnableBuilding = new System.Windows.Forms.CheckBox();
            this.textBoxLocalSitePath = new System.Windows.Forms.TextBox();
            this.textBoxPostsPath = new System.Windows.Forms.TextBox();
            this.textBoxPagesPath = new System.Windows.Forms.TextBox();
            this.textBoxBuildCmd = new System.Windows.Forms.TextBox();
            this.textBoxPublishCmd = new System.Windows.Forms.TextBox();

            this.btnLocalSiteBrowse = new System.Windows.Forms.Button();

            this.panelMain.SuspendLayout();
            this.SuspendLayout();
            panelMain.Controls.Add(labelLocalSitePath);
            panelMain.Controls.Add(textBoxLocalSitePath);
            panelMain.Controls.Add(btnLocalSiteBrowse);
            panelMain.Controls.Add(labelPostsPath);
            panelMain.Controls.Add(textBoxPostsPath);
            panelMain.Controls.Add(labelPagesPath);
            panelMain.Controls.Add(textBoxPagesPath);
            panelMain.Controls.Add(labelBuildCmd);
            panelMain.Controls.Add(textBoxBuildCmd);
            panelMain.Controls.Add(labelPublishCmd);
            panelMain.Controls.Add(textBoxPublishCmd);
            //
            // checkBoxSavePassword
            //
            /*this.checkBoxSavePassword.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxSavePassword.Location = new System.Drawing.Point(20, 98);
            this.checkBoxSavePassword.Name = "checkBoxSavePassword";
            this.checkBoxSavePassword.Size = new System.Drawing.Size(165, 26);
            this.checkBoxSavePassword.TabIndex = 5;
            this.checkBoxSavePassword.Text = "&Remember my password";
            this.checkBoxSavePassword.TextAlign = System.Drawing.ContentAlignment.TopLeft;*/

            //
            // labelLocalSitePath
            //
            this.labelLocalSitePath.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelLocalSitePath.Location = new System.Drawing.Point(20, 0);
            this.labelLocalSitePath.Name = "labelLocalSitePath";
            this.labelLocalSitePath.Size = new System.Drawing.Size(167, 13);
            this.labelLocalSitePath.TabIndex = 1;
            this.labelLocalSitePath.Text = "Path to local site:";
            //
            // textBoxLocalSitePath
            //
            this.textBoxLocalSitePath.Location = new System.Drawing.Point(20, 74);
            this.textBoxLocalSitePath.Name = "labelLocalSitePath";
            this.textBoxLocalSitePath.Size = new System.Drawing.Size(275, 22);
            this.textBoxLocalSitePath.TabIndex = 2;
            //
            // btnLocalSiteBrowse
            //
            this.btnLocalSiteBrowse.Location = new System.Drawing.Point(20, 74);
            this.btnLocalSiteBrowse.Name = "btnLocalSiteBrowse";
            this.btnLocalSiteBrowse.Text = "...";
            this.btnLocalSiteBrowse.FlatStyle = FlatStyle.System;
            this.btnLocalSiteBrowse.Size = new System.Drawing.Size(20, 22);
            this.btnLocalSiteBrowse.Margin = new Padding(0);
            this.btnLocalSiteBrowse.TabIndex = 3;
            this.btnLocalSiteBrowse.Click += new System.EventHandler(this.BtnLocalSiteBrowse_Click);
            //
            // labelPostsPath
            //
            this.labelPostsPath.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelPostsPath.Location = new System.Drawing.Point(20, 0);
            this.labelPostsPath.Name = "labelPostsPath";
            this.labelPostsPath.Size = new System.Drawing.Size(167, 13);
            this.labelPostsPath.TabIndex = 4;
            this.labelPostsPath.Text = "Path to posts directory: (relative)";
            //
            // textBoxPostsPath
            //
            this.textBoxPostsPath.Location = new System.Drawing.Point(20, 74);
            this.textBoxPostsPath.Name = "textBoxPostsPath";
            this.textBoxPostsPath.Size = new System.Drawing.Size(180, 22);
            this.textBoxPostsPath.TabIndex = 5;
            //
            // labelPagesPath
            //
            this.labelPagesPath.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelPagesPath.Location = new System.Drawing.Point(20, 0);
            this.labelPagesPath.Name = "labelPagesPath";
            this.labelPagesPath.Size = new System.Drawing.Size(167, 13);
            this.labelPagesPath.TabIndex = 6;
            this.labelPagesPath.Text = "Path to posts directory: (relative)";
            //
            // textBoxPagesPath
            //
            this.textBoxPagesPath.Location = new System.Drawing.Point(20, 74);
            this.textBoxPagesPath.Name = "textBoxPagesPath";
            this.textBoxPagesPath.Size = new System.Drawing.Size(180, 22);
            this.textBoxPagesPath.TabIndex = 7;
            //
            // labelBuildCmd
            //
            this.labelBuildCmd.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelBuildCmd.Location = new System.Drawing.Point(20, 0);
            this.labelBuildCmd.Name = "labelBuildCmd";
            this.labelBuildCmd.Size = new System.Drawing.Size(167, 13);
            this.labelBuildCmd.TabIndex = 8;
            this.labelBuildCmd.Text = "Build command:";
            //
            // textBoxBuildCmd
            //
            this.textBoxBuildCmd.Location = new System.Drawing.Point(20, 74);
            this.textBoxBuildCmd.Name = "textBoxBuildCmd";
            this.textBoxBuildCmd.Size = new System.Drawing.Size(275, 22);
            this.textBoxBuildCmd.TabIndex = 9;
            //
            // labelPublishCmd
            //
            this.labelPublishCmd.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelPublishCmd.Location = new System.Drawing.Point(20, 0);
            this.labelPublishCmd.Name = "labelBuildCmd";
            this.labelPublishCmd.Size = new System.Drawing.Size(167, 13);
            this.labelPublishCmd.TabIndex = 10;
            this.labelPublishCmd.Text = "Publish command:";
            //
            // textBoxPublishCmd
            //
            this.textBoxPublishCmd.Location = new System.Drawing.Point(20, 74);
            this.textBoxPublishCmd.Name = "textBoxBuildCmd";
            this.textBoxPublishCmd.Size = new System.Drawing.Size(275, 22);
            this.textBoxPublishCmd.TabIndex = 11;

            //
            // WeblogConfigurationWizardPanelBasicInfo
            //
            this.Name = "WeblogConfigurationWizardPanelStaticSiteConfig";
            this.Size = new System.Drawing.Size(432, 244);
            this.AutoSize = true;
            this.panelMain.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private void BtnLocalSiteBrowse_Click(object sender, EventArgs args)
        {
            var folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.ShowNewFolderButton = false;
            folderBrowserDialog.Description = Res.Get(StringId.CWStaticSiteLocalSiteFolderPicker);
            var result = folderBrowserDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                textBoxLocalSitePath.Text = folderBrowserDialog.SelectedPath;
            }
        }
    }
}
