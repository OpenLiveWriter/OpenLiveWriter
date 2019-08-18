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
    internal class WeblogConfigurationWizardPanelStaticSitePaths2 : WeblogConfigurationWizardPanel, IWizardPanelStaticSite
    {
        private Label labelImagesPath;
        private TextBox textBoxImagesPath;
        private Label labelOutputPath;
        private TextBox textBoxOutputPath;

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
        }

        public override void NaturalizeLayout()
        {
            // Wizard views are very broken in the VS Form Designer, due to runtime control layout.
            if (DesignMode) return;

            MaximizeWidth(labelImagesPath);
            MaximizeWidth(textBoxImagesPath);
            MaximizeWidth(labelOutputPath);
            MaximizeWidth(textBoxOutputPath);

            LayoutHelper.NaturalizeHeightAndDistributeNoScale(3, labelImagesPath, textBoxImagesPath);
            LayoutHelper.NaturalizeHeightAndDistributeNoScale(3, labelOutputPath, textBoxOutputPath);

            LayoutHelper.DistributeVerticallyNoScale(10, false,
                new ControlGroup(labelImagesPath, textBoxImagesPath),
                new ControlGroup(labelOutputPath, textBoxOutputPath)
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
            get => PathHelper.RemoveLeadingAndTrailingSlash(textBoxOutputPath.Text);
            set => textBoxOutputPath.Text = value;
        }

        public void ValidateWithConfig(StaticSiteConfig config)
            => config.Validator
            .ValidateImagesPath()
            .ValidateOutputPath();

        /// <summary>
        /// Saves panel form fields into a StaticSiteConfig
        /// </summary>
        /// <param name="config">a StaticSiteConfig instance</param>
        public void SaveToConfig(StaticSiteConfig config)
        {
            config.ImagesPath = ImagesPath;
            config.OutputPath = OutputPath;
        }

        /// <summary>
        /// Loads panel form fields from a StaticSiteConfig
        /// </summary>
        /// <param name="config">a StaticSiteConfig instance</param>
        public void LoadFromConfig(StaticSiteConfig config)
        {
            ImagesEnabled = config.ImagesEnabled;
            ImagesPath = config.ImagesPath;
            BuildingEnabled = config.BuildingEnabled;
            OutputPath = config.OutputPath;
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
            this.panelMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.labelImagesPath);
            this.panelMain.Controls.Add(this.textBoxImagesPath);
            this.panelMain.Controls.Add(this.labelOutputPath);
            this.panelMain.Controls.Add(this.textBoxOutputPath);
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
            this.labelOutputPath.Enabled = false;
            this.labelOutputPath.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelOutputPath.Location = new System.Drawing.Point(20, 40);
            this.labelOutputPath.Name = "labelOutputPath";
            this.labelOutputPath.Size = new System.Drawing.Size(83, 13);
            this.labelOutputPath.TabIndex = 2;
            this.labelOutputPath.Text = "Build output path: (relative)";
            // 
            // textBoxOutputPath
            // 
            this.textBoxOutputPath.Enabled = false;
            this.textBoxOutputPath.Location = new System.Drawing.Point(20, 17);
            this.textBoxOutputPath.Name = "textBoxOutputPath";
            this.textBoxOutputPath.Size = new System.Drawing.Size(368, 20);
            this.textBoxOutputPath.TabIndex = 3;
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