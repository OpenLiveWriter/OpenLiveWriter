// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Com.StructuredStorage
{
    /// <summary>
    /// The ILockBytes interface is implemented on a byte array object that is backed
    /// by some physical storage, such as a disk file, global memory, or a database.
    /// It is used by a COM compound file storage object to give its root storage
    /// access to the physical device, while isolating the root storage from the
    /// details of accessing the physical storage.
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("0000000a-0000-0000-C000-000000000046")]
    public interface ILockBytes
    {
        /// <summary>
        /// Reads a specified number of bytes starting at a specified offset from the
        /// beginning of the byte array.
        /// </summary>
        /// <param name="ulOffset">[in] Specifies the starting point from the beginning of the byte array for reading data. </param>
        /// <param name="pv">[in] Pointer to the buffer into which the byte array is read.</param>
        /// <param name="cb">[in] Specifies the number of bytes of data to attempt to read from the byte array.</param>
        /// <param name="pcbRead">[out] Pointer to a location where this method writes the actual number of bytes read from the byte array. You can set this pointer to NULL to indicate that you are not interested in this value. In this case, this method does not provide the actual number of bytes that were read.</param>
        void ReadAt(
            [In] UInt64 ulOffset,
            [Out] IntPtr pv,
            [In] UInt32 cb,
            [Out] out UInt32 pcbRead);

        /// <summary>
        /// Writes a specified number of bytes to a specified location in the byte array.
        /// </summary>
        /// <param name="ulOffset">[in] Specifies the starting point from the beginning of the byte array for the data to be written.</param>
        /// <param name="pv">[in] Pointer to the buffer containing the data to be written.</param>
        /// <param name="cb">[in] Specifies the number of bytes of data to attempt to write into the byte array.</param>
        /// <param name="pcbWritten">[out] Pointer to a location where this method specifies the actual number of bytes written to the byte array. You can set this pointer to NULL to indicate that you are not interested in this value. In this case, this method does not provide the actual number of bytes written.</param>
        void WriteAt(
            [In] UInt64 ulOffset,
            [In] IntPtr pv,
            [In] UInt32 cb,
            [Out] out UInt32 pcbWritten);

        /// <summary>
        /// Ensures that any internal buffers maintained by the byte array object are
        /// written out to the backing storage
        /// </summary>
        void Flush();

        /// <summary>
        /// Changes the size of the byte array
        /// </summary>
        /// <param name="cb">[in] Specifies the new size of the byte array as a number of bytes.</param>
        void SetSize(
            [In] UInt64 cb);

        /// <summary>
        /// Restricts access to a specified range of bytes in the byte array
        /// </summary>
        /// <param name="libOffset">[in] Specifies the byte offset for the beginning of the range.</param>
        /// <param name="cb">[in] Specifies, in bytes, the length of the range to be restricted.</param>
        /// <param name="dwLockType">[in] Specifies the type of restrictions being requested on accessing the range. This parameter uses one of the values from the LOCKTYPE enumeration.</param>
        void LockRegion(
            [In] UInt64 libOffset,
            [In] UInt64 cb,
            [In] uint dwLockType);

        /// <summary>
        /// Removes the access restriction on a range of bytes previously restricted
        /// with ILockBytes::LockRegion
        /// </summary>
        /// <param name="libOffset">[in] Specifies the byte offset for the beginning of the range.</param>
        /// <param name="cb">[in] Specifies, in bytes, the length of the range that is restricted.</param>
        /// <param name="dwLockType">[in] Specifies the type of access restrictions previously placed on the range. This parameter uses a value from the LOCKTYPE enumeration.</param>
        void UnlockRegion(
            [In] UInt64 libOffset,
            [In] UInt64 cb,
            [In] LOCKTYPE dwLockType);

        /// <summary>
        /// Retrieves a STATSTG structure for this byte array object
        /// </summary>
        /// <param name="pstatstg">[out] Pointer to a STATSTG structure in which this method places information about this byte array object. The pointer is NULL if an error occurs.</param>
        /// <param name="grfStatFlag">[in] Specifies whether this method should supply the pwcsName member of the STATSTG structure through values taken from the STATFLAG enumeration. If the STATFLAG_NONAME is specified, the pwcsName member of STATSTG is not supplied, thus saving a memory-allocation operation. The other possible value, STATFLAG_DEFAULT, indicates that all members of the STATSTG structure be supplied</param>
        void Stat(
            [Out] out System.Runtime.InteropServices.ComTypes.STATSTG pstatstg,
            [In] STATFLAG grfStatFlag);
    }

}

