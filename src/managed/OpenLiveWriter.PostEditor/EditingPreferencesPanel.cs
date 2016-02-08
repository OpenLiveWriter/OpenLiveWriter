// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework.Preferences;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.PostEditor.Autoreplace;
using OpenLiveWriter.PostEditor.WordCount;

namespace OpenLiveWriter.PostEditor
{

    public class EditingPreferencesPanel : PreferencesPanel
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        private GroupBox groupBoxEditing;
        private CheckBox checkBoxTypographic;
        private AutoreplacePreferences _autoReplacePreferences;
        private CheckBox checkBoxSmartQuotes;
        private CheckBox checkBoxSpecialChars;
        private CheckBox checkBoxEmoticons;

        public EditingPreferencesPanel()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            if (!DesignMode)
            {
                this.groupBoxEditing.Text = Res.Get(StringId.SpellingPrefOptions);
                PanelName = Res.Get(StringId.EditingName);
                checkBoxTypographic.Text = Res.Get(StringId.AutoreplaceTypographic);
                checkBoxSmartQuotes.Text = Res.Get(StringId.AutoreplaceSmartQuotes);
                checkBoxSpecialChars.Text = Res.Get(StringId.AutoreplaceOtherChars);
                checkBoxEmoticons.Text = Res.Get(StringId.AutoreplaceEmoticons);
            }

            PanelBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Configuration.Settings.Images.EditingPanelBitmap.png");

            _autoReplacePreferences = new AutoreplacePreferences();
            _autoReplacePreferences.PreferencesModified += _autoReplacePreferences_PreferencesModified;

            checkBoxTypographic.Checked = _autoReplacePreferences.EnableTypographicReplacement;
            checkBoxTypographic.CheckedChanged += new EventHandler(checkBoxTypographic_CheckedChanged);

            checkBoxSmartQuotes.Checked = _autoReplacePreferences.EnableSmartQuotes;
            checkBoxSmartQuotes.Visible = !BidiHelper.IsRightToLeft;

            checkBoxSpecialChars.Checked = _autoReplacePreferences.EnableSpecialCharacterReplacement;
            checkBoxSpecialChars.CheckedChanged += new EventHandler(checkBoxSpecialChars_CheckedChanged);

