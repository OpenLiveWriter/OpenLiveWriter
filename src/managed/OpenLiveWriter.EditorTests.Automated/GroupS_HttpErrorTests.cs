// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Dialogs;
using OpenLiveWriter.Publishing;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group S follow-up: HTTP-level error diagnostics. A non-success status from the
    /// endpoint (401/403/500) must surface as a <see cref="BlogClientHttpException"/>
    /// carrying the status, an actionable auth hint for 401/403, and a bounded,
    /// whitespace-collapsed snippet of the response body — never a bare
    /// "Response status code does not indicate success" dump. Also covers the
    /// MessageDialog overflow fix (long errors scroll instead of growing off-screen).
    /// </summary>
    [TestFixture]
    [Category("GroupS")]
    public class GroupS_HttpErrorTests
    {
        private const string Endpoint = "https://blog.example.com/xmlrpc.php";

        [Test]
        public void Http401_ThrowsWithStatusHintAndSnippet()
        {
            var client = BuildClient(HttpStatusCode.Unauthorized,
                "<html>\n  <body>\n    <h1>401 Authorization Required</h1>\n  </body>\n</html>");

            var ex = Assert.CatchAsync<BlogClientHttpException>(async () =>
                await client.VerifyCredentialsAsync());

            Assert.That(ex.StatusCode, Is.EqualTo(401));
            Assert.That(ex.Message, Does.Contain("HTTP 401"));
            Assert.That(ex.Message, Does.Contain("application password"),
                "401/403 must hint at the application-password requirement");
            Assert.That(ex.Message, Does.Contain("401 Authorization Required"),
                "the server body snippet must be visible so users can see what rejected the call");
            Assert.That(ex.Message, Does.Not.Contain("\n"), "snippet is whitespace-collapsed");
        }

        [Test]
        public void Http403_IncludesAuthHint()
        {
            var client = BuildClient(HttpStatusCode.Forbidden, "Forbidden");

            var ex = Assert.CatchAsync<BlogClientHttpException>(async () =>
                await client.VerifyCredentialsAsync());

            Assert.That(ex.StatusCode, Is.EqualTo(403));
            Assert.That(ex.Message, Does.Contain("application password"));
        }

        [Test]
        public void Http500_NoAuthHint_ButStatusAndSnippet()
        {
            var client = BuildClient(HttpStatusCode.InternalServerError, "boom");

            var ex = Assert.CatchAsync<BlogClientHttpException>(async () =>
                await client.VerifyCredentialsAsync());

            Assert.That(ex.StatusCode, Is.EqualTo(500));
            Assert.That(ex.Message, Does.Contain("HTTP 500"));
            Assert.That(ex.Message, Does.Contain("boom"));
            Assert.That(ex.Message, Does.Not.Contain("application password"));
        }

        [Test]
        public void LongErrorBody_IsBounded()
        {
            string huge = new string('x', 5000);
            var client = BuildClient(HttpStatusCode.Unauthorized, huge);

            var ex = Assert.CatchAsync<BlogClientHttpException>(async () =>
                await client.VerifyCredentialsAsync());

            Assert.That(ex.Message.Length, Is.LessThan(800),
                "a huge error page must not balloon the user-facing message");
        }

        [Test]
        public void XmlRpcFault_OnSuccessStatus_Unchanged()
        {
            var handler = new FakeHandler((req, ct) => Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        "<?xml version=\"1.0\"?><methodResponse><fault><value><struct>"
                        + "<member><name>faultCode</name><value><int>403</int></value></member>"
                        + "<member><name>faultString</name><value><string>Incorrect username or password.</string></value></member>"
                        + "</struct></value></fault></methodResponse>",
                        System.Text.Encoding.UTF8, "text/xml")
                }));
            var client = new MetaWeblogXmlRpcClient(Endpoint, "user", "pw",
                httpClient: new HttpClient(handler));

            var ex = Assert.CatchAsync<BlogClientPublishException>(async () =>
                await client.VerifyCredentialsAsync());

            Assert.That(ex, Is.Not.InstanceOf<BlogClientHttpException>());
            Assert.That(ex.Message, Does.Contain("403"));
            Assert.That(ex.Message, Does.Contain("Incorrect username or password."));
        }

        [Avalonia.Headless.NUnit.AvaloniaTest]
        public void MessageDialog_LongMessage_ScrollsInsteadOfOverflowing()
        {
            var dialog = new MessageDialog("Error", new string('y', 4000));
            try
            {
                dialog.Show();
                var scroller = dialog.GetLogicalDescendants().OfType<ScrollViewer>().FirstOrDefault();
                Assert.That(scroller, Is.Not.Null, "message text must live in a ScrollViewer");
                Assert.That(scroller.MaxHeight, Is.GreaterThan(0).And.LessThanOrEqualTo(400));
            }
            finally
            {
                dialog.Close();
            }
        }

        private static MetaWeblogXmlRpcClient BuildClient(HttpStatusCode status, string body)
        {
            var handler = new FakeHandler((req, ct) => Task.FromResult(
                new HttpResponseMessage(status)
                {
                    Content = new StringContent(body, System.Text.Encoding.UTF8, "text/html")
                }));
            return new MetaWeblogXmlRpcClient(Endpoint, "user", "pw",
                httpClient: new HttpClient(handler));
        }

        private sealed class FakeHandler : HttpMessageHandler
        {
            private readonly System.Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _respond;

            public FakeHandler(System.Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond)
            {
                _respond = respond;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken) =>
                _respond(request, cancellationToken);
        }
    }
}
