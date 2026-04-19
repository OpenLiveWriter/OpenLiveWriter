// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenLiveWriter.PostEditor.PostHtmlEditing;

namespace OpenLiveWriter.UnitTest.PostEditor
{
    /// <summary>
    /// Tests for the EnsureMinimumImageSize logic in ImageInsertionManager,
    /// covering issue #143: pasting a clipboard image (PrintScreen) should
    /// not produce a 1x1 pixel box.
    /// </summary>
    [TestClass]
    public class ImageMinimumSizeTests
    {
        [TestMethod]
        public void EnsureMinimumImageSize_LargeSize_ReturnsUnchanged()
        {
            Size input = new Size(800, 600);
            Size result = ImageInsertionManager.EnsureMinimumImageSize(input, null);
            Assert.AreEqual(input, result);
        }

        [TestMethod]
        public void EnsureMinimumImageSize_AtMinimumThreshold_ReturnsUnchanged()
        {
            int min = ImageInsertionManager.MINIMUM_IMAGE_DIMENSION;
            Size input = new Size(min, min);
            Size result = ImageInsertionManager.EnsureMinimumImageSize(input, null);
            Assert.AreEqual(input, result);
        }

        [TestMethod]
        public void EnsureMinimumImageSize_OneByOne_NoFile_ClampsToMinimum()
        {
            Size input = new Size(1, 1);
            Size result = ImageInsertionManager.EnsureMinimumImageSize(input, null);
            Assert.AreEqual(ImageInsertionManager.MINIMUM_IMAGE_DIMENSION, result.Width);
            Assert.AreEqual(ImageInsertionManager.MINIMUM_IMAGE_DIMENSION, result.Height);
        }

        [TestMethod]
        public void EnsureMinimumImageSize_OneByOne_NonExistentFile_ClampsToMinimum()
        {
            Size input = new Size(1, 1);
            string fakePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".png");
            Size result = ImageInsertionManager.EnsureMinimumImageSize(input, fakePath);
            Assert.AreEqual(ImageInsertionManager.MINIMUM_IMAGE_DIMENSION, result.Width);
            Assert.AreEqual(ImageInsertionManager.MINIMUM_IMAGE_DIMENSION, result.Height);
        }

        [TestMethod]
        public void EnsureMinimumImageSize_OneByOne_ValidImageFile_ReturnsActualSize()
        {
            string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".png");
            try
            {
                // Create a 200x150 test image
                using (Bitmap bmp = new Bitmap(200, 150))
                {
                    bmp.Save(tempFile, ImageFormat.Png);
                }

                Size input = new Size(1, 1);
                Size result = ImageInsertionManager.EnsureMinimumImageSize(input, tempFile);
                Assert.AreEqual(200, result.Width);
                Assert.AreEqual(150, result.Height);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void EnsureMinimumImageSize_WidthBelowMinimum_ClampsWidth()
        {
            Size input = new Size(5, 100);
            Size result = ImageInsertionManager.EnsureMinimumImageSize(input, null);
            Assert.AreEqual(ImageInsertionManager.MINIMUM_IMAGE_DIMENSION, result.Width);
            Assert.AreEqual(100, result.Height);
        }

        [TestMethod]
        public void EnsureMinimumImageSize_HeightBelowMinimum_ClampsHeight()
        {
            Size input = new Size(100, 3);
            Size result = ImageInsertionManager.EnsureMinimumImageSize(input, null);
            Assert.AreEqual(100, result.Width);
            Assert.AreEqual(ImageInsertionManager.MINIMUM_IMAGE_DIMENSION, result.Height);
        }

        [TestMethod]
        public void MinimumImageDimension_IsReasonableValue()
        {
            // The minimum should be large enough to be visible but not overly large
            Assert.IsTrue(ImageInsertionManager.MINIMUM_IMAGE_DIMENSION >= 16,
                "Minimum dimension should be at least 16 pixels");
            Assert.IsTrue(ImageInsertionManager.MINIMUM_IMAGE_DIMENSION <= 200,
                "Minimum dimension should not be excessively large");
        }
    }
}
