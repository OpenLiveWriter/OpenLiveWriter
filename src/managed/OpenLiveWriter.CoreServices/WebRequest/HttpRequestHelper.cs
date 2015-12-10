// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Globalization;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.CoreServices.HTML;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Delegate for augmenting and HTTP request.
    /// </summary>
    public delegate void HttpRequestFilter(HttpWebRequest request);

    /// <summary>
    /// Utility class for doing HTTP requests -- uses the Feeds Proxy settings (if any) for requests
    /// </summary>
    public class HttpRequestHelper
    {
        static HttpRequestHelper()
        {
            // This is necessary to avoid problems connecting to Blogger server from behind a proxy.
            ServicePointManager.Expect100Continue = false;

            try
            {
                // Add WSSE support everywhere.
                AuthenticationManager.Register(new WsseAuthenticationModule());
            }
            catch (InvalidOperationException)
            {
                // See http://blogs.msdn.com/shawnfa/archive/2005/05/16/417975.aspx
                Trace.WriteLine("Warning: WSSE support disabled");
            }

            if (ApplicationDiagnostics.AllowUnsafeCertificates)
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);

            }
        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                Trace.WriteLine("SSL Policy error " + sslPolicyErrors);
            }

            return true;
        }

        public static void TrackResponseClosing(ref HttpWebRequest req)
        {
            CloseTrackingHttpWebRequest.Wrap(ref req);
        }

        /// <summary>
        /// Download a file and return a path to it -- returns null if the file
        /// could not be found or any other error occurs
        /// </summary>
        /// <param name="fileUrl">file url</param>
        /// <returns>path to file or null if it could not be downloaded</returns>
        public static Stream SafeDownloadFile(string fileUrl)
        {
            string responseUri;
            return SafeDownloadFile(fileUrl, out responseUri, null);
        }

        public static Stream SafeDownloadFile(string fileUrl, out string responseUri)
        {
            return SafeDownloadFile(fileUrl, out responseUri, null);
        }

        public static Stream SafeDownloadFile(string fileUrl, out string responseUri, HttpRequestFilter filter)
        {
            responseUri = null;
            try
            {
                HttpWebResponse response = SafeSendRequest(fileUrl, filter);

                if (response != null)
                {
                    responseUri = UrlHelper.SafeToAbsoluteUri(response.ResponseUri);
                    return response.GetResponseStream();
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Unable to download file \"" + fileUrl + "\" during Blog service detection: " + ex.ToString());
                return null;
            }
        }

        public static HttpWebResponse SendRequest(string requestUri)
        {
            return SendRequest(requestUri, null);
        }

        public static HttpWebResponse SendRequest(string requestUri, HttpRequestFilter filter)
        {
            HttpWebRequest request = CreateHttpWebRequest(requestUri, true, null, null);
            if (filter != null)
                filter(request);

            // get the response
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //hack: For some reason, disabling auto-redirects also disables throwing WebExceptions for 300 status codes,
                //so if we detect a non-2xx error code here, throw a web exception.
                int statusCode = (int)response.StatusCode;
                if (statusCode > 299)
                    throw new WebException(response.StatusCode.ToString() + ": " + response.StatusDescription, null, WebExceptionStatus.UnknownError, response);
                return response;
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.Timeout)
                {
                    //throw a typed exception that lets callers know that the response timed out after the request was sent
                    throw new WebResponseTimeoutException(e);
                }
                else
                    throw;
            }
        }

        public static void ApplyLanguage(HttpWebRequest request)
        {
            string acceptLang = CultureInfo.CurrentUICulture.Name.Split('/')[0];
            if (acceptLang.ToUpperInvariant() == "SR-SP-LATN")
                acceptLang = "sr-Latn-CS";
            if (acceptLang != "en-US")
                acceptLang += ", en-US";
            acceptLang += ", en, *";
            request.Headers["Accept-Language"] = acceptLang;
        }

        public static HttpWebResponse SafeSendRequest(string requestUri, HttpRequestFilter filter)
        {
            try
            {
                return SendRequest(requestUri, filter);
            }
            catch (WebException we)
            {
                if (ApplicationDiagnostics.TestMode)
                    LogException(we);
                return null;
            }
        }

        public static void ApplyProxyOverride(WebRequest request)
        {
            WebProxy proxy = GetProxyOverride();
            if (proxy != null)
                request.Proxy = proxy;
        }

        /// <summary>
        /// Returns the default proxy for an HTTP request.
        ///
        /// Consider using ApplyProxyOverride instead.
        /// </summary>
        /// <returns></returns>
        public static WebProxy GetProxyOverride()
        {
            WebProxy proxy = null;
            if (WebProxySettings.ProxyEnabled)
            {
                string proxyServerUrl = WebProxySettings.Hostname;
                if (proxyServerUrl.IndexOf("://", StringComparison.OrdinalIgnoreCase) == -1)
                    proxyServerUrl = "http://" + proxyServerUrl;
                if (WebProxySettings.Port > 0)
                    proxyServerUrl += ":" + WebProxySettings.Port;

                ICredentials proxyCredentials = CreateHttpCredentials(WebProxySettings.Username, WebProxySettings.Password, proxyServerUrl);
                proxy = new WebProxy(proxyServerUrl, false, new string[0], proxyCredentials);
            }
            return proxy;
        }

        public static ICredentials CreateHttpCredentials(string username, string password, string url)
        {
            return CreateHttpCredentials(username, password, url, false);
        }

        /// <summary>
        /// Creates a set of credentials for the specified user/pass, or returns the default credentials if user/pass is null.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static ICredentials CreateHttpCredentials(string username, string password, string url, bool digestOnly)
        {
            ICredentials credentials = CredentialCache.DefaultCredentials;
            if (username != null || password != null)
            {
                CredentialCache credentialCache = new CredentialCache();
                string userDomain = String.Empty;

                if (username != null)
                {
                    //try to parse the username string into a domain\userId
                    int domainIndex = username.IndexOf(@"\", StringComparison.OrdinalIgnoreCase);
                    if (domainIndex != -1)
                    {
                        userDomain = username.Substring(0, domainIndex);
                        username = username.Substring(domainIndex + 1);
                    }
                }

                credentialCache.Add(new Uri(url), "Digest", new NetworkCredential(username, password, userDomain));

                if (!digestOnly)
                {
                    credentialCache.Add(new Uri(url), "Basic", new NetworkCredential(username, password, userDomain));
                    credentialCache.Add(new Uri(url), "NTLM", new NetworkCredential(username, password, userDomain));
                    credentialCache.Add(new Uri(url), "Negotiate", new NetworkCredential(username, password, userDomain));
                    credentialCache.Add(new Uri(url), "Kerberos", new NetworkCredential(username, password, userDomain));
                }
                credentials = credentialCache;
            }
            return credentials;
        }

        public static HttpWebRequest CreateHttpWebRequest(string requestUri, bool allowAutoRedirect)
        {
            return CreateHttpWebRequest(requestUri, allowAutoRedirect, null, null);
        }

        public static HttpWebRequest CreateHttpWebRequest(string requestUri, bool allowAutoRedirect, int? connectTimeoutMs, int? readWriteTimeoutMs)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestUri);
            TrackResponseClosing(ref request);

            // Set Accept to */* to stop Bad Behavior plugin for WordPress from
            // thinking we're a spam cannon
            request.Accept = "*/*";
            ApplyLanguage(request);

            int timeout = WebProxySettings.HttpRequestTimeout;
            request.Timeout = timeout;
            request.ReadWriteTimeout = timeout * 5;

            if (connectTimeoutMs != null)
                request.Timeout = connectTimeoutMs.Value;
            if (readWriteTimeoutMs != null)
                request.ReadWriteTimeout = readWriteTimeoutMs.Value;

            request.AllowAutoRedirect = allowAutoRedirect;
            request.UserAgent = ApplicationEnvironment.UserAgent;

            ApplyProxyOverride(request);

            //For robustness, we turn off keep alive and piplining by default.
            //If the caller wants to override, the filter parameter can be used to adjust these settings.
            //Warning: NTLM authentication requires keep-alive, so without adjusting this, NTLM-secured requests will always fail.
            request.KeepAlive = false;
            request.Pipelined = false;
            request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.Reload);
            return request;
        }

        public static string DumpResponse(HttpWebResponse resp)
        {
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture))
            {
                sw.WriteLine(String.Format(CultureInfo.InvariantCulture, "{0}/{1} {2} {3}", "HTTP", resp.ProtocolVersion, (int)resp.StatusCode, resp.StatusDescription));
                foreach (string key in resp.Headers.AllKeys)
                {
                    sw.WriteLine(String.Format(CultureInfo.InvariantCulture, "{0}: {1}", key, resp.Headers[key]));
                }
                sw.WriteLine("");
                sw.WriteLine(DecodeBody(resp));
            }
            return sb.ToString();
        }

        public static string DumpRequestHeader(HttpWebRequest req)
        {
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture))
            {
                sw.WriteLine(String.Format(CultureInfo.InvariantCulture, "{0} {1} HTTP/{2}", req.Method, UrlHelper.SafeToAbsoluteUri(req.RequestUri), req.ProtocolVersion));
                foreach (string key in req.Headers.AllKeys)
                {
                    sw.WriteLine(String.Format(CultureInfo.InvariantCulture, "{0}: {1}", key, req.Headers[key]));
                }
            }
            return sb.ToString();
        }

        public static DateTime GetExpiresHeader(HttpWebResponse response)
        {
            string expires = response.GetResponseHeader("Expires");
            if (expires != null && expires != String.Empty && expires.Trim() != "-1")
            {
                try
                {
                    DateTime expiresDate = DateTime.Parse(expires, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                    return expiresDate;
                }
                catch (Exception ex)
                {
                    // look for ANSI c's asctime() format as a last gasp
                    try
                    {
                        string asctimeFormat = "ddd' 'MMM' 'd' 'HH':'mm':'ss' 'yyyy";
                        DateTime expiresDate = DateTime.ParseExact(expires, asctimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
                        return expiresDate;
                    }
                    catch
                    {
                    }

                    Trace.Fail("Exception parsing HTTP date - " + expires + ": " + ex.ToString());
                    return DateTime.MinValue;
                }
            }
            else
            {
                return DateTime.MinValue;
            }
        }

        public static string GetETagHeader(HttpWebResponse response)
        {
            return GetStringHeader(response, "ETag");
        }

        public static string GetStringHeader(HttpWebResponse response, string headerName)
        {
            string headerValue = response.GetResponseHeader(headerName);
            if (headerValue != null)
                return headerValue;
            else
                return String.Empty;
        }

        public static void LogException(WebException ex)
        {
            Trace.WriteLine("== BEGIN WebException =====================");
            Trace.WriteLine("Status: " + ex.Status);
            Trace.WriteLine(ex.ToString());
            HttpWebResponse response = ex.Response as HttpWebResponse;
            if (response != null)
                Trace.WriteLine(DumpResponse(response));
            Trace.WriteLine("== END WebException =======================");
        }

        public static string GetFriendlyErrorMessage(WebException we)
        {
            if (we.Response != null && we.Response is HttpWebResponse)
            {
                HttpWebResponse response = (HttpWebResponse)we.Response;
                string bodyText = GetBodyText(response);
                int statusCode = (int)response.StatusCode;
                string statusDesc = response.StatusDescription;

                return String.Format(CultureInfo.CurrentCulture,
                    "{0} {1}\r\n\r\n{2}",
                    statusCode, statusDesc,
                    bodyText);
            }
            else
            {
                return we.Message;
            }
        }

        private static string GetBodyText(HttpWebResponse resp)
        {
            if (resp.ContentType != null && resp.ContentType.Length > 0)
            {
                IDictionary contentTypeData = MimeHelper.ParseContentType(resp.ContentType, true);
                string mainType = (string)contentTypeData[""];
                switch (mainType)
                {
                    case "text/plain":
                        {
                            return DecodeBody(resp);
                        }
                    case "text/html":
                        {
                            return StringHelper.CompressExcessWhitespace(
                                HTMLDocumentHelper.HTMLToPlainText(
                                LightWeightHTMLThinner2.Thin(
                                DecodeBody(resp), true)));
                        }
                }
            }
            return "";
        }

        private static string DecodeBody(HttpWebResponse response)
        {
            Stream s = response.GetResponseStream();
            StreamReader sr = new StreamReader(s);
            return sr.ReadToEnd();
        }
    }

    public class HttpRequestCredentialsFilter
    {
        public static HttpRequestFilter Create(string username, string password, string url, bool digestOnly)
        {
            return new HttpRequestFilter(new HttpRequestCredentialsFilter(username, password, url, digestOnly).Filter);
        }

        private HttpRequestCredentialsFilter(string username, string password, string url, bool digestOnly)
        {
            _username = username;
            _password = password;
            _url = url;
            _digestOnly = digestOnly;
        }

        private void Filter(HttpWebRequest request)
        {
            request.Credentials = HttpRequestHelper.CreateHttpCredentials(_username, _password, _url, _digestOnly);
        }

        private string _username;
        private string _password;
        private string _url;
        private bool _digestOnly;
    }

    /// <summary>
    /// Allow chaining together of http request filters
    /// </summary>
    public class CompoundHttpRequestFilter
    {
        public static HttpRequestFilter Create(HttpRequestFilter[] filters)
        {
            return new HttpRequestFilter(new CompoundHttpRequestFilter(filters).Filter);
        }

        private CompoundHttpRequestFilter(HttpRequestFilter[] filters)
        {
            _filters = filters;
        }

        private void Filter(HttpWebRequest request)
        {
            foreach (HttpRequestFilter filter in _filters)
                filter(request);
        }

        private HttpRequestFilter[] _filters;
    }

    /// <summary>
    /// Typed-exception that occurs when an HTTP request times out after the request has been sent, but
    /// before the response is received.
    /// </summary>
    public class WebResponseTimeoutException : WebException
    {
        public WebResponseTimeoutException(WebException innerException) : base(innerException.Message, innerException, innerException.Status, innerException.Response)
        {

        }
    }
}
