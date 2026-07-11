// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Threading.Tasks;
using AngleSharp.Dom;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.EditorTests.Automated.Infrastructure;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group B — source / preview round-trip. The source view is produced by
    /// <see cref="EditorPanel.FormatHtml"/> (pure) and pushed back verbatim to the
    /// editor on return, so B1–B3 are validated headlessly by checking that the
    /// DOM structure survives the WYSIWYG↔source hop. The live WebView round-trip
    /// and the preview render are [Explicit] (they need a WKWebView / a populated
    /// PreviewHost).
    /// </summary>
    [TestFixture]
    [Category("GroupB")]
    public class GroupB_RoundtripTests
    {
        // B1: WYSIWYG -> source contains the expected tags.
        [Test]
        public void B1_WysiwygToSource_ContainsExpectedTags()
        {
            var editorHtml = "<h2>Heading</h2><p><b>bold</b> and <i>italic</i></p><ul><li>a</li><li>b</li></ul>";
            var source = EditorPanel.FormatHtml(editorHtml);

            var doc = Dom.Parse(source);
            Assert.Multiple(() =>
            {
                Assert.That(doc.QuerySelector("h2"), Is.Not.Null);
                Assert.That(doc.QuerySelector("b"), Is.Not.Null);
                Assert.That(doc.QuerySelector("i"), Is.Not.Null);
                Assert.That(doc.QuerySelectorAll("ul > li"), Has.Length.EqualTo(2));
            });
            // Formatting for readability must not corrupt well-formedness.
            Assert.That(HtmlWellFormednessGate.IsWellFormed(source), Is.True);
        }

        // B2: source -> WYSIWYG preserves content (structure + text unchanged).
        [Test]
        public void B2_SourceToWysiwyg_PreservesContent()
        {
            var original = "<p>Keep <b>this</b> exact <a href=\"https://x.io\">link</a>.</p>";
            var source = EditorPanel.FormatHtml(original);

            // Returning to edit view pushes the (unformatted) source text back; the
            // parsed DOM must be equivalent to the original.
            AssertDomEquivalent(original, source);
        }

        // B3: hand-edited source (h2 + ul) survives the round-trip.
        [Test]
        public void B3_HandEditedSource_H2AndList_SurvivesRoundTrip()
        {
            var handEdited = "<h2>Shopping</h2>\n<ul>\n<li>Milk</li>\n<li>Eggs</li>\n</ul>";
            var source = EditorPanel.FormatHtml(handEdited);
            var doc = Dom.Parse(source);

            Assert.Multiple(() =>
            {
                Assert.That(doc.QuerySelector("h2")?.TextContent, Is.EqualTo("Shopping"));
                var items = doc.QuerySelectorAll("ul > li");
                Assert.That(items, Has.Length.EqualTo(2));
                Assert.That(items[0].TextContent, Is.EqualTo("Milk"));
                Assert.That(items[1].TextContent, Is.EqualTo("Eggs"));
            });
        }

        private static void AssertDomEquivalent(string expectedHtml, string actualHtml)
        {
            IElement a = Dom.ParseBody(expectedHtml);
            IElement b = Dom.ParseBody(actualHtml);
            Assert.That(Normalize(b.InnerHtml), Is.EqualTo(Normalize(a.InnerHtml)));
        }

        private static string Normalize(string html) =>
            System.Text.RegularExpressions.Regex.Replace(html, @"\s+", " ").Trim();

        // --- Live WebView round-trip (needs a real WKWebView backend) ---

        [Test]
        [Explicit("Requires a live WKWebView backend")]
        [Category(WebViewCategories.WebView)]
        public async Task Live_WysiwygSourceRoundTrip_PreservesContent()
        {
            await using var harness = await EditorTestHarness.CreateAsync();
            await harness.SetContentAsync("<h2>Title</h2><p><b>bold</b></p>");

            var html = await harness.GetContentAsync();
            Assert.That(Dom.Has(html, "h2"), Is.True);

            // Round-trip back in.
            await harness.SetContentAsync(html);
            var again = await harness.GetContentAsync();
            Assert.That(Dom.Has(again, "b"), Is.True);
        }

        // B4: Preview render — the preview document is composed by the pure
        // PreviewRenderer (separated from the live WebView display). The composed
        // document must be a well-formed article that contains the post body,
        // applies the neutral preview style, and joins main + extended content.

        [Test]
        public void B4_PreviewRender_ComposesArticleWithBody()
        {
            var editorHtml = "<h2>My Post</h2><p>Hello <b>world</b>.</p><ul><li>one</li><li>two</li></ul>";
            var document = PreviewRenderer.BuildPreviewDocument(editorHtml, "My Post");

            var doc = Dom.Parse(document);
            Assert.Multiple(() =>
            {
                // Wrapped in a neutral article container.
                Assert.That(doc.QuerySelector("article"), Is.Not.Null, "expected an <article> wrapper");
                // Body content survives verbatim.
                Assert.That(doc.QuerySelector("article h2")?.TextContent, Is.EqualTo("My Post"));
                Assert.That(doc.QuerySelector("article b"), Is.Not.Null);
                Assert.That(doc.QuerySelectorAll("article ul > li"), Has.Length.EqualTo(2));
                // Neutral preview style is applied.
                Assert.That(doc.QuerySelector("style"), Is.Not.Null);
            });
        }

        [Test]
        public void B4_PreviewRender_JoinsExtendedContent_StripsMoreMarker()
        {
            var editorHtml = "<p>Intro</p><!--more--><p>Rest of the story</p>";
            var document = PreviewRenderer.BuildPreviewDocument(editorHtml);

            Assert.That(document, Does.Not.Contain("<!--more-->"), "the extended-entry marker must not render");
            var doc = Dom.Parse(document);
            Assert.That(doc.QuerySelectorAll("article p"), Has.Length.EqualTo(2));
        }

        [Test]
        public void B4_PreviewRender_EmptyBody_StillWellFormed()
        {
            var document = PreviewRenderer.BuildPreviewDocument(null);
            var doc = Dom.Parse(document);
            Assert.That(doc.QuerySelector("article"), Is.Not.Null);
            Assert.That(doc.QuerySelector("html"), Is.Not.Null);
        }

        // --- Live preview render (needs a live WKWebView backend to display) ---

        [Test]
        [Explicit("Requires a live WKWebView backend to display the composed preview")]
        [Category(WebViewCategories.WebView)]
        public async Task Live_PreviewRender_ShowsContent()
        {
            await using var harness = await EditorTestHarness.CreateAsync();
            await harness.SetContentAsync("<h2>Preview me</h2>");
            var html = await harness.GetContentAsync();
            var document = PreviewRenderer.BuildPreviewDocument(html);
            Assert.That(Dom.Has(document, "article h2"), Is.True);
        }
    }
}
