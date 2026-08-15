using System.Linq;
using NUnit.Framework;
using OpenLiveWriter.BlogClient.Clients.StaticSite;

namespace OpenLiveWriter.Tests.BlogClient.Clients.StaticSite
{
    [TestFixture]
    class StaticSiteItemFrontMatterTests
    {
        [Test]
        public void Deserialize_Basic()
        {
            // Expected
            var expected = new StaticSiteItemFrontMatter(new StaticSiteConfigFrontMatterKeys())
            {
                Title = "Test title",
                Date = "2019-01-01 00:00:00",
                Layout = "post",
                Tags = new string[] { "programming", ".net" }
            };

            // Act
            var fm = new StaticSiteItemFrontMatter(new StaticSiteConfigFrontMatterKeys());
            fm.Deserialize(@"title: Test title
date: 2019-01-01 00:00:00
layout: post
tags:
  - programming
  - .net");

            // Assert
            Assert.AreEqual(fm.Title, expected.Title);
            Assert.AreEqual(fm.Date, expected.Date);
            Assert.AreEqual(fm.Layout, expected.Layout);
            Assert.IsTrue(Enumerable.SequenceEqual(fm.Tags, expected.Tags));
        }

        [Test]
        public void Deserialize_MissingKeys()
        {
            // Expected
            var expected = new StaticSiteItemFrontMatter(new StaticSiteConfigFrontMatterKeys())
            {
                Title = "Test title",
                Date = "2019-01-01 00:00:00",
                Tags = new string[] {}
            };

            // Act
            var fm = new StaticSiteItemFrontMatter(new StaticSiteConfigFrontMatterKeys());
            fm.Deserialize(@"title: Test title
date: 2019-01-01 00:00:00");

            // Assert
            Assert.AreEqual(fm.Title, expected.Title);
            Assert.AreEqual(fm.Date, expected.Date);
            Assert.AreEqual(fm.Layout, expected.Layout);
            Assert.IsTrue(Enumerable.SequenceEqual(fm.Tags, expected.Tags));
        }

        [Test]
        public void Serialize_Basic()
        {
            // Expected. Normalized because a verbatim literal carries whatever
            // line endings the working copy was checked out with, while
            // Serialize always emits LF by contract: front matter is read by
            // static site generators cross-platform. Without this the test
            // passes on an LF checkout and fails on a CRLF one.
            var expected = @"title: Test title
date: 2019-01-01 00:00:00
layout: post
".Replace("\r\n", "\n");

            // Act
            var fm = new StaticSiteItemFrontMatter(new StaticSiteConfigFrontMatterKeys())
            {
                Title = "Test title",
                Date = "2019-01-01 00:00:00",
                Layout = "post"
            };

            // Assert
            Assert.AreEqual(expected, fm.Serialize());
        }


        [Test]
        public void Serialize_WithTags()
        {
            // Expected (normalized to LF; see Serialize_Basic).
            var expected = @"title: Test title
date: 2019-01-01 00:00:00
layout: post
tags:
- hello
- world
".Replace("\r\n", "\n");

            // Act
            var fm = new StaticSiteItemFrontMatter(new StaticSiteConfigFrontMatterKeys())
            {
                Title = "Test title",
                Date = "2019-01-01 00:00:00",
                Layout = "post",
                Tags = new string[] {"hello", "world"}
            };

            // Assert
            Assert.AreEqual(expected, fm.Serialize());
        }
    }
}
