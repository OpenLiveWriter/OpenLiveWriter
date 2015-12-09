// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 2), Guid("3050F55F-98B5-11CF-BB82-00AA00BDCE0B"), TypeLibType((short) 0x1010)]
    public interface DispHTMLDocument
    {
        [DispId(0x3e9)]
        object Script { [return: MarshalAs(UnmanagedType.IDispatch)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 0x440), DispId(0x3e9)] get; }
        [DispId(0x3eb)]
        IHTMLElementCollection all { [return: MarshalAs(UnmanagedType.Interface)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3eb)] get; }
        [DispId(0x3ec)]
        IHTMLElement body { [return: MarshalAs(UnmanagedType.Interface)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3ec)] get; }
        [DispId(0x3ed)]
        IHTMLElement activeElement { [return: MarshalAs(UnmanagedType.Interface)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3ed)] get; }
        [DispId(0x3f3)]
        IHTMLElementCollection images { [return: MarshalAs(UnmanagedType.Interface)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3f3)] get; }
        [DispId(0x3f0)]
        IHTMLElementCollection applets { [return: MarshalAs(UnmanagedType.Interface)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3f0)] get; }
        [DispId(0x3f1)]
        IHTMLElementCollection links { [return: MarshalAs(UnmanagedType.Interface)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3f1)] get; }
        [DispId(0x3f2)]
        IHTMLElementCollection forms { [return: MarshalAs(UnmanagedType.Interface)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3f2)] get; }
        [DispId(0x3ef)]
        IHTMLElementCollection anchors { [return: MarshalAs(UnmanagedType.Interface)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3ef)] get; }
        [DispId(0x3f4)]
        string title { [param: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3f4)] set; [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3f4)] get; }
        [DispId(0x3f5)]
        IHTMLElementCollection scripts { [return: MarshalAs(UnmanagedType.Interface)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3f5)] get; }
        [DispId(0x3f6)]
        string designMode { [param: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 0x40), DispId(0x3f6)] set; [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 0x40), DispId(0x3f6)] get; }
        [DispId(0x3f9)]
        IHTMLSelectionObject selection { [return: MarshalAs(UnmanagedType.Interface)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3f9)] get; }
        [DispId(0x3fa)]
        string readyState { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3fa), TypeLibFunc((short) 4)] get; }
        [DispId(0x3fb)]
        FramesCollection frames { [return: MarshalAs(UnmanagedType.Interface)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3fb)] get; }
        [DispId(0x3f7)]
        IHTMLElementCollection embeds { [return: MarshalAs(UnmanagedType.Interface)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3f7)] get; }
        [DispId(0x3fd)]
        IHTMLElementCollection plugins { [return: MarshalAs(UnmanagedType.Interface)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3fd)] get; }
        [DispId(0x3fe)]
        object alinkColor { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3fe)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3fe)] get; }
        [DispId(-501)]
        object bgColor { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-501)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-501)] get; }
        [DispId(-2147413110)]
        object fgColor { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147413110)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147413110)] get; }
        [DispId(0x400)]
        object linkColor { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x400)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x400)] get; }
        [DispId(0x3ff)]
        object vlinkColor { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3ff)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3ff)] get; }
        [DispId(0x403)]
        string referrer { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x403)] get; }
        [DispId(0x402)]
        HTMLLocation location { [return: MarshalAs(UnmanagedType.Interface)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x402)] get; }
        [DispId(0x404)]
        string lastModified { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x404)] get; }
        [DispId(0x401)]
        string url { [param: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x401)] set; [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x401)] get; }
        [DispId(0x405)]
        string domain { [param: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x405)] set; [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x405)] get; }
        [DispId(0x406)]
        string cookie { [param: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x406)] set; [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x406)] get; }
        [DispId(0x407)]
        bool expando { [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 0x44), DispId(0x407)] set; [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 0x44), DispId(0x407)] get; }
        [DispId(0x408)]
        string charset { [param: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x408), TypeLibFunc((short) 0x40)] set; [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x408), TypeLibFunc((short) 0x40)] get; }
        [DispId(0x409)]
        string defaultCharset { [param: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x409)] set; [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x409)] get; }
        [DispId(0x411)]
        string mimeType { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x411)] get; }
        [DispId(0x412)]
        string fileSize { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x412)] get; }
        [DispId(0x413)]
        string fileCreatedDate { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x413)] get; }
        [DispId(0x414)]
        string fileModifiedDate { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x414)] get; }
        [DispId(0x415)]
        string fileUpdatedDate { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x415)] get; }
        [DispId(0x416)]
        string security { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x416)] get; }
        [DispId(0x417)]
        string protocol { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x417)] get; }
        [DispId(0x418)]
        string nameProp { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x418)] get; }
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x41e)]
        void write([In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType=VarEnum.VT_VARIANT)] params object[] psarray);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x41f)]
        void writeln([In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType=VarEnum.VT_VARIANT)] params object[] psarray);
        [return: MarshalAs(UnmanagedType.IDispatch)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x420)]
        object open([In, Optional, MarshalAs(UnmanagedType.BStr)] string url /* = "text/html" */, [In, Optional, MarshalAs(UnmanagedType.Struct)] object name, [In, Optional, MarshalAs(UnmanagedType.Struct)] object features, [In, Optional, MarshalAs(UnmanagedType.Struct)] object replace);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x421)]
        void close();
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x422)]
        void clear();
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x423)]
        bool queryCommandSupported([In, MarshalAs(UnmanagedType.BStr)] string cmdID);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x424)]
        bool queryCommandEnabled([In, MarshalAs(UnmanagedType.BStr)] string cmdID);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x425)]
        bool queryCommandState([In, MarshalAs(UnmanagedType.BStr)] string cmdID);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x426)]
        bool queryCommandIndeterm([In, MarshalAs(UnmanagedType.BStr)] string cmdID);
        [return: MarshalAs(UnmanagedType.BStr)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x427)]
        string queryCommandText([In, MarshalAs(UnmanagedType.BStr)] string cmdID);
        [return: MarshalAs(UnmanagedType.Struct)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x428)]
        object queryCommandValue([In, MarshalAs(UnmanagedType.BStr)] string cmdID);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x429)]
        bool execCommand([In, MarshalAs(UnmanagedType.BStr)] string cmdID, [In, Optional] bool showUI /* = false */, [In, Optional, MarshalAs(UnmanagedType.Struct)] object value);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x42a)]
        bool execCommandShowHelp([In, MarshalAs(UnmanagedType.BStr)] string cmdID);
        [return: MarshalAs(UnmanagedType.Interface)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x42b)]
        IHTMLElement createElement([In, MarshalAs(UnmanagedType.BStr)] string eTag);
        [DispId(-2147412099)]
        object onhelp { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412099)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412099), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412104)]
        object onclick { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412104)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412104), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412103)]
        object ondblclick { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412103), TypeLibFunc((short) 20)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412103)] get; }
        [DispId(-2147412106)]
        object onkeyup { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412106)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412106), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412107)]
        object onkeydown { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412107)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412107)] get; }
        [DispId(-2147412105)]
        object onkeypress { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412105)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412105)] get; }
        [DispId(-2147412109)]
        object onmouseup { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412109)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412109), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412110)]
        object onmousedown { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412110)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412110)] get; }
        [DispId(-2147412108)]
        object onmousemove { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412108)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412108), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412111)]
        object onmouseout { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412111)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412111), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412112)]
        object onmouseover { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412112), TypeLibFunc((short) 20)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412112), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412087)]
        object onreadystatechange { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412087)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412087), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412090)]
        object onafterupdate { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412090), TypeLibFunc((short) 20)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412090), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412094)]
        object onrowexit { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412094)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412094), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412093)]
        object onrowenter { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412093), TypeLibFunc((short) 20)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412093), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412077)]
        object ondragstart { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412077), TypeLibFunc((short) 20)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412077)] get; }
        [DispId(-2147412075)]
        object onselectstart { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412075)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412075), TypeLibFunc((short) 20)] get; }
        [return: MarshalAs(UnmanagedType.Interface)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x42c)]
        IHTMLElement elementFromPoint([In] int x, [In] int y);
        [DispId(0x40a)]
        IHTMLWindow2 parentWindow { [return: MarshalAs(UnmanagedType.Interface)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x40a)] get; }
        [DispId(0x42d)]
        HTMLStyleSheetsCollection styleSheets { [return: MarshalAs(UnmanagedType.Interface)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x42d)] get; }
        [DispId(-2147412091)]
        object onbeforeupdate { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412091)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412091), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412074)]
        object onerrorupdate { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412074)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412074)] get; }
        [return: MarshalAs(UnmanagedType.BStr)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x42e)]
        string toString();
        [return: MarshalAs(UnmanagedType.Interface)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x42f)]
        IHTMLStyleSheet createStyleSheet([In, Optional, MarshalAs(UnmanagedType.BStr)] string bstrHref /* = "" */, [In, Optional] int lIndex /* = -1 */);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x430)]
        void releaseCapture();
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x431)]
        void recalc([In, Optional] bool fForce /* = false */);
        [return: MarshalAs(UnmanagedType.Interface)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x432)]
        IHTMLDOMNode createTextNode([In, MarshalAs(UnmanagedType.BStr)] string text);
        [DispId(0x433)]
        IHTMLElement documentElement { [return: MarshalAs(UnmanagedType.Interface)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x433)] get; }
        [DispId(0x435)]
        string uniqueID { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 0x40), DispId(0x435)] get; }
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417605)]
        bool attachEvent([In, MarshalAs(UnmanagedType.BStr)] string @event, [In, MarshalAs(UnmanagedType.IDispatch)] object pdisp);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417604)]
        void detachEvent([In, MarshalAs(UnmanagedType.BStr)] string @event, [In, MarshalAs(UnmanagedType.IDispatch)] object pdisp);
        [DispId(-2147412050)]
        object onrowsdelete { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412050)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412050), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412049)]
        object onrowsinserted { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412049), TypeLibFunc((short) 20)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412049)] get; }
        [DispId(-2147412048)]
        object oncellchange { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412048), TypeLibFunc((short) 20)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412048)] get; }
        [DispId(-2147412072)]
        object ondatasetchanged { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412072)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412072)] get; }
        [DispId(-2147412071)]
        object ondataavailable { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412071), TypeLibFunc((short) 20)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412071)] get; }
        [DispId(-2147412070)]
        object ondatasetcomplete { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412070)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412070)] get; }
        [DispId(-2147412065)]
        object onpropertychange { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412065)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412065)] get; }
        [DispId(-2147412995)]
        string dir { [param: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412995)] set; [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412995)] get; }
        [DispId(-2147412047)]
        object oncontextmenu { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412047)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412047), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412044)]
        object onstop { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412044), TypeLibFunc((short) 20)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412044)] get; }
        [return: MarshalAs(UnmanagedType.Interface)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x434)]
        IHTMLDocument2 createDocumentFragment();
        [DispId(0x436)]
        IHTMLDocument2 parentDocument { [return: MarshalAs(UnmanagedType.Interface)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 0x41), DispId(0x436)] get; }
        [DispId(0x437)]
        bool enableDownload { [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x437), TypeLibFunc((short) 0x41)] set; [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x437), TypeLibFunc((short) 0x41)] get; }
        [DispId(0x438)]
        string baseUrl { [param: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 0x41), DispId(0x438)] set; [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 0x41), DispId(0x438)] get; }
        [DispId(0x43a)]
        bool inheritStyleSheets { [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 0x41), DispId(0x43a)] set; [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x43a), TypeLibFunc((short) 0x41)] get; }
        [DispId(-2147412043)]
        object onbeforeeditfocus { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412043)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412043), TypeLibFunc((short) 20)] get; }
        [return: MarshalAs(UnmanagedType.Interface)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x43e)]
        IHTMLElementCollection getElementsByName([In, MarshalAs(UnmanagedType.BStr)] string v);
        [return: MarshalAs(UnmanagedType.Interface)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x440)]
        IHTMLElement getElementById([In, MarshalAs(UnmanagedType.BStr)] string v);
        [return: MarshalAs(UnmanagedType.Interface)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x43f)]
        IHTMLElementCollection getElementsByTagName([In, MarshalAs(UnmanagedType.BStr)] string v);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x441)]
        void focus();
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x442)]
        bool hasFocus();
        [DispId(-2147412032)]
        object onselectionchange { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412032)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412032), TypeLibFunc((short) 20)] get; }
        [DispId(0x443)]
        object namespaces { [return: MarshalAs(UnmanagedType.IDispatch)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x443)] get; }
        [return: MarshalAs(UnmanagedType.Interface)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x444)]
        IHTMLDocument2 createDocumentFromUrl([In, MarshalAs(UnmanagedType.BStr)] string bstrUrl, [In, MarshalAs(UnmanagedType.BStr)] string bstrOptions);
        [DispId(0x445)]
        string media { [param: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x445)] set; [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x445)] get; }
        [return: MarshalAs(UnmanagedType.Interface)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x446)]
        IHTMLEventObj CreateEventObject([In, Optional, MarshalAs(UnmanagedType.Struct)] ref object pvarEventObject);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x447)]
        bool FireEvent([In, MarshalAs(UnmanagedType.BStr)] string bstrEventName, [In, Optional, MarshalAs(UnmanagedType.Struct)] ref object pvarEventObject);
        [return: MarshalAs(UnmanagedType.Interface)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x448)]
        IHTMLRenderStyle createRenderStyle([In, MarshalAs(UnmanagedType.BStr)] string v);
        [DispId(-2147412033)]
        object oncontrolselect { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412033), TypeLibFunc((short) 20)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412033), TypeLibFunc((short) 20)] get; }
        [DispId(0x449)]
        string URLUnencoded { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x449)] get; }
        [DispId(-2147412036)]
        object onmousewheel { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412036), TypeLibFunc((short) 20)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412036), TypeLibFunc((short) 20)] get; }
        [DispId(0x44a)]
        IHTMLDOMNode doctype { [return: MarshalAs(UnmanagedType.Interface)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x44a)] get; }
        [DispId(0x44b)]
        IHTMLDOMImplementation implementation { [return: MarshalAs(UnmanagedType.Interface)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x44b)] get; }
        [return: MarshalAs(UnmanagedType.Interface)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x44c)]
        IHTMLDOMAttribute createAttribute([In, MarshalAs(UnmanagedType.BStr)] string bstrattrName);
        [return: MarshalAs(UnmanagedType.Interface)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x44d)]
        IHTMLDOMNode createComment([In, MarshalAs(UnmanagedType.BStr)] string bstrdata);
        [DispId(-2147412021)]
        object onfocusin { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412021), TypeLibFunc((short) 20)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412021)] get; }
        [DispId(-2147412020)]
        object onfocusout { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412020), TypeLibFunc((short) 20)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412020)] get; }
        [DispId(-2147412025)]
        object onactivate { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412025), TypeLibFunc((short) 20)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412025)] get; }
        [DispId(-2147412024)]
        object ondeactivate { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412024)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412024), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412022)]
        object onbeforeactivate { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412022)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412022)] get; }
        [DispId(-2147412035)]
        object onbeforedeactivate { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412035), TypeLibFunc((short) 20)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412035), TypeLibFunc((short) 20)] get; }
        [DispId(0x44e)]
        string compatMode { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x44e)] get; }
        [DispId(-2147417066)]
        int nodeType { [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417066)] get; }
        [DispId(-2147417065)]
        IHTMLDOMNode parentNode { [return: MarshalAs(UnmanagedType.Interface)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417065)] get; }
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417064)]
        bool hasChildNodes();
        [DispId(-2147417063)]
        object childNodes { [return: MarshalAs(UnmanagedType.IDispatch)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417063)] get; }
        [DispId(-2147417062)]
        object attributes { [return: MarshalAs(UnmanagedType.IDispatch)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417062)] get; }
        [return: MarshalAs(UnmanagedType.Interface)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417061)]
        IHTMLDOMNode insertBefore([In, MarshalAs(UnmanagedType.Interface)] IHTMLDOMNode newChild, [In, Optional, MarshalAs(UnmanagedType.Struct)] object refChild);
        [return: MarshalAs(UnmanagedType.Interface)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417060)]
        IHTMLDOMNode removeChild([In, MarshalAs(UnmanagedType.Interface)] IHTMLDOMNode oldChild);
        [return: MarshalAs(UnmanagedType.Interface)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417059)]
        IHTMLDOMNode replaceChild([In, MarshalAs(UnmanagedType.Interface)] IHTMLDOMNode newChild, [In, MarshalAs(UnmanagedType.Interface)] IHTMLDOMNode oldChild);
        [return: MarshalAs(UnmanagedType.Interface)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417051)]
        IHTMLDOMNode cloneNode([In] bool fDeep);
        [return: MarshalAs(UnmanagedType.Interface)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417046)]
        IHTMLDOMNode removeNode([In, Optional] bool fDeep /* = false */);
        [return: MarshalAs(UnmanagedType.Interface)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417044)]
        IHTMLDOMNode swapNode([In, MarshalAs(UnmanagedType.Interface)] IHTMLDOMNode otherNode);
        [return: MarshalAs(UnmanagedType.Interface)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417045)]
        IHTMLDOMNode replaceNode([In, MarshalAs(UnmanagedType.Interface)] IHTMLDOMNode replacement);
        [return: MarshalAs(UnmanagedType.Interface)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417039)]
        IHTMLDOMNode appendChild([In, MarshalAs(UnmanagedType.Interface)] IHTMLDOMNode newChild);
        [DispId(-2147417038)]
        string nodeName { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417038)] get; }
        [DispId(-2147417037)]
        object nodeValue { [param: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417037)] set; [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417037)] get; }
        [DispId(-2147417036)]
        IHTMLDOMNode firstChild { [return: MarshalAs(UnmanagedType.Interface)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417036)] get; }
        [DispId(-2147417035)]
        IHTMLDOMNode lastChild { [return: MarshalAs(UnmanagedType.Interface)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417035)] get; }
        [DispId(-2147417034)]
        IHTMLDOMNode previousSibling { [return: MarshalAs(UnmanagedType.Interface)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417034)] get; }
        [DispId(-2147417033)]
        IHTMLDOMNode nextSibling { [return: MarshalAs(UnmanagedType.Interface)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417033)] get; }
        [DispId(-2147416999)]
        object ownerDocument { [return: MarshalAs(UnmanagedType.IDispatch)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147416999)] get; }
    }
}

