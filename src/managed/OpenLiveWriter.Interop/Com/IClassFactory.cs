// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Com
{

    /// <summary>
    /// The IClassFactory interface contains two methods intended to deal with an
    /// entire class of objects, and so is implemented on the class object for a
    /// specific class of objects (identified by a CLSID). The first method,
    /// CreateInstance, creates an uninitialized object of a specified CLSID,
    /// and the second, LockServer, locks the object's server in memory, allowing
    /// new objects to be created more quickly.
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("00000001-0000-0000-C000-000000000046")]
    public interface IClassFactory
    {
        /// <summary>
        /// Creates an uninitialized object.
        /// </summary>
        /// <param name="pUnkOuter">[in] If the object is being created as part of an
        /// aggregate, pointer to the controlling IUnknown interface of the aggregate.
        /// Otherwise, pUnkOuter must be NULL. </param>
        /// <param name="riid">[in] Reference to the identifier of the interface
        /// to be used to communicate with the newly created object. If pUnkOuter
        /// is NULL, this parameter is frequently the IID of the initializing
        /// interface; if pUnkOuter is non-NULL, riid must be IID_IUnknown (defined
        /// in the header as the IID for IUnknown). /param>
        /// <param name="ppv">[out] Address of pointer variable that receives the
        /// interface pointer requested in riid. Upon successful return, *ppvObject
        /// contains the requested interface pointer. If the object does not support
        /// the interface specified in riid, the implementation must set *ppvObject
        /// to NULL.</param>
        void CreateInstance(
            [In] IntPtr pUnkOuter,
            [In] ref Guid riid,
            out IntPtr ppv);

        /// <summary>
        /// Called by the client of a class object to keep a server open in memory,
        /// allowing instances to be created more quickly.
        /// </summary>
        /// <param name="lck">[in] If TRUE, increments the lock count; if FALSE,
        /// decrements the lock count.</param>
        void LockServer(
            [In, MarshalAs(UnmanagedType.Bool)] bool lck);
    }
}
