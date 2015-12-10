// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.CoreServices.Settings;
using OpenLiveWriter.PostEditor;
using OpenLiveWriter.PostEditor.BlogProviderButtons;

namespace OpenLiveWriter.PostEditor.Configuration.Wizard
{
    public sealed class WeblogConfigurationWizardSettings
    {

        public static string LastServiceName
        {
            get
            {
                return SettingsKey.GetString(LAST_SERVICE_NAME, String.Empty);
            }
            set
            {
                // record the service name and add it to our list
                SettingsKey.SetString(LAST_SERVICE_NAME, value);
                _serviceNames.SetString(value, String.Empty);
            }

        }
        private const string LAST_SERVICE_NAME = "LastServiceName";

        public static string[] ServiceNamesUsed
        {
            get
            {
                return _serviceNames.SettingsPersister.GetNames();
            }
        }

        internal static SettingsPersisterHelper SettingsKey = PostEditorSettings.SettingsKey.GetSubSettings("ConfigurationWizard");
        private static readonly SettingsPersisterHelper _serviceNames = SettingsKey.GetSubSettings("ServiceNames");

    }
}
