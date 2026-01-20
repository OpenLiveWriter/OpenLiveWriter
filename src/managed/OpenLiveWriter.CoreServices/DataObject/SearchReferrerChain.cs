// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// A referrer chain holds a table of referrers for urls.
    /// NOTE: This is a stub implementation. The original ExplorerUrlTracker/SearchSpec types
    /// are no longer available. This functionality is obsolete.
    /// </summary>
    [Serializable]
    public class SearchReferrerChain
    {
        public static string REFERRER_NAMESPACE = "OpenLiveWriter";
        public static string REFERRER_NAME = "ReferrerChain";

        private SearchReferrerChain() { }

        private static SearchReferrerChain singleton = new SearchReferrerChain();
        public static SearchReferrerChain Instance => singleton;

        public void Add(string url, string referrer)
        {
            // No-op: ExplorerUrlTracker no longer available
        }

        public SearchSpec FindSearchSpec(string url)
        {
            // No-op: Search tracking no longer available
            return null;
        }
    }

    /// <summary>
    /// Holds search specification information (stub implementation)
    /// </summary>
    [Serializable]
    public class SearchSpec
    {
        public string SearchProviderName { get; set; }
        public string SearchUrl { get; set; }
        public string[] Keywords { get; set; }
    }
}
