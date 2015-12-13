// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Graphics helper class.
    /// </summary>
    public sealed class GraphicsHelper
    {
        /// <summary>
        /// Initializes a new instance of the GraphicsHelper class.
        /// </summary>
        private GraphicsHelper()
        {
        }

        /// <summary>
        /// Helper to tile fill an unscaled image vertically.
        /// </summary>
        /// <param name="graphics">Graphics context in which the image is to be tiled.</param>
        /// <param name="image">The image to tile.</param>
        /// <param name="rectangle">The rectangle to fill.</param>
        public static void TileFillUnscaledImageVertically(Graphics graphics, Image image, Rectangle rectangle)
        {
            //	Tile while there is height left to fill.
            int fillWidth = Math.Min(image.Width, rectangle.Width);
            Rectangle imageRectangle = new Rectangle(Point.Empty, image.Size);
            for (int y = rectangle.Y; y < rectangle.Bottom;)
            {
                //	Calculate the fill height for this iteration.
                int fillHeight = Math.Min(image.Height, rectangle.Bottom - y);

                //	Fill the fill height with the image.
                graphics.DrawImage(image,
                                    new Rectangle(rectangle.X, y, fillWidth, fillHeight),
                                    new Rectangle(0, 0, fillWidth, fillHeight),
                                    GraphicsUnit.Pixel);

                //	Adjust the y position for the next loop iteration.
                y += fillHeight;
            }
        }

        /// <summary>
        /// Helper to tile fill an unscaled image vertically.
        /// </summary>
        /// <param name="graphics">Graphics context in which the image is to be tiled.</param>
        /// <param name="image">The image to tile.</param>
        /// <param name="srcRectangle">The source rectangle in the image.</param>
        /// <param name="srcRectangle">The destination rectangle to fill.</param>
        [Obsolete("Slow. Use OpenLiveWriter.CoreServices.UI.BorderPaint instead", false)]
        public static void TileFillUnscaledImageVertically(Graphics graphics, Image image, Rectangle srcRectangle, Rectangle destRectangle)
        {
            //	Tile while there is height left to fill.
            int fillWidth = Math.Min(srcRectangle.Width, destRectangle.Width);
            for (int y = destRectangle.Y; y < destRectangle.Bottom;)
            {
                //	Calculate the fill height for this iteration.
                int fillHeight = Math.Min(srcRectangle.Height, destRectangle.Bottom - y);

                //	Fill the fill height with the image.
                graphics.DrawImage(image,
                                    new Rectangle(destRectangle.X, y, fillWidth, fillHeight),
                                    new Rectangle(srcRectangle.X, srcRectangle.Y, fillWidth, fillHeight),
                                    GraphicsUnit.Pixel);

                //	Adjust the y position for the next loop iteration.
                y += fillHeight;
            }
        }

        /// <summary>
        /// Helper to tile fill an unscaled image horizontally.
        /// </summary>
        /// <param name="graphics">Graphics context in which the image is to be tiled.</param>
        /// <param name="image">The image to tile.</param>
        /// <param name="rectangle">The rectangle to fill.</param>
        public static void TileFillUnscaledImageHorizontally(Graphics graphics, Image image, Rectangle rectangle)
        {
            //	Tile while there is width left to fill.
            int fillHeight = Math.Min(image.Height, rectangle.Height);
            for (int x = rectangle.X; x < rectangle.Right;)
            {
                //	Calculate the fill width for this iteration.
                int fillWidth = Math.Min(image.Width, rectangle.Right - x);

                //	Fill the fill width with the image.
                graphics.DrawImage(image,
                                    new Rectangle(x, rectangle.Y, fillWidth, fillHeight),
                                    new Rectangle(0, 0, fillWidth, fillHeight),
                                    GraphicsUnit.Pixel);

                //	Adjust the x position for the next loop iteration.
                x += fillWidth;
            }
        }

        /// <summary>
        /// Helper to tile fill an unscaled image horizontally.
        /// </summary>
        /// <param name="graphics">Graphics context in which the image is to be tiled.</param>
        /// <param name="image">The image to tile.</param>
        /// <param name="srcRectangle">The source rectangle in the image.</param>
        /// <param name="srcRectangle">The destination rectangle to fill.</param>
        [Obsolete("Slow. Use OpenLiveWriter.CoreServices.UI.BorderPaint instead", false)]
        public static void TileFillUnscaledImageHorizontally(Graphics graphics, Image image, Rectangle srcRectangle, Rectangle destRectangle)
        {
            //	Tile while there is width left to fill.
            int fillHeight = Math.Min(srcRectangle.Height, destRectangle.Height);
            for (int x = destRectangle.X; x < destRectangle.Right;)
            {
                //	Calculate the fill width for this iteration.
                int fillWidth = Math.Min(srcRectangle.Width, destRectangle.Right - x);

                //	Fill the fill width with the image.
                graphics.DrawImage(image,
                                    new Rectangle(x, destRectangle.Y, fillWidth, fillHeight),
                                    new Rectangle(srcRectangle.X, srcRectangle.Y, fillWidth, fillHeight),
                                    GraphicsUnit.Pixel);

                //	Adjust the x position for the next loop iteration.
                x += fillWidth;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="image"></param>
        /// <param name="rectangle"></param>
        public static void TileFillScaledImageHorizontally(BidiGraphics graphics, Image image, Rectangle rectangle)
        {
            Rectangle imageRectangle = new Rectangle(Point.Empty, image.Size);
            TileFillScaledImageHorizontally(graphics, image, rectangle, imageRectangle);
        }

        private static void TileFillScaledImageHorizontally(BidiGraphics graphics, Image image, Rectangle rectangle, Rectangle srcRectangle)
        {
            for (int x = rectangle.X; x < rectangle.Right; x += srcRectangle.Width)
                graphics.DrawImage(true,
                                    image,
                                       new Rectangle(x, rectangle.Y, Math.Min(srcRectangle.Width, rectangle.Right - x), rectangle.Height),
                                       srcRectangle,
                                       GraphicsUnit.Pixel);

        }

        public static Rectangle[] SliceCompositedImageBorder(Size imgSize, int vert1, int vert2, int horiz1, int horiz2)
        {
            int left = 0, center = vert1, right = vert2, x4 = imgSize.Width;
            int top = 0, middle = horiz1, bottom = horiz2, y4 = imgSize.Height;
            int leftWidth = center, centerWidth = right - center, rightWidth = x4 - right;
            int topHeight = middle, middleHeight = bottom - middle, bottomHeight = y4 - bottom;

            return new Rectangle[]
                {
                    // top left
                    new Rectangle(left, top, leftWidth, topHeight),
                    // top center
                    new Rectangle(center, top, centerWidth, topHeight),
                    // top right
                    new Rectangle(right, top, rightWidth, topHeight),
                    // left
                    new Rectangle(left, middle, leftWidth, middleHeight),
                    // middle
                    new Rectangle(center, middle, centerWidth, middleHeight),
                    // right
                    new Rectangle(right, middle, rightWidth, middleHeight),
                    // bottom left
                    new Rectangle(left, bottom, leftWidth, bottomHeight),
                    // bottom center
                    new Rectangle(center, bottom, centerWidth, bottomHeight),
                    // bottom right
                    new Rectangle(right, bottom, rightWidth, bottomHeight)
                };
        }

        // Slow. Use OpenLiveWriter.CoreServices.UI.BorderPaint if performance matters
        public static void DrawLeftCenterRightImageBorder(
            BidiGraphics graphics,
            Rectangle rectangle,
            Image image,
            Rectangle leftSlice,
            Rectangle centerSlice,
            Rectangle rightSlice)
        {
            GraphicsContainer graphicsContainer = graphics.Graphics.BeginContainer();
            // Have to remove this line because it messes with mirrored images.
            //   Specifically, right-to-left drawing of the hover effect for the context menu dongle
            //   hanging off "Save Draft" doesn't happen at all. Seems like a short-circuit happens
            //   in Graphics when the image's location is outside the clipping area.
            //graphics.Graphics.SetClip(rectangle);
            graphics.Graphics.CompositingMode = CompositingMode.SourceOver;
            graphics.Graphics.CompositingQuality = CompositingQuality.HighQuality;

            graphics.DrawImage(
                true,
                image,
                new Rectangle(rectangle.Left, rectangle.Top, leftSlice.Width, rectangle.Height),
                leftSlice.Left, leftSlice.Top, leftSlice.Width, leftSlice.Height,
                GraphicsUnit.Pixel
                );
            TileFillScaledImageHorizontally(
                graphics,
                image,
                new Rectangle(rectangle.Left + leftSlice.Width, rectangle.Top, Math.Max(0, rectangle.Width - leftSlice.Width - rightSlice.Width), rectangle.Height),
                centerSlice);
            graphics.DrawImage(
                true,
                image,
                new Rectangle(rectangle.Right - rightSlice.Width, rectangle.Top, rightSlice.Width, rectangle.Height),
                rightSlice.Left, rightSlice.Top, rightSlice.Width, rightSlice.Height,
                GraphicsUnit.Pixel
                );

            graphics.Graphics.EndContainer(graphicsContainer);
        }

        [Obsolete("Slow. Use OpenLiveWriter.CoreServices.UI.BorderPaint instead", false)]
        public static void DrawCompositedImageBorder(
            Graphics graphics,
            Rectangle rectangle,
            Image image,
            Rectangle[] slices)
        {
            DrawCompositedImageBorder(graphics, rectangle, image,
                slices[0],
                slices[1],
                slices[2],
                slices[3],
                slices[4],
                slices[5],
                slices[6],
                slices[7]);
        }

        /// <summary>
        /// Draws a composited image border.
        /// </summary>
        /// <remarks>
        ///	Note that because it would be too computationally expensive, it is assumed that images
        ///	will fit into the specified rectangle.
        ///	</remarks>
        /// <param name="graphics">A graphics context into which the image-based border is to be drawn.</param>
        /// <param name="rectangle">The rectangle into which the image-based border is to be drawn.</param>
        /// <param name="topLeftRectangle">The top left rectangle.</param>
        /// <param name="topCenterRectangle">The top center (fill) rectangle.</param>
        /// <param name="topRightRectangle">The top right rectangle.</param>
        /// <param name="leftCenterRectangle">The left center (fill) rectangle.</param>
        /// <param name="rightCenterRectangle">The right center (fill) rectangle.</param>
        /// <param name="bottomLeftRectangle">The bottom left rectangle.</param>
        /// <param name="bottomCenterRectangle">The bottom center (fill) rectangle.</param>
        /// <param name="bottomRightRectangle">The bottom right rectangle.</param>
        [Obsolete("Slow. Use OpenLiveWriter.CoreServices.UI.BorderPaint instead", false)]
        public static void DrawCompositedImageBorder
            (
            Graphics graphics,
            Rectangle rectangle,
            Image image,
            Rectangle topLeftRectangle,
            Rectangle topCenterRectangle,
            Rectangle topRightRectangle,
            Rectangle leftCenterRectangle,
            Rectangle rightCenterRectangle,
            Rectangle bottomLeftRectangle,
            Rectangle bottomCenterRectangle,
            Rectangle bottomRightRectangle
            )
        {
            Rectangle fillRectangle;

            //	Save a graphics container with the current state of the graphics object and open
            //	and use a new, clipped graphics container.
            GraphicsContainer graphicsContainer = graphics.BeginContainer();
            graphics.SetClip(rectangle);
            graphics.CompositingMode = CompositingMode.SourceOver;
            graphics.CompositingQuality = CompositingQuality.HighQuality;

            if (HasArea(topLeftRectangle))
            {
                //	Top left.
                graphics.DrawImage(image,
                                       new Rectangle(rectangle.X, rectangle.Y, topLeftRectangle.Width, topLeftRectangle.Height),
                                       topLeftRectangle,
                                       GraphicsUnit.Pixel);
            }

            if (HasArea(topCenterRectangle))
            {
                //	Top center.
                fillRectangle = new Rectangle(rectangle.X + topLeftRectangle.Width,
                                                  rectangle.Y,
                                                  rectangle.Width - (topLeftRectangle.Width + topRightRectangle.Width),
                                                  topCenterRectangle.Height);
                TileFillUnscaledImageHorizontally(graphics, image, topCenterRectangle, fillRectangle);
            }

            if (HasArea(topRightRectangle))
            {
                //	Top right.
                graphics.DrawImage(image,
                                       new Rectangle(rectangle.Right - topRightRectangle.Width, rectangle.Y, topRightRectangle.Width, topRightRectangle.Height),
                                       topRightRectangle,
                                       GraphicsUnit.Pixel);
            }

            if (HasArea(leftCenterRectangle))
            {
                //	Left center.
                fillRectangle = new Rectangle(rectangle.X,
                                                  rectangle.Y + topLeftRectangle.Height,
                                                  leftCenterRectangle.Width,
                                                  rectangle.Height - (topLeftRectangle.Height + bottomLeftRectangle.Height));
                TileFillUnscaledImageVertically(graphics, image, leftCenterRectangle, fillRectangle);
            }

            if (HasArea(rightCenterRectangle))
            {
                //	Right center.
                fillRectangle = new Rectangle(rectangle.Right - rightCenterRectangle.Width,
                                                  rectangle.Y + topRightRectangle.Height,
                                                  rightCenterRectangle.Width,
                                                  rectangle.Height - (topRightRectangle.Height + bottomRightRectangle.Height));
                TileFillUnscaledImageVertically(graphics, image, rightCenterRectangle, fillRectangle);
            }

            if (HasArea(bottomLeftRectangle))
            {
                //	Bottom left.
                graphics.DrawImage(image,
                                       new Rectangle(rectangle.X, rectangle.Bottom - bottomLeftRectangle.Height, bottomLeftRectangle.Width, bottomLeftRectangle.Height),
                                       bottomLeftRectangle,
                                       GraphicsUnit.Pixel);
            }

            if (HasArea(bottomCenterRectangle))
            {
                //	Bottom center.
                fillRectangle = new Rectangle(rectangle.X + bottomLeftRectangle.Width,
                                                  rectangle.Bottom - bottomCenterRectangle.Height,
                                                  rectangle.Width - (bottomLeftRectangle.Width + bottomRightRectangle.Width),
                                                  bottomCenterRectangle.Height);
                TileFillUnscaledImageHorizontally(graphics, image, bottomCenterRectangle, fillRectangle);
            }

            if (HasArea(bottomRightRectangle))
            {
                //	Botom right.
                graphics.DrawImage(image,
                                       new Rectangle(rectangle.Right - bottomRightRectangle.Width, rectangle.Bottom - bottomRightRectangle.Height, bottomRightRectangle.Width, bottomRightRectangle.Height),
                                       bottomRightRectangle,
                                       GraphicsUnit.Pixel);
            }

            //	End the graphics container.
            graphics.EndContainer(graphicsContainer);
        }

        private static bool HasArea(Rectangle rectangle)
        {
            return rectangle.Height > 0 && rectangle.Width > 0;
        }

        /// <summary>
        /// Draws a composited image border.  Note that because it would be too computationally
        /// expensive, it is ASSUMED that the rectangle supplied is large enough to draw the border
        /// witout
        /// </summary>
        /// <remarks>
        ///	Note that because it would be too computationally expensive, it is assumed that images
        ///	will fit into the specified rectangle.
        ///	</remarks>
        /// <param name="graphics">A graphics context into which the image-based border is to be drawn.</param>
        /// <param name="rectangle">The rectangle into which the image-based border is to be drawn.</param>
        /// <param name="topLeftImage">The top left image.</param>
        /// <param name="topCenterImage">The top center (fill) image.</param>
        /// <param name="topRightImage">The top right image.</param>
        /// <param name="leftCenterImage">The left center (fill) image.</param>
        /// <param name="rightCenterImage">The right center (fill) image.</param>
        /// <param name="bottomLeftImage">The bottom left image.</param>
        /// <param name="bottomCenterImage">The bottom center (fill) image.</param>
        /// <param name="bottomRightImage">The bottom right image.</param>
        public static void DrawCompositedImageBorder
            (
            Graphics graphics,
            Rectangle rectangle,
            Image topLeftImage,
            Image topCenterImage,
            Image topRightImage,
            Image leftCenterImage,
            Image rightCenterImage,
            Image bottomLeftImage,
            Image bottomCenterImage,
            Image bottomRightImage
            )
        {
            Rectangle fillRectangle;

            //	Save a graphics container with the current state of the graphics object and open
            //	and use a new, clipped graphics container.
            GraphicsContainer graphicsContainer = graphics.BeginContainer();
            graphics.SetClip(rectangle);

            //	Top left.
            graphics.DrawImageUnscaled(topLeftImage, rectangle.X, rectangle.Y);

            //	Top center.
            fillRectangle = new Rectangle(rectangle.X + topLeftImage.Width,
                                            rectangle.Y,
                                            rectangle.Width - (topLeftImage.Width + topRightImage.Width),
                                            topCenterImage.Height);
            TileFillUnscaledImageHorizontally(graphics, topCenterImage, fillRectangle);

            //	Top right.
            graphics.DrawImageUnscaled(topRightImage, rectangle.Right - topRightImage.Width, rectangle.Y);

            //	Left center.
            fillRectangle = new Rectangle(rectangle.X,
                                            rectangle.Y + topLeftImage.Height,
                                            leftCenterImage.Width,
                                            rectangle.Height - (topLeftImage.Height + bottomLeftImage.Height));
            TileFillUnscaledImageVertically(graphics, leftCenterImage, fillRectangle);

            //	Right center.
            fillRectangle = new Rectangle(rectangle.Right - rightCenterImage.Width,
                                            rectangle.Y + topRightImage.Height,
                                            rightCenterImage.Width,
                                            rectangle.Height - (topRightImage.Height + bottomRightImage.Height));
            TileFillUnscaledImageVertically(graphics, rightCenterImage, fillRectangle);

            //	Bottom left.
            graphics.DrawImageUnscaled(bottomLeftImage, rectangle.X, rectangle.Bottom - bottomLeftImage.Height);

            //	Bottom center.
            fillRectangle = new Rectangle(rectangle.X + bottomLeftImage.Width,
                                            rectangle.Bottom - bottomCenterImage.Height,
                                            rectangle.Width - (bottomLeftImage.Width + bottomRightImage.Width),
                                            bottomCenterImage.Height);
            TileFillUnscaledImageHorizontally(graphics, bottomCenterImage, fillRectangle);

            //	Bottom right.
            graphics.DrawImageUnscaled(bottomRightImage, rectangle.Right - bottomRightImage.Width, rectangle.Bottom - bottomRightImage.Height);

            //	End the graphics container.
            graphics.EndContainer(graphicsContainer);
        }

        /// <summary>
        /// Converts an opacity percent between 0.0 and 100.0, inclusive, into an alpha component
        /// value between 0 and 255.
        /// </summary>
        /// <param name="opacity">Opacity percent between 0.0 and 100.0.</param>
        /// <returns>Alpha component value between 0 and 255</returns>
        public static int Opacity(double opacity)
        {
            Debug.Assert(opacity >= 0.0 && opacity <= 100.0, "Invalid opacity specified", "Specify opacity as a value between 0.0 and 100.0, inclusive.");
            if (opacity >= 0.0 && opacity <= 100.0)
                return Convert.ToInt32((255.0 * opacity) / 100.0);
            else
                return 255;
        }

        public static IDisposable Offset(Graphics g, Rectangle dest, Rectangle src)
        {
            Debug.Assert(dest.Size.Equals(src.Size), "Can't offset with rectangles of unequal sizes");
            return Offset(g, dest.Location.X - src.Location.X, dest.Location.Y - src.Location.Y);
        }

        public static IDisposable Offset(Graphics g, int x, int y)
        {
            GraphicsState graphicsState = g.Save();
            g.TranslateTransform(x, y);
            return new GraphicsStateRestorer(g, graphicsState);
        }

        private class GraphicsStateRestorer : IDisposable
        {
            private readonly Graphics graphics;
            private readonly GraphicsState graphicsState;
            private bool disposed = false;

            public GraphicsStateRestorer(Graphics graphics, GraphicsState graphicsState)
            {
                this.graphics = graphics;
                this.graphicsState = graphicsState;
            }

            public void Dispose()
            {
                if (!disposed)
                {
                    disposed = true;
                    graphics.Restore(graphicsState);
                }
            }
        }
    }
}
