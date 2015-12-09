// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 1), Guid("3050F672-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface IElementNamespaceFactory
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void create([In, MarshalAs(UnmanagedType.Interface)] IElementNamespace pNamespace);
    }
}

