// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenLiveWriter.CoreServices.Diagnostics;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Modern HttpClient-based HTTP service for Open Live Writer.
    /// Replaces the deprecated WebRequest/HttpWebRequest APIs.
    /// </summary>
    public static class HttpClientService
    {
        private static readonly Lazy<HttpClient> _defaultClient = new Lazy<HttpClient>(CreateDefaultClient);
        private static readonly object _lock = new object();

        /// <summary>
        /// Gets the default HttpClient instance configured with standard settings.
        /// </summary>
        public static HttpClient DefaultClient => _defaultClient.Value;

        /// <summary>
        /// Creates an HttpClient with the standard Open Live Writer configuration.
        /// </summary>
        public static HttpClient CreateDefaultClient()
        {
            var handler = CreateHandler();
            var client = new HttpClient(handler);
            ConfigureClient(client);
            return client;
        }

        /// <summary>
        /// Creates an HttpClient with custom credentials.
        /// </summary>
        public static HttpClient CreateClientWithCredentials(string username, string password, string url)
        {
            var handler = CreateHandler(username, password, url);
            var client = new HttpClient(handler);
            ConfigureClient(client);
            return client;
        }

        /// <summary>
        /// Creates the HttpClientHandler with proxy and certificate settings.
        /// </summary>
        public static HttpClientHandler CreateHandler(string username = null, string password = null, string url = null)
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false, // Match HttpRequestHelper behavior
                UseCookies = true,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            // Configure proxy
            if (WebProxySettings.ProxyEnabled)
            {
                string proxyServerUrl = WebProxySettings.Hostname;
                if (proxyServerUrl.IndexOf("://", StringComparison.OrdinalIgnoreCase) == -1)
                    proxyServerUrl = "http://" + proxyServerUrl;
                if (WebProxySettings.Port > 0)
                    proxyServerUrl += ":" + WebProxySettings.Port;

                var proxy = new WebProxy(proxyServerUrl, false);
                if (!string.IsNullOrEmpty(WebProxySettings.Username))
                {
                    proxy.Credentials = new NetworkCredential(WebProxySettings.Username, WebProxySettings.Password);
                }
                handler.Proxy = proxy;
                handler.UseProxy = true;
            }

            // Configure credentials
            if (!string.IsNullOrEmpty(username) || !string.IsNullOrEmpty(password))
            {
                string userDomain = string.Empty;
                string user = username ?? string.Empty;

                int domainIndex = user.IndexOf(@"\", StringComparison.OrdinalIgnoreCase);
                if (domainIndex != -1)
                {
                    userDomain = user.Substring(0, domainIndex);
                    user = user.Substring(domainIndex + 1);
                }

                handler.Credentials = new NetworkCredential(user, password ?? string.Empty, userDomain);
            }

            // Configure SSL certificate validation
            if (ApplicationDiagnostics.AllowUnsafeCertificates)
            {
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                {
                    if (errors != SslPolicyErrors.None)
                    {
                        Trace.WriteLine("SSL Policy error " + errors);
                    }
                    return true;
                };
            }

            return handler;
        }

        /// <summary>
        /// Configures the HttpClient with standard headers and timeouts.
        /// </summary>
        private static void ConfigureClient(HttpClient client)
        {
            int timeout = WebProxySettings.HttpRequestTimeout;
            client.Timeout = TimeSpan.FromMilliseconds(timeout);

            // Set default headers
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            client.DefaultRequestHeaders.UserAgent.ParseAdd(ApplicationEnvironment.UserAgent);

            // Set Accept-Language
            string acceptLang = CultureInfo.CurrentUICulture.Name.Split('/')[0];
            if (acceptLang.ToUpperInvariant() == "SR-SP-LATN")
                acceptLang = "sr-Latn-CS";
            if (acceptLang != "en-US")
                acceptLang += ", en-US";
            acceptLang += ", en, *";
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd(acceptLang);
        }

        /// <summary>
        /// Sends a GET request and returns the response.
        /// </summary>
        public static async Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            return await DefaultClient.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a GET request and returns the response (synchronous wrapper).
        /// </summary>
        public static HttpResponseMessage Get(string requestUri)
        {
            return GetAsync(requestUri).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Sends a GET request and returns the response stream.
        /// </summary>
        public static async Task<Stream> GetStreamAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            var response = await DefaultClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a GET request and returns the response as a string.
        /// </summary>
        public static async Task<string> GetStringAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            return await DefaultClient.GetStringAsync(requestUri, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a POST request with the specified content.
        /// </summary>
        public static async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
        {
            return await DefaultClient.PostAsync(requestUri, content, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a POST request (synchronous wrapper).
        /// </summary>
        public static HttpResponseMessage Post(string requestUri, HttpContent content)
        {
            return PostAsync(requestUri, content).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Downloads a file to a stream, returning null on failure.
        /// </summary>
        public static Stream SafeDownloadFile(string fileUrl, out string responseUri)
        {
            responseUri = null;
            try
            {
                var response = Get(fileUrl);
                if (response.IsSuccessStatusCode)
                {
                    responseUri = response.RequestMessage?.RequestUri?.AbsoluteUri ?? fileUrl;
                    return response.Content.ReadAsStream();
                }
                return null;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Unable to download file \"{fileUrl}\": {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Sends a request with full control over the request message.
        /// </summary>
        public static async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            return await DefaultClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a request (synchronous wrapper).
        /// </summary>
        public static HttpResponseMessage Send(HttpRequestMessage request)
        {
            return SendAsync(request).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a configured HttpRequestMessage.
        /// </summary>
        public static HttpRequestMessage CreateRequest(HttpMethod method, string requestUri)
        {
            var request = new HttpRequestMessage(method, requestUri);

            // Set Accept header
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

            // Set Accept-Language
            string acceptLang = CultureInfo.CurrentUICulture.Name.Split('/')[0];
            if (acceptLang.ToUpperInvariant() == "SR-SP-LATN")
                acceptLang = "sr-Latn-CS";
            if (acceptLang != "en-US")
                acceptLang += ", en-US";
            acceptLang += ", en, *";
            request.Headers.AcceptLanguage.ParseAdd(acceptLang);

            // Temporary fix for Blogger photos issue
            if (request.RequestUri?.Host?.Contains("picasaweb.google.com") == true)
            {
                request.Headers.Add("deprecation-extension", "true");
            }

            return request;
        }

        /// <summary>
        /// Gets a user-friendly error message from an HttpResponseMessage.
        /// </summary>
        public static string GetFriendlyErrorMessage(HttpResponseMessage response)
        {
            if (response == null)
                return "No response received";

            var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return $"{(int)response.StatusCode} {response.ReasonPhrase}\r\n\r\n{content}";
        }

        /// <summary>
        /// Logs details of an HTTP response for debugging.
        /// </summary>
        public static string DumpResponse(HttpResponseMessage response)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"HTTP/{response.Version} {(int)response.StatusCode} {response.ReasonPhrase}");

            foreach (var header in response.Headers)
            {
                sb.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }
            foreach (var header in response.Content.Headers)
            {
                sb.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }

            sb.AppendLine();
            sb.AppendLine(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            return sb.ToString();
        }
    }
}
