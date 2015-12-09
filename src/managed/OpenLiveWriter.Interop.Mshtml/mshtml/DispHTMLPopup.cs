// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 2), Guid("3050F589-98B5-11CF-BB82-00AA00BDCE0B"), TypeLibType((short) 0x1010)]
    public interface DispHTMLPopup
    {
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6979)]
        void Show([In] int x, [In] int y, [In] int w, [In] int h, [In, MarshalAs(UnmanagedType.Struct)] ref object pElement);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x697a)]
        void Hide();
        [DispId(0x697b)]
        IHTMLDocument document { [return: MarshalAs(UnmanagedType.Interface)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x697b)] get; }
        [DispId(0x697c)]
        bool isOpen { [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x697c)] get; }
    }
}

