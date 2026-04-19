// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenLiveWriter.UnitTest.Controls
{
    [TestClass]
    public class ImageCropMakeGrayTest
    {
        /// <summary>
        /// Verifies that the ColorMatrix grayscale approach produces an image
        /// with the same dimensions as the input.
        /// </summary>
        [TestMethod]
        public void MakeGray_PreservesDimensions()
        {
            using (Bitmap input = new Bitmap(200, 100, PixelFormat.Format32bppArgb))
            {
                using (Bitmap result = MakeGray(input))
                {
                    Assert.AreEqual(input.Width, result.Width);
                    Assert.AreEqual(input.Height, result.Height);
                }
            }
        }

        /// <summary>
        /// Verifies that the ColorMatrix grayscale conversion produces
        /// pixels where the color channels are equal (i.e. a gray pixel).
        /// </summary>
        [TestMethod]
        public void MakeGray_ProducesGrayscalePixels()
        {
            using (Bitmap input = new Bitmap(10, 10, PixelFormat.Format32bppArgb))
            {
                // Fill with a known color
                using (Graphics g = Graphics.FromImage(input))
                {
                    g.Clear(Color.FromArgb(255, 200, 100, 50));
                }

                using (Bitmap result = MakeGray(input))
                {
                    Color pixel = result.GetPixel(5, 5);

                    // For a grayscale image the R, G, B channels should be equal
                    Assert.AreEqual(pixel.R, pixel.G,
                        "Red and Green channels should be equal for grayscale output.");
                    Assert.AreEqual(pixel.G, pixel.B,
                        "Green and Blue channels should be equal for grayscale output.");

                    // Expected luminance: 0.3*200 + 0.59*100 + 0.11*50 = 60+59+5.5 = 124.5
                    // Allow small rounding tolerance
                    Assert.IsTrue(pixel.R >= 120 && pixel.R <= 130,
                        string.Format("Expected grayscale value near 125, got {0}.", pixel.R));
                }
            }
        }

        /// <summary>
        /// Replicates the MakeGray logic from ImageCropControl using ColorMatrix
        /// so we can test it without needing to access the private method.
        /// </summary>
        private static Bitmap MakeGray(Bitmap orig)
        {
            Bitmap grayed = new Bitmap(orig.Width, orig.Height, orig.PixelFormat);
            using (Graphics g = Graphics.FromImage(grayed))
            {
                ColorMatrix colorMatrix = new ColorMatrix(new float[][] {
                    new float[] {0.3f, 0.3f, 0.3f, 0, 0},
                    new float[] {0.59f, 0.59f, 0.59f, 0, 0},
                    new float[] {0.11f, 0.11f, 0.11f, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                });
                using (ImageAttributes attrs = new ImageAttributes())
                {
                    attrs.SetColorMatrix(colorMatrix);
                    g.DrawImage(orig, new Rectangle(0, 0, orig.Width, orig.Height),
                        0, 0, orig.Width, orig.Height, GraphicsUnit.Pixel, attrs);
                }
            }
            return grayed;
        }
    }
}
