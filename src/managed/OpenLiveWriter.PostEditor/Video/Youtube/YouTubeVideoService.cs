// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Xml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor.ContentSources.Common;
using OpenLiveWriter.PostEditor.Video.VideoService;

namespace OpenLiveWriter.PostEditor.Video.YouTube
{
    class YouTubeVideoService : IVideoService
    {

        static YouTubeVideoService()
        {
            _requestTypes = new VideoRequestType[]
                {
                    new MyVideosRequestType(),
                    new MyFavoritesRequestType()
                };
        }

        public YouTubeVideoService()
        {
            _videoAuth.LoginStatusChanged += new EventHandler(_videoAuth_LoginStatusChanged);
        }

        public static VideoRequestType[] SupportedRequests
        {
            get { return _requestTypes; }
        }

        public Bitmap Image
        {
            get { return _sidebarIcon; }
        }

        public string ServiceName
        {
            get { return Res.Get(StringId.Plugin_Video_Youtube_Publish_Name); }
        }

        public string ServiceUrl
        {
            get { return "http://www.youtube.com"; }
        }

        public IAuth Auth
        {
            get { return _videoAuth; }
        }

        IVideoRequestType[] IVideoService.SupportedRequests
        {
            get { return _requestTypes; }
        }

        public IVideo[] GetVideos(IVideoRequestType requestType, int timeoutMs, int maxPerPage, int page, out int videosAvailable)
        {
            VideoBuffer buffer = GetBuffer(requestType, timeoutMs, maxPerPage);

            // Insure we have enough videos in the Arraylist to satisfy the request
            int maxRequest = page * maxPerPage;
            buffer.FetchMore(maxRequest - buffer.Videos.Count);

            videosAvailable = buffer.Available;

            int startRequest = (page - 1) * maxPerPage;

            int count = maxPerPage;
            if (startRequest + maxPerPage > buffer.Videos.Count)
            {
                count = buffer.Videos.Count - startRequest;
            }

            // Deal with out of range
            return buffer.Videos.GetRange(startRequest, count).ToArray();
        }
        private readonly Hashtable _ytVideoBuffers = new Hashtable();

        private VideoBuffer GetBuffer(IVideoRequestType requestType, int timeoutMs, int pageSize)
        {
            if (!_ytVideoBuffers.ContainsKey(requestType))
            {
                VideoContext context = new VideoContext();
                _ytVideoBuffers[requestType] = new VideoBuffer(GetVideosInternal(requestType, timeoutMs, pageSize, context), context);
            }
            return _ytVideoBuffers[requestType] as VideoBuffer;
        }

        void _videoAuth_LoginStatusChanged(object sender, EventArgs e)
        {
            if (!_videoAuth.IsLoggedIn)
                _ytVideoBuffers.Clear();
        }

        private class VideoContext
        {
            public int Available
            {
                get
                {
                    return _available;
                }
                set
                {
                    _available = value;
                }
            }
            private int _available;

            public bool Full
            {
                get { return _full; }
                set { _full = value; }
            }

            private bool _full;

        }

        private class VideoBuffer
        {
            private readonly IEnumerator<IVideo> _videoSource;
            private readonly VideoContext _context;

            public VideoBuffer(IEnumerator<IVideo> videoSource, VideoContext context)
            {
                _videoSource = videoSource;
                _context = context;
            }

            public List<IVideo> Videos
            {
                get
                {
                    return _videos;
                }
            }

            private readonly List<IVideo> _videos = new List<IVideo>();

            public int Available
            {
                get
                {
                    if (_context.Full)
                        return _videos.Count;
                    else
                        return _context.Available;
                }
            }

            public void FetchMore(int numToFetch)
            {
                for (int i = 0; i < numToFetch; i++)
                {
                    if (!_videoSource.MoveNext())
                        break;
                    _videos.Add(_videoSource.Current);
                }
            }
        }

