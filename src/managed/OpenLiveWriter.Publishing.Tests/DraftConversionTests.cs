// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.IO;
using NUnit.Framework;
using OpenLiveWriter.Markdown;
using OpenLiveWriter.Publishing.Drafts;

namespace OpenLiveWriter.Publishing.Tests
{
    [TestFixture]
    public class DraftConversionTests
    {
        private sealed class FakeMarkdownService : IMarkdownService
        {
            public string ToHtml(string markdown) => markdown ?? string.Empty;

            public string ToMarkdown(string html) => "md:" + (html ?? string.Empty);
        }

        private static string CreateTempDraftDir()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "olw-draft-conversion-" + Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            return tempDir;
        }

        [Test]
        public void ConvertBlogDraftsToMarkdown_ConvertsHtmlDraftsForMatchingBlog()
        {
            string tempDir = CreateTempDraftDir();
            try
            {
                var store = new FileDraftStore(tempDir);
                var markdown = new FakeMarkdownService();

                store.Save(new PostDocument
                {
                    BlogId = "blog1",
                    Title = "Html one",
                    BodyFormat = ContentFormat.Html,
                    BodyHtml = "<p>One</p>"
                });
                store.Save(new PostDocument
                {
                    BlogId = "blog1",
                    Title = "Html two",
                    BodyFormat = ContentFormat.Html,
                    BodyHtml = "<p>Two</p>"
                });
                store.Save(new PostDocument
                {
                    BlogId = "blog2",
                    Title = "Other blog",
                    BodyFormat = ContentFormat.Html,
                    BodyHtml = "<p>Other</p>"
                });
                PostDocument alreadyMarkdown = store.Save(new PostDocument
                {
                    BlogId = "blog1",
                    Title = "Already markdown",
                    BodyFormat = ContentFormat.Markdown,
                    BodyMarkdown = "# Done"
                });

                int converted = DraftConversion.ConvertBlogDraftsToMarkdown(store, "blog1", markdown);

                Assert.That(converted, Is.EqualTo(2));

                foreach (DraftInfo info in store.List())
                {
                    PostDocument loaded = store.Load(info.Id);
                    if (loaded.Title == "Already markdown")
                    {
                        Assert.That(loaded.BodyFormat, Is.EqualTo(ContentFormat.Markdown));
                        Assert.That(loaded.BodyMarkdown, Is.EqualTo("# Done"));
                        continue;
                    }

                    if (loaded.BlogId == "blog1")
                    {
                        Assert.That(loaded.BodyFormat, Is.EqualTo(ContentFormat.Markdown));
                        Assert.That(loaded.BodyMarkdown, Does.StartWith("md:"));
                    }
                    else if (loaded.BlogId == "blog2")
                    {
                        Assert.That(loaded.BodyFormat, Is.EqualTo(ContentFormat.Html));
                        Assert.That(loaded.BodyMarkdown, Is.EqualTo(string.Empty));
                    }
                }
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, recursive: true);
            }
        }

        [Test]
        public void ConvertBlogDraftsToMarkdown_TreatsLegacyHtmlBodyAsHtml()
        {
            string tempDir = CreateTempDraftDir();
            try
            {
                var store = new FileDraftStore(tempDir);
                var markdown = new FakeMarkdownService();

                PostDocument saved = store.Save(new PostDocument
                {
                    BlogId = "blog1",
                    BodyHtml = "<p>Legacy</p>"
                });

                int converted = DraftConversion.ConvertBlogDraftsToMarkdown(store, "blog1", markdown);

                Assert.That(converted, Is.EqualTo(1));
                PostDocument loaded = store.Load(saved.Id);
                Assert.That(loaded.BodyFormat, Is.EqualTo(ContentFormat.Markdown));
                Assert.That(loaded.BodyMarkdown, Is.EqualTo("md:<p>Legacy</p>"));
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, recursive: true);
            }
        }

        [Test]
        public void ConvertBlogDraftsToMarkdown_ReturnsZeroForEmptyBlogId()
        {
            var store = new FakeDraftStore();
            var markdown = new FakeMarkdownService();

            Assert.That(DraftConversion.ConvertBlogDraftsToMarkdown(store, string.Empty, markdown), Is.EqualTo(0));
            Assert.That(DraftConversion.ConvertBlogDraftsToMarkdown(store, null, markdown), Is.EqualTo(0));
        }

        [Test]
        public void HasDraftsForBlog_ReturnsTrueWhenDraftExists()
        {
            string tempDir = CreateTempDraftDir();
            try
            {
                var store = new FileDraftStore(tempDir);
                store.Save(new PostDocument { BlogId = "target", Title = "Draft" });

                Assert.That(DraftConversion.HasDraftsForBlog(store, "target"), Is.True);
                Assert.That(DraftConversion.HasDraftsForBlog(store, "other"), Is.False);
                Assert.That(DraftConversion.HasDraftsForBlog(store, string.Empty), Is.False);
                Assert.That(DraftConversion.HasDraftsForBlog(null, "target"), Is.False);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, recursive: true);
            }
        }

        private sealed class FakeDraftStore : IDraftStore
        {
            public PostDocument Save(PostDocument document) => document;
            public PostDocument Load(string id) => null;
            public System.Collections.Generic.IReadOnlyList<DraftInfo> List() =>
                System.Array.Empty<DraftInfo>();
            public void Delete(string id) { }
            public bool Exists(string id) => false;
        }
    }
}
