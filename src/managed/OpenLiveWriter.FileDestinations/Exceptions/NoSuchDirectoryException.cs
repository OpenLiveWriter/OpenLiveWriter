// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.FileDestinations
{

    /// <summary>
    /// NoSuchDirectoryException Exception Class
    /// </summary>
    public class NoSuchDirectoryException : SiteDestinationException
    {
        /// <summary>
        /// NoSuchDirectoryException constructor
        /// </summary>
        /// <param name="innerException">any caught exception to be kept in the exception chain</param>
        /// <param name="arguments">Any exception type specific arguments</param>
        public NoSuchDirectoryException(string path)
            : base(null, SiteDestinationException.UnexpectedException, path)
        {
            this.path = path;
        }

        private string path;
        public string Path
        {
            get
            {
                return path;
            }
        }
    }
}
