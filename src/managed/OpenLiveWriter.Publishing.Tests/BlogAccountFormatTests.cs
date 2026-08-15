// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using NUnit.Framework;
using OpenLiveWriter.Publishing.Accounts;

namespace OpenLiveWriter.Publishing.Tests
{
    [TestFixture]
    public class BlogAccountFormatTests
    {
        [Test]
        public void Defaults_EditingFormatIsHtml()
        {
            var account = new BlogAccount();
            Assert.That(account.EditingFormat, Is.EqualTo(ContentFormat.Html));
        }

        [Test]
        public void Defaults_PublishFormatIsHtml()
        {
            var account = new BlogAccount();
            Assert.That(account.PublishFormat, Is.EqualTo(ContentFormat.Html));
        }

        [Test]
        public void Clone_CopiesEditingAndPublishFormats()
        {
            var account = new BlogAccount
            {
                Id = "abc",
                DisplayName = "Test Blog",
                EditingFormat = ContentFormat.Markdown,
                PublishFormat = ContentFormat.Markdown
            };

            BlogAccount clone = account.Clone();

            Assert.That(clone.EditingFormat, Is.EqualTo(ContentFormat.Markdown));
            Assert.That(clone.PublishFormat, Is.EqualTo(ContentFormat.Markdown));
            Assert.That(clone.Id, Is.EqualTo(account.Id));
            Assert.That(clone.DisplayName, Is.EqualTo(account.DisplayName));
        }
    }
}
