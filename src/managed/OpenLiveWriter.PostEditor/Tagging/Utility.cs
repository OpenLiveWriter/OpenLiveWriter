// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;

namespace OpenLiveWriter.PostEditor.Tagging
{
    /// <summary>
    /// Utility helper class.
    /// </summary>
    public sealed class Utility
    {
        //	Initializes a new instance of the Utility class.
        private Utility()
        {
        }

        /// <summary>
        /// Returns the bounds that should be used to center something
        /// of size "size" relative to the area represented by "rectangle".
        /// </summary>
        /// <param name="size">The size of the thing to be centered.</param>
        /// <param name="rectangle">The area within which the thing should be centered.</param>
        /// <returns>The bounds that should be applied to the thing being centered.</returns>
        public static Rectangle CenterInRectangle(Size size, Rectangle rectangle)
        {
            Point center = Center(rectangle);
            return CenterAroundPoint(size, center);
        }

        /// <summary>
        /// Returns the bounds that should be used to center something
        /// of size "size" relative to a point.
        /// </summary>
        /// <param name="size">The size of the thing to be centered.</param>
        /// <param name="point">The point around which the thing should be centered.</param>
        /// <returns>The bounds that should be applied to the thing being centered.</returns>
        public static Rectangle CenterAroundPoint(Size size, Point point)
        {
            int x = point.X - (size.Width / 2);
            int y = point.Y - (size.Height / 2);
            return new Rectangle(new Point(x, y), size);
        }

        /// <summary>
        /// Returns the center point of a rectangle.
        /// </summary>
        public static Point Center(Rectangle bounds)
        {
            return new Point((bounds.X * 2 + bounds.Width) / 2, (bounds.Y * 2 + bounds.Height) / 2);
        }

        /// <summary>
        /// Simple helper to center something in another thing with a minimum value of zero.
        /// </summary>
        /// <param name="valueToCenter">Value of the thing to center.</param>
        /// <param name="valueToCenterIn">Value of the thing to center in.</param>
        /// <returns>A zero-based value representing the offset required to center valueToCenter inside valueToCenter.</returns>
        public static int CenterMinZero(int valueToCenter, int valueToCenterIn)
        {
            return Math.Max(0, (valueToCenterIn - valueToCenter) / 2);
        }

        /// <summary>
        /// Simple helper to center something in another thing with the possibility of a negative
        /// number being returned.
        /// </summary>
        /// <param name="valueToCenter">Value of the thing to center.</param>
        /// <param name="valueToCenterIn">Value of the thing to center in.</param>
        /// <returns>A zero-based value representing the offset required to center valueToCenter inside valueToCenter.</returns>
        public static int Center(int valueToCenter, int valueToCenterIn)
        {
            return (valueToCenterIn - valueToCenter) / 2;
        }

        /// <summary>
        /// Gets a Size structure that contains the union of an array of Size structures.
        /// </summary>
        /// <param name="sizes">Array of sizes to compute the union of.</param>
        /// <returns>Size which represents the union of the </returns>
        public static Size Union(Size[] sizes)
        {
            //	Enumerate the sizes and compute the union of them.
            Rectangle rectangle = Rectangle.Empty;
            foreach (Size size in sizes)
                rectangle = Rectangle.Union(rectangle, new Rectangle(Point.Empty, size));

            //	Return the union of the sizes.
            return rectangle.Size;
        }

        public static Size GetScaledMaxSize(Size initialSize, Size maxSize)
        {
            return GetScaledMaxSize(initialSize, maxSize.Width, maxSize.Height);
        }

        /// <summary>
        /// Increases an initial size without exceeding a maximum height and width while (closely) preserving the current aspect ratio.
        /// </summary>
        /// <param name="initialSize"></param>
        /// <param name="maxWidth"></param>
        /// <param name="maxHeight"></param>
        /// <returns></returns>
        public static Size GetScaledMaxSize(Size initialSize, int maxWidth, int maxHeight)
        {
            // The dimensions of the image from which the thumbnail is needed
            double width = initialSize.Width;
            double height = initialSize.Height;
            double imageRatio = width / height;

            return GetScaledMaxSize(imageRatio, maxWidth, maxHeight);
        }

        public static Size GetScaledMaxSize(double imageRatio, int maxWidth, int maxHeight)
        {
            // The width/height ratio of the maximum thumbnail dimensions
            double maxRatio = (double)maxWidth / (double)maxHeight;

            // The dimensions of the thumbnail that we'll request
            double requestedWidth;
            double requestedHeight;

            if (imageRatio >= maxRatio)
            {
                // the image's width is the determinant in scaling, scale based upon that
                requestedWidth = maxWidth;
                requestedHeight = requestedWidth / imageRatio;
            }
            else
            {
                // the image's height is the determinant in scaling, scale based upon that
                requestedHeight = maxHeight;
                requestedWidth = requestedHeight * imageRatio;
            }

            return new Size((int)requestedWidth, (int)requestedHeight);
        }

        /// <summary>
        /// Helper to determine whether a string is "empty" (null or zero-length).
        /// </summary>
        /// <param name="value">String to check.</param>
        /// <returns>True if the string is null or zero-length; false if not.</returns>
        public static bool IsEmpty(string value)
        {
            return value == null || value.Length == 0;
        }
    }
}