        private IEnumerator<IVideo> GetVideosInternal(IVideoRequestType requestType, int timeoutMs, int maxPerPage, VideoContext context)
        {
            string baseUrl;
            if (requestType is MyFavoritesRequestType)
            {
                // format the url for downloading
                baseUrl = String.Format(
                    CultureInfo.InvariantCulture,
                    "http://gdata.youtube.com/feeds/api/users/{0}/favorites",
                    _videoAuth.Username);
            }
            else if (requestType is MyVideosRequestType)
            {
                baseUrl = String.Format(
                    CultureInfo.InvariantCulture,
                    "http://gdata.youtube.com/feeds/api/users/{0}/uploads",
                    _videoAuth.Username);

            }
            else
            {
                throw new Exception("Unknown request type.");
            }

            int page = 1;
            while (true)
            {
                string queryString =
                    string.Format(CultureInfo.InvariantCulture, "?max-results={0}&start-index={1}", maxPerPage, ((page - 1) * maxPerPage + 1));
                string requestUrl = baseUrl + queryString;

                YouTubeVideo[] videos;
                int totalResults;

                // download the document
                Stream videoListStream = CallYouTubeApi(requestUrl, timeoutMs);

                // parse it into a list of videos
                videos = ParseVideoList(videoListStream, out totalResults);

                context.Available = totalResults;
                if (videos.Length == 0)
                {
                    context.Full = true;
                    yield break;
                }

                foreach (YouTubeVideo video in videos)
                    yield return video;

                page++;
            }
        }

        public void Dispose()
        {
            _videoAuth.LoginStatusChanged -= _videoAuth_LoginStatusChanged;
        }

