// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Com.Ribbon
{
    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid("00000100-0000-0000-C000-000000000046")
    ]
    public interface IEnumUnknown
    {
        [PreserveSig]
        Int32 Next(
            uint celt,
            [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.IUnknown, SizeParamIndex = 0)] object[] rgelt,
            IntPtr pceltFetched
            );

        [PreserveSig]
        Int32 Skip(
            uint celt
            );

        void Reset();

        IEnumUnknown Clone();
    }
}
