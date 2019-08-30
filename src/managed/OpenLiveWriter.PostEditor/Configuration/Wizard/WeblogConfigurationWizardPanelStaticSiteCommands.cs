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
    internal class WeblogConfigurationWizardPanelStaticSiteCommands : WeblogConfigurationWizardPanel, IWizardPanelStaticSite
    {
        private Label labelPublishCommand;
        private TextBox textBoxBuildCommand;
        private Label labelBuildCommand;
        private TextBox textBoxPublishCommand;
        
        /// <summary>
        /// Local site path, loaded from config, used for validation
        /// </summary>
        private string _localSitePath;
        private Label labelSubtitle;
        private Label labelPublishCommandSubtitle;
        private Label labelBuildCommandSubtitle;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public WeblogConfigurationWizardPanelStaticSiteCommands()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            labelHeader.Text = Res.Get(StringId.CWStaticSiteCommandsTitle);
            labelSubtitle.Text = string.Format(Res.Get(StringId.CWStaticSiteCommandsSubtitle), Res.Get(StringId.ProductNameVersioned));
            labelBuildCommand.Text = Res.Get(StringId.CWStaticSiteCommandsBuildCommand);
            labelBuildCommandSubtitle.Text = Res.Get(StringId.CWStaticSiteCommandsBuildCommandSubtitle);
            labelPublishCommand.Text = Res.Get(StringId.CWStaticSiteCommandsPublishCommand);
            labelPublishCommandSubtitle.Text = Res.Get(StringId.CWStaticSiteCommandsPublishCommandSubtitle);

            labelBuildCommandSubtitle.ForeColor = labelPublishCommandSubtitle.ForeColor = 
                !SystemInformation.HighContrast ? Color.FromArgb(136, 136, 136) : SystemColors.GrayText;
        }

        public override void NaturalizeLayout()
        {
            // Wizard views are very broken in the VS Form Designer, due to runtime control layout.
            if (DesignMode) return;

            MaximizeWidth(labelSubtitle);
            MaximizeWidth(labelBuildCommand);
            MaximizeWidth(labelBuildCommandSubtitle);
            MaximizeWidth(textBoxBuildCommand);
            MaximizeWidth(labelPublishCommand);
            MaximizeWidth(labelPublishCommandSubtitle);
            MaximizeWidth(textBoxPublishCommand);

            LayoutHelper.NaturalizeHeight(labelSubtitle);
            LayoutHelper.NaturalizeHeightAndDistributeNoScale(3, labelBuildCommand, labelBuildCommandSubtitle, textBoxBuildCommand);
            LayoutHelper.NaturalizeHeightAndDistributeNoScale(3, labelPublishCommand, labelPublishCommandSubtitle, textBoxPublishCommand);

            LayoutHelper.DistributeVerticallyNoScale(10, false,
                labelSubtitle,
                new ControlGroup(labelBuildCommand, labelBuildCommandSubtitle, textBoxBuildCommand),
                new ControlGroup(labelPublishCommand, labelPublishCommandSubtitle, textBoxPublishCommand)
                );
        }

        public override ConfigPanelId? PanelId
        {
            get { return ConfigPanelId.StaticSiteConfigCommands; }
        }

        public string PublishCommand
        {
            get => textBoxPublishCommand.Text;
            set => textBoxPublishCommand.Text = value;
        }

        private bool _buildingEnabled = false;
        public bool BuildingEnabled
        {
            get => _buildingEnabled;
            set => _buildingEnabled 
                = labelBuildCommand.Enabled 
                = labelBuildCommandSubtitle.Enabled 
                = textBoxBuildCommand.Enabled
                = value;
        }

        public string BuildCommand
        {
            get => textBoxBuildCommand.Text;
            set => textBoxBuildCommand.Text = value;
        }

        public void ValidateWithConfig(StaticSiteConfig config)
            => config.Validator
            .ValidateBuildCommand()
            .ValidatePublishCommand();

        /// <summary>
        /// Saves panel form fields into a StaticSiteConfig
        /// </summary>
        /// <param name="config">a StaticSiteConfig instance</param>
        public void SaveToConfig(StaticSiteConfig config)
        {
            config.BuildCommand = BuildCommand;
            config.PublishCommand = PublishCommand;
        }

        /// <summary>
        /// Loads panel form fields from a StaticSiteConfig
        /// </summary>
        /// <param name="config">a StaticSiteConfig instance</param>
        public void LoadFromConfig(StaticSiteConfig config)
        {
            _localSitePath = config.LocalSitePath;

            BuildingEnabled = config.BuildingEnabled;
            BuildCommand = config.BuildCommand;
            PublishCommand = config.PublishCommand;
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
            this.labelPublishCommand = new System.Windows.Forms.Label();
            this.textBoxBuildCommand = new System.Windows.Forms.TextBox();
            this.labelBuildCommand = new System.Windows.Forms.Label();
            this.textBoxPublishCommand = new System.Windows.Forms.TextBox();
            this.labelSubtitle = new System.Windows.Forms.Label();
            this.labelBuildCommandSubtitle = new System.Windows.Forms.Label();
            this.labelPublishCommandSubtitle = new System.Windows.Forms.Label();
            this.panelMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.labelSubtitle);
            this.panelMain.Controls.Add(this.labelBuildCommand);
            this.panelMain.Controls.Add(this.labelBuildCommandSubtitle);
            this.panelMain.Controls.Add(this.textBoxBuildCommand);
            this.panelMain.Controls.Add(this.labelPublishCommandSubtitle);
            this.panelMain.Controls.Add(this.labelPublishCommand);
            this.panelMain.Controls.Add(this.textBoxPublishCommand);
            this.panelMain.Size = new System.Drawing.Size(435, 242);
            // 
            // labelPublishCommand
            // 
            this.labelPublishCommand.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelPublishCommand.Location = new System.Drawing.Point(20, 125);
            this.labelPublishCommand.Name = "labelPublishCommand";
            this.labelPublishCommand.Size = new System.Drawing.Size(167, 13);
            this.labelPublishCommand.TabIndex = 4;
            this.labelPublishCommand.Text = "labelPublishCommand";
            // 
            // textBoxBuildCommand
            // 
            this.textBoxBuildCommand.Location = new System.Drawing.Point(20, 86);
            this.textBoxBuildCommand.Name = "textBoxBuildCommand";
            this.textBoxBuildCommand.Size = new System.Drawing.Size(368, 20);
            this.textBoxBuildCommand.TabIndex = 3;
            // 
            // labelBuildCommand
            // 
            this.labelBuildCommand.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelBuildCommand.Location = new System.Drawing.Point(20, 52);
            this.labelBuildCommand.Name = "labelBuildCommand";
            this.labelBuildCommand.Size = new System.Drawing.Size(83, 13);
            this.labelBuildCommand.TabIndex = 1;
            this.labelBuildCommand.Text = "labelBuildCommand";
            // 
            // textBoxPublishCommand
            // 
            this.textBoxPublishCommand.Location = new System.Drawing.Point(20, 154);
            this.textBoxPublishCommand.Name = "textBoxPublishCommand";
            this.textBoxPublishCommand.Size = new System.Drawing.Size(368, 20);
            this.textBoxPublishCommand.TabIndex = 6;
            // 
            // labelSubtitle
            // 
            this.labelSubtitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelSubtitle.Location = new System.Drawing.Point(20, 0);
            this.labelSubtitle.Name = "labelSubtitle";
            this.labelSubtitle.Size = new System.Drawing.Size(64, 13);
            this.labelSubtitle.TabIndex = 0;
            this.labelSubtitle.Text = "labelSubtitle";
            // 
            // labelBuildCommandSubtitle
            // 
            this.labelBuildCommandSubtitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelBuildCommandSubtitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelBuildCommandSubtitle.Location = new System.Drawing.Point(20, 70);
            this.labelBuildCommandSubtitle.Name = "labelBuildCommandSubtitle";
            this.labelBuildCommandSubtitle.Size = new System.Drawing.Size(83, 13);
            this.labelBuildCommandSubtitle.TabIndex = 2;
            this.labelBuildCommandSubtitle.Text = "labelBuildCommandSubtitle";
            // 
            // labelPublishCommandSubtitle
            // 
            this.labelPublishCommandSubtitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelPublishCommandSubtitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPublishCommandSubtitle.Location = new System.Drawing.Point(20, 138);
            this.labelPublishCommandSubtitle.Name = "labelPublishCommandSubtitle";
            this.labelPublishCommandSubtitle.Size = new System.Drawing.Size(167, 13);
            this.labelPublishCommandSubtitle.TabIndex = 5;
            this.labelPublishCommandSubtitle.Text = "labelPublishCommandSubtitle";
            // 
            // WeblogConfigurationWizardPanelStaticSiteCommands
            // 
            this.AutoSize = true;
            this.Name = "WeblogConfigurationWizardPanelStaticSiteCommands";
            this.Size = new System.Drawing.Size(455, 291);
            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion
    }
}