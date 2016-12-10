// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Api
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;

    using JetBrains.Annotations;

    /// <summary>
    /// Interface to supporting-files used by the SmartContent object. Any type of file can be added
    /// to the list of supporting files however only image files with a .gif or .jpg extension can
    /// be included in published output.
    /// </summary>
    public interface ISupportingFiles
    {
        /// <summary>
        /// Gets the list of all supporting files associated with this object.
        /// </summary>
        [NotNull]
        string[] Filenames { get; }

        /// <summary>
        /// Access the contents of a supporting file.
        /// </summary>
        /// <param name="fileName">Name of supporting file</param>
        /// <returns>Stream interface to file.</returns>
        [NotNull]
        Stream Open([NotNull] string fileName);

        /// <summary>
        /// Access the contents of a supporting file.
        /// </summary>
        /// <param name="fileName">Name of supporting file.</param>
        /// <param name="create">Create the file if it doesn't already exist.</param>
        /// <returns>Stream interface to file.</returns>
        [NotNull]
        Stream Open([NotNull] string fileName, bool create);

        /// <summary>
        /// This method is not supported.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="sourceFilePath">The source file path.</param>
        [Obsolete("Use Add instead", true)]
        void AddFile([NotNull] string fileName, [NotNull] string sourceFilePath);

        /// <summary>
        /// Add a file by copying the contents of an existing (external) file.
        /// </summary>
        /// <param name="fileName">Name of supporting file.</param>
        /// <param name="sourceFilePath">Full path to external file to be copied.</param>
        void Add([NotNull] string fileName, [NotNull] string sourceFilePath);

        /// <summary>
        /// Add an image to the list of supporting files.
        /// </summary>
        /// <param name="fileName">Name of supporting file.</param>
        /// <param name="image">Source image</param>
        /// <param name="imageFormat">Source image format</param>
        void AddImage([NotNull] string fileName, [NotNull] Image image, [NotNull] ImageFormat imageFormat);

        /// <summary>
        /// Remove a file.
        /// </summary>
        /// <param name="fileName">Name of supporting file to remove.</param>
        void Remove([NotNull] string fileName);

        /// <summary>
        /// Remove all supporting files.
        /// </summary>
        void RemoveAll();

        /// <summary>
        /// Get a URI for the specified supporting file. In order to reference a file contained
        /// within supporting files you must call this method and include the return value (rather
        /// than the original fileName).
        /// </summary>
        /// <param name="fileName">Name of supporting file.</param>
        /// <returns>URI suitable for use within generated HTML output.</returns>
        [NotNull]
        Uri GetUri([NotNull] string fileName);

        /// <summary>
        /// Check whether a file of a specified name is included within the files associated with this object.
        /// </summary>
        /// <param name="fileName">Name of supporting file.</param>
        /// <returns>True if the file is included within the files associated with this object, otherwise false.</returns>
        bool Contains([NotNull] string fileName);
    }
}
