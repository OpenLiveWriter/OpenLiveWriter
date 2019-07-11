using System.Linq;
using NUnit.Framework;
using OpenLiveWriter.BlogClient.Clients.StaticSite;

namespace OpenLiveWriter.Tests.BlogClient.Clients.StaticSite
{
    [TestFixture]
    class StaticSitePostFrontMatterTests
    {
        [Test]
        public void Deserialize_Basic()
        {
            // Expected
            var expected = new StaticSitePostFrontMatter()
            {
                Title = "Test title",
                Date = "2019-01-01 00:00:00",
                Layout = "post",
                Tags = new string[] { "programming", ".net" }
            };

            // Act
            var fm = new StaticSitePostFrontMatter();
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
            var expected = new StaticSitePostFrontMatter()
            {
                Title = "Test title",
                Date = "2019-01-01 00:00:00",
                Tags = new string[] {}
            };

            // Act
            var fm = new StaticSitePostFrontMatter();
            fm.Deserialize(@"title: Test title
date: 2019-01-01 00:00:00");

            // Assert
            Assert.AreEqual(fm.Title, expected.Title);
            Assert.AreEqual(fm.Date, expected.Date);
            Assert.AreEqual(fm.Layout, expected.Layout);
            Assert.IsTrue(Enumerable.SequenceEqual(fm.Tags, expected.Tags));
        }

    }
}
