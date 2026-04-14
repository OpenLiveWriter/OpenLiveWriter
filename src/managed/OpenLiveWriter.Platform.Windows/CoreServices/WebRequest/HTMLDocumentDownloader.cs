// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.CoreServices.Progress;

namespace OpenLiveWriter.CoreServices
{

    /// <summary>
    /// Gets HTMLDocuments for a given URL.  It gets two versions of the Document,
    /// one that can be used to get a list of references in the Document and one that
    /// can used to extract the literal HTML that should be used when saving or dealing
    /// with the HTML directly.
    /// </summary>
    public class HTMLDocumentDownloader : IDisposable
    {

        /// <summary>
        /// Constructs a new HTMLDocument downloader
        /// </summary>
        /// <param name="parentControl">The control that parents the downloader (must be on a STA thread)</param>
        public HTMLDocumentDownloader(Control parentControl)
        {
            if (parentControl != null)
                _parentControl = parentControl;
        }

        public HTMLDocumentDownloader(Control parentControl, string url, string title, string cookieString, int timeOutMs, bool permitScriptExecution) :
            this(parentControl, url, title, cookieString, timeOutMs, permitScriptExecution, null)
        {

        }

        /// <summary>
        /// Creates a new HTMLDocumentDownloader
        /// </summary>
        /// <param name="synchronizeInvoke">The control that parents the downloader (must be on a STA thread)</param>
        /// <param name="url">The url to download</param>
        public HTMLDocumentDownloader(Control parentControl, string url, string title, string cookieString, int timeOutMs, bool permitScriptExecution, byte[] postData) : this(parentControl)
        {
            _timeoutMs = timeOutMs;
            _cookieString = cookieString;
            _title = title;
            _permitScriptExecution = permitScriptExecution;
            _postData = postData;
            _url = url;
        }

        public bool PermitScriptExecution
        {
            get
            {
                return _permitScriptExecution;
            }
            set
            {
                _permitScriptExecution = value;
            }
        }
        private bool _permitScriptExecution = true;
        private readonly byte[] _postData;

        private string CleanUrl(string url)
        {
            if (url == null)
                return url;

            url = UrlHelper.GetUrlWithoutAnchorIdentifier(url);

            if (UrlHelper.IsFileUrl(url))
                url = HttpUtility.UrlDecode(url);

            return url;
        }

        /// <summary>
        /// The url to download
        /// </summary>
        public string Url
        {
            get
            {
                return _url;
            }
            set
            {
                _url = value;
            }
        }

        public string Title
        {
            get
            {
                if (_title == null)
                    return _url;
                else
                    return _title;
            }
            set
            {
                _title = value;
            }
        }
        private string _title = null;

        public int TimeoutMs
        {
            get
            {
                return _timeoutMs;
            }
            set
            {
                _timeoutMs = value;
            }
        }
        private int _timeoutMs = 120000;

        public IHTMLDocument2 HtmlDocument
        {
            get
            {
                return _htmlDocument;
            }
        }
        private IHTMLDocument2 _htmlDocument = null;

        private delegate void Download(IProgressHost progressHost);

        /// <summary>
        /// Initiate a download, using the progressHost to provide progress feedback
        /// </summary>
        /// <param name="progressHost">The progressHost to provide feedback to</param>
        /// <returns>this</returns>
        public object DownloadHTMLDocument(IProgressHost progressHost)
        {
            _downloadComplete = false;

            // Call the download method on the parent STA thread
            // Then wait for it to complete (the Monitor will be pulsed upon completion)
            _parentControl.Invoke(new Download(DoDownload), new object[] { progressHost });

            lock (this)
            {
                if (_downloadComplete)
                    return this;

                DateTime endDateTime = DateTime.Now.AddMilliseconds(TimeoutMs);
                while (!_downloadComplete)
                {
                    if (!Monitor.Wait(this, Math.Max(0, (int)endDateTime.Subtract(DateTime.Now).TotalMilliseconds)))
                        throw new OperationTimedOutException();

                }

                if (_downloader.Result.Exception != null)
                    throw _downloader.Result.Exception;

                progressHost.UpdateProgress(1, 1);
                return this;
            }
        }
        private bool _downloadComplete = false;

        private void CleanupDownloader()
        {
            if (_downloader != null)
            {
                _downloader.DownloadComplete -= new EventHandler(downloader_DownloadComplete);
                _parentControl.BeginInvoke(new ThreadStart(_downloader.Dispose));
                _downloader = null;
            }
        }

        /// <summary>
        /// Initiate the download providing no progress
        /// </summary>
        public void DownloadHTMLDocument()
        {
            DownloadHTMLDocument(SilentProgressHost.Instance);
        }

        /// <summary>
        /// Indicates whether the selected URL is downloadable by this control
        /// (for example, a javascript URL isn't really downloadable by this control)
        /// </summary>
        /// <param name="url">The url to download</param>
        /// <returns>true if the url is downloadable, otherwise false</returns>
        public static bool IsDownloadableUrl(string url)
        {
            try
            {
                Uri uri = new Uri(url);
                foreach (string scheme in DownloadableSchemes)
                    if (uri.Scheme == scheme)
                        return true;
            }
            catch (Exception)
            {
                //may occur if the URL is malformed
            }
            return false;
        }

        /// <summary>
        /// The list of well known schemes
        /// </summary>
        private static string[] DownloadableSchemes = new string[] { Uri.UriSchemeFile, Uri.UriSchemeHttp, Uri.UriSchemeHttps };

        /// <summary>
        /// Downloads the Document for a particular URL
        /// </summary>
        private void DoDownload(IProgressHost progressHost)
        {
            if (_downloader == null)
                _downloader = new WebPageDownloader(_parentControl);

            // Configure the downloader, hook its complete event and start the download
            _downloader.Title = _title;
            _downloader.Url = CleanUrl(_url);
            _downloader.ExecuteScripts = _permitScriptExecution;
            _downloader.CookieString = CookieString;
            _downloader.PostData = _postData;
            _downloader.DownloadComplete += new EventHandler(downloader_DownloadComplete);
            _downloader.DownloadFromUrl(progressHost);
        }

        /// <summary>
        /// Handles the download complete event from the downloader
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void downloader_DownloadComplete(object sender, EventArgs e)
        {
            // If we're downloading the first DOM, save the HTMLSourceDom and start the Reference DOM download
            if (_downloader.Result == WebPageDownloader.WebPageDownloaderResult.Ok)
            {
                _url = _downloader.HTMLDocument.url;
                this._htmlDocument = _downloader.HTMLDocument;
            }
            MarkDownloadComplete();
        }

        private void MarkDownloadComplete()
        {
            lock (this)
            {
                _downloadComplete = true;
                Monitor.PulseAll(this);
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Disposes of this object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~HTMLDocumentDownloader()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes this object
        /// </summary>
        /// <param name="disposing">Indicates whether this was called from dispose</param>
        private void Dispose(bool disposing)
        {
            if (!disposing)
                Debug.Fail("You must dispose of HTMLDocumentDownloader");

            CleanupDownloader();

        }
        #endregion

        private WebPageDownloader _downloader;
        private Control _parentControl = null;
        private string _url;

        public string CookieString
        {
            get { return _cookieString; }
            set { _cookieString = value; }
        }
        private string _cookieString = null;
    }

    public class OperationTimedOutException : Exception
    {
    }
}
