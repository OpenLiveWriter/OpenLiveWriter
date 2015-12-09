// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, TypeLibType((short) 0x1010), InterfaceType((short) 2), Guid("3050F51D-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface DispIHTMLInputImage
    {
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417611)]
        void setAttribute([In, MarshalAs(UnmanagedType.BStr)] string strAttributeName, [In, MarshalAs(UnmanagedType.Struct)] object AttributeValue, [In, Optional] int lFlags /* = 1 */);
        [return: MarshalAs(UnmanagedType.Struct)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417610)]
        object getAttribute([In, MarshalAs(UnmanagedType.BStr)] string strAttributeName, [In, Optional] int lFlags /* = 0 */);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417609)]
        bool removeAttribute([In, MarshalAs(UnmanagedType.BStr)] string strAttributeName, [In, Optional] int lFlags /* = 1 */);
        [DispId(-2147417111)]
        string className { [param: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 4), DispId(-2147417111)] set; [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417111), TypeLibFunc((short) 4)] get; }
        [DispId(-2147417110)]
        string id { [param: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417110), TypeLibFunc((short) 4)] set; [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 4), DispId(-2147417110)] get; }
        [DispId(-2147417108)]
        string tagName { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417108)] get; }
        [DispId(-2147418104)]
        IHTMLElement parentElement { [return: MarshalAs(UnmanagedType.Interface)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418104)] get; }
        [DispId(-2147418038)]
        IHTMLStyle style { [return: MarshalAs(UnmanagedType.Interface)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418038), TypeLibFunc((short) 0x400)] get; }
        [DispId(-2147412099)]
        object onhelp { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412099), TypeLibFunc((short) 20)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412099)] get; }
        [DispId(-2147412104)]
        object onclick { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412104), TypeLibFunc((short) 20)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412104), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412103)]
        object ondblclick { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412103), TypeLibFunc((short) 20)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412103), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412107)]
        object onkeydown { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412107)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412107), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412106)]
        object onkeyup { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412106), TypeLibFunc((short) 20)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412106)] get; }
        [DispId(-2147412105)]
        object onkeypress { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412105)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412105)] get; }
        [DispId(-2147412111)]
        object onmouseout { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412111)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412111)] get; }
        [DispId(-2147412112)]
        object onmouseover { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412112), TypeLibFunc((short) 20)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412112)] get; }
        [DispId(-2147412108)]
        object onmousemove { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412108), TypeLibFunc((short) 20)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412108)] get; }
        [DispId(-2147412110)]
        object onmousedown { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412110), TypeLibFunc((short) 20)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412110)] get; }
        [DispId(-2147412109)]
        object onmouseup { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412109)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412109), TypeLibFunc((short) 20)] get; }
        [DispId(-2147417094)]
        object document { [return: MarshalAs(UnmanagedType.IDispatch)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417094)] get; }
        [DispId(-2147418043)]
        string title { [param: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147418043)] set; [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147418043)] get; }
        [DispId(-2147413012)]
        string language { [param: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413012)] set; [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147413012), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412075)]
        object onselectstart { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412075), TypeLibFunc((short) 20)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412075), TypeLibFunc((short) 20)] get; }
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417093)]
        void scrollIntoView([In, Optional, MarshalAs(UnmanagedType.Struct)] object varargStart);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417092)]
        bool contains([In, MarshalAs(UnmanagedType.Interface)] IHTMLElement pChild);
        [DispId(-2147417088)]
        int sourceIndex { [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417088), TypeLibFunc((short) 4)] get; }
        [DispId(-2147417087)]
        object recordNumber { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417087)] get; }
        [DispId(-2147413103)]
        string lang { [param: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147413103)] set; [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147413103)] get; }
        [DispId(-2147417104)]
        int offsetLeft { [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417104)] get; }
        [DispId(-2147417103)]
        int offsetTop { [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417103)] get; }
        [DispId(-2147417102)]
        int offsetWidth { [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417102)] get; }
        [DispId(-2147417101)]
        int offsetHeight { [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417101)] get; }
        [DispId(-2147417100)]
        IHTMLElement offsetParent { [return: MarshalAs(UnmanagedType.Interface)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417100)] get; }
        [DispId(-2147417086)]
        string innerHTML { [param: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417086)] set; [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417086)] get; }
        [DispId(-2147417085)]
        string innerText { [param: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417085)] set; [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417085)] get; }
        [DispId(-2147417084)]
        string outerHTML { [param: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417084)] set; [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417084)] get; }
        [DispId(-2147417083)]
        string outerText { [param: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417083)] set; [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417083)] get; }
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417082)]
        void insertAdjacentHTML([In, MarshalAs(UnmanagedType.BStr)] string where, [In, MarshalAs(UnmanagedType.BStr)] string html);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417081)]
        void insertAdjacentText([In, MarshalAs(UnmanagedType.BStr)] string where, [In, MarshalAs(UnmanagedType.BStr)] string text);
        [DispId(-2147417080)]
        IHTMLElement parentTextEdit { [return: MarshalAs(UnmanagedType.Interface)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417080)] get; }
        [DispId(-2147417078)]
        bool isTextEdit { [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417078)] get; }
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417079)]
        void click();
        [DispId(-2147417077)]
        IHTMLFiltersCollection filters { [return: MarshalAs(UnmanagedType.Interface)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417077)] get; }
        [DispId(-2147412077)]
        object ondragstart { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412077)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412077), TypeLibFunc((short) 20)] get; }
        [return: MarshalAs(UnmanagedType.BStr)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417076)]
        string toString();
        [DispId(-2147412091)]
        object onbeforeupdate { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412091), TypeLibFunc((short) 20)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412091), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412090)]
        object onafterupdate { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412090)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412090)] get; }
        [DispId(-2147412074)]
        object onerrorupdate { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412074), TypeLibFunc((short) 20)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412074), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412094)]
        object onrowexit { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412094), TypeLibFunc((short) 20)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412094), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412093)]
        object onrowenter { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412093), TypeLibFunc((short) 20)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412093)] get; }
        [DispId(-2147412072)]
        object ondatasetchanged { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412072)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412072), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412071)]
        object ondataavailable { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412071)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412071)] get; }
        [DispId(-2147412070)]
        object ondatasetcomplete { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412070), TypeLibFunc((short) 20)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412070), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412069)]
        object onfilterchange { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412069)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412069), TypeLibFunc((short) 20)] get; }
        [DispId(-2147417075)]
        object children { [return: MarshalAs(UnmanagedType.IDispatch)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417075)] get; }
        [DispId(-2147417074)]
        object all { [return: MarshalAs(UnmanagedType.IDispatch)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417074)] get; }
        [DispId(-2147418097)]
        short tabIndex { [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147418097)] set; [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418097), TypeLibFunc((short) 20)] get; }
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147416112)]
        void focus();
        [DispId(-2147416107)]
        string accessKey { [param: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147416107)] set; [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147416107), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412097)]
        object onblur { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412097)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412097)] get; }
        [DispId(-2147412098)]
        object onfocus { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412098)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412098), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412076)]
        object onresize { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412076)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412076)] get; }
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147416110)]
        void blur();
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147416095)]
        void addFilter([In, MarshalAs(UnmanagedType.IUnknown)] object pUnk);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147416094)]
        void removeFilter([In, MarshalAs(UnmanagedType.IUnknown)] object pUnk);
        [DispId(-2147416093)]
        int clientHeight { [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147416093)] get; }
        [DispId(-2147416092)]
        int clientWidth { [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147416092), TypeLibFunc((short) 20)] get; }
        [DispId(-2147416091)]
        int clientTop { [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147416091)] get; }
        [DispId(-2147416090)]
        int clientLeft { [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147416090), TypeLibFunc((short) 20)] get; }
        [DispId(0x7d0)]
        string type { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x7d0)] get; }
        [DispId(-2147418036)]
        bool disabled { [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147418036)] set; [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147418036)] get; }
        [DispId(0x7dc)]
        object border { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(0x7dc)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x7dc), TypeLibFunc((short) 20)] get; }
        [DispId(0x7dd)]
        int vspace { [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x7dd), TypeLibFunc((short) 20)] set; [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x7dd), TypeLibFunc((short) 20)] get; }
        [DispId(0x7de)]
        int hspace { [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(0x7de)] set; [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x7de), TypeLibFunc((short) 20)] get; }
        [DispId(0x7da)]
        string alt { [param: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(0x7da)] set; [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x7da), TypeLibFunc((short) 20)] get; }
        [DispId(0x7db)]
        string src { [param: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(0x7db)] set; [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(0x7db)] get; }
        [DispId(0x7df)]
        string lowsrc { [param: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(0x7df)] set; [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x7df), TypeLibFunc((short) 20)] get; }
        [DispId(0x7e0)]
        string vrml { [param: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(0x7e0)] set; [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x7e0), TypeLibFunc((short) 20)] get; }
        [DispId(0x7e1)]
        string dynsrc { [param: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(0x7e1)] set; [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x7e1), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412996)]
        string readyState { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412996)] get; }
        [DispId(0x7e2)]
        bool complete { [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x7e2)] get; }
        [DispId(0x7e3)]
        object loop { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(0x7e3)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x7e3), TypeLibFunc((short) 20)] get; }
        [DispId(-2147418039)]
        string align { [param: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147418039)] set; [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418039), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412080)]
        object onload { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412080)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412080)] get; }
        [DispId(-2147412083)]
        object onerror { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412083), TypeLibFunc((short) 20)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412083), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412084)]
        object onabort { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412084)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412084), TypeLibFunc((short) 20)] get; }
        [DispId(-2147418112)]
        string name { [param: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418112), TypeLibFunc((short) 20)] set; [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147418112)] get; }
        [DispId(-2147418107)]
        int width { [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418107)] set; [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418107)] get; }
        [DispId(-2147418106)]
        int height { [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418106)] set; [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418106)] get; }
        [DispId(0x7e4)]
        string Start { [param: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x7e4), TypeLibFunc((short) 20)] set; [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(0x7e4)] get; }
    }
}

