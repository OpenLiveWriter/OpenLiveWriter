// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using OpenLiveWriter.Interop.Com;

namespace OpenLiveWriter.Interop.Windows
{
    /// <summary>
    /// Summary description for Shlwapi.
    /// </summary>
    public class Shlwapi
    {
        /// <summary>
        /// Combines two paths (relative and base) to form a new path
        /// </summary>
        /// <param name="pszBase">The base path</param>
        /// <param name="pszRelative">The relative path</param>
        /// <returns>The combined path</returns>
        public static string UrlCombine(string pszBase, string pszRelative)
        {
            StringBuilder builder = new StringBuilder(DEFAULT_URL_BUFFER_SIZE);
            IntPtr bufferSize = new IntPtr(builder.Capacity);

            int hResult = Shlwapi.UrlCombine(
                                    pszBase,
                                    pszRelative,
                                    builder,
                                    ref bufferSize,
                                    0);

            // The buffer wasn't large enough, grow it!'ll
            while (hResult == HRESULT.E_POINTER)
            {
                builder = new StringBuilder(bufferSize.ToInt32());
                hResult = Shlwapi.UrlCombine(
                    pszBase,
                    pszRelative,
                    builder,
                    ref bufferSize,
                    0);
            }

            // Some other exception has occurred, bail
            if (hResult != HRESULT.S_OK)
                throw new COMException("Unabled to combine Urls", hResult);

            // We've got the new URL
            return builder.ToString();
        }

        private static readonly int DEFAULT_URL_BUFFER_SIZE = 32;

        [DllImport("Shlwapi.dll", CharSet = CharSet.Auto)]
        public static extern int UrlCombine(
            [In, MarshalAs(UnmanagedType.LPTStr)] string pszBase,
            [In, MarshalAs(UnmanagedType.LPTStr)] string pszRelative,
            [Out, MarshalAs(UnmanagedType.LPTStr)] StringBuilder pszCombined,
            [In, Out] ref IntPtr pcchCombined,
            uint dwFlags
            );

        public struct URL
        {
            public const UInt32 ESCAPE_SPACES_ONLY = 0x04000000;
            public const UInt32 DONT_SIMPLIFY = 0x08000000;
            public const UInt32 ESCAPE_PERCENT = 0x00001000;
            public const UInt32 UNESCAPE = 0x10000000;
            public const UInt32 ESCAPE_UNSAFE = 0x20000000;
            public const UInt32 PLUGGABLE_PROTOCOL = 0x40000000;
        }

        [DllImport("Shlwapi.dll")]
        public static extern int SHAutoComplete(IntPtr hwndEdit, uint dwFlags);

        // interop declaration for converting a path to a URL
        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        public static extern int UrlCreateFromPath(
            [In, MarshalAs(UnmanagedType.LPTStr)] string pszPath,
            [Out, MarshalAs(UnmanagedType.LPTStr)] StringBuilder pszUrl,
            [In, Out] ref uint pcchUrl,
            [In] uint dwReserved);


        /// <summary>
        /// Searches for and retrieves a file association-related string from the registry.
        /// </summary>
        /// <param name="flags">Flags that can be used to control the search. It can be any combination of ASSOCF values, except that only one INIT value can be included.</param>
        /// <param name="str">ASSOCSTR value that specifies the type of string that is to be returned.</param>
        /// <param name="pszAssoc">
        /// Pointer to a null-terminated string that is used to determine the root key. Four types of strings can be used.
        /// File name extension
        ///	    A file name extension, such as .txt.
        /// CLSID
        ///     A class identifier (CLSID) globally unique identifier (GUID) in the standard "{GUID}" format.
        /// ProgID
        ///     An application's ProgID, such as Word.Document.8.
        /// Executable name
        ///     The name of an application's .exe file. The ASSOCF_OPEN_BYEXENAME flag must be set in flags.
        /// </param>
        /// <param name="pszExtra">Optional null-terminated string with additional information about the location of the string. It is normally set to a Shell verb such as open. Set this parameter to NULL if it is not used.</param>
        /// <param name="pszOut">Null-terminated string used to return the requested string. Set this parameter to NULL to retrieve the required buffer size.</param>
        /// <param name="pcchOut">Pointer to a value that is set to the number of characters in the pszOut buffer. When the function returns, it will be set to the number of characters actually placed in the buffer.
        /// If the ASSOCF.NOTRUNCATE flag is set in flags and the buffer specified in pszOut is too small, the function returns E_POINTER and the value is set to the required size of the buffer.
        /// If pszOut is NULL, the function returns S_FALSE and pcchOut points to the required size of the buffer.</param>
        /// <returns>
        /// Returns a standard error value or one of the following: Error Meaning
        /// S_OK Success.
        /// E_POINTER The pszOut buffer is too small to hold the entire string.
        /// S_FALSE pszOut is NULL. pcchOut contains the required buffer size.
        /// </returns>
        [DllImport("Shlwapi.dll", CharSet = CharSet.Auto)]
        public static extern int AssocQueryString(
            [In] ASSOCF flags,
            [In] ASSOCSTR str,
            [In] string pszAssoc,
            [In] string pszExtra,
            [Out, MarshalAs(UnmanagedType.LPTStr)] StringBuilder pszOut,
            [In, Out] ref int pcchOut
            );

