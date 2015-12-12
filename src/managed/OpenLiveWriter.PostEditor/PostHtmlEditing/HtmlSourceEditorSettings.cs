// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using OpenLiveWriter.CoreServices.Settings;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    public sealed class HtmlSourceEditorSettings
    {

        internal static SettingsPersisterHelper SettingsKey = PostEditorSettings.SettingsKey.GetSubSettings("HtmlSourceEditor");
    }
}
