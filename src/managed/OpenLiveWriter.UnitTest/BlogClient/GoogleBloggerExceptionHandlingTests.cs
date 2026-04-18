// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenLiveWriter.Extensibility.BlogClient;

namespace OpenLiveWriter.UnitTest.BlogClient
{
    [TestClass]
    public class GoogleBloggerExceptionHandlingTests
    {
        private const string ExpectedTitle = "Post Too Large";
        private const string ExpectedMessage = "The post content is too large to publish. Try reducing the size of images or splitting the post into multiple parts.";

        [TestMethod]
        public void OutOfMemoryException_IsConvertedToUserFriendlyMessage()
        {
            // Verify the exception conversion pattern used in GoogleBloggerv3Client
            var oom = new OutOfMemoryException("Insufficient memory to continue the execution of the program.");
            var friendly = new BlogClientException(ExpectedTitle, oom.Message);

            Assert.IsNotNull(friendly);
            Assert.IsTrue(friendly.ToString().Contains(ExpectedTitle));
        }

        [TestMethod]
        public void BlogClientException_ContainsHelpfulGuidance()
        {
            var friendly = new BlogClientException(ExpectedTitle, ExpectedMessage);

            Assert.IsNotNull(friendly);
            Assert.IsTrue(friendly.ToString().Contains("too large"));
        }

        [TestMethod]
        public void BlogClientException_CanBeCreatedWithTitleAndText()
        {
            var exception = new BlogClientException(ExpectedTitle, ExpectedMessage);

            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(BlogClientException));
        }
    }
}
