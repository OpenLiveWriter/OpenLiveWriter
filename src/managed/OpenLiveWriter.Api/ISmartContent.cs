// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace OpenLiveWriter.Api
{
    /// <summary>
    /// Interface used to manipulate the state, layout, and supporting files of SmartContent objects.
    /// </summary>
    public interface ISmartContent
    {
        /// <summary>
        /// Property-set that represents the state of a SmartContent object.
        /// </summary>
        IProperties Properties
        {
            get;
        }

        /// <summary>
        /// Supporting-files used by the SmartContent object.
        /// </summary>
        ISupportingFiles Files
        {
            get;
        }

        /// <summary>
        /// Layout options for SmartContent object.
        /// </summary>
        ILayoutStyle Layout
        {
            get;
        }
    }

    /// <summary>
    /// SmartContent object alignment.
    /// </summary>
    public enum Alignment
    {
        /// <summary>
        /// No alignment (display object inline with text).
        /// </summary>
        None,

        /// <summary>
        /// Left alignment (text wraps around the right side of the object).
        /// </summary>
        Left,

        /// <summary>
        /// Right alignment (text wraps around the left side of the object)
        /// </summary>
        Right,
        /// <summary>
        /// Places the smart content in the middle of the block, sometimes splitting the
        /// current block to accomplish the task.
        /// </summary>
        Center
    }

    /// <summary>
    ///  Layout options for SmartContent object.
    /// </summary>
    public interface ILayoutStyle
    {
        /// <summary>
        /// Alignment of object relative to text.
        /// </summary>
        Alignment Alignment
        {
            get;
            set;
        }

        /// <summary>
        /// Margin (in pixels) above the object.
        /// </summary>
        int TopMargin
        {
            get;
            set;
        }

        /// <summary>
        /// Margin (in pixels) to the right of object.
        /// </summary>
        int RightMargin
        {
            get;
            set;
        }

        /// <summary>
        /// Margin (in pixels) below the object.
        /// </summary>
        int BottomMargin
        {
            get;
            set;
        }

        /// <summary>
        /// Margin (in pixels) to the left of the object.
        /// </summary>
        int LeftMargin
        {
            get;
            set;
        }

    }

    /// <summary>
    /// Interface to supporting-files used by the SmartContent object. Any type of file can be added
    /// to the list of supporting files however only image files with a .gif or .jpg extension can
    /// be included in published output.
    /// </summary>
    public interface ISupportingFiles
    {
        /// <summary>
        /// Access the contents of a supporting file.
        /// </summary>
        /// <param name="fileName">Name of supporting file</param>
        /// <returns>Stream interface to file.</returns>
        Stream Open(string fileName);

        /// <summary>
        /// Access the contents of a supporting file.
        /// </summary>
        /// <param name="fileName">Name of supporting file.</param>
        /// <param name="create">Create the file if it doesn't already exist.</param>
        /// <returns>Stream interface to file.</returns>
        Stream Open(string fileName, bool create);

        /// <summary>
        /// This method is not supported.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="sourceFilePath"></param>
        [Obsolete("Use Add instead", true)]
        void AddFile(string fileName, string sourceFilePath);

        /// <summary>
        /// Add a file by copying the contents of an existing (external) file.
        /// </summary>
        /// <param name="fileName">Name of supporting file.</param>
        /// <param name="sourceFilePath">Full path to external file to be copied.</param>
        void Add(string fileName, string sourceFilePath);

        /// <summary>
        /// Add an image to the list of supporting files.
        /// </summary>
        /// <param name="fileName">Name of supporting file.</param>
        /// <param name="image">Source image</param>
        /// <param name="imageFormat">Source image format</param>
        void AddImage(string fileName, Image image, ImageFormat imageFormat);

        /// <summary>
        /// Remove a file.
        /// </summary>
        /// <param name="fileName">Name of supporting file to remove.</param>
        void Remove(string fileName);

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
        Uri GetUri(string fileName);

        /// <summary>
        /// Check whether a file of a specified name is included within the files associated with this object.
        /// </summary>
        /// <param name="fileName">Name of supporting file.</param>
        /// <returns>True if the file is included within the files associated with this object, otherwise false.</returns>
        bool Contains(string fileName);

        /// <summary>
        /// List of all supporting files associated with this object.
        /// </summary>
        string[] Filenames { get; }
    }

}

