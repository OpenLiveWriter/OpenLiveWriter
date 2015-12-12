// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.IO;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Helper base class which provides implementations for RootFile and SupportingFiles.
    /// Derived classes are required to implement a GetStoredFiles method that returns
    /// a list of all files in storage (in no particular order)
    /// </summary>
    public abstract class SiteStorageBase : ISiteStorage
    {
        /// <summary>
        /// Do-nothing default constructor (initialize without specifying a RootFile).
        /// The RootFile must eventually be specified in order for the site to be valid.
        /// </summary>
        protected SiteStorageBase()
        {
        }

        /// <summary>
        /// Initialize with the specified RootFile
        /// </summary>
        /// <param name="rootFile">Root file for the web site (e.g. index.htm)</param>
        protected SiteStorageBase(string rootFile)
        {
            RootFile = rootFile;
        }

        /// <summary>
        /// Root file for the web site (e.g. index.htm). This value must be
        /// set for a site to be valid.
        /// </summary>
        public string RootFile
        {
            get
            {
                // verify that RootFile has been set
                if (m_rootFile == null)
                    throw new SiteStorageException(
                        null, SiteStorageException.NoRootFileSpecified);

                // return the value
                return m_rootFile;
            }

            set
            {
                // verify that the value is valid
                if (value == null || value.Length == 0 || value.IndexOf('/') != -1)
                    throw new SiteStorageException(
                        null, SiteStorageException.InvalidRootFileName, value);

                // set the value
                m_rootFile = value;
            }
        }

        /// <summary>
        /// Files contained within the site. Path sub-directories
        /// are separated by the "/" character. The listing will start with the
        /// RootFile and will then be ordered accoring to the FileListingComparer
        /// implementation of IComparer (standard recursive directory listing).
        /// </summary>
        /// <returns>List of the files contained within the site (represented as
        /// strings)</returns>
        public ArrayList Manifest
        {
            get
            {
                // get a list of files contained within storage
                // (GetStoredFiles is a virtual method implemented by subclasses)
                ArrayList files = GetStoredFiles();

                // remove the root file prior to sorting (will throw an exception
                // if there is no root file at this point)
                files.Remove(RootFile);

                // sort the list using the FileListingComparer
                files.Sort(m_comparer);

                // add the root file back in at the front of the list
                files.Insert(0, RootFile);

                // return an enumeration to the files in storage
                return files;
            }
        }

        /// <summary>
        /// Method implemented by subclasses to return their list of files (including
        /// their root file). The method should not return a reference to any internal
        /// data structures as the caller will modify the returned ArrayList.
        /// The SiteStorageBase implementation of the SupportingFiles property is built
        /// on top of this virtual method.
        /// </summary>
        /// <returns>New ArrayList containing the names all files in storage</returns>
        protected abstract ArrayList GetStoredFiles();

        /// <summary>
        /// Test to see whether the specified file already exists
        /// </summary>
        /// <param name="file">file name</param>
        /// <returns>true if it exists, otherwise false</returns>
        public abstract bool Exists(string file);
        // defer implementation to subclasses

        /// <summary>
        /// Retrieve a Stream for the given path (Read or Write access can be specified)
        /// Stream.Close() should be called when you are finished using the Stream.
        /// Imlementation of Open is deferred to subclasses.
        /// </summary>
        /// <param name="path">Heirarchical path designating stream location (uses "/" as
        /// path designator)</param>
        /// <param name="mode">Read or Write. Write will overwrite any exising path of
        /// the same name.</param>
        /// <returns>Stream that can be used to access the path (Stream.Close() must be
        /// called when you are finished using the Stream).</returns>
        public abstract Stream Open(string path, AccessMode mode);
        // defer implementation to subclasses

        /// <summary>
        /// Helper function used to validate that a specified path string has a
        /// valid format. Throws a SiteStorageException of type InvalidPath if
        /// the path is not valid.
        /// </summary>
        /// <param name="path"></param>
        protected static void ValidatePath(string path)
        {
            // validate the path
            if (path.IndexOfAny(pathInvalid) != -1 ||
                path.IndexOfAny(mimeInvalid) != -1)
            {
                throw new SiteStorageException(null,
                    SiteStorageException.InvalidPath, path);
            }
        }
        // platform-specific invalid path characters
        static char[] pathInvalid = Path.GetInvalidPathChars();
        // other invalid chars (parens are invalid in MIME Content-Location headers)
        static char[] mimeInvalid = new char[] { };
        // both ( and ) were protected, but I couldn't replicate a problem w/parens in Content headers.

        // storage for root file name
        private string m_rootFile = null;

        // static FileListingComparer for use in sorting file listings
        private static FileListingComparer m_comparer = new FileListingComparer();
    }

    /// <summary>
    /// Implementation of IComparer used to implement a recursive file listing of
    /// a directory and its subdirectories. Code is a bit tricky but there are
    /// no straightforward heuristics for this comparison. We take care of simple
    /// cases first and then get into the nitty gritty of splitting up paths
    /// and distinguishing between file and directory entries, etc.
    /// </summary>
    public class FileListingComparer : IComparer
    {
        /// <summary>
        /// Implemet Compare method
        /// </summary>
        /// <param name="x">left hand side</param>
        /// <param name="y">right hand side</param>
        /// <returns></returns>
        public int Compare(object x, object y)
        {
            // grab the strings
            string left = x.ToString();
            string right = y.ToString();

            // optimize for common-case: one or both of the entries is a root file entry
            int leftSlash = left.IndexOf('/');
            int rightSlash = right.IndexOf('/');
            if (leftSlash == -1 && rightSlash == -1)
                return String.Compare(left, right, StringComparison.OrdinalIgnoreCase); // both are root entries
            else if (leftSlash == -1)
                return -1;  // left is a root entry and right is not
            else if (rightSlash == -1)
                return 1;  // right is a root entry and left is not

            // both paths are in subdirectories off the root, parse out the
            // individual sub-paths in each full path
            char[] separators = new char[] { '/' };
            string[] leftPaths = left.Split(separators);
            string[] rightPaths = right.Split(separators);

            // find the lowest shared level
            int searchLevels = Math.Min(leftPaths.Length, rightPaths.Length);
            int level = 0;
            while (level < searchLevels)
            {
                if (leftPaths[level] == rightPaths[level])
                {
                    // if both paths are at the end, that is the lowest shared level
                    if (leftPaths.Length == (level + 1) && rightPaths.Length == (level + 1))
                        break;

                    else  // otherwise go to the next level
                        level++;
                }
                else // we are the last level for one of the two paths
                    break;
            }

            // with the lowest shared level established, we have a basis for comparison
            int sharedLevel = level;

            // see if one of them is out of levels and the other has additional ones
            // (case where a file name is the same as a directory name -- illegal in
            // most file systems, but technically not impossible)
            if (leftPaths.Length == sharedLevel && rightPaths.Length > sharedLevel)
                return -1;
            else if (rightPaths.Length == sharedLevel && leftPaths.Length > sharedLevel)
                return 1;

            // both share paths up to their last level, compare the last level
            // (case where both are file entries)
            else if (leftPaths.Length == (sharedLevel + 1) && rightPaths.Length == (sharedLevel + 1))
                return String.Compare(leftPaths[sharedLevel], rightPaths[sharedLevel], StringComparison.OrdinalIgnoreCase);

            // left is a file entry and right has additional directories
            else if (leftPaths.Length == (sharedLevel + 1))
                return -1;

            // right is a file entry and left has additional directories
            else if (rightPaths.Length == (sharedLevel + 1))
                return 1;

            // they are both directories w/ varying content after the shared level,
            // compare them at the level just after the shared one
            else
                return String.Compare(leftPaths[sharedLevel], rightPaths[sharedLevel], StringComparison.OrdinalIgnoreCase);
        }
    }

}
