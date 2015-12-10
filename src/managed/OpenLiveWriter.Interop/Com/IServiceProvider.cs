// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Com
{
    /// <summary>
    /// Generic access mechanism to locate a GUID identified service.
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("6d5140c1-7436-11ce-8034-00aa006009fa")]
    public interface IServiceProvider
    {
        /// <summary>
        /// Factory method for services exposed through IServiceProvider
        /// </summary>
        /// <param name="guid"> Unique identifier of the service (an SID)</param>
        /// <param name="riid">Unique identifier of the interface the caller wishes to receive
        /// for the service</param>
        /// <param name="Obj">Address of the caller-allocated variable to receive the
        /// interface pointer of the service</param>
        void QueryService(
            [In] ref Guid guid,
            [In] ref Guid riid,
            [Out, MarshalAs(UnmanagedType.Interface)] out Object Obj);
    }

    /// <summary>
    /// Generic access mechanism to locate a GUID identified service.
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("6d5140c1-7436-11ce-8034-00aa006009fa")]
    public interface IServiceProviderRaw
    {
        /// <summary>
        /// Factory method for services exposed through IServiceProvider
        /// </summary>
        /// <param name="guid"> Unique identifier of the service (an SID)</param>
        /// <param name="riid">Unique identifier of the interface the caller wishes to receive
        /// for the service</param>
        /// <param name="Obj">Address of the caller-allocated variable to receive the
        /// interface pointer of the service</param>
        [PreserveSig]
        int QueryService(
            [In] ref Guid guid,
            [In] ref Guid riid,
            [Out] out IntPtr service);
    }

}