            checkBoxEmoticons.Checked = _autoReplacePreferences.EnableEmoticonsReplacement;
            checkBoxEmoticons.CheckedChanged += new EventHandler(checkBoxEmoticons_CheckedChanged);
        }

        private bool _layedOut = false;
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!DesignMode && !_layedOut)
            {
                LayoutHelper.FixupGroupBox(groupBoxEditing);
                _layedOut = true;
            }
        }

        public override void Save()
        {
            if (_autoReplacePreferences.IsModified())
                _autoReplacePreferences.Save();
        }

        void checkBoxTypographic_CheckedChanged(object sender, EventArgs e)
        {
            _autoReplacePreferences.EnableTypographicReplacement = checkBoxTypographic.Checked;
        }

        void checkBoxSpecialChars_CheckedChanged(object sender, EventArgs e)
        {
            _autoReplacePreferences.EnableSpecialCharacterReplacement = checkBoxSpecialChars.Checked;
        }

        private void checkBoxSmartQuotes_CheckedChanged(object sender, EventArgs e)
        {
            _autoReplacePreferences.EnableSmartQuotes = checkBoxSmartQuotes.Checked;
        }

        private void checkBoxEmoticons_CheckedChanged(object sender, EventArgs e)
        {
            _autoReplacePreferences.EnableEmoticonsReplacement = checkBoxEmoticons.Checked;
        }

        void _autoReplacePreferences_PreferencesModified(object sender, EventArgs e)
        {
            OnModified(EventArgs.Empty);
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
            this.groupBoxEditing = new System.Windows.Forms.GroupBox();
            this.checkBoxSpecialChars = new System.Windows.Forms.CheckBox();
            this.checkBoxSmartQuotes = new System.Windows.Forms.CheckBox();
            this.checkBoxTypographic = new System.Windows.Forms.CheckBox();
            this.checkBoxEmoticons = new System.Windows.Forms.CheckBox();
            this.groupBoxEditing.SuspendLayout();
            this.SuspendLayout();
            //
            // groupBoxEditing
            //
            this.groupBoxEditing.Controls.Add(this.checkBoxSpecialChars);
            this.groupBoxEditing.Controls.Add(this.checkBoxSmartQuotes);
            this.groupBoxEditing.Controls.Add(this.checkBoxTypographic);
            this.groupBoxEditing.Controls.Add(this.checkBoxEmoticons);
            this.groupBoxEditing.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxEditing.Location = new System.Drawing.Point(8, 32);
            this.groupBoxEditing.Name = "groupBoxEditing";
            this.groupBoxEditing.Size = new System.Drawing.Size(345, 179);
            this.groupBoxEditing.TabIndex = 0;
            this.groupBoxEditing.TabStop = false;
            this.groupBoxEditing.Text = "Editing";
            //
            // checkBoxEmoticons
            //
            this.checkBoxEmoticons.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.checkBoxEmoticons.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxEmoticons.Location = new System.Drawing.Point(16, 99);
            this.checkBoxEmoticons.Name = "checkBoxEmoticons";
            this.checkBoxEmoticons.Size = new System.Drawing.Size(312, 18);
            this.checkBoxEmoticons.TabIndex = 8;
            this.checkBoxEmoticons.Text = "Replace text emoticons with emoticon graphics";
            this.checkBoxEmoticons.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.checkBoxEmoticons.UseVisualStyleBackColor = true;
            //
            // checkBoxSpecialChars
            //
            this.checkBoxSpecialChars.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.checkBoxSpecialChars.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxSpecialChars.Location = new System.Drawing.Point(16, 73);
            this.checkBoxSpecialChars.Name = "checkBoxSpecialChars";
            this.checkBoxSpecialChars.Size = new System.Drawing.Size(312, 18);
            this.checkBoxSpecialChars.TabIndex = 7;
            this.checkBoxSpecialChars.Text = "Replace other special characters";
            this.checkBoxSpecialChars.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.checkBoxSpecialChars.UseVisualStyleBackColor = true;
            //
            // checkBoxSmartQuotes
            //
            this.checkBoxSmartQuotes.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.checkBoxSmartQuotes.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxSmartQuotes.Location = new System.Drawing.Point(16, 47);
            this.checkBoxSmartQuotes.Name = "checkBoxSmartQuotes";
            this.checkBoxSmartQuotes.Size = new System.Drawing.Size(312, 18);
            this.checkBoxSmartQuotes.TabIndex = 5;
            this.checkBoxSmartQuotes.Text = "Replace \"straight quotes\" with “smart &quotes”";
            this.checkBoxSmartQuotes.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.checkBoxSmartQuotes.UseVisualStyleBackColor = true;
            this.checkBoxSmartQuotes.CheckedChanged += new System.EventHandler(this.checkBoxSmartQuotes_CheckedChanged);
            //
            // checkBoxTypographic
            //
            this.checkBoxTypographic.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.checkBoxTypographic.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxTypographic.Location = new System.Drawing.Point(16, 21);
            this.checkBoxTypographic.Name = "checkBoxTypographic";
            this.checkBoxTypographic.Size = new System.Drawing.Size(312, 18);
            this.checkBoxTypographic.TabIndex = 4;
            this.checkBoxTypographic.Text = "Replace h&yphens (--) with dash (—)";
            this.checkBoxTypographic.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.checkBoxTypographic.UseVisualStyleBackColor = true;
            //
            // EditingPrefencesPanel
            //
            this.AccessibleName = "Preferences";
            this.Controls.Add(this.groupBoxEditing);
            this.Name = "EditingPrefencesPanel";
            this.PanelName = "Preferences";
            this.Size = new System.Drawing.Size(370, 314);
            this.Controls.SetChildIndex(this.groupBoxEditing, 0);
            this.groupBoxEditing.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

    }
}
