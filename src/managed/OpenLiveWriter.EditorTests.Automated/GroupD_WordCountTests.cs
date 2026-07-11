// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Editor;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group D5 — word count (P1-8). Exercises the cross-platform
    /// <see cref="WordCounter"/> (HTML → plain text + word/char/paragraph counts)
    /// against known strings. Pure logic; no WebView backend required.
    /// </summary>
    [TestFixture]
    [Category("GroupD")]
    public class GroupD_WordCountTests
    {
        [TestCase("", 0)]
        [TestCase("Hello", 1)]
        [TestCase("The quick brown fox", 4)]
        [TestCase("one  two   three", 3)]      // collapse runs of spaces
        [TestCase("<p>The quick brown fox</p>", 4)]
        [TestCase("<p>Hello <b>bold</b> world</p>", 3)]
        public void Words_CountedFromPlainText(string html, int expected)
        {
            Assert.That(new WordCounter(html).Words, Is.EqualTo(expected));
        }

        [Test]
        public void Chars_WithAndWithoutSpaces()
        {
            var counter = new WordCounter("<p>ab cd</p>");
            // Plain text "ab cd" → 5 chars incl. space, 4 without.
            Assert.That(counter.Chars, Is.EqualTo(5));
            Assert.That(counter.CharsWithoutSpaces, Is.EqualTo(4));
        }

        [TestCase("", 0)]
        [TestCase("Single paragraph", 1)]
        [TestCase("<p>One</p><p>Two</p>", 2)]
        [TestCase("<p>One</p><p>Two</p><p>Three</p>", 3)]
        public void Paragraphs_CountedFromBlockBoundaries(string html, int expected)
        {
            Assert.That(new WordCounter(html).Paragraphs, Is.EqualTo(expected));
        }

        [Test]
        public void HtmlToPlainText_StripsTagsAndDecodesEntities()
        {
            string text = WordCounter.HtmlToPlainText("<p>Tom &amp; Jerry &lt;3</p>");
            Assert.That(text, Is.EqualTo("Tom & Jerry <3"));
        }

        [Test]
        public void HtmlToPlainText_BrBecomesNewline()
        {
            string text = WordCounter.HtmlToPlainText("Line1<br>Line2");
            Assert.That(text, Is.EqualTo("Line1\nLine2"));
        }

        [Test]
        public void EmptyHtml_AllCountsZero()
        {
            var counter = new WordCounter("");
            Assert.Multiple(() =>
            {
                Assert.That(counter.Words, Is.EqualTo(0));
                Assert.That(counter.Chars, Is.EqualTo(0));
                Assert.That(counter.CharsWithoutSpaces, Is.EqualTo(0));
                Assert.That(counter.Paragraphs, Is.EqualTo(0));
            });
        }

        [Test]
        public void RichDocument_AllStatsConsistent()
        {
            // Two headings + two paragraphs, some inline formatting.
            const string html =
                "<h1>Title Here</h1>" +
                "<p>First paragraph with <b>five</b> words.</p>" +
                "<h2>Section</h2>" +
                "<p>Second paragraph text.</p>";

            var counter = new WordCounter(html);
            // Words: "Title Here"(2) + "First paragraph with five words."(5) +
            // "Section"(1) + "Second paragraph text."(3) = 11.
            Assert.That(counter.Words, Is.EqualTo(11));
            Assert.That(counter.Paragraphs, Is.EqualTo(4));
        }
    }
}
