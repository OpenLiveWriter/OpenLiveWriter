// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Settings;

namespace OpenLiveWriter.HtmlEditor.Linking
{
    /// <summary>
    /// Summary description for LinkSettings.
    /// </summary>
    public class LinkSettings
    {

        public static bool ShowAdvancedOptions
        {
            get { return SettingsKey.GetBoolean(LINK_ADVANCED_OPTIONS, LINK_ADVANCED_OPTIONS_DEFAULT); }
            set
            {
                SettingsKey.SetBoolean(LINK_ADVANCED_OPTIONS, value);
            }
        }
        private const string LINK_ADVANCED_OPTIONS = "ShowLinkAdvancedOptions";
        private const bool LINK_ADVANCED_OPTIONS_DEFAULT = false;

        public static bool OpenInNewWindow
        {
            get { return SettingsKey.GetBoolean(LINK_NEW_WINDOW, LINK_NEW_WINDOW_DEFAULT); }
            set
            {
                SettingsKey.SetBoolean(LINK_NEW_WINDOW, value);
            }
        }
        private const string LINK_NEW_WINDOW = "OpenLinkNewWindow";
        private const bool LINK_NEW_WINDOW_DEFAULT = false;

        internal static SettingsPersisterHelper SettingsKey = ApplicationEnvironment.PreferencesSettingsRoot.GetSubSettings("PostEditor");
    }
}
