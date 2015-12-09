// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Com
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("79eac9d5-bafa-11ce-8c82-00aa004ba90b")]
    public interface IWindowForBindingUI
    {
        [PreserveSig]
        int GetWindow(
            [In] ref Guid rguidReason,
            [Out] out IntPtr phwnd);
    };
}
