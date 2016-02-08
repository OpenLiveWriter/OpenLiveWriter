// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Com
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("00000103-0000-0000-C000-000000000046")]
    public interface IEnumFORMATETC
    {

        [PreserveSig]
        int Next(
            uint celt,
            [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)]
            FORMATETC[] rgelt,
            IntPtr pceltFetched);

        /*
        [PreserveSig]
        int Next(
            uint celt,
            IntPtr rgelt,
            IntPtr pceltFetched);
        */

        [PreserveSig]
        int Skip(uint celt);

        void Reset();

        void Clone(out IEnumFORMATETC ppenum);
    }

}

