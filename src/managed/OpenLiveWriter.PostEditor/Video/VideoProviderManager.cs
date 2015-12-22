// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Video
{
    /// <summary>
    /// Summary description for VideoProviderManager.
    /// </summary>
    public class VideoProviderManager
    {
        public const string WIDTH = "{width}";
        public const string HEIGHT = "{height}";

        public static VideoProvider FindProviderFromUrl(string urlInput)
        {
            VideoProvider[] providers = VideoProviderHelper.VideoProviders;
            foreach (VideoProvider provider in providers)
            {
                if (provider.MatchesUrl(urlInput))
                    return provider;
            }
            return null;
        }

        public static VideoProvider FindProviderFromEmbed(string embedInput)
        {
            VideoProvider[] providers = VideoProviderHelper.VideoProviders;
            foreach (VideoProvider provider in providers)
            {
                if (provider.MatchesEmbed(embedInput))
                    return provider;
            }
            return null;
        }

        public static VideoProvider FindProviderFromProviderId(string id)
        {
            VideoProvider[] providers = VideoProviderHelper.VideoProviders;
            foreach (VideoProvider provider in providers)
            {
                if (String.Compare(provider.ServiceId, id, true, CultureInfo.InvariantCulture) == 0)
                    return provider;
            }
            return null;
        }

        private static bool ContainsEmbed(string input)
        {
            IElementPredicate predicate = new OrPredicate(new BeginTagPredicate("embed"), new BeginTagPredicate("object"));
            HtmlExtractor ex = new HtmlExtractor(input);
            return ex.Seek(predicate).Success;
        }

        private static Size ParseEmbed(string embedFormat, int defaultWidth, int defaultHeight, out string newEmbed)
        {
            Size size = new Size(defaultWidth, defaultHeight);
            newEmbed = embedFormat;
            //width="xx" or width='xx' or width=xx
            string widthPattern = "width=('|\")?(?<width>[0-9]+)('|\")?";
            Match match = Regex.Match(newEmbed, widthPattern);
            while (match.Success)
            {
                Group group = match.Groups["width"];
                try
                {
                    size.Width = int.Parse(group.Value, CultureInfo.InvariantCulture);
                }
                catch
                {
                }
                int start = group.Index;
                int len = group.Length;
                newEmbed = newEmbed.Remove(start, len);
                newEmbed = newEmbed.Insert(start, WIDTH);
                match = Regex.Match(newEmbed, widthPattern);
            }
            //height="xx";
            string heightPattern = "height=('|\")?(?<height>[0-9]+)('|\")?";
            match = Regex.Match(newEmbed, heightPattern);
            while (match.Success)
            {
                Group group = match.Groups["height"];
                try
                {
                    size.Height = int.Parse(group.Value, CultureInfo.InvariantCulture);
                }
                catch
                {
                }
                int start = group.Index;
                int len = group.Length;
                newEmbed = newEmbed.Remove(start, len);
                newEmbed = newEmbed.Insert(start, HEIGHT);
                match = Regex.Match(newEmbed, heightPattern);
            }
            return size;
        }

        internal static Video FindVideo(string input)
        {
            VideoProvider provider;
            //try URL first
            provider = FindProviderFromUrl(input);
            if (provider != null)
            {
                if (provider.UrlConvertError)
                    throw new VideoUrlConvertException();

                return provider.VideoFromUrl(input);
            }

            //if no match, try embed tag
            provider = FindProviderFromEmbed(input);
            if (provider != null)
                return provider.VideoFromEmbed(input);

            //special case--show the embed, but we can't do much else (linking wise)
            if (ContainsEmbed(input))
            {
                string embed;
                Size videoSize = ParseEmbed(input, 425, 350, out embed);
                return new Video(Guid.NewGuid().ToString(),
                                 String.Empty,
                                 embed,
                                 embed,
                                 null,
                                 videoSize.Width,
                                 videoSize.Height,
                                 VideoAspectRatioType.Unknown);
            }
            return null;
        }

        public static Video CreateSoapboxVideoFromId(string id)
        {
            VideoProvider[] providers = VideoProviderHelper.VideoProviders;
            foreach (VideoProvider provider in providers)
            {
                if (provider.IsSoapbox)
                    return provider.VideoFromId(id);
            }
            return null;
        }

        public static string SoapboxAppId
        {
            get
            {
                VideoProvider[] providers = VideoProviderHelper.VideoProviders;
                foreach (VideoProvider provider in providers)
                {
                    if (provider.IsSoapbox)
                        return provider.AppId;
                }
                return String.Empty;
            }
        }

        public static bool CheckForWhitelist(string blogProviderId, string videoProviderId, string videoId, Size videoSize, out string output)
        {
            //if the provider name has a whitelist...
            WhiteList[] whiteLists = VideoProviderHelper.WhiteLists;
            foreach (WhiteList list in whiteLists)
            {
                //and the video provider is on the whitelist...
                if (list.MatchesBlogProviderId(blogProviderId))
                {
                    //change the embed to be that special one
                    string pattern = list.HasPattern(videoProviderId);
                    if (pattern != null && pattern != String.Empty)
                    {
                        pattern = pattern.Replace("{id}", "{0}");
                        pattern = pattern.Replace("{width}", videoSize.Width.ToString(CultureInfo.InvariantCulture));
                        pattern = pattern.Replace("{height}", videoSize.Height.ToString(CultureInfo.InvariantCulture));
                        output = String.Format(CultureInfo.InvariantCulture, pattern, videoId);
                        return true;
                    }
                }
            }
            output = String.Empty;
            return false;
        }

    }

    public class WhiteList
    {
        private string _blogProviderId;
        private Hashtable _mappings;

        public WhiteList(string providerName, Hashtable mappings)
        {
            _blogProviderId = providerName;
            _mappings = mappings;
        }

        public bool MatchesBlogProviderId(string testBlogProviderId)
        {
            if (String.Compare(_blogProviderId, testBlogProviderId, true, CultureInfo.InvariantCulture) == 0)
                return true;
            return false;
        }

        public string HasPattern(string videoProvider)
        {
            if (_mappings.ContainsKey(videoProvider))
                return (string)_mappings[videoProvider];
            return null;
        }
    }

    public class VideoUrlConvertException : Exception
    {

    }
}
