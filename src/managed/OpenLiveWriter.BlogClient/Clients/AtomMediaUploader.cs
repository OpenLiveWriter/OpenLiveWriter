// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#define APIHACK
using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Threading;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.HtmlParser.Parser;

namespace OpenLiveWriter.BlogClient.Clients
{
    public class AtomMediaUploader
    {
        protected const string EDIT_MEDIA_LINK = "EditMediaLink";
        protected const string EDIT_MEDIA_ENTRY_LINK = "EditMediaLinkEntryLink";
        protected const string MEDIA_ETAG = "MediaEtag";

        protected XmlNamespaceManager _nsMgr
        {
            get;
            private set;
        }
        protected HttpRequestFilter _requestFilter
        {
            get;
            private set;
        }
        protected readonly string _collectionUri;
        protected IBlogClientOptions _options
        {
            get;
            private set;
        }
        protected XmlRestRequestHelper xmlRestRequestHelper
        {
            get;
            private set;
        }

        public AtomMediaUploader(XmlNamespaceManager nsMgr, HttpRequestFilter requestFilter, string collectionUri, IBlogClientOptions options)
            : this(nsMgr, requestFilter, collectionUri, options, new XmlRestRequestHelper())
        {
        }

        public AtomMediaUploader(XmlNamespaceManager nsMgr, HttpRequestFilter requestFilter, string collectionUri, IBlogClientOptions options, XmlRestRequestHelper xmlRestRequestHelper)
        {
            this._nsMgr = nsMgr;
            this._requestFilter = requestFilter;
            this._collectionUri = collectionUri;
            this._options = options;
            this.xmlRestRequestHelper = xmlRestRequestHelper;
        }

        public string DoBeforePublishUploadWork(IFileUploadContext uploadContext)
        {
            string path = uploadContext.GetContentsLocalFilePath();
            string srcUrl;
            string editUri = uploadContext.Settings.GetString(EDIT_MEDIA_LINK, null);
            string editEntryUri = uploadContext.Settings.GetString(EDIT_MEDIA_ENTRY_LINK, null);
            string etag = uploadContext.Settings.GetString(MEDIA_ETAG, null);
            if (string.IsNullOrEmpty(editUri))
            {
                PostNewImage(path, false, out srcUrl, out editUri, out editEntryUri);
            }
            else
            {
                try
                {
                    UpdateImage(ref editUri, path, editEntryUri, etag, true, out srcUrl);
                }
                catch (Exception e)
                {
                    Trace.Fail(e.ToString());

                    bool success = false;
                    srcUrl = null; // compiler complains without this line
                    try
                    {
                        // couldn't update existing image? try posting a new one
                        PostNewImage(path, false, out srcUrl, out editUri, out editEntryUri);
                        success = true;

                        if (e is WebException)
                        {
                            Trace.WriteLine("Image PUT failed, but POST succeeded. PUT exception follows.");
                            HttpRequestHelper.LogException((WebException)e);
                        }
                    }
                    catch
                    {
                    }
                    if (!success)
                        throw;  // rethrow the exception from the update, not the post
                }
            }
            uploadContext.Settings.SetString(EDIT_MEDIA_LINK, editUri);
            uploadContext.Settings.SetString(EDIT_MEDIA_ENTRY_LINK, editEntryUri);
            uploadContext.Settings.SetString(MEDIA_ETAG, null);

            UpdateETag(uploadContext, editUri);
            return srcUrl;
        }

        protected virtual void UpdateETag(IFileUploadContext uploadContext, string editUri)
        {
            try
            {
                string newEtag = AtomClient.GetEtag(editUri, _requestFilter);
                uploadContext.Settings.SetString(MEDIA_ETAG, newEtag);
            }
            catch (Exception)
            {

            }
        }

        public virtual void PostNewImage(string path, bool allowWriteStreamBuffering, out string srcUrl, out string editMediaUri, out string editEntryUri)
        {
            string mediaCollectionUri = _collectionUri;
            if (mediaCollectionUri == null || mediaCollectionUri == "")
                throw new BlogClientFileUploadNotSupportedException();

            HttpWebResponse response = null;
            try
            {
                response = RedirectHelper.GetResponse(mediaCollectionUri,
                new RedirectHelper.RequestFactory(new ImageUploadHelper(this, path, "POST", null, allowWriteStreamBuffering).Create));

                string entryUri;
                string etag;
                string selfPage;
                XmlDocument xmlDoc = GetCreatedEntity(response, out entryUri, out etag);
                ParseResponse(xmlDoc, out srcUrl, out editMediaUri, out editEntryUri, out selfPage);
            }
            catch (WebException we)
            {
                // The error may have been due to the server requiring stream buffering (WinLive 114314, 252175)
                // Try again with stream buffering.
                if (we.Status == WebExceptionStatus.ProtocolError && !allowWriteStreamBuffering)
                {
                    PostNewImage(path, true, out srcUrl, out editMediaUri, out editEntryUri);

                }
                else
                {
                    throw;
                }
            }
            finally
            {
                if (response != null)
                    response.Close();
            }
        }

