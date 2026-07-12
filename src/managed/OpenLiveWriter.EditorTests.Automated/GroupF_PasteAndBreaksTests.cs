// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.EditorTests.Automated.Infrastructure;
using OpenLiveWriter.Publishing;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group F — Paste Special (clean paste) and Insert Breaks (clear break /
    /// extended-entry marker). The sanitizers and marker snippets are pure, so they
    /// are asserted headlessly, including that the inserted extended-entry marker is
    /// still recognized by the publish split.
    /// </summary>
    [TestFixture]
    [Category("GroupF")]
    public class GroupF_PasteAndBreaksTests
    {
        // --- Paste as plain text ---

        [Test]
        public void PastePlainText_StripsAllMarkupAndDecodesEntities()
        {
            string html = "<p>Hello <b>bold</b> &amp; <a href=\"x\">link</a></p>";
            string text = PasteCleaner.ToPlainText(html);
            Assert.That(text, Is.EqualTo("Hello bold & link"));
        }

        [Test]
        public void PastePlainText_KeepsLineShapeAcrossBlocks()
        {
            string html = "<p>Line one</p><p>Line two</p>";
            string text = PasteCleaner.ToPlainText(html);
            Assert.That(text, Is.EqualTo("Line one\nLine two"));
        }

        [Test]
        public void PastePlainText_InsertionEscapesAndBreaks()
        {
            string payload = PasteCleaner.BuildPlainTextInsertion("a < b\nc & d");
            Assert.That(payload, Is.EqualTo("a &lt; b<br />c &amp; d"));
        }

        // --- Paste as clean HTML ---

        [Test]
        public void CleanHtml_DropsScriptsStylesAndForeignAttributes()
        {
            string dirty = "<div class=\"x\" style=\"color:red\" onclick=\"evil()\">" +
                           "<script>steal()</script><style>.a{}</style>" +
                           "<p id=\"p1\">Keep <b>this</b></p></div>";
            string clean = PasteCleaner.CleanHtml(dirty);

            Assert.Multiple(() =>
            {
                Assert.That(clean, Does.Not.Contain("script"));
                Assert.That(clean, Does.Not.Contain("style"));
                Assert.That(clean, Does.Not.Contain("onclick"));
                Assert.That(clean, Does.Not.Contain("class"));
                Assert.That(clean, Does.Not.Contain("id="));
                // Whitelisted structure + text survive.
                var doc = Dom.Parse(clean);
                Assert.That(doc.QuerySelector("p b")?.TextContent, Is.EqualTo("this"));
            });
        }

        [Test]
        public void CleanHtml_KeepsAnchorHrefButDropsJavascriptScheme()
        {
            string keep = PasteCleaner.CleanHtml("<a href=\"https://ok.example\" title=\"t\" rel=\"x\">go</a>");
            var a = Dom.Parse(keep).QuerySelector("a");
            Assert.That(a.GetAttribute("href"), Is.EqualTo("https://ok.example"));
            Assert.That(a.GetAttribute("title"), Is.EqualTo("t"));
            Assert.That(a.GetAttribute("rel"), Is.Null);

            string drop = PasteCleaner.CleanHtml("<a href=\"javascript:evil()\">x</a>");
            var a2 = Dom.Parse(drop).QuerySelector("a");
            Assert.That(a2.GetAttribute("href"), Is.Null, "javascript: URL must be dropped");
        }

        [Test]
        public void CleanHtml_UnknownTagsAreUnwrappedKeepingText()
        {
            string clean = PasteCleaner.CleanHtml("<article><p>Body</p></article>");
            Assert.That(clean, Does.Not.Contain("article"));
            Assert.That(Dom.Parse(clean).QuerySelector("p")?.TextContent, Is.EqualTo("Body"));
        }

        // --- Insert breaks ---

        [Test]
        public void ClearBreak_IsWellFormedClearingBreak()
        {
            Assert.That(EditorMarkup.ClearBreakHtml, Does.Contain("clear"));
            Assert.That(HtmlWellFormednessGate.IsWellFormed("<p>x</p>" + EditorMarkup.ClearBreakHtml), Is.True);
        }

        [Test]
        public void ExtendedEntry_MarkerMatchesPublishSplitMarker()
        {
            Assert.That(EditorMarkup.ExtendedEntryBreakHtml, Is.EqualTo(ExtendedEntry.BreakMarker));
            Assert.That(EditorMarkup.ExtendedEntryBreakHtml, Is.EqualTo("<!--more-->"));
        }

        [Test]
        public void ExtendedEntry_InsertedMarkerIsRecognizedByPublishSplit()
        {
            // Author writes intro, inserts the extended-entry break, then more.
            string body = "<p>Teaser</p>" + EditorMarkup.ExtendedEntryBreakHtml + "<p>Full story</p>";

            var (main, extended) = ExtendedEntry.Split(body);
            Assert.Multiple(() =>
            {
                Assert.That(main, Is.EqualTo("<p>Teaser</p>"));
                Assert.That(extended, Is.EqualTo("<p>Full story</p>"));
            });
        }

        [Test]
        public void ExtendedEntry_SplitFeedsBlogPostMainAndExtended()
        {
            string body = "<p>Teaser</p>" + EditorMarkup.ExtendedEntryBreakHtml + "<p>Full story</p>";
            var post = new BlogPost { Title = "T", Contents = body };

            Assert.Multiple(() =>
            {
                Assert.That(post.MainContents, Is.EqualTo("<p>Teaser</p>"));
                Assert.That(post.ExtendedContents, Is.EqualTo("<p>Full story</p>"));
            });
        }
    }
}
