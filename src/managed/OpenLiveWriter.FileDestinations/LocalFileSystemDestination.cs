// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;

namespace OpenLiveWriter.FileDestinations
{
    /// <summary>
    /// Summary description for LocalFileSystemDestination.
    /// </summary>
    public class LocalFileSystemDestination : FileSystemDestination
    {
        /// <summary>
        /// A Local File System Destination
        /// </summary>
        /// <param name="path">The path to the local file system destination</param>
        public LocalFileSystemDestination(string path)
            : base(path != null && path != "" ? path : Directory.GetDirectoryRoot(Directory.GetCurrentDirectory()))
        {
        }

        /// <summary>
        /// Determines whether file transfer operations should retry
        /// given an exception.
        /// </summary>
        /// <param name="e">The exception that has occurred</param>
        /// <returns>True indicates to retry operation, otherwise false</returns>
        public override bool RetryOnException(Exception e)
        {
            // Don't retry on the local file system
            return false;
        }
    }
}
