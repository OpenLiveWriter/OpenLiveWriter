// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 1), Guid("3050F69D-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface IDisplayServices
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void CreateDisplayPointer([MarshalAs(UnmanagedType.Interface)] out IDisplayPointer ppDispPointer);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void TransformRect([In, Out] ref tagRECT pRect, [In] _COORD_SYSTEM eSource, [In] _COORD_SYSTEM eDestination, [In, MarshalAs(UnmanagedType.Interface)] IHTMLElement pIElement);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void TransformPoint([In, Out] ref tagPOINT pPoint, [In] _COORD_SYSTEM eSource, [In] _COORD_SYSTEM eDestination, [In, MarshalAs(UnmanagedType.Interface)] IHTMLElement pIElement);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetCaret([MarshalAs(UnmanagedType.Interface)] out IHTMLCaret ppCaret);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetComputedStyle([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointer, [MarshalAs(UnmanagedType.Interface)] out IHTMLComputedStyle ppComputedStyle);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ScrollRectIntoView([In, MarshalAs(UnmanagedType.Interface)] IHTMLElement pIElement, [In] tagRECT rect);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void HasFlowLayout([In, MarshalAs(UnmanagedType.Interface)] IHTMLElement pIElement, out int pfHasFlowLayout);
    }
}

