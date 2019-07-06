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
    internal class WeblogConfigurationWizardPanelStaticSitePaths2 : WeblogConfigurationWizardPanel, IWizardPanelStaticSiteConfigProvider
    {
        private Label labelImagesPath;
        private TextBox textBoxImagesPath;
        private Label labelOutputPath;
        private TextBox textBoxOutputPath;
        private Label labelUrlFormat;
        private Label labelUrlFormatSubtitle;
        private TextBox textBoxUrlFormat;
        
        /// <summary>
        /// Local site path, loaded from config, used for validation
        /// </summary>
        private string _localSitePath;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public WeblogConfigurationWizardPanelStaticSitePaths2()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            this.labelHeader.Text = Res.Get(StringId.CWStaticSitePathsTitle);
            this.labelImagesPath.Text = Res.Get(StringId.CWStaticSitePathsImagesPath);
            this.labelOutputPath.Text = Res.Get(StringId.CWStaticSitePathsOutputPath);
            this.labelUrlFormat.Text = Res.Get(StringId.CWStaticSitePathsUrlFormat);
            this.labelUrlFormatSubtitle.Text = Res.Get(StringId.CWStaticSitePathsUrlFormatSubtitle);
            this.labelUrlFormatSubtitle.ForeColor = !SystemInformation.HighContrast ? Color.FromArgb(136, 136, 136) : SystemColors.GrayText;
        }

        public override void NaturalizeLayout()
        {
            // Wizard views are very broken in the VS Form Designer, due to runtime control layout.
            if (DesignMode) return;

            MaximizeWidth(labelImagesPath);
            MaximizeWidth(textBoxImagesPath);
            MaximizeWidth(labelOutputPath);
            MaximizeWidth(textBoxOutputPath);
            MaximizeWidth(labelUrlFormat);
            MaximizeWidth(labelUrlFormatSubtitle);
            MaximizeWidth(textBoxUrlFormat);

            LayoutHelper.NaturalizeHeight(labelUrlFormatSubtitle);

            LayoutHelper.NaturalizeHeightAndDistribute(3, labelImagesPath, textBoxImagesPath);
            LayoutHelper.NaturalizeHeightAndDistribute(3, labelOutputPath, textBoxOutputPath);
            LayoutHelper.NaturalizeHeightAndDistribute(3, labelUrlFormat, labelUrlFormatSubtitle, textBoxUrlFormat);

            LayoutHelper.DistributeVertically(10, false,
                new ControlGroup(labelImagesPath, textBoxImagesPath),
                new ControlGroup(labelOutputPath, textBoxOutputPath),
                new ControlGroup(labelUrlFormat, labelUrlFormatSubtitle, textBoxUrlFormat)
                );
        }

        public override ConfigPanelId? PanelId
        {
            get { return ConfigPanelId.StaticSiteConfigPaths2; }
        }

        private bool _imagesEnabled = false;
        public bool ImagesEnabled
        {
            get => _imagesEnabled;
            set => _imagesEnabled = labelImagesPath.Enabled = textBoxImagesPath.Enabled = value;
        }

        public string ImagesPath
        {
            get => PathHelper.RemoveLeadingAndTrailingSlash(textBoxImagesPath.Text);
            set => textBoxImagesPath.Text = value;
        }

        private bool _buildingEnabled = false;
        public bool BuildingEnabled
        {
            get => _buildingEnabled;
            set => _buildingEnabled = labelOutputPath.Enabled = textBoxOutputPath.Enabled = value;
        }

        public string OutputPath
        {
            get => textBoxOutputPath.Text;
            set => textBoxOutputPath.Text = value;
        }

        public string UrlFormat
        {
            get => textBoxUrlFormat.Text;
            set => textBoxUrlFormat.Text = value;
        }

        public override bool ValidatePanel()
        {
            var imagesPathFull = $"{_localSitePath}\\{ImagesPath}";
            var outputPathFull = $"{_localSitePath}\\{OutputPath}";

            // If images are enabled, and the images path is empty or doesn't exist, display an error
            if (ImagesEnabled && (ImagesPath.Trim() == string.Empty || !Directory.Exists(imagesPathFull)))
            {
                ShowValidationError(
                    textBoxImagesPath,
                    MessageId.FolderNotFound,
                    ImagesPath.Trim() == string.Empty ? "Images path empty" : imagesPathFull); // TODO Replace string from with string from resources

                return false;
            }

            // If local building is enabled, and the site output path is empty or doesn't exist, display an error
            if (BuildingEnabled && (OutputPath == string.Empty || !Directory.Exists(outputPathFull)))
            {
                ShowValidationError(
                    textBoxOutputPath,
                    MessageId.FolderNotFound,
                    OutputPath.Trim() == string.Empty ? "Output path empty" : outputPathFull); // TODO Replace string from with string from resources

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
            config.ImagesPath = ImagesPath;
            config.OutputPath = OutputPath;
            config.PostUrlFormat = UrlFormat;
        }

        /// <summary>
        /// Loads panel form fields from a StaticSiteConfig
        /// </summary>
        /// <param name="config">a StaticSiteConfig instance</param>
        public void LoadFromConfig(StaticSiteConfig config)
        {
            _localSitePath = config.LocalSitePath;
            ImagesEnabled = config.ImagesEnabled;
            ImagesPath = config.ImagesPath;
            BuildingEnabled = config.BuildingEnabled;
            OutputPath = config.OutputPath;
            UrlFormat = config.PostUrlFormat;
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
            this.labelImagesPath = new System.Windows.Forms.Label();
            this.textBoxImagesPath = new System.Windows.Forms.TextBox();
            this.labelOutputPath = new System.Windows.Forms.Label();
            this.textBoxOutputPath = new System.Windows.Forms.TextBox();
            this.labelUrlFormat = new System.Windows.Forms.Label();
            this.labelUrlFormatSubtitle = new System.Windows.Forms.Label();
            this.textBoxUrlFormat = new System.Windows.Forms.TextBox();
            this.panelMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.labelImagesPath);
            this.panelMain.Controls.Add(this.textBoxImagesPath);
            this.panelMain.Controls.Add(this.labelOutputPath);
            this.panelMain.Controls.Add(this.textBoxOutputPath);
            this.panelMain.Controls.Add(this.labelUrlFormat);
            this.panelMain.Controls.Add(this.labelUrlFormatSubtitle);
            this.panelMain.Controls.Add(this.textBoxUrlFormat);
            this.panelMain.Size = new System.Drawing.Size(435, 242);
            // 
            // labelImagesPath
            // 
            this.labelImagesPath.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelImagesPath.Location = new System.Drawing.Point(20, 0);
            this.labelImagesPath.Name = "labelImagesPath";
            this.labelImagesPath.Size = new System.Drawing.Size(167, 13);
            this.labelImagesPath.TabIndex = 0;
            this.labelImagesPath.Text = "Images path: (relative)";
            // 
            // textBoxImagesPath
            // 
            this.textBoxImagesPath.Location = new System.Drawing.Point(20, 61);
            this.textBoxImagesPath.Name = "textBoxImagesPath";
            this.textBoxImagesPath.Size = new System.Drawing.Size(368, 20);
            this.textBoxImagesPath.TabIndex = 1;
            // 
            // labelOutputPath
            // 
            this.labelOutputPath.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelOutputPath.Location = new System.Drawing.Point(20, 0);
            this.labelOutputPath.Name = "labelOutputPath";
            this.labelOutputPath.Size = new System.Drawing.Size(83, 13);
            this.labelOutputPath.TabIndex = 2;
            this.labelOutputPath.Text = "Build output path: (relative)";
            this.labelOutputPath.Enabled = false;
            // 
            // textBoxOutputPath
            // 
            this.textBoxOutputPath.Location = new System.Drawing.Point(20, 17);
            this.textBoxOutputPath.Name = "textBoxOutputPath";
            this.textBoxOutputPath.Size = new System.Drawing.Size(368, 20);
            this.textBoxOutputPath.TabIndex = 3;
            this.textBoxOutputPath.Enabled = false;
            // 
            // labelUrlFormat
            // 
            this.labelUrlFormat.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelUrlFormat.Location = new System.Drawing.Point(20, 0);
            this.labelUrlFormat.Name = "labelUrlFormat";
            this.labelUrlFormat.Size = new System.Drawing.Size(83, 13);
            this.labelUrlFormat.TabIndex = 4;
            this.labelUrlFormat.Text = "Blog post URL format:";
            // 
            // labelUrlFormatSubtitle
            // 
            this.labelUrlFormatSubtitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelUrlFormatSubtitle.Location = new System.Drawing.Point(20, 0);
            this.labelUrlFormatSubtitle.Name = "labelUrlFormatSubtitle";
            this.labelUrlFormatSubtitle.Size = new System.Drawing.Size(83, 13);
            this.labelUrlFormatSubtitle.TabIndex = 5;
            this.labelUrlFormatSubtitle.Text = "Subtitle";
            // 
            // textBoxUrlFormat
            // 
            this.textBoxUrlFormat.Location = new System.Drawing.Point(20, 113);
            this.textBoxUrlFormat.Name = "textBoxPagesPath";
            this.textBoxUrlFormat.Size = new System.Drawing.Size(368, 20);
            this.textBoxUrlFormat.TabIndex = 6;
            // 
            // WeblogConfigurationWizardPanelStaticSitePaths2
            // 
            this.AutoSize = true;
            this.Name = "WeblogConfigurationWizardPanelStaticSitePaths2";
            this.Size = new System.Drawing.Size(455, 291);
            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion
    }
}