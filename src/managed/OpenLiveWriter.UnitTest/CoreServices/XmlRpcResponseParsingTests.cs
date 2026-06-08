// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.UnitTest.CoreServices
{
    [TestFixture]
    public class XmlRpcResponseParsingTests
    {
        private const string ValidXmlRpcResponse =
            "<?xml version=\"1.0\"?>" +
            "<methodResponse>" +
            "<params><param><value><string>Hello</string></value></param></params>" +
            "</methodResponse>";

        [Test]
        public void ValidXmlRpcResponse_ParsesSuccessfully()
        {
            var response = new XmlRpcMethodResponse(ValidXmlRpcResponse);
            ClassicAssert.IsFalse(response.FaultOccurred);
            ClassicAssert.IsNotNull(response.Response);
        }

        [Test]
        public void MalformedXml_ThrowsInvalidResponseException()
        {
            Assert.Throws<XmlRpcClientInvalidResponseException>(() =>
                new XmlRpcMethodResponse("<methodResponse><params><param><value>unclosed"));
        }

        [Test]
        public void NullResponse_ThrowsInvalidResponseException()
        {
            Assert.Throws<XmlRpcClientInvalidResponseException>(() =>
                new XmlRpcMethodResponse((string)null));
        }

        [Test]
        public void EmptyResponse_ThrowsInvalidResponseException()
        {
            Assert.Throws<XmlRpcClientInvalidResponseException>(() =>
                new XmlRpcMethodResponse(string.Empty));
        }

        [Test]
        public void WhitespaceOnlyResponse_ThrowsInvalidResponseException()
        {
            Assert.Throws<XmlRpcClientInvalidResponseException>(() =>
                new XmlRpcMethodResponse("   \t\r\n   "));
        }
    }
}

