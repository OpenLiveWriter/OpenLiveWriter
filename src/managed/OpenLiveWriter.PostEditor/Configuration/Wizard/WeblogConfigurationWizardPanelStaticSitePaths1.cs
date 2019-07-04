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
using OpenLiveWriter.BlogClient.Clients;
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
    internal class WeblogConfigurationWizardPanelStaticSitePaths1 : WeblogConfigurationWizardPanel, IWizardPanelStaticSiteConfigProvider
    {
        private Label labelPostsPath;
        private TextBox textBoxPostsPath;
        private TextBox textBoxPagesPath;
        private CheckBox checkBoxEnablePages;
        private Label labelSiteUrl;
        private TextBox textBoxSiteUrl;
        private TextBox textBoxDraftsPath;
        private CheckBox checkBoxEnableDrafts;
        private CheckBox checkBoxPagesInRoot;

        /// <summary>
        /// Local site path, loaded from config, used for validation
        /// </summary>
        private string _localSitePath;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public WeblogConfigurationWizardPanelStaticSitePaths1()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            this.labelHeader.Text = Res.Get(StringId.CWStaticSitePathsTitle);
            this.labelSiteUrl.Text = Res.Get(StringId.CWStaticSitePathsSiteUrl);
            this.labelPostsPath.Text = Res.Get(StringId.CWStaticSitePathsPostsPath);
            this.checkBoxEnablePages.Text = Res.Get(StringId.CWStaticSitePathsPagesPath);
            this.checkBoxEnableDrafts.Text = Res.Get(StringId.CWStaticSitePathsDraftsPath);
            this.checkBoxPagesInRoot.Text = Res.Get(StringId.CWStaticSitePathsPagesInRoot);
        }

        public override void NaturalizeLayout()
        {
            // Wizard views are very broken in the VS Form Designer, due to runtime control layout.
            if (DesignMode) return;

            MaximizeWidth(textBoxSiteUrl);
            MaximizeWidth(textBoxPostsPath);
            MaximizeWidth(checkBoxEnablePages);
            MaximizeWidth(textBoxPagesPath);
            MaximizeWidth(checkBoxPagesInRoot);
            MaximizeWidth(checkBoxEnableDrafts);
            MaximizeWidth(textBoxDraftsPath);

            LayoutHelper.NaturalizeHeightAndDistribute(3, labelSiteUrl, textBoxSiteUrl);
            LayoutHelper.NaturalizeHeightAndDistribute(3, labelPostsPath, textBoxPostsPath);
            LayoutHelper.NaturalizeHeightAndDistribute(3, checkBoxEnablePages, textBoxPagesPath, checkBoxPagesInRoot);
            LayoutHelper.NaturalizeHeightAndDistribute(3, checkBoxEnableDrafts, textBoxDraftsPath);

            LayoutHelper.DistributeVertically(10, false,
                new ControlGroup(labelSiteUrl, textBoxSiteUrl),
                new ControlGroup(labelPostsPath, textBoxPostsPath),
                new ControlGroup(checkBoxEnablePages, textBoxPagesPath, checkBoxPagesInRoot),
                new ControlGroup(checkBoxEnableDrafts, textBoxDraftsPath)
                );
        }

        public override ConfigPanelId? PanelId
        {
            get { return ConfigPanelId.StaticSiteConfig; }
        }

        public string SiteUrl
        {
            get => PathHelper.RemoveLeadingAndTrailingSlash(textBoxSiteUrl.Text);
            set { textBoxSiteUrl.Text = value; }
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

        public string DraftsPath
        {
            get => PathHelper.RemoveLeadingAndTrailingSlash(textBoxDraftsPath.Text);
            set { textBoxDraftsPath.Text = value; }
        }

        public bool EnablePages
        {
            get => checkBoxEnablePages.Checked;
            set { checkBoxEnablePages.Checked = value; }
        }

        public bool EnableDrafts
        {
            get => checkBoxEnableDrafts.Checked;
            set { checkBoxEnableDrafts.Checked = value; }
        }

        public bool PagesInRoot
        {
            get => checkBoxPagesInRoot.Checked;
            set { checkBoxPagesInRoot.Checked = value; }
        }

        public override bool ValidatePanel()
        {
            var postsPathFull = $"{_localSitePath}\\{PostsPath}";
            var pagesPathFull = $"{_localSitePath}\\{PagesPath}";
            var draftsPathFull = $"{_localSitePath}\\{DraftsPath}";

            // If the Posts path is empty or doesn't exist, display an error
            if (PostsPath.Trim().Length == 0 || !Directory.Exists(postsPathFull))
            {
                ShowValidationError(textBoxPostsPath, MessageId.FolderNotFound, postsPathFull);
                return false;
            }

            // If Pages are enabled and the path doesn't exist/is empty, display an error
            if (EnablePages && (PagesPath.Trim() == string.Empty || !Directory.Exists(pagesPathFull)))
            {
                ShowValidationError(
                    textBoxPagesPath, 
                    MessageId.FolderNotFound,
                    PagesPath.Trim() == string.Empty ? "Pages path empty" : pagesPathFull); // TODO Replace string from with string from resources
                return false;
            }

            // If Drafts are enabled and the path doesn't exist/is empty, display an error
            if (EnableDrafts && (DraftsPath.Trim() == string.Empty || !Directory.Exists(draftsPathFull)))
            {
                ShowValidationError(textBoxDraftsPath, 
                    MessageId.FolderNotFound,
                    DraftsPath.Trim() == string.Empty ? "Drafts path empty" : draftsPathFull); // TODO Replace string with string from resources
                return false;
            }

            return true;
        }

        /// <summary>
        /// Saves panel form fields into a StaticSiteConfig
        /// </summary>
        /// <param name="config">a StaticSiteConfig instance</param>
        public void SaveToConfig(StaticSiteConfig config)
        {
            config.SiteUrl    = SiteUrl;
            config.PostsPath  = PostsPath;
            config.PagesPath  = EnablePages ? (PagesInRoot ? "." : PagesPath) : "";
            config.DraftsPath = EnableDrafts ? DraftsPath : "";
        }

        /// <summary>
        /// Loads panel form fields from a StaticSiteConfig
        /// </summary>
        /// <param name="config">a StaticSiteConfig instance</param>
        public void LoadFromConfig(StaticSiteConfig config)
        {
            _localSitePath = config.LocalSitePath;

            SiteUrl = config.SiteUrl;
            PostsPath = config.PostsPath;

            EnablePages = config.PagesPath != string.Empty;
            PagesInRoot = config.PagesPath == ".";
            PagesPath = config.PagesPath;

            EnableDrafts = config.DraftsPath != string.Empty;
            DraftsPath = config.DraftsPath;
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
            this.labelPostsPath = new System.Windows.Forms.Label();
            this.textBoxPostsPath = new System.Windows.Forms.TextBox();
            this.textBoxPagesPath = new System.Windows.Forms.TextBox();
            this.checkBoxEnablePages = new System.Windows.Forms.CheckBox();
            this.labelSiteUrl = new System.Windows.Forms.Label();
            this.textBoxSiteUrl = new System.Windows.Forms.TextBox();
            this.checkBoxEnableDrafts = new System.Windows.Forms.CheckBox();
            this.textBoxDraftsPath = new System.Windows.Forms.TextBox();
            this.checkBoxPagesInRoot = new System.Windows.Forms.CheckBox();
            this.panelMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.checkBoxPagesInRoot);
            this.panelMain.Controls.Add(this.textBoxDraftsPath);
            this.panelMain.Controls.Add(this.checkBoxEnableDrafts);
            this.panelMain.Controls.Add(this.textBoxSiteUrl);
            this.panelMain.Controls.Add(this.labelSiteUrl);
            this.panelMain.Controls.Add(this.checkBoxEnablePages);
            this.panelMain.Controls.Add(this.labelPostsPath);
            this.panelMain.Controls.Add(this.textBoxPostsPath);
            this.panelMain.Controls.Add(this.textBoxPagesPath);
            this.panelMain.Size = new System.Drawing.Size(435, 242);
            // 
            // labelPostsPath
            // 
            this.labelPostsPath.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelPostsPath.Location = new System.Drawing.Point(20, 45);
            this.labelPostsPath.Name = "labelPostsPath";
            this.labelPostsPath.Size = new System.Drawing.Size(167, 13);
            this.labelPostsPath.TabIndex = 2;
            this.labelPostsPath.Text = "Posts path: (relative)";
            // 
            // textBoxPostsPath
            // 
            this.textBoxPostsPath.Location = new System.Drawing.Point(20, 61);
            this.textBoxPostsPath.Name = "textBoxPostsPath";
            this.textBoxPostsPath.Size = new System.Drawing.Size(368, 20);
            this.textBoxPostsPath.TabIndex = 3;
            // 
            // textBoxPagesPath
            // 
            this.textBoxPagesPath.Enabled = false;
            this.textBoxPagesPath.Location = new System.Drawing.Point(20, 113);
            this.textBoxPagesPath.Name = "textBoxPagesPath";
            this.textBoxPagesPath.Size = new System.Drawing.Size(368, 20);
            this.textBoxPagesPath.TabIndex = 5;
            // 
            // checkBoxEnablePages
            // 
            this.checkBoxEnablePages.AutoSize = true;
            this.checkBoxEnablePages.Location = new System.Drawing.Point(20, 90);
            this.checkBoxEnablePages.Name = "checkBoxEnablePages";
            this.checkBoxEnablePages.Size = new System.Drawing.Size(126, 17);
            this.checkBoxEnablePages.TabIndex = 4;
            this.checkBoxEnablePages.Text = "Pages path: (relative)";
            this.checkBoxEnablePages.UseVisualStyleBackColor = true;
            this.checkBoxEnablePages.CheckedChanged += new System.EventHandler(this.CheckBoxEnablePages_CheckedChanged);
            // 
            // labelSiteUrl
            // 
            this.labelSiteUrl.AutoSize = true;
            this.labelSiteUrl.Location = new System.Drawing.Point(17, 0);
            this.labelSiteUrl.Name = "labelSiteUrl";
            this.labelSiteUrl.Size = new System.Drawing.Size(83, 13);
            this.labelSiteUrl.TabIndex = 0;
            this.labelSiteUrl.Text = "Public site URL:";
            // 
            // textBoxSiteUrl
            // 
            this.textBoxSiteUrl.Location = new System.Drawing.Point(20, 17);
            this.textBoxSiteUrl.Name = "textBoxSiteUrl";
            this.textBoxSiteUrl.Size = new System.Drawing.Size(368, 20);
            this.textBoxSiteUrl.TabIndex = 1;
            // 
            // checkBoxEnableDrafts
            // 
            this.checkBoxEnableDrafts.AutoSize = true;
            this.checkBoxEnableDrafts.Location = new System.Drawing.Point(20, 170);
            this.checkBoxEnableDrafts.Name = "checkBoxEnableDrafts";
            this.checkBoxEnableDrafts.Size = new System.Drawing.Size(124, 17);
            this.checkBoxEnableDrafts.TabIndex = 7;
            this.checkBoxEnableDrafts.Text = "Drafts path: (relative)";
            this.checkBoxEnableDrafts.UseVisualStyleBackColor = true;
            this.checkBoxEnableDrafts.CheckedChanged += new System.EventHandler(this.CheckBoxEnableDrafts_CheckedChanged);
            // 
            // textBoxDraftsPath
            // 
            this.textBoxDraftsPath.Enabled = false;
            this.textBoxDraftsPath.Location = new System.Drawing.Point(20, 193);
            this.textBoxDraftsPath.Name = "textBoxDraftsPath";
            this.textBoxDraftsPath.Size = new System.Drawing.Size(368, 20);
            this.textBoxDraftsPath.TabIndex = 8;
            // 
            // checkBoxPagesInRoot
            // 
            this.checkBoxPagesInRoot.AutoSize = true;
            this.checkBoxPagesInRoot.Enabled = false;
            this.checkBoxPagesInRoot.Location = new System.Drawing.Point(20, 139);
            this.checkBoxPagesInRoot.Name = "checkBoxPagesInRoot";
            this.checkBoxPagesInRoot.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.checkBoxPagesInRoot.Size = new System.Drawing.Size(139, 17);
            this.checkBoxPagesInRoot.TabIndex = 6;
            this.checkBoxPagesInRoot.Text = "Pages stored in site root";
            this.checkBoxPagesInRoot.UseVisualStyleBackColor = true;
            this.checkBoxPagesInRoot.CheckedChanged += new System.EventHandler(this.CheckBoxPagesInRoot_CheckedChanged);
            // 
            // WeblogConfigurationWizardPanelStaticSitePaths
            // 
            this.AutoSize = true;
            this.Name = "WeblogConfigurationWizardPanelStaticSitePaths";
            this.Size = new System.Drawing.Size(455, 291);
            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion

        private void CheckBoxEnablePages_CheckedChanged(object sender, EventArgs e)
        {
            RecomputeEnabledStates();
        }

        private void CheckBoxPagesInRoot_CheckedChanged(object sender, EventArgs e)
        {
            RecomputeEnabledStates();
            if (PagesInRoot) PagesPath = ".";
        }

        private void CheckBoxEnableDrafts_CheckedChanged(object sender, EventArgs e)
        {
            RecomputeEnabledStates();
        }

        private void RecomputeEnabledStates()
        {
            textBoxPagesPath.Enabled = EnablePages && !PagesInRoot;
            checkBoxPagesInRoot.Enabled = EnablePages;
            textBoxDraftsPath.Enabled = EnableDrafts;
        }
    }
}