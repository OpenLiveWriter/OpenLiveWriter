// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace OpenLiveWriter.Interop.Com.StructuredStorage
{
    /// <summary>
    /// Storage Exception provides the exceptions throw by structured storage.
    /// </summary>
    [Serializable]
    public class StorageException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner Exception</param>
        public StorageException(string message, COMException innerException) :
            base(message, innerException)
        {
            nativeErrorCode = innerException.ErrorCode;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="innerException">The inner Exception</param>
        public StorageException(COMException innerException) :
            this(innerException.Message, innerException)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="errorCode">The error code</param>
        public StorageException(string message, int errorCode) :
            base(message)
        {
            nativeErrorCode = errorCode;
        }

        /// <summary>
        /// The native error code underlying this storage exception
        /// </summary>
        public int NativeErrorCode
        {
            get
            {
                return nativeErrorCode;
            }
        }
        private int nativeErrorCode;

        public const int NO_ERROR_CODE = 0;

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="info">The SerializationInfo to deserialize value from.</param>
        /// <param name="context">The source for this deserialization.</param>
        protected StorageException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize this ObjectStoreFileException.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data.</param>
        /// <param name="context">The destination for this serialization.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public new void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //	Serialize the base class.
            base.GetObjectData(info, context);
        }
    }

    /// <summary>
    /// An invalid operation (typically caused by programming error).
    /// </summary>
    [Serializable]
    public class StorageInvalidOperationException : StorageException
    {
        private const string message = "Cannot perform requested operation";

        public StorageInvalidOperationException(COMException innerException)
            : base(message, innerException)
        {
            // Debug.Fail("Invalid Operation using Storage: " + innerException.Message);
        }

        public StorageInvalidOperationException(string message) :
            base(message, NO_ERROR_CODE)
        {
            // Debug.Fail("Invalid Operation using Storage");
        }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="info">The SerializationInfo to deserialize value from.</param>
        /// <param name="context">The source for this deserialization.</param>
        protected StorageInvalidOperationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize this ObjectStoreFileException.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data.</param>
        /// <param name="context">The destination for this serialization.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public new void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //	Serialize the base class.
            base.GetObjectData(info, context);
        }
    }

    /// <summary>
    /// The Storage file is not found.
    /// </summary>
    [Serializable]
    public class StorageFileNotFoundException : StorageException
    {
        private const string message = "Storage file not found";

        public StorageFileNotFoundException(COMException innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="info">The SerializationInfo to deserialize value from.</param>
        /// <param name="context">The source for this deserialization.</param>
        protected StorageFileNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize this ObjectStoreFileException.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data.</param>
        /// <param name="context">The destination for this serialization.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public new void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //	Serialize the base class.
            base.GetObjectData(info, context);
        }
    }

    /// <summary>
    /// The network location cannot be reached.
    /// </summary>
    [Serializable]
    public class StorageNetworkUnreachableException : StorageException
    {
        private const string message = "The network location cannot be reached.";

        public StorageNetworkUnreachableException(COMException innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="info">The SerializationInfo to deserialize value from.</param>
        /// <param name="context">The source for this deserialization.</param>
        protected StorageNetworkUnreachableException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize this ObjectStoreFileException.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data.</param>
        /// <param name="context">The destination for this serialization.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public new void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //	Serialize the base class.
            base.GetObjectData(info, context);
        }
    }

    /// <summary>
    /// There is insufficient disk space to complete the operation.
    /// </summary>
    [Serializable]
    public class StorageNoDiskSpaceException : StorageException
    {
        private const string message = "There is insufficient disk space to complete operation.";

        public StorageNoDiskSpaceException(COMException innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="info">The SerializationInfo to deserialize value from.</param>
        /// <param name="context">The source for this deserialization.</param>
        protected StorageNoDiskSpaceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize this ObjectStoreFileException.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data.</param>
        /// <param name="context">The destination for this serialization.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public new void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //	Serialize the base class.
            base.GetObjectData(info, context);
        }
    }

    /// <summary>
    /// The network location cannot be reached.
    /// </summary>
    [Serializable]
    public class StorageLogonFailureException : StorageException
    {
        private const string message = "Logon failure: unknown user name or bad password.";

        public StorageLogonFailureException(COMException innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="info">The SerializationInfo to deserialize value from.</param>
        /// <param name="context">The source for this deserialization.</param>
        protected StorageLogonFailureException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize this ObjectStoreFileException.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data.</param>
        /// <param name="context">The destination for this serialization.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public new void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //	Serialize the base class.
            base.GetObjectData(info, context);
        }
    }

    /// <summary>
    /// The storage path is not found.
    /// </summary>
    [Serializable]
    public class StoragePathNotFoundException : StorageException
    {
        private const string message = "Storage path not found";

        public StoragePathNotFoundException(COMException innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="info">The SerializationInfo to deserialize value from.</param>
        /// <param name="context">The source for this deserialization.</param>
        protected StoragePathNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize this ObjectStoreFileException.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data.</param>
        /// <param name="context">The destination for this serialization.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public new void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //	Serialize the base class.
            base.GetObjectData(info, context);
        }
    }

    /// <summary>
    /// The storage file already exists.
    /// </summary>
    [Serializable]
    public class StorageFileAlreadyExistsException : StorageException
    {
        private const string message = "Storage file already exists";

        public StorageFileAlreadyExistsException(COMException innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="info">The SerializationInfo to deserialize value from.</param>
        /// <param name="context">The source for this deserialization.</param>
        protected StorageFileAlreadyExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize this ObjectStoreFileException.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data.</param>
        /// <param name="context">The destination for this serialization.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public new void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //	Serialize the base class.
            base.GetObjectData(info, context);
        }
    }

    /// <summary>
    /// The storage is corrupt or has illegal headers.
    /// </summary>
    [Serializable]
    public class StorageInvalidFormatException : StorageException
    {
        private const string message = "Storage format is invalid (corrupt or has illegal headers)";

        public StorageInvalidFormatException(COMException innerException)
            : base(message, innerException)
        { }

        public StorageInvalidFormatException()
            : base(message, 0)
        { }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="info">The SerializationInfo to deserialize value from.</param>
        /// <param name="context">The source for this deserialization.</param>
        protected StorageInvalidFormatException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize this ObjectStoreFileException.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data.</param>
        /// <param name="context">The destination for this serialization.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public new void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //	Serialize the base class.
            base.GetObjectData(info, context);
        }
    }

    /// <summary>
    /// Share violation.  Usually occurs when trying to attain writelock on an already
    /// writelocked storage.
    /// </summary>
    [Serializable]
    public class StorageShareViolationException : StorageException
    {
        private const string message = "Share violation--storage may already be opened.";

        public StorageShareViolationException(COMException innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="info">The SerializationInfo to deserialize value from.</param>
        /// <param name="context">The source for this deserialization.</param>
        protected StorageShareViolationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    /// <summary>
    /// Access denied.  Either file is read-only, or the user does not have the correct
    /// permissions.
    /// </summary>
    [Serializable]
    public class StorageAccessDeniedException : StorageException
    {
        private const string message = "Access denied.";

        public StorageAccessDeniedException(COMException innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="info">The SerializationInfo to deserialize value from.</param>
        /// <param name="context">The source for this deserialization.</param>
        protected StorageAccessDeniedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    /// <summary>
    /// The storage is locked.
    /// </summary>
    [Serializable]
    public class StorageLockViolationException : StorageException
    {
        private const string message = "Storage is locked.";

        public StorageLockViolationException(COMException innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="info">The SerializationInfo to deserialize value from.</param>
        /// <param name="context">The source for this deserialization.</param>
        protected StorageLockViolationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize this ObjectStoreFileException.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data.</param>
        /// <param name="context">The destination for this serialization.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public new void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //	Serialize the base class.
            base.GetObjectData(info, context);
        }
    }

    /// <summary>
    /// The storage is no longer current (storage has been updated since opened -
    /// transaction related)
    /// </summary>
    [Serializable]
    public class StorageNotCurrentException : StorageException
    {
        private const string message = "Storage is not current.";

        public StorageNotCurrentException(COMException innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="info">The SerializationInfo to deserialize value from.</param>
        /// <param name="context">The source for this deserialization.</param>
        protected StorageNotCurrentException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize this ObjectStoreFileException.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data.</param>
        /// <param name="context">The destination for this serialization.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public new void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //	Serialize the base class.
            base.GetObjectData(info, context);
        }
    }
}
