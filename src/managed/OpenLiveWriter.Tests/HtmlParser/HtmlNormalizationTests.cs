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
            Assert.That(HtmlUtils.NormalizeHtmlClosingTags(null), Is.Null);
            Assert.That(HtmlUtils.NormalizeHtmlClosingTags(""), Is.EqualTo(""));
        }

        [Test]
        public void ParagraphsWithoutClosingTags()
        {
            // This is the main bug scenario - MSHTML generates paragraphs without closing tags
            Assert.That(HtmlUtils.NormalizeHtmlClosingTags("<p>Hello"), Is.EqualTo("<p>Hello</p>"));
            Assert.That(HtmlUtils.NormalizeHtmlClosingTags("<p>First<p>Second"), Is.EqualTo("<p>First</p><p>Second</p>"));
            Assert.That(HtmlUtils.NormalizeHtmlClosingTags("<p>First<p>Second<p>Third"), Is.EqualTo("<p>First</p><p>Second</p><p>Third</p>"));
        }

        [Test]
        public void ParagraphsWithClosingTags_Preserved()
        {
            Assert.That(HtmlUtils.NormalizeHtmlClosingTags("<p>Hello</p>"), Is.EqualTo("<p>Hello</p>"));
            Assert.That(HtmlUtils.NormalizeHtmlClosingTags("<p>First</p><p>Second</p>"), Is.EqualTo("<p>First</p><p>Second</p>"));
        }

        [Test]
        public void ListItems()
        {
            Assert.That(HtmlUtils.NormalizeHtmlClosingTags("<ul><li>One<li>Two</ul>"), 
                Is.EqualTo("<ul><li>One</li><li>Two</li></ul>"));
            Assert.That(HtmlUtils.NormalizeHtmlClosingTags("<ol><li>One<li>Two<li>Three</ol>"),
                Is.EqualTo("<ol><li>One</li><li>Two</li><li>Three</li></ol>"));
        }

        [Test]
        public void TableCells()
        {
            Assert.That(HtmlUtils.NormalizeHtmlClosingTags("<table><tr><td>A<td>B</tr></table>"),
                Is.EqualTo("<table><tr><td>A</td><td>B</td></tr></table>"));
        }

        [Test]
        public void TableRows()
        {
            Assert.That(HtmlUtils.NormalizeHtmlClosingTags("<table><tr><td>A<tr><td>B</table>"),
                Is.EqualTo("<table><tr><td>A</td></tr><tr><td>B</td></tr></table>"));
        }

        [Test]
        public void DefinitionList()
        {
            Assert.That(HtmlUtils.NormalizeHtmlClosingTags("<dl><dt>Term<dd>Definition</dl>"),
                Is.EqualTo("<dl><dt>Term</dt><dd>Definition</dd></dl>"));
        }

        [Test]
        public void SelectOptions()
        {
            Assert.That(HtmlUtils.NormalizeHtmlClosingTags("<select><option>One<option>Two</select>"),
                Is.EqualTo("<select><option>One</option><option>Two</option></select>"));
        }

        [Test]
        public void VoidElements_NotAffected()
        {
            // Void elements should pass through unchanged
            Assert.That(HtmlUtils.NormalizeHtmlClosingTags("<p>Hello<br>World</p>"), Is.EqualTo("<p>Hello<br>World</p>"));
            Assert.That(HtmlUtils.NormalizeHtmlClosingTags("<p>Image: <img src=\"test.jpg\"></p>"), 
                Is.EqualTo("<p>Image: <img src=\"test.jpg\"></p>"));
        }

        [Test]
        public void SelfClosingElements()
        {
            Assert.That(HtmlUtils.NormalizeHtmlClosingTags("<p>Hello<br />World</p>"), Is.EqualTo("<p>Hello<br />World</p>"));
        }

        [Test]
        public void PreservesAttributes()
        {
            Assert.That(HtmlUtils.NormalizeHtmlClosingTags("<p class=\"intro\" id=\"first\">Hello"),
                Is.EqualTo("<p class=\"intro\" id=\"first\">Hello</p>"));
        }

        [Test]
        public void PreservesWhitespaceAndComments()
        {
            Assert.That(HtmlUtils.NormalizeHtmlClosingTags("<p>Hello\n  World"), Is.EqualTo("<p>Hello\n  World</p>"));
            Assert.That(HtmlUtils.NormalizeHtmlClosingTags("<p>Hello<!-- comment -->World"), 
                Is.EqualTo("<p>Hello<!-- comment -->World</p>"));
        }

        [Test]
        public void CaseInsensitive()
        {
            Assert.That(HtmlUtils.NormalizeHtmlClosingTags("<P>Hello"), Is.EqualTo("<P>Hello</P>"));
            Assert.That(HtmlUtils.NormalizeHtmlClosingTags("<P>First<p>Second"), Is.EqualTo("<P>First</P><p>Second</p>"));
        }

        [Test]
        public void NonOptionalElements_NotAffected()
        {
            // Elements that require closing tags should pass through unchanged
            Assert.That(HtmlUtils.NormalizeHtmlClosingTags("<div><span>Text</span></div>"), 
                Is.EqualTo("<div><span>Text</span></div>"));
        }

        [Test]
        public void NestedElements()
        {
            // Paragraph inside div
            Assert.That(HtmlUtils.NormalizeHtmlClosingTags("<div><p>Paragraph</div>"), 
                Is.EqualTo("<div><p>Paragraph</p></div>"));
        }

        [Test]
        public void BlockElementClosesOpenParagraph()
        {
            // Per HTML5 spec, a P element's end tag can be omitted if immediately followed by block-level elements
            Assert.That(HtmlUtils.NormalizeHtmlClosingTags("<p>Text<div>Content</div>"),
                Is.EqualTo("<p>Text</p><div>Content</div>"));
            Assert.That(HtmlUtils.NormalizeHtmlClosingTags("<p>Text<table><tr><td>Cell</td></tr></table>"),
                Is.EqualTo("<p>Text</p><table><tr><td>Cell</td></tr></table>"));
            Assert.That(HtmlUtils.NormalizeHtmlClosingTags("<p>Before<ul><li>Item</ul>"),
                Is.EqualTo("<p>Before</p><ul><li>Item</li></ul>"));
            Assert.That(HtmlUtils.NormalizeHtmlClosingTags("<p>Before<h1>Heading</h1>"),
                Is.EqualTo("<p>Before</p><h1>Heading</h1>"));
        }

        [Test]
        public void NestedTables()
        {
            // Nested tables should only close tags within their own scope
            Assert.That(HtmlUtils.NormalizeHtmlClosingTags("<table><tr><td><table><tr><td>Inner</table></td></tr></table>"),
                Is.EqualTo("<table><tr><td><table><tr><td>Inner</td></tr></table></td></tr></table>"));
        }

        [Test]
        public void ComplexTable()
        {
            string input = "<table><thead><tr><th>H</th></tr><tbody><tr><td>D</td></tr></table>";
            string result = HtmlUtils.NormalizeHtmlClosingTags(input);
            Assert.That(result, Is.EqualTo("<table><thead><tr><th>H</th></tr></thead><tbody><tr><td>D</td></tr></tbody></table>"));
        }

        [Test]
        public void RealWorldPasteScenario()
        {
            // Simulates actual MSHTML output when pasting content
            string input = "<p>This is the first paragraph.<p>This is the second paragraph.<p>And this is the third.";
            string result = HtmlUtils.NormalizeHtmlClosingTags(input);
            Assert.That(result, Is.EqualTo("<p>This is the first paragraph.</p><p>This is the second paragraph.</p><p>And this is the third.</p>"));
        }
    }
}
