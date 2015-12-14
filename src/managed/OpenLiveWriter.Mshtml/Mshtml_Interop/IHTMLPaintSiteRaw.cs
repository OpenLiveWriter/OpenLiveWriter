// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using mshtml;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.Mshtml
{
    /// <summary>
    /// Interface used for querying the paint site of a rendering behavior
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("3050f6a7-98b5-11cf-bb82-00aa00bdce0b")]
    public interface IHTMLPaintSiteRaw
    {
        void InvalidatePainterInfo();

        void InvalidateRect(
            [In] ref RECT prcInvalid);

        void InvalidateRegion(
          [In] IntPtr rgnInvalid);

        void GetDrawInfo(
            [In] int lFlags,
            ref _HTML_PAINT_DRAW_INFO pDrawInfo);

        void TransformGlobalToLocal(
            [In] POINT ptGlobal,
            ref POINT pptLocal);

        void TransformLocalToGlobal(
            [In] POINT ptLocal,
            ref POINT pptGlobal);

        void GetHitTestCookie(
         ref int plCookie);
    }

    /// <summary>
    /// Interface used for querying the paint site of a rendering behavior.
    /// This is a special declaration of the interface that allows the passing
    /// of an IntPtr to InvalidateRect (which enables us to pass IntPtr.Zero
    /// requesting that the entire site be invalidated)
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("3050f6a7-98b5-11cf-bb82-00aa00bdce0b")]
    public interface IHTMLPaintSiteInvalidateAll
    {
        void InvalidatePainterInfo();

        void InvalidateRect(
            [In] IntPtr prcInvalid);

        void InvalidateRegion(
            [In] IntPtr rgnInvalid);

        void GetDrawInfo(
            [In] int lFlags,
            ref _HTML_PAINT_DRAW_INFO pDrawInfo);

        void TransformGlobalToLocal(
            [In] POINT ptGlobal,
            ref POINT pptLocal);

        void TransformLocalToGlobal(
            [In] POINT ptLocal,
            ref POINT pptGlobal);

        void GetHitTestCookie(
            ref int plCookie);
    }
}

