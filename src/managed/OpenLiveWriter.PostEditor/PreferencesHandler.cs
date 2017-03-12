// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework.Preferences;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.HtmlEditor.Linking;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor.Autoreplace;
using OpenLiveWriter.PostEditor.LiveClipboard;
using OpenLiveWriter.PostEditor.Configuration.Accounts;
using OpenLiveWriter.SpellChecker;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.PostEditor
{
    /// <summary>
    /// Class for handling preferences
    /// </summary>
    public class PreferencesHandler
    {
        /// <summary>
        /// The array of preferences panel types.
        /// </summary>
        private static Type[] preferencesPanelTypes;

        /// <summary>
        /// The name to preferences panel type table.
        /// </summary>
        private static Hashtable preferencesPanelTypeTable;

        public static PreferencesHandler Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new PreferencesHandler();
                return _instance;
            }
        }
        private static PreferencesHandler _instance;

        /// <summary>
        /// A version of show preferences that allows the caller to specify which
        /// panel should be selected when the dialog opens
        /// </summary>
        /// <param name="selectedPanelName">The selected panel name.</param>
        public void ShowPreferences(IWin32Window owner, string selectedPanelName)
        {
            LoadPreferencesPanels();
            Type type;
            if (selectedPanelName == null || selectedPanelName.Length == 0)
                type = null;
            else
                type = (Type)preferencesPanelTypeTable[selectedPanelName.ToLower(CultureInfo.InvariantCulture)];
            ShowPreferences(owner, null, type);
        }

        /// <summary>
        /// A version of show preferences that allows the caller to specify which
        /// panel should be selected when the dialog opens
        /// </summary>
        /// <param name="selectedPanelType"></param>
        public void ShowPreferences(IWin32Window owner, IBlogPostEditingSite editingSite, Type selectedPanelType)
        {
            //	Load preferences panels.
            LoadPreferencesPanels();

            //	Show the preferences form.
            using (new WaitCursor())
            {
                using (PreferencesForm preferencesForm = new PreferencesForm())
                {
                    //	Set the PreferencesPanel entries.
                    for (int i = 0; i < preferencesPanelTypes.Length; i++)
                    {
                        //	Add the entry.
                        Type type = preferencesPanelTypes[i];
                        PreferencesPanel panel = Activator.CreateInstance(type) as PreferencesPanel;
                        if (editingSite != null && panel is IBlogPostEditingSitePreferences)
                        {
                            (panel as IBlogPostEditingSitePreferences).EditingSite = editingSite;
                        }
                        preferencesForm.SetEntry(i, panel);

                        //	Select it, if requested.
                        if (type.Equals(selectedPanelType))
                            preferencesForm.SelectedIndex = i;
                    }

                    //	Provide a default selected index if none was specified.
                    if (preferencesForm.SelectedIndex == -1)
                        preferencesForm.SelectedIndex = 0;

                    //	Show the form.
                    preferencesForm.Win32Owner = owner;
                    preferencesForm.ShowDialog(owner);

                    // if we have an editing site then let it know that the account
                    // list may have been edited (allows it to adapt to the currently
                    // active weblog being deleted)
                    if (editingSite != null)
                        editingSite.NotifyWeblogAccountListEdited();
                }
            }
        }

        public void ShowWebProxyPreferences(IWin32Window owner)
        {
            //	Show the preferences form.
            using (new WaitCursor())
            {
                using (PreferencesForm preferencesForm = new PreferencesForm())
                {
                    preferencesForm.Text = Res.Get(StringId.ProxyPrefTitle);
                    preferencesForm.SetEntry(0, new WebProxyPreferencesPanel());
                    preferencesForm.SelectedIndex = 0;
                    preferencesForm.Win32Owner = owner;
                    preferencesForm.ShowDialog(owner);
                }
            }
        }

        /// <summary>
        /// Helper to load the preferences panels.
        /// </summary>
        private static void LoadPreferencesPanels()
        {
            //	If preferences panels have been loaded already, return.
            if (preferencesPanelTypes != null)
                return;

            Type type;
            ArrayList types = new ArrayList();
            preferencesPanelTypeTable = new Hashtable();

            //	Writer Preferences
            type = typeof(PostEditorPreferencesPanel);
            preferencesPanelTypeTable["preferences"] = type;
            types.Add(type);

            type = typeof(EditingPreferencesPanel);
            preferencesPanelTypeTable["editing"] = type;
            types.Add(type);

            type = typeof(WeblogAccountPreferencesPanel);
            preferencesPanelTypeTable["accounts"] = type;
            types.Add(type);

            // Spelling preferences.
            type = typeof(SpellingPreferencesPanel);
            preferencesPanelTypeTable["spelling"] = type;
            types.Add(type);

            //glossary management
            type = typeof(GlossaryPreferencesPanel);
            preferencesPanelTypeTable["glossary"] = type;
            types.Add(type);

            type = typeof(AutoreplacePreferencesPanel);
            preferencesPanelTypeTable["autoreplace"] = type;
            //types.Add(type);

            if (ApplicationDiagnostics.TestMode || LiveClipboardManager.LiveClipboardFormatHandlers.Length > 0)
            {
                type = typeof(LiveClipboardPreferencesPanel);
                preferencesPanelTypeTable["liveclipboard"] = type;
                types.Add(type);
            }

            // Plugin preferences
            type = typeof(PluginsPreferencesPanel);
            preferencesPanelTypeTable["plugins"] = type;
            types.Add(type);

            //	WebProxy preferences
            type = typeof(WebProxyPreferencesPanel);
            preferencesPanelTypeTable["webproxy"] = type;
            types.Add(type);

            type = typeof(PingPreferencesPanel);
            preferencesPanelTypeTable["pings"] = type;
            types.Add(type);

            type = typeof(PrivacyPreferencesPanel);
            preferencesPanelTypeTable["privacy"] = type;
            types.Add(type);

            //	Set the preferences panels type array.
            preferencesPanelTypes = (Type[])types.ToArray(typeof(Type));
        }

    }

    internal interface IBlogPostEditingSitePreferences
    {
        IBlogPostEditingSite EditingSite { get; set; }
    }
}
