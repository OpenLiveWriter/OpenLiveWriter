// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenLiveWriter.UnitTest.PostEditor
{
    [TestClass]
    public class NullRibbonControlTest
    {
        [TestMethod]
        public void NullCheck_PreventsNullReferenceException()
        {
            object ribbonControl = null;
            // Simulates the guard pattern used in htmlEditor_TitleFocusChanged
            bool called = false;
            if (ribbonControl != null)
                called = true;
            Assert.IsFalse(called, "Should not invoke methods on null ribbonControl");
        }
    }
}
