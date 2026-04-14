// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Summary description for DIBHelper.
    /// </summary>
    public class DIBHelper
    {

        public static void DibToFile(Stream stream, string filePath)
        {
            // Read the DIB into a Byte array and pin it
            // This is necessary because some of the unmanaged calls
            // need a pointer to the Dib bytes.
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, (int)stream.Length);

            string imagePath = string.Empty;
            GCHandle gcHandle = new GCHandle();
            try
            {
                gcHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

                // Reset the stream position since it was read into the byte array
                // Get the bitmapInfoHeader so we can do some calculations
                stream.Position = 0;
                BITMAPINFOHEADER bmpInfoHeader = GetBitMapInfoHeader(stream);
                if ((bmpInfoHeader.biClrUsed == 0) && (bmpInfoHeader.biBitCount < 16))
                    bmpInfoHeader.biClrUsed = 1 << bmpInfoHeader.biBitCount;

                // Get IntPtrs for the DIB itself as well as for the actual pixels in the DIB
                IntPtr dibPtr = Marshal.UnsafeAddrOfPinnedArrayElement(bytes, 0);
                IntPtr pixPtr = new IntPtr((int)dibPtr + bmpInfoHeader.biSize + (bmpInfoHeader.biClrUsed * 4));

                // Get a GDI Bitmap
                IntPtr img = IntPtr.Zero;

                try
                {
                    int st = GdiPlus.GdipCreateBitmapFromGdiDib(dibPtr, pixPtr, ref img);

                    // Couldn't create bitmap, return null and log
                    if ((st != 0) || (img == IntPtr.Zero))
                    {
                        throw new DIBHelperException("Couldn't get DIB IntPtr");
                    }

                    // Write the bitmap to a file of the specified type
                    Guid clsid = GetCodecClsid(filePath);
                    st = GdiPlus.GdipSaveImageToFile(img, filePath, ref clsid, IntPtr.Zero);
                    if (st != 0)
                    {
                        throw new DIBHelperException("Couldn't write Dib to File");
                    }
                }
                finally
                {
                    // Dispose of resources
                    GdiPlus.GdipDisposeImage(img);
                }
            }
            finally
            {
                gcHandle.Free();
            }
        }

        /// <summary>
        /// Returns a codec for a given file extension.  Note that this requires that the
        /// extension include the '.'
        /// </summary>
        /// <param name="ext">The extension of the file type</param>
        /// <returns>The clsid of the codec for encoding this file type</returns>
        private static Guid GetCodecClsid(string path)
        {
            string ext = Path.GetExtension(path);
            Guid clsid = Guid.Empty;
            ext = "*" + ext;
            foreach (ImageCodecInfo codec in m_codecs)
            {
                if (codec.FilenameExtension.IndexOf(ext, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    clsid = codec.Clsid;
                    break;
                }
            }
            return clsid;
        }

        /// <summary>
        /// Gets a bitmapheaderinfo struct from a stream containing a dib
        /// </summary>
        /// <param name="stream">The stream to the dib</param>
        /// <returns>The bitmapheaderinfo</returns>
        private static BITMAPINFOHEADER GetBitMapInfoHeader(Stream stream)
        {
            // Get the Bitmap Info Header
            BinaryReader reader = new BinaryReader(stream);

            BITMAPINFOHEADER bmpInfoHeader = new BITMAPINFOHEADER();
            bmpInfoHeader.biSize = reader.ReadInt32();
            bmpInfoHeader.biWidth = reader.ReadInt32();
            bmpInfoHeader.biHeight = reader.ReadInt32();
            bmpInfoHeader.biPlanes = reader.ReadInt16();
            bmpInfoHeader.biBitCount = reader.ReadInt16();
            bmpInfoHeader.biCompression = reader.ReadInt32();
            bmpInfoHeader.biSizeImage = reader.ReadInt32();
            bmpInfoHeader.biXPelsPerMeter = reader.ReadInt32();
            bmpInfoHeader.biYPelsPerMeter = reader.ReadInt32();
            bmpInfoHeader.biClrUsed = reader.ReadInt32();
            bmpInfoHeader.biClrImportant = reader.ReadInt32();

            return bmpInfoHeader;
        }

        /// <summary>
        /// The bitmapheaderinfo struct (used for in memory device independent bitmaps)
        /// </summary>
        private struct BITMAPINFOHEADER
        {
            public int biSize;
            public int biWidth;
            public int biHeight;
            public short biPlanes;
            public short biBitCount;
            public int biCompression;
            public int biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public int biClrUsed;
            public int biClrImportant;
        }

        /// <summary>
        /// Supported codecs
        /// </summary>
        private static ImageCodecInfo[] m_codecs = ImageCodecInfo.GetImageEncoders();

    }

    /// <summary>
    /// Exception thrown by the DIBHelper
    /// </summary>
    public class DIBHelperException : Exception
    {
        public DIBHelperException() : base()
        {
        }

        public DIBHelperException(string message) : base(message)
        {
        }

        public DIBHelperException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
