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
            Assert.That(fm.Title, Is.EqualTo(expected.Title));
            Assert.That(fm.Date, Is.EqualTo(expected.Date));
            Assert.That(fm.Layout, Is.EqualTo(expected.Layout));
            Assert.That(fm.Tags, Is.EqualTo(expected.Tags));
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
            Assert.That(fm.Title, Is.EqualTo(expected.Title));
            Assert.That(fm.Date, Is.EqualTo(expected.Date));
            Assert.That(fm.Layout, Is.EqualTo(expected.Layout));
            Assert.That(fm.Tags, Is.EqualTo(expected.Tags));
        }

        [Test]
        public void Serialize_Basic()
        {
            // Expected
            var expected = @"title: Test title
date: 2019-01-01 00:00:00
layout: post
";

            // Act
            var fm = new StaticSiteItemFrontMatter(new StaticSiteConfigFrontMatterKeys())
            {
                Title = "Test title",
                Date = "2019-01-01 00:00:00",
                Layout = "post"
            };

            // Assert
            Assert.That(fm.Serialize(), Is.EqualTo(expected));
        }


        [Test]
        public void Serialize_WithTags()
        {
            // Expected
            var expected = @"title: Test title
date: 2019-01-01 00:00:00
layout: post
tags:
- hello
- world
";

            // Act
            var fm = new StaticSiteItemFrontMatter(new StaticSiteConfigFrontMatterKeys())
            {
                Title = "Test title",
                Date = "2019-01-01 00:00:00",
                Layout = "post",
                Tags = new string[] {"hello", "world"}
            };

            // Assert
            Assert.That(fm.Serialize(), Is.EqualTo(expected));
        }
    }
}
