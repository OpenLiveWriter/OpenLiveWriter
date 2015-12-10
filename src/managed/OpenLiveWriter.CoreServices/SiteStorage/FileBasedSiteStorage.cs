// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace OpenLiveWriter.CoreServices
{

    /// <summary>
    /// Implementation of ISiteStorage that uses the file system as a backing store.
    /// </summary>
    public class FileBasedSiteStorage : SiteStorageBase
    {
        /// <summary>
        /// Initialize using the file system path that contains the site. RootFile
        /// must also be specified once it is known.
        /// </summary>
        /// <param name="basePath">File system path that contains the site</param>
        public FileBasedSiteStorage(string basePath)
            : base()
        {
            m_basePath = basePath;
        }

        /// <summary>
        /// Initialize with the specified BasePath and RootFile
        /// </summary>
        /// <param name="basePath">File system path that contains the site</param>
        /// <param name="rootFile">Root file for the web site (e.g. index.htm)</param>
        public FileBasedSiteStorage(string basePath, string rootFile)
            : base(rootFile)
        {
            m_basePath = basePath;
        }

        /// <summary>
        /// Initialize using the file system path that contains the site. RootFile
        /// must also be specified once it is known.
        /// </summary>
        /// <param name="basePath">File system path that contains the site</param>
        public FileBasedSiteStorage(string basePath, SiteStorageFileFilter filter)
            : this(basePath)
        {
            fileFilter = filter;
        }

        /// <summary>
        /// Initialize with the specified BasePath and RootFile
        /// </summary>
        /// <param name="basePath">File system path that contains the site</param>
        /// <param name="rootFile">Root file for the web site (e.g. index.htm)</param>
        public FileBasedSiteStorage(string basePath, string rootFile, SiteStorageFileFilter filter)
            : this(basePath, rootFile)
        {
            fileFilter = filter;
        }

        /// <summary>
        /// File system path that contains the site.
        /// </summary>
        public string BasePath { get { return m_basePath; } }

        /// <summary>
        /// Method called by base class SupportingFiles implementation
        /// </summary>
        /// <returns>New ArrayList containing the names all files in storage</returns>
        protected override ArrayList GetStoredFiles()
        {
            // get the stored files
            ArrayList files = DirectoryHelper.ListRecursive(BasePath, true);

            // apply a filter to them if we have one
            if (fileFilter != null)
            {
                ArrayList filteredFiles = new ArrayList(files.Count);
                foreach (string file in files)
                {
                    if (fileFilter(file.ToLower(CultureInfo.InvariantCulture)))
                        filteredFiles.Add(file);
                }
                return filteredFiles;
            }
            else // no filter, just return list
            {
                return files;
            }
        }

        /// <summary>
        /// Test to see whether the specified file already exists
        /// </summary>
        /// <param name="file">file name</param>
        /// <returns>true if it exists, otherwise false</returns>
        public override bool Exists(string file)
        {
            string osPath = NormalizePath(file);
            string fullPath = Path.Combine(m_basePath, osPath);
            return File.Exists(fullPath);
        }

        /// <summary>
        /// Retrieve a Stream for the given path (Read or Write access can be specified).
        /// Stream.Close() should be called when you are finished using the Stream.
        /// </summary>
        /// <param name="file">Heirarchical path designating stream location (uses "/" as
        /// path designator)</param>
        /// <param name="mode">Read or Write. Write will overwrite any exising path of the
        /// same name.</param>
        /// <returns>Stream that can be used to access the path (Stream.Close() should be
        /// called when you are finished using the Stream).</returns>
        public override Stream Open(string file, AccessMode mode)
        {
            // normalize path (convert separators)
            string osPath = NormalizePath(file);

            // validate the path (throws an exception if it is invalid)
            ValidatePath(osPath);

            switch (mode)
            {
                case AccessMode.Read:
                    return OpenFileStreamForRead(osPath);

                case AccessMode.Write:
                    return OpenFileStreamForWrite(osPath);

                default:
                    Debug.Assert(false, "Invalid AccessMode");
                    return null;
            }
        }

        public void ConvertAllReferencesToAbsolute(string baseUrl)
        {
            foreach (string filePath in Manifest)
            {
                if (filePath != null && (filePath.EndsWith(".htm", StringComparison.OrdinalIgnoreCase) || filePath.EndsWith(".html", StringComparison.OrdinalIgnoreCase)))
                {
                    string path = string.Empty;

                    // split path at directory|file
                    int lastSlash = filePath.LastIndexOf('/');
                    if (lastSlash != -1)
                    {
                        path = filePath.Substring(0, lastSlash + 1);
                    }

                    string output;
                    using (StreamReader reader = new StreamReader(OpenFileStreamForRead(filePath), Encoding.UTF8))
                    {
                        string thisUrl = baseUrl + path;
                        Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "Base url: {0} / {1}", thisUrl, filePath));
                        output = LightWeightHTMLUrlToAbsolute.ConvertToAbsolute(reader.ReadToEnd(), thisUrl);
                    }
                    using (StreamWriter writer = new StreamWriter(OpenFileStreamForWrite(filePath), Encoding.UTF8))
                    {
                        writer.Write(output);
                    }
                }
            }
        }

        /// <summary>
        /// Convert our path separator to the os-specific one
        /// </summary>
        /// <param name="path">path to normalize</param>
        /// <returns>normalized path</returns>
        private string NormalizePath(string path)
        {
            // convert all separators to the OS-specific one
            string osPath = path.Replace('/', Path.DirectorySeparatorChar);
            return osPath.Replace('\\', Path.DirectorySeparatorChar);
        }

        // open a read-only stream to the file at the specified path
        private Stream OpenFileStreamForRead(string path)
        {
            return OpenFileStream(path, FileMode.Open, FileAccess.Read);
        }

        // open a read/write stream to the file at the specified path
        private Stream OpenFileStreamForWrite(string path)
        {
            // create the directory if necessary
            try
            {
                string directory = Path.Combine(m_basePath, Path.GetDirectoryName(path));
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
            }
            catch (Exception e)
            {
                throw new SiteStorageException(
                    e, SiteStorageException.UnableToCreateStorage,
                    Path.Combine(BasePath, path));
            }

            // open the file and return its stream
            return OpenFileStream(path, FileMode.Create, FileAccess.Write);
        }

        // core function for opening a file and checking for errors, etc.
        private Stream OpenFileStream(string path, FileMode mode, FileAccess access)
        {
            // create a fully qualified path to the file
            string fullPath = Path.Combine(m_basePath, path);

            // try to open the file
            try
            {
                return new FileStream(fullPath, mode, access);
            }
            catch (FileNotFoundException e)
            {
                // catch FileNotFound explicitly and convert it to a PathNotFound error
                throw new
                    SiteStorageException(e, SiteStorageException.PathNotFound, fullPath);
            }
            catch (Exception e)
            {
                throw new
                    SiteStorageException(e, SiteStorageException.PathAccessError, fullPath);
            }
        }

        // storage for root file name
        private string m_rootFile = String.Empty;

        // path where the site's files are stored
        private string m_basePath;

        // optional file-filter used to determine which files are included and excluded
        private SiteStorageFileFilter fileFilter = null;
    }

    /// <summary>
    /// Delegate used for filtering the manifest of a FileBasedSiteStorage.
    /// The name of the file will always be lower case to aid in case-
    /// insensitive comparisions.
    /// (return true to include the file, otherwise return false)
    /// </summary>
    public delegate bool SiteStorageFileFilter(string fileRelativePath);
}
