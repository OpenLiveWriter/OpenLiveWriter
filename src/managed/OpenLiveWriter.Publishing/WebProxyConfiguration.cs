// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Publishing
{
    /// <summary>
    /// Cross-platform web-proxy settings consumed by publishing HTTP transports
    /// (MetaWeblog XML-RPC, RSD detection, image upload). Mirrors the Windows
    /// <c>WebProxySettings</c> fields without pulling in ApplicationFramework.
    /// </summary>
    public sealed class WebProxyConfiguration
    {
        public bool Enabled { get; set; }
        public string Hostname { get; set; }
        public int Port { get; set; } = 8080;
        public string Username { get; set; }
        public string Password { get; set; }

        /// <summary>Whether a proxy host is configured and should be applied.</summary>
        public bool IsActive =>
            Enabled && !string.IsNullOrWhiteSpace(Hostname);
    }
}
