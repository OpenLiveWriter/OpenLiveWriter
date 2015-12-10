// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Windows.Forms;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Summary description for FileDataObject.
    /// </summary>
    public class FileDataObject : DataObjectBase
    {

        /// <summary>
        /// Creates a new fileDataObject based upon a string array of paths
        /// </summary>
        /// <param name="paths">The paths to the files from which to
        /// construct the file data object.</param>
        public FileDataObject(string[] paths)
        {
            IDataObject = new DataObject(DataFormats.FileDrop, paths);
        }
    }
}

