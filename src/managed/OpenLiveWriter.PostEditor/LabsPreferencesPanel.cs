// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework.Preferences;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor
{
    /// <summary>
    /// Preferences panel for experimental ("Labs") features. Currently hosts the
    /// switch between the managed ribbon and the classic native Windows ribbon.
    /// </summary>
    class LabsPreferencesPanel : PreferencesPanel
    {
        public LabsPreferencesPanel()
        {
            InitializeComponent();

            PanelName = Res.Get(StringId.LabsPreferencesPanelName);
            PanelBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.PreferencesOther.png");
            _labelExplanation.Text = Res.Get(StringId.LabsPreferencesExplanation);
            _groupBoxRibbon.Text = Res.Get(StringId.LabsRibbonGroup);
            _radioRibbonManaged.Text = Res.Get(StringId.LabsRibbonManaged);
            _radioRibbonNative.Text = Res.Get(StringId.LabsRibbonNative);

            _radioRibbonManaged.Checked = !PostEditorSettings.UseNativeRibbon;
            _radioRibbonNative.Checked = PostEditorSettings.UseNativeRibbon;
            _radioRibbonManaged.CheckedChanged += ribbon_CheckedChanged;
            _radioRibbonNative.CheckedChanged += ribbon_CheckedChanged;
        }

        private GroupBox _groupBoxRibbon;
        private Label _labelExplanation;
        private RadioButton _radioRibbonManaged;
        private RadioButton _radioRibbonNative;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            LayoutHelper.FixupGroupBox(_groupBoxRibbon);
            LayoutHelper.NaturalizeHeightAndDistribute(10, _labelExplanation, _groupBoxRibbon);
        }

        private void ribbon_CheckedChanged(object sender, EventArgs e)
        {
            OnModified(EventArgs.Empty);
        }

        public override void Save()
        {
            PostEditorSettings.UseNativeRibbon = _radioRibbonNative.Checked;
        }

        private void InitializeComponent()
        {
            this._labelExplanation = new System.Windows.Forms.Label();
            this._groupBoxRibbon = new System.Windows.Forms.GroupBox();
            this._radioRibbonManaged = new System.Windows.Forms.RadioButton();
            this._radioRibbonNative = new System.Windows.Forms.RadioButton();
            this._groupBoxRibbon.SuspendLayout();
            this.SuspendLayout();
            //
            // _labelExplanation
            //
            this._labelExplanation.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._labelExplanation.Location = new System.Drawing.Point(8, 32);
            this._labelExplanation.Name = "_labelExplanation";
            this._labelExplanation.Size = new System.Drawing.Size(343, 32);
            this._labelExplanation.TabIndex = 1;
            //
            // _groupBoxRibbon
            //
            this._groupBoxRibbon.Controls.Add(this._radioRibbonNative);
            this._groupBoxRibbon.Controls.Add(this._radioRibbonManaged);
            this._groupBoxRibbon.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._groupBoxRibbon.Location = new System.Drawing.Point(8, 72);
            this._groupBoxRibbon.Name = "_groupBoxRibbon";
            this._groupBoxRibbon.Size = new System.Drawing.Size(343, 70);
            this._groupBoxRibbon.TabIndex = 2;
            this._groupBoxRibbon.TabStop = false;
            //
            // _radioRibbonManaged
            //
            this._radioRibbonManaged.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._radioRibbonManaged.Location = new System.Drawing.Point(16, 19);
            this._radioRibbonManaged.Name = "_radioRibbonManaged";
            this._radioRibbonManaged.Size = new System.Drawing.Size(311, 20);
            this._radioRibbonManaged.TabIndex = 0;
            //
            // _radioRibbonNative
            //
            this._radioRibbonNative.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._radioRibbonNative.Location = new System.Drawing.Point(16, 42);
            this._radioRibbonNative.Name = "_radioRibbonNative";
            this._radioRibbonNative.Size = new System.Drawing.Size(311, 20);
            this._radioRibbonNative.TabIndex = 1;
            //
            // LabsPreferencesPanel
            //
            this.Controls.Add(this._groupBoxRibbon);
            this.Controls.Add(this._labelExplanation);
            this.Name = "LabsPreferencesPanel";
            this.Size = new System.Drawing.Size(360, 220);
            this._groupBoxRibbon.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
