// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 2), Guid("3050F625-98B5-11CF-BB82-00AA00BDCE0B"), TypeLibType((short) 0x1010)]
    public interface HTMLWindowEvents2
    {
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3eb)]
        void onload([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3f0)]
        void onunload([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418102)]
        bool onhelp([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418111)]
        void onfocus([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418112)]
        void onblur([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3ea)]
        void onerror([In, MarshalAs(UnmanagedType.BStr)] string description, [In, MarshalAs(UnmanagedType.BStr)] string url, [In] int line);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3f8)]
        void onresize([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3f6)]
        void onscroll([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3f9)]
        void onbeforeunload([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x400)]
        void onbeforeprint([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x401)]
        void onafterprint([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
    }
}

