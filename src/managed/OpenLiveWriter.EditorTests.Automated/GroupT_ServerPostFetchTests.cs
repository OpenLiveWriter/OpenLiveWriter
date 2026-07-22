// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenLiveWriter.Publishing;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group T (part 1) — reading posts/pages back from the blog (Band 3a, P1-4).
    /// The metaWeblog.getRecentPosts / metaWeblog.getPost / wp.getPages response
    /// parsing is exercised against fixture XML (pure, offline), and the real
    /// <see cref="MetaWeblogXmlRpcClient"/> transport is driven end-to-end over a fake
    /// <see cref="HttpMessageHandler"/> so the request method names / parameters are
    /// pinned without a network.
    /// </summary>
    [TestFixture]
    [Category("GroupT")]
    public class GroupT_ServerPostFetchTests
    {
        private const string Endpoint = "https://blog.example.com/xmlrpc.php";

        private const string RecentPostsXml =
            "<?xml version=\"1.0\"?>"
            + "<methodResponse><params><param><value><array><data>"
            + "<value><struct>"
            + "<member><name>dateCreated</name><value><dateTime.iso8601>20240310T14:22:31</dateTime.iso8601></value></member>"
            + "<member><name>postid</name><value><string>412</string></value></member>"
            + "<member><name>title</name><value><string>Hello macOS</string></value></member>"
            + "<member><name>description</name><value><string>&lt;p&gt;Main body&lt;/p&gt;</string></value></member>"
            + "<member><name>mt_text_more</name><value><string>&lt;p&gt;Extended body&lt;/p&gt;</string></value></member>"
            + "<member><name>categories</name><value><array><data>"
            + "<value><string>News</string></value><value><string>macOS</string></value>"
            + "</data></array></value></member>"
            + "<member><name>permalink</name><value><string>https://blog.example.com/hello-macos</string></value></member>"
            + "<member><name>post_status</name><value><string>publish</string></value></member>"
            + "</struct></value>"
            + "<value><struct>"
            + "<member><name>postid</name><value><int>411</int></value></member>"
            + "<member><name>title</name><value><string>Draft thoughts</string></value></member>"
            + "<member><name>description</name><value><string>&lt;p&gt;Only main&lt;/p&gt;</string></value></member>"
            + "<member><name>post_status</name><value><string>draft</string></value></member>"
            + "</struct></value>"
            + "</data></array></value></param></params></methodResponse>";

        private const string GetPostXml =
            "<?xml version=\"1.0\"?>"
            + "<methodResponse><params><param><value><struct>"
            + "<member><name>postid</name><value><string>412</string></value></member>"
            + "<member><name>title</name><value><string>Hello macOS</string></value></member>"
            + "<member><name>description</name><value><string>&lt;p&gt;Main body&lt;/p&gt;</string></value></member>"
            + "<member><name>mt_text_more</name><value><string>&lt;p&gt;Extended body&lt;/p&gt;</string></value></member>"
            + "<member><name>mt_keywords</name><value><string>avalonia, macos</string></value></member>"
            + "<member><name>dateCreated</name><value><dateTime.iso8601>20240310T14:22:31</dateTime.iso8601></value></member>"
            + "<member><name>permalink</name><value><string>https://blog.example.com/hello-macos</string></value></member>"
            + "<member><name>post_status</name><value><string>publish</string></value></member>"
            + "</struct></value></param></params></methodResponse>";

        private const string GetPagesXml =
            "<?xml version=\"1.0\"?>"
            + "<methodResponse><params><param><value><array><data>"
            + "<value><struct>"
            + "<member><name>page_id</name><value><string>87</string></value></member>"
            + "<member><name>page_title</name><value><string>About</string></value></member>"
            + "<member><name>description</name><value><string>&lt;p&gt;About us&lt;/p&gt;</string></value></member>"
            + "<member><name>mt_text_more</name><value><string></string></value></member>"
            + "<member><name>page_status</name><value><string>publish</string></value></member>"
            + "<member><name>permalink</name><value><string>https://blog.example.com/about</string></value></member>"
            + "</struct></value>"
            + "</data></array></value></param></params></methodResponse>";

        // ---- getRecentPosts parsing (fixture, pure) ----

        [Test]
        public void ParseRecentPosts_FullStruct_AllFields()
        {
            var posts = MetaWeblogXmlRpcClient.ParseServerPostsResponse(RecentPostsXml);

            Assert.That(posts.Count, Is.EqualTo(2));
            ServerPost first = posts[0];
            Assert.That(first.PostId, Is.EqualTo("412"));
            Assert.That(first.Title, Is.EqualTo("Hello macOS"));
            Assert.That(first.Description, Is.EqualTo("<p>Main body</p>"));
            Assert.That(first.TextMore, Is.EqualTo("<p>Extended body</p>"));
            Assert.That(first.Categories, Is.EqualTo(new[] { "News", "macOS" }));
            Assert.That(first.Permalink, Is.EqualTo("https://blog.example.com/hello-macos"));
            Assert.That(first.Status, Is.EqualTo("publish"));
            Assert.That(first.IsPage, Is.False);
            Assert.That(first.DateCreatedUtc, Is.EqualTo(new DateTime(2024, 3, 10, 14, 22, 31, DateTimeKind.Utc)));
        }

        [Test]
        public void ParseRecentPosts_SparseStruct_ToleratesMissingMembers()
        {
            var posts = MetaWeblogXmlRpcClient.ParseServerPostsResponse(RecentPostsXml);

            ServerPost second = posts[1];
            Assert.That(second.PostId, Is.EqualTo("411"), "int postid must parse like a string one");
            Assert.That(second.TextMore, Is.Empty);
            Assert.That(second.Categories, Is.Empty);
            Assert.That(second.Permalink, Is.Empty);
            Assert.That(second.DateCreatedUtc, Is.Null);
            Assert.That(second.HasBody, Is.True);
        }

        [Test]
        public void ParseRecentPosts_EmptyArray_ReturnsEmpty()
        {
            const string xml =
                "<?xml version=\"1.0\"?><methodResponse><params><param>"
                + "<value><array><data></data></array></value>"
                + "</param></params></methodResponse>";

            Assert.That(MetaWeblogXmlRpcClient.ParseServerPostsResponse(xml), Is.Empty);
        }

        // ---- BodyHtml rejoin (extended-entry round-trip) ----

        [Test]
        public void BodyHtml_JoinsMainAndExtended_WithMoreBreak()
        {
            var posts = MetaWeblogXmlRpcClient.ParseServerPostsResponse(RecentPostsXml);

            Assert.That(posts[0].BodyHtml,
                Is.EqualTo("<p>Main body</p>" + BlogPost.ExtendedEntryBreak + "<p>Extended body</p>"));
        }

        [Test]
        public void BodyHtml_NoExtended_IsMainOnly()
        {
            var posts = MetaWeblogXmlRpcClient.ParseServerPostsResponse(RecentPostsXml);

            Assert.That(posts[1].BodyHtml, Is.EqualTo("<p>Only main</p>"));
        }

        [Test]
        public void BodyHtml_RoundTrips_ThroughBlogPostContents()
        {
            // The publish-time split (BlogPost.Contents) and the fetch-time rejoin
            // (ServerPost.BodyHtml) must be exact inverses.
            var original = new BlogPost { Title = "t" };
            original.Contents = "<p>Main body</p>" + BlogPost.ExtendedEntryBreak + "<p>Extended body</p>";

            var fetched = new ServerPost
            {
                Description = original.MainContents,
                TextMore = original.ExtendedContents
            };

            Assert.That(fetched.BodyHtml, Is.EqualTo(original.Contents));
        }

        // ---- getPost parsing (fixture, pure) ----

        [Test]
        public void ParseGetPost_FullStruct_IncludesKeywords()
        {
            ServerPost post = MetaWeblogXmlRpcClient.ParseGetPostResponse(GetPostXml);

            Assert.That(post.PostId, Is.EqualTo("412"));
            Assert.That(post.Title, Is.EqualTo("Hello macOS"));
            Assert.That(post.Description, Is.EqualTo("<p>Main body</p>"));
            Assert.That(post.TextMore, Is.EqualTo("<p>Extended body</p>"));
            Assert.That(post.Keywords, Is.EqualTo("avalonia, macos"));
            Assert.That(post.Status, Is.EqualTo("publish"));
        }

        // ---- wp.getPages parsing (fixture, pure) ----

        [Test]
        public void ParsePages_PageStruct_UsesPageMemberNames_AndMarksIsPage()
        {
            var pages = MetaWeblogXmlRpcClient.ParseServerPostsResponse(GetPagesXml, isPage: true);

            Assert.That(pages.Count, Is.EqualTo(1));
            ServerPost page = pages[0];
            Assert.That(page.PostId, Is.EqualTo("87"));
            Assert.That(page.Title, Is.EqualTo("About"));
            Assert.That(page.Description, Is.EqualTo("<p>About us</p>"));
            Assert.That(page.Status, Is.EqualTo("publish"));
            Assert.That(page.IsPage, Is.True);
        }

        // ---- dateCreated tolerance ----

        [TestCase("20240310T14:22:31", true)]
        [TestCase("20240310T14:22:31Z", true)]
        [TestCase("not-a-date", false)]
        [TestCase("", false)]
        public void ParseRecentPosts_DateCreated_Tolerant(string dateValue, bool expectParsed)
        {
            string xml =
                "<?xml version=\"1.0\"?><methodResponse><params><param><value><array><data>"
                + "<value><struct>"
                + "<member><name>postid</name><value><string>1</string></value></member>"
                + "<member><name>dateCreated</name><value><dateTime.iso8601>" + dateValue + "</dateTime.iso8601></value></member>"
                + "</struct></value>"
                + "</data></array></value></param></params></methodResponse>";

            var posts = MetaWeblogXmlRpcClient.ParseServerPostsResponse(xml);
            Assert.That(posts.Count, Is.EqualTo(1), "a bad date must not fail the fetch");
            Assert.That(posts[0].DateCreatedUtc.HasValue, Is.EqualTo(expectParsed));
        }

        // ---- Transport-level: real client over fake HTTP ----

        [Test]
        public async Task GetRecentPostsAsync_SendsMethodAndCount_ParsesResponse()
        {
            var handler = new FakeHandler((req, ct) => Task.FromResult(Respond(RecentPostsXml)));
            var client = new MetaWeblogXmlRpcClient(Endpoint, "user", "pw",
                httpClient: new HttpClient(handler));

            var posts = await client.GetRecentPostsAsync("blog-7", 25);

            Assert.That(handler.LastRequestBody, Does.Contain("metaWeblog.getRecentPosts"));
            Assert.That(handler.LastRequestBody, Does.Contain("<string>blog-7</string>"));
            Assert.That(handler.LastRequestBody, Does.Contain("<int>25</int>"));
            Assert.That(posts.Count, Is.EqualTo(2));
            Assert.That(posts[0].PostId, Is.EqualTo("412"));
        }

        [Test]
        public async Task GetPostAsync_SendsPostId_ParsesResponse()
        {
            var handler = new FakeHandler((req, ct) => Task.FromResult(Respond(GetPostXml)));
            var client = new MetaWeblogXmlRpcClient(Endpoint, "user", "pw",
                httpClient: new HttpClient(handler));

            ServerPost post = await client.GetPostAsync("412");

            Assert.That(handler.LastRequestBody, Does.Contain("metaWeblog.getPost"));
            Assert.That(handler.LastRequestBody, Does.Contain("<string>412</string>"));
            Assert.That(post.Title, Is.EqualTo("Hello macOS"));
            Assert.That(post.Keywords, Is.EqualTo("avalonia, macos"));
        }

        [Test]
        public async Task GetPagesAsync_SendsWpGetPages_MarksIsPage()
        {
            var handler = new FakeHandler((req, ct) => Task.FromResult(Respond(GetPagesXml)));
            var client = new MetaWeblogXmlRpcClient(Endpoint, "user", "pw",
                httpClient: new HttpClient(handler));

            var pages = await client.GetPagesAsync("blog-7");

            Assert.That(handler.LastRequestBody, Does.Contain("wp.getPages"));
            Assert.That(handler.LastRequestBody, Does.Contain("<string>blog-7</string>"));
            Assert.That(pages.Count, Is.EqualTo(1));
            Assert.That(pages[0].IsPage, Is.True);
            Assert.That(pages[0].PostId, Is.EqualTo("87"));
        }

        [Test]
        public void GetRecentPostsAsync_XmlRpcFault_ThrowsBlogClientPublishException()
        {
            const string fault =
                "<?xml version=\"1.0\"?><methodResponse><fault><value><struct>"
                + "<member><name>faultCode</name><value><int>403</int></value></member>"
                + "<member><name>faultString</name><value><string>Incorrect username or password.</string></value></member>"
                + "</struct></value></fault></methodResponse>";
            var handler = new FakeHandler((req, ct) => Task.FromResult(Respond(fault)));
            var client = new MetaWeblogXmlRpcClient(Endpoint, "user", "wrong",
                httpClient: new HttpClient(handler));

            Assert.ThrowsAsync<BlogClientPublishException>(async () =>
                await client.GetRecentPostsAsync("blog-7", 10));
        }

        private static HttpResponseMessage Respond(string body) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, System.Text.Encoding.UTF8, "text/xml")
            };

        private sealed class FakeHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _respond;

            public FakeHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond)
            {
                _respond = respond;
            }

            public string LastRequestBody { get; private set; }

            protected override async Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                LastRequestBody = request.Content != null
                    ? await request.Content.ReadAsStringAsync()
                    : string.Empty;
                return await _respond(request, cancellationToken);
            }
        }
    }
}
