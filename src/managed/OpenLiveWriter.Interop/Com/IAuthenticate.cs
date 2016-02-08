// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Com
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("79eac9d0-baf9-11ce-8c82-00aa004ba90b")]
    public interface IAuthenticate
    {
        [PreserveSig]
        int Authenticate(
            [Out] out IntPtr phwnd,
            [Out] out IntPtr pszUsername,
            [Out] out IntPtr pszPassword);
    }
}
