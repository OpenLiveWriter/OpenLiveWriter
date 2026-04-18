// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.UnitTest.Mshtml
{
    [TestClass]
    public class ZoomHelperTests
    {
        [TestMethod]
        public void GetNextZoomLevel_ZoomIn_FromDefault_Returns110()
        {
            int result = ZoomHelper.GetNextZoomLevel(100, zoomIn: true);
            Assert.AreEqual(110, result);
        }

        [TestMethod]
        public void GetNextZoomLevel_ZoomOut_FromDefault_Returns90()
        {
            int result = ZoomHelper.GetNextZoomLevel(100, zoomIn: false);
            Assert.AreEqual(90, result);
        }

        [TestMethod]
        public void GetNextZoomLevel_ZoomIn_FromMinimum_Returns33()
        {
            int result = ZoomHelper.GetNextZoomLevel(25, zoomIn: true);
            Assert.AreEqual(33, result);
        }

        [TestMethod]
        public void GetNextZoomLevel_ZoomOut_FromMaximum_Returns400()
        {
            int result = ZoomHelper.GetNextZoomLevel(500, zoomIn: false);
            Assert.AreEqual(400, result);
        }

        [TestMethod]
        public void GetNextZoomLevel_ZoomIn_AtMaximum_StaysAtMaximum()
        {
            int result = ZoomHelper.GetNextZoomLevel(500, zoomIn: true);
            Assert.AreEqual(500, result);
        }

        [TestMethod]
        public void GetNextZoomLevel_ZoomOut_AtMinimum_StaysAtMinimum()
        {
            int result = ZoomHelper.GetNextZoomLevel(25, zoomIn: false);
            Assert.AreEqual(25, result);
        }

        [TestMethod]
        public void GetNextZoomLevel_ZoomIn_FromNonStandardValue_ReturnsNextStep()
        {
            // Current zoom is 115 (between 110 and 125), next step up should be 125
            int result = ZoomHelper.GetNextZoomLevel(115, zoomIn: true);
            Assert.AreEqual(125, result);
        }

        [TestMethod]
        public void GetNextZoomLevel_ZoomOut_FromNonStandardValue_ReturnsNextStep()
        {
            // Current zoom is 115 (between 110 and 125), next step down should be 110
            int result = ZoomHelper.GetNextZoomLevel(115, zoomIn: false);
            Assert.AreEqual(110, result);
        }

        [TestMethod]
        public void GetNextZoomLevel_ZoomIn_From125_Returns150()
        {
            int result = ZoomHelper.GetNextZoomLevel(125, zoomIn: true);
            Assert.AreEqual(150, result);
        }

        [TestMethod]
        public void GetNextZoomLevel_ZoomOut_From75_Returns67()
        {
            int result = ZoomHelper.GetNextZoomLevel(75, zoomIn: false);
            Assert.AreEqual(67, result);
        }

        [TestMethod]
        public void ZoomLevels_ContainsDefaultZoom()
        {
            bool contains100 = false;
            foreach (int level in ZoomHelper.ZoomLevels)
            {
                if (level == 100)
                {
                    contains100 = true;
                    break;
                }
            }
            Assert.IsTrue(contains100, "ZoomLevels should contain the default 100% level.");
        }

        [TestMethod]
        public void ZoomLevels_AreInAscendingOrder()
        {
            for (int i = 1; i < ZoomHelper.ZoomLevels.Length; i++)
            {
                Assert.IsTrue(ZoomHelper.ZoomLevels[i] > ZoomHelper.ZoomLevels[i - 1],
                    "ZoomLevels should be in ascending order.");
            }
        }
    }
}
