// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Windows
{
    /// <summary>
    /// Summary description for Mapi32.
    /// </summary>
    public class Mapi32
    {
        // Mapi functions, structures, and constants (based on declarations found
        // in Microsoft Knowledge Base Article Q315653, SAMPLE: SimpleMAPIAssembly
        // Demonstrates Use of Simple MAPI from a .NET Application at:
        //	http://support.microsoft.com/default.aspx?scid=kb;EN-US;Q315653

        /// <summary>
        /// DLL to load MAPI entry points from
        /// </summary>
        private const string MAPIDLL = "mapi32.dll";

        /// <summary>
        /// Begins a Simple MAPI session, loading the default message store provider
        /// </summary>
        [DllImport(MAPIDLL, CharSet=CharSet.Ansi)]
        public static extern int MAPILogon(
            int ulUIParam,
            string lpszProfileName,
            string lpszPassword,
            int flFlags,
            int ulReserved,
            out int lplhSession);

        /// <summary>
        /// Ends a session with the messaging system.
        /// </summary>
        [DllImport(MAPIDLL, CharSet=CharSet.Ansi)]
        public static extern int MAPILogoff(
            int lhSession,
            int ulUIParam,
            int flFlags,
            int ulReserved);

        /// <summary>
        /// Sends a message using the messaging system.
        /// </summary>
        [DllImport(MAPIDLL, CharSet=CharSet.Ansi)]
        public static extern int MAPISendMail(
            int lhSession,
            int ulUIParam,
            IntPtr /*MapiMessage*/ lpMessage,
            int flFlags,
            int ulReserved);

    }

    /// <summary>
    /// Structure that contains information about a MAPI mail messsage
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct MapiMessage
    {
        public int ulReserved;
        public String lpszSubject;
        public String lpszNoteText;
        public String lpszMessageType;
        public String lpszDateReceived;
        public String lpszConversationID;
        public int flFlags;
        public IntPtr lpOriginator;
        public int nRecipCount;
        public IntPtr lpRecips;
        public int nFileCount;
        public IntPtr /*MapiFileDesc[]*/ lpFiles;
    };

    /// <summary>
    /// Structure that contains the MAPI recipient descriptor
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct MapiRecipDesc
    {
        public int ulReserved;
        public int ulReciptClass;
        public String lpszName;
        public String lpszAddress;
        public int ulEIDSize;
        public IntPtr lpEntryID;
    }

    /// <summary>
    /// Structure that contains MAPI file attachment descriptor
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct MapiFileDesc
    {
        public int ulReserved;
        public int flFlags;
        public int nPosition;
        public String lpszPathName;
        public String lpszFileName;
        public IntPtr /*MapiFileTagExt*/ lpFileType;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct MapiFileTagExt
    {
        public int Reserved;		/* Reserved, must be zero.                  */
        public int cbTag;			/* Size (in bytes) of                       */
        public String szTag;		/* X.400 OID for this attachment type       */
        public int cbEncoding;		/* Size (in bytes) of                       */
        public String szEncoding;	/* X.400 OID for this attachment's encoding */
    };

    /// <summary>
    /// Structure containing MAPI error constants
    /// </summary>
    public struct MAPI
    {
        public const int SUCCESS_SUCCESS = 0;

        public const int E_USER_ABORT = 1;
        public const int E_FAILURE = 2;
        public const int E_LOGIN_FAILURE = 3;
        public const int E_DISK_FULL = 4;
        public const int E_INSUFFICIENT_MEMORY = 5;
        public const int E_BLK_TOO_SMALL = 6;
        public const int E_TOO_MANY_SESSIONS = 8;
        public const int E_TOO_MANY_FILES = 9;
        public const int E_TOO_MANY_RECIPIENTS = 10;
        public const int E_ATTACHMENT_NOT_FOUND = 11;
        public const int E_ATTACHMENT_OPEN_FAILURE = 12;
        public const int E_ATTACHMENT_WRITE_FAILURE = 13;
        public const int E_UNKNOWN_RECIPIENT = 14;
        public const int E_BAD_RECIPTYPE = 15;
        public const int E_NO_MESSAGES = 16;
        public const int E_INVALID_MESSAGE = 17;
        public const int E_TEXT_TOO_LARGE = 18;
        public const int E_INVALID_SESSION = 19;
        public const int E_TYPE_NOT_SUPPORTED = 20;
        public const int E_AMBIGUOUS_RECIPIENT = 21;
        public const int E_MESSAGE_IN_USE = 22;
        public const int E_NETWORK_FAILURE = 23;
        public const int E_INVALID_EDITFIELDS = 24;
        public const int E_INVALID_RECIPS = 25;
        public const int E_NOT_SUPPORTED = 26;
        public const int E_NO_LIBRARY = 999;
        public const int E_INVALID_PARAMETER = 998;

        public const int ORIG = 0;
        public const int TO = 1;
        public const int CC = 2;
        public const int BCC = 3;

        public const int UNREAD = 1;
        public const int RECEIPT_REQUESTED = 2;
        public const int SENT = 4;

        public const int LOGON_UI = 0x1;
        public const int NEW_SESSION = 0x2;
        public const int DIALOG = 0x8;

        public const int UNREAD_ONLY = 0x20;
        public const int EXTENDED = 0x20;
        public const int ENVELOPE_ONLY = 0x40;
        public const int PEEK = 0x80;
        public const int GUARANTEE_FIFO = 0x100;
        public const int BODY_AS_FILE = 0x200;
        public const int AB_NOMODIFY = 0x400;
        public const int SUPPRESS_ATTACH = 0x800;
        public const int FORCE_DOWNLOAD = 0x1000;
        public const int LONG_MSGID = 0x4000;
        public const int PASSWORD_UI = 0x20000;

        public const int OLE = 0x1;
        public const int OLE_STATIC = 0x2;
    }
}
