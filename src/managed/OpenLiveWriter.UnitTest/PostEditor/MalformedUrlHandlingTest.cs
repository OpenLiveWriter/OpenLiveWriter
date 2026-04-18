// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenLiveWriter.UnitTest.PostEditor
{
    [TestClass]
    public class MalformedUrlHandlingTest
    {
        [TestMethod]
        public void UriTryCreate_RejectsMalformedUrls()
        {
            Uri result;
            Assert.IsFalse(Uri.TryCreate("not a url", UriKind.Absolute, out result));
            Assert.IsFalse(Uri.TryCreate("://missing-scheme", UriKind.Absolute, out result));
            Assert.IsFalse(Uri.TryCreate("", UriKind.Absolute, out result));
        }

        [TestMethod]
        public void UriTryCreate_AcceptsValidFileUrls()
        {
            Uri result;
            Assert.IsTrue(Uri.TryCreate("file:///C:/Users/test/image.png", UriKind.Absolute, out result));
            Assert.IsNotNull(result);
            Assert.IsTrue(Uri.TryCreate("http://example.com/image.png", UriKind.Absolute, out result));
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void MalformedUrl_CanBeReturnedUnchanged()
        {
            string malformedUrl = "not a valid url at all";
            Uri uri;
            Assert.IsFalse(Uri.TryCreate(malformedUrl, UriKind.Absolute, out uri));
            Assert.IsNull(uri);
        }
    }
}
