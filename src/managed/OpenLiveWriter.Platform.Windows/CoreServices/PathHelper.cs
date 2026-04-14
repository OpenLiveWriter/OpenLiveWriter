// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Class providing utilities for dealing with Paths.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class PathHelper
    {
        /// <summary>
        /// Determines whether a path is a path to a directory
        /// </summary>
        /// <param name="path">The path to check</param>
        /// <returns>true if the path is a directory, otherwise false</returns>
        public static bool IsDirectory(string path)
        {
            return (File.GetAttributes(path) & FileAttributes.Directory) != 0;
        }

#if FALSE
        // not localized
        public static string GetPrettyPath(string path, bool hideExtensionsForKnowTypes)
        {
            if (path == null)
                return null;

            // obtain special paths.
            string myDocumentsPath = (Environment.GetFolderPath(Environment.SpecialFolder.Personal)+Path.DirectorySeparatorChar).ToLower() ;
            string desktopPath = (Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)+Path.DirectorySeparatorChar).ToLower() ;

            // default to pretty path being the same as base path
            string prettyPath = path ;

            if ( prettyPath.ToLower().StartsWith(myDocumentsPath) )
                prettyPath = "My Documents\\" + prettyPath.Substring( myDocumentsPath.Length ) ;

                // also check for the desktop
            else if ( prettyPath.ToLower().StartsWith(desktopPath) )
                prettyPath = "Desktop\\" + prettyPath.Substring( desktopPath.Length ) ;

            // if we should hide the extension, do so.
            if (hideExtensionsForKnowTypes)
                prettyPath = Path.Combine(Path.GetDirectoryName(prettyPath), Path.GetFileNameWithoutExtension(prettyPath));

            // return the pretty path
            return prettyPath ;
        }
