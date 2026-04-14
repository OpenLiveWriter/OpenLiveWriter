// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// File helper class.
    /// </summary>
    public sealed class FileHelper
    {
        /// <summary>
        /// Initializes a new instance of the FileHelper class.
        /// </summary>
        private FileHelper()
        {
        }

        public static string GetFileType(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            string fileType = null;
            if (extension != null && extension != string.Empty)
                fileType = ShellHelper.GetTypeNameForExtension(extension);
            if (fileType == null || fileType == string.Empty)
            {
                if (extension == string.Empty)
                    fileType = "Unknown";
                else
                    fileType = extension.Trim('.').ToUpper(CultureInfo.CurrentCulture) + " File";
            }
            return fileType;
        }

        public static bool IsErrorCodeNetworkRelated(int hresult)
        {
            int[] NET_CODES =
                {
                    0x0040, // network name no longer available
                    0x0035, // bad net path
                    0x0033, // ERROR_REM_NOT_LIST
                    0x0036, // network busy
                    0x0037, // DEV_NOT_EXIST
                    // see http://msdn.microsoft.com/library/en-us/debug/base/system_error_codes__1000-1299_.asp
                    1203,
                    1222,
                    1225,
                    1226,
                    1227,
                    1228,
                    1229,
                    1230,
                    1231,
                    1232,
                    1233,
                    1234,
                    1235,
                    1236,
                };

            foreach (int code in NET_CODES)
            {
                if (code == (hresult & 0xFFFF))
                    return true;

                // We are playing it pretty fast and loose here... .NET framework
                // classes transform hresults in ways that are not well documented.
                if ((code & 0x80130000) == 0x80130000 && (hresult & 0xFF) == code)
                {
                    Debug.Fail("HResult " + hresult + " looks like a network-related error code. If this is not correct, please notify Joe.");
                    return true;
                }
            }
            return false;
        }

        public static bool IsErrorCodeLockOrShareViolation(int hresult)
        {
            switch (hresult & 0xFFFF)
            {
                case 32:
                case 33:
                case 32 | 0x1620:  // See CorError.h
                case 33 | 0x1620:  // See CorError.h
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Returns -1 if unknown.
        /// </summary>
        /// <param name="ioe"></param>
        /// <returns></returns>
        public static int GetErrorCodeFromIOException(IOException ioe)
        {
            try
            {
                PropertyInfo property = typeof(Exception).GetProperty("HResult", BindingFlags.Instance | BindingFlags.NonPublic);
                return (int)property.GetValue(ioe, new object[0]);
            }
            catch (Exception e)
            {
                Trace.Fail("Error determining error code for IOException: " + e.ToString());
                return -1;
            }
        }

        /// <summary>
        ///	Gets a string containing the illegal characters.
        /// </summary>
        public static string IllegalCharsString
        {
            get
            {
                return @"/\:*?""<>|";
            }
        }

        /// <summary>
        /// The array of characters that aren't legal in a valid filename
        /// </summary>
        public static char[] IllegalChars
        {
            get
            {
                // This is a more restrictive set of characters
                if (m_illegalChars == null)
                {
                    char[] invalidPathChars = Path.GetInvalidPathChars();
                    int endInvalidPathChars = invalidPathChars.Length;
                    // our list of illegal characters
                    char[] notedChars = new char[] { (char)9, (char)10, (char)11, (char)12, (char)13, (char)42, (char)47, (char)58, (char)63, (char)92, (char)160 };

                    // system provided illegal characters
                    char[] illegalChars = new char[endInvalidPathChars + notedChars.Length];

                    // Copy the arrays to a new array
                    invalidPathChars.CopyTo(illegalChars, 0);
                    notedChars.CopyTo(illegalChars, endInvalidPathChars);

                    m_illegalChars = illegalChars;
                }

                return m_illegalChars;

            }
        }
        private static char[] m_illegalChars;
        public static int MaxFileNameLength = 100;

        /// <summary>
        /// Determines if a string is a valid OS filename.
        /// </summary>
        /// <param name="fileName">The string to validate</param>
        /// <returns>True if the string is a valid filename, otherwise false.</returns>
        public static bool IsValidOSFileName(string fileName)
        {
            if (fileName != null &&
                fileName.IndexOfAny(IllegalChars) < 0 &&
                fileName.Length <= Kernel32.MAX_PATH)
                return true;
            else
                return false;
        }

        public static bool IsValidFileName(string fileName)
        {
            return IsValidFileName(fileName, MaxFileNameLength);
        }

        /// <summary>
        /// Determines if a string is a valid filename.
        /// </summary>
        /// <param name="fileName">The string to validate</param>
        /// <returns>True if the string is a valid filename, otherwise false.</returns>
        public static bool IsValidFileName(string fileName, int maxLength)
        {
            if (fileName != null &&
                fileName.IndexOfAny(IllegalChars) < 0 &&
                fileName.Length <= maxLength &&
                !EndsWithEvilChar(fileName))
            {
                foreach (char ch in fileName.ToCharArray())
                    if (char.IsControl(ch))
                        return false;

                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Convert a file name to an Ansi file name (some external systems such as FTP
        /// and MAPI won't take double-byte file names).
        /// </summary>
        /// <param name="fileName">file name</param>
        /// <param name="maxFileNameLength">the maximum numbr of chars to allow in filename. If -1, no max will be enforced.</param>
        public static string GetValidAnsiFileName(string fileName, int maxFileNameLength)
        {
            // strip unicode characters
            StringBuilder ansiNameBuilder = new StringBuilder();
            foreach (char ch in fileName)
            {
                if (ch < 128
                    && (Char.IsLetterOrDigit(ch)
                    || ch == '.' || ch == '_' || ch == '-'))
                {
                    ansiNameBuilder.Append(ch);
                }
                else if (Char.IsWhiteSpace(ch))
                {
                    // Replace whitespace with hyphens.
                    ansiNameBuilder.Append('-');
                }
            }

            string safeFileName = ansiNameBuilder.ToString();
            safeFileName = TrimFileAndReplaceWithGuidIfEmpty(safeFileName, AnsiTrimChars);

            // return the converted file name
            string validFileName;
            if (maxFileNameLength != -1)
                validFileName = GetValidFileName(safeFileName, maxFileNameLength);
            else
                validFileName = GetValidFileName(safeFileName);
            return validFileName;
        }

        /// <summary>
        /// Convert a file name to an Ansi file name (some external systems such as FTP
        /// and MAPI won't take double-byte file names).
        /// </summary>
        /// <param name="fileName">file name</param>
        public static string GetValidAnsiFileName(string fileName)
        {
            return GetValidAnsiFileName(fileName, -1);
        }
        private static char[] AnsiTrimChars = new char[] { '~' };

        public static string GetValidFileName(string fileName)
        {
            return GetValidFileName(fileName, MaxFileNameLength);
        }

        public static string GetValidFileName(string fileName, int maxFileNameLength)
        {
            return GetValidFileName(fileName, maxFileNameLength, true);
        }

        /// <summary>
        /// Converts a string into a valid filename (by replacing invalid characters)
        /// </summary>
        /// <param name="fileName">The string to convert into a valid filename</param>
        /// <returns>The valid filename</returns>
        public static string GetValidFileName(string fileName, int maxFileNameLength, bool guidIfNecessary)
        {
            string originalFileName = fileName;

            if (!IsValidFileName(fileName, maxFileNameLength))
            {

                // strip invalid characters
                for (int i = 0; i < IllegalChars.Length; i++)
                    fileName = fileName.Replace(IllegalChars[i], ' ');

                StringBuilder nameBuilder = new StringBuilder();
                foreach (char ch in fileName)
                {
                    if (!char.IsControl(ch))
                        nameBuilder.Append(ch);
                }
                fileName = nameBuilder.ToString();

                // collapse any multiple spaces (in the event that we created them when stripping)
                fileName = Regex.Replace(fileName, @"\s+", " ");

                // When dealing with long names, make sure to try to leave the extension of the file
                // intact.  But also be aware that some names will be interpreted as having a long
                // extension.  In this case, make sure you truncate the extension to shorten the name.
                if (fileName.Length > maxFileNameLength)
                {
                    int minFileNameLength = 3;

                    // this includes the dot
                    int minExtensionLength = 4;
                    int numberOfCharsToRemove = fileName.Length - maxFileNameLength;

                    // If this is a very short max file name length, just cut it off
                    if (maxFileNameLength < minFileNameLength + minExtensionLength)
                    {
                        Debug.Fail("You passed in a very short maxFileNameLength, the fileName is likely going to be corrupted");
                        if (fileName.Length > maxFileNameLength)
                            fileName = fileName.Substring(0, maxFileNameLength);
                        return fileName;
                    }

                    // If we can, preserve the extension when shortening the filename.
                    // If we have to, fall back to 8.3 naming
                    string fileNameNoExtension = Path.GetFileNameWithoutExtension(fileName);
                    string extension = Path.GetExtension(fileName);

                    /* TODO This block of code is a potential candidate to replace some of the code below.  It's too late in the RC cycle to do it now.

                    fileNameNoExtension = StringHelper.RestrictLength(fileNameNoExtension,
                        Math.Max(fileNameNoExtension.Length - numberOfCharsToRemove, minFileNameLength));

                    string newFileName = fileNameNoExtension.Trim() + extension;

                    // Trim the extension to be as short as required (or the minimum length)
                    fileName = StringHelper.RestrictLength(fileNameNoExtension + extension, maxFileNameLength);

                    */

                    // Trim the filename itself to be as short as required (or the minimum length)
                    if (fileNameNoExtension.Length - minFileNameLength > numberOfCharsToRemove)
                    {
                        fileNameNoExtension = fileNameNoExtension.Substring(0, fileNameNoExtension.Length - numberOfCharsToRemove);
                    }
                    else
                    {
                        fileNameNoExtension = fileNameNoExtension.Substring(0, (fileNameNoExtension.Length >= minFileNameLength ? minFileNameLength : fileNameNoExtension.Length));
                    }

                    string newFileName = fileNameNoExtension + extension;

                    // Trim the extension to be as short as required (or the minimum length)
                    if (newFileName.Length > maxFileNameLength)
                    {
                        int numberOfCharsToRemoveFromExtension = newFileName.Length - maxFileNameLength;
                        if (extension.Length > numberOfCharsToRemoveFromExtension)
                            extension = extension.Substring(0, extension.Length - numberOfCharsToRemoveFromExtension);
                        else
                            extension = extension.Substring(0, (extension.Length >= minExtensionLength ? minExtensionLength : extension.Length));
                    }

                    fileName = fileNameNoExtension + extension;
                }

            }

            string noExt = Path.GetFileNameWithoutExtension(fileName);

            //trim whitespace from the beginning and end of the string
            noExt = noExt.Trim();

            //trim evil chars which are ignored by the OS from the end (such as '...')
            noExt = noExt.TrimEnd(EvilEndChars);

            if (guidIfNecessary)
            {
                //avoid bug 1354: if the filename has been stripped of all useful data,
                //then just generate a guid.
                // If a filename is something like '.htaccess', leave it alone since it is ok
                // Do not call Path.GetFileName() and like methods on originalFileName
                //    since these will throw exceptions if originalFileName has system-illegal characters
                //    in it, such as ", |, <, >, etc.
                if (originalFileName.Length == 0 ||  // empty string gets guided
                    originalFileName[0] != '.' ||    // non-".htaccess"-like names get guided
                    Regex.IsMatch(originalFileName, @"^\.+$"))   // ".", "..", "...", etc. gets guided
                    noExt = TrimFileAndReplaceWithGuidIfEmpty(noExt, null);
            }

            return noExt + Path.GetExtension(fileName);
        }

        public static string StripInvalidChars(string fileName)
        {
            // strip invalid characters
            for (int i = 0; i < IllegalChars.Length; i++)
            {
                int indexChar = fileName.IndexOf(IllegalChars[i], 0);
                while (indexChar != -1)
                {
                    fileName = fileName.Replace(IllegalChars[i], ' ');
                    indexChar = fileName.IndexOf(IllegalChars[i], 0);
                }
            }
            return fileName;
        }

        /// <summary>
        /// Changes the extension of a file
        /// </summary>
        /// <param name="filePath">The file whose extension should be changed</param>
        /// <param name="newExtension">The new extension for the file</param>
        public static string ChangeExtension(string filePath, string newExtension)
        {
            string oldPath = Path.GetDirectoryName(filePath);
            string oldFileName = Path.GetFileNameWithoutExtension(filePath);
            string newFileName = oldFileName + newExtension;
            string newPath = Path.Combine(oldPath, newFileName);
            Rename(filePath, newPath);
            return newPath;
        }

        /// <summary>
        ///	Renames a file.  Handles the case where the "to" file exists.
        /// </summary>
        /// <param name="filePathFrom">From file name.</param>
        /// <param name="filePathTo">To file name.</param>
        public static void Rename(string filePathFrom, string filePathTo)
        {
            //	If the "to" file doesn't exist, perform the rename.  Otherwise, rename the "to"
            //	file, rename the "from" file, and delete the "to" file if works.  If something
            //	fails, revert the renamed "to" file.
            if (!File.Exists(filePathTo))
                File.Move(filePathFrom, filePathTo);
            else
            {
                //	Construct a work path.
                string filePathWork = Path.ChangeExtension(filePathTo, ".wrk");

                //	Rename the "to" file.
                File.Move(filePathTo, filePathWork);

                //	Rename the "from" file to the "to" file.  If the rename fails, restore the "to"
                //	file.
                try
                {
                    File.Move(filePathFrom, filePathTo);
                }
                catch (Exception)
                {
                    File.Move(filePathWork, filePathTo);
                    throw;
                }

                //	Delete the work file.
                File.Delete(filePathWork);
            }
        }

        /// <summary>
        /// Returns a progId based upon the extension of a file
        /// </summary>
        /// <param name="extension">The extension (i.e. .doc)</param>
        /// <returns>The progId</returns>
        public static string GetProgIDFromExtension(string extension)
        {
            string progId = null;
            try
            {
                using (RegistryKey extensionKey = Registry.ClassesRoot.OpenSubKey(extension))
                {
                    if (extensionKey != null)
                        progId = (string)extensionKey.GetValue(null);
                }
            }
            catch (Exception ex)
            {
                if (RegistryHelper.IsRegistryException(ex))
                {
                    Trace.Fail("Exception thrown while getting ProgId for extension(" + extension + "): " + ex);
                }
                else
                {
                    throw;
                }
            }

            return progId;
        }

        /// <summary>
        /// Returns the command string used for opening a file
        /// based upon a progId
        /// </summary>
        /// <param name="progId">The progId</param>
        /// <returns>A string representing the open command</returns>
        public static string GetOpenCommandFromProgID(string progId)
        {
            string command = null;
            RegistryKey progKey = Registry.ClassesRoot.OpenSubKey(progId);
            if (progKey != null)
            {
                RegistryKey openKey = progKey.OpenSubKey("shell\\open\\command");
                if (openKey != null)
                    command = (string)openKey.GetValue(null);
            }
            return command;
        }

        /// <summary>
        /// Returns a CLSID based upon a progId
        /// </summary>
        /// <param name="progId">The progId</param>
        /// <returns>A Guid that is the CLSID for the given progId</returns>
        public static Guid GetClsidFromProgID(string progId)
        {
            Guid clsid = Guid.Empty;
            int hresult = Ole32.CLSIDFromProgID(progId, out clsid);
            return clsid;
        }

        /// <summary>
        /// Returns true if the file has the readonly attribute
        /// </summary>
        public static bool IsReadOnly(FileInfo file)
        {
            //must refresh the fileInfo in case it is stale
            file.Refresh();
            return (0 != (file.Attributes & FileAttributes.ReadOnly));
        }

        /// <summary>
        /// Sets the readonly attribute
        /// </summary>
        public static void SetReadOnly(FileInfo file)
        {
            //must refresh the fileInfo in case it is stale--this happens in IsReadOnly
            if (IsReadOnly(file))
                return;
            else
                file.Attributes = (file.Attributes | FileAttributes.ReadOnly);
        }

        public static void SetHidden(FileInfo file)
        {
            file.Refresh();
            FileAttributes attribs = file.Attributes;
            if (0 == (attribs & FileAttributes.Hidden))
                file.Attributes = attribs | FileAttributes.Hidden;
        }

        /// <summary>
        /// Removes the readonly attribute
        /// </summary>
        public static void SetWritable(FileInfo file)
        {
            //must refresh the fileInfo in case it is stale
            file.Refresh();

            if (!IsReadOnly(file))
                return;
            else
            {
                file.Attributes = (file.Attributes ^ FileAttributes.ReadOnly);
            }
        }

        /// <summary>
        /// Quick CRC calculation for a file.
        /// </summary>
        public static int QuickCRC(FileInfo file)
        {
            //must refresh the fileInfo in case it is stale
            file.Refresh();

            using (FileStream stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, false))
            {
                int accum = 0;
                int b;
                while (-1 != (b = stream.ReadByte()))
                    accum = accum * 31 + (b + 65535);
                return accum;
            }
        }

        /// <summary>
        /// Returns true if two files are probably equal.
        /// </summary>
        public static bool FilesEqual(FileInfo file1, FileInfo file2)
        {
            //must refresh the fileInfos in case they are stale
            file1.Refresh();
            file2.Refresh();

            if (!file1.Exists || !file2.Exists)
                throw new ArgumentException("Nonexistent file(s) cannot be compared");

            if (file1.Length != file2.Length)
                return false;
            if (file1.FullName == file2.FullName)
                return true;

            return QuickCRC(file1) == QuickCRC(file2);
        }

        /// <summary>
        /// Helper to get a pretty file name.  For example, passing a path value of:
        ///
        ///		c:\documents and settings\blambert\my documents\cape cod tourist attractions.cfs
        ///
        /// would result in a return value of:
        ///
        ///		Cape Cod Tourist Attractions.cfs
        ///
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <returns>Pretty file name.</returns>
        public static string GetPrettyFileName(string path)
        {
            string directoryName = Path.GetDirectoryName(path);

            if (Directory.Exists(directoryName) && File.Exists(path))
            {
                string[] files = Directory.GetFiles(directoryName, Path.GetFileName(path));
                if (files.Length == 1)
                    path = files[0];
            }

            return Path.GetFileName(path);
        }

        /// <summary>
        /// Helper to get a pretty path.  For example, passing a path value of:
        ///
        ///		c:\documents and settings\blambert\my documents\cape cod tourist attractions.cfs
        ///
        /// would result in a return value of:
        ///
        ///		C:\Documents and Settings\blambert\My Documents\Cape Cod Tourist Attractions.cfs
        ///
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <returns>Pretty file name.</returns>
        public static string GetPrettyPath(string path)
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
                    result.Append(el.Name.ToUpper(CultureInfo.CurrentCulture));
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

        public static string ReadFile(string path)
        {
            using (StreamReader reader = File.OpenText(path))
            {
                return reader.ReadToEnd();
            }
        }

        public static void WriteFile(string path, string str, bool append)
        {
            using (StreamWriter writer = new StreamWriter(path, append))
            {
                writer.Write(str);
            }
        }

        public static void WriteFile(string path, string str, bool append, Encoding encoding)
        {
            using (StreamWriter writer = new StreamWriter(path, append, encoding))
            {
                writer.Write(str);
            }
        }

        /// <summary>
        /// Gets the maximum file name name length for the specified path and file extension.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="extension">The extension (including the .).</param>
        /// <returns>The maximum file name name length for the specified path and file extension.</returns>
        public static int MaximumFileNameLengthForPathAndFileExtension(string path, string extension)
        {
            //	MAX_PATH-([path's length]+[PathSeparator's length]+[extension's length]+[mystery 1, for '\0'?];
            return Kernel32.MAX_PATH - (path.Length + 1 + extension.Length + 1);
        }

        /// <summary>
        /// Returns an IDisposable whose Dispose() will cause the file's
        /// last write time to be reset to the value it held when this
        /// method was called.
        ///
        /// This mechanism will fail silently (for example if the necessary
        /// rights are not available, the file does not exist, etc.).
        ///
        /// Example:
        ///
        /// using (FileHelper.PreserveLastWriteTime("c:\\foo.txt"))
        /// {
        ///		// do stuff with foo.txt
        /// }
        /// </summary>
        public static IDisposable PreserveLastWriteTime(string path)
        {
            return new PreserveLastWriteTimeHelper(path);
        }

        /// <summary>
        /// Trims characters from a filename and replaces the filename with a partial GUID if the trim
        /// results in an empty file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="additionalTrimChars">an set of chars (in addition to whitespace) that should be trimmed</param>
        /// <returns></returns>
        private static string TrimFileAndReplaceWithGuidIfEmpty(string fileName, char[] additionalTrimChars)
        {
            fileName = fileName.Trim();
            if (additionalTrimChars != null && additionalTrimChars.Length > 0)
            {
                fileName = fileName.Trim(additionalTrimChars);
            }

            //avoid bug 1354: if the filename has been stripped of all useful data,
            //then just generate a guid.
            // WinLive 272918: Make sure there is at least one alphanumeric character.
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            if (fileNameWithoutExtension == String.Empty ||
                !ArrayHelper.Any(fileNameWithoutExtension.ToCharArray(), c => Char.IsLetterOrDigit(c)))
            {
                string ext = Path.GetExtension(fileName);
                fileName = Guid.NewGuid().ToString().Split('-')[4] + ext;
            }

            return fileName;
        }

        /// <summary>
        /// Returns true if the specified filename ends with a char that is ignored by the OS in filenaming.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static bool EndsWithEvilChar(string fileName)
        {
            foreach (char ch in EvilEndChars)
            {
                if (fileName.EndsWith(new String(ch, 1), StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
        private static char[] EvilEndChars = new char[] { '.', ' ' };

        public static bool IsFileInUse(string filePath)
        {
            try
            {
                using (new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    // do nothing
                }
                return false;
            }
            catch (IOException)
            {
                return true;
            }

        }

        private class PreserveLastWriteTimeHelper : IDisposable
        {
            private string path;
            private DateTime dateTime;

            public PreserveLastWriteTimeHelper(string path)
            {
                this.path = path;
                try
                {
                    this.path = Path.GetFullPath(path);
                }
                catch
                {
                }

                this.dateTime = DateTime.MinValue;
                try
                {
                    this.dateTime = File.GetLastWriteTimeUtc(this.path);
                }
                catch
                {
                }
            }

            public void Dispose()
            {
                try
                {
                    File.SetLastWriteTimeUtc(this.path, this.dateTime);
                }
                catch
                {
                }
            }
        }

        public static bool IsSystemFile(string path)
        {
            return (File.GetAttributes(path) & FileAttributes.System) == FileAttributes.System;
        }
    }
}
