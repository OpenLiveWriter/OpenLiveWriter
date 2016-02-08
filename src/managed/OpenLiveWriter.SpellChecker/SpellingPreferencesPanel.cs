// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework.Preferences;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.SpellChecker
{
    /// <summary>
    /// Summary description for SpellingOptions.
    /// </summary>
    public class SpellingPreferencesPanel : PreferencesPanel
    {
        private GroupBox _groupBoxGeneralOptions;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;
        private CheckBox _checkBoxCheckBeforePublish;
        private CheckBox _checkBoxRealTimeChecking;
        private CheckBox _checkBoxAutoCorrect;
        private System.Windows.Forms.Label _labelDictionaryLanguage;
        private System.Windows.Forms.ComboBox _comboBoxLanguage;

        private SpellingPreferences spellingPreferences;

        public SpellingPreferencesPanel()
            : this(new SpellingPreferences())
        {
        }

        public SpellingPreferencesPanel(SpellingPreferences preferences)
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            _labelDictionaryLanguage.Text = Res.Get(StringId.DictionaryLanguageLabel);
            _groupBoxGeneralOptions.Text = Res.Get(StringId.SpellingPrefOptions);
            _checkBoxRealTimeChecking.Text = Res.Get(StringId.SpellingPrefReal);
            _checkBoxCheckBeforePublish.Text = Res.Get(StringId.SpellingPrefPub);
            _checkBoxAutoCorrect.Text = Res.Get(StringId.SpellingPrefAuto);
            PanelName = Res.Get(StringId.SpellingPrefName);

            // set panel bitmap
            PanelBitmap = _spellingPanelBitmap;

            // initialize preferences
            spellingPreferences = preferences;
            spellingPreferences.PreferencesModified += new EventHandler(spellingPreferences_PreferencesModified);

            // core options
            _checkBoxCheckBeforePublish.Checked = spellingPreferences.CheckSpellingBeforePublish;
            _checkBoxRealTimeChecking.Checked = spellingPreferences.RealTimeSpellChecking;
            _checkBoxAutoCorrect.Checked = spellingPreferences.EnableAutoCorrect;

            // initialize language combo
            _comboBoxLanguage.BeginUpdate();
            _comboBoxLanguage.Items.Clear();
            string currentLanguage = spellingPreferences.Language;

            SpellingLanguageEntry[] languages = SpellingSettings.GetInstalledLanguages();
            Array.Sort(languages, new SentryLanguageEntryComparer(CultureInfo.CurrentUICulture));

            _comboBoxLanguage.Items.Add(new SpellingLanguageEntry(string.Empty, Res.Get(StringId.DictionaryLanguageNone)));

            foreach (SpellingLanguageEntry language in languages)
            {
                int index = _comboBoxLanguage.Items.Add(language);
                if (language.BCP47Code == currentLanguage)
                    _comboBoxLanguage.SelectedIndex = index;
            }
            // defend against invalid value
            if (_comboBoxLanguage.SelectedIndex == -1)
            {
                if (!string.IsNullOrEmpty(currentLanguage))
                {
                    Debug.Fail("Language in registry not supported!");
                }
                _comboBoxLanguage.SelectedIndex = 0; // "None"
            }
            _comboBoxLanguage.EndUpdate();

            ManageSpellingOptions();

            // hookup to changed events to update preferences
            _checkBoxCheckBeforePublish.CheckedChanged += new EventHandler(checkBoxCheckBeforePublish_CheckedChanged);
            _checkBoxRealTimeChecking.CheckedChanged += new EventHandler(checkBoxRealTimeChecking_CheckedChanged);
            _checkBoxAutoCorrect.CheckedChanged += new EventHandler(checkBoxAutoCorrect_CheckedChanged);

            _comboBoxLanguage.SelectedIndexChanged += new EventHandler(comboBoxLanguage_SelectedIndexChanged);

        }

        private void ManageSpellingOptions()
        {
            bool enabled = _comboBoxLanguage.SelectedIndex != 0;  // "None"
            _checkBoxCheckBeforePublish.Enabled = enabled;
            _checkBoxRealTimeChecking.Enabled = enabled;
            _checkBoxAutoCorrect.Enabled = enabled;
        }

        private class SentryLanguageEntryComparer : IComparer
        {
            private CultureInfo cultureInfo;

            public SentryLanguageEntryComparer(CultureInfo cultureInfo)
            {
                this.cultureInfo = cultureInfo;
            }

            public int Compare(object x, object y)
            {
                return string.Compare(
                    ((SpellingLanguageEntry)x).DisplayName,
                    ((SpellingLanguageEntry)y).DisplayName,
                    true,
                    cultureInfo);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            DisplayHelper.AutoFitSystemCombo(_comboBoxLanguage, _comboBoxLanguage.Width,
                _groupBoxGeneralOptions.Width - _comboBoxLanguage.Left - 8,
                false);
            LayoutHelper.FixupGroupBox(8, _groupBoxGeneralOptions);
        }

        /// <summary>
        /// Save data
        /// </summary>
        public override void Save()
        {
            if (spellingPreferences.IsModified())
                spellingPreferences.Save();
        }

        /// <summary>
        /// flagsPreferences_PreferencesModified event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void spellingPreferences_PreferencesModified(object sender, EventArgs e)
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

        private const string SPELLING_IMAGE_PATH = "Images.";
        //private Bitmap spellingDictionariesBitmap = ResourceHelper.LoadAssemblyResourceBitmap( SPELLING_IMAGE_PATH + "SpellingDictionaries.png") ;
        private readonly Bitmap _spellingPanelBitmap = ResourceHelper.LoadAssemblyResourceBitmap(SPELLING_IMAGE_PATH + "SpellingPanelBitmapSmall.png");

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._groupBoxGeneralOptions = new System.Windows.Forms.GroupBox();
            this._comboBoxLanguage = new System.Windows.Forms.ComboBox();
            this._labelDictionaryLanguage = new System.Windows.Forms.Label();
            this._checkBoxRealTimeChecking = new System.Windows.Forms.CheckBox();
            this._checkBoxCheckBeforePublish = new System.Windows.Forms.CheckBox();
            this._checkBoxAutoCorrect = new System.Windows.Forms.CheckBox();
            this._groupBoxGeneralOptions.SuspendLayout();
            this.SuspendLayout();
            //
            // _groupBoxGeneralOptions
            //
            this._groupBoxGeneralOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._groupBoxGeneralOptions.Controls.Add(this._comboBoxLanguage);
            this._groupBoxGeneralOptions.Controls.Add(this._labelDictionaryLanguage);
            this._groupBoxGeneralOptions.Controls.Add(this._checkBoxRealTimeChecking);
            this._groupBoxGeneralOptions.Controls.Add(this._checkBoxCheckBeforePublish);
            this._groupBoxGeneralOptions.Controls.Add(this._checkBoxAutoCorrect);
            this._groupBoxGeneralOptions.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._groupBoxGeneralOptions.Location = new System.Drawing.Point(8, 32);
            this._groupBoxGeneralOptions.Name = "_groupBoxGeneralOptions";
            this._groupBoxGeneralOptions.Size = new System.Drawing.Size(345, 189);
            this._groupBoxGeneralOptions.TabIndex = 1;
            this._groupBoxGeneralOptions.TabStop = false;
            this._groupBoxGeneralOptions.Text = "General options";
            //
            // _comboBoxLanguage
            //
            this._comboBoxLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._comboBoxLanguage.Location = new System.Drawing.Point(48, 37);
            this._comboBoxLanguage.Name = "_comboBoxLanguage";
            this._comboBoxLanguage.Size = new System.Drawing.Size(195, 21);
            this._comboBoxLanguage.TabIndex = 1;
            //
            // _labelDictionaryLanguage
            //
            this._labelDictionaryLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._labelDictionaryLanguage.AutoSize = true;
            this._labelDictionaryLanguage.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._labelDictionaryLanguage.Location = new System.Drawing.Point(16, 18);
            this._labelDictionaryLanguage.Name = "_labelDictionaryLanguage";
            this._labelDictionaryLanguage.Size = new System.Drawing.Size(106, 13);
            this._labelDictionaryLanguage.TabIndex = 0;
            this._labelDictionaryLanguage.Text = "Dictionary &language:";
            //
            // _checkBoxRealTimeChecking
            //
            this._checkBoxRealTimeChecking.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._checkBoxRealTimeChecking.Location = new System.Drawing.Point(16, 65);
            this._checkBoxRealTimeChecking.Name = "_checkBoxRealTimeChecking";
            this._checkBoxRealTimeChecking.Size = new System.Drawing.Size(323, 18);
            this._checkBoxRealTimeChecking.TabIndex = 2;
            this._checkBoxRealTimeChecking.Text = "Use &real time spell checking (squiggles)";
            //
            // _checkBoxCheckBeforePublish
            //
            this._checkBoxCheckBeforePublish.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._checkBoxCheckBeforePublish.Location = new System.Drawing.Point(16, 88);
            this._checkBoxCheckBeforePublish.Name = "_checkBoxCheckBeforePublish";
            this._checkBoxCheckBeforePublish.Size = new System.Drawing.Size(323, 18);
            this._checkBoxCheckBeforePublish.TabIndex = 5;
            this._checkBoxCheckBeforePublish.Text = "Check spelling before &publishing";
            //
            // _checkBoxAutoCorrect
            //
            this._checkBoxAutoCorrect.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._checkBoxAutoCorrect.Location = new System.Drawing.Point(16, 111);
            this._checkBoxAutoCorrect.Name = "_checkBoxAutoCorrect";
            this._checkBoxAutoCorrect.Size = new System.Drawing.Size(323, 18);
            this._checkBoxAutoCorrect.TabIndex = 6;
            this._checkBoxAutoCorrect.Text = "Automatically &correct common capitalization and spelling mistakes";
            //
            // SpellingPreferencesPanel
            //
            this.AccessibleName = "Spelling";
            this.Controls.Add(this._groupBoxGeneralOptions);
            this.Name = "SpellingPreferencesPanel";
            this.PanelName = "Spelling";
            this.Controls.SetChildIndex(this._groupBoxGeneralOptions, 0);
            this._groupBoxGeneralOptions.ResumeLayout(false);
            this._groupBoxGeneralOptions.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion

        private void checkBoxCheckBeforePublish_CheckedChanged(object sender, EventArgs e)
        {
            spellingPreferences.CheckSpellingBeforePublish = _checkBoxCheckBeforePublish.Checked;
        }

        private void checkBoxRealTimeChecking_CheckedChanged(object sender, EventArgs e)
        {
            spellingPreferences.RealTimeSpellChecking = _checkBoxRealTimeChecking.Checked;
        }

        private void checkBoxAutoCorrect_CheckedChanged(object sender, EventArgs e)
        {
            spellingPreferences.EnableAutoCorrect = _checkBoxAutoCorrect.Checked;
        }

        private void comboBoxLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            spellingPreferences.Language = (_comboBoxLanguage.SelectedItem as SpellingLanguageEntry).BCP47Code;
            ManageSpellingOptions();
        }
    }
}
