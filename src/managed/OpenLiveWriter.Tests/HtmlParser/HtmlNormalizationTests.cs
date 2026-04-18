// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using NUnit.Framework;
using OpenLiveWriter.HtmlParser.Parser;

namespace OpenLiveWriter.Tests.HtmlParser
{
    /// <summary>
    /// Tests for HtmlUtils.NormalizeHtmlClosingTags which fixes HTML generated
    /// by MSHTML that lacks closing tags for elements with optional end tags.
    /// </summary>
    [TestFixture]
    public class HtmlNormalizationTests
    {
        [Test]
        public void NullAndEmptyInput()
        {
            Assert.IsNull(HtmlUtils.NormalizeHtmlClosingTags(null));
            Assert.AreEqual("", HtmlUtils.NormalizeHtmlClosingTags(""));
        }

        [Test]
        public void ParagraphsWithoutClosingTags()
        {
            // This is the main bug scenario - MSHTML generates paragraphs without closing tags
            Assert.AreEqual("<p>Hello</p>", HtmlUtils.NormalizeHtmlClosingTags("<p>Hello"));
            Assert.AreEqual("<p>First</p><p>Second</p>", HtmlUtils.NormalizeHtmlClosingTags("<p>First<p>Second"));
            Assert.AreEqual("<p>First</p><p>Second</p><p>Third</p>", HtmlUtils.NormalizeHtmlClosingTags("<p>First<p>Second<p>Third"));
        }

        [Test]
        public void ParagraphsWithClosingTags_Preserved()
        {
            Assert.AreEqual("<p>Hello</p>", HtmlUtils.NormalizeHtmlClosingTags("<p>Hello</p>"));
            Assert.AreEqual("<p>First</p><p>Second</p>", HtmlUtils.NormalizeHtmlClosingTags("<p>First</p><p>Second</p>"));
        }

        [Test]
        public void ListItems()
        {
            Assert.AreEqual("<ul><li>One</li><li>Two</li></ul>", 
                HtmlUtils.NormalizeHtmlClosingTags("<ul><li>One<li>Two</ul>"));
            Assert.AreEqual("<ol><li>One</li><li>Two</li><li>Three</li></ol>",
                HtmlUtils.NormalizeHtmlClosingTags("<ol><li>One<li>Two<li>Three</ol>"));
        }

        [Test]
        public void TableCells()
        {
            Assert.AreEqual("<table><tr><td>A</td><td>B</td></tr></table>",
                HtmlUtils.NormalizeHtmlClosingTags("<table><tr><td>A<td>B</tr></table>"));
        }

        [Test]
        public void TableRows()
        {
            Assert.AreEqual("<table><tr><td>A</td></tr><tr><td>B</td></tr></table>",
                HtmlUtils.NormalizeHtmlClosingTags("<table><tr><td>A<tr><td>B</table>"));
        }

        [Test]
        public void DefinitionList()
        {
            Assert.AreEqual("<dl><dt>Term</dt><dd>Definition</dd></dl>",
                HtmlUtils.NormalizeHtmlClosingTags("<dl><dt>Term<dd>Definition</dl>"));
        }

        [Test]
        public void SelectOptions()
        {
            Assert.AreEqual("<select><option>One</option><option>Two</option></select>",
                HtmlUtils.NormalizeHtmlClosingTags("<select><option>One<option>Two</select>"));
        }

        [Test]
        public void VoidElements_NotAffected()
        {
            // Void elements should pass through unchanged
            Assert.AreEqual("<p>Hello<br>World</p>", HtmlUtils.NormalizeHtmlClosingTags("<p>Hello<br>World</p>"));
            Assert.AreEqual("<p>Image: <img src=\"test.jpg\"></p>", 
                HtmlUtils.NormalizeHtmlClosingTags("<p>Image: <img src=\"test.jpg\"></p>"));
        }

