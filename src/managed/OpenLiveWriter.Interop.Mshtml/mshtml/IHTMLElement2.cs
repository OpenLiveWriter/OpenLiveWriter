// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("3050F434-98B5-11CF-BB82-00AA00BDCE0B"), TypeLibType((short)0x1040)]
    public interface IHTMLElement2
    {
        [DispId(-2147417073)]
        string scopeName {[return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147417073)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147417072)]
        void setCapture([In, Optional] bool containerCapture /* = true */);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147417071)]
        void releaseCapture();
        [DispId(-2147412066)]
        object onlosecapture {[param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147412066), TypeLibFunc((short)20)] set;[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412066)] get; }
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147417070)]
        string componentFromPoint([In] int x, [In] int y);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147417069)]
        void doScroll([In, Optional, MarshalAs(UnmanagedType.Struct)] object component);
        [DispId(-2147412081)]
        object onscroll {[param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412081)] set;[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412081)] get; }
        [DispId(-2147412063)]
        object ondrag {[param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412063)] set;[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412063)] get; }
        [DispId(-2147412062)]
        object ondragend {[param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147412062), TypeLibFunc((short)20)] set;[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412062)] get; }
        [DispId(-2147412061)]
        object ondragenter {[param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147412061), TypeLibFunc((short)20)] set;[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412061)] get; }
        [DispId(-2147412060)]
        object ondragover {[param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147412060), TypeLibFunc((short)20)] set;[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147412060), TypeLibFunc((short)20)] get; }
        [DispId(-2147412059)]
        object ondragleave {[param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147412059), TypeLibFunc((short)20)] set;[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412059)] get; }
        [DispId(-2147412058)]
        object ondrop {[param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412058)] set;[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412058)] get; }
        [DispId(-2147412054)]
        object onbeforecut {[param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412054)] set;[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412054)] get; }
        [DispId(-2147412057)]
        object oncut {[param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412057)] set;[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412057)] get; }
        [DispId(-2147412053)]
        object onbeforecopy {[param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412053)] set;[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412053)] get; }
        [DispId(-2147412056)]
        object oncopy {[param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147412056), TypeLibFunc((short)20)] set;[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147412056), TypeLibFunc((short)20)] get; }
        [DispId(-2147412052)]
        object onbeforepaste {[param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412052)] set;[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412052)] get; }
        [DispId(-2147412055)]
        object onpaste {[param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147412055), TypeLibFunc((short)20)] set;[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147412055), TypeLibFunc((short)20)] get; }
        [DispId(-2147417105)]
        IHTMLCurrentStyle currentStyle {[return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)0x400), DispId(-2147417105)] get; }
        [DispId(-2147412065)]
        object onpropertychange {[param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147412065), TypeLibFunc((short)20)] set;[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412065)] get; }
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147417068)]
        IHTMLRectCollection getClientRects();
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147417067)]
        IHTMLRect getBoundingClientRect();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147417608)]
        void setExpression([In, MarshalAs(UnmanagedType.BStr)] string propname, [In, MarshalAs(UnmanagedType.BStr)] string expression, [In, Optional, MarshalAs(UnmanagedType.BStr)] string language /* = "" */);
        [return: MarshalAs(UnmanagedType.Struct)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147417607)]
        object getExpression([In, MarshalAs(UnmanagedType.BStr)] string propname);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147417606)]
        bool removeExpression([In, MarshalAs(UnmanagedType.BStr)] string propname);
        [DispId(-2147418097)]
        short tabIndex {[param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418097), TypeLibFunc((short)20)] set;[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147418097)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147416112)]
        void focus();
        [DispId(-2147416107)]
        string accessKey {[param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147416107)] set;[return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147416107), TypeLibFunc((short)20)] get; }
        [DispId(-2147412097)]
        object onblur {[param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412097)] set;[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412097)] get; }
        [DispId(-2147412098)]
        object onfocus {[param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412098)] set;[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147412098), TypeLibFunc((short)20)] get; }
        [DispId(-2147412076)]
        object onresize {[param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147412076), TypeLibFunc((short)20)] set;[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412076)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147416110)]
        void blur();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147416095)]
        void addFilter([In, MarshalAs(UnmanagedType.IUnknown)] object pUnk);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147416094)]
        void removeFilter([In, MarshalAs(UnmanagedType.IUnknown)] object pUnk);
        [DispId(-2147416093)]
        int clientHeight {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147416093)] get; }
        [DispId(-2147416092)]
        int clientWidth {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147416092), TypeLibFunc((short)20)] get; }
        [DispId(-2147416091)]
        int clientTop {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147416091), TypeLibFunc((short)20)] get; }
        [DispId(-2147416090)]
        int clientLeft {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147416090)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147417605)]
        bool attachEvent([In, MarshalAs(UnmanagedType.BStr)] string @event, [In, MarshalAs(UnmanagedType.IDispatch)] object pdisp);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147417604)]
        void detachEvent([In, MarshalAs(UnmanagedType.BStr)] string @event, [In, MarshalAs(UnmanagedType.IDispatch)] object pdisp);
        [DispId(-2147412996)]
        object readyState {[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147412996)] get; }
        [DispId(-2147412087)]
        object onreadystatechange {[param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412087)] set;[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412087)] get; }
        [DispId(-2147412050)]
        object onrowsdelete {[param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412050)] set;[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412050)] get; }
        [DispId(-2147412049)]
        object onrowsinserted {[param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412049)] set;[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412049)] get; }
        [DispId(-2147412048)]
        object oncellchange {[param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412048)] set;[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147412048), TypeLibFunc((short)20)] get; }
        [DispId(-2147412995)]
        string dir {[param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147412995), TypeLibFunc((short)20)] set;[return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412995)] get; }
        [return: MarshalAs(UnmanagedType.IDispatch)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147417056)]
        object createControlRange();
        [DispId(-2147417055)]
        int scrollHeight {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147417055), TypeLibFunc((short)20)] get; }
        [DispId(-2147417054)]
        int scrollWidth {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147417054)] get; }
        [DispId(-2147417053)]
        int scrollTop {[param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147417053), TypeLibFunc((short)20)] set;[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147417053), TypeLibFunc((short)20)] get; }
        [DispId(-2147417052)]
        int scrollLeft {[param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147417052), TypeLibFunc((short)20)] set;[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147417052), TypeLibFunc((short)20)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147417050)]
        void clearAttributes();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147417049)]
        void mergeAttributes([In, MarshalAs(UnmanagedType.Interface)] IHTMLElement mergeThis);
        [DispId(-2147412047)]
        object oncontextmenu {[param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412047)] set;[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412047)] get; }
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147417043)]
        IHTMLElement insertAdjacentElement([In, MarshalAs(UnmanagedType.BStr)] string where, [In, MarshalAs(UnmanagedType.Interface)] IHTMLElement insertedElement);
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147417047)]
        IHTMLElement applyElement([In, MarshalAs(UnmanagedType.Interface)] IHTMLElement apply, [In, MarshalAs(UnmanagedType.BStr)] string where);
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147417042)]
        string getAdjacentText([In, MarshalAs(UnmanagedType.BStr)] string where);
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147417041)]
        string replaceAdjacentText([In, MarshalAs(UnmanagedType.BStr)] string where, [In, MarshalAs(UnmanagedType.BStr)] string newText);
        [DispId(-2147417040)]
        bool canHaveChildren {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147417040)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147417032)]
        int addBehavior([In, MarshalAs(UnmanagedType.BStr)] string bstrUrl, [In, Optional, MarshalAs(UnmanagedType.Struct)] ref object pvarFactory);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147417031)]
        bool removeBehavior([In] int cookie);
        [DispId(-2147417048)]
        IHTMLStyle runtimeStyle {[return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147417048), TypeLibFunc((short)0x400)] get; }
        [DispId(-2147417030)]
        object behaviorUrns {[return: MarshalAs(UnmanagedType.IDispatch)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147417030)] get; }
        [DispId(-2147417029)]
        string tagUrn {[param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147417029)] set;[return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147417029)] get; }
        [DispId(-2147412043)]
        object onbeforeeditfocus {[param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147412043), TypeLibFunc((short)20)] set;[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147412043), TypeLibFunc((short)20)] get; }
        [DispId(-2147417028)]
        int readyStateValue {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)0x41), DispId(-2147417028)] get; }
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147417027)]
        IHTMLElementCollection getElementsByTagName([In, MarshalAs(UnmanagedType.BStr)] string v);
    }
}

