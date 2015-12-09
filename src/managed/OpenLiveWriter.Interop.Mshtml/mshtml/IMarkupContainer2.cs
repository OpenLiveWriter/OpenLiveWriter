// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 1), Guid("3050F648-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface IMarkupContainer2 : IMarkupContainer
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void OwningDoc([MarshalAs(UnmanagedType.Interface)] out IHTMLDocument2 ppDoc);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void CreateChangeLog([In, MarshalAs(UnmanagedType.Interface)] IHTMLChangeSink pChangeSink, [MarshalAs(UnmanagedType.Interface)] out IHTMLChangeLog ppChangeLog, [In] int fForward, [In] int fBackward);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RegisterForDirtyRange([In, MarshalAs(UnmanagedType.Interface)] IHTMLChangeSink pChangeSink, out uint pdwCookie);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void UnRegisterForDirtyRange([In] uint dwCookie);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetAndClearDirtyRange([In] uint dwCookie, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pIPointerBegin, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pIPointerEnd);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int GetVersionNumber();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetMasterElement([MarshalAs(UnmanagedType.Interface)] out IHTMLElement ppElementMaster);
    }
}

