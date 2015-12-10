// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace OpenLiveWriter.Mshtml.Mshtml_Interop
{
    /// <summary>
    /// Summary description for IInternetSecurityManager.
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("79eac9ee-baf9-11ce-8c82-00aa004ba90b")]
    public interface IInternetSecurityManager
    {
        [PreserveSig]
        int SetSecuritySite(
            [In] IntPtr pSite);

        [PreserveSig]
        int GetSecuritySite(
            [Out] out IntPtr pSite);

        [PreserveSig]
        int MapUrlToZone(
            [In][MarshalAs(UnmanagedType.LPWStr)] string pwszUrl,
            ref int pdwZone,
            [In] int dwFlags);

        [PreserveSig]
        int GetSecurityId(
            [In][MarshalAs(UnmanagedType.LPWStr)] string pwszUrl,
            [Out] out Byte pbSecurityId,
            ref int pcbSecurityId,
            [In] IntPtr dwReserved);

        [PreserveSig]
        int ProcessUrlAction(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwszUrl,
            [In] int dwAction,
            [Out] out byte pPolicy,
            [In] int cbPolicy,
            [In] IntPtr pContext,
            [In] int cbContext,
            [In] int dwFlags,
            [In] int dwReserved);

        [PreserveSig]
        int QueryCustomPolicy(
            [In][MarshalAs(UnmanagedType.LPWStr)] string pwszUrl,
            [In] ref Guid guidKey,
            [Out] out Byte ppPolicy,
            [Out] out int pcbPolicy,
            [In] Byte pContext,
            [In] int cbContext,
            [In] int dwReserved);

        [PreserveSig]
        int SetZoneMapping(
            [In] int dwZone,
            [In][MarshalAs(UnmanagedType.LPWStr)] string lpszPattern,
            [In] int dwFlags);

        [PreserveSig]
        int GetZoneMappings(
            [In] int dwZone,
            [Out] out IEnumString ppenumString,
            [In] int dwFlags);
    }

    #region INET_E Flags
    public struct INET_E
    {
        public const int USE_DEFAULT_PROTOCOLHANDLER = unchecked((int)0x800C0011);
        public const int USE_DEFAULT_SETTING = unchecked((int)0x800C0012);
        public const int DEFAULT_ACTION = USE_DEFAULT_PROTOCOLHANDLER;
        public const int QUERYOPTION_UNKNOWN = unchecked((int)0x800C0013);
        public const int REDIRECTING = unchecked((int)0x800C0014);
    }
    #endregion

    #region URLACTION Flags
    /// <summary>
    /// Common COM HRESULT codes
    /// </summary>
    public struct URLACTION
    {
        public const int MIN = 0x00001000;
        public const int DOWNLOAD_MIN = 0x00001000;
        public const int DOWNLOAD_SIGNED_ACTIVEX = 0x00001001;
        public const int DOWNLOAD_UNSIGNED_ACTIVEX = 0x00001004;
        public const int DOWNLOAD_CURR_MAX = 0x00001004;
        public const int DOWNLOAD_MAX = 0x000011FF;
        public const int ACTIVEX_MIN = 0x00001200;
        public const int ACTIVEX_RUN = 0x00001200;
        public const int ACTIVEX_OVERRIDE_OBJECT_SAFETY = 0x00001201;
        public const int ACTIVEX_OVERRIDE_DATA_SAFETY = 0x00001202;
        public const int ACTIVEX_OVERRIDE_SCRIPT_SAFETY = 0x00001203;
        public const int SCRIPT_OVERRIDE_SAFETY = 0x00001401;
        public const int ACTIVEX_CONFIRM_NOOBJECTSAFETY = 0x00001204;
        public const int ACTIVEX_TREATASUNTRUSTED = 0x00001205;
        public const int ACTIVEX_NO_WEBOC_SCRIPT = 0x00001206;
        public const int ACTIVEX_OVERRIDE_REPURPOSEDETECTION = 0x00001207;
        public const int ACTIVEX_OVERRIDE_OPTIN = 0x00001208;
        public const int ACTIVEX_SCRIPTLET_RUN = 0x00001209;
        public const int ACTIVEX_DYNSRC_VIDEO_AND_ANIMATION = 0x0000120A;
        public const int ACTIVEX_CURR_MAX = 0x0000120A;
        public const int ACTIVEX_MAX = 0x000013ff;
        public const int SCRIPT_MIN = 0x00001400;
        public const int SCRIPT_RUN = 0x00001400;
        public const int SCRIPT_JAVA_USE = 0x00001402;
        public const int SCRIPT_SAFE_ACTIVEX = 0x00001405;
        public const int CROSS_DOMAIN_DATA = 0x00001406;
        public const int SCRIPT_PASTE = 0x00001407;
        public const int ALLOW_XDOMAIN_SUBFRAME_RESIZE = 0x00001408;
        public const int SCRIPT_CURR_MAX = 0x00001408;
        public const int SCRIPT_MAX = 0x000015ff;
        public const int HTML_MIN = 0x00001600;
        public const int HTML_SUBMIT_FORMS = 0x00001601;
        public const int HTML_SUBMIT_FORMS_FROM = 0x00001602;
        public const int HTML_SUBMIT_FORMS_TO = 0x00001603;
        public const int HTML_FONT_DOWNLOAD = 0x00001604;
        public const int HTML_JAVA_RUN = 0x00001605;
        public const int HTML_USERDATA_SAVE = 0x00001606;
        public const int HTML_SUBFRAME_NAVIGATE = 0x00001607;
        public const int HTML_META_REFRESH = 0x00001608;
        public const int HTML_MIXED_CONTENT = 0x00001609;
        public const int HTML_INCLUDE_FILE_PATH = 0x0000160A;
        public const int HTML_MAX = 0x000017ff;
        public const int SHELL_MIN = 0x00001800;
        public const int SHELL_INSTALL_DTITEMS = 0x00001800;
        public const int SHELL_MOVE_OR_COPY = 0x00001802;
        public const int SHELL_FILE_DOWNLOAD = 0x00001803;
        public const int SHELL_VERB = 0x00001804;
        public const int SHELL_WEBVIEW_VERB = 0x00001805;
        public const int SHELL_SHELLEXECUTE = 0x00001806;
        public const int SHELL_EXECUTE_HIGHRISK = 0x00001806;
        public const int SHELL_EXECUTE_MODRISK = 0x00001807;
        public const int SHELL_EXECUTE_LOWRISK = 0x00001808;
        public const int SHELL_POPUPMGR = 0x00001809;
        public const int SHELL_RTF_OBJECTS_LOAD = 0x0000180A;
        public const int SHELL_ENHANCED_DRAGDROP_SECURITY = 0x0000180B;
        public const int SHELL_EXTENSIONSECURITY = 0x0000180C;
        public const int SHELL_CURR_MAX = 0x0000180C;
        public const int SHELL_MAX = 0x000019ff;
        public const int NETWORK_MIN = 0x00001A00;
        public const int CREDENTIALS_USE = 0x00001A00;
        public const int AUTHENTICATE_CLIENT = 0x00001A01;
        public const int COOKIES = 0x00001A02;
        public const int COOKIES_SESSION = 0x00001A03;
        public const int CLIENT_CERT_PROMPT = 0x00001A04;
        public const int COOKIES_THIRD_PARTY = 0x00001A05;
        public const int COOKIES_SESSION_THIRD_PARTY = 0x00001A06;
        public const int COOKIES_ENABLED = 0x00001A10;
        public const int NETWORK_CURR_MAX = 0x00001A10;
        public const int NETWORK_MAX = 0x00001Bff;
        public const int JAVA_MIN = 0x00001C00;
        public const int JAVA_PERMISSIONS = 0x00001C00;
        public const int JAVA_CURR_MAX = 0x00001C00;
        public const int JAVA_MAX = 0x00001Cff;
        public const int INFODELIVERY_MIN = 0x00001D00;
        public const int INFODELIVERY_NO_ADDING_CHANNELS = 0x00001D00;
        public const int INFODELIVERY_NO_EDITING_CHANNELS = 0x00001D01;
        public const int INFODELIVERY_NO_REMOVING_CHANNELS = 0x00001D02;
        public const int INFODELIVERY_NO_ADDING_SUBSCRIPTIONS = 0x00001D03;
        public const int INFODELIVERY_NO_EDITING_SUBSCRIPTIONS = 0x00001D04;
        public const int INFODELIVERY_NO_REMOVING_SUBSCRIPTIONS = 0x00001D05;
        public const int INFODELIVERY_NO_CHANNEL_LOGGING = 0x00001D06;
        public const int INFODELIVERY_CURR_MAX = 0x00001D06;
        public const int INFODELIVERY_MAX = 0x00001Dff;
        public const int CHANNEL_SOFTDIST_MIN = 0x00001E00;
        public const int CHANNEL_SOFTDIST_PERMISSIONS = 0x00001E05;
        public const int CHANNEL_SOFTDIST_MAX = 0x00001Eff;
        public const int BEHAVIOR_MIN = 0x00002000;
        public const int BEHAVIOR_RUN = 0x00002000;
        public const int FEATURE_MIN = 0x00002100;
        public const int FEATURE_MIME_SNIFFING = 0x00002100;
        public const int FEATURE_ZONE_ELEVATION = 0x00002101;
        public const int FEATURE_WINDOW_RESTRICTIONS = 0x00002102;
        public const int FEATURE_SCRIPT_STATUS_BAR = 0x00002103;
        public const int FEATURE_FORCE_ADDR_AND_STATUS = 0x00002104;
        public const int FEATURE_BLOCK_INPUT_PROMPTS = 0x00002105;
        public const int AUTOMATIC_DOWNLOAD_UI_MIN = 0x00002200;
        public const int AUTOMATIC_DOWNLOAD_UI = 0x00002200;
        public const int AUTOMATIC_ACTIVEX_UI = 0x00002201;
        public const int ALLOW_RESTRICTEDPROTOCOLS = 0x00002300;
        public const int ALLOW_APEVALUATION = 0x00002301;
        public const int WINDOWS_BROWSER_APPLICATIONS = 0x00002400;
        public const int XPS_DOCUMENTS = 0x00002401;
        public const int LOOSE_XAML = 0x00002402;
        public const int LOWRIGHTS = 0x00002500;
        public const int WINFX_SETUP = 0x00002600;
    }
    #endregion

    #region URLPOLICY Flags
    public struct URLPOLICY
    {
        public const byte ACTIVEX_CHECK_LIST = unchecked((byte)0x00010000);
        public const byte ALLOW = 0x00;
        public const byte AUTHENTICATE_CHALLENGE_RESPONSE = unchecked((byte)0x00010000);
        public const byte AUTHENTICATE_CLEARTEXT_OK = 0x00000000;
        public const byte AUTHENTICATE_MUTUAL_ONLY = unchecked((byte)0x00030000);
        public const byte BEHAVIOR_CHECK_LIST = unchecked((byte)0x00010000);
        public const byte CHANNEL_SOFTDIST_AUTOINSTALL = unchecked((byte)0x00030000);
        public const byte CHANNEL_SOFTDIST_PRECACHE = unchecked((byte)0x00020000);
        public const byte CHANNEL_SOFTDIST_PROHIBIT = unchecked((byte)0x00010000);
        public const byte CREDENTIALS_ANONYMOUS_ONLY = unchecked((byte)0x00030000);
        public const byte CREDENTIALS_CONDITIONAL_PROMPT = unchecked((byte)0x00020000);
        public const byte CREDENTIALS_MUST_PROMPT_USER = unchecked((byte)0x00010000);
        public const byte CREDENTIALS_SILENT_LOGON_OK = 0x00000000;
        public const byte DISALLOW = 0x03;
        public const byte JAVA_CUSTOM = unchecked((byte)0x00800000);
        public const byte JAVA_HIGH = unchecked((byte)0x00010000);
        public const byte JAVA_LOW = unchecked((byte)0x00030000);
        public const byte JAVA_MEDIUM = unchecked((byte)0x00020000);
        public const byte JAVA_PROHIBIT = 0x00000000;
        public const byte MASK_PERMISSIONS = 0x0f;
        public const byte QUERY = 0x01;
    }
    #endregion

