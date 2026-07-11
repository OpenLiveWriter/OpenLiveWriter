// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.EditorTests.Automated.Infrastructure;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group A (link generation): the anchor-building/HTML-escaping half of the
    /// "Insert Link" feature is pure C# in <see cref="WebViewEditor"/>, so it is
    /// tested here without a live WebView. DOM assertions via AngleSharp.
    /// </summary>
    [TestFixture]
    [Category("GroupA")]
    public class GroupA_LinkHtmlTests
    {
        [Test]
        public void CreateLink_SimpleAnchor_HasHrefAndText()
        {
            var html = WebViewEditor.BuildAnchorHtml("https://example.com", "Example", null, false);

            var a = Dom.Select(html, "a");
            Assert.That(a, Is.Not.Null, "anchor element should be produced");
            Assert.That(a.GetAttribute("href"), Is.EqualTo("https://example.com"));
            Assert.That(a.TextContent, Is.EqualTo("Example"));
        }

        [Test]
        public void CreateLink_WithTitleAndNewWindow_SetsAttributes()
        {
            var html = WebViewEditor.BuildAnchorHtml(
                "https://example.com/path", "Docs", "Read the docs", openInNewWindow: true);

            var a = Dom.Select(html, "a");
            Assert.That(a.GetAttribute("href"), Is.EqualTo("https://example.com/path"));
            Assert.That(a.GetAttribute("title"), Is.EqualTo("Read the docs"));
            Assert.That(a.GetAttribute("target"), Is.EqualTo("_blank"));
            Assert.That(a.GetAttribute("rel"), Is.EqualTo("noopener"));
            Assert.That(a.TextContent, Is.EqualTo("Docs"));
        }

        [Test]
        public void CreateLink_NoTitle_OmitsTitleAttribute()
        {
            var html = WebViewEditor.BuildAnchorHtml("https://example.com", "Example", "", false);
            var a = Dom.Select(html, "a");
            Assert.That(a.HasAttribute("title"), Is.False);
            Assert.That(a.HasAttribute("target"), Is.False);
        }

        [Test]
        public void CreateLink_EscapesAmpersandLtQuoteInAllFields()
        {
            // & < > and " must be escaped so the anchor stays well-formed.
            var url = "https://example.com/?a=1&b=2&c=<x>";
            var text = "Tom & \"Jerry\" <fun>";
            var title = "He said \"hi\" & <b>bold</b>";

            var html = WebViewEditor.BuildAnchorHtml(url, text, title, openInNewWindow: true);

            // Raw markup must contain escaped entities, not raw & < ".
            Assert.That(html, Does.Contain("&amp;"));
            Assert.That(html, Does.Contain("&lt;"));
            Assert.That(html, Does.Contain("&quot;"));

            // A raw, unescaped attribute-breaking quote must not appear inside the tag.
            var tagOnly = html.Substring(0, html.IndexOf('>') + 1);
            Assert.That(tagOnly, Does.Not.Contain("\"Jerry\""));

            // And the parsed DOM must decode back to the exact original values.
            var a = Dom.Select(html, "a");
            Assert.That(a.GetAttribute("href"), Is.EqualTo(url));
            Assert.That(a.GetAttribute("title"), Is.EqualTo(title));
            Assert.That(a.TextContent, Is.EqualTo(text));
        }

        [Test]
        public void CreateLink_OutputIsWellFormed()
        {
            var html = WebViewEditor.BuildAnchorHtml(
                "https://example.com/?a=1&b=2", "A & B <link>", "t\"t", true);

            Assert.That(HtmlWellFormednessGate.IsWellFormed(html), Is.True,
                HtmlWellFormednessGate.Validate(html).ToString());
        }
    }
}
