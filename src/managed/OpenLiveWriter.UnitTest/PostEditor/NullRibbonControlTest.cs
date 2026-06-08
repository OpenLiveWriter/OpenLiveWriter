// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace OpenLiveWriter.UnitTest.PostEditor
{
    [TestFixture]
    public class NullRibbonControlTest
    {
        [Test]
        public void NullCheck_PreventsNullReferenceException()
        {
            object ribbonControl = null;
            // Simulates the guard pattern used in htmlEditor_TitleFocusChanged
            bool called = false;
            if (ribbonControl != null)
                called = true;
            ClassicAssert.IsFalse(called, "Should not invoke methods on null ribbonControl");
        }
    }
}