        private static Stream CallYouTubeApi(string requestUrl, int timeoutMs)
        {
            // download the document
            Stream responseStream;
            try
            {
                HttpWebRequest req = HttpRequestHelper.CreateHttpWebRequest(requestUrl, true, timeoutMs, timeoutMs);
                YouTubeUploadRequestHelper.AddSimpleHeader(req, YouTubeAuth.Instance.AuthToken);
                using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
                {
                    using (Stream responseStreamOrginal = response.GetResponseStream())
                    {
                        responseStream = StreamHelper.CopyToMemoryStream(responseStreamOrginal);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new YouTubeException("YouTube Error", "Unable to download request from YouTube due to the following error: " + ex.Message);
            }

            // check for timeout
            if (responseStream == null)
                throw new YouTubeException("Request Timed Out", "The request to YouTube timed out (the service may be unavailable right now).");
            else
                return responseStream;
        }

        private static YouTubeVideo[] ParseVideoList(Stream videoListStream, out int totalResults)
        {
            totalResults = 0;
            ArrayList videos = new ArrayList();

            using (videoListStream)
            {

                XmlDocument document = new XmlDocument();

                XmlNamespaceManager mgr = new XmlNamespaceManager(document.NameTable);

                mgr.AddNamespace(
                "atom", "http://www.w3.org/2005/Atom");

                mgr.AddNamespace(
                "media", "http://search.yahoo.com/mrss/");

                mgr.AddNamespace(
                "yt", "http://gdata.youtube.com/schemas/2007");

                mgr.AddNamespace(
                "gd", "http://schemas.google.com/g/2005");

                mgr.AddNamespace(
                    "openSearch", "http://a9.com/-/spec/opensearchrss/1.0/");

                mgr.AddNamespace(
                    "app", "http://purl.org/atom/app#");

                document.Load(videoListStream);

                XmlNodeList nodes = document.SelectNodes("//atom:feed//atom:entry", mgr);

                XmlNode totalCount = document.SelectSingleNode("//atom:feed//openSearch:totalResults", mgr);
                if (totalCount != null)
                    int.TryParse(totalCount.InnerText, out totalResults);

                foreach (XmlNode node in nodes)
                {
                    YouTubeVideo ytVid = new YouTubeVideo();
                    ytVid.Load(node, mgr);
                    if (ytVid.IsPublished)
                        videos.Add(ytVid);
                }
            }

            return videos.ToArray(typeof(YouTubeVideo)) as YouTubeVideo[];
        }

        private static readonly VideoRequestType[] _requestTypes;
        private readonly Bitmap _sidebarIcon = ResourceHelper.LoadAssemblyResourceBitmap("Video.YouTube.Images.Sidebar.png");
        private readonly IAuth _videoAuth = YouTubeAuth.Instance;

        #region IMediaSource Members

        public string Id
        {
            get
            {
                return "D8124520-3499-46FB-8F6B-988455B09445";
            }
        }

        public void Init(MediaSmartContent media, IWin32Window DialogOwner, string blogId)
        {

        }

        #endregion
    }

    public class YouTubeVideo : IVideo
    {
        public void Load(XmlNode entryNode, XmlNamespaceManager mgr)
        {
            XmlElement stateNode = entryNode.SelectSingleNode("app:control/yt:state", mgr) as XmlElement;
            if (stateNode != null)
            {
                string name = stateNode.GetAttribute("name");
                if (name.ToLower(CultureInfo.InvariantCulture) == "rejected")
                    _isPublished = false;
            }

            _author = entryNode.SelectSingleNode(
            "atom:author/atom:name", mgr).InnerText;
            _id = entryNode.SelectSingleNode(
            "atom:id", mgr).InnerText;
            _title = entryNode.SelectSingleNode(
            "atom:title", mgr).InnerText;
            _uploadTime = SafeParseUploadTime(entryNode.SelectSingleNode(
            "atom:published", mgr).InnerText);
            _description = entryNode.SelectSingleNode(
            "atom:content", mgr).InnerText;
            XmlElement lengthElement = entryNode.SelectSingleNode("media:group/yt:duration", mgr) as XmlElement;
            if (lengthElement != null)
                _lengthSeconds = SafeParseInt(lengthElement.GetAttribute(
                                                  "seconds"));
            XmlElement ratingElement = entryNode.SelectSingleNode("gd:rating", mgr) as XmlElement;
            if (ratingElement != null)
            {
                _ratingAvg = SafeParseFloat(ratingElement.GetAttribute(
                                                "average"));
                _ratingCount = SafeParseInt(ratingElement.GetAttribute(
                                                "numRaters"));
            }
            XmlElement statsElement = entryNode.SelectSingleNode("yt:statistics", mgr) as XmlElement;
            if (statsElement != null)
                _viewCount = SafeParseInt(statsElement.GetAttribute(
                                              "viewCount"));
            XmlElement commentsFeedEl = entryNode.SelectSingleNode("gd:comments/gd:feedLink", mgr) as XmlElement;
            if (commentsFeedEl != null)
                _commentCount = SafeParseInt(commentsFeedEl.GetAttribute(
                "countHint"));
            XmlElement mediaPlayerEl = entryNode.SelectSingleNode("media:group/media:player", mgr) as XmlElement;
            if (mediaPlayerEl != null)
                _url = mediaPlayerEl.GetAttribute(
                    "url");

            XmlElement thumbnailEl = entryNode.SelectSingleNode("media:group/media:thumbnail", mgr) as XmlElement;
            if (thumbnailEl != null) _thumbnailUrl = thumbnailEl.GetAttribute("url");
        }
        public string Author { get { return _author; } }
        public string Id { get { return _id; } set { _id = value; } }
        public string Title { get { return _title; } }
        public int LengthSeconds { get { return _lengthSeconds; } }
        public float RatingAvg { get { return _ratingAvg; } }
        public int RatingCount { get { return _ratingCount; } }
        public string Description { get { return _description; } }
        public int ViewCount { get { return _viewCount; } }
        public DateTime UploadTime { get { return _uploadTime; } }
        public int CommentCount { get { return _commentCount; } }
        public string[] Tags { get { return _tags; } }
        public string Url { get { return _url; } set { _url = value; } }
        public string ThumbnailUrl { get { return _thumbnailUrl; } }
        public bool IsPublished { get { return _isPublished; } }

        public Video GetVideo()
        {
            VideoProvider provider = VideoProviderManager.FindProviderFromUrl(_url);
            return provider.VideoFromUrl(_url);
        }

        public override string ToString()
        {
            return Title;
        }
        public override bool Equals(object obj)
        {
            YouTubeVideo video = obj as YouTubeVideo;
            if (video == null)
                return false;
            return video.Id == Id;
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        private static DateTime SafeParseUploadTime(string content)
        {
            try
            {
                return DateTime.Parse(content, CultureInfo.InvariantCulture);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        private static int SafeParseInt(string input)
        {
            try
            {
                return int.Parse(input, CultureInfo.InvariantCulture);
            }
            catch
            {
                Trace.Fail("Unexpected malformed integer input: " + input);
                return 0;
            }
        }

        private static float SafeParseFloat(string input)
        {
            try
            {
                return float.Parse(input, CultureInfo.InvariantCulture);
            }
            catch
            {
                Trace.Fail("Unexpected malformed float input: " + input);
                return 0.0F;
            }
        }
        private string _author = String.Empty;
        private string _id = String.Empty;
        private string _title = String.Empty;
        private int _lengthSeconds = 0;
        private float _ratingAvg = 0.0F;
        private int _ratingCount = 0;
        private string _description = String.Empty;
        private int _viewCount = 0;
        private DateTime _uploadTime = DateTime.MinValue;
        private int _commentCount = 0;
        private string[] _tags = new string[] { };
        private string _url = String.Empty;
        private string _thumbnailUrl = String.Empty;
        private bool _isPublished = true;

    }

}
