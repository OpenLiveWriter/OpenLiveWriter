// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("3050F69E-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceType((short)1)]
    public interface IDisplayPointer
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void moveToPoint([In] tagPOINT ptPoint, [In] _COORD_SYSTEM eCoordSystem, [In, MarshalAs(UnmanagedType.Interface)] IHTMLElement pElementContext, [In] uint dwHitTestOptions, out uint pdwHitTestResults);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void MoveUnit([In] _DISPLAY_MOVEUNIT eMoveUnit, [In] int lXPos);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void PositionMarkupPointer([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pMarkupPointer);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void MoveToPointer([In, MarshalAs(UnmanagedType.Interface)] IDisplayPointer pDispPointer);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetPointerGravity([In] _POINTER_GRAVITY eGravity);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetPointerGravity(out _POINTER_GRAVITY peGravity);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetDisplayGravity([In] _DISPLAY_GRAVITY eGravity);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetDisplayGravity(out _DISPLAY_GRAVITY peGravity);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void IsPositioned(out int pfPositioned);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Unposition();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void IsEqualTo([In, MarshalAs(UnmanagedType.Interface)] IDisplayPointer pDispPointer, out int pfIsEqual);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void IsLeftOf([In, MarshalAs(UnmanagedType.Interface)] IDisplayPointer pDispPointer, out int pfIsLeftOf);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void IsRightOf([In, MarshalAs(UnmanagedType.Interface)] IDisplayPointer pDispPointer, out int pfIsRightOf);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void IsAtBOL(out int pfBOL);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void MoveToMarkupPointer([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointer, [In, MarshalAs(UnmanagedType.Interface)] IDisplayPointer pDispLineContext);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void scrollIntoView();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetLineInfo([MarshalAs(UnmanagedType.Interface)] out ILineInfo ppLineInfo);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetFlowElement([MarshalAs(UnmanagedType.Interface)] out IHTMLElement ppLayoutElement);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void QueryBreaks(out uint pdwBreaks);
    }
}

