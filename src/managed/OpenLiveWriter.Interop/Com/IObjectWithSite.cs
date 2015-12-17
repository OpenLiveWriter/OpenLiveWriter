// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Com
{
    /// <summary>
    /// Interface for communication between an object and its site in a container
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("FC4801A3-2BA9-11CF-A229-00AA003D7352")]
    public interface IObjectWithSite
    {
        /// <summary>
        /// In the context of a DeskBand, provides the IUnknown site pointer for Explorer
        /// </summary>
        /// <param name="pUnkSite">Browser interface as IUnknown</param>
        [PreserveSig]
        int SetSite([In, MarshalAs(UnmanagedType.IUnknown)] object pUnkSite);

        /// <summary>
        /// Returns the last site set with SetSite
        /// </summary>
        /// <param name="riid">The interface identifer whose pointer should be
        /// returned in ppvSite</param>
        /// <param name="ppvSite">The last site pointer passed in to SetSite</param>
        void GetSite(ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppvSite);
    }
}
