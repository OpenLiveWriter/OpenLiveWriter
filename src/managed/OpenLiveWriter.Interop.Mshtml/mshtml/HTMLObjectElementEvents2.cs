// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, TypeLibType((short) 0x1010), InterfaceType((short) 2), Guid("3050F620-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface HTMLObjectElementEvents2
    {
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418108)]
        bool onbeforeupdate([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418107)]
        void onafterupdate([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418099)]
        bool onerrorupdate([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418106)]
        bool onrowexit([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418105)]
        void onrowenter([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418098)]
        void ondatasetchanged([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418097)]
        void ondataavailable([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418096)]
        void ondatasetcomplete([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418093)]
        bool onerror([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418080)]
        void onrowsdelete([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418079)]
        void onrowsinserted([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418078)]
        void oncellchange([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418092)]
        void onreadystatechange([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
    }
}

