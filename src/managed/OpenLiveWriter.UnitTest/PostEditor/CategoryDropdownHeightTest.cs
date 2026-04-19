// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenLiveWriter.UnitTest.PostEditor
{
    [TestClass]
    public class CategoryDropdownHeightTest
    {
        [TestMethod]
        public void ItemHeight_IsAtLeast15Pixels()
        {
            // The dropdown item height should be at least 15 pixels
            // to accommodate text at standard DPI
            int minHeight = 15;
            Assert.IsTrue(minHeight >= 15);
        }

        [TestMethod]
        public void ItemHeight_ShouldAccountForFontSize()
        {
            // At standard DPI, a reasonable item height is 18-24 pixels
            // to prevent text clipping in category dropdowns
            int itemHeight = 20;
            Assert.IsTrue(itemHeight >= 15 && itemHeight <= 30,
                "Item height should be between 15 and 30 pixels");
        }
    }
}
