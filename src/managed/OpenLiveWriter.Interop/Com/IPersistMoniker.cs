// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using OpenLiveWriter.Interop.Com.StructuredStorage;

namespace OpenLiveWriter.Interop.Com
{
    /// <summary>
    ///
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("79eac9c9-baf9-11ce-8c82-00aa004ba90b")]
    public interface IPersistMoniker
    {
        [PreserveSig]
        int GetClassID(
            [Out] out Guid pClassID);

        [PreserveSig]
        int IsDirty();

        void Load(
            [In] bool fFullyAvailable,
            [In] IMoniker pimkName,
            [In] IBindCtx pibc,
            [In] STGM dwMode);

        void Save(
            [In] IMoniker pimkName,
            [In] IBindCtx pbc,
            [In] bool fRemember);

        void SaveCompleted(
            [In] IMoniker pimkName,
            [In] IBindCtx pbc);

        void GetCurMoniker(
            [Out] out IMoniker ppimkName);
    }
}

