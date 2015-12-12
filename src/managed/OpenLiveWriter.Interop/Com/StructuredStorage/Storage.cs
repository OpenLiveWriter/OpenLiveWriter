// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace OpenLiveWriter.Interop.Com.StructuredStorage
{
    /// <summary>
    /// Summary description for Storage.
    /// </summary>
    public class Storage : IDisposable
    {
        /// <summary>
        /// Constructor for Storage based on a file path
        /// </summary>
        /// <param name="storage">The IStorage from which to construct this storage.</param>
        /// <param name="writable">Whether the storage is writable.</param>
        public Storage(string path, StorageMode mode, bool writable) :
            this(OpenStorage(path, mode, writable, new CompoundFileOpener()), writable)
        {
        }

        /// <summary>
        /// Construct a storage based on an existing IStorage
        /// </summary>
        /// <param name="storage">The IStorage from which to construct this storage.</param>
        /// <param name="writable">Whether the storage is writable.</param>
        public Storage(IStorage storage, bool writable)
        {
            this.storage = storage;
            this.writable = writable;
        }

        /// <summary>
        /// Get the underlying IStorage
        /// </summary>
        public IStorage IStorage
        {
            get { return this.storage; }
        }

        /// <summary>
        /// Whether the storage is writable.
        /// </summary>
        public bool Writable
        {
            get { return writable; }
        }

        /// <summary>
        /// The Clsid of the storage or stream
        /// </summary>
        public Guid Clsid
        {
            get
            {
                if (clsid == Guid.Empty)
                    clsid = ReadClsid();
                return clsid;
            }
            set
            {
                WriteClsid(value);
                clsid = value;
            }
        }
        private Guid clsid;

        /// <summary>
        /// Opens a Storage in this storage.
        /// </summary>
        /// <param name="name">The guid representing the name of the substorage.</param>
        /// <param name="mode">The Storagemode to open the substorage.</param>
        /// <returns>The Storage.</returns>
        public Storage OpenStorage(Guid name, StorageMode mode)
        {
            return OpenStorage(name, mode, Writable);
        }

        /// <summary>
        /// Opens a Storage in this storage.
        /// </summary>
        /// <param name="name">The guid representing the name of the substorage.</param>
        /// <param name="mode">The Storagemode to open the substorage.</param>
        /// <param name="writable">Whether to open the storage as writable.  If
        /// the parent storage is readonly, the substorage cannot be opened
        /// as writable (StorageInvalidOperationException will be thrown).</param>
        /// <returns>The Storage.</returns>
        public Storage OpenStorage(Guid name, StorageMode mode, bool writable)
        {
            bool openWritable = ResolveWritableOverride(writable);
            return new Storage(
                OpenStorage(NameFromGuid(name), mode, openWritable, new SubStorageOpener(this.storage)),
                openWritable
                );
        }

        /// <summary>
        /// Opens a Storage in this storage.
        /// </summary>
        /// <param name="name">The string representing the name of the substorage (31 characters or less).</param>
        /// <param name="mode">The Storagemode to open the substorage.</param>
        /// <param name="writable">Whether to open the storage as writable.  If
        /// the parent storage is readonly, the substorage cannot be opened
        /// as writable (StorageInvalidOperationException will be thrown).</param>
        /// <returns>The Storage.</returns>
        public Storage OpenStorage(string name, StorageMode mode, bool writable)
        {
            bool openWritable = ResolveWritableOverride(writable);
            return new Storage(
                OpenStorage(name, mode, openWritable, new SubStorageOpener(this.storage)),
                openWritable
                );
        }

        /// <summary>
        /// Opens a stream in this storage.
        /// </summary>
        /// <param name="name">The guid representing the name of the stream to open.</param>
        /// <param name="mode">The storagemode to use when opening the stream.</param>
        /// <returns>The stream.</returns>
        public ComStream OpenStream(Guid guid, StorageMode mode)
        {
            return OpenStream(NameFromGuid(guid), mode, Writable);
        }

        /// <summary>
        /// Opens a stream in this storage.
        /// </summary>
        /// <param name="name">The string representing the name of the stream to open.</param>
        /// <param name="mode">The storagemode to use when opening the stream.</param>
        /// <param name="writable">Whether to open the stream as writable.  If
        /// the parent storage is readonly, the stream cannot be opened
        /// as writable (StorageInvalidOperationException will be thrown).</param>
        /// <returns>The stream.</returns>
        public ComStream OpenStream(Guid guid, StorageMode mode, bool writable)
        {
            return OpenStream(NameFromGuid(guid), mode, writable);
        }

        /// <summary>
        /// Opens a stream in this storage.
        /// </summary>
        /// <param name="name">The string representing the name of the stream to open.</param>
        /// <param name="mode">The storagemode to use when opening the stream.</param>
        /// <returns>The stream.</returns>
        public ComStream OpenStream(string name, StorageMode mode)
        {
            return OpenStream(name, mode, Writable);
        }

        /// <summary>
        /// Opens a stream in this storage.
        /// </summary>
        /// <param name="name">The guid representing the name of the stream to open.</param>
        /// <param name="mode">The storagemode to use when opening the stream.</param>
        /// <param name="writable">Whether to open the stream as writable.  If
        /// the parent storage is readonly, the stream cannot be opened
        /// as writable (StorageInvalidOperationException will be thrown).</param>
        /// <returns>The stream.</returns>
        public ComStream OpenStream(string name, StorageMode mode, bool writable)
        {
            bool openWritable = ResolveWritableOverride(writable);
            ComStream stream = null;
            try
            {
                switch (mode)
                {
                    case (StorageMode.Create):
                        CreateStream(name, out stream);
                        break;

                    case (StorageMode.Open):
                        OpenStream(name, openWritable, out stream);
                        break;

                    case (StorageMode.OpenOrCreate):
                        try
                        {
                            OpenStream(name, true, out stream);
                        }
                        catch (COMException e)
                        {
                            if (e.ErrorCode == STG_E.FILENOTFOUND)
                                CreateStream(name, out stream);
                            else
                                throw;
                        }
                        break;
                }

            }
            catch (COMException e)
            {
                ThrowStorageException(e);
            }
            return stream;
        }

        public void SetModifiedTime(string streamName, System.Runtime.InteropServices.ComTypes.FILETIME modified)
        {
            this.storage.SetElementTimes(streamName, IntPtr.Zero, IntPtr.Zero, ref modified);
        }

        public System.Runtime.InteropServices.ComTypes.STATSTG GetStat(string name)
        {
            using (ComStream stream = this.OpenStream(name, StorageMode.Open, false))
            {
                return stream.Stat;
            }
        }

        /// <summary>
        /// Enumerate the elements of this storage (STATSTG stucture)
        /// </summary>
        /// <returns>Array of STATSTG elements</returns>
        public System.Runtime.InteropServices.ComTypes.STATSTG[] Elements
        {
            get
            {
                IEnumSTATSTG enumerator = null;
                try
                {
                    // get the Com-based enumerator
                    storage.EnumElements(0, IntPtr.Zero, 0, out enumerator);

                    // Iterate over the IEnumSTATSTG and copy the elements into a managed array
                    ArrayList statstgList = new ArrayList();
                    System.Runtime.InteropServices.ComTypes.STATSTG current;
                    int result;
                    while ((result = enumerator.Next(1, out current, IntPtr.Zero)) == HRESULT.S_OK)
                        statstgList.Add(current);

                    // check to see if the last result was an error rather than HRESULT.S_FALSE
                    if (result != HRESULT.S_FALSE)
                        Marshal.ThrowExceptionForHR(result);

                    // Return an array containing the elements in the storage
                    return (System.Runtime.InteropServices.ComTypes.STATSTG[])statstgList.ToArray(typeof(System.Runtime.InteropServices.ComTypes.STATSTG));
                }
                catch (COMException e)
                {
                    ThrowStorageException(e);
                    return null; // keep compiler happy
                }
                finally
                {
                    // always release the enumerator (see comment on IStorage.EnumElements
                    // for why this MUST ALWAYS be done after calling EnumElements)
                    if (enumerator != null)
                        Marshal.ReleaseComObject(enumerator);
                }
            }
        }

        /// <summary>
        /// Copies this storage to another storage.
        /// </summary>
        /// <param name="destination">The destination to which this storage should
        /// be copied.</param>
        public void Copy(Storage destination)
        {
            try
            {
                storage.CopyTo(
                    0, IntPtr.Zero, IntPtr.Zero, destination.storage);
            }
            catch (COMException e)
            {
                ThrowStorageException(e);
            }
        }

        /// <summary>
        /// Deletes an element from this storage.
        /// </summary>
        /// <param name="guid">The guid representing the name of the element to be deleted.</param>
        public void DeleteElement(Guid guid)
        {
            DeleteElement(NameFromGuid(guid));
        }

        /// <summary>
        /// Deletes an element from this storage.
        /// </summary>
        /// <param name="name">The string representing the name of the element to be deleted.</param>
        public void DeleteElement(string name)
        {
            try
            {
                storage.DestroyElement(name);
            }
            catch (COMException e)
            {
                ThrowStorageException(e);
            }
        }

        /// <summary>
        /// Renames an element in this storage.
        /// </summary>
        /// <param name="guid">The guid representing the name of the element to rename.</param>
        /// <param name="newGuid">The guid representing the new name of the element.</param>
        public void RenameElement(Guid guid, Guid newGuid)
        {
            RenameElement(NameFromGuid(guid), NameFromGuid(newGuid));
        }

        /// <summary>
        /// Renames an element in this storage.
        /// </summary>
        /// <param name="name">The string representing the name of the element to rename.</param>
        /// <param name="newName">The string representing the new name of the element.</param>
        public void RenameElement(string name, string newName)
        {
            try
            {
                storage.RenameElement(name, newName);
            }
            catch (COMException e)
            {
                ThrowStorageException(e);
            }
        }

        /// <summary>
        /// Commits changes to this storage.  Note that for changes to be committed,
        /// the parent storage must also be committed.  Does not currently support
        /// multiwriter transaction (only a single instance of the storage can be writable)
        ///
        /// </summary>
        public void Commit()
        {
            try
            {
                storage.Commit(STGC.DEFAULT);
            }
            catch (COMException e)
            {
                ThrowStorageException(e);
            }
        }

        public void CommitAndConsolidate()
        {
            try
            {
                storage.Commit(STGC.CONSOLIDATE);
            }
            catch (COMException e)
            {
                ThrowStorageException(e);
            }
        }

        /// <summary>
        /// Reverts any changes to this storage.
        /// </summary>
        public void Revert()
        {
            try
            {
                storage.Revert();
            }
            catch (COMException e)
            {
                ThrowStorageException(e);
            }
        }

        /// <summary>
        /// Closes this storage.  Always use close or dispose when using Storage!
        /// </summary>
        public void Close()
        {
            if (storage != null)
            {
                Marshal.ReleaseComObject(this.storage);
                storage = null;
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Disposes this storage.  Always use close or dispose when using Storage!
        /// </summary>
        public void Dispose()
        {
            Close();
        }

#if FALSE
        /// <summary>
        /// Storage finalizer
        /// </summary>
        ~Storage()
        {
            //Debug.Assert(storage == null, "Object not disposed properly - Use Close or Dispose!");
        }
#endif

        /// <summary>
        /// Opens a Storage using the specified opener.
        /// </summary>
        /// <param name="name">The name of the substorage.</param>
        /// <param name="mode">The Storagemode to open the substorage.</param>
        /// <param name="writable">Whether to open the storage as writable.  If
        /// the parent storage is readonly, the substorage cannot be opened
        /// as writable (InvalidoperationException will be thrown).</param>
        /// <param name="opener">The IStorageOpener to use to open the storage.</param>
        /// <param name="created">Out parameter indicated whether the storage was created (or opened)</param>
        /// <returns>The Storage.</returns>
        internal static IStorage OpenStorage(string name, StorageMode mode, bool writable, IStorageOpener opener)
        {
            IStorage storage = null;
            try
            {
                switch (mode)
                {
                    case (StorageMode.Create):
                        opener.CreateStorage(name, out storage);
                        break;

                    case (StorageMode.Open):
                        if (!opener.OpenStorage(name, writable, out storage))
                        {
                            throw new COMException("StorageName does not exist", STG_E.FILENOTFOUND);
                        }
                        break;

                    case (StorageMode.OpenOrCreate):
                        if (!opener.OpenStorage(name, writable, out storage))
                        {
                            opener.CreateStorage(name, out storage);
                        }
                        break;
                }
            }
            catch (COMException e)
            {
                ThrowStorageException(e);
            }
            return storage;
        }

        /// <summary>
        /// Creates a stream in this storage.
        /// </summary>
        /// <param name="name">The string representing the name of the stream to create. (31 characters or less)</param>
        /// <param name="stream">The stream.</param>
        private void CreateStream(string name, out ComStream stream)
        {
            // Note: The compound file implementation of structured
            /// storage requires that all substorages and substreams be opened as share_exclusive (locking
            /// is managed at the file level).
            storage.CreateStream(
                name,
                STGM.CREATE | STGM.READWRITE | STGM.SHARE_EXCLUSIVE,
                0,
                0,
                out stream);
        }

        /// <summary>
        /// Opens a stream in this storage.
        /// </summary>
        /// <param name="name">The string representing the name of the stream to create (31 characters or less).</param>
        /// <param name="stream">The stream.</param>
        /// <param name="writable">Whether to open the stream as writable.</param>
        private void OpenStream(string name, bool writable, out ComStream stream)
        {
            // Note: The compound file implementation of structured
            // storage requires that all substorages and substreams be opened as share_exclusive (locking
            // is managed at the file level).
            storage.OpenStream(
                name,
                0,
                (writable ? STGM.READWRITE : STGM.READ) |
                STGM.SHARE_EXCLUSIVE,
                0,
                out stream);
        }

        /// <summary>
        /// Converts a Guid to an encoded string (that has a shorter length)
        /// </summary>
        /// <param name="guid">The Guid to get the name from</param>
        /// <returns>The encoded string</returns>
        public static string NameFromGuid(Guid guid)
        {
            byte[] bytes = guid.ToByteArray();
            char[] chars = new char[bytes.Length];

            for (int i = 0; i < bytes.Length; i++)
                chars[i] = Convert.ToChar(bytes[i] + FIRST_NON_RESERVED_CHAR);

            return new string(chars);
        }

        /// <summary>
        /// Converts a string produced with NameFromGuid back to a guid
        /// </summary>
        /// <param name="name">The encoded string to convert to a guid</param>
        /// <returns>The guid</returns>
        public static Guid GuidFromName(string name)
        {
            char[] chars = name.ToCharArray();
            byte[] bytes = new byte[chars.Length];

            for (int i = 0; i < chars.Length; i++)
                bytes[i] = Convert.ToByte(chars[i] - FIRST_NON_RESERVED_CHAR);

            return new Guid(bytes);
        }

        /// <summary>
        /// Writes the clsid to this storage.
        /// </summary>
        /// <param name="guid">The guid representing the clsid</param>
        private void WriteClsid(Guid guid)
        {
            try
            {
                Ole32Storage.WriteClassStg(storage, ref guid);
            }
            catch (COMException e)
            {
                ThrowStorageException(e);
            }
        }

        /// <summary>
        /// Reads the clsid from this storage
        /// </summary>
        /// <returns>The guid representing the clsid for this storage</returns>
        private Guid ReadClsid()
        {
            Guid clsid = Guid.NewGuid();
            try
            {
                Ole32Storage.ReadClassStg(storage, ref clsid);
            }
            catch (COMException e)
            {
                ThrowStorageException(e);
            }
            return clsid;
        }

        /// <summary>
        /// Resolves whether the requested writable state should override the
        /// current Storage writable state. (Handles the case when a user would like
        /// a readonly substorage or stream in a read/write storage).
        /// </summary>
        /// <param name="writable">The requested writable state.</param>
        /// <returns>True indicates writable, otherwise false</returns>
        private bool ResolveWritableOverride(bool writable)
        {
            if (!Writable && writable)
                throw new StorageInvalidOperationException("Cannot open writable storage in readonly storage.");

            bool openWritable = Writable;
            if (openWritable && !writable)
                openWritable = writable;
            return openWritable;
        }

        /// <summary>
        /// Static helper method for coalescing COM exceptions into storage
        /// exceptions.
        /// </summary>
        /// <param name="e">The COM Exception being thrown</param>
        protected static void ThrowStorageException(COMException e)
        {
            switch (e.ErrorCode)
            {
                // Unable to perform requested Operation
                case STG_E.INVALIDFUNCTION:
                case STG_E.INVALIDHANDLE:
                case STG_E.INVALIDPOINTER:
                case STG_E.INVALIDPARAMETER:
                case STG_E.PROPSETMISMATCHED:
                case STG_E.UNIMPLEMENTEDFUNCTION:
                case STG_E.NOTFILEBASEDSTORAGE:
                case STG_E.INVALIDNAME:
                case STG_E.REVERTED:
                case STG_E.INVALIDFLAG:
                    throw new StorageInvalidOperationException(e);

                // File not found
                case STG_E.FILENOTFOUND:
                    throw new StorageFileNotFoundException(e);

                // Path not found
                case STG_E.PATHNOTFOUND:
                    throw new StoragePathNotFoundException(e);

                // File Already exists
                case STG_E.FILEALREADYEXISTS:
                    throw new StorageFileAlreadyExistsException(e);

                // Storage Format
                case STG_E.DOCFILECORRUPT:
                case STG_E.INVALIDHEADER:
                    throw new StorageInvalidFormatException(e);

                // Share violation
                case STG_E.SHAREVIOLATION:
                    throw new StorageShareViolationException(e);

                case STG_E.DISKISWRITEPROTECTED:
                case STG_E.ACCESSDENIED:
                    throw new StorageAccessDeniedException(e);

                case STG_E.MEDIUMFULL:
                    throw new StorageNoDiskSpaceException(e);

                // Lock violation
                case STG_E.LOCKVIOLATION:
                    throw new StorageLockViolationException(e);

                // Storage not current when attempting to commit changes
                case STG_E.NOTCURRENT:
                    throw new StorageNotCurrentException(e);

                case STG_E.NETWORKUNREACHABLE:
                case STG_E.BAD_NETPATH:
                case STG_E.ERROR_REM_NOT_LIST:
                case STG_E.NETWORK_BUSY:
                case STG_E.DEV_NOT_EXIST:
                    throw new StorageNetworkUnreachableException(e);

                case STG_E.LOGON_FAILURE:
                    throw new StorageLogonFailureException(e);

                // Other Storage Exceptions
                case STG_E.NOMOREFILES:
                case STG_E.ABNORMALAPIEXIT:
                case STG_E.UNKNOWN:
                case STG_E.INUSE:
                case STG_E.SHAREREQUIRED:
                case STG_E.EXTANTMARSHALLINGS:
                case STG_E.BADBASEADDRESS:
                case STG_E.INCOMPLETE:
                case STG_E.TERMINATED:
                case STG_E.NOTSIMPLEFORMAT:
                case STG_E.STATUS_COPY_PROTECTION_FAILURE:
                case STG_E.CSS_AUTHENTICATION_FAILURE:
                case STG_E.CSS_KEY_NOT_PRESENT:
                case STG_E.CSS_KEY_NOT_ESTABLISHED:
                case STG_E.CSS_SCRAMBLED_SECTOR:
                case STG_E.CSS_REGION_MISMATCH:
                case STG_E.RESETS_EXHAUSTED:
                // A Disk Fault Occurred.
                case STG_E.WRITEFAULT:
                case STG_E.READFAULT:
                case STG_E.SEEKERROR:
                // Can't Save
                case STG_E.CANTSAVE:
                // Resource Errors
                case STG_E.TOOMANYOPENFILES:
                case STG_E.INSUFFICIENTMEMORY:
                // Incompatible Storage Version
                case STG_E.OLDFORMAT:
                case STG_E.OLDDLL:
                // Storage too large
                case STG_E.DOCFILETOOLARGE:
                default:
                    throw new StorageException(e);
            }
        }

        /// <summary>
        /// The storage's IStorage
        /// </summary>
        private IStorage storage = null;

        /// <summary>
        /// Whether the storage is writable.
        /// </summary>
        private bool writable = false;

        private const int FIRST_NON_RESERVED_CHAR = 0x5D;
    }

    /// <summary>
    /// The substorage opener for a storage.
    /// </summary>
    internal class SubStorageOpener : IStorageOpener
    {
        /// <summary>
        /// Substorage Constructor
        /// </summary>
        /// <param name="storage">The IStorage underlying this substorage opener </param>
        public SubStorageOpener(IStorage storage)
        {
            this.storage = storage;
        }

        /// <summary>
        /// Creates a substorage
        /// </summary>
        /// <param name="pwcsName">The name of the substorage to create</param>
        /// <param name="ppstg">The IStorage that is created</param>
        public void CreateStorage(
            string pwcsName,
            out IStorage ppstg)
        {
            // Note: The compound file implementation of structured
            // storage requires that all substorages and substreams be opened as share_exclusive (locking
            // is managed at the file level).
            this.storage.CreateStorage(
                pwcsName,
                STGM.CREATE | STGM.TRANSACTED |
                STGM.READWRITE | STGM.SHARE_EXCLUSIVE,
                0,
                0,
                out ppstg);
        }

        /// <summary>
        /// Opens an existing substorage.
        /// </summary>
        /// <param name="pwcsName">The name of the substorage to create</param>
        /// <param name="writable">Whether to open the storage as writable</param>
        /// <param name="ppstg">The IStorage that is created</param>
        public bool OpenStorage(
            string pwcsName,
            bool writable,
            out IStorage ppstg)
        {
            return OpenStorage(pwcsName,
                STGM.TRANSACTED |
                    (writable ? STGM.READWRITE : STGM.READ) |
                    STGM.SHARE_EXCLUSIVE,
                out ppstg);
        }

        /// <summary>
        /// Opens an existing substorage.
        /// </summary>
        /// <param name="pwcsName">The name of the substorage to create</param>
        /// <param name="writable">Whether to open the storage as writable</param>
        /// <param name="ppstg">The IStorage that is created</param>
        public bool OpenStorage(
            string pwcsName,
            STGM flags,
            out IStorage ppstg)
        {
            // Note: The compound file implementation of structured
            // storage requires that all substorages and substreams be opened as share_exclusive (locking
            // is managed at the file level).
            try
            {
                this.storage.OpenStorage(
                pwcsName,
                IntPtr.Zero,
                flags,
                IntPtr.Zero,
                0,
                out ppstg
                );
            }
            catch (COMException e)
            {
                if (e.ErrorCode == STG_E.FILENOTFOUND)
                {
                    ppstg = null;
                    return false;
                }

                throw;
            }
            return true;
        }

        private IStorage storage;
    }

    /// <summary>
    /// The IStorageOpener for compound files
    /// </summary>
    public class CompoundFileOpener : IStorageOpener
    {
        /// <summary>
        /// Creates the compound file storage
        /// </summary>
        /// <param name="pwcsName">The path to the compound file to create</param>
        /// <param name="ppstg">(out) The IStorage representing this compound file</param>
        public void CreateStorage(
            string pwcsName,
            out IStorage ppstg)
        {
            Ole32Storage.STGOPTIONS stgoptions = new Ole32Storage.STGOPTIONS();
            stgoptions.usVersion = 1;
            stgoptions.ulSectorSize = 4096;

            Guid guidIStorage = typeof(IStorage).GUID;
            int result = Ole32Storage.StgCreateStorageEx(
                pwcsName,
                STGM.FAILIFTHERE | STGM.TRANSACTED |
                STGM.READWRITE | STGM.SHARE_DENY_WRITE,
                STGFMT.STGFMT_DOCFILE,
                0,
                ref stgoptions,
                IntPtr.Zero,
                ref guidIStorage,
                out ppstg);

            if (result != HRESULT.S_OK)
                Marshal.ThrowExceptionForHR(result);
        }

        /// <summary>
        /// Opens the compound file storage.
        /// </summary>
        /// <param name="pwcsName">The path to the compound file to create</param>
        /// <param name="ppstg">(out) The IStorage representing this compound file</param>
        /// <returns>True indicates it opened successfully.  False indicates that the file
        /// could not be found.</returns>
        public bool OpenStorage(
            string pwcsName,
            bool writable,
            out IStorage ppstg)
        {
            return OpenStorage(pwcsName,
                STGM.TRANSACTED |
                (writable ? STGM.READWRITE : STGM.READ) |
                (writable ? STGM.SHARE_DENY_WRITE : STGM.SHARE_DENY_NONE),
                out ppstg);
        }

        /// <summary>
        /// Opens the compound file storage.
        /// </summary>
        /// <param name="pwcsName">The path to the compound file to create</param>
        /// <param name="ppstg">(out) The IStorage representing this compound file</param>
        /// <returns>True indicates it opened successfully.  False indicates that the file
        /// could not be found.</returns>
        public bool OpenStorage(
            string pwcsName,
            STGM flags,
            out IStorage ppstg)
        {
            if (!ValidateCompoundFileStorage(pwcsName))
            {
                ppstg = null;
                return false;
            }

            Ole32Storage.STGOPTIONS stgoptions = new Ole32Storage.STGOPTIONS();
            stgoptions.usVersion = 1;
            stgoptions.ulSectorSize = 4096;

            Guid guidIStorage = typeof(IStorage).GUID;
            int result = Ole32Storage.StgOpenStorageEx(
                pwcsName,
                flags,
                STGFMT.STGFMT_DOCFILE,
                0,
                ref stgoptions,
                IntPtr.Zero,
                ref guidIStorage,
                out ppstg);

            if (result != HRESULT.S_OK)
                Marshal.ThrowExceptionForHR(result);
            return true;
        }

        /// <summary>
        /// Validates that the path to the file is a valid Compound File.
        /// Returns false if the file does not exist.
        /// </summary>
        /// <param name="path"></param>
        private bool ValidateCompoundFileStorage(string storageName)
        {
            if (!File.Exists(storageName))
            {
                //throw new COMException("StorageName does not exist", STG_E.FILENOTFOUND ) ;
                return false;
            }
            else
            {
                int result = Ole32Storage.StgIsStorageFile(storageName);
                if (result == HRESULT.S_FALSE)
                    Marshal.ThrowExceptionForHR(STG_E.INVALIDHEADER);
                else if (result != HRESULT.S_OK)
                    Marshal.ThrowExceptionForHR(result);
            }
            return true;
        }
    }

    /// <summary>
    /// The StorageModes used when getting Storage and streams.
    /// </summary>
    public enum StorageMode
    {
        Create,
        Open,
        OpenOrCreate
    }

}
