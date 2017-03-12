// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;

namespace OpenLiveWriter.Api
{
    /// <summary>
    /// Exception thrown by the CreateContent methods of the ContentSource and SmartContentSource classes
    /// when they are unable to create content due to an error. Exceptions of this type are caught
    /// and displayed using a standard content-creation error dialog.
    /// </summary>
    public class ContentCreationException : ApplicationException
    {
        /// <summary>
        /// Create a new ContentCreationException
        /// </summary>
        /// <param name="title">Title of exception (used as the caption of the error dialog).</param>
        /// <param name="description">Description of exception (used to provide additional details within the error dialog).</param>
        public ContentCreationException(string title, string description)
            : base(String.Format(CultureInfo.CurrentCulture, "{0}: {1}", title, description))
        {
            _title = title;
            _description = description;
        }

        /// <summary>
        /// Title of exception (used as the caption of the error dialog)
        /// </summary>
        public string Title { get { return _title; } }
        private string _title;

        /// <summary>
        /// Description of exception (used to provide additional details within the error dialog).
        /// </summary>
        public string Description { get { return _description; } }
        private string _description;
    }
}
