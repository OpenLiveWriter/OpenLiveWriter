// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using OpenLiveWriter.PostEditor.PostHtmlEditing;

namespace OpenLiveWriter.UnitTest.PostEditor
{
    /// <summary>
    /// Tests for the EnsureMinimumImageSize logic in ImageInsertionManager,
    /// covering issue #143: pasting a clipboard image (PrintScreen) should
    /// not produce a 1x1 pixel box.
    /// </summary>
    [TestFixture]
    public class ImageMinimumSizeTests
    {
        [Test]
        public void EnsureMinimumImageSize_LargeSize_ReturnsUnchanged()
        {
            Size input = new Size(800, 600);
            Size result = ImageInsertionManager.EnsureMinimumImageSize(input, null);
            ClassicAssert.AreEqual(input, result);
        }

        [Test]
        public void EnsureMinimumImageSize_AtMinimumThreshold_ReturnsUnchanged()
        {
            int min = ImageInsertionManager.MINIMUM_IMAGE_DIMENSION;
            Size input = new Size(min, min);
            Size result = ImageInsertionManager.EnsureMinimumImageSize(input, null);
            ClassicAssert.AreEqual(input, result);
        }

        [Test]
        public void EnsureMinimumImageSize_OneByOne_NoFile_ClampsToMinimum()
        {
            Size input = new Size(1, 1);
            Size result = ImageInsertionManager.EnsureMinimumImageSize(input, null);
            ClassicAssert.AreEqual(ImageInsertionManager.MINIMUM_IMAGE_DIMENSION, result.Width);
            ClassicAssert.AreEqual(ImageInsertionManager.MINIMUM_IMAGE_DIMENSION, result.Height);
        }

        [Test]
        public void EnsureMinimumImageSize_OneByOne_NonExistentFile_ClampsToMinimum()
        {
            Size input = new Size(1, 1);
            string fakePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".png");
            Size result = ImageInsertionManager.EnsureMinimumImageSize(input, fakePath);
            ClassicAssert.AreEqual(ImageInsertionManager.MINIMUM_IMAGE_DIMENSION, result.Width);
            ClassicAssert.AreEqual(ImageInsertionManager.MINIMUM_IMAGE_DIMENSION, result.Height);
        }

        [Test]
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
                ClassicAssert.AreEqual(200, result.Width);
                ClassicAssert.AreEqual(150, result.Height);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Test]
        public void EnsureMinimumImageSize_WidthBelowMinimum_ClampsWidth()
        {
            Size input = new Size(5, 100);
            Size result = ImageInsertionManager.EnsureMinimumImageSize(input, null);
            ClassicAssert.AreEqual(ImageInsertionManager.MINIMUM_IMAGE_DIMENSION, result.Width);
            ClassicAssert.AreEqual(100, result.Height);
        }

        [Test]
        public void EnsureMinimumImageSize_HeightBelowMinimum_ClampsHeight()
        {
            Size input = new Size(100, 3);
            Size result = ImageInsertionManager.EnsureMinimumImageSize(input, null);
            ClassicAssert.AreEqual(100, result.Width);
            ClassicAssert.AreEqual(ImageInsertionManager.MINIMUM_IMAGE_DIMENSION, result.Height);
        }

        [Test]
        public void MinimumImageDimension_IsReasonableValue()
        {
            // The minimum should be large enough to be visible but not overly large
            ClassicAssert.IsTrue(ImageInsertionManager.MINIMUM_IMAGE_DIMENSION >= 16,
                "Minimum dimension should be at least 16 pixels");
            ClassicAssert.IsTrue(ImageInsertionManager.MINIMUM_IMAGE_DIMENSION <= 200,
                "Minimum dimension should not be excessively large");
        }
    }
}


