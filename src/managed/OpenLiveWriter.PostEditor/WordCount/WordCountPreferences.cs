// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;

namespace OpenLiveWriter.PostEditor.WordCount
{
    public class WordCountPreferences : OpenLiveWriter.ApplicationFramework.Preferences.Preferences
    {
        public WordCountPreferences()
            : base("WordCount")
        {

        }

        protected override void LoadPreferences()
        {
            EnableRealTimeWordCount = WordCountSettings.EnableRealTimeWordCount;
        }

        protected override void SavePreferences()
        {
            WordCountSettings.EnableRealTimeWordCount = EnableRealTimeWordCount;
        }

        public bool EnableRealTimeWordCount
        {
            get { return _enableRealTimeWordCount; }
            set { _enableRealTimeWordCount = value; Modified(); }
        }
        private bool _enableRealTimeWordCount;

    }
}
