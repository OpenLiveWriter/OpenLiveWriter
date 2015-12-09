// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 2), Guid("3050F58F-98B5-11CF-BB82-00AA00BDCE0B"), TypeLibType((short) 0x1010)]
    public interface DispHTMLDOMImplementation
    {
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3e8)]
        bool hasFeature([In, MarshalAs(UnmanagedType.BStr)] string bstrfeature, [In, Optional, MarshalAs(UnmanagedType.Struct)] object version);
    }
}

