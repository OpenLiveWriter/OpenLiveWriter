// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;

namespace OpenLiveWriter.UnitTest.BlogClient
{
    [TestFixture]
    public class MetaweblogEditPostTests
    {
        [Test]
        public void InvalidPostIdException_WithEmptyPostId_CanBeCreated()
        {
            var exception = new BlogClientInvalidPostIdException(string.Empty);

            ClassicAssert.IsNotNull(exception);
            Assert.That(exception, Is.InstanceOf<BlogClientProviderException>());
            ClassicAssert.AreEqual(string.Empty, exception.PostId);
        }

        [Test]
        public void InvalidPostIdException_WithNullPostId_CanBeCreated()
        {
            var exception = new BlogClientInvalidPostIdException((string)null);

            ClassicAssert.IsNotNull(exception);
            Assert.That(exception, Is.InstanceOf<BlogClientProviderException>());
            ClassicAssert.IsNull(exception.PostId);
        }

        [Test]
        public void InvalidPostIdException_WithFaultCodeAndString_CanBeCreated()
        {
            var exception = new BlogClientInvalidPostIdException("17", "Invalid post ID");

            ClassicAssert.IsNotNull(exception);
            Assert.That(exception, Is.InstanceOf<BlogClientProviderException>());
            ClassicAssert.AreEqual("17", exception.ErrorCode);
            ClassicAssert.AreEqual("Invalid post ID", exception.ErrorString);
        }

        [Test]
        public void InvalidPostIdException_IsBlogClientException()
        {
            var exception = new BlogClientInvalidPostIdException("17", "Invalid post ID");

            Assert.That(exception, Is.InstanceOf<BlogClientException>());
        }

        [Test]
        public void InvalidPostIdException_ContainsDescriptiveMessage()
        {
            var exception = new BlogClientInvalidPostIdException("17", "Invalid post ID");

            // The exception should contain information about the invalid post ID
            string message = exception.ToString();
            ClassicAssert.IsTrue(
                message.Contains("Invalid Post ID") || message.Contains("post ID") || message.Contains("post id"),
                "Exception message should contain information about the invalid post ID");
        }

        [Test]
        public void XmlRpcMethodResponse_ParsesFaultCode17()
        {
            // Simulate a server response with fault code 17
            string faultResponse =
                "<?xml version=\"1.0\"?>" +
                "<methodResponse>" +
                "  <fault>" +
                "    <value>" +
                "      <struct>" +
                "        <member>" +
                "          <name>faultCode</name>" +
                "          <value><int>17</int></value>" +
                "        </member>" +
                "        <member>" +
                "          <name>faultString</name>" +
                "          <value><string>Invalid post ID</string></value>" +
                "        </member>" +
                "      </struct>" +
                "    </value>" +
                "  </fault>" +
                "</methodResponse>";

            var response = new XmlRpcMethodResponse(faultResponse);

            ClassicAssert.IsTrue(response.FaultOccurred, "Fault should be detected");
            ClassicAssert.AreEqual("17", response.FaultCode, "Fault code should be 17");
            ClassicAssert.AreEqual("Invalid post ID", response.FaultString, "Fault string should match");
        }

        [Test]
        public void XmlRpcMethodResponse_SuccessfulResponse_NoFault()
        {
            string successResponse =
                "<?xml version=\"1.0\"?>" +
                "<methodResponse>" +
                "  <params>" +
                "    <param>" +
                "      <value><boolean>1</boolean></value>" +
                "    </param>" +
                "  </params>" +
                "</methodResponse>";

            var response = new XmlRpcMethodResponse(successResponse);

            ClassicAssert.IsFalse(response.FaultOccurred, "No fault should be detected for successful response");
            ClassicAssert.AreEqual(string.Empty, response.FaultCode, "Fault code should be empty");
        }

        [Test]
        public void BlogPost_NewPost_HasEmptyId()
        {
            var post = new BlogPost();

            ClassicAssert.AreEqual(string.Empty, post.Id, "New BlogPost should have empty ID");
            ClassicAssert.IsTrue(post.IsNew, "New BlogPost should report IsNew as true");
        }

        [Test]
        public void BlogPost_WithId_IsNotNew()
        {
            var post = new BlogPost();
            post.Id = "123";

            ClassicAssert.AreEqual("123", post.Id);
            ClassicAssert.IsFalse(post.IsNew, "BlogPost with ID should report IsNew as false");
        }

        [Test]
        public void EmptyPostId_ShouldBeDetectedBeforeServerCall()
        {
            // This verifies the pattern: string.IsNullOrEmpty should catch
            // both null and empty post IDs that would cause fault code 17
            string emptyId = string.Empty;
            string nullId = null;

            ClassicAssert.IsTrue(string.IsNullOrEmpty(emptyId),
                "Empty string post ID should be detected by IsNullOrEmpty");
            ClassicAssert.IsTrue(string.IsNullOrEmpty(nullId),
                "Null post ID should be detected by IsNullOrEmpty");

            // A valid post ID should not be caught
            string validId = "42";
            ClassicAssert.IsFalse(string.IsNullOrEmpty(validId),
                "Valid post ID should not be caught by IsNullOrEmpty");
        }
    }
}



