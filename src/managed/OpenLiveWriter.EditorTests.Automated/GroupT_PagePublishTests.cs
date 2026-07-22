// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenLiveWriter.EditorTests.Automated.Publish;
using OpenLiveWriter.Publishing;
using OpenLiveWriter.Publishing.Accounts;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group T (part 2) — pages publish as pages (Band 3a, P1-5). The
    /// <c>wp.newPage</c>/<c>wp.editPage</c> payload shape is pinned offline, and the
    /// <see cref="EditorContentPublisher"/>/<see cref="BlogAccountService"/> dispatch
    /// (page → wp.*, new vs edit on PublishedPostId) is verified against
    /// <see cref="FakeBlogClient"/>.
    /// </summary>
    [TestFixture]
    [Category("GroupT")]
    public class GroupT_PagePublishTests
    {
        private static MetaWeblogXmlRpcClient NewClient() =>
            new MetaWeblogXmlRpcClient("https://blog.example.com/xmlrpc.php", "user", "pw");

        private static BlogPost NewPagePost()
        {
            var post = new BlogPost { Title = "About", IsPage = true };
            post.Contents = "<p>Main</p>" + BlogPost.ExtendedEntryBreak + "<p>More</p>";
            return post;
        }

        // ---- Payload shape ----

        [Test]
        public void BuildNewPageXml_PageStruct_TitleDescriptionTextMore()
        {
            string xml = NewClient().BuildNewPageXml("blog-1", NewPagePost(), publish: true);

            Assert.That(xml, Does.Contain("wp.newPage"));
            Assert.That(xml, Does.Contain("<string>blog-1</string>"));
            Assert.That(xml, Does.Contain("<string>user</string>"));
            Assert.That(xml, Does.Contain("<name>title</name>"));
            Assert.That(xml, Does.Contain("<string>About</string>"));
            Assert.That(xml, Does.Contain("<name>description</name>"));
            Assert.That(xml, Does.Contain("&lt;p&gt;Main&lt;/p&gt;"));
            Assert.That(xml, Does.Contain("<name>mt_text_more</name>"));
            Assert.That(xml, Does.Contain("&lt;p&gt;More&lt;/p&gt;"));
            Assert.That(xml, Does.Contain("<boolean>1</boolean>"));
        }

        [Test]
        public void BuildNewPageXml_DraftPage_PublishFlagFalse()
        {
            string xml = NewClient().BuildNewPageXml("blog-1", NewPagePost(), publish: false);
            Assert.That(xml, Does.Contain("<boolean>0</boolean>"));
        }

        [Test]
        public void BuildNewPageXml_PageStruct_HasNoCategoriesOrKeywords()
        {
            BlogPost post = NewPagePost();
            post.Categories.Add("News");
            post.Keywords = "tag1";

            string xml = NewClient().BuildNewPageXml("blog-1", post, publish: true);

            Assert.That(xml, Does.Not.Contain("<name>categories</name>"));
            Assert.That(xml, Does.Not.Contain("<name>mt_keywords</name>"));
        }

        [Test]
        public void BuildEditPageXml_SendsBlogIdAndPageId()
        {
            BlogPost post = NewPagePost();
            post.Id = "87";

            string xml = NewClient().BuildEditPageXml("blog-1", post, publish: true);

            Assert.That(xml, Does.Contain("wp.editPage"));
            Assert.That(xml, Does.Contain("<string>blog-1</string>"));
            Assert.That(xml, Does.Contain("<string>87</string>"));
        }

        // ---- PublishOrEdit dispatch ----

        [Test]
        public async Task PublishOrEdit_Page_NoExistingId_CallsNewPage()
        {
            var fake = new FakeBlogClient();
            string id = await EditorContentPublisher.PublishOrEditAsync(
                fake, "blog-1", existingPostId: null, "About", "<p>Body</p>",
                publish: true, categories: null, isPage: true);

            Assert.That(fake.NewPageCount, Is.EqualTo(1));
            Assert.That(fake.NewPostCount, Is.EqualTo(0), "a page must never go through metaWeblog.newPost");
            Assert.That(fake.EditPageCount, Is.EqualTo(0));
            Assert.That(id, Is.EqualTo("fake-page-1"));
        }

        [Test]
        public async Task PublishOrEdit_Page_ExistingId_CallsEditPage_WithSameId()
        {
            var fake = new FakeBlogClient();
            string id = await EditorContentPublisher.PublishOrEditAsync(
                fake, "blog-1", existingPostId: "87", "About", "<p>Body</p>",
                publish: true, categories: null, isPage: true);

            Assert.That(fake.EditPageCount, Is.EqualTo(1));
            Assert.That(fake.EditPostCount, Is.EqualTo(0), "a page must never go through metaWeblog.editPost");
            Assert.That(fake.NewPageCount, Is.EqualTo(0));
            Assert.That(fake.LastPost.Id, Is.EqualTo("87"));
            Assert.That(id, Is.EqualTo("87"));
        }

        [Test]
        public async Task PublishOrEdit_Post_StillUsesMetaWeblogMethods()
        {
            var fake = new FakeBlogClient();
            await EditorContentPublisher.PublishOrEditAsync(
                fake, "blog-1", existingPostId: null, "Post", "<p>Body</p>",
                publish: true, categories: null, isPage: false);

            Assert.That(fake.NewPostCount, Is.EqualTo(1));
            Assert.That(fake.NewPageCount, Is.EqualTo(0));
        }

        // ---- Service-level flow ----

        [Test]
        public async Task Service_PublishPage_NewThenRepublish_EditsSamePage()
        {
            string dir = Path.Combine(Path.GetTempPath(), "OLWPagePublish", Guid.NewGuid().ToString("N"));
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
                    Username = "author",
                    ProviderType = BlogAccount.WordPressProviderType
                };
                BlogAccount saved = service.SaveAccount(account, "pw");
                service.SetCurrentAccount(saved.Id);

                var doc = new PostDocument { Title = "About", IsPage = true };

                // First publish -> wp.newPage, records the server page id.
                PublishOutcome first = await service.PublishAsync(doc, "<p>v1</p>", publish: true);
                Assert.That(first.Succeeded, Is.True);
                Assert.That(fake.NewPageCount, Is.EqualTo(1));
                Assert.That(fake.NewPostCount, Is.EqualTo(0));
                Assert.That(doc.PublishedPostId, Is.EqualTo("fake-page-1"));

                // Republish the SAME page document -> wp.editPage (no duplicate).
                PublishOutcome second = await service.PublishAsync(doc, "<p>v2</p>", publish: true);
                Assert.That(second.Succeeded, Is.True);
                Assert.That(fake.NewPageCount, Is.EqualTo(1), "no duplicate newPage on republish");
                Assert.That(fake.EditPageCount, Is.EqualTo(1));
                Assert.That(fake.EditPostCount, Is.EqualTo(0));
                Assert.That(fake.LastPost.Id, Is.EqualTo("fake-page-1"));
                Assert.That(fake.LastPost.IsPage, Is.True);
            }
            finally
            {
                try { if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true); }
                catch { /* best effort */ }
            }
        }
    }
}
