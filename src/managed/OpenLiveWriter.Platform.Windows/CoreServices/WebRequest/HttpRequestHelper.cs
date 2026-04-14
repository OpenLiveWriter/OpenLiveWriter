// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Globalization;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.CoreServices.HTML;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Delegate for augmenting an HTTP request.
    /// </summary>
    public delegate void HttpRequestFilter(HttpWebRequest request);

    /// <summary>
    /// Utility class for doing HTTP requests -- uses the Feeds Proxy settings (if any) for requests.
    /// 
    /// MIGRATION NOTE: New code should prefer HttpClientService for modern HttpClient-based requests.
    /// This class is maintained for backward compatibility with existing code that uses HttpWebRequest.
    /// The WebRequest APIs are deprecated in .NET 10 but still functional.
    /// </summary>
    public class HttpRequestHelper
    {
        private static bool _initialized;
        private static readonly object _initLock = new object();

        /// <summary>
        /// Ensures legacy WebRequest infrastructure is initialized.
        /// Called lazily on first use of WebRequest-based methods.
        /// </summary>
        private static void EnsureInitialized()
        {
            if (_initialized) return;
            lock (_initLock)
            {
                if (_initialized) return;

                // Configure legacy WebRequest settings for backward compatibility
                // SYSLIB0014: ServicePointManager is obsolete but needed for legacy WebRequest code
                #pragma warning disable SYSLIB0014
                // This is necessary to avoid problems connecting to Blogger server from behind a proxy.
                ServicePointManager.Expect100Continue = false;

                // Enable TLS 1.2 for modern HTTPS connections (required for most servers in 2024+)
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                #pragma warning restore SYSLIB0014

                try
                {
                    // Register WSSE authentication module for blog clients that require it
                    // (e.g., SixApart/TypePad). This only affects WebRequest-based code.
                    // SYSLIB0009: AuthenticationManager is obsolete but required for WSSE auth
                    #pragma warning disable SYSLIB0009
                    AuthenticationManager.Register(new WsseAuthenticationModule());
                    #pragma warning restore SYSLIB0009
                }
                catch (InvalidOperationException)
                {
                    // Registration may fail in some environments
                    Trace.WriteLine("Warning: WSSE authentication support disabled");
                }

                _initialized = true;
            }
        }

        public static void TrackResponseClosing(ref HttpWebRequest req)
        {
            CloseTrackingHttpWebRequest.Wrap(ref req);
        }
        
        /// <summary>
        /// Wraps an HttpWebResponse for close tracking in DEBUG builds.
        /// Returns a tracker that should be marked as closed when the response is disposed.
        /// </summary>
        public static ResponseCloseTracker TrackResponse(HttpWebResponse response)
        {
            return CloseTrackingHttpWebRequest.TrackResponse(response);
        }
        
        /// <summary>
        /// Gets a response stream that automatically tracks disposal in DEBUG builds.
        /// Disposing this stream marks the response as properly closed.
        /// </summary>
        public static Stream GetTrackedResponseStream(HttpWebResponse response)
        {
            return CloseTrackingHttpWebRequest.GetTrackedResponseStream(response);
        }

        #region HttpClient-based methods (Modern API)

        private static readonly Lazy<HttpClient> _httpClient = new Lazy<HttpClient>(CreateHttpClient);

        private static HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
                UseCookies = true,
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            };

            int timeoutMs = 60000; // Default 60 seconds

            // Configure proxy (safely, in case ApplicationEnvironment is not initialized)
            try
            {
                if (WebProxySettings.ProxyEnabled)
                {
                    string proxyServerUrl = WebProxySettings.Hostname;
                    if (proxyServerUrl.IndexOf("://", StringComparison.OrdinalIgnoreCase) == -1)
                        proxyServerUrl = "http://" + proxyServerUrl;
                    if (WebProxySettings.Port > 0)
                        proxyServerUrl += ":" + WebProxySettings.Port;

                    handler.Proxy = new WebProxy(proxyServerUrl, false);
                    if (!string.IsNullOrEmpty(WebProxySettings.Username))
                    {
                        handler.Proxy.Credentials = new NetworkCredential(WebProxySettings.Username, WebProxySettings.Password);
                    }
                    handler.UseProxy = true;
                }
                timeoutMs = WebProxySettings.HttpRequestTimeout;
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Unable to configure proxy settings for HttpClient: " + ex.Message);
            }

            var client = new HttpClient(handler);
            client.Timeout = TimeSpan.FromMilliseconds(timeoutMs);
            client.DefaultRequestHeaders.Accept.ParseAdd("*/*");

            // Set User-Agent (safely)
            try
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd(ApplicationEnvironment.UserAgent);
            }
            catch
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("OpenLiveWriter/1.0");
            }

            // Set Accept-Language
            string acceptLang = CultureInfo.CurrentUICulture.Name.Split('/')[0];
            if (acceptLang.ToUpperInvariant() == "SR-SP-LATN")
                acceptLang = "sr-Latn-CS";
            if (acceptLang != "en-US")
                acceptLang += ", en-US";
            acceptLang += ", en, *";
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd(acceptLang);

            return client;
        }

        /// <summary>
        /// Gets the shared HttpClient instance configured with standard Open Live Writer settings.
        /// Use this for modern HTTP requests instead of CreateHttpWebRequest.
        /// </summary>
        public static HttpClient HttpClient => _httpClient.Value;

        /// <summary>
        /// Sends a GET request and returns the response (HttpClient-based).
        /// </summary>
        public static HttpResponseMessage SendRequestAsync(string requestUri)
        {
            return HttpClient.GetAsync(requestUri).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Sends a request with the specified method and content (HttpClient-based).
        /// </summary>
        public static HttpResponseMessage SendRequestAsync(HttpMethod method, string requestUri, HttpContent content = null, Action<HttpRequestMessage> configureRequest = null)
        {
            using var request = new HttpRequestMessage(method, requestUri);
            if (content != null)
                request.Content = content;
            configureRequest?.Invoke(request);
            return HttpClient.SendAsync(request).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Downloads content from a URL and returns it as a stream (HttpClient-based).
        /// </summary>
        public static Stream DownloadStream(string url, out string responseUri)
        {
            responseUri = null;
            try
            {
                var response = HttpClient.GetAsync(url).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    responseUri = response.RequestMessage?.RequestUri?.AbsoluteUri ?? url;
                    return response.Content.ReadAsStream();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Unable to download \"{url}\": {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// Checks if a URL is reachable (HttpClient-based HEAD request).
        /// </summary>
        public static bool CheckUrlReachable(string url, int? timeoutMs = null)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Head, url);
                using var cts = timeoutMs.HasValue
                    ? new System.Threading.CancellationTokenSource(timeoutMs.Value)
                    : new System.Threading.CancellationTokenSource();
                var response = HttpClient.SendAsync(request, cts.Token).GetAwaiter().GetResult();
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a response, returning the response message with access to the final URI after redirects.
        /// Caller is responsible for disposing the response.
        /// </summary>
        public static HttpResponseMessage GetResponse(string url)
        {
            var response = HttpClient.GetAsync(url).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            return response;
        }

        /// <summary>
        /// Gets a response as a stream with the final response URI.
        /// </summary>
        public static Stream GetResponseStream(string url, out Uri responseUri)
        {
            var response = GetResponse(url);
            responseUri = response.RequestMessage?.RequestUri ?? new Uri(url);
            return response.Content.ReadAsStream();
        }

        /// <summary>
        /// Posts form data to a URL and returns the response.
        /// Caller is responsible for disposing the response.
        /// </summary>
        public static HttpResponseMessage PostForm(string url, byte[] formData, string contentType = "application/x-www-form-urlencoded")
        {
            using var content = new ByteArrayContent(formData);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            var response = HttpClient.PostAsync(url, content).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            return response;
        }

        /// <summary>
        /// Posts form data to a URL and returns the response stream with the final response URI.
        /// </summary>
        public static Stream PostFormStream(string url, byte[] formData, out Uri responseUri, string contentType = "application/x-www-form-urlencoded")
        {
            using var response = PostForm(url, formData, contentType);
            responseUri = response.RequestMessage?.RequestUri ?? new Uri(url);
            var stream = new MemoryStream();
            response.Content.ReadAsStream().CopyTo(stream);
            stream.Position = 0;
            return stream;
        }

        #endregion

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
            Debug.WriteLine($"[OLW-DEBUG] HttpRequestHelper.SendRequest() - requestUri: {requestUri}");
            #pragma warning disable SYSLIB0014 // ServicePointManager is obsolete but used for debug logging
            Debug.WriteLine($"[OLW-DEBUG] HttpRequestHelper.SendRequest() - SecurityProtocol: {ServicePointManager.SecurityProtocol}");
            #pragma warning restore SYSLIB0014
            
            HttpWebRequest request = CreateHttpWebRequest(requestUri, true, null, null);
            Debug.WriteLine($"[OLW-DEBUG] HttpRequestHelper.SendRequest() - Request created, Method: {request.Method}");
            
            if (filter != null)
                filter(request);

            // get the response
            try
            {
                Debug.WriteLine("[OLW-DEBUG] HttpRequestHelper.SendRequest() - Calling GetResponse()");
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Debug.WriteLine($"[OLW-DEBUG] HttpRequestHelper.SendRequest() - Got response, StatusCode: {response.StatusCode}");
                
                //hack: For some reason, disabling auto-redirects also disables throwing WebExceptions for 300 status codes,
                //so if we detect a non-2xx error code here, throw a web exception.
                int statusCode = (int)response.StatusCode;
                if (statusCode > 299)
                    throw new WebException(response.StatusCode.ToString() + ": " + response.StatusDescription, null, WebExceptionStatus.UnknownError, response);
                return response;
            }
            catch (WebException e)
            {
                Debug.WriteLine($"[OLW-DEBUG] HttpRequestHelper.SendRequest() - WebException: {e.Status} - {e.Message}");
                if (e.InnerException != null)
                {
                    Debug.WriteLine($"[OLW-DEBUG] HttpRequestHelper.SendRequest() - InnerException: {e.InnerException.GetType().Name} - {e.InnerException.Message}");
                }
                if (e.Response != null)
                {
                    var httpResp = e.Response as HttpWebResponse;
                    if (httpResp != null)
                    {
                        Debug.WriteLine($"[OLW-DEBUG] HttpRequestHelper.SendRequest() - Response StatusCode: {httpResp.StatusCode}");
                    }
                }
                
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
            // Ensure WSSE auth is registered for legacy blog client support
            EnsureInitialized();

            #pragma warning disable SYSLIB0014 // WebRequest is obsolete - maintained for backward compatibility
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestUri);
            #pragma warning restore SYSLIB0014
            TrackResponseClosing(ref request);

            // Set Accept to */* to stop Bad Behavior plugin for WordPress from
            // thinking we're a spam cannon
            request.Accept = "*/*";
            ApplyLanguage(request);

            // Temporary fix for Blogger photos issue
            // Remove after March 15, 2019 as it will no longer be effective
            if (request.RequestUri.Host.Contains("picasaweb.google.com"))
            {
                request.Headers["deprecation-extension"] = "true";
            }

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

            //For robustness, we turn off keep alive and pipelining by default.
            //If the caller wants to override, the filter parameter can be used to adjust these settings.
            //Warning: NTLM authentication requires keep-alive, so without adjusting this, NTLM-secured requests will always fail.
            request.KeepAlive = false;
            request.Pipelined = false;
            // Bypass cache entirely - some blogs, specifically static blogs on GH pages, have very aggressive caching policies
            request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
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

        #region Legacy Filter Adapter

        /// <summary>
        /// Applies a legacy HttpRequestFilter to an HttpRequestMessage by creating a temporary
        /// HttpWebRequest, running the filter, and extracting the headers that were set.
        /// This allows legacy code to work with HttpClient while maintaining compatibility.
        /// </summary>
        /// <param name="request">The HttpRequestMessage to configure</param>
        /// <param name="filter">The legacy filter to apply</param>
        /// <param name="tempUri">A URI to use for creating the temporary HttpWebRequest</param>
        public static void ApplyLegacyFilter(HttpRequestMessage request, HttpRequestFilter filter, string tempUri)
        {
            if (filter == null) return;

            // Create a temporary HttpWebRequest to capture filter settings
            #pragma warning disable SYSLIB0014 // WebRequest is obsolete - used only to capture filter settings
            var tempRequest = (HttpWebRequest)WebRequest.Create(tempUri);
            #pragma warning restore SYSLIB0014

            // Apply the filter
            filter(tempRequest);

            // Extract and apply headers
            foreach (string key in tempRequest.Headers.AllKeys)
            {
                // Skip headers that are set via other properties
                if (key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)) continue;
                if (key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase)) continue;
                if (key.Equals("User-Agent", StringComparison.OrdinalIgnoreCase))
                {
                    request.Headers.UserAgent.Clear();
                    request.Headers.TryAddWithoutValidation("User-Agent", tempRequest.Headers[key]);
                    continue;
                }
                if (key.Equals("Accept", StringComparison.OrdinalIgnoreCase))
                {
                    request.Headers.Accept.Clear();
                    request.Headers.TryAddWithoutValidation("Accept", tempRequest.Headers[key]);
                    continue;
                }

                request.Headers.TryAddWithoutValidation(key, tempRequest.Headers[key]);
            }

            // Apply credentials if set (via Authorization header typically)
            if (tempRequest.Credentials != null)
            {
                // For NetworkCredential, we can set basic auth
                if (tempRequest.Credentials is NetworkCredential nc && !string.IsNullOrEmpty(nc.UserName))
                {
                    var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{nc.UserName}:{nc.Password}"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authValue);
                }
            }

            // Apply content type if set
            if (!string.IsNullOrEmpty(tempRequest.ContentType) && request.Content != null)
            {
                try
                {
                    request.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(tempRequest.ContentType);
                }
                catch
                {
                    // Ignore parse errors
                }
            }
        }

        /// <summary>
        /// Creates an Action&lt;HttpRequestMessage&gt; that applies a legacy HttpRequestFilter.
        /// </summary>
        public static Action<HttpRequestMessage> CreateLegacyFilterAdapter(HttpRequestFilter filter, string uri)
        {
            if (filter == null) return null;
            return request => ApplyLegacyFilter(request, filter, uri);
        }

        #endregion
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
