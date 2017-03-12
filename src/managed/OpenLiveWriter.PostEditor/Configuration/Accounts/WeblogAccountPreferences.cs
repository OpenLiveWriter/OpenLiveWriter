// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using OpenLiveWriter.ApplicationFramework.Preferences;

namespace OpenLiveWriter.PostEditor
{
    public class WeblogAccountPreferences : Preferences
    {
        public WeblogAccountPreferences() : base("Accounts")
        {
        }

        public bool AllowSettingsAutoUpdate
        {
            get { return _allowSettingsAutoUpdate; }
            set { _allowSettingsAutoUpdate = value; Modified(); }
        }
        private bool _allowSettingsAutoUpdate;

        public bool AllowProviderButtons
        {
            get { return _allowProviderButtons; }
            set { _allowProviderButtons = value; Modified(); }
        }
        private bool _allowProviderButtons;

        protected override void LoadPreferences()
        {
            AllowProviderButtons = PostEditorSettings.AllowProviderButtons;
            AllowSettingsAutoUpdate = PostEditorSettings.AllowSettingsAutoUpdate;
        }

        protected override void SavePreferences()
        {
            PostEditorSettings.AllowProviderButtons = AllowProviderButtons;
            PostEditorSettings.AllowSettingsAutoUpdate = AllowSettingsAutoUpdate;
        }

    }
}
