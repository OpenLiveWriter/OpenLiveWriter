// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace OpenLiveWriter.Interop.Com.ActiveDocuments
{
    /// <summary>
    ///
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("00000118-0000-0000-C000-000000000046")]
    public interface IOleClientSite
    {
        void SaveObject();

        [PreserveSig]
        int GetMoniker(
            [In] OLEGETMONIKER dwAssign,
            [In] OLEWHICHMK dwWhichMoniker,
            [Out] out IMoniker ppmk);

        [PreserveSig]
        int GetContainer(
            [Out] out IOleContainer ppContainer);

        void ShowObject();

        void OnShowWindow(
            [In, MarshalAs(UnmanagedType.Bool)] bool fShow);

        [PreserveSig]
        int RequestNewObjectLayout();
    }
}
