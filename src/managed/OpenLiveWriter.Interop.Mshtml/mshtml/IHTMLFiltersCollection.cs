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

    [ComImport, DefaultMember("item"), Guid("3050F3EE-98B5-11CF-BB82-00AA00BDCE0B"), TypeLibType((short)0x1040)]
    public interface IHTMLFiltersCollection : IEnumerable
    {
        [DispId(0x3e9)]
        int length {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3e9)] get; }
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(EnumeratorToEnumVariantMarshaler))]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-4), TypeLibFunc((short)0x41)]
        new IEnumerator GetEnumerator();
        [return: MarshalAs(UnmanagedType.Struct)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0)]
        object item([In, MarshalAs(UnmanagedType.Struct)] ref object pvarIndex);
    }
}

