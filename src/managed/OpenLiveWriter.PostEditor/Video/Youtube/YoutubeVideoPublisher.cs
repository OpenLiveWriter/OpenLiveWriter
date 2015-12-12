// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using OpenLiveWriter.Localization;
using OpenLiveWriter.CoreServices;
using System.Xml;
using OpenLiveWriter.PostEditor.ContentSources.Common;

namespace OpenLiveWriter.PostEditor.Video.YouTube
{
    internal class YouTubeVideoPublisher : IVideoPublisher
    {
        public const string CLIENT_CODE = "";
        public const string DEVELOPER_KEY = "";

        private static VideoProvider _youTubeVideoProvider;

        public YouTubeVideoPublisher()
        {
            if (_youTubeVideoProvider == null)
            {
                foreach (VideoProvider videoProvider in VideoProviderHelper.VideoProviders)
                {
                    if (videoProvider.IsYouTube)
                    {
                        _youTubeVideoProvider = videoProvider;
                        return;
                    }
                }
            }
        }

        public IStatusWatcher Publish(string title, string description, string tags, string categoryId, string categoryString, string permissionValue, string permissionString, string path)
        {
            YouTubeVideoUploader uploader = new YouTubeVideoUploader(path, YouTubeAuth.Instance.AuthToken, title, description, tags, categoryId, permissionValue, _youTubeVideoProvider.UrlAtomPattern);
            uploader.Start();
            return uploader;
        }

        public string Id
        {
            get
            {
                return ID;
            }
        }

        public const string ID = "A5D392D0-9D77-4212-A823-E1FFBBD9A71C";

        public string ServiceName
        {
            get
            {
                return Res.Get(StringId.Plugin_Video_Youtube_Publish_Name);
            }
        }

        public override string ToString()
        {
            return ServiceName;
        }

        public string FormatTags(string rawTags)
        {
            return StringHelper.Join(rawTags.Trim().Split(new char[] { ',', ' ' }), ", ", true);
        }

        public Video GetVideo(string title, string description, string tags, string categoryId, string categoryString, string permissionValue, string permissionString)
        {
            // Make the video by the ID that we got back form soapbox
            Video video = _youTubeVideoProvider.CreateBlankVideo();

            video.Permission = permissionString;

            // This lets the editor know that it should track the progress by the status watcher
            video.IsUploading = true;

            return video;
        }

        private List<CategoryItem> _categories;
        public List<CategoryItem> Categories
        {
            get
            {
                if (_categories == null)
                {
                    _categories = new List<CategoryItem>();
                    _categories.Add(new CategoryItem("Film", Res.Get(StringId.Plugin_Video_Publish_Categories_Film)));
                    _categories.Add(new CategoryItem("Autos", Res.Get(StringId.Plugin_Video_Publish_Categories_Autos)));
                    _categories.Add(new CategoryItem("Music", Res.Get(StringId.Plugin_Video_Publish_Categories_Music)));
                    _categories.Add(new CategoryItem("Animals", Res.Get(StringId.Plugin_Video_Publish_Categories_Animals)));
                    _categories.Add(new CategoryItem("Sports", Res.Get(StringId.Plugin_Video_Publish_Categories_Sports)));
                    _categories.Add(new CategoryItem("Travel", Res.Get(StringId.Plugin_Video_Publish_Categories_Travel)));
                    _categories.Add(new CategoryItem("People", Res.Get(StringId.Plugin_Video_Publish_Categories_Blogs)));
                    _categories.Add(new CategoryItem("Games", Res.Get(StringId.Plugin_Video_Publish_Categories_Games)));
                    _categories.Add(new CategoryItem("Comedy", Res.Get(StringId.Plugin_Video_Publish_Categories_Comedy)));
                    _categories.Add(new CategoryItem("People", Res.Get(StringId.Plugin_Video_Publish_Categories_People)));
                    _categories.Add(new CategoryItem("News", Res.Get(StringId.Plugin_Video_Publish_Categories_News)));
                    _categories.Add(new CategoryItem("Entertainment", Res.Get(StringId.Plugin_Video_Publish_Categories_Entertainment)));
                    _categories.Add(new CategoryItem("Education", Res.Get(StringId.Plugin_Video_Publish_Categories_Education)));
                    _categories.Add(new CategoryItem("Howto", Res.Get(StringId.Plugin_Video_Publish_Categories_How_To)));
                    _categories.Add(new CategoryItem("Nonprofit", Res.Get(StringId.Plugin_Video_Publish_Categories_Non_Profit)));
                    _categories.Add(new CategoryItem("Tech", Res.Get(StringId.Plugin_Video_Publish_Categories_Technology)));
                    _categories.Sort();
                }

                return _categories;
            }
        }

