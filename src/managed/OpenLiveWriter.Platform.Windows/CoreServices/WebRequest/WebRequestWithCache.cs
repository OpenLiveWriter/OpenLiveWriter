// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// WebRequestWithCache provides a mechanism for retrieve web data synchronously.
    /// It includes the capability to automatically check the local
    /// internet explorer cache for improved performance.
    /// </summary>
    internal class WebRequestWithCache
    {
        private readonly bool _isFileUrl;
        private readonly string _filePath;

        /// <summary>
        /// Constructs a new WebRequestWithCache
        /// </summary>
        /// <param name="url">The url to which the request will be made</param>
        public WebRequestWithCache(string url)
        {
            m_url = url;

            // Check if this is a file:// URL - handle directly without WebRequest
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri) && uri.Scheme == Uri.UriSchemeFile)
            {
                _isFileUrl = true;
                _filePath = uri.LocalPath;
            }
        }

        /// <summary>
        /// Synchronously retrieves a response stream for this request
        /// </summary>
        /// <returns>The stream</returns>
        public Stream GetResponseStream()
        {
            return GetResponseStream(CacheSettings.CHECKCACHE);
        }

        public HttpResponseMessage GetHeadOnly()
        {
            return GetHeadOnly(DEFAULT_TIMEOUT_MS);
        }

        public HttpResponseMessage GetHeadOnly(int timeOut)
        {
            // For file:// URLs, we can't get "head only" - return null
            // The caller should check file existence directly
            if (_isFileUrl)
            {
                return null;
            }

            // Note that in the event that the server returns a 403 for head, we try again using a get
            try
            {
                using var cts = new CancellationTokenSource(timeOut);
                using var headRequest = new HttpRequestMessage(HttpMethod.Head, m_url);
                var response = HttpRequestHelper.HttpClient.SendAsync(headRequest, cts.Token).GetAwaiter().GetResult();
                
                if (response.IsSuccessStatusCode)
                {
                    return response;
                }
                
                // Try GET as fallback
                response.Dispose();
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (HttpRequestException)
            {
                // Fall through to try GET
            }

            // Try GET as fallback
            try
            {
                using var cts = new CancellationTokenSource(timeOut);
                using var getRequest = new HttpRequestMessage(HttpMethod.Get, m_url);
                return HttpRequestHelper.HttpClient.SendAsync(getRequest, HttpCompletionOption.ResponseHeadersRead, cts.Token).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Error while finding WEB content type using GET: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Synchronously retrieves a response stream for this request
        /// </summary>
        /// <param name="useCache">true to use cache, otherwise false</param>
        /// <returns>The stream</returns>
        public Stream GetResponseStream(CacheSettings cacheSettings)
        {
            return GetResponseStream(cacheSettings, DEFAULT_TIMEOUT_MS);
        }

        /// <summary>
        /// Synchronously retrieves a response stream for this request
        /// </summary>
        /// <param name="timeOut">timeout, in ms</param>
        /// <param name="useCache">true to use cache, otherwise false</param>
        /// <returns>The stream</returns>
        public Stream GetResponseStream(CacheSettings cacheSettings, int timeOut)
        {
            Stream stream = Stream.Null;

            // Handle file:// URLs directly without WebRequest
            if (_isFileUrl)
            {
                try
                {
                    if (File.Exists(_filePath))
                    {
                        return new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"WebRequestWithCache: Error reading file {_filePath}: {ex.Message}");
                }
                return Stream.Null;
            }

            // Check the cache
            if (cacheSettings != CacheSettings.NOCACHE)
            {
                Internet_Cache_Entry_Info cacheInfo;
                if (WinInet.GetUrlCacheEntryInfo(m_url, out cacheInfo))
                {
                    if (File.Exists(cacheInfo.lpszLocalFileName))
                    {
                        stream = new FileStream(cacheInfo.lpszLocalFileName, FileMode.Open, FileAccess.Read);
                    }
                }
            }

            // Make a synchronous request, if necessary
            if (stream == Stream.Null && cacheSettings != CacheSettings.CACHEONLY)
            {
                if (m_url == null)
                    return null;

                try
                {
                    using var cts = new CancellationTokenSource(timeOut);
                    var response = HttpRequestHelper.HttpClient.GetAsync(m_url, cts.Token).GetAwaiter().GetResult();
                    if (response.IsSuccessStatusCode)
                    {
                        // Copy to memory stream so we own the data
                        var memStream = new MemoryStream();
                        response.Content.ReadAsStream().CopyTo(memStream);
                        memStream.Position = 0;
                        stream = memStream;
                    }
                }
                catch (Exception ex) when (ex is HttpRequestException || ex is OperationCanceledException)
                {
                    Trace.WriteLine($"WebRequestWithCache: Request failed for {m_url}: {ex.Message}");
                }
            }
            return stream;
        }

        /// <summary>
        /// The url
        /// </summary>
        private string m_url;

        /// <summary>
        /// default timeout for request
        /// </summary>
        private static int DEFAULT_TIMEOUT_MS = 20000;

        /// <summary>
        /// Cache settings control how the cache is checked.
        /// </summary>
        public enum CacheSettings
        {
            CHECKCACHE,
            NOCACHE,
            CACHEONLY
        }
    }
}
