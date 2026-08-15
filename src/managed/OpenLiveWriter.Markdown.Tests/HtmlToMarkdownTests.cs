// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using NUnit.Framework;

namespace OpenLiveWriter.Markdown.Tests
{
    [TestFixture]
    public class HtmlToMarkdownTests
    {
        private IMarkdownService _service;

        [SetUp]
        public void SetUp()
        {
            _service = new MarkdownService();
        }

        [Test]
        public void ToMarkdown_Heading_ConvertsToHashHeading()
        {
            var markdown = _service.ToMarkdown("<h1>Hello</h1>");

            Assert.That(markdown, Is.EqualTo("# Hello"));
        }

        [Test]
        public void ToMarkdown_Paragraph_ConvertsToPlainText()
        {
            var markdown = _service.ToMarkdown("<p>Plain paragraph.</p>");

            Assert.That(markdown, Is.EqualTo("Plain paragraph."));
        }

        [Test]
        public void ToMarkdown_StrongAndEm_ConvertToMarkdownEmphasis()
        {
            var markdown = _service.ToMarkdown("<p><strong>bold</strong> and <em>italic</em></p>");

            Assert.That(markdown, Does.Contain("**bold**"));
            Assert.That(markdown, Does.Contain("*italic*"));
        }

        [Test]
        public void ToMarkdown_UnorderedList_ConvertsToDashItems()
        {
            var markdown = _service.ToMarkdown("<ul><li>one</li><li>two</li></ul>");

            Assert.That(markdown, Does.Contain("- one"));
            Assert.That(markdown, Does.Contain("- two"));
        }

        [Test]
        public void ToMarkdown_OrderedList_ConvertsToNumberedItems()
        {
            var markdown = _service.ToMarkdown("<ol><li>first</li><li>second</li></ol>");

            Assert.That(markdown, Does.Contain("1. first"));
            Assert.That(markdown, Does.Contain("2. second"));
        }

        [Test]
        public void ToMarkdown_Link_ConvertsToMarkdownLink()
        {
            var markdown = _service.ToMarkdown("<p><a href=\"https://example.com\">Example</a></p>");

            Assert.That(markdown, Does.Contain("[Example](https://example.com)"));
        }

        [Test]
        public void ToMarkdown_Image_ConvertsToMarkdownImage()
        {
            var markdown = _service.ToMarkdown("<p><img src=\"pic.png\" alt=\"alt text\" /></p>");

            Assert.That(markdown, Does.Contain("![alt text](pic.png)"));
        }

        [Test]
        public void ToMarkdown_Table_ConvertsToGfmTable()
        {
            var html = "<table><tr><th>H1</th><th>H2</th></tr><tr><td>a</td><td>b</td></tr></table>";
            var markdown = _service.ToMarkdown(html);

            Assert.That(markdown, Does.Contain("| H1 | H2 |"));
            Assert.That(markdown, Does.Contain("| --- | --- |"));
            Assert.That(markdown, Does.Contain("| a | b |"));
        }

        [Test]
        public void ToMarkdown_Strikethrough_ConvertsToTildeSyntax()
        {
            var markdown = _service.ToMarkdown("<p><del>removed</del></p>");

            Assert.That(markdown, Does.Contain("~~removed~~"));
        }

        [Test]
        public void ToMarkdown_TaskList_ConvertsToCheckboxSyntax()
        {
            var html = "<ul><li><input type=\"checkbox\" checked /> done</li><li><input type=\"checkbox\" /> todo</li></ul>";
            var markdown = _service.ToMarkdown(html);

            Assert.That(markdown, Does.Contain("- [x] done"));
            Assert.That(markdown, Does.Contain("- [ ] todo"));
        }

        [Test]
        public void ToMarkdown_CodeBlock_ConvertsToFencedBlock()
        {
            var markdown = _service.ToMarkdown("<pre><code>var x = 1;</code></pre>");

            Assert.That(markdown, Does.Contain("```"));
            Assert.That(markdown, Does.Contain("var x = 1;"));
        }

        [Test]
        public void ToMarkdown_Blockquote_ConvertsToQuotedLines()
        {
            var markdown = _service.ToMarkdown("<blockquote><p>Quoted text</p></blockquote>");

            Assert.That(markdown, Does.Contain("> Quoted text"));
        }

        [Test]
        public void ToMarkdown_UnknownElement_PreservesRawHtml()
        {
            var html = "<div class=\"x\">y</div>";
            var markdown = _service.ToMarkdown(html);

            Assert.That(markdown, Does.Contain("<div class=\"x\">y</div>"));
        }

        [Test]
        public void ToMarkdown_MoreComment_IsPreserved()
        {
            var markdown = _service.ToMarkdown("<p>Before</p><!--more--><p>After</p>");

            Assert.That(markdown, Does.Contain("<!--more-->"));
            Assert.That(markdown, Does.Contain("Before"));
            Assert.That(markdown, Does.Contain("After"));
        }

        [Test]
        public void ToMarkdown_SampleHtmlFixture_MatchesExpectedStructure()
        {
            var fixturePath = TestContext.CurrentContext.TestDirectory;
            var htmlPath = System.IO.Path.Combine(fixturePath, "Fixtures", "sample-post.html");
            var html = System.IO.File.ReadAllText(htmlPath);

            var markdown = _service.ToMarkdown(html);

            Assert.That(markdown, Does.Contain("# Sample Post"));
            Assert.That(markdown, Does.Contain("**bold**"));
            Assert.That(markdown, Does.Contain("*italic*"));
            Assert.That(markdown, Does.Contain("<!--more-->"));
            Assert.That(markdown, Does.Contain("## Section Two"));
            Assert.That(markdown, Does.Contain("- First item"));
            Assert.That(markdown, Does.Contain("| Col A | Col B |"));
            Assert.That(markdown, Does.Contain("<div class=\"plugin-embed\">"));
        }
    }
}
