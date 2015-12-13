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

    [ComImport, Guid("3050F383-98B5-11CF-BB82-00AA00BDCE0B"), TypeLibType((short) 0x1040), DefaultMember("item")]
    public interface IHTMLAreasCollection : IEnumerable
    {
        [DispId(0x5dc)]
        int length { [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5dc)] set; [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5dc)] get; }
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler))]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 0x41), DispId(-4)]
        new IEnumerator GetEnumerator();
        [return: MarshalAs(UnmanagedType.IDispatch)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0)]
        object item([In, Optional, MarshalAs(UnmanagedType.Struct)] object name, [In, Optional, MarshalAs(UnmanagedType.Struct)] object index);
        [return: MarshalAs(UnmanagedType.IDispatch)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5de)]
        object tags([In, MarshalAs(UnmanagedType.Struct)] object tagName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5df)]
        void add([In, MarshalAs(UnmanagedType.Interface)] IHTMLElement element, [In, Optional, MarshalAs(UnmanagedType.Struct)] object before);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5e0)]
        void remove([In, Optional] int index /* = -1 */);
    }
}

