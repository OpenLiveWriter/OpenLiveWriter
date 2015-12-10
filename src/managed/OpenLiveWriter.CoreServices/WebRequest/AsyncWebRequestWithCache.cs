// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Net;
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
        /// The Reponse Stream returned by the WebRequestHelper
        /// </summary>
        public Stream ResponseStream;

        /// <summary>
        /// Event called when an ansychronous request is completed.
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

            // Check the cache
            if (cacheSettings != CacheSettings.NOCACHE)
            {
                Internet_Cache_Entry_Info cacheInfo;
                if (WinInet.GetUrlCacheEntryInfo(m_url, out cacheInfo))
                {
                    ResponseStream = new FileStream(cacheInfo.lpszLocalFileName, FileMode.Open, FileAccess.Read);
                    FireRequestComplete();
                }
            }

            // Make an async request
            if (ResponseStream == null && cacheSettings != CacheSettings.CACHEONLY)
            {
                try
                {
                    m_webRequest = HttpRequestHelper.CreateHttpWebRequest(m_url, true);
                }
                catch (InvalidCastException)
                {
                    m_webRequest = WebRequest.Create(m_url);
                }

                m_webRequest.Timeout = timeOut;

                m_webRequest.BeginGetResponse(new AsyncCallback(RequestCompleteHandler), new object());
            }
        }

        /// <summary>
        /// Cancels a running a synchronous request
        /// </summary>
        public void Cancel()
        {
            if (requestRunning)
                m_webRequest.Abort();
        }

        /// <summary>
        /// Handles completed asynchronous webRequest
        /// </summary>
        private void RequestCompleteHandler(
            IAsyncResult ar
            )
        {
            WebResponse response = m_webRequest.EndGetResponse(ar);
            ResponseStream = response.GetResponseStream();
            FireRequestComplete();
        }

        /// <summary>
        /// Helper method that notifies of request completion
        /// </summary>
        private void FireRequestComplete()
        {
            OnRequestComplete(EventArgs.Empty);
            requestRunning = false;
        }

        /// <summary>
        /// The web request
        /// </summary>
        private WebRequest m_webRequest;

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
