// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenLiveWriter.Interop.Windows
{
    /// <summary>
    /// Summary description for WinInet.
    /// </summary>
    public class WinInet
    {
        /// <summary>
        /// Set property that controls global WorkOffline state. This code
        /// is based on the MSDN article "Supporting Offline Browsing in
        /// Applications and Components" at:
        ///	 http://msdn.microsoft.com/workshop/components/offline/offline.asp
        /// Note: 'unsafe' is used because we need the address-of operator (&)
        /// to use the lpBuffer parameter of InternetSetOption and
        /// InternetQueryOption.
        /// </summary>
        public unsafe static bool WorkOffline
        {
            // determine the global offline state
            get
            {
                // setup parameters for query option
                uint state = 0;
                uint dwSize = (uint)Marshal.SizeOf(state);

                // check the connected state
                if (InternetQueryOption(IntPtr.Zero,
                    INTERNET_OPTION.CONNECTED_STATE, new IntPtr(&state), ref dwSize))
                {
                    // if we are disconnected and it was via user action, then
                    // we are in 'Work Offline' mode
                    if ((state & INTERNET_STATE.DISCONNECTED_BY_USER) > 0)
                        return true;
                }

                // if we don't detect WorkOffline, return false
                return false;
            }

            // set the global offline state
            set
            {
                // record user's desired state
                bool bWorkOffline = value;

                // initialize structure
                INTERNET_CONNECTED_INFO ci = new INTERNET_CONNECTED_INFO();
                if (bWorkOffline) // work offline
                {
                    ci.dwConnectedState = INTERNET_STATE.DISCONNECTED_BY_USER;
                    ci.dwFlags = ISO.FORCE_DISCONNECTED;
                }
                else // work connected
                {
                    ci.dwConnectedState = INTERNET_STATE.CONNECTED;
                    ci.dwFlags = 0;
                }

                // set the option
                InternetSetOption(IntPtr.Zero, INTERNET_OPTION.CONNECTED_STATE,
                    new IntPtr(&ci), (uint)Marshal.SizeOf(ci));
            }
        }

        [DllImport("WinInet.dll", CharSet = CharSet.Unicode)]
        public static extern bool InternetCheckConnection(
            [MarshalAs(UnmanagedType.LPTStr)] string lpszUrl, uint dwFlags, uint dwReserved);

        /// <summary>
        /// Sets an Internet option.
        /// </summary>
        [DllImport("WinInet.dll")]
        public static extern bool InternetSetOption(
            IntPtr hInternet, INTERNET_OPTION dwOption, IntPtr lpBuffer, uint dwBufferLength);

        [DllImport("WinInet.dll")]
        public static extern bool InternetSetCookieEx(string lpszUrl, string lpszCookieName, string lpszCookieData, uint dwFlags, IntPtr dwReserved);

        public static void InternetSetCookies(string lpszUrl, string lpszCookieName, string cookies)
        {
            try
            {
                if (cookies == null)
                    return;

                const uint INTERNET_COOKIE_EVALUATE_P3P = 0x00000040;
                string[] cookieStrings = cookies.Split(';');
                for (int i = 0; i < cookieStrings.Length; i++)
                    InternetSetCookieEx(lpszUrl, lpszCookieName, cookieStrings[i], INTERNET_COOKIE_EVALUATE_P3P, IntPtr.Zero);
            }
            catch (Exception e)
            {
                Trace.Fail("Error setting Internet Cookies: " + e.ToString());
            }
        }

        /// <summary>
        /// Queries an Internet option on the specified handle.
        /// </summary>
        [DllImport("WinInet.dll")]
        public static extern bool InternetQueryOption(
            IntPtr hInternet, INTERNET_OPTION dwOption, IntPtr lpBuffer, ref uint lpdwBufferLength);

        /// <summary>
        /// Initializes WinInet to prepare library for use
        /// </summary>
        [DllImport("wininet.dll", SetLastError = true)]
        public static extern IntPtr InternetOpen(
            string lpszAgent,
            int dwAccessType,
            string lpszProxyName, //always null
            string lpszProxyBypass,  // always null
            int dwFlags);

        /// <summary>
        /// Creates the internet connection
        /// </summary>
        [DllImport("wininet.dll", SetLastError = true)]
        public static extern IntPtr InternetConnect(
            IntPtr hInternet,
            string lpszServerName,
            int nServerPort,
            string lpszUsername,
            string lpszPassword,
            int dwService,
            int dwFlags,
            int dwContext
            );

        [DllImport("WinInet.dll")]
        public static extern bool InternetGetConnectedState(
            out uint lpdwFlags,
            int dwReserved);

        public static bool InternetConnectionAvailable
        {
            get
            {
                uint flags;
                return InternetGetConnectedState(out flags, 0);
            }
        }

        /// <summary>
        /// Gets information about a file in the internet explorer cache
        /// </summary>
        /// <param name="Url">The url of the file to check the cache for</param>
        /// <returns>The Internet_Cache_Entry_Info about the file</returns>
        public static bool GetUrlCacheEntryInfo(string url, out Internet_Cache_Entry_Info cacheInfo)
        {
            if (url == null)
            {
                cacheInfo = new Internet_Cache_Entry_Info();
                return false;
            }

            // Set up buffer
            int bufferSize =
                Marshal.SizeOf(typeof(Internet_Cache_Entry_Info)) + url.Length + Kernel32.MAX_PATH;
            IntPtr buffer = Marshal.AllocCoTaskMem(bufferSize);

            try
            {
                // Get the URL Cache info
                while (!GetUrlCacheEntryInfo(url, buffer, ref bufferSize))
                {
                    switch (Marshal.GetLastWin32Error())
                    {
                        // Grow the buffer
                        case ERROR.INSUFFICIENT_BUFFER:
                            buffer = Marshal.ReAllocCoTaskMem(buffer, bufferSize);
                            break;

                        // Didn't find the file!
                        case ERROR.FILE_NOT_FOUND:
                            cacheInfo = new Internet_Cache_Entry_Info();
                            return false;

                        // Some other exception occurred
                        default:
                            cacheInfo = new Internet_Cache_Entry_Info();
                            return false;
                    }
                }

                // The resulting cache info
                cacheInfo =
                    (Internet_Cache_Entry_Info)Marshal.PtrToStructure(buffer, typeof(Internet_Cache_Entry_Info));

                return true;
            }
            finally
            {
                Marshal.FreeCoTaskMem(buffer);
            }
        }

        /// <summary>
        /// Get Url Cache data for a cached item
        /// </summary>
        [DllImport("wininet.dll", SetLastError = true)]
        public static extern bool GetUrlCacheEntryInfo(
            string lpszUrlName,
            IntPtr lpCacheEntryInfo,
            ref int lpdwCacheEntryInfoBufferSize
            );

        [DllImport("wininet.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool CreateUrlCacheEntry(
            string lpszUrlName,
            uint dwExpectedFileSize,
            string lpszFileExtension,
            StringBuilder lpszFileName,
            uint dwReserved);

        [DllImport("wininet.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool CommitUrlCacheEntry(
            string lpszUrlName,
            string lpszLocalFileName,
            System.Runtime.InteropServices.ComTypes.FILETIME ExpireTime,
            System.Runtime.InteropServices.ComTypes.FILETIME LastModifiedTime,
            uint CacheEntryType,
            IntPtr lpHeaderInfo,
            uint dwHeaderSize,
            IntPtr lpszFileExtension,
            IntPtr lpszOriginalUrl
            );

        /// <summary>
        /// Sets the current directory on the FTP Server
        /// </summary>
        [DllImport("wininet.dll", SetLastError = true)]
        public static extern bool FtpSetCurrentDirectory(
            IntPtr hConnect,
            string lpszDirectory
            );

        /// <summary>
        /// Gets the current directory on the FTP Server
        /// </summary>
        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool FtpGetCurrentDirectory(
            IntPtr hConnect,
            StringBuilder lpszDirectory,
            ref uint lpdwBufferLength
            );

        /// <summary>
        /// Creates a directory on the FTP Server
        /// </summary>
        [DllImport("wininet.dll", SetLastError = true)]
        public static extern bool FtpCreateDirectory(
            IntPtr hConnect,
            string lpszDirectory
            );

        /// <summary>
        /// Deletes a file from the server
        /// </summary>
        [DllImport("wininet.dll", SetLastError = true)]
        public static extern bool FtpDeleteFile(
            IntPtr hConnect,
            string lpszFilename
            );

        /// <summary>
        /// Puts the file onto the FTP Server
        /// </summary>
        [DllImport("wininet.dll", SetLastError = true)]
        public static extern bool FtpPutFile(
            IntPtr hConnect,
            string lpszLocalFile,
            string lpszNewRemoteFile,
            int dwFlags,
            int dwContext
            );

        /// <summary>
        /// Gets a file from the FTP server and copies it locally
        /// </summary>
        [DllImport("wininet.dll", SetLastError = true)]
        public static extern bool FtpGetFile(
            IntPtr hConnect,
            string lpszLocalFile,
            string lpszNewRemoteFile,
            bool fFailIfExists,
            int dwAttributes,
            int dwFlags,
            int dwContext
            );

        /// <summary>
        /// Starts an FTP Find command, and returns the first file found.
        /// </summary>
        [DllImport("wininet.dll", SetLastError = true)]
        private static extern IntPtr FtpFindFirstFile(
            IntPtr hConnect,
            string lpszSearchFile,
            IntPtr lpWin32FindData,
            int dwFlags,
            int dwContext
            );

        /// <summary>
        /// Returns the next file in an FTP find command.
        /// </summary>
        /// <param name="hFindFile">The handle to the open find handle (created using FtpFindFirstFile)</param>
        /// <param name="lpWin32FindData">The result Win32_Find_Data structure</param>
        /// <returns>true if the call succeeded</returns>
        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetFindNextFile(
            IntPtr hFindFile,
            IntPtr lpWin32FindData
            );

        /// <summary>
        /// Closes the handle to an internet connection
        /// </summary>
        [DllImport("wininet.dll", SetLastError = true)]
        public static extern bool InternetCloseHandle(
            IntPtr hInternet
            );

        /// <summary>
        /// Retrieves the last error description or server response on
        /// the thread calling this function
        /// </summary>
        [DllImport("wininet.dll", SetLastError = true)]
        public static extern bool InternetGetLastResponseInfo(
            out uint lpdwError,
            StringBuilder lpszBuffer,
            ref uint lpdwBufferLength);

        /// <summary>
        /// Retrieves the set of files that match the specified search string.
        /// </summary>
        /// <param name="hConnect"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        public static Win32_Find_Data[] FtpFind(IntPtr hConnect, string search)
        {

            ArrayList list = new ArrayList();
            Win32_Find_Data findData = new Win32_Find_Data();

            // Set up buffer
            int bufferSize = Marshal.SizeOf(typeof(Win32_Find_Data)) + Kernel32.MAX_PATH + 14;
            IntPtr buffer = Marshal.AllocCoTaskMem(bufferSize);
            IntPtr m_hFind = FtpFindFirstFile(hConnect, search, buffer, 0, 0);
            try
            {
                if (m_hFind != IntPtr.Zero)
                {
                    findData = (Win32_Find_Data)Marshal.PtrToStructure(buffer, typeof(Win32_Find_Data));
                    list.Add(findData);

                    //Iterate over the next files
                    while (InternetFindNextFile(m_hFind, buffer))
                    {
                        findData = (Win32_Find_Data)Marshal.PtrToStructure(buffer, typeof(Win32_Find_Data));
                        list.Add(findData);
                    }
                }

                //return the list of Win32_Find_Data structures
                return (Win32_Find_Data[])list.ToArray(typeof(Win32_Find_Data));
            }
            finally
            {
                Marshal.FreeCoTaskMem(buffer);
                if (m_hFind != IntPtr.Zero) InternetCloseHandle(m_hFind);
            }
        }

        public static string GetFtpCurrentDirectory(IntPtr hConnect)
        {
            // the starting bufferLength
            uint bufferLength = 512;

            // The string buffer that will hold the extended information
            StringBuilder stringBuffer = new StringBuilder((int)bufferLength);

            // try to get the extended information.  If this returns false, there was an
            // error (likely, the buffer isn't large enough)
            if (!WinInet.FtpGetCurrentDirectory(
                hConnect,
                stringBuffer,
                ref bufferLength))
            {

                // If the buffer wasn't large enough
                int error = Marshal.GetLastWin32Error();
                if (error == ERROR.INSUFFICIENT_BUFFER)
                {
                    // Resize the buffer to the size specified in the results of the
                    // last call and try again.
                    stringBuffer.Capacity = (int)bufferLength;
                    WinInet.FtpGetCurrentDirectory(
                        hConnect,
                        stringBuffer,
                        ref bufferLength);
                }
                else
                {
                    Debug.Assert(false, "Error getting current directory");
                    return "/";
                }

            }
            return stringBuffer.ToString();
        }

        /// <summary>
        /// Returns any extended information from the last call to WinInet.
        /// </summary>
        /// <returns>A string containing the extended information</returns>
        public static string GetExtendedInfo()
        {
            // holder for any error code
            uint errorCode;

            // the starting bufferLength
            uint bufferLength = 512;

            // The string buffer that will hold the extended information
            StringBuilder stringBuffer = new StringBuilder((int)bufferLength);

            // try to get the extended information.  If this returns false, there was an
            // error (likely, the buffer isn't large enough)
            if (!WinInet.InternetGetLastResponseInfo(
                out errorCode,
                stringBuffer,
                ref bufferLength))
            {

                // If the buffer wasn't large enough
                int error = Marshal.GetLastWin32Error();
                if (error == ERROR.INSUFFICIENT_BUFFER)
                {
                    // Resize the buffer to the size specified in the results of the
                    // last call and try again.
                    stringBuffer.Capacity = (int)bufferLength;
                    WinInet.InternetGetLastResponseInfo(
                        out errorCode,
                        stringBuffer,
                        ref bufferLength);

                }
                else
                {
                    Debug.Assert(false, "Error getting extended info");
                }

            }

            return stringBuffer.ToString();
        }

    }

    public struct FLAG_ICC
    {
        public const uint FORCE_CONNECTION = 0x00000001;
    }

    public struct CACHE_ENTRY
    {
        public const uint NORMAL = 0x00000001;
        public const uint STICKY = 0x00000004;
        public const uint EDITED = 0x00000008;
        public const uint TRACK_OFFLINE = 0x00000010;
        public const uint TRACK_ONLINE = 0x00000020;
        public const uint SPARSE = 0x00010000;
        public const uint COOKIE = 0x00100000;
        public const uint URLHISTORY = 0x00200000;
    }

    // Struct representing the information retrieved about a URL cache entry
    public struct Internet_Cache_Entry_Info
    {
        public int dwStructSize;
        public string lpszSourceUrlName;
        public string lpszLocalFileName;
        public int CacheEntryType;
        public int dwUseCount;
        public int dwHitRate;
        public int dwSizeLow;
        public int dwSizeHigh;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastModifiedTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME ExpireTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastAccessTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastSyncTime;
        public IntPtr lpHeaderInfo;
        public int dwHeaderInfoSize;
        public string lpszFileExtension;
        public int dwExemptDelta;
    }

    //Represents the _WIN32_FIND_DATA structure
    public struct Win32_Find_Data
    {
        public int dwFileAttributes;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
        public int nFileSizeHigh;
        public int nFileSizeLow;
        public int dwReserved0;
        public int dwReserved1;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string cFileName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
        public string cAlternateFileName;
    }


    /// <summary>
    /// Enumeration of WinInet option flags
    /// </summary>
    [Flags]
    public enum INTERNET_OPTION : uint
    {
        CONNECTED_STATE = 50
    }

    /// <summary>
    /// Enumeration of internet states
    /// </summary>
    public struct INTERNET_STATE
    {
        /// <summary>
        /// connected state (mutually exclusive with disconnected)
        /// </summary>
        public const uint CONNECTED = 0x00000001;

        /// <summary>
        /// disconnected from network
        /// </summary>
        public const uint DISCONNECTED = 0x00000002;

        /// <summary>
        /// disconnected by user request
        /// </summary>
        public const uint DISCONNECTED_BY_USER = 0x00000010;

        /// <summary>
        /// no network requests being made (by Wininet)
        /// </summary>
        public const uint STATE_IDLE = 0x00000100;

        /// <summary>
        /// network requests being made (by Wininet)
        /// </summary>
        public const uint STATE_BUSY = 0x00000200;
    }

    /// <summary>
    /// Information used to set the global connected state
    /// </summary>
    public struct INTERNET_CONNECTED_INFO
    {
        /// <summary>
        /// dwConnectedState - new connected/disconnected state.
        /// See INTERNET_STATE_CONNECTED, etc.
        /// </summary>
        public uint dwConnectedState;

        // dwFlags - flags controlling connected->disconnected (or disconnected->
        // connected) transition (see below for definition of ISO enum)
        public uint dwFlags;
    }

    /// <summary>
    /// InternetSetOption flags (used in INTERNET_CONNECTED_INFO structure)
    /// </summary>
    public struct ISO
    {
        /// <summary>
        /// If set when putting Wininet into disconnected mode, all outstanding
        /// requests will be aborted with a cancelled error
        /// </summary>
        public const uint FORCE_DISCONNECTED = 0x00000001;
    }

    /// <summary>
    /// Values for base internet connection
    /// </summary>
    public struct INTERNET
    {
        /// <summary>
        /// Use the Protocol Specific Default
        /// </summary>
        public const int INVALID_PORT_NUMBER = 0;

        /// <summary>
        /// The default FTP Port
        /// </summary>
        public const int DEFAULT_FTP_PORT = 21;

        /// <summary>
        /// The default Gopher Port
        /// </summary>
        public const int DEFAULT_GOPHER_PORT = 70;

        /// <summary>
        /// The default HTTP port
        /// </summary>
        public const int DEFAULT_HTTP_PORT = 80;

        /// <summary>
        /// The default HTTPS port
        /// </summary>
        public const int DEFAULT_HTTPS_PORT = 443;

        /// <summary>
        /// Default for SOCKS firewall servers
        /// </summary>
        public const int DEFAULT_SOCKS_PORT = 1080;

        /// <summary>
        /// Maximum Length of a host name
        /// </summary>
        public const int MAX_HOST_NAME_LENGTH = 256;

        /// <summary>
        /// Maximum length of a user name
        /// </summary>
        public const int MAX_USER_NAME_LENGTH = 128;

        /// <summary>
        /// Maximum Length of a password
        /// </summary>
        public const int MAX_PASSWORD_LENGTH = 128;

        /// <summary>
        /// INTERNET_PORT is unsigned short
        /// </summary>
        public const int MAX_PORT_NUMBER_LENGTH = 5;

        /// <summary>
        /// maximum unsigned short value
        /// </summary>
        public const int MAX_PORT_NUMBER_VALUE = 65535;

        /// <summary>
        /// Maximum length of a path
        /// </summary>
        public const int MAX_PATH_LENGTH = 2048;

        /// <summary>
        /// Longest Protocol name length
        /// </summary>
        public const int MAX_SCHEME_LENGTH = 32;

        /// <summary>
        /// Keep alive is enabled
        /// </summary>
        public const int KEEP_ALIVE_ENABLED = 1;

        /// <summary>
        /// Keep alive is disabled
        /// </summary>
        public const int KEEP_ALIVE_DISABLED = 0;
    }

    /// <summary>
    /// Flags used for controlling connections
    /// </summary>
    public struct INTERNET_FLAG
    {
        /// <summary>
        /// retrieve the original item
        /// </summary>
        public const uint RELOAD = 0x80000000;

        /// <summary>
        /// FTP/gopher find: receive the item as raw (structured) data
        /// </summary>
        public const uint RAW_DATA = 0x40000000;

        /// <summary>
        /// FTP: use existing InternetConnect handle for server if possible
        /// </summary>
        public const uint EXISTING_CONNECT = 0x20000000;

        /// <summary>
        /// this request is asynchronous (where supported)
        /// </summary>
        public const uint ASYNC = 0x10000000;

        /// <summary>
        /// used for FTP connections
        /// </summary>
        public const uint PASSIVE = 0x08000000;

        /// <summary>
        /// don't write this item to the cache
        /// </summary>
        public const uint NO_CACHE_WRITE = 0x04000000;

        /// <summary>
        /// don't write this item to the cache
        /// </summary>
        public const uint DONT_CACHE = 0x04000000;

        /// <summary>
        /// make this item persistent in cache
        /// </summary>
        public const uint MAKE_PERSISTENT = 0x02000000;

        /// <summary>
        /// use offline semantics
        /// </summary>
        public const uint FROM_CACHE = 0x01000000;

        /// <summary>
        /// use offline semantics
        /// </summary>
        public const uint OFFLINE = 0x01000000;

        /// <summary>
        /// use PCT/SSL if applicable (HTTP)
        /// </summary>
        public const uint SECURE = 0x00800000;

        /// <summary>
        /// use keep-alive semantics
        /// </summary>
        public const uint KEEP_CONNECTION = 0x00400000;

        /// <summary>
        /// don't handle redirections automatically
        /// </summary>
        public const uint NO_AUTO_REDIRECT = 0x00200000;

        /// <summary>
        /// do background read prefetch
        /// </summary>
        public const uint READ_PREFETCH = 0x00100000;

        /// <summary>
        /// no automatic cookie handling
        /// </summary>
        public const uint NO_COOKIES = 0x00080000;

        /// <summary>
        /// no automatic authentication handling
        /// </summary>
        public const uint NO_AUTH = 0x00040000;

        /// <summary>
        /// return cache file if net request fails
        /// </summary>
        public const uint CACHE_IF_NET_FAIL = 0x00010000;

        /// <summary>
        /// ex: https:// to http://
        /// </summary>
        public const uint IGNORE_REDIRECT_TO_HTTP = 0x00008000;

        /// <summary>
        /// ex: http:// to https://
        /// </summary>
        public const uint IGNORE_REDIRECT_TO_HTTPS = 0x00004000;

        /// <summary>
        /// expired X509 Cert.
        /// </summary>
        public const uint IGNORE_CERT_DATE_INVALID = 0x00002000;

        /// <summary>
        /// bad common name in X509 Cert.
        /// </summary>
        public const uint IGNORE_CERT_CN_INVALID = 0x00001000;

        /// <summary>
        /// asking wininet to update an item if it is newer
        /// </summary>
        public const uint RESYNCHRONIZE = 0x00000800;

        /// <summary>
        /// asking wininet to do hyperlinking semantic which works right for scripts
        /// </summary>
        public const uint HYPERLINK = 0x00000400;

        /// <summary>
        /// no cookie popup
        /// </summary>
        public const uint NO_UI = 0x00000200;

        /// <summary>
        /// asking wininet to add "pragma: no-cache"
        /// </summary>
        public const uint PRAGMA_NOCACHE = 0x00000100;

        /// <summary>
        /// ok to perform lazy cache-write
        /// </summary>
        public const uint CACHE_ASYNC = 0x00000080;

        /// <summary>
        /// this is a forms submit
        /// </summary>
        public const uint FORMS_SUBMIT = 0x00000040;

        /// <summary>
        /// fwd-back button op
        /// </summary>
        public const uint FWD_BACK = 0x00000020;

        /// <summary>
        /// need a file for this request
        /// </summary>
        public const uint NEED_FILE = 0x00000010;

        /// <summary>
        /// need a file for this request
        /// </summary>
        public const uint MUST_CACHE_REQUEST = 0x00000010;

        public const uint TRANSFER_ASCII = 0x00000001;
        public const uint TRANSFER_BINARY = 0x00000002;
    }

    /// <summary>
    /// Determines how internet connection uses proxy
    /// </summary>
    public struct INTERNET_OPEN
    {
        /// <summary>
        /// use registry configuration
        /// </summary>
        public const int TYPE_PRECONFIG = 0;

        /// <summary>
        /// direct to net
        /// </summary>
        public const int TYPE_DIRECT = 1;

        /// <summary>
        /// via named proxy
        /// </summary>
        public const int TYPE_PROXY = 3;

        /// <summary>
        /// prevent using java/script/INS
        /// </summary>
        public const int TYPE_PRECONFIG_WITH_NO_AUTOPROXY = 4;

    }

    /// <summary>
    /// service types for InternetConnect()
    /// </summary>
    public struct INTERNET_SERVICE
    {
        public const int FTP = 1;
        public const int GOPHER = 2;
        public const int HTTP = 3;
    }

    /// <summary>
    /// Error codes that can be returned for WinInet Errors.
    /// Use kernel32 GetLastError() to get the last error on the thread
    /// </summary>
    public struct ERROR_INTERNET
    {
        public const int BASE = 12000;

        public const int OUT_OF_HANDLES = (BASE + 1);
        public const int TIMEOUT = (BASE + 2);
        public const int EXTENDED_ERROR = (BASE + 3);
        public const int INTERNAL_ERROR = (BASE + 4);
        public const int INVALID_URL = (BASE + 5);
        public const int UNRECOGNIZED_SCHEME = (BASE + 6);
        public const int NAME_NOT_RESOLVED = (BASE + 7);
        public const int PROTOCOL_NOT_FOUND = (BASE + 8);
        public const int INVALID_OPTION = (BASE + 9);
        public const int BAD_OPTION_LENGTH = (BASE + 10);
        public const int OPTION_NOT_SETTABLE = (BASE + 11);
        public const int SHUTDOWN = (BASE + 12);
        public const int INCORRECT_USER_NAME = (BASE + 13);
        public const int INCORRECT_PASSWORD = (BASE + 14);
        public const int LOGIN_FAILURE = (BASE + 15);
        public const int INVALID_OPERATION = (BASE + 16);
        public const int OPERATION_CANCELLED = (BASE + 17);
        public const int INCORRECT_HANDLE_TYPE = (BASE + 18);
        public const int INCORRECT_HANDLE_STATE = (BASE + 19);
        public const int NOT_PROXY_REQUEST = (BASE + 20);
        public const int REGISTRY_VALUE_NOT_FOUND = (BASE + 21);
        public const int BAD_REGISTRY_PARAMETER = (BASE + 22);
        public const int NO_DIRECT_ACCESS = (BASE + 23);
        public const int NO_CONTEXT = (BASE + 24);
        public const int NO_CALLBACK = (BASE + 25);
        public const int REQUEST_PENDING = (BASE + 26);
        public const int INCORRECT_FORMAT = (BASE + 27);
        public const int ITEM_NOT_FOUND = (BASE + 28);
        public const int CANNOT_CONNECT = (BASE + 29);
        public const int CONNECTION_ABORTED = (BASE + 30);
        public const int CONNECTION_RESET = (BASE + 31);
        public const int FORCE_RETRY = (BASE + 32);
        public const int INVALID_PROXY_REQUEST = (BASE + 33);
        public const int NEED_UI = (BASE + 34);

        public const int HANDLE_EXISTS = (BASE + 36);
        public const int SEC_CERT_DATE_INVALID = (BASE + 37);
        public const int SEC_CERT_CN_INVALID = (BASE + 38);
        public const int HTTP_TO_HTTPS_ON_REDIR = (BASE + 39);
        public const int HTTPS_TO_HTTP_ON_REDIR = (BASE + 40);
        public const int MIXED_SECURITY = (BASE + 41);
        public const int CHG_POST_IS_NON_SECURE = (BASE + 42);
        public const int POST_IS_NON_SECURE = (BASE + 43);
        public const int CLIENT_AUTH_CERT_NEEDED = (BASE + 44);
        public const int INVALID_CA = (BASE + 45);
        public const int CLIENT_AUTH_NOT_SETUP = (BASE + 46);
        public const int ASYNC_THREAD_FAILED = (BASE + 47);
        public const int REDIRECT_SCHEME_CHANGE = (BASE + 48);
        public const int DIALOG_PENDING = (BASE + 49);
        public const int RETRY_DIALOG = (BASE + 50);
        public const int HTTPS_HTTP_SUBMIT_REDIR = (BASE + 52);
        public const int INSERT_CDROM = (BASE + 53);
        public const int FORTEZZA_LOGIN_NEEDED = (BASE + 54);
        public const int SEC_CERT_ERRORS = (BASE + 55);
        public const int SEC_CERT_NO_REV = (BASE + 56);
        public const int SEC_CERT_REV_FAILED = (BASE + 57);
    }

    /// <summary>
    /// FTP API errors
    /// </summary>
    public struct ERROR_FTP
    {
        public const int TRANSFER_IN_PROGRESS = (ERROR_INTERNET.BASE + 110);
        public const int DROPPED = (ERROR_INTERNET.BASE + 111);
        public const int NO_PASSIVE_MODE = (ERROR_INTERNET.BASE + 112);
    }
}
