// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices.Progress;
using OpenLiveWriter.Localization;
// TODO: Convert Site downloading to simply fill up a siteStorage and then return it when complete (use file based to minimize memory usage for site)

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Creates a list of PagesToDownload, with the ability to get subpages for the parent URL and add them to the list
    /// </summary>
    public class PageToDownloadFactory : IDisposable
    {
        /// <summary>
        /// Creates a new PageToDownloadFactory
        /// </summary>
        /// <param name="url">The url that represents the root page to download</param>
        /// <param name="downloadContext">Context that control factory behavior</param>
        /// <param name="parent">The control that should be used as a parent</param>
        public PageToDownloadFactory(string url, PageDownloadContext downloadContext, Control parent) : this(downloadContext, parent)
        {
            _url = url;

        }

        /// <summary>
        /// Creates a new PageToDownloadFactory based upon an existing HTMLDocument
        /// </summary>
        /// <param name="lightWeightHTMLDocument">The HTMLDocument that represents this page</param>
        /// <param name="downloadContext">Context that control factory behavior</param>
        /// <param name="parent">The control that should be used as a parent</param>
        public PageToDownloadFactory(LightWeightHTMLDocument lightWeightHTMLDocument, PageDownloadContext downloadContext, Control parentControl) : this(downloadContext, parentControl)
        {
            _lightWeightHTMLDocument = lightWeightHTMLDocument;
            _url = lightWeightHTMLDocument.Url;
        }

        private PageToDownloadFactory(PageDownloadContext downloadContext, Control parentControl)
        {
            _context = downloadContext;
            _parentControl = parentControl;

        }
        private Control _parentControl = null;

        /// <summary>
        /// The list of pages to download.  Note that you need to call 'DownloadPageInfo' prior to accessing
        /// this property
        /// </summary>
        public PageToDownload[] PagesToDownload
        {
            get
            {
                if (_pagesToDownload == null)
                    Debug.Fail("Call DownloadPageInfo prior to accessing this property");
                return _pagesToDownload;
            }
        }

        /// <summary>
        /// Generates the list of pages to download, providing progress.  The operation
        /// can be very time consuming, particularly if the context specifies creating multiple
        /// levels of pagesToDownload
        /// </summary>
        /// <param name="progressHost">The progress host to use to provide feedback</param>
        /// <returns>this object</returns>
        public object CreatePagesToDownload(IProgressHost progressHost)
        {
            _pagesToDownload = DownloadPages(progressHost, _url, _lightWeightHTMLDocument, null);
            return null;
        }

        /// <summary>
        /// The list of error that occurred during the page generation
        /// </summary>
        public ArrayList Errors
        {
            get
            {
                return _errors;
            }
            set
            {
                _errors = value;
            }
        }
        private ArrayList _errors = new ArrayList();

        private Hashtable _headerInfo = new Hashtable();

        /// <summary>
        /// Actually downloads the pages
        /// </summary>
        private PageToDownload[] DownloadPages(IProgressHost progress, string url, LightWeightHTMLDocument lightWeightDocument, PageToDownload parentPageToDownload)
        {
            // Check for cancel
            if (progress.CancelRequested)
                throw new OperationCancelledException();

            _currentDepth++;
            ArrayList downloadedPages = new ArrayList();

            // Set up our progress
            int thisPageTicks = FIRSTPAGETICKS;
            if (_context.Depth == _currentDepth)
                thisPageTicks = TOTALTICKS;
            ProgressTick firstPagedownloadProgress = new ProgressTick(progress, thisPageTicks, TOTALTICKS);

            string safeUrl = UrlHelper.GetUrlWithoutAnchorIdentifier(url);

            // Look up the content type of this pageToDownload
            UrlContentTypeInfo headerInfo = null;
            if (_headerInfo.ContainsKey(safeUrl))
            {
                headerInfo = (UrlContentTypeInfo)_headerInfo[safeUrl];
            }
            else
            {
                if (lightWeightDocument != null)
                    headerInfo = new UrlContentTypeInfo("text/html", url);
                else if (headerInfo == null && !_context.IsTimedOutUrl(url) && _context.ShouldDownloadThisUrl(url))
                {
                    progress.UpdateProgress(string.Format(CultureInfo.CurrentCulture, Res.Get(StringId.ProgressDeterminingType), url));
                    if (lightWeightDocument == null)
                        headerInfo = ContentTypeHelper.ExpensivelyGetUrlContentType(url, _context.TimeoutMS);
                    else
                        headerInfo = ContentTypeHelper.InexpensivelyGetUrlContentType(url);
                }
                _headerInfo.Add(safeUrl, headerInfo);
            }

            // If this is a web page and we should download it, do it!
            if ((lightWeightDocument != null && IsDownloadablePageResource(headerInfo)) ||
                (lightWeightDocument == null && IsDownloadablePageResource(headerInfo) && _context.ShouldDownloadThisUrl(headerInfo))
                )
            {
                bool downloadWorked = false;
                int downloadAttempts = -1;
                bool timedOut = true;

                // Max sure we are retrying the correct number of times
                ProgressTick pageDownloadProgress = new ProgressTick(firstPagedownloadProgress, 80, 100);
                while (!downloadWorked && downloadAttempts++ < _context.RetryCount && timedOut)
                {
                    timedOut = false;

                    pageDownloadProgress.UpdateProgress(0, 1);
                    try
                    {
                        // If we haven't downloaded this page yet download it
                        PageToDownload thisPageToDownload = null;

                        if (!_context.UrlAlreadyDownloaded(safeUrl))
                        {
                            if (lightWeightDocument == null)
                                thisPageToDownload = DownloadUrl(url, parentPageToDownload, pageDownloadProgress);
                            else
                            {
                                LightWeightHTMLDocument htmlDoc = lightWeightDocument;

                                // Only redownload if we absolutely need to
                                if (htmlDoc.HasFramesOrStyles && (htmlDoc.Frames == null || htmlDoc.StyleResourcesUrls == null))
                                {
                                    string html = htmlDoc.GenerateHtml();
                                    // Use HTML string-based extraction (WebView2-compatible)
                                    htmlDoc.UpdateBasedUponHTMLContent(html, url);
                                }
                                thisPageToDownload = new PageToDownload(htmlDoc, url, null, parentPageToDownload);
                                if (htmlDoc.StyleResourcesUrls != null)
                                    foreach (HTMLDocumentHelper.ResourceUrlInfo styleUrl in htmlDoc.StyleResourcesUrls)
                                        thisPageToDownload.AddReference(new ReferenceToDownload(styleUrl.ResourceUrl, thisPageToDownload, styleUrl.ResourceAbsoluteUrl));
                            }
                            // Add this page to our lists
                            _context.AddPageToDownload(safeUrl, thisPageToDownload, true);
                            downloadedPages.Add(thisPageToDownload);

                        }
                        else
                            thisPageToDownload = (PageToDownload)_context.CreatedPageToDownloadTable[safeUrl];

                        // If we're downloading a site, add a second copy of the root page in the references subdir
                        // This was, if the root page gets renamed, links back to it will still work correctly
                        // This is a bit of a hack, but otherwise, we'll need to escape urls whenever we output
                        // the site and change the root file name
                        if (thisPageToDownload.IsRootPage && _context.Depth > 0)
                        {
                            PageToDownload copyOfThisPageToDownload = new PageToDownload(thisPageToDownload.LightWeightHTMLDocument.Clone(), thisPageToDownload.UrlToReplace, thisPageToDownload.FileName, thisPageToDownload);
                            downloadedPages.Add(copyOfThisPageToDownload);
                        }

                        // enumerate the frames of this page and add them to the list of pages
                        PageToDownload[] subFramesToDownload = GetFramePagesToDownload(thisPageToDownload);
                        downloadedPages.AddRange(subFramesToDownload);
                        foreach (PageToDownload pageToDownload in subFramesToDownload)
                            _context.AddPageToDownload(pageToDownload.AbsoluteUrl, pageToDownload, false);

                        // Now drill down based upon the depth configuration
                        if (_context.ShouldContinue(_currentDepth))
                        {
                            ProgressTick otherPagesdownloadProgress = new ProgressTick(progress, TOTALTICKS - thisPageTicks, TOTALTICKS);
                            downloadedPages.AddRange(GetSubPagesToDownload(otherPagesdownloadProgress, downloadedPages, thisPageToDownload));
                        }
                        downloadWorked = true;
                        firstPagedownloadProgress.UpdateProgress(1, 1);

                    }
                    catch (OperationTimedOutException)
                    {
                        timedOut = true;
                    }
                    catch (WebPageDownloaderException htex)
                    {
                        HandleException(new Exception(htex.Message, htex));
                    }
                    catch (Exception ex)
                    {
                        HandleException(new Exception(String.Format(CultureInfo.CurrentCulture, "{0} could not be downloaded", _url), ex));
                    }
                }

                // If we never got the download to succeed, add it to the list of timed out Urls
                if (!downloadWorked && timedOut)
                {
                    _context.AddTimedOutUrl(_url);
                    firstPagedownloadProgress.UpdateProgress(1, 1);

                }
            }
            // If it isn't a page we'll just add the file to the reference list for the parent page
            // There is not an else, because we could be looking at a reference, but a reference that
            // should not be downloaded (in which case we just ignore it)
            else if (headerInfo != null && _context.ShouldDownloadThisUrl(headerInfo))
            {
                parentPageToDownload.AddReference(new ReferenceToDownload(url, parentPageToDownload));
                progress.UpdateProgress(1, 1);
            }

            progress.UpdateProgress(1, 1);

            _currentDepth--;
            return (PageToDownload[])downloadedPages.ToArray(typeof(PageToDownload));
        }

        private PageToDownload DownloadUrl(string url, PageToDownload parent, IProgressHost progress)
        {
            PageToDownload thisPageToDownload = null;

            // Download the current page
            LightWeightHTMLDocument lightWeightDoc = null;

            using (var downloader = HTMLDocumentDownloaderFactory.Create(_parentControl, url, null, _context.CookieString, _context.TimeoutMS, true))
            {
                downloader.DownloadHTMLDocument(progress);
                
                // Get HTML content and create document
                string htmlContent = downloader.GetHTMLContent();
                string finalUrl = downloader.FinalUrl;
                
                lightWeightDoc = LightWeightHTMLDocument.FromString(htmlContent, finalUrl, true);
                
                // Update frames and styles using HTML-based extraction (WebView2-compatible)
                lightWeightDoc.UpdateBasedUponHTMLContent(htmlContent, finalUrl);
                
                thisPageToDownload = new PageToDownload(lightWeightDoc, url, null, parent);
                // Reset the url in the event that a redirect occurred
                thisPageToDownload.AbsoluteUrl = finalUrl;
            }

            foreach (HTMLDocumentHelper.ResourceUrlInfo styleUrl in lightWeightDoc.StyleResourcesUrls)
                thisPageToDownload.AddReference(new ReferenceToDownload(styleUrl.ResourceUrl, thisPageToDownload, styleUrl.ResourceAbsoluteUrl));

            return thisPageToDownload;
        }

        private PageToDownload[] GetSubPagesToDownload(IProgressHost progress, ArrayList downloadedPagesToScan, PageToDownload parentPage)
        {
            ArrayList subPages = new ArrayList();
            // enumerate the other downloads to do (if we're scanning)
            string[] subUrlsToDownload;
            if (_context.SelectedUrlsToDownload.Count < 1)
                subUrlsToDownload = GetSubPagesToDownload((PageToDownload[])downloadedPagesToScan.ToArray(typeof(PageToDownload)), parentPage);
            else
                subUrlsToDownload = (string[])_context.SelectedUrlsToDownload.ToArray(typeof(string));

            // do the other downloads, passing the context controlling depth
            foreach (string subUrl in subUrlsToDownload)
            {
                if (_context.ShouldContinue(_currentDepth))
                {
                    ProgressTick tick = new ProgressTick(progress, 1, subUrlsToDownload.Length);
                    subPages.AddRange(DownloadPages(tick, subUrl, null, parentPage));
                }
            }
            return (PageToDownload[])subPages.ToArray(typeof(PageToDownload));
        }

        /// <summary>
        /// The root pageToDownload
        /// </summary>
        public PageToDownload RootPageToDownload
        {
            get
            {
                if (_rootPage == null)
                {
                    foreach (PageToDownload page in PagesToDownload)
                    {
                        if (page.IsRootPage)
                        {
                            _rootPage = page;
                            break;
                        }
                    }
                }
                return _rootPage;
            }
        }

        /// <summary>
        /// Gets pagesToDownloadFactories for a set of pagesToDownload
        /// </summary>
        /// <param name="pagesToDownload">The pagesToDownload to get factories for</param>
        /// <param name="context">The context controlling the factories</param>
        /// <returns>An array of PageToDownloadFactories</returns>
        private string[] GetSubPagesToDownload(PageToDownload[] pagesToDownload, PageToDownload parentPage)
        {
            ArrayList subItemsToDownload = new ArrayList();
            foreach (PageToDownload pageToDownload in pagesToDownload)
            {

                foreach (UrlInfo urlInfo in pageToDownload.LightWeightHTMLDocument.Anchors)
                {
                    if (urlInfo != null)
                    {
                        if (ShouldAddUrl(urlInfo.Url, pagesToDownload, parentPage))
                            subItemsToDownload.Add(urlInfo.Url);
                    }
                }

                LightWeightTag[] tags = pageToDownload.LightWeightHTMLDocument.GetTagsByName("AREA");
                foreach (LightWeightTag tag in tags)
                {
                    string url = tag.BeginTag.GetAttributeValue("href");
                    if (url != null)
                    {
                        if (ShouldAddUrl(url, pagesToDownload, parentPage))
                            subItemsToDownload.Add(url);
                    }
                }
            }

            return (string[])subItemsToDownload.ToArray(typeof(string));
        }

        private bool ShouldAddUrl(string url, PageToDownload[] pagesToDownload, PageToDownload parentPage)
        {
            // Filter it if its already in the list
            for (int i = 0; i < pagesToDownload.Length; i++)
                if (pagesToDownload[i] != null && UrlHelper.UrlsAreEqual(url, pagesToDownload[i].AbsoluteUrl))
                    return false;

            // Filter it if it is one of this pages parents
            PageToDownload currentParent = parentPage;
            while (currentParent != null)
            {
                if (UrlHelper.UrlsAreEqual(url, currentParent.AbsoluteUrl))
                    return false;
                currentParent = currentParent.ParentInfo;
            }
            return true;

        }

        /// <summary>
        /// Returns an array of page download infos representing the information
        /// required to download a given IHTMLDocument2
        /// </summary>
        /// <param name="rootDocument">The IHTMLDocument for which to fetch the infos</param>
        /// <param name="parentInfo">The Parent PageDownloadInfo for the subitem</param>
        /// <returns>Array of PageDownloadInfo</returns>
        private static PageToDownload[] GetFramePagesToDownload(PageToDownload parentPageToDownload)
        {
            ArrayList subFrames = new ArrayList();
            if (parentPageToDownload.LightWeightHTMLDocument.Frames != null)
            {
                foreach (LightWeightHTMLDocument frameDocument in parentPageToDownload.LightWeightHTMLDocument.Frames)
                {
                    PageToDownload subFramePageToDownload = new PageToDownload(frameDocument, frameDocument.Url, null, parentPageToDownload);
                    subFrames.Add(subFramePageToDownload);
                    subFrames.AddRange(GetFramePagesToDownload(subFramePageToDownload));
                }
            }
            return (PageToDownload[])subFrames.ToArray(typeof(PageToDownload));
        }

        private bool IsDownloadablePageResource(UrlContentTypeInfo urlContentTypeInfo)
        {
            if (urlContentTypeInfo == null)
                return false;

            // We should download pages that are web pages, css, or js files and treat them as web pages!
            return MimeHelper.IsContentTypeWebPage(urlContentTypeInfo.ContentType);
        }

        /// <summary>
        /// Helper that handles exceptions
        /// </summary>
        /// <param name="e">The exception to handle</param>
        private void HandleException(Exception e)
        {
            if (_context.ThrowOnFailure)
                ExceptionDispatchInfo.Capture(e).Throw();
            else
                Errors.Add(e);
        }

        #region IDisposable Members

        /// <summary>
        /// Disposes of this
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~PageToDownloadFactory()
        {
            Dispose(false);
        }

        /// <summary>
        /// Dispose of this
        /// </summary>
        /// <param name="disposing">indicates whether this is being called from Dispose()</param>
        private void Dispose(bool disposing)
        {
            if (!disposing)
                Debug.Fail("Please dispose of PageToDownloadFactory");
        }
        #endregion

        private int _currentDepth = -1;
        private string _url;
        private PageDownloadContext _context;
        private PageToDownload[] _pagesToDownload;
        private PageToDownload _rootPage = null;
        private LightWeightHTMLDocument _lightWeightHTMLDocument = null;

        public static int TOTALTICKS = 100000000;
        public static int FIRSTPAGETICKS = 20000000;
        public static int REMAININGTICKS = TOTALTICKS - FIRSTPAGETICKS;

    }
}
