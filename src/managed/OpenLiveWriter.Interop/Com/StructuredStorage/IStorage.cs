// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace OpenLiveWriter.Interop.Com.StructuredStorage
{
    /// <summary>
    /// Main interface for structured storage
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("0000000b-0000-0000-C000-000000000046")]
    public interface IStorage
    {
        /// <summary>
        /// Creates and opens a stream object with the specified name
        /// contained in this storage object. All elements within a
        /// storage object — both streams and other storage objects —
        /// are kept in the same name space.
        /// </summary>
        /// <param name="pwcsName">Pointer to a wide character null-terminated
        /// Unicode string that contains the name of the newly created stream. The
        /// name can be used later to open or reopen the stream. The name must not
        /// exceed 31 characters in length, not including the string terminator.
        /// The 000 through 01f characters, serving as the first character of the
        /// stream/storage name, are reserved for use by OLE. This is a compound
        /// file restriction, not a structured storage restriction. </param>
        /// <param name="grfMode">Specifies the access mode to use when opening the
        /// newly created stream. For descriptions of the possible values, see the
        /// STGM enumeration. </param>
        /// <param name="reserved1">Reserved for future use; must be zero. </param>
        /// <param name="reserved2">Reserved for future use; must be zero. </param>
        /// <param name="ppstm">On return, pointer to the location of the new
        /// IStream interface pointer. This is only valid if the operation is successful.
        /// When an error occurs, this parameter is set to NULL. </param>
        void CreateStream(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwcsName,
            [In] STGM grfMode,
            [In] uint reserved1, // must be zero
            [In] uint reserved2, // must be zero
            [Out, MarshalAs(UnmanagedType.CustomMarshaler,
                MarshalTypeRef = typeof(StreamMarshaler))]
            out ComStream ppstm
            );

        /// <summary>
        /// Opens an existing stream object within this storage object in the specified access mode.
        /// </summary>
        /// <param name="pwcsName">Pointer to a wide character null-terminated Unicode string that
        /// contains the name of the stream to open. The 000 through 01f characters, serving as
        /// the first character of the stream/storage name, are reserved for use by OLE. This is a
        /// compound file restriction, not a structured storage restriction. </param>
        /// <param name="reserved1">Reserved for future use; must be NULL. </param>
        /// <param name="grfMode">Specifies the access mode to be assigned to the open stream.
        /// For descriptions of the possible values, see the STGM enumeration. Whatever other
        /// modes you may choose, you must at least specify STGM_SHARE_EXCLUSIVE when calling
        /// this method in the compound file implementation. </param>
        /// <param name="reserved2">Reserved for future use; must be zero.</param>
        /// <param name="ppstm">Pointer to IStream pointer variable that receives the
        /// interface pointer to the newly opened stream object. If an error occurs,
        /// *ppstm must be set to NULL. </param>
        void OpenStream(
           [In, MarshalAs(UnmanagedType.LPWStr)] string pwcsName,
           [In] uint reserved1, // must be zero
           [In] STGM grfMode,
           [In] uint reserved2, // must be zero
           [Out, MarshalAs(UnmanagedType.CustomMarshaler,
                    MarshalTypeRef = typeof(StreamMarshaler))]
             out ComStream ppstm
            );

        /// <summary>
        /// Creates and opens a new storage object nested within this storage
        /// object with the specified name in the specified access mode.
        /// </summary>
        /// <param name="pwcsName">Pointer to a wide character null-terminated Unicode
        /// string that contains the name of the newly created storage object. The name
        /// can be used later to reopen the storage object. The name must not exceed 31
        /// characters in length, not including the string terminator. The 000 through
        /// 01f characters, serving as the first character of the stream/storage name,
        /// are reserved for use by OLE. This is a compound file restriction, not a structured
        /// storage restriction. </param>
        /// <param name="grfMode">Specifies the access mode to use when opening the newly created
        /// storage object. For descriptions of the possible values, see the STGM enumeration. </param>
        /// <param name="reserved1">Reserved for future use; must be zero</param>
        /// <param name="reserved2">Reserved for future use; must be zero</param>
        /// <param name="ppstg">When successful, pointer to the location of the IStorage
        /// pointer to the newly created storage object. This parameter is set to NULL
        /// if an error occurs.</param>
        void CreateStorage(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwcsName,
            [In] STGM grfMode,
            [In] uint reserved1, // must be zero
            [In] uint reserved2, // must be zero
            [Out] out IStorage ppstg);

        /// <summary>
        /// Opens an existing storage object with the specified name in the specified access mode.
        /// </summary>
        /// <param name="pwcsName">Pointer to a wide character null-terminated Unicode string that
        /// contains the name of the storage object to open. The 000 through 01f characters, serving
        /// as the first character of the stream/storage name, are reserved for use by OLE. This is a
        /// compound file restriction, not a structured storage restriction. It is ignored if
        /// pstgPriority is non-NULL.</param>
        /// <param name="pstgPriority">Must be NULL. A non-NULL value will return STG_E_INVALIDPARAMETER.</param>
        /// <param name="grfMode">Specifies the access mode to use when opening the storage object. For
        /// descriptions of the possible values, see the STGM enumeration. Whatever other modes you may
        /// choose, you must at least specify STGM_SHARE_EXCLUSIVE when calling this method. </param>
        /// <param name="snbExclude">Must be NULL. A non-NULL value will return STG_E_INVALIDPARAMETER. </param>
        /// <param name="reserved">Reserved for future use; must be zero. </param>
        /// <param name="ppstg">When successful, pointer to the location of an IStorage pointer to the
        /// opened storage object. This parameter is set to NULL if an error occurs. </param>
        void OpenStorage(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwcsName,
            [In] IntPtr pstgPriority, // must be null
            [In] STGM grfMode,
            [In] IntPtr snbExclude, // must be null
            [In] uint reserved, // must be zero
            [Out] out IStorage ppstg);

        /// <summary>
        /// Copies the entire contents of an open storage object to another storage object.
        /// </summary>
        /// <param name="ciidExclude">The number of elements in the array pointed to by rgiidExclude.
        /// If rgiidExclude is NULL, then ciidExclude is ignored. </param>
        /// <param name="rgiidExclude">An array of interface identifiers (IIDs) that either the caller
        /// knows about and does not want copied or that the storage object does not support but whose
        /// state the caller will later explicitly copy. The array can include IStorage, indicating that
        /// only stream objects are to be copied, and IStream, indicating that only storage objects are
        /// to be copied. An array length of zero indicates that only the state exposed by the IStorage
        /// object is to be copied; all other interfaces on the object are to be ignored. Passing NULL
        /// indicates that all interfaces on the object are to be copied. </param>
        /// <param name="snbExclude">A string name block (refer to SNB) that specifies a block of storage
        /// or stream objects that are not to be copied to the destination. These elements are not created
        /// at the destination. If IID_IStorage is in the rgiidExclude array, this parameter is ignored.
        /// This parameter may be NULL.
        /// pstgDest </param>
        /// <param name="pstgDest">Pointer to the open storage object into which this storage object is
        /// to be copied. The destination storage object can be a different implementation of the IStorage
        /// interface from the source storage object. Thus, IStorage::CopyTo can use only publicly
        /// available methods of the destination storage object. If pstgDest is open in transacted
        /// mode, it can be reverted by calling its IStorage::Revert method. </param>
        void CopyTo(
            [In] uint ciidExclude, // exclude not supported, pass 0
            [In] IntPtr rgiidExclude, // exclude not supported, pass null
            [In] IntPtr snbExclude, // exclude not supported, pass null
            [In] IStorage pstgDest);

        /// <summary>
        /// Copies or moves a substorage or stream from this storage object to another storage object.
        /// </summary>
        /// <param name="pwcsName">Pointer to a wide character null-terminated Unicode string that
        /// contains the name of the element in this storage object to be moved or copied.</param>
        /// <param name="pstgDest">Storage pointer to the destination storage object.</param>
        /// <param name="pwcsNewName">Pointer to a wide character null-terminated unicode
        /// string that contains the new name for the element in its new storage object. </param>
        /// <param name="grfFlags">Specifies whether the operation should be a move
        /// (STGMOVE_MOVE) or a copy (STGMOVE_COPY). See the STGMOVE enumeration.</param>
        void MoveElementTo(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwcsName,
            [In] IStorage pstgDest,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwcsNewName,
            [In] STGMOVE grfFlags);

        /// <summary>
        /// Ensures that any changes made to a storage object open in transacted
        /// mode are reflected in the parent storage. For nonroot storage objects
        /// in direct mode, this method has no effect. For a root storage, it
        /// reflects the changes in the actual device; for example, a file on
        /// disk. For a root storage object opened in direct mode, always call
        /// the IStorage::Commit method prior to Release. IStorage::Commit flushes
        /// all memory buffers to the disk for a root storage in direct mode
        /// and will return an error code upon failure. Although Release also
        /// flushes memory buffers to disk, it has no capacity to return any
        /// error codes upon failure. Therefore, calling Release without first
        /// calling Commit causes indeterminate results.
        /// </summary>
        /// <param name="grfCommitFlags">Controls how the changes are committed
        /// to the storage object. See the STGC enumeration for a definition of
        /// these values. </param>
        void Commit(
            [In] STGC grfCommitFlags);

        /// <summary>
        /// Discards all changes that have been made to the storage object since
        /// the last commit operation
        /// </summary>
        void Revert();

        /// <summary>
        /// Retrieves a pointer to an enumerator object that can be used to
        /// enumerate the storage and stream objects contained within this
        /// storage object.
        /// WARNING: If you use this method to retrieve a storage enumerator you
        /// must call Marshal.ReleaseComObject on the returned enumerator as
        /// soon as you are finished using it! Otherwise the enumerator maintains
        /// a reference to the IStorage and does not allow the underlying file
        /// to be closed when it is released. In general, after this call a copy
        /// of the STATSTG structures should be made and the IEnumSTATSTG object
        /// should be released immedately.
        /// </summary>
        /// <param name="reserved1">[In] Reserved for future use; must be zero</param>
        /// <param name="reserved2">[In] Reserved for future use; must be NULL</param>
        /// <param name="reserved3">[In] Reserved for future use; must be zero</param>
        /// <param name="ppenum">[Out] Pointer to IEnumSTATSTG that receives the
        /// interface pointer to the new enumerator object</param>
        void EnumElements(
            [In] uint reserved1, // must be zero
            [In] IntPtr reserved2, // must be null
            [In] uint reserved3, // must be zero
            [Out] out IEnumSTATSTG ppenum);

        /// <summary>
        /// Removes the specified storage or stream from this storage object.
        /// </summary>
        /// <param name="pwcsName">Pointer to a wide character null-terminated
        /// Unicode string that contains the name of the storage or stream
        /// to be removed.</param>
        void DestroyElement(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwcsName);

        /// <summary>
        /// Renames the specified substorage or stream in this storage object.
        /// </summary>
        /// <param name="pwcsOldName">Pointer to a wide character null-terminated
        /// Unicode string that contains the name of the substorage or stream
        /// to be changed. Note: The pwcsName, created in CreateStorage or
        /// CreateStream must not exceed 31 characters in length, not including
        /// the string terminator.</param>
        /// <param name="pwcsNewName">Pointer to a wide character null-terminated
        /// unicode string that contains the new name for the specified substorage
        /// or stream. Note: The pwcsName, created in CreateStorage or CreateStream
        /// must not exceed 31 characters in length, not including the string
        /// terminator. </param>
        void RenameElement(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwcsOldName,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwcsNewName);

        /// <summary>
        /// Sets the modification, access, and creation times of the specified
        /// storage element, if the underlying file system supports this method.
        /// </summary>
        /// <param name="pwcsName">The name of the storage object element whose
        /// times are to be modified. If NULL, the time is set on the root
        /// storage rather than one of its elements. </param>
        /// <param name="pctime">Either the new creation time for the element
        /// or NULL if the creation time is not to be </param>
        /// <param name="patime">Either the new access time for the element or
        /// NULL if the access time is not to be modified. </param>
        /// <param name="pmtime">Either the new modification time for the element
        /// or NULL if the modification time is not to be modified. </param>
        void SetElementTimes(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwcsName,
            [In] IntPtr pctime,
            [In] IntPtr patime,
            [In] ref System.Runtime.InteropServices.ComTypes.FILETIME pmtime);

        /// <summary>
        /// Assigns the specified class identifier (CLSID) to this storage object.
        /// </summary>
        /// <param name="clsid">The CLSID that is to be associated with the storage
        /// object. </param>
        void SetClass(
            [In] ref Guid clsid);

        /// <summary>
        /// Stores up to 32 bits of state information in this storage object.
        /// This method is reserved for future use.
        /// </summary>
        /// <param name="grfStateBits">Specifies the new values of the bits to
        /// set. No legal values are defined for these bits; they are all reserved
        /// for future use and must not be used by applications. </param>
        /// <param name="grfMask">A binary mask indicating which bits in
        /// grfStateBits are significant in this call. </param>
        void SetStateBits(
            [In] uint grfStateBits,
            [In] uint grfMask);

        /// <summary>
        /// Retrieves the STATSTG structure for this open storage object.
        /// </summary>
        /// <param name="pstatstg">On return, pointer to a STATSTG structure where
        /// this method places information about the open storage object. This
        /// parameter is NULL if an error occurs. </param>
        /// <param name="grfStatFlag">Specifies that some of the members in the
        /// STATSTG structure are not returned, thus saving a memory allocation
        /// operation. Values are taken from the STATFLAG enumeration.</param>
        void Stat(
            [Out] out System.Runtime.InteropServices.ComTypes.STATSTG pstatstg,
           [In] STATFLAG grfStatFlag
            );
    }

    // <summary>
    /// Enumeration of structured storage elements.
    /// WARNING: If you obtain an IEnumSTATSTG from an IStorage you
    /// must call Marshal.ReleaseComObject on the returned enumerator as
    /// soon as you are finished using it! Otherwise the enumerator maintains
    /// a reference to the IStorage and does not allow the underlying file
    /// to be closed when it is released. In general, when an IEnumSTATSTG
    /// is retreived from an IStorage a copy of the STATSTG structures should
    /// be made and the IEnumSTATSTG object should be released immedately.
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("0000000d-0000-0000-C000-000000000046")]
    public interface IEnumSTATSTG
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
           [Out] out System.Runtime.InteropServices.ComTypes.STATSTG rgelt,
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
            [Out] out IEnumSTATSTG ppenum);
    }

    /// <summary>
    /// The STGM enumeration values are used in the IStorage, IStream,
    /// and IPropertySetStorage interfaces, and in the StgCreateDocfile,
    /// StgCreateStorageEx, StgCreateDocfileOnILockBytes, StgOpenStorage,
    /// StgOpenStorageEx functions to indicate the conditions for creating
    /// and deleting the object and to indicate access modes for the object.
    ///
    /// These elements are often combined using an OR operator. They are
    /// interpreted in groups as shown in the following table. It is not
    /// valid to use more than one element from a single group.
    /// </summary>
    [Flags]
    public enum STGM : uint
    {
        READ = 0x00000000,
        WRITE = 0x00000001,
        READWRITE = 0x00000002,
        SHARE_DENY_NONE = 0x00000040,
        SHARE_DENY_READ = 0x00000030,
        SHARE_DENY_WRITE = 0x00000020,
        SHARE_EXCLUSIVE = 0x00000010,
        PRIORITY = 0x00040000,
        CREATE = 0x00001000,
        CONVERT = 0x00020000,
        FAILIFTHERE = 0x00000000,
        DIRECT = 0x00000000,
        TRANSACTED = 0x00010000,
        NOSCRATCH = 0x00100000,
        NOSNAPSHOT = 0x00200000,
        SIMPLE = 0x08000000,
        DIRECT_SWMR = 0x00400000,
        DELETEONRELEASE = 0x04000000
    }

    [Flags]
    public enum STGFMT : uint
    {
        STGFMT_STORAGE = 0,
        STGFMT_FILE = 3,
        STGFMT_ANY = 4,
        STGFMT_DOCFILE = 5
    }

    /// <summary>
    /// The STGMOVE enumeration values indicate whether a storage
    /// element is to be moved or copied. They are used in the
    /// IStorage::MoveElementTo method.
    /// </summary>
    public enum STGMOVE : uint
    {
        MOVE = 0,
        COPY = 1
    };

    /// <summary>
    /// The STGC enumeration constants specify the conditions
    /// for performing the commit operation in the IStorage::Commit
    /// and IStream::Commit methods.
    /// </summary>
    [Flags]
    public enum STGC : uint
    {
        DEFAULT = 0,
        OVERWRITE = 1,
        ONLYIFCURRENT = 2,
        DANGEROUSLYCOMMITMERELYTODISKCACHE = 4,
        CONSOLIDATE = 8
    };

    /// <summary>
    /// The STATFLAG enumeration values indicate whether the method
    /// should try to return a name in the pwcsName member of the
    /// STATSTG structure. The values are used in the ILockBytes::Stat,
    /// IStorage::Stat, and IStream::Stat methods to save memory
    /// when the pwcsName member is not needed.
    /// </summary>
    public enum STATFLAG : uint
    {
        DEFAULT = 0,
        NONAME = 1
    }

    /// <summary>
    /// The STGTY enumeration values are used in the type member
    /// of the STATSTG structure to indicate the type of the storage
    /// element. A storage element is a storage object, a stream
    /// object, or a byte-array object (LOCKBYTES).
    /// </summary>
    public enum STGTY : int
    {
        STORAGE = 1,
        STREAM = 2,
        LOCKBYTES = 3,
        PROPERTY = 4
    }

    /// <summary>
    /// The LOCKTYPE enumeration values indicate the type of
    /// locking requested for the specified range of bytes. The
    /// values are used in the ILockBytes::LockRegion and
    /// IStream::LockRegion methods.
    /// </summary>
    public enum LOCKTYPE : uint
    {
        /// <summary>
        /// If this lock is granted, the specified range of bytes can be opened and
        /// read any number of times, but writing to the locked range is prohibited
        /// except for the owner that was granted this lock.
        /// </summary>
        WRITE = 1,

        /// <summary>
        /// If this lock is granted, writing to the specified range of bytes is
        /// prohibited except by the owner that was granted this lock
        /// </summary>
        EXCLUSIVE = 2,

        /// <summary>
        /// If this lock is granted, no other LOCK_ONLYONCE lock can be obtained on
        /// the range. Usually this lock type is an alias for some other lock type.
        /// Thus, specific implementations can have additional behavior associated
        /// with this lock type
        /// </summary>
        ONLYONCE = 4
    }
}