#endif

        public static bool PathsEqual(string file1, string file2)
        {
            if (file1 == null ^ file2 == null)
                return false;

            // both null
            if (file1 == null)
                return true;
            // exactly equal
            if (file1 == file2)
                return true;
            if (file1.ToLower(CultureInfo.CurrentCulture) == file2.ToLower(CultureInfo.CurrentCulture))
                return true;

            // try short filenames
            string sfile1 = Kernel32.GetShortPathName(file1);
            string sfile2 = Kernel32.GetShortPathName(file2);
            if (sfile1 == sfile2)
                return true;

            // try short, lower-case filenames
            if (sfile1 != null)
                sfile1 = sfile1.ToLower(CultureInfo.CurrentCulture);
            if (sfile2 != null)
                sfile2 = sfile2.ToLower(CultureInfo.CurrentCulture);

            return sfile1 == sfile2;
        }

        /// <summary>
        /// Determines if a Path is a path to an image file
        /// </summary>
        /// <param name="path">the path to check</param>
        /// <returns>true if the path is to an image file, otherwise false</returns>
        public static bool IsPathImage(string path)
        {
            switch (Path.GetExtension(path).ToUpperInvariant())
            {
                case ".GIF":
                case ".JPG":
                case ".PNG":
                case ".JPEG":
                case ".TIF":
                case ".TIFF":
                case ".BMP":
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Determines if a path to a file is a path to a Url Shortcut file
        /// </summary>
        /// <param name="path">the path to check</param>
        /// <returns>true if the path is to an url file, otherwise false</returns>
        public static bool IsPathUrlFile(string path)
        {
            switch (Path.GetExtension(path).ToUpperInvariant())
            {
                case ".URL":
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Determines if a Path is a path to a web page.
        /// </summary>
        /// <param name="path">the path to check</param>
        /// <returns>true if the path is to an web page, otherwise false</returns>
        public static bool IsWebPage(string path)
        {
            switch (Path.GetExtension(path).ToUpperInvariant())
            {
                case ".HTM":
                case ".HTML":
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Contracts the specified path.
        /// </summary>
        /// <param name="path">Path to contract.</param>
        /// <returns>Contracted path.</returns>
        public static string ContractPath(string path, int maxLength)
        {
            if (path.Length < maxLength)
                return path;

            //	Contract the path.
            try
            {
                StringBuilder resultStringBuilder = new StringBuilder(1024);
                if (PathCompactPathEx(resultStringBuilder, path, maxLength, 0))
                    return resultStringBuilder.ToString();
                else
                    return path;
            }
            catch
            {
                return path;
            }
        }

        /// <summary>
        /// Creates a path that will no overwrite any files that already exist at
        /// that location.
        ///
        /// To ensure this, a zero-length file is created and its full path is returned.
        /// </summary>
        /// <param name="path">The path requested</param>
        /// <returns>The path that contains the new zero-length file</returns>
        public static string GetNonConflictingPath(string path)
        {
            string directory = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);

            return TempFileManager.CreateNewFile(directory, fileName, false);
        }

        public static bool IsPathInTempDirectory(string path)
        {
            string longTempDirectory = Kernel32.GetLongPathName(Path.GetTempPath());
            string longPath = Kernel32.GetLongPathName(path);

            if (longPath != null && longTempDirectory != null)
                return longPath.StartsWith(longTempDirectory, StringComparison.OrdinalIgnoreCase);
            else
                return false;
        }

        [DllImport("Shlwapi.dll", CharSet = CharSet.Auto)]
        public static extern bool PathCompactPathEx(
            [MarshalAs(UnmanagedType.LPTStr)]   StringBuilder pszOut,
            [MarshalAs(UnmanagedType.LPTStr)]   string pszSource,
            [MarshalAs(UnmanagedType.U4)]       int cchMax,
            [MarshalAs(UnmanagedType.U4)]       int dwReserved);

        /// <summary>
        /// Normalizes the capitalization for the given path, if
        /// it exists.
        /// </summary>
        public static string GetNormalizedPathName(string path)
        {
            if (path == null)
                return string.Empty;
            if (!File.Exists(path) && !Directory.Exists(path))
                return path;
            if (path.StartsWith("\\", StringComparison.OrdinalIgnoreCase))
                return path;

            FileSystemInfo[] infos = Decompose(path);
            StringBuilder result = new StringBuilder();
            FileSystemInfo lastEl = null;
            for (int i = 0; i < infos.Length; i++)
            {
                FileSystemInfo el = infos[i];

                if (lastEl == null)
                {
                    result.Append(el.Name.ToUpper(CultureInfo.InvariantCulture));
                }
                else
                {
                    if (el.Exists)
                    {
                        string[] entries = Directory.GetFileSystemEntries(lastEl.FullName, el.Name);
                        if (entries.Length == 1)
                        {
                            result.Append(Path.GetFileName(entries[0]));
                        }
                        else
                        {
                            // something went terribly wrong!
                            return path;
                        }
                    }
                    else
                    {
                        result.Append(el.Name);
                    }
                }
                if (i > 0 && i != infos.Length - 1)
                    result.Append(Path.DirectorySeparatorChar);
                lastEl = el;
            }
            return result.ToString();
        }

        private static FileSystemInfo[] Decompose(string path)
        {
            ArrayList result = new ArrayList();
            DirectoryInfo curr = new DirectoryInfo(path);
            while (curr != null)
            {
                result.Add(curr);
                curr = curr.Parent;
            }
            result.Reverse();
            return (FileSystemInfo[])result.ToArray(typeof(FileSystemInfo));
        }

        public static bool IsPathVideo(string path)
        {
            if (!File.Exists(path)) return false;

            switch (Path.GetExtension(path).ToUpperInvariant())
            {
                case ".ASF":
                case ".AVI":
                case ".DV":
                case ".WMV":
                case ".MOV":
                case ".MPG":
                case ".MPEG":
                case ".MPEG4":
                case ".MP4":
                case ".QT":
                case ".3GP":
                case ".3G2":
                case ".3GPP":
                case ".3GP2":
                case ".DIVX":
                case ".XVID":
                case ".VOB":
                case ".MOD":
                case ".MQV":
                case ".MPE":
                case ".M1V":
                case ".M2V":
                case ".264":
                case ".H264":
                case ".MT2S":
                case ".MT2TS":
                case ".M4V":
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Removes leading and trailing slashes on a path, if they exist.
        /// </summary>
        /// <param name="path">The path to process</param>
        /// <returns></returns>
        public static string RemoveLeadingAndTrailingSlash(string path)
            => path.Trim(new char[] {'/', '\\'});
    }
}
