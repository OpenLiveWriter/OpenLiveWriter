// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.CoreServices.Settings;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.CoreServices
{

    public sealed class WebProxySettings
    {
        public static int HttpRequestTimeout
        {
            get
            {
                const int DEFAULT_TIMEOUT = 100000 * 3;

                using (SettingsPersisterHelper networkSettings = ApplicationEnvironment.PreferencesSettingsRoot.GetSubSettings("Network"))
                {
                    if (networkSettings.HasValue("Timeout"))
                    {
                        return networkSettings.GetInt32("Timeout", DEFAULT_TIMEOUT);
                    }
                }

                return DEFAULT_TIMEOUT;
            }
            set
            {
                using (SettingsPersisterHelper networkSettings = ApplicationEnvironment.PreferencesSettingsRoot.GetSubSettings("Network"))
                    networkSettings.SetInt32("Timeout", value);
            }
        }

        public static bool ProxyEnabled
        {
            get { return ReadSettingsKey.GetBoolean("Enabled", false); }
            set { WriteSettingsKey.SetBoolean("Enabled", value); }
        }

        public static string Hostname
        {
            get { return ReadSettingsKey.GetString("Hostname", null); }
            set { WriteSettingsKey.SetString("Hostname", value); }
        }

        public static int Port
        {
            get { return ReadSettingsKey.GetInt32("Port", 8080); }
            set { WriteSettingsKey.SetInt32("Port", value); }
        }

        public static string Username
        {
            get { return ReadSettingsKey.GetString("Username", null); }
            set { WriteSettingsKey.SetString("Username", value); }
        }

        public static string Password
        {
            get
            {
                return ReadSettingsKey.GetEncryptedString("Password");
            }
            set
            {
                if (value == null)
                    WriteSettingsKey.Unset("Password");
                else
                {
                    WriteSettingsKey.SetEncryptedString("Password", value);
                }
            }
        }

        #region Class Configuration (location of settings, etc)

        private static SettingsPersisterHelper WriteSettingsKey
        {
            get
            {
                return _settingsKey;
            }
        }

        private static SettingsPersisterHelper ReadSettingsKey
        {
            get
            {
                return _readSettingsKey;
            }
        }

        private static SettingsPersisterHelper _settingsKey;
        private static SettingsPersisterHelper _readSettingsKey;

        static WebProxySettings()
        {
            _settingsKey = ApplicationEnvironment.PreferencesSettingsRoot.GetSubSettings("WebProxy");
            if (ApplicationDiagnostics.ProxySettingsOverride != null)
            {
                Match m = Regex.Match(ApplicationDiagnostics.ProxySettingsOverride,
                            @"^ ( (?<username>[^@:]+) : (?<password>[^@:]+) @)? (?<host>[^@:]+) (:(?<port>\d*))? $",
                            RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture);
                if (m.Success)
                {
                    string username = m.Groups["username"].Value;
                    string password = m.Groups["password"].Value;
                    string host = m.Groups["host"].Value;
                    string port = m.Groups["port"].Value;

                    _readSettingsKey = new SettingsPersisterHelper(new MemorySettingsPersister());

                    _readSettingsKey.SetBoolean("Enabled", true);

                    if (!string.IsNullOrEmpty(username))
                        _readSettingsKey.SetString("Username", username);
                    if (!string.IsNullOrEmpty(password))
                        _readSettingsKey.SetEncryptedString("Password", password);

                    _readSettingsKey.SetString("Hostname", host);

                    if (!string.IsNullOrEmpty(port))
                        _readSettingsKey.SetInt32("Port", int.Parse(port, CultureInfo.InvariantCulture));
                }
            }

            if (_readSettingsKey == null)
                _readSettingsKey = _settingsKey;
        }

        #endregion
    }


}