        [DllImport("Shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int SHCreateStreamOnFileEx(string pszFile,
                                                        int grfMode,
                                                        int dwAttributes,
                                                        bool fCreate,
                                                        IntPtr pstmTemplate,
                                                        out IStream ppstm);
    }

    public enum ASSOCF
    {
        /// <summary>
        /// do not remap clsids to progids
        /// </summary>
        INIT_NOREMAPCLSID = 0x00000001,

        /// <summary>
        /// executable is being passed in
        /// </summary>
        INIT_BYEXENAME = 0x00000002,

        /// <summary>
        /// executable is being passed in
        /// </summary>
        OPEN_BYEXENAME = 0x00000002,

        /// <summary>
        /// treat "*" as the BaseClass
        /// </summary>
        INIT_DEFAULTTOSTAR = 0x00000004,

        /// <summary>
        /// treat "Folder" as the BaseClass
        /// </summary>
        INIT_DEFAULTTOFOLDER = 0x00000008,

        /// <summary>
        /// dont use HKCU
        /// </summary>
        NOUSERSETTINGS = 0x00000010,

        /// <summary>
        /// dont truncate the return string
        /// </summary>
        NOTRUNCATE = 0x00000020,

        /// <summary>
        /// verify data is accurate (DISK HITS)
        /// </summary>
        VERIFY = 0x00000040,

        /// <summary>
        /// actually gets info about rundlls target if applicable
        /// </summary>
        REMAPRUNDLL = 0x00000080,

        /// <summary>
        /// attempt to fix errors if found
        /// </summary>
        NOFIXUPS = 0x00000100,

        /// <summary>
        /// dont recurse into the baseclass
        /// </summary>
        IGNOREBASECLASS = 0x00000200,
    }

    public enum ASSOCSTR
    {
        COMMAND = 1,  //  shell\verb\command string
        EXECUTABLE,        //  the executable part of command string
        FRIENDLYDOCNAME,   //  friendly name of the document type
        FRIENDLYAPPNAME,   //  friendly name of executable
        NOOPEN,            //  noopen value
        SHELLNEWVALUE,     //  query values under the shellnew key
        DDECOMMAND,        //  template for DDE commands
        DDEIFEXEC,         //  DDECOMMAND to use if just create a process
        DDEAPPLICATION,    //  Application name in DDE broadcast
        DDETOPIC,          //  Topic Name in DDE broadcast
        INFOTIP,           //  info tip for an item, or list of properties to create info tip from
        QUICKTIP,          //  same as INFOTIP, except, this list contains only quickly retrievable properties
        TILEINFO,          //  similar to INFOTIP - lists important properties for tileview
        CONTENTTYPE,       //  MIME Content type
        DEFAULTICON,       //  Default icon source
        SHELLEXTENSION,    //  Guid string pointing to the Shellex\Shellextensionhandler value.
        MAX                //  last item in enum...
    }

    public struct SHACF
    {
        public const uint DEFAULT = 0x00000000;  // Currently (SHACF_FILESYSTEM | SHACF_URLALL)
        public const uint FILESYSTEM = 0x00000001;  // This includes the File System as well as the rest of the shell (Desktop\My Computer\Control Panel\)
        public const uint URLALL = (SHACF.URLHISTORY | SHACF.URLMRU);
        public const uint URLHISTORY = 0x00000002;  // URLs in the User's History
        public const uint URLMRU = 0x00000004;  // URLs in the User's Recently Used list.
        public const uint USETAB = 0x00000008;  // Use the tab to move thru the autocomplete possibilities instead of to the next dialog/window control.
        public const uint FILESYS_ONLY = 0x00000010;  // This includes the File System
        public const uint AUTOSUGGEST_FORCE_ON = 0x10000000;  // Ignore the registry default and force the feature on.
        public const uint AUTOSUGGEST_FORCE_OFF = 0x20000000;  // Ignore the registry default and force the feature off.
        public const uint AUTOAPPEND_FORCE_ON = 0x40000000;  // Ignore the registry default and force the feature on. (Also know as AutoComplete)
        public const uint AUTOAPPEND_FORCE_OFF = 0x80000000;  // Ignore the registry default and force the feature off. (Also know as AutoComplete)
    }

}
