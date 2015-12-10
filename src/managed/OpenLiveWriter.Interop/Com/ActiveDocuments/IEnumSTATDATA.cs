// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Com.ActiveDocuments
{
    /// <summary>
    ///
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("00000105-0000-0000-C000-000000000046")]
    public interface IEnumSTATDATA
    {
        /// <summary>
        /// Retrieves the next item in the enumeration sequence (note, the way we
        /// have declared this interface we only support retrieving a single-item
        /// at a time. This simplifies marshalling and error-handling). Users
        /// must pass in 1 for the celt parameter and IntPtr.Zero for the
        /// pceltFetched parameter. Next will return S_OK if an item was fetched,
        /// otherwise it will return S_FALSE If there are fewer than
        /// </summary>
        /// <param name="celt">[In] Number of items to fetch (must be 1)</param>
        /// <param name="rgelt">[Out] Storage for item to be fetched</param>
        /// <param name="pceltFetched">[Out] Items fetched (not used, always pass
        /// IntPtr.Zero)</param>
        /// <returns>S_OK if item fetched, else S_FALSE</returns>
        [PreserveSig]
        int Next(
            [In] uint celt, // must be 1
            [Out] out STATDATA rgelt,
            [In] IntPtr pceltFetched); // must be IntPtr.Zero

        /// <summary>
        /// Skips over the next specified number of elements in the enumeration sequence
        /// </summary>
        /// <param name="celt">[in] Number of elements to be skipped</param>
        /// <returns>S_OK if the number of elements skipped is celt;
        /// otherwise S_FALSE</returns>
        [PreserveSig]
        int Skip(
            [In] uint celt);

        /// <summary>
        /// Resets the enumeration sequence to the beginning
        /// </summary>
        void Reset();

        /// <summary>
        /// Creates another enumerator that contains the same enumeration state as
        /// the current one. Using this function, a client can record a particular
        /// point in the enumeration sequence and then return to that point at a
        /// later time. The new enumerator supports the same interface as the
        /// original one
        /// </summary>
        /// <param name="ppenum">[Out] Storage for clone of this enumeration</param>
        void Clone(
            [Out] out IEnumSTATDATA ppenum);
    }
}
