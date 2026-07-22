// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenLiveWriter.EditorTests.Automated.Infrastructure;
using OpenLiveWriter.Publishing;
using OpenLiveWriter.Publishing.Accounts;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Opt-in LIVE fetch/pages integration tests. They perform real
    /// <c>metaWeblog.getRecentPosts</c> / <c>metaWeblog.getPost</c> / <c>wp.getPages</c>
    /// calls against a blog endpoint supplied via environment variables, so they are
    /// <see cref="ExplicitAttribute"/> and excluded from the default headless run —
    /// the manual live-endpoint verification step for the Band-3a fetch path.
    ///
    /// Required environment variables (same as LiveBlogPublishTests):
    ///   OLW_LIVEBLOG_ENDPOINT   e.g. https://your-blog.example.com/xmlrpc.php
    ///   OLW_LIVEBLOG_BLOGID     the blog id (often "1" for single-blog WordPress)
    ///   OLW_LIVEBLOG_USER       account username
    ///   OLW_LIVEBLOG_PASS       account password / app password
    ///
    /// Run them (macOS/zsh):
    ///   OLW_LIVEBLOG_ENDPOINT=https://blog.example.com/xmlrpc.php \
    ///   OLW_LIVEBLOG_BLOGID=1 OLW_LIVEBLOG_USER=me OLW_LIVEBLOG_PASS=app-pass \
    ///   dotnet test src/managed/OpenLiveWriter.EditorTests.Automated \
    ///     --filter "Category=LiveBlog" -- NUnit.Explicit=true
    ///
    /// The page round-trip creates a server-side DRAFT page (never published), so a
    /// stray run leaves nothing public.
    /// </summary>
    [TestFixture]
    [Category(WebViewCategories.LiveBlog)]
    [Explicit("Fetches from / writes drafts to a real blog endpoint; requires OLW_LIVEBLOG_* env vars.")]
    public class LiveBlogServerFetchTests
    {
        private static bool TryCreateClient(out IBlogClient client, out string blogId)
        {
            string endpoint = Environment.GetEnvironmentVariable("OLW_LIVEBLOG_ENDPOINT");
            blogId = Environment.GetEnvironmentVariable("OLW_LIVEBLOG_BLOGID");
            string user = Environment.GetEnvironmentVariable("OLW_LIVEBLOG_USER");
            string pass = Environment.GetEnvironmentVariable("OLW_LIVEBLOG_PASS");

            client = null;
            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(blogId) ||
                string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                return false;
            }

            client = new WordPressXmlRpcClient(endpoint, user, pass);
            return true;
        }

        [Test]
        public async Task LiveFetch_GetRecentPosts_ReturnsPosts()
        {
            if (!TryCreateClient(out IBlogClient client, out string blogId))
                Assert.Ignore("Set OLW_LIVEBLOG_ENDPOINT/BLOGID/USER/PASS to run the live fetch tests.");

            var posts = await client.GetRecentPostsAsync(blogId, 10);

            TestContext.WriteLine($"getRecentPosts returned {posts.Count} post(s).");
            foreach (ServerPost p in posts.Take(3))
                TestContext.WriteLine($"  [{p.PostId}] {p.Title} ({p.Status}, body {p.BodyHtml.Length} chars)");
            Assert.That(posts, Is.Not.Null);
            if (posts.Count > 0)
                Assert.That(posts[0].PostId, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public async Task LiveFetch_GetPost_RoundTrips_BodyAndMore()
        {
            if (!TryCreateClient(out IBlogClient client, out string blogId))
                Assert.Ignore("Set OLW_LIVEBLOG_ENDPOINT/BLOGID/USER/PASS to run the live fetch tests.");

            // Create a server-side DRAFT (never published) with a main/extended split,
            // then read it back: the rejoined BodyHtml must reproduce the original.
            string title = "OLW macOS live fetch test " + DateTime.UtcNow.ToString("O");
            string body = "<p>Main part.</p>" + BlogPost.ExtendedEntryBreak + "<p>Extended part.</p>";
            var draft = new BlogPost { Title = title };
            draft.Contents = body;

            string postId = await client.NewPostAsync(blogId, draft, publish: false);
            Assert.That(postId, Is.Not.Null.And.Not.Empty);
            TestContext.WriteLine($"created draft post {postId}");

            ServerPost fetched = await client.GetPostAsync(postId);

            Assert.That(fetched, Is.Not.Null);
            Assert.That(fetched.PostId, Is.EqualTo(postId));
            Assert.That(fetched.Title, Is.EqualTo(title));
            Assert.That(fetched.BodyHtml, Does.Contain("Main part."));
            Assert.That(fetched.BodyHtml, Does.Contain("Extended part."));
        }

        [Test]
        public async Task LiveFetch_GetPages_Succeeds()
        {
            if (!TryCreateClient(out IBlogClient client, out string blogId))
                Assert.Ignore("Set OLW_LIVEBLOG_ENDPOINT/BLOGID/USER/PASS to run the live fetch tests.");

            var pages = await client.GetPagesAsync(blogId);

            TestContext.WriteLine($"wp.getPages returned {pages.Count} page(s).");
            foreach (ServerPost p in pages.Take(3))
                TestContext.WriteLine($"  [{p.PostId}] {p.Title} ({p.Status})");
            Assert.That(pages, Is.Not.Null);
            Assert.That(pages.All(p => p.IsPage), Is.True, "entries from wp.getPages must be marked IsPage");
        }

        [Test]
        public async Task LivePage_NewPageThenEdit_StaysAPage()
        {
            if (!TryCreateClient(out IBlogClient client, out string blogId))
                Assert.Ignore("Set OLW_LIVEBLOG_ENDPOINT/BLOGID/USER/PASS to run the live fetch tests.");

            // Draft page round-trip: create unpublished, edit it, confirm it lists as a page.
            string title = "OLW macOS live page test " + DateTime.UtcNow.ToString("O");
            var page = new BlogPost { Title = title, IsPage = true };
            page.Contents = "<p>Draft page body.</p>";

            string pageId = await client.NewPageAsync(blogId, page, publish: false);
            Assert.That(pageId, Is.Not.Null.And.Not.Empty);
            TestContext.WriteLine($"created draft page {pageId}");

            page.Id = pageId;
            page.Contents = "<p>Draft page body, edited.</p>";
            await client.EditPageAsync(blogId, page, publish: false);

            var pages = await client.GetPagesAsync(blogId);
            Assert.That(pages.Any(p => p.PostId == pageId), Is.True,
                "the edited draft page should appear in wp.getPages");
        }
    }
}
