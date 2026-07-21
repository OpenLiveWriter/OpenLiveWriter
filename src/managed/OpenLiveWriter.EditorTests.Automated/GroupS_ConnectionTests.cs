// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia;
using OpenLiveWriter.App.Avalonia.Dialogs;
using OpenLiveWriter.App.Avalonia.Settings;
using OpenLiveWriter.EditorTests.Automated.Infrastructure;
using OpenLiveWriter.Publishing;
using OpenLiveWriter.Publishing.Accounts;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group S — "Test Connection" credential verification (account dialog) and the
    /// post-publish follow-up preferences (view-after-publish / close-on-publish).
    /// The verifier runs against a fake <see cref="HttpMessageHandler"/> so the real
    /// <see cref="MetaWeblogXmlRpcClient"/> transport is exercised end-to-end with no
    /// network; a single opt-in <see cref="ExplicitAttribute"/> test hits a live
    /// endpoint. All headless.
    /// </summary>
    [TestFixture]
    [Category("GroupS")]
    public class GroupS_ConnectionTests
    {
        private const string Endpoint = "https://blog.example.com/xmlrpc.php";

        private const string UsersBlogsOk =
            "<?xml version=\"1.0\"?>"
            + "<methodResponse><params><param><value><array><data></data></array></value></param></params></methodResponse>";

        private const string Fault403 =
            "<?xml version=\"1.0\"?>"
            + "<methodResponse><fault><value><struct>"
            + "<member><name>faultCode</name><value><int>403</int></value></member>"
            + "<member><name>faultString</name><value><string>Incorrect username or password.</string></value></member>"
            + "</struct></value></fault></methodResponse>";

        // ---- MetaWeblogConnectionVerifier over the real transport (fake HTTP) ----

        [Test]
        public async Task Verify_Success_Completes_AndSendsGetUsersBlogs()
        {
            var handler = new FakeHandler((req, ct) => Task.FromResult(Respond(UsersBlogsOk)));
            var verifier = new MetaWeblogConnectionVerifier(() => new HttpClient(handler));

            await verifier.VerifyAsync(Endpoint, "user", "pw", CancellationToken.None);

            Assert.That(handler.LastRequestBody, Does.Contain("blogger.getUsersBlogs"));
            Assert.That(handler.LastRequestBody, Does.Contain("<string>user</string>"));
        }

        [Test]
        public void Verify_XmlRpcFault_ThrowsBlogClientPublishException()
        {
            var handler = new FakeHandler((req, ct) => Task.FromResult(Respond(Fault403)));
            var verifier = new MetaWeblogConnectionVerifier(() => new HttpClient(handler));

            var ex = Assert.ThrowsAsync<BlogClientPublishException>(async () =>
                await verifier.VerifyAsync(Endpoint, "user", "wrong", CancellationToken.None));
            Assert.That(ex.Message, Does.Contain("403"));
            Assert.That(ex.Message, Does.Contain("Incorrect username or password."));
        }

        [Test]
        public void Verify_NetworkError_BubblesTransportException()
        {
            var handler = new FakeHandler((req, ct) =>
                Task.FromException<HttpResponseMessage>(new HttpRequestException("Name or service not known")));
            var verifier = new MetaWeblogConnectionVerifier(() => new HttpClient(handler));

            Assert.ThrowsAsync<HttpRequestException>(async () =>
                await verifier.VerifyAsync(Endpoint, "user", "pw", CancellationToken.None));
        }

        [Test]
        public void Verify_BlankEndpoint_Throws()
        {
            var verifier = new MetaWeblogConnectionVerifier(
                () => new HttpClient(new FakeHandler((req, ct) => Task.FromResult(Respond(UsersBlogsOk)))));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await verifier.VerifyAsync(string.Empty, "user", "pw", CancellationToken.None));
        }

        [Test]
        public async Task Verify_Cancellation_Observed()
        {
            var handler = new FakeHandler((req, ct) =>
            {
                ct.ThrowIfCancellationRequested();
                return Task.FromResult(Respond(UsersBlogsOk));
            });
            var verifier = new MetaWeblogConnectionVerifier(() => new HttpClient(handler));

            using var cts = new CancellationTokenSource();
            cts.Cancel();
            // Catch (not Throws): the concrete type may be TaskCanceledException.
            Assert.CatchAsync<OperationCanceledException>(async () =>
                await verifier.VerifyAsync(Endpoint, "user", "pw", cts.Token));
            await Task.CompletedTask;
        }

        // ---- The transport is genuinely async (no sync-over-async) ----

        [Test]
        public async Task NewPostAsync_DoesNotCompleteSynchronously()
        {
            // A delayed response proves the async path yields instead of blocking the
            // caller's thread: a synchronous Send would return only after completion.
            var handler = new FakeHandler(async (req, ct) =>
            {
                await Task.Delay(200, ct);
                return Respond("<?xml version=\"1.0\"?><methodResponse><params><param>"
                    + "<value><string>post-9</string></value></param></params></methodResponse>");
            });
            var client = new MetaWeblogXmlRpcClient(Endpoint, "user", "pw",
                httpClient: new HttpClient(handler));

            var post = new BlogPost { Title = "t" };
            post.Contents = "<p>body</p>";
            Task<string> task = client.NewPostAsync("blog-1", post, publish: false);

            Assert.That(task.IsCompleted, Is.False, "transport must yield while the network round-trip runs");
            Assert.That(await task, Is.EqualTo("post-9"));
        }

        // ---- Account dialog enable rule ----

        [TestCase(Endpoint, "user", "pw", true)]
        [TestCase("", "user", "pw", false)]
        [TestCase(Endpoint, "", "pw", false)]
        [TestCase(Endpoint, "user", "", false)]
        [TestCase(Endpoint, "user", null, false)]
        public void AccountDialog_TestConnectionEnableRule(string endpoint, string user, string pw, bool expected)
        {
            Assert.That(AccountDialog.CanTestConnection(endpoint, user, pw), Is.EqualTo(expected));
        }

        // ---- Post-publish follow-up preference mapping ----

        [Test]
        public void ViewAfterPublish_OnlyForPublish_WithPrefAndHomepage()
        {
            var prefs = new AppPreferences { ViewPostAfterPublish = true };
            var account = new BlogAccount { HomepageUrl = "https://blog.example.com/" };

            Assert.That(MainWindow.ShouldViewPostAfterPublish(prefs, publish: true, account), Is.True);
            Assert.That(MainWindow.ShouldViewPostAfterPublish(prefs, publish: false, account), Is.False,
                "server drafts must not open the browser");
            Assert.That(MainWindow.ShouldViewPostAfterPublish(
                new AppPreferences { ViewPostAfterPublish = false }, publish: true, account), Is.False);
            Assert.That(MainWindow.ShouldViewPostAfterPublish(
                prefs, publish: true, new BlogAccount { HomepageUrl = " " }), Is.False,
                "no homepage — nothing honest to open");
        }

        [Test]
        public void CloseAfterPublish_FollowsPreference()
        {
            Assert.That(MainWindow.ShouldCloseAfterPublish(
                new AppPreferences { CloseWindowOnPublish = true }), Is.True);
            Assert.That(MainWindow.ShouldCloseAfterPublish(
                new AppPreferences { CloseWindowOnPublish = false }), Is.False);
            Assert.That(MainWindow.ShouldCloseAfterPublish(null), Is.False);
        }

        // ---- Browser launch seam ----

        [Test]
        public void BrowserLauncher_BlankUrl_ReturnsFalse()
        {
            Assert.That(BrowserLauncher.Open(string.Empty), Is.False);
            Assert.That(BrowserLauncher.Open(null), Is.False);
        }

        [Test]
        public void BrowserLauncher_UrlHandler_InterceptsLaunch()
        {
            string launched = null;
            BrowserLauncher.UrlHandler = url => launched = url;
            try
            {
                Assert.That(BrowserLauncher.Open("https://blog.example.com/"), Is.True);
                Assert.That(launched, Is.EqualTo("https://blog.example.com/"));
            }
            finally
            {
                BrowserLauncher.UrlHandler = null;
            }
        }

        // ---- Opt-in live verification ----

        [Test]
        [Category(WebViewCategories.LiveBlog)]
        [Explicit("Performs a live blogger.getUsersBlogs call; requires OLW_LIVEBLOG_* env vars.")]
        public async Task LiveVerify_Credentials_Succeeds()
        {
            string endpoint = Environment.GetEnvironmentVariable("OLW_LIVEBLOG_ENDPOINT");
            string user = Environment.GetEnvironmentVariable("OLW_LIVEBLOG_USER");
            string pass = Environment.GetEnvironmentVariable("OLW_LIVEBLOG_PASS");

            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
                Assert.Ignore("Set OLW_LIVEBLOG_ENDPOINT/USER/PASS to run the live connection test.");

            await new MetaWeblogConnectionVerifier().VerifyAsync(
                endpoint, user, pass, CancellationToken.None);
            TestContext.WriteLine($"Live connection test OK: endpoint={endpoint}");
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
