// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Com
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("00000002-0000-0000-C000-000000000046")]
    public interface IMalloc
    {
        [PreserveSig]
        IntPtr Alloc(int cb);

        [PreserveSig]
        IntPtr Realloc(IntPtr pv, int cb);

        [PreserveSig]
        void Free(IntPtr pv);

        [PreserveSig]
        uint GetSize(IntPtr pv);

        [PreserveSig]
        int DidAlloc(IntPtr pv);

        [PreserveSig]
        void HeapMinimize();
    }
}

