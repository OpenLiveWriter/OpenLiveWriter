// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using ImageManipulation;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.CoreServices
{
    public delegate Bitmap ImageFilter(Bitmap bitmap);

    /// <summary>
    /// Helper functions for working with Images
    /// </summary>
    public class ImageHelper2
    {
        private const int JPEG_QUALITY = 93;

        /// <summary>
        /// Overlays a bitmap onto another bitmap.
        /// </summary>
        /// <param name="bitmap">the base bit map that will have the overlay applied to it</param>
        /// <param name="overlay">the bitmap that will be overlayed (the top layer)</param>
        /// <param name="location">the location where the overlay should be placed.</param>
        public static void OverlayBitmap(Bitmap bitmap, Bitmap overlay, LOCATION location)
        {
            Graphics g = Graphics.FromImage(bitmap);

            Rectangle bitmapRectangle;
            if (location == LOCATION.TOP_LEFT)
                bitmapRectangle = new Rectangle(0, 0, overlay.Width, overlay.Height);
            else if (location == LOCATION.BOTTOM_LEFT)
                bitmapRectangle = new Rectangle(0, bitmap.Height - overlay.Height, overlay.Width, overlay.Height);
            else if (location == LOCATION.TOP_RIGHT)
                bitmapRectangle = new Rectangle(bitmap.Width - overlay.Width, 0, overlay.Width, overlay.Height);
            else if (location == LOCATION.BOTTOM_RIGHT)
                bitmapRectangle = new Rectangle(bitmap.Width - overlay.Width, bitmap.Height - overlay.Height, overlay.Width, overlay.Height);
            else
            {
                Debug.Fail("Unsupported location: " + location);
                bitmapRectangle = new Rectangle(0, 0, overlay.Width, overlay.Height);
            }

            g.DrawImageUnscaled(overlay,
                bitmapRectangle.Left,
                bitmapRectangle.Top);
        }
        public enum LOCATION { TOP_LEFT = 1, BOTTOM_LEFT = 2, TOP_RIGHT = 3, BOTTOM_RIGHT = 4 };

        /// <summary>
        /// Shifts the alpha value of a bitmap by a specified percentage (ex: .85 decreases by 15%, 1.15 increase by 15%).
        /// </summary>
        /// <param name="bitmap">the bitmap whose pixels will be updated with the new alpha settings</param>
        /// <param name="alphaPercentage">a fraction designating the % increase/decrease in each pixel's alpha value</param>
        public static void ApplyAlphaShift(Bitmap bitmap, double alphaPercentage)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color c = bitmap.GetPixel(x, y);
                    if (c.A > 0) //never make transparent pixels non-transparent
                    {
                        int newAlphaValue = (int)(c.A * alphaPercentage);
                        newAlphaValue = Math.Max(0, Math.Min(255, newAlphaValue)); //value must be between 0 and 255
                        bitmap.SetPixel(x, y, Color.FromArgb(newAlphaValue, c));
                    }
                    else
                        bitmap.SetPixel(x, y, c);
                }
            }
        }

        public static bool IsTransparentGif(string imagePath)
        {
            if (Path.GetExtension(imagePath).ToUpperInvariant() != ".GIF")
                return false;

            using (Bitmap bitmap = new Bitmap(imagePath))
            {
                return IsIndexedWithTransparency(bitmap, ImageFormat.Gif);
            }
        }

        public static bool IsAnimated(string imagePath)
        {
            if (Path.GetExtension(imagePath).ToUpperInvariant() != ".GIF")
                return false;

            using (Bitmap bitmap = new Bitmap(imagePath))
            {
                return IsAnimated(bitmap);
            }
        }

        public static bool IsAnimated(Image image)
        {
            return image.RawFormat.Guid == ImageFormat.Gif.Guid && image.GetFrameCount(FrameDimension.Time) > 1;
        }

        public static bool IsMetafile(string path)
        {
            switch ((Path.GetExtension(path) ?? "").ToUpperInvariant())
            {
                case ".WMF":
                case ".EMF":
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Corrects the extension of an image file
        /// </summary>
        /// <param name="filePath">The path to the image file</param>
        public static string CorrectImageExtensionAsNecessary(string filePath)
        {
            ImageFormat rawFormat = null;
            try
            {
                string newExtension = null;
                using (Image image = Image.FromFile(filePath))
                {
                    rawFormat = image.RawFormat;

                    if (rawFormat == null)
                        return filePath;

                    if (rawFormat.Equals(ImageFormat.Bmp) && Path.GetExtension(filePath).ToLower(CultureInfo.InvariantCulture) != BMP)
                        newExtension = BMP;
                    else if (rawFormat.Equals(ImageFormat.Gif) && Path.GetExtension(filePath).ToLower(CultureInfo.InvariantCulture) != GIF)
                        newExtension = GIF;
                    else if (rawFormat.Equals(ImageFormat.Icon) && Path.GetExtension(filePath).ToLower(CultureInfo.InvariantCulture) != ICO)
                        newExtension = ICO;
                    else if (rawFormat.Equals(ImageFormat.Jpeg) && Path.GetExtension(filePath).ToLower(CultureInfo.InvariantCulture) != JPG)
                        newExtension = JPG;
                    else if (rawFormat.Equals(ImageFormat.Png) && Path.GetExtension(filePath).ToLower(CultureInfo.InvariantCulture) != PNG)
                        newExtension = PNG;
                }

                if (newExtension != null)
                    filePath = FileHelper.ChangeExtension(filePath, newExtension);

            }
            catch (OutOfMemoryException)
            {
            }
            return filePath;
        }
        private const string BMP = ".bmp";
        private const string GIF = ".gif";
        private const string JPG = ".jpg";
        private const string PNG = ".png";
        private const string ICO = ".ico";

        public static Image SafeGetEmbeddedThumbnail(string path)
        {
            try
            {
                return GetEmbeddedThumbnail(path);
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine("Failed to load thumbnail:" + ex);
                return null;
            }
            catch (Exception ex)
            {
                Trace.Fail("Failed to load thumbnail:" + ex);
                return null;
            }
        }

        public static Image GetEmbeddedThumbnail(string filePath)
        {
            Stream embeddedThumbnail = GetEmbeddedThumbnailStream(filePath);

            if (embeddedThumbnail != null)
                return Image.FromStream(embeddedThumbnail);

            return null;
        }

        public static Stream GetEmbeddedThumbnailStream(string filePath)
        {
            const int THUMBNAIL = 0x501B;

            using (Stream s = File.OpenRead(filePath))
            {
                using (Image img = Image.FromStream(s, false, false))
                {
                    if (!Array.Exists(img.PropertyIdList, propertyId => propertyId == THUMBNAIL))
                        return null;

                    try
                    {
                        PropertyItem item = img.GetPropertyItem(THUMBNAIL);
                        return StreamHelper.AsStream(item.Value);
                    }
                    catch (ArgumentException)
                    {
                        return null;
                    }
                }
            }
        }

        public static Image SafeFromFile(string path)
        {
            try
            {
                return Image.FromFile(path);
            }
            catch (OutOfMemoryException ex)
            {
                Debug.WriteLine("Invalid image file:" + ex);
                return null;
            }
            catch (Exception ex)
            {
                Trace.Fail("Failed to load image:" + ex);
                return null;
            }
        }

        public static Bitmap CreateResizedBitmap(Bitmap image, int width, int height, ImageFormat format)
        {
            return ImageResizer.ResizeBitmap(image, width, height, format);
        }

        /// <summary>
        /// Returns a scaled thumbnail imaged based upon a maximum set of dimensions
        /// </summary>
        /// <param name="maxWidth">The maximum width of the thumbnail</param>
        /// <param name="maxHeight">The maximum height of the thumbnail</param>
        /// <param name="image">The image to provide a thumbnail of</param>
        /// <returns>The scaled size that best fit the maximum dimensions</returns>
        public static Size SaveScaledThumbnailImage(int maxWidth, int maxHeight, Bitmap image, ImageFormat format, Stream thumbOut)
        {
            return SaveScaledThumbnailImage(maxWidth, maxHeight, image, format, thumbOut, null);
        }

        public static Size SaveScaledThumbnailImage(int maxWidth, int maxHeight, Bitmap image, ImageFormat format, Stream thumbOut, ImageFilter imageFilter)
        {
            // The dimensions of the image from which the thumbnail is needed
            float width = image.Width;
            float height = image.Height;
            float imageRatio = width / height;

            // The width/height ratio of the maximum thumbnail dimensions
            float maxRatio = (float)maxWidth / (float)maxHeight;

            // The dimensions of the thumbnail that we'll request
            float requestedWidth;
            float requestedHeight;

            if (imageRatio >= maxRatio)
            {
                // the image's width is the determinant in scaling, scale based upon that
                requestedWidth = maxWidth;
                requestedHeight = (requestedWidth * height) / width;
            }
            else
            {
                // the image's height is the determinant in scaling, scale based upon that
                requestedHeight = maxHeight;
                requestedWidth = (requestedHeight * width) / height;
            }

            Size scaledSize = new Size((int)requestedWidth, (int)requestedHeight);

            //save the thumbnail in the requested format using the scaled sizes.
            SaveThumbnailImage(scaledSize.Width, scaledSize.Height, image, format, thumbOut, imageFilter);

            return scaledSize;
        }

        /// <summary>
        /// Saves a thumbnailed image to a stream using the specified image format.
        /// </summary>
        /// <param name="width">the width of thumbnail to create</param>
        /// <param name="height">the height of thumbnail to create</param>
        /// <param name="image">the source image to generate the thumbnail from</param>
        /// <param name="newFormat">the format to save the thumbnail as (supported formats: Jpeg, Gif, Png)</param>
        /// <param name="thumbOut">the stream to save the thumbnail to</param>
        public static void SaveThumbnailImage(int width, int height, Bitmap image, ImageFormat newFormat, Stream thumbOut)
        {
            SaveThumbnailImage(width, height, image, newFormat, thumbOut, null);
        }
        public static void SaveThumbnailImage(int width, int height, Bitmap image, ImageFormat newFormat, Stream thumbOut, ImageFilter filter)
        {
            ThumbMaker thumb = new ThumbMaker(image, newFormat);
            if (newFormat == ImageFormat.Jpeg)
            {
                //note: supports high quality JPEG thumbnailing
                thumb.ResizeToJpeg(width, height, JPEG_QUALITY, thumbOut, filter);
            }
            else if (newFormat == ImageFormat.Gif)
            {
                //note: supports transparent images
                thumb.ResizeToGif(width, height, thumbOut, filter);
            }
            else if (newFormat == ImageFormat.Png)
            {
                //note: nothing special needs to be done here since PNG format rules!
                thumb.ResizeToPng(width, height, thumbOut, filter);
            }
            else if (newFormat == ImageFormat.Bmp)
            {
                thumb.ResizeToBmp(width, height, thumbOut, filter);
            }
            else
            {
                throw new Exception("unsupported image format detected");
            }
        }

        public static void SaveImage(Bitmap image, ImageFormat newFormat, Stream imgOut)
        {
            ImageSaver.SaveImage(image, newFormat, imgOut);
        }

        /// <summary>
        /// Utility class for managing image manipulation.
        /// </summary>
        public class ImageResizer
        {
            internal static void AdjustSizes(Bitmap bitmap, ref int xSize, ref int ySize)
            {
                if (xSize != 0 && ySize == 0)
                    ySize = Math.Abs((int)(xSize * bitmap.Height / bitmap.Width));
                else if (xSize == 0 && ySize != 0)
                    xSize = Math.Abs((int)(ySize * bitmap.Width / bitmap.Height));
                else if (xSize == 0 && ySize == 0)
                {
                    xSize = bitmap.Width;
                    ySize = bitmap.Height;
                }
            }

            //Internal resize for indexed colored images
            static unsafe Bitmap IndexedResize(Bitmap bitmap, int xSize, int ySize, ImageFormat format)
            {
                AdjustSizes(bitmap, ref xSize, ref ySize);

                Bitmap scaledBitmap = new Bitmap(xSize, ySize, bitmap.PixelFormat);
                scaledBitmap.Palette = bitmap.Palette;

                int sourceWidth = bitmap.Width;   // width of source
                int sourceHeight = bitmap.Height;  // height of source
                int destWidth = scaledBitmap.Width;  // width of dest
                int destHeight = scaledBitmap.Height; // height of dest

                switch (bitmap.PixelFormat)
                {
                    case PixelFormat.Format1bppIndexed:
                    case PixelFormat.Format4bppIndexed:
                        throw new ArgumentException("Unsupported pixel format " + bitmap.PixelFormat);
                    case PixelFormat.Format8bppIndexed:
                        break;
                    default:
                        throw new ArgumentException("Unsupported pixel format " + bitmap.PixelFormat);
                }

                float xRatio = (float)sourceWidth / destWidth;
                int xOffset = (int)(xRatio / 2);
                float yRatio = (float)sourceHeight / destHeight;
                int yOffset = (int)(yRatio / 2);

                BitmapData sourceBitmapData = bitmap.LockBits(new Rectangle(0, 0, sourceWidth, sourceHeight), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                try
                {
                    BitmapData destBitmapData = scaledBitmap.LockBits(new Rectangle(0, 0, destWidth, destHeight), ImageLockMode.WriteOnly, scaledBitmap.PixelFormat);
                    try
                    {
                        byte* s0 = (byte*)sourceBitmapData.Scan0.ToPointer();
                        int sourceStride = sourceBitmapData.Stride;
                        byte* d0 = (byte*)destBitmapData.Scan0.ToPointer();
                        int destStride = destBitmapData.Stride;

                        for (int y = 0; y < destHeight; y++)
                        {
                            byte* d = d0 + y * destStride;
                            byte* sRow = s0 + ((int)(y * yRatio) + yOffset) * sourceStride + xOffset;

                            // nearest y neighbor row

                            for (int x = 0; x < destWidth; x++)
                            {
                                *d++ = *(sRow + (int)(x * xRatio));
                            }
                        }

                    }
                    finally
                    {
                        scaledBitmap.UnlockBits(destBitmapData);
                    }
                }
                finally
                {
                    bitmap.UnlockBits(sourceBitmapData);
                }

                MemoryStream ms = new MemoryStream();
                scaledBitmap.Save(ms, format);
                scaledBitmap.Dispose();
                ms.Seek(0, SeekOrigin.Begin);
                return new Bitmap(new MemoryStream(ms.ToArray()));
            }

            //Internal resize for RGB colored images
            static Bitmap RGBResize(Bitmap bitmap, int xSize, int ySize)
            {
                Rectangle srcRect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                AdjustSizes(bitmap, ref xSize, ref ySize);
                Bitmap scaledBitmap = new Bitmap(xSize, ySize, bitmap.PixelFormat);
                using (Graphics g = Graphics.FromImage(scaledBitmap))
                {
                    Rectangle destRect = new Rectangle(0, 0, xSize, ySize);

                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.CompositingMode = CompositingMode.SourceCopy;
                    g.PixelOffsetMode = PixelOffsetMode.Half;

                    ImageAttributes ia = new ImageAttributes();
                    ia.SetWrapMode(WrapMode.TileFlipXY); // prevents single-pixel darkness at edges

                    using (new QuickTimer("Resize"))
                        g.DrawImage(bitmap, new PointF[]
                                                {
                                                    new PointF(destRect.Left, destRect.Top),
                                                    new PointF(destRect.Right, destRect.Top),
                                                    new PointF(destRect.Left, destRect.Bottom),
                                                }, srcRect, GraphicsUnit.Pixel, ia);
                }
                return scaledBitmap;
            }

            private static void DoDrawImage(Graphics g, Bitmap bitmap, Rectangle destRect, Rectangle srcRect)
            {
                /*
                                using (bitmap = bitmap.Clone(srcRect, bitmap.PixelFormat))
                                    using (TextureBrush brush = new TextureBrush(bitmap, WrapMode.Tile, new Rectangle(0, 0, bitmap.Width, bitmap.Height)))
                                        g.FillRectangle(brush, destRect);
                */

                g.DrawImage(bitmap, destRect, srcRect, GraphicsUnit.Pixel);
            }

            static Bitmap StandardResize(Bitmap bitmap, int xSize, int ySize)
            {
                AdjustSizes(bitmap, ref xSize, ref ySize);
                return new Bitmap(bitmap, xSize, ySize);
            }

            public static Size ScaledResize(Bitmap bitmap, Size maxSize)
            {
                double width = bitmap.Width;
                double height = bitmap.Height;

                if (width > maxSize.Width)
                {
                    double d = maxSize.Width / (double)bitmap.Width;
                    width = maxSize.Width;
                    height = (int)(height * d);
                }

                if (height > maxSize.Height)
                {
                    double d = maxSize.Height / (double)height;
                    height = maxSize.Height;
                    width = (int)(width * d);
                }

                return new Size((int)Math.Max(width, 1), (int)Math.Max(height, 1));
            }

            public static Bitmap ResizeBitmap(Bitmap bitmap, int xSize, int ySize, ImageFormat format)
            {
                if (IsIndexedWithTransparency(bitmap, format))
                {
                    try
                    {
                        return IndexedResize(bitmap, xSize, ySize, format);
                    }
                    catch (Exception e)
                    {
                        //Warning:  aborted execution of IndexedResize has been seen to cause
                        //"object in use" errors downstream when using the source bitmap.
                        //Avoid the use of this method for this image!
                        Trace.Fail("An error while using the indexed image resize algorithm", e.ToString());
                    }
                }

                switch (bitmap.PixelFormat)
                {
                    case PixelFormat.Format24bppRgb:
                    case PixelFormat.Format32bppArgb:
                    case PixelFormat.Format32bppPArgb:
                    case PixelFormat.Format32bppRgb:
                        return RGBResize(bitmap, xSize, ySize);
                    default:
                        return StandardResize(bitmap, xSize, ySize);
                }
            }
        }

        /// <summary>
        /// Use indexed resize if we are dealing with a transparent, indexed format.
        /// </summary>
        private static bool IsIndexedWithTransparency(Bitmap bitmap, ImageFormat targetFormat)
        {
            if ((bitmap.PixelFormat & PixelFormat.Indexed) != PixelFormat.Indexed)
                return false;

            // Bug 580406: This doesn't work with our indexed resize algorithm
            if (bitmap.PixelFormat == PixelFormat.Format1bppIndexed)
                return false;

            //avoid bug 449902 - Writer will crash when trying to resize an indexed PNG
            if (targetFormat.Equals(ImageFormat.Png))
                return false;

            foreach (Color c in bitmap.Palette.Entries)
                if (c.A == 0)
                    return true;

            return false;
        }

        /// <summary>
        /// Utility class for managing image manipulation.
        ///
        ///Based on code obtained from:
        /// http://www.c-sharpcorner.com/Code/2003/March/ThumbnailImages.asp
        ///
        /// LICENSE: http://www.c-sharpcorner.com/terms.asp
        ///
        /// </summary>
        internal class ImageSaver
        {
            Bitmap bitmap;

            private ImageSaver(Bitmap image)
            {
                bitmap = image;
            }

            public static void SaveImage(Bitmap bitmap, ImageFormat format, Stream stream)
            {
                ImageSaver saver = new ImageSaver(bitmap);
                if (format == ImageFormat.Jpeg)
                {
                    saver.SaveJpeg(stream, JPEG_QUALITY, format);
                }
                else
                {
                    saver.Save(stream, format);
                }
            }

            void Save(Stream stream, ImageFormat format)
            {
                if (format == ImageFormat.Gif)
                {
                    if ((bitmap.PixelFormat & PixelFormat.Indexed) != PixelFormat.Indexed && bitmap.Palette.Entries.Length == 0 && !ImageHelper2.IsAnimated(bitmap))
                    {
                        OctreeQuantizer quantizer = new OctreeQuantizer(255, 8);
                        bitmap = quantizer.Quantize(bitmap);
                    }

                    /*
                    ColorPalette p = bitmap.Palette;
                    bool seenTransparent = false;
                    foreach (Color c in p.Entries)
                    {
                        if (c.A == 0)
                        {
                            Trace.Assert(!seenTransparent);
                            seenTransparent = true;
                        }
                    }
                    */
                }

                bitmap.Save(stream, format);
            }

            void SaveJpeg(Stream stream, long jQuality, ImageFormat format)
            {
                ImageCodecInfo jpegCodecInfo = GetEncoderInfo("image/jpeg");
                Encoder qualityEncoder = Encoder.Quality;
                EncoderParameters encoderParams = new EncoderParameters(1);
                EncoderParameter qualityEncoderParam = new EncoderParameter(qualityEncoder, jQuality);
                encoderParams.Param[0] = qualityEncoderParam;
                bitmap.Save(stream, jpegCodecInfo, encoderParams);
            }

            ImageCodecInfo GetEncoderInfo(String mimeType)
            {
                int j;
                ImageCodecInfo[] encoders;
                encoders = ImageCodecInfo.GetImageEncoders();
                for (j = 0; j < encoders.Length; ++j)
                {
                    if (encoders[j].MimeType.ToUpper(CultureInfo.InvariantCulture) == mimeType.ToUpper(CultureInfo.InvariantCulture))
                        return encoders[j];
                }
                return null;
            }
        }

        /// <summary>
        /// Utility class for managing image manipulation.
        ///
        /// Code obtained from:
        /// http://www.c-sharpcorner.com/Code/2003/March/ThumbnailImages.asp
        ///
        /// LICENSE: http://www.c-sharpcorner.com/terms.asp
        ///
        /// </summary>
        internal class ThumbMaker
        {
            Bitmap scaledBitmap, bitmap;
            ImageFormat targetImageFormat;

            internal ThumbMaker(Bitmap image, ImageFormat targetImageFormat)
            {
                this.bitmap = image;
                this.targetImageFormat = targetImageFormat;
            }

            void Resize(int xSize, int ySize)
            {
                scaledBitmap = ImageResizer.ResizeBitmap(bitmap, xSize, ySize, targetImageFormat);
            }

            void Save(string fileName, ImageFormat format)
            {
                scaledBitmap.Save(fileName, format);
            }

            void Save(string fileName, long jQuality, ImageFormat format)
            {
                ImageCodecInfo jpegCodecInfo = GetEncoderInfo("image/jpeg");
                Encoder qualityEncoder = Encoder.Quality;
                EncoderParameters encoderParams = new EncoderParameters(1);
                EncoderParameter qualityEncoderParam = new EncoderParameter(qualityEncoder, jQuality);
                encoderParams.Param[0] = qualityEncoderParam;
                scaledBitmap.Save(fileName, jpegCodecInfo, encoderParams);
            }

            void Save(Stream stream, ImageFormat format)
            {
                scaledBitmap.Save(stream, format);
            }

            void Save(Stream stream, long jQuality, ImageFormat format)
            {
                ImageCodecInfo jpegCodecInfo = GetEncoderInfo("image/jpeg");
                Encoder qualityEncoder = Encoder.Quality;
                EncoderParameters encoderParams = new EncoderParameters(1);
                EncoderParameter qualityEncoderParam = new EncoderParameter(qualityEncoder, jQuality);
                encoderParams.Param[0] = qualityEncoderParam;
                scaledBitmap.Save(stream, jpegCodecInfo, encoderParams);
            }

            ImageCodecInfo GetEncoderInfo(String mimeType)
            {
                int j;
                ImageCodecInfo[] encoders;
                encoders = ImageCodecInfo.GetImageEncoders();
                for (j = 0; j < encoders.Length; ++j)
                {
                    if (encoders[j].MimeType.ToUpper(CultureInfo.InvariantCulture) == mimeType.ToUpper(CultureInfo.InvariantCulture))
                        return encoders[j];
                }
                return null;
            }

            internal void ResizeToJpeg(int xSize, int ySize, string fileName, ImageFilter imageFilter)
            {
                Resize(xSize, ySize);
                ApplyFilter(imageFilter);
                this.Save(fileName, ImageFormat.Jpeg);
            }

            internal void ResizeToJpeg(int xSize, int ySize, Stream stream, ImageFilter imageFilter)
            {
                Resize(xSize, ySize);
                ApplyFilter(imageFilter);
                this.Save(stream, ImageFormat.Jpeg);
            }

            internal void ResizeToJpeg(int xSize, int ySize, long jQuality, string fileName, ImageFilter imageFilter)
            {
                Resize(xSize, ySize);
                ApplyFilter(imageFilter);
                this.Save(fileName, jQuality, ImageFormat.Jpeg);
            }

            internal void ResizeToJpeg(int xSize, int ySize, long jQuality, Stream stream, ImageFilter imageFilter)
            {
                Resize(xSize, ySize);
                ApplyFilter(imageFilter);
                this.Save(stream, jQuality, ImageFormat.Jpeg);
            }

            internal void ResizeToBmp(int xSize, int ySize, Stream stream, ImageFilter imageFilter)
            {
                Resize(xSize, ySize);
                ApplyFilter(imageFilter);
                this.Save(stream, ImageFormat.Bmp);
            }

            internal void ResizeToGif(int xSize, int ySize, string fileName, ImageFilter imageFilter)
            {
                Resize(xSize, ySize);
                ApplyFilter(imageFilter);
                this.Save(fileName, ImageFormat.Gif);
            }

            internal void ResizeToGif(int xSize, int ySize, Stream stream, ImageFilter imageFilter)
            {
                Resize(xSize, ySize);
                ApplyFilter(imageFilter);
                this.Save(stream, ImageFormat.Gif);
            }

            internal void ResizeToPng(int xSize, int ySize, string fileName, ImageFilter imageFilter)
            {
                Resize(xSize, ySize);
                ApplyFilter(imageFilter);
                this.Save(fileName, ImageFormat.Png);
            }

            internal void ResizeToPng(int xSize, int ySize, Stream stream, ImageFilter imageFilter)
            {
                Resize(xSize, ySize);
                ApplyFilter(imageFilter);
                this.Save(stream, ImageFormat.Png);
            }

            private void ApplyFilter(ImageFilter imageFilter)
            {
                if (imageFilter != null)
                    scaledBitmap = imageFilter(scaledBitmap);
            }
        }

        public static void GetImageFormat(string srcFileName, out string extension, out ImageFormat imageFormat)
        {
            extension = Path.GetExtension(srcFileName).ToLower(CultureInfo.InvariantCulture);
            if (extension == ".jpg" || extension == ".jpeg")
            {
                imageFormat = ImageFormat.Jpeg;
                extension = ".jpg";
            }
            else if (extension == ".gif")
            {
                imageFormat = ImageFormat.Gif;
            }
            else
            {
                imageFormat = ImageFormat.Png;
                extension = ".png";
            }
        }

        public static Bitmap CropBitmap(Bitmap bitmap, Rectangle rectangle)
        {
            Bitmap newBitmap = new Bitmap(rectangle.Width, rectangle.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(newBitmap))
            {
                g.DrawImage(bitmap, new Rectangle(0, 0, newBitmap.Width, newBitmap.Height), rectangle, GraphicsUnit.Pixel);
            }
            return newBitmap;
        }

        public static ImageClassification Classify(string path)
        {
            if (!path.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
                return ImageClassification.Normal;

            using (new QuickTimer("Classify image"))
            {
                using (Bitmap bitmap = (Bitmap)Image.FromFile(path))
                {
                    if (!bitmap.RawFormat.Equals(ImageFormat.Gif))
                        return ImageClassification.Normal;

                    ImageClassification result = ImageClassification.Normal;
                    if (IsAnimated(bitmap))
                        result |= ImageClassification.AnimatedGif;
                    if (IsIndexedWithTransparency(bitmap, ImageFormat.Gif))
                        result |= ImageClassification.TransparentGif;

                    return result;
                }
            }
        }
    }

    [Flags]
    public enum ImageClassification
    {
        Normal = 0,
        TransparentGif = 1,
        AnimatedGif = 2
    };
}