        public string FileFilter
        {
            get
            {
                // There was a typo in the file filter where it was ,*.mov instead of ;*.mov, so we replace it at runtime
                // This should be removed after we branch for M2
                string filter = Res.Get(StringId.Plugin_Video_YouTube_Publish_Video_File_Open_Filter_Ext);
                string[] parts = StringHelper.Split(filter, "|");
                parts[1] = "*.avi;*.wmv;*.mpg;*.mpeg;*.mp4;*.mpeg4;*.mov;";
                return StringHelper.Join(parts, "|");
            }
        }

        public string AcceptanceText
        {
            get
            {
                return Res.Get(StringId.Plugin_Video_YouTube_Publish_Terms_Agree);
            }
        }

        public string AcceptanceUrl
        {
            get
            {
                return GLink.Instance.YouTubeTermOfUse;
            }
        }

        public string SafetyTipUrl
        {
            get
            {
                return GLink.Instance.YouTubeSafety;
            }
        }

        public IStatusWatcher CreateStatusWatcher(Video video)
        {
            if (YouTubeAuth.Instance.IsLoggedIn)
            {
                YouTubeVideoUploader uploader = new YouTubeVideoUploader(YouTubeAuth.Instance.Username, YouTubeAuth.Instance.AuthToken, video.Id, _youTubeVideoProvider.UrlAtomFormat);
                uploader.Start();
                return uploader;
            }
            return null;
        }

        public void Init(MediaSmartContent media, IWin32Window DialogOwner, string blogId)
        {

        }

        public string AcceptanceTitle
        {
            get
            {
                return Res.Get(StringId.Plugin_Video_Publish_Terms_View);
            }
        }

        public string SafetyTipTitle
        {
            get
            {
                return Res.Get(StringId.Plugin_Video_Publish_Safety_View);
            }
        }

        public IAuth Auth
        {
            get
            {
                return YouTubeAuth.Instance;
            }
        }

        public Bitmap Image
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public void Dispose()
        {

        }

    }

    internal class YouTubeVideoUploader : AsyncOperation, IStatusWatcher
    {
        private readonly string _filePath;
        private readonly string _authToken;
        private readonly string _title;
        private readonly string _description;
        private readonly string _tags;
        private readonly string _categoryId;
        private readonly string _permissionValue;
        private readonly string _urlAtomPattern;

        // this is the url to the feed which contains the status for the video
        private volatile string _updateUrl;

        private volatile string _message;
        private volatile VideoPublishStatus _status;
        private volatile string _id;

        private volatile CancelableStream _stream;

        public YouTubeVideoUploader(string path, string authToken, string title, string description, string tags, string categoryId, string permissionValue, string urlAtomPattern)
            : base(null)
        {
            _filePath = path;
            _authToken = authToken;
            _title = title;
            _description = description;
            _tags = tags;
            _categoryId = categoryId;
            _permissionValue = permissionValue;
            _urlAtomPattern = urlAtomPattern;
        }

        public YouTubeVideoUploader(string username, string authToken, string videoId, string urlAtomFormat)
            : base(null)
        {
            _filePath = String.Empty;
            _authToken = authToken;
            _title = String.Empty;
            _description = String.Empty;
            _tags = String.Empty;
            _categoryId = String.Empty;
            _permissionValue = String.Empty;

            _updateUrl = urlAtomFormat.Replace("{user}", username).Replace("{videoId", videoId);
        }

        public PublishStatus Status
        {
            get
            {
                return new PublishStatus(_status, _message, _id);
            }
        }

