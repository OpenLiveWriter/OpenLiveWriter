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
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.PostEditor;
using OpenLiveWriter.ApplicationFramework.Preferences;
using OpenLiveWriter.PostEditor.Configuration.Wizard;
using OpenLiveWriter.BlogClient.Clients.StaticSite;

namespace OpenLiveWriter.PostEditor.Configuration.StaticSiteAdvanced
{
    /// <summary>
    /// Summary description for AccountPanel.
    /// </summary>
    public class GeneralPanel : StaticSitePreferencesPanel
    {
        private System.Windows.Forms.GroupBox groupBoxSetup;
        private TextBox textBoxSiteTitle;
        private Label labelSiteTitle;
        private TextBox textBoxLocalSitePath;
        private Label labelLocalSitePath;
        private TextBox textBoxSiteUrl;
        private Button buttonRunAccountWizard;
        private GroupBox groupBoxOptions;
        private Label labelAutoDetect;
        private Button buttonRunAutoDetect;
        private Label labelRunWizardAgain;
        private Label labelSiteUrl;
        private Button buttonBrowseLocalSitePath;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public string SiteTitle
        {
            get => textBoxSiteTitle.Text;
            set => textBoxSiteTitle.Text = value;
        }

        public string SiteUrl
        {
            get => textBoxSiteUrl.Text;
            set => textBoxSiteUrl.Text = value;
        }

        public string LocalSitePath
        {
            get => textBoxLocalSitePath.Text;
            set => textBoxLocalSitePath.Text = value;
        }

        public GeneralPanel() : base()
        {
            InitializeComponent();
            LocalizeStrings();
        }

        public GeneralPanel(StaticSitePreferencesController controller)
            : base(controller)
        {
            InitializeComponent();
            LocalizeStrings();
        }

        private void LocalizeStrings()
        {
            PanelName = Res.Get(StringId.SSGConfigGeneralTitle);

            groupBoxSetup.Text = Res.Get(StringId.SSGConfigGeneralSetupGroup);
            labelSiteTitle.Text = Res.Get(StringId.SSGConfigGeneralSiteTitle);
            labelSiteUrl.Text = Res.Get(StringId.SSGConfigGeneralSiteUrl);
            labelLocalSitePath.Text = Res.Get(StringId.SSGConfigGeneralLocalSitePath);

            groupBoxOptions.Text = Res.Get(StringId.SSGConfigGeneralOptionsGroup);
            labelRunWizardAgain.Text = Res.Get(StringId.SSGConfigGeneralWizardLabel);
            buttonRunAccountWizard.Text = Res.Get(StringId.SSGConfigGeneralWizardButton);
            labelAutoDetect.Text = Res.Get(StringId.SSGConfigGeneralDetectLabel);
            buttonRunAutoDetect.Text = Res.Get(StringId.SSGConfigGeneralDetectButton);
        }

        public override void LoadConfig()
        {
            SiteTitle = _controller.Config.SiteTitle;
            SiteUrl = _controller.Config.SiteUrl;
            LocalSitePath = _controller.Config.LocalSitePath;
        }

        public override void ValidateConfig()
            => _controller.Config.Validator.ValidateLocalSitePath();

