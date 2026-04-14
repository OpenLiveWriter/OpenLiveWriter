// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// WebRequestHelper provides a mechanism for retrieve web data synchronously and
    /// asynchronously.  It includes the capability to automatically check the local
    /// internet explorer cache for improved performance.
    /// </summary>
    public class AsyncWebRequestWithCache
    {
        /// <summary>
        /// The Response Stream returned by the WebRequestHelper
        /// </summary>
        public Stream ResponseStream;

        private readonly bool _isFileUrl;
        private readonly string _filePath;
        private CancellationTokenSource _cts;

        /// <summary>
        /// Event called when an asynchronous request is completed.
        /// </summary>
        public event EventHandler RequestComplete;
        protected void OnRequestComplete(EventArgs e)
        {
            if (RequestComplete != null)
                RequestComplete(this, e);
        }

        /// <summary>
        /// Constructs a new WebRequestHelper
        /// </summary>
        /// <param name="url">The url to which the request will be made</param>
        public AsyncWebRequestWithCache(string url)
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
        /// Begins an asynchronous request using default cache and timeout behaviour.
        /// </summary>
        public void StartRequest()
        {
            StartRequest(CacheSettings.CHECKCACHE);
        }

        /// <summary>
        /// Begins an asynchronous request using the default timeout
        /// </summary>
        /// <param name="useCache">true to cache, otherwise false</param>
        public void StartRequest(CacheSettings cacheSettings)
        {
            StartRequest(cacheSettings, DEFAULT_TIMEOUT_MS);
        }

        /// <summary>
        /// Begins an asynchronous request
        /// </summary>
        /// <param name="useCache">true to use cache, otherwise false</param>
        /// <param name="timeOut">timeout, in milliseconds</param>
        public void StartRequest(CacheSettings cacheSettings, int timeOut)
        {
            requestRunning = true;

            // Handle file:// URLs directly without WebRequest
            if (_isFileUrl)
            {
                try
                {
                    if (File.Exists(_filePath))
                    {
                        ResponseStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"AsyncWebRequestWithCache: Error reading file {_filePath}: {ex.Message}");
                }
                FireRequestComplete();
                return;
            }

            // Check the cache
            if (cacheSettings != CacheSettings.NOCACHE)
            {
                Internet_Cache_Entry_Info cacheInfo;
                if (WinInet.GetUrlCacheEntryInfo(m_url, out cacheInfo))
                {
                    ResponseStream = new FileStream(cacheInfo.lpszLocalFileName, FileMode.Open, FileAccess.Read);
                    FireRequestComplete();
                    return;
                }
            }

            // Make an async request
            if (ResponseStream == null && cacheSettings != CacheSettings.CACHEONLY)
            {
                try
                {
                    _cts = new CancellationTokenSource(timeOut);
                    _ = SendRequestAsync(_cts.Token);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"AsyncWebRequestWithCache: Unable to start request for {m_url}: {ex.Message}");
                    FireRequestComplete();
                }
            }
        }

        private async Task SendRequestAsync(CancellationToken cancellationToken)
        {
            try
            {
                var response = await HttpRequestHelper.HttpClient.GetAsync(m_url, cancellationToken).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    // Copy to memory stream so we own the data
                    var memStream = new MemoryStream();
                    await response.Content.CopyToAsync(memStream, cancellationToken).ConfigureAwait(false);
                    memStream.Position = 0;
                    ResponseStream = memStream;
                }
            }
            catch (OperationCanceledException)
            {
                // Request was cancelled - that's OK
            }
            catch (HttpRequestException ex)
            {
                Trace.WriteLine($"AsyncWebRequestWithCache: Request failed for {m_url}: {ex.Message}");
            }
            finally
            {
                FireRequestComplete();
            }
        }

        /// <summary>
        /// Cancels a running request
        /// </summary>
        public void Cancel()
        {
            if (requestRunning && _cts != null)
            {
                try
                {
                    _cts.Cancel();
                }
                catch (ObjectDisposedException)
                {
                    // Already disposed
                }
            }
        }

        /// <summary>
        /// Helper method that notifies of request completion
        /// </summary>
        private void FireRequestComplete()
        {
            OnRequestComplete(EventArgs.Empty);
            requestRunning = false;
            
            // Clean up cancellation token
            try
            {
                _cts?.Dispose();
                _cts = null;
            }
            catch
            {
                // Ignore disposal errors
            }
        }

        /// <summary>
        /// The url
        /// </summary>
        public string Url
        {
            get
            {
                return m_url;
            }
        }
        private string m_url;

        /// <summary>
        /// Indicated whether the request is actually running
        /// </summary>
        private bool requestRunning = false;

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
