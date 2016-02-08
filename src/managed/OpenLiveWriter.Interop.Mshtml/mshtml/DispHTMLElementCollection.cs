// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.CustomMarshalers;

    [ComImport, Guid("3050F56B-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceType((short) 2), DefaultMember("item"), TypeLibType((short) 0x1010)]
    public interface DispHTMLElementCollection
    {
        [return: MarshalAs(UnmanagedType.BStr)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5dd)]
        string toString();
        [DispId(0x5dc)]
        int length { [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5dc)] set; [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5dc)] get; }
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler))]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 0x41), DispId(-4)]
        IEnumerator GetEnumerator();
        [return: MarshalAs(UnmanagedType.IDispatch)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0)]
        object item([In, Optional, MarshalAs(UnmanagedType.Struct)] object name, [In, Optional, MarshalAs(UnmanagedType.Struct)] object index);
        [return: MarshalAs(UnmanagedType.IDispatch)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5de)]
        object tags([In, MarshalAs(UnmanagedType.Struct)] object tagName);
        [return: MarshalAs(UnmanagedType.IDispatch)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5e1)]
        object urns([In, MarshalAs(UnmanagedType.Struct)] object urn);
        [return: MarshalAs(UnmanagedType.IDispatch)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5e2)]
        object namedItem([In, MarshalAs(UnmanagedType.BStr)] string name);
    }
}

