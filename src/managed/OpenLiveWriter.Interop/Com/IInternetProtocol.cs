// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.Interop.Com
{

    /// <summary>
    /// This interface provides an implementation of the IUnknown interface, which allows
    /// client programs to determine if asynchronous pluggable protocols are supported.
    /// No additional methods are supported by this interface.
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("79eac9e0-baf9-11ce-8c82-00aa004ba90b")]
    public interface IInternet { }

    /// <summary>
    /// This interface is used to control the operation of an asynchronous pluggable protocol handler.
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("79EAC9E3-bAF9-11CE-8C82-00AA004BA90B")]
    public interface IInternetProtocolRoot
    {
        /// <summary>
        /// Starts the operations
        /// </summary>
        /// <param name="szUrl">[in] Address of a string value that contains the URL. For a pluggable MIME filter, this parameter contains the MIME type. </param>
        /// <param name="pOIProtSink">[in] Address of the protocol sink provided by the client.</param>
        /// <param name="pOIBindInfo">[in] Address of the IInternetBindInfo interface from which the protocol gets download-specific information.</param>
        /// <param name="grfPI">[in] Unsigned long integer value that contains the flags that determine if the method only parses or if it parses and downloads the URL. This can be one of the PI_FLAGS values.</param>
        /// <param name="dwReserved">[in] For pluggable MIME filters, contains the address of a PROTOCOLFILTERDATA structure. Otherwise, it is reserved and must be set to NULL</param>
        /// <returns>Returns one of the following values:
        ///		S_OK -- Success.
        ///		E_PENDING -- The operation started and will complete asynchronously.
        ///		INET_E_USE_DEFAULT_PROTOCOLHANDLER -- The handler cannot handle this URL, so the default handler should be used.
        ///		INET_E_xxx -- Internet-specific errors. For additional information, see the URL Moniker Error Codes definitions.
        ///	</returns>																																																										   </returns>
        [PreserveSig]
        int Start(
            [In, MarshalAs(UnmanagedType.LPWStr)] string szUrl,
            [In] IInternetProtocolSink pOIProtSink,
            [In] IInternetBindInfo pOIBindInfo,
            [In] PI_FLAGS grfPI,
            [In] IntPtr dwReserved);

        /// <summary>
        /// Allows the pluggable protocol handler to continue processing data on the apartment thread.
        /// </summary>
        /// <param name="pProtocolData">[in] Address of the PROTOCOLDATA structure data passed to IInternetProtocolSink::Switch.</param>
        /// <returns>Returns one of the following values:
        ///		S_OK -- Success.
        ///		E_PENDING -- The next state will complete asynchronously.
        ///		INET_E_xxx -- Internet-specific errors. For additional information, see the URL Moniker Error Codes definitions.
        /// </returns>																																				</returns>
        [PreserveSig]
        int Continue(
            [In, Out] ref PROTOCOLDATA pProtocolData);

        /// <summary>
        /// Cancels an operation that is in progress.
        /// </summary>
        /// <param name="hrReason">[in] HRESULT value that contains the reason for canceling the operation. This is the HRESULT that is reported by the pluggable protocol if it successfully canceled the binding. The pluggable protocol passes this HRESULT to urlmon.dll using the IInternetProtocolSink::ReportResult method. Urlmon.dll then passes this HRESULT to the host using IBindStatusCallback::OnStopBinding.</param>
        /// <param name="dwOptions">[in] Reserved. Must be set to 0.</param>
        /// <returns>Returns one of the following values:
        ///		S_OK -- Success.
        ///		E_PENDING -- The operation started and is completed asynchronously.
        ///		INET_E_xxx -- Internet-specific errors. For additional information, see the URL Moniker Error Codes definitions.
        /// </returns>
        [PreserveSig]
        int Abort(
            [In] int hrReason,
            [In] uint dwOptions);

        /// <summary>
        /// Releases the resources used by the pluggable protocol handler.
        /// </summary>
        /// <param name="dwOptions">[in] Reserved. Must be set to 0.</param>
        void Terminate(
            [In] uint dwOptions);

        /// <summary>
        /// Not currently implemented
        /// </summary>
        /// <returns>E_NOTIMPL</returns>
        [PreserveSig]
        int Suspend();

        /// <summary>
        /// Not currently implemented
        /// </summary>
        /// <returns>E_NOTIMPL</returns>
        [PreserveSig]
        int Resume();
    }

    /// <summary>
    /// This is the main interface exposed by an asynchronous pluggable protocol.
    /// This interface and the IInternetProtocolSink interface communicate with each
    /// other very closely during download operations.
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("79EAC9E4-BAF9-11CE-8C82-00AA004BA90B")]
    public interface IInternetProtocol
    {
        /// <summary>
        /// Starts the operations
        /// </summary>
        /// <param name="szUrl">[in] Address of a string value that contains the URL. For a pluggable MIME filter, this parameter contains the MIME type. </param>
        /// <param name="pOIProtSink">[in] Address of the protocol sink provided by the client.</param>
        /// <param name="pOIBindInfo">[in] Address of the IInternetBindInfo interface from which the protocol gets download-specific information.</param>
        /// <param name="grfPI">[in] Unsigned long integer value that contains the flags that determine if the method only parses or if it parses and downloads the URL. This can be one of the PI_FLAGS values.</param>
        /// <param name="dwReserved">[in] For pluggable MIME filters, contains the address of a PROTOCOLFILTERDATA structure. Otherwise, it is reserved and must be set to NULL</param>
        /// <returns>Returns one of the following values:
        ///		S_OK -- Success.
        ///		E_PENDING -- The operation started and will complete asynchronously.
        ///		INET_E_USE_DEFAULT_PROTOCOLHANDLER -- The handler cannot handle this URL, so the default handler should be used.
        ///		INET_E_xxx -- Internet-specific errors. For additional information, see the URL Moniker Error Codes definitions.
        ///	</returns>																																																										   </returns>
        [PreserveSig]
        int Start(
            [In, MarshalAs(UnmanagedType.LPWStr)] string szUrl,
            [In] IInternetProtocolSink pOIProtSink,
            [In] IInternetBindInfo pOIBindInfo,
            [In] PI_FLAGS grfPI,
            [In] IntPtr dwReserved);

        /// <summary>
        /// Allows the pluggable protocol handler to continue processing data on the apartment thread.
        /// </summary>
        /// <param name="pProtocolData">[in] Address of the PROTOCOLDATA structure data passed to IInternetProtocolSink::Switch.</param>
        /// <returns>Returns one of the following values:
        ///		S_OK -- Success.
        ///		E_PENDING -- The next state will complete asynchronously.
        ///		INET_E_xxx -- Internet-specific errors. For additional information, see the URL Moniker Error Codes definitions.
        /// </returns>																																				</returns>
        [PreserveSig]
        int Continue(
            [In, Out] ref PROTOCOLDATA pProtocolData);

        /// <summary>
        /// Cancels an operation that is in progress.
        /// </summary>
        /// <param name="hrReason">[in] HRESULT value that contains the reason for canceling the operation. This is the HRESULT that is reported by the pluggable protocol if it successfully canceled the binding. The pluggable protocol passes this HRESULT to urlmon.dll using the IInternetProtocolSink::ReportResult method. Urlmon.dll then passes this HRESULT to the host using IBindStatusCallback::OnStopBinding.</param>
        /// <param name="dwOptions">[in] Reserved. Must be set to 0.</param>
        /// <returns>Returns one of the following values:
        ///		S_OK -- Success.
        ///		E_PENDING -- The operation started and is completed asynchronously.
        ///		INET_E_xxx -- Internet-specific errors. For additional information, see the URL Moniker Error Codes definitions.
        /// </returns>
        [PreserveSig]
        int Abort(
            [In] int hrReason,
            [In] uint dwOptions);

        /// <summary>
        /// Releases the resources used by the pluggable protocol handler.
        /// </summary>
        /// <param name="dwOptions">[in] Reserved. Must be set to 0.</param>
        void Terminate(
            [In] uint dwOptions);

        /// <summary>
        /// Not currently implemented
        /// </summary>
        /// <returns>E_NOTIMPL</returns>
        [PreserveSig]
        int Suspend();

        /// <summary>
        /// Not currently implemented
        /// </summary>
        /// <returns>E_NOTIMPL</returns>
        [PreserveSig]
        int Resume();

        /// <summary>
        /// Reads data retrieved by the pluggable protocol handler.
        /// </summary>
        /// <param name="pv">[in] Address of the buffer where the information should be stored.</param>
        /// <param name="cb">[in] ULONG value that indicates the size of the buffer.</param>
        /// <param name="pcbRead">[out] Address of a ULONG value that indicates the amount of data stored in the buffer. </param>
        /// <returns>Returns one of the following values:
        ///		INET_E_DATA_NOT_AVAILABLE -- There is no more data available from the server, but more data was expected.
        ///		INET_E_DOWNLOAD_FAILURE -- The read failed.
        ///		E_PENDING -- The read operation is pending.
        ///		S_OK -- The read was successful, but there is still additional data available.
        ///		S_FALSE -- All of the data has been completely downloaded.</returns>
        [PreserveSig]
        int Read(
            [In] IntPtr pv,
            [In] uint cb,
            [Out] out uint pcbRead);

        /// <summary>
        /// Moves the current seek offset.
        /// </summary>
        /// <param name="dlibMove">[in] Large integer value that indicates how far to move the offset.</param>
        /// <param name="dwOrigin">[in] Enumerated DWORD value that indicates where the move should begin.</param>
        /// <param name="plibNewPosition">[out] Address of an unsigned long integer value that indicates the new offset.</param>
        /// <returns>Returns S_OK if successful, or E_FAIL if the protocol does not
        /// support seekable data retrieval.</returns>
        int Seek(
            [In] Int64 dlibMove,
            [In] uint dwOrigin,
            [Out] out UInt64 plibNewPosition);

        /// <summary>
        /// Locks the requested resource so that the IInternetProtocolRoot::Terminate method can be called and the remaining data can be read.
        /// </summary>
        /// <param name="dwOptions">[in] Reserved. Must be set to 0.</param>
        void LockRequest(
            [In] uint dwOptions);

        /// <summary>
        /// Frees any resources associated with a lock.
        /// </summary>
        void UnlockRequest();
    }

    /// <summary>
    /// This interface receives the reports and binding data from the asynchronous
    /// pluggable protocol. It is a free-threaded interface and can be called from
    /// any thread.
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("79EAC9E5-BAF9-11CE-8C82-00AA004BA90B")]
    public interface IInternetProtocolSink
    {
        /// <summary>
        /// Passes data from an asynchronous pluggable protocol's worker thread intended for the same asynchronous pluggable protocol's apartment thread.
        /// </summary>
        /// <param name="pProtocolData">[in] Address of a PROTOCOLDATA structure containing the data to be passed back to the apartment thread.</param>
        /// <returns>Returns S_OK if successful, or E_FAIL if called when an asynchronous operation is pending.</returns>
        [PreserveSig]
        int Switch(
            [In] ref PROTOCOLDATA pProtocolData);

        /// <summary>
        /// Reports progress made during a state operation.
        /// </summary>
        /// <param name="ulStatusCode">[in] BINDSTATUS value that indicates the status of the state operation.</param>
        /// <param name="szStatusText">[in] String value that describes the status of the state operation.</param>
        /// <returns>Returns S_OK if successful, or E_FAIL if called after IInternetProtocolRoot::Abort.</returns>
        [PreserveSig]
        int ReportProgress(
            [In] BINDSTATUS ulStatusCode,
            [In, MarshalAs(UnmanagedType.LPWStr)] string szStatusText);

        /// <summary>
        /// Reports the amount of data that is available on the thread.
        /// </summary>
        /// <param name="grfBSCF">[in] DWORD value that evaluates to a BSCF value
        /// that indicates the type of data available. BSCF_LASTDATANOTIFICATION
        /// indicates that all available data has been reported.</param>
        /// <param name="ulProgress">[in] Unsigned long integer value that indicates the progress made so far.</param>
        /// <param name="ulProgressMax">[in] Unsigned long integer value that indicates the total amount of work to be done.</param>
        /// <returns>Returns S_OK if successful, or E_FAIL if called after an
        /// InternetProtocolRoot::Abort call.</returns>
        [PreserveSig]
        int ReportData(
            [In] BSCF grfBSCF,
            [In] uint ulProgress,
            [In] uint ulProgressMax);

        /// <summary>
        /// Reports the result of the operation when called on any thread.
        /// </summary>
        /// <param name="hrResult">[in] HRESULT value that indicates the result returned by the operation.</param>
        /// <param name="dwError">[in] Unsigned long integer value that is a protocol-specific code.</param>
        /// <param name="szResult">[in] Protocol-specific result string that should be NULL if the operation succeeded.</param>
        /// <returns>Returns S_OK if successful, or E_FAIL if called in the wrong sequence</returns>
        [PreserveSig]
        int ReportResult(
            int hrResult,
            uint dwError,
            [In, MarshalAs(UnmanagedType.LPWStr)] string szStatusText);
    }

    /// <summary>
    /// This interface is implemented by the system and provides data that the protocol
    /// might need to bind successfully.
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("79EAC9E1-BAF9-11CE-8C82-00AA004BA90B")]
    public interface IInternetBindInfo
    {
        /// <summary>
        /// Gets the BINDINFO structure associated with the binding operation.
        /// </summary>
        /// <param name="grfBINDF">[out] Address of a value taken from the BINDF
        /// enumeration indicating whether the bind should proceed synchronously
        /// or asynchronously.</param>
        /// <param name="pbindinfo">[in, out] Address of the BINDINFO structure,
        /// which describes how the client wants the binding to occur.</param>
        void GetBindInfo(
            [Out] out BINDF grfBINDF,
            [In, Out] ref BINDINFO pbindinfo);

        /// <summary>
        /// Retrieves the strings needed by the protocol for its operation.
        /// This method is used if the protocol requires any additional information,
        /// such as a user name or password needed to access a URL.
        /// </summary>
        /// <param name="ulStringType">BINDSTRING indicating the type of string or strings that should be returned</param>
        /// <param name="ppwzStr">Address of an array of string pointers</param>
        /// <param name="cEl">[in] Unsigned long integer value that indicates the number of elements in the array provided. </param>
        /// <param name="pcElFetched">[in, out] Address of an unsigned long integer value that indicates the number of elements in the array that are filled.</param>
        void GetBindString(
            [In] BINDSTRING ulStringType,
            [In, Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)] string[] ppwzStr,
            [In] uint cEl,
            [In, Out] ref uint pcElFetched);
    }

    /// <summary>
    /// This interface provides information about the URL being handled by the
    /// protocol handler. The interface is optional for implementors of
    /// IInternetProtocol.
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("79EAC9EC-BAF9-11CE-8C82-00AA004BA90B")]
    public interface IInternetProtocolInfo
    {
        /// <summary>
        /// Parses a URL.
        /// </summary>
        /// <param name="pwzUrl">[in] String value that contains the URL to parse.</param>
        /// <param name="ParseAction">[in] PARSEACTION value that determines the information to be parsed from the URL. </param>
        /// <param name="dwParseFlags"[in] Reserved. Must be set to 0.></param>
        /// <param name="pwzResult">[out] String value that contains the information parsed from the URL. </param>
        /// <param name="cchResult">[in] Unsigned long integer value that contains the size of the buffer. </param>
        /// <param name="pcchResult">[out] Pointer to an unsigned long integer value that contains the size of the information stored in the buffer. </param>
        /// <param name="dwReserved">[in] Reserved. Must be set to 0.</param>
        /// <returns>Returns one of the following values.
        ///		S_OK -- Success.
        ///		S_FALSE -- The buffer was too small to contain the resulting URL.
        ///		INET_E_DEFAULT_ACTION -- Use the default action.
        /// </returns>
        [PreserveSig]
        int ParseUrl(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwzUrl,
            [In] PARSEACTION ParseAction,
            [In] uint dwParseFlags,
            [Out, MarshalAs(UnmanagedType.LPWStr)] string pwzResult,
            [In] uint cchResult,
            [Out] out uint pcchResult,
            [In] uint dwReserved);

        /// <summary>
        /// Combines a base and relative URL into a full URL.
        /// </summary>
        /// <param name="pwzBaseUrl">[in] String value containing the base URL. </param>
        /// <param name="pwzRelatuveUrl">[in] String value containing the relative URL. </param>
        /// <param name="dwCombineFlags">[in] Unsigned long integer value that controls the combining process. Can be one of the following values.
        ///	ICU_BROWSER_MODE, ICU_ENCODE_SPACES_ONLY, ICU_NO_ENCODE, ICU_NO_META</param>
        /// <param name="pwzResult">[out] String variable where the full URL will be stored. </param>
        /// <param name="cchResult">[in] Unsigned long integer value that contains the size of the buffer. </param>
        /// <param name="pcchResult">[out] Pointer to an unsigned long integer value to store the size of the information stored in the buffer. </param>
        /// <param name="dwReserved">[in] Reserved. Must be set to 0.</param>
        /// <returns>Returns S_OK if successful, or S_FALSE if the buffer is too small to contain the resulting URL.</returns>
        [PreserveSig]
        int CombineUrl(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwzBaseUrl,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwzRelatuveUrl,
            [In] uint dwCombineFlags,
            [Out, MarshalAs(UnmanagedType.LPWStr)] string pwzResult,
            [In] uint cchResult,
            [Out] out uint pcchResult,
            [In] uint dwReserved);

        /// <summary>
        /// Compares two URLs and determines if they are equal.
        /// </summary>
        /// <param name="pwzUrl1">[in] String value that contains the first URL. </param>
        /// <param name="pwzUrl2">in] String value that contains the second URL. </param>
        /// <param name="dwCompareFlags">[in] Unsigned long integer value that controls
        /// the comparison. Set this to TRUE to ignore slashes, or to FALSE otherwise.</param>
        void CompareUrl(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwzUrl1,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwzUrl2,
            [In] uint dwCompareFlags);

        /// <summary>
        /// Retrieves information related to the specified URL.
        /// </summary>
        /// <param name="pwzUrl">[in] String value that contains the URL. </param>
        /// <param name="QueryOption">[in] QUERYOPTION value that indicates what option to query. </param>
        /// <param name="dwQueryFlags">[in] Reserved. Must be set to 0.</param>
        /// <param name="pBuffer">[in, out] Pointer to the buffer to store the information. </param>
        /// <param name="cbBuffer"><[in] Unsigned long integer value that contains the size of the buffer. /param>
        /// <param name="pcbBuf">[in, out] Pointer to an unsigned long integer variable to store the size of the requested information. </param>
        /// <param name="dwReserved">[in] Reserved. Must be set to 0.</param>
        /// <returns>Returns one of the following values.
        ///		S_OK -- Success.
        ///		S_FALSE --  The buffer was too small to store the information.
        ///		INET_E_QUERYOPTION_UNKNOWN --  The option requested is unknown.
        /// </returns>
        [PreserveSig]
        int QueryInfo(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwzUrl,
            [In] QUERYOPTION QueryOption,
            [In] uint dwQueryFlags,
            [In, Out] IntPtr pBuffer,
            [In] uint cbBuffer,
            [In, Out] ref uint pcbBuf,
            [In] uint dwReserved);
    }

    /// <summary>
    /// Contains state information about the protocol that is transparent to the
    /// transaction handler.
    /// </summary>
    public struct PROTOCOLDATA
    {
        /// <summary>
        /// Unsigned long integer value that contains the flags.
        /// </summary>
        public uint grfFlags;

        /// <summary>
        /// Unsigned long integer value that contains the state of the protocol handler.
        /// </summary>
        public uint dwState;

        /// <summary>
        /// Address of the data buffer.
        /// </summary>
        public IntPtr pData;

        /// <summary>
        /// Unsigned long integer value that contains the size of the data buffer.
        /// </summary>
        public uint cbData;
    }

    /// <summary>
    /// Contains additional information on the requested binding operation. The
    /// meaning of this structure is specific to the type of asynchronous moniker.
    /// </summary>
    public struct BINDINFO
    {
        /// <summary>
        /// Size of the structure, in bytes.
        /// </summary>
        public uint cbSize;

        /// <summary>
        /// Behavior of this field is moniker-specific. For URL monikers, this
        /// string is appended to the URL when the bind operation is started.
        /// Like other OLE strings, this value is a Unicode string that the
        /// client should allocate using CoTaskMemAlloc. The URL moniker frees
        /// the memory later.
        /// </summary>
        [MarshalAs(UnmanagedType.LPWStr)]
        public string szExtraInfo;

        /// <summary>
        /// Data to be used in a PUT or POST operation specified by the
        /// dwBindVerb member.
        /// </summary>
        public STGMEDIUM stgmedData;

        /// <summary>
        /// Flag from the BINDINFOF enumeration that determines the use of URL
        /// encoding during the binding operation. This member is specific to
        /// URL monikers.
        /// </summary>
        public BINDINFOF grfBindInfoF;

        /// <summary>
        /// Value from the BINDVERB enumeration specifying an action to be
        /// performed during the bind operation.
        /// </summary>
        public BINDVERB dwBindVerb;

        /// <summary>
        /// BSTR specifying a protocol-specific custom action to be performed
        /// during the bind operation (only if dwBindVerb is set to
        /// BINDVERB_CUSTOM).
        /// </summary>
        [MarshalAs(UnmanagedType.LPWStr)]
        public string szCustomVerb;

        /// <summary>
        /// Size of the data provided in the stgmedData member.
        /// </summary>
        public uint cbstgmedData;

        /// <summary>
        /// Reserved. Must be set to 0.
        /// </summary>
        public uint dwOptions;

        /// <summary>
        /// Reserved. Must be set to 0.
        /// </summary>
        public uint dwOptionsFlags;

        /// <summary>
        /// Unsigned long integer value that contains the code page used to
        /// perform the conversion. This can be one of the following values.
        /// CP_ACP
        ///		ANSI code page.
        /// CP_MACCP
        ///		Macintosh code page.
        /// CP_OEMCP
        ///		OEM code page.
        /// CP_SYMBOL
        ///		Symbol code page (42).
        /// CP_THREAD_ACP
        ///		Current thread's ANSI code page.
        /// CP_UTF7
        ///		Translate using UTF-7.
        /// CP_UTF8
        /// 	Translate using UTF-8.
        /// </summary>
        public uint dwCodePage;

        /// <summary>
        /// SECURITY_ATTRIBUTES structure that contains the descriptor for the
        /// object being bound to and indicates whether the handle retrieved by
        /// specifying this structure is inheritable.
        /// </summary>
        public SECURITY_ATTRIBUTES securityAttributes;

        /// <summary>
        /// Interface identifier of the IUnknown interface referred to by pUnk.
        /// </summary>
        public Guid iid;

        /// <summary>
        /// Pointer to the IUnknown interface.
        /// </summary>
        [MarshalAs(UnmanagedType.IUnknown)]
        public object punk;

        /// <summary>
        /// Reserved. Must be set to 0.
        /// </summary>
        public uint dwReserved;
    }

    /// <summary>
    /// Contains values that determine the use of URL encoding during the binding
    /// operation.
    /// </summary>
    public enum BINDINFOF : uint
    {
        /// <summary>
        /// Use URL encoding to pass in the data provided in the stgmedData member
        /// of the BINDINFO structure for PUT and POST operations.
        /// </summary>
        URLENCODESTGMEDDATA = 0x00000001,

        /// <summary>
        /// Use URL encoding to pass in the data provided in the szExtraInfo
        /// member of the BINDINFO structure.
        /// </summary>
        URLENCODEDEXTRAINFO = 0x00000002
    };

    /// <summary>
    /// Contains values that specify an action, such as an HTTP request, to be
    /// performed during the binding operation.
    /// </summary>
    public enum BINDVERB : uint
    {
        /// <summary>
        /// Perform an HTTP GET operation, the default operation. The stgmedData
        /// member of the BINDINFO structure should be set to TYMED_NULL for the
        /// GET operation.
        /// </summary>
        GET = 0x00000000,

        /// <summary>
        /// Perform an HTTP POST operation. The data to be posted should be
        /// specified in the stgmedData member of the BINDINFO structure.
        /// </summary>
        POST = 0x00000001,

        /// <summary>
        /// Perform an HTTP PUT operation. The data to put should be specified
        /// in the stgmedData member of the BINDINFO structure.
        /// </summary>
        PUT = 0x00000002,

        /// <summary>
        /// Perform a custom operation that is protocol-specific. See the
        /// szCustomVerb member of the BINDINFO structure. The data to be used
        /// in the custom operation should be specified in the stgmedData
        /// structure.
        /// </summary>
        CUSTOM = 0x00000003
    }

    /// <summary>
    /// Contains the values for the bind string types available for
    /// IInternetBindInfo::GetBindString.
    /// </summary>
    public enum BINDSTRING : uint
    {
        /// <summary>
        /// Retrieve the headers associated with the request.
        /// </summary>
        HEADERS = 1,

        /// <summary>
        /// Retrieve the accepted MIME types.
        /// </summary>
        ACCEPT_MIMES,

        /// <summary>
        /// Not currently supported.
        /// </summary>
        EXTRA_URL,

        /// <summary>
        /// Retrieve the language types accepted.
        /// </summary>
        LANGUAGE,

        /// <summary>
        /// Retrieve the user name sent with the request.
        /// </summary>
        USERNAME,

        /// <summary>
        /// Retrieve the password sent with the request.
        /// </summary>
        PASSWORD,

        /// <summary>
        /// Not currently supported.
        /// </summary>
        UA_PIXELS,

        /// <summary>
        /// Not currently supported.
        /// </summary>
        UA_COLOR,

        /// <summary>
        /// Retrieve the server's operating system.
        /// </summary>
        OS,

        /// <summary>
        /// Retrieve the user agent string used.
        /// </summary>
        USER_AGENT,

        /// <summary>
        /// Retrieve the encoding types accepted.
        /// </summary>
        ACCEPT_ENCODINGS,

        /// <summary>
        /// Retrieve the posted cookie.
        /// </summary>
        POST_COOKIE,

        /// <summary>
        /// Retrieve the MIME type of the posted data.
        /// </summary>
        POST_DATA_MIME,

        /// <summary>
        /// Retrieve the URL.
        /// </summary>
        URL,

        /// <summary>
        /// Retrieve the CLSID of the resource. This value was added for Microsoft
        /// Internet Explorer 5.
        /// </summary>
        IID,

        /// <summary>
        /// Retrieve a string that indicates if the protocol handler is binding to
        /// an object. This value was added for Internet Explorer 5.
        /// </summary>
        FLAG_BIND_TO_OBJECT,

        /// <summary>
        /// Retrieve the address of the IBindCtx interface. This value was added
        /// for Internet Explorer 5.
        /// </summary>
        PTR_BIND_CONTEXT
    }

    /// <summary>
    /// Contains the flags that control the asynchronous pluggable protocol handler.
    /// </summary>
    [Flags]
    public enum PI_FLAGS : uint
    {
        /// <summary>
        /// Asynchronous pluggable protocol should parse the URL and return S_OK if
        /// the URL is syntactically correct; otherwise S_FALSE.
        /// </summary>
        PI_PARSE_URL = 0x00000001,

        /// <summary>
        /// Asynchronous pluggable protocol handler should be running in filter mode
        /// and data will come in through the IInternetProtocolSink interface it
        /// exposes. The QueryInterface method will be called on the protocol handler
        /// for its IInternetProtocolSink interface.
        /// </summary>
        PI_FILTER_MODE = 0x00000002,

        /// <summary>
        /// Asynchronous pluggable protocol handler should do as little work as possible
        /// on the apartment (or user interface) thread and continue on a worker thread
        /// as soon as possible.
        /// </summary>
        PI_FORCE_ASYNC = 0x00000004,

        /// <summary>
        /// Asynchronous pluggable protocol handler should use worker threads and not
        /// use the apartment thread.
        /// </summary>
        PI_USE_WORKERTHREAD = 0x00000008,

        /// <summary>
        /// Asynchronous pluggable protocol handler should verify and report the
        /// MIME type.
        /// </summary>
        PI_MIMEVERIFICATION = 0x00000010,

        /// <summary>
        /// Asynchronous pluggable protocol handler should find the CLSID associated
        /// with the MIME type.
        /// </summary>
        PI_CLSIDLOOKUP = 0x00000020,

        /// <summary>
        /// Asynchronous pluggable protocol handler should report its progress.
        /// </summary>
        PI_DATAPROGRESS = 0x00000040,

        /// <summary>
        /// Asynchronous pluggable protocol handler should work synchronously.
        /// </summary>
        PI_SYNCHRONOUS = 0x00000080,

        /// <summary>
        /// Asynchronous pluggable protocol handler should use the apartment
        /// (or user interface) thread only.
        /// </summary>
        PI_APARTMENTTHREADED = 0x00000100,

        /// <summary>
        /// Asynchronous pluggable protocol handler should install the class if the
        /// class is not found.
        /// </summary>
        PI_CLASSINSTALL = 0x00000200,

        /// <summary>
        /// Asynchronous pluggable protocol handler should pass address of the
        /// IBindCtx interface to the pUnk member of the PROTOCOLFILTERDATA
        /// structure.
        /// </summary>
        PI_PASSONBINDCTX = 0x00002000,

        /// <summary>
        /// Asynchronous pluggable protocol handler should disable MIME filters.
        /// </summary>
        PI_NOMIMEHANDLER = 0x00008000,

        /// <summary>
        /// Asynchronous pluggable protocol handler should load the application directly.
        /// </summary>
        PI_LOADAPPDIRECT = 0x00004000,

        /// <summary>
        /// Asynchronous pluggable protocol handler should switch to the apartment thread,
        /// even if it does not need to.
        /// </summary>
        PD_FORCE_SWITCH = 0x00010000,

        /// <summary>
        /// Asynchronous pluggable protocol handler should choose the default handler over
        /// a custom handler.
        /// </summary>
        PI_PREFERDEFAULTHANDLER = 0x00020000
    }

    /// <summary>
    /// Contains values that are passed to the client application's implementation of the
    /// ReportProgress method to indicate the progress of the bind operation.
    /// </summary>
    public enum BINDSTATUS : uint
    {
        /// <summary>
        /// Notifies the client application that the bind operation is finding the
        /// resource that holds the object or storage being bound to. The szStatusText
        /// parameter to the IBindStatusCallback::OnProgress method provides the
        /// display name of the resource being searched for (for example, "www.foo.com").
        /// </summary>
        FINDINGRESOURCE = 1,

        /// <summary>
        /// Notifies the client application that the bind operation is connecting to
        /// the resource that holds the object or storage being bound to. The
        /// szStatusText parameter to the IBindStatusCallback::OnProgress method
        /// provides the display name of the resource being connected to (for example,
        /// an IP address).
        /// </summary>
        CONNECTING,

        /// <summary>
        /// Notifies the client application that the bind operation has been
        /// redirected to a different data location. The szStatusText parameter
        /// to the IBindStatusCallback::OnProgress method provides the display
        /// name of the new data location.
        /// </summary>
        REDIRECTING,

        /// <summary>
        /// Notifies the client application that the bind operation has begun
        /// receiving the object or storage being bound to. The szStatusText
        /// parameter to the IBindStatusCallback::OnProgress method provides the
        /// display name of the data location.
        /// </summary>
        BEGINDOWNLOADDATA,

        /// <summary>
        /// Notifies the client application that the bind operation has begun
        /// receiving the object or storage being bound to. The szStatusText
        /// parameter to the IBindStatusCallback::OnProgress method provides
        /// the display name of the data location.
        /// </summary>
        DOWNLOADINGDATA,

        /// <summary>
        /// Notifies the client application that the bind operation has finished
        /// receiving the object or storage being bound to. The szStatusText
        /// parameter to the IBindStatusCallback::OnProgress method provides
        /// the display name of the data location.
        /// </summary>
        ENDDOWNLOADDATA,

        /// <summary>
        /// Notifies the client application that the bind operation is beginning
        /// to download the component.
        /// </summary>
        BEGINDOWNLOADCOMPONENTS,

        /// <summary>
        /// Notifies the client application that the bind operation is installing
        /// the component.
        /// </summary>
        INSTALLINGCOMPONENTS,

        /// <summary>
        /// Notifies the client application that the bind operation has finished
        /// downloading the component.
        /// </summary>
        ENDDOWNLOADCOMPONENTS,

        /// <summary>
        /// Notifies the client application that the bind operation is retrieving
        /// the requested object or storage from a cached copy. The szStatusText
        /// parameter to the IBindStatusCallback::OnProgress method is NULL.
        /// </summary>
        USINGCACHEDCOPY,

        /// <summary>
        /// Notifies the client application that the bind operation is requesting
        /// the object or storage being bound to. The szStatusText parameter to the
        /// IBindStatusCallback::OnProgress method provides the display name of the
        /// object (for example, a file name).
        /// </summary>
        SENDINGREQUEST,

        /// <summary>
        /// Notifies the client application that the CLSID of the resource is available.
        /// </summary>
        CLASSIDAVAILABLE,

        /// <summary>
        /// Notifies the client application that the MIME type of the resource is
        /// available.
        /// </summary>
        MIMETYPEAVAILABLE,

        /// <summary>
        /// Notifies the client application that the temporary or cache file name
        /// of the resource is available. The temporary file name might be returned
        /// if BINDF_NOWRITECACHE is called. The temporary file will be deleted once
        /// the storage is released.
        /// </summary>
        CACHEFILENAMEAVAILABLE,

        /// <summary>
        /// Notifies the client application that a synchronous operation has started.
        /// </summary>
        BEGINSYNCOPERATION,

        /// <summary>
        /// Notifies the client application that the synchronous operation has completed.
        /// </summary>
        ENDSYNCOPERATION,

        /// <summary>
        /// Notifies the client application that the file upload has started.
        /// </summary>
        BEGINUPLOADDATA,

        /// <summary>
        /// Notifies the client application that the file upload is in progress.
        /// </summary>
        UPLOADINGDATA,

        /// <summary>
        /// Notifies the client application that the file upload has completed.
        /// </summary>
        ENDUPLOADDATA,

        /// <summary>
        /// Notifies the client application that the CLSID of the protocol
        /// handler being used is available.
        /// </summary>
        PROTOCOLCLASSID,

        /// <summary>
        /// Notifies the client application that the Urlmon.dll is encoding data.
        /// </summary>
        ENCODING,

        /// <summary>
        /// Notifies the client application that the verified MIME type is available.
        /// </summary>
        VERIFIEDMIMETYPEAVAILABLE,

        /// <summary>
        /// Notifies the client application that the class install location is available.
        /// </summary>
        CLASSINSTALLLOCATION,

        /// <summary>
        /// Notifies the client application that the bind operation is decoding data.
        /// </summary>
        DECODING,

        /// <summary>
        /// Notifies the client application that a pluggable MIME handler is being loaded.
        /// This value was added for Microsoft® Internet Explorer 5.
        /// </summary>
        LOADINGMIMEHANDLER,

        /// <summary>
        /// Notifies the client application that this resource contained a
        /// Content-Disposition header that indicates that this resource is an
        /// attachment. The content of this resource should not be automatically
        /// displayed. Client applications should request permission from the user.
        /// This value was added for Internet Explorer 5.
        /// </summary>
        CONTENTDISPOSITIONATTACH,

        /// <summary>
        /// Notifies the client application of the new MIME type of the resource.
        /// This is used by a pluggable MIME filter to report a change in the MIME
        /// type after it has processed the resource. This value was added for
        /// Internet Explorer 5.
        /// </summary>
        FILTERREPORTMIMETYPE,

        /// <summary>
        /// Notifies the Urlmon.dll that this CLSID is for the class the Urlmon.dll
        /// should return to the client on a call to IMoniker::BindToObject. This
        /// value was added for Internet Explorer 5.
        /// </summary>
        CLSIDCANINSTANTIATE,

        /// <summary>
        /// Reports that the IUnknown interface has been released. This value was
        /// added for Internet Explorer 5.
        /// </summary>
        IUNKNOWNAVAILABLE,

        /// <summary>
        /// Reports whether or not the client application is connected directly to
        /// the pluggable protocol handler. This value was added for Internet
        /// Explorer 5.
        /// </summary>
        DIRECTBIND,

        /// <summary>
        /// Reports the MIME type of the resource, before any code sniffing is done.
        /// This value was added for Internet Explorer 5.
        /// </summary>
        RAWMIMETYPE,

        /// <summary>
        /// Reports that a proxy server has been detected. This value was added
        /// for Internet Explorer 5.
        /// </summary>
        PROXYDETECTING,

        /// <summary>
        /// Reports the valid types of range requests for a resource. This value
        /// was added for Internet Explorer 5.
        /// </summary>
        ACCEPTRANGES,

        /// <summary>
        ///
        /// </summary>
        COOKIE_SENT,

        /// <summary>
        ///
        /// </summary>
        COMPACT_POLICY_RECEIVED,

        /// <summary>
        ///
        /// </summary>
        COOKIE_SUPPRESSED,

        /// <summary>
        ///
        /// </summary>
        COOKIE_STATE_UNKNOWN,

        /// <summary>
        ///
        /// </summary>
        COOKIE_STATE_ACCEPT,

        /// <summary>
        ///
        /// </summary>
        COOKIE_STATE_REJECT,

        /// <summary>
        ///
        /// </summary>
        COOKIE_STATE_PROMPT,

        /// <summary>
        ///
        /// </summary>
        COOKIE_STATE_LEASH,

        /// <summary>
        ///
        /// </summary>
        COOKIE_STATE_DOWNGRADE,

        /// <summary>
        ///
        /// </summary>
        POLICY_HREF,

        /// <summary>
        ///
        /// </summary>
        P3P_HEADER,

        /// <summary>
        ///
        /// </summary>
        SESSION_COOKIE_RECEIVED,

        /// <summary>
        ///
        /// </summary>
        PERSISTENT_COOKIE_RECEIVED,

        /// <summary>
        ///
        /// </summary>
        SESSION_COOKIES_ALLOWED
    }

    /// <summary>
    /// Contains the values that determine how a resource should be bound to a moniker.
    /// </summary>
    [Flags]
    public enum BINDF : int
    {
        /// <summary>
        /// Value that indicates that the moniker should return immediately from
        /// IMoniker::BindToStorage or IMoniker::BindToObject. The actual result
        /// of the bind to an object or the bind to storage arrives asynchronously.
        /// The client is notified through calls to its IBindStatusCallback -
        /// OnDataAvailable or IBindStatusCallback - OnObjectAvailable method.
        /// If the client does not specify this flag, the bind operation will be
        /// synchronous, and the client will not receive any data from the bind
        /// operation until the IMoniker::BindToStorage or IMoniker::BindToObject
        /// call returns.
        /// </summary>
        ASYNCHRONOUS = 0x00000001,

        /// <summary>
        /// Value that indicates the client application calling the IMoniker -
        /// BindToStorage method prefers that the storage and stream objects
        /// returned in IBindStatusCallback::OnDataAvailable return E_PENDING
        /// when they reference data not yet available through their read methods,
        /// rather than blocking until the data becomes available. This flag
        /// applies only to BINDF_ASYNCHRONOUS operations. Note that asynchronous
        /// stream objects return E_PENDING while data is still downloading and
        /// return S_FALSE for the end of the file.
        /// </summary>
        ASYNCSTORAGE = 0x00000002,

        /// <summary>
        /// Value that indicates that progressive rendering should not be allowed.
        /// </summary>
        NOPROGRESSIVERENDERING = 0x00000004,

        /// <summary>
        /// Value that indicates that the moniker should be bound to the cached
        /// version of the resource.
        /// </summary>
        OFFLINEOPERATION = 0x00000008,

        /// <summary>
        /// Value that indicates the bind operation should retrieve the newest version
        /// of the data/object possible. For URL monikers, this flag maps to the
        /// Microsoft® Win32® Internet (WinInet) flag, INTERNET_FLAG_RELOAD, which
        /// forces a download of the requested resource.
        /// </summary>
        GETNEWESTVERSION = 0x00000010,

        /// <summary>
        /// Value that indicates the bind operation should not store retrieved data
        /// in the disk cache. BINDF_PULLDATA must also be specified to turn off the
        /// cache file generation when using the IMoniker::BindToStorage method.
        /// </summary>
        NOWRITECACHE = 0x00000020,

        /// <summary>
        /// Value that indicates the downloaded resource must be saved in the
        /// cache or a local file.
        /// </summary>
        NEEDFILE = 0x00000040,

        /// <summary>
        /// Value that indicates the asynchronous moniker allows the client of
        /// IMoniker::BindToStorage to drive the bind operation by pulling the
        /// data, rather than having the moniker drive the operation by pushing
        /// the data to the client. When this flag is specified, new data is
        /// only read/downloaded after the client finishes downloading all data
        /// that is currently available. This means data is only downloaded for
        /// the client after the client does an IStream::Read operation that
        /// blocks or returns E_PENDING. When the client specifies this flag, it
        /// must be sure to read all the data it can, even data that is not
        /// necessarily available yet. When this flag is not specified, the
        /// moniker continues downloading data and calls the client with
        /// IBindStatusCallback::OnDataAvailable whenever new data is available.
        ///  This flag applies only to BINDF_ASYNCHRONOUS bind operations.
        /// </summary>
        PULLDATA = 0x00000080,

        /// <summary>
        /// Value that indicates that security problems related to bad certificates
        /// and redirects between HTTP and Secure Hypertext Transfer Protocol (HTTPS)
        /// servers should be ignored
        /// </summary>
        IGNORESECURITYPROBLEM = 0x00000100,

        /// <summary>
        /// Value that indicates the resource should be resynchronized. For URL monikers,
        /// this flag maps to the WinInet flag, INTERNET_FLAG_RESYNCHRONIZE, which
        /// reloads an HTTP resource if the resource has been modified since the last
        /// time it was downloaded. All File Transfer Protocol (FTP) and Gopher
        /// resources are reloaded.
        /// </summary>
        RESYNCHRONIZE = 0x00000200,

        /// <summary>
        /// Value that indicates hyperlinks are allowed.
        /// </summary>
        HYPERLINK = 0x00000400,

        /// <summary>
        /// Value that indicates that the bind operation should not display any user
        /// interfaces.
        /// </summary>
        NO_UI = 0x00000800,

        /// <summary>
        /// Value that indicates the bind operation should be completed silently.
        /// No user interface or user notification should occur.
        /// </summary>
        SILENTOPERATION = 0x00001000,

        /// <summary>
        /// Value that indicates that the resource should not be stored in the
        /// Internet cache.
        /// </summary>
        PRAGMA_NO_CACHE = 0x00002000,

        /// <summary>
        /// Value that indicates that the class object should be retrieved.
        /// Normally the class instance is retrieved.
        /// </summary>
        GETCLASSOBJECT = 0x00004000,

        /// <summary>
        /// Reserved.
        /// </summary>
        RESERVED_1 = 0x00008000,

        /// <summary>
        /// Reserved.
        /// </summary>
        FREE_THREADED = 0x00010000,

        /// <summary>
        /// Value that indicates that the client application does not need to know
        /// the exact size of the data available, so the information is read
        /// directly from the source.
        /// </summary>
        DIRECT_READ = 0x00020000,

        /// <summary>
        /// Value that indicates that this transaction should be handled as a
        /// forms submittal.
        /// </summary>
        FORMS_SUBMIT = 0x00040000,

        /// <summary>
        /// Value that indicates the resource should be retrieved from the
        /// cache if the attempt to download the resource from the network fails.
        /// </summary>
        GETFROMCACHE_IF_NET_FAIL = 0x00080000,

        /// <summary>
        /// Value that indicates the binding is from a URL moniker. This value
        /// was added for Internet Explorer 5.
        /// </summary>
        FROMURLMON = 0x00100000,

        /// <summary>
        /// Value that indicates that the moniker should bind to the copy of the
        /// resource that is currently in the Internet cache. If the requested
        /// item is not found in the Internet cache, the system will attempt to
        /// locate the resource on the network. This value maps to the Win32
        /// Internet application programming interface (API) flag,
        /// INTERNET_FLAG_USE_CACHED_COPY.
        /// </summary>
        FWD_BACK = 0x00200000,

        /// <summary>
        /// Urlmon.dll searches for temporary or permanent namespace handlers
        /// before it uses the default registered handler for particular protocols.
        /// This value changes this behavior by allowing the moniker client to
        /// specify that Urlmon.dll should look for and use the default system
        /// protocol first.
        /// </summary>
        PREFERDEFAULTHANDLER = 0x00400000,

        /// <summary>
        /// Reserved/unknown/undocumented flag
        /// </summary>
        ENFORCERESTRICTED = 0x00800000
    }

    /// <summary>
    /// Values from the BSCF enumeration are passed to ReportData to indicate the type
    /// of data that is available.
    /// </summary>
    [Flags]
    public enum BSCF : uint
    {
        /// <summary>
        /// Identify the first call to ReportData for a given bind operation.
        /// </summary>
        FIRSTDATANOTIFICATION = 0x00000001,

        /// <summary>
        /// Identify an intermediate call to ReportData for a bind operation.
        /// </summary>
        INTERMEDIATEDATANOTIFICATION = 0x00000002,

        /// <summary>
        /// Identify the last call to ReportData for a bind operation.
        /// </summary>
        LASTDATANOTIFICATION = 0x00000004,

        /// <summary>
        /// All of the requested data is available.
        /// </summary>
        DATAFULLYAVAILABLE = 0x00000008,

        /// <summary>
        /// Size of the data available is unknown.
        /// </summary>
        AVAILABLEDATASIZEUNKNOWN = 0x00000010
    }

    /// <summary>
    /// Contains the different options for URL parsing operations
    /// </summary>
    public enum PARSEACTION
    {
        /// <summary>
        /// Canonicalize the URL.
        /// </summary>
        PARSE_CANONICALIZE = 1,

        /// <summary>
        /// Retrieve the user-friendly name for the URL.
        /// </summary>
        PARSE_FRIENDLY,

        /// <summary>
        /// Retrieve the URL that should be used by the security manager
        /// to make security decisions. The returned URL should either
        /// return just the namespace of the protocol or map the protocol
        /// to a known protocol (such as HTTP).
        /// </summary>
        PARSE_SECURITY_URL,

        /// <summary>
        /// Return the URL of the root document for this site.
        /// </summary>
        PARSE_ROOTDOCUMENT,

        /// <summary>
        /// Remove the anchor part of the URL
        /// </summary>
        PARSE_DOCUMENT,

        /// <summary>
        /// Remove everything from the URL before the anchor (#).
        /// </summary>
        PARSE_ANCHOR,

        /// <summary>
        /// Encode the URL
        /// </summary>
        PARSE_ENCODE,

        /// <summary>
        /// Decode the URL
        /// </summary>
        PARSE_DECODE,

        /// <summary>
        /// Get the path from the URL, if available.
        /// </summary>
        PARSE_PATH_FROM_URL,

        /// <summary>
        /// Create a URL from the given path
        /// </summary>
        PARSE_URL_FROM_PATH,

        /// <summary>
        /// Return the MIME type of this URL
        /// </summary>
        PARSE_MIME,

        /// <summary>
        /// Return the server name
        /// </summary>
        PARSE_SERVER,

        /// <summary>
        /// Retrieve the schema for this URL
        /// </summary>
        PARSE_SCHEMA,

        /// <summary>
        /// Retrieve the site associated with this URL
        /// </summary>
        PARSE_SITE,

        /// <summary>
        /// Retrieve the domain associated with this URL
        /// </summary>
        PARSE_DOMAIN,

        /// <summary>
        /// Retrieve the location associated with this URL
        /// </summary>
        PARSE_LOCATION,

        /// <summary>
        /// Retrieve the security form of the URL. The returned URL should return a
        /// base URL that contains no user name, password, directory path, resource,
        /// or any other extra information
        /// </summary>
        PARSE_SECURITY_DOMAIN,

        /// <summary>
        /// Convert unsafe characters to escape sequences
        /// </summary>
        PARSE_ESCAPE,

        /// <summary>
        /// Convert escape sequences to the characters they represent
        /// </summary>
        PARSE_UNESCAPE
    }

    /// <summary>
    /// Contains the available query options.
    /// </summary>
    public enum QUERYOPTION
    {
        /// <summary>
        /// Request the expiration date in a SYSTEMTIME format
        /// </summary>
        QUERY_EXPIRATION_DATE = 1,

        /// <summary>
        /// Request the last changed date in a SYSTEMTIME format
        /// </summary>
        QUERY_TIME_OF_LAST_CHANGE,

        /// <summary>
        /// Request the content encoding schema
        /// </summary>
        QUERY_CONTENT_ENCODING,

        /// <summary>
        /// Request the content type header
        /// </summary>
        QUERY_CONTENT_TYPE,

        /// <summary>
        /// Request a refresh
        /// </summary>
        QUERY_REFRESH,

        /// <summary>
        /// Combine the page URL with the nearest base URL if TRUE.
        /// </summary>
        QUERY_RECOMBINE,

        /// <summary>
        /// Check if the protocol can navigate
        /// </summary>
        QUERY_CAN_NAVIGATE,

        /// <summary>
        /// Check if the URL needs to access the network
        /// </summary>
        QUERY_USES_NETWORK,

        /// <summary>
        ///  Check if the resource is cached locally
        /// </summary>
        QUERY_IS_CACHED,

        /// <summary>
        /// Check if this resource is installed locally on a CD-ROM
        /// </summary>
        QUERY_IS_INSTALLEDENTRY,

        /// <summary>
        /// Check if this resource is stored in the cache or if it is
        /// on a mapped drive (in a cache container).
        /// </summary>
        QUERY_IS_CACHED_OR_MAPPED,

        /// <summary>
        /// Check if the specified protocol uses the Internet cache
        /// </summary>
        QUERY_USES_CACHE,

        /// <summary>
        /// Check if the protocol is encrypted
        /// </summary>
        QUERY_IS_SECURE,

        /// <summary>
        /// Check if the protocol only serves trusted content
        /// </summary>
        QUERY_IS_SAFE,
    }

}

