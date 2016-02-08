// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Settings;

namespace OpenLiveWriter.PostEditor
{
    /// <summary>
    /// Summary description for PostEditorSettingsM1.
    /// </summary>
    internal sealed class PostEditorSettingsM1
    {

        internal static SettingsPersisterHelper SettingsKey = ApplicationEnvironment.PreferencesSettingsRoot.GetSubSettings("PostEditor");

    }
}
