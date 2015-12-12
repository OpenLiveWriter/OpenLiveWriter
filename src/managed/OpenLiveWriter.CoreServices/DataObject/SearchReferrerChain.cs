// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace Project31.CoreServices
{

    /// <summary>
    /// A referrer chain holds a table of referrers for urls
    /// </summary>
    [Serializable]
    public class SearchReferrerChain
    {
        /// <summary>
        /// The namespace that holds the referrer chain
        /// </summary>
        public static string REFERRER_NAMESPACE = "Project31";

        /// <summary>
        /// The name that holds the referrer chain
        /// </summary>
        public static string REFERRER_NAME = "ReferrerChain";

        /// <summary>
        /// Constructs a new referrer chain
        /// </summary>
        private SearchReferrerChain()
        {
        }

        private static SearchReferrerChain singleton = new SearchReferrerChain();
        public static SearchReferrerChain Instance
        {
            get
            {
                return singleton;
            }
        }
        private static ExplorerUrlTracker explorerUrlTracker = new ExplorerUrlTracker();

        /// <summary>
        /// Adds an entry to the referrerChain.  Note that entries are only added to the referrer chain
        /// if they are either a search or parented by something already in the referrer chain.
        /// </summary>
        /// <param name="url">The url to add</param>
        /// <param name="referrer">The url's referrer</param>
        public void Add(string url, string referrer)
        {
            if (referrer == null)
                referrer = string.Empty;

            // Add the item if it is a search or a descendant of a search
            if (IsSearchUrl(url))
                explorerUrlTracker.AddUrl(new ExplorerUrlTracker.UrlInfo(url, null));
            else if ( ContainsReferrer(referrer) )
                explorerUrlTracker.AddUrl(new ExplorerUrlTracker.UrlInfo(url, referrer));
        }

        /// <summary>
        /// Finds a search spec for a given url.
        /// </summary>
        /// <param name="url">The url to find the search spec for</param>
        /// <returns>The matching searc spec, null if no search spec could be found.</returns>
        public SearchSpec FindSearchSpec(string url)
        {
            string parent = FindParent(url, m_urlList.Length - 1);

            // Only return a search spec if the url isn't itself a search
            if (parent != url)
                return GetSearchSpec(parent);
            else
                return null;
        }

        /// <summary>
        /// Tests the url against the system provided search descriptors and returns the first matching
        /// search spec (if any).
        /// </summary>
        /// <param name="url">The url to test</param>
        /// <returns>The first matching spec, null if no search spec could be matched</returns>
        private SearchSpec GetSearchSpec(string url)
        {
            SearchSpec searchSpec = null;
            foreach (SearchDescriptor searchDescriptor in SearchDescriptors)
            {
                if (IsSearchUrl(url, searchDescriptor))
                {
                    string keywords = (string)UrlHelper.GetQueryParams(url)[searchDescriptor.KeyWordQueryParam];

                    searchSpec = new SearchSpec();
                    searchSpec.SearchProviderName = searchDescriptor.SearchProviderName;
                    searchSpec.SearchUrl = url;
                    searchSpec.Keywords = keywords.Split('+');
                    break;
                }

            }
            return searchSpec;
        }

        /// <summary>
        /// Determines if the url is a search url
        /// </summary>
        /// <param name="url">the url to test</param>
        /// <returns>true if the url is a search url, otherwise false</returns>
        private bool IsSearchUrl(string url)
        {
            if (GetSearchSpec(url) != null)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Determines if a search url is a specific search
        /// </summary>
        /// <param name="url">The url to test</param>
        /// <param name="searchDescriptor">The search descriptor to use to determine if the url
        /// is a search</param>
        /// <returns>true if the url is a search, otherwise false</returns>
        private bool IsSearchUrl(string url, SearchDescriptor searchDescriptor)
        {
            Hashtable t = UrlHelper.GetQueryParams(url);
            Uri uri = new Uri(url);
            if (uri.GetLeftPart(UriPartial.Path).IndexOf(searchDescriptor.BaseUrlMatch) > -1
                && UrlHelper.GetQueryParams(url).ContainsKey(searchDescriptor.KeyWordQueryParam))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Finds the parent of the url
        /// </summary>
        /// <param name="url">The url to find the parent of</param>
        /// <param name="startingIndex">The location in the referrers to start looking (reverse lookup)</param>
        /// <returns>The parent url</returns>
        private string FindParent(string url, int startingIndex)
        {
            string referrer = string.Empty;
            int urlIndex = GetUrlIndex(url, startingIndex);
            if (urlIndex > -1)
                referrer = m_urlList[urlIndex].Referrer;

            if (referrer == string.Empty)
                return url;
            else
                return FindParent(referrer, urlIndex);
        }

        /// <summary>
        /// Searches the referrer list for a specific url.  Note that this searches
        /// backwards through the list from the given starting index.
        /// </summary>
        /// <param name="url">The url to locate</param>
        /// <param name="startingIndex">The index in the referrers at which to start looking</param>
        /// <returns>The index to the url, -1 if the url couldn't be found</returns>
        private int GetUrlIndex(string url, int startingIndex)
        {
            int urlIndex = -1;

            for (int i = startingIndex; i > -1; i--)
            {
                if (m_urlList[i].Url == url)
                {
                    urlIndex = i;
                    break;
                }

            }
            return urlIndex;
        }

        /// <summary>
        /// Determines whether the referrer list contains a specific referrer
        /// </summary>
        /// <param name="referrer">The refrerrer</param>
        /// <returns>true if it contains the referrer, otherwise false</returns>
        private bool ContainsReferrer(string referrer)
        {
            bool containsReferrer = false;
            foreach (ExplorerUrlTracker.UrlInfo urlInfo in m_urlList)
            {
                if (urlInfo.Url == referrer)
                {
                    containsReferrer = true;
                    break;
                }
            }
            return containsReferrer;
        }

        /// <summary>
        /// The list of referrers
        /// </summary>
        private ExplorerUrlTracker.UrlInfo[] m_urlList
        {
            get
            {
                return explorerUrlTracker.GetUrlHistory();
            }
        }

        /// <summary>
        /// The search descriptors to use when matching
        /// </summary>
        private SearchDescriptor[] SearchDescriptors
        {
            get
            {
                if (m_searchDescriptors == null)
                    m_searchDescriptors = GetSearchDescriptors();
                return m_searchDescriptors;
            }
        }
        private SearchDescriptor[] m_searchDescriptors;

        /// <summary>
        /// Retrieves the search descriptors
        /// </summary>
        /// <returns></returns>
        private SearchDescriptor[] GetSearchDescriptors()
        {
            return new SearchDescriptor[]
                        {
                            new SearchDescriptor("Google", @"google.com", @"q"),
                            new SearchDescriptor("Teoma", @"teoma.com/search", @"q"),
                            new SearchDescriptor("AllTheWeb", @"alltheweb.com/search", @"q"),
                            new SearchDescriptor("Lycos", @"lycos.com", @"query"),
                            new SearchDescriptor("EBay", @"ebay.com/search", @"query"),
                            new SearchDescriptor("Yahoo", @"yahoo.com", @"p"),
                            new SearchDescriptor("Overture", @"overture.com/d/search", @"Keywords"),
                            new SearchDescriptor("Alta Vista", @"altavista.com/web/results", @"q"),
                            new SearchDescriptor("DayPop", @"daypop.com/search", @"q")
                        };
        }
    }

    /// <summary>
    /// A Search Descriptor provides the information to process
    /// a url and determine whether it is a search (and parse keywords)
    /// </summary>
    [Serializable]
    internal class SearchDescriptor
    {
        /// <summary>
        /// constructs a new search descriptor
        /// </summary>
        /// <param name="name">The human readable name of the search engine</param>
        /// <param name="baseUrlMatch">The portion of the url that will determine a
        /// match (in combination with the keywordqueryparam)</param>
        /// <param name="keyWordQueryParam">The queryparam that holds the keywords</param>
        public SearchDescriptor(string name, string baseUrlMatch, string keyWordQueryParam)
        {
            m_searchProviderName = name;
            m_baseUrlMatch = baseUrlMatch;
            m_keyWordQueryParam = keyWordQueryParam;
        }

        /// <summary>
        /// The human readable name of the search engine
        /// </summary>
        public string SearchProviderName
        {
            get
            {
                return m_searchProviderName;
            }
        }
        private string m_searchProviderName;

        /// <summary>
        /// The portion of the url that will determine a
        /// match (in combination with the keywordqueryparam)
        /// </summary>
        public string BaseUrlMatch
        {
            get
            {
                return m_baseUrlMatch;
            }
        }
        private string m_baseUrlMatch;

        /// <summary>
        /// The queryparam that holds the keywords
        /// </summary>
        public string KeyWordQueryParam
        {
            get
            {
                return m_keyWordQueryParam;
            }
        }
        private string m_keyWordQueryParam;
    }

}
