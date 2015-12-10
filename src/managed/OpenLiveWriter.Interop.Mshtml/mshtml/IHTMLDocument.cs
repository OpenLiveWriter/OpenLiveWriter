// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("626FC520-A41E-11CF-A731-00A0C9082637"), TypeLibType((short)0x1040)]
    public interface IHTMLDocument
    {
        [DispId(0x3e9)]
        object Script {[return: MarshalAs(UnmanagedType.IDispatch)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)0x440), DispId(0x3e9)] get; }
    }
}

