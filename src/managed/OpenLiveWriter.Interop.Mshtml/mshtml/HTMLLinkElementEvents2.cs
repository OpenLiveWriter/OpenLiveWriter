// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 2), Guid("3050F61D-98B5-11CF-BB82-00AA00BDCE0B"), TypeLibType((short) 0x1010)]
    public interface HTMLLinkElementEvents2
    {
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418102)]
        bool onhelp([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-600)]
        bool onclick([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-601)]
        bool ondblclick([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-603)]
        bool onkeypress([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-602)]
        void onkeydown([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-604)]
        void onkeyup([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418103)]
        void onmouseout([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418104)]
        void onmouseover([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-606)]
        void onmousemove([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-605)]
        void onmousedown([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-607)]
        void onmouseup([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418100)]
        bool onselectstart([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418095)]
        void onfilterchange([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418101)]
        bool ondragstart([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
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
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418094)]
        void onlosecapture([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418093)]
        void onpropertychange([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3f6)]
        void onscroll([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418111)]
        void onfocus([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418112)]
        void onblur([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3f8)]
        void onresize([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418092)]
        bool ondrag([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418091)]
        void ondragend([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418090)]
        bool ondragenter([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418089)]
        bool ondragover([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418088)]
        void ondragleave([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418087)]
        bool ondrop([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418083)]
        bool onbeforecut([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418086)]
        bool oncut([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418082)]
        bool onbeforecopy([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418085)]
        bool oncopy([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418081)]
        bool onbeforepaste([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418084)]
        bool onpaste([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3ff)]
        bool oncontextmenu([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418080)]
        void onrowsdelete([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418079)]
        void onrowsinserted([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418078)]
        void oncellchange([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-609)]
        void onreadystatechange([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x406)]
        void onlayoutcomplete([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x407)]
        void onpage([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x412)]
        void onmouseenter([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x413)]
        void onmouseleave([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x414)]
        void onactivate([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x415)]
        void ondeactivate([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x40a)]
        bool onbeforedeactivate([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x417)]
        bool onbeforeactivate([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x418)]
        void onfocusin([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x419)]
        void onfocusout([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x40b)]
        void onmove([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x40c)]
        bool oncontrolselect([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x40e)]
        bool onmovestart([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x40f)]
        void onmoveend([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x410)]
        bool onresizestart([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x411)]
        void onresizeend([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x409)]
        bool onmousewheel([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3eb)]
        void onload([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3ea)]
        void onerror([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
    }
}

