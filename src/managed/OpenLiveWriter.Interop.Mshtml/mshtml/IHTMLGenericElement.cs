// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, TypeLibType((short) 0x1040), Guid("3050F4B7-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface IHTMLGenericElement
    {
        [DispId(0x3e9)]
        object recordset { [return: MarshalAs(UnmanagedType.IDispatch)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 0x40), DispId(0x3e9)] get; }
        [return: MarshalAs(UnmanagedType.IDispatch)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3ea)]
        object namedRecordset([In, MarshalAs(UnmanagedType.BStr)] string dataMember, [In, Optional, MarshalAs(UnmanagedType.Struct)] ref object hierarchy);
    }
}

