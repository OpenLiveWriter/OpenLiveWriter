// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.IO;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Summary description for TransientFileSystemItem.
    /// </summary>
    public interface TransientFileSystemItem
    {
        string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Searches this tree of TransientFileSystemItems for paths
        /// longer than inputLength, and of all the paths that match,
        /// returns the one with the maximum number of elements.
        /// </summary>
        int MaxElementsOfPathLongerThan(int inputLength);

        FileSystemInfo Create(DirectoryInfo destination);
    }
}
