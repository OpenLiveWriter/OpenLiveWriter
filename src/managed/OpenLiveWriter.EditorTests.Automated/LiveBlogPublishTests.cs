// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenLiveWriter.EditorTests.Automated.Infrastructure;
using OpenLiveWriter.Publishing;
using OpenLiveWriter.Publishing.Accounts;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Opt-in LIVE publish integration test. It performs a real <c>metaWeblog.newPost</c>
    /// against a blog endpoint supplied via environment variables, so it is
    /// <see cref="ExplicitAttribute"/> and excluded from the default headless run. This is
    /// the manual live-endpoint verification step for the ported transport.
    ///
    /// Required environment variables:
    ///   OLW_LIVEBLOG_ENDPOINT   e.g. https://your-blog.example.com/xmlrpc.php
    ///   OLW_LIVEBLOG_BLOGID     the blog id (often "1" for single-blog WordPress)
    ///   OLW_LIVEBLOG_USER       account username
    ///   OLW_LIVEBLOG_PASS       account password / app password
    /// Optional:
    ///   OLW_LIVEBLOG_PUBLISH    "true" to publish live, otherwise posts as a draft (default)
    ///
    /// Run it (macOS/zsh):
    ///   OLW_LIVEBLOG_ENDPOINT=https://blog.example.com/xmlrpc.php \
    ///   OLW_LIVEBLOG_BLOGID=1 OLW_LIVEBLOG_USER=me OLW_LIVEBLOG_PASS=app-pass \
    ///   dotnet test src/managed/OpenLiveWriter.EditorTests.Automated \
    ///     --filter "Category=LiveBlog" -- NUnit.Explicit=true
    ///
    /// It defaults to posting an UNPUBLISHED draft so a stray run doesn't publish a live
    /// post; set OLW_LIVEBLOG_PUBLISH=true to exercise the published path.
    /// </summary>
    [TestFixture]
    [Category(WebViewCategories.LiveBlog)]
    [Explicit("Publishes to a real blog endpoint; requires OLW_LIVEBLOG_* env vars.")]
    public class LiveBlogPublishTests
    {
        [Test]
        public async Task LivePublish_NewPost_ReturnsServerPostId()
        {
            string endpoint = Environment.GetEnvironmentVariable("OLW_LIVEBLOG_ENDPOINT");
            string blogId = Environment.GetEnvironmentVariable("OLW_LIVEBLOG_BLOGID");
            string user = Environment.GetEnvironmentVariable("OLW_LIVEBLOG_USER");
            string pass = Environment.GetEnvironmentVariable("OLW_LIVEBLOG_PASS");

            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(blogId) ||
                string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                Assert.Ignore("Set OLW_LIVEBLOG_ENDPOINT/BLOGID/USER/PASS to run the live publish test.");
            }

            bool publish = string.Equals(
                Environment.GetEnvironmentVariable("OLW_LIVEBLOG_PUBLISH"), "true",
                StringComparison.OrdinalIgnoreCase);

            var account = new BlogAccount
            {
                DisplayName = "Live test blog",
                ApiEndpointUrl = endpoint,
                BlogId = blogId,
                Username = user
            };

            IBlogClient client = BlogClientFactory.CreateClient(account, pass);

            string title = "OLW macOS live test " + DateTime.UtcNow.ToString("O");
            string html = "<p>Automated live publish test from the macOS port.</p>";

            string postId = await EditorContentPublisher.PublishAsync(
                client, blogId, title, html, publish, "Test");

            Assert.That(postId, Is.Not.Null.And.Not.Empty,
                "The blog should return a server post id for the new post.");
            TestContext.WriteLine(
                $"Live publish OK: postId={postId}, published={publish}, endpoint={endpoint}");
        }
    }
}
