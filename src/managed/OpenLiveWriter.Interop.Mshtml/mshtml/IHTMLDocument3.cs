// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("3050F485-98B5-11CF-BB82-00AA00BDCE0B"), TypeLibType((short)0x1040)]
    public interface IHTMLDocument3
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x430)]
        void releaseCapture();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x431)]
        void recalc([In, Optional] bool fForce /* = false */);
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x432)]
        IHTMLDOMNode createTextNode([In, MarshalAs(UnmanagedType.BStr)] string text);
        [DispId(0x433)]
        IHTMLElement documentElement {[return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x433)] get; }
        [DispId(0x435)]
        string uniqueID {[return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x435), TypeLibFunc((short)0x40)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147417605)]
        bool attachEvent([In, MarshalAs(UnmanagedType.BStr)] string @event, [In, MarshalAs(UnmanagedType.IDispatch)] object pdisp);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147417604)]
        void detachEvent([In, MarshalAs(UnmanagedType.BStr)] string @event, [In, MarshalAs(UnmanagedType.IDispatch)] object pdisp);
        [DispId(-2147412050)]
        object onrowsdelete {[param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412050)] set;[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147412050), TypeLibFunc((short)20)] get; }
        [DispId(-2147412049)]
        object onrowsinserted {[param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147412049), TypeLibFunc((short)20)] set;[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412049)] get; }
        [DispId(-2147412048)]
        object oncellchange {[param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412048)] set;[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147412048), TypeLibFunc((short)20)] get; }
        [DispId(-2147412072)]
        object ondatasetchanged {[param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147412072), TypeLibFunc((short)20)] set;[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147412072), TypeLibFunc((short)20)] get; }
        [DispId(-2147412071)]
        object ondataavailable {[param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412071)] set;[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412071)] get; }
        [DispId(-2147412070)]
        object ondatasetcomplete {[param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412070)] set;[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147412070), TypeLibFunc((short)20)] get; }
        [DispId(-2147412065)]
        object onpropertychange {[param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412065)] set;[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412065)] get; }
        [DispId(-2147412995)]
        string dir {[param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147412995)] set;[return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147412995)] get; }
        [DispId(-2147412047)]
        object oncontextmenu {[param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412047)] set;[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412047)] get; }
        [DispId(-2147412044)]
        object onstop {[param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147412044), TypeLibFunc((short)20)] set;[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412044)] get; }
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x434)]
        IHTMLDocument2 createDocumentFragment();
        [DispId(0x436)]
        IHTMLDocument2 parentDocument {[return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)0x41), DispId(0x436)] get; }
        [DispId(0x437)]
        bool enableDownload {[param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x437), TypeLibFunc((short)0x41)] set;[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)0x41), DispId(0x437)] get; }
        [DispId(0x438)]
        string baseUrl {[param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x438), TypeLibFunc((short)0x41)] set;[return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x438), TypeLibFunc((short)0x41)] get; }
        [DispId(-2147417063)]
        object childNodes {[return: MarshalAs(UnmanagedType.IDispatch)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147417063)] get; }
        [DispId(0x43a)]
        bool inheritStyleSheets {[param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x43a), TypeLibFunc((short)0x41)] set;[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x43a), TypeLibFunc((short)0x41)] get; }
        [DispId(-2147412043)]
        object onbeforeeditfocus {[param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412043)] set;[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412043)] get; }
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x43e)]
        IHTMLElementCollection getElementsByName([In, MarshalAs(UnmanagedType.BStr)] string v);
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x440)]
        IHTMLElement getElementById([In, MarshalAs(UnmanagedType.BStr)] string v);
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x43f)]
        IHTMLElementCollection getElementsByTagName([In, MarshalAs(UnmanagedType.BStr)] string v);
    }
}

