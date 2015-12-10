// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Summary description for IconHelper.
    /// </summary>
    public class IconHelper
    {
        public static Bitmap GetBitmapForIcon(Icon icon)
        {
            using (IconInfo iconInfo = new IconInfo(icon))
            {
                if (!iconInfo.Info.fIcon)
                    return null; // this is a cursor!

                if (iconInfo.BitmapStruct.bmBitsPixel!=32)
                    return Bitmap.FromHicon(icon.Handle);  // no alpha blending, so use regular .Net

                Bitmap iconBitmap = Bitmap.FromHbitmap(iconInfo.Info.hbmColor);

                // get broken bitmap (broken - pixelformat doesn't specify alpha channel, even though the alpha data is there),
                // copy to a new bitmap with a healthy (includes the alpha channel) pixelformat
                Bitmap transparentBitmap;
                using (LockedBitMap lockedBitmap = new LockedBitMap(iconBitmap))
                {
                    transparentBitmap = new Bitmap(lockedBitmap.Data.Width, lockedBitmap.Data.Height, lockedBitmap.Data.Stride, PixelFormat.Format32bppArgb, lockedBitmap.Data.Scan0);
                }
                return transparentBitmap;
            }
        }

        private class LockedBitMap : IDisposable
        {
            public LockedBitMap(Bitmap bitmapToLock)
            {
                _bitmapToLock = bitmapToLock;
                _bitmapData = bitmapToLock.LockBits(new Rectangle(0, 0, bitmapToLock.Width, bitmapToLock.Height),
                    ImageLockMode.ReadOnly, bitmapToLock.PixelFormat);

            }
            private BitmapData _bitmapData;
            private Bitmap _bitmapToLock;

            public BitmapData Data
            {
                get
                {
                    return _bitmapData;
                }
            }

            public void Dispose()
            {
                _bitmapToLock.UnlockBits(_bitmapData);
            }

        }

        private class IconInfo : IDisposable
        {
            public IconInfo(Icon icon)
            {
                User32.GetIconInfo(icon.Handle, out _iconInfo);

                int size = Marshal.SizeOf(typeof(BITMAP));

                IntPtr ptr = Marshal.AllocCoTaskMem(size);
                try
                {
                    Gdi32.GetObject(Info.hbmColor, size , ptr);
                    _bitmapStruct = (BITMAP)Marshal.PtrToStructure(ptr, typeof(BITMAP));
                }
                finally
                {
                    Marshal.FreeCoTaskMem(ptr);
                }
            }

            public User32.ICONINFO Info
            {
                get
                {
                    return _iconInfo;
                }
            }

            public BITMAP BitmapStruct
            {
                get
                {
                    return _bitmapStruct;
                }
            }
            private BITMAP _bitmapStruct;

            public void Dispose()
            {
                Gdi32.DeleteObject(_iconInfo.hbmColor);
                Gdi32.DeleteObject(_iconInfo.hbmMask);
            }

            private User32.ICONINFO _iconInfo;

        }
    }
}
