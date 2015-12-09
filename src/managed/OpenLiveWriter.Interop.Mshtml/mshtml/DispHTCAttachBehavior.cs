// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, DefaultMember("FireEvent"), Guid("3050F583-98B5-11CF-BB82-00AA00BDCE0B"), TypeLibType((short) 0x1010), InterfaceType((short) 2)]
    public interface DispHTCAttachBehavior
    {
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0)]
        void FireEvent([In, MarshalAs(UnmanagedType.Struct)] object evt);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417612)]
        void detachEvent();
    }
}

