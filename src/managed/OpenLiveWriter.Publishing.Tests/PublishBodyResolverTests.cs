// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using NUnit.Framework;
using OpenLiveWriter.Markdown;

namespace OpenLiveWriter.Publishing.Tests
{
    [TestFixture]
    public class PublishBodyResolverTests
    {
        private sealed class StubMarkdownService : IMarkdownService
        {
            public string ToHtml(string markdown) => $"<html-from-md>{markdown}</html-from-md>";

            public string ToMarkdown(string html) => $"# md-from-html\n\n{html}";
        }

        private StubMarkdownService _markdown;

        [SetUp]
        public void SetUp()
        {
            _markdown = new StubMarkdownService();
        }

        [Test]
        public void Resolve_HtmlBody_HtmlPublish_ReturnsCanonicalBody()
        {
            const string body = "<p>Hello</p>";

            string result = PublishBodyResolver.Resolve(body, ContentFormat.Html, ContentFormat.Html, _markdown);

            Assert.That(result, Is.EqualTo(body));
        }

        [Test]
        public void Resolve_MarkdownBody_HtmlPublish_ConvertsToHtml()
        {
            const string body = "# Hello";

            string result = PublishBodyResolver.Resolve(body, ContentFormat.Markdown, ContentFormat.Html, _markdown);

            Assert.That(result, Is.EqualTo("<html-from-md># Hello</html-from-md>"));
        }

        [Test]
        public void Resolve_MarkdownBody_MarkdownPublish_ReturnsCanonicalBody()
        {
            const string body = "# Hello";

            string result = PublishBodyResolver.Resolve(body, ContentFormat.Markdown, ContentFormat.Markdown, _markdown);

            Assert.That(result, Is.EqualTo(body));
        }

        [Test]
        public void Resolve_HtmlBody_MarkdownPublish_ConvertsToMarkdown()
        {
            const string body = "<p>Hello</p>";

            string result = PublishBodyResolver.Resolve(body, ContentFormat.Html, ContentFormat.Markdown, _markdown);

            Assert.That(result, Is.EqualTo("# md-from-html\n\n<p>Hello</p>"));
        }

        [Test]
        public void Resolve_NullBody_TreatsAsEmpty()
        {
            string result = PublishBodyResolver.Resolve(null, ContentFormat.Html, ContentFormat.Html, _markdown);

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Resolve_EmptyBody_ReturnsEmpty()
        {
            string result = PublishBodyResolver.Resolve(string.Empty, ContentFormat.Html, ContentFormat.Html, _markdown);

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Resolve_MarkdownToHtml_NullMarkdownService_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                PublishBodyResolver.Resolve("# Hi", ContentFormat.Markdown, ContentFormat.Html, null));
        }

        [Test]
        public void Resolve_HtmlToMarkdown_NullMarkdownService_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                PublishBodyResolver.Resolve("<p>Hi</p>", ContentFormat.Html, ContentFormat.Markdown, null));
        }
    }
}
