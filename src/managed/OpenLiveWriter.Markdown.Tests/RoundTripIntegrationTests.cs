// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.App.Avalonia.Settings;
using OpenLiveWriter.Markdown;
using OpenLiveWriter.Publishing;
using OpenLiveWriter.Publishing.Drafts;

namespace OpenLiveWriter.Markdown.Tests
{
    [TestFixture]
    public class RoundTripIntegrationTests
    {
        private string _tempDir;

        [SetUp]
        public void SetUp()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "olw-md-rt-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                if (Directory.Exists(_tempDir))
                    Directory.Delete(_tempDir, recursive: true);
            }
            catch
            {
                // best effort
            }
        }

        [Test]
        public void DraftSession_SaveOpen_PreservesMarkdownBody()
        {
            var session = new DraftSession(new FileDraftStore(_tempDir));
            const string markdown = "# Hello\n\n**world**";

            session.Save(
                "Markdown post",
                bodyMarkdown: markdown,
                bodyFormat: ContentFormat.Markdown);
            string id = session.Current.Id;

            session.NewPost();
            Assert.That(session.Open(id), Is.True);
            Assert.That(session.Current.BodyFormat, Is.EqualTo(ContentFormat.Markdown));
            Assert.That(session.Current.BodyMarkdown, Is.EqualTo(markdown));
            Assert.That(session.Current.Title, Is.EqualTo("Markdown post"));
        }

        [Test]
        public void DraftSession_UpdateBody_Markdown_SetsFormatAndCanonicalBody()
        {
            var session = new DraftSession(new FileDraftStore(_tempDir));
            session.UpdateBody("# Title", ContentFormat.Markdown);

            Assert.That(session.Current.BodyFormat, Is.EqualTo(ContentFormat.Markdown));
            Assert.That(session.Current.BodyMarkdown, Is.EqualTo("# Title"));
            Assert.That(session.IsDirty, Is.True);
        }

        [Test]
        public async Task AutosaveController_MarkdownDraft_PersistsBodyMarkdown()
        {
            var session = new DraftSession(new FileDraftStore(_tempDir));
            session.UpdateBody("initial", ContentFormat.Markdown);

            const string updated = "## Section\n\nUpdated body.";
            var prefs = new AppPreferences { AutoSaveDrafts = true, AutoSaveMinutes = 5 };
            var controller = new AutosaveController(
                session,
                () => prefs,
                () => Task.FromResult((
                    "Autosaved title",
                    ContentFormat.Markdown,
                    (string)null,
                    updated)));

            bool saved = await controller.TickAsync();

            Assert.That(saved, Is.True);
            Assert.That(session.ListDrafts().Count, Is.EqualTo(1));
            PostDocument loaded = new FileDraftStore(_tempDir).Load(session.Current.Id);
            Assert.That(loaded.BodyFormat, Is.EqualTo(ContentFormat.Markdown));
            Assert.That(loaded.BodyMarkdown, Is.EqualTo(updated));
            Assert.That(loaded.Title, Is.EqualTo("Autosaved title"));
        }

        [Test]
        public void DesignHtmlFromMarkdownDocument_MatchesMarkdownService()
        {
            var markdown = new MarkdownService();
            var controller = new MarkdownEditingController(markdown);
            controller.SetContentFormat(ContentFormat.Markdown);

            const string source = "# Heading\n\nA paragraph.";
            string html = controller.HtmlFromCanonical(source);

            Assert.That(html, Does.Contain("<h1"));
            Assert.That(html, Does.Contain("Heading"));
            Assert.That(markdown.ToMarkdown(html), Is.EqualTo(source));
        }
    }
}
