// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, TypeLibType((short)0x1010), InterfaceType((short)2), Guid("3050F613-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface HTMLDocumentEvents2
    {
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418102)]
        bool onhelp([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-600)]
        bool onclick([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-601)]
        bool ondblclick([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-602)]
        void onkeydown([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-604)]
        void onkeyup([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-603)]
        bool onkeypress([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-605)]
        void onmousedown([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-606)]
        void onmousemove([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-607)]
        void onmouseup([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418103)]
        void onmouseout([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418104)]
        void onmouseover([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-609)]
        void onreadystatechange([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418108)]
        bool onbeforeupdate([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418107)]
        void onafterupdate([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418106)]
        bool onrowexit([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418105)]
        void onrowenter([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418101)]
        bool ondragstart([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418100)]
        bool onselectstart([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418099)]
        bool onerrorupdate([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3ff)]
        bool oncontextmenu([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x402)]
        bool onstop([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418080)]
        void onrowsdelete([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418079)]
        void onrowsinserted([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418078)]
        void oncellchange([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418093)]
        void onpropertychange([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418098)]
        void ondatasetchanged([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418097)]
        void ondataavailable([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418096)]
        void ondatasetcomplete([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x403)]
        void onbeforeeditfocus([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x40d)]
        void onselectionchange([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x40c)]
        bool oncontrolselect([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x409)]
        bool onmousewheel([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x418)]
        void onfocusin([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x419)]
        void onfocusout([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x414)]
        void onactivate([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x415)]
        void ondeactivate([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x417)]
        bool onbeforeactivate([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x40a)]
        bool onbeforedeactivate([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
    }
}

