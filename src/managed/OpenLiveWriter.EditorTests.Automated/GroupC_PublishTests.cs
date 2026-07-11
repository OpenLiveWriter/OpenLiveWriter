// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Linq;
using NUnit.Framework;
using OpenLiveWriter.EditorTests.Automated.Infrastructure;
using OpenLiveWriter.EditorTests.Automated.Publish;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group C — publish HTML generation. The Windows pipeline (editor HTML →
    /// BlogPost → IBlogClient MetaWeblog struct) is net10.0-windows and not yet
    /// ported. These tests pin the cross-platform CONTRACT the port must satisfy,
    /// driving a <see cref="FakeBlogClient"/> transport. They run and pass today so
    /// the seam is proven; the [Explicit] probes below fail until the real
    /// production pipeline exists (TDD targets).
    /// </summary>
    [TestFixture]
    [Category("GroupC")]
    public class GroupC_PublishTests
    {
        [Test]
        public void BuildPost_MapsTitleAndMainContents()
        {
            var post = EditorHtmlPublisher.BuildPost(
                "My Title", "<p>Hello <b>world</b></p>", publish: true);

            Assert.That(post.Title, Is.EqualTo("My Title"));
            Assert.That(post.MainContents, Is.EqualTo("<p>Hello <b>world</b></p>"));
            Assert.That(post.ExtendedContents, Is.Empty);
        }

        [Test]
        public void Publish_MetaWeblogPayload_DescriptionEqualsMainContents_AndPublishFlagSet()
        {
            var client = new FakeBlogClient();
            var id = EditorHtmlPublisher.Publish(
                client, "blog-1", "Post A", "<p>Body</p>", publish: true, "News");

            Assert.That(id, Is.EqualTo("fake-post-1"));
            Assert.That(client.NewPostCount, Is.EqualTo(1));
            Assert.That(client.LastPayload.Title, Is.EqualTo("Post A"));
            Assert.That(client.LastPayload.Description, Is.EqualTo("<p>Body</p>"));
            Assert.That(client.LastPayload.Publish, Is.True);
            Assert.That(client.LastPayload.Categories, Does.Contain("News"));
        }

        [Test]
        public void Publish_AsDraft_SetsPublishFalse()
        {
            var client = new FakeBlogClient();
            EditorHtmlPublisher.Publish(client, "blog-1", "Draft", "<p>WIP</p>", publish: false);

            Assert.That(client.LastPayload.Publish, Is.False);
            Assert.That(client.LastPost.IsPublished, Is.False);
        }

        [Test]
        public void ExtendedEntrySplit_PopulatesMainAndTextMore()
        {
            var client = new FakeBlogClient();
            var html = "<p>Intro paragraph</p>" + ExtendedEntry.BreakMarker + "<p>Rest of the post</p>";

            EditorHtmlPublisher.Publish(client, "blog-1", "Split", html, publish: true);

            Assert.That(client.LastPayload.Description, Is.EqualTo("<p>Intro paragraph</p>"));
            Assert.That(client.LastPayload.TextMore, Is.EqualTo("<p>Rest of the post</p>"));
        }

        [Test]
        public void ExtendedEntrySplit_NoBreak_LeavesTextMoreEmpty()
        {
            var (main, extended) = ExtendedEntry.Split("<p>All in one</p>");
            Assert.That(main, Is.EqualTo("<p>All in one</p>"));
            Assert.That(extended, Is.Empty);
        }

        // --- XmlCharacterHelper-style scrubbing (runnable unit test of the contract) ---

        [Test]
        public void XmlScrub_RemovesInvalidControlChars()
        {
            var input = "Hello\u0000\u0001\u0008World\u001F!";
            var scrubbed = XmlCharacterScrubber.RemoveInvalidXmlChars(input);
            Assert.That(scrubbed, Is.EqualTo("HelloWorld!"));
        }

        [Test]
        public void XmlScrub_KeepsValidWhitespaceAndUnicode()
        {
            var input = "Line1\tLine1b\nLine2\r\nCaf\u00e9 \u2014 done";
            var scrubbed = XmlCharacterScrubber.RemoveInvalidXmlChars(input);
            Assert.That(scrubbed, Is.EqualTo(input));
        }

        [Test]
        public void XmlScrub_ContractMatchesXmlCharRanges()
        {
            Assert.Multiple(() =>
            {
                Assert.That(XmlCharacterScrubber.IsValidXmlChar('\t'), Is.True);
                Assert.That(XmlCharacterScrubber.IsValidXmlChar('\n'), Is.True);
                Assert.That(XmlCharacterScrubber.IsValidXmlChar('\r'), Is.True);
                Assert.That(XmlCharacterScrubber.IsValidXmlChar(' '), Is.True);
                Assert.That(XmlCharacterScrubber.IsValidXmlChar('\u0000'), Is.False);
                Assert.That(XmlCharacterScrubber.IsValidXmlChar('\u0008'), Is.False);
                Assert.That(XmlCharacterScrubber.IsValidXmlChar('\uFFFF'), Is.False);
            });
        }

        [Test]
        public void Publish_ScrubbedHtml_IsWellFormedForXmlRpc()
        {
            var client = new FakeBlogClient();
            var html = "<p>ok\u0001body</p>";
            EditorHtmlPublisher.Publish(client, "b", "t", html, publish: true);
            Assert.That(HtmlWellFormednessGate.IsWellFormed(client.LastPayload.Description), Is.True,
                "scrubbed description must be XML-RPC safe");
        }

        // ---------------------------------------------------------------------
        // TDD targets — fail until BlogClient/PostEditor are ported off WinForms
        // to a cross-platform assembly referenceable from the Avalonia app.
        // Run with:  dotnet test --filter "Category=PublishTdd"
        // ---------------------------------------------------------------------

        [Test]
        [Explicit("Blocked on BlogClient/PostEditor cross-platform port")]
        [Category(WebViewCategories.PublishTdd)]
        public void RealPipeline_HasCrossPlatformPostModel()
        {
            var appAsm = typeof(OpenLiveWriter.App.Avalonia.Editor.WebViewEditor).Assembly;
            var referenced = appAsm.GetReferencedAssemblies().Select(a => a.Name);
            Assert.That(referenced, Does.Contain("OpenLiveWriter.BlogClient").Or.Contain("OpenLiveWriter.PostEditor"),
                "Avalonia app must reference a ported publish pipeline before real publish tests can run.");
        }

        [Test]
        [Explicit("Blocked on BlogClient/PostEditor cross-platform port")]
        [Category(WebViewCategories.PublishTdd)]
        public void RealPipeline_ExposesPublishCommandOnEditor()
        {
            var editorType = typeof(OpenLiveWriter.App.Avalonia.Editor.WebViewEditor);
            var publishMethod = editorType.GetMethod("PublishAsync")
                ?? editorType.GetMethod("PostAndPublishAsync");
            Assert.That(publishMethod, Is.Not.Null,
                "WebViewEditor should expose a publish entry point once the pipeline is ported.");
        }
    }
}
