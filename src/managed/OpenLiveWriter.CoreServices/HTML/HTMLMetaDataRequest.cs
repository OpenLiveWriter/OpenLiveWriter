// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.IO;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Summary description for HTMLMetaDataRequest.
    /// </summary>
    public class HTMLMetaDataRequest
    {
        /// <summary>
        /// Constructs a new HTMLMetaDataRequest
        /// </summary>
        /// <param name="url">The URL from which to get the HTML Metadata</param>
        public HTMLMetaDataRequest(string url)
        {
            m_url = url;
        }

        /// <summary>
        /// Attempts to fetch HTMLMetaData using only the local cache.
        /// </summary>
        /// <returns>The HTMLMetaData, null if no MetaData could be provided</returns>
        public LightWeightHTMLMetaData GetMetaDataFromCache()
        {
            return GetMetaData(WebRequestWithCache.CacheSettings.CACHEONLY);
        }

        /// <summary>
        /// Attempts to fetch HTMLMetaData using the local cache or by making
        /// a synchronous request
        /// </summary>
        /// <returns>The HTMLMetaData, null if no MetaData could be provided</returns>
        public LightWeightHTMLMetaData GetMetaData()
        {
            return GetMetaData(WebRequestWithCache.CacheSettings.CHECKCACHE);
        }

        /// <summary>
        /// Fetches the HTMLMetaData
        /// </summary>
        private LightWeightHTMLMetaData GetMetaData(WebRequestWithCache.CacheSettings cacheSettings)
        {
            WebRequestWithCache webRequest = new WebRequestWithCache(m_url);
            Stream stream = webRequest.GetResponseStream(cacheSettings);
            LightWeightHTMLDocument document = null;
            try
            {
                document = LightWeightHTMLDocument.FromStream(stream, m_url);
            }
            catch (Exception e)
            {
                Debug.Fail("Couldn't get metadata from stream: " + e.Message);
            }

            if (document != null)
                return new LightWeightHTMLMetaData(document);
            else
                return null;
        }

        /// <summary>
        /// The url
        /// </summary>
        private string m_url;

    }
}
