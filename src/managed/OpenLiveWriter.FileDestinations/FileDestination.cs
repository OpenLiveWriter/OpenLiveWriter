// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.IO;
using System.Windows.Forms;

namespace OpenLiveWriter.FileDestinations
{
    /// <summary>
    /// The base class for all file destinations.
    /// </summary>
    abstract public class FileDestination : IDisposable
    {

        /// <summary>
        /// File Destination constructor
        /// </summary>
        /// <param name="path">The path to the destination</param>
        protected FileDestination(string path)
        {
            m_path = path;
            m_retryCount = 5;
            m_retryPause = 1000;
        }

        /// <summary>
        /// File Destination constructor
        /// </summary>
        /// <param name="path">The path to the destination</param>
        /// <param name="retryCount">The number of times to retry a file transfer that has
        /// suffered a non fatal error</param>
        /// <param name="retryPause">The amount of time, in milliseconds, to wait between
        /// retries</param>
        protected FileDestination(string path, int retryCount, int retryPause)
        {
            m_path = path;
            m_retryCount = retryCount;
            m_retryPause = retryPause;
        }

        /// <summary>
        /// Insures that the directory we need to move an item into exists in the destination
        /// </summary>
        /// <param name="path">The directory that we should insure exists</param>
        public void InsureDirectoryExists(string path)
        {
            // empty paths can just be ignored
            if (path == String.Empty)
                return;

            // keep a hashtable of all the directories we've created, if we haven't
            // already created this one
            if (!m_createdDirectories.ContainsKey(path))
            {
                // If the path doesn't exist already, create it
                if (!DirectoryExists(path))
                    CreateDirectory(path);

                // Add this to the hashtable of created paths
                m_createdDirectories.Add(path, true);
            }

        }

        /// <summary>
        /// Transfers the file from a path to a destination
        /// </summary>
        /// <param name="fromPath">The path from which to transfer the file</param>
        /// <param name="toPath">The path to which to transfer the file</param>
        /// <param name="overWrite">Whether or not to overwrite already existing files</param>
        /// <param name="retryCount">How many times to retry the transfer of an individual
        /// file</param>
        /// <param name="retryPause">How long to pause between retry attempts</param>
        /// <param name="status">An IPublisherStatus for status updates</param>
        public void DoTransfer(
            string fromPath,
            string toPath,
            bool overWrite)
        {

            //fully qualify the toPath
            toPath = CombinePath(m_path, toPath);

            int attempts = 0;
            // wait and retry
            while (attempts <= m_retryCount)
            {
                // if it worked, continue, otherwise keep trying
                if (CopyFile(fromPath, toPath, overWrite))
                    break;

                // Wait and then retry copyfile
                attempts++;

                // NOTE: This is not CPU optimized- need to optimize

                // Pause - in case an intermittent connection or something
                // gets restored and allows the retry to suceed
                int startTime = Environment.TickCount;
                while (Environment.TickCount < (startTime + m_retryPause))
                {
                    Application.DoEvents();
                }
            }
            // If we retry and it never works, throw an exception
            if (attempts > m_retryCount)
                throw new SiteDestinationException(
                    null,
                    SiteDestinationException.TransferException,
                    GetType().Name,
                    0);

        }

        /// <summary>
        /// This method is called to make a connection to the destination
        /// </summary>
        /// <returns>True/False - False means that a non fatal error has occurred</returns>
        virtual public void Connect()
        {
        }

        /// <summary>
        /// Combines 2 paths parts together into a full path.
        /// This method is overridable so that subclasses can determine the proper delimiter.
        /// </summary>
        /// <param name="path1"></param>
        /// <param name="path2"></param>
        virtual public string CombinePath(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        /// <summary>
        /// The path delimiter for this destination.
        /// This property is overridable so that subclasses can determine the proper delimiter.
        /// </summary>
        virtual public string PathDelimiter
        {
            get
            {
                return @"\";
            }
        }

        /// <summary>
        /// The path delimiter for this destination.
        /// This property is overridable so that subclasses can determine the proper delimiter.
        /// </summary>
        virtual public char PathDelimiterChar
        {
            get
            {
                return '\\';
            }
        }

        /// <summary>
        /// This method is called to actually copy the files from one destination to another.
        /// </summary>
        /// <param name="FromPath">The path from which to copy files</param>
        /// <param name="ToPath">The path to which to copy files</param>
        /// <param name="overWriteFiles">Whether or not existing files should be overwritten</param>
        /// <returns>True indicates success, False indicates a non fatal error</returns>
        abstract public bool CopyFile(string FromPath, string ToPath, bool overWriteFiles);

        /// <summary>
        /// This method is called to determine whether a directory exists.  This is used to
        /// determine whether to create a new directory or not.
        /// </summary>
        /// <param name="Path">The directory</param>
        /// <returns>true indicates the directory exists, false indicates it doesn't</returns>
        abstract public bool DirectoryExists(string Path);

        /// <summary>
        /// This method is called to determine whether a file exists.
        /// </summary>
        /// <param name="Path">The file</param>
        /// <returns>true indicates the file exists, false indicates it doesn't</returns>
        abstract public bool FileExists(string Path);

        /// <summary>
        /// Deletes a file from the destination location.
        /// </summary>
        /// <param name="Path">The file</param>
        abstract public void DeleteFile(string Path);

        /// <summary>
        /// Retrieves a file from the destination location.
        /// </summary>
        /// <param name="RemotePath">the path of the file on the destination to retrieve</param>
        /// <param name="LocalFile">the local file path to copy the contents of the file</param>
        /// <param name="isBinary">true if the remote file is binary</param>
        /// <returns>true indicates the file was successfully transferred</returns>
        abstract public bool GetFile(String RemotePath, string LocalFile, bool isBinary);

        /// <summary>
        /// Creates a directory based upon a path, not just a directory name.
        /// </summary>
        /// <param name="path">The path to create</param>
        abstract public void CreateDirectory(string path);

        /// <summary>
        /// List the names of the directories in the specified path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        abstract public string[] ListDirectories(string path);

        /// <summary>
        /// List the names of the files in the specified directory.
        /// </summary>
        /// <param name="path">the directory path</param>
        /// <returns></returns>
        abstract public string[] ListFiles(string path);

        /// <summary>
        /// This method is called to disconnect from the destination
        /// </summary>
        /// <returns>True indicates success, False indicates a non fatal error</returns>
        virtual public void Disconnect()
        {
        }

        /// <summary>
        /// The internal representation of the file path
        /// </summary>
        protected string m_path;

        private int m_retryCount;

        private int m_retryPause;

        /// <summary>
        /// Stores a hash table of directories that were created during the transfer
        /// to this file destination.
        /// </summary>
        private Hashtable m_createdDirectories = new Hashtable();
        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                this.Disconnect();
            }
            catch (Exception)
            {
            }
        }

        #endregion
    }
}
