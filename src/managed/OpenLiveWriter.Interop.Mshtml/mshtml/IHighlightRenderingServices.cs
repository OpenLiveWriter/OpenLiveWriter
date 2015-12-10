// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("3050F606-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceType((short)1)]
    public interface IHighlightRenderingServices
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void AddSegment([In, MarshalAs(UnmanagedType.Interface)] IDisplayPointer pDispPointerStart, [In, MarshalAs(UnmanagedType.Interface)] IDisplayPointer pDispPointerEnd, [In, MarshalAs(UnmanagedType.Interface)] IHTMLRenderStyle pIRenderStyle, [MarshalAs(UnmanagedType.Interface)] out IHighlightSegment ppISegment);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void MoveSegmentToPointers([In, MarshalAs(UnmanagedType.Interface)] IHighlightSegment pISegment, [In, MarshalAs(UnmanagedType.Interface)] IDisplayPointer pDispPointerStart, [In, MarshalAs(UnmanagedType.Interface)] IDisplayPointer pDispPointerEnd);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void RemoveSegment([In, MarshalAs(UnmanagedType.Interface)] IHighlightSegment pISegment);
    }
}

