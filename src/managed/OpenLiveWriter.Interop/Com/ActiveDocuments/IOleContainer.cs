// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace OpenLiveWriter.Interop.Com.ActiveDocuments
{
    /// <summary>
    ///
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("0000011B-0000-0000-C000-000000000046")]
    public interface IOleContainer
    {
        [PreserveSig]
        int ParseDisplayName(
            [In] IBindCtx pbc,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszDisplayName,
            [Out] out uint pchEaten,
            [Out] out IMoniker ppmkOut);

        [PreserveSig]
        int EnumObjects(
            [In] OLECONTF grfFlags,
            [Out, MarshalAs(UnmanagedType.Interface)] out IEnumUnknown ppenum);

        void LockContainer(
            [In, MarshalAs(UnmanagedType.Bool)] bool fLock);
    }

    [Flags]
    public enum OLECONTF : uint
    {
        EMBEDDINGS = 1,
        LINKS = 2,
        OTHERS = 4,
        ONLYUSER = 8,
        ONLYIFRUNNING = 16
    };

}

