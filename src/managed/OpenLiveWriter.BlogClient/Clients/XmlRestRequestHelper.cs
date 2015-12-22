// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.HtmlParser.Parser.FormAgent;

namespace OpenLiveWriter.BlogClient.Clients
{
    /// <summary>
    /// Helper class for making REST-ful XML HTTP requests.
    /// </summary>
    public class XmlRestRequestHelper
    {
        public XmlRestRequestHelper()
        {
        }

        public XmlDocument Get(ref Uri uri, HttpRequestFilter filter, params string[] parameters)
        {
            WebHeaderCollection responseHeaders;
            return Get(ref uri, filter, out responseHeaders, parameters);
        }

        /// <summary>
        /// Retrieve the specified URI, using the given filter, with the supplied parameters (if any).
        /// The parameters parameter should be an even number of strings, where each odd element is
        /// a param name and each following even element is the corresponding param value.  For example,
        /// to retrieve http://www.vox.com/atom?svc=post&id=100, you would say:
        ///
        /// Get("http://www.vox.com/atom", "svc", "post", "id", "100");
        ///
        /// If a param value is null or empty string, that param will not be included in the final URL
        /// (i.e. the corresponding param name will also be dropped).
        /// </summary>
        public virtual XmlDocument Get(ref Uri uri, HttpRequestFilter filter, out WebHeaderCollection responseHeaders, params string[] parameters)
        {
            return SimpleRequest("GET", ref uri, filter, out responseHeaders, parameters);
        }

        /// <summary>
        /// Performs an HTTP DELETE on the URL and contains no body, returns the body as an XmlDocument if there is one
        /// </summary>
        public virtual XmlDocument Delete(Uri uri, HttpRequestFilter filter, out WebHeaderCollection responseHeaders)
        {
            return SimpleRequest("DELETE", ref uri, filter, out responseHeaders, new string[] { });
        }

