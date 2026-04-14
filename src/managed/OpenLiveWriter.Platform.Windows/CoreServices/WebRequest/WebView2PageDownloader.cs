// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using OpenLiveWriter.CoreServices.Progress;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// WebView2-based page downloader that replaces IE-based WebPageDownloader.
    /// Downloads web pages and provides the rendered HTML (after JavaScript execution).
    /// </summary>
    public class WebView2PageDownloader : IDisposable
    {
        private WebView2 _webView;
        private Control _parentControl;
        private bool _isInitialized;
        private string _finalUrl;
        private string _htmlContent;
        private IProgressHost _progressHost;
        private ManualResetEvent _completionEvent;

        public WebView2PageDownloader(Control parentControl)
            : this(parentControl, null)
        {
        }

        public WebView2PageDownloader(Control parentControl, WinInetCredentialsContext credentialsContext)
        {
            _parentControl = parentControl;
            _completionEvent = new ManualResetEvent(false);
            CredentialsContext = credentialsContext;

            // Create WebView2 control
            _webView = new WebView2();
            _webView.Visible = false;
            _webView.Size = new System.Drawing.Size(1024, 768);

            if (_parentControl != null)
            {
                _parentControl.Controls.Add(_webView);
            }
        }

        public string Url { get; set; }
        public string Title { get; set; }
        public byte[] PostData { get; set; }
        public bool ExecuteScripts { get; set; } = true;
        public bool DownloadIsComplete { get; private set; }
        public WinInetCredentialsContext CredentialsContext { get; set; }

        /// <summary>
        /// Gets the downloaded HTML content (after JavaScript execution).
        /// Only available after DownloadComplete fires.
        /// </summary>
        public string HtmlContent
        {
            get { return _htmlContent; }
        }

        /// <summary>
        /// Gets the final URL after any redirects.
        /// </summary>
        public string FinalUrl
        {
            get { return _finalUrl; }
        }

        public WebPageDownloaderResult Result { get; private set; }

        public event EventHandler DownloadComplete;

        public object DownloadFromUrl(IProgressHost progressHost)
        {
            _progressHost = progressHost;
            DownloadIsComplete = false;
            _completionEvent.Reset();

            try
            {
                // Initialize WebView2 and navigate
                if (_parentControl != null && _parentControl.InvokeRequired)
                {
                    _parentControl.Invoke(new Action(() => InitializeAndNavigate()));
                }
                else
                {
                    InitializeAndNavigate();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[OLW-DEBUG] WebView2PageDownloader.DownloadFromUrl exception: " + ex.Message);
                throw;
            }

            return this;
        }

        private void InitializeAndNavigate()
        {
            try
            {
                Debug.WriteLine("[OLW-DEBUG] WebView2PageDownloader - Initializing WebView2");

                // Initialize WebView2 if not already done
                if (!_isInitialized)
                {
                    // Use synchronous wait with message pump to avoid async void issues
                    var initTask = _webView.EnsureCoreWebView2Async(null);
                    WaitWithMessagePump(initTask);
                    _isInitialized = true;

                    // Configure settings
                    _webView.CoreWebView2.Settings.IsScriptEnabled = ExecuteScripts;
                    _webView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
                    _webView.CoreWebView2.Settings.IsStatusBarEnabled = false;
                    _webView.CoreWebView2.Settings.AreDevToolsEnabled = false;

                    // Hook events
                    _webView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
                    _webView.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
                }

                Debug.WriteLine("[OLW-DEBUG] WebView2PageDownloader - Navigating to: " + Url);

                // Navigate
                if (PostData != null && PostData.Length > 0)
                {
                    // WebView2 doesn't have direct POST support like IE, use a workaround
                    // For now, just do a GET - most blog detection doesn't need POST
                    _webView.CoreWebView2.Navigate(Url);
                }
                else
                {
                    _webView.CoreWebView2.Navigate(Url);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[OLW-DEBUG] WebView2PageDownloader.InitializeAndNavigate exception: " + ex.Message);
                Result = new WebPageDownloaderResult(500, Url);
                _completionEvent.Set();
                OnDownloadComplete(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Wait for a task while pumping Windows messages.
        /// This avoids deadlocks when waiting on UI thread.
        /// </summary>
        private void WaitWithMessagePump(Task task)
        {
            while (!task.IsCompleted)
            {
                Application.DoEvents();
                Thread.Sleep(10);
            }

            // Propagate any exception
            if (task.IsFaulted && task.Exception != null)
            {
                throw task.Exception.InnerException ?? task.Exception;
            }
        }

        private void CoreWebView2_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            Debug.WriteLine("[OLW-DEBUG] WebView2PageDownloader - Navigation starting to: " + e.Uri);

            if (_progressHost != null && _progressHost.CancelRequested)
            {
                e.Cancel = true;
                throw new OperationCancelledException();
            }

            _progressHost?.UpdateProgress(0, 100, string.Format(CultureInfo.CurrentCulture, Res.Get(StringId.ProgressDownloading), Title ?? Url));
        }

        private void CoreWebView2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            Debug.WriteLine("[OLW-DEBUG] WebView2PageDownloader - Navigation completed, Success: " + e.IsSuccess);

            _finalUrl = _webView.CoreWebView2.Source;

            if (e.IsSuccess)
            {
                try
                {
                    // Get the rendered HTML - use synchronous wait with message pump
                    var scriptTask = _webView.CoreWebView2.ExecuteScriptAsync("document.documentElement.outerHTML");
                    WaitWithMessagePump(scriptTask);
                    _htmlContent = scriptTask.Result;

                    // The result comes back as a JSON string, need to unescape it
                    if (_htmlContent != null && _htmlContent.StartsWith("\"") && _htmlContent.EndsWith("\""))
                    {
                        // Simple JSON string unescape - remove quotes and handle escape sequences
                        _htmlContent = _htmlContent.Substring(1, _htmlContent.Length - 2)
                            .Replace("\\n", "\n")
                            .Replace("\\r", "\r")
                            .Replace("\\t", "\t")
                            .Replace("\\\"", "\"")
                            .Replace("\\\\", "\\");
                    }

                    Debug.WriteLine("[OLW-DEBUG] WebView2PageDownloader - Got HTML, length: " + (_htmlContent?.Length ?? 0));

                    Result = WebPageDownloaderResult.Ok;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[OLW-DEBUG] WebView2PageDownloader - Error getting HTML: " + ex.Message);
                    Result = new WebPageDownloaderResult(500, Url);
                }
            }
            else
            {
                Debug.WriteLine("[OLW-DEBUG] WebView2PageDownloader - Navigation failed: " + e.WebErrorStatus);
                int httpStatusCode = MapWebErrorToHttpStatus(e.WebErrorStatus);
                Result = new WebPageDownloaderResult(httpStatusCode, Url);
            }

            DownloadIsComplete = true;
            _completionEvent.Set();
            OnDownloadComplete(EventArgs.Empty);
        }

        private int MapWebErrorToHttpStatus(CoreWebView2WebErrorStatus webError)
        {
            switch (webError)
            {
                case CoreWebView2WebErrorStatus.ServerUnreachable:
                case CoreWebView2WebErrorStatus.ConnectionAborted:
                case CoreWebView2WebErrorStatus.ConnectionReset:
                    return 503;
                case CoreWebView2WebErrorStatus.Timeout:
                    return 504;
                case CoreWebView2WebErrorStatus.CertificateCommonNameIsIncorrect:
                case CoreWebView2WebErrorStatus.CertificateExpired:
                case CoreWebView2WebErrorStatus.ClientCertificateContainsErrors:
                case CoreWebView2WebErrorStatus.CertificateRevoked:
                case CoreWebView2WebErrorStatus.CertificateIsInvalid:
                    return 495; // SSL Certificate Error
                default:
                    return 500;
            }
        }

        protected void OnDownloadComplete(EventArgs args)
        {
            DownloadComplete?.Invoke(this, args);
        }

        /// <summary>
        /// Wait for the download to complete.
        /// </summary>
        /// <param name="timeoutMs">Timeout in milliseconds</param>
        /// <returns>True if completed, false if timed out</returns>
        public bool WaitForCompletion(int timeoutMs)
        {
            return _completionEvent.WaitOne(timeoutMs);
        }

        public void Dispose()
        {
            if (_webView != null)
            {
                if (_isInitialized && _webView.CoreWebView2 != null)
                {
                    _webView.CoreWebView2.NavigationCompleted -= CoreWebView2_NavigationCompleted;
                    _webView.CoreWebView2.NavigationStarting -= CoreWebView2_NavigationStarting;
                }

                if (_parentControl != null)
                {
                    _parentControl.Controls.Remove(_webView);
                }

                _webView.Dispose();
                _webView = null;
            }

            _completionEvent?.Dispose();
            GC.SuppressFinalize(this);
        }

        ~WebView2PageDownloader()
        {
            Debug.Fail("Failed to dispose WebView2PageDownloader. Please call Dispose.");
        }

        /// <summary>
        /// Result class compatible with WebPageDownloader.WebPageDownloaderResult
        /// </summary>
        public class WebPageDownloaderResult
        {
            public static WebPageDownloaderResult Ok = new WebPageDownloaderResult(-1);

            internal WebPageDownloaderResult(int result) : this(result, null)
            {
            }

            internal WebPageDownloaderResult(int result, string url)
            {
                _result = result;
                _url = url;
            }

            private int _result;
            private string _url;

            public WebPageDownloaderException Exception
            {
                get
                {
                    if (_exception == null && (_result >= 400 && _result <= 599) || _result < -1)
                        _exception = GetExceptionForStatusCode(_result, _url);
                    return _exception;
                }
            }
            private WebPageDownloaderException _exception;

            private WebPageDownloaderException GetExceptionForStatusCode(int statusCode, string url)
            {
                switch (statusCode)
                {
                    case 400:
                        return new WebPageDownloaderException(statusCode, "A bad request occurred downloading this document", url);
                    case 401:
                        return new WebPageDownloaderException(statusCode, "Downloading this document is not authorized.", url);
                    case 403:
                        return new WebPageDownloaderException(statusCode, "Access to the document is forbidden.", url);
                    case 404:
                        return new WebPageDownloaderException(statusCode, "The document could not be found.", url);
                    case 495:
                        return new WebPageDownloaderException(statusCode, "SSL certificate error.", url);
                    case 500:
                        return new WebPageDownloaderException(statusCode, "An internal error occurred on the server.", url);
                    case 503:
                        return new WebPageDownloaderException(statusCode, "The server is unreachable.", url);
                    case 504:
                        return new WebPageDownloaderException(statusCode, "The connection timed out.", url);
                    default:
                        return new WebPageDownloaderException(string.Format(CultureInfo.CurrentCulture, "An error occurred while downloading: {0}", statusCode), url);
                }
            }
        }
    }
}
