// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.UnitTest.CoreServices
{
    [TestClass]
    public class XmlRpcResponseParsingTests
    {
        private const string ValidXmlRpcResponse =
            "<?xml version=\"1.0\"?>" +
            "<methodResponse>" +
            "<params><param><value><string>Hello</string></value></param></params>" +
            "</methodResponse>";

        [TestMethod]
        public void ValidXmlRpcResponse_ParsesSuccessfully()
        {
            var response = new XmlRpcMethodResponse(ValidXmlRpcResponse);
            Assert.IsFalse(response.FaultOccurred);
            Assert.IsNotNull(response.Response);
        }

        [TestMethod]
        [ExpectedException(typeof(XmlRpcClientInvalidResponseException))]
        public void MalformedXml_ThrowsInvalidResponseException()
        {
            new XmlRpcMethodResponse("<methodResponse><params><param><value>unclosed");
        }

        [TestMethod]
        [ExpectedException(typeof(XmlRpcClientInvalidResponseException))]
        public void NullResponse_ThrowsInvalidResponseException()
        {
            new XmlRpcMethodResponse((string)null);
        }

        [TestMethod]
        [ExpectedException(typeof(XmlRpcClientInvalidResponseException))]
        public void EmptyResponse_ThrowsInvalidResponseException()
        {
            new XmlRpcMethodResponse(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(XmlRpcClientInvalidResponseException))]
        public void WhitespaceOnlyResponse_ThrowsInvalidResponseException()
        {
            new XmlRpcMethodResponse("   \t\r\n   ");
        }
    }
}
