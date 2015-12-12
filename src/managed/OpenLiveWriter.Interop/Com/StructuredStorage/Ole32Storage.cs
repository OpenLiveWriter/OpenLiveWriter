// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace OpenLiveWriter.Interop.Com.StructuredStorage
{
    /// <summary>
    /// Provides marshalling layer for the creation of Ole 'DocFile'
    /// </summary>
    public class Ole32Storage
    {
        /// <summary>
        /// StgCreateDocfile creates a new compound file storage object
        /// using the COM-provided compound file implementation for the IStorage interface.
        /// </summary>
        /// <param name="pwcsName">Pointer to a null-terminated Unicode string
        /// name for the compound file being created. It is passed uninterpreted
        /// to the file system. This can be a relative name or NULL. If NULL, a
        /// temporary compound file is allocated with a unique name. </param>
        /// <param name="grfMode">Specifies the access mode to use when opening
        /// the new storage object. For more information, see the STGM enumeration.
        /// If the caller specifies transacted mode together with STGM_CREATE
        /// or STGM_CONVERT, the overwrite or conversion takes place when the
        /// commit operation is called for the root storage. If IStorage::Commit
        /// is not called for the root storage object, previous contents of
        /// the file will be restored. STGM_CREATE and STGM_CONVERT cannot be
        /// combined with the STGM_NOSNAPSHOT flag, because a snapshot copy is
        /// required when a file is overwritten or converted in the transacted
        /// mode. </param>
        /// <param name="reserved">Reserved for future use; must be zero. </param>
        /// <param name="ppstgOpen">Pointer to the location of the IStorage
        /// pointer to the new storage object.</param>
        /// <returns>HResult indicating status code.</returns>
        [DllImport("Ole32.dll", CharSet = CharSet.Unicode)]
        public static extern int StgCreateDocfile(
            [MarshalAs(UnmanagedType.LPTStr)] string pwcsName,
            STGM grfMode,
            uint reserved,
            out IStorage ppstgOpen);

        /// <summary>
        /// StgOpenStorage opens an existing root storage object in the file
        /// system. You can use this function to open compound files, but you
        /// cannot use it to open directories, files, or summary catalogs.
        /// Nested storage objects can only be opened using their parent's
        /// IStorage::OpenStorage method.
        /// </summary>
        /// <param name="pwcsName">Pointer to the path of the null-terminated
        /// Unicode string file containing the storage object to open. This
        /// parameter is ignored if the pstgPriority parameter is not NULL. </param>
        /// <param name="pstgPriority">Most often NULL. If not NULL, this parameter
        /// is used instead of the pwcsName parameter to specify the pointer to the
        /// IStorage interface on the storage object to open. It points to a
        /// previous opening of a root storage object, most often one that
        /// was opened in priority mode. After the StgOpenStorage function
        /// returns, the storage object specified in the pstgPriority parameter
        /// on function entry is not valid, and can no longer be used. Instead,
        /// use the storage object specified in the ppStgOpen parameter.</param>
        /// <param name="grfMode">Specifies the access mode to use to open the storage
        /// object. </param>
        /// <param name="snbExclude">If not NULL, pointer to a block of elements
        /// in the storage that are to be excluded as the storage object is opened.
        /// The exclusion occurs regardless of whether a snapshot copy happens
        /// on the open. May be NULL. </param>
        /// <param name="reserved">Indicates reserved for future use; must be zero. </param>
        /// <param name="ppstgOpen">Pointer IStorage* pointer variable that
        /// receives the interface pointer to the opened storage. </param>
        /// <returns>HResult indicating status code.</returns>
        [DllImport("Ole32.dll", CharSet = CharSet.Unicode)]
        public static extern int StgOpenStorage(
            [MarshalAs(UnmanagedType.LPTStr)] string pwcsName,
            IStorage pstgPriority,
            STGM grfMode,
            IntPtr snbExclude, // exclude not supported, pass null
            uint reserved,
            out IStorage ppstgOpen);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct STGOPTIONS
        {
            public ushort usVersion;
            public ushort reserved;
            public int ulSectorSize;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pwcsTemplateFile;
        }

        [DllImport("Ole32.dll", CharSet = CharSet.Unicode)]
        public static extern int StgOpenStorageEx(
            [MarshalAs(UnmanagedType.LPTStr)] string pwcsName,
            STGM grfMode,
            STGFMT stgfmt,
            uint grfAttrs,
            ref STGOPTIONS pStgOptions,
            IntPtr reserved2, // must be zero
            ref Guid riid,
            out IStorage ppObjectOpen
            );

        [DllImport("Ole32.dll", CharSet = CharSet.Unicode)]
        public unsafe static extern int StgCreateStorageEx(
            [MarshalAs(UnmanagedType.LPTStr)] string pwcsName,
            STGM grfMode,
            STGFMT stgfmt,
            uint grfAttrs,
            ref STGOPTIONS pStgOptions,
            IntPtr reserved2, // must be zero
            ref Guid riid,
            out IStorage ppObjectOpen
            );

        /// <summary>
        /// Indicates whether a particular disk file contains a storage object.
        /// </summary>
        /// <param name="pwcsName">Pointer to the null-terminated Unicode string
        /// name of the disk file to be examined. The pwcsName parameter is
        /// passed uninterpreted to the underlying file system. </param>
        /// <returns>HResult indicating status code</returns>
        [DllImport("Ole32.dll", CharSet = CharSet.Unicode)]
        public static extern int StgIsStorageFile(
            [MarshalAs(UnmanagedType.LPTStr)] string pwcsName);

        /// <summary>
        /// Reads the CLSID previously written to a storage object with the WriteClassStg function.
        /// </summary>
        /// <param name="pStg">Pointer to the IStorage interface on the storage object
        /// containing the CLSID to be retrieved. </param>
        /// <param name="pclsid">Pointer to where the CLSID is written. May return CLSID_NULL. </param>
        [DllImport("Ole32.dll")]
        public static extern int ReadClassStg(
            IStorage pStg,
            ref Guid pclsid
            );

        /// <summary>
        /// Stores the specified class identifier (CLSID) in a storage object.
        /// </summary>
        /// <param name="pStg">IStorage pointer to the storage object that gets a new CLSID. </param>
        /// <param name="rclsid">Pointer to the CLSID to be stored with the object. </param>
        [DllImport("Ole32.dll")]
        public static extern int WriteClassStg(
            IStorage pStg,
            ref Guid rclsid
        );

        /// <summary>
        /// Creates a byte array object that allows you to use global memory as the
        /// physical device underneath a compound file implementation. This object
        /// supports a COM implementation of the ILockBytes interface.
        /// </summary>
        /// <param name="hGlobal">[in] Memory handle allocated by the GlobalAlloc function. The handle must be allocated as moveable and nondiscardable. If the handle is to be shared between processes, it must also be allocated as shared. New handles should be allocated with a size of zero. If hGlobal is NULL, CreateILockBytesOnHGlobal internally allocates a new shared memory block of size zero.</param>
        /// <param name="fDeleteOnRelease">[in] Determines whether the underlying handle for this byte array object should be automatically freed when the object is released. If set to FALSE, the caller must free the hGlobal after the final release. If set to TRUE, the final release will automatically free the hGlobal parameter.</param>
        /// <param name="ppLkbyt">[out] Address of ILockBytes pointer variable that receives the interface pointer to the new byte array object.</param>
        /// <returns></returns>
        [DllImport("Ole32.dll")]
        public static extern int CreateILockBytesOnHGlobal(
            [In] IntPtr hGlobal,
            [In] int fDeleteOnRelease,
            [Out] out ILockBytes ppLkbyt
        );

        /// <summary>
        /// Creates a stream object stored in global memory.
        /// </summary>
        /// <param name="hGlobal">[in] Memory handle allocated by the GlobalAlloc function. The handle must be allocated as movable and nondiscardable. If the handle is to be shared between processes, it must also be allocated as shared. New handles should be allocated with a size of zero. If hGlobal is NULL, the CreateStreamOnHGlobal function internally allocates a new shared memory block of size zero.</param>
        /// <param name="fDeleteOnRelease">[in] Determines whether the underlying handle for this byte array object should be automatically freed when the object is released. If set to FALSE, the caller must free the hGlobal after the final release. If set to TRUE, the final release will automatically free the hGlobal parameter.</param>
        /// <param name="ppLkbyt">[out] Address of IStream* pointer variable that receives the interface pointer to the new stream object. Its value cannot be NULL.</param>
        /// <returns>S_OK if the stream was successfully created</returns>
        [DllImport("Ole32.dll")]
        public static extern int CreateStreamOnHGlobal(
            [In] IntPtr hGlobal,
            [In] int fDeleteOnRelease,
            [Out] out IStream ppstm
            );

        /// <summary>
        /// Creates and opens a new compound file storage object on top of a byte-array
        /// object provided by the caller. The storage object supports the COM-provided,
        /// compound-file implementation for the IStorage interface
        /// </summary>
        /// <param name="plkbyt">[in] Pointer to the ILockBytes interface on the underlying byte-array object on which to create a compound file.</param>
        /// <param name="grfMode">[in] Specifies the access mode to use when opening the new compound file. For more information, see the STGM enumeration. </param>
        /// <param name="reserved">[in] Reserved for future use; must be zero.</param>
        /// <param name="ppstgOpen">[out] Pointer to the location of the IStorage pointer on the new storage object.</param>
        /// <returns></returns>
        [DllImport("Ole32.dll")]
        public static extern int StgCreateDocfileOnILockBytes(
            [In] ILockBytes plkbyt,
            [In] STGM grfMode,
            [In] uint reserved,
            [Out] out IStorage ppstgOpen
        );

        /// <summary>
        /// Retrieves a global memory handle to a byte array object created using the
        /// CreateILockBytesOnHGlobal function. The contents of the returned memory
        /// handle can be written to a clean disk file, and then opened as a storage
        /// object using the StgOpenStorage function.
        /// </summary>
        /// <param name="pLkbyt">[in] Pointer to the ILockBytes interface on the
        /// byte-array object previously created by a call to the
        /// CreateILockBytesOnHGlobal function.</param>
        /// <param name="phglobal">[out] Pointer to the current memory handle used by the specified byte-array object.</param>
        /// <returns></returns>
        [DllImport("Ole32.dll")]
        public static extern int GetHGlobalFromILockBytes(
            [In] ILockBytes pLkbyt,
            [Out] out IntPtr phglobal
        );
    }
}
