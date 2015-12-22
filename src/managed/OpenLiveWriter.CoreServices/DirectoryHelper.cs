// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Directory helper methods
    /// </summary>
    public class DirectoryHelper
    {
        /// <summary>
        /// Iterates over the contents of a directory and returns a complete list of all
        /// of the files contained within. The file names are returned as relative paths.
        /// Note that this method provides only a thin-layer over file system calls --
        /// low-level file system exceptions may be thrown and they should be caught and
        /// handled appropriately.
        /// </summary>
        /// <param name="path">Path to get a recursive listing for</param>
        /// <returns>An ArrayList of file names (represented as string objects)
        /// </returns>
        public static ArrayList ListRecursive(string path)
        {
            // enumerate directories (use file system standard separator, probabaly backslash)
            return ListRecursive(path, false);
        }

        /// <summary>
        /// Iterates over the contents of a directory and returns a complete list of all
        /// of the files contained within. The file names are returned as relative paths.
        /// Note that this method provides only a thin-layer over file system calls --
        /// low-level file system exceptions may be thrown and they should be
        /// caught and handled appropriately.
        /// </summary>
        /// <param name="path">Path to enumerate</param>
        /// <param name="useUriSeparator">Use URI standard (forward slash) to separate directories</param>
        /// <returns>An ArrayList of file names (represented as string objects)
        /// </returns>
        public static ArrayList ListRecursive(string path, bool useUriSeparator)
        {
            // recursively iterate through the directory to find the list of files
            DirectoryLister lister = new DirectoryLister(path, useUriSeparator, true);
            return lister.GetFiles();
        }

        /// <summary>
        /// Recursively copy the contents of one directory to another. Note  that this method
        /// provides a thin-layer over file system calls -- low-level file system exceptions
        /// may be thrown from this class and they should be caught and handled appropriately.
        /// </summary>
        /// <param name="sourcePath">Path to copy files from</param>
        /// <param name="destinationPath">Path to copy files to</param>
        /// <param name="overwrite">Overwrite existing files (if set to false then
        /// an error is thrown if a destination file already exists)</param>
        public static void Copy(
            string sourcePath, string destinationPath, bool overwrite)
        {
            // recursively copy all files in the source to the destination
            Copy(sourcePath, destinationPath, overwrite, true);
        }

        /// <summary>
        /// Copy the contents of one directory to another. Note  that this method
        /// provides a thin-layer over file system calls -- low-level file system exceptions
        /// may be thrown from this class and they should be caught and handled appropriately.
        /// </summary>
        /// <param name="sourcePath">Path to copy files from</param>
        /// <param name="destinationPath">Path to copy files to</param>
        /// <param name="overwrite">Overwrite existing files (if set to false then
        /// an error is thrown if a destination file already exists)</param>
        /// <param name="recursive">if true, then copy files and subfolders, if false only copy files</param>
        public static void Copy(
            string sourcePath, string destinationPath, bool overwrite, bool recursive)
        {
            // copy all files in the source to the destination
            DirectoryCopier copier =
                new DirectoryCopier(sourcePath, destinationPath, overwrite, recursive);
            copier.Copy();
        }

        /// <summary>
        /// Determine whether a path is a network volume or not.  Note that non network
        /// volumes may be of several types.
        /// </summary>
        /// <param name="path">The path</param>
        /// <returns>true indicates it is a network drive, false indicates it is not a network drive.
        /// (could be local, removable, ramdisk, unknown, etc. . . )</returns>
        public static bool IsNetworkDrive(string path)
        {
            // "\\" indicates a UNC path
            if (path.StartsWith(@"\\", StringComparison.OrdinalIgnoreCase))
                return true;
            else
            {
                // If it has a drive letter, check if that is a network mapped drive
                long driveType = Kernel32.GetDriveType(Directory.GetDirectoryRoot(path));
                if ((driveType & Kernel32.DRIVE.REMOTE)
                    == Kernel32.DRIVE.REMOTE)
                    return true;	// remote disk
                else
                    return false;	// local disk (or some other sort of disk)
            }
        }

        /// <summary>
        /// Determines whether a folder is a web page complete supporting directory
        /// </summary>
        /// <param name="directory">The directory to check</param>
        /// <returns>true if it is a supporting directory, otherwise false</returns>
        public static bool IsWebPageCompleteSupportingDirectory(string directory)
        {
            if (directory.EndsWith(FILES, StringComparison.OrdinalIgnoreCase))
            {
                string newPath = directory.Substring(0, directory.Length - FILES.Length);
                int lastSlash = directory.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase);
                string directoryName = newPath.Substring(lastSlash + 1, newPath.Length - lastSlash - 1);
                string baseDirectory = newPath.Substring(0, lastSlash);
                if (File.Exists(Path.Combine(baseDirectory, directoryName + ".htm")))
                    return true;
            }
            return false;
        }
        private const string FILES = "_files";

        private static Guid FOLDERID_Videos = new Guid("18989B1D-99B5-455B-841C-AB7C74E4DDFC");

        public static string GetVideosFolder()
        {
            string path = null;
            IntPtr ppidl;

            int ret = Shell32.SHGetKnownFolderIDList(ref FOLDERID_Videos, 0, IntPtr.Zero, out ppidl);

            if (ret == 0)
            {
                StringBuilder sb = new StringBuilder(Kernel32.MAX_PATH + 1);

                if (Shell32.SHGetPathFromIDList(ppidl, sb))
                {
                    path = sb.ToString();
                }
            }

            if (ppidl != IntPtr.Zero)
                Shell32.ILFree(ppidl);

            return path;
        }
    }

    /// <summary>
    /// Abstract class which iterates recursively over a directory and its subdirctories.
    /// Subclasses are notified of the directories and files found via the OnSubdirectory
    /// and OnFile virtual methods. These can be overridden to provide custom functionality
    /// such as enumeration, copying, compression, etc.
    /// </summary>
    public abstract class DirectoryIterator
    {
        /// <summary>
        /// Create a DirectoryIterator and attached it to the specified path
        /// </summary>
        /// <param name="rootPath">Path to iterate over</param>
        /// <param name="includeSubfolders">true if files in subfolders should be included</param>
        protected DirectoryIterator(string rootPath, bool includeSubfolders)
        {
            // store root path and make sure it ends with "/"
            m_rootPath = rootPath;
            m_includeSubfolders = includeSubfolders;
            if (!m_rootPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.OrdinalIgnoreCase))
                m_rootPath += Path.DirectorySeparatorChar;
        }

        /// <summary>
        /// Directory which is being iterated
        /// </summary>
        public string RootPath { get { return m_rootPath; } }

        /// <summary>
        /// Kick-off recursive iteration
        /// </summary>
        protected void Iterate()
        {
            Iterate("");
        }

        /// <summary>
        /// Virtual function called whenever a new subdirectory is found
        /// </summary>
        /// <param name="directoryName">sub-directory name (relative path)</param>
        protected virtual void OnSubdirectory(string directoryName) { }

        /// <summary>
        /// Virtual function called whenever a new file is found
        /// </summary>
        /// <param name="fileName">file name (relative path)</param>
        protected virtual void OnFile(string fileName) { }

        /// <summary>
        /// Recursive iteration function
        /// </summary>
        /// <param name="relativePath">relative path to iterate over</param>
        private void Iterate(string relativePath)
        {
            // determine the absolute path of this directory
            string absolutePath = Path.Combine(RootPath, relativePath);

            // retreive a list of the files and sort them
            string[] files;
            try
            {
                files = Directory.GetFiles(absolutePath);
            }
            catch (Exception e)
            {
                if (e is IOException || e is UnauthorizedAccessException)
                {
                    files = new string[0];
                }
                else
                    throw;
            }

            Array.Sort(files);

            // notify subclass of each file
            foreach (string file in files)
            {
                // screen out hidden and system files
                FileAttributes attribs = File.GetAttributes(file);
                if ((attribs & FileAttributes.Hidden) == 0 &&
                     (attribs & FileAttributes.System) == 0)
                {
                    OnFile(AbsoluteToRelative(file));
                }
            }

            if (m_includeSubfolders)
            {
                // retreive a list of the sub-directories and sort them
                string[] directories;
                try
                {
                    directories = Directory.GetDirectories(absolutePath);
                }
                catch (Exception e)
                {
                    if (e is IOException || e is UnauthorizedAccessException)
                    {
                        directories = new string[0];
                    }
                    else
                        throw;
                }
                Array.Sort(directories);

                // notify subclass of each directory and then recursively
                // iterate over the contents of the directory
                foreach (string directory in directories)
                {
                    // calculate the relative path
                    string relativeDir = AbsoluteToRelative(directory);

                    // notify subclass
                    OnSubdirectory(relativeDir);

                    // recursively explore its files and subdirectories
                    Iterate(relativeDir);
                }
            }
        }

        /// <summary>
        /// Convert an absolute path to a relative path
        /// </summary>
        /// <param name="path">absolute path</param>
        /// <returns>relative path</returns>
        private string AbsoluteToRelative(string path)
        {
            // verify we are being called correctly
            Debug.Assert(path.StartsWith(RootPath, StringComparison.OrdinalIgnoreCase));

            // remove the root path from the absolute path to create a relative path
            return path.Remove(0, RootPath.Length);
        }

        // directory which is being iterated
        private string m_rootPath;

        //flag to set whether subfolders should be included in listing.
        private bool m_includeSubfolders;
    }

    public class DirectoryException : Exception
    {
        public MessageId? MessageId;
        public string Path;

        public DirectoryException(string path)
        {
            MessageId = null;
            Path = path;
        }

        public DirectoryException(MessageId message)
        {
            MessageId = message;
            Path = null;
        }
    }

    /// <summary>
    /// Class which iterates over the contents of a directory and returns a
    /// complete list of all of the files contained within. The file names are
    /// returned in alphabetical-order as relative paths. Note that this class provides
    /// a thin-layer over file system calls -- low-level file system exceptions may
    /// be thrown from this class and they should be caught and handled appropriately.
    /// </summary>
    public class DirectoryLister : DirectoryIterator
    {
        /// <summary>
        /// Create a DirectoryLister for a specified path
        /// </summary>
        /// <param name="rootPath">Path to iterate over</param>
        /// <param name="useUriSeparator">Use URI standard (forward slash) to separate directories</param>
        public DirectoryLister(string rootPath, bool useUriSeparator, bool includeSubfolders)
            : base(rootPath, includeSubfolders)
        {
            m_useUriSeparator = useUriSeparator;
        }

        /// <summary>
        /// Retreive an alphabetically sorted list of all the files contained within
        /// the directory and its sub-directories
        /// </summary>
        /// <returns>An ArrayList of file names (represented as string objects)</returns>
        public ArrayList GetFiles()
        {
            // clear out any existing contents of file list
            m_files = new ArrayList();

            // iterate over the files
            Iterate();

            // return a list of them
            return m_files;
        }

        /// <summary>
        /// When a file is discovered, add it to our list of files
        /// </summary>
        /// <param name="fileName">file name (relative path)</param>
        protected override void OnFile(string fileName)
        {
            // if we need to convert to URI separators do so
            string file = fileName;
            if (m_useUriSeparator)
                file = file.Replace(Path.DirectorySeparatorChar, '/');

            // add the file
            m_files.Add(file);
        }

        // flag indicating whether we should convert OS path separators to URI separtors
        bool m_useUriSeparator;

        // storage for list of files
        private ArrayList m_files;
    }

    /// <summary>
    /// Class which recursively copies the contents of one directory to another. Note
    /// that this class provides a thin-layer over file system calls -- low-level files
    /// ystem exceptions may be thrown from this class and they should be caught and
    /// handled appropriately.
    /// </summary>
    public class DirectoryCopier : DirectoryIterator
    {
        /// <summary>
        /// Initialize a DirectoryCopier
        /// </summary>
        /// <param name="sourcePath">Path to copy files from</param>
        /// <param name="destinationPath">Path to copy files to</param>
        /// <param name="overwrite">Overwrite existing files (if set to false then
        /// an error is thrown if a destination file already exists)</param>
        public DirectoryCopier(string sourcePath, string destinationPath, bool overwrite, bool recursive)
            : base(sourcePath, recursive)
        {
            m_destinationPath = destinationPath;
            m_overwrite = overwrite;
            m_recursive = recursive;
        }

        /// <summary>
        /// Perform the copy
        /// </summary>
        public void Copy()
        {
            if (!Directory.Exists(m_destinationPath))
                Directory.CreateDirectory(m_destinationPath);
            Iterate();
        }

        /// <summary>
        /// When notified of a sub-directory in the source tree, create the corresponding
        /// sub-directory in the destination tree.
        /// </summary>
        /// <param name="directoryName">sub-directory name (relative path)</param>
        protected override void OnSubdirectory(string directoryName)
        {
            string absolutePath = Path.Combine(m_destinationPath, directoryName);
            if (!Directory.Exists(absolutePath))
                Directory.CreateDirectory(absolutePath);
        }

        /// <summary>
        /// When notified of a file in the source tree, copy the file to the corresponding
        /// location in the destination tree.
        /// </summary>
        /// <param name="fileName">file name (relative path)</param>
        protected override void OnFile(string fileName)
        {
            // generate full paths
            string sourcePath = Path.Combine(RootPath, fileName);
            string destPath = Path.Combine(m_destinationPath, fileName);

            // copy the file
            File.Copy(sourcePath, destPath, m_overwrite);
        }

        /// <summary>
        /// Get the destination path
        /// </summary>
        protected string DestinationPath
        {
            get
            {
                return m_destinationPath;
            }
        }

        // path to copy files to
        private string m_destinationPath;

        // flag specifying whether to overwrite existing files
        private bool m_overwrite;

        //flag specifying whether to copy subfolders
        private bool m_recursive;
    }
}
