// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Com
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("79eac9d7-bafa-11ce-8c82-00aa004ba90b")]
    public interface IHttpSecurity
    {
        [PreserveSig]
        int GetWindow(
            [In] ref Guid rguidReason,
            [Out] out IntPtr phwnd);

        [PreserveSig]
        int OnSecurityProblem(
            uint dwProblem);
    };

    public struct RPC_E
    {
        public const int RETRY = unchecked((int)(0x80010109));
        public const int CALL_FAILED_DNE = unchecked((int)(0x800706BF));

    }
}