        public override void Save()
        {
            _controller.Config.SiteTitle = SiteTitle;
            _controller.Config.SiteUrl = SiteUrl;
            _controller.Config.LocalSitePath = LocalSitePath;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            DisplayHelper.AutoFitSystemButton(buttonRunAccountWizard);
            DisplayHelper.AutoFitSystemButton(buttonRunAutoDetect);

            LayoutHelper.NaturalizeHeightAndDistributeNoScale(3, labelRunWizardAgain, buttonRunAccountWizard);
            LayoutHelper.NaturalizeHeightAndDistributeNoScale(3, labelAutoDetect, buttonRunAutoDetect);

            LayoutHelper.DistributeVerticallyNoScale(10, 
                new ControlGroup(
                    labelRunWizardAgain,
                    buttonRunAccountWizard),
                new ControlGroup(
                    labelAutoDetect,
                    buttonRunAutoDetect));
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GeneralPanel));
            this.groupBoxSetup = new System.Windows.Forms.GroupBox();
            this.buttonBrowseLocalSitePath = new System.Windows.Forms.Button();
            this.textBoxLocalSitePath = new System.Windows.Forms.TextBox();
            this.labelLocalSitePath = new System.Windows.Forms.Label();
            this.textBoxSiteUrl = new System.Windows.Forms.TextBox();
            this.labelSiteUrl = new System.Windows.Forms.Label();
            this.textBoxSiteTitle = new System.Windows.Forms.TextBox();
            this.labelSiteTitle = new System.Windows.Forms.Label();
            this.buttonRunAccountWizard = new System.Windows.Forms.Button();
            this.groupBoxOptions = new System.Windows.Forms.GroupBox();
            this.labelAutoDetect = new System.Windows.Forms.Label();
            this.buttonRunAutoDetect = new System.Windows.Forms.Button();
            this.labelRunWizardAgain = new System.Windows.Forms.Label();
            this.groupBoxSetup.SuspendLayout();
            this.groupBoxOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxSetup
            // 
            this.groupBoxSetup.Controls.Add(this.buttonBrowseLocalSitePath);
            this.groupBoxSetup.Controls.Add(this.textBoxLocalSitePath);
            this.groupBoxSetup.Controls.Add(this.labelLocalSitePath);
            this.groupBoxSetup.Controls.Add(this.textBoxSiteUrl);
            this.groupBoxSetup.Controls.Add(this.labelSiteUrl);
            this.groupBoxSetup.Controls.Add(this.textBoxSiteTitle);
            this.groupBoxSetup.Controls.Add(this.labelSiteTitle);
            this.groupBoxSetup.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxSetup.Location = new System.Drawing.Point(8, 32);
            this.groupBoxSetup.Name = "groupBoxSetup";
            this.groupBoxSetup.Size = new System.Drawing.Size(345, 158);
            this.groupBoxSetup.TabIndex = 1;
            this.groupBoxSetup.TabStop = false;
            this.groupBoxSetup.Text = "Setup";
            // 
            // buttonBrowseLocalSitePath
            // 
            this.buttonBrowseLocalSitePath.Location = new System.Drawing.Point(308, 125);
            this.buttonBrowseLocalSitePath.Name = "buttonBrowseLocalSitePath";
            this.buttonBrowseLocalSitePath.Size = new System.Drawing.Size(24, 24);
            this.buttonBrowseLocalSitePath.TabIndex = 6;
            this.buttonBrowseLocalSitePath.Text = "...";
            this.buttonBrowseLocalSitePath.UseVisualStyleBackColor = true;
            this.buttonBrowseLocalSitePath.Click += new System.EventHandler(this.ButtonBrowseLocalSitePath_Click);
            // 
            // textBoxLocalSitePath
            // 
            this.textBoxLocalSitePath.Location = new System.Drawing.Point(16, 126);
            this.textBoxLocalSitePath.Name = "textBoxLocalSitePath";
            this.textBoxLocalSitePath.Size = new System.Drawing.Size(286, 23);
            this.textBoxLocalSitePath.TabIndex = 5;
            // 
            // labelLocalSitePath
            // 
            this.labelLocalSitePath.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelLocalSitePath.Location = new System.Drawing.Point(16, 107);
            this.labelLocalSitePath.Name = "labelLocalSitePath";
            this.labelLocalSitePath.Size = new System.Drawing.Size(144, 16);
            this.labelLocalSitePath.TabIndex = 4;
            this.labelLocalSitePath.Text = "&Local Site Path:";
            // 
            // textBoxSiteUrl
            // 
            this.textBoxSiteUrl.Location = new System.Drawing.Point(16, 81);
            this.textBoxSiteUrl.Name = "textBoxSiteUrl";
            this.textBoxSiteUrl.Size = new System.Drawing.Size(316, 23);
            this.textBoxSiteUrl.TabIndex = 3;
            // 
            // labelSiteUrl
            // 
            this.labelSiteUrl.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelSiteUrl.Location = new System.Drawing.Point(16, 63);
            this.labelSiteUrl.Name = "labelSiteUrl";
            this.labelSiteUrl.Size = new System.Drawing.Size(144, 16);
            this.labelSiteUrl.TabIndex = 2;
            this.labelSiteUrl.Text = "Site &URL:";
            // 
            // textBoxSiteTitle
            // 
            this.textBoxSiteTitle.Location = new System.Drawing.Point(16, 37);
            this.textBoxSiteTitle.Name = "textBoxSiteTitle";
            this.textBoxSiteTitle.Size = new System.Drawing.Size(316, 23);
            this.textBoxSiteTitle.TabIndex = 1;
            // 
            // labelSiteTitle
            // 
            this.labelSiteTitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelSiteTitle.Location = new System.Drawing.Point(16, 19);
            this.labelSiteTitle.Name = "labelSiteTitle";
            this.labelSiteTitle.Size = new System.Drawing.Size(144, 16);
            this.labelSiteTitle.TabIndex = 0;
            this.labelSiteTitle.Text = "Site &Title:";
            // 
            // buttonRunAccountWizard
            // 
            this.buttonRunAccountWizard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRunAccountWizard.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonRunAccountWizard.Location = new System.Drawing.Point(188, 70);
            this.buttonRunAccountWizard.Name = "buttonRunAccountWizard";
            this.buttonRunAccountWizard.Size = new System.Drawing.Size(144, 23);
            this.buttonRunAccountWizard.TabIndex = 1;
            this.buttonRunAccountWizard.Text = "Run Account &Wizard";
            this.buttonRunAccountWizard.Click += new System.EventHandler(this.ButtonRunAccountWizard_Click);
            // 
            // groupBoxOptions
            // 
            this.groupBoxOptions.Controls.Add(this.labelAutoDetect);
            this.groupBoxOptions.Controls.Add(this.buttonRunAutoDetect);
            this.groupBoxOptions.Controls.Add(this.labelRunWizardAgain);
            this.groupBoxOptions.Controls.Add(this.buttonRunAccountWizard);
            this.groupBoxOptions.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxOptions.Location = new System.Drawing.Point(8, 196);
            this.groupBoxOptions.Name = "groupBoxOptions";
            this.groupBoxOptions.Size = new System.Drawing.Size(345, 226);
            this.groupBoxOptions.TabIndex = 2;
            this.groupBoxOptions.TabStop = false;
            this.groupBoxOptions.Text = "Options";
            // 
            // labelAutoDetect
            // 
            this.labelAutoDetect.Location = new System.Drawing.Point(13, 96);
            this.labelAutoDetect.Name = "labelAutoDetect";
            this.labelAutoDetect.Size = new System.Drawing.Size(316, 75);
            this.labelAutoDetect.TabIndex = 2;
            this.labelAutoDetect.Text = resources.GetString("labelAutoDetect.Text");
            // 
            // buttonRunAutoDetect
            // 
            this.buttonRunAutoDetect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRunAutoDetect.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonRunAutoDetect.Location = new System.Drawing.Point(188, 174);
            this.buttonRunAutoDetect.Name = "buttonRunAutoDetect";
            this.buttonRunAutoDetect.Size = new System.Drawing.Size(144, 23);
            this.buttonRunAutoDetect.TabIndex = 3;
            this.buttonRunAutoDetect.Text = "Run Auto-&Detect";
            this.buttonRunAutoDetect.Click += new System.EventHandler(this.ButtonRunAutoDetect_Click);
            // 
            // labelRunWizardAgain
            // 
            this.labelRunWizardAgain.Location = new System.Drawing.Point(16, 19);
            this.labelRunWizardAgain.Name = "labelRunWizardAgain";
            this.labelRunWizardAgain.Size = new System.Drawing.Size(316, 48);
            this.labelRunWizardAgain.TabIndex = 0;
            this.labelRunWizardAgain.Text = "You can chose to run the Account Wizard again if you wish to be guided through th" +
    "e core static site configuration options interactively.";
            // 
            // GeneralPanel
            // 
            this.AccessibleName = "General";
            this.Controls.Add(this.groupBoxSetup);
            this.Controls.Add(this.groupBoxOptions);
            this.Name = "GeneralPanel";
            this.PanelName = "General";
            this.Size = new System.Drawing.Size(370, 425);
            this.Controls.SetChildIndex(this.groupBoxOptions, 0);
            this.Controls.SetChildIndex(this.groupBoxSetup, 0);
            this.groupBoxSetup.ResumeLayout(false);
            this.groupBoxSetup.PerformLayout();
            this.groupBoxOptions.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private void ButtonRunAccountWizard_Click(object sender, EventArgs e)
            => _controller.GeneralPanel_RunAccountWizard();

        private void ButtonRunAutoDetect_Click(object sender, EventArgs e)
            => _controller.GeneralPanel_RunAutoDetect();

        private void ButtonBrowseLocalSitePath_Click(object sender, EventArgs e)
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
