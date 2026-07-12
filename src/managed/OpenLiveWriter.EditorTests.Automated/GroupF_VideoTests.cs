// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.EditorTests.Automated.Infrastructure;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group F — Insert Video (modern web embed). URL normalization
    /// (YouTube/Vimeo/generic + pasted iframe) and the responsive embed HTML are
    /// produced by the pure <see cref="VideoEmbedBuilder"/>, asserted headlessly on
    /// the parsed DOM. The live insertion runs inside the WebView (manual bench).
    /// </summary>
    [TestFixture]
    [Category("GroupF")]
    public class GroupF_VideoTests
    {
        [TestCase("https://www.youtube.com/watch?v=dQw4w9WgXcQ", "https://www.youtube.com/embed/dQw4w9WgXcQ")]
        [TestCase("https://youtu.be/dQw4w9WgXcQ", "https://www.youtube.com/embed/dQw4w9WgXcQ")]
        [TestCase("https://www.youtube.com/embed/dQw4w9WgXcQ", "https://www.youtube.com/embed/dQw4w9WgXcQ")]
        [TestCase("https://www.youtube.com/shorts/dQw4w9WgXcQ", "https://www.youtube.com/embed/dQw4w9WgXcQ")]
        [TestCase("https://www.youtube.com/watch?v=dQw4w9WgXcQ&t=42s", "https://www.youtube.com/embed/dQw4w9WgXcQ")]
        [TestCase("https://vimeo.com/123456789", "https://player.vimeo.com/video/123456789")]
        [TestCase("https://player.vimeo.com/video/123456789", "https://player.vimeo.com/video/123456789")]
        public void Video_NormalizesWatchUrlToEmbedUrl(string input, string expected)
        {
            Assert.That(VideoEmbedBuilder.NormalizeToEmbedUrl(input), Is.EqualTo(expected));
        }

        [TestCase("https://example.com/page")]
        [TestCase("not a url")]
        [TestCase("")]
        [TestCase(null)]
        public void Video_NormalizeReturnsNullForNonProviderUrls(string input)
        {
            Assert.That(VideoEmbedBuilder.NormalizeToEmbedUrl(input), Is.Null);
        }

        [Test]
        public void Video_BuildsResponsiveIframeEmbed()
        {
            string html = VideoEmbedBuilder.BuildEmbedHtml("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
            var doc = Dom.Parse(html);
            var iframe = doc.QuerySelector("iframe");

            Assert.Multiple(() =>
            {
                Assert.That(doc.QuerySelector("div.olw-video"), Is.Not.Null, "expected a responsive wrapper");
                Assert.That(iframe, Is.Not.Null);
                Assert.That(iframe.GetAttribute("src"), Is.EqualTo("https://www.youtube.com/embed/dQw4w9WgXcQ"));
                // Responsive container uses a 16:9 padding-bottom trick.
                Assert.That(doc.QuerySelector("div.olw-video").GetAttribute("style"), Does.Contain("56.25%"));
            });
        }

        [Test]
        public void Video_ExtractsSrcFromPastedIframe_AndNormalizes()
        {
            string pasted = "<iframe width=\"560\" height=\"315\" src=\"https://www.youtube.com/embed/abc123XYZ\" " +
                            "frameborder=\"0\" allowfullscreen></iframe>";
            string html = VideoEmbedBuilder.BuildEmbedHtml(pasted);
            var iframe = Dom.Parse(html).QuerySelector("iframe");

            Assert.That(iframe, Is.Not.Null);
            Assert.That(iframe.GetAttribute("src"), Is.EqualTo("https://www.youtube.com/embed/abc123XYZ"));
            // The boolean allowfullscreen from the pasted snippet is re-emitted with a value.
            Assert.That(iframe.GetAttribute("allowfullscreen"), Is.EqualTo("true"));
        }

        [Test]
        public void Video_ProtocolRelativeIframeSrcGetsHttps()
        {
            string pasted = "<iframe src=\"//player.vimeo.com/video/987654321\"></iframe>";
            string html = VideoEmbedBuilder.BuildEmbedHtml(pasted);
            var iframe = Dom.Parse(html).QuerySelector("iframe");
            Assert.That(iframe.GetAttribute("src"), Is.EqualTo("https://player.vimeo.com/video/987654321"));
        }

        [Test]
        public void Video_GenericHttpUrlEmbedsDirectly()
        {
            string html = VideoEmbedBuilder.BuildEmbedHtml("https://videos.example.com/embed/xyz");
            var iframe = Dom.Parse(html).QuerySelector("iframe");
            Assert.That(iframe.GetAttribute("src"), Is.EqualTo("https://videos.example.com/embed/xyz"));
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase(null)]
        [TestCase("just some text, not a url")]
        public void Video_ReturnsNullForUnembeddableInput(string input)
        {
            Assert.That(VideoEmbedBuilder.BuildEmbedHtml(input), Is.Null);
        }

        [Test]
        public void Video_EmbedIsWellFormed()
        {
            string html = VideoEmbedBuilder.BuildEmbedHtml("https://vimeo.com/123456789");
            Assert.That(HtmlWellFormednessGate.IsWellFormed(html), Is.True, html);
        }
    }
}
