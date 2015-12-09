// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, TypeLibType((short) 0x1040), Guid("3050F809-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface IHTMLDOMTextNode2
    {
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3ec)]
        string substringData([In] int offset, [In] int Count);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3ed)]
        void appendData([In, MarshalAs(UnmanagedType.BStr)] string bstrstring);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3ee)]
        void insertData([In] int offset, [In, MarshalAs(UnmanagedType.BStr)] string bstrstring);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3ef)]
        void deleteData([In] int offset, [In] int Count);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3f0)]
        void replaceData([In] int offset, [In] int Count, [In, MarshalAs(UnmanagedType.BStr)] string bstrstring);
    }
}

