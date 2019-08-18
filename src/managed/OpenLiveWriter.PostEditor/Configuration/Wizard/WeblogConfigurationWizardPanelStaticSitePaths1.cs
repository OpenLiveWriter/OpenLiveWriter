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
using OpenLiveWriter.BlogClient.Clients.StaticSite;
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
    internal class WeblogConfigurationWizardPanelStaticSitePaths1 : WeblogConfigurationWizardPanel, IWizardPanelStaticSite
    {
        /// <summary>
        /// Local site path, loaded from config, used for validation
        /// </summary>
        private string _localSitePath;

        private Label labelPostsPath;
        private TextBox textBoxPostsPath;
        private TextBox textBoxPagesPath;
        private Label labelSiteUrl;
        private TextBox textBoxSiteUrl;
        private TextBox textBoxDraftsPath;
        private CheckBox checkBoxPagesInRoot;
        private Label labelDraftsPath;
        private Label labelPagesPath;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public WeblogConfigurationWizardPanelStaticSitePaths1()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            labelHeader.Text = Res.Get(StringId.CWStaticSitePathsTitle);
            labelSiteUrl.Text = Res.Get(StringId.CWStaticSitePathsSiteUrl);
            labelPostsPath.Text = Res.Get(StringId.CWStaticSitePathsPostsPath);
            labelPagesPath.Text = Res.Get(StringId.CWStaticSitePathsPagesPath);
            labelDraftsPath.Text = Res.Get(StringId.CWStaticSitePathsDraftsPath);
            checkBoxPagesInRoot.Text = Res.Get(StringId.CWStaticSitePathsPagesInRoot);
        }

        public override void NaturalizeLayout()
        {
            // Wizard views are very broken in the VS Form Designer, due to runtime control layout.
            if (DesignMode) return;

            MaximizeWidth(textBoxSiteUrl);
            MaximizeWidth(labelPostsPath);
            MaximizeWidth(textBoxPostsPath);
            MaximizeWidth(labelPagesPath);
            MaximizeWidth(textBoxPagesPath);
            MaximizeWidth(checkBoxPagesInRoot);
            MaximizeWidth(labelDraftsPath);
            MaximizeWidth(textBoxDraftsPath);

            LayoutHelper.NaturalizeHeightAndDistributeNoScale(3, labelSiteUrl, textBoxSiteUrl);
            LayoutHelper.NaturalizeHeightAndDistributeNoScale(3, labelPostsPath, textBoxPostsPath);
            LayoutHelper.NaturalizeHeightAndDistributeNoScale(3, labelPagesPath, textBoxPagesPath, checkBoxPagesInRoot);
            LayoutHelper.NaturalizeHeightAndDistributeNoScale(3, labelDraftsPath, textBoxDraftsPath);

            LayoutHelper.DistributeVerticallyNoScale(10, false,
                new ControlGroup(labelSiteUrl, textBoxSiteUrl),
                new ControlGroup(labelPostsPath, textBoxPostsPath),
                new ControlGroup(labelPagesPath, textBoxPagesPath, checkBoxPagesInRoot),
                new ControlGroup(labelDraftsPath, textBoxDraftsPath)
                );
        }

        public override ConfigPanelId? PanelId
        {
            get { return ConfigPanelId.StaticSiteConfigPaths1; }
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

        private bool _pagesEnabled = false;
        public bool PagesEnabled
        {
            get => _pagesEnabled;
            set
            {
                _pagesEnabled
                = labelPagesPath.Enabled
                = value;

                RecomputeEnabledStates();
            }
        }

        public string PagesPath
        {
            get => PathHelper.RemoveLeadingAndTrailingSlash(textBoxPagesPath.Text);
            set { textBoxPagesPath.Text = value; }
        }

        private bool _draftsEnabled = false;
        public bool DraftsEnabled
        {
            get => _draftsEnabled;
            set => _draftsEnabled 
                = labelDraftsPath.Enabled
                = textBoxDraftsPath.Enabled 
                = value;
        }

        public string DraftsPath
        {
            get => PathHelper.RemoveLeadingAndTrailingSlash(textBoxDraftsPath.Text);
            set { textBoxDraftsPath.Text = value; }
        }

        public bool PagesInRoot
        {
            get => checkBoxPagesInRoot.Checked;
            set { checkBoxPagesInRoot.Checked = value; }
        }

        public void ValidateWithConfig(StaticSiteConfig config)
            => config.Validator
            .ValidatePostsPath()
            .ValidatePagesPath()
            .ValidateDraftsPath();

        /// <summary>
        /// Saves panel form fields into a StaticSiteConfig
        /// </summary>
        /// <param name="config">a StaticSiteConfig instance</param>
        public void SaveToConfig(StaticSiteConfig config)
        {
            config.SiteUrl    = SiteUrl;
            config.PostsPath  = PostsPath;
            config.PagesPath  = PagesInRoot ? "." : PagesPath;
            config.DraftsPath = DraftsPath;
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

            PagesEnabled = config.PagesEnabled;
            PagesInRoot = config.PagesPath == ".";
            PagesPath = config.PagesPath;

            DraftsEnabled = config.DraftsEnabled;
            DraftsPath = config.DraftsPath;

            RecomputeEnabledStates();
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
            this.labelSiteUrl = new System.Windows.Forms.Label();
            this.textBoxSiteUrl = new System.Windows.Forms.TextBox();
            this.textBoxDraftsPath = new System.Windows.Forms.TextBox();
            this.checkBoxPagesInRoot = new System.Windows.Forms.CheckBox();
            this.labelPagesPath = new System.Windows.Forms.Label();
            this.labelDraftsPath = new System.Windows.Forms.Label();
            this.panelMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.labelDraftsPath);
            this.panelMain.Controls.Add(this.labelPagesPath);
            this.panelMain.Controls.Add(this.checkBoxPagesInRoot);
            this.panelMain.Controls.Add(this.textBoxDraftsPath);
            this.panelMain.Controls.Add(this.textBoxSiteUrl);
            this.panelMain.Controls.Add(this.labelSiteUrl);
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
            this.labelPostsPath.Text = "labelPostsPath";
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
            // labelSiteUrl
            // 
            this.labelSiteUrl.AutoSize = true;
            this.labelSiteUrl.Location = new System.Drawing.Point(17, 0);
            this.labelSiteUrl.Name = "labelSiteUrl";
            this.labelSiteUrl.Size = new System.Drawing.Size(60, 13);
            this.labelSiteUrl.TabIndex = 0;
            this.labelSiteUrl.Text = "labelSiteUrl";
            // 
            // textBoxSiteUrl
            // 
            this.textBoxSiteUrl.Location = new System.Drawing.Point(20, 17);
            this.textBoxSiteUrl.Name = "textBoxSiteUrl";
            this.textBoxSiteUrl.Size = new System.Drawing.Size(368, 20);
            this.textBoxSiteUrl.TabIndex = 1;
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
            this.checkBoxPagesInRoot.Size = new System.Drawing.Size(136, 17);
            this.checkBoxPagesInRoot.TabIndex = 6;
            this.checkBoxPagesInRoot.Text = "checkBoxPagesInRoot";
            this.checkBoxPagesInRoot.UseVisualStyleBackColor = true;
            this.checkBoxPagesInRoot.CheckedChanged += new System.EventHandler(this.CheckBoxPagesInRoot_CheckedChanged);
            // 
            // labelPagesPath
            // 
            this.labelPagesPath.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelPagesPath.Location = new System.Drawing.Point(20, 97);
            this.labelPagesPath.Name = "labelPagesPath";
            this.labelPagesPath.Size = new System.Drawing.Size(167, 13);
            this.labelPagesPath.TabIndex = 9;
            this.labelPagesPath.Text = "labelPagesPath";
            // 
            // labelDraftsPath
            // 
            this.labelDraftsPath.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelDraftsPath.Location = new System.Drawing.Point(20, 177);
            this.labelDraftsPath.Name = "labelDraftsPath";
            this.labelDraftsPath.Size = new System.Drawing.Size(167, 13);
            this.labelDraftsPath.TabIndex = 10;
            this.labelDraftsPath.Text = "labelDraftsPath";
            // 
            // WeblogConfigurationWizardPanelStaticSitePaths1
            // 
            this.AutoSize = true;
            this.Name = "WeblogConfigurationWizardPanelStaticSitePaths1";
            this.Size = new System.Drawing.Size(455, 291);
            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion

        private void CheckBoxPagesInRoot_CheckedChanged(object sender, EventArgs e)
        {
            RecomputeEnabledStates();
            if (PagesInRoot) PagesPath = ".";
        }

        private void RecomputeEnabledStates()
        {
            checkBoxPagesInRoot.Enabled = PagesEnabled;
            textBoxPagesPath.Enabled = PagesEnabled && !PagesInRoot;
        }
    }
}