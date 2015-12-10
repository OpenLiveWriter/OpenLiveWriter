// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Settings;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    public sealed class HtmlEditorSettings
    {
        static HtmlEditorSettings()
        {
            try
            {
                FileVersionInfo info = BrowserHelper.IEVersion;
                if (info != null)
                {
                    // Don't be aggressive if IE7 isn't installed
                    // if (info.FileMajorPart <= 6)
                    //	_aggressivelyInvalidate = false;
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Error while attempting to detect whether to aggressively invalidate: " + ex.ToString());
            }
        }

        // If IE7 Beta 2 Preview is installed, the editor runs into problems
        // updating the display of title text and the first line of body text.
        //
        // This flag indicates that we should use invalidate to workaround this issue.
        public static bool AggressivelyInvalidate
        {
            get
            {
                return false;
            }
        }
        //private static bool _aggressivelyInvalidate = true;

        internal static SettingsPersisterHelper SettingsKey = PostEditorSettings.SettingsKey.GetSubSettings("HtmlEditor");
    }
}
