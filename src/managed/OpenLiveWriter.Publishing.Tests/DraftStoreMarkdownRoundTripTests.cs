// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.IO;
using NUnit.Framework;
using OpenLiveWriter.Publishing.Drafts;

namespace OpenLiveWriter.Publishing.Tests
{
    [TestFixture]
    public class DraftStoreMarkdownRoundTripTests
    {
        private string _tempDir;

        [SetUp]
        public void SetUp()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "olw-draft-md-" + Path.GetRandomFileName());
            Directory.CreateDirectory(_tempDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }

        [Test]
        public void SaveAndReload_PreservesBodyFormatAndBodyMarkdown()
        {
            var store = new FileDraftStore(_tempDir);
            var original = new PostDocument
            {
                Title = "Markdown draft",
                BodyFormat = ContentFormat.Markdown,
                BodyMarkdown = "# Title\n\nParagraph with **emphasis**.",
                BodyHtml = "<h1>Title</h1><p>Paragraph with <strong>emphasis</strong>.</p>"
            };

            PostDocument saved = store.Save(original);
            PostDocument loaded = store.Load(saved.Id);

            Assert.That(loaded.BodyFormat, Is.EqualTo(ContentFormat.Markdown));
            Assert.That(loaded.BodyMarkdown, Is.EqualTo(original.BodyMarkdown));
            Assert.That(loaded.BodyHtml, Is.EqualTo(original.BodyHtml));
            Assert.That(loaded.Title, Is.EqualTo(original.Title));
        }

        [Test]
        public void OverwriteExistingDraft_PreservesMarkdownFields()
        {
            var store = new FileDraftStore(_tempDir);
            PostDocument saved = store.Save(new PostDocument
            {
                Title = "Version 1",
                BodyFormat = ContentFormat.Markdown,
                BodyMarkdown = "v1"
            });

            saved.Title = "Version 2";
            saved.BodyMarkdown = "## Updated\n\n- one\n- two";
            store.Save(saved);

            PostDocument loaded = store.Load(saved.Id);

            Assert.That(loaded.Title, Is.EqualTo("Version 2"));
            Assert.That(loaded.BodyFormat, Is.EqualTo(ContentFormat.Markdown));
            Assert.That(loaded.BodyMarkdown, Is.EqualTo("## Updated\n\n- one\n- two"));
        }

        [Test]
        public void HtmlDraftRoundTrip_UnchangedByMarkdownStore()
        {
            var store = new FileDraftStore(_tempDir);
            var original = new PostDocument
            {
                Title = "HTML draft",
                BodyFormat = ContentFormat.Html,
                BodyHtml = "<p>Hello <strong>world</strong></p>"
            };

            PostDocument saved = store.Save(original);
            PostDocument loaded = store.Load(saved.Id);

            Assert.That(loaded.BodyFormat, Is.EqualTo(ContentFormat.Html));
            Assert.That(loaded.BodyHtml, Is.EqualTo(original.BodyHtml));
            Assert.That(loaded.BodyMarkdown, Is.EqualTo(string.Empty));
        }
    }
}
