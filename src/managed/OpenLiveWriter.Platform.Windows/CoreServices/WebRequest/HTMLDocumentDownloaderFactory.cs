// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices.Progress;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Interface for downloading web pages and getting the rendered content.
    /// Abstracts the difference between IE (IHTMLDocument2) and WebView2 (HTML string).
    /// </summary>
    public interface IHTMLDocumentDownloader : IDisposable
    {
        /// <summary>
        /// The URL to download.
        /// </summary>
        string Url { get; set; }

        /// <summary>
        /// Title for progress display.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Timeout in milliseconds.
        /// </summary>
        int TimeoutMs { get; set; }

        /// <summary>
        /// Whether to permit script execution.
        /// </summary>
        bool PermitScriptExecution { get; set; }

        /// <summary>
        /// Cookie string to use for the request.
        /// </summary>
        string CookieString { get; set; }

        /// <summary>
        /// Downloads the HTML document synchronously.
        /// </summary>
        /// <param name="progressHost">Progress host for feedback.</param>
        /// <returns>this</returns>
        object DownloadHTMLDocument(IProgressHost progressHost);

        /// <summary>
        /// Downloads the HTML document synchronously with no progress feedback.
        /// </summary>
        void DownloadHTMLDocument();

        /// <summary>
        /// Gets the downloaded HTML content as a string.
        /// Works with both IE and WebView2 implementations.
        /// </summary>
        string GetHTMLContent();

        /// <summary>
        /// Gets the final URL after any redirects.
        /// </summary>
        string FinalUrl { get; }
    }

    /// <summary>
    /// Factory for creating HTML document downloaders.
    /// Uses environment variable OLW_USE_MSHTML to determine which implementation to use.
    /// Default is WebView2, set OLW_USE_MSHTML=1 for legacy IE engine.
    /// NOTE: WebView2 page downloading is currently disabled due to timing issues.
    /// </summary>
    public static class HTMLDocumentDownloaderFactory
    {
        // Temporarily unused - WebView2 page downloader has timing issues
#pragma warning disable CS0169
        private static bool? _useMshtml;
#pragma warning restore CS0169

        /// <summary>
        /// Gets whether to use MSHTML (IE) for downloading.
        /// Controlled by OLW_USE_MSHTML environment variable.
        /// NOTE: Currently always returns true - WebView2 page downloading causes timing issues with 
        /// the main editor due to Application.DoEvents() message pumping.
        /// TODO: Need a different async approach that doesn't interfere with main window.
        /// </summary>
        public static bool UseMshtml
        {
            get
            {
                // Temporarily forced to true - WebView2 page downloader's message pump
                // interferes with MshtmlEditor loading, causing "document not loaded" errors.
                // The WebView2 browser control (BrowserControlFactory) still works for UI.
                return true;

                // Original logic for when WebView2 page downloading is fixed:
                // if (!_useMshtml.HasValue)
                // {
                //     string envVar = Environment.GetEnvironmentVariable("OLW_USE_MSHTML");
                //     _useMshtml = !string.IsNullOrEmpty(envVar) && 
                //                  (envVar == "1" || envVar.Equals("true", StringComparison.OrdinalIgnoreCase));
                // }
                // return _useMshtml.Value;
            }
        }

        /// <summary>
        /// Gets whether to use WebView2 (inverse of UseMshtml).
        /// Currently always false due to timing issues.
        /// </summary>
        public static bool UseWebView2 => !UseMshtml;

        /// <summary>
        /// Creates an HTML document downloader using the appropriate implementation.
        /// </summary>
        public static IHTMLDocumentDownloader Create(Control parentControl)
        {
            if (UseWebView2)
            {
                return new WebView2HTMLDocumentDownloader(parentControl);
            }
            else
            {
                return new IEHTMLDocumentDownloader(parentControl);
            }
        }

        /// <summary>
        /// Creates an HTML document downloader with full configuration.
        /// </summary>
        public static IHTMLDocumentDownloader Create(Control parentControl, string url, string title, 
            string cookieString, int timeOutMs, bool permitScriptExecution, byte[] postData = null)
        {
            if (UseWebView2)
            {
                return new WebView2HTMLDocumentDownloader(parentControl, url, title, cookieString, 
                    timeOutMs, permitScriptExecution, postData);
            }
            else
            {
                return new IEHTMLDocumentDownloader(parentControl, url, title, cookieString, 
                    timeOutMs, permitScriptExecution, postData);
            }
        }
    }

    /// <summary>
    /// IE-based implementation wrapping the existing HTMLDocumentDownloader.
    /// </summary>
    internal class IEHTMLDocumentDownloader : IHTMLDocumentDownloader
    {
        private readonly HTMLDocumentDownloader _downloader;

        public IEHTMLDocumentDownloader(Control parentControl)
        {
            _downloader = new HTMLDocumentDownloader(parentControl);
        }

        public IEHTMLDocumentDownloader(Control parentControl, string url, string title,
            string cookieString, int timeOutMs, bool permitScriptExecution, byte[] postData)
        {
            _downloader = new HTMLDocumentDownloader(parentControl, url, title, cookieString, 
                timeOutMs, permitScriptExecution, postData);
        }

        public string Url
        {
            get { return _downloader.Url; }
            set { _downloader.Url = value; }
        }

        public string Title
        {
            get { return _downloader.Title; }
            set { _downloader.Title = value; }
        }

        public int TimeoutMs
        {
            get { return _downloader.TimeoutMs; }
            set { _downloader.TimeoutMs = value; }
        }

        public bool PermitScriptExecution
        {
            get { return _downloader.PermitScriptExecution; }
            set { _downloader.PermitScriptExecution = value; }
        }

        public string CookieString
        {
            get { return _downloader.CookieString; }
            set { _downloader.CookieString = value; }
        }

        public string FinalUrl
        {
            get { return _downloader.HtmlDocument?.url ?? _downloader.Url; }
        }

        public object DownloadHTMLDocument(IProgressHost progressHost)
        {
            return _downloader.DownloadHTMLDocument(progressHost);
        }

        public void DownloadHTMLDocument()
        {
            _downloader.DownloadHTMLDocument();
        }

        public string GetHTMLContent()
        {
            if (_downloader.HtmlDocument != null)
            {
                // Get the full HTML from IHTMLDocument2
                var doc = _downloader.HtmlDocument;
                if (doc.body != null && doc.body.parentElement != null)
                {
                    return doc.body.parentElement.outerHTML;
                }
            }
            return null;
        }

        public void Dispose()
        {
            _downloader.Dispose();
        }
    }

    /// <summary>
    /// WebView2-based implementation using WebView2PageDownloader.
    /// </summary>
    internal class WebView2HTMLDocumentDownloader : IHTMLDocumentDownloader
    {
        private WebView2PageDownloader _downloader;
        private Control _parentControl;
        private string _url;
        private string _title;
        private string _cookieString;
        private int _timeoutMs = 120000;
        private bool _permitScriptExecution = true;
        private byte[] _postData;
        private string _htmlContent;
        private string _finalUrl;
        private bool _downloadComplete;

        public WebView2HTMLDocumentDownloader(Control parentControl)
        {
            _parentControl = parentControl;
        }

        public WebView2HTMLDocumentDownloader(Control parentControl, string url, string title,
            string cookieString, int timeOutMs, bool permitScriptExecution, byte[] postData)
        {
            _parentControl = parentControl;
            _url = url;
            _title = title;
            _cookieString = cookieString;
            _timeoutMs = timeOutMs;
            _permitScriptExecution = permitScriptExecution;
            _postData = postData;
        }

        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }

        public string Title
        {
            get { return _title ?? _url; }
            set { _title = value; }
        }

        public int TimeoutMs
        {
            get { return _timeoutMs; }
            set { _timeoutMs = value; }
        }

        public bool PermitScriptExecution
        {
            get { return _permitScriptExecution; }
            set { _permitScriptExecution = value; }
        }

        public string CookieString
        {
            get { return _cookieString; }
            set { _cookieString = value; }
        }

        public string FinalUrl
        {
            get { return _finalUrl ?? _url; }
        }

        public object DownloadHTMLDocument(IProgressHost progressHost)
        {
            _downloadComplete = false;

            // Create and configure WebView2 downloader on UI thread
            if (_parentControl != null && _parentControl.InvokeRequired)
            {
                _parentControl.Invoke(new Action(() => DoDownload(progressHost)));
            }
            else
            {
                DoDownload(progressHost);
            }

            // Wait for completion
            if (_downloader != null && !_downloadComplete)
            {
                if (!_downloader.WaitForCompletion(_timeoutMs))
                {
                    throw new OperationTimedOutException();
                }

                if (_downloader.Result?.Exception != null)
                {
                    throw _downloader.Result.Exception;
                }
            }

            progressHost.UpdateProgress(1, 1);
            return this;
        }

        private void DoDownload(IProgressHost progressHost)
        {
            _downloader = new WebView2PageDownloader(_parentControl);
            _downloader.Url = CleanUrl(_url);
            _downloader.Title = _title;
            _downloader.ExecuteScripts = _permitScriptExecution;
            _downloader.PostData = _postData;
            _downloader.DownloadComplete += Downloader_DownloadComplete;
            _downloader.DownloadFromUrl(progressHost);
        }

        private void Downloader_DownloadComplete(object sender, EventArgs e)
        {
            if (_downloader.Result == WebView2PageDownloader.WebPageDownloaderResult.Ok)
            {
                _htmlContent = _downloader.HtmlContent;
                _finalUrl = _downloader.FinalUrl;
            }
            _downloadComplete = true;
        }

        private string CleanUrl(string url)
        {
            if (url == null)
                return url;

            url = UrlHelper.GetUrlWithoutAnchorIdentifier(url);

            if (UrlHelper.IsFileUrl(url))
                url = System.Web.HttpUtility.UrlDecode(url);

            return url;
        }

        public void DownloadHTMLDocument()
        {
            DownloadHTMLDocument(SilentProgressHost.Instance);
        }

        public string GetHTMLContent()
        {
            return _htmlContent;
        }

        public void Dispose()
        {
            if (_downloader != null)
            {
                _downloader.DownloadComplete -= Downloader_DownloadComplete;
                if (_parentControl != null)
                {
                    _parentControl.BeginInvoke(new System.Threading.ThreadStart(_downloader.Dispose));
                }
                else
                {
                    _downloader.Dispose();
                }
                _downloader = null;
            }
        }
    }
}
