// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using OpenLiveWriter.Interop.Com;

namespace OpenLiveWriter.Interop.Windows
{
    /// <summary>
    /// Imports from UrlMon.dll
    /// </summary>
    public class UrlMon
    {
        [DllImport("urlmon.dll")]
        public static extern Int32 IsValidURL(
            [In] IntPtr pBC,
            [In] string szURL,
            [In] Int32 dxReserved);

        [DllImport("urlmon.dll")]
        public static extern int CreateURLMoniker(
            [In] IMoniker pmkContext,
            [In, MarshalAs(UnmanagedType.LPWStr)]  string szURL,
            [Out] out IMoniker ppmk);

        [DllImport("urlmon.dll", CharSet = CharSet.Auto)]
        public static extern int URLDownloadToFile(
            [In] IntPtr pCaller,
            [In, MarshalAs(UnmanagedType.LPTStr)] string szURL,
            [In, MarshalAs(UnmanagedType.LPTStr)] string szFileName,
            uint dwReserved,
            [In] IBindStatusCallback lpfnCB
            );
        [DllImport("urlmon.dll", CharSet = CharSet.Auto)]
        public static extern int URLOpenBlockingStream([In] IntPtr pCaller,
        [In, MarshalAs(UnmanagedType.LPTStr)] string szURL,
        out IStream ppStream,
        uint dwReserved,
        IBindStatusCallback lpfnCB);

        [DllImport("urlmon.dll")]
        public static extern int CoInternetSetFeatureEnabled(
            [In] int FEATURE,
            [In] int dwFlags,
            [In] bool fEnable);
    }
    public struct FEATURE
    {
        public const int OBJECT_CACHING = 0;
        public const int ZONE_ELEVATION = 1;
        public const int MIME_HANDLING = 2;
        public const int MIME_SNIFFING = 3;
        public const int WINDOW_RESTRICTIONS = 4;
        public const int WEBOC_POPUPMANAGEMENT = 5;
        public const int BEHAVIORS = 6;
        public const int DISABLE_MK_PROTOCOL = 7;
        public const int LOCALMACHINE_LOCKDOWN = 8;
        public const int SECURITYBAND = 9;
        public const int RESTRICT_ACTIVEXINSTALL = 10;
        public const int VALIDATE_NAVIGATE_URL = 11;
        public const int RESTRICT_FILEDOWNLOAD = 12;
        public const int ADDON_MANAGEMENT = 13;
        public const int PROTOCOL_LOCKDOWN = 14;
        public const int HTTP_USERNAME_PASSWORD_DISABLE = 15;
        public const int SAFE_BINDTOOBJECT = 16;
        public const int UNC_SAVEDFILECHECK = 17;
        public const int GET_URL_DOM_FILEPATH_UNENCODED = 18;
        public const int TABBED_BROWSING = 19;
        public const int SSLUX = 20;
        public const int DISABLE_NAVIGATION_SOUNDS = 21;
        public const int DISABLE_LEGACY_COMPRESSION = 22;
        public const int FORCE_ADDR_AND_STATUS = 23;
        public const int XMLHTTP = 24;
        public const int DISABLE_TELNET_PROTOCOL = 25;
        public const int FEEDS = 26;
        public const int BLOCK_INPUT_PROMPTS = 27;
        public const int ENTRY_COUNT = 28;

    }
    public struct INTERNETSETFEATURE
    {
        public const int ON_THREAD = 0x00000001;
        public const int ON_PROCESS = 0x00000002;
        public const int IN_REGISTRY = 0x00000004;
        public const int ON_THREAD_LOCALMACHINE = 0x00000008;
        public const int ON_THREAD_INTRANET = 0x00000010;
        public const int ON_THREAD_TRUSTED = 0x00000020;
        public const int ON_THREAD_INTERNET = 0x00000040;
        public const int ON_THREAD_RESTRICTED = 0x00000080;
    }

}
