// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace OpenLiveWriter.Api
{
    using System;

    using JetBrains.Annotations;

    /// <summary>
    /// Provides date for the HtmlDocumentAvailable event.
    /// </summary>
    public class HtmlDocumentAvailableEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlDocumentAvailableEventArgs"/> class.
        /// </summary>
        /// <param name="document">
        /// HTML document (guaranteed to be castable to an IHTMLDocument2)
        /// </param>
        public HtmlDocumentAvailableEventArgs([NotNull] object document)
        {
            this.Document = document;
        }

        /// <summary>
        /// Gets the HTML document (guaranteed to be castable to an IHTMLDocument2)
        /// </summary>
        [NotNull]
        public object Document { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the document is ready for a screen capture. Set this value
        /// to false to indicate that the document is not yet ready. This is useful for HTML
        /// documents that load in stages, such as documents that use embedded JavaScript to
        /// fetch and render additional content after the main document has loaded.
        /// </summary>
        public bool DocumentReady { get; set; } = true;
    }
}