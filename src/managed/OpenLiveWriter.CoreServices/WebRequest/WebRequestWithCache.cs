// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
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

        /// <summary>
        /// Constructs a new WebRequestWithCache
        /// </summary>
        /// <param name="url">The url to which the request will be made</param>
        public WebRequestWithCache(string url)
        {
            m_url = url;
        }

        /// <summary>
        /// Synchronously retrieves a response stream for this request
        /// </summary>
        /// <returns>The stream</returns>
        public Stream GetResponseStream()
        {
            return GetResponseStream(CacheSettings.CHECKCACHE);
        }

        public WebResponse GetHeadOnly()
        {
            return GetHeadOnly(DEFAULT_TIMEOUT_MS);
        }

        public WebResponse GetHeadOnly(int timeOut)
        {

            // Note that in the event that the server returns a 403 for head, we try again using a get
            WebResponse response = null;
            if (WebRequest is HttpWebRequest)
            {
                try
                {
                    WebRequest.Method = "HEAD";
                    WebRequest.Timeout = timeOut;
                    response = WebRequest.GetResponse();
                }
                catch (WebException e)
                {
                    if (e.Status == WebExceptionStatus.Timeout)
                        return null;

                    try
                    {
                        m_webRequest = null;
                        WebRequest.Method = "GET";
                        WebRequest.Timeout = timeOut;
                        response = WebRequest.GetResponse();
                    }
                    catch (WebException we)
                    {
                        Trace.WriteLine("Error while finding WEB content type using " + WebRequest.Method + ": " + we.Message);
                    }
                }
            }
            else if (WebRequest is FileWebRequest)
            {
                try
                {
                    response = WebRequest.GetResponse();
                }
                catch (WebException we)
                {
                    Trace.WriteLine("Error while finding FILE content type using " + WebRequest.Method + ": " + we.Message);
                }
            }
            return response;
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

                if (WebRequest is HttpWebRequest)
                {
                    HttpWebRequest thisRequest = (HttpWebRequest)WebRequest;
                    thisRequest.Timeout = timeOut;
                }

                try
                {
                    stream = WebRequest.GetResponse().GetResponseStream();
                }
                catch (WebException)
                {
                }
            }
            return stream;
        }

        private WebRequest WebRequest
        {
            get
            {
                if (m_webRequest == null)
                {
                    try
                    {
                        m_webRequest = HttpRequestHelper.CreateHttpWebRequest(m_url, true);
                    }
                    catch (InvalidCastException)
                    {
                        try
                        {
                            m_webRequest = WebRequest.Create(m_url);
                        }
                        catch (NotSupportedException)
                        {
                        }
                    }
                }
                return m_webRequest;
            }
        }

        /// <summary>
        /// The web request
        /// </summary>
        private WebRequest m_webRequest;

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
