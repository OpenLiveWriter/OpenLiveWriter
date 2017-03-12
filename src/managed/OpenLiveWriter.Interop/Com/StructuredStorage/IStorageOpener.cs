// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Interop.Com.StructuredStorage
{
    /// <summary>
    /// Interface for creating or opening a Storage.  Implementations of Storage
    /// should provide an IStorageOpener specific to their implementation.
    /// </summary>
    public interface IStorageOpener
    {
        /// <summary>
        /// Creates a new storage.
        /// </summary>
        /// <param name="pwcsName">The name of the storage to be created.</param>
        /// <param name="ppstg">The IStorage that is created.</param>
        void CreateStorage(
            string pwcsName,
            out IStorage ppstg);

        /// <summary>
        /// Opens an existing storage.
        /// </summary>
        /// <param name="pwcsName">The name of the storage to be opened.</param>
        /// <param name="writable">True to open the storage in readwrite, false if readonly.</param>
        /// <param name="ppstg">The IStorage that is opened.</param>
        bool OpenStorage(
            string pwcsName,
            bool writable,
            out IStorage ppstg);

        /// <summary>
        /// Opens an existing storage.
        /// </summary>
        /// <param name="pwcsName">The name of the storage to be opened.</param>
        /// <param name="flags">The STGM flags to use when opening the storage.</param>
        /// <param name="ppstg">The IStorage that is opened.</param>
        bool OpenStorage(
            string pwcsName,
            STGM flags,
            out IStorage ppstg);
    }
}
