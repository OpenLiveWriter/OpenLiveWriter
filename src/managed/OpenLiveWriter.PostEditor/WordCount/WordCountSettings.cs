// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using OpenLiveWriter.CoreServices.Settings;

namespace OpenLiveWriter.PostEditor.WordCount
{
    static class WordCountSettings
    {
        public static event EventHandler SettingsChanged;

        public static bool EnableRealTimeWordCount
        {
            get
            {
                if (!_enableRealTimeWordCountInit)
                {
                    _enableRealTimeWordCount = Settings.GetBoolean(SHOWWORDCOUNT, false);
                    _enableRealTimeWordCountInit = true;
                }
                return _enableRealTimeWordCount;
            }
            set
            {
                Settings.SetBoolean(SHOWWORDCOUNT, value);
                _enableRealTimeWordCount = value;
                OnSettingsChanged();
            }
        }

        private static readonly SettingsPersisterHelper Settings = PostEditorSettings.SettingsKey.GetSubSettings("WordCount");
        private const string SHOWWORDCOUNT = "ShowWordCount";

        public static void OnSettingsChanged()
        {
            if (SettingsChanged != null)
                SettingsChanged(null, EventArgs.Empty);
        }

        private static bool _enableRealTimeWordCount = false;
        private static bool _enableRealTimeWordCountInit = false;
    }
}
