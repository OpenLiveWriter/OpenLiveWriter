// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.IO;
using System.Text.Json;
using NUnit.Framework;
using OpenLiveWriter.Publishing.Drafts;

namespace OpenLiveWriter.Publishing.Tests
{
    [TestFixture]
    public class PostDocumentMarkdownTests
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        [Test]
        public void Defaults_BodyFormatIsHtml_AndBodyMarkdownEmpty()
        {
            var doc = new PostDocument();
            Assert.That(doc.BodyFormat, Is.EqualTo(ContentFormat.Html));
            Assert.That(doc.BodyMarkdown, Is.EqualTo(string.Empty));
        }

        [Test]
        public void JsonRoundTrip_PreservesBodyFormatAndBodyMarkdown()
        {
            var original = new PostDocument
            {
                Title = "Markdown post",
                BodyFormat = ContentFormat.Markdown,
                BodyMarkdown = "# Hello\n\n**world**",
                BodyHtml = "<h1>Hello</h1><p><strong>world</strong></p>"
            };

            string json = JsonSerializer.Serialize(original, SerializerOptions);
            var restored = JsonSerializer.Deserialize<PostDocument>(json, SerializerOptions);

            Assert.That(restored.BodyFormat, Is.EqualTo(ContentFormat.Markdown));
            Assert.That(restored.BodyMarkdown, Is.EqualTo(original.BodyMarkdown));
            Assert.That(restored.BodyHtml, Is.EqualTo(original.BodyHtml));
            Assert.That(restored.Title, Is.EqualTo(original.Title));
        }

        [Test]
        public void LegacyJson_WithoutFormatFields_DeserializesAsHtml()
        {
            const string legacyJson = """
                {
                  "Id": "draft1",
                  "Title": "Old draft",
                  "BodyHtml": "<p>Hello</p>"
                }
                """;

            var doc = JsonSerializer.Deserialize<PostDocument>(legacyJson, SerializerOptions);

            Assert.That(doc.BodyFormat, Is.EqualTo(ContentFormat.Html));
            Assert.That(doc.BodyMarkdown, Is.EqualTo(string.Empty));
            Assert.That(doc.BodyHtml, Is.EqualTo("<p>Hello</p>"));
        }

        [Test]
        public void FileDraftStore_RoundTrip_PreservesMarkdownBody()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "olw-publishing-tests-" + Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);

            try
            {
                var store = new FileDraftStore(tempDir);
                var original = new PostDocument
                {
                    Title = "Saved markdown",
                    BodyFormat = ContentFormat.Markdown,
                    BodyMarkdown = "## Section\n\n- item one"
                };

                PostDocument saved = store.Save(original);
                PostDocument loaded = store.Load(saved.Id);

                Assert.That(loaded.BodyFormat, Is.EqualTo(ContentFormat.Markdown));
                Assert.That(loaded.BodyMarkdown, Is.EqualTo(original.BodyMarkdown));
                Assert.That(loaded.Title, Is.EqualTo(original.Title));
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, recursive: true);
            }
        }
    }
}
