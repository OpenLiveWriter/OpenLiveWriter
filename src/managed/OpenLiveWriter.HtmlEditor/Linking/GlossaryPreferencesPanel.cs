// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework.Preferences;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.HtmlEditor.Linking
{
    /// <summary>
    /// Summary description for GlossaryPreferencesPanel.
    /// </summary>
    public class GlossaryPreferencesPanel : PreferencesPanel
    {
        private GlossaryManagementControl glossaryManagementControl1;
        private CheckBox checkBoxAutoLink;
        private readonly GlossaryPreferences _glossaryPreferences;
        private GroupBox groupboxAutoLink;
        private CheckBox checkBoxOnlyOnce;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public GlossaryPreferencesPanel()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            PanelBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Linking.Images.GlossarySmall.png");

            _glossaryPreferences = new GlossaryPreferences();
            checkBoxAutoLink.Checked = _glossaryPreferences.AutoLinkEnabled;
            checkBoxOnlyOnce.Checked = _glossaryPreferences.AutoLinkTermsOnlyOnce;
            checkBoxOnlyOnce.Enabled = _glossaryPreferences.AutoLinkEnabled;

            if (!DesignMode)
            {
                PanelName = Res.Get(StringId.GlossaryPrefName);
                checkBoxOnlyOnce.Text = Res.Get(StringId.GlossaryAutomaticallyLinkFirstTime);
                checkBoxAutoLink.Text = Res.Get(StringId.GlossaryAutomaticallyLink);
                groupboxAutoLink.Text = Res.Get(StringId.GlossaryAutomaticLinkOptions);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            LayoutHelper.FixupGroupBox(groupboxAutoLink);

            base.OnLoad(e);
        }

        public override bool PrepareSave(SwitchToPanel switchToPanel)
        {
            return true;
        }

        public override void Save()
        {
            _glossaryPreferences.Save();
            base.Save();
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
            this.glossaryManagementControl1 = new OpenLiveWriter.HtmlEditor.Linking.GlossaryManagementControl();
            this.checkBoxAutoLink = new System.Windows.Forms.CheckBox();
            this.groupboxAutoLink = new System.Windows.Forms.GroupBox();
            this.checkBoxOnlyOnce = new System.Windows.Forms.CheckBox();
            this.groupboxAutoLink.SuspendLayout();
            this.SuspendLayout();
            //
            // glossaryManagementControl1
            //
            this.glossaryManagementControl1.Location = new System.Drawing.Point(8, 32);
            this.glossaryManagementControl1.Name = "glossaryManagementControl1";
            this.glossaryManagementControl1.Size = new System.Drawing.Size(345, 224);
            this.glossaryManagementControl1.TabIndex = 1;
            //
            // checkBoxAutoLink
            //
            this.checkBoxAutoLink.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxAutoLink.Location = new System.Drawing.Point(16, 24);
            this.checkBoxAutoLink.Name = "checkBoxAutoLink";
            this.checkBoxAutoLink.Size = new System.Drawing.Size(312, 18);
            this.checkBoxAutoLink.TabIndex = 2;
            this.checkBoxAutoLink.Text = "Automatically link terms in link glossary:";
            this.checkBoxAutoLink.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.checkBoxAutoLink.UseVisualStyleBackColor = true;
            this.checkBoxAutoLink.CheckedChanged += new System.EventHandler(this.checkBoxAutoLink_CheckedChanged);
            //
            // groupboxAutoLink
            //
            this.groupboxAutoLink.Controls.Add(this.checkBoxOnlyOnce);
            this.groupboxAutoLink.Controls.Add(this.checkBoxAutoLink);
            this.groupboxAutoLink.Location = new System.Drawing.Point(8, 262);
            this.groupboxAutoLink.Name = "groupboxAutoLink";
            this.groupboxAutoLink.Size = new System.Drawing.Size(345, 77);
            this.groupboxAutoLink.TabIndex = 3;
            this.groupboxAutoLink.TabStop = false;
            this.groupboxAutoLink.Text = "Automatic Link Options";
            //
            // checkBoxOnlyOnce
            //
            this.checkBoxOnlyOnce.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxOnlyOnce.Location = new System.Drawing.Point(32, 48);
            this.checkBoxOnlyOnce.Name = "checkBoxOnlyOnce";
            this.checkBoxOnlyOnce.Size = new System.Drawing.Size(296, 22);
            this.checkBoxOnlyOnce.TabIndex = 4;
            this.checkBoxOnlyOnce.Text = "Link to each term once per post";
            this.checkBoxOnlyOnce.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.checkBoxOnlyOnce.UseVisualStyleBackColor = true;
            this.checkBoxOnlyOnce.CheckedChanged += new System.EventHandler(this.checkBoxOnlyOnce_CheckedChanged);
            //
            // GlossaryPreferencesPanel
            //
            this.AccessibleName = "Link Glossary";
            this.Controls.Add(this.groupboxAutoLink);
            this.Controls.Add(this.glossaryManagementControl1);
            this.Name = "GlossaryPreferencesPanel";
            this.PanelName = "Link Glossary";
            this.Controls.SetChildIndex(this.glossaryManagementControl1, 0);
            this.Controls.SetChildIndex(this.groupboxAutoLink, 0);
            this.groupboxAutoLink.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private void checkBoxAutoLink_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxOnlyOnce.Enabled = _glossaryPreferences.AutoLinkEnabled = checkBoxAutoLink.Checked;
            OnModified(EventArgs.Empty);
        }

        private void checkBoxOnlyOnce_CheckedChanged(object sender, EventArgs e)
        {
            _glossaryPreferences.AutoLinkTermsOnlyOnce = checkBoxOnlyOnce.Checked;
            OnModified(EventArgs.Empty);
        }

    }
}

