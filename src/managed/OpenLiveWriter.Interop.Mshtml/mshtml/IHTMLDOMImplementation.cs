// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, TypeLibType((short)0x1040), Guid("3050F80D-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface IHTMLDOMImplementation
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3e8)]
        bool hasFeature([In, MarshalAs(UnmanagedType.BStr)] string bstrfeature, [In, Optional, MarshalAs(UnmanagedType.Struct)] object version);
    }
}

