// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using OpenLiveWriter.Publishing;
using OpenLiveWriter.Publishing.Accounts;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group T (part 3) — the WordPress provider (Band 3a, P1-6):
    /// <see cref="BlogClientFactory"/> builds a <see cref="WordPressXmlRpcClient"/> for
    /// WordPress accounts, RSD detection reports WordPress when the engine/API list
    /// says so, and the /xmlrpc.php probe recovers an endpoint when the homepage
    /// advertises no RSD link. All offline (in-memory fake fetcher).
    /// </summary>
    [TestFixture]
    [Category("GroupT")]
    public class GroupT_WordPressProviderTests
    {
        private const string WpRsd =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>"
            + "<rsd version=\"1.0\" xmlns=\"http://archipelago.phrasewise.com/rsd\">"
            + "<service>"
            + "<engineName>WordPress</engineName>"
            + "<engineLink>https://wordpress.org/</engineLink>"
            + "<homePageLink>https://blog.example.com/</homePageLink>"
            + "<apis>"
            + "<api name=\"WordPress\" blogID=\"1\" preferred=\"true\" apiLink=\"https://blog.example.com/xmlrpc.php\" />"
            + "<api name=\"MetaWeblog\" blogID=\"1\" preferred=\"false\" apiLink=\"https://blog.example.com/xmlrpc.php\" />"
            + "</apis>"
            + "</service>"
            + "</rsd>";

        private const string GenericRsd =
            "<rsd version=\"1.0\"><service><engineName>Generic CMS</engineName><apis>"
            + "<api name=\"MetaWeblog\" blogID=\"7\" apiLink=\"https://blog.example.com/api/xmlrpc\" />"
            + "</apis></service></rsd>";

        // ---- Factory ----

        [Test]
        public void Factory_WordPressProvider_BuildsWordPressClient()
        {
            var account = new BlogAccount
            {
                ApiEndpointUrl = "https://blog.example.com/xmlrpc.php",
                Username = "author",
                ProviderType = BlogAccount.WordPressProviderType
            };

            IBlogClient client = BlogClientFactory.CreateClient(account, "pw");

            Assert.That(client, Is.TypeOf<WordPressXmlRpcClient>());
        }

        [Test]
        public void Factory_MetaWeblogProvider_BuildsMetaWeblogClient_NotWordPress()
        {
            var account = new BlogAccount
            {
                ApiEndpointUrl = "https://blog.example.com/xmlrpc.php",
                Username = "author"
            };

            IBlogClient client = BlogClientFactory.CreateClient(account, "pw");

            // TypeOf (exact): a WordPress subclass must not leak into MetaWeblog accounts.
            Assert.That(client, Is.TypeOf<MetaWeblogXmlRpcClient>());
        }

        [Test]
        public void Factory_WordPressClient_SupportsFetchAndPages()
        {
            // The WordPress transport inherits the full MetaWeblog + wp.* surface.
            var account = new BlogAccount { ProviderType = "wordpress" }; // case-insensitive
            IBlogClient client = BlogClientFactory.CreateClient(account, "pw");

            Assert.That(client, Is.TypeOf<WordPressXmlRpcClient>());
        }

        [Test]
        public void Factory_UnsupportedProvider_StillThrows()
        {
            var account = new BlogAccount { ProviderType = "AtomPub" };
            Assert.Throws<NotSupportedException>(() => BlogClientFactory.CreateClient(account, "pw"));
        }

        // ---- Provider selection from RSD ----

        [Test]
        public void DetectProviderType_WordPressEngine_ReturnsWordPress()
        {
            RsdServiceDescription rsd = RsdServiceDetector.ParseRsd(WpRsd, "https://blog.example.com/");
            Assert.That(RsdServiceDetector.DetectProviderType(rsd),
                Is.EqualTo(BlogAccount.WordPressProviderType));
        }

        [Test]
        public void DetectProviderType_GenericEngineWithMetaWeblogApi_ReturnsMetaWeblog()
        {
            RsdServiceDescription rsd = RsdServiceDetector.ParseRsd(GenericRsd, "https://blog.example.com/");
            Assert.That(RsdServiceDetector.DetectProviderType(rsd),
                Is.EqualTo(BlogAccount.DefaultProviderType));
        }

        [Test]
        public void DetectProviderType_WordPressApiAdvertised_GenericEngine_ReturnsWordPress()
        {
            const string rsdXml =
                "<rsd version=\"1.0\"><service><engineName>My Custom Engine</engineName><apis>"
                + "<api name=\"WordPress\" blogID=\"1\" preferred=\"true\" apiLink=\"https://blog.example.com/xmlrpc.php\" />"
                + "<api name=\"MetaWeblog\" blogID=\"1\" preferred=\"false\" apiLink=\"https://blog.example.com/xmlrpc.php\" />"
                + "</apis></service></rsd>";

            RsdServiceDescription rsd = RsdServiceDetector.ParseRsd(rsdXml, "https://blog.example.com/");
            Assert.That(RsdServiceDetector.DetectProviderType(rsd),
                Is.EqualTo(BlogAccount.WordPressProviderType));
        }

        [Test]
        public void DetectProviderType_NullDescription_ReturnsDefault()
        {
            Assert.That(RsdServiceDetector.DetectProviderType(null),
                Is.EqualTo(BlogAccount.DefaultProviderType));
        }

        [Test]
        public void Detect_WordPressRsd_ReportsWordPressProvider()
        {
            var fetcher = new FakeFetcher
            {
                ["https://blog.example.com/"] =
                    "<html><head><link rel=\"EditURI\" type=\"application/rsd+xml\" href=\"/xmlrpc.php?rsd\"></head></html>",
                ["https://blog.example.com/xmlrpc.php?rsd"] = WpRsd
            };

            RsdDetectionResult result = RsdServiceDetector.Detect("https://blog.example.com/", fetcher);

            Assert.That(result.Found, Is.True);
            Assert.That(result.EndpointUrl, Is.EqualTo("https://blog.example.com/xmlrpc.php"));
            Assert.That(result.ProviderType, Is.EqualTo(BlogAccount.WordPressProviderType));
        }

        [Test]
        public void Detect_GenericRsd_ReportsMetaWeblogProvider()
        {
            var fetcher = new FakeFetcher
            {
                ["https://blog.example.com/"] =
                    "<html><head><link rel=\"EditURI\" href=\"/rsd.xml\"></head></html>",
                ["https://blog.example.com/rsd.xml"] = GenericRsd
            };

            RsdDetectionResult result = RsdServiceDetector.Detect("https://blog.example.com/", fetcher);

            Assert.That(result.Found, Is.True);
            Assert.That(result.ProviderType, Is.EqualTo(BlogAccount.DefaultProviderType));
        }

        // ---- /xmlrpc.php fallback probe ----

        [Test]
        public void Detect_NoRsdLink_ProbeFindsXmlRpcPhp_WordPressProvider()
        {
            var fetcher = new FakeFetcher
            {
                ["https://blog.example.com/"] = "<html><head><title>No RSD here</title></head></html>",
                ["https://blog.example.com/xmlrpc.php"] = "XML-RPC server accepts POST requests only."
            };

            RsdDetectionResult result = RsdServiceDetector.Detect("https://blog.example.com/", fetcher);

            Assert.That(result.Found, Is.True, "the probe should recover the conventional WordPress endpoint");
            Assert.That(result.EndpointUrl, Is.EqualTo("https://blog.example.com/xmlrpc.php"));
            Assert.That(result.ProviderType, Is.EqualTo(BlogAccount.WordPressProviderType));
        }

        [Test]
        public void Detect_NoRsdLink_ProbeMisses_NotFound()
        {
            var fetcher = new FakeFetcher
            {
                ["https://blog.example.com/"] = "<html><head><title>No RSD here</title></head></html>"
                // no /xmlrpc.php entry — the fake fetcher returns null (404-ish)
            };

            RsdDetectionResult result = RsdServiceDetector.Detect("https://blog.example.com/", fetcher);

            Assert.That(result.Found, Is.False);
            Assert.That(result.EndpointUrl, Is.Empty);
        }

        [Test]
        public void Detect_NoRsdLink_ProbeReturnsHtmlNotXmlRpc_NotFound()
        {
            var fetcher = new FakeFetcher
            {
                ["https://blog.example.com/"] = "<html><head><title>No RSD here</title></head></html>",
                ["https://blog.example.com/xmlrpc.php"] = "<html><body>404 Not Found</body></html>"
            };

            RsdDetectionResult result = RsdServiceDetector.Detect("https://blog.example.com/", fetcher);

            Assert.That(result.Found, Is.False, "an HTML 404 page is not an XML-RPC endpoint");
        }

        [TestCase("XML-RPC server accepts POST requests only.", true)]
        [TestCase("<?xml version=\"1.0\"?><methodResponse><params/></methodResponse>", true)]
        [TestCase("<html><body>hello</body></html>", false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void LooksLikeXmlRpcEndpoint_Classifies(string body, bool expected)
        {
            Assert.That(RsdServiceDetector.LooksLikeXmlRpcEndpoint(body), Is.EqualTo(expected));
        }

        private sealed class FakeFetcher : IRsdHttpFetcher
        {
            private readonly Dictionary<string, string> _map = new Dictionary<string, string>(StringComparer.Ordinal);
            public string this[string url] { set => _map[url] = value; }
            public string Fetch(string url) => _map.TryGetValue(url, out string content) ? content : null;
        }
    }
}
