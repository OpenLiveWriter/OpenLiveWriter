// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Net;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using OpenLiveWriter.CoreServices.Progress;
using OpenLiveWriter.CoreServices.Threading;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Downloads the file addressed by a URL to a local file.
    /// </summary>
    public class UrlDownloadToFile :
        IBindStatusCallback, IHttpNegotiate, IAuthenticate,
        IWindowForBindingUI, IHttpSecurity
    {

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
        /// Simple wrapper function that downloads a url and returns a file path
        /// (uses all defaults, including writing the file to a temporary path)
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string Download(string url)
        {
            return Download(url, NO_TIMEOUT);
        }

        public static string Download(string url, int timeoutMs)
        {
            return Download(url, timeoutMs, null);
        }

        public static string Download(string url, WinInetCredentialsContext credentialsContext)
        {
            return Download(url, NO_TIMEOUT, credentialsContext);
        }

        /// <summary>
        /// Simple wrapper function that downloads a url and returns a file path
        /// (uses all defaults, including writing the file to a temporary path)
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string Download(string url, int timeoutMs, WinInetCredentialsContext credentialsContext)
        {
            // do the download (will use cached images correctly, etc.)
            UrlDownloadToFile urlDownloadToFile = new UrlDownloadToFile();
            urlDownloadToFile.Url = url;
            urlDownloadToFile.TimeoutMs = timeoutMs;
            urlDownloadToFile.CredentialsContext = credentialsContext;
            urlDownloadToFile.Download();

            // return a bitmap based on the file
            return urlDownloadToFile.FilePath;
        }

        public enum DownloadActions
        {
            GET,
            POST,
        }

        public UrlDownloadToFile()
        {
            _downloadAction = DownloadActions.GET;
        }

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
        private string _url;

        public string FilePath
        {
            get
            {
                if (_filePath == null)
                {
                    _filePath = TempFileManager.Instance.CreateTempFile();
                }
                return _filePath;
            }
            set
            {
                _filePath = value;
            }
        }
        private string _filePath;

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
        private int _timeoutMs = NO_TIMEOUT;
        private const int NO_TIMEOUT = -1;

        public WinInetCredentialsContext CredentialsContext
        {
            set
            {
                if (value != null)
                {
                    if (value.NetworkCredential != null)
                        _networkCredential = value.NetworkCredential;

                    if (value.CookieString != null)
                        _cookieString = value.CookieString.Cookies;
                }
            }
        }
        private NetworkCredential _networkCredential = null;
        private string _cookieString = null;

        public DownloadActions DownloadAction
        {
            get
            {
                return _downloadAction;
            }
            set
            {
                _downloadAction = value;
            }
        }
        private DownloadActions _downloadAction;

        public byte[] PostData
        {
            get
            {
                return _postData;
            }
            set
            {
                _postData = value;
            }
        }
        private byte[] _postData;

        public bool ShowSecurityUI
        {
            get
            {
                return _showSecurityUI;
            }
            set
            {
                _showSecurityUI = value;
            }
        }
        private bool _showSecurityUI = false;

        public object Download()
        {
            return Download(SilentProgressHost.Instance);
        }

        public object Download(IProgressHost progressHost)
        {
            ResetTimeoutState();

            _finalUrl = Url;
            ProgressHost = progressHost;
            // call the URLDownloadToFile (synchronous, notifies us of progress
            // via calls to IBindStatusCallback)
            SetCookies();
            int result = UrlMon.URLDownloadToFile(
                IntPtr.Zero, Url, FilePath, 0, this);

            // check for errors in the call
            // TODO: account for errors that we purposely generate (E_ACCESSDENIED)
            // HRESULT E_ABORT 0x80004004
            switch (result)
            {
                case HRESULT.S_OK:              // The download suceeded
                    break;
                case HRESULT.E_ABORT:           // The download has been cancelled
                                                //case HRESULT.E_ACCESSDENIED:	// no idea
                    if (progressHost.CancelRequested)
                        throw new OperationCancelledException();
                    break;
                default:
                    throw new COMException("Unable to download file " + Url, result);
            };
            return this;
        }

        private void ResetTimeoutState()
        {
            _timedOut = false;
            _timeoutTime = DateTime.MinValue;

            // if the download has a timeout then calculate the end-time
            if (TimeoutMs != NO_TIMEOUT)
            {
                _timeoutTime = DateTime.Now.AddMilliseconds(TimeoutMs);
            }
        }

        private void SetCookies()
        {
            if (_cookieString != null)
                WinInet.InternetSetCookies(Url, null, _cookieString);
        }

        public bool TimedOut
        {
            get
            {
                return _timedOut;
            }
        }
        private bool _timedOut = false;
        private DateTime _timeoutTime = DateTime.MinValue;

        public string ResponseHeaders
        {
            get
            {
                return m_responseHeaders;
            }
        }
        private string m_responseHeaders;

        public string FinalUrl
        {
            get
            {
                return _finalUrl;
            }
        }
        private string _finalUrl;

        public string ContentType
        {
            get
            {
                return m_contentType;
            }
        }
        private string m_contentType = null;

        private static string[] DownloadableSchemes = new string[] { Uri.UriSchemeFile, Uri.UriSchemeHttp, Uri.UriSchemeHttps };
        private IProgressHost ProgressHost;

        #region IBindStatusCallback Members

        /// <summary>
        /// Provides information about how the bind operation should be handled when called by an asynchronous moniker.
        /// </summary>
        void IBindStatusCallback.GetBindInfo(ref uint grfBINDF, ref BINDINFO pbindinfo)
        {
            if (DownloadAction == DownloadActions.POST)
            {
                grfBINDF |= (uint)BINDF.FORMS_SUBMIT;
                grfBINDF |= (uint)BINDF.IGNORESECURITYPROBLEM;

                pbindinfo.dwBindVerb = BINDVERB.POST;

                pbindinfo.stgmedData.tymed = TYMED.HGLOBAL;
                pbindinfo.stgmedData.contents = Marshal.AllocHGlobal(PostData.Length);
                Marshal.Copy(PostData, 0, pbindinfo.stgmedData.contents, PostData.Length);
                pbindinfo.stgmedData.pUnkForRelease = IntPtr.Zero;

                pbindinfo.cbstgmedData = (uint)PostData.Length;
            }

            LOG("IBindStatusCallback", "GetBindInfo");
        }

        /// <summary>
        /// Notifies the client about the callback methods it is registered to receive.
        /// </summary>
        void IBindStatusCallback.OnStartBinding(uint dwReserved, IntPtr pib)
        {
            LOG("IBindStatusCallback", "OnStartBinding");
        }

        /// <summary>
        /// The moniker calls this method repeatedly to indicate the current progress of the bind
        /// operation, typically at reasonable intervals during a lengthy operation.
        ///
        /// The client can use the progress notification to provide progress information to the
        /// user from the ulProgress, ulProgressMax, and szStatusText parameters, or to make
        /// programmatic decisions based on the ulStatusCode parameter.
        /// </summary>
        int IBindStatusCallback.OnProgress(uint ulProgress, uint ulProgressMax, BINDSTATUS ulStatusCode, string szStatusText)
        {
            //Debug.WriteLine( String.Format( "IBindStatusCallback.OnProgress {0}: {1} ({2}/{3})", ulStatusCode, szStatusText != null ? szStatusText : String.Empty, ulProgress, ulProgressMax ) ) ;

            // check for timeout
            if (TimeoutMs != NO_TIMEOUT)
            {
                if (DateTime.Now > _timeoutTime)
                {
                    _timedOut = true;
                    return HRESULT.E_ABORT;
                }
            }

            if (ulStatusCode == BINDSTATUS.ENDDOWNLOADDATA)
                // We've completed the download of the file
                return HRESULT.S_OK;
            else if (ulStatusCode == BINDSTATUS.MIMETYPEAVAILABLE)
                // record the mime type
                m_contentType = szStatusText;
            else if (ulStatusCode == BINDSTATUS.REDIRECTING)
                _finalUrl = szStatusText;
            else if ((ProgressHost != null && ProgressHost.CancelRequested) || ThreadHelper.Interrupted)
                // Cancel this operation
                return HRESULT.E_ABORT;
            else
            {
                if (ProgressHost != null)
                    // UrlDownloadToFile can sometimes return progress that exceeds 100%- stop this from happening
                    ProgressHost.UpdateProgress((int)(Math.Min(ulProgressMax, ulProgress)), (int)ulProgressMax, String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.ProgressDownloading), Url));
            }

            return HRESULT.S_OK;
        }

        /// <summary>
        /// This method is always called, whether the bind operation succeeded, failed, or was aborted by a client.
        /// </summary>
        void IBindStatusCallback.OnStopBinding(int hresult, string szError)
        {
            LOG("IBindStatusCallback", "OnStopBinding");
        }

        /// <summary>
        /// Obtains the priority for the bind operation when called by an asynchronous moniker.
        /// </summary>
        void IBindStatusCallback.GetPriority(out int pnPriority)
        {
            LOG_UN("IBindStatusCallback", "GetPriority");

            // just in case....
            pnPriority = THREAD_PRIORITY.NORMAL;
        }

        /// <summary>
        /// This is not currently implemented and should never be called.
        /// </summary>
        void IBindStatusCallback.OnLowResource(uint reserved)
        {
            // not implemented
            LOG_UN("IBindStatusCallback", "OnLowResource");
        }

        /// <summary>
        /// Provides data to the client as it becomes available during asynchronous bind operations.
        /// </summary>
        void IBindStatusCallback.OnDataAvailable(BSCF grfBSCF, uint dwSize, ref FORMATETC pformatetc, ref STGMEDIUM pstgmed)
        {
            // never called by URLDownloadToFile
            LOG_UN("IBindStatusCallback", "OnDataAvailable");
        }

        /// <summary>
        /// Passes the requested object interface pointer to the client.
        /// </summary>
        void IBindStatusCallback.OnObjectAvailable(ref Guid riid, IntPtr punk)
        {
            // never called by URLDownloadToFile
            LOG_UN("IBindStatusCallback", "OnObjectAvailable");
        }

        #endregion

        #region IHttpNegotiate Members

        /// <summary>
        /// The URL moniker calls this method before sending an HTTP request.
        /// It notifies the client of the URL being bound to at the beginning
        /// of the HTTP transaction. It also allows the client to add
        /// additional headers, such as Accept-Language, to the request.
        /// </summary>
        public int BeginningTransaction(string szURL, string szHeaders, uint dwReserved, out IntPtr pszAdditionalHeaders)
        {
            LOG("IHttpNegotiate", "BeginningTransaction");

            string additionalHeaders = null;

            // Send form header
            if (DownloadAction == DownloadActions.POST)
            {
                additionalHeaders = additionalHeaders + "Content-Type: application/x-www-form-urlencoded\r\n";
            }

            if (additionalHeaders == null)
                pszAdditionalHeaders = IntPtr.Zero;
            else
                pszAdditionalHeaders = Marshal.StringToCoTaskMemUni(additionalHeaders);

            return HRESULT.S_OK;
        }

        /// <summary>
        /// The URL moniker calls this method when it receives a response to an
        /// HTTP request. If dwResponseCode indicates a success, the client can
        /// examine the response headers and can optionally abort the bind operation.
        /// If dwResponseCode indicates a failure, the client can add HTTP headers
        /// to the request before it is sent again.
        /// </summary>
        public int OnResponse(uint dwResponseCode, string szResponseHeaders, string szRequestHeaders, out IntPtr pszAdditionalRequestHeaders)
        {
            LOG("IHttpNegotiate", "OnResponse");
            m_responseHeaders = szResponseHeaders;

            // NOTE: use Marshal.StringToCoTaskMemUni if you want to add additional
            // headers, e.g. pszAdditionalHeaders = Marshal.StringToCoTakMemUni("headers")
            // They presumably free this memory -- the documentation doesn't specify
            pszAdditionalRequestHeaders = IntPtr.Zero;
            return HRESULT.S_OK;
        }

        #endregion

        #region IAuthenticate Members

        /// <summary>
        /// Supplies authentication support to a URL moniker from a client application.
        ///
        /// If we need to support automatically logging users in to download files, we
        /// should implement a response here (the operation should call this in order
        /// to get the authentication information to send with the request).  The
        /// current implementation for URLs appears to only support based authentication
        /// at this time.
        /// </summary>
        public int Authenticate(out IntPtr phwnd, out IntPtr pszUsername, out IntPtr pszPassword)
        {
            LOG("IAuthenticate", "Authenticate");

            if (_networkCredential != null)
            {
                phwnd = IntPtr.Zero; // no ui
                pszUsername = Marshal.StringToCoTaskMemUni(_networkCredential.UserName);
                pszPassword = Marshal.StringToCoTaskMemUni(_networkCredential.Password);
                return HRESULT.S_OK;
            }
            else
            {
                phwnd = IntPtr.Zero;
                pszUsername = IntPtr.Zero;
                pszPassword = IntPtr.Zero;
                return HRESULT.E_ACCESSDENIED;
            }
        }

        #endregion

        #region IWindowForBindingUI

        /// <summary>
        /// This interface allows clients of URL monikers to display information
        /// in the client's user interface when necessary.
        ///
        /// We always request silent, so this should never be called.
        /// </summary>
        [PreserveSig]
        int IWindowForBindingUI.GetWindow(ref Guid rguidReason, out IntPtr phwnd)
        {
            //This now happens for IE8 post-beta 2
            //LOG_UN( "IWindowForBindingUI", "GetWindow" ) ;
            phwnd = User32.GetDesktopWindow();
            return HRESULT.S_OK;
        }

        #endregion

        #region IHttpSecurity Members

        /// <summary>
        /// Undocumented, but likely used to get the client window so
        /// notification ui can be displayed.  We request silent operation,
        /// so this should never be called.
        /// </summary>
        int IHttpSecurity.GetWindow(ref Guid rguidReason, out IntPtr phwnd)
        {
            LOG("IHttpSecurity", "GetWindow");
            phwnd = User32.GetDesktopWindow();
            return HRESULT.S_OK;
        }

        /// <summary>
        /// Notifies the client application about an authentication problem.
        ///
        /// We are requesting to ignore these problems, so this should never get called.
        /// </summary>
        int IHttpSecurity.OnSecurityProblem(uint dwProblem)
        {
            LOG("IHttpSecurity", "OnSecurityProblem");
            if (ShowSecurityUI)
                return HRESULT.S_FALSE;
            else
                return HRESULT.E_ABORT;
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
            //Debug.WriteLine( String.Format( "{0}.{1}", iface, method ) ) ;
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

        #endregion

    }

    public class WinInetCredentialsContext
    {
        public WinInetCredentialsContext(NetworkCredential credential)
            : this(credential, null)
        {
        }

        public WinInetCredentialsContext(CookieString cookieString)
            : this(null, cookieString)
        {
        }

        public WinInetCredentialsContext(NetworkCredential credential, CookieString cookieString)
        {
            _networkCredential = credential;
            _cookieString = cookieString;
        }

        public NetworkCredential NetworkCredential
        {
            get { return _networkCredential; }
        }
        private NetworkCredential _networkCredential = null;

        public CookieString CookieString
        {
            get { return _cookieString; }
        }
        private CookieString _cookieString;
    }

    public class CookieString
    {
        public CookieString(string url, string cookies)
        {
            _url = url;
            _cookies = cookies;
        }
        public string Url
        {
            get { return _url; }
        }
        private string _url;

        public string Cookies
        {
            get { return _cookies; }
        }
        private string _cookies;
    }
}
