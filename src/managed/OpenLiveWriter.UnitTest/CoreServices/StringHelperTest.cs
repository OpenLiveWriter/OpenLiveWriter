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
    public class StringHelperTest
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void CommonPrefix()
        {
            string result = StringHelper.CommonPrefix("foobar", "foobat");
            if (result != "fooba")
                throw new Exception("CommonPrefix failed");
        }

        [Test]
        public void GetHashCodeStable()
        {
            int result = StringHelper.GetHashCodeStable("abc");
            if (result != 536991770)
                throw new Exception("GetHashCodeStable failed");

            result = StringHelper.GetHashCodeStable("acb");
            if (result != 2103010175)
                throw new Exception("GetHashCodeStable failed");

            result = StringHelper.GetHashCodeStable("123456");
            if (result != 1849190365)
                throw new Exception("GetHashCodeStable failed");

            result = StringHelper.GetHashCodeStable("The quick brown fox jumped over the lazy dog");
            if (result != 16963814)
                throw new Exception("GetHashCodeStable failed");
        }

        [TearDown]
        public void TearDown()
        {

        }
    }
}
