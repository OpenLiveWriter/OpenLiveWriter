// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.IO;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Implementation of FileBasedSiteStorage that uses a temporary directory for
    /// the storage. The caller can choose between the system temp path or a custom
    /// temp path. Implements IDisposable and recursively deletes the contents of the
    /// directory when Dispose is called.
    /// </summary>
    public class TempFileSiteStorage : FileBasedSiteStorage, IDisposable
    {
        /// <summary>
        /// Create a TempFileSiteStorage using the specified directory name prefix.
        /// RootFile must also be specified once it is known.
        /// </summary>
        public TempFileSiteStorage()
            : base(TempFileManager.Instance.CreateTempDir())
        {
        }

        /// <summary>
        /// Create a TempFileSiteStorage using the specified directory name prefix
        /// and RootFile.
        /// </summary>
        /// <param name="rootFile">Root file for the web site (e.g. index.htm)</param>
        public TempFileSiteStorage(string rootFile)
            : base(TempFileManager.Instance.CreateTempDir(), rootFile)
        {
        }

        /// <summary>
        /// Create a temp file stite storage from an existing directory
        /// with the specified root file (directory will be deleted upon disposal)
        /// </summary>
        /// <param name="existingDirectory">existing directory</param>
        /// <param name="rootFile">root file</param>
        public TempFileSiteStorage(string existingDirectory, string rootFile)
            : base(existingDirectory, rootFile)
        {
        }

        /// <summary>
        /// Create a TempFileSiteStorage using the specified directory name prefix.
        /// RootFile must also be specified once it is known.
        /// </summary>
        public TempFileSiteStorage(SiteStorageFileFilter filter)
            : base(TempFileManager.Instance.CreateTempDir(), filter)
        {
        }

        /// <summary>
        /// Create a TempFileSiteStorage using the specified directory name prefix
        /// and RootFile.
        /// </summary>
        /// <param name="rootFile">Root file for the web site (e.g. index.htm)</param>
        public TempFileSiteStorage(string rootFile, SiteStorageFileFilter filter)
            : base(TempFileManager.Instance.CreateTempDir(), rootFile, filter)
        {
        }

        /// <summary>
        /// Create a temp file stite storage from an existing directory
        /// with the specified root file (directory will be deleted upon disposal)
        /// </summary>
        /// <param name="existingDirectory">existing directory</param>
        /// <param name="rootFile">root file</param>
        public TempFileSiteStorage(string existingDirectory, string rootFile, SiteStorageFileFilter filter)
            : base(existingDirectory, rootFile, filter)
        {
        }

        /// <summary>
        /// Recursively delete all files contained in the temporary directory
        /// </summary>
        public void Dispose()
        {
            try
            {
                DeleteFiles(BasePath);
            }
            catch (Exception e)
            {
                Trace.Fail("Unexpected error attempting to delete temp site storage: " + e.ToString());
            }
        }

        // Helper function to delete files beneath the specified path
        private static void DeleteFiles(string path)
        {
            try
            {
                // recursively delete all of the site's files and directories
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
            }
            catch (Exception e)
            {
                throw new SiteStorageException(
                    e, SiteStorageException.UnableToDeleteSite, path);
            }
        }

        // Helper function to generate a temporary site path name
        private static string GetTemporarySitePath(string prefix, string tempDirectory)
        {
            // generate the path name
            string siteDir = prefix + Guid.NewGuid().ToString();
            string sitePath = Path.Combine(tempDirectory, siteDir);

            // attempt to create the directory then return the path if successful
            try
            {
                Directory.CreateDirectory(sitePath);
                return sitePath;
            }
            catch (Exception e)
            {
                throw new SiteStorageException(e,
                    SiteStorageException.UnableToCreateStorage, sitePath);
            }
        }

    }
}
