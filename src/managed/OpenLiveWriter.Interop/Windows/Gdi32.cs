// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Windows
{

    /// <summary>
    /// Imports from Gdi32.dll
    /// </summary>
    public class Gdi32
    {
        [DllImport("gdi32.dll")]
        public static extern uint GetBoundsRect(IntPtr hdc, out RECT lprcBounds, uint flags);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateSolidBrush(uint crColor);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreatePatternBrush(IntPtr hbmp);

        [DllImport("gdi32.dll")]
        public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        [DllImport("Gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(
            IntPtr hdc,        // handle to DC
            int nWidth,     // width of bitmap, in pixels
            int nHeight     // height of bitmap, in pixels
            );

        [DllImport("Gdi32.dll", SetLastError = true)]
        public static extern IntPtr CreateCompatibleDC(
            IntPtr hdc   // handle to DC
            );

        [DllImport("Gdi32.dll", SetLastError = true)]
        public static extern bool DeleteDC(IntPtr hdc);

        [DllImport("Gdi32.dll", SetLastError = true)]
        public static extern IntPtr SelectObject(
            IntPtr hdc,          // handle to DC
            IntPtr hgdiobj   // handle to object
            );

        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        public static extern int GetObject(IntPtr hObject, int cbBuffer, IntPtr lpvObject);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern int SetBkMode(IntPtr hdc, int bkMode);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern int SetBkColor(IntPtr hdc, int crColor);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern int SetTextColor(IntPtr hdc, int crColor);

        public static int ToColorRef(Color color)
        {
            return (color.B << 16) | (color.G << 8) | color.R;
        }

        [DllImport("Gdi32.dll", SetLastError = true)]
        public static extern bool DeleteObject(IntPtr hObject);  // handle to object

        [DllImport("Gdi32.dll")]
        public static extern IntPtr CreateDC(
            string lpszDriver,        // driver name
            IntPtr lpszDevice,        // device name
            IntPtr lpszOutput,        // not used; should be NULL
            IntPtr lpInitData  // optional printer data
                               );

        [DllImport("Gdi32.dll")]
        public static extern int BitBlt(
            IntPtr hdcDest, // handle to destination DC
            int nXDest,  // x-coord of destination upper-left corner
            int nYDest,  // y-coord of destination upper-left corner
            int nWidth,  // width of destination rectangle
            int nHeight, // height of destination rectangle
            IntPtr hdcSrc,  // handle to source DC
            int nXSrc,   // x-coordinate of source upper-left corner
            int nYSrc,   // y-coordinate of source upper-left corner
            TernaryRasterOperations dwRop  // raster operation code
            );

        public enum TernaryRasterOperations : uint
        {
            SRCCOPY = 0x00CC0020,
            SRCPAINT = 0x00EE0086,
            SRCAND = 0x008800C6,
            SRCINVERT = 0x00660046,
            SRCERASE = 0x00440328,
            NOTSRCCOPY = 0x00330008,
            NOTSRCERASE = 0x001100A6,
            MERGECOPY = 0x00C000CA,
            MERGEPAINT = 0x00BB0226,
            PATCOPY = 0x00F00021,
            PATPAINT = 0x00FB0A09,
            PATINVERT = 0x005A0049,
            DSTINVERT = 0x00550009,
            BLACKNESS = 0x00000042,
            WHITENESS = 0x00FF0062
        }

        [DllImport("gdi32.dll")]
        public static extern GraphicsMode SetGraphicsMode(IntPtr hdc, GraphicsMode iMode);

        public enum GraphicsMode : int
        {
            Compatible = 1,
            Advanced = 2
        }

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern bool SetWorldTransform(IntPtr hdc, [In] ref XFORM lpXform);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern bool GetWorldTransform(IntPtr hdc, [Out] out XFORM lpXform);

        public struct XFORM
        {
            public float eM11;
            public float eM12;
            public float eM21;
            public float eM22;
            public float eDx;
            public float eDy;
        }

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern uint GetLayout(IntPtr hdc);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern bool GetWindowOrgEx(IntPtr hdc, out POINT lpPoint);

        [DllImport("gdi32.dll")]
        public static extern bool RoundRect(IntPtr hdc, int nLeftRect, int nTopRect,
            int nRightRect, int nBottomRect, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateRoundRectRgn(
            int nLeftRect,      // x-coordinate of upper-left corner
            int nTopRect,       // y-coordinate of upper-left corner
            int nRightRect,     // x-coordinate of lower-right corner
            int nBottomRect,    // y-coordinate of lower-right corner
            int nWidthEllipse,  // height of ellipse
            int nHeightEllipse  // width of ellipse
            );

        [DllImport("gdi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool GetTextExtentPoint32(
            IntPtr hdc,
            [MarshalAs(UnmanagedType.LPTStr)] string lpString,
            int cbString,
            out SIZE lpSize
            );

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern bool LPtoDP(
            IntPtr hdc,
            POINT[] lpPoints,
            int nCounts);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct SIZEL
    {
        public int cx;
        public int cy;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct POINTL
    {
        public int x;
        public int y;
    }

    public struct DEVICECAPS
    {
        public const int LOGPIXELSX = 88;
        public const int LOGPIXELSY = 90;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct BITMAP
    {
        public int bmType;
        public int bmWidth;
        public int bmHeight;
        public int bmWidthBytes;
        public short bmPlanes;
        public short bmBitsPixel;
        public int bmBits;
    }
}
