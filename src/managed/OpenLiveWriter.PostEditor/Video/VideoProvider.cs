// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Progress;
using OpenLiveWriter.HtmlParser.Parser;
using System.Diagnostics;
using System.Text;

namespace OpenLiveWriter.PostEditor.Video
{
    /// <summary>
    /// Summary description for VideoProvider.
    /// </summary>
    public class VideoProvider
    {
        public const string VIDEOID = "{videoId}";

        private string _serviceName;
        private string _serviceId;
        private string _embedFormat;
        private string _urlFormat;
        private string _urlPattern;
        private EmbedPattern[] _embedPatterns;
        private int _width;
        private int _height;
        private string _useBackgroundColor;
        private string _appId;
        private String[] _publishPingUrls;
        private string _editorFormat;
        private bool _urlConvertError;
        private string _urlAtomPattern;
        private string _urlAtomFormat;

        public VideoProvider(string serviceName, string serviceId, string embedFormat, string editorFormat, string urlFormat, EmbedPattern[] embedPatterns,
            string urlPattern, bool urlConvertError, int width, int height, string useBackgroundColor, string appId, String[] publishPingUrls, string urlAtomPattern, string urlAtomFormat)
        {
            _serviceName = serviceName;
            _serviceId = serviceId;
            _embedFormat = embedFormat;
            _urlFormat = urlFormat;
            _urlPattern = urlPattern;
            _embedPatterns = embedPatterns;
            _width = width;
            _height = height;
            _useBackgroundColor = useBackgroundColor;
            _appId = appId;
            _publishPingUrls = publishPingUrls;
            _urlAtomPattern = urlAtomPattern;
            _urlAtomFormat = urlAtomFormat;

            _urlConvertError = urlConvertError;

            if (!String.IsNullOrEmpty(editorFormat))
                _editorFormat = editorFormat;
            else
                _editorFormat = embedFormat;
        }

        public string UrlAtomPattern
        { get { return _urlAtomPattern; } }

        public string UrlAtomFormat
        { get { return _urlAtomFormat; } }

        public bool UrlConvertError
        { get { return _urlConvertError; } }

        public string ServiceName
        { get { return _serviceName; } }

        public string ServiceId
        { get { return _serviceId; } }

        public string AppId
        { get { return _appId; } }

        public string UseBackgroundColor
        { get { return _useBackgroundColor; } }

        public string EditorFormat
        { get { return _editorFormat; } }

        public String[] PublishPingUrls
        { get { return _publishPingUrls; } }

        public bool IsSoapbox
        {
            get
            {
                return String.Compare(_serviceId, "A64F0391-DC63-46cc-B106-D1E6B4EFA9EB", true, CultureInfo.CurrentCulture) == 0;
            }
        }

        public bool IsYouTube
        {
            get
            {
                return String.Compare(_serviceId, "56744E0A-1892-4b56-B7FF-3A2568EE2D27", true, CultureInfo.CurrentCulture) == 0;
            }
        }

        public bool MatchesUrl(string input)
        {
            return Regex.IsMatch(input, _urlPattern, RegexOptions.IgnoreCase);
        }

        private string MakeUrl(string id)
        {
            return _urlFormat.Replace(VIDEOID, id);
        }

        private string IdFromUrl(string input)
        {
            Match m = Regex.Match(input, _urlPattern, RegexOptions.IgnoreCase);
            if (m.Success)
            {
                return m.Groups["id"].Value;
            }
            return String.Empty;
        }

        public bool MatchesEmbed(string input)
        {
            foreach (EmbedPattern check in _embedPatterns)
            {
                IElementPredicate predicate = new BeginTagPredicate("embed", new RequiredAttribute[] { new RequiredAttribute(check.Attr) });
                HtmlExtractor ex = new HtmlExtractor(input);
                ex = ex.Seek(predicate);
                if (ex.Success)
                {
                    BeginTag bt = ex.Element as BeginTag;
                    string srcRef = bt.GetAttributeValue(check.Attr);
                    if (!Regex.IsMatch(srcRef, check.Pattern, RegexOptions.IgnoreCase))
                    {
                        return false;
                    }
                }
                else
                {
                    return false; //didn't find embed tag with the attr
                }
            }
            return true; //found all predicates
        }

