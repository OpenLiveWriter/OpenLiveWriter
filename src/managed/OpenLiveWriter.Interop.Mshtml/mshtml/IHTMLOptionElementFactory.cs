// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("3050F38C-98B5-11CF-BB82-00AA00BDCE0B"), TypeLibType((short)0x1040), DefaultMember("create")]
    public interface IHTMLOptionElementFactory
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0)]
        IHTMLOptionElement create([In, Optional, MarshalAs(UnmanagedType.Struct)] object text, [In, Optional, MarshalAs(UnmanagedType.Struct)] object value, [In, Optional, MarshalAs(UnmanagedType.Struct)] object defaultSelected, [In, Optional, MarshalAs(UnmanagedType.Struct)] object selected);
    }
}

