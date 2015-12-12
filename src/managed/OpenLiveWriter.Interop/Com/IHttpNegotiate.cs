// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Com
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("79eac9d2-baf9-11ce-8c82-00aa004ba90b")]
    public interface IHttpNegotiate
    {
        [PreserveSig]
        int BeginningTransaction(
            [In, MarshalAs(UnmanagedType.LPWStr)] string szURL,
            [In, MarshalAs(UnmanagedType.LPWStr)] string szHeaders,
            [In] uint dwReserved,
            [Out] out IntPtr pszAdditionalHeaders);

        [PreserveSig]
        int OnResponse(
            [In] uint dwResponseCode,
            [In, MarshalAs(UnmanagedType.LPWStr)] string szResponseHeaders,
            [In, MarshalAs(UnmanagedType.LPWStr)] string szRequestHeaders,
            [Out] out IntPtr pszAdditionalRequestHeaders);
    }
}

