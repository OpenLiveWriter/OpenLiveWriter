// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using mshtml;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.Mshtml
{
    /// <summary>
    /// Interface used for customizing editing behavior of MSHTML
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("3050f6a6-98b5-11cf-bb82-00aa00bdce0b")]
    public interface IHTMLPainterRaw
    {
        void Draw(
            RECT rcBounds,
            RECT rcUpdate,
            int lDrawFlags,
            IntPtr hdc,
            IntPtr pvDrawObject);

        void OnResize(
            SIZE size);

        void GetPainterInfo(
            ref _HTML_PAINTER_INFO pInfo);

        [PreserveSig]
        int HitTestPoint(
            POINT pt,
            ref bool pbHit,
            ref int plPartID);
    }

    [Flags]
    public enum HTML_PAINTER : int
    {
        OPAQUE = 0x1,
        TRANSPARENT = 0x2,
        ALPHA = 0x4,
        COMPLEX = 0x8,
        OVERLAY = 0x10,
        HITTEST = 0x20,
        SURFACE = 0x100,
        THREEDSURFACE = 0x200,
        NOBAND = 0x400,
        NODC = 0x1000,
        NOPHYSICALCLIP = 0x2000,
        NOSAVEDC = 0x4000,
        SUPPORTS_XFORM = 0x8000,
        EXPAND = 0x10000,
        NOSCROLLBITS = 0x20000,
    };
}

