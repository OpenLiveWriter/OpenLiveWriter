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
    [Guid("0000010b-0000-0000-C000-000000000046")]
    public interface IPersistFile
    {
        [PreserveSig]
        int GetClassID(
            [Out] out Guid pClassID);

        [PreserveSig]
        int IsDirty();

        void Load(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName,
            [In] STGM dwMode);

        void Save(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName,
            [In, MarshalAs(UnmanagedType.Bool)] bool fRemember);

        void SaveCompleted(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName);

        void GetCurFile(
            [Out] out IntPtr ppszFileName  /* (LPOLESTR*) */ );
    }
}

