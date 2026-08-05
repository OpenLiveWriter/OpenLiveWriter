// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using AngleSharp.Dom;
using NUnit.Framework;
using OpenLiveWriter.WebView2Shim;
using AngleSharpHtmlParser = AngleSharp.Html.Parser.HtmlParser;

namespace OpenLiveWriter.Tests.WebView2Editor
{
    /// <summary>
    /// Parity tests for the WebView2 source view. FormatHtmlForDisplay pretty-prints
    /// the HTML shown in source editing mode; it must never corrupt the markup.
    /// Mirrors the invariant assertions of the macOS GroupB round-trip suite
    /// (EditorTests.Automated): expected tags survive, output stays well-formed,
    /// and the formatted source is DOM-equivalent to the original input.
    /// </summary>
    [TestFixture]
    public class SourceViewQualityTests
    {
        private static readonly AngleSharpHtmlParser Parser = new AngleSharpHtmlParser();

        [Test]
        public void Format_HeadingsBoldItalicAndLists_PreservesTags()
        {
            const string input = "<h2>Heading</h2><p><b>bold</b> and <i>italic</i></p><ul><li>a</li><li>b</li></ul>";
            string source = WebView2SourceEditorControl.FormatHtmlForDisplay(input);

            IElement body = ParseBody(source);
            Assert.IsNotNull(body.QuerySelector("h2"), "h2 lost during formatting");
            Assert.IsNotNull(body.QuerySelector("b"), "b lost during formatting");
            Assert.IsNotNull(body.QuerySelector("i"), "i lost during formatting");
            Assert.AreEqual(2, body.QuerySelectorAll("ul > li").Length, "list items lost during formatting");

            AssertWellFormed(source);
            AssertDomEquivalent(input, source);
        }

        [Test]
        public void Format_ParagraphsWithBreaks_PreservesTags()
        {
            const string input = "<p>First line<br>second line</p><p>Next paragraph</p>";
            string source = WebView2SourceEditorControl.FormatHtmlForDisplay(input);

            IElement body = ParseBody(source);
            Assert.AreEqual(2, body.QuerySelectorAll("p").Length, "paragraphs lost during formatting");
            Assert.IsNotNull(body.QuerySelector("p > br"), "br lost during formatting");

            AssertWellFormed(source);
            AssertDomEquivalent(input, source);
        }

        [Test]
        public void Format_Hyperlink_PreservesLinkAndHref()
        {
            const string input = "<p>Keep <b>this</b> exact <a href=\"https://x.io\">link</a>.</p>";
            string source = WebView2SourceEditorControl.FormatHtmlForDisplay(input);

            IElement body = ParseBody(source);
            IElement anchor = body.QuerySelector("a");
            Assert.IsNotNull(anchor, "anchor lost during formatting");
            Assert.AreEqual("https://x.io", anchor.GetAttribute("href"), "href corrupted during formatting");
            Assert.AreEqual("link", anchor.TextContent);

            AssertWellFormed(source);
            AssertDomEquivalent(input, source);
        }

        private static IElement ParseBody(string html)
        {
            return Parser.ParseDocument("<!DOCTYPE html><html><body>" + (html ?? string.Empty) + "</body></html>").Body;
        }

        private static void AssertDomEquivalent(string expectedHtml, string actualHtml)
        {
            string expected = Normalize(ParseBody(expectedHtml).InnerHtml);
            string actual = Normalize(ParseBody(actualHtml).InnerHtml);
            Assert.AreEqual(expected, actual, "formatted source is not DOM-equivalent to the input");
        }

        private static string Normalize(string html)
        {
            // Collapse whitespace runs and drop whitespace-only text nodes between
            // tags; both are insignificant for DOM equivalence.
            string collapsed = Regex.Replace(html, @"\s+", " ").Trim();
            collapsed = collapsed.Replace("> <", "><");
            // A newline pretty-printed after a <br> is an insignificant whitespace
            // text node (unlike whitespace between inline elements, which matters).
            return Regex.Replace(collapsed, @"(<br\s*/?>) ", "$1", RegexOptions.IgnoreCase);
        }

        // Mirrors the intent of the macOS HtmlWellFormednessGate: validate the
        // markup as XML (self-closing void elements, numeric entities) so broken
        // nesting or unclosed tags are rejected instead of silently repaired.
        private static readonly string[] VoidElements =
        {
            "area", "base", "br", "col", "embed", "hr", "img", "input",
            "link", "meta", "param", "source", "track", "wbr"
        };

        private static void AssertWellFormed(string html)
        {
            string prepared = html ?? string.Empty;
            prepared = prepared.Replace("&nbsp;", "&#160;");
            foreach (string tag in VoidElements)
            {
                prepared = Regex.Replace(
                    prepared,
                    $@"<{tag}(\s[^>]*?)?\s*/?>",
                    m =>
                    {
                        string attrs = m.Groups[1].Success ? m.Groups[1].Value.TrimEnd() : string.Empty;
                        return $"<{tag}{attrs}/>";
                    },
                    RegexOptions.IgnoreCase);
            }

            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                CheckCharacters = true
            };
            using (var reader = XmlReader.Create(new StringReader("<root>" + prepared + "</root>"), settings))
            {
                while (reader.Read())
                {
                    // consume to surface any structural errors
                }
            }
        }
    }
}
