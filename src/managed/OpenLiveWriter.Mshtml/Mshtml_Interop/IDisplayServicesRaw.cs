// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

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
    [Guid("3050f69d-98b5-11cf-bb82-00aa00bdce0b")]
    public interface IDisplayServicesRaw
    {
        void CreateDisplayPointer(
            [Out] out IDisplayPointerRaw ppDispPointer);

        void TransformRect(
            [Out, In] ref RECT pRect,
            [In] _COORD_SYSTEM eSource,
            [In] _COORD_SYSTEM eDestination,
            [In] IHTMLElement pIElement);

        void TransformPoint(
            [In, Out] ref POINT pPoint,
            [In] _COORD_SYSTEM eSource,
            [In] _COORD_SYSTEM eDestination,
            [In] IHTMLElement pIElement);

        void GetCaret(
            [Out] out IHTMLCaretRaw ppCaret);

        void GetComputedStyle(
            [In] IMarkupPointerRaw pPointer,
            [Out] out IHTMLComputedStyle ppComputedStyle);

        void ScrollRectIntoView(
            [In] IHTMLElement pIElement,
            [In] RECT rect);

        void HasFlowLayout(
            [In] IHTMLElement pIElement,
            [Out, MarshalAs(UnmanagedType.Bool)] out bool pfHasFlowLayout);
    }
}

