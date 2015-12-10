// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using OpenLiveWriter.CoreServices.Settings;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.PostEditor.Autoreplace
{
    class AutoreplaceSettings
    {

        public static event EventHandler SettingsChanged;

        public static bool AnyReplaceEnabled
        {
            get
            {
                return EnableHyphenReplacement || EnableSmartQuotes || EnableSpecialCharacterReplacement || EnableEmoticonsReplacement;
            }
        }

        public static bool EnableHyphenReplacement
        {
            get
            {
                if (!_enableHyphenReplacement.HasValue)
                    _enableHyphenReplacement = Settings.GetBoolean(TYPO, true);

                return _enableHyphenReplacement.Value;
            }
            set
            {
                Settings.SetBoolean(TYPO, value);
                OnSettingsChanged();
                _enableHyphenReplacement = value;
            }
        }

        public static bool EnableSpecialCharacterReplacement
        {
            get
            {
                if (!_enableSpecialCharacterReplacement.HasValue)
                    _enableSpecialCharacterReplacement = Settings.GetBoolean(SPECIAL, true);

                return _enableSpecialCharacterReplacement.Value;
            }
            set
            {
                Settings.SetBoolean(SPECIAL, value);
                OnSettingsChanged();
                _enableSpecialCharacterReplacement = value;
            }
        }

        public static bool EnableEmoticonsReplacement
        {
            get
            {
                if (!_enableEmoticonsReplacement.HasValue)
                    _enableEmoticonsReplacement = Settings.GetBoolean(EMOTICONS, true);

                return _enableEmoticonsReplacement.Value;
            }
            set
            {
                Settings.SetBoolean(EMOTICONS, value);
                OnSettingsChanged();
                _enableEmoticonsReplacement = value;
            }
        }

        public static bool EnableSmartQuotes
        {
            get
            {
                if (BidiHelper.IsRightToLeft) // disable smart quotes for Bidi builds. This is consistent with Outlook and others.
                    return false;

                if (!_enableSmartQuotes.HasValue)
                    _enableSmartQuotes = Settings.GetBoolean(SMARTQUOTES, true);

                return _enableSmartQuotes.Value;
            }
            set
            {
                Settings.SetBoolean(SMARTQUOTES, value);
                OnSettingsChanged();
                _enableSmartQuotes = value;
            }
        }

        public static AutoreplacePhrase[] AutoreplacePhrases
        {
            get
            {
                if (_autoreplacePhrases == null)
                {
                    List<AutoreplacePhrase> phrases = new List<AutoreplacePhrase>();
                    SettingsPersisterHelper replacephraseSettings = Settings.GetSubSettings(PHRASES);
                    foreach (string phrase in replacephraseSettings.GetNames())
                    {
                        phrases.Add(
                            new AutoreplacePhrase(phrase, replacephraseSettings.GetString(phrase, null)));
                    }
                    _autoreplacePhrases = phrases.ToArray();

                }
                return _autoreplacePhrases;
            }
            set
            {
                SettingsPersisterHelper replacephraseSettings = Settings.GetSubSettings(PHRASES);
                using (replacephraseSettings.BatchUpdate())
                {
                    ArrayList phrasesToDelete = new ArrayList(replacephraseSettings.GetNames());
                    foreach (AutoreplacePhrase replacePhrase in value)
                    {
                        replacephraseSettings.SetString(replacePhrase.Phrase, replacePhrase.ReplaceValue);
                        phrasesToDelete.Remove(replacePhrase.Phrase);
                    }

                    foreach (string phrase in phrasesToDelete)
                    {
                        replacephraseSettings.Unset(phrase);
                    }
                }

                _autoreplacePhrases = value;
                OnSettingsChanged();
            }
        }

        protected static void OnSettingsChanged()
        {
            if (SettingsChanged != null)
                SettingsChanged(null, EventArgs.Empty);
        }

        private static bool? _enableEmoticonsReplacement;
        private static bool? _enableSpecialCharacterReplacement;
        private static bool? _enableHyphenReplacement;
        private static bool? _enableSmartQuotes;

        private static AutoreplacePhrase[] _autoreplacePhrases = null;

        private static readonly SettingsPersisterHelper Settings = PostEditorSettings.SettingsKey.GetSubSettings("Autoreplace");
        private const string TYPO = "Hyphens";
        private const string SMARTQUOTES = "SmartQuotes";
        private const string PHRASES = "ReplacePhrases";
        private const string SPECIAL = "OtherSpecialCharacters";
        private const string EMOTICONS = "Emoticons";
    }

    public class AutoreplacePhrase
    {
        public AutoreplacePhrase(string phrase, string replaceValue)
        {
            _phrase = phrase;
            _replaceValue = replaceValue;
        }

        public string Phrase
        {
            get { return _phrase; }
        }

        public string ReplaceValue
        {
            get { return _replaceValue; }
        }
        private readonly string _phrase;
        private readonly string _replaceValue;

    }
}
