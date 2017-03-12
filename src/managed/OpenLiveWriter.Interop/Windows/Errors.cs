// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Interop.Windows
{

    /// <summary>
    /// Error codes that can be returned from Win32 functions
    /// Use kernel32 GetLastError() to get the last error on the thread
    /// </summary>
    public struct ERROR
    {
        public const int SUCCESS = 0;
        public const int FILE_NOT_FOUND = 2;
        public const int NO_MORE_FILES = 18;
        public const int INSUFFICIENT_BUFFER = 122;
        public const int PROC_NOT_FOUND = 127;
        public const int ALREADY_EXISTS = 183;
        public const int MORE_DATA = 234;

        #region registry-related errors

        //  The configuration registry database is corrupt.
        public const int BADDB = 1009;

        //  The configuration registry key is invalid.
        public const int BADKEY = 1010;

        //  The configuration registry key could not be opened.
        public const int CANTOPEN = 1011;

        //  The configuration registry key could not be read.
        public const int CANTREAD = 1012;

        //  The configuration registry key could not be written.
        public const int CANTWRITE = 1013;

        //  One of the files in the registry database had to be recovered by use of a log or alternate copy. The recovery was successful.
        public const int REGISTRY_RECOVERED = 1014;

        //  The registry is corrupted. The structure of one of the files containing registry data is corrupted, or the system's memory image of the file is corrupted, or the file could not be recovered because the alternate copy or log was absent or corrupted.
        public const int REGISTRY_CORRUPT = 1015;

        //  An I/O operation initiated by the registry failed unrecoverably. The registry could not read in, or write out, or flush, one of the files that contain the system's image of the registry.
        public const int REGISTRY_IO_FAILED = 1016;

        //  The system has attempted to load or restore a file into the registry, but the specified file is not in a registry file format.
        public const int NOT_REGISTRY_FILE = 1017;

        //  Illegal operation attempted on a registry key that has been marked for deletion.
        public const int KEY_DELETED = 1018;

        //  System could not allocate the required space in a registry log.
        public const int NO_LOG_SPACE = 1019;

        //  Cannot create a symbolic link in a registry key that already has subkeys or values.
        public const int KEY_HAS_CHILDREN = 1020;

        //  Cannot create a stable subkey under a volatile parent key.
        public const int CHILD_MUST_BE_VOLATILE = 1021;

        //  A notify change request is being completed and the information is not being returned in the caller's buffer. The caller now needs to enumerate the files to find the changes.
        public const int NOTIFY_ENUM_DIR = 1022;

        #endregion

        public const int NO_ASSOCIATION = 1155;
        public const int UNABLE_TO_REMOVE_REPLACED = 1175;
        public const int UNABLE_TO_MOVE_REPLACEMENT = 1176;
        public const int UNABLE_TO_MOVE_REPLACEMENT_2 = 1177;
        public const int BAD_DEVICE = 1200;
        public const int NO_NET_OR_BAD_PATH = 1203;
        public const int HOTKEY_ALREADY_REGISTERED = 1409;
        public const int CONNECTION_RESET = 10054;
        public const int CONNECTION_REFUSED = 10061;
        public const int NOT_CONNECTED = 2250;
    }
}
