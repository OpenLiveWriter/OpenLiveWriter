// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Com.ActiveDocuments
{
    /// <summary>
    ///
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("b722bcca-4e68-101b-a2bc-00aa00404770")]
    public interface IContinueCallback
    {
        [PreserveSig]
        int FContinue() ;

        [PreserveSig]
        int FContinuePrinting(
            [In] int nCntPrinted,
            [In] int nCurPage,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwszPrintStatus ) ;
    }
}
