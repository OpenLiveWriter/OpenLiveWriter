// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework.Preferences;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor
{
    class PrivacyPreferencesPanel : PreferencesPanel
    {
        public PrivacyPreferencesPanel()
        {
            _loading = true;
            InitializeComponent();

            PanelName = Res.Get(StringId.PrivacyPreferencesPanelName);
            PanelBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.PreferencesPrivacy.png");
            _labelPrivacyExplanation.Text = Res.Get(StringId.PrivacyPreferencesPrivacyExplanation);
            _linkLabelPrivacyStatement.Text = Res.Get(StringId.PrivacyPreferencesPrivacyStatement);
            _linkLabelCodeOfConduct.Text = Res.Get(StringId.PrivacyPreferencesCodeOfConduct);

            _loading = false;
        }

        private System.Windows.Forms.Label _labelPrivacyExplanation;
        private System.Windows.Forms.LinkLabel _linkLabelPrivacyStatement;
        private System.Windows.Forms.LinkLabel _linkLabelCodeOfConduct;
        private readonly bool _loading = false;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            LayoutHelper.NaturalizeHeightAndDistribute(10, _labelPrivacyExplanation, _linkLabelPrivacyStatement);
            LayoutHelper.NaturalizeHeightAndDistribute(2, _linkLabelPrivacyStatement, _linkLabelCodeOfConduct);
        }

        private void InitializeComponent()
        {
            this._labelPrivacyExplanation = new System.Windows.Forms.Label();
            this._linkLabelPrivacyStatement = new System.Windows.Forms.LinkLabel();
            this._linkLabelCodeOfConduct = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            //
            // _labelPrivacyExplanation
            //
            this._labelPrivacyExplanation.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._labelPrivacyExplanation.Location = new System.Drawing.Point(8, 32);
            this._labelPrivacyExplanation.Name = "_labelPrivacyExplanation";
            this._labelPrivacyExplanation.Size = new System.Drawing.Size(343, 57);
            this._labelPrivacyExplanation.TabIndex = 1;
            this._labelPrivacyExplanation.Text = "Your privacy is important. For more information about how Open Live Writer helps to protect it, see the:";
            //
            // _linkLabelPrivacyStatement
            //
            this._linkLabelPrivacyStatement.Location = new System.Drawing.Point(32, 32);
            this._linkLabelPrivacyStatement.Name = "_linkLabelPrivacyStatement";
            this._linkLabelPrivacyStatement.Size = new System.Drawing.Size(319, 15);
            this._linkLabelPrivacyStatement.TabIndex = 2;
            this._linkLabelPrivacyStatement.TabStop = true;
            this._linkLabelPrivacyStatement.Text = "Microsoft Privacy statement";
            this._linkLabelPrivacyStatement.LinkBehavior = LinkBehavior.HoverUnderline;
            this._linkLabelPrivacyStatement.LinkColor = SystemColors.HotTrack;
            this._linkLabelPrivacyStatement.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(linkLabelPrivacyStatement_LinkClicked);
            //
            // _linkLabelCodeOfConduct
            //
            this._linkLabelCodeOfConduct.Location = new System.Drawing.Point(32, 32);
            this._linkLabelCodeOfConduct.Name = "_linkLabelCodeOfConduct";
            this._linkLabelCodeOfConduct.Size = new System.Drawing.Size(319, 15);
            this._linkLabelCodeOfConduct.TabIndex = 4;
            this._linkLabelCodeOfConduct.TabStop = true;
            this._linkLabelCodeOfConduct.Text = "Code of Conduct";
            this._linkLabelCodeOfConduct.LinkBehavior = LinkBehavior.HoverUnderline;
            this._linkLabelCodeOfConduct.LinkColor = SystemColors.HotTrack;
            this._linkLabelCodeOfConduct.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(linkLabelCodeOfConduct_LinkClicked);
            //
            // PrivacyPreferencesPanel
            //
            this.Controls.Add(this._labelPrivacyExplanation);
            this.Controls.Add(this._linkLabelPrivacyStatement);
            this.Controls.Add(this._linkLabelCodeOfConduct);
            this.Name = "PrivacyPreferencesPanel";
            this.Controls.SetChildIndex(this._labelPrivacyExplanation, 0);
            this.Controls.SetChildIndex(this._linkLabelPrivacyStatement, 0);
            this.Controls.SetChildIndex(this._linkLabelCodeOfConduct, 0);
            this.ResumeLayout(false);
        }

        public override void Save()
        {
            base.Save();
        }

        private void checkBoxOptIn_CheckedChanged(object sender, EventArgs e)
        {
            if (!_loading)
                OnModified(EventArgs.Empty);
        }

        private static void linkLabelPrivacyStatement_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            ShellHelper.LaunchUrl("http://www.dotnetfoundation.org/privacy-policy");
        }

        private static void linkLabelCodeOfConduct_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            ShellHelper.LaunchUrl("http://www.dotnetfoundation.org/code-of-conduct");
        }
    }
}
