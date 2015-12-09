// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, TypeLibType((short) 0x1040), Guid("3050F5DC-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface IHTCDescBehavior
    {
        [DispId(-2147417612)]
        string urn { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417612), TypeLibFunc((short) 4)] get; }
        [DispId(-2147417611)]
        string name { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 4), DispId(-2147417611)] get; }
    }
}

