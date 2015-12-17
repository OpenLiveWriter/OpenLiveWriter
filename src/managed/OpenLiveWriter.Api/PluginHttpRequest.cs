// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.Api
{
    /// <summary>
    /// Specifies the desired interaction with the Internet cache during
    /// HTTP requests.
    /// </summary>
    public enum HttpRequestCacheLevel
    {
        /// <summary>
        /// Do not read from or write to the Internet cache.
        /// </summary>
        BypassCache,

        /// <summary>
        /// Read from the Internet cache if possible, otherwise retrieve
        /// from the network. If the request is successful then write the
        /// response to the Internet cache.
        /// </summary>
        CacheIfAvailable,

        /// <summary>
        /// Attempt to read the requested resource from the Internet cache
        /// (in no case attempt to retrieve the resource from the network).
        /// </summary>
        CacheOnly,

        /// <summary>
        /// Attempt to retrieve the requested resource from the network. If
        /// the request is successful then write the response to the Internet cache.
        /// </summary>
        Reload
    }

    /// <summary>
    /// Provides the ability to execute Http requests that utilize the (optional) Web Proxy
    /// settings specified by the user in the Web Proxy Preferences panel. Also enables
    /// reading from and writing to the Internet cache as part of request processing.
    /// </summary>
    public class PluginHttpRequest
    {
        /// <summary>
        /// Is an Internet connection currently available.
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
        /// Returns the Web proxy that Writer is configured to use.
        /// </summary>
        public static IWebProxy GetWriterProxy()
        {
            IWebProxy proxy = HttpRequestHelper.GetProxyOverride();

            if (proxy == null)
                // TODO: Some plugins (like Flickr4Writer) cast this to a WebProxy
                // Since the fix for this returns an explicit IWebProxy, we'll need to have
                // the Flickr4Writer plugin fixed, then alter this to use the correct call.
#pragma warning disable 612,618
                proxy = System.Net.WebProxy.GetDefaultProxy();
#pragma warning restore 612, 618
            return proxy;
        }

        /// <summary>
        /// Initialize a new Http request.
        /// </summary>
        /// <param name="requestUrl">Url for resource to request.</param>
        public PluginHttpRequest(string requestUrl)
            : this(requestUrl, HttpRequestCacheLevel.BypassCache)
        {
        }

        /// <summary>
        /// Initialize a new Http request with the specified cache level.
        /// </summary>
        /// <param name="requestUrl">Url for resource to request.</param>
        /// <param name="cacheLevel">Cache level for request.</param>
        public PluginHttpRequest(string requestUrl, HttpRequestCacheLevel cacheLevel)
        {
            _requestUrl = requestUrl;
            _cacheLevel = cacheLevel;
        }

        /// <summary>
        /// Automatically follow host redirects of the request (defaults to true).
        /// </summary>
        public bool AllowAutoRedirect
        {
            get { return _allowAutoRedirect; }
            set { _allowAutoRedirect = value; }
        }
        private bool _allowAutoRedirect = true;

        /// <summary>
        /// Cache level for Http request (defaults to BypassCache).
        /// </summary>
        public HttpRequestCacheLevel CacheLevel
        {
            get { return _cacheLevel; }
            set { _cacheLevel = value; }
        }
        private HttpRequestCacheLevel _cacheLevel = HttpRequestCacheLevel.BypassCache;

        /// <summary>
        /// Content-type of post data (this value must be specified if post data is included
        /// in the request).
        /// </summary>
        public string ContentType
        {
            get { return _contentType; }
            set { _contentType = value; }
        }
        private string _contentType = null;

        /// <summary>
        /// Post data to send along with the request.
        /// </summary>
        public byte[] PostData
        {
            get { return _postData; }
            set { _postData = value; }
        }
        private byte[] _postData;

        /// <summary>
        /// Retrieve the resource (with no timeout).
        /// </summary>
        /// <returns>A stream representing the requested resource. Can return null
        /// if the CacheLevel is CacheOnly and the resource could not be found
        /// in the cache.</returns>
        public Stream GetResponse()
        {
            return GetResponse(System.Threading.Timeout.Infinite);
        }

        /// <summary>
        /// Retrieve the resource with the specified timeout (in ms).
        /// </summary>
        /// <param name="timeoutMs">Timeout (in ms) for the request.</param>
        /// <returns>A stream representing the requested resource. Can return null
        /// if the CacheLevel is CacheOnly and the resource could not be found
        /// in the cache.</returns>
        public Stream GetResponse(int timeoutMs)
        {
            // always try to get the url from the cache first
            if (ReadFromCache)
            {
                Internet_Cache_Entry_Info cacheInfo;
                if (WinInet.GetUrlCacheEntryInfo(_requestUrl, out cacheInfo))
                {
                    if (File.Exists(cacheInfo.lpszLocalFileName))
                    {
                        return new FileStream(cacheInfo.lpszLocalFileName, FileMode.Open, FileAccess.Read);
                    }
                }
            }

            // if that didn't succeed then try to get the file from
            // the web as long as the user has requested we do this
            if (MakeRequest)
            {
                HttpWebResponse response = HttpRequestHelper.SendRequest(_requestUrl, delegate (HttpWebRequest request)
                {
                    request.AllowAutoRedirect = AllowAutoRedirect;
                    request.Timeout = timeoutMs;
                    request.ContentType = ContentType;
                    if (PostData != null)
                    {
                        request.Method = "POST";
                        using (Stream requestStream = request.GetRequestStream())
                            StreamHelper.Transfer(new MemoryStream(PostData), requestStream);
                    }
                });

                try
                {
                    Stream responseStream = response.GetResponseStream();
                    if (responseStream != null)
                    {
                        if (WriteToCache)
                            return WriteResponseToCache(responseStream);
                        else
                            return StreamHelper.CopyToMemoryStream(responseStream);
                    }
                    else
                    {
                        return null;
                    }
                }
                finally
                {
                    response.Close();
                }
            }
            else
            {
                // look only in the cache
                return null;
            }
        }

        private bool ReadFromCache
        {
            get
            {
                return CacheLevel == HttpRequestCacheLevel.CacheIfAvailable ||
                        CacheLevel == HttpRequestCacheLevel.CacheOnly;
            }
        }

        private bool MakeRequest
        {
            get
            {
                return CacheLevel != HttpRequestCacheLevel.CacheOnly;
            }
        }

        private bool WriteToCache
        {
            get
            {
                return CacheLevel == HttpRequestCacheLevel.CacheIfAvailable ||
                        CacheLevel == HttpRequestCacheLevel.CacheOnly ||
                        CacheLevel == HttpRequestCacheLevel.Reload;
            }
        }

        private Stream WriteResponseToCache(Stream responseStream)
        {
            StringBuilder fileNameBuffer = new StringBuilder(Kernel32.MAX_PATH * 2);
            bool created = WinInet.CreateUrlCacheEntry(
                _requestUrl, 0, UrlHelper.GetExtensionForUrl(_requestUrl), fileNameBuffer, 0);

            if (created)
            {
                // copy the stream to the file
                string cacheFileName = fileNameBuffer.ToString();
                using (FileStream cacheFile = new FileStream(cacheFileName, FileMode.Create))
                    StreamHelper.Transfer(responseStream, cacheFile);

                // commit the file to the cache

                System.Runtime.InteropServices.ComTypes.FILETIME zeroFiletime = new System.Runtime.InteropServices.ComTypes.FILETIME();
                bool committed = WinInet.CommitUrlCacheEntry(
                    _requestUrl, cacheFileName, zeroFiletime, zeroFiletime, CACHE_ENTRY.NORMAL, IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero);
                Trace.Assert(committed);

                // return a stream to the file
                return new FileStream(cacheFileName, FileMode.Open);
            }
            else
            {
                Trace.Fail("Unexpedcted failure to create cache entry for url " + _requestUrl + ": " + Marshal.GetLastWin32Error().ToString(CultureInfo.InvariantCulture));
                return responseStream;
            }
        }

        private string _requestUrl;
    }
}
