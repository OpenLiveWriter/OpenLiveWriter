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

    [ComImport, InterfaceType((short) 2), TypeLibType((short) 0x1010), DefaultMember("item"), Guid("3050F56C-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface DispHTMLAttributeCollection
    {
        [DispId(0x5dc)]
        int length { [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5dc)] get; }
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler))]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 0x41), DispId(-4)]
        IEnumerator GetEnumerator();
        [return: MarshalAs(UnmanagedType.IDispatch)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0)]
        object item([In, Optional, MarshalAs(UnmanagedType.Struct)] ref object name);
        [return: MarshalAs(UnmanagedType.Interface)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5dd)]
        IHTMLDOMAttribute getNamedItem([In, MarshalAs(UnmanagedType.BStr)] string bstrName);
        [return: MarshalAs(UnmanagedType.Interface)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5de)]
        IHTMLDOMAttribute setNamedItem([In, MarshalAs(UnmanagedType.Interface)] IHTMLDOMAttribute ppNode);
        [return: MarshalAs(UnmanagedType.Interface)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5df)]
        IHTMLDOMAttribute removeNamedItem([In, MarshalAs(UnmanagedType.BStr)] string bstrName);
    }
}

