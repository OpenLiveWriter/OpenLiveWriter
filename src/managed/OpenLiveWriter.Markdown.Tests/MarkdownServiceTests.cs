// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using NUnit.Framework;

namespace OpenLiveWriter.Markdown.Tests
{
    [TestFixture]
    public class MarkdownServiceTests
    {
        private IMarkdownService _service;

        [SetUp]
        public void SetUp()
        {
            _service = new MarkdownService();
        }

        [Test]
        public void ToHtml_Null_ReturnsEmptyString()
        {
            Assert.That(_service.ToHtml(null), Is.EqualTo(string.Empty));
        }

        [Test]
        public void ToHtml_Empty_ReturnsEmptyString()
        {
            Assert.That(_service.ToHtml(string.Empty), Is.EqualTo(string.Empty));
        }

        [Test]
        public void ToMarkdown_Null_ReturnsEmptyString()
        {
            Assert.That(_service.ToMarkdown(null), Is.EqualTo(string.Empty));
        }

        [Test]
        public void ToMarkdown_Empty_ReturnsEmptyString()
        {
            Assert.That(_service.ToMarkdown(string.Empty), Is.EqualTo(string.Empty));
        }

        [Test]
        public void ToHtml_Heading_RendersH1()
        {
            var html = _service.ToHtml("# Hello");

            Assert.That(html, Does.Contain("<h1"));
            Assert.That(html, Does.Contain("Hello"));
        }

        [Test]
        public void ToHtml_Paragraph_RendersParagraph()
        {
            var html = _service.ToHtml("Plain paragraph.");

            Assert.That(html, Does.Contain("<p"));
            Assert.That(html, Does.Contain("Plain paragraph."));
        }

        [Test]
        public void ToHtml_Bold_RendersStrong()
        {
            var html = _service.ToHtml("**bold**");

            Assert.That(html, Does.Contain("<strong"));
            Assert.That(html, Does.Contain("bold"));
        }

        [Test]
        public void ToHtml_Italic_RendersEmphasis()
        {
            var html = _service.ToHtml("*italic*");

            Assert.That(html, Does.Contain("<em"));
            Assert.That(html, Does.Contain("italic"));
        }

        [Test]
        public void ToHtml_UnorderedList_RendersUl()
        {
            var html = _service.ToHtml("- one\n- two");

            Assert.That(html, Does.Contain("<ul"));
            Assert.That(html, Does.Contain("<li"));
            Assert.That(html, Does.Contain("one"));
            Assert.That(html, Does.Contain("two"));
        }

        [Test]
        public void ToHtml_Link_RendersAnchor()
        {
            var html = _service.ToHtml("[Open Live Writer](https://example.com)");

            Assert.That(html, Does.Contain("<a"));
            Assert.That(html, Does.Contain("href=\"https://example.com\""));
            Assert.That(html, Does.Contain("Open Live Writer"));
        }

        [Test]
        public void ToHtml_Image_RendersImg()
        {
            var html = _service.ToHtml("![alt text](image.png)");

            Assert.That(html, Does.Contain("<img"));
            Assert.That(html, Does.Contain("src=\"image.png\""));
            Assert.That(html, Does.Contain("alt=\"alt text\""));
        }

        [Test]
        public void ToHtml_GfmTable_RendersTable()
        {
            var markdown = "| H1 | H2 |\n| --- | --- |\n| a | b |";
            var html = _service.ToHtml(markdown);

            Assert.That(html, Does.Contain("<table"));
            Assert.That(html, Does.Contain("<th"));
            Assert.That(html, Does.Contain("<td"));
            Assert.That(html, Does.Contain("H1"));
            Assert.That(html, Does.Contain("a"));
        }

        [Test]
        public void ToHtml_Strikethrough_RendersDel()
        {
            var html = _service.ToHtml("~~removed~~");

            Assert.That(html, Does.Contain("<del") | Does.Contain("<s"));
            Assert.That(html, Does.Contain("removed"));
        }

        [Test]
        public void ToHtml_TaskList_RendersCheckbox()
        {
            var html = _service.ToHtml("- [ ] todo\n- [x] done");

            Assert.That(html, Does.Contain("type=\"checkbox\""));
            Assert.That(html, Does.Contain("todo"));
            Assert.That(html, Does.Contain("done"));
        }

        [Test]
        public void ToHtml_MoreComment_IsPreserved()
        {
            var html = _service.ToHtml("Before\n\n<!--more-->\n\nAfter");

            Assert.That(html, Does.Contain("<!--more-->"));
        }

        [Test]
        public void RoundTrip_HeadingsAndParagraphs_ArePreserved()
        {
            const string markdown = "# Title\n\nFirst paragraph.\n\nSecond paragraph.";

            var roundTripped = _service.ToMarkdown(_service.ToHtml(markdown));

            Assert.That(roundTripped, Does.Contain("# Title"));
            Assert.That(roundTripped, Does.Contain("First paragraph."));
            Assert.That(roundTripped, Does.Contain("Second paragraph."));
        }

        [Test]
        public void RoundTrip_Emphasis_ArePreserved()
        {
            const string markdown = "Text with **bold** and *italic*.";

            var roundTripped = _service.ToMarkdown(_service.ToHtml(markdown));

            Assert.That(roundTripped, Does.Contain("**bold**"));
            Assert.That(roundTripped, Does.Contain("*italic*"));
        }

        [Test]
        public void RoundTrip_SampleFixture_PreservesCoreStructure()
        {
            var fixturePath = TestContext.CurrentContext.TestDirectory;
            var markdownPath = System.IO.Path.Combine(fixturePath, "Fixtures", "sample-post.md");
            var markdown = System.IO.File.ReadAllText(markdownPath);

            var roundTripped = _service.ToMarkdown(_service.ToHtml(markdown));

            Assert.That(roundTripped, Does.Contain("# Sample Post"));
            Assert.That(roundTripped, Does.Contain("**bold**"));
            Assert.That(roundTripped, Does.Contain("*italic*"));
            Assert.That(roundTripped, Does.Contain("<!--more-->"));
            Assert.That(roundTripped, Does.Contain("## Section Two"));
            Assert.That(roundTripped, Does.Contain("- First item"));
            Assert.That(roundTripped, Does.Contain("| Col A | Col B |"));
            Assert.That(roundTripped, Does.Contain("<div class=\"plugin-embed\">"));
        }
    }
}
