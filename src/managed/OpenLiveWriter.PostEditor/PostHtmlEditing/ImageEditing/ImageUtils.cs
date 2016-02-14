// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// Summary description for ImageUtils.
    /// </summary>
    public class ImageUtils
    {
        public static Size GetScaledImageSize(int maxWidth, int maxHeight, Size size)
        {
            if (maxWidth == int.MaxValue && maxHeight == int.MaxValue)
                return size;

            if (size == Size.Empty)
                return size;

            // The dimensions of the image from which the thumbnail is needed
            float width = size.Width;
            float height = size.Height;
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
            return new Size((int)Math.Round(requestedWidth), (int)Math.Round(requestedHeight));
        }

        public static Size GetImageSize(string imagePath)
        {
            using (FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            {
                using (Image img = System.Drawing.Image.FromStream(fs, false, false))
                {
                    return img.Size;
                }
            }
        }

        public static void CopyEXIF(Image from, Image to)
        {
            // This should work most the time.  Could on Vista and below get
            // some improved results with WIC
            foreach (PropertyItem pi in from.PropertyItems)
            {
                to.SetPropertyItem(pi);
            }
        }

        public static bool IsRotated90(RotateFlipType rotation)
        {
            switch (rotation)
            {
                case RotateFlipType.Rotate90FlipNone:
                case RotateFlipType.Rotate90FlipX:
                case RotateFlipType.Rotate90FlipXY:
                case RotateFlipType.Rotate90FlipY:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Constrains the newImageSize to have the same aspect ratio as currentImageSize if aspectRatioLocked is true.
        /// </summary>
        public static Size GetConstrainedImageSize(Size newImageSize, bool aspectRatioLocked, Size targetAspectRatioSize, Size currentImageSize)
        {
            if (aspectRatioLocked)
            {
                bool widthChanged = (newImageSize.Width != currentImageSize.Width);

                if (widthChanged)
                    newImageSize.Height = ImageUtils.GetScaledImageSize(newImageSize.Width, Int32.MaxValue, targetAspectRatioSize).Height;
                else
                    newImageSize.Width = ImageUtils.GetScaledImageSize(Int32.MaxValue, newImageSize.Height, targetAspectRatioSize).Width;
            }

            return newImageSize;
        }

        /// <summary>
        /// Scales the given imageSourceSize to the imageSizeName constraints.
        /// </summary>
        public static Size ScaleImageSizeName(ImageSizeName imageSizeName, Size imageSourceSize, RotateFlipType currentRotation)
        {
            Debug.Assert(imageSizeName != ImageSizeName.Custom, "No scaling policy associated with ImageSizeName.Custom");

            Size scaledImageSize = imageSourceSize;

            if (imageSizeName != ImageSizeName.Full)
            {
                Size sizeConstraints = ImageSizeHelper.GetSizeConstraints(imageSizeName);

                if (ImageUtils.IsRotated90(currentRotation))
                {
                    scaledImageSize = ImageUtils.GetScaledImageSize(sizeConstraints.Height, sizeConstraints.Width, imageSourceSize);
                    scaledImageSize = new Size(scaledImageSize.Height, scaledImageSize.Width);
                }
                else
                {
                    scaledImageSize = ImageUtils.GetScaledImageSize(sizeConstraints.Width, sizeConstraints.Height, imageSourceSize);
                }
            }

            return scaledImageSize;
        }

        /// <summary>
        /// Examines the exif metadata in am image and determines if the picture was rotated
        /// when it was taken.
        /// </summary>
        /// <param name="image"></param>
        /// <returns>The RotateFlipType that should be applied to fix the image orientation</returns>
        public static RotateFlipType GetFixupRotateFlipFromExifOrientation(Bitmap image)
        {
            try
            {
                Orientation orientation = Orientation.Straight;
                foreach (PropertyItem pi in image.PropertyItems)
                {
                    if (pi.Id == ExifTags.Orientation)
                    {
                        orientation = (Orientation)pi.Value[0];
                    }
                }

                if (orientation == Orientation.Right)
                {
                    return RotateFlipType.Rotate90FlipXY;
                }
                else if (orientation == Orientation.Left)
                {
                    return RotateFlipType.Rotate270FlipXY;
                }
            }
            catch (Exception)
            {
                //image.PropertyItems will throw an exception if the image does not contain any exif data
            }
            return RotateFlipType.RotateNoneFlipNone;
        }

        enum Orientation
        {
            Straight = 1,
            Left = 6,
            Right = 8
        }

        class ExifTags
        {
            internal const int Orientation = 0x112;
        }

        public static Bitmap ApplyDropShadowOutside(Bitmap bitmap, Color backgroundColor, Color shadowColor, int offset, int borderWidth)
        {
            return ApplyDropShadowOutside(bitmap, new SolidBrush(backgroundColor), shadowColor, offset, borderWidth, borderWidth, borderWidth,
                                   borderWidth);
        }

        public static Bitmap ApplyDropShadowOutside(Bitmap bitmap, Brush backgroundBrush, Color shadowColor, int offset, int borderTop, int borderRight, int borderBottom, int borderLeft)
        {
            return ApplyDropShadowOutside(bitmap, backgroundBrush, shadowColor, 4, offset, borderTop, borderRight, borderBottom, borderLeft);
        }
        public static Bitmap ApplyDropShadowOutside(Bitmap bitmap, Brush backgroundBrush, Color shadowColor, int shadowRatio, int offset, int borderTop, int borderRight, int borderBottom, int borderLeft)
        {
            return ApplyDropShadowOutside(bitmap, null, backgroundBrush, shadowColor, shadowRatio, offset, borderTop, borderRight, borderBottom, borderLeft);
        }

        public static Bitmap ApplyDropShadowOutside(Bitmap bitmap, RectangleF? crop, Brush backgroundBrush, Color shadowColor, int shadowRatio, int offset, int borderTop, int borderRight, int borderBottom, int borderLeft)
        {
            RectangleF defaultCrop = new RectangleF(0, 0, bitmap.Width, bitmap.Height);
            RectangleF actualCrop = crop ?? defaultCrop;

            shadowRatio = Math.Max(shadowRatio, 1);

            // TODO: don't short circuit if a crop needs to be applied
            // Shadow ratio cannot be zero
            if (((offset | borderTop | borderRight | borderBottom | borderLeft) == 0 && actualCrop.Equals(defaultCrop)))
            {
                return new Bitmap(bitmap);
            }

            //Draw a drop shadow based on Bob Powell's text drop shadow technique, this lets GDI give us
            //"alpha blended shadows with delicious umbra and penumbra shadows".  Thanks for the tip Bob!
            //http://www.bobpowell.net/dropshadowtext.htm

            //some constants that can be used to tweak the shadow
            //const int shadowRatio = 4; //increasing this value will lighten the color of the shadow
            const int shadowMargin = 1; //the amount of background color to include on the shadow's edges

            //Make a bitmap that is used to hold the shadow rectangle. To give the rectangle soft
            //edges, we draw it at reduced size, and then scale it up to large size so that GDI will
            //blend the shadow colors together to create softened edges.
            Bitmap shadowImage = new Bitmap(Math.Max(1, Convert.ToInt32(actualCrop.Width / shadowRatio)), Math.Max(1, Convert.ToInt32(actualCrop.Height / shadowRatio)));
            using (shadowImage)
            {
                //
                //Get a graphics object for it
                Graphics g = Graphics.FromImage(shadowImage);

                //Create the shadow rectangle. Note: the outer edge of the rectangle is the color of the background so that the background color
                //will be blended into the shadow rectangle when the image is enlarged onto the real bitmap.
                g.FillRectangle(backgroundBrush, new Rectangle(0, 0, shadowImage.Width, shadowImage.Height));
                g.FillRectangle(new SolidBrush(Color.FromArgb(200, shadowColor)), new Rectangle(shadowMargin, shadowMargin, shadowImage.Width - shadowMargin, shadowImage.Height - shadowMargin));
                g.Dispose();

                //set the border color
                Color borderColor = Color.White;

                //create a version of the original bitmap that is reduced enough to make room for the dropshadow.
                Bitmap enlargedBitmap = new Bitmap(
                    Convert.ToInt32(actualCrop.Width) + offset + borderLeft + borderRight,
                    Convert.ToInt32(actualCrop.Height) + offset + borderTop + borderBottom);
                //Create a graphics object to draw on the original bitmap
                g = Graphics.FromImage(enlargedBitmap);
                using (g)
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    //draw the small shadow image onto the large bitmap image (adjusting for the offset)
                    g.FillRectangle(backgroundBrush, new Rectangle(0, 0, enlargedBitmap.Width, enlargedBitmap.Height));

                    int shadowOffset = offset - shadowMargin * shadowRatio;
                    g.DrawImage(shadowImage,
                        new Rectangle(shadowOffset, shadowOffset, enlargedBitmap.Width - shadowOffset, enlargedBitmap.Height - shadowOffset),
                        0, 0, shadowImage.Width, shadowImage.Height,
                        GraphicsUnit.Pixel);

                    if (borderTop > 0 || borderRight > 0 || borderBottom > 0 || borderLeft > 0)
                    {
                        g.FillRectangle(new SolidBrush(borderColor), new Rectangle(0, 0, enlargedBitmap.Width - offset, enlargedBitmap.Height - offset));
                        g.DrawRectangle(new Pen(Color.FromArgb(50, shadowColor), 1), new Rectangle(0, 0, enlargedBitmap.Width - offset, enlargedBitmap.Height - offset));
                    }

                    Rectangle destRect = new Rectangle(borderLeft, borderTop,
                                                       Convert.ToInt32(actualCrop.Width),
                                                       Convert.ToInt32(actualCrop.Height));

                    //draw the image on top of the bordered image
                    if (!actualCrop.Equals(defaultCrop))
                    {
                        g.DrawImage(bitmap,
                                    destRect,
                                    actualCrop.Left, actualCrop.Top, actualCrop.Width, actualCrop.Height,
                                    GraphicsUnit.Pixel);
                    }
                    else
                    {
                        g.DrawImage(bitmap, destRect.Left, destRect.Top, destRect.Width, destRect.Height);
                    }
                }
                return enlargedBitmap;
            }
        }

        public static ColorMatrix Multiply(ColorMatrix f1, ColorMatrix f2)
        {
            ColorMatrix X = new ColorMatrix();
            int size = 5;
            float[] column = new float[5];
            for (int j = 0; j < 5; j++)
            {
                for (int k = 0; k < 5; k++)
                {
                    column[k] = f1[k, j];
                }
                for (int i = 0; i < 5; i++)
                {
                    float s = 0;
                    for (int k = 0; k < size; k++)
                    {
                        s += f2[i, k] * column[k];
                    }
                    X[i, j] = s;
                }
            }
            return X;
        }

        public static ColorMatrix Transpose(ColorMatrix m)
        {
            ColorMatrix X = new ColorMatrix();
            for (int j = 0; j < 5; j++)
            {
                for (int k = 0; k < 5; k++)
                {
                    X[j, k] = m[k, j];
                }
            }
            return X;
        }
    }
}
