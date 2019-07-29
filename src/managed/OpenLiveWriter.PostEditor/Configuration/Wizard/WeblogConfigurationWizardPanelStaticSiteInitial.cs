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
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.PostEditor.BlogProviderButtons;

using OpenLiveWriter.BlogClient.Clients.StaticSite;

namespace OpenLiveWriter.PostEditor.Configuration.Wizard
{
    /// <summary>
    /// Summary description for WelcomeToBlogControl.
    /// </summary>
    internal class WeblogConfigurationWizardPanelStaticSiteInitial : WeblogConfigurationWizardPanel, IWizardPanelStaticSite
    {
        private System.Windows.Forms.Label labelSubtitle;
        private System.Windows.Forms.Label labelLocalSitePath;

        private System.Windows.Forms.TextBox textBoxLocalSitePath;
        private System.Windows.Forms.Button btnLocalSiteBrowse;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public WeblogConfigurationWizardPanelStaticSiteInitial()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            this.labelHeader.Text = Res.Get(StringId.CWStaticSiteInitialTitle);
            this.labelSubtitle.Text = string.Format(Res.Get(StringId.CWStaticSiteInitialSubtitle), Res.Get(StringId.ProductNameVersioned));
            this.labelLocalSitePath.Text = Res.Get(StringId.CWStaticSiteLocalSitePath);
        }

        public override void NaturalizeLayout()
        {
            // Wizard views are very broken in the VS Form Designer, due to runtime control layout.
            if (DesignMode) return;

            MaximizeWidth(labelSubtitle);
            MaximizeWidth(labelLocalSitePath);

            LayoutHelper.DistributeHorizontally(5, textBoxLocalSitePath, btnLocalSiteBrowse);
            LayoutHelper.NaturalizeHeight(labelSubtitle);
            LayoutHelper.NaturalizeHeightAndDistributeNoScale(3, labelLocalSitePath, textBoxLocalSitePath);

            // Align browse button exactly with textbox
            btnLocalSiteBrowse.Height = textBoxLocalSitePath.Height;
            btnLocalSiteBrowse.Top = textBoxLocalSitePath.Top;

            LayoutHelper.DistributeVerticallyNoScale(20, false,
                labelSubtitle,
                new ControlGroup(labelLocalSitePath, textBoxLocalSitePath, btnLocalSiteBrowse)
            );
        }

        public override ConfigPanelId? PanelId
        {
            get { return ConfigPanelId.StaticSiteConfigInitial; }
        }

        public string LocalSitePath
        {
            get => PathHelper.RemoveLeadingAndTrailingSlash(textBoxLocalSitePath.Text);
            set { textBoxLocalSitePath.Text = value; }
        }

        public void ValidateWithConfig(StaticSiteConfig config)
            => config.Validator.ValidateLocalSitePath();

        /// <summary>
        /// Saves panel form fields into a StaticSiteConfig
        /// </summary>
        /// <param name="config">a StaticSiteConfig instance</param>
        public void SaveToConfig(StaticSiteConfig config)
        {
            config.LocalSitePath = LocalSitePath;
        }

        /// <summary>
        /// Loads panel form fields from a StaticSiteConfig
        /// </summary>
        /// <param name="config">a StaticSiteConfig instance</param>
        public void LoadFromConfig(StaticSiteConfig config)
        {
            LocalSitePath = config.LocalSitePath;
            if(config.Initialised) labelSubtitle.Text = string.Format(Res.Get(StringId.CWStaticSiteInitialSubtitleAlreadyDetected), Res.Get(StringId.ProductNameVersioned));
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WeblogConfigurationWizardPanelStaticSiteInitial));
            this.labelSubtitle = new System.Windows.Forms.Label();
            this.labelLocalSitePath = new System.Windows.Forms.Label();
            this.textBoxLocalSitePath = new System.Windows.Forms.TextBox();
            this.btnLocalSiteBrowse = new System.Windows.Forms.Button();
            this.panelMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.labelSubtitle);
            this.panelMain.Controls.Add(this.labelLocalSitePath);
            this.panelMain.Controls.Add(this.textBoxLocalSitePath);
            this.panelMain.Controls.Add(this.btnLocalSiteBrowse);
            this.panelMain.Size = new System.Drawing.Size(435, 215);
            // 
            // labelSubtitle
            // 
            this.labelSubtitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelSubtitle.Location = new System.Drawing.Point(20, 0);
            this.labelSubtitle.Margin = new System.Windows.Forms.Padding(0);
            this.labelSubtitle.Name = "labelSubtitle";
            this.labelSubtitle.Size = new System.Drawing.Size(415, 75);
            this.labelSubtitle.TabIndex = 0;
            // 
            // labelLocalSitePath
            // 
            this.labelLocalSitePath.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelLocalSitePath.Location = new System.Drawing.Point(20, 75);
            this.labelLocalSitePath.Name = "labelLocalSitePath";
            this.labelLocalSitePath.Size = new System.Drawing.Size(167, 13);
            this.labelLocalSitePath.TabIndex = 1;
            this.labelLocalSitePath.Text = "Path to local site:";
            // 
            // textBoxLocalSitePath
            // 
            this.textBoxLocalSitePath.Location = new System.Drawing.Point(20, 91);
            this.textBoxLocalSitePath.Name = "textBoxLocalSitePath";
            this.textBoxLocalSitePath.Size = new System.Drawing.Size(275, 20);
            this.textBoxLocalSitePath.TabIndex = 2;
            // 
            // btnLocalSiteBrowse
            // 
            this.btnLocalSiteBrowse.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnLocalSiteBrowse.Location = new System.Drawing.Point(298, 91);
            this.btnLocalSiteBrowse.Margin = new System.Windows.Forms.Padding(0);
            this.btnLocalSiteBrowse.Name = "btnLocalSiteBrowse";
            this.btnLocalSiteBrowse.Size = new System.Drawing.Size(20, 23);
            this.btnLocalSiteBrowse.TabIndex = 3;
            this.btnLocalSiteBrowse.Text = "...";
            this.btnLocalSiteBrowse.Click += new System.EventHandler(this.BtnLocalSiteBrowse_Click);
            // 
            // WeblogConfigurationWizardPanelStaticSiteInitial
            // 
            this.AutoSize = true;
            this.Name = "WeblogConfigurationWizardPanelStaticSiteInitial";
            this.Size = new System.Drawing.Size(455, 264);
            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
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