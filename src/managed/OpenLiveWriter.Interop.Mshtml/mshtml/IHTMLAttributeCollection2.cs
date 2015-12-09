// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, TypeLibType((short) 0x1040), Guid("3050F80A-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface IHTMLAttributeCollection2
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5dd)]
        IHTMLDOMAttribute getNamedItem([In, MarshalAs(UnmanagedType.BStr)] string bstrName);
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5de)]
        IHTMLDOMAttribute setNamedItem([In, MarshalAs(UnmanagedType.Interface)] IHTMLDOMAttribute ppNode);
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5df)]
        IHTMLDOMAttribute removeNamedItem([In, MarshalAs(UnmanagedType.BStr)] string bstrName);
    }
}

