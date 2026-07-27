// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Editor;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// HTML span classification for the Source view's syntax highlighting:
    /// comments, tag names, attribute names/values, and elided-image tokens are
    /// tagged at the right offsets; plain text falls through untouched.
    /// </summary>
    [TestFixture]
    [Category("GroupB")]
    public class GroupB_HtmlSyntaxSpansTests
    {
        [Test]
        public void SimpleTag_TagNameAndAttributesClassified()
        {
            const string html = "<p class=\"intro\">hi</p>";
            var spans = HtmlSyntaxSpans.Compute(html);

            AssertSpan(html, spans, "p", HtmlSpanKind.TagName);
            AssertSpan(html, spans, "class", HtmlSpanKind.AttributeName);
            AssertSpan(html, spans, "\"intro\"", HtmlSpanKind.AttributeValue);
            AssertSpan(html, spans, "hi", HtmlSpanKind.Text);
        }

        [Test]
        public void Comment_ClassifiedAndContentsNotTagged()
        {
            const string html = "<p>a</p><!-- <b>not a tag</b> --><p>b</p>";
            var spans = HtmlSyntaxSpans.Compute(html);

            Assert.That(spans.Any(s => s.Kind == HtmlSpanKind.Comment), Is.True);
            Assert.That(spans.Count(s => s.Kind == HtmlSpanKind.TagName), Is.EqualTo(4),
                "tags inside the comment must not be classified (2 real <p> tags, open+close)");
        }

        [Test]
        public void UnterminatedComment_ConsumesToEnd()
        {
            var spans = HtmlSyntaxSpans.Compute("<p>x</p><!-- trailing");
            Assert.That(spans.Last().Kind, Is.EqualTo(HtmlSpanKind.Comment));
        }

        [Test]
        public void EmbeddedImageToken_GetsOwnKind()
        {
            const string html = "<img src=\"data-olw-img:0\" alt=\"photo\" />";
            var spans = HtmlSyntaxSpans.Compute(html);

            AssertSpan(html, spans, "\"data-olw-img:0\"", HtmlSpanKind.EmbeddedImageToken);
            AssertSpan(html, spans, "\"photo\"", HtmlSpanKind.AttributeValue);
        }

        [Test]
        public void ClosingTag_TagNameClassified()
        {
            const string html = "</blockquote>";
            AssertSpan(html, HtmlSyntaxSpans.Compute(html), "blockquote", HtmlSpanKind.TagName);
        }

        [Test]
        public void EmptyAndNull_Safe()
        {
            Assert.That(HtmlSyntaxSpans.Compute(null), Is.Empty);
            Assert.That(HtmlSyntaxSpans.Compute(""), Is.Empty);
            Assert.That(HtmlSyntaxSpans.Compute("plain text only"),
                Has.Count.EqualTo(1).And.All.Matches<HtmlSyntaxSpans.Span>(s => s.Kind == HtmlSpanKind.Text));
        }

        private static void AssertSpan(string html, List<HtmlSyntaxSpans.Span> spans,
            string fragment, HtmlSpanKind kind)
        {
            bool found = spans.Any(s =>
                s.Kind == kind && html.Substring(s.Start, s.Length) == fragment);
            Assert.That(found, Is.True,
                $"expected a {kind} span '{fragment}' in: {string.Join(", ", spans.Select(s => $"{s.Kind}@{s.Start}+{s.Length}"))}");
        }
    }
}
