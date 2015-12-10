// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Extensibility.ImageEditing;
using OpenLiveWriter.PostEditor;
using OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    public static class ImageDecoratorDirective
    {
        [ThreadStatic]
        private static int suppressLinked;

        /// <summary>
        /// Suppresses updates on ImageEmbedType.Linked images.
        /// </summary>
        public static IDisposable SuppressLinked()
        {
            suppressLinked++;
            return new DelegateDisposable(delegate { suppressLinked--; });
        }

        internal static bool ShouldSuppressLinked
        {
            get { return suppressLinked > 0; }
        }
    }

    /// <summary>
    /// Summary description for ImageInsertHandler.
    /// </summary>
    public class ImageInsertHandler
    {
        public ImageInsertHandler()
        {
        }

        /// <summary>
        /// Writes a set of images based on the settings specified in a ImagePropertiesInfo object.
        /// </summary>
        /// <param name="imageInfo">the image properties</param>
        /// <param name="allowEnlargement">if true, generated images will be scaled larger than the source image (if the imageInfo sizes are larger) </param>
        public void WriteImages(ImagePropertiesInfo imageInfo, bool allowEnlargement, ImageDecoratorInvocationSource invocationSource, CreateFileCallback inlineFileCreator, CreateFileCallback linkedFileCreator, IEditorOptions clientOptions)
        {
            string inlinePrefix = imageInfo.LinkTarget == LinkTargetType.IMAGE ? "_thumb" : "";

            ImageFilter inlineFilter = ImageFilterDecoratorAdapter.CreateImageDecoratorsFilter(imageInfo, ImageEmbedType.Embedded, invocationSource, clientOptions);
            ImageFilter targetFilter = ImageFilterDecoratorAdapter.CreateImageDecoratorsFilter(imageInfo, ImageEmbedType.Linked, invocationSource, clientOptions);

            using (Bitmap inlineBitmap = new Bitmap(imageInfo.ImageSourceUri.LocalPath))
            {
                string imgPath = writeImage(inlineBitmap, imageInfo.ImageSourceUri.LocalPath, inlinePrefix, inlineFilter, inlineFileCreator);
                string inlineImgPath = new Uri(UrlHelper.CreateUrlFromPath(imgPath)).ToString();
                imageInfo.InlineImageUrl = inlineImgPath;
            }

            //Generate the link image
            //Warning! this imageInfo.LinkTarget check must be done after the inline image because the resize
            //         decorator will set the default link target for the image the first time it
            //         is applied.
            //imageInfo.LinkTarget = origLinkTarget;
            if (imageInfo.LinkTarget == LinkTargetType.IMAGE && !ImageDecoratorDirective.ShouldSuppressLinked)
            {
                using (Bitmap targetBitmap = new Bitmap(imageInfo.ImageSourceUri.LocalPath))
                {
                    string anchorPath = writeImage(targetBitmap, imageInfo.ImageSourceUri.LocalPath, "", targetFilter, linkedFileCreator);
                    string targetUrl = new Uri(UrlHelper.CreateUrlFromPath(anchorPath)).ToString();
                    imageInfo.LinkTargetUrl = targetUrl;
                }
            }
        }

        private string writeImage(Bitmap image, string srcFileName, string suffix, ImageFilter filter, CreateFileCallback createFileCallback)
        {
            string extension = Path.GetExtension(srcFileName).ToLower(CultureInfo.InvariantCulture);
            srcFileName = Path.GetFileNameWithoutExtension(srcFileName) + suffix + extension;
            try
            {
                //save the thumbnail to disk
                ImageFormat imageFormat;
                ImageHelper2.GetImageFormat(srcFileName, out extension, out imageFormat);
                string filename = createFileCallback(Path.GetFileNameWithoutExtension(srcFileName) + extension);
                try
                {
                    if (filter != null)
                        image = filter(image);

                    using (FileStream fs = new FileStream(filename, FileMode.Create))
                    {
                        ImageHelper2.SaveImage(image, imageFormat, fs);
                    }
                    return filename;
                }
                catch (Exception e)
                {
                    Trace.Fail("Failed to save image as format " + imageFormat.Guid + ": " + e.ToString());

                    //try to fall back to generating a PNG version of the image
                    imageFormat = ImageFormat.Png;
                    extension = ".png";
                    filename = createFileCallback(Path.GetFileNameWithoutExtension(srcFileName) + extension);

                    using (FileStream fs = new FileStream(filename, FileMode.Create))
                    {
                        ImageHelper2.SaveImage(image, ImageFormat.Png, fs);
                    }
                    return filename;
                }
            }
            catch (Exception e)
            {
                Trace.Fail("Error while trying to create thumbnail: " + e.Message, e.StackTrace);
                throw;
            }
        }

        public static Size WriteImageToFile(string sourceFile, int width, int height, string outputFile, bool preserveConstraints)
        {
            using (Bitmap sourceImage = new Bitmap(sourceFile))
            {
                FixImageOrientation(sourceImage);
                using (Stream imageOut = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
                {
                    ImageFormat format;
                    string fileExt;
                    ImageHelper2.GetImageFormat(sourceFile, out fileExt, out format);
                    if (preserveConstraints)
                    {
                        ImageHelper2.SaveScaledThumbnailImage(width, height, sourceImage, format, imageOut);
                    }
                    else
                    {
                        ImageHelper2.SaveThumbnailImage(width, height, sourceImage, format, imageOut);
                    }
                }
            }
            if (preserveConstraints)
            {
                using (Bitmap img = new Bitmap(outputFile))
                    return img.Size;
            }
            else
            {
                return new Size(width, height);
            }
        }

        /// <summary>
        /// Examines the exif metadata in am image and determines if the picture was rotated
        /// when it was taken. If the orientation is rotated, the image will be rotated to make it
        /// straight.
        /// </summary>
        /// <param name="image"></param>
        private static void FixImageOrientation(Bitmap image)
        {
            try
            {
                ExifOrientation orientation = ExifOrientation.Normal;
                try
                {
                    orientation = ExifMetadata.FromImage(image).Orientation;
                    if (orientation == ExifOrientation.Unknown)
                        orientation = ExifOrientation.Normal;
                }
                catch (Exception e) { Debug.Fail("Unexpected error getting image orientation", e.ToString()); }

                if (orientation == ExifOrientation.Rotate270CW)
                {
                    image.RotateFlip(RotateFlipType.Rotate90FlipXY);
                }
                else if (orientation == ExifOrientation.Rotate90CW)
                {
                    image.RotateFlip(RotateFlipType.Rotate270FlipXY);
                }
            }
            catch (Exception)
            {
                //image.PropertyItems will throw an exception if the image does not contain any exif data
            }
        }
    }
}
