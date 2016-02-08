// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.CoreServices.Settings;
using OpenLiveWriter.Localization;
using System.Collections.Generic;

/*
TODO:
Should we pass the language code?
Be robust against the selected or default language not being present in the list of dictionaries
Include tech.tlx for everyone?
*/

namespace OpenLiveWriter.SpellChecker
{
    /// <summary>
    /// Summary description for SpellingSettings.
    /// </summary>
    public class SpellingSettings
    {
        public static event EventHandler SpellingSettingsChanged;

        public static void FireChangedEvent()
        {
            if (SpellingSettingsChanged != null)
                SpellingSettingsChanged(null, new EventArgs());
        }

        /// <summary>
        /// Run real time spell checker (squiggles)
        /// </summary>
        public static bool RealTimeSpellChecking
        {
            get
            {
                // Just doing this GetString to see if the value is present.
                // There isn't a SpellingKey.HasValue().
                if (SpellingKey.HasValue(REAL_TIME_SPELL_CHECKING))
                    return SpellingKey.GetBoolean(REAL_TIME_SPELL_CHECKING, true);

                // This is GetBoolean rather than just "return RealTimeSpellCheckingDefault"
                // to ensure that the default gets written to the registry.
                return SpellingKey.GetBoolean(REAL_TIME_SPELL_CHECKING, true);
            }
            set { SpellingKey.SetBoolean(REAL_TIME_SPELL_CHECKING, value); }
        }
        private const string REAL_TIME_SPELL_CHECKING = "RealTimeSpellChecking";

        /// <summary>
        /// Languages supported by the sentry spelling checker
        /// </summary>
        public static SpellingLanguageEntry[] GetInstalledLanguages()
        {
            List<SpellingLanguageEntry> list = new List<SpellingLanguageEntry>();

            foreach (string entry in WinSpellingChecker.GetInstalledLanguages())
            {
                try
                {
                    CultureInfo entryCulture = CultureInfo.GetCultureInfo(entry);

                    list.Add(new SpellingLanguageEntry(entry, entryCulture.DisplayName));
                }
                catch
                {
                    list.Add(new SpellingLanguageEntry(entry, entry));
                }
            }

            return list.ToArray();
        }

        /// <summary>
        /// Run spelling form before publishing
        /// </summary>
        public static bool CheckSpellingBeforePublish
        {
            get { return SpellingKey.GetBoolean(CHECK_SPELLING_BEFORE_PUBLISH, CHECK_SPELLING_BEFORE_PUBLISH_DEFAULT); }
            set { SpellingKey.SetBoolean(CHECK_SPELLING_BEFORE_PUBLISH, value); }
        }
        private const string CHECK_SPELLING_BEFORE_PUBLISH = "CheckSpellingBeforePublish";
        private const bool CHECK_SPELLING_BEFORE_PUBLISH_DEFAULT = false;

        /// <summary>
        /// Ignore words in upper-case
        /// </summary>
        public static bool IgnoreUppercase
        {
            get { return SpellingKey.GetBoolean(IGNORE_UPPERCASE, IGNORE_UPPERCASE_DEFAULT); }
            set { SpellingKey.SetBoolean(IGNORE_UPPERCASE, value); }
        }
        private const string IGNORE_UPPERCASE = "IgnoreUppercase";
        private const bool IGNORE_UPPERCASE_DEFAULT = true;

        /// <summary>
        /// Ignore words with numbers
        /// </summary>
        public static bool IgnoreWordsWithNumbers
        {
            get { return SpellingKey.GetBoolean(IGNORE_NUMBERS, IGNORE_NUMBERS_DEFAULT); }
            set { SpellingKey.SetBoolean(IGNORE_NUMBERS, value); }
        }
        private const string IGNORE_NUMBERS = "IgnoreNumbers";
        private const bool IGNORE_NUMBERS_DEFAULT = true;

        /// <summary>
        /// Enable AutoCorrect
        /// </summary>
        public static bool EnableAutoCorrect
        {
            get { return SpellingKey.GetBoolean(AUTOCORRECT, AUTOCORRECT_DEFAULT); }
            set { SpellingKey.SetBoolean(AUTOCORRECT, value); }
        }
        private const string AUTOCORRECT = "AutoCorrectEnabled";
        private const bool AUTOCORRECT_DEFAULT = true;

        /// <summary>
        /// Main language for spell checking
        /// </summary>
        public static string Language
        {
            get
            {
                // Check if the language specified in the registry has dictionaries installed.
                // If so, use it. If not, check if the default language has dictionaries
                // installed. If so, use it. If not, use the English-US.

                // CurrentUICulture is currently forced into en-US which is unlikely to be the preferred dictionary
                // CurrentCulture is not modified so is more likely to be correct
                string defaultLanguage = CultureInfo.CurrentCulture.Name;
                string preferredLanguage;
                try
                {
                    preferredLanguage = SpellingKey.GetString(LANGUAGE, defaultLanguage);
                }
                catch (Exception e)
                {
                    Trace.Fail(e.ToString());
                    preferredLanguage = defaultLanguage;
                }

                if (string.IsNullOrEmpty(preferredLanguage))
                    return string.Empty;

                if (WinSpellingChecker.IsLanguageSupported(preferredLanguage))
                {
                    return preferredLanguage;
                }

                // Language in registry is not installed!
                Trace.WriteLine("Dictionary language specified in registry is not installed. Using fallback");

                if (WinSpellingChecker.IsLanguageSupported(defaultLanguage))
                { 
                    Language = defaultLanguage;
                    return defaultLanguage;
                }

                if (WinSpellingChecker.IsLanguageSupported("en-US"))
                {
                    Language = "en-US";
                    return "en-US";
                }

                return string.Empty;
            }
            set { SpellingKey.SetString(LANGUAGE, value); }
        }
        private const string LANGUAGE = "SpellingLanguage";

        internal static SettingsPersisterHelper SettingsKey = ApplicationEnvironment.PreferencesSettingsRoot.GetSubSettings("PostEditor");
        public static SettingsPersisterHelper SpellingKey = SettingsKey.GetSubSettings("Spelling");
    }
}
