namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("3050F662-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceType((short) 1)]
    public interface IHTMLEditDesigner
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void PreHandleEvent([In] int inEvtDispId, [In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pIEventObj);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void PostHandleEvent([In] int inEvtDispId, [In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pIEventObj);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void TranslateAccelerator([In] int inEvtDispId, [In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pIEventObj);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void PostEditorEventNotify([In] int inEvtDispId, [In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pIEventObj);
    }
}

