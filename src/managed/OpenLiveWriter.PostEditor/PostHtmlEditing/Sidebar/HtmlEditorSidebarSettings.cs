// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.CoreServices.Settings;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Sidebar
{
    public sealed class HtmlEditorSidebarSettings
    {
        public static bool SidebarVisible
        {
            get { return SettingsKey.GetBoolean(SIDEBAR_VISIBLE, true); }
            set { SettingsKey.SetBoolean(SIDEBAR_VISIBLE, value); }
        }
        private const string SIDEBAR_VISIBLE = "SidebarVisible";

        internal static SettingsPersisterHelper SettingsKey = HtmlEditorSettings.SettingsKey.GetSubSettings("Sidebar");
    }
}
