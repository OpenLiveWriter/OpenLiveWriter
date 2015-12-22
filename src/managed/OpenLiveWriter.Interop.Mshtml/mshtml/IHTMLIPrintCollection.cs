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

    [ComImport, DefaultMember("item"), Guid("3050F6B5-98B5-11CF-BB82-00AA00BDCE0B"), TypeLibType((short) 0x1040)]
    public interface IHTMLIPrintCollection : IEnumerable
    {
        [DispId(0x5dd)]
        int length { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 0x40), DispId(0x5dd)] get; }
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler))]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 0x41), DispId(-4)]
        new IEnumerator GetEnumerator();
        [return: MarshalAs(UnmanagedType.IUnknown)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0)]
        object item([In] int index);
    }
}

