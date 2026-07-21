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
    /// Group A (live editor) — drives real document.execCommand formatting through
    /// the OLWBridge and asserts on the produced DOM. These require a live WKWebView
    /// backend which is unavailable in a headless <c>dotnet test</c> run, so they
    /// are [Explicit] + [Category("WebView")].
    ///
    /// Run on a real macOS desktop session with:
    ///   dotnet test --filter "Category=WebView"
    /// </summary>
    [TestFixture]
    [Category("GroupA")]
    [Category(WebViewCategories.WebView)]
    [Explicit("Requires a live WKWebView backend — run on a real macOS session")]
    public class GroupA_EditorCommandTests
    {
        private async Task<IDocument> ApplyToParagraph(string command, string value = null, string html = "<p>The quick brown fox</p>")
        {
            await using var harness = await EditorTestHarness.CreateAsync();
            await harness.SetContentAsync(html);
            await harness.SelectAllAsync();
            await harness.ExecAsync(command, value);
            await Task.Delay(150);
            return await harness.GetContentDomAsync();
        }

        [Test]
        public async Task Bold_WrapsSelection()
        {
            var dom = await ApplyToParagraph("bold");
            Assert.That(dom.QuerySelector("b, strong"), Is.Not.Null);
        }

        [Test]
        public async Task Italic_WrapsSelection()
        {
            var dom = await ApplyToParagraph("italic");
            Assert.That(dom.QuerySelector("i, em"), Is.Not.Null);
        }

        [Test]
        public async Task Underline_WrapsSelection()
        {
            var dom = await ApplyToParagraph("underline");
            Assert.That(dom.QuerySelector("u"), Is.Not.Null);
        }

        [Test]
        public async Task Strikethrough_WrapsSelection()
        {
            var dom = await ApplyToParagraph("strikeThrough");
            Assert.That(dom.QuerySelector("strike, s, del"), Is.Not.Null);
        }

        [Test]
        public async Task Subscript_WrapsSelection()
        {
            var dom = await ApplyToParagraph("subscript");
            Assert.That(dom.QuerySelector("sub"), Is.Not.Null);
        }

        [Test]
        public async Task Superscript_WrapsSelection()
        {
            var dom = await ApplyToParagraph("superscript");
            Assert.That(dom.QuerySelector("sup"), Is.Not.Null);
        }

        [Test]
        public async Task UnorderedList_CreatesUl()
        {
            var dom = await ApplyToParagraph("insertUnorderedList");
            Assert.That(dom.QuerySelector("ul > li"), Is.Not.Null);
        }

        [Test]
        public async Task OrderedList_CreatesOl()
        {
            var dom = await ApplyToParagraph("insertOrderedList");
            Assert.That(dom.QuerySelector("ol > li"), Is.Not.Null);
        }

        [Test]
        public async Task IndentThenOutdent_IsIdempotent()
        {
            await using var harness = await EditorTestHarness.CreateAsync();
            await harness.SetContentAsync("<p>Indent me</p>");
            await harness.SelectAllAsync();

            var before = await harness.GetContentAsync();
            await harness.ExecAsync("indent");
            await Task.Delay(150);
            await harness.SelectAllAsync();
            await harness.ExecAsync("outdent");
            await Task.Delay(150);
            var after = await harness.GetContentAsync();

            Assert.That(Dom.ParseBody(after).TextContent.Trim(),
                Is.EqualTo(Dom.ParseBody(before).TextContent.Trim()));
        }

        [Test]
        public async Task AlignCenter_SetsTextAlign()
        {
            var dom = await ApplyToParagraph("justifyCenter");
            var el = dom.QuerySelector("[style*='center'], [align='center']");
            Assert.That(el, Is.Not.Null);
        }

        [Test]
        public async Task AlignRight_SetsTextAlign()
        {
            var dom = await ApplyToParagraph("justifyRight");
            Assert.That(dom.QuerySelector("[style*='right'], [align='right']"), Is.Not.Null);
        }

        [Test]
        public async Task Justify_SetsTextAlign()
        {
            var dom = await ApplyToParagraph("justifyFull");
            Assert.That(dom.QuerySelector("[style*='justify'], [align='justify']"), Is.Not.Null);
        }

        [Test]
        public async Task Blockquote_TogglesOnThenRevertsToParagraph()
        {
            await using var harness = await EditorTestHarness.CreateAsync();
            await harness.SetContentAsync("<p>Quote this</p>");
            await harness.SelectAllAsync();

            await harness.Editor.ExecuteBlockquoteAsync();
            await Task.Delay(150);
            var on = await harness.GetContentAsync();
            Assert.That(Dom.Has(on, "blockquote"), Is.True, "blockquote should be applied");

            await harness.SelectAllAsync();
            await harness.Editor.ExecuteBlockquoteAsync();
            await Task.Delay(150);
            var off = await harness.GetContentAsync();
            Assert.That(Dom.Has(off, "blockquote"), Is.False, "blockquote should revert");
            Assert.That(Dom.Has(off, "p"), Is.True, "content should revert to a paragraph");
        }

        [TestCase("h1")]
        [TestCase("h2")]
        [TestCase("h3")]
        [TestCase("h4")]
        [TestCase("h5")]
        [TestCase("h6")]
        [TestCase("p")]
        [TestCase("pre")]
        public async Task FormatBlock_AppliesHeadingOrBlock(string tag)
        {
            // h4-h6 and pre are reachable through the bridge (formatBlock) even
            // though the toolbar HeadingCombo currently exposes only h1-h3 — this
            // documents that gap (see GroupA_ToolbarGapTests).
            await using var harness = await EditorTestHarness.CreateAsync();
            await harness.SetContentAsync("<p>Block me</p>");
            await harness.SelectAllAsync();
            await harness.Editor.SetBlockFormatAsync(tag);
            await Task.Delay(150);
            var html = await harness.GetContentAsync();
            Assert.That(Dom.Has(html, tag), Is.True, $"expected <{tag}> in: {html}");
        }

        [Test]
        public async Task CreateLink_WrapsSelectionInAnchor()
        {
            await using var harness = await EditorTestHarness.CreateAsync();
            await harness.SetContentAsync("<p>Link me</p>");
            await harness.SelectAllAsync();
            await harness.Editor.CreateLinkAsync("https://example.com");
            await Task.Delay(150);
            var dom = await harness.GetContentDomAsync();
            Assert.That(dom.QuerySelector("a")?.GetAttribute("href"), Is.EqualTo("https://example.com"));
        }

        [Test]
        public async Task InsertLink_WithTextTitleNewWindow_EscapesSpecialChars()
        {
            await using var harness = await EditorTestHarness.CreateAsync();
            await harness.SetContentAsync("<p>anchor here</p>");
            await harness.SelectAllAsync();
            await harness.Editor.InsertLinkAsync(
                "https://example.com/?a=1&b=2", "Tom & \"Jerry\"", "Title <x>", openInNewWindow: true);
            await Task.Delay(150);
            var dom = await harness.GetContentDomAsync();
            var a = dom.QuerySelector("a");
            Assert.That(a, Is.Not.Null);
            Assert.That(a.GetAttribute("href"), Is.EqualTo("https://example.com/?a=1&b=2"));
            Assert.That(a.GetAttribute("title"), Is.EqualTo("Title <x>"));
            Assert.That(a.GetAttribute("target"), Is.EqualTo("_blank"));
            Assert.That(a.TextContent, Is.EqualTo("Tom & \"Jerry\""));
        }

        [Test]
        public async Task HorizontalRule_InsertsHr()
        {
            await using var harness = await EditorTestHarness.CreateAsync();
            await harness.SetContentAsync("<p>Before</p>");
            await harness.SelectAllAsync();
            await harness.Editor.InsertHorizontalLineAsync();
            await Task.Delay(150);
            var dom = await harness.GetContentDomAsync();
            Assert.That(dom.QuerySelector("hr"), Is.Not.Null);
        }

        [Test]
        public async Task ClearFormatting_RemovesInlineTags()
        {
            var dom = await ApplyToParagraph("removeFormat", html: "<p><b>bold</b> <i>italic</i></p>");
            Assert.Multiple(() =>
            {
                Assert.That(dom.QuerySelector("b, strong"), Is.Null);
                Assert.That(dom.QuerySelector("i, em"), Is.Null);
            });
        }

        [Test]
        public async Task PartialSelection_BoldsOnlySelectedRange()
        {
            await using var harness = await EditorTestHarness.CreateAsync();
            await harness.SetContentAsync("<p>Format only part of this</p>");
            harness.Editor.WebView.Focus();
            await Task.Delay(50);
            // Select the substring "only part".
            await harness.Editor.WebView.InvokeScript(
                "var tn=document.body.querySelector('p').firstChild;" +
                "var r=document.createRange();r.setStart(tn,7);r.setEnd(tn,16);" +
                "var s=window.getSelection();s.removeAllRanges();s.addRange(r);" +
                "OLWBridge.saveSelection();");
            await Task.Delay(50);
            await harness.ExecAsync("bold");
            await Task.Delay(150);
            var dom = await harness.GetContentDomAsync();
            var bold = dom.QuerySelector("b, strong");
            Assert.That(bold, Is.Not.Null);
            Assert.That(bold.TextContent, Is.EqualTo("only part"));
        }

        [Test]
        public async Task FontFamily_AppliesFace()
        {
            var dom = await ApplyToParagraph("fontName", "Georgia");
            var el = dom.QuerySelector("font[face], [style*='font-family']");
            Assert.That(el, Is.Not.Null);
        }

        [Test]
        public async Task FontSize_AppliesSize()
        {
            var dom = await ApplyToParagraph("fontSize", "5");
            var el = dom.QuerySelector("font[size], [style*='font-size']");
            Assert.That(el, Is.Not.Null);
        }

        [Test]
        public async Task FontSizePx_AppliesPixelSize()
        {
            await using var harness = await EditorTestHarness.CreateAsync();
            await harness.SetContentAsync("<p>Size me</p>");
            await harness.SelectAllAsync();
            await harness.Editor.SetFontSizeAsync("18");
            await Task.Delay(150);
            var dom = await harness.GetContentDomAsync();
            var el = dom.QuerySelector("[style*='font-size']");
            Assert.That(el, Is.Not.Null, "expected an inline font-size style");
            Assert.That(el.GetAttribute("style"), Does.Contain("18px"));
        }

        [Test]
        public async Task FindStats_CountsMatchesAcrossTextNodes()
        {
            await using var harness = await EditorTestHarness.CreateAsync();
            await harness.SetContentAsync("<p>cat and cat</p><p>dog</p><h2>cat</h2>");
            await Task.Delay(100);
            FindStats stats = await harness.Editor.FindStatsAsync("cat", matchCase: false);
            Assert.That(stats, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(stats.Total, Is.EqualTo(3));
                Assert.That(stats.Current, Is.EqualTo(0), "no match selected yet");
            });
        }

        [Test]
        public async Task ReplaceCurrent_ReplacesOnlySelectedMatch()
        {
            await using var harness = await EditorTestHarness.CreateAsync();
            await harness.SetContentAsync("<p>brown fox brown dog</p>");
            harness.Editor.WebView.Focus();
            await Task.Delay(50);
            // Select the first "brown" (chars 0-5).
            await harness.Editor.WebView.InvokeScript(
                "var tn=document.body.querySelector('p').firstChild;" +
                "var r=document.createRange();r.setStart(tn,0);r.setEnd(tn,5);" +
                "var s=window.getSelection();s.removeAllRanges();s.addRange(r);" +
                "OLWBridge.saveSelection();");
            await Task.Delay(50);

            bool replaced = await harness.Editor.ReplaceCurrentAsync("brown", "black", matchCase: false);
            await Task.Delay(150);

            var dom = await harness.GetContentDomAsync();
            Assert.That(replaced, Is.True);
            Assert.That(dom.QuerySelector("p").TextContent, Is.EqualTo("black fox brown dog"));
        }

        // A16 — publish-readiness gate applied to LIVE editor output.
        [Test]
        public async Task A16_AfterBatteryOfCommands_OutputIsWellFormed()
        {
            await using var harness = await EditorTestHarness.CreateAsync();
            await harness.SetContentAsync("<p>The quick brown fox jumps</p>");

            await harness.SelectAllAsync();
            await harness.ExecAsync("bold");
            await harness.SelectAllAsync();
            await harness.ExecAsync("italic");
            await harness.SelectAllAsync();
            await harness.ExecAsync("insertUnorderedList");
            await harness.SelectAllAsync();
            await harness.Editor.SetBlockFormatAsync("h2");
            await harness.Editor.InsertHorizontalLineAsync();
            await Task.Delay(200);

            var html = await harness.GetContentAsync();
            var result = HtmlWellFormednessGate.Validate(html);
            Assert.That(result.IsWellFormed, Is.True, result.ToString());
        }
    }
}
