// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("3050F848-98B5-11CF-BB82-00AA00BDCE0D"), InterfaceType((short) 1)]
    public interface IHTMLEditHost2 : IHTMLEditHost
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void SnapRect([In, MarshalAs(UnmanagedType.Interface)] IHTMLElement pIElement, [In, Out] ref tagRECT prcNew, [In] _ELEMENT_CORNER eHandle);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void PreDrag();
    }
}

