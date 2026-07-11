// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using NUnit.Framework;
using OpenLiveWriter.EditorTests.Automated.Infrastructure;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group A16 — the publish-readiness gate. After a battery of editor commands
    /// the produced HTML must be well-formed and XML-safe so it can be embedded in
    /// a MetaWeblog XML-RPC payload. Here the gate is exercised against
    /// representative editor-output samples (no live WebView needed); the live
    /// end-to-end variant lives in GroupA_EditorCommandTests (WebView category).
    /// </summary>
    [TestFixture]
    [Category("GroupA")]
    public class GroupA_WellFormednessTests
    {
        // Representative of what document.execCommand produces after a battery of
        // formatting commands: nested inline tags, lists, a heading, a link, a rule.
        private const string BatteryOutput =
            "<h2>Title</h2>" +
            "<p><b>Bold</b> <i>italic</i> <u>under</u> <strike>strike</strike> " +
            "<sub>sub</sub><sup>sup</sup></p>" +
            "<blockquote><p>Quoted text</p></blockquote>" +
            "<ul><li>one</li><li>two</li></ul>" +
            "<ol><li>first</li><li>second</li></ol>" +
            "<p style=\"text-align:center\">Centered</p>" +
            "<p><a href=\"https://example.com/?a=1&amp;b=2\" title=\"t\">link</a></p>" +
            "<hr>" +
            "<p>Done&nbsp;&mdash; ok.</p>";

        [Test]
        public void BatteryOutput_IsWellFormedAndPublishReady()
        {
            var result = HtmlWellFormednessGate.Validate(BatteryOutput);
            Assert.That(result.IsWellFormed, Is.True, result.ToString());
        }

        [Test]
        public void BatteryOutput_AllTagsParseAndClose()
        {
            var doc = Dom.Parse(BatteryOutput);
            Assert.Multiple(() =>
            {
                Assert.That(doc.QuerySelector("h2"), Is.Not.Null);
                Assert.That(doc.QuerySelector("blockquote p"), Is.Not.Null);
                Assert.That(doc.QuerySelectorAll("ul > li"), Has.Length.EqualTo(2));
                Assert.That(doc.QuerySelectorAll("ol > li"), Has.Length.EqualTo(2));
                Assert.That(doc.QuerySelector("a").GetAttribute("href"),
                    Is.EqualTo("https://example.com/?a=1&b=2"));
                Assert.That(doc.QuerySelector("hr"), Is.Not.Null);
            });
        }

        [Test]
        public void SelfClosingVoidElements_AreWellFormed()
        {
            Assert.That(HtmlWellFormednessGate.IsWellFormed("<p>a<br>b</p><hr>"), Is.True);
            Assert.That(HtmlWellFormednessGate.IsWellFormed("<p>a<br/>b</p><hr/>"), Is.True);
            Assert.That(HtmlWellFormednessGate.IsWellFormed(
                "<p><img src=\"x.png\" alt=\"pic\"></p>"), Is.True);
        }

        [Test]
        public void CommonNamedEntities_AreWellFormed()
        {
            Assert.That(HtmlWellFormednessGate.IsWellFormed(
                "<p>A&nbsp;B &copy; 2026 &mdash; &ldquo;quote&rdquo;</p>"), Is.True);
        }

        [Test]
        public void UnclosedTag_IsRejected()
        {
            var result = HtmlWellFormednessGate.Validate("<p>oops");
            Assert.That(result.IsWellFormed, Is.False);
        }

        [Test]
        public void MisnestedTags_AreRejected()
        {
            var result = HtmlWellFormednessGate.Validate("<b><i>x</b></i>");
            Assert.That(result.IsWellFormed, Is.False);
        }

        [Test]
        public void UnescapedAmpersand_IsRejected()
        {
            // A bare & (not part of an entity) is not XML-safe.
            var result = HtmlWellFormednessGate.Validate("<p>Tom & Jerry</p>");
            Assert.That(result.IsWellFormed, Is.False);
        }

        [Test]
        public void InvalidXmlControlChar_IsRejected()
        {
            var result = HtmlWellFormednessGate.Validate("<p>bad\u0001char</p>");
            Assert.That(result.IsWellFormed, Is.False);
            Assert.That(result.Errors[0], Does.Contain("invalid XML character"));
        }

        [Test]
        public void EmptyContent_IsWellFormed()
        {
            Assert.That(HtmlWellFormednessGate.IsWellFormed(""), Is.True);
            Assert.That(HtmlWellFormednessGate.IsWellFormed(null), Is.True);
        }
    }
}
