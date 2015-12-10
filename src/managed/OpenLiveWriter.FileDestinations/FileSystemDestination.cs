// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;

namespace OpenLiveWriter.FileDestinations
{
    /// <summary>
    /// Summary description for FileSystemDestination.
    /// </summary>
    public class FileSystemDestination : FileDestination
    {

        /// <summary>
        /// FileSystemDestination Constructor
        /// </summary>
        /// <param name="path">The path to the destination.</param>
        public FileSystemDestination(string path)
            : base(path, 5, 1000)
        {
            if (!Directory.Exists(path))
            {
                throw new NoSuchDirectoryException(path);
            }
        }

        /// <summary>
        /// FileSystemDestination Constructor
        /// </summary>
        /// <param name="path">The path to the destination.</param>
        /// <param name="retryCount">The number of times to retry the file copy</param>
        /// <param name="retryPause">The length of time in milliseconds to pause between retries</param>
        public FileSystemDestination(string path, int retryCount, int retryPause)
            : base(path, retryCount, retryPause)
        {
            if (!Directory.Exists(path))
            {
                throw new NoSuchDirectoryException(path);
            }
        }

        /// <summary>
        /// Copies files to a local destination
        /// </summary>
        /// <param name="fromPath">The path from which to copy files (relative to basePath)</param>
        /// <param name="toPath">The path to which to copy files (relative to basePath)</param>
        /// <param name="overwrite">Whether or not to overwrite existing files</param>
        /// <returns>True indicates success, False indicates non fatal error</returns>
        public override bool CopyFile(string fromPath, string toPath, bool overwrite)
        {
            fromPath = Path.Combine(m_path, fromPath);
            toPath = Path.Combine(m_path, toPath);
            try
            {
                // Delete existing file if we're supposed to overwrite
                if (overwrite && File.Exists(toPath))
                    File.Delete(toPath);

                // Move the file
                File.Copy(fromPath, toPath);

                return true;
            }
            catch (Exception e)
            {
                if (RetryOnException(e))
                    return false;
                else
                {
                    SiteDestinationException ex = new SiteDestinationException(e, SiteDestinationException.TransferException, GetType().Name, 0);
                    ex.DestinationExtendedMessage = e.Message;
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Retrieves a file from the destination and copies it to the specified path.
        /// </summary>
        /// <param name="fromPath">the file to retrieve from the destination (relative to basePath)</param>
        /// <param name="toPath">the local location to save the contents of the file (fully-qualifed path)</param>
        /// <param name="isBinary">true if the file is binary</param>
        override public bool GetFile(String fromPath, string toPath, bool isBinary)
        {
            fromPath = Path.Combine(m_path, fromPath);
            FileInfo fromFile = new FileInfo(fromPath);
            FileInfo toFile = new FileInfo(fromPath);
            if (fromFile.Equals(toFile))
            {
                //then the source is the same as the destination, so ignore
                return true;
            }

            try
            {
                // Delete existing file
                if (File.Exists(toPath))
                    File.Delete(toPath);

                // Copy the file
                File.Copy(fromPath, toPath);

                return true;
            }
            catch (Exception e)
            {
                if (RetryOnException(e))
                    return false;
                else
                {
                    SiteDestinationException ex = new SiteDestinationException(e, SiteDestinationException.TransferException, GetType().Name, 0);
                    ex.DestinationExtendedMessage = e.Message;
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Determines whether file transfer operations should retry
        /// given an exception.
        /// </summary>
        /// <param name="e">The exception that has occurred</param>
        /// <returns>True indicates to retry operation, otherwise false</returns>
        public virtual bool RetryOnException(Exception e)
        {
            return false;
        }

        /// <summary>
        /// This method is called to determine whether a directory exists.  This is used to
        /// determine whether to create a new directory or not.
        /// </summary>
        /// <param name="path">The directory</param>
        /// <returns>true indicates the directory exists, false indicates it doesn't</returns>
        public override bool DirectoryExists(string path)
        {
            return Directory.Exists(Path.Combine(m_path, path));
        }

        /// <summary>
        /// This method is called to determine whether a file exists.
        /// </summary>
        /// <param name="Path">The file</param>
        /// <returns>true indicates the file exists, false indicates it doesn't</returns>
        override public bool FileExists(string path)
        {
            return File.Exists(Path.Combine(m_path, path));
        }

        /// <summary>
        /// Deletes a file from the filesysytem.
        /// </summary>
        /// <param name="path">The file</param>
        public override void DeleteFile(string path)
        {
            File.Delete(Path.Combine(m_path, path));
        }

        /// <summary>
        /// Creates a local directory
        /// </summary>
        /// <param name="path">The path of the directory to create (typically
        /// relative to the current directory)</param>
        /// <returns>true indicates success, otherwise false</returns>
        public override void CreateDirectory(string path)
        {
            Directory.CreateDirectory(Path.Combine(m_path, path));
        }

        public override string[] ListDirectories(string path)
        {
            path = Path.Combine(m_path, path);
            string[] dirs = Directory.GetDirectories(path);
            //trim off the parental path information so that only the file name is returned.
            for (int i = 0; i < dirs.Length; i++)
            {
                string directory = dirs[i];
                dirs[i] = directory.Substring(directory.LastIndexOf(@"\", StringComparison.OrdinalIgnoreCase) + 1);
            }
            return dirs;
        }

        /// <summary>
        /// List the names of the files in the specified directory.
        /// </summary>
        /// <param name="path">the directory path</param>
        /// <returns></returns>
        public override string[] ListFiles(string path)
        {
            path = Path.Combine(m_path, path);
            string[] files = Directory.GetFiles(path);
            //trim off the parental path information so that only the file name is returned.
            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i];
                files[i] = file.Substring(file.LastIndexOf(@"\", StringComparison.OrdinalIgnoreCase) + 1);
            }
            return files;
        }
    }
}
