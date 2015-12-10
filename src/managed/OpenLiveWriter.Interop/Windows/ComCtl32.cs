// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Windows
{
    /// <summary>
    /// Imports from ComCtl32.dll
    /// </summary>
    public class ComCtl32
    {
        /// <summary>
        /// Creates a toolbar window and adds the specified buttons to the toolbar
        /// </summary>
        [DllImport("ComCtl32.dll")]
        public static extern IntPtr CreateToolbarEx(
            IntPtr hParent, uint ws, UIntPtr wID, int nBitmaps, IntPtr hBMInst,
            UIntPtr wBMID, IntPtr lpButtons, int iNumButtons, int dxButton,
            int dyButton, int dxBitmap, int dyBitmap, UIntPtr uStructSize);
    }

    /// <summary>
    /// Constants for toolbar styles
    /// </summary>
    public struct TBSTYLE
    {
        public const UInt32 TRANSPARENT = 0x8000;
        public const UInt32 FLAT = 0x0800;
        public const UInt32 LIST = 0x1000;
        public const UInt32 TOOLTIPS = 0x0100;
        public const UInt32 WRAPABLE = 0x0200;
    }

    public struct BUTTON_IMAGELIST_ALIGN
    {
        public const uint LEFT = 0;
        public const uint RIGHT = 1;
        public const uint TOP = 2;
        public const uint BOTTOM = 3;
        public const uint CENTER = 4;       // Doesn't draw text
    }

    /// <summary>
    /// Constants for common control styles
    /// </summary>
    public struct CCS
    {
        public const UInt32 TOP = 0x00000001;
        public const UInt32 NORESIZE = 0x00000004;
        public const UInt32 NODIVIDER = 0x00000040;
        public const UInt32 NOPARENTALIGN = 0x00000008;
    }

}
