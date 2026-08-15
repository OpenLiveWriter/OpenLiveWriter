// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.Publishing;

namespace OpenLiveWriter.Markdown.Tests
{
    [TestFixture]
    public class MarkdownEditingControllerTests
    {
        private IMarkdownService _service;
        private MarkdownEditingController _controller;

        [SetUp]
        public void SetUp()
        {
            _service = new MarkdownService();
            _controller = new MarkdownEditingController(_service);
        }

        [Test]
        public void HtmlMode_PassesHtmlThroughUnchanged()
        {
            _controller.SetContentFormat(ContentFormat.Html);

            const string html = "<p>Hello <strong>world</strong></p>";
            Assert.That(_controller.HtmlFromCanonical(html), Is.EqualTo(html));
            Assert.That(_controller.CanonicalFromHtml(html), Is.EqualTo(html));
        }

        [Test]
        public void MarkdownMode_HtmlFromCanonical_RendersMarkdown()
        {
            _controller.SetMarkdownMode(true);

            string html = _controller.HtmlFromCanonical("# Title");

            Assert.That(html, Does.Contain("<h1"));
            Assert.That(html, Does.Contain("Title"));
        }

        [Test]
        public void MarkdownMode_CanonicalFromHtml_ConvertsToMarkdown()
        {
            _controller.SetMarkdownMode(true);

            string markdown = _controller.CanonicalFromHtml("<h1>Title</h1><p>Body</p>");

            Assert.That(markdown, Does.Contain("# Title"));
            Assert.That(markdown, Does.Contain("Body"));
        }

        [Test]
        public void MarkdownMode_RoundTrip_PreservesHeadingAndParagraph()
        {
            _controller.SetMarkdownMode(true);

            const string original = "# Hello\n\nParagraph with **bold**.";
            string html = _controller.HtmlFromCanonical(original);
            string roundTripped = _controller.CanonicalFromHtml(html);

            Assert.That(roundTripped, Does.Contain("# Hello"));
            Assert.That(roundTripped, Does.Contain("**bold**"));
            Assert.That(roundTripped, Does.Contain("Paragraph"));
        }

        [Test]
        public void SetContentFormat_Markdown_EnablesMode()
        {
            _controller.SetContentFormat(ContentFormat.Markdown);
            Assert.That(_controller.IsMarkdownMode, Is.True);

            _controller.SetContentFormat(ContentFormat.Html);
            Assert.That(_controller.IsMarkdownMode, Is.False);
        }

        [Test]
        public void FontDisabledTooltip_MatchesSpec()
        {
            Assert.That(
                MarkdownEditingController.FontFamilySizeDisabledTooltip,
                Is.EqualTo("Font family and size are not available in Markdown mode because Markdown does not encode visual fonts."));
        }
    }
}
