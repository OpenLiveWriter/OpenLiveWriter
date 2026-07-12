// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Net;
using System.Net.Http;

namespace OpenLiveWriter.Publishing
{
    /// <summary>
    /// Builds <see cref="HttpClient"/> instances for the cross-platform publish
    /// transports, optionally routing through a configured web proxy.
    /// </summary>
    public static class PublishingHttpClientFactory
    {
        /// <summary>
        /// Creates an <see cref="HttpClient"/> whose handler honours
        /// <paramref name="proxy"/> when <see cref="WebProxyConfiguration.IsActive"/>
        /// is true.
        /// </summary>
        public static HttpClient Create(WebProxyConfiguration proxy = null) =>
            new HttpClient(CreateHandler(proxy));

        /// <summary>
        /// Builds the handler so unit tests can assert proxy wiring without a live client.
        /// </summary>
        public static HttpClientHandler CreateHandler(WebProxyConfiguration proxy = null)
        {
            var handler = new HttpClientHandler();

            if (proxy != null && proxy.IsActive)
            {
                string proxyUrl = proxy.Hostname.Trim();
                if (proxy.Port > 0)
                    proxyUrl += ":" + proxy.Port;

                var webProxy = new WebProxy(proxyUrl, false);
                if (!string.IsNullOrEmpty(proxy.Username))
                {
                    webProxy.Credentials = new NetworkCredential(
                        proxy.Username, proxy.Password ?? string.Empty);
                }

                handler.Proxy = webProxy;
                handler.UseProxy = true;
            }

            return handler;
        }
    }
}
