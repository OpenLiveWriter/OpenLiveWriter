// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.IO;
using System.Globalization;
using System.Drawing.Drawing2D;

namespace OpenLiveWriter.CoreServices
{
    public class ImageCache : IDisposable
    {
        private Dictionary<string, Bitmap> imageCache;

        public ImageCache()
        {
            imageCache = new Dictionary<string, Bitmap>();
        }

        public Bitmap Load(string path, Size size)
        {
            if (path == null)
                return null;

            if (UrlHelper.IsUrl(path))
            {
                return null;
            }

            if (imageCache.ContainsKey(path))
                return imageCache[path];

            using (Image img = ImageHelper2.SafeGetEmbeddedThumbnail(path) ?? ImageHelper2.SafeFromFile(path))
            {
                if (img == null)
                {
                    imageCache.Add(path, null);
                }
                else
                {
                    ImageHelper.AutoRotateFromExifOrientation(img);
                    imageCache.Add(path, ResizeImage((Bitmap)img, size.Width, size.Height));
                }
            }
            return imageCache[path];
        }

        private static Bitmap ResizeImage(Bitmap image, int maxWidth, int maxHeight)
        {
            Size resize = ImageHelper2.ImageResizer.ScaledResize(image, new Size(maxWidth, maxHeight));

            return ImageHelper2.CreateResizedBitmap(image, resize.Width, resize.Height, ImageFormat.Jpeg);
        }

        public void Dispose()
        {
            foreach (KeyValuePair<string, Bitmap> pair in imageCache)
            {
                if (pair.Value != null)
                    pair.Value.Dispose();
            }
            imageCache.Clear();
        }
    }
}
