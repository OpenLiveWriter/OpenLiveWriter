// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections;

namespace OpenLiveWriter.CoreServices
{

    /// <summary>
    /// Provides controlling behavior for the pageToDownloadFactory
    /// </summary>
    public class PageDownloadContext
    {
        /// <summary>
        /// Creates a new page download context
        /// </summary>
        /// <param name="depth">The depth of links to follow</param>
        public PageDownloadContext(int depth)
        {
            Depth = depth;
        }

        /// <summary>
        /// The depth of links to follow (0 means only this page, 1 means this page and all its sub pages, and so on)
        /// </summary>
        public int Depth = -1;

        /// <summary>
        /// Indicates whether the downloader should only download pages that are on this domain
        /// </summary>
        public bool RestrictToDomain = false;

        /// <summary>
        /// The root domain that should be restricted
        /// </summary>
        public string RootDomain = null;

        /// <summary>
        /// The maximum total number of pages to download. The downloader will stop downloading as soon as it has exceeded
        /// this limit (no matter the depth or position in the tree).  All additional pages will not be downloaded, but the links
        /// to them will be resolved to point to their location on the web.
        /// </summary>
        public int MaxNumberOfPagesToDownload = -1;

        public bool LimitNumberOfPages = false;

        public bool LimitSizeOfFile = false;

        public bool LimitDepth = true;

        public bool ThrowOnFailure = true;

        public string CookieString = null;

        /// <summary>
        /// Indicates whether scanning should continue at a given depth
        /// </summary>
        /// <param name="currentDepth">The current depth</param>
        /// <returns>true if scanning should continue, otherwise false</returns>
        public bool ShouldContinue(int currentDepth)
        {
            // If we've exceeded the maximum number of pages that we're allowed to download
            if (LimitNumberOfPages && MaxNumberOfPagesToDownload > -1 && _currentPageCount >= MaxNumberOfPagesToDownload)
                return false;
            if (LimitDepth && currentDepth >= Depth)
                return false;
            return true;
        }

        private int _currentPageCount = 0;

        /// <summary>
        /// The number of times to try downloading a page that is timing out
        /// </summary>
        public int RetryCount = 0;

        /// <summary>
        /// The number of milliseconds to wait before timing out a page download
        /// </summary>
        public int TimeoutMS = 120000;

        /// <summary>
        /// Indicates whether a failed download from this host due to timeout should exclude any future attempt to
        /// download pages from the site (the address will be resolved to point to the host on the web)
        /// </summary>
        public bool RemoveHostIfTimeout = true;

        /// <summary>
        /// Indicates whether the downloader should obey robots.txt and robots meta tags.  Obeying them could cause the
        /// downloader to not download all pages.  Disobeying them is a bit rude.
        /// </summary>
        public bool ObeyRobotsExclusion = true;

        /// <summary>
        /// The largest non-web page file that should be downloaded.  If the file isn't downloaded, the link will be resolved
        /// to point at the file on the web.
        /// </summary>
        public float MaxFileSizeToDownload = .5f;

        /// <summary>
        /// Indicates how the downloader should behave with respect to files
        /// </summary>
        public SiteCaptureDownloadFilter DownloadFilter = SiteCaptureDownloadFilter.AllFiles;

        public bool ShouldDownloadThisUrl(UrlContentTypeInfo info)
        {
            string url = info.FinalUrl;

            if (!ShouldDownloadThisUrl(url))
                return false;

            // If we've exceeded the maximum number of pages that we're allowed to download
            if (LimitNumberOfPages && MaxNumberOfPagesToDownload > -1 && _currentPageCount >= MaxNumberOfPagesToDownload)
                return false;

            // If this file is too large
            // TODO: Should this apply to files or also web pages?  Currently applies to web pages too
            if (LimitSizeOfFile && MaxFileSizeToDownload > 0 && info.ContentLength > MaxFileSizeToDownload * 1048576)
                return false;

            // If we should only download pages and this isn't a page, filter it out
            if (DownloadFilter == SiteCaptureDownloadFilter.Pages && !MimeHelper.IsContentTypeWebPage(info.ContentType))
                return false;

            // If we should only download pages and documents and this isn't a document, filter it out
            if (DownloadFilter == SiteCaptureDownloadFilter.PagesAndDocuments && (!MimeHelper.IsContentTypeDocument(info.ContentType) && !MimeHelper.IsContentTypeWebPage(info.ContentType)))
                return false;

            return true;
        }

        public ArrayList SelectedUrlsToDownload = new ArrayList();

        public bool ShouldDownloadThisUrl(string url)
        {
            // If the url is not downloadable
            if (!HTMLDocumentDownloader.IsDownloadableUrl(url))
                return false;

            // If the url isn't in the restricted domain
            if (RestrictToDomain && RootDomain != null && UrlHelper.GetDomain(url) != UrlHelper.GetDomain(RootDomain))
                return false;

            // If we should skip timed out hosts, and this host has timed out
            if (IsTimedOutUrl(url))
                return false;

            return true;
        }

        public enum SiteCaptureDownloadFilter
        {
            Pages,
            PagesAndDocuments,
            AllFiles
        }

        public bool IsTimedOutUrl(string url)
        {
            return (TimedOutHosts.Contains(UrlHelper.GetHostName(url)) && RemoveHostIfTimeout);
        }

        public void AddPageToDownload(string url, PageToDownload pageToDownload, bool countAsPageDownload)
        {
            if (countAsPageDownload)
                _currentPageCount++;

            if (!CreatedPageToDownloadTable.ContainsKey(url))
                CreatedPageToDownloadTable.Add(url, pageToDownload);
        }

        public Hashtable CreatedPageToDownloadTable = new Hashtable();

        public bool UrlAlreadyDownloaded(string url)
        {
            return CreatedPageToDownloadTable.ContainsKey(url);
        }

        public void AddTimedOutUrl(string url)
        {
            if (!TimedOutHosts.Contains(url))
                TimedOutHosts.Add(UrlHelper.GetHostName(url));
        }
        private ArrayList TimedOutHosts = new ArrayList();

        public static PageDownloadContext OnlyThisPage
        {
            get
            {
                return new PageDownloadContext(0);
            }
        }
    }

}
