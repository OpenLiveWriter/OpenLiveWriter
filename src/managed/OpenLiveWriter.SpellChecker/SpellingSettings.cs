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
        [Obsolete]
        public static bool CanSpellCheck
        {
            get
            {
                return Language != SpellingCheckerLanguage.None;
            }
        }

        /// <summary>
        /// Path to dictionary files
        /// </summary>
        public static string DictionaryPath
        {
            get
            {
                return Path.Combine(ApplicationEnvironment.InstallationDirectory, "Dictionaries");
            }
        }

        public static string UserDictionaryPath
        {
            get
            {
                // In the past this property returned the DIRECTORY
                // where the user dictionary file should go. Now it returns the path to
                // the FILE itself.

                string userDictionaryPath = Path.Combine(ApplicationEnvironment.ApplicationDataDirectory, "Dictionaries");
                if (!Directory.Exists(userDictionaryPath))
                    Directory.CreateDirectory(userDictionaryPath);
                return Path.Combine(userDictionaryPath, "User.dic");
            }
        }

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
                return SpellingKey.GetBoolean(REAL_TIME_SPELL_CHECKING, RealTimeSpellCheckingDefault);
            }
            set { SpellingKey.SetBoolean(REAL_TIME_SPELL_CHECKING, value); }
        }

        private const string REAL_TIME_SPELL_CHECKING = "RealTimeSpellChecking";
        private static bool RealTimeSpellCheckingDefault
        {
            get
            {
                try
                {
                    string lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                    foreach (SpellingLanguageEntry sentryLang in GetInstalledLanguages())
                        if (sentryLang.TwoLetterIsoLanguageName == lang)
                            return true;
                    return false;
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e.ToString());
                    return false;
                }
            }
        }

        /// <summary>
        /// Languages supported by the sentry spelling checker
        /// </summary>
        public static SpellingLanguageEntry[] GetInstalledLanguages()
        {
            string lexiconPath = DictionaryPath;
            ArrayList list = new ArrayList();
            foreach (SpellingLanguageEntry entry in SpellingConfigReader.Languages)
            {
                if (entry.Language == SpellingCheckerLanguage.None
                    || entry.IsInstalled(lexiconPath))
                {
                    list.Add(entry);
                }
            }
            return (SpellingLanguageEntry[])list.ToArray(typeof(SpellingLanguageEntry));
        }

        public static SpellingLanguageEntry GetInstalledLanguage(SpellingCheckerLanguage language)
        {
            foreach (SpellingLanguageEntry entry in GetInstalledLanguages())
            {
                if (entry.Language == language)
                    return entry;
            }
            return null;
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
        public static SpellingCheckerLanguage Language
        {
            get
            {
                // Check if the language specified in the registry has dictionaries installed.
                // If so, use it. If not, check if the default language has dictionaries
                // installed. If so, use it. If not, use the English-US.

                SpellingCheckerLanguage defaultLanguage = SpellingConfigReader.DefaultLanguage;
                SpellingCheckerLanguage preferredLanguage;
                try
                {
                    preferredLanguage = (SpellingCheckerLanguage)SpellingKey.GetEnumValue(LANGUAGE, typeof(SpellingCheckerLanguage), defaultLanguage);
                }
                catch (Exception e)
                {
                    Trace.Fail(e.ToString());
                    preferredLanguage = defaultLanguage;
                }

                if (preferredLanguage == SpellingCheckerLanguage.None)
                    return SpellingCheckerLanguage.None;

                foreach (SpellingLanguageEntry lang in GetInstalledLanguages())
                {
                    if (lang.Language == preferredLanguage)
                        return preferredLanguage;
                }

                // Language in registry is not installed!
                Trace.WriteLine("Dictionary language specified in registry is not installed. Using fallback");

                foreach (SpellingLanguageEntry lang in GetInstalledLanguages())
                {
                    if (lang.Language == defaultLanguage)
                    {
                        Language = defaultLanguage;
                        return defaultLanguage;
                    }
                }

                return Language = SpellingCheckerLanguage.EnglishUS;
            }
            set { SpellingKey.SetString(LANGUAGE, value.ToString()); }
        }
        private const string LANGUAGE = "DictionaryLanguage";

        internal static SettingsPersisterHelper SettingsKey = ApplicationEnvironment.PreferencesSettingsRoot.GetSubSettings("PostEditor");
        public static SettingsPersisterHelper SpellingKey = SettingsKey.GetSubSettings("Spelling");
    }
}
