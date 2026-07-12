// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.EditorTests.Automated.Infrastructure;
using OpenLiveWriter.Publishing;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group M — Insert Tags / keywords. The rel="tag" microformat link builder
    /// (<see cref="TagLinkBuilder"/>) and keyword propagation through the post model
    /// (<see cref="PostDocument"/> ↔ <see cref="BlogPost"/> ↔ the MetaWeblog
    /// <c>mt_keywords</c> member) are pure and asserted headlessly on the parsed DOM /
    /// XML-RPC struct.
    /// </summary>
    [TestFixture]
    [Category("GroupM")]
    public class GroupM_TagTests
    {
        // ---- Tag-link HTML builder (rel="tag" microformat) ----

        [Test]
        public void BuildTagLinks_ProducesRelTagAnchors()
        {
            string html = TagLinkBuilder.BuildTagLinksHtml(new[] { "dotnet", "avalonia" });
            var doc = Dom.Parse(html);

            var anchors = doc.QuerySelectorAll("a[rel~='tag']").ToList();
            Assert.Multiple(() =>
            {
                Assert.That(anchors, Has.Count.EqualTo(2));
                Assert.That(anchors[0].TextContent, Is.EqualTo("dotnet"));
                Assert.That(anchors[0].GetAttribute("href"), Is.EqualTo("/tag/dotnet"));
                Assert.That(anchors[1].TextContent, Is.EqualTo("avalonia"));
                // Wrapper carries the olw-tags class.
                Assert.That(doc.QuerySelector("p.olw-tags"), Is.Not.Null);
            });
        }

        [Test]
        public void BuildTagLinks_UrlEncodesAndHtmlEscapes()
        {
            string html = TagLinkBuilder.BuildTagLinksHtml(new[] { "C# & F#" });
            var anchor = Dom.Parse(html).QuerySelector("a[rel~='tag']");
            Assert.Multiple(() =>
            {
                // href is URL-encoded ("C%23%20%26%20F%23"); text is HTML-escaped.
                Assert.That(anchor.GetAttribute("href"), Does.StartWith("/tag/"));
                Assert.That(anchor.GetAttribute("href"), Does.Contain("%23")); // '#'
                Assert.That(anchor.TextContent, Is.EqualTo("C# & F#"));
            });
        }

        [Test]
        public void BuildTagLinks_CustomBaseUrlAndCaption()
        {
            string html = TagLinkBuilder.BuildTagLinksHtml(
                new[] { "travel" },
                baseUrl: "https://example.com/tags/",
                caption: "Filed under: ");
            var doc = Dom.Parse(html);
            Assert.Multiple(() =>
            {
                Assert.That(doc.QuerySelector("a").GetAttribute("href"),
                    Is.EqualTo("https://example.com/tags/travel"));
                Assert.That(doc.QuerySelector("p").TextContent, Does.StartWith("Filed under:"));
            });
        }

        [Test]
        public void BuildTagLinks_DedupesAndDropsEmpties()
        {
            string html = TagLinkBuilder.BuildTagLinksHtml(new[] { "a", " a ", "", "  ", "B", "b" });
            var anchors = Dom.Parse(html).QuerySelectorAll("a[rel~='tag']");
            // "a"/" a " dedup to one; "B"/"b" dedup to one (case-insensitive).
            Assert.That(anchors, Has.Length.EqualTo(2));
        }

        [Test]
        public void BuildTagLinks_NoUsableTags_ReturnsNull()
        {
            Assert.That(TagLinkBuilder.BuildTagLinksHtml(new[] { "", "  " }), Is.Null);
            Assert.That(TagLinkBuilder.BuildTagLinksHtml(null), Is.Null);
        }

        [Test]
        public void BuildTagLinks_IsWellFormed()
        {
            string html = TagLinkBuilder.BuildTagLinksHtml(new[] { "one", "two", "three" });
            Assert.That(HtmlWellFormednessGate.IsWellFormed(html), Is.True, html);
        }

        [TestCase("a, b, c", new[] { "a", "b", "c" })]
        [TestCase("a\nb\nc", new[] { "a", "b", "c" })]
        [TestCase("  spaced ,  tags ", new[] { "spaced", "tags" })]
        [TestCase("dup, DUP", new[] { "dup" })]
        public void ParseTags_SplitsAndNormalizes(string input, string[] expected)
        {
            Assert.That(TagLinkBuilder.ParseTags(input), Is.EqualTo(expected));
        }

        // ---- Keyword propagation into the post model ----

        [Test]
        public void PostDocument_KeywordsProjectIntoBlogPost()
        {
            var doc = new PostDocument { Title = "T", BodyHtml = "<p>x</p>" };
            doc.Keywords.AddRange(new[] { "one", "two" });

            BlogPost post = doc.ToBlogPost();
            Assert.That(post.Keywords, Is.EqualTo("one, two"));
        }

        [Test]
        public void BlogPost_FromBlogPost_SplitsKeywordsBack()
        {
            var post = new BlogPost { Title = "T", Keywords = "alpha, beta, gamma" };
            PostDocument doc = PostDocument.FromBlogPost(post);
            Assert.That(doc.Keywords, Is.EqualTo(new List<string> { "alpha", "beta", "gamma" }));
        }

        [Test]
        public void MetaWeblog_IncludesKeywordsAsMtKeywords()
        {
            var client = new MetaWeblogXmlRpcClient("http://example/xmlrpc", "u", "p");
            var post = new BlogPost { Title = "Hello", Contents = "<p>Body</p>", Keywords = "news, tech" };

            string xml = client.BuildNewPostXml("blog1", post, publish: true);
            Assert.Multiple(() =>
            {
                Assert.That(xml, Does.Contain("mt_keywords"));
                Assert.That(xml, Does.Contain("news, tech"));
            });
        }

        [Test]
        public void MetaWeblog_OmitsKeywordsWhenEmpty()
        {
            var client = new MetaWeblogXmlRpcClient("http://example/xmlrpc", "u", "p");
            var post = new BlogPost { Title = "Hello", Contents = "<p>Body</p>" };

            string xml = client.BuildNewPostXml("blog1", post, publish: true);
            Assert.That(xml, Does.Not.Contain("mt_keywords"));
        }

        [Test]
        public void MetaWeblog_RespectsSupportsKeywordsOption()
        {
            var options = new BlogClientOptions { SupportsKeywords = false };
            var client = new MetaWeblogXmlRpcClient("http://example/xmlrpc", "u", "p", options);
            var post = new BlogPost { Title = "Hello", Contents = "<p>Body</p>", Keywords = "x, y" };

            string xml = client.BuildNewPostXml("blog1", post, publish: true);
            Assert.That(xml, Does.Not.Contain("mt_keywords"));
        }
    }
}
