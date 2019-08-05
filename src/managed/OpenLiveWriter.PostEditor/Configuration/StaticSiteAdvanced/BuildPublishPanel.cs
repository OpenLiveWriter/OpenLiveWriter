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
    /// Summary description for BuildPublishPanel.
    /// </summary>
    public class BuildPublishPanel : StaticSitePreferencesPanel
    {
        private System.Windows.Forms.GroupBox groupBoxGeneral;
        private CheckBox checkBoxShowCommandWindows;
        private Label labelCmdTimeout;
        private NumericUpDown numericUpDownCmdTimeout;
        private CheckBox checkBoxEnableCmdTimeout;
        private GroupBox groupBoxBuilding;
        private TextBox textBoxBuildCommand;
        private Label labelBuildCommand;
        private CheckBox checkBoxBuildingEnabled;
        private TextBox textBoxOutputPath;
        private Label labelOutputPath;
        private GroupBox groupBoxPublishing;
        private TextBox textBoxPublishCommand;
        private Label labelPublishCommand;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public bool ShowCmdWindows
        {
            get => checkBoxShowCommandWindows.Checked;
            set => checkBoxShowCommandWindows.Checked = value;
        }

        public int CmdTimeoutMs
        {
            get
            {
                if (checkBoxEnableCmdTimeout.Checked)
                    return Convert.ToInt32(numericUpDownCmdTimeout.Value);

                return -1; // -1 for disabled
            }

            set
            {
                if (value >= 0)
                {
                    checkBoxEnableCmdTimeout.Checked = true;
                    numericUpDownCmdTimeout.Value = value;
                }
                else
                    checkBoxEnableCmdTimeout.Checked = false;

                RecomputeEnabledStates();
            }
        }

        public bool BuildingEnabled
        {
            get => checkBoxBuildingEnabled.Checked;
            set => checkBoxBuildingEnabled.Checked = value;
        }

        public string BuildCommand
        {
            get => textBoxBuildCommand.Text;
            set => textBoxBuildCommand.Text = value;
        }

        public string OutputPath
        {
            get => textBoxOutputPath.Text;
            set => textBoxOutputPath.Text = value;
        }

        public string PublishCommand
        {
            get => textBoxPublishCommand.Text;
            set => textBoxPublishCommand.Text = value;
        }

        public BuildPublishPanel() : base()
        {
            InitializeComponent();
            LocalizeStrings();
            numericUpDownCmdTimeout.Maximum = int.MaxValue;
        }

        public BuildPublishPanel(StaticSitePreferencesController controller)
            : base(controller)
        {
            InitializeComponent();
            LocalizeStrings();
            numericUpDownCmdTimeout.Maximum = int.MaxValue;
        }

        private void LocalizeStrings()
        {
            groupBoxGeneral.Text = Res.Get(StringId.SSGConfigBuildPublishGeneralGroup);
            checkBoxShowCommandWindows.Text = Res.Get(StringId.SSGConfigBuildPublishShowCmdWindows);
            labelCmdTimeout.Text = Res.Get(StringId.SSGConfigBuildPublishCmdTimeout);
            checkBoxEnableCmdTimeout.Text = Res.Get(StringId.SSGConfigBuildPublishEnableCmdTimeout);

            groupBoxBuilding.Text = Res.Get(StringId.SSGConfigBuildPublishBuildingGroup);
            labelBuildCommand.Text = Res.Get(StringId.SSGConfigBuildPublishBuildCommand);
            checkBoxBuildingEnabled.Text = Res.Get(StringId.SSGConfigBuildPublishEnableBuilding);
            labelOutputPath.Text = Res.Get(StringId.SSGConfigBuildPublishOutputPath);

            groupBoxPublishing.Text = Res.Get(StringId.SSGConfigBuildPublishPublishingGroup);
            labelPublishCommand.Text = Res.Get(StringId.SSGConfigBuildPublishPublishCommand);
        }

        public override void LoadConfig()
        {
            ShowCmdWindows = _controller.Config.ShowCmdWindows;
            CmdTimeoutMs = _controller.Config.CmdTimeoutMs;
            BuildingEnabled = _controller.Config.BuildingEnabled;
            BuildCommand = _controller.Config.BuildCommand;
            OutputPath = _controller.Config.OutputPath;
            PublishCommand = _controller.Config.PublishCommand;
        }

        public override void ValidateConfig()
            => _controller.Config.Validator
            .ValidateBuildCommand()
            .ValidateOutputPath()
            .ValidatePublishCommand();

        public override void Save()
        {
            _controller.Config.ShowCmdWindows = ShowCmdWindows;
            _controller.Config.CmdTimeoutMs = CmdTimeoutMs;
            _controller.Config.BuildingEnabled = BuildingEnabled;
            _controller.Config.BuildCommand = BuildCommand;
            _controller.Config.OutputPath = OutputPath;
            _controller.Config.PublishCommand = PublishCommand;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            RecomputeEnabledStates();
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
            this.groupBoxGeneral = new System.Windows.Forms.GroupBox();
            this.checkBoxEnableCmdTimeout = new System.Windows.Forms.CheckBox();
            this.labelCmdTimeout = new System.Windows.Forms.Label();
            this.numericUpDownCmdTimeout = new System.Windows.Forms.NumericUpDown();
            this.checkBoxShowCommandWindows = new System.Windows.Forms.CheckBox();
            this.groupBoxBuilding = new System.Windows.Forms.GroupBox();
            this.textBoxOutputPath = new System.Windows.Forms.TextBox();
            this.labelOutputPath = new System.Windows.Forms.Label();
            this.textBoxBuildCommand = new System.Windows.Forms.TextBox();
            this.labelBuildCommand = new System.Windows.Forms.Label();
            this.checkBoxBuildingEnabled = new System.Windows.Forms.CheckBox();
            this.groupBoxPublishing = new System.Windows.Forms.GroupBox();
            this.textBoxPublishCommand = new System.Windows.Forms.TextBox();
            this.labelPublishCommand = new System.Windows.Forms.Label();
            this.groupBoxGeneral.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownCmdTimeout)).BeginInit();
            this.groupBoxBuilding.SuspendLayout();
            this.groupBoxPublishing.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxGeneral
            // 
            this.groupBoxGeneral.Controls.Add(this.checkBoxEnableCmdTimeout);
            this.groupBoxGeneral.Controls.Add(this.labelCmdTimeout);
            this.groupBoxGeneral.Controls.Add(this.numericUpDownCmdTimeout);
            this.groupBoxGeneral.Controls.Add(this.checkBoxShowCommandWindows);
            this.groupBoxGeneral.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxGeneral.Location = new System.Drawing.Point(8, 32);
            this.groupBoxGeneral.Name = "groupBoxGeneral";
            this.groupBoxGeneral.Size = new System.Drawing.Size(345, 111);
            this.groupBoxGeneral.TabIndex = 1;
            this.groupBoxGeneral.TabStop = false;
            this.groupBoxGeneral.Text = "General";
            // 
            // checkBoxEnableCmdTimeout
            // 
            this.checkBoxEnableCmdTimeout.AutoSize = true;
            this.checkBoxEnableCmdTimeout.Location = new System.Drawing.Point(16, 50);
            this.checkBoxEnableCmdTimeout.Margin = new System.Windows.Forms.Padding(13, 5, 3, 3);
            this.checkBoxEnableCmdTimeout.Name = "checkBoxEnableCmdTimeout";
            this.checkBoxEnableCmdTimeout.Size = new System.Drawing.Size(168, 19);
            this.checkBoxEnableCmdTimeout.TabIndex = 1;
            this.checkBoxEnableCmdTimeout.Text = "Enable Command Timeout";
            this.checkBoxEnableCmdTimeout.UseVisualStyleBackColor = true;
            this.checkBoxEnableCmdTimeout.CheckedChanged += new System.EventHandler(this.CheckBoxEnableCmdTimeout_CheckedChanged);
            // 
            // labelCmdTimeout
            // 
            this.labelCmdTimeout.AutoSize = true;
            this.labelCmdTimeout.Location = new System.Drawing.Point(13, 77);
            this.labelCmdTimeout.Name = "labelCmdTimeout";
            this.labelCmdTimeout.Size = new System.Drawing.Size(141, 15);
            this.labelCmdTimeout.TabIndex = 2;
            this.labelCmdTimeout.Text = "Command Timeout (ms):";
            // 
            // numericUpDownCmdTimeout
            // 
            this.numericUpDownCmdTimeout.Location = new System.Drawing.Point(160, 75);
            this.numericUpDownCmdTimeout.Maximum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownCmdTimeout.Name = "numericUpDownCmdTimeout";
            this.numericUpDownCmdTimeout.Size = new System.Drawing.Size(169, 23);
            this.numericUpDownCmdTimeout.TabIndex = 3;
            // 
            // checkBoxShowCommandWindows
            // 
            this.checkBoxShowCommandWindows.AutoSize = true;
            this.checkBoxShowCommandWindows.Location = new System.Drawing.Point(16, 23);
            this.checkBoxShowCommandWindows.Margin = new System.Windows.Forms.Padding(13, 3, 3, 3);
            this.checkBoxShowCommandWindows.Name = "checkBoxShowCommandWindows";
            this.checkBoxShowCommandWindows.Size = new System.Drawing.Size(167, 19);
            this.checkBoxShowCommandWindows.TabIndex = 0;
            this.checkBoxShowCommandWindows.Text = "Show Command Windows";
            this.checkBoxShowCommandWindows.UseVisualStyleBackColor = true;
            // 
            // groupBoxBuilding
            // 
            this.groupBoxBuilding.Controls.Add(this.textBoxOutputPath);
            this.groupBoxBuilding.Controls.Add(this.labelOutputPath);
            this.groupBoxBuilding.Controls.Add(this.textBoxBuildCommand);
            this.groupBoxBuilding.Controls.Add(this.labelBuildCommand);
            this.groupBoxBuilding.Controls.Add(this.checkBoxBuildingEnabled);
            this.groupBoxBuilding.Location = new System.Drawing.Point(8, 149);
            this.groupBoxBuilding.Name = "groupBoxBuilding";
            this.groupBoxBuilding.Size = new System.Drawing.Size(345, 142);
            this.groupBoxBuilding.TabIndex = 2;
            this.groupBoxBuilding.TabStop = false;
            this.groupBoxBuilding.Text = "Building";
            // 
            // textBoxOutputPath
            // 
            this.textBoxOutputPath.Location = new System.Drawing.Point(16, 106);
            this.textBoxOutputPath.Name = "textBoxOutputPath";
            this.textBoxOutputPath.Size = new System.Drawing.Size(313, 23);
            this.textBoxOutputPath.TabIndex = 8;
            // 
            // labelOutputPath
            // 
            this.labelOutputPath.AutoSize = true;
            this.labelOutputPath.Location = new System.Drawing.Point(13, 88);
            this.labelOutputPath.Name = "labelOutputPath";
            this.labelOutputPath.Size = new System.Drawing.Size(146, 15);
            this.labelOutputPath.TabIndex = 7;
            this.labelOutputPath.Text = "Site Output Path: (relative)";
            // 
            // textBoxBuildCommand
            // 
            this.textBoxBuildCommand.Location = new System.Drawing.Point(16, 62);
            this.textBoxBuildCommand.Margin = new System.Windows.Forms.Padding(13, 3, 13, 3);
            this.textBoxBuildCommand.Name = "textBoxBuildCommand";
            this.textBoxBuildCommand.Size = new System.Drawing.Size(313, 23);
            this.textBoxBuildCommand.TabIndex = 6;
            // 
            // labelBuildCommand
            // 
            this.labelBuildCommand.AutoSize = true;
            this.labelBuildCommand.Location = new System.Drawing.Point(13, 44);
            this.labelBuildCommand.Margin = new System.Windows.Forms.Padding(13, 0, 3, 0);
            this.labelBuildCommand.Name = "labelBuildCommand";
            this.labelBuildCommand.Size = new System.Drawing.Size(97, 15);
            this.labelBuildCommand.TabIndex = 5;
            this.labelBuildCommand.Text = "Build Command:";
            // 
            // checkBoxBuildingEnabled
            // 
            this.checkBoxBuildingEnabled.AutoSize = true;
            this.checkBoxBuildingEnabled.Location = new System.Drawing.Point(16, 22);
            this.checkBoxBuildingEnabled.Margin = new System.Windows.Forms.Padding(13, 3, 3, 3);
            this.checkBoxBuildingEnabled.Name = "checkBoxBuildingEnabled";
            this.checkBoxBuildingEnabled.Size = new System.Drawing.Size(108, 19);
            this.checkBoxBuildingEnabled.TabIndex = 4;
            this.checkBoxBuildingEnabled.Text = "Enable Building";
            this.checkBoxBuildingEnabled.UseVisualStyleBackColor = true;
            this.checkBoxBuildingEnabled.CheckedChanged += new System.EventHandler(this.CheckBoxBuildingEnabled_CheckedChanged);
            // 
            // groupBoxPublishing
            // 
            this.groupBoxPublishing.Controls.Add(this.textBoxPublishCommand);
            this.groupBoxPublishing.Controls.Add(this.labelPublishCommand);
            this.groupBoxPublishing.Location = new System.Drawing.Point(8, 298);
            this.groupBoxPublishing.Name = "groupBoxPublishing";
            this.groupBoxPublishing.Size = new System.Drawing.Size(345, 73);
            this.groupBoxPublishing.TabIndex = 3;
            this.groupBoxPublishing.TabStop = false;
            this.groupBoxPublishing.Text = "Publishing";
            // 
            // textBoxPublishCommand
            // 
            this.textBoxPublishCommand.Location = new System.Drawing.Point(16, 37);
            this.textBoxPublishCommand.Margin = new System.Windows.Forms.Padding(13, 3, 13, 3);
            this.textBoxPublishCommand.Name = "textBoxPublishCommand";
            this.textBoxPublishCommand.Size = new System.Drawing.Size(313, 23);
            this.textBoxPublishCommand.TabIndex = 1;
            // 
            // labelPublishCommand
            // 
            this.labelPublishCommand.AutoSize = true;
            this.labelPublishCommand.Location = new System.Drawing.Point(13, 19);
            this.labelPublishCommand.Name = "labelPublishCommand";
            this.labelPublishCommand.Size = new System.Drawing.Size(109, 15);
            this.labelPublishCommand.TabIndex = 0;
            this.labelPublishCommand.Text = "Publish Command:";
            // 
            // BuildPublishPanel
            // 
            this.AccessibleName = "Building and Publishing";
            this.Controls.Add(this.groupBoxPublishing);
            this.Controls.Add(this.groupBoxBuilding);
            this.Controls.Add(this.groupBoxGeneral);
            this.Name = "BuildPublishPanel";
            this.PanelName = "Building and Publishing";
            this.Size = new System.Drawing.Size(370, 425);
            this.Controls.SetChildIndex(this.groupBoxGeneral, 0);
            this.Controls.SetChildIndex(this.groupBoxBuilding, 0);
            this.Controls.SetChildIndex(this.groupBoxPublishing, 0);
            this.groupBoxGeneral.ResumeLayout(false);
            this.groupBoxGeneral.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownCmdTimeout)).EndInit();
            this.groupBoxBuilding.ResumeLayout(false);
            this.groupBoxBuilding.PerformLayout();
            this.groupBoxPublishing.ResumeLayout(false);
            this.groupBoxPublishing.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion

        private void RecomputeEnabledStates()
        {
            textBoxOutputPath.Enabled 
                = textBoxBuildCommand.Enabled 
                = labelOutputPath.Enabled
                = labelBuildCommand.Enabled
                = checkBoxBuildingEnabled.Checked;

            labelCmdTimeout.Enabled 
                = numericUpDownCmdTimeout.Enabled 
                = checkBoxEnableCmdTimeout.Checked;
        }

        private void CheckBoxEnableCmdTimeout_CheckedChanged(object sender, EventArgs e)
            => RecomputeEnabledStates();

        private void CheckBoxBuildingEnabled_CheckedChanged(object sender, EventArgs e)
            => RecomputeEnabledStates();
    }
}
