// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 1), Guid("3050F663-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface IHTMLEditServices
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void AddDesigner([In, MarshalAs(UnmanagedType.Interface)] IHTMLEditDesigner pIDesigner);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RemoveDesigner([In, MarshalAs(UnmanagedType.Interface)] IHTMLEditDesigner pIDesigner);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSelectionServices([In, MarshalAs(UnmanagedType.Interface)] IMarkupContainer pIContainer, [MarshalAs(UnmanagedType.Interface)] out ISelectionServices ppSelSvc);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void MoveToSelectionAnchor([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pIStartAnchor);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void MoveToSelectionEnd([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pIEndAnchor);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SelectRange([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pStart, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pEnd, [In] _SELECTION_TYPE eType);
    }
}

