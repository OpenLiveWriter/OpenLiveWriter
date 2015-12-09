// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using OpenLiveWriter.Interop.Com.StructuredStorage;

namespace OpenLiveWriter.Interop.Com
{
    /// <summary>
    ///
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("0000010a-0000-0000-C000-000000000046")]
    public interface IPersistStorage
    {
        [PreserveSig]
        int GetClassID(
            [Out] out Guid pClassID);

        [PreserveSig]
        int IsDirty();

        void InitNew(
            [In] IStorage pStg);

        void Load(
            [In] IStorage pStg);

        void Save(
            [In] IStorage pStgSave,
            [In, MarshalAs(UnmanagedType.Bool)] bool fSameAsLoad);

        void SaveCompleted(
            [In] IStorage pStgNew);

        void HandsOffStorage();
    }
}

