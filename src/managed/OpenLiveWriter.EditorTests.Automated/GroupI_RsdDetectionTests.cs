// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenLiveWriter.EditorTests.Automated.Infrastructure;
using OpenLiveWriter.Publishing.Accounts;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group I — RSD provider endpoint auto-detection. The HTML-link and RSD-XML parsing
    /// are exercised as pure functions against fixtures, and the full detect flow is driven
    /// through an in-memory fake fetcher so no network is touched. A single opt-in
    /// <see cref="ExplicitAttribute"/> test performs a real detection when an env var is set.
    /// </summary>
    [TestFixture]
    [Category("GroupI")]
    public class GroupI_RsdDetectionTests
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
            + "<api name=\"Movable Type\" blogID=\"1\" preferred=\"false\" apiLink=\"https://blog.example.com/xmlrpc.php\" />"
            + "<api name=\"MetaWeblog\" blogID=\"1\" preferred=\"false\" apiLink=\"https://blog.example.com/xmlrpc.php\" />"
            + "</apis>"
            + "</service>"
            + "</rsd>";

        // ---- FindRsdUrl ----

        [Test]
        public void FindRsdUrl_FindsEditUriLink_AndResolvesRelative()
        {
            string html =
                "<html><head>"
                + "<link rel=\"EditURI\" type=\"application/rsd+xml\" title=\"RSD\" href=\"/xmlrpc.php?rsd\" />"
                + "</head><body></body></html>";

            string url = RsdServiceDetector.FindRsdUrl(html, "https://blog.example.com/");
            Assert.That(url, Is.EqualTo("https://blog.example.com/xmlrpc.php?rsd"));
        }

        [Test]
        public void FindRsdUrl_AbsoluteHref_Unchanged()
        {
            string html = "<link rel=\"editURI\" href=\"https://other.example.com/rsd.xml\">";
            string url = RsdServiceDetector.FindRsdUrl(html, "https://blog.example.com/");
            Assert.That(url, Is.EqualTo("https://other.example.com/rsd.xml"));
        }

        [Test]
        public void FindRsdUrl_ByRsdType_WhenNoEditUriRel()
        {
            string html = "<link type=\"application/rsd+xml\" href=\"/rsd.xml\">";
            string url = RsdServiceDetector.FindRsdUrl(html, "https://blog.example.com/sub/");
            Assert.That(url, Is.EqualTo("https://blog.example.com/rsd.xml"));
        }

        [Test]
        public void FindRsdUrl_NoLink_ReturnsNull()
        {
            string html = "<html><head><title>No RSD here</title></head></html>";
            Assert.That(RsdServiceDetector.FindRsdUrl(html, "https://blog.example.com/"), Is.Null);
        }

        // ---- ParseRsd ----

        [Test]
        public void ParseRsd_ParsesEngineAndApis()
        {
            RsdServiceDescription rsd = RsdServiceDetector.ParseRsd(WpRsd, "https://blog.example.com/xmlrpc.php?rsd");

            Assert.That(rsd, Is.Not.Null);
            Assert.That(rsd.EngineName, Is.EqualTo("WordPress"));
            Assert.That(rsd.Apis.Count, Is.EqualTo(3));

            RsdApi mw = rsd.ScanForApi("MetaWeblog");
            Assert.That(mw, Is.Not.Null);
            Assert.That(mw.ApiLink, Is.EqualTo("https://blog.example.com/xmlrpc.php"));
            Assert.That(mw.BlogId, Is.EqualTo("1"));
        }

        [Test]
        public void ParseRsd_ResolvesRelativeApiLink()
        {
            string rsd =
                "<rsd version=\"1.0\"><service><engineName>Generic</engineName><apis>"
                + "<api name=\"MetaWeblog\" blogID=\"7\" apiLink=\"/api/xmlrpc\" />"
                + "</apis></service></rsd>";

            RsdServiceDescription parsed = RsdServiceDetector.ParseRsd(rsd, "https://blog.example.com/rsd.xml");
            Assert.That(parsed.ScanForApi("MetaWeblog").ApiLink, Is.EqualTo("https://blog.example.com/api/xmlrpc"));
        }

        [Test]
        public void ParseRsd_NoApis_ReturnsNull()
        {
            string rsd = "<rsd version=\"1.0\"><service><engineName>Empty</engineName><apis></apis></service></rsd>";
            Assert.That(RsdServiceDetector.ParseRsd(rsd, "x"), Is.Null);
        }

        [Test]
        public void SelectMetaWeblogApi_PrefersMetaWeblogByName()
        {
            RsdServiceDescription rsd = RsdServiceDetector.ParseRsd(WpRsd, "https://blog.example.com/");
            RsdApi api = RsdServiceDetector.SelectMetaWeblogApi(rsd);
            Assert.That(api.Name, Is.EqualTo("MetaWeblog"));
            Assert.That(api.ApiLink, Is.EqualTo("https://blog.example.com/xmlrpc.php"));
        }

        // ---- Full Detect flow (fake fetcher) ----

        [Test]
        public void Detect_EndToEnd_ResolvesMetaWeblogEndpoint()
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
            Assert.That(result.BlogId, Is.EqualTo("1"));
            Assert.That(result.EngineName, Is.EqualTo("WordPress"));
            Assert.That(result.RsdUrl, Is.EqualTo("https://blog.example.com/xmlrpc.php?rsd"));
        }

        [Test]
        public void Detect_NoRsdLink_NotFound()
        {
            var fetcher = new FakeFetcher
            {
                ["https://blog.example.com/"] = "<html><head><title>Nothing</title></head></html>"
            };

            RsdDetectionResult result = RsdServiceDetector.Detect("https://blog.example.com/", fetcher);
            Assert.That(result.Found, Is.False);
            Assert.That(result.EndpointUrl, Is.Empty);
        }

        [Test]
        public void Detect_FetchFailure_NotFound_DoesNotThrow()
        {
            var fetcher = new FakeFetcher(); // returns null for everything
            RsdDetectionResult result = RsdServiceDetector.Detect("https://blog.example.com/", fetcher);
            Assert.That(result.Found, Is.False);
        }

        // ---- Opt-in live detection ----

        [Test]
        [Category(WebViewCategories.LiveBlog)]
        [Explicit("Performs a live RSD fetch; set OLW_RSD_HOMEPAGE to run.")]
        public void LiveDetect_FromHomepage_FindsEndpoint()
        {
            string homepage = Environment.GetEnvironmentVariable("OLW_RSD_HOMEPAGE");
            if (string.IsNullOrEmpty(homepage))
                Assert.Ignore("Set OLW_RSD_HOMEPAGE to run the live RSD detection test.");

            RsdDetectionResult result = RsdServiceDetector.Detect(homepage, new HttpRsdFetcher());
            TestContext.WriteLine($"Detected endpoint={result.EndpointUrl}, blogId={result.BlogId}, engine={result.EngineName}");
            Assert.That(result.Found, Is.True, "Expected to detect an endpoint from the live homepage.");
        }

        private sealed class FakeFetcher : IRsdHttpFetcher
        {
            private readonly Dictionary<string, string> _map = new Dictionary<string, string>(StringComparer.Ordinal);
            public string this[string url] { set => _map[url] = value; }
            public string Fetch(string url) => _map.TryGetValue(url, out string content) ? content : null;
        }
    }
}
