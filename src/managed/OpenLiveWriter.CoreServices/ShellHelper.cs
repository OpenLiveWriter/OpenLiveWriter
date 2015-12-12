// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Summary description for ShellHelper.
    /// </summary>
    public class ShellHelper
    {
        private ShellHelper()
        {
        }

        public static string GetAbsolutePath(string path)
        {
            if (Uri.IsWellFormedUriString(path, UriKind.Absolute))
                return path;
            else
                return Path.GetFullPath((path));

        }

        /// <summary>
        /// Sets the window's application id by its window handle.
        /// </summary>
        /// <param name="hwnd">The window handle.</param>
        /// <param name="appId">The application id.</param>
        public static void SetWindowAppId(IntPtr hwnd, string appId)
        {
            SetWindowProperty(hwnd, SystemProperties.System.AppUserModel.ID, appId);
        }

        internal static void SetWindowProperty(IntPtr hwnd, PropertyKey propkey, string value)
        {
            // Get the IPropertyStore for the given window handle
            IPropertyStore propStore = GetWindowPropertyStore(hwnd);

            // Set the value
            PropVariant pv = new PropVariant();
            propStore.SetValue(ref propkey, ref pv);

            // Dispose the IPropertyStore and PropVariant
            Marshal.ReleaseComObject(propStore);
            pv.Clear();
        }

        internal static IPropertyStore GetWindowPropertyStore(IntPtr hwnd)
        {
            IPropertyStore propStore;
            Guid guid = new Guid(Shell32.IPropertyStore);
            int rc = Shell32.SHGetPropertyStoreForWindow(
                hwnd,
                ref guid,
                out propStore);
            if (rc != 0)
                throw Marshal.GetExceptionForHR(rc);
            return propStore;
        }

        /// <summary>
        /// For a file extension (with leading period) and a verb (or null for default
        /// verb), returns the (full?) path to the executable file that is assigned to
        /// that extension/verb.  Returns null if an error occurs.
        /// </summary>
        public static string GetExecutablePath(string extension, string verb)
        {
            int capacity = 270;

        attempt:  // we may need to retry with a different (larger) value for "capacity"
            StringBuilder buffer = new StringBuilder(capacity);  // the buffer that will hold the result
            int hresult = Shlwapi.AssocQueryString(ASSOCF.NOTRUNCATE, ASSOCSTR.EXECUTABLE, extension, verb, buffer, ref capacity);

            switch (hresult)
            {
                case HRESULT.S_OK:
                    return buffer.ToString();  // success; return the path

                // failure; buffer was too small
                case HRESULT.E_POINTER:
                case HRESULT.S_FALSE:
                    // the capacity variable now holds the number of chars necessary (AssocQueryString
                    // assigns it).  it should work if we try again.
                    goto attempt;

                // failure.  the default case will catch all, but I'm explicitly
                // calling out the two failure codes I know about in case we need
                // them someday.
                case HRESULT.E_INVALIDARG:
                case HRESULT.E_FAILED:
                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets the friendly type string for an extension.
        /// </summary>
        public static string GetTypeNameForExtension(string extension)
        {
            if (extension != null)
                extension = extension.Trim();

            int capacity = 270;

        attempt:
            StringBuilder builder = new StringBuilder(capacity);
            int hresult = Shlwapi.AssocQueryString(ASSOCF.NOTRUNCATE, ASSOCSTR.FRIENDLYDOCNAME, extension, null, builder, ref capacity);

            switch (hresult)
            {
                case HRESULT.S_OK:
                    return builder.ToString();
                case HRESULT.E_POINTER:
                case HRESULT.S_FALSE:
                    // the capacity variable now holds the number of chars necessary.  try again
                    goto attempt;
                case HRESULT.E_INVALIDARG:
                case HRESULT.E_FAILED:
                default:
                    break;
            }

            if (extension == null || extension == string.Empty)
                return "Unknown";
            else
                return extension.TrimStart('.').ToUpper(CultureInfo.InvariantCulture) + " File";
        }

        public struct ExecuteFileResult
        {
            public ExecuteFileResult(bool newProcessCreated, int processId, string processName)
                : this(newProcessCreated, new int[] { processId }, new string[] { processName })
            {
            }

            public ExecuteFileResult(bool newProcessCreated, int[] processIdList, string[] processNameList)
            {
                this.NewProcessCreated = newProcessCreated;
                this.ProcessIdList = processIdList;
                this.ProcessNameList = processNameList;
            }

            public bool NewProcessCreated;
            public int[] ProcessIdList;
            public string[] ProcessNameList;
        }

        /// <summary>
        /// Shell-executes a file with a specific verb, then returns either the exact
        /// process that was launched, or if nothing was launched (i.e. a process was
        /// reused), returns a set of processes that might be handling the file.
        /// </summary>
        public static ExecuteFileResult ExecuteFile(string filePath, string verb)
        {
            // Execute the document using the shell.
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = true;
            startInfo.FileName = filePath;
            startInfo.Verb = verb;

            using (Process p = Process.Start(startInfo))
            {
                if (p != null)
                {
                    string processName = ProcessHelper.GetProcessName(p.Handle);
                    if (processName != null)
                        return new ExecuteFileResult(true, p.Id, processName);
                }
            }

            // A process was reused.  Need to find all the possible processes
            // that could've been reused and return them all.
            int[] processIds;
            string[] processNames;

            string command = GetExecutablePath(Path.GetExtension(filePath), verb);
            if (command == null)
            {
                // The extension/verb combination has no registered application.
                // We can't even guess at what process could be editing the file.
                processIds = new int[0];
                processNames = new string[0];
            }
            else
            {
                // A registered app was found.  We assume that a process with the
                // same name is editing the file.
                string imageName = Path.GetFileName(command);
                processIds = ProcessHelper.GetProcessIdsByName(imageName);
                processNames = new string[processIds.Length];
                for (int i = 0; i < processIds.Length; i++)
                    processNames[i] = imageName;
            }
            return new ExecuteFileResult(false, processIds, processNames);
        }

        public static ExecuteFileResult ExecuteFileWithExecutable(string filePath, string executable)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(executable);
            startInfo.Arguments = "\"" + filePath + "\"";

            using (Process p = Process.Start(startInfo))
            {
                if (p != null)
                    return new ExecuteFileResult(true, p.Id, ProcessHelper.GetProcessName(p.Handle));
            }

            string imageName = Path.GetFileName(executable);
            int[] processIds = ProcessHelper.GetProcessIdsByName(imageName);
            string[] processNames = new string[processIds.Length];
            for (int i = 0; i < processIds.Length; i++)
                processNames[i] = imageName;
            return new ExecuteFileResult(false, processIds, processNames);
        }

        /// <summary>
        /// This should be used as the default URL launching method, instead of Process.Start.
        /// </summary>
        public static void LaunchUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch (Win32Exception w32e)
            {
                // Benign but common error due to Firefox and/or Windows stupidity
                // http://kb.mozillazine.org/Windows_error_opening_Internet_shortcut_or_local_HTML_file_-_Firefox
                // The unchecked cast is necessary to make the uint wrap around to the proper int error code.
                if (w32e.ErrorCode == unchecked((int)0x80004005))
                    return;

                throw;
            }
        }

        /// <summary>
        /// Parse a "shell" file list (space delimited list with filenames that contain spaces
        /// being contained in quotes
        /// </summary>
        /// <param name="fileList">shell file list</param>
        /// <returns>array of file paths in the file list</returns>
        public static string[] ParseShellFileList(string fileList)
        {
            // otherwise check for a "shell format" list
            fileList = fileList.Trim();
            ArrayList fileListArray = new ArrayList();
            int currentLoc = 0;

            // scan for file entries
            while (currentLoc < fileList.Length)
            {
                // file entry
                string file = null;

                // skip leading white-space
                while (currentLoc < fileList.Length && Char.IsWhiteSpace(fileList[currentLoc]))
                    currentLoc++;

                // account for quoted entries
                if (fileList[currentLoc] == '"')
                {
                    // find next quote
                    int nextQuote = fileList.IndexOf('"', currentLoc + 1);
                    if (nextQuote != -1)
                    {
                        file = fileList.Substring(currentLoc + 1, nextQuote - currentLoc - 1);
                        currentLoc = nextQuote + 1;
                    }
                    else
                        break; // no end quote!
                }

                // if we didn't have a quoted entry then find next space delimited entry
                if (file == null)
                {
                    // skip leading white-space
                    while (currentLoc < fileList.Length && Char.IsWhiteSpace(fileList[currentLoc]))
                        currentLoc++;

                    // if we aren't at the end then get the next entry
                    if (currentLoc < fileList.Length)
                    {
                        // find the end of the entry
                        int endEntry = currentLoc;
                        while (endEntry < fileList.Length)
                            if (!Char.IsWhiteSpace(fileList[endEntry]))
                                endEntry++;
                            else
                                break;

                        // get the value for the entry
                        file = fileList.Substring(currentLoc, endEntry - currentLoc);
                        currentLoc = endEntry;
                    }
                    else
                        break; // at the end
                }

                // add the file to our list
                fileListArray.Add(file.Trim());
            }

            // return the list
            return (string[])fileListArray.ToArray(typeof(string));
        }

        /// <summary>
        /// Determine if there is a custom icon handler for the specified file extension
        /// </summary>
        /// <param name="fileExtension">file extension (including ".")</param>
        /// <returns>true if it has a custom icon handler, otherwise false</returns>
        public static bool HasCustomIconHandler(string fileExtension)
        {
            using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(fileExtension))
            {
                if (key != null)
                {
                    using (RegistryKey classKey = Registry.ClassesRoot.OpenSubKey(
                                key.GetValue(null, String.Empty) + @"\ShellEx\IconHandler"))
                    {
                        if (classKey != null)
                            return true;
                        else
                            return false;
                    }
                }
                else
                    return false;
            }
        }

        /// <summary>
        /// Get the small icon for the specified file
        /// </summary>
        /// <param name="filePath">path to file</param>
        /// <returns>IconHandle (or null if none found). IconHandle represents
        /// a Win32 HICON. It must be Disposed when the caller is finished with
        /// it to free the underlying resources used by the Icon. </returns>
        public static IconHandle GetSmallIconForFile(string filePath)
        {
            return GetIconForFile(filePath, SHGFI.SMALLICON);
        }

        /// <summary>
        /// Get the larege icon for the specified file
        /// </summary>
        /// <param name="filePath">path to file</param>
        /// <returns>IconHandle (or null if none found). IconHandle represents
        /// a Win32 HICON. It must be Disposed when the caller is finished with
        /// it to free the underlying resources used by the Icon. </returns>
        public static IconHandle GetLargeIconForFile(string filePath)
        {
            return GetIconForFile(filePath, SHGFI.LARGEICON);
        }

        /// <summary>
        /// Get the icon for the specified file
        /// </summary>
        /// <param name="filePath">path to file</param>
        /// <param name="iconType">icon type (small or large)</param>
        /// <returns>IconHandle (or null if none found). IconHandle represents
        /// a Win32 HICON. It must be Disposed when the caller is finished with
        /// it to free the underlying resources used by the Icon. </returns>
        private static IconHandle GetIconForFile(string filePath, uint iconType)
        {
            // allocate SHFILEINFO for holding results
            SHFILEINFO fileInfo = new SHFILEINFO();

            // get icon info
            IntPtr result = Shell32.SHGetFileInfo(filePath, 0, ref fileInfo,
                (uint)Marshal.SizeOf(fileInfo), SHGFI.ICON | iconType);
            if (result == IntPtr.Zero)
            {
                Debug.Fail("Error getting icon for file: " + Marshal.GetLastWin32Error());
                return null;

            }

            // return IconHandle
            return new IconHandle(fileInfo.hIcon);
        }

        /// <summary>
        /// Get the small icon for the specified file extension
        /// </summary>
        /// <param name="extension">file extension (including leading .)</param>
        /// <returns>IconHandle (or null if none found). IconHandle represents
        /// a Win32 HICON. It must be Disposed when the caller is finished with
        /// it to free the underlying resources used by the Icon. </returns>
        public static IconHandle GetSmallShortcutIconForExtension(string extension)
        {
            return GetIconForExtension(extension, SHGFI.SMALLICON | SHGFI.LINKOVERLAY);
        }

        /// <summary>
        /// Get the small icon for the specified file extension
        /// </summary
        /// <param name="extension">file extension (including leading .)</param>
        /// <returns>IconHandle (or null if none found). IconHandle represents
        /// a Win32 HICON. It must be Disposed when the caller is finished with
        /// it to free the underlying resources used by the Icon. </returns>
        public static IconHandle GetLargeShortcutIconForExtension(string extension)
        {
            return GetIconForExtension(extension, SHGFI.LARGEICON | SHGFI.LINKOVERLAY);
        }

        /// <summary>
        /// Get the small icon for the specified file extension
        /// </summary>
        /// <param name="extension">file extension (including leading .)</param>
        /// <returns>IconHandle (or null if none found). IconHandle represents
        /// a Win32 HICON. It must be Disposed when the caller is finished with
        /// it to free the underlying resources used by the Icon. </returns>
        public static IconHandle GetSmallIconForExtension(string extension)
        {
            return GetIconForExtension(extension, SHGFI.SMALLICON);
        }

        /// <summary>
        /// Get the large icon for the specified file extension
        /// </summary>
        /// <param name="extension">file extension (including leading .)</param>
        /// <returns>IconHandle (or null if none found). IconHandle represents
        /// a Win32 HICON. It must be Disposed when the caller is finished with
        /// it to free the underlying resources used by the Icon. </returns>
        public static IconHandle GetLargeIconForExtension(string extension)
        {
            return GetIconForExtension(extension, SHGFI.LARGEICON);
        }

        public static string[] PathExtensions
        {
            get
            {
                string pathext = Environment.GetEnvironmentVariable("PATHEXT");
                if (pathext == null)
                    pathext = ".COM;.EXE;.BAT;.CMD";

                string[] pathexts = StringHelper.Split(pathext, ";");
                for (int i = 0; i < pathexts.Length; i++)
                    pathexts[i] = pathexts[i].ToLower(CultureInfo.CurrentCulture).Trim();

                return pathexts;
            }
        }

        public static bool IsPathExtension(string extension)
        {
            return Array.IndexOf(PathExtensions, extension) >= 0;
        }

        /// <summary>
        /// Get the icon for the specified file extension
        /// </summary>
        /// <param name="extension">file extension (including leading .)</param>
        /// <param name="flags">icon type (small or large)</param>
        /// <returns>IconHandle (or null if none found). IconHandle represents
        /// a Win32 HICON. It must be Disposed when the caller is finished with
        /// it to free the underlying resources used by the Icon. </returns>
        private static IconHandle GetIconForExtension(string extension, uint flags)
        {
            // allocate SHFILEINFO for holding results
            SHFILEINFO fileInfo = new SHFILEINFO();

            // get icon info for file extension
            IntPtr result = Shell32.SHGetFileInfo(extension, FILE_ATTRIBUTE.NORMAL, ref fileInfo,
                (uint)Marshal.SizeOf(fileInfo), SHGFI.ICON | flags | SHGFI.USEFILEATTRIBUTES);
            if (result == IntPtr.Zero)
            {
                Debug.Fail("Error getting icon for file: " + Marshal.GetLastWin32Error());
                return null;
            }

            // return IconHandle
            return new IconHandle(fileInfo.hIcon);
        }

        /// <summary>
        /// Extension used for shortcuts
        /// </summary>
        public static string ShortcutExtension = ".lnk";

        public static void ParseCommand(string command, out string executable, out string arguments)
        {
            if (command == null)
                throw new ArgumentNullException("command", "Command cannot be null");
            if (command.Length == 0)
                throw new ArgumentOutOfRangeException("command", "Command cannot be the empty string");

            command = command.TrimStart();

            if (command[0] == '"')
            {
                int split = command.IndexOf('"', 1);
                if (split != -1)
                {
                    executable = command.Substring(1, split - 1);
                    arguments = string.Empty;
                    if (command.Length > split + 2)
                        arguments = command.Substring(split + 2);
                }
                else
                {
                    executable = command;
                    arguments = string.Empty;
                }
            }
            else
            {
                int split = command.IndexOf(' ');
                if (split != -1)
                {
                    executable = command.Substring(0, split);
                    arguments = string.Empty;
                    if (command.Length > split + 1)
                        arguments = command.Substring(split + 1);
                }
                else
                {
                    executable = command;
                    arguments = string.Empty;
                }
            }

        }

    }

    /// <summary>
    /// Class that encapsulates a Win32 Icon Handle. The class can be implicitly
    /// converted to a .NET Icon. The class must be disposed when the caller
    /// is finished with using the Icon (this frees the HANDLE via DestroyIcon
    /// </summary>
    public class IconHandle : IDisposable
    {
        /// <summary>
        /// Initialize from an HICON
        /// </summary>
        /// <param name="hIcon"></param>
        public IconHandle(IntPtr hIcon)
        {
            this.hIcon = hIcon;
        }

        /// <summary>
        /// Underlying HICON
        /// </summary>
        public IntPtr Handle
        {
            get
            {
                return hIcon;
            }
        }

        /// <summary>
        /// .NET Icon for HICON (tied to underlying HICON)
        /// </summary>
        public Icon Icon
        {
            get
            {
                if (icon == null)
                    icon = System.Drawing.Icon.FromHandle(hIcon);
                return icon;
            }
        }

        /// <summary>
        /// Dispose by destroying underlying HICON (makes all .NET icons returned
        /// from the Icon property invalid)
        /// </summary>
        public void Dispose()
        {
            User32.DestroyIcon(hIcon);
        }

        /// <summary>
        /// Underlying HICON
        /// </summary>
        private IntPtr hIcon = IntPtr.Zero;

        /// <summary>
        /// .NET Icon for HICON
        /// </summary>
        private Icon icon = null;
    }

}
