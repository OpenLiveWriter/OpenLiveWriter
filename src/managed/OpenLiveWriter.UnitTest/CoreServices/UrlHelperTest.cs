// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.UnitTest.CoreServices
{
    [TestFixture]
    public class UrlHelperTest
    {

        private static readonly string[] _urlsToCheck = new string[]
                {

                   @"c:\temp\232323232%7Ffp%3A%3C%3Dot%3E2378%3D664%3D88%3B%3DXROQDF%3E2323%3A77%3B9%3B537ot1lsi.gif",
                   @"c:\temp\foo.gif",
                   @"c:\",
                   @"\\unknown\test\foo.gif",
                   @"c:\program files\windows live\writer\OpenLiveWriter.exe",
                   @"c:\program files\",
                   @"c:\こんにちは.txt",
                   @"c:\こんにちは\+2.txt",
                   @"c:\temp\232323232%7Ffp%3A%3C%3Dot%3E2378%3D664%3D88%3B%3DXROQDF%3E2323%3A77%3B9%3B537ot1lは.gif",
                   // @"foo\bar\foo.txt",

                   // @"c:\temp\..\foo.txt",  // This path gets turned into the canonical path
        };

        [Test]
        public void TestCreateUrlFromPath()
        {
            for (int i = 0; i < _urlsToCheck.Length; i++)
            {
                try
                {
                    string urlToCheck = _urlsToCheck[i];
                    string result = UrlHelper.CreateUrlFromPath(urlToCheck);
                    string uriLocalPath = new Uri(result).LocalPath;
                    //Assert.AreEqual(uriLocalPath, urlToCheck);

                    string simpleUriLocalPath = new Uri(UrlHelper.SafeToAbsoluteUri(new Uri(urlToCheck))).LocalPath;
                    Assert.AreEqual(simpleUriLocalPath, urlToCheck);
                    Assert.AreEqual(simpleUriLocalPath, uriLocalPath);
                }
                catch(Exception ex)
                {
                    Assert.Fail(ex.ToString());
                }
            }
        }

        [Test]
        public void TestCreateUrlFromUri()
        {

            for (int i = 0; i < _urlsToCheck.Length; i++)
            {

            }
        }

        [Test]
        public void TestLocalUriIECompat()
        {
            VerifyPathToUri(@"c:\こんにちは.txt", @"file:///c:/こんにちは.txt");
            VerifyPathToUri(@"c:\こんにちは#.txt", @"file:///c:/こんにちは%23.txt");
            VerifyPathToUri(UrlHelper.SafeToAbsoluteUri(new Uri(@"c:\こんにちは#.txt")),
                @"file:///c:/こんにちは%23.txt");
        }

        private void VerifyPathToUri(string path, string uri)
        {
            Assert.AreEqual(
                UrlHelper.SafeToAbsoluteUri(new Uri(path)),
                uri);
        }
    }
}