        public static string GenerateEmbedHtml(string embedFormat, string id, Size size)
        {
            string pattern = embedFormat.Replace(VIDEOID, id).Replace(VideoProviderManager.WIDTH, "{0}").Replace(VideoProviderManager.HEIGHT, "{1}");
            return String.Format(CultureInfo.InvariantCulture, pattern, size.Width, size.Height);
        }

        private string IdFromEmbed(string input)
        {
            foreach (EmbedPattern check in _embedPatterns)
            {
                IElementPredicate predicate = new BeginTagPredicate("embed", new RequiredAttribute[] { new RequiredAttribute(check.Attr) });
                HtmlExtractor ex = new HtmlExtractor(input);
                ex = ex.Seek(predicate);
                if (ex.Success)
                {
                    BeginTag bt = ex.Element as BeginTag;
                    string srcRef = bt.GetAttributeValue(check.Attr);
                    Match m = Regex.Match(srcRef, check.Pattern, RegexOptions.IgnoreCase);
                    if (m.Success && m.Groups["id"].Success)
                    {
                        return m.Groups["id"].Value;
                    }
                }
            }
            return String.Empty;
        }

        public Video VideoFromUrl(string input)
        {
            string id = IdFromUrl(input);
            if (id == String.Empty)
                return null;
            return new Video(id,
                             input,
                             _embedFormat,
                             _editorFormat,
                             this,
                             _width,
                             _height,
                             VideoAspectRatioType.Widescreen);
        }

        public Video VideoFromEmbed(string input)
        {
            string id = IdFromEmbed(input);
            if (id == String.Empty)
                return null;
            string url = MakeUrl(id);

            Size inputSize = FindSizeAttribute(input);

            return new Video(id,
                 url,
                 _embedFormat,
                 _editorFormat,
                 this,
                 inputSize.Width,
                 inputSize.Height,
                 VideoAspectRatioType.Unknown);
        }

        Size FindSizeAttribute(string input)
        {
            Size size = new Size(_width, _height);

            if (string.IsNullOrEmpty(input))
                return size;

            try
            {
                RequiredAttribute[] attrWidth = new RequiredAttribute[] { new RequiredAttribute("width"), new RequiredAttribute("height") };
                IElementPredicate predicate = new OrPredicate(new BeginTagPredicate("embed", attrWidth), new BeginTagPredicate("object", attrWidth));
                HtmlExtractor ex = new HtmlExtractor(input);
                if (ex.Seek(predicate).Success)
                {
                    BeginTag tag = (BeginTag)ex.Element;
                    size = new Size(Convert.ToInt32(tag.GetAttributeValue("width"), CultureInfo.InvariantCulture), Convert.ToInt32(tag.GetAttributeValue("height"), CultureInfo.InvariantCulture));

                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Exception thrown while trying to find video size: " + ex);
            }

            return size;
        }

        public Video VideoFromId(string id)
        {
            if (id == String.Empty)
                return null;
            string url = MakeUrl(id);
            return new Video(id,
                             url,
                             _embedFormat,
                             _editorFormat,
                             this,
                             _width,
                             _height,
                             VideoAspectRatioType.Widescreen);
        }

        public Video CreateBlankVideo()
        {
            return new Video(null,
                 _urlFormat,
                 _embedFormat,
                 _editorFormat,
                 this,
                 _width,
                 _height,
                 VideoAspectRatioType.Unknown);
        }
    }

    public class EmbedPattern
    {
        private string _attr;
        private string _pattern;

        public EmbedPattern(string attr, string pattern)
        {
            _attr = attr;
            _pattern = pattern;
        }

        public string Attr { get { return _attr; } }
        public string Pattern { get { return _pattern; } }
    }
}
