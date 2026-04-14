// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using OpenLiveWriter.ApplicationFramework.Preferences;

namespace OpenLiveWriter.SpellChecker
{

    /// <summary>
    /// Interface used for specifying spelling options
    /// </summary>
    public class SpellingPreferences : Preferences
    {
        /// <summary>
        /// The FlagsPreferences sub-key.
        /// </summary>
        private const string PREFERENCES_SUB_KEY = "Spelling";

        /// <summary>
        /// Initializes a new instance of the SpellingPreferences class.
        /// </summary>
        public SpellingPreferences()
            : base(PREFERENCES_SUB_KEY)
        {
        }

        /// <summary>
        /// Run the spelling form before publishing
        /// </summary>
        public bool CheckSpellingBeforePublish
        {
            get { return checkSpellingBeforePublish; }
            set { checkSpellingBeforePublish = value; Modified(); }
        }
        private bool checkSpellingBeforePublish;

        /// <summary>
        /// Run real time spell checking
        /// </summary>
        public bool RealTimeSpellChecking
        {
            get { return realTimeSpellChecking; }
            set { realTimeSpellChecking = value; Modified(); }
        }
        private bool realTimeSpellChecking;
        
        /// <summary>
        /// Main language for spell checking
        /// </summary>
        public string Language
        {
            get { return language; }
            set { language = value; Modified(); }
        }
        private string language;

        /// <summary>
        /// Enables/Disables AutoCorrect
        /// </summary>
        public bool EnableAutoCorrect
        {
            get { return enableAutoCorrect; }
            set { enableAutoCorrect = value; Modified(); }
        }
        private bool enableAutoCorrect;

        /// <summary>
        /// Load preference values
        /// </summary>
        protected override void LoadPreferences()
        {
            realTimeSpellChecking = SpellingSettings.RealTimeSpellChecking;
            checkSpellingBeforePublish = SpellingSettings.CheckSpellingBeforePublish;
            enableAutoCorrect = SpellingSettings.EnableAutoCorrect;
            language = SpellingSettings.Language;
        }

        /// <summary>
        /// Save preference values
        /// </summary>
        protected override void SavePreferences()
        {
            SpellingSettings.RealTimeSpellChecking = realTimeSpellChecking;
            SpellingSettings.CheckSpellingBeforePublish = checkSpellingBeforePublish;
            SpellingSettings.EnableAutoCorrect = enableAutoCorrect;
            SpellingSettings.Language = language;
            SpellingSettings.FireChangedEvent();
        }
    }

}
