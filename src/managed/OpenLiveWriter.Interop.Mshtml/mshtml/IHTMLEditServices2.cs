// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("3050F812-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceType((short) 1)]
    public interface IHTMLEditServices2 : IHTMLEditServices
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void AddDesigner([In, MarshalAs(UnmanagedType.Interface)] IHTMLEditDesigner pIDesigner);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void RemoveDesigner([In, MarshalAs(UnmanagedType.Interface)] IHTMLEditDesigner pIDesigner);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void GetSelectionServices([In, MarshalAs(UnmanagedType.Interface)] IMarkupContainer pIContainer, [MarshalAs(UnmanagedType.Interface)] out ISelectionServices ppSelSvc);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void MoveToSelectionAnchor([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pIStartAnchor);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void MoveToSelectionEnd([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pIEndAnchor);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void SelectRange([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pStart, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pEnd, [In] _SELECTION_TYPE eType);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void MoveToSelectionAnchorEx([In, MarshalAs(UnmanagedType.Interface)] IDisplayPointer pIStartAnchor);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void MoveToSelectionEndEx([In, MarshalAs(UnmanagedType.Interface)] IDisplayPointer pIEndAnchor);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void FreezeVirtualCaretPos([In] int fReCompute);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void UnFreezeVirtualCaretPos([In] int fReset);
    }
}

