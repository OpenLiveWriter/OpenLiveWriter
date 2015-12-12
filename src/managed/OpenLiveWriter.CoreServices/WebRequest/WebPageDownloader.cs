// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using Microsoft.Win32;
using mshtml;
using OpenLiveWriter.BrowserControl;
using OpenLiveWriter.CoreServices.Progress;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Com.ActiveDocuments;
using OpenLiveWriter.Interop.SHDocVw;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;
using DWebBrowserEvents2_NewWindow2EventHandler = OpenLiveWriter.BrowserControl.DWebBrowserEvents2_NewWindow2EventHandler;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// WebPageDownloader uses the browser to download an HTML page in order to get
    /// the DOM for the page.  WebPageDownloader is designed to not display any user
    /// interface while doing this download.  The downloader does not download any files
    /// that are referenced by the page.  It does permit scripts to execute (some scripts modify
    /// the DOM).
    /// </summary>
    public class WebPageDownloader : IDisposable, IOleClientSite
    {

        public WebPageDownloader(Control parentControl)
            : this(parentControl, null)
        {
        }

        /// <summary>
        /// Constructs a new WebPageDownloader, permits script execution by default
        /// </summary>
        public WebPageDownloader(Control parentControl, WinInetCredentialsContext credentialsContext)
        {
            // create the undelrying control
            browserControl = new ExplorerBrowserControl();

            if (parentControl != null)
                browserControl.Parent = parentControl;

            browserControl.DownloadOptions = GetDownloadOptions();

            CredentialsContext = credentialsContext;

            // configure options
            browserControl.Silent = true;
        }

        public string Url;
        private string _title = null;
        public bool ExecuteScripts
        {
            set
            {
                // Control whether or not to execute scripts
                if (!value)
                {
                    browserControl.DownloadOptions |= DLCTL.NO_SCRIPTS;
                }
                else
                {
                    browserControl.DownloadOptions &= ~DLCTL.NO_SCRIPTS;
                }
            }
        }
        public byte[] PostData = null;

        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
            }
        }

        public object DownloadFromUrl(IProgressHost progressHost)
        {
            this.progressHost = progressHost;
            try
            {
                HookEvents();
                DownloadIsComplete = false;
                browserControl.Navigate(Url, false, null, PostData);
            }
            catch
            {

                UnHookEvents(false);
                throw;
            }
            return this;
        }
        private IProgressHost progressHost;

        /// <summary>
        /// Indicates whether the download is complete
        /// </summary>
        public bool DownloadIsComplete = false;

        /// <summary>
        /// Get the underlying document that the control has downloaded (only
        /// available after DownloadComplete fires)
        /// </summary>
        public IHTMLDocument2 HTMLDocument
        {
            get { return (IHTMLDocument2)browserControl.Document; }
        }

        public WebPageDownloaderResult Result
        {
            get
            {
                return _result;
            }
        }
        private WebPageDownloaderResult _result = null;

        /// <summary>
        /// Disposes this object
        /// </summary>
        public void Dispose()
        {
            if (browserControl != null)
            {
                UnHookEvents(true);
                browserControl.Dispose();
            }
            GC.SuppressFinalize(this);
        }

        ~WebPageDownloader()
        {
            Debug.Fail("Failed to dispose WebPageDownloader. Please call Dispose to clean it up.");
        }

        public int GetDownloadOptions()
        {
            int downloadControl = DLCTL.DOWNLOADONLY | DLCTL.NO_CLIENTPULL |
                                    DLCTL.NO_JAVA | DLCTL.NO_DLACTIVEXCTLS |
                                    DLCTL.NO_RUNACTIVEXCTLS | DLCTL.SILENT;

            return downloadControl;
        }

        /// <summary>
        /// Hook the browser events
        /// </summary>
        private void HookEvents()
        {
            // subscribe to events
            browserControl.ProgressChange += new BrowserProgressChangeEventHandler(browserControl_ProgressChange);
            browserControl.DocumentComplete += new BrowserDocumentEventHandler(browserControl_DocumentComplete);
            browserControl.NewWindow2 += new DWebBrowserEvents2_NewWindow2EventHandler(browserControl_NewWindow2);
            browserControl.NavigateError += new BrowserNavigateErrorEventHandler(browserControl_NavigateError);
        }

        /// <summary>
        /// Unhook the browser events
        /// </summary>
        private void UnHookEvents(bool disposing)
        {
            // unsubscribe to events
            browserControl.ProgressChange -= new BrowserProgressChangeEventHandler(browserControl_ProgressChange);
            browserControl.DocumentComplete -= new BrowserDocumentEventHandler(browserControl_DocumentComplete);
            if (disposing)
                browserControl.NewWindow2 -= new DWebBrowserEvents2_NewWindow2EventHandler(browserControl_NewWindow2);
            browserControl.NavigateError -= new BrowserNavigateErrorEventHandler(browserControl_NavigateError);
        }

        /// <summary>
        /// Event indicates that the download is complete
        /// </summary>
        public event EventHandler DownloadComplete;

        /// <summary>
        /// Called when the download is complete
        /// </summary>
        /// <param name="args">Event args</param>
        protected void OnDownloadComplete(EventArgs args)
        {
            if (DownloadComplete != null)
                DownloadComplete(this, args);
        }

        /// <summary>
        /// Handle document complete event
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void browserControl_DocumentComplete(object sender, BrowserDocumentEventArgs e)
        {
            // verify ready-state complete
            Debug.Assert(browserControl.Browser.ReadyState == tagREADYSTATE.READYSTATE_COMPLETE);

            UnHookEvents(false);

            // propagate event
            DownloadIsComplete = true;

            if (UrlHelper.IsUrl(browserControl.LocationURL) && IsDangerousSSLBoundaryCrossing())
                _result = new WebPageDownloaderResult(599, browserControl.LocationURL); //599 is hack placeholder, nothing official
            else if (_result == null)
                _result = WebPageDownloaderResult.Ok;

            OnDownloadComplete(e);
        }

        /// <summary>
        /// Handle new window event (prevent all pop-up windows from displaying)
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void browserControl_NewWindow2(object sender, DWebBrowserEvents2_NewWindow2Event e)
        {
            // prevent pop-ups!
            e.cancel = true;
        }

        private void browserControl_NavigateError(object sender, BrowserNavigateErrorEventArgs e)
        {
            _result = new WebPageDownloaderResult((int)e.StatusCode, Url);
        }

        private bool IsDangerousSSLBoundaryCrossing()
        {
            if (Url == browserControl.LocationURL)
                return false;

            if (!WarnOnZoneCrossing)
                return false;

            string currentScheme = new Uri(this.Url).Scheme.ToUpperInvariant();
            string newScheme = new Uri(browserControl.LocationURL).Scheme.ToUpperInvariant();

            if (currentScheme == newScheme)
                return false;

            return (currentScheme == HTTPS || newScheme == HTTPS);
        }
        private const string HTTPS = "HTTPS";

        private bool WarnOnZoneCrossing
        {
            get
            {
                int warnOnZoneCrossing = 0;
                try
                {

                    using (RegistryKey settingsKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings"))
                    {
                        warnOnZoneCrossing = (int)settingsKey.GetValue("WarnOnZoneCrossing", warnOnZoneCrossing);
                    }
                }
                catch (Exception e)
                {
                    Trace.WriteLine("Error checking for WarnOnZoneCrossing " + e.ToString());
                }
                return (warnOnZoneCrossing == 1);
            }
        }

        /// <summary>
        /// Handle progress changed event
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void browserControl_ProgressChange(object sender, BrowserProgressChangeEventArgs e)
        {
            if (progressHost.CancelRequested)
                throw new OperationCancelledException();

            long longMax = e.ProgressMax;
            long longComp = e.Progress;
            int intMax = (int)longMax;
            int intComp = (int)longComp;

            // Unfortunately, either Max or Completed can be greater, due to IE bug.
            // Make sure the bigger one is no bigger than int.MaxValue.
            while (longMax > int.MaxValue || longComp > int.MaxValue)
            {
                if (longMax > longComp)
                {
                    intMax = int.MaxValue;
                    intComp = (int)(((double)longComp / longMax) * intMax);
                }
                else
                {
                    intComp = int.MaxValue;
                    intMax = (int)(((double)longMax / longComp) * intComp);
                }
            }

            // Don't allow progress to exceed 100%
            if (intComp > intMax)
                intComp = intMax;

            progressHost.UpdateProgress(intComp, intMax, string.Format(CultureInfo.CurrentCulture, Res.Get(StringId.ProgressDownloading), ProgressName));
        }

        private string ProgressName
        {
            get
            {
                if (_title != null)
                    return _title;
                else
                    return Url;
            }
        }

        /// <summary>
        /// Embedded web browser
        /// </summary>
        private ExplorerBrowserControl browserControl = null;

        #region IOleClientSite Members

        /// <summary>
        /// Saves the object associated with the client site.  This should not be called.
        /// </summary>
        void IOleClientSite.SaveObject()
        {
            LOG_UN("IOleClientSite", "SaveObject");
        }

        /// <summary>
        /// Returns a moniker to the object's client site.  Not implemented, but may be called.
        /// </summary>
        int IOleClientSite.GetMoniker(OLEGETMONIKER dwAssign, OLEWHICHMK dwWhichMoniker, out IMoniker ppmk)
        {
            LOG("IOleClientSite", "GetMoniker");
            ppmk = null;
            return HRESULT.E_NOTIMPL;
        }

        /// <summary>
        /// Returns the object's IOleContainer.  Indicate container doesn't support this interface.
        /// </summary>
        int IOleClientSite.GetContainer(out IOleContainer ppContainer)
        {
            LOG("IOleClientSite", "GetContainer");
            ppContainer = null;
            return HRESULT.E_NOINTERFACE;
        }

        /// <summary>
        /// Notifies the object to make itself visible to the user.  Should not be called.
        /// </summary>
        void IOleClientSite.ShowObject()
        {
            LOG_UN("IOleClientSite", "ShowObject");
        }

        /// <summary>
        /// Notifies the object when the window should be shown or hidden.  Should not be called.
        /// </summary>
        void IOleClientSite.OnShowWindow(bool fShow)
        {
            LOG_UN("IOleClientSite", "OnShowWindow");
        }

        /// <summary>
        /// Asks container to allocate more or less space for displaying an embedded object.
        /// Should not be called.
        /// </summary>
        int IOleClientSite.RequestNewObjectLayout()
        {
            LOG_UN("IOleClientSite", "RequestNewObjectLayout");
            return HRESULT.E_NOTIMPL;
        }

        #endregion

        #region Debug helpers

        /// <summary>
        /// Log access to an interface method
        /// </summary>
        /// <param name="iface">name of interface</param>
        /// <param name="method">name of method</param>
        [Conditional("DEBUG")]
        private static void LOG(string iface, string method)
        {
            Debug.WriteLine(String.Format(CultureInfo.InvariantCulture, "{0}.{1}", iface, method));
        }

        /// <summary>
        /// Log an unexpected access to an interface method (during active
        /// development this will assert to notify us of a new use of our
        /// interface implementations)
        /// </summary>
        /// <param name="iface">name of interface</param>
        /// <param name="method">name of method</param>
        [Conditional("DEBUG")]
        private static void LOG_UN(string iface, string method)
        {
            Debug.Fail(
                String.Format(CultureInfo.InvariantCulture, "Unexpected call to {0}.{1}", iface, method));
            LOG(iface, method);
        }

        public WinInetCredentialsContext CredentialsContext
        {
            set
            {
                if (value != null)
                {
                    if (value.NetworkCredential != null)
                        browserControl.NetworkCredential = value.NetworkCredential;

                    if (value.CookieString != null)
                        browserControl.SetCookies(value.CookieString.Url, value.CookieString.Cookies);
                }
            }
        }

        public string CookieString
        {
            set { browserControl.SetCookies(Url, value); }
        }

        #endregion

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
            private int _result = -1;
            private string _url = null;

            public WebPageDownloaderException Exception
            {
                get
                {
                    if (_exception == null && IsInRange(_result, 400, 599) || _result < -1)
                        _exception = GetExceptionForStatusCode(_result, _url);
                    return _exception;
                }
            }
            private WebPageDownloaderException _exception = null;

            private bool IsInRange(int value, int startRange, int endRange)
            {
                return (value >= startRange && value <= endRange);
            }

            private WebPageDownloaderException GetExceptionForStatusCode(int statusCode, string url)
            {
                switch (statusCode)
                {
                    case 400:
                        return new WebPageDownloaderException(statusCode, "An bad request occurred downloading this document", url);
                    case 401:
                        return new WebPageDownloaderException(statusCode, "Downloading this document is not authorized.", url);
                    case 403:
                        return new WebPageDownloaderException(statusCode, "Access to the document is forbidden.", url);
                    case 404:
                        return new WebPageDownloaderException(statusCode, "The document could not be found.", url);
                    case 406:
                        return new WebPageDownloaderException(statusCode, "The request for the document was unacceptable.", url);
                    case 410:
                        return new WebPageDownloaderException(statusCode, "The document is no longer available at this address and there is no forwarding address.", url);
                    case 500:
                        return new WebPageDownloaderException(statusCode, "An internal error occurred on the server when requesting the document.", url);
                    case 501:
                        return new WebPageDownloaderException(statusCode, "The server does not support functionality to fulfill the document request.", url);
                    default:
                        return new WebPageDownloaderException(string.Format(CultureInfo.CurrentCulture, "An unknown exception occurred while downloading this document: {0}", statusCode), url);
                }
            }

        }

    }

    public class WebPageDownloaderException : Exception
    {
        public WebPageDownloaderException(string message, string finalUrl) : this(-1, message, finalUrl)
        {

        }

        public WebPageDownloaderException(int statusCode, string message, string finalUrl) : base(message)
        {
            _statusCode = statusCode;
            _finalUrl = finalUrl;
        }
        private int _statusCode = -1;
        private string _finalUrl = null;

        public int StatusCode
        {
            get
            {
                return _statusCode;
            }
        }

        public string Url
        {
            get
            {
                return _finalUrl;
            }
        }
    }

}
