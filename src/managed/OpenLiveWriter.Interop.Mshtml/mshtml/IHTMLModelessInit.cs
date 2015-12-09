// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, TypeLibType((short) 0x1040), Guid("3050F5E4-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface IHTMLModelessInit
    {
        [DispId(0x61a8)]
        object parameters { [return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x61a8)] get; }
        [DispId(0x61a9)]
        object optionString { [return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x61a9)] get; }
        [DispId(0x61ae)]
        object moniker { [return: MarshalAs(UnmanagedType.IUnknown)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x61ae)] get; }
        [DispId(0x61af)]
        object document { [return: MarshalAs(UnmanagedType.IUnknown)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x61af)] get; }
    }
}

