// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Com.Ribbon
{
    internal static class ImageLoader
    {
        //
        // Interface

        // Returns HBITMAP for stream containing 32bppBGRA bitmap resource.
        public static IntPtr LoadBitmapFromStream(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);

            // Read "BM" marker bytes.
            byte bm1 = reader.ReadByte();
            if (bm1 != 0x42) throw new FormatException("Bad BMP format.");
            byte bm2 = reader.ReadByte();
            if (bm2 != 0x4D) throw new FormatException("Bad BMP format.");

            // Read file size.
            uint filesize = reader.ReadUInt32();

            // Skip unused fields.
            reader.ReadUInt16();
            reader.ReadUInt16();

            // Read offset to start of bitmap.
            uint offset = reader.ReadUInt32();
            if (offset < 30) throw new FormatException("Bad BMP format: missing one or more required header fields.");

            // Read BitmapInfoHeader fields.
            uint biSize = reader.ReadUInt32();
            uint biWidth = reader.ReadUInt32();
            uint biHeight = reader.ReadUInt32();
            ushort biPlanes = reader.ReadUInt16();
            ushort biBitCount = reader.ReadUInt16();

            if (biPlanes != 1) throw new FormatException("Bad BMP format: only single-plane images supported.");
            if (biBitCount != 32) throw new FormatException("Bad BMP format: only 32bpp BGRA images supported.");

            // Skip reading any remaining header fields.
            const uint bytesReadSoFar = 30;
            uint skipBytes = offset - bytesReadSoFar;
            reader.ReadBytes((int)skipBytes);

            // Read remainder of source stream.
            uint imageSize = filesize - offset;
            byte[] imageBytes = reader.ReadBytes((int)imageSize);

            // Create unmanaged bitmap.
            BitmapInfoHeader bitmapInfoHeader = BitmapInfoHeader.InitSize();
            bitmapInfoHeader.biWidth = biWidth;
            bitmapInfoHeader.biHeight = biHeight;
            bitmapInfoHeader.biPlanes = biPlanes;
            bitmapInfoHeader.biBitCount = biBitCount;
            bitmapInfoHeader.biSizeImage = imageSize;

            const uint iUsage = 0; // DIB_RGB_COLORS

            IntPtr pvBits = IntPtr.Zero;
            IntPtr hBitmap = UnsafeNativeMethods.CreateDIBSection(
                IntPtr.Zero, ref bitmapInfoHeader, iUsage, out pvBits, IntPtr.Zero, 0);

            if (hBitmap == IntPtr.Zero)
                throw new System.ComponentModel.Win32Exception("CreateDIBSection() failed");

            // Copy bits into DIBSection, then return.
            Marshal.Copy(imageBytes, 0, pvBits, (int)imageSize);
            UnsafeNativeMethods.GdiFlush();

            return hBitmap;
        }

        //
        // Interop declarations

        private static class UnsafeNativeMethods
        {
            [DllImport("Gdi32.dll")]
            internal static extern IntPtr CreateDIBSection(
                IntPtr hdc,                     // optional, NULL
                [In] ref BitmapInfoHeader pbmi, // ptr to const BITMAPINFO struct
                uint iUsage,                    // we want DIB_RGB_COLORS (0)
                out IntPtr ppvBits,             // [out] ptr to callee-allocated buffer
                IntPtr hSection,                // unused, NULL
                uint dwOffset                   // unused, 0
                ); // returns: HBITMAP

            [DllImport("Gdi32.dll")]
            internal static extern void GdiFlush();
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BitmapInfoHeader
        {
            internal static BitmapInfoHeader InitSize()
            {
                BitmapInfoHeader @this = new BitmapInfoHeader();
                @this.biSize = (uint)Marshal.SizeOf(@this);
                return @this;
            }

            public uint biSize;
            public uint biWidth;
            public uint biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public uint biCompression;
            public uint biSizeImage;
            public uint biXPelsPerMeter;
            public uint biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;
        }
    }
}
