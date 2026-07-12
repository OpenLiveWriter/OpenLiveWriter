// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using NUnit.Framework;
using OpenLiveWriter.EditorTests.Automated.Publish;
using OpenLiveWriter.Publishing;
using OpenLiveWriter.Publishing.Accounts;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group J — re-publish semantics. A first publish creates a new server post and records
    /// its id; a subsequent publish of the same document (same blog) edits that post via
    /// <c>metaWeblog.editPost</c> instead of creating a duplicate. Covers both the
    /// <see cref="EditorContentPublisher"/> primitive and the <see cref="BlogAccountService"/>
    /// orchestration. Offline via <see cref="FakeBlogClient"/>.
    /// </summary>
    [TestFixture]
    [Category("GroupJ")]
    public class GroupJ_RepublishTests
    {
        [Test]
        public void PublishOrEdit_NoExistingId_CallsNewPost()
        {
            var fake = new FakeBlogClient();
            string id = EditorContentPublisher.PublishOrEdit(
                fake, "blog-1", existingPostId: null, "Title", "<p>Body</p>", publish: true, categories: null);

            Assert.That(fake.NewPostCount, Is.EqualTo(1));
            Assert.That(fake.EditPostCount, Is.EqualTo(0));
            Assert.That(id, Is.EqualTo("fake-post-1"));
        }

        [Test]
        public void PublishOrEdit_ExistingId_CallsEditPost_WithSameId()
        {
            var fake = new FakeBlogClient();
            string id = EditorContentPublisher.PublishOrEdit(
                fake, "blog-1", existingPostId: "server-77", "Title", "<p>Body</p>", publish: true, categories: null);

            Assert.That(fake.NewPostCount, Is.EqualTo(0));
            Assert.That(fake.EditPostCount, Is.EqualTo(1));
            Assert.That(fake.LastPost.Id, Is.EqualTo("server-77"));
            Assert.That(id, Is.EqualTo("server-77"));
        }

        [Test]
        public void Service_RepublishSameDocument_EditsInsteadOfCreatingDuplicate()
        {
            string dir = Path.Combine(Path.GetTempPath(), "OLWRepublish", Guid.NewGuid().ToString("N"));
            try
            {
                var fake = new FakeBlogClient();
                var service = new BlogAccountService(
                    new FileAccountStore(dir), new InMemoryCredentialStore(), (a, p) => fake);
                var account = new BlogAccount
                {
                    DisplayName = "Blog",
                    ApiEndpointUrl = "https://blog.example.com/xmlrpc.php",
                    BlogId = "blog-5",
                    Username = "author"
                };
                BlogAccount saved = service.SaveAccount(account, "pw");
                service.SetCurrentAccount(saved.Id);

                var doc = new PostDocument { Title = "Post" };

                // First publish -> NewPost, records the server id.
                PublishOutcome first = service.Publish(doc, "<p>v1</p>", publish: true);
                Assert.That(first.Succeeded, Is.True);
                Assert.That(fake.NewPostCount, Is.EqualTo(1));
                Assert.That(doc.PublishedPostId, Is.EqualTo("fake-post-1"));
                Assert.That(doc.BlogId, Is.EqualTo("blog-5"));

                // Second publish of the SAME document -> EditPost (no new post).
                PublishOutcome second = service.Publish(doc, "<p>v2</p>", publish: true);
                Assert.That(second.Succeeded, Is.True);
                Assert.That(fake.NewPostCount, Is.EqualTo(1), "no duplicate NewPost on republish");
                Assert.That(fake.EditPostCount, Is.EqualTo(1));
                Assert.That(fake.LastPost.Id, Is.EqualTo("fake-post-1"));
                Assert.That(fake.LastPost.MainContents, Is.EqualTo("<p>v2</p>"));
                Assert.That(doc.PublishedPostId, Is.EqualTo("fake-post-1"));
            }
            finally
            {
                try { if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true); }
                catch { /* best effort */ }
            }
        }
    }
}
