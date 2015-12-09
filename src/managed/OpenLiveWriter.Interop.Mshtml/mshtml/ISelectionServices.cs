// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 1), Guid("3050F684-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface ISelectionServices
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetSelectionType([In] _SELECTION_TYPE eType, [In, MarshalAs(UnmanagedType.Interface)] ISelectionServicesListener pIListener);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetMarkupContainer([MarshalAs(UnmanagedType.Interface)] out IMarkupContainer ppIContainer);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void AddSegment([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pIStart, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pIEnd, [MarshalAs(UnmanagedType.Interface)] out ISegment ppISegmentAdded);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void AddElementSegment([In, MarshalAs(UnmanagedType.Interface)] IHTMLElement pIElement, [MarshalAs(UnmanagedType.Interface)] out IElementSegment ppISegmentAdded);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RemoveSegment([In, MarshalAs(UnmanagedType.Interface)] ISegment pISegment);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSelectionServicesListener([MarshalAs(UnmanagedType.Interface)] out ISelectionServicesListener ppISelectionServicesListener);
    }
}

