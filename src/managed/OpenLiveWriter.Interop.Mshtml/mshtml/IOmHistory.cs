// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, TypeLibType((short)0x1040), Guid("FECEAAA2-8405-11CF-8BA1-00AA00476DA6")]
    public interface IOmHistory
    {
        [DispId(1)]
        short length {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(1)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(2)]
        void back([In, Optional, MarshalAs(UnmanagedType.Struct)] ref object pvargdistance);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(3)]
        void forward([In, Optional, MarshalAs(UnmanagedType.Struct)] ref object pvargdistance);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(4)]
        void go([In, Optional, MarshalAs(UnmanagedType.Struct)] ref object pvargdistance);
    }
}

