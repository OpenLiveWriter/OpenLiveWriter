// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Interop.Com.StructuredStorage
{
    public class STG_E
    {
        // MessageText:
        //
        //  Unable to perform requested operation.
        //
        public const int INVALIDFUNCTION = unchecked((int)0x80030001);

        //
        // MessageId: STG_E_FILENOTFOUND
        //
        // MessageText:
        //
        //  %1 could not be found.
        //
        public const int FILENOTFOUND = unchecked((int)0x80030002);

        //
        // MessageId: STG_E_PATHNOTFOUND
        //
        // MessageText:
        //
        //  The path %1 could not be found.
        //
        public const int PATHNOTFOUND = unchecked((int)0x80030003);

        //
        // MessageId: STG_E_TOOMANYOPENFILES
        //
        // MessageText:
        //
        //  There are insufficient resources to open another file.
        //
        public const int TOOMANYOPENFILES = unchecked((int)0x80030004);

        //
        // MessageId: STG_E_ACCESSDENIED
        //
        // MessageText:
        //
        //  Access Denied.
        //
        public const int ACCESSDENIED = unchecked((int)0x80030005);

        //
        // MessageId: STG_E_INVALIDHANDLE
        //
        // MessageText:
        //
        //  Attempted an operation on an invalid object.
        //
        public const int INVALIDHANDLE = unchecked((int)0x80030006);

        //
        // MessageId: STG_E_INSUFFICIENTMEMORY
        //
        // MessageText:
        //
        //  There is insufficient memory available to complete operation.
        //
        public const int INSUFFICIENTMEMORY = unchecked((int)0x80030008);

        //
        // MessageId: STG_E_INVALIDPOINTER
        //
        // MessageText:
        //
        //  Invalid pointer error.
        //
        public const int INVALIDPOINTER = unchecked((int)0x80030009);

        //
        // MessageId: STG_E_NOMOREFILES
        //
        // MessageText:
        //
        //  There are no more entries to return.
        //
        public const int NOMOREFILES = unchecked((int)0x80030012);

        //
        // MessageId: STG_E_DISKISWRITEPROTECTED
        //
        // MessageText:
        //
        //  Disk is write-protected.
        //
        public const int DISKISWRITEPROTECTED = unchecked((int)0x80030013);

        //
        // MessageId: STG_E_SEEKERROR
        //
        // MessageText:
        //
        //  An error occurred during a seek operation.
        //
        public const int SEEKERROR = unchecked((int)0x80030019);

        //
        // MessageId: STG_E_WRITEFAULT
        //
        // MessageText:
        //
        //  A disk error occurred during a write operation.
        //
        public const int WRITEFAULT = unchecked((int)0x8003001D);

        //
        // MessageId: STG_E_READFAULT
        //
        // MessageText:
        //
        //  A disk error occurred during a read operation.
        //
        public const int READFAULT = unchecked((int)0x8003001E);

        //
        // MessageId: STG_E_SHAREVIOLATION
        //
        // MessageText:
        //
        //  A share violation has occurred.
        //
        public const int SHAREVIOLATION = unchecked((int)0x80030020);

        //
        // MessageId: STG_E_LOCKVIOLATION
        //
        // MessageText:
        //
        //  A lock violation has occurred.
        //
        public const int LOCKVIOLATION = unchecked((int)0x80030021);

        //
        // MessageId: STG_E_FILEALREADYEXISTS
        //
        // MessageText:
        //
        //  %1 already exists.
        //
        public const int FILEALREADYEXISTS = unchecked((int)0x80030050);

        //
        // MessageId: STG_E_INVALIDPARAMETER
        //
        // MessageText:
        //
        //  Invalid parameter error.
        //
        public const int INVALIDPARAMETER = unchecked((int)0x80030057);

        //
        // MessageId: STG_E_MEDIUMFULL
        //
        // MessageText:
        //
        //  There is insufficient disk space to complete operation.
        //
        public const int MEDIUMFULL = unchecked((int)0x80030070);

        //
        // MessageId: STG_E_PROPSETMISMATCHED
        //
        // MessageText:
        //
        //  Illegal write of non-simple property to simple property set.
        //
        public const int PROPSETMISMATCHED = unchecked((int)0x800300F0);

        //
        // MessageId: STG_E_ABNORMALAPIEXIT
        //
        // MessageText:
        //
        //  An API call exited abnormally.
        //
        public const int ABNORMALAPIEXIT = unchecked((int)0x800300FA);

        //
        // MessageId: STG_E_INVALIDHEADER
        //
        // MessageText:
        //
        //  The file %1 is not a valid compound file.
        //
        public const int INVALIDHEADER = unchecked((int)0x800300FB);

        //
        // MessageId: STG_E_INVALIDNAME
        //
        // MessageText:
        //
        //  The name %1 is not valid.
        //
        public const int INVALIDNAME = unchecked((int)0x800300FC);

        //
        // MessageId: STG_E_UNKNOWN
        //
        // MessageText:
        //
        //  An unexpected error occurred.
        //
        public const int UNKNOWN = unchecked((int)0x800300FD);

        //
        // MessageId: STG_E_UNIMPLEMENTEDFUNCTION
        //
        // MessageText:
        //
        //  That function is not implemented.
        //
        public const int UNIMPLEMENTEDFUNCTION = unchecked((int)0x800300FE);

        //
        // MessageId: STG_E_INVALIDFLAG
        //
        // MessageText:
        //
        //  Invalid flag error.
        //
        public const int INVALIDFLAG = unchecked((int)0x800300FF);

        //
        // MessageId: STG_E_INUSE
        //
        // MessageText:
        //
        //  Attempted to use an object that is busy.
        //
        public const int INUSE = unchecked((int)0x80030100);

        //
        // MessageId: STG_E_NOTCURRENT
        //
        // MessageText:
        //
        //  The storage has been changed since the last commit.
        //
        public const int NOTCURRENT = unchecked((int)0x80030101);

        //
        // MessageId: STG_E_REVERTED
        //
        // MessageText:
        //
        //  Attempted to use an object that has ceased to exist.
        //
        public const int REVERTED = unchecked((int)0x80030102);

        //
        // MessageId: STG_E_CANTSAVE
        //
        // MessageText:
        //
        //  Can't save.
        //
        public const int CANTSAVE = unchecked((int)0x80030103);

        //
        // MessageId: STG_E_OLDFORMAT
        //
        // MessageText:
        //
        //  The compound file %1 was produced with an incompatible version of storage.
        //
        public const int OLDFORMAT = unchecked((int)0x80030104);

        //
        // MessageId: STG_E_OLDDLL
        //
        // MessageText:
        //
        //  The compound file %1 was produced with a newer version of storage.
        //
        public const int OLDDLL = unchecked((int)0x80030105);

        //
        // MessageId: STG_E_SHAREREQUIRED
        //
        // MessageText:
        //
        //  Share.exe or equivalent is required for operation.
        //
        public const int SHAREREQUIRED = unchecked((int)0x80030106);

        //
        // MessageId: STG_E_NOTFILEBASEDSTORAGE
        //
        // MessageText:
        //
        //  Illegal operation called on non-file based storage.
        //
        public const int NOTFILEBASEDSTORAGE = unchecked((int)0x80030107);

        //
        // MessageId: STG_E_EXTANTMARSHALLINGS
        //
        // MessageText:
        //
        //  Illegal operation called on object with extant marshallings.
        //
        public const int EXTANTMARSHALLINGS = unchecked((int)0x80030108);

        //
        // MessageId: STG_E_DOCFILECORRUPT
        //
        // MessageText:
        //
        //  The docfile has been corrupted.
        //
        public const int DOCFILECORRUPT = unchecked((int)0x80030109);

        //
        // MessageId: STG_E_BADBASEADDRESS
        //
        // MessageText:
        //
        //  OLE32.DLL has been loaded at the wrong address.
        //
        public const int BADBASEADDRESS = unchecked((int)0x80030110);

        //
        // MessageId: STG_E_DOCFILETOOLARGE
        //
        // MessageText:
        //
        //  The compound file is too large for the current implementation
        //
        public const int DOCFILETOOLARGE = unchecked((int)0x80030111);

        //
        // MessageId: STG_E_NOTSIMPLEFORMAT
        //
        // MessageText:
        //
        //  The compound file was not created with the STGM_SIMPLE flag
        //
        public const int NOTSIMPLEFORMAT = unchecked((int)0x80030112);

        //
        // MessageId: STG_E_INCOMPLETE
        //
        // MessageText:
        //
        //  The file download was aborted abnormally.  The file is incomplete.
        //
        public const int INCOMPLETE = unchecked((int)0x80030201);

        //
        // MessageId: STG_E_TERMINATED
        //
        // MessageText:
        //
        //  The file download has been terminated.
        //
        public const int TERMINATED = unchecked((int)0x80030202);

        /*++

             MessageId's 0x0305 - 0x031f (inclusive) are reserved for **STORAGE**
             copy protection errors.

            --*/
        //
        // MessageId: STG_E_STATUS_COPY_PROTECTION_FAILURE
        //
        // MessageText:
        //
        //  Generic Copy Protection Error.
        //
        public const int STATUS_COPY_PROTECTION_FAILURE = unchecked((int)0x80030305);

        //
        // MessageId: STG_E_CSS_AUTHENTICATION_FAILURE
        //
        // MessageText:
        //
        //  Copy Protection Error - DVD CSS Authentication failed.
        //
        public const int CSS_AUTHENTICATION_FAILURE = unchecked((int)0x80030306);

        //
        // MessageId: STG_E_CSS_KEY_NOT_PRESENT
        //
        // MessageText:
        //
        //  Copy Protection Error - The given sector does not have a valid CSS key.
        //
        public const int CSS_KEY_NOT_PRESENT = unchecked((int)0x80030307);

        //
        // MessageId: STG_E_CSS_KEY_NOT_ESTABLISHED
        //
        // MessageText:
        //
        //  Copy Protection Error - DVD session key not established.
        //
        public const int CSS_KEY_NOT_ESTABLISHED = unchecked((int)0x80030308);

        //
        // MessageId: STG_E_CSS_SCRAMBLED_SECTOR
        //
        // MessageText:
        //
        //  Copy Protection Error - The read failed because the sector is encrypted.
        //
        public const int CSS_SCRAMBLED_SECTOR = unchecked((int)0x80030309);

        //
        // MessageId: STG_E_CSS_REGION_MISMATCH
        //
        // MessageText:
        //
        //  Copy Protection Error - The current DVD's region does not correspond to the region setting of the drive.
        //
        public const int CSS_REGION_MISMATCH = unchecked((int)0x8003030A);

        //
        // MessageId: STG_E_RESETS_EXHAUSTED
        //
        // MessageText:
        //
        //  Copy Protection Error - The drive's region setting may be permanent or the number of user resets has been exhausted.
        //
        public const int RESETS_EXHAUSTED = unchecked((int)0x8003030B);

        //
        // "The network location cannot be reached. For information about network troubleshooting, see Windows Help."
        //
        public const int NETWORKUNREACHABLE = unchecked((int)0x800704CF);

        public const int BAD_NETPATH = unchecked((int)0x80070035);
        public const int ERROR_REM_NOT_LIST = unchecked((int)0x80070033);
        public const int NETWORK_BUSY = unchecked((int)0x80070036);
        public const int DEV_NOT_EXIST = unchecked((int)0x80070037);

        //
        // "Logon failure: unknown user name or bad password."
        //
        public const int LOGON_FAILURE = unchecked((int)0x8007052E);

    }

    public class STG_S
    {

        //
        // MessageId: STG_S_CONVERTED
        //
        // MessageText:
        //
        //  The underlying file was converted to compound file format.
        //
        public const int CONVERTED = unchecked((int)0x00030200);

        //
        // MessageId: STG_S_BLOCK
        //
        // MessageText:
        //
        //  The storage operation should block until more data is available.
        //
        public const int BLOCK = unchecked((int)0x00030201);

        //
        // MessageId: STG_S_RETRYNOW
        //
        // MessageText:
        //
        //  The storage operation should retry immediately.
        //
        public const int RETRYNOW = unchecked((int)0x00030202);

        //
        // MessageId: STG_S_MONITORING
        //
        // MessageText:
        //
        //  The notified event sink will not influence the storage operation.
        //
        public const int MONITORING = unchecked((int)0x00030203);

        //
        // MessageId: STG_S_MULTIPLEOPENS
        //
        // MessageText:
        //
        //  Multiple opens prevent consolidated. (commit succeeded).
        //
        public const int MULTIPLEOPENS = unchecked((int)0x00030204);

        //
        // MessageId: STG_S_CONSOLIDATIONFAILED
        //
        // MessageText:
        //
        //  Consolidation of the storage file failed. (commit succeeded).
        //
        public const int CONSOLIDATIONFAILED = unchecked((int)0x00030205);

        //
        // MessageId: STG_S_CANNOTCONSOLIDATE
        //
        // MessageText:
        //
        //  Consolidation of the storage file is inappropriate. (commit succeeded).
        //
        public const int CANNOTCONSOLIDATE = unchecked((int)0x00030206);

    }
}
