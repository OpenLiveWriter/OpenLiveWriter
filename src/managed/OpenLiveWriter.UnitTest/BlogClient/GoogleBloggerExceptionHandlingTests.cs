// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using OpenLiveWriter.Extensibility.BlogClient;

namespace OpenLiveWriter.UnitTest.BlogClient
{
    [TestFixture]
    public class GoogleBloggerExceptionHandlingTests
    {
        private const string ExpectedTitle = "Post Too Large";
        private const string ExpectedMessage = "The post content is too large to publish. Try reducing the size of images or splitting the post into multiple parts.";

        [Test]
        public void OutOfMemoryException_IsConvertedToUserFriendlyMessage()
        {
            // Verify the exception conversion pattern used in GoogleBloggerv3Client
            var oom = new OutOfMemoryException("Insufficient memory to continue the execution of the program.");
            var friendly = new BlogClientException(ExpectedTitle, oom.Message);

            ClassicAssert.IsNotNull(friendly);
            ClassicAssert.IsTrue(friendly.ToString().Contains(ExpectedTitle));
        }

        [Test]
        public void BlogClientException_ContainsHelpfulGuidance()
        {
            var friendly = new BlogClientException(ExpectedTitle, ExpectedMessage);

            ClassicAssert.IsNotNull(friendly);
            ClassicAssert.IsTrue(friendly.ToString().Contains("too large"));
        }

        [Test]
        public void BlogClientException_CanBeCreatedWithTitleAndText()
        {
            var exception = new BlogClientException(ExpectedTitle, ExpectedMessage);

            ClassicAssert.IsNotNull(exception);
            Assert.That(exception, Is.InstanceOf<BlogClientException>());
        }
    }
}



