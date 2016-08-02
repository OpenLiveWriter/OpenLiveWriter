using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenLiveWriter.CoreServices;
using System.Net;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.HtmlParser.Parser.FormAgent;
using System.IO;

namespace OpenLiveWriter.BlogClient.Clients
{
    public class JSONRestRequestHelper
    {
        public string Get(ref Uri uri, HttpRequestFilter filter, out WebHeaderCollection responseHeaders, params string[] parameters)
        {
            return SimpleRequest("GET", ref uri, filter, out responseHeaders, parameters);
        }

        public string Put(ref Uri uri, string etag, HttpRequestFilter filter, string contentType, string jsonContent, string encoding, bool ignoreResponse)
        {
            WebHeaderCollection responseHeaders;
            return Send("PUT", ref uri, etag, filter, contentType, jsonContent, encoding, null, ignoreResponse, out responseHeaders);
        }

        public string Delete(ref Uri uri, string etag, HttpRequestFilter filter, string contentType, string jsonContent, string encoding, bool ignoreResponse)
        {
            WebHeaderCollection responseHeaders;
            return Send("DELETE", ref uri, etag, filter, contentType, jsonContent, encoding, null, ignoreResponse, out responseHeaders);
        }

        private static string SimpleRequest(string method, ref Uri uri, HttpRequestFilter filter, out WebHeaderCollection responseHeaders, params string[] parameters)
        {
            var absUri = UrlHelper.SafeToAbsoluteUri(uri);

            if (parameters.Length > 0)
            {
                var formData = new FormData(true, parameters);

                if (absUri.IndexOf('?') == -1)
                {
                    absUri += "?" + formData;
                }
                else
                {
                    absUri += "&" + formData;
                }
            }

            var simpleRequest = new RedirectHelper.SimpleRequest(method, filter);
            var response = RedirectHelper.GetResponse(absUri, simpleRequest.Create);

            try
            {
                uri = response.ResponseUri;
                responseHeaders = response.Headers;
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    return sr.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                HttpRequestHelper.LogException(ex);

                // see if this was a 404 not found
                switch (ex.Status)
                {
                    case WebExceptionStatus.ProtocolError:
                        var exceptionResponse = ex.Response as HttpWebResponse;
                        if (exceptionResponse != null && exceptionResponse.StatusCode == HttpStatusCode.NotFound)
                        {
                            throw new BlogClientPostUrlNotFoundException(uri.AbsoluteUri, ex.Message);
                        }
                        else if (exceptionResponse != null && exceptionResponse.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            throw new BlogClientAuthenticationException(exceptionResponse.StatusCode.ToString(),
                                ex.Message, ex);
                        }
                        else
                        {
                            throw new BlogClientHttpErrorException(uri.AbsoluteUri, string.Format(CultureInfo.InvariantCulture, "{0} {1}", (int)exceptionResponse.StatusCode, exceptionResponse.StatusDescription), ex);
                        }
                    default:
                        throw new BlogClientConnectionErrorException(uri.AbsoluteUri, ex.Message);
                }
            }
            finally
            {
                if (response != null)
                    response.Close();
            }
        }

        private string Send(string method, ref Uri uri, string etag, HttpRequestFilter filter, string contentType, string jsonContent, string encoding, string filename, bool ignoreResponse, out WebHeaderCollection responseHeaders)
        {
            var absUri = UrlHelper.SafeToAbsoluteUri(uri);

            var sf = new SendFactory(etag, method, filter, contentType, jsonContent, encoding);

            var response = RedirectHelper.GetResponse(absUri, sf.Create);
            try
            {
                responseHeaders = response.Headers;
                uri = response.ResponseUri;
                if (ignoreResponse || response.StatusCode == HttpStatusCode.NoContent)
                {
                    return null;
                }
                else
                {
                    using (var sr = new StreamReader(response.GetResponseStream()))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch (WebException ex)
            {
                HttpRequestHelper.LogException(ex);

                // see if this was a 404 not found
                switch (ex.Status)
                {
                    case WebExceptionStatus.ProtocolError:
                        var exceptionResponse = ex.Response as HttpWebResponse;
                        if (exceptionResponse != null && exceptionResponse.StatusCode == HttpStatusCode.NotFound)
                        {
                            throw new BlogClientPostUrlNotFoundException(uri.AbsoluteUri, ex.Message);
                        }
                        else if (exceptionResponse != null && exceptionResponse.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            throw new BlogClientAuthenticationException(exceptionResponse.StatusCode.ToString(),
                                ex.Message, ex);
                        }
                        else
                        {
                            throw new BlogClientHttpErrorException(uri.AbsoluteUri, string.Format(CultureInfo.InvariantCulture, "{0} {1}", (int)exceptionResponse.StatusCode, exceptionResponse.StatusDescription), ex);
                        }
                    default:
                        throw new BlogClientConnectionErrorException(uri.AbsoluteUri, ex.Message);
                }
            }
            finally
            {
                if (response != null)
                    response.Close();
            }
        }

        private struct SendFactory
        {
            private readonly string _etag;
            private readonly string _method;
            private readonly HttpRequestFilter _filter;
            private readonly string _contentType;
            private readonly string _jsonContent;
            private readonly Encoding _encodingToUse;

            public SendFactory(string etag, string method, HttpRequestFilter filter, string contentType, string jsonContent, string encoding)
            {
                _etag = etag;
                _method = method;
                _filter = filter;
                _contentType = contentType;
                _jsonContent = jsonContent;

                _encodingToUse = new UTF8Encoding(false, false);
                _encodingToUse = StringHelper.GetEncoding(encoding, _encodingToUse);
            }

            public HttpWebRequest Create(string uri)
            {
                var request = HttpRequestHelper.CreateHttpWebRequest(uri, true);

                request.Method = _method;
                if (!String.IsNullOrEmpty(_etag))
                {
                    request.Headers["If-match"] = _etag;
                }

                if (_contentType != null)
                {
                    request.ContentType = _contentType;
                }

                if (_filter != null)
                {
                    _filter(request);
                }

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(_jsonContent);
                    streamWriter.Flush();
                }

                return request;
            }
        }
    }
}
