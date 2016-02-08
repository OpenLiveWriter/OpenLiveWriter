// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Settings;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.FileDestinations
{
    /// <summary>
    /// Summary description for FTPFileDestination.
    /// </summary>
    public class WinInetFTPFileDestination : FileDestination
    {

        /// <summary>
        /// FTP Destination constructor, uses default FTP port.
        /// </summary>
        /// <param name="ftpAddress">The IP Address / Network name of the FTP destination</param>
        /// <param name="targetDirectory">The target directory (/ delimited path) on the server</param>
        /// <param name="username">The username to use to login to the server</param>
        /// <param name="password">The password to use to login to the server</param>
        public WinInetFTPFileDestination(string ftpAddress, string targetDirectory,
            string username, string password) : this(GetServerName(ftpAddress),
            GetServerPort(ftpAddress), targetDirectory, username, password)
        {
        }

        /// <summary>
        /// FTP Destination constructor, uses default FTP port.
        /// </summary>
        /// <param name="ftpAddress">The IP Address / Network name of the FTP destination</param>
        /// <param name="targetDirectory">The target directory (/ delimited path) on the server</param>
        /// <param name="username">The username to use to login to the server</param>
        /// <param name="password">The password to use to login to the server</param>
        /// <param name="retryCount">The number of times to retry a file transfer that has
        /// suffered a non fatal error</param>
        /// <param name="retryPause">The amount of time, in milliseconds, to wait between
        /// retries</param>
        public WinInetFTPFileDestination(string ftpAddress, string targetDirectory,
            string username, string password, int retryCount, int retryPause) :
            this(GetServerName(ftpAddress), GetServerPort(ftpAddress), targetDirectory,
            username, password, retryCount, retryPause)
        {
        }

        /// <summary>
        /// FTP Destination constructor, specify FTP port.
        /// </summary>
        /// <param name="ftpAddress">The IP Address / Network name of the FTP destination</param>
        /// <param name="ftpPort">The port number of the FTP destination</param>
        /// <param name="targetDirectory">The target directory (/ delimited path) on the server</param>
        /// <param name="username">The username to use to login to the server</param>
        /// <param name="password">The password to use to login to the server</param>
        public WinInetFTPFileDestination(
            string ftpAddress,
            int ftpPort,
            string targetDirectory,
            string username,
            string password) : base(targetDirectory)
        {
            m_userName = username;
            m_passWord = password;
            m_ftpAddress = ftpAddress;
            m_ftpPort = ftpPort;
        }

        /// <summary>
        /// FTP Destination constructor, specify FTP port.
        /// </summary>
        /// <param name="ftpAddress">The IP Address / Network name of the FTP destination</param>
        /// <param name="ftpPort">The port number of the FTP destination</param>
        /// <param name="targetDirectory">The target directory (/ delimited path) on the server</param>
        /// <param name="username">The username to use to login to the server</param>
        /// <param name="password">The password to use to login to the server</param>
        /// <param name="retryCount">The number of times to retry a file transfer that has
        /// suffered a non fatal error</param>
        /// <param name="retryPause">The amount of time, in milliseconds, to wait between
        /// retries</param>
        public WinInetFTPFileDestination(
            string ftpAddress,
            int ftpPort,
            string targetDirectory,
            string username,
            string password,
            int retryCount,
            int retryPause) : base(targetDirectory, retryCount, retryPause)
        {
            m_userName = username;
            m_passWord = password;
            m_ftpAddress = ftpAddress;
            m_ftpPort = ftpPort;
        }

        /// <summary>
        /// Connects to the FTP server
        /// </summary>
        public override void Connect()
        {

            // Init wininet and get the base handle
            m_hInternet = WinInet.InternetOpen(
                null,                           // The 'user-agent' making the request
                INTERNET_OPEN.TYPE_PRECONFIG,   // Use the default proxy settings
                null,                           // proxy name (no proxy)
                null,                           // proxy bypass (no bypass)
                0                               // Flags for the connection
                );

            // Validate the connection and throw an exception if necessary
            ValidateConnection(m_hInternet);

            // Get a FTP handle to use to put files
            m_hFtp = WinInet.InternetConnect(
                m_hInternet,                    // The internet connection to use
                m_ftpAddress,
                m_ftpPort,                      // The FTP port number
                m_userName,
                m_passWord,
                INTERNET_SERVICE.FTP,           // Open the connection for this type of service
                FtpConnectionFlags,             // FTP Flags
                0                               // Application Context
                );

            // Validate the handle and throw an exception if necessary
            ValidateConnection(m_hFtp);

            //save the home dir
            HomeDir = WinInet.GetFtpCurrentDirectory(m_hFtp);

            // Set the current directory to the path.  If the path is empty, don't set
            // the current directory as this is invalid.
            if (m_path != "")
            {
                if (!m_path.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    string homeDir = HomeDir;
                    if (!homeDir.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                        homeDir = homeDir + "/";
                    m_path = homeDir + m_path + "/";
                }

                if (!m_path.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    //Mindspring's FTP behaves strangely if the current
                    //directory isn't set with a trailing "/"
                    m_path += "/";
                }

                if (!RawDirectoryExists(m_path))
                {
                    if (!(HomeDir != null && HomeDir.StartsWith(m_path, StringComparison.OrdinalIgnoreCase) &&
                        m_path != "" && m_path.EndsWith("/", StringComparison.OrdinalIgnoreCase)))
                        throw new NoSuchDirectoryException(m_path);
                }

                // Some FTP servers don't let you cwd to /, but we
                // still need to use it as our m_path.
                //
                // ValidateFTPCommand(pushDirectory(m_path));
            }
            else
            {
                //remember what the default working directory assigned by the server was.
                m_path = HomeDir;
            }

        }

        /// <summary>
        /// Copy a file to the FTP destination
        /// </summary>
        /// <param name="from">The path from which to copy the file</param>
        /// <param name="to">The file to which to copy the file</param>
        /// <param name="overWrite">Ignored- files are always overwritten on the FTP server</param>
        /// <returns>true if the copy succeeded, otherwise false</returns>
        public override bool CopyFile(string from, string to, bool overWrite)
        {

            // Validate that the file transferred, throw an exception if necessary
            // Non fatal errors will allow putFile = false to pass through without
            // throwing an exception.  That will pass that CopyFile failed up
            // up the chain, allowing any retry logic to execute.

            // NOTE: This should respect the bool overWrite.  Right now, it just
            // blows over the top
            return ValidateFileTransfer(WinInet.FtpPutFile(
                m_hFtp,
                from,
                to,
                0,              // FTP Flags (none necessary)
                0               // Application Context
                ));
        }

        /// <summary>
        /// Retrieves a file from the destination location.
        /// </summary>
        /// <param name="RemotePath">The path of the file to get from the FTP server</param>
        /// <param name="LocalFile">The path of the local file to copy the file contents to</param>
        /// <param name="isBinary">True if the remote file is binary</param>
        override public bool GetFile(String RemotePath, string LocalFile, bool isBinary)
        {
            uint dwFlags = isBinary ? INTERNET_FLAG.TRANSFER_BINARY : INTERNET_FLAG.TRANSFER_ASCII;
            return WinInet.FtpGetFile(m_hFtp, RemotePath, LocalFile, false, (int)FileAttributes.Normal, (int)dwFlags, 0);
        }

        /// <summary>
        /// Combines 2 paths parts together into a full path.
        /// This method is overridable so that subclasses can determine the proper delimiter.
        /// </summary>
        /// <param name="path1"></param>
        /// <param name="path2"></param>
        override public string CombinePath(string path1, string path2)
        {
            if (!path1.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                path1 += "/";
            }

            //if path2 has leading slashes, yank them
            path2 = path2.TrimStart(new char[] { '/' });

            return path1 + path2;
        }

        /// <summary>
        /// The path delimiter for this destination.
        /// </summary>
        override public string PathDelimiter
        {
            get
            {
                return "/";
            }
        }

        /// <summary>
        /// The path delimiter for this destination.
        /// </summary>
        override public char PathDelimiterChar
        {
            get
            {
                return '/';
            }
        }

        /// <summary>
        /// This method is called to determine whether a directory exists.  This is used to
        /// determine whether to create a new directory or not.
        /// </summary>
        /// <param name="Path">the relative directory</param>
        /// <returns>true indicates the directory exists, false indicates it doesn't</returns>
        public override bool DirectoryExists(string path)
        {
            path = CombinePath(m_path, path);
            return RawDirectoryExists(path);
        }

        private bool RawDirectoryExists(string path)
        {
            // Try to set the FTP directory to the Path.  This will return false
            // if the the set directory didn't work (b/c the directory doesn't exist
            // or b/c of some other error).  This will mask any error other than the
            // directory already existing.
            bool dirExists = pushDirectory(path);

            // If we did set the directory to the Path, we need to reset the
            // directory to the old root path.
            if (dirExists)
            {
                popDirectory();
            }
            return dirExists;
        }

        /// <summary>
        /// This method is called to determine whether a file exists.
        /// </summary>
        /// <param name="path">The file</param>
        /// <returns>true indicates the file exists, false indicates it doesn't</returns>
        override public bool FileExists(string path)
        {
            path = CombinePath(m_path, path);
            string fname = path;
            string dir = "";

            int index = path.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
            if (index != -1)
            {
                fname = path.Substring(index + 1);
                dir = path.Substring(0, index);
            }
            if (pushDirectory(dir))
            {
                try
                {
                    Win32_Find_Data[] files = WinInet.FtpFind(m_hFtp, fname);
                    if (files.Length > 0)
                    {
                        return true;
                    }
                }
                finally
                {
                    popDirectory();
                }
            }
            return false;
        }

        /// <summary>
        /// Creates a directory on the FTP server
        /// </summary>
        /// <param name="path">The path of the directory to create (typically
        /// relative to the current directory)</param>
        public override void CreateDirectory(string path)
        {
            path = CombinePath(m_path, path);
            RawCreateDirectory(path);
        }

        private void RawCreateDirectory(string path)
        {
            int index = path.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
            if (index == -1)
            {
                path = "/" + path;
                index = 0;
            }
            string baseDir = path.Substring(0, index);
            string dirName = path.Substring(index + 1);
            bool pushedBaseDir = false;
            if (baseDir.Trim() != String.Empty) //fix bug 4886
            {
                pushedBaseDir = pushDirectory(baseDir);
                if (!pushedBaseDir)
                {
                    RawCreateDirectory(baseDir);
                    ValidateFTPCommand(pushDirectory(baseDir)); ////fix bug 4886 - avoid stackoverflow if directory was not properly pushed.
                    pushedBaseDir = true;
                }
            }

            ValidateFTPCommand(WinInet.FtpCreateDirectory(m_hFtp, dirName));

            if (pushedBaseDir)
                popDirectory();
        }

        public string getCurrentDirectory()
        {
            return WinInet.GetFtpCurrentDirectory(m_hFtp);
        }

        /// <summary>
        /// Deletes a file on the FTP server
        /// </summary>
        /// <param name="path">The path of the file to delete (typically
        /// relative to the current directory)</param>
        public override void DeleteFile(string path)
        {
            path = CombinePath(m_path, path);
            ValidateFTPCommand(WinInet.FtpDeleteFile(m_hFtp, path));
        }

        public override string[] ListDirectories(string path)
        {
            path = CombinePath(m_path, path);
            if (!path.EndsWith("/", StringComparison.OrdinalIgnoreCase)) path += "/";
            ArrayList dirList = new ArrayList();

            //note: some FTP servers have problems listing directories with spaces in them,
            //so we need to change the directory and then do a simple listing instead
            if (pushDirectory(path))
            {
                try
                {
                    Win32_Find_Data[] findData = WinInet.FtpFind(m_hFtp, "*.*");
                    for (int i = 0; i < findData.Length; i++)
                    {
                        if ((findData[i].dwFileAttributes & (int)FileAttributes.Directory) > 0)
                        {
                            dirList.Add(findData[i].cFileName);
                        }
                    }
                }
                finally
                {
                    popDirectory();
                }
            }
            return (string[])ArrayHelper.CollectionToArray(dirList, typeof(string));
        }

        /// <summary>
        /// List the files in the specified directory.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public override string[] ListFiles(string path)
        {
            path = CombinePath(m_path, path);
            if (!path.EndsWith("/", StringComparison.OrdinalIgnoreCase)) path += "/";
            ArrayList dirList = new ArrayList();

            //note: some FTP servers have problems listing directories with spaces in them,
            //so we need to change the directory and then do a simple listing instead
            if (pushDirectory(path))
            {
                try
                {
                    Win32_Find_Data[] findData = WinInet.FtpFind(m_hFtp, "*.*");
                    for (int i = 0; i < findData.Length; i++)
                    {
                        if ((findData[i].dwFileAttributes & (int)FileAttributes.Directory) == 0)
                        {
                            dirList.Add(findData[i].cFileName);
                        }
                    }
                }
                finally
                {
                    popDirectory();
                }
            }
            return (string[])ArrayHelper.CollectionToArray(dirList, typeof(string));
        }

        /// <summary>
        /// Enables/disables the use of passive FTP.  If false, then active FTP will be used.
        /// </summary>
        public bool UsePassiveFTP
        {
            get
            {
                return _passiveFTP;
            }
            set
            {
                _passiveFTP = value;
            }
        }
        bool _passiveFTP = DefaultUsePassiveFTP;

        /// <summary>
        /// The WinInet flags that are used when connecting.
        /// </summary>
        private int FtpConnectionFlags
        {
            get
            {
                uint flags = 0;
                if (UsePassiveFTP)
                {
                    flags = flags | INTERNET_FLAG.PASSIVE;
                }
                return (int)flags;
            }
        }

        /// <summary>
        /// By default, destinations will use the same Passive FTP settings as Internet Explorer.
        /// If use passive FTP is enabled in the Internet Explorer advanced settings, then the default
        /// is true.
        /// </summary>
        private static bool DefaultUsePassiveFTP
        {
            get
            {
                string pasv = "no";
                try
                {
                    RegistrySettingsPersister reg = new RegistrySettingsPersister(Registry.CurrentUser, @"Software\Microsoft\Ftp");
                    using (reg)
                    {
                        pasv = (string)reg.Get("Use PASV", typeof(string), "no");
                    }
                }
                catch (Exception e)
                {
                    Debug.Fail("failed to read FTP Settings key: " + @"Software\Microsoft\Ftp\Use PASV", e.Message);
                }
                if (pasv.ToUpperInvariant().Equals("YES"))
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Disconnect from the FTP Server
        /// </summary>
        /// <returns>true if disconnected succeeded, otherwise false</returns>
        public override void Disconnect()
        {
            // Close the handles
            if (!WinInet.InternetCloseHandle(m_hFtp))
            {
                // ignore as this can commonly happen if the connection
                //was never properly established
            }
            else if (!WinInet.InternetCloseHandle(m_hInternet))
            {
                // If the handles didn't close, this is very unexpected and bad
                Debug.Assert(false,
                    "Disconnect couldn't close the Internet handle in " + GetType().ToString());
            }
        }

        /// <summary>
        /// Validates that a WinInet connection was successfully made.  If the connection
        /// wasn't successful, this will throw a SiteDestination exception of the correct
        /// type and include corresponding details describing the cause of the exception.
        /// </summary>
        /// <param name="connectionHandle">IntPtr to the Connection Handle</param>
        private bool ValidateConnection(IntPtr connectionHandle)
        {
            // validate that there wasn't an exception
            if (connectionHandle == IntPtr.Zero)
            {
                // Throw the correct destination exception for this
                // type of WinInet error code
                ThrowDestinationException(Marshal.GetLastWin32Error());

            }

            return true;
        }

        ///<summary>
        /// Validates that a WinInet file transfer was successfully made.  If the transfer
        /// wasn't successful, this will attempt to determine the cause and either return false
        /// indicating that the file transfer should be retried, or it will throw a SiteDestination
        /// exception of the correct type and include corresponding details describing the
        /// cause of the exception.
        /// </summary>
        /// <param name="didTransfer">Bool indicating the result of the file transfer</param>
        private bool ValidateFileTransfer(bool didTransfer)
        {
            // If the file transfer failed, check to see if the failure was
            // fatal.
            if (!didTransfer)
            {
                // Get the error code for the last error
                int error = Marshal.GetLastWin32Error();

                // NOTE: Retry logic can be improved by handling connection errors
                // and retrying the connection, i.e.
                // error == ERROR_INTERNET.CONNECTION_ABORTED ||
                // error == ERROR_INTERNET.CONNECTION_RESET ||

                // If we receive any of the below exceptions, we should retry
                // so throw no exception
                if (error != ERROR_INTERNET.TIMEOUT &&
                    error != ERROR_INTERNET.OPERATION_CANCELLED &&
                    error != ERROR_INTERNET.FORCE_RETRY)
                    // Throw the correct destination exception for this
                    // type of WinInet error code
                    ThrowDestinationException(error);
            }
            return didTransfer;
        }

        ///<summary>
        /// Validates that a WinInet FTP command was successfully executed.  If the command
        /// wasn't successful this will throw a SiteDestination exception of the correct type
        /// and include corresponding details describing the cause of the exception.
        /// </summary>
        /// <param name="commandSucceeded">Bool indicating the result of the FTP command</param>
        private void ValidateFTPCommand(bool commandSucceeded)
        {
            if (!commandSucceeded)
            {
                // Throw the correct destination exception for this
                // type of WinInet error code
                ThrowDestinationException(Marshal.GetLastWin32Error());
            }
        }

        /// <summary>
        /// Throws the correct Site Destination exception for a given WinInet Error Code.
        /// This attempts to map common WinInet errors to corresponding Destination errors.
        /// </summary>
        /// <param name="error">The WinInet error code</param>
        private void ThrowDestinationException(int error)
        {

            // Exception detail data
            string exceptionType = null;

            string extendedMessage = WinInet.GetExtendedInfo();
            if (extendedMessage == null || extendedMessage == String.Empty)
                extendedMessage = "The source of the error is unknown.";

            SiteDestinationException exception = null;

            // This wraps the lower level WinInet errors into higher level
            // Site destination exceptions.  It also gathers extened info from
            // the connection to assist with troubleshooting.
            switch (error)
            {
                case ERROR_INTERNET.CANNOT_CONNECT:
                    exceptionType = SiteDestinationException.ConnectionException;
                    break;
                case ERROR_INTERNET.NAME_NOT_RESOLVED:
                    exceptionType = SiteDestinationException.ConnectionException;
                    break;
                case ERROR_INTERNET.ITEM_NOT_FOUND:
                    exceptionType = SiteDestinationException.TransferException;
                    break;

                case ERROR_INTERNET.TIMEOUT:
                    exceptionType = SiteDestinationException.TransferException;
                    break;

                case ERROR_INTERNET.LOGIN_FAILURE:
                    exception = new LoginException(null, error);
                    break;

                case ERROR_INTERNET.INCORRECT_PASSWORD:
                    exception = new LoginException(null, error);
                    break;

                case ERROR_INTERNET.INCORRECT_USER_NAME:
                    exception = new LoginException(null, error);
                    break;

                case ERROR_INTERNET.EXTENDED_ERROR:
                    exceptionType = SiteDestinationException.UnexpectedException;
                    break;
                default:
                    exceptionType = SiteDestinationException.UnexpectedException;
                    break;
            }

            // Set up a sitedestination exception with the correct information
            if (exception == null)
            {
                exception = new SiteDestinationException(
                    null,
                    exceptionType,
                    GetType().Name,
                    error);
            }
            exception.DestinationErrorCode = error;
            exception.DestinationType = GetType().Name;
            exception.DestinationExtendedMessage = extendedMessage;

            // throw it
            throw exception;
        }

        /// <summary>
        /// Pushes a directory change onto the stack if a directory change is successful.
        /// This operation should be used to change directories because some FTP server
        /// behave strangely when performing operations outside of the target directory.
        /// This stack behavior makes it very easy for operations to transparently change
        /// directories and then return to the previous directory.
        /// </summary>
        /// <param name="path">the directory to change to</param>
        /// <returns>true if the directory change was successful</returns>
        private bool pushDirectory(string path)
        {
            Trace.Assert(path.StartsWith(PathDelimiter, StringComparison.OrdinalIgnoreCase), "pushDirectory called with an unexpected (relative) path: " + path);
            if (pathStack.Count > 0 && pathStack.Peek().Equals(path))
            {
                //then we're already in the specified directory, so just push
                //the directory onto the stack and return true.
                //This optimization prevents needless CWD network I/O.
                pathStack.Push(path);
                return true;
            }

            bool success = WinInet.FtpSetCurrentDirectory(m_hFtp, path);
            if (success) pathStack.Push(path);
            return success;
        }

        /// <summary>
        /// Pops a directory change off of the stack and returns to the previous working directory.
        /// </summary>
        private void popDirectory()
        {
            string currPath = (string)pathStack.Pop();
            string newPath = pathStack.Count > 0 ? (string)pathStack.Peek() : m_path;
            if (!currPath.Equals(newPath))//This optimization prevents needless CWD network I/O.
            {
                WinInet.FtpSetCurrentDirectory(m_hFtp, newPath);
            }
        }

        private static string GetServerName(string ftpAddress)
        {
            Uri ftpUri = CreateFtpUri(ftpAddress);
            return ftpUri.Host;
        }

        private static int GetServerPort(string ftpAddress)
        {
            Uri ftpUri = CreateFtpUri(ftpAddress);
            return ftpUri.Port;
        }

        private static Uri CreateFtpUri(string ftpAddress)
        {
            if (ftpAddress == null)
                throw new ArgumentNullException("ftpAddress");
            if (!ftpAddress.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase))
            {
                ftpAddress = "ftp://" + ftpAddress;
            }
            Uri ftpUri = new Uri(ftpAddress);
            if (ftpUri.Scheme.ToUpperInvariant() != "FTP")
            {
                throw new ArgumentException("unsupported FTP address: " + ftpAddress);
            }
            return ftpUri;
        }

        /// <summary>
        /// The ftp username
        /// </summary>
        private string m_userName;

        /// <summary>
        /// The ftp password
        /// </summary>
        private string m_passWord;

        /// <summary>
        /// The ftp server address
        /// </summary>
        private string m_ftpAddress;

        /// <summary>
        /// The ftp port number.  If not provided in the constructor, defaulted
        /// to the WinInet default port number.
        /// </summary>
        private int m_ftpPort;

        /// <summary>
        /// The base handle to an internet connection
        /// </summary>
        private IntPtr m_hInternet;

        /// <summary>
        /// The FTP handle, based upon the internet handle
        /// </summary>
        private IntPtr m_hFtp;

        /// <summary>
        /// Saves the directories that commands are changing to/from
        /// </summary>
        private Stack pathStack = new Stack();

        public string HomeDir;
    }
}
