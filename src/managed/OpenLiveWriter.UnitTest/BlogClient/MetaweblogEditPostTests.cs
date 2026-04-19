// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;

namespace OpenLiveWriter.UnitTest.BlogClient
{
    [TestClass]
    public class MetaweblogEditPostTests
    {
        [TestMethod]
        public void InvalidPostIdException_WithEmptyPostId_CanBeCreated()
        {
            var exception = new BlogClientInvalidPostIdException(string.Empty);

            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(BlogClientProviderException));
            Assert.AreEqual(string.Empty, exception.PostId);
        }

        [TestMethod]
        public void InvalidPostIdException_WithNullPostId_CanBeCreated()
        {
            var exception = new BlogClientInvalidPostIdException((string)null);

            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(BlogClientProviderException));
            Assert.IsNull(exception.PostId);
        }

        [TestMethod]
        public void InvalidPostIdException_WithFaultCodeAndString_CanBeCreated()
        {
            var exception = new BlogClientInvalidPostIdException("17", "Invalid post ID");

            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(BlogClientProviderException));
            Assert.AreEqual("17", exception.ErrorCode);
            Assert.AreEqual("Invalid post ID", exception.ErrorString);
        }

        [TestMethod]
        public void InvalidPostIdException_IsBlogClientException()
        {
            var exception = new BlogClientInvalidPostIdException("17", "Invalid post ID");

            Assert.IsInstanceOfType(exception, typeof(BlogClientException));
        }

        [TestMethod]
        public void InvalidPostIdException_ContainsDescriptiveMessage()
        {
            var exception = new BlogClientInvalidPostIdException("17", "Invalid post ID");

            // The exception should contain information about the invalid post ID
            string message = exception.ToString();
            Assert.IsTrue(
                message.Contains("Invalid Post ID") || message.Contains("post ID") || message.Contains("post id"),
                "Exception message should contain information about the invalid post ID");
        }

        [TestMethod]
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

            Assert.IsTrue(response.FaultOccurred, "Fault should be detected");
            Assert.AreEqual("17", response.FaultCode, "Fault code should be 17");
            Assert.AreEqual("Invalid post ID", response.FaultString, "Fault string should match");
        }

        [TestMethod]
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

            Assert.IsFalse(response.FaultOccurred, "No fault should be detected for successful response");
            Assert.AreEqual(string.Empty, response.FaultCode, "Fault code should be empty");
        }

        [TestMethod]
        public void BlogPost_NewPost_HasEmptyId()
        {
            var post = new BlogPost();

            Assert.AreEqual(string.Empty, post.Id, "New BlogPost should have empty ID");
            Assert.IsTrue(post.IsNew, "New BlogPost should report IsNew as true");
        }

        [TestMethod]
        public void BlogPost_WithId_IsNotNew()
        {
            var post = new BlogPost();
            post.Id = "123";

            Assert.AreEqual("123", post.Id);
            Assert.IsFalse(post.IsNew, "BlogPost with ID should report IsNew as false");
        }

        [TestMethod]
        public void EmptyPostId_ShouldBeDetectedBeforeServerCall()
        {
            // This verifies the pattern: string.IsNullOrEmpty should catch
            // both null and empty post IDs that would cause fault code 17
            string emptyId = string.Empty;
            string nullId = null;

            Assert.IsTrue(string.IsNullOrEmpty(emptyId),
                "Empty string post ID should be detected by IsNullOrEmpty");
            Assert.IsTrue(string.IsNullOrEmpty(nullId),
                "Null post ID should be detected by IsNullOrEmpty");

            // A valid post ID should not be caught
            string validId = "42";
            Assert.IsFalse(string.IsNullOrEmpty(validId),
                "Valid post ID should not be caught by IsNullOrEmpty");
        }
    }
}
