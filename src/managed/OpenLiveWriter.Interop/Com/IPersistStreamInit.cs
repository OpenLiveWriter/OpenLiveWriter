// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace OpenLiveWriter.Interop.Com
{
    /// <summary>
    ///
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("7FD52380-4E07-101B-AE2D-08002B2EC713")]
    public interface IPersistStreamInit
    {
        [PreserveSig]
        int GetClassID(
            [Out] out Guid pClassID);

        [PreserveSig]
        int IsDirty();

        void Load(
            [In] IStream pStm);

        void Save(
            [In] IStream pStm,
            [In, MarshalAs(UnmanagedType.Bool)] bool fClearDirty);

        void GetSizeMax(
            [Out] out UInt64 pcbSize);

        void InitNew();

    }
}

