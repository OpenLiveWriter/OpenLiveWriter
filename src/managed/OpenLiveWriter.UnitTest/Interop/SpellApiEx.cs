// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using OpenLiveWriter.SpellChecker.NLG;

namespace OpenLiveWriter.UnitTest.Interop
{
    [TestFixture]
    public class SpellApiEx
    {
        [SetUp]
        public void SetUp()
        {
            SpellLibrary lib = new SpellLibrary(@"C:\writer\target\Debug\i386\Writer\Dictionaries\mssp7en.dll");
        }

        [Test]
        public void Test()
        {

        }
    }
}
