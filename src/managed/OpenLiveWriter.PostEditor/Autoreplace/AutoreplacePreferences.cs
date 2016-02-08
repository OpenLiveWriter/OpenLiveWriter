// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Settings;

namespace OpenLiveWriter.PostEditor.Autoreplace
{
    public class AutoreplacePreferences : OpenLiveWriter.ApplicationFramework.Preferences.Preferences
    {
        public AutoreplacePreferences() : base("Autoreplace")
        {

        }

        public bool EnableSmartQuotes
        {
            get { return _enableSmartQuotes; }
            set
            {
                _enableSmartQuotes = value;
                Modified();
            }
        }
        private bool _enableSmartQuotes;

        public bool EnableTypographicReplacement
        {
            get { return _enableTypographicReplacement; }
            set { _enableTypographicReplacement = value; Modified(); }
        }
        private bool _enableTypographicReplacement;

        public bool EnableSpecialCharacterReplacement
        {
            get { return _enableSpecialCharacterReplacement; }
            set { _enableSpecialCharacterReplacement = value; Modified(); }
        }
        private bool _enableSpecialCharacterReplacement;

        public bool EnableEmoticonsReplacement
        {
            get { return _enableEmoticonsReplacement; }
            set { _enableEmoticonsReplacement = value; Modified(); }
        }
        private bool _enableEmoticonsReplacement;

        public AutoreplacePhrase[] GetAutoreplacePhrases()
        {
            AutoreplacePhrase[] phrases = new AutoreplacePhrase[_autoReplacePhrases.Keys.Count];
            int i = 0;
            foreach (string phrase in _autoReplacePhrases.Keys)
            {
                phrases[i] = new AutoreplacePhrase(phrase, (string)_autoReplacePhrases[phrase]);
                i++;
            }
            return phrases;
        }

        public AutoreplacePhrase GetPhrase(string phrase)
        {
            return new AutoreplacePhrase(phrase, (string)_autoReplacePhrases[phrase]);
        }

        public void SetAutoreplacePhrase(string phrase, string replaceValue)
        {
            _autoReplacePhrases[phrase] = replaceValue;
            Modified();
        }

        public void RemoveAutoreplacePhrase(string phrase)
        {
            _autoReplacePhrases.Remove(phrase);
            Modified();
        }

        private readonly Hashtable _autoReplacePhrases = new Hashtable();

        protected override void LoadPreferences()
        {
            EnableTypographicReplacement = AutoreplaceSettings.EnableHyphenReplacement;
            EnableSmartQuotes = AutoreplaceSettings.EnableSmartQuotes;
            EnableSpecialCharacterReplacement = AutoreplaceSettings.EnableSpecialCharacterReplacement;
            EnableEmoticonsReplacement = AutoreplaceSettings.EnableEmoticonsReplacement;
            foreach (AutoreplacePhrase phrase in AutoreplaceSettings.AutoreplacePhrases)
                _autoReplacePhrases[phrase.Phrase] = phrase.ReplaceValue;
        }

        protected override void SavePreferences()
        {
            AutoreplaceSettings.EnableHyphenReplacement = EnableTypographicReplacement;
            AutoreplaceSettings.AutoreplacePhrases = GetAutoreplacePhrases();
            AutoreplaceSettings.EnableSmartQuotes = EnableSmartQuotes;
            AutoreplaceSettings.EnableSpecialCharacterReplacement = EnableSpecialCharacterReplacement;
            AutoreplaceSettings.EnableEmoticonsReplacement = EnableEmoticonsReplacement;
        }

    }
}
