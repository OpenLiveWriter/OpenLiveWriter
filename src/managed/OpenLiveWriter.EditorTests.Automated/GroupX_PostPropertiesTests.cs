// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.LogicalTree;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Dialogs;
using OpenLiveWriter.EditorTests.Automated.Publish;
using OpenLiveWriter.Publishing;
using OpenLiveWriter.Publishing.Drafts;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group X — P1-9 remainder: slug, excerpt and ping/trackback URLs. The values
    /// flow Post Properties dialog → <see cref="PostDocument"/> (draft JSON
    /// round-trip) → <see cref="BlogPost"/> → the MetaWeblog <c>wp_slug</c> /
    /// <c>mt_excerpt</c> / <c>mt_tb_ping_urls</c> struct members (emitted only when
    /// non-empty; pages get slug/excerpt but never ping URLs), and back again via
    /// the Open-from-Blog struct parsing (<see cref="ServerPost"/> →
    /// <see cref="PostDocument.FromServerPost"/>).
    /// </summary>
    [TestFixture]
    [Category("GroupX")]
    public class GroupX_PostPropertiesTests
    {
        private static MetaWeblogXmlRpcClient NewClient() =>
            new MetaWeblogXmlRpcClient("http://example.test/xmlrpc", "user", "pass");

        private static XmlDocument LoadXml(string methodCallXml)
        {
            var doc = new XmlDocument();
            doc.LoadXml(methodCallXml);
            return doc;
        }

        private static string StructMember(string methodCallXml, string name, int paramIndex = 4)
        {
            XmlNode member = LoadXml(methodCallXml).SelectSingleNode(
                $"/methodCall/params/param[{paramIndex}]/value/struct/member[name='{name}']/value");
            return member?.InnerText;
        }

        private static string[] StructArrayMember(string methodCallXml, string name, int paramIndex = 4)
        {
            XmlNodeList values = LoadXml(methodCallXml).SelectNodes(
                $"/methodCall/params/param[{paramIndex}]/value/struct/member[name='{name}']/value/array/data/value");
            return values == null
                ? null
                : values.Cast<XmlNode>().Select(v => v.InnerText).ToArray();
        }

        private static BlogPost PostWithProperties()
        {
            var post = new BlogPost
            {
                Title = "T",
                Slug = "my-custom-slug",
                Excerpt = "A short excerpt."
            };
            post.PingUrls.Add("https://example.com/trackback-1");
            post.PingUrls.Add("https://example.com/trackback-2");
            return post;
        }

        // ---- post struct membership ----

        [Test]
        public void PostStruct_IncludesSlugExcerptPingUrls_WhenSet()
        {
            string xml = NewClient().BuildNewPostXml("blog-1", PostWithProperties(), publish: true);

            Assert.That(StructMember(xml, "wp_slug"), Is.EqualTo("my-custom-slug"));
            Assert.That(StructMember(xml, "mt_excerpt"), Is.EqualTo("A short excerpt."));
            Assert.That(StructArrayMember(xml, "mt_tb_ping_urls"), Is.EqualTo(new[]
            {
                "https://example.com/trackback-1",
                "https://example.com/trackback-2"
            }));
        }

        [Test]
        public void PostStruct_OmitsSlugExcerptPingUrls_WhenEmpty()
        {
            var post = new BlogPost { Title = "T" };
            string xml = NewClient().BuildNewPostXml("blog-1", post, publish: true);

            Assert.That(xml, Does.Not.Contain("wp_slug"));
            Assert.That(xml, Does.Not.Contain("mt_excerpt"));
            Assert.That(xml, Does.Not.Contain("mt_tb_ping_urls"));
        }

        [Test]
        public void PostStruct_OmitsPingUrls_WhenOnlyBlankEntries()
        {
            var post = new BlogPost { Title = "T" };
            post.PingUrls.Add(string.Empty);
            string xml = NewClient().BuildNewPostXml("blog-1", post, publish: true);

            Assert.That(xml, Does.Not.Contain("mt_tb_ping_urls"));
        }

        [Test]
        public void EditPostStruct_IncludesSlugExcerptPingUrls_WhenSet()
        {
            BlogPost post = PostWithProperties();
            post.Id = "99";
            string xml = NewClient().BuildEditPostXml(post, publish: true);

            Assert.That(xml, Does.Contain("metaWeblog.editPost"));
            Assert.That(StructMember(xml, "wp_slug"), Is.EqualTo("my-custom-slug"));
            Assert.That(StructMember(xml, "mt_excerpt"), Is.EqualTo("A short excerpt."));
            Assert.That(StructArrayMember(xml, "mt_tb_ping_urls"), Has.Length.EqualTo(2));
        }

        // ---- page struct membership (slug/excerpt only — pages don't ping) ----

        [Test]
        public void PageStruct_IncludesSlugExcerpt_ButNeverPingUrls_WhenSet()
        {
            BlogPost page = PostWithProperties();
            page.IsPage = true;
            string xml = NewClient().BuildNewPageXml("blog-1", page, publish: true);

            Assert.That(StructMember(xml, "wp_slug"), Is.EqualTo("my-custom-slug"));
            Assert.That(StructMember(xml, "mt_excerpt"), Is.EqualTo("A short excerpt."));
            Assert.That(xml, Does.Not.Contain("mt_tb_ping_urls"));
        }

        [Test]
        public void PageStruct_OmitsSlugExcerpt_WhenEmpty()
        {
            var page = new BlogPost { Title = "T", IsPage = true };
            string xml = NewClient().BuildNewPageXml("blog-1", page, publish: true);

            Assert.That(xml, Does.Not.Contain("wp_slug"));
            Assert.That(xml, Does.Not.Contain("mt_excerpt"));
        }

        [Test]
        public void EditPageStruct_IncludesSlugExcerpt_ButNeverPingUrls_WhenSet()
        {
            BlogPost page = PostWithProperties();
            page.IsPage = true;
            page.Id = "87";
            // wp.editPage: blogId, pageId, user, password, struct, publish → struct is param 5.
            string xml = NewClient().BuildEditPageXml("blog-1", page, publish: true);

            Assert.That(xml, Does.Contain("wp.editPage"));
            Assert.That(StructMember(xml, "wp_slug", paramIndex: 5), Is.EqualTo("my-custom-slug"));
            Assert.That(StructMember(xml, "mt_excerpt", paramIndex: 5), Is.EqualTo("A short excerpt."));
            Assert.That(xml, Does.Not.Contain("mt_tb_ping_urls"));
        }

        // ---- publisher pass-through ----

        [Test]
        public async Task PublishOrEdit_CarriesSlugExcerptPingUrls_ToPost()
        {
            var fake = new FakeBlogClient();
            await EditorContentPublisher.PublishOrEditAsync(
                fake, "blog-1", existingPostId: null, "T", "<p>Body</p>", publish: true,
                categories: Enumerable.Empty<string>(),
                slug: "my-custom-slug", excerpt: "A short excerpt.",
                pingUrls: new[] { "https://example.com/trackback", " " });

            Assert.That(fake.NewPostCount, Is.EqualTo(1));
            Assert.That(fake.LastPost.Slug, Is.EqualTo("my-custom-slug"));
            Assert.That(fake.LastPost.Excerpt, Is.EqualTo("A short excerpt."));
            Assert.That(fake.LastPost.PingUrls, Is.EqualTo(new[] { "https://example.com/trackback" }),
                "blank ping-URL entries must be dropped");
        }

        [Test]
        public async Task PublishOrEdit_NoProperties_LeavesDefaults()
        {
            var fake = new FakeBlogClient();
            await EditorContentPublisher.PublishAsync(fake, "blog-1", "T", "<p>Body</p>", publish: true);

            Assert.That(fake.LastPost.Slug, Is.Empty);
            Assert.That(fake.LastPost.Excerpt, Is.Empty);
            Assert.That(fake.LastPost.PingUrls, Is.Empty);
        }

        // ---- document mapping ----

        [Test]
        public void ToBlogPost_MapsSlugExcerptPingUrls()
        {
            var doc = new PostDocument
            {
                Title = "T",
                Slug = "my-custom-slug",
                Excerpt = "A short excerpt.",
                PingUrls = { "https://example.com/trackback", "" }
            };

            BlogPost post = doc.ToBlogPost();
            Assert.That(post.Slug, Is.EqualTo("my-custom-slug"));
            Assert.That(post.Excerpt, Is.EqualTo("A short excerpt."));
            Assert.That(post.PingUrls, Is.EqualTo(new[] { "https://example.com/trackback" }));
        }

        [Test]
        public void FromBlogPost_MapsSlugExcerptPingUrls()
        {
            BlogPost post = PostWithProperties();
            PostDocument doc = PostDocument.FromBlogPost(post);

            Assert.That(doc.Slug, Is.EqualTo("my-custom-slug"));
            Assert.That(doc.Excerpt, Is.EqualTo("A short excerpt."));
            Assert.That(doc.PingUrls, Is.EqualTo(new[]
            {
                "https://example.com/trackback-1",
                "https://example.com/trackback-2"
            }));
        }

        [Test]
        public void FromServerPost_MapsSlugExcerptPingUrls()
        {
            var server = new ServerPost
            {
                PostId = "412",
                Title = "T",
                Slug = "my-custom-slug",
                Excerpt = "A short excerpt.",
                PingUrls = new[] { "https://example.com/trackback" }
            };

            PostDocument doc = PostDocument.FromServerPost(server, "blog-1");
            Assert.That(doc.Slug, Is.EqualTo("my-custom-slug"));
            Assert.That(doc.Excerpt, Is.EqualTo("A short excerpt."));
            Assert.That(doc.PingUrls, Is.EqualTo(new[] { "https://example.com/trackback" }));
        }

        // ---- draft JSON round-trip ----

        [Test]
        public void DraftRoundTrip_PersistsSlugExcerptPingUrls()
        {
            string dir = Path.Combine(Path.GetTempPath(), "OLWPostProperties", Guid.NewGuid().ToString("N"));
            try
            {
                var store = new FileDraftStore(dir);
                var doc = new PostDocument
                {
                    Title = "T",
                    Slug = "my-custom-slug",
                    Excerpt = "A short excerpt.",
                    PingUrls = { "https://example.com/trackback-1", "https://example.com/trackback-2" }
                };
                PostDocument saved = store.Save(doc);

                PostDocument loaded = store.Load(saved.Id);
                Assert.That(loaded.Slug, Is.EqualTo("my-custom-slug"));
                Assert.That(loaded.Excerpt, Is.EqualTo("A short excerpt."));
                Assert.That(loaded.PingUrls, Is.EqualTo(new[]
                {
                    "https://example.com/trackback-1",
                    "https://example.com/trackback-2"
                }));
            }
            finally
            {
                try { if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true); }
                catch { /* best effort */ }
            }
        }

        [Test]
        public void DraftLoad_ToleratesUnknownAndMissingFields()
        {
            string dir = Path.Combine(Path.GetTempPath(), "OLWPostProperties", Guid.NewGuid().ToString("N"));
            try
            {
                Directory.CreateDirectory(dir);
                // A draft written by a build with fields this build does not know
                // (and without the P1-9 fields) must still load with defaults.
                const string id = "legacydraft";
                File.WriteAllText(Path.Combine(dir, id + ".oldraft.json"),
                    "{ \"Title\": \"Legacy\", \"BodyHtml\": \"<p>x</p>\", \"FutureField\": 42 }");

                var store = new FileDraftStore(dir);
                PostDocument loaded = store.Load(id);
                Assert.That(loaded.Title, Is.EqualTo("Legacy"));
                Assert.That(loaded.Slug, Is.Empty);
                Assert.That(loaded.Excerpt, Is.Empty);
                Assert.That(loaded.PingUrls, Is.Empty);
            }
            finally
            {
                try { if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true); }
                catch { /* best effort */ }
            }
        }

        // ---- server-struct parse-back ----

        [Test]
        public void ParseServerPostStruct_ReadsSlugExcerptPingUrls()
        {
            const string xml =
                "<?xml version=\"1.0\"?>"
                + "<methodResponse><params><param><value><struct>"
                + "<member><name>postid</name><value><string>412</string></value></member>"
                + "<member><name>title</name><value><string>Hello macOS</string></value></member>"
                + "<member><name>description</name><value><string>&lt;p&gt;Body&lt;/p&gt;</string></value></member>"
                + "<member><name>wp_slug</name><value><string>hello-macos</string></value></member>"
                + "<member><name>mt_excerpt</name><value><string>A short excerpt.</string></value></member>"
                + "<member><name>mt_tb_ping_urls</name><value><array><data>"
                + "<value><string>https://example.com/trackback-1</string></value>"
                + "<value><string>https://example.com/trackback-2</string></value>"
                + "</data></array></value></member>"
                + "</struct></value></param></params></methodResponse>";

            ServerPost post = MetaWeblogXmlRpcClient.ParseGetPostResponse(xml);

            Assert.That(post.Slug, Is.EqualTo("hello-macos"));
            Assert.That(post.Excerpt, Is.EqualTo("A short excerpt."));
            Assert.That(post.PingUrls, Is.EqualTo(new[]
            {
                "https://example.com/trackback-1",
                "https://example.com/trackback-2"
            }));
        }

        [Test]
        public void ParseServerPostStruct_MissingMembers_DefaultToEmpty()
        {
            const string xml =
                "<?xml version=\"1.0\"?>"
                + "<methodResponse><params><param><value><struct>"
                + "<member><name>postid</name><value><string>412</string></value></member>"
                + "<member><name>title</name><value><string>Hello macOS</string></value></member>"
                + "</struct></value></param></params></methodResponse>";

            ServerPost post = MetaWeblogXmlRpcClient.ParseGetPostResponse(xml);

            Assert.That(post.Slug, Is.Empty);
            Assert.That(post.Excerpt, Is.Empty);
            Assert.That(post.PingUrls, Is.Empty);
        }

        // ---- ping-URL text parsing (pure) ----

        [Test]
        public void SplitPingUrls_ParsesOnePerLine_DroppingBlanks()
        {
            var urls = PostDocument.SplitPingUrls(
                " https://example.com/a \r\n\r\nhttps://example.com/b\n   \n");

            Assert.That(urls, Is.EqualTo(new[]
            {
                "https://example.com/a",
                "https://example.com/b"
            }));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   \n  ")]
        public void SplitPingUrls_Empty_YieldsEmptyList(string text)
        {
            Assert.That(PostDocument.SplitPingUrls(text), Is.Empty);
        }

        // ---- dialog pre-fill + save-back (headless) ----

        [AvaloniaTest]
        public void PostPropertiesDialog_PrefillsSlugExcerptPingUrls()
        {
            var dialog = new PostPropertiesDialog(
                slug: "my-custom-slug",
                excerpt: "A short excerpt.",
                pingUrls: new[] { "https://example.com/a", "https://example.com/b" });

            TextBox slugBox = dialog.GetLogicalDescendants().OfType<TextBox>()
                .First(t => t.Name == "SlugBox");
            TextBox excerptBox = dialog.GetLogicalDescendants().OfType<TextBox>()
                .First(t => t.Name == "ExcerptBox");
            TextBox pingUrlsBox = dialog.GetLogicalDescendants().OfType<TextBox>()
                .First(t => t.Name == "PingUrlsBox");

            Assert.That(slugBox.Text, Is.EqualTo("my-custom-slug"));
            Assert.That(excerptBox.Text, Is.EqualTo("A short excerpt."));
            Assert.That(pingUrlsBox.Text, Is.EqualTo("https://example.com/a\nhttps://example.com/b"));
            Assert.That(excerptBox.AcceptsReturn, Is.True, "excerpt is multiline");
            Assert.That(pingUrlsBox.AcceptsReturn, Is.True, "ping URLs are one per line");
        }

        [AvaloniaTest]
        public void PostPropertiesDialog_BuildResult_CapturesSlugExcerptPingUrls()
        {
            var dialog = new PostPropertiesDialog();
            TextBox slugBox = dialog.GetLogicalDescendants().OfType<TextBox>()
                .First(t => t.Name == "SlugBox");
            TextBox excerptBox = dialog.GetLogicalDescendants().OfType<TextBox>()
                .First(t => t.Name == "ExcerptBox");
            TextBox pingUrlsBox = dialog.GetLogicalDescendants().OfType<TextBox>()
                .First(t => t.Name == "PingUrlsBox");

            slugBox.Text = " my-custom-slug ";
            excerptBox.Text = "A short excerpt.";
            pingUrlsBox.Text = "https://example.com/a\r\n\nhttps://example.com/b";

            PostPropertiesDialogResult result = dialog.BuildResult();
            Assert.That(result.PublishDateUtc, Is.Null, "immediate is the default");
            Assert.That(result.Slug, Is.EqualTo("my-custom-slug"));
            Assert.That(result.Excerpt, Is.EqualTo("A short excerpt."));
            Assert.That(result.PingUrls, Is.EqualTo(new[]
            {
                "https://example.com/a",
                "https://example.com/b"
            }));
        }
    }
}
