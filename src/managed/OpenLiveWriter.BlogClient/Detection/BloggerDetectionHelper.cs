// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Text.RegularExpressions;
using OpenLiveWriter.HtmlParser.Parser;

namespace OpenLiveWriter.BlogClient.Detection
{
    internal class BloggerDetectionHelper
    {
        private readonly string homepageUrl;
        private readonly string html;

        public BloggerDetectionHelper(string homepageUrl, string html)
        {
            this.homepageUrl = homepageUrl;
            this.html = html;
        }

        public bool IsBlogger()
        {
            if (Regex.IsMatch(homepageUrl, @"^http://.+\.blogspot\.com($|/)", RegexOptions.IgnoreCase)
                || Regex.IsMatch(homepageUrl, @"^http(s)?://(www\.)?blogger\.com($|/)", RegexOptions.IgnoreCase)
                || new HtmlExtractor(html).Seek(new BloggerGeneratorCriterion()).Success)
            {
                return true;
            }

            HtmlExtractor ex = new HtmlExtractor(html);
            while (ex.Seek("<link href rel='service.post' type='application/atom+xml'>").Success)
            {
                BeginTag bt = (BeginTag)ex.Element;
                string atomHref = bt.GetAttributeValue("href");

                // these obsolete Blogger atom links can't be used, but are
                // still a good indication that it's Blogger
                if (atomHref.StartsWith("https://www.blogger.com/atom/", StringComparison.OrdinalIgnoreCase))
                    return true;

                // any other blogger or blogspot atom link will be considered a match
                if (Regex.IsMatch(atomHref, @"^https?\:\/\/.+\.blog(ger|spot)\.com\/.*", RegexOptions.IgnoreCase))
                    return true;
            }

            return false;
        }

        private class BloggerGeneratorCriterion : IElementPredicate
        {
            public bool IsMatch(Element e)
            {
                BeginTag tag = e as BeginTag;
                if (tag == null)
                    return false;

                if (!tag.NameEquals("meta"))
                    return false;

                if (tag.GetAttributeValue("name") != "generator")
                    return false;

                string generator = tag.GetAttributeValue("content");
                if (generator == null || CaseInsensitiveComparer.DefaultInvariant.Compare("blogger", generator) != 0)
                    return false;

                return true;
            }
        }
    }
}
