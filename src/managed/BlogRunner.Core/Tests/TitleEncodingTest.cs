// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using OpenLiveWriter.Extensibility.BlogClient;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using OpenLiveWriter.HtmlParser.Parser;

namespace BlogRunner.Core.Tests
{
    public class TitleEncodingTest : PostTest
    {
        private const string TEST_STRING = "<b>&amp;&amp;amp;</b>";
        private string guid1, guid2;

        protected internal override void PreparePost(BlogPost blogPost, ref bool? publish)
        {
            guid1 = BlogUtil.ShortGuid;
            guid2 = BlogUtil.ShortGuid;

            blogPost.Title = guid1 + TEST_STRING + guid2;
            blogPost.Contents = "foo";
        }

        protected internal override void HandleResult(string homepageHtml, ITestResults results)
        {
            Regex regex = new Regex(Regex.Escape(guid1) + "(.*?)" + Regex.Escape(guid2));

            SimpleHtmlParser parser = new SimpleHtmlParser(homepageHtml);
            for (Element e = parser.Next(); e != null; e = parser.Next())
            {
                if (e is Text)
                {
                    Match m = regex.Match(e.ToString());
                    if (m.Success)
                    {
                        string str = m.Groups[1].Value;
                        if (str == HtmlUtils.EscapeEntities(TEST_STRING))
                            results.AddResult("requiresHtmlTitles", YES);
                        else if (str == HtmlUtils.EscapeEntities(HtmlUtils.EscapeEntities(TEST_STRING)))
                            results.AddResult("requiresHtmlTitles", NO);
                        else
                            results.AddResult("requiresHtmlTitles", "[ERROR] (value was: " + str + ")");

                        return;
                    }
                }
            }

            throw new InvalidOperationException("Title encoding test failed--title was not detected");
        }
    }
}