        [Test]
        public void SelfClosingElements()
        {
            Assert.AreEqual("<p>Hello<br />World</p>", HtmlUtils.NormalizeHtmlClosingTags("<p>Hello<br />World</p>"));
        }

        [Test]
        public void PreservesAttributes()
        {
            Assert.AreEqual("<p class=\"intro\" id=\"first\">Hello</p>",
                HtmlUtils.NormalizeHtmlClosingTags("<p class=\"intro\" id=\"first\">Hello"));
        }

        [Test]
        public void PreservesWhitespaceAndComments()
        {
            Assert.AreEqual("<p>Hello\n  World</p>", HtmlUtils.NormalizeHtmlClosingTags("<p>Hello\n  World"));
            Assert.AreEqual("<p>Hello<!-- comment -->World</p>", 
                HtmlUtils.NormalizeHtmlClosingTags("<p>Hello<!-- comment -->World"));
        }

        [Test]
        public void CaseInsensitive()
        {
            Assert.AreEqual("<P>Hello</P>", HtmlUtils.NormalizeHtmlClosingTags("<P>Hello"));
            Assert.AreEqual("<P>First</P><p>Second</p>", HtmlUtils.NormalizeHtmlClosingTags("<P>First<p>Second"));
        }

        [Test]
        public void NonOptionalElements_NotAffected()
        {
            // Elements that require closing tags should pass through unchanged
            Assert.AreEqual("<div><span>Text</span></div>", 
                HtmlUtils.NormalizeHtmlClosingTags("<div><span>Text</span></div>"));
        }

        [Test]
        public void NestedElements()
        {
            // Paragraph inside div
            Assert.AreEqual("<div><p>Paragraph</p></div>", 
                HtmlUtils.NormalizeHtmlClosingTags("<div><p>Paragraph</div>"));
        }

        [Test]
        public void BlockElementClosesOpenParagraph()
        {
            // Per HTML5 spec, a P element's end tag can be omitted if immediately followed by block-level elements
            Assert.AreEqual("<p>Text</p><div>Content</div>",
                HtmlUtils.NormalizeHtmlClosingTags("<p>Text<div>Content</div>"));
            Assert.AreEqual("<p>Text</p><table><tr><td>Cell</td></tr></table>",
                HtmlUtils.NormalizeHtmlClosingTags("<p>Text<table><tr><td>Cell</td></tr></table>"));
            Assert.AreEqual("<p>Before</p><ul><li>Item</li></ul>",
                HtmlUtils.NormalizeHtmlClosingTags("<p>Before<ul><li>Item</ul>"));
            Assert.AreEqual("<p>Before</p><h1>Heading</h1>",
                HtmlUtils.NormalizeHtmlClosingTags("<p>Before<h1>Heading</h1>"));
        }

        [Test]
        public void NestedTables()
        {
            // Nested tables should only close tags within their own scope
            Assert.AreEqual("<table><tr><td><table><tr><td>Inner</td></tr></table></td></tr></table>",
                HtmlUtils.NormalizeHtmlClosingTags("<table><tr><td><table><tr><td>Inner</table></td></tr></table>"));
        }

        [Test]
        public void ComplexTable()
        {
            string input = "<table><thead><tr><th>H</th></tr><tbody><tr><td>D</td></tr></table>";
            string result = HtmlUtils.NormalizeHtmlClosingTags(input);
            Assert.AreEqual("<table><thead><tr><th>H</th></tr></thead><tbody><tr><td>D</td></tr></tbody></table>", result);
        }

        [Test]
        public void RealWorldPasteScenario()
        {
            // Simulates actual MSHTML output when pasting content
            string input = "<p>This is the first paragraph.<p>This is the second paragraph.<p>And this is the third.";
            string result = HtmlUtils.NormalizeHtmlClosingTags(input);
            Assert.AreEqual("<p>This is the first paragraph.</p><p>This is the second paragraph.</p><p>And this is the third.</p>", result);
        }
    }
}
