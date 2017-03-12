// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Runtime.InteropServices;
using mshtml;

namespace OpenLiveWriter.Mshtml
{
    /// <summary>
    /// Interface used for customizing editing behavior of MSHTML
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("3050f648-98b5-11cf-bb82-00aa00bdce0b")]
    public interface IMarkupContainer2Raw
    {
        void OwningDoc(
            [Out] out IHTMLDocument2 ppDoc);

        void CreateChangeLog(
            [In] IHTMLChangeSink pChangeSink,
            [Out] out IHTMLChangeLog ppChangeLog,
            [In, MarshalAs(UnmanagedType.Bool)] bool fForward,
            [In, MarshalAs(UnmanagedType.Bool)] bool fBackward);

        void RegisterForDirtyRange(
            [In] IHTMLChangeSink pChangeSink,
            [Out] out uint pdwCookie);

        void UnRegisterForDirtyRange(
            [In] uint dwCookie);

        void GetAndClearDirtyRange(
            [In] uint dwCookie,
            [In] IMarkupPointerRaw pIPointerBegin,
            [In] IMarkupPointerRaw pIPointerEnd);

        [PreserveSig]
        int GetVersionNumber();

        void GetMasterElement(
            [Out] out IHTMLElement ppElementMaster);
    }
}