        private XmlDocument GetCreatedEntity(HttpWebResponse postResponse, out string editUri, out string etag)
        {
            editUri = postResponse.Headers["Location"];
            string contentLocation = postResponse.Headers["Content-Location"];
            if (string.IsNullOrEmpty(editUri) || editUri != contentLocation)
            {
                Uri uri = postResponse.ResponseUri;
                if (!string.IsNullOrEmpty(editUri))
                    uri = new Uri(editUri);
                WebHeaderCollection responseHeaders;
                XmlDocument doc = xmlRestRequestHelper.Get(ref uri, _requestFilter, out responseHeaders);
                etag = responseHeaders["ETag"];
                return doc;
            }
            else
            {
                etag = postResponse.Headers["ETag"];
                XmlDocument xmlDoc = new XmlDocument();
                using (Stream s = postResponse.GetResponseStream())
                    xmlDoc.Load(s);
                XmlHelper.ApplyBaseUri(xmlDoc, postResponse.ResponseUri);
                return xmlDoc;
            }
        }

        protected virtual void UpdateImage(ref string editMediaUri, string path, string editEntryUri, string etag, bool getEditInfo, out string srcUrl)
        {
            string thumbnailSmall;
            string thumbnailLarge;

            UpdateImage(false, ref editMediaUri, path, editEntryUri, etag, getEditInfo, out srcUrl, out thumbnailSmall, out thumbnailLarge);
        }

        protected virtual void UpdateImage(bool allowWriteStreamBuffering, ref string editMediaUri, string path, string editEntryUri, string etag, bool getEditInfo, out string srcUrl)
        {
            string thumbnailSmall;
            string thumbnailLarge;

            UpdateImage(allowWriteStreamBuffering, ref editMediaUri, path, editEntryUri, etag, getEditInfo, out srcUrl, out thumbnailSmall, out thumbnailLarge);
        }

        protected virtual void UpdateImage(bool allowWriteStreamBuffering, ref string editMediaUri, string path, string editEntryUri, string etag, bool getEditInfo, out string srcUrl, out string thumbnailSmall, out string thumbnailLarge)
        {
            HttpWebResponse response = null;
            try
            {
                response = RedirectHelper.GetResponse(editMediaUri,
                    new RedirectHelper.RequestFactory(new ImageUploadHelper(this, path, "PUT", etag, allowWriteStreamBuffering).Create));
                response.Close();

            }
            catch (WebException we)
            {
                bool recovered = false;

                if (we.Status == WebExceptionStatus.ProtocolError && we.Response != null)
                {
                    HttpWebResponse errResponse = we.Response as HttpWebResponse;
                    if (errResponse != null && errResponse.StatusCode == HttpStatusCode.PreconditionFailed)
                    {
                        string newEtag = AtomClient.GetEtag(editMediaUri, _requestFilter);
                        if (newEtag != null && newEtag.Length > 0 && newEtag != etag)
                        {
                            if (!AtomClient.ConfirmOverwrite())
                                throw new BlogClientOperationCancelledException();

                            try
                            {
                                response = RedirectHelper.GetResponse(editMediaUri,
                                new RedirectHelper.RequestFactory(new ImageUploadHelper(this, path, "PUT", newEtag, allowWriteStreamBuffering).Create));
                            }
                            finally
                            {
                                if (response != null)
                                    response.Close();
                            }

                            recovered = true;
                        }
                    }
                    else if (!allowWriteStreamBuffering)
                    {
                        // The error may have been due to the server requiring stream buffering (WinLive 114314, 252175)
                        // Try again with stream buffering.
                        UpdateImage(true, ref editMediaUri, path, editEntryUri, etag, getEditInfo, out srcUrl, out thumbnailSmall, out thumbnailLarge);
                        recovered = true;
                    }
                }
                if (!recovered)
                    throw;
            }

            // Check to see if we are going to get the src url and the etag, in most cases we will want to get this
            // information, but in the case of a photo album, since we never edit the image or link directly to them
            // we don't need the information and it can saves an http request.
            if (getEditInfo)
            {
                string selfPage;
                Uri uri = new Uri(editEntryUri);
                XmlDocument mediaLinkEntry = xmlRestRequestHelper.Get(ref uri, _requestFilter);
                ParseResponse(mediaLinkEntry, out srcUrl, out editMediaUri, out editEntryUri, out selfPage, out thumbnailSmall, out thumbnailLarge);
            }
            else
            {
                thumbnailSmall = null;
                thumbnailLarge = null;
                srcUrl = null;
            }
        }

        protected virtual void ParseResponse(XmlDocument xmlDoc, out string srcUrl, out string editUri, out string editEntryUri, out string selfPage, out string thumbnailSmall, out string thumbnailLarge)
        {
            thumbnailSmall = null;
            thumbnailLarge = null;
            ParseResponse(xmlDoc, out srcUrl, out editUri, out editEntryUri, out selfPage);
        }

