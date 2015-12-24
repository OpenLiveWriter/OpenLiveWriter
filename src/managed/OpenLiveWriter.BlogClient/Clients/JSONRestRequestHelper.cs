using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenLiveWriter.CoreServices;
using System.Net;
using OpenLiveWriter.HtmlParser.Parser.FormAgent;
using System.IO;

namespace OpenLiveWriter.BlogClient.Clients
{
    public class JSONRestRequestHelper
    {
        public JSONRestRequestHelper()
        {
        }

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
            string absUri = UrlHelper.SafeToAbsoluteUri(uri);

            if (parameters.Length > 0)
            {
                FormData formData = new FormData(true, parameters);

                if (absUri.IndexOf('?') == -1)
                    absUri += "?" + formData.ToString();
                else
                    absUri += "&" + formData.ToString();
            }

            RedirectHelper.SimpleRequest simpleRequest = new RedirectHelper.SimpleRequest(method, filter);
            HttpWebResponse response = RedirectHelper.GetResponse(absUri, new RedirectHelper.RequestFactory(simpleRequest.Create));
            try
            {
                uri = response.ResponseUri;
                responseHeaders = response.Headers;
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    return sr.ReadToEnd();
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
            string absUri = UrlHelper.SafeToAbsoluteUri(uri);

            SendFactory sf = new SendFactory(etag, method, filter, contentType, jsonContent, encoding);

            HttpWebResponse response = RedirectHelper.GetResponse(absUri, new RedirectHelper.RequestFactory(sf.Create));
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
                try
                {
                    _encodingToUse = StringHelper.GetEncoding(encoding, _encodingToUse);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            public HttpWebRequest Create(string uri)
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);

                // Set Accept to */* to stop Bad Behavior plugin for WordPress from
                // thinking we're a spam cannon
                request.Accept = "*/*";
                request.AllowAutoRedirect = true;

                request.KeepAlive = false;
                request.Pipelined = false;

                request.Method = _method;
                if (_etag != null && _etag != "")
                    request.Headers["If-match"] = _etag;
                if (_contentType != null)
                    request.ContentType = _contentType;
                if (_filter != null)
                    _filter(request);

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
