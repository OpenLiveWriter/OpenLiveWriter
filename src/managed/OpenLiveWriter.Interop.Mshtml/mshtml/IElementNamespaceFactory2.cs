// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("3050F805-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceType((short) 1)]
    public interface IElementNamespaceFactory2 : IElementNamespaceFactory
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void create([In, MarshalAs(UnmanagedType.Interface)] IElementNamespace pNamespace);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void CreateWithImplementation([In, MarshalAs(UnmanagedType.Interface)] IElementNamespace pNamespace, [In, MarshalAs(UnmanagedType.BStr)] string bstrImplementation);
    }
}

