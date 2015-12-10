

using System.Runtime.InteropServices;
using mshtml;

namespace OpenLiveWriter.Mshtml
{
    /// <summary>
    /// Interface used for customizing editing behavior of MSHTML
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("3050f662-98b5-11cf-bb82-00aa00bdce0b")]
    public interface IHTMLEditDesignerRaw
    {
        [PreserveSig]
        int PreHandleEvent(
            [In] int inEvtDispId,
            [In] IHTMLEventObj pIEventObj);

        [PreserveSig]
        int PostHandleEvent(
            [In] int inEvtDispId,
            [In] IHTMLEventObj pIEventObj);

        [PreserveSig]
        int TranslateAccelerator(
            [In] int inEvtDispId,
            [In] IHTMLEventObj pIEventObj);

        void PostEditorEventNotify(
            [In] int inEvtDispId,
            [In] IHTMLEventObj pIEventObj);
    }
}