        protected virtual void ParseResponse(XmlDocument xmlDoc, out string srcUrl, out string editUri, out string editEntryUri, out string selfPage)
        {
            XmlElement contentEl = xmlDoc.SelectSingleNode("/atom:entry/atom:content", _nsMgr) as XmlElement;
            srcUrl = XmlHelper.GetUrl(contentEl, "@src", null);
            editUri = AtomEntry.GetLink(xmlDoc.SelectSingleNode("/atom:entry", _nsMgr) as XmlElement, _nsMgr, "edit-media",
                              null, null, null);
            editEntryUri = AtomEntry.GetLink(xmlDoc.SelectSingleNode("/atom:entry", _nsMgr) as XmlElement, _nsMgr, "edit",
                                   null, null, null);
            selfPage = AtomEntry.GetLink(xmlDoc.SelectSingleNode("/atom:entry", _nsMgr) as XmlElement, _nsMgr, "alternate",
                       null, null, null);
        }

        protected class ImageUploadHelper
        {
            private readonly AtomMediaUploader _parent;
            private readonly string _filename;
            private readonly string _method;
            private readonly string _etag;
            private readonly bool _allowWriteStreamBuffering;

            public ImageUploadHelper(AtomMediaUploader parent, string filename, string method, string etag, bool allowWriteStreamBuffering)
            {
                _parent = parent;
                _filename = filename;
                _method = method;
                _etag = etag;
                _allowWriteStreamBuffering = allowWriteStreamBuffering;
            }

            public HttpWebRequest Create(string uri)
            {
                // TODO: ETag support required??
                // TODO: choose rational timeout values
                HttpWebRequest request = HttpRequestHelper.CreateHttpWebRequest(uri, false);

                request.ContentType = MimeHelper.GetContentType(Path.GetExtension(_filename));
                if (_parent._options != null && _parent._options.SupportsSlug)
                    request.Headers.Add("Slug", Path.GetFileNameWithoutExtension(_filename));

                request.Method = _method;

                request.AllowWriteStreamBuffering = _allowWriteStreamBuffering;

                if (_etag != null && _etag.Length != 0)
                    request.Headers.Add("If-match", _etag);

                _parent._requestFilter(request);

                using (Stream inS = new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (CancelableStream cs = new CancelableStream(inS))
                    {
                        request.ContentLength = cs.Length;
                        using (Stream s = request.GetRequestStream())
                        {

                            StreamHelper.Transfer(cs, s);
                        }
                    }
                }

                return request;
            }
        }

    }

    public class MultipartMimeRequestHelper
    {
        private string _boundary;
        private HttpWebRequest _request;
        Stream _requestStream;
        protected MemoryStream _requestBodyTop = new MemoryStream();
        protected MemoryStream _requestBodyBottom = new MemoryStream();

        public virtual void Init(HttpWebRequest request)
        {
            _boundary = String.Format(CultureInfo.InvariantCulture, "============{0}==", Guid.NewGuid().ToString().Replace("-", ""));
            _request = request;
            _request.Method = "POST";
            _request.ContentType = String.Format(CultureInfo.InvariantCulture,
                                                @"multipart/related; boundary=""{0}""; type = ""application/atom+xml""",
                                                _boundary);
        }

        public virtual void Open()
        {
            AddBoundary(true, _requestBodyTop);
        }

        public virtual void Close()
        {
            AddBoundary(false, _requestBodyBottom);
            Write("--" + Environment.NewLine, _requestBodyBottom);
        }

        public virtual void AddBoundary(bool newLine, MemoryStream stream)
        {
            Write("--" + _boundary + (newLine ? Environment.NewLine : ""), stream);
        }

        public virtual void AddXmlRequest(XmlDocument xmlDocument)
        {
            throw new NotImplementedException();
        }

        public virtual void AddFile(string filePath)
        {
            throw new NotImplementedException();
        }

        protected UTF8Encoding _utf8NoBOMEncoding = new UTF8Encoding(false);
        protected virtual void Write(String s, MemoryStream stream)
        {
            byte[] newText = _utf8NoBOMEncoding.GetBytes(s);
            stream.Write(newText, 0, newText.Length);
        }

        public virtual HttpWebRequest SendRequest(CancelableStream stream)
        {
            _request.ContentLength = _requestBodyTop.Length + stream.Length + _requestBodyBottom.Length;
            _request.AllowWriteStreamBuffering = false;
            _requestStream = _request.GetRequestStream();
            _requestStream.Write(_requestBodyTop.ToArray(), 0, (int)_requestBodyTop.Length);
            StreamHelper.Transfer(stream, _requestStream, 8192, true);
            _requestStream.Write(_requestBodyBottom.ToArray(), 0, (int)_requestBodyBottom.Length);
            _requestStream.Close();
            return _request;
        }
    }
}
