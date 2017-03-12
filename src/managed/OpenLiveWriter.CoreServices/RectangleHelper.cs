// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Rectangle operation helper.
    /// </summary>
    public sealed class RectangleHelper
    {
        /// <summary>
        /// Initializes a new instance of the RectangleHelper class.
        /// </summary>
        private RectangleHelper()
        {
        }

        /// <summary>
        /// Creates a rectangle from two points.
        /// </summary>
        /// <param name="topLeft">Top left point of the rectangle.</param>
        /// <param name="bottomRight">Bottom right point of the rectangle.</param>
        /// <returns>New Rectangle structure.</returns>
        public static Rectangle Create(Point topLeft, Point bottomRight)
        {
            return new Rectangle(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
        }

        /// <summary>
        /// Adjusts the specified rectangle structure by the specified x and y amount and returns
        /// a new rectangle structure.
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Rectangle Adjust(Rectangle rectangle, int x, int y)
        {
            return new Rectangle(rectangle.X + x,
                                    rectangle.Y + y,
                                     Math.Max(0, rectangle.Width - x),
                                    Math.Max(0, rectangle.Height - y));
        }

        /// <summary>
        /// Returns the top half of the specified rectangle.
        /// </summary>
        /// <param name="rectangle">The rectangle to return the top half of.</param>
        /// <returns>A new rectangle representing the top half of the input rectangle.</returns>
        public static Rectangle TopHalf(Rectangle rectangle)
        {
            return new Rectangle(rectangle.X,
                                    rectangle.Y,
                                    rectangle.Width,
                                    rectangle.Height / 2);
        }

        /// <summary>
        /// Returns the bottom half of the specified rectangle.
        /// </summary>
        /// <param name="rectangle">The rectangle to return the bottom half of.</param>
        /// <returns>A new rectangle representing the bottom half of the input rectangle.</returns>
        public static Rectangle BottomHalf(Rectangle rectangle)
        {
            int height = rectangle.Height / 2;
            return new Rectangle(rectangle.X,
                                    rectangle.Y + height,
                                    rectangle.Width,
                                    rectangle.Height - height);
        }

        /// <summary>
        /// Pins a rectangle to the top-left of another rectangle.
        /// </summary>
        /// <param name="rectangleToPin">Rectangle to pin.</param>
        /// <param name="rectangleToPinTo">Rectangle to pin to.</param>
        /// <returns>Pinned rectangle.</returns>
        public static Rectangle PinTopLeft(Rectangle rectangleToPin, Rectangle rectangleToPinTo)
        {
            return new Rectangle(rectangleToPinTo.X,
                                    rectangleToPinTo.Y,
                                    rectangleToPin.Width,
                                    rectangleToPin.Height);
        }

        /// <summary>
        /// Pins a rectangle to the top-right of another rectangle.
        /// </summary>
        /// <param name="rectangleToPin">Rectangle to pin.</param>
        /// <param name="rectangleToPinTo">Rectangle to pin to.</param>
        /// <returns>Pinned rectangle.</returns>
        public static Rectangle PinTopRight(Rectangle rectangleToPin, Rectangle rectangleToPinTo)
        {
            return new Rectangle(rectangleToPinTo.Right - rectangleToPin.Width,
                                    rectangleToPinTo.Y,
                                    rectangleToPin.Width,
                                    rectangleToPin.Height);
        }

        /// <summary>
        /// Pins a rectangle to the bottom-right of another rectangle.
        /// </summary>
        /// <param name="rectangleToPin">Rectangle to pin.</param>
        /// <param name="rectangleToPinTo">Rectangle to pin to.</param>
        /// <returns>Pinned rectangle.</returns>
        public static Rectangle PinBottomRight(Rectangle rectangleToPin, Rectangle rectangleToPinTo)
        {
            return new Rectangle(rectangleToPinTo.Right - rectangleToPin.Width,
                                    rectangleToPinTo.Bottom - rectangleToPin.Height,
                                    rectangleToPin.Width,
                                    rectangleToPin.Height);
        }

        /// <summary>
        /// Pins a rectangle to the bottom-left of another rectangle.
        /// </summary>
        /// <param name="rectangleToPin">Rectangle to pin.</param>
        /// <param name="rectangleToPinTo">Rectangle to pin to.</param>
        /// <returns>Pinned rectangle.</returns>
        public static Rectangle PinBottomLeft(Rectangle rectangleToPin, Rectangle rectangleToPinTo)
        {
            return new Rectangle(rectangleToPinTo.X,
                                    rectangleToPinTo.Bottom - rectangleToPin.Height,
                                    rectangleToPin.Width,
                                    rectangleToPin.Height);
        }

        /// <summary>
        /// Convert a Win32 RECT structure into a .NET rectangle
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static Rectangle Convert(RECT rect)
        {
            return new Rectangle(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
        }

        /// <summary>
        /// Convert a .NET rectangle structure into a Win32 RECT structure
        /// </summary>
        /// <param name="rect">rect to convert</param>
        /// <returns>converted rect</returns>
        public static RECT Convert(Rectangle rect)
        {
            RECT newRect = new RECT();
            newRect.left = rect.Left;
            newRect.top = rect.Top;
            newRect.right = rect.Right;
            newRect.bottom = rect.Bottom;
            return newRect;
        }

        public static Rectangle Center(Size desiredSize, Rectangle anchor, bool shrinkIfNecessary)
        {
            int dW = anchor.Width - desiredSize.Width;
            int dH = anchor.Height - desiredSize.Height;
            int width = desiredSize.Width;
            int height = desiredSize.Height;

            if (shrinkIfNecessary && dW < 0)
            {
                dW = 0;
                width = anchor.Width;
            }
            if (shrinkIfNecessary && dH < 0)
            {
                dH = 0;
                height = anchor.Height;
            }

            return new Rectangle(anchor.Left + dW / 2, anchor.Top + dH / 2, width, height);
        }

        public static Rectangle RotateFlip(Size container, Rectangle rect, RotateFlipType rotateFlip)
        {
            bool rotate = false;
            bool flipX = false;
            bool flipY = false;

            switch (rotateFlip)
            {
                case RotateFlipType.RotateNoneFlipNone:
                    return rect;
                case RotateFlipType.RotateNoneFlipX:
                    flipX = true;
                    break;
                case RotateFlipType.RotateNoneFlipXY:
                    flipX = flipY = true;
                    break;
                case RotateFlipType.RotateNoneFlipY:
                    flipY = true;
                    break;
                case RotateFlipType.Rotate90FlipNone:
                    rotate = true;
                    break;
                case RotateFlipType.Rotate90FlipX:
                    rotate = flipX = true;
                    break;
                case RotateFlipType.Rotate90FlipXY:
                    rotate = flipX = flipY = true;
                    break;
                case RotateFlipType.Rotate90FlipY:
                    rotate = flipY = true;
                    break;
            }

            if (flipX)
                rect.X = container.Width - rect.Right;
            if (flipY)
                rect.Y = container.Height - rect.Bottom;
            if (rotate)
            {
                Point p = new Point(rect.Left, rect.Bottom);
                rect.Location = new Point(container.Height - p.Y, p.X);

                int oldWidth = rect.Width;
                rect.Width = rect.Height;
                rect.Height = oldWidth;
            }

            return rect;
        }

        public static Rectangle UndoRotateFlip(Size container, Rectangle rect, RotateFlipType rotateFlip)
        {
            switch (rotateFlip)
            {
                case RotateFlipType.RotateNoneFlipNone:
                    return rect;
                case RotateFlipType.RotateNoneFlipX:
                    rotateFlip = RotateFlipType.RotateNoneFlipX;
                    break;
                case RotateFlipType.RotateNoneFlipXY:
                    rotateFlip = RotateFlipType.RotateNoneFlipXY;
                    break;
                case RotateFlipType.RotateNoneFlipY:
                    rotateFlip = RotateFlipType.RotateNoneFlipY;
                    break;
                case RotateFlipType.Rotate90FlipNone:
                    rotateFlip = RotateFlipType.Rotate270FlipNone;
                    break;
                case RotateFlipType.Rotate90FlipX:
                    rotateFlip = RotateFlipType.Rotate270FlipX;
                    break;
                case RotateFlipType.Rotate90FlipXY:
                    rotateFlip = RotateFlipType.Rotate270FlipXY;
                    break;
                case RotateFlipType.Rotate90FlipY:
                    rotateFlip = RotateFlipType.Rotate270FlipY;
                    break;
            }
            return RotateFlip(container, rect, rotateFlip);
        }

        public static Rectangle EnforceAspectRatio(Rectangle bounds, float aspectRatio)
        {
            Size originalSize = bounds.Size;

            float portalAspectRatio = aspectRatio;
            float imageAspectRatio = originalSize.Width / (float)originalSize.Height;

            Rectangle srcRect = new Rectangle(Point.Empty, originalSize);
            if (imageAspectRatio < portalAspectRatio)
            {
                srcRect.Height = System.Convert.ToInt32(originalSize.Width / portalAspectRatio);
                srcRect.Y = Math.Max(0, (originalSize.Height - srcRect.Height) / 2);
                srcRect.Height = Math.Min(srcRect.Height, originalSize.Height - srcRect.Y);
            }
            else
            {
                srcRect.Width = System.Convert.ToInt32(originalSize.Height * portalAspectRatio);
                srcRect.X = Math.Max(0, (originalSize.Width - srcRect.Width) / 2);
                srcRect.Width = Math.Min(srcRect.Width, originalSize.Width - srcRect.X);
            }

            srcRect.Offset(bounds.Location);
            return srcRect;
        }
    }
}
