// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Exceptions for ISiteStorage
    /// </summary>
    public class SiteStorageException : ResourceFileException
    {
        /// <summary>
        /// SiteStorageException constructor
        /// </summary>
        /// <param name="innerException">any caught exception to be kept in the exception chain</param>
        /// <param name="exceptionType">The type of Exception- see SiteStorageException members for
        /// exception types</param>
        /// <param name="arguments">Any exception type specific arguments</param>
        public SiteStorageException(Exception innerException, string exceptionType,
            params object[] arguments) : base(innerException, typeof(SiteStorageException), exceptionType, arguments)
        {
        }

        // valid exception types

        /// <summary>
        /// A RootFile was not specified for the site
        /// </summary>
        public static string NoRootFileSpecified = "NoRootFileSpecified";

        /// <summary>
        /// The specified RootFile value ({0}) is invalid.
        ///		{0} Invalid RootFile value
        /// </summary>
        public static string InvalidRootFileName = "InvalidRootFileName";

        /// <summary>
        /// The specified path ({0}) is invalid.
        ///		{0} Invalid path value.
        /// </summary>
        public static string InvalidPath = "InvalidPath";

        /// <summary>
        /// The specified path was not found.
        ///		{0} name of path
        /// </summary>
        public static string PathNotFound = "PathNotFound";

        /// <summary>
        /// Unexpected error occurred while attempting to access a path.
        ///		{0} name of path
        /// </summary>
        public static string PathAccessError = "PathAccessError";

        /// <summary>
        /// Unexpected error occurred while attempting to access a manifest.
        ///		{0} name of backing store associated with manifest
        /// </summary>
        public static string ManifestAccessError = "ManifestAccessError";

        /// <summary>
        /// Unexpected error occurred while attempting to copy a site.
        ///		{0} source directory
        ///		(1) destination directory
        /// </summary>
        public static string UnableToCopySite = "UnableToCopySite";

        /// <summary>
        /// Unexpected error occurred while attempting to move a site.
        ///		{0} source directory
        ///		(1) destination directory
        /// </summary>
        public static string UnableToMoveSite = "UnableToMoveSite";

        /// <summary>
        /// Unexpected error occurred while attempting to delete a site.
        ///		{0} directory containing site
        /// </summary>
        public static string UnableToDeleteSite = "UnableToDeleteSite";

        /// <summary>
        /// Unexpected error occurred while attempting to create storage for a site
        ///		{0} location where we attempted to create storage
        /// </summary>
        public static string UnableToCreateStorage = "UnableToCreateStorage";

        /// <summary>
        /// A stream that was written to was not closed prior to attempting to read from it.
        ///		{0} path of stream that was not closed
        /// </summary>
        public static string StreamNotClosed = "StreamNotClosed";

    }

}
