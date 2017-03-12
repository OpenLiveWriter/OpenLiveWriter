// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("3050F38E-98B5-11CF-BB82-00AA00BDCE0B"), DefaultMember("create"), TypeLibType((short)0x1040)]
    public interface IHTMLImageElementFactory
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0)]
        IHTMLImgElement create([In, Optional, MarshalAs(UnmanagedType.Struct)] object width, [In, Optional, MarshalAs(UnmanagedType.Struct)] object height);
    }
}