#if false
    MIDL_INTERFACE("79eac9ee-baf9-11ce-8c82-00aa004ba90b")
    IInternetSecurityManager : public IUnknown
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE SetSecuritySite(
            /* [unique][in] */ IInternetSecurityMgrSite *pSite) = 0;

        virtual HRESULT STDMETHODCALLTYPE GetSecuritySite(
            /* [out] */ IInternetSecurityMgrSite **ppSite) = 0;

        virtual HRESULT STDMETHODCALLTYPE MapUrlToZone(
            /* [in] */ LPCWSTR pwszUrl,
            /* [out] */ DWORD *pdwZone,
            /* [in] */ DWORD dwFlags) = 0;

        virtual HRESULT STDMETHODCALLTYPE GetSecurityId(
            /* [in] */ LPCWSTR pwszUrl,
            /* [size_is][out] */ BYTE *pbSecurityId,
            /* [out][in] */ DWORD *pcbSecurityId,
            /* [in] */ DWORD_PTR dwReserved) = 0;

        virtual HRESULT STDMETHODCALLTYPE ProcessUrlAction(
            /* [in] */ LPCWSTR pwszUrl,
            /* [in] */ DWORD dwAction,
            /* [size_is][out] */ BYTE *pPolicy,
            /* [in] */ DWORD cbPolicy,
            /* [in] */ BYTE *pContext,
            /* [in] */ DWORD cbContext,
            /* [in] */ DWORD dwFlags,
            /* [in] */ DWORD dwReserved) = 0;

        virtual HRESULT STDMETHODCALLTYPE QueryCustomPolicy(
            /* [in] */ LPCWSTR pwszUrl,
            /* [in] */ REFGUID guidKey,
            /* [size_is][size_is][out] */ BYTE **ppPolicy,
            /* [out] */ DWORD *pcbPolicy,
            /* [in] */ BYTE *pContext,
            /* [in] */ DWORD cbContext,
            /* [in] */ DWORD dwReserved) = 0;

        virtual HRESULT STDMETHODCALLTYPE SetZoneMapping(
            /* [in] */ DWORD dwZone,
            /* [in] */ LPCWSTR lpszPattern,
            /* [in] */ DWORD dwFlags) = 0;

        virtual HRESULT STDMETHODCALLTYPE GetZoneMappings(
            /* [in] */ DWORD dwZone,
            /* [out] */ IEnumString **ppenumString,
            /* [in] */ DWORD dwFlags) = 0;

    };
#endif
}
