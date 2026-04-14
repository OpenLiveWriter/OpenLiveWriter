// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Helper functions for working with Images
    /// </summary>
    public class ImageHelper
    {
        public static byte[] GetBitmapBytes(Bitmap bitmap, Size requiredSize)
        {
            ImageFormat bitmapFormat = bitmap.RawFormat;
            if (requiredSize != Size.Empty && bitmap.Size != requiredSize)
            {
                // shrink or grow the bitmap as appropriate
                Bitmap correctedBitmap = new Bitmap(bitmap, requiredSize);

                // update corrected
                bitmap = correctedBitmap;
            }

            return GetBitmapBytes(bitmap, bitmapFormat);
        }

        public static byte[] GetBitmapBytes(Bitmap bitmap)
        {
            return GetBitmapBytes(bitmap, bitmap.RawFormat);
        }
        public static byte[] GetBitmapBytes(Bitmap bitmap, ImageFormat imageFormat)
        {
            if (bitmap != null)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    bitmap.Save(memoryStream, imageFormat);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    return memoryStream.ToArray();
                }
            }
            else
            {
                return null;
            }
        }

        public static Bitmap DownloadBitmap(string bitmapUrl)
        {
            return DownloadBitmap(bitmapUrl, null);
        }

        public static Bitmap DownloadBitmap(string bitmapUrl, WinInetCredentialsContext credentialsContext)
        {
            // download the image
            try
            {
                string fileName = UrlDownloadToFile.Download(bitmapUrl, credentialsContext);
                return new Bitmap(fileName);
            }
            catch (Exception ex)
            {
                // reformat error for contextual clarity
                throw new Exception(
                    String.Format(CultureInfo.CurrentCulture, "Error attempting to download bitmap from url {0}: {1}", bitmapUrl, ex.Message), ex);
            }
        }

        /// <summary>
        /// Creates a "disabled" version of the given bitmap.
        /// Applies 95% desaturation and 45% opacity.
        /// </summary>
        public static Bitmap MakeDisabled(Bitmap enabled)
        {
            if (enabled == null)
                return null;

            Bitmap disabled = new Bitmap(enabled.Width, enabled.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(disabled))
            {
                ImageAttributes ia = new ImageAttributes();

                float rw = 0.3f;
                float gw = 0.3f;
                float bw = 0.3f;
                float d = 0.95f;  // desaturate by 95%
                ColorMatrix cm = new ColorMatrix(new float[][] {
                                                                   new float[] {d*rw+1-d, d*rw, d*rw, 0, 0},
                                                                   new float[] {d*gw, d*gw+1-d, d*gw, 0, 0},
                                                                   new float[] {d*bw, d*bw, d*bw+1-d, 0, 0},
                                                                   new float[] {0, 0, 0, 0.45f, 0},
                                                                   new float[] {0, 0, 0, 0, 1}
                                                               }
                    );
                ia.SetColorMatrix(cm);
                Rectangle bounds = new Rectangle(0, 0, enabled.Width, enabled.Height);
                g.DrawImage(enabled, bounds, 0, 0, bounds.Width, bounds.Height, GraphicsUnit.Pixel, ia);
            }
            return disabled;
        }

        public static ColorMatrix GetHighContrastImageMatrix()
        {
            Color c = SystemColors.WindowText;

            //convert to black/white
            float rwgt = 0.3086f;
            float gwgt = 0.6094f;
            float bwgt = 0.0820f;

            //shift colors toward the window text color
            float cm4R = c.R / 255f;
            float cm4G = c.G / 255f;
            float cm4B = c.B / 255f;

            if (cm4R == 1 && cm4G == 1 & cm4B == 1) //don't oversaturate black and white state with white from text color
            {
                cm4R = .3f;
                cm4G = .3f;
                cm4B = .3f;
            }

            ColorMatrix cm = new ColorMatrix(new float[][]{
                new float[]{rwgt, rwgt, rwgt,    0f,    0f},
                new float[]{gwgt, gwgt, gwgt,    0f,    0f},
                new float[]{bwgt, bwgt, bwgt,    0f,    0f},
                new float[]{0f,     0f,   0f,    1f,    0f},
                new float[]{cm4R,  cm4G,   cm4B,    0f,    1f}});

            return cm;
        }

        /// <summary>
        /// Gets a color matrix that discards source color information
        /// while preserving source alpha.
        /// </summary>
        public static ColorMatrix GetColorOverrideImageMatrix(Color c)
        {
            ColorMatrix cm = new ColorMatrix();
            cm.Matrix00 = 0;
            cm.Matrix11 = 0;
            cm.Matrix22 = 0;
            cm.Matrix40 = c.R / 255f;
            cm.Matrix41 = c.G / 255f;
            cm.Matrix42 = c.B / 255f;
            return cm;
        }

        public static void ConvertToHighContrast(Bitmap bitmap)
        {
            ColorMatrix cm = GetHighContrastImageMatrix();
            ImageAttributes ia = new ImageAttributes();
            ia.SetColorMatrix(cm);
            Graphics g = Graphics.FromImage(bitmap);
            using (g)
                g.DrawImage(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, ia);
        }

        public static void ConvertColors(Bitmap bitmap, params ColorMap[] colorMaps)
        {
            ImageAttributes ia = new ImageAttributes();
            ia.SetRemapTable(colorMaps);
            Graphics g = Graphics.FromImage(bitmap);
            using (g)
                g.DrawImage(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, ia);
        }

        #region clever constraining
        public static Rectangle CleverConstrain(Rectangle initialBounds, double? aspectRatio, AnchorStyles anchor, Point newLoc)
        {
            if (aspectRatio == null)
                aspectRatio = initialBounds.Width / (double)initialBounds.Height;

            bool up = AnchorStyles.Bottom == (anchor & AnchorStyles.Bottom);
            bool left = AnchorStyles.Right == (anchor & AnchorStyles.Right);

            PointF origin = new PointF(
                initialBounds.Left + (left ? initialBounds.Width : 0),
                initialBounds.Top + (up ? initialBounds.Height : 0));

            double desiredAngle = Math.Atan2(
                1d * (up ? -1 : 1),
                1d * aspectRatio.Value * (left ? -1 : 1));

            double actualAngle = Math.Atan2(
                newLoc.Y - origin.Y,
                newLoc.X - origin.X);

            double angleDiff = Math.Abs(actualAngle - desiredAngle);

            if (angleDiff >= Math.PI / 2) // >=90 degrees--too much angle!
                return DoResize(initialBounds, anchor, new Point((int)origin.X, (int)origin.Y));

            double distance = Distance(origin, newLoc);

            double resizeMagnitude = Math.Cos(angleDiff) * distance;
            double xMagnitude = resizeMagnitude * Math.Cos(desiredAngle);
            double yMagnitude = resizeMagnitude * Math.Sin(desiredAngle);

            newLoc.X = Round(origin.X + xMagnitude);
            newLoc.Y = Round(origin.Y + yMagnitude);

            return DoResize(initialBounds, anchor, newLoc);
        }

        private static int Round(double dblVal) { return (int)Math.Round(dblVal); }

        private static double Distance(PointF pFrom, PointF pTo)
        {
            double deltaX = Math.Abs((double)(pTo.X - pFrom.X));
            double deltaY = Math.Abs((double)(pTo.Y - pFrom.Y));
            return Math.Sqrt(
                deltaX * deltaX + deltaY * deltaY
                );
        }

        private static Rectangle DoResize(Rectangle initialBounds, AnchorStyles anchor, Point newLoc)
        {
            const int MIN_WIDTH = 1;
            const int MIN_HEIGHT = 1;

            Rectangle newRect = initialBounds;

            int deltaX;// = newLoc.X - initialLoc.X;
            int deltaY;// = newLoc.Y - initialLoc.Y;

            switch (anchor & (AnchorStyles.Left | AnchorStyles.Right))
            {
                case AnchorStyles.Right:
                    deltaX = newLoc.X - initialBounds.Left;
                    if (MIN_WIDTH >= initialBounds.Width - deltaX)
                        deltaX = initialBounds.Width - MIN_WIDTH;
                    newRect.Width -= deltaX;
                    newRect.Offset(deltaX, 0);
                    break;
                case AnchorStyles.Left:
                    deltaX = newLoc.X - initialBounds.Right;
                    if (MIN_WIDTH >= initialBounds.Width + deltaX)
                        deltaX = -initialBounds.Width + MIN_WIDTH;
                    newRect.Width += deltaX;
                    break;
            }

            switch (anchor & (AnchorStyles.Top | AnchorStyles.Bottom))
            {
                case AnchorStyles.Bottom:
                    deltaY = newLoc.Y - initialBounds.Top;
                    if (MIN_HEIGHT >= initialBounds.Height - deltaY)
                        deltaY = initialBounds.Height - MIN_HEIGHT;
                    newRect.Height -= deltaY;
                    newRect.Offset(0, deltaY);
                    break;
                case AnchorStyles.Top:
                    deltaY = newLoc.Y - initialBounds.Bottom;
                    if (MIN_HEIGHT >= initialBounds.Height + deltaY)
                        deltaY = -initialBounds.Height + MIN_HEIGHT;
                    newRect.Height += deltaY;
                    break;
            }

            return newRect;
        }
        #endregion

        public static Bitmap RotateBitmap(Bitmap image, double degrees, Color? bgColor)
        {
            // The 1-pixel padding is necessary to avoid ugly aliasing on the edges of the
            // bitmap. I don't know why this happens...
            using (Bitmap paddedImage = new Bitmap(image.Width + 2, image.Height + 2))
            {
                using (Graphics g = Graphics.FromImage(paddedImage))
                    g.DrawImage(image, 1, 1, image.Width, image.Height);
                return RotateBitmap_NoPadding(paddedImage, degrees, bgColor);
            }
        }

        public static Bitmap RotateBitmap_NoPadding(Bitmap image, double degrees, Color? bgColor)
        {
            PointF[] points = new PointF[] {
                new PointF(0, 0),
                new PointF(image.Width, 0),
                new PointF(0, image.Height),
                new PointF(image.Width, image.Height),
                };

            double angle = PolarPoint.ToRadians(degrees);

            float minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            for (int i = 0; i < points.Length; i++)
            {
                PolarPoint p = new PolarPoint(points[i]);
                p.Angle += angle;
                points[i] = p.ToPointF();

                minX = Math.Min(minX, points[i].X);
                minY = Math.Min(minY, points[i].Y);
                maxX = Math.Max(maxX, points[i].X);
                maxY = Math.Max(maxY, points[i].Y);
            }

            Size size = Size.Ceiling(new SizeF(maxX - minX, maxY - minY));
            Bitmap target = new Bitmap(size.Width, size.Height);
            using (Graphics g = Graphics.FromImage(target))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                if (bgColor != null && bgColor != Color.Transparent)
                {
                    using (Brush b = new SolidBrush(bgColor.Value))
                        g.FillRectangle(b, 0, 0, size.Width, size.Height);
                }

                g.TranslateTransform(-minX, -minY);
                g.DrawImage(image, new PointF[] { points[0], points[1], points[2] });
            }
            return target;
        }

        public static Bitmap ApplyColorMatrix(Bitmap bitmap, ColorMatrix cm)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            Bitmap newBitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(newBitmap))
            {
                using (ImageAttributes ia = new ImageAttributes())
                {
                    ia.SetColorMatrix(cm);
                    g.DrawImage(bitmap, new Rectangle(0, 0, width, height), 0, 0, width, height, GraphicsUnit.Pixel, ia);
                }
            }
            return newBitmap;
        }

        // Inspects the EXIF orientation of originalImage and performs the same rotation on originalImage
        public static void AutoRotateFromExifOrientation(Image originalImage)
        {
            ExifMetadata exif = ExifMetadata.FromImage(originalImage);
            // Rotate the original image appropriately
            if (RotateFromExifOrientation(exif.Orientation, originalImage))
            {
                // If we rotated, then reset the exif data
                exif.Orientation = ExifOrientation.Normal;
            }
        }

        /// <summary>
        /// Given an exif orientation, rotates the passed in image accordingly
        /// </summary>
        /// <param name="orientation"></param>
        /// <param name="image"></param>
        public static bool RotateFromExifOrientation(ExifOrientation orientation, Image image)
        {
            switch (orientation)
            {
                case ExifOrientation.Rotate90CW:
                    image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    return true;
                case ExifOrientation.Rotate270CW:
                    image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    return true;
                case ExifOrientation.Normal:
                case ExifOrientation.Unknown:
                    return false;
            }

            return false;
        }
    }

    /// <summary>
    /// Describes a point in polar coordinate space
    /// http://en.wikipedia.org/wiki/Polar_coordinates
    /// </summary>
    public struct PolarPoint
    {
        /// <summary>
        /// Angle in radians
        /// </summary>
        public double Angle;
        public double Radius;

        public PolarPoint(PointF p)
        {
            Angle = Math.Atan2(p.Y, p.X);
            Radius = Math.Sqrt(p.X * p.X + p.Y * p.Y);
        }

        public PointF ToPointF()
        {
            return new PointF(
                (float)(Radius * Math.Cos(Angle)),
                (float)(Radius * Math.Sin(Angle)));
        }

        public static double ToRadians(double degrees)
        {
            return Math.PI / 180 * degrees;
        }

        public static double ToDegrees(double radians)
        {
            return radians * 180 / Math.PI;
        }

    }

}
