// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using OpenLiveWriter.Publishing;

namespace OpenLiveWriter.App.Avalonia.Settings
{
    /// <summary>Maps shell <see cref="AppPreferences"/> to publishing-layer proxy config.</summary>
    public static class WebProxyMapper
    {
        public static WebProxyConfiguration ToConfiguration(AppPreferences prefs)
        {
            if (prefs == null)
                return new WebProxyConfiguration();

            return new WebProxyConfiguration
            {
                Enabled = prefs.ProxyEnabled,
                Hostname = prefs.ProxyHostname,
                Port = prefs.ProxyPort,
                Username = prefs.ProxyUsername,
                Password = prefs.ProxyPassword
            };
        }
    }
}
