// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, TypeLibType((short) 0x1040), Guid("3050F6BB-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface IHTMLNamespace
    {
        [DispId(0x3e8)]
        string name { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3e8), TypeLibFunc((short) 4)] get; }
        [DispId(0x3e9)]
        string urn { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 4), DispId(0x3e9)] get; }
        [DispId(0x3ea)]
        object tagNames { [return: MarshalAs(UnmanagedType.IDispatch)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3ea), TypeLibFunc((short) 4)] get; }
        [DispId(-2147412996)]
        object readyState { [return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412996), TypeLibFunc((short) 4)] get; }
        [DispId(-2147412087)]
        object onreadystatechange { [param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412087), TypeLibFunc((short) 20)] set; [return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412087)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3eb)]
        void doImport([In, MarshalAs(UnmanagedType.BStr)] string bstrImplementationUrl);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417605)]
        bool attachEvent([In, MarshalAs(UnmanagedType.BStr)] string @event, [In, MarshalAs(UnmanagedType.IDispatch)] object pdisp);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417604)]
        void detachEvent([In, MarshalAs(UnmanagedType.BStr)] string @event, [In, MarshalAs(UnmanagedType.IDispatch)] object pdisp);
    }
}

