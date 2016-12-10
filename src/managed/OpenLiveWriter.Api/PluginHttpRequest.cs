// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Api
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Text;

    using JetBrains.Annotations;

    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.Interop.Windows;

    using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

    /// <summary>
    /// Provides the ability to execute Http requests that utilize the (optional) Web Proxy
    /// settings specified by the user in the Web Proxy Preferences panel. Also enables
    /// reading from and writing to the Internet cache as part of request processing.
    /// </summary>
    public class PluginHttpRequest
    {
        /// <summary>
        /// The request URL
        /// </summary>
        private readonly string requestUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginHttpRequest"/> class.
        /// </summary>
        /// <param name="requestUrl">
        /// Url for resource to request.
        /// </param>
        /// <param name="cacheLevel">
        /// Cache level for request.
        /// </param>
        public PluginHttpRequest(
            [NotNull] string requestUrl, HttpRequestCacheLevel cacheLevel = HttpRequestCacheLevel.BypassCache)
        {
            this.requestUrl = requestUrl;
            this.CacheLevel = cacheLevel;
        }

        /// <summary>
        /// Gets a value indicating whether an Internet connection is currently available.
        /// </summary>
        public static bool InternetConnectionAvailable
        {
            get
            {
                uint flags;
                return WinInet.InternetGetConnectedState(out flags, 0);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to automatically follow host redirects of the request (defaults to true).
        /// </summary>
        public bool AllowAutoRedirect { get; set; } = true;

        /// <summary>
        /// Gets or sets the cache level for Http request (defaults to BypassCache).
        /// </summary>
        public HttpRequestCacheLevel CacheLevel { get; set; } = HttpRequestCacheLevel.BypassCache;

        /// <summary>
        /// Gets or sets the content-type of post data (this value must be specified if post data is included
        /// in the request).
        /// </summary>
        [CanBeNull]
        public string ContentType { get; set; } = null;

        /// <summary>
        /// Gets or sets the post data to send along with the request.
        /// </summary>
        [CanBeNull]
        public byte[] PostData { get; set; }

        /// <summary>
        /// Gets a value indicating whether [make request].
        /// </summary>
        /// <value><c>true</c> if [make request]; otherwise, <c>false</c>.</value>
        private bool MakeRequest => this.CacheLevel != HttpRequestCacheLevel.CacheOnly;

        /// <summary>
        /// Gets a value indicating whether [read from cache].
        /// </summary>
        /// <value><c>true</c> if [read from cache]; otherwise, <c>false</c>.</value>
        private bool ReadFromCache
            =>
            this.CacheLevel == HttpRequestCacheLevel.CacheIfAvailable
            || this.CacheLevel == HttpRequestCacheLevel.CacheOnly;

        /// <summary>
        /// Gets a value indicating whether [write to cache].
        /// </summary>
        /// <value><c>true</c> if [write to cache]; otherwise, <c>false</c>.</value>
        private bool WriteToCache
            =>
            this.CacheLevel == HttpRequestCacheLevel.CacheIfAvailable
            || this.CacheLevel == HttpRequestCacheLevel.CacheOnly || this.CacheLevel == HttpRequestCacheLevel.Reload;

        /// <summary>
        /// Returns the Web proxy that Writer is configured to use.
        /// </summary>
        /// <returns>The Web proxy.</returns>
        [NotNull]
        public static IWebProxy GetWriterProxy()
        {
#pragma warning disable 612, 618

            // TODO: Some plugins (like Flickr4Writer) cast this to a WebProxy
            // Since the fix for this returns an explicit IWebProxy, we'll need to have
            // the Flickr4Writer plugin fixed, then alter this to use the correct call.
            IWebProxy proxy = HttpRequestHelper.GetProxyOverride() ?? WebProxy.GetDefaultProxy();
#pragma warning restore 612, 618

            return proxy;
        }

        /// <summary>
        /// Retrieve the resource with the specified timeout (in milliseconds).
        /// </summary>
        /// <param name="timeoutMs">Timeout (in milliseconds) for the request.</param>
        /// <returns>A stream representing the requested resource. Can return null
        /// if the CacheLevel is CacheOnly and the resource could not be found
        /// in the cache.</returns>
        [CanBeNull]
        public Stream GetResponse(int timeoutMs = System.Threading.Timeout.Infinite)
        {
            // always try to get the url from the cache first
            if (this.ReadFromCache)
            {
                Internet_Cache_Entry_Info cacheInfo;
                if (WinInet.GetUrlCacheEntryInfo(this.requestUrl, out cacheInfo))
                {
                    if (File.Exists(cacheInfo.lpszLocalFileName))
                    {
                        return new FileStream(cacheInfo.lpszLocalFileName, FileMode.Open, FileAccess.Read);
                    }
                }
            }

            // if that didn't succeed then try to get the file from
            // the web as long as the user has requested we do this
            if (this.MakeRequest)
            {
                var response = HttpRequestHelper.SendRequest(
                    this.requestUrl,
                    request =>
                        {
                            request.AllowAutoRedirect = this.AllowAutoRedirect;
                            request.Timeout = timeoutMs;
                            request.ContentType = this.ContentType;
                            if (this.PostData == null)
                            {
                                return;
                            }

                            request.Method = @"POST";
                            using (var requestStream = request.GetRequestStream())
                            {
                                using (var input = new MemoryStream(this.PostData))
                                {
                                    StreamHelper.Transfer(input, requestStream);
                                }
                            }
                        });

                try
                {
                    var responseStream = response.GetResponseStream();
                    return responseStream == null
                               ? null
                               : (this.WriteToCache
                                      ? this.WriteResponseToCache(responseStream)
                                      : StreamHelper.CopyToMemoryStream(responseStream));
                }
                finally
                {
                    response.Close();
                }
            }

            // look only in the cache
            return null;
        }

        /// <summary>
        /// Writes the response to cache.
        /// </summary>
        /// <param name="responseStream">The response stream.</param>
        /// <returns>A Stream.</returns>
        [NotNull]
        private Stream WriteResponseToCache([NotNull] Stream responseStream)
        {
            var fileNameBuffer = new StringBuilder(Kernel32.MAX_PATH * 2);
            var created = WinInet.CreateUrlCacheEntry(
                this.requestUrl,
                0,
                UrlHelper.GetExtensionForUrl(this.requestUrl),
                fileNameBuffer,
                0);

            if (created)
            {
                // copy the stream to the file
                var cacheFileName = fileNameBuffer.ToString();
                using (var cacheFile = new FileStream(cacheFileName, FileMode.Create))
                {
                    StreamHelper.Transfer(responseStream, cacheFile);
                }

                // commit the file to the cache
                var zeroFiletime = new FILETIME();
                var committed = WinInet.CommitUrlCacheEntry(
                    this.requestUrl,
                    cacheFileName,
                    zeroFiletime,
                    zeroFiletime,
                    CACHE_ENTRY.NORMAL,
                    IntPtr.Zero,
                    0,
                    IntPtr.Zero,
                    IntPtr.Zero);
                Trace.Assert(committed);

                // return a stream to the file
                return new FileStream(cacheFileName, FileMode.Open);
            }

            Trace.Fail(
                $"Unexpedcted failure to create cache entry for url {this.requestUrl}: {Marshal.GetLastWin32Error().ToString(CultureInfo.InvariantCulture)}");
            return responseStream;
        }
    }
}
