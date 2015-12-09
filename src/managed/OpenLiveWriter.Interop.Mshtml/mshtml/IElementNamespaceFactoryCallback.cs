// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 1), Guid("3050F7FD-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface IElementNamespaceFactoryCallback
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Resolve([In, MarshalAs(UnmanagedType.BStr)] string bstrNamespace, [In, MarshalAs(UnmanagedType.BStr)] string bstrTagName, [In, MarshalAs(UnmanagedType.BStr)] string bstrAttrs, [In, MarshalAs(UnmanagedType.Interface)] IElementNamespace pNamespace);
    }
}

