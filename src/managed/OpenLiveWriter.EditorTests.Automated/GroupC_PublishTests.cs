// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using NUnit.Framework;
using OpenLiveWriter.EditorTests.Automated.Infrastructure;
using OpenLiveWriter.EditorTests.Automated.Publish;
using OpenLiveWriter.Publishing;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group C — publish HTML generation. These tests now drive the REAL
    /// cross-platform publish pipeline in <c>OpenLiveWriter.Publishing</c>
    /// (<see cref="BlogPost"/>, <see cref="EditorContentPublisher"/>,
    /// <see cref="MetaWeblogXmlRpcClient"/>, <see cref="XmlCharacterHelper"/>) —
    /// the ported types the Avalonia app references. <see cref="FakeBlogClient"/>
    /// stands in only for the network transport so NewPost/EditPost round-trips
    /// run offline. Payload assertions parse the ACTUAL MetaWeblog XML-RPC struct
    /// the client generates.
    /// </summary>
    [TestFixture]
    [Category("GroupC")]
    public class GroupC_PublishTests
    {
        private static MetaWeblogXmlRpcClient NewClient(IBlogClientOptions options = null) =>
            new MetaWeblogXmlRpcClient("http://example.test/xmlrpc", "user", "pass", options);

        // Reads the post <struct> (4th param) from a metaWeblog.newPost/editPost call.
        private static XmlNode PostStruct(string methodCallXml)
        {
            var doc = new XmlDocument();
            doc.LoadXml(methodCallXml);
            return doc.SelectSingleNode("/methodCall/params/param[4]/value/struct");
        }

        // Select the typed <string> element (not <value>) so indentation whitespace
        // between value/child elements is not folded into the asserted text.
        private static string StructMember(XmlNode postStruct, string name) =>
            postStruct.SelectSingleNode($"member[name='{name}']/value/string")?.InnerText;

        private static IEnumerable<string> StructCategories(XmlNode postStruct) =>
            postStruct.SelectNodes("member[name='categories']/value/array/data/value/string")
                      .Cast<XmlNode>()
                      .Select(n => n.InnerText);

        private static bool PublishFlag(string methodCallXml)
        {
            var doc = new XmlDocument();
            doc.LoadXml(methodCallXml);
            return doc.SelectSingleNode("/methodCall/params/param[5]/value/boolean")?.InnerText == "1";
        }

        [Test]
        public void BuildPost_MapsTitleAndMainContents()
        {
            BlogPost post = EditorContentPublisher.BuildPost(
                "My Title", "<p>Hello <b>world</b></p>", publish: true);

            Assert.That(post.Title, Is.EqualTo("My Title"));
            Assert.That(post.MainContents, Is.EqualTo("<p>Hello <b>world</b></p>"));
            Assert.That(post.ExtendedContents, Is.Empty);
        }

        [Test]
        public void Publish_MetaWeblogPayload_DescriptionEqualsMainContents_AndPublishFlagSet()
        {
            // Round-trip through the transport returns the server post id.
            var fake = new FakeBlogClient();
            string id = EditorContentPublisher.Publish(fake, "blog-1", "Post A", "<p>Body</p>", publish: true, "News");
            Assert.That(id, Is.EqualTo("fake-post-1"));
            Assert.That(fake.NewPostCount, Is.EqualTo(1));

            // Assert the REAL MetaWeblog XML-RPC payload the client would transmit.
            BlogPost post = EditorContentPublisher.BuildPost("Post A", "<p>Body</p>", publish: true, "News");
            string xml = NewClient().BuildNewPostXml("blog-1", post, publish: true);
            XmlNode postStruct = PostStruct(xml);

            Assert.That(StructMember(postStruct, "title"), Is.EqualTo("Post A"));
            Assert.That(StructMember(postStruct, "description"), Is.EqualTo("<p>Body</p>"));
            Assert.That(StructCategories(postStruct), Does.Contain("News"));
            Assert.That(PublishFlag(xml), Is.True);
        }

        [Test]
        public void Publish_AsDraft_SetsPublishFalse()
        {
            var fake = new FakeBlogClient();
            EditorContentPublisher.Publish(fake, "blog-1", "Draft", "<p>WIP</p>", publish: false);

            Assert.That(fake.LastPublish, Is.False);
            Assert.That(fake.LastPost.IsPublished, Is.False);

            BlogPost post = EditorContentPublisher.BuildPost("Draft", "<p>WIP</p>", publish: false);
            string xml = NewClient().BuildNewPostXml("blog-1", post, publish: false);
            Assert.That(PublishFlag(xml), Is.False);
        }

        [Test]
        public void ExtendedEntrySplit_PopulatesMainAndTextMore()
        {
            var html = "<p>Intro paragraph</p>" + ExtendedEntry.BreakMarker + "<p>Rest of the post</p>";
            BlogPost post = EditorContentPublisher.BuildPost("Split", html, publish: true);

            string xml = NewClient().BuildNewPostXml("blog-1", post, publish: true);
            XmlNode postStruct = PostStruct(xml);

            Assert.That(StructMember(postStruct, "description"), Is.EqualTo("<p>Intro paragraph</p>"));
            Assert.That(StructMember(postStruct, "mt_text_more"), Is.EqualTo("<p>Rest of the post</p>"));
        }

        [Test]
        public void ExtendedEntrySplit_NoBreak_LeavesTextMoreEmpty()
        {
            var (main, extended) = ExtendedEntry.Split("<p>All in one</p>");
            Assert.That(main, Is.EqualTo("<p>All in one</p>"));
            Assert.That(extended, Is.Empty);
        }

        // --- XmlCharacterHelper scrubbing (the REAL ported helper) ---

        [Test]
        public void XmlScrub_RemovesInvalidControlChars()
        {
            var input = "Hello\u0000\u0001\u0008World\u001F!";
            var scrubbed = XmlCharacterHelper.RemoveInvalidXmlChars(input);
            Assert.That(scrubbed, Is.EqualTo("HelloWorld!"));
        }

        [Test]
        public void XmlScrub_KeepsValidWhitespaceAndUnicode()
        {
            var input = "Line1\tLine1b\nLine2\r\nCaf\u00e9 \u2014 done";
            var scrubbed = XmlCharacterHelper.RemoveInvalidXmlChars(input);
            Assert.That(scrubbed, Is.EqualTo(input));
        }

        [Test]
        public void XmlScrub_ContractMatchesXmlCharRanges()
        {
            Assert.Multiple(() =>
            {
                Assert.That(XmlCharacterHelper.IsValidXmlChar('\t'), Is.True);
                Assert.That(XmlCharacterHelper.IsValidXmlChar('\n'), Is.True);
                Assert.That(XmlCharacterHelper.IsValidXmlChar('\r'), Is.True);
                Assert.That(XmlCharacterHelper.IsValidXmlChar(' '), Is.True);
                Assert.That(XmlCharacterHelper.IsValidXmlChar('\u0000'), Is.False);
                Assert.That(XmlCharacterHelper.IsValidXmlChar('\u0008'), Is.False);
                Assert.That(XmlCharacterHelper.IsValidXmlChar('\uFFFF'), Is.False);
            });
        }

        [Test]
        public void Publish_ScrubbedHtml_IsWellFormedForXmlRpc()
        {
            var html = "<p>ok\u0001body</p>";
            BlogPost post = EditorContentPublisher.BuildPost("t", html, publish: true);
            Assert.That(HtmlWellFormednessGate.IsWellFormed(post.MainContents), Is.True,
                "scrubbed description must be XML-RPC safe");
        }

        // ---------------------------------------------------------------------
        // Real-pipeline probes — now GREEN because BlogClient's publish slice is
        // ported to OpenLiveWriter.Publishing and referenced from the Avalonia app.
        // ---------------------------------------------------------------------

        [Test]
        [Category(WebViewCategories.PublishTdd)]
        public void RealPipeline_HasCrossPlatformPostModel()
        {
            var appAsm = typeof(OpenLiveWriter.App.Avalonia.Editor.WebViewEditor).Assembly;
            var referenced = appAsm.GetReferencedAssemblies().Select(a => a.Name);
            Assert.That(referenced, Does.Contain("OpenLiveWriter.Publishing"),
                "Avalonia app must reference the ported publish pipeline.");
        }

        [Test]
        [Category(WebViewCategories.PublishTdd)]
        public void RealPipeline_ExposesPublishCommandOnEditor()
        {
            var editorType = typeof(OpenLiveWriter.App.Avalonia.Editor.WebViewEditor);
            bool hasPublishMethod = editorType.GetMethods()
                .Any(m => m.Name == "PublishAsync" || m.Name == "PostAndPublishAsync");
            Assert.That(hasPublishMethod, Is.True,
                "WebViewEditor should expose a publish entry point once the pipeline is ported.");
        }
    }
}
