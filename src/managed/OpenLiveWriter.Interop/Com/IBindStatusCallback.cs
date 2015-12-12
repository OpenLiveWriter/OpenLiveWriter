// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Com
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("79eac9c1-baf9-11ce-8c82-00aa004ba90b")]
    public interface IBindStatusCallback
    {
        void OnStartBinding(
            [In] uint dwReserved,
            [In] IntPtr pib);

        void GetPriority(
            [Out] out int pnPriority);

        void OnLowResource(
            [In] uint reserved);

        [PreserveSig]
        int OnProgress(
            [In] uint ulProgress,
            [In] uint ulProgressMax,
            [In] BINDSTATUS ulStatusCode,
            [In, MarshalAs(UnmanagedType.LPWStr)] string szStatusText);

        void OnStopBinding(
            [In] int hresult,
            [In, MarshalAs(UnmanagedType.LPWStr)] string szError);

        void GetBindInfo(
            [In, Out] ref uint grfBINDF,
            [In, Out] ref BINDINFO pbindinfo);

        void OnDataAvailable(
            [In] BSCF grfBSCF,
            [In] uint dwSize,
            [In] ref FORMATETC pformatetc,
            [In] ref STGMEDIUM pstgmed);

        void OnObjectAvailable(
            [In] ref Guid riid,
            [In] IntPtr punk);
    }

}

