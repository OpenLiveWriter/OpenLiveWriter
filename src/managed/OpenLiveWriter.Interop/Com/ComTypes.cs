// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.Interop.Com
{
    /// <summary>
    /// Common COM HRESULT codes
    /// </summary>
    public struct HRESULT
    {
        /// <summary>
        /// S_OK return code
        /// </summary>
        public const Int32 S_OK = 0;

        /// <summary>
        /// S_FALSE return code
        /// </summary>
        public const Int32 S_FALSE = 1;

        /// <summary>
        /// E_NOINTERFACE return code
        /// </summary>
        public const int E_NOINTERFACE = unchecked((int)0x80000004);

        /// <summary>
        /// E_NOTIMPL return code
        /// </summary>
        public const int E_NOTIMPL = unchecked((int)0x80004001);

        /// <summary>
        /// E_FAILED return code
        /// </summary>
        public const int E_FAILED = unchecked((int)0x80004005);

        /// <summary>
        /// E_ABORT return code
        /// </summary>
        public const int E_ABORT = unchecked((int)0x80004004);

        /// <summary>
        /// E_NOTOOLSPACE return code
        /// </summary>
        public const int E_NOTOOLSPACE = unchecked((int)0x800401A1);

        /// <summary>
        /// E_INVALIDARG return code
        /// </summary>
        public const int E_INVALIDARG = unchecked((int)0x80070057L);

        /// <summary>
        /// E_ACCESSDENIED return code
        /// </summary>
        public const int E_ACCESSDENIED = unchecked((int)0x80070005);

        /// <summary>
        /// E_POINTER return code
        /// </summary>
        public const int E_POINTER = unchecked((int)0x80004003);
    }

    /// <summary>
    /// Common OLE error codes
    /// </summary>
    public struct OLE_E
    {
        /// <summary>
        /// This implementation doesn't take advises
        /// </summary>
        public const int ADVISENOTSUPPORTED = unchecked((int)0x80040003L);
    }

    public struct CTL_E
    {
        public const int ILLEGALFUNCTIONCALL = unchecked((int)0x800A0005L); // STD_CTL_SCODE(5)
        public const int OVERFLOW = unchecked((int)0x800A0006L); // STD_CTL_SCODE(6)
        public const int OUTOFMEMORY = unchecked((int)0x800A0007L); // STD_CTL_SCODE(7)
        public const int DIVISIONBYZERO = unchecked((int)0x800A000BL); // STD_CTL_SCODE(11)
        public const int OUTOFSTRINGSPACE = unchecked((int)0x800A000EL); // STD_CTL_SCODE(15)
        public const int OUTOFSTACKSPACE = unchecked((int)0x800A001CL); // STD_CTL_SCODE(28)
        public const int BADFILENAMEORNUMBER = unchecked((int)0x800A0034L); // STD_CTL_SCODE(52)
        public const int FILENOTFOUND = unchecked((int)0x800A0035L); // STD_CTL_SCODE(53)
        public const int BADFILEMODE = unchecked((int)0x800A0036L); // STD_CTL_SCODE(54)
        public const int FILEALREADYOPEN = unchecked((int)0x800A0037L); // STD_CTL_SCODE(55)
        public const int DEVICEIOERROR = unchecked((int)0x800A0039L); // STD_CTL_SCODE(57)
        public const int FILEALREADYEXISTS = unchecked((int)0x800A003AL); // STD_CTL_SCODE(58)
        public const int BADRECORDLENGTH = unchecked((int)0x800A003BL); // STD_CTL_SCODE(59)
        public const int DISKFULL = unchecked((int)0x800A003DL); // STD_CTL_SCODE(61)
        public const int BADRECORDNUMBER = unchecked((int)0x800A003FL); // STD_CTL_SCODE(63)
        public const int BADFILENAME = unchecked((int)0x800A0040L); // STD_CTL_SCODE(64)
        public const int TOOMANYFILES = unchecked((int)0x800A0043L); // STD_CTL_SCODE(67)
        public const int DEVICEUNAVAILABLE = unchecked((int)0x800A0044L); // STD_CTL_SCODE(68)
        public const int PERMISSIONDENIED = unchecked((int)0x800A0046L); // STD_CTL_SCODE(70)
        public const int DISKNOTREADY = unchecked((int)0x800A0047L); // STD_CTL_SCODE(71)
        public const int PATHFILEACCESSERROR = unchecked((int)0x800A004BL); // STD_CTL_SCODE(75)
        public const int PATHNOTFOUND = unchecked((int)0x800A004CL); // STD_CTL_SCODE(76)
        public const int INVALIDPATTERNSTRING = unchecked((int)0x800A005DL); // STD_CTL_SCODE(93)
        public const int INVALIDUSEOFNULL = unchecked((int)0x800A005EL); // STD_CTL_SCODE(94)
        public const int INVALIDFILEFORMAT = unchecked((int)0x800A0141L); // STD_CTL_SCODE(321)
        public const int INVALIDPROPERTYVALUE = unchecked((int)0x800A017CL); // STD_CTL_SCODE(380)
        public const int INVALIDPROPERTYARRAYINDEX = unchecked((int)0x800A017DL); // STD_CTL_SCODE(381)
        public const int SETNOTSUPPORTEDATRUNTIME = unchecked((int)0x800A017EL); // STD_CTL_SCODE(382)
        public const int SETNOTSUPPORTED = unchecked((int)0x800A017FL); // STD_CTL_SCODE(383)
        public const int NEEDPROPERTYARRAYINDEX = unchecked((int)0x800A0181L); // STD_CTL_SCODE(385)
        public const int SETNOTPERMITTED = unchecked((int)0x800A0183L); // STD_CTL_SCODE(387)
        public const int GETNOTSUPPORTEDATRUNTIME = unchecked((int)0x800A0189L); // STD_CTL_SCODE(393)
        public const int GETNOTSUPPORTED = unchecked((int)0x800A018AL); // STD_CTL_SCODE(394)
        public const int PROPERTYNOTFOUND = unchecked((int)0x800A01A6L); // STD_CTL_SCODE(422)
        public const int INVALIDCLIPBOARDFORMAT = unchecked((int)0x800A01CCL); // STD_CTL_SCODE(460)
        public const int INVALIDPICTURE = unchecked((int)0x800A01E1L); // STD_CTL_SCODE(481)
        public const int PRINTERERROR = unchecked((int)0x800A01E2L); // STD_CTL_SCODE(482)
        public const int CANTSAVEFILETOTEMP = unchecked((int)0x800A02DFL); // STD_CTL_SCODE(735)
        public const int SEARCHTEXTNOTFOUND = unchecked((int)0x800A02E8L); // STD_CTL_SCODE(744)
        public const int REPLACEMENTSTOOLONG = unchecked((int)0x800A02EAL); // STD_CTL_SCODE(746)
    }

    // COMExceptions thrown by IE calls may have error codes like these
    public struct IE_CTL_E
    {
        // Based on http://sharepoint/sites/IE/Teams/Dev/Shared%20Documents/Documentation/diagrams/doxygen_03_24_2007/html/cdutil_8hxx-source.html#l00392
        public const int METHODNOTAPPLICABLE = unchecked((int)0x800A01BCL); // STD_CTL_SCODE(444)
        public const int CANTMOVEFOCUSTOCTRL = unchecked((int)0x800A083EL); // STD_CTL_SCODE(2110)
        public const int CONTROLNEEDSFOCUS = unchecked((int)0x800A0889L); // STD_CTL_SCODE(2185)
        public const int INVALIDPICTURETYPE = unchecked((int)0x800A01E5L); // STD_CTL_SCODE(485)
        public const int INVALIDPASTETARGET = unchecked((int)0x800A0258L); // CUSTOM_CTL_SCODE( CUSTOM_FIRST + 0 )
        public const int INVALIDPASTESOURCE = unchecked((int)0x800A0259L); // CUSTOM_CTL_SCODE( CUSTOM_FIRST + 1 )
        public const int MISMATCHEDTAG = unchecked((int)0x800A025AL); // CUSTOM_CTL_SCODE( CUSTOM_FIRST + 2 )
        public const int INCOMPATIBLEPOINTERS = unchecked((int)0x800A025BL); // CUSTOM_CTL_SCODE( CUSTOM_FIRST + 3 )
        public const int UNPOSITIONEDPOINTER = unchecked((int)0x800A025CL); // CUSTOM_CTL_SCODE( CUSTOM_FIRST + 4 )
        public const int UNPOSITIONEDELEMENT = unchecked((int)0x800A025DL); // CUSTOM_CTL_SCODE( CUSTOM_FIRST + 5 )
        public const int INVALIDLINE = unchecked((int)0x800A025EL); // CUSTOM_CTL_SCODE( CUSTOM_FIRST + 6 )
    }

    /// <summary>
    /// Commonly used interface IDs
    /// </summary>
    public struct IID
    {
        /// <summary>
        /// IID of IUnknown
        /// </summary>
        public static readonly Guid IUnknown =
            new Guid("{00000000-0000-0000-C000-000000000046}");

        /// <summary>
        /// SID (IID) of Internet Explorer
        /// </summary>
        public static readonly Guid IWebBrowserApp =
            new Guid("{0002DF05-0000-0000-C000-000000000046}");
    }

    /// <summary>
    /// Commonly used category IDs
    /// </summary>
    public struct CATID
    {
        /// <summary>
        /// CATID for DeskBand
        /// </summary>
        public static readonly Guid DeskBand =
            new Guid("00021492-0000-0000-C000-000000000046");

        /// <summary>
        /// CATID for ExplorerBar (vertical/info-band)
        /// </summary>
        public static readonly Guid VerticalExplorerBar =
            new Guid("00021493-0000-0000-C000-000000000046");

        /// <summary>
        /// CATID for ExplorerBar (horizontal/communication-band)
        /// </summary>
        public static readonly Guid HorizontalExplorerBar =
            new Guid("00021494-0000-0000-C000-000000000046");
    }
}