        private static XmlDocument SimpleRequest(string method, ref Uri uri, HttpRequestFilter filter, out WebHeaderCollection responseHeaders, params string[] parameters)
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
                return ParseXmlResponse(response);
            }
            finally
            {
                if (response != null)
                    response.Close();
            }
        }

        /// <summary>
        /// Performs an HTTP PUT with the specified XML document as the request body.
        /// </summary>
        public XmlDocument Put(ref Uri uri, string etag, HttpRequestFilter filter, string contentType, XmlDocument doc, string encoding, bool ignoreResponse)
        {
            WebHeaderCollection responseHeaders;
            return Put(ref uri, etag, filter, contentType, doc, encoding, ignoreResponse, out responseHeaders);
        }

        /// <summary>
        /// Performs an HTTP PUT with the specified XML document as the request body.
        /// </summary>
        public XmlDocument Put(ref Uri uri, string etag, HttpRequestFilter filter, string contentType, XmlDocument doc, string encoding, bool ignoreResponse, out WebHeaderCollection responseHeaders)
        {
            return Send("PUT", ref uri, etag, filter, contentType, doc, encoding, null, ignoreResponse, out responseHeaders);
        }

        /// <summary>
        /// Performs an HTTP POST with the specified XML document as the request body.
        /// </summary>
        public XmlDocument Post(ref Uri uri, HttpRequestFilter filter, string contentType, XmlDocument doc, string encoding)
        {
            WebHeaderCollection responseHeaders;
            return Post(ref uri, filter, contentType, doc, encoding, out responseHeaders);
        }

        /// <summary>
        /// Performs an HTTP POST with the specified XML document as the request body.
        /// </summary>
        public XmlDocument Post(ref Uri uri, HttpRequestFilter filter, string contentType, XmlDocument doc, string encoding, out WebHeaderCollection responseHeaders)
        {
            return Send("POST", ref uri, null, filter, contentType, doc, encoding, null, false, out responseHeaders);
        }

        /// <summary>
        /// Performs a multipart MIME HTTP POST with the specified XML document as the request body and filename as the payload.
        /// </summary>
        public XmlDocument Post(ref Uri uri, HttpRequestFilter filter, string contentType, XmlDocument doc, string encoding, string filename, out WebHeaderCollection responseHeaders)
        {
            return Send("POST", ref uri, null, filter, contentType, doc, encoding, filename, false, out responseHeaders);
        }

        protected virtual XmlDocument MultipartSend(string method, ref Uri uri, string etag, HttpRequestFilter filter, string contentType, XmlDocument doc, string encoding, string filename, bool ignoreResponse, out WebHeaderCollection responseHeaders)
        {
            throw new NotImplementedException();
        }

        protected virtual XmlDocument Send(string method, ref Uri uri, string etag, HttpRequestFilter filter, string contentType, XmlDocument doc, string encoding, string filename, bool ignoreResponse, out WebHeaderCollection responseHeaders)
        {
            if (!String.IsNullOrEmpty(filename))
            {
                return MultipartSend(method, ref uri, etag, filter, contentType, doc, encoding, filename, ignoreResponse,
                                     out responseHeaders);
            }

            string absUri = UrlHelper.SafeToAbsoluteUri(uri);
            Debug.WriteLine("XML Request to " + absUri + ":\r\n" + doc.InnerXml);

            SendFactory sf = new SendFactory(etag, method, filter, contentType, doc, encoding);
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
                    XmlDocument xmlDocResponse = ParseXmlResponse(response);
                    return xmlDocResponse;
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
            private readonly XmlDocument _doc;
            private readonly Encoding _encodingToUse;

            public SendFactory(string etag, string method, HttpRequestFilter filter, string contentType, XmlDocument doc, string encoding)
            {
                _etag = etag;
                _method = method;
                _filter = filter;
                _contentType = contentType;
                _doc = doc;

                //select the encoding
                _encodingToUse = new UTF8Encoding(false, false);
                try
                {
                    _encodingToUse = StringHelper.GetEncoding(encoding, _encodingToUse);
                }
                catch (Exception ex)
                {
                    Trace.Fail("Error while getting transport encoding: " + ex.ToString());
                }
            }

            public HttpWebRequest Create(string uri)
            {
                HttpWebRequest request = HttpRequestHelper.CreateHttpWebRequest(uri, true);
                request.Method = _method;
                //			    request.KeepAlive = true;
                //			    request.Pipelined = true;
                if (_etag != null && _etag != "")
                    request.Headers["If-match"] = _etag;
                if (_contentType != null)
                    request.ContentType = _contentType;
                if (_filter != null)
                    _filter(request);

                if (ApplicationDiagnostics.VerboseLogging)
                    Trace.WriteLine(
                        string.Format(CultureInfo.InvariantCulture, "XML REST request:\r\n{0} {1}\r\n{2}\r\n{3}",
                            _method, uri, (_etag != null && _etag != "") ? "If-match: " + _etag : "(no etag)", _doc.OuterXml));

                using (Stream s = request.GetRequestStream())
                {
                    XmlTextWriter writer = new XmlTextWriter(s, _encodingToUse);
                    writer.Formatting = Formatting.Indented;
                    writer.Indentation = 1;
                    writer.IndentChar = ' ';
                    writer.WriteStartDocument();
                    _doc.DocumentElement.WriteTo(writer);
                    writer.Close();
                }

                return request;
            }
        }

        public class MultipartMimeSendFactory
        {
            private readonly string _filename;
            private readonly XmlDocument _xmlDoc;
            private readonly Encoding _encoding;
            private readonly HttpRequestFilter _filter;
            private readonly MultipartMimeRequestHelper _multipartMimeRequestHelper;
            public MultipartMimeSendFactory(HttpRequestFilter filter, XmlDocument xmlRequest, string filename, string encoding, MultipartMimeRequestHelper multipartMimeRequestHelper)
            {
                if (xmlRequest == null)
                    throw new ArgumentNullException();

                // Add boundary to params
                _filename = filename;
                _xmlDoc = xmlRequest;
                _filter = filter;
                _multipartMimeRequestHelper = multipartMimeRequestHelper;

                //select the encoding
                _encoding = new UTF8Encoding(false, false);
                try
                {
                    _encoding = StringHelper.GetEncoding(encoding, _encoding);
                }
                catch (Exception ex)
                {
                    Trace.Fail("Error while getting transport encoding: " + ex.ToString());
                }
            }

            public HttpWebRequest Create(string uri)
            {
                HttpWebRequest req = HttpRequestHelper.CreateHttpWebRequest(uri, true);
                _multipartMimeRequestHelper.Init(req);

                if (_filter != null)
                    _filter(req);

                _multipartMimeRequestHelper.Open();
                _multipartMimeRequestHelper.AddXmlRequest(_xmlDoc);
                _multipartMimeRequestHelper.AddFile(_filename);
                _multipartMimeRequestHelper.Close();

                using (CancelableStream stream = new CancelableStream(new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    return _multipartMimeRequestHelper.SendRequest(stream);
                }
            }
        }

        protected static XmlDocument ParseXmlResponse(HttpWebResponse response)
        {
            MemoryStream ms = new MemoryStream();
            using (Stream s = response.GetResponseStream())
            {
                StreamHelper.Transfer(s, ms);
            }
            ms.Seek(0, SeekOrigin.Begin);

            try
            {
                if (ApplicationDiagnostics.VerboseLogging)
                    Trace.WriteLine("XML REST response:\r\n" + UrlHelper.SafeToAbsoluteUri(response.ResponseUri) + "\r\n" + new StreamReader(ms, Encoding.UTF8).ReadToEnd());
            }
            catch (Exception e)
            {
                Trace.Fail("Failed to log REST response: " + e.ToString());
            }

            ms.Seek(0, SeekOrigin.Begin);
            if (ms.Length == 0)
                return null;

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(ms);
                XmlHelper.ApplyBaseUri(xmlDoc, response.ResponseUri);

                return xmlDoc;
            }
            catch (Exception e)
            {
                Trace.Fail("Malformed XML document: " + e.ToString());
                return null;
            }
        }
    }
}
