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

    [ComImport, TypeLibType((short) 0x1000), DefaultMember("item"), Guid("3050F839-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface IFontNames : IEnumerable
    {
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler))]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 1)]
        new IEnumerator GetEnumerator();
        [DispId(1)]
        int Count { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)] get; }
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        string item([In, MarshalAs(UnmanagedType.Struct)] ref object pvarIndex);
    }
}

