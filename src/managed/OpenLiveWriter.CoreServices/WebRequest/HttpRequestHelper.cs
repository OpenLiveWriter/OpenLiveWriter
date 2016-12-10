// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Cache;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using JetBrains.Annotations;
    using OpenLiveWriter.CoreServices.Diagnostics;
    using OpenLiveWriter.CoreServices.HTML;

    /// <summary>
    /// Utility class for doing HTTP requests -- uses the Feeds Proxy settings (if any) for requests
    /// </summary>
    public static class HttpRequestHelper
    {
        /// <summary>
        /// Initializes static members of the <see cref="HttpRequestHelper"/> class.
        /// </summary>
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
                ServicePointManager.ServerCertificateValidationCallback = HttpRequestHelper.CheckValidationResult;
            }
        }

        /// <summary>
        /// Applies the language.
        /// </summary>
        /// <param name="request">The request.</param>
        public static void ApplyLanguage([NotNull] HttpWebRequest request)
        {
            var acceptLang = CultureInfo.CurrentUICulture.Name.Split('/')[0];
            if (acceptLang.ToUpperInvariant() == @"SR-SP-LATN")
            {
                acceptLang = "sr-Latn-CS"; // Not L10N
            }

            if (acceptLang != @"en-US")
            {
                acceptLang += ", en-US"; // Not L10N
            }

            acceptLang += ", en, *"; // Not L10N
            request.Headers["Accept-Language"] = acceptLang; // Not L10N
        }

        /// <summary>
        /// Applies the proxy override.
        /// </summary>
        /// <param name="request">The request.</param>
        public static void ApplyProxyOverride([NotNull] WebRequest request)
        {
            var proxy = HttpRequestHelper.GetProxyOverride();
            if (proxy != null)
            {
                request.Proxy = proxy;
            }
        }

        /// <summary>
        /// Creates the HTTP credentials.
        /// </summary>
        /// <param name="username">The user name.</param>
        /// <param name="password">The password.</param>
        /// <param name="url">The URL.</param>
        /// <returns>The ICredentials.</returns>
        [NotNull]
        public static ICredentials CreateHttpCredentials(
                [CanBeNull] string username,
                [CanBeNull] string password,
                [NotNull] string url)
            => HttpRequestHelper.CreateHttpCredentials(username, password, url, false);

        /// <summary>
        /// Creates a set of credentials for the specified user/pass, or returns the default credentials if user/pass is null.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="url">The URL.</param>
        /// <param name="digestOnly">if set to <c>true</c> [digest only].</param>
        /// <returns>The ICredentials.</returns>
        [NotNull]
        public static ICredentials CreateHttpCredentials(
            [CanBeNull] string username,
            [CanBeNull] string password,
            [NotNull] string url,
            bool digestOnly)
        {
            var credentials = CredentialCache.DefaultCredentials;
            if (username == null && password == null)
            {
                return credentials;
            }

            var credentialCache = new CredentialCache();
            var userDomain = string.Empty;

            if (username != null)
            {
                // try to parse the username string into a domain\userId
                var domainIndex = username.IndexOf(@"\", StringComparison.OrdinalIgnoreCase);
                if (domainIndex != -1)
                {
                    userDomain = username.Substring(0, domainIndex);
                    username = username.Substring(domainIndex + 1);
                }
            }

            credentialCache.Add(new Uri(url), "Digest", new NetworkCredential(username, password, userDomain)); // Not L10N

            if (!digestOnly)
            {
                credentialCache.Add(new Uri(url), "Basic", new NetworkCredential(username, password, userDomain)); // Not L10N
                credentialCache.Add(new Uri(url), "NTLM", new NetworkCredential(username, password, userDomain)); // Not L10N
                credentialCache.Add(new Uri(url), "Negotiate", new NetworkCredential(username, password, userDomain)); // Not L10N
                credentialCache.Add(new Uri(url), "Kerberos", new NetworkCredential(username, password, userDomain)); // Not L10N
            }

            credentials = credentialCache;

            return credentials;
        }

        /// <summary>
        /// Creates the HTTP web request.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="allowAutoRedirect">if set to <c>true</c> [allow automatic redirect].</param>
        /// <param name="connectTimeoutMs">The connect timeout milliseconds.</param>
        /// <param name="readWriteTimeoutMs">The read write timeout milliseconds.</param>
        /// <returns>The HttpWebRequest.</returns>
        [NotNull]
        public static HttpWebRequest CreateHttpWebRequest(
            [NotNull] string requestUri, bool allowAutoRedirect, int? connectTimeoutMs = null, int? readWriteTimeoutMs = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(requestUri);
            HttpRequestHelper.TrackResponseClosing(ref request);

            // Set Accept to */* to stop Bad Behavior plugin for WordPress from
            // thinking we're a spam cannon
            request.Accept = "*/*"; // Not L10N
            HttpRequestHelper.ApplyLanguage(request);

            var timeout = WebProxySettings.HttpRequestTimeout;
            request.Timeout = timeout;
            request.ReadWriteTimeout = timeout * 5;

            if (connectTimeoutMs != null)
            {
                request.Timeout = connectTimeoutMs.Value;
            }

            if (readWriteTimeoutMs != null)
            {
                request.ReadWriteTimeout = readWriteTimeoutMs.Value;
            }

            request.AllowAutoRedirect = allowAutoRedirect;
            request.UserAgent = ApplicationEnvironment.UserAgent;

            HttpRequestHelper.ApplyProxyOverride(request);

            // For robustness, we turn off keep alive and pipelining by default.
            // If the caller wants to override, the filter parameter can be used to adjust these settings.
            // Warning: NTLM authentication requires keep-alive, so without adjusting this, NTLM-secured requests will always fail.
            request.KeepAlive = false;
            request.Pipelined = false;
            request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.Reload);
            return request;
        }

        /// <summary>
        /// Dumps the request header.
        /// </summary>
        /// <param name="req">The request.</param>
        /// <returns>The request header.</returns>
        [NotNull]
        public static string DumpRequestHeader([NotNull] HttpWebRequest req)
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb, CultureInfo.InvariantCulture))
            {
                sw.WriteLine($"{req.Method} {UrlHelper.SafeToAbsoluteUri(req.RequestUri)} HTTP/{req.ProtocolVersion}");
                foreach (var key in req.Headers.AllKeys)
                {
                    sw.WriteLine($"{key}: {req.Headers[key]}");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Dumps the response.
        /// </summary>
        /// <param name="resp">The HTTP response.</param>
        /// <returns>The response string.</returns>
        [NotNull]
        public static string DumpResponse([NotNull] HttpWebResponse resp)
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb, CultureInfo.InvariantCulture))
            {
                sw.WriteLine($"HTTP/{resp.ProtocolVersion} {(int)resp.StatusCode} {resp.StatusDescription}");
                foreach (var key in resp.Headers.AllKeys)
                {
                    sw.WriteLine($"{key}: {resp.Headers[key]}");
                }

                sw.WriteLine(string.Empty);
                sw.WriteLine(HttpRequestHelper.DecodeBody(resp));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets the E-tag header.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>The E-tag header.</returns>
        [NotNull]
        public static string GetETagHeader([NotNull] HttpWebResponse response)
            => HttpRequestHelper.GetStringHeader(response, "ETag"); // Not L10N

        /// <summary>
        /// Gets the expires header.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>The expiration DateTime.</returns>
        public static DateTime GetExpiresHeader([NotNull] HttpWebResponse response)
        {
            var expires = response.GetResponseHeader("Expires"); // Not L10N
            if (string.IsNullOrWhiteSpace(expires) || expires.Trim() == @"-1")
            {
                return DateTime.MinValue;
            }

            DateTime expiresDate;
            if (DateTime.TryParse(
                expires,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal,
                out expiresDate))
            {
                return expiresDate;
            }

            // look for ANSI c's asctime() format as a last gasp
            const string AsctimeFormat = "ddd' 'MMM' 'd' 'HH':'mm':'ss' 'yyyy"; // Not L10N
            if (DateTime.TryParseExact(
                expires,
                AsctimeFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces,
                out expiresDate))
            {
                return expiresDate;
            }

            Trace.Fail($"Exception parsing HTTP date - {expires}");
            return DateTime.MinValue;
        }

        /// <summary>
        /// Gets the friendly error message.
        /// </summary>
        /// <param name="we">The web exception.</param>
        /// <returns>The friendly error message.</returns>
        [NotNull]
        public static string GetFriendlyErrorMessage([NotNull] WebException we)
        {
            var httpWebResponse = we.Response as HttpWebResponse;
            if (httpWebResponse == null)
            {
                return we.Message;
            }

            var response = httpWebResponse;
            var bodyText = HttpRequestHelper.GetBodyText(response);
            var statusCode = (int)response.StatusCode;
            var statusDesc = response.StatusDescription;

            return $"{statusCode} {statusDesc}\r\n\r\n{bodyText}";
        }

        /// <summary>
        /// Returns the default proxy for an HTTP request.
        /// Consider using ApplyProxyOverride instead.
        /// </summary>
        /// <returns>The default proxy for an HTTP request.</returns>
        [CanBeNull]
        public static WebProxy GetProxyOverride()
        {
            if (!WebProxySettings.ProxyEnabled)
            {
                return null;
            }

            var proxyServerUrl = WebProxySettings.Hostname;
            if (proxyServerUrl.IndexOf(@"://", StringComparison.OrdinalIgnoreCase) == -1)
            {
                proxyServerUrl = $"http://{proxyServerUrl}";
            }

            if (WebProxySettings.Port > 0)
            {
                proxyServerUrl += $":{WebProxySettings.Port}";
            }

            var proxyCredentials = HttpRequestHelper.CreateHttpCredentials(
                WebProxySettings.Username,
                WebProxySettings.Password,
                proxyServerUrl);
            var proxy = new WebProxy(proxyServerUrl, false, new string[0], proxyCredentials);

            return proxy;
        }

        /// <summary>
        /// Gets the string header.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <param name="headerName">Name of the header.</param>
        /// <returns>The string header.</returns>
        [NotNull]
        public static string GetStringHeader([NotNull] HttpWebResponse response, [NotNull] string headerName)
        {
            var headerValue = response.GetResponseHeader(headerName);
            return headerValue ?? string.Empty;
        }

        /// <summary>
        /// Logs the exception.
        /// </summary>
        /// <param name="ex">The exception.</param>
        public static void LogException([NotNull] WebException ex)
        {
            Trace.WriteLine("== BEGIN WebException =====================");
            Trace.WriteLine($"Status: {ex.Status}");
            Trace.WriteLine(ex.ToString());
            var response = ex.Response as HttpWebResponse;
            if (response != null)
            {
                Trace.WriteLine(HttpRequestHelper.DumpResponse(response));
            }

            Trace.WriteLine("== END WebException =======================");
        }

        /// <summary>
        /// Download a file and return a path to it -- returns null if the file
        /// could not be found or any other error occurs
        /// </summary>
        /// <param name="fileUrl">file url</param>
        /// <returns>path to file or null if it could not be downloaded</returns>
        [CanBeNull]
        public static Stream SafeDownloadFile([NotNull] string fileUrl)
        {
            string responseUri;
            return HttpRequestHelper.SafeDownloadFile(fileUrl, out responseUri);
        }

        /// <summary>
        /// Safes the download file.
        /// </summary>
        /// <param name="fileUrl">The file URL.</param>
        /// <param name="responseUri">The response URI.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>A Stream.</returns>
        [CanBeNull]
        public static Stream SafeDownloadFile(
            [NotNull] string fileUrl,
            [CanBeNull] out string responseUri,
            [CanBeNull] HttpRequestFilter filter = null)
        {
            responseUri = null;
            try
            {
                var response = HttpRequestHelper.SafeSendRequest(fileUrl, filter);
                if (response == null)
                {
                    return null;
                }

                responseUri = UrlHelper.SafeToAbsoluteUri(response.ResponseUri);
                return response.GetResponseStream();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Unable to download file \"{fileUrl}\" during Blog service detection: {ex}");
                return null;
            }
        }

        /// <summary>
        /// Safes the send request.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>The HttpWebResponse.</returns>
        [CanBeNull]
        public static HttpWebResponse SafeSendRequest([NotNull] string requestUri, [CanBeNull] HttpRequestFilter filter)
        {
            try
            {
                return HttpRequestHelper.SendRequest(requestUri, filter);
            }
            catch (WebException we)
            {
                if (ApplicationDiagnostics.TestMode)
                {
                    HttpRequestHelper.LogException(we);
                }

                return null;
            }
        }

        /// <summary>
        /// Sends the request.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>The HttpWebResponse.</returns>
        /// <exception cref="System.Net.WebException">An unknown error occurred.</exception>
        /// <exception cref="OpenLiveWriter.CoreServices.WebResponseTimeoutException">The request timed out.</exception>
        [NotNull]
        public static HttpWebResponse SendRequest([NotNull] string requestUri, [CanBeNull] HttpRequestFilter filter = null)
        {
            var request = HttpRequestHelper.CreateHttpWebRequest(requestUri, true);
            filter?.Invoke(request);

            // get the response
            try
            {
                var response = (HttpWebResponse)request.GetResponse();

                // hack: For some reason, disabling auto-redirects also disables throwing WebExceptions for 300 status codes,
                // so if we detect a non-2xx error code here, throw a web exception.
                var statusCode = (int)response.StatusCode;
                if (statusCode > 299)
                {
                    throw new WebException(
                              $"{response.StatusCode}: {response.StatusDescription}",
                              null,
                              WebExceptionStatus.UnknownError,
                              response);
                }

                return response;
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.Timeout)
                {
                    // throw a typed exception that lets callers know that the response timed out after the request was sent
                    throw new WebResponseTimeoutException(e);
                }

                throw;
            }
        }

        /// <summary>
        /// Tracks the response closing.
        /// </summary>
        /// <param name="req">The request.</param>
        public static void TrackResponseClosing([NotNull] ref HttpWebRequest req)
        {
            CloseTrackingHttpWebRequest.Wrap(ref req);
        }

        /// <summary>
        /// Checks the validation result.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="certificate">The certificate.</param>
        /// <param name="chain">The chain.</param>
        /// <param name="sslPolicyErrors">The SSL policy errors.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private static bool CheckValidationResult(
            [CanBeNull] object sender,
            [CanBeNull] X509Certificate certificate,
            [CanBeNull] X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                Trace.WriteLine($"SSL Policy error {sslPolicyErrors}");
            }

            return true;
        }

        /// <summary>
        /// Decodes the body.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>The body.</returns>
        [NotNull]
        private static string DecodeBody([NotNull] WebResponse response)
        {
            var s = response.GetResponseStream();
            if (s == null)
            {
                return string.Empty;
            }

            using (var sr = new StreamReader(s))
            {
                return sr.ReadToEnd();
            }
        }

        /// <summary>
        /// Gets the body text.
        /// </summary>
        /// <param name="resp">The response.</param>
        /// <returns>The body text.</returns>
        [NotNull]
        private static string GetBodyText([NotNull] WebResponse resp)
        {
            if (string.IsNullOrEmpty(resp.ContentType))
            {
                return string.Empty;
            }

            var contentTypeData = MimeHelper.ParseContentType(resp.ContentType, true);
            var mainType = (string)contentTypeData[string.Empty];
            switch (mainType)
            {
                case @"text/plain":
                    {
                        return HttpRequestHelper.DecodeBody(resp);
                    }

                case @"text/html":
                    {
                        return StringHelper.CompressExcessWhitespace(
                            HTMLDocumentHelper.HTMLToPlainText(
                                LightWeightHTMLThinner2.Thin(
                                    HttpRequestHelper.DecodeBody(resp), true)));
                    }

                default:
                    return string.Empty;
            }
        }
    }
}
