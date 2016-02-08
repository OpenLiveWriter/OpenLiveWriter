// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections;
using System.IO;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Generic interface for storing a collection of site data represented by
    /// heirarchical path designations. Paths should be separated using the "/"
    /// character. This class and its subclasses all throw SiteStorageException
    /// based exceptions. See the documentation for that class for an enumeration
    /// of exception types.
    /// </summary>
    public interface ISiteStorage
    {
        /// <summary>
        /// Root file for the web site (e.g. index.htm). This value must
        /// be set for a site to be valid.
        /// </summary>
        string RootFile { get; set; }

        /// <summary>
        /// Files contained within the site. Path sub-directories
        /// are separated by the "/" character. The listing will start with the
        /// RootFile and will then be ordered accoring to the FileListingComparer
        /// implementation of IComparer (standard recursive directory listing).
        /// </summary>
        ArrayList Manifest { get; }

        /// <summary>
        /// Test to see whether the specified file already exists
        /// </summary>
        /// <param name="file">file name</param>
        /// <returns>true if it exists, otherwise false</returns>
        bool Exists(string file);

        /// <param name="file">Heirarchical path designating stream location (uses "/"
        /// as path designator)</param>
        /// <param name="mode">Read or Write. Write will overwrite any exising path of
        /// the same name.</param>
        /// <returns>Stream that can be used to access the path (Stream.Close() should
        /// be called when you are finished using the Stream).</returns>
        Stream Open(string file, AccessMode mode);
    }

    /// <summary>
    /// Modes that can be used for accessing a site path
    /// </summary>
    public enum AccessMode
    {
        /// <summary>
        /// Read access to the path
        /// </summary>
        Read,

        /// <summary>
        /// Write access to the path
        /// </summary>
        Write
    };

}
