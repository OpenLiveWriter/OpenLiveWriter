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
    [Guid("3050f69e-98b5-11cf-bb82-00aa00bdce0b")]
    public interface IDisplayPointerRaw
    {
        void MoveToPoint(
            [In] POINT ptPoint,
            [In] _COORD_SYSTEM eCoordSystem,
            [In] IHTMLElement pElementContext,
            [In] uint dwHitTestOptions,
            [Out] out uint pdwHitTestResults);

        void MoveUnit(
            [In] _DISPLAY_MOVEUNIT eMoveUnit,
            [In] int lXPos);

        void PositionMarkupPointer(
            [In] IMarkupPointerRaw pMarkupPointer);

        void MoveToPointer(
            [In] IDisplayPointerRaw pDispPointer);

        void SetPointerGravity(
            [In] _POINTER_GRAVITY eGravity);

        void GetPointerGravity(
            [Out] out _POINTER_GRAVITY peGravity);

        void SetDisplayGravity(
            [In] _DISPLAY_GRAVITY eGravity);

        void GetDisplayGravity(
            [Out] out _DISPLAY_GRAVITY peGravity);

        void IsPositioned(
            [Out, MarshalAs(UnmanagedType.Bool)] out bool pfPositioned);

        void Unposition();

        void IsEqualTo(
            [In] IDisplayPointerRaw pDispPointer,
            [Out, MarshalAs(UnmanagedType.Bool)] out bool pfIsEqual);

        void IsLeftOf(
            [In] IDisplayPointerRaw pDispPointer,
            [Out, MarshalAs(UnmanagedType.Bool)] out bool pfIsLeftOf);

        void IsRightOf(
            [In] IDisplayPointerRaw pDispPointer,
            [Out, MarshalAs(UnmanagedType.Bool)] out bool pfIsRightOf);

        void IsAtBOL(
            [Out, MarshalAs(UnmanagedType.Bool)] out bool pfBOL);

        void MoveToMarkupPointer(
            [In] IMarkupPointerRaw pPointer,
            [In] IDisplayPointerRaw pDispLineContext);

        void ScrollIntoView();

        void GetLineInfo(
            [Out] out ILineInfo ppLineInfo);

        void GetFlowElement(
            [Out] out IHTMLElement ppLayoutElement);

        void QueryBreaks(
            [Out] out uint pdwBreaks);
    }
}

