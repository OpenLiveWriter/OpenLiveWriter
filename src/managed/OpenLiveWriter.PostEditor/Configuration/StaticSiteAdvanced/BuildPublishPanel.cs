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

namespace OpenLiveWriter.PostEditor.Configuration.StaticSiteAdvanced
{
    /// <summary>
    /// Summary description for BuildPublishPanel.
    /// </summary>
    public class BuildPublishPanel : PreferencesPanel
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
        // private System.ComponentModel.Container components = null;

        private PreferencesController _controller;

        public BuildPublishPanel(PreferencesController controller)
            : base()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            numericUpDownCmdTimeout.Maximum = int.MaxValue;

            //UpdateStrings();

            _controller = controller;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBoxGeneral = new System.Windows.Forms.GroupBox();
            this.labelCmdTimeout = new System.Windows.Forms.Label();
            this.numericUpDownCmdTimeout = new System.Windows.Forms.NumericUpDown();
            this.checkBoxShowCommandWindows = new System.Windows.Forms.CheckBox();
            this.checkBoxEnableCmdTimeout = new System.Windows.Forms.CheckBox();
            this.groupBoxBuilding = new System.Windows.Forms.GroupBox();
            this.checkBoxBuildingEnabled = new System.Windows.Forms.CheckBox();
            this.labelBuildCommand = new System.Windows.Forms.Label();
            this.textBoxBuildCommand = new System.Windows.Forms.TextBox();
            this.labelOutputPath = new System.Windows.Forms.Label();
            this.textBoxOutputPath = new System.Windows.Forms.TextBox();
            this.groupBoxPublishing = new System.Windows.Forms.GroupBox();
            this.labelPublishCommand = new System.Windows.Forms.Label();
            this.textBoxPublishCommand = new System.Windows.Forms.TextBox();
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
            this.numericUpDownCmdTimeout.TabIndex = 1;
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
            // checkBoxEnableCmdTimeout
            // 
            this.checkBoxEnableCmdTimeout.AutoSize = true;
            this.checkBoxEnableCmdTimeout.Location = new System.Drawing.Point(16, 50);
            this.checkBoxEnableCmdTimeout.Margin = new System.Windows.Forms.Padding(13, 5, 3, 3);
            this.checkBoxEnableCmdTimeout.Name = "checkBoxEnableCmdTimeout";
            this.checkBoxEnableCmdTimeout.Size = new System.Drawing.Size(168, 19);
            this.checkBoxEnableCmdTimeout.TabIndex = 3;
            this.checkBoxEnableCmdTimeout.Text = "Enable Command Timeout";
            this.checkBoxEnableCmdTimeout.UseVisualStyleBackColor = true;
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
            // checkBoxBuildingEnabled
            // 
            this.checkBoxBuildingEnabled.AutoSize = true;
            this.checkBoxBuildingEnabled.Location = new System.Drawing.Point(16, 22);
            this.checkBoxBuildingEnabled.Margin = new System.Windows.Forms.Padding(13, 3, 3, 3);
            this.checkBoxBuildingEnabled.Name = "checkBoxBuildingEnabled";
            this.checkBoxBuildingEnabled.Size = new System.Drawing.Size(108, 19);
            this.checkBoxBuildingEnabled.TabIndex = 0;
            this.checkBoxBuildingEnabled.Text = "Enable Building";
            this.checkBoxBuildingEnabled.UseVisualStyleBackColor = true;
            // 
            // labelBuildCommand
            // 
            this.labelBuildCommand.AutoSize = true;
            this.labelBuildCommand.Location = new System.Drawing.Point(13, 44);
            this.labelBuildCommand.Margin = new System.Windows.Forms.Padding(13, 0, 3, 0);
            this.labelBuildCommand.Name = "labelBuildCommand";
            this.labelBuildCommand.Size = new System.Drawing.Size(97, 15);
            this.labelBuildCommand.TabIndex = 1;
            this.labelBuildCommand.Text = "Build Command:";
            // 
            // textBoxBuildCommand
            // 
            this.textBoxBuildCommand.Location = new System.Drawing.Point(16, 62);
            this.textBoxBuildCommand.Margin = new System.Windows.Forms.Padding(13, 3, 13, 3);
            this.textBoxBuildCommand.Name = "textBoxBuildCommand";
            this.textBoxBuildCommand.Size = new System.Drawing.Size(313, 23);
            this.textBoxBuildCommand.TabIndex = 2;
            // 
            // labelOutputPath
            // 
            this.labelOutputPath.AutoSize = true;
            this.labelOutputPath.Location = new System.Drawing.Point(13, 88);
            this.labelOutputPath.Name = "labelOutputPath";
            this.labelOutputPath.Size = new System.Drawing.Size(146, 15);
            this.labelOutputPath.TabIndex = 3;
            this.labelOutputPath.Text = "Site Output Path: (relative)";
            // 
            // textBoxOutputPath
            // 
            this.textBoxOutputPath.Location = new System.Drawing.Point(16, 106);
            this.textBoxOutputPath.Name = "textBoxOutputPath";
            this.textBoxOutputPath.Size = new System.Drawing.Size(313, 23);
            this.textBoxOutputPath.TabIndex = 4;
            // 
            // groupBoxPublishing
            // 
            this.groupBoxPublishing.Controls.Add(this.textBoxPublishCommand);
            this.groupBoxPublishing.Controls.Add(this.labelPublishCommand);
            this.groupBoxPublishing.Location = new System.Drawing.Point(8, 298);
            this.groupBoxPublishing.Name = "groupBoxPublishing";
            this.groupBoxPublishing.Size = new System.Drawing.Size(354, 73);
            this.groupBoxPublishing.TabIndex = 3;
            this.groupBoxPublishing.TabStop = false;
            this.groupBoxPublishing.Text = "Publishing";
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
            // textBoxPublishCommand
            // 
            this.textBoxPublishCommand.Location = new System.Drawing.Point(16, 37);
            this.textBoxPublishCommand.Margin = new System.Windows.Forms.Padding(13, 3, 13, 3);
            this.textBoxPublishCommand.Name = "textBoxPublishCommand";
            this.textBoxPublishCommand.Size = new System.Drawing.Size(313, 23);
            this.textBoxPublishCommand.TabIndex = 5;
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
    }
}
