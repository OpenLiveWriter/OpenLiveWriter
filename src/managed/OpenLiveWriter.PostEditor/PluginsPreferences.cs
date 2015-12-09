// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections;
using OpenLiveWriter.ApplicationFramework.Preferences;
using OpenLiveWriter.PostEditor.ContentSources;

namespace OpenLiveWriter.PostEditor
{
    internal class PluginsPreferences : OpenLiveWriter.ApplicationFramework.Preferences.Preferences
    {
        public PluginsPreferences() : base("Plugins")
        {
        }

        public void SetPluginEnabledState(ContentSourceInfo contentSourceInfo, bool enabled)
        {
            _enabledStateChanges[contentSourceInfo] = enabled;
            Modified();
        }

        public bool GetPluginEnabledState(ContentSourceInfo contentSourceInfo)
        {
            if (_enabledStateChanges.Contains(contentSourceInfo))
                return (bool)_enabledStateChanges[contentSourceInfo];
            else
                return contentSourceInfo.Enabled;
        }

        protected override void LoadPreferences()
        {
            _enabledStateChanges.Clear();
        }

        protected override void SavePreferences()
        {
            foreach (DictionaryEntry entry in _enabledStateChanges)
            {
                ContentSourceInfo contentSourceInfo = entry.Key as ContentSourceInfo;
                contentSourceInfo.Enabled = (bool)entry.Value;
            }
            _enabledStateChanges.Clear();
        }

        private Hashtable _enabledStateChanges = new Hashtable();

    }
}