        public bool IsCancelable
        {
            get
            {
                return _status != VideoPublishStatus.Completed && YouTubeAuth.Instance.IsLoggedIn;
            }
        }

        public void CancelPublish()
        {
            Cancel();

            try
            {
                if (_stream != null)
                {
                    _stream.Cancel();
                    _stream.Dispose();
                    _stream = null;
                }
            }
            catch (NullReferenceException)
            {

            }

            if (!string.IsNullOrEmpty(_updateUrl))
            {
                HttpWebRequest req = HttpRequestHelper.CreateHttpWebRequest(_updateUrl, true, -1, -1);
                YouTubeUploadRequestHelper.AddSimpleHeader(req, _authToken);
                req.Method = "DELETE";
                req.GetResponse().Close();
            }

        }

        public void Dispose()
        {
            Debug.Assert(_stream == null, "Failed to close file stream for YouTubeVideoPublisher.");
        }

        private string Upload()
        {
            HttpWebRequest req = HttpRequestHelper.CreateHttpWebRequest("http://uploads.gdata.youtube.com/feeds/api/users/default/uploads", true, -1, -1);

            YouTubeUploadRequestHelper uploader = new YouTubeUploadRequestHelper(req);
            uploader.AddHeader(_authToken, Path.GetFileName(Path.GetFileName(_filePath)));
            uploader.Open();
            uploader.AddXmlRequest(_title, _description, _tags, _categoryId, _permissionValue);
            uploader.AddFile(_filePath);
            uploader.Close();

            try
            {
                using (_stream = new CancelableStream(new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    uploader.SendRequest(_stream);
                }
            }
            finally
            {
                _stream = null;
            }

            string result;
            using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
            {
                using (StreamReader responseReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    result = responseReader.ReadToEnd();
                }
            }

            return result;
        }

        private XmlNamespaceManager _nsMgr;
        private XmlNamespaceManager NamespaceManager
        {
            get
            {
                if (_nsMgr == null)
                {
                    _nsMgr = new XmlNamespaceManager(new NameTable());

                    _nsMgr.AddNamespace("atom", "http://www.w3.org/2005/Atom");
                    _nsMgr.AddNamespace("gml", "http://www.opengis.net/gml");
                    _nsMgr.AddNamespace("georss", "http://www.w3.org/2005/Atom");
                    _nsMgr.AddNamespace("media", "http://search.yahoo.com/mrss/");
                    _nsMgr.AddNamespace("yt", "http://gdata.youtube.com/schemas/2007");
                    _nsMgr.AddNamespace("gd", "http://schemas.google.com/g/2005");
                    _nsMgr.AddNamespace("app", "http://purl.org/atom/app#");
                }

                return _nsMgr;
            }
        }

        protected override void DoWork()
        {
            _status = VideoPublishStatus.LocalProcessing;
            _message = Res.Get(StringId.VideoLoading);

            try
            {
                if (string.IsNullOrEmpty(_updateUrl))
                {

                    _message = Res.Get("Video" + _status);
                    string result;
                    try
                    {
                        result = Upload();
                    }
                    catch (WebException ex)
                    {
                        HandleWebException(ex);
                        return;
                    }
                    catch (Exception ex)
                    {
                        HandleException(ex);
                        return;
                    }

                    // Get the ID for this video
                    _id = ExtractIdFromResult(result);
                }

                _status = VideoPublishStatus.RemoteProcessing;
                _message = String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.VideoRemoteProcessing), Res.Get(StringId.Plugin_Video_Youtube_Publish_Name));

                bool allow404 = true;

                // Check to see if this video has been canceled already or is completed
                while (_status != VideoPublishStatus.Completed && !CancelRequested)
                {
                    try
                    {
                        // Check to see if the video has finished
                        if (IsVideoComepleted())
                        {
                            _status = VideoPublishStatus.Completed;
                            _message = Res.Get("Video" + _status);
                            return;
                        }
                    }
                    catch (WebException ex)
                    {
                        HttpWebResponse response = ex.Response as HttpWebResponse;
                        if (allow404 && response != null && response.StatusCode == HttpStatusCode.NotFound)
                        {
                            allow404 = false;
                        }
                        else
                        {
                            HandleWebException(ex);
                            return;
                        }
                    }

                    Thread.Sleep(30000);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void HandleException(Exception ex)
        {
            Trace.WriteLine("Failed to upload video: " + ex);
            _status = VideoPublishStatus.Error;

            _message = Res.Get(StringId.YouTubeVideoError) + Environment.NewLine + Res.Get(StringId.VideoErrorTryAgain);
        }

        private void HandleWebException(WebException ex)
        {
            HttpRequestHelper.LogException(ex);
            _status = VideoPublishStatus.Error;
            if (ex.Status != WebExceptionStatus.ProtocolError)
                _message = Res.Get(StringId.VideoNetworkError) + Environment.NewLine + Res.Get(StringId.VideoErrorTryAgain);
            else
                _message = Res.Get(StringId.YouTubeVideoError) + Environment.NewLine + Res.Get(StringId.VideoErrorTryAgain);
        }

        private bool IsVideoComepleted()
        {
            // Send a request to get the status
            HttpWebRequest req = HttpRequestHelper.CreateHttpWebRequest(_updateUrl, true, -1, -1);
            YouTubeUploadRequestHelper.AddSimpleHeader(req, _authToken);
            string innerResult;
            using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
            {
                using (StreamReader responseReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    innerResult = responseReader.ReadToEnd();
                }
            }

            // Find the status elemnt
            XmlDocument xmlDoc = new XmlDocument(NamespaceManager.NameTable);
            xmlDoc.LoadXml(innerResult);
            XmlNode stateNode = xmlDoc.SelectSingleNode("//atom:entry/app:control/yt:state", NamespaceManager);

            // If there is no element with status, then the video is completed
            if (stateNode == null)
            {
                _status = VideoPublishStatus.Completed;
                _message = Res.Get("Video" + _status);
                return true;
            }

            // Check to make sure the video doesnt have a bad status
            string value = stateNode.Attributes["name"] != null ? stateNode.Attributes["name"].Value : "";
            if (value == "rejected" || value == "failed")
            {
                throw new Exception(Res.Get("YouTube" + stateNode.Attributes["reasonCode"].Value));
            }

            return false;
        }

        private string ExtractIdFromResult(string result)
        {
            // Make sure we go some kind of result to work with
            if (string.IsNullOrEmpty(result))
            {
                Trace.WriteLine("No result from YouTube upload.");
                throw new Exception(Res.Get(StringId.YouTubeInvalidResult));
            }

            // Find the element that has a rel of self and tagname of link
            XmlDocument xmlDoc = new XmlDocument(NamespaceManager.NameTable);
            xmlDoc.LoadXml(result);
            XmlNode selfNode = xmlDoc.SelectSingleNode("/atom:entry/atom:link[@rel='self']", NamespaceManager);

            if (selfNode == null)
            {
                Trace.WriteLine("No rel=self in result from YouTube upload. Response: " + result);
                throw new Exception(Res.Get(StringId.YouTubeInvalidResult));
            }

            // Set this as the url we can use to get status updates
            _updateUrl = XmlHelper.GetUrl(selfNode, "@href", new Uri("http://uploads.gdata.youtube.com/feeds/api/users/default/uploads"));
            if (string.IsNullOrEmpty(_updateUrl))
            {
                Trace.WriteLine("Could not get url for status updates. Response: " + result);
                throw new Exception(Res.Get(StringId.YouTubeInvalidResult));
            }

            // Read the ID from the url
            Match m = Regex.Match(_updateUrl, _urlAtomPattern);

            if (!m.Success)
                throw new Exception(Res.Get(StringId.YouTubeInvalidResult));

            return m.Groups["id"].Value;
        }
    }

    public class YouTubeUploadRequestHelper
    {
        private readonly string _boundary;
        private readonly HttpWebRequest _request;
        Stream _requestStream;
        private MemoryStream _requestBodyTop = new MemoryStream();
        private MemoryStream _requestBodyBottom = new MemoryStream();

        private UTF8Encoding _utf8NoBOMEncoding = new UTF8Encoding(false);

        internal YouTubeUploadRequestHelper(HttpWebRequest request)
        {
            _boundary = "--------------------------" + Guid.NewGuid().ToString().Replace("-", "");
            _request = request;
            _request.Method = "POST";
            _request.ContentType = "multipart/related; boundary=\"" + _boundary + "\"";
        }

        internal void AddHeader(string authToken, string path)
        {
            AddSimpleHeader(_request, authToken);
            _request.Headers.Add("Slug", path);
        }

        internal static void AddSimpleHeader(HttpWebRequest request, string authToken)
        {
            request.Headers.Add("Authorization", authToken);
            //request.Headers.Add("X-GData-Client", YouTubeVideoPublisher.CLIENT_CODE);
            request.Headers.Add("X-GData-Key", "key=" + YouTubeVideoPublisher.DEVELOPER_KEY);
        }

        internal void Open()
        {
            AddBoundary(true, _requestBodyTop);
        }

        internal void Close()
        {
            AddBoundary(false, _requestBodyBottom);
            Write("--" + Environment.NewLine, _requestBodyBottom);
        }

        internal void AddBoundary(bool newLine, MemoryStream stream)
        {
            Write("--" + _boundary + (newLine ? Environment.NewLine : ""), stream);
        }

        internal void AddXmlRequest(string title, string description, string keywords, string category, string permission)
        {
            Write("Content-Type: application/atom+xml; charset=UTF-8" + Environment.NewLine + Environment.NewLine, _requestBodyTop);

            MemoryStream xmlMemoryStream = new MemoryStream();
            XmlTextWriter xmlWriter = new XmlTextWriter(xmlMemoryStream, Encoding.UTF8);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("entry", "http://www.w3.org/2005/Atom");
            xmlWriter.WriteStartElement("media", "group", "http://search.yahoo.com/mrss/");

            xmlWriter.WriteStartElement("media", "title", "http://search.yahoo.com/mrss/");
            xmlWriter.WriteAttributeString("type", "plain");
            xmlWriter.WriteString(title);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("media", "description", "http://search.yahoo.com/mrss/");
            xmlWriter.WriteAttributeString("type", "plain");
            xmlWriter.WriteString(description);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("media", "category", "http://search.yahoo.com/mrss/");
            xmlWriter.WriteAttributeString("scheme", "http://gdata.youtube.com/schemas/2007/categories.cat");
            xmlWriter.WriteString(category);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("media", "keywords", "http://search.yahoo.com/mrss/");
            xmlWriter.WriteString(keywords);
            xmlWriter.WriteEndElement();

            if (permission == "1")
            {
                xmlWriter.WriteStartElement("yt", "private", "http://gdata.youtube.com/schemas/2007");
                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();

            xmlWriter.Flush();
            xmlMemoryStream.Position = 0;

            StreamReader sr = new StreamReader(xmlMemoryStream);
            string newXML = sr.ReadToEnd();
            Write(newXML + Environment.NewLine, _requestBodyTop);
            AddBoundary(true, _requestBodyTop);
        }

        internal void AddFile(string filePath)
        {
            Write("Content-Type: " + MimeHelper.GetContentType(Path.GetExtension(filePath)) + Environment.NewLine, _requestBodyTop);
            Write("Content-Transfer-Encoding: binary" + Environment.NewLine + Environment.NewLine, _requestBodyTop);
            Write(Environment.NewLine, _requestBodyBottom);
        }

        private void Write(String s, MemoryStream stream)
        {
            byte[] newText = _utf8NoBOMEncoding.GetBytes(s);
            stream.Write(newText, 0, newText.Length);
        }

        internal void SendRequest(CancelableStream stream)
        {
            _request.ContentLength = _requestBodyTop.Length + stream.Length + _requestBodyBottom.Length;
            _request.AllowWriteStreamBuffering = false;
            _requestStream = _request.GetRequestStream();
            _requestStream.Write(_requestBodyTop.ToArray(), 0, (int)_requestBodyTop.Length);
            StreamHelper.Transfer(stream, _requestStream, 8192, true);
            _requestStream.Write(_requestBodyBottom.ToArray(), 0, (int)_requestBodyBottom.Length);
            _requestStream.Close();
        }
    }
}
