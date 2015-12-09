// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, TypeLibType((short) 0x1040), DefaultMember("FireEvent"), Guid("3050F7EB-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface IHTCAttachBehavior2
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0)]
        void FireEvent([In, MarshalAs(UnmanagedType.Struct)] object evt);
    }
}

