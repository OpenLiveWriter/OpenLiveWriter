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
    internal class WeblogConfigurationWizardPanelStaticSiteFeatures : WeblogConfigurationWizardPanel, IWizardPanelStaticSite
    {
        private CheckBox checkBoxPagesEnabled;
        private CheckBox checkBoxBuildingEnabled;
        private CheckBox checkBoxImagesEnabled;
        private CheckBox checkBoxDraftsEnabled;
        private Label labelSubtitle;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public WeblogConfigurationWizardPanelStaticSiteFeatures()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            labelHeader.Text = Res.Get(StringId.CWStaticSiteFeaturesTitle);
            labelSubtitle.Text = Res.Get(StringId.CWStaticSiteFeaturesSubtitle);
            checkBoxPagesEnabled.Text = Res.Get(StringId.CWStaticSiteFeaturesPages);
            checkBoxImagesEnabled.Text = Res.Get(StringId.CWStaticSiteFeaturesImages);
            checkBoxDraftsEnabled.Text = Res.Get(StringId.CWStaticSiteFeaturesDrafts);
            checkBoxBuildingEnabled.Text = Res.Get(StringId.CWStaticSiteFeaturesBuilding);
        }

        public override void NaturalizeLayout()
        {
            // Wizard views are very broken in the VS Form Designer, due to runtime control layout.
            if (DesignMode) return;

            MaximizeWidth(labelSubtitle);
            MaximizeWidth(checkBoxPagesEnabled);
            MaximizeWidth(checkBoxDraftsEnabled);
            MaximizeWidth(checkBoxImagesEnabled);
            MaximizeWidth(checkBoxBuildingEnabled);

            LayoutHelper.NaturalizeHeight(labelSubtitle);
            LayoutHelper.NaturalizeHeightAndDistributeNoScale(3, 
                checkBoxPagesEnabled, 
                checkBoxDraftsEnabled, 
                checkBoxImagesEnabled, 
                checkBoxBuildingEnabled);

            LayoutHelper.DistributeVerticallyNoScale(10, false,
                labelSubtitle,
                new ControlGroup(checkBoxPagesEnabled,
                checkBoxDraftsEnabled,
                checkBoxImagesEnabled,
                checkBoxBuildingEnabled)
                );
        }

        public override ConfigPanelId? PanelId
        {
            get { return ConfigPanelId.StaticSiteConfigCapabilities; }
        }

        public bool PagesEnabled
        {
            get => checkBoxPagesEnabled.Checked;
            set => checkBoxPagesEnabled.Checked = value;
        }

        public bool DraftsEnabled
        {
            get => checkBoxDraftsEnabled.Checked;
            set => checkBoxDraftsEnabled.Checked = value;
        }

        public bool ImagesEnabled
        {
            get => checkBoxImagesEnabled.Checked;
            set => checkBoxImagesEnabled.Checked = value;
        }

        public bool BuildingEnabled
        {
            get => checkBoxBuildingEnabled.Checked;
            set => checkBoxBuildingEnabled.Checked = value;
        }

        // No validation is required on this panel
        public void ValidateWithConfig(StaticSiteConfig config) { }

        /// <summary>
        /// Saves panel form fields into a StaticSiteConfig
        /// </summary>
        /// <param name="config">a StaticSiteConfig instance</param>
        public void SaveToConfig(StaticSiteConfig config)
        {
            config.PagesEnabled = PagesEnabled;
            config.DraftsEnabled = DraftsEnabled;
            config.ImagesEnabled = ImagesEnabled;
            config.BuildingEnabled = BuildingEnabled;
        }

        /// <summary>
        /// Loads panel form fields from a StaticSiteConfig
        /// </summary>
        /// <param name="config">a StaticSiteConfig instance</param>
        public void LoadFromConfig(StaticSiteConfig config)
        {
            PagesEnabled = config.PagesEnabled;
            DraftsEnabled = config.DraftsEnabled;
            ImagesEnabled = config.ImagesEnabled;
            BuildingEnabled = config.BuildingEnabled;
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
            this.checkBoxPagesEnabled = new System.Windows.Forms.CheckBox();
            this.labelSubtitle = new System.Windows.Forms.Label();
            this.checkBoxDraftsEnabled = new System.Windows.Forms.CheckBox();
            this.checkBoxImagesEnabled = new System.Windows.Forms.CheckBox();
            this.checkBoxBuildingEnabled = new System.Windows.Forms.CheckBox();
            this.panelMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.checkBoxBuildingEnabled);
            this.panelMain.Controls.Add(this.checkBoxImagesEnabled);
            this.panelMain.Controls.Add(this.checkBoxDraftsEnabled);
            this.panelMain.Controls.Add(this.labelSubtitle);
            this.panelMain.Controls.Add(this.checkBoxPagesEnabled);
            this.panelMain.Size = new System.Drawing.Size(435, 242);
            // 
            // checkBoxPagesEnabled
            // 
            this.checkBoxPagesEnabled.Location = new System.Drawing.Point(20, 31);
            this.checkBoxPagesEnabled.Name = "checkBoxPagesEnabled";
            this.checkBoxPagesEnabled.Size = new System.Drawing.Size(104, 24);
            this.checkBoxPagesEnabled.TabIndex = 1;
            this.checkBoxPagesEnabled.Text = "checkBoxPagesEnabled";
            this.checkBoxPagesEnabled.UseVisualStyleBackColor = true;
            // 
            // labelSubtitle
            // 
            this.labelSubtitle.Location = new System.Drawing.Point(20, 0);
            this.labelSubtitle.Name = "labelSubtitle";
            this.labelSubtitle.Size = new System.Drawing.Size(100, 23);
            this.labelSubtitle.TabIndex = 0;
            this.labelSubtitle.Text = "labelSubtitle";
            this.labelSubtitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
            // 
            // checkBoxDraftsEnabled
            // 
            this.checkBoxDraftsEnabled.Location = new System.Drawing.Point(20, 55);
            this.checkBoxDraftsEnabled.Name = "checkBoxDraftsEnabled";
            this.checkBoxDraftsEnabled.Size = new System.Drawing.Size(104, 24);
            this.checkBoxDraftsEnabled.TabIndex = 2;
            this.checkBoxDraftsEnabled.Text = "checkBoxDraftsEnabled";
            this.checkBoxDraftsEnabled.UseVisualStyleBackColor = true;
            // 
            // checkBoxImagesEnabled
            // 
            this.checkBoxImagesEnabled.Location = new System.Drawing.Point(20, 79);
            this.checkBoxImagesEnabled.Name = "checkBoxImagesEnabled";
            this.checkBoxImagesEnabled.Size = new System.Drawing.Size(104, 24);
            this.checkBoxImagesEnabled.TabIndex = 3;
            this.checkBoxImagesEnabled.Text = "checkBoxImagesEnabled";
            this.checkBoxImagesEnabled.UseVisualStyleBackColor = true;
            // 
            // checkBoxBuildingEnabled
            // 
            this.checkBoxBuildingEnabled.Location = new System.Drawing.Point(20, 103);
            this.checkBoxBuildingEnabled.Name = "checkBoxBuildingEnabled";
            this.checkBoxBuildingEnabled.Size = new System.Drawing.Size(104, 24);
            this.checkBoxBuildingEnabled.TabIndex = 4;
            this.checkBoxBuildingEnabled.Text = "checkBoxBuildingEnabled";
            this.checkBoxBuildingEnabled.UseVisualStyleBackColor = true;
            // 
            // WeblogConfigurationWizardPanelStaticSiteFeatures
            // 
            this.AutoSize = true;
            this.Name = "WeblogConfigurationWizardPanelStaticSiteFeatures";
            this.Size = new System.Drawing.Size(455, 291);
            this.panelMain.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion
    }
}